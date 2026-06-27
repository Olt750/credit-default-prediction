using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Threading.Tasks;
using CreditDefault.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CreditDefault.Api.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/dashboard")]
    public class DashboardController : ControllerBase
    {
        private readonly DashboardService _dashboardService;

        public DashboardController(DashboardService dashboardService)
        {
            _dashboardService = dashboardService;
        }

        [HttpGet("summary")]
        public async Task<IActionResult> GetSummary()
        {
            var userId = GetUserId();
            if (userId == null) return Unauthorized();

            return Ok(await _dashboardService.GetSummaryAsync(userId.Value, IsAdmin()));
        }

        [HttpGet("risk-distribution")]
        public async Task<IActionResult> GetRiskDistribution()
        {
            var userId = GetUserId();
            if (userId == null) return Unauthorized();

            return Ok(await _dashboardService.GetRiskDistributionAsync(userId.Value, IsAdmin()));
        }

        [HttpGet("monthly-activity")]
        public async Task<IActionResult> GetMonthlyActivity()
        {
            var userId = GetUserId();
            if (userId == null) return Unauthorized();

            return Ok(await _dashboardService.GetMonthlyActivityAsync(userId.Value, IsAdmin()));
        }

        [HttpGet("loan-status")]
        public async Task<IActionResult> GetLoanStatus()
        {
            var userId = GetUserId();
            if (userId == null) return Unauthorized();

            return Ok(await _dashboardService.GetLoanStatusAsync(userId.Value, IsAdmin()));
        }

        [HttpGet("recent-predictions")]
        public async Task<IActionResult> GetRecentPredictions([FromQuery] int limit = 5)
        {
            var userId = GetUserId();
            if (userId == null) return Unauthorized();

            return Ok(await _dashboardService.GetRecentPredictionsAsync(userId.Value, IsAdmin(), limit));
        }

        private Guid? GetUserId()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue(JwtRegisteredClaimNames.Sub);
            return Guid.TryParse(userId, out var parsed) ? parsed : null;
        }

        private bool IsAdmin()
        {
            return User.IsInRole("Admin") || User.FindFirstValue(ClaimTypes.Role) == "Admin" || User.FindFirstValue("role") == "Admin";
        }
    }
}
