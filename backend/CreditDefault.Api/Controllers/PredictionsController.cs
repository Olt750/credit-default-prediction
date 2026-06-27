using System;
using System.Linq;
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
    [Route("api/predictions")]
    public class PredictionsController : ControllerBase
    {
        private readonly IPredictionRepository _predictionRepo;
        private readonly IUserRepository _userRepo;
        private readonly PredictionEngine _engine;
        private readonly PythonCreditRiskPredictionService _pythonPredictionService;

        public PredictionsController(
            IPredictionRepository predictionRepo,
            IUserRepository userRepo,
            PredictionEngine engine,
            PythonCreditRiskPredictionService pythonPredictionService)
        {
            _predictionRepo = predictionRepo;
            _userRepo = userRepo;
            _engine = engine;
            _pythonPredictionService = pythonPredictionService;
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
            var user = await _userRepo.GetByIdAsync(Guid.Parse(userId));
            if (user == null) return Unauthorized();

            var result = await _pythonPredictionService.PredictAsync(dto);
            var prediction = new Prediction
            {
                Id = Guid.NewGuid(),
                UserId = user.Id,
                Age = dto.Age,
                Income = dto.AnnualIncome,
                EmploymentStatus = dto.EmploymentStatus,
                CreditScore = dto.CreditScore,
                ExistingDebt = dto.TotalMonthlyDebt,
                LoanAmount = dto.LoanAmount,
                LoanTerm = dto.LoanTerm,
                PreviousDefaults = dto.PreviousDefaults,
                Education = dto.Education,
                MaritalStatus = dto.MaritalStatus,
                MonthlyCarLoanPayment = dto.MonthlyCarLoanPayment,
                MonthlyMortgageOrRentPayment = dto.MonthlyMortgageOrRentPayment,
                MonthlyPersonalLoanPayment = dto.MonthlyPersonalLoanPayment,
                MonthlyCreditCardPayment = dto.MonthlyCreditCardPayment,
                MonthlyOtherDebtPayment = dto.MonthlyOtherDebtPayment,
                TotalMonthlyDebt = dto.TotalMonthlyDebt,
                DebtToIncomeRatio = dto.DebtToIncomeRatio,
                PaymentHistory = $"Previous defaults: {dto.PreviousDefaults}; Education: {dto.Education}; Marital status: {dto.MaritalStatus}",
                RiskScore = result.RiskScore,
                RiskLevel = result.RiskLevel,
                LoanStatus = result.RiskLevel == "Low" ? "Approved" : result.RiskLevel == "Medium" ? "Pending" : "Rejected",
                ExplanationMessage = result.Explanation,
                CreatedAt = DateTime.UtcNow
            };
            await _predictionRepo.AddAsync(prediction);

            return Ok(result);
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> CreatePrediction(PredictionDto dto)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue(System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Sub);
            if (userId == null) return Unauthorized();
            var user = await _userRepo.GetByIdAsync(Guid.Parse(userId));
            if (user == null) return Unauthorized();
            var (riskScore, riskLevel, explanation) = _engine.Predict(dto.CreditScore, dto.Income, dto.ExistingDebt, dto.EmploymentStatus);
            var prediction = new Prediction
            {
                Id = Guid.NewGuid(),
                UserId = user.Id,
                Age = dto.Age,
                Income = dto.Income,
                EmploymentStatus = dto.EmploymentStatus,
                CreditScore = dto.CreditScore,
                ExistingDebt = dto.ExistingDebt,
                LoanAmount = dto.LoanAmount,
                LoanTerm = dto.LoanTerm,
                PaymentHistory = dto.PaymentHistory,
                RiskScore = riskScore,
                RiskLevel = riskLevel,
                ExplanationMessage = explanation,
                CreatedAt = DateTime.UtcNow
            };
            await _predictionRepo.AddAsync(prediction);
            return Ok(new PredictionResultDto
            {
                RiskScore = riskScore,
                RiskLevel = riskLevel,
                ExplanationMessage = explanation,
                CreatedAt = prediction.CreatedAt,
                UserId = user.Id
            });
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
