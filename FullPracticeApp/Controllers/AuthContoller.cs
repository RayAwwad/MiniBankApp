using FullPracticeApp.Contracts.Dtos;
using FullPracticeApp.Contracts.Interfaces;
using FullPracticeApp.Domain.Entities;
using Microsoft.AspNetCore.Mvc;

namespace FullPracticeApp.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthContoller : Controller
    {
        private readonly IAuthService authService;
        public AuthContoller(IAuthService authService)
        {
            this.authService = authService;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto loginDto)
        {
            var result = await authService.Login(loginDto);
            Response.Cookies.Append("refreshToken", result.RefreshToken, new CookieOptions
            {
                HttpOnly = true,
                Expires = result.RefreshTokenExpiresAtExpires
            });
            Response.Cookies.Append("accessToken", result.AccessToken);
            return Ok(result);
        }

        [HttpPost("signup")]
        public async Task<IActionResult> Signup([FromBody] SignupDto signupDto)
        {
            var result = await authService.Signup(signupDto);
            Response.Cookies.Append("refreshToken", result.RefreshToken, new CookieOptions
            {
                HttpOnly = true,
                Expires = result.RefreshTokenExpiresAtExpires
            });
            Response.Cookies.Append("accessToken", result.AccessToken);
            return Ok(result);
        }
        [HttpPost("refresh")]
        public async Task<IActionResult> Refresh([FromQuery] string refreshToken, [FromQuery] int userId)
        {
            var result = await authService.Refresh(refreshToken, userId);
            Response.Cookies.Append("refreshToken", result.RefreshToken, new CookieOptions
            {
                HttpOnly = true,
                Expires = result.RefreshTokenExpiresAtExpires
            });
            Response.Cookies.Append("accessToken", result.AccessToken);
            Response.Cookies.Append("userId", userId.ToString());
            return Ok(result);
        }
    }
}
