using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using CreditDefault.Api.Data;
using CreditDefault.Api.DTOs;
using CreditDefault.Api.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace CreditDefault.Api.Services
{
    public class DashboardService
    {
        private readonly AppDbContext _context;
        private readonly ICacheService _cache;

        public DashboardService(AppDbContext context, ICacheService cache)
        {
            _context = context;
            _cache = cache;
        }

        public async Task<DashboardSummaryDto> GetSummaryAsync(Guid userId, bool isAdmin)
        {
            var key = isAdmin ? CacheKeys.DashboardAdmin : CacheKeys.DashboardUser(userId);
            var cached = await _cache.GetAsync<DashboardSummaryDto>(key);
            if (cached != null) return cached;

            var predictions = GetPredictionsScope(userId, isAdmin);
            var totalPredictions = await predictions.CountAsync();
            var avgRisk = totalPredictions == 0
                ? 0
                : await predictions.AverageAsync(p => p.RiskScore);

            var summary = new DashboardSummaryDto
            {
                TotalClients = isAdmin
                    ? await _context.ClientProfiles.CountAsync()
                    : await _context.ClientProfiles.CountAsync(p => p.UserId == userId),
                HighRiskClients = await predictions
                    .Where(p => p.RiskLevel == "High")
                    .Select(p => p.UserId)
                    .Distinct()
                    .CountAsync(),
                ApprovedLoans = await predictions.CountAsync(p => p.RiskLevel == "Low"),
                AverageDefaultRisk = Math.Round((decimal)avgRisk, 1)
            };

            await _cache.SetAsync(key, summary, TimeSpan.FromMinutes(5));
            return summary;
        }

        public async Task<DashboardDto> GetDashboardAsync(Guid userId, bool isAdmin)
        {
            return new DashboardDto
            {
                Summary = await GetSummaryAsync(userId, isAdmin),
                RiskDistribution = await GetRiskDistributionAsync(userId, isAdmin),
                MonthlyActivity = await GetMonthlyActivityAsync(userId, isAdmin),
                LoanStatusSummary = await GetLoanStatusAsync(userId, isAdmin),
                RecentPredictions = await GetRecentPredictionsAsync(userId, isAdmin)
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
                new DashboardLoanStatusDto { Status = "Approved", Count = await predictions.CountAsync(p => p.LoanStatus == "Approved" || p.RiskLevel == "Low") },
                new DashboardLoanStatusDto { Status = "Pending", Count = await predictions.CountAsync(p => p.LoanStatus == "Pending" || p.RiskLevel == "Medium") },
                new DashboardLoanStatusDto { Status = "Rejected", Count = await predictions.CountAsync(p => p.LoanStatus == "Rejected" || p.RiskLevel == "High") }
            };
        }

        public async Task<IReadOnlyList<DashboardPredictionDto>> GetRecentPredictionsAsync(Guid userId, bool isAdmin, int limit = 5)
        {
            var safeLimit = Math.Clamp(limit, 1, 50);
            var key = CacheKeys.PredictionsRecent(userId, isAdmin, safeLimit);
            var cached = await _cache.GetAsync<List<DashboardPredictionDto>>(key);
            if (cached != null) return cached;

            var predictions = await GetPredictionsScope(userId, isAdmin)
                .Include(p => p.User)
                .OrderByDescending(p => p.CreatedAt)
                .Take(safeLimit)
                .Select(p => new DashboardPredictionDto
                {
                    Id = p.Id,
                    Client = p.User.FullName,
                    Amount = p.LoanAmount,
                    Score = p.RiskScore,
                    Level = p.RiskLevel,
                    Date = p.CreatedAt,
                    Status = p.LoanStatus == "" ? p.RiskLevel == "Low" ? "Approved" : p.RiskLevel == "Medium" ? "Pending" : "Rejected" : p.LoanStatus,
                    Explanation = p.ExplanationMessage
                })
                .ToListAsync();

            await _cache.SetAsync(key, predictions, TimeSpan.FromMinutes(2));
            return predictions;
        }

        public async Task InvalidatePredictionCachesAsync(Guid userId)
        {
            await _cache.RemoveAsync(CacheKeys.DashboardUser(userId));
            await _cache.RemoveAsync(CacheKeys.DashboardAdmin);
            foreach (var limit in new[] { 5, 10, 20, 50 })
            {
                await _cache.RemoveAsync(CacheKeys.PredictionsRecent(userId, false, limit));
                await _cache.RemoveAsync(CacheKeys.PredictionsRecent(userId, true, limit));
            }
        }

        private IQueryable<CreditDefault.Api.Models.Prediction> GetPredictionsScope(Guid userId, bool isAdmin)
        {
            var query = _context.Predictions.AsQueryable();
            return isAdmin ? query : query.Where(p => p.UserId == userId);
        }
    }
}
