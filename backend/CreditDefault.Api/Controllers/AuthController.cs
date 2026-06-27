using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using CreditDefault.Api.DTOs;
using CreditDefault.Api.Interfaces;
using CreditDefault.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CreditDefault.Api.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {
        private readonly AuthService _authService;
        private readonly IUserRepository _userRepo;

        public AuthController(AuthService authService, IUserRepository userRepo)
        {
            _authService = authService;
            _userRepo = userRepo;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterDto dto)
        {
            try
            {
                return Ok(await _authService.RegisterAsync(dto, GetIpAddress()));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginDto dto)
        {
            try
            {
                return Ok(await _authService.LoginAsync(dto, GetIpAddress()));
            }
            catch (UnauthorizedAccessException)
            {
                return Unauthorized(new { error = "Invalid credentials." });
            }
        }

        [HttpPost("refresh")]
        public async Task<IActionResult> Refresh(RefreshTokenRequestDto dto)
        {
            try
            {
                return Ok(await _authService.RefreshAsync(dto.RefreshToken, GetIpAddress()));
            }
            catch (UnauthorizedAccessException)
            {
                return Unauthorized(new { error = "Invalid refresh token." });
            }
        }

        [HttpPost("logout")]
        public async Task<IActionResult> Logout(LogoutRequestDto dto)
        {
            await _authService.LogoutAsync(dto.RefreshToken, GetIpAddress());
            return Ok(new { message = "Logged out." });
        }

        [HttpGet("me")]
        [Authorize]
        public async Task<IActionResult> Me()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue(JwtRegisteredClaimNames.Sub);
            if (userId == null) return Unauthorized();
            var user = await _userRepo.GetByIdAsync(Guid.Parse(userId));
            if (user == null) return Unauthorized();

            var roles = user.UserRoles.Select(ur => ur.Role.Name).Distinct().ToList();
            return Ok(new { user.Id, user.FullName, user.Email, Role = roles.FirstOrDefault() ?? user.Role, Roles = roles });
        }

        private string? GetIpAddress() => HttpContext.Connection.RemoteIpAddress?.ToString();
    }
}
