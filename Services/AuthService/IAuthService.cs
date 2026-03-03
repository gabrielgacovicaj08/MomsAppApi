using MomsAppApi.Models.LoginDTO;

namespace MomsAppApi.Services.AuthService
{
    public interface IAuthService
    {
        Task<LoginAttemptResultDTO> LoginAsync(LoginRequestDTO request);
        Task<TokenResponseDTO?> RefreshTokensAsync(RefreshTokenRequestDTO request);
    }
}
