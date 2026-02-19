using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using MomsAppApi.Entities;
using MomsAppApi.Models.LoginDTO;
using MomsAppApi.Services.AuthService;
using System.Globalization;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace MomsAppApi.Controller
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController(IAuthService authService) : ControllerBase
    {
        
        

        [HttpPost("login")]
        public async Task<ActionResult<TokenResponseDTO>> Login(LoginRequestDTO request)
        {
            var result = await authService.LoginAsync(request);
            if(result == null)
            {
                return BadRequest("Invalid username or password.");
            }
            return Ok(result);

        }

        [HttpPost("refresh-token")]
        public async Task<ActionResult<TokenResponseDTO>> RefreshToken(RefreshTokenRequestDTO request)
        {
            var result = await authService.RefreshTokensAsync(request);
            if(result == null)
            {
                return BadRequest("Invalid refresh token.");
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
