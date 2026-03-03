using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using MomsAppApi.Models.LoginDTO;
using MomsAppApi.Services.AuthService;

namespace MomsAppApi.Controller
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController(IAuthService authService) : ControllerBase
    {
        [EnableRateLimiting("AuthPolicy")]
        [HttpPost("login")]
        public async Task<ActionResult<TokenResponseDTO>> Login(LoginRequestDTO request)
        {
            var result = await authService.LoginAsync(request);

            if (result.IsLockedOut)
            {
                if (result.RetryAfterSeconds.HasValue)
                {
                    Response.Headers.RetryAfter = result.RetryAfterSeconds.Value.ToString();
                }

                return StatusCode(StatusCodes.Status429TooManyRequests, new ProblemDetails
                {
                    Title = "Account temporarily locked",
                    Detail = "Too many failed login attempts. Please try again later.",
                    Status = StatusCodes.Status429TooManyRequests
                });
            }

            if (result.Tokens == null)
            {
                return Unauthorized(new ProblemDetails
                {
                    Title = "Authentication failed",
                    Detail = "Invalid email or password.",
                    Status = StatusCodes.Status401Unauthorized
                });
            }

            return Ok(result.Tokens);
        }

        [EnableRateLimiting("AuthPolicy")]
        [HttpPost("refresh-token")]
        public async Task<ActionResult<TokenResponseDTO>> RefreshToken(RefreshTokenRequestDTO request)
        {
            var result = await authService.RefreshTokensAsync(request);
            if (result == null)
            {
                return Unauthorized(new ProblemDetails
                {
                    Title = "Refresh token invalid",
                    Detail = "Refresh token is invalid or expired.",
                    Status = StatusCodes.Status401Unauthorized
                });
            }

            return Ok(result);
        }

        [Authorize(Roles = "ADMIN")]
        [HttpGet]
        public IActionResult AuthenticatedOnlyEndpoint()
        {
            return Ok("You are an ADMIN");
        }
    }
}
