using System;
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
    [Route("api/users")]
    public class UsersController : ControllerBase
    {
        private readonly IUserRepository _userRepo;
        private readonly PasswordService _passwordService;

        public UsersController(IUserRepository userRepo, PasswordService passwordService)
        {
            _userRepo = userRepo;
            _passwordService = passwordService;
        }

        [HttpGet("profile")]
        [Authorize]
        public async Task<IActionResult> GetProfile()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue(System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Sub);
            if (userId == null) return Unauthorized();
            var user = await _userRepo.GetByIdAsync(Guid.Parse(userId));
            if (user == null) return Unauthorized();
            return Ok(new UserProfileDto
            {
                FullName = user.FullName,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber,
                Role = user.Role
            });
        }

        [HttpPut("profile")]
        [Authorize]
        public async Task<IActionResult> UpdateProfile(UpdateProfileDto dto)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue(System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Sub);
            if (userId == null) return Unauthorized();
            var user = await _userRepo.GetByIdAsync(Guid.Parse(userId));
            if (user == null) return Unauthorized();
            user.FullName = dto.FullName ?? user.FullName;
            user.Email = dto.Email ?? user.Email;
            user.PhoneNumber = dto.PhoneNumber ?? user.PhoneNumber;
            if (!string.IsNullOrWhiteSpace(dto.Password))
                user.PasswordHash = _passwordService.HashPassword(dto.Password);
            user.UpdatedAt = DateTime.UtcNow;
            await _userRepo.UpdateAsync(user);
            return Ok(new { message = "Profile updated." });
        }
    }
}