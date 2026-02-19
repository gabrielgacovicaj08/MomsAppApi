using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
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
    public class AuthService(MomsAppDbContext context, IConfiguration configuration) : IAuthService
    {
        public async Task<TokenResponseDTO?> LoginAsync(LoginRequestDTO request)
        {
            if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
                return null;

            var user = await context.UserAccounts
                .FirstOrDefaultAsync(u => u.email == request.Email);

            if (user == null)
                return null;

            var result = new PasswordHasher<UserAccount>()
                .VerifyHashedPassword(user, user.password_hash, request.Password);

            if (result == PasswordVerificationResult.Failed)
                return null;
            TokenResponseDTO response = await CreateTokenResponse(user);
            // ✅ return token, not a string message
            return response;
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
            if(user is null || user.refresh_token != refresh_token || user.refresh_token_expiry_time <= DateTime.UtcNow)
            {
                return null;
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
            user.refresh_token = refreshToken;
            user.refresh_token_expiry_time = DateTime.Now.AddDays(7);
            context.UserAccounts.Update(user);
            await context.SaveChangesAsync();
            return refreshToken;
        }

        private string CreateToken(UserAccount user)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Email, user.email),
                new Claim(ClaimTypes.Role, user.role)
            };

            var key = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(configuration.GetValue<string>("AppSettings:Token")));

            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha512);

            var tokenDescriptor = new JwtSecurityToken(
                issuer: configuration.GetValue<string>("AppSettings:Issuer"),
                audience: configuration.GetValue<string>("AppSettings:Audience"),
                claims: claims,
                expires: DateTime.Now.AddDays(1),
                signingCredentials: creds

                );

            return new JwtSecurityTokenHandler().WriteToken(tokenDescriptor);
        }

       
    }
}
