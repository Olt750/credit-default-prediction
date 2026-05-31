using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CreditDefault.Api.DTOs;
using CreditDefault.Api.Interfaces;
using CreditDefault.Api.Models;
using CreditDefault.Api.Services;

namespace CreditDefault.Api.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {
        private readonly IUserRepository _userRepo;
        private readonly JwtService _jwtService;
        private readonly PasswordService _passwordService;

        public AuthController(IUserRepository userRepo, JwtService jwtService, PasswordService passwordService)
        {
            _userRepo = userRepo;
            _jwtService = jwtService;
            _passwordService = passwordService;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterDto dto)
        {
            if (dto.Password != dto.ConfirmPassword)
                return BadRequest(new { error = "Passwords do not match." });
            if (await _userRepo.GetByEmailAsync(dto.Email) != null)
                return BadRequest(new { error = "Email already registered." });
            var user = new User
            {
                Id = Guid.NewGuid(),
                FullName = dto.FullName,
                Email = dto.Email,
                PasswordHash = _passwordService.HashPassword(dto.Password),
                PhoneNumber = dto.PhoneNumber,
                Role = "User",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            await _userRepo.AddAsync(user);
            return Ok(new { message = "Registration successful." });
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginDto dto)
        {
            var user = await _userRepo.GetByEmailAsync(dto.Email);
            if (user == null || !_passwordService.VerifyPassword(dto.Password, user.PasswordHash))
                return Unauthorized(new { error = "Invalid credentials." });
            var token = _jwtService.GenerateToken(user);
            return Ok(new { token, user = new { user.Id, user.FullName, user.Email, user.Role } });
        }

        [HttpGet("me")]
        [Authorize]
        public async Task<IActionResult> Me()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue(JwtRegisteredClaimNames.Sub);
            if (userId == null) return Unauthorized();
            var user = await _userRepo.GetByIdAsync(Guid.Parse(userId));
            if (user == null) return Unauthorized();
            return Ok(new { user.Id, user.FullName, user.Email, user.Role });
        }
    }
}