using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CreditDefault.Api.Interfaces;

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

        public AdminController(IUserRepository userRepo, IPredictionRepository predictionRepo, IAuditLogRepository auditLogRepo)
        {
            _userRepo = userRepo;
            _predictionRepo = predictionRepo;
            _auditLogRepo = auditLogRepo;
        }

        [HttpGet("users")]
        public async Task<IActionResult> GetUsers() => Ok(await _userRepo.GetAllAsync());

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
    }
}