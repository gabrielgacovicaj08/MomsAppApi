using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.IdentityModel.Tokens;
using MomsAppApi.Data;
using MomsAppApi.Entities;
using MomsAppApi.Models.LoginDTO;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace MomsAppApi.Services.AuthService
{
    public class AuthService(
        MomsAppDbContext context,
        IConfiguration configuration,
        IMemoryCache memoryCache,
        ILogger<AuthService> logger) : IAuthService
    {
        private const int MaxFailedLoginAttempts = 5;
        private static readonly TimeSpan LockoutDuration = TimeSpan.FromMinutes(15);
        private static readonly TimeSpan AttemptWindow = TimeSpan.FromMinutes(15);

        public async Task<LoginAttemptResultDTO> LoginAsync(LoginRequestDTO request)
        {
            if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
                return LoginAttemptResultDTO.InvalidCredentials();

            var normalizedEmail = request.Email.Trim().ToLowerInvariant();

            if (TryGetLockoutRemaining(normalizedEmail, out var remainingLockout))
            {
                logger.LogWarning("Blocked login for locked account {Email}", normalizedEmail);
                return LoginAttemptResultDTO.LockedOut((int)Math.Ceiling(remainingLockout.TotalSeconds));
            }

            var user = await context.UserAccounts
                .FirstOrDefaultAsync(u => u.email == normalizedEmail);

            if (user == null)
            {
                RecordFailedLogin(normalizedEmail);
                return LoginAttemptResultDTO.InvalidCredentials();
            }

            var result = new PasswordHasher<UserAccount>()
                .VerifyHashedPassword(user, user.password_hash, request.Password);

            if (result == PasswordVerificationResult.Failed)
            {
                RecordFailedLogin(normalizedEmail);
                return LoginAttemptResultDTO.InvalidCredentials();
            }

            ClearFailedLoginState(normalizedEmail);

            TokenResponseDTO response = await CreateTokenResponse(user);
            return LoginAttemptResultDTO.Success(response);
        }

        private async Task<TokenResponseDTO> CreateTokenResponse(UserAccount user)
        {
            return new TokenResponseDTO
            {
                AccessToken = CreateToken(user),
                RefreshToken = await GeenerateAndSaveRefreshTokenAsync(user)
            };
        }

        public async Task<TokenResponseDTO?> RefreshTokensAsync(RefreshTokenRequestDTO request)
        {
            var user = await ValidateRefreshTokenAsync(request.user_id, request.refresh_token);
            if (user == null)
                return null;

            return await CreateTokenResponse(user);
        }

        private async Task<UserAccount?> ValidateRefreshTokenAsync(int user_id, string refresh_token)
        {
            var user = await context.UserAccounts.FindAsync(user_id);
            if (user is null || !user.refresh_token_expiry_time.HasValue || user.refresh_token_expiry_time <= DateTime.UtcNow)
            {
                return null;
            }

            var storedToken = user.refresh_token ?? string.Empty;
            var incomingTokenHash = HashRefreshToken(refresh_token);

            var matches = IsHashedToken(storedToken)
                ? FixedTimeEquals(storedToken, incomingTokenHash)
                : FixedTimeEquals(storedToken, refresh_token);

            if (!matches)
            {
                return null;
            }

            // One-time migration path for legacy plain-text tokens in the DB.
            if (!IsHashedToken(storedToken))
            {
                user.refresh_token = incomingTokenHash;
                context.UserAccounts.Update(user);
                await context.SaveChangesAsync();
            }

            return user;
        }

        private string GenerateRefreshToken()
        {
            var randomNumber = new byte[32];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomNumber);
            return Convert.ToBase64String(randomNumber);
        }

        private async Task<string> GeenerateAndSaveRefreshTokenAsync(UserAccount user)
        {
            var refreshToken = GenerateRefreshToken();
            user.refresh_token = HashRefreshToken(refreshToken);
            user.refresh_token_expiry_time = DateTime.UtcNow.AddDays(7);
            context.UserAccounts.Update(user);
            await context.SaveChangesAsync();
            return refreshToken;
        }

        private static string HashRefreshToken(string token)
        {
            var hashBytes = SHA256.HashData(Encoding.UTF8.GetBytes(token));
            return Convert.ToHexString(hashBytes);
        }

        private static bool IsHashedToken(string token)
        {
            return token.Length == 64 && token.All(Uri.IsHexDigit);
        }

        private static bool FixedTimeEquals(string left, string right)
        {
            return CryptographicOperations.FixedTimeEquals(
                Encoding.UTF8.GetBytes(left),
                Encoding.UTF8.GetBytes(right));
        }

        private string CreateToken(UserAccount user)
        {
            var claims = new List<Claim>
            {
                new Claim("employee_id", user.employee_id.ToString()),
                new Claim(ClaimTypes.Email, user.email),
                new Claim(ClaimTypes.Role, user.role)
            };

            var signingToken = configuration.GetValue<string>("AppSettings:Token")
                ?? throw new InvalidOperationException("Missing AppSettings:Token configuration.");

            var key = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(signingToken));

            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha512);

            var tokenDescriptor = new JwtSecurityToken(
                issuer: configuration.GetValue<string>("AppSettings:Issuer"),
                audience: configuration.GetValue<string>("AppSettings:Audience"),
                claims: claims,
                expires: DateTime.UtcNow.AddDays(1),
                signingCredentials: creds

                );

            return new JwtSecurityTokenHandler().WriteToken(tokenDescriptor);
        }

        private bool TryGetLockoutRemaining(string normalizedEmail, out TimeSpan remaining)
        {
            remaining = TimeSpan.Zero;

            if (!memoryCache.TryGetValue(GetLockoutCacheKey(normalizedEmail), out DateTimeOffset lockoutUntil))
                return false;

            var now = DateTimeOffset.UtcNow;
            if (lockoutUntil <= now)
            {
                memoryCache.Remove(GetLockoutCacheKey(normalizedEmail));
                return false;
            }

            remaining = lockoutUntil - now;
            return true;
        }

        private void RecordFailedLogin(string normalizedEmail)
        {
            var attemptsCacheKey = GetAttemptsCacheKey(normalizedEmail);
            var failedAttempts = memoryCache.Get<int?>(attemptsCacheKey) ?? 0;
            failedAttempts++;

            memoryCache.Set(attemptsCacheKey, failedAttempts, AttemptWindow);

            if (failedAttempts >= MaxFailedLoginAttempts)
            {
                memoryCache.Set(GetLockoutCacheKey(normalizedEmail), DateTimeOffset.UtcNow.Add(LockoutDuration), LockoutDuration);
                memoryCache.Remove(attemptsCacheKey);

                logger.LogWarning(
                    "Account {Email} temporarily locked after {Attempts} failed login attempts",
                    normalizedEmail,
                    MaxFailedLoginAttempts);
            }
        }

        private void ClearFailedLoginState(string normalizedEmail)
        {
            memoryCache.Remove(GetAttemptsCacheKey(normalizedEmail));
            memoryCache.Remove(GetLockoutCacheKey(normalizedEmail));
        }

        private static string GetAttemptsCacheKey(string normalizedEmail) => $"auth:failed-attempts:{normalizedEmail}";
        private static string GetLockoutCacheKey(string normalizedEmail) => $"auth:lockout:{normalizedEmail}";
    }
}
