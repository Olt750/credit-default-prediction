using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CreditDefault.Api.Interfaces;
using CreditDefault.Api.Services;
using CreditDefault.Api.DTOs;

namespace CreditDefault.Api.Controllers
{
    [ApiController]
    [Route("api/admin")]
    [Authorize(Roles = "Admin")]
    public class AdminController : ControllerBase
    {
        private readonly IUserRepository _userRepo;
        private readonly IPredictionRepository _predictionRepo;
        private readonly IAuditLogRepository _auditLogRepo;
        private readonly NotificationService _notificationService;

        public AdminController(
            IUserRepository userRepo,
            IPredictionRepository predictionRepo,
            IAuditLogRepository auditLogRepo,
            NotificationService notificationService)
        {
            _userRepo = userRepo;
            _predictionRepo = predictionRepo;
            _auditLogRepo = auditLogRepo;
            _notificationService = notificationService;
        }

        [HttpGet("users")]
        public async Task<IActionResult> GetUsers()
        {
            var users = await _userRepo.GetAllAsync();
            return Ok(users.Select(u => new UserListItemDto
            {
                Id = u.Id,
                FullName = u.FullName,
                Email = u.Email,
                PhoneNumber = u.PhoneNumber,
                Role = u.UserRoles.Select(ur => ur.Role.Name).FirstOrDefault() ?? u.Role,
                Roles = u.UserRoles.Select(ur => ur.Role.Name).ToList(),
                CreatedAt = u.CreatedAt
            }));
        }

        [HttpGet("predictions")]
        public async Task<IActionResult> GetPredictions() => Ok(await _predictionRepo.GetAllAsync());

        [HttpGet("statistics")]
        public async Task<IActionResult> GetStatistics()
        {
            var users = await _userRepo.GetAllAsync();
            var predictions = await _predictionRepo.GetAllAsync();
            return Ok(new
            {
                totalUsers = users?.Count() ?? 0,
                totalPredictions = predictions?.Count() ?? 0,
                highRisk = predictions?.Count(p => p.RiskLevel == "High") ?? 0,
                mediumRisk = predictions?.Count(p => p.RiskLevel == "Medium") ?? 0,
                lowRisk = predictions?.Count(p => p.RiskLevel == "Low") ?? 0
            });
        }

        [HttpGet("logs")]
        public async Task<IActionResult> GetLogs() => Ok(await _auditLogRepo.GetAllAsync());

        [HttpGet("notifications")]
        public async Task<IActionResult> GetNotifications([FromQuery] int page = 1, [FromQuery] int pageSize = 50)
        {
            var userId = User.FindFirstValue(System.Security.Claims.ClaimTypes.NameIdentifier)
                ?? User.FindFirstValue(System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Sub);
            return Guid.TryParse(userId, out var parsed)
                ? Ok(await _notificationService.GetForUserAsync(parsed, page, pageSize))
                : Unauthorized();
        }
    }
}
