using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CreditDefault.Api.DTOs;
using CreditDefault.Api.Interfaces;
using CreditDefault.Api.Services;

namespace CreditDefault.Api.Controllers
{
    [ApiController]
    [Route("api/predictions")]
    public class PredictionsController : ControllerBase
    {
        private readonly IPredictionRepository _predictionRepo;
        private readonly PredictionWorkflowService _predictionWorkflowService;

        public PredictionsController(
            IPredictionRepository predictionRepo,
            PredictionWorkflowService predictionWorkflowService)
        {
            _predictionRepo = predictionRepo;
            _predictionWorkflowService = predictionWorkflowService;
        }

        [HttpPost("predict")]
        [Authorize]
        public async Task<IActionResult> Predict(CreditRiskPredictionRequestDto dto)
        {
            if (!ModelState.IsValid)
            {
                return ValidationProblem(ModelState);
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue(System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Sub);
            if (userId == null) return Unauthorized();

            var result = await _predictionWorkflowService.PredictWithMlAsync(Guid.Parse(userId), dto);
            return result == null ? Unauthorized() : Ok(result);
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> CreatePrediction(PredictionDto dto)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue(System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Sub);
            if (userId == null) return Unauthorized();
            var result = await _predictionWorkflowService.CreateRuleBasedPredictionAsync(Guid.Parse(userId), dto);
            return result == null ? Unauthorized() : Ok(result);
        }

        [HttpGet("my")]
        [Authorize]
        public async Task<IActionResult> GetMyPredictions()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue(System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Sub);
            if (userId == null) return Unauthorized();
            var predictions = await _predictionRepo.GetByUserIdAsync(Guid.Parse(userId));
            return Ok(predictions);
        }

        [HttpGet("recent")]
        [Authorize]
        public async Task<IActionResult> GetRecentPredictions([FromServices] DashboardService dashboardService, [FromQuery] int limit = 5)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue(System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Sub);
            if (userId == null) return Unauthorized();

            var isAdmin = User.IsInRole("Admin") || User.FindFirstValue(ClaimTypes.Role) == "Admin";
            return Ok(await dashboardService.GetRecentPredictionsAsync(Guid.Parse(userId), isAdmin, limit));
        }

        [HttpGet("{id}")]
        [Authorize]
        public async Task<IActionResult> GetPrediction(Guid id)
        {
            var prediction = await _predictionRepo.GetByIdAsync(id);
            if (prediction == null) return NotFound();
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue(System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Sub);
            if (userId == null || prediction.UserId.ToString() != userId) return Forbid();
            return Ok(prediction);
        }
    }
}
