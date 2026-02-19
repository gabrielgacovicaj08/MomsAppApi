using MomsAppApi.Entities;
using MomsAppApi.Models.LoginDTO;

namespace MomsAppApi.Services.AuthService
{
    public interface IAuthService
    {
        Task<TokenResponseDTO?> LoginAsync(LoginRequestDTO request);
        Task<TokenResponseDTO?> RefreshTokensAsync(RefreshTokenRequestDTO request);
    }
}
