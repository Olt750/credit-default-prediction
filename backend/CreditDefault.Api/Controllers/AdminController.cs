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
        private readonly AdminUserService _adminUserService;

        public AdminController(
            IUserRepository userRepo,
            IPredictionRepository predictionRepo,
            IAuditLogRepository auditLogRepo,
            NotificationService notificationService,
            AdminUserService adminUserService)
        {
            _userRepo = userRepo;
            _predictionRepo = predictionRepo;
            _auditLogRepo = auditLogRepo;
            _notificationService = notificationService;
            _adminUserService = adminUserService;
        }

        [HttpGet("users")]
        public async Task<IActionResult> GetUsers() => Ok(await _adminUserService.GetUsersAsync());

        [HttpGet("users/{id:guid}")]
        public async Task<IActionResult> GetUser(Guid id)
        {
            var user = await _adminUserService.GetUserAsync(id, GetCurrentUserId(), GetPerformedBy(), GetIpAddress());
            return user == null ? NotFound(new { error = "User was not found." }) : Ok(user);
        }

        [HttpPost("users/invite")]
        public async Task<IActionResult> InviteUser(AdminUserInviteRequest request)
        {
            if (!ModelState.IsValid) return ValidationProblem(ModelState);
            try
            {
                return Ok(await _adminUserService.InviteAsync(request, GetCurrentUserId(), GetPerformedBy(), GetIpAddress()));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpPut("users/{id:guid}")]
        public async Task<IActionResult> UpdateUser(Guid id, AdminUserUpdateRequest request)
        {
            if (!ModelState.IsValid) return ValidationProblem(ModelState);
            try
            {
                var user = await _adminUserService.UpdateAsync(id, request, GetCurrentUserId(), GetPerformedBy(), GetIpAddress());
                return user == null ? NotFound(new { error = "User was not found." }) : Ok(user);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpPut("users/{id:guid}/status")]
        public async Task<IActionResult> UpdateUserStatus(Guid id, AdminUserStatusRequest request)
        {
            try
            {
                var user = await _adminUserService.SetStatusAsync(id, request.IsActive, GetCurrentUserId(), GetPerformedBy(), GetIpAddress());
                return user == null ? NotFound(new { error = "User was not found." }) : Ok(user);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpPut("users/{id:guid}/roles")]
        public async Task<IActionResult> UpdateUserRoles(Guid id, AdminUserRolesRequest request)
        {
            if (!ModelState.IsValid) return ValidationProblem(ModelState);
            try
            {
                var user = await _adminUserService.SetRolesAsync(id, request, GetCurrentUserId(), GetPerformedBy(), GetIpAddress());
                return user == null ? NotFound(new { error = "User was not found." }) : Ok(user);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpDelete("users/{id:guid}")]
        public Task<IActionResult> DeleteUser(Guid id) =>
            UpdateUserStatus(id, new AdminUserStatusRequest { IsActive = false });

        private Guid GetCurrentUserId()
        {
            var userId = User.FindFirstValue(System.Security.Claims.ClaimTypes.NameIdentifier)
                ?? User.FindFirstValue(System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Sub);
            return Guid.Parse(userId!);
        }

        private string GetPerformedBy() =>
            User.FindFirstValue(System.Security.Claims.ClaimTypes.Email)
            ?? User.Identity?.Name
            ?? User.FindFirstValue(System.Security.Claims.ClaimTypes.NameIdentifier)
            ?? "UnknownUser";

        private string GetIpAddress() => HttpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown";

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
