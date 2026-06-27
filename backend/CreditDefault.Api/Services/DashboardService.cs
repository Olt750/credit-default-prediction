using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using CreditDefault.Api.Data;
using CreditDefault.Api.DTOs;
using Microsoft.EntityFrameworkCore;

namespace CreditDefault.Api.Services
{
    public class DashboardService
    {
        private readonly AppDbContext _context;

        public DashboardService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<DashboardSummaryDto> GetSummaryAsync(Guid userId, bool isAdmin)
        {
            var predictions = GetPredictionsScope(userId, isAdmin);
            var totalPredictions = await predictions.CountAsync();
            var avgRisk = totalPredictions == 0
                ? 0
                : await predictions.AverageAsync(p => p.RiskScore);

            return new DashboardSummaryDto
            {
                TotalClients = isAdmin ? await _context.Users.CountAsync() : 1,
                HighRiskClients = await predictions
                    .Where(p => p.RiskLevel == "High")
                    .Select(p => p.UserId)
                    .Distinct()
                    .CountAsync(),
                ApprovedLoans = await predictions.CountAsync(p => p.RiskLevel == "Low"),
                AvgDefaultRisk = Math.Round((decimal)avgRisk, 1)
            };
        }

        public async Task<IReadOnlyList<DashboardChartItemDto>> GetRiskDistributionAsync(Guid userId, bool isAdmin)
        {
            var predictions = GetPredictionsScope(userId, isAdmin);
            var grouped = await predictions
                .GroupBy(p => p.RiskLevel)
                .Select(g => new { Level = g.Key, Count = g.Count() })
                .ToListAsync();

            return new[]
            {
                new DashboardChartItemDto { Name = "Low", Value = grouped.FirstOrDefault(g => g.Level == "Low")?.Count ?? 0, Color = "var(--color-success)" },
                new DashboardChartItemDto { Name = "Medium", Value = grouped.FirstOrDefault(g => g.Level == "Medium")?.Count ?? 0, Color = "var(--color-warning)" },
                new DashboardChartItemDto { Name = "High", Value = grouped.FirstOrDefault(g => g.Level == "High")?.Count ?? 0, Color = "var(--color-destructive)" }
            };
        }

        public async Task<IReadOnlyList<DashboardMonthlyActivityDto>> GetMonthlyActivityAsync(Guid userId, bool isAdmin)
        {
            var start = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1).AddMonths(-5);
            var predictions = GetPredictionsScope(userId, isAdmin).Where(p => p.CreatedAt >= start);
            var grouped = await predictions
                .GroupBy(p => new { p.CreatedAt.Year, p.CreatedAt.Month })
                .Select(g => new
                {
                    g.Key.Year,
                    g.Key.Month,
                    Predictions = g.Count(),
                    Approved = g.Count(p => p.RiskLevel == "Low")
                })
                .ToListAsync();

            return Enumerable.Range(0, 6)
                .Select(offset => start.AddMonths(offset))
                .Select(month =>
                {
                    var item = grouped.FirstOrDefault(g => g.Year == month.Year && g.Month == month.Month);
                    return new DashboardMonthlyActivityDto
                    {
                        Month = month.ToString("MMM", CultureInfo.InvariantCulture),
                        Predictions = item?.Predictions ?? 0,
                        Approved = item?.Approved ?? 0
                    };
                })
                .ToList();
        }

        public async Task<IReadOnlyList<DashboardLoanStatusDto>> GetLoanStatusAsync(Guid userId, bool isAdmin)
        {
            var predictions = GetPredictionsScope(userId, isAdmin);

            return new[]
            {
                new DashboardLoanStatusDto { Status = "Approved", Count = await predictions.CountAsync(p => p.RiskLevel == "Low") },
                new DashboardLoanStatusDto { Status = "Pending", Count = await predictions.CountAsync(p => p.RiskLevel == "Medium") },
                new DashboardLoanStatusDto { Status = "Rejected", Count = await predictions.CountAsync(p => p.RiskLevel == "High") }
            };
        }

        public async Task<IReadOnlyList<DashboardPredictionDto>> GetRecentPredictionsAsync(Guid userId, bool isAdmin, int limit = 5)
        {
            return await GetPredictionsScope(userId, isAdmin)
                .Include(p => p.User)
                .OrderByDescending(p => p.CreatedAt)
                .Take(Math.Clamp(limit, 1, 50))
                .Select(p => new DashboardPredictionDto
                {
                    Id = p.Id,
                    Client = p.User.FullName,
                    Amount = p.LoanAmount,
                    Score = p.RiskScore,
                    Level = p.RiskLevel,
                    Date = p.CreatedAt,
                    Status = p.RiskLevel == "Low" ? "Approved" : p.RiskLevel == "Medium" ? "Pending" : "Rejected",
                    Explanation = p.ExplanationMessage
                })
                .ToListAsync();
        }

        private IQueryable<CreditDefault.Api.Models.Prediction> GetPredictionsScope(Guid userId, bool isAdmin)
        {
            var query = _context.Predictions.AsQueryable();
            return isAdmin ? query : query.Where(p => p.UserId == userId);
        }
    }
}
