using CreditDefault.Api.DTOs;
using CreditDefault.Api.Interfaces;
using CreditDefault.Api.Models;

namespace CreditDefault.Api.Services
{
    public class PredictionWorkflowService
    {
        private readonly IPredictionRepository _predictionRepository;
        private readonly IUserRepository _userRepository;
        private readonly PredictionEngine _engine;
        private readonly PythonCreditRiskPredictionService _pythonPredictionService;
        private readonly DashboardService _dashboardService;
        private readonly NotificationService _notificationService;

        public PredictionWorkflowService(
            IPredictionRepository predictionRepository,
            IUserRepository userRepository,
            PredictionEngine engine,
            PythonCreditRiskPredictionService pythonPredictionService,
            DashboardService dashboardService,
            NotificationService notificationService)
        {
            _predictionRepository = predictionRepository;
            _userRepository = userRepository;
            _engine = engine;
            _pythonPredictionService = pythonPredictionService;
            _dashboardService = dashboardService;
            _notificationService = notificationService;
        }

        public async Task<CreditRiskPredictionResponseDto?> PredictWithMlAsync(Guid userId, CreditRiskPredictionRequestDto dto)
        {
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null) return null;

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

            await CompletePredictionAsync(user.Id, prediction);
            return result;
        }

        public async Task<PredictionResultDto?> CreateRuleBasedPredictionAsync(Guid userId, PredictionDto dto)
        {
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null) return null;

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
                LoanStatus = riskLevel == "Low" ? "Approved" : riskLevel == "Medium" ? "Pending" : "Rejected",
                ExplanationMessage = explanation,
                CreatedAt = DateTime.UtcNow
            };

            await CompletePredictionAsync(user.Id, prediction);
            return new PredictionResultDto
            {
                RiskScore = riskScore,
                RiskLevel = riskLevel,
                ExplanationMessage = explanation,
                CreatedAt = prediction.CreatedAt,
                UserId = user.Id
            };
        }

        private async Task CompletePredictionAsync(Guid userId, Prediction prediction)
        {
            await _predictionRepository.AddAsync(prediction);
            await _dashboardService.InvalidatePredictionCachesAsync(userId);
            await _notificationService.NotifyPredictionCompletedAsync(userId, prediction.Id, prediction.RiskLevel, prediction.RiskScore);

            if (prediction.RiskLevel == "High")
            {
                await _notificationService.NotifyAdminAsync(
                    "HighRiskAlert",
                    "High Risk Credit Application",
                    "A user submitted a high-risk credit prediction that may require review.");
            }
        }
    }
}
