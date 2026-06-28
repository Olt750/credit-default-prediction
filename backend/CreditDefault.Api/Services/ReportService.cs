using System.Text.Json;
using CreditDefault.Api.Data;
using CreditDefault.Api.DTOs;
using CreditDefault.Api.Hubs;
using CreditDefault.Api.Interfaces;
using CreditDefault.Api.Models;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace CreditDefault.Api.Services
{
    public class ReportService
    {
        private readonly AppDbContext _context;
        private readonly TabularFileService _fileService;
        private readonly ICacheService _cache;
        private readonly NotificationService _notificationService;
        private readonly IHubContext<NotificationsHub> _hubContext;
        private readonly IWebHostEnvironment _environment;

        public ReportService(
            AppDbContext context,
            TabularFileService fileService,
            ICacheService cache,
            NotificationService notificationService,
            IHubContext<NotificationsHub> hubContext,
            IWebHostEnvironment environment)
        {
            _context = context;
            _fileService = fileService;
            _cache = cache;
            _notificationService = notificationService;
            _hubContext = hubContext;
            _environment = environment;
        }

        public async Task<PagedResult<ReportDto>> GetReportsAsync(ReportSearchRequest request, Guid currentUserId, bool canViewAll, SearchService searchService) =>
            await searchService.SearchReportsAsync(request, currentUserId, canViewAll);

        public async Task<ReportDto?> GetReportAsync(Guid id, Guid currentUserId, bool canViewAll)
        {
            var report = await _context.Reports
                .Include(r => r.CreatedByUser)
                .Include(r => r.Exports)
                .AsNoTracking()
                .FirstOrDefaultAsync(r => r.Id == id);

            if (report == null || (!canViewAll && report.CreatedByUserId != currentUserId)) return null;
            return ToDto(report, includePayload: true);
        }

        public async Task<ReportDto> GenerateAsync(ReportFilterRequest filters, Guid currentUserId, bool canViewAll)
        {
            if (!canViewAll)
            {
                filters.UserId = currentUserId;
            }

            filters.Format = TabularFileService.NormalizeFormat(filters.Format);
            var summary = await BuildSummaryAsync(filters);
            var rows = BuildReportRows(summary, filters.ReportType);
            var file = _fileService.CreateFile($"report-{DateTime.UtcNow:yyyyMMddHHmmss}", filters.Format, rows);
            var savedFile = await SaveReportFileAsync(file, currentUserId);

            var payload = new StoredReportPayload
            {
                Filters = filters,
                Summary = summary
            };

            var report = new Report
            {
                Id = Guid.NewGuid(),
                CreatedByUserId = currentUserId,
                Name = $"{filters.ReportType} - {DateTime.UtcNow:yyyy-MM-dd HH:mm}",
                ReportType = filters.ReportType,
                ParametersJson = JsonSerializer.Serialize(payload),
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            report.Exports.Add(new ReportExport
            {
                Id = Guid.NewGuid(),
                ReportId = report.Id,
                FileRecordId = savedFile.Id,
                FileRecord = savedFile,
                Format = filters.Format,
                ExportedAt = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                CreatedBy = currentUserId
            });

            _context.Reports.Add(report);
            await _context.SaveChangesAsync();
            await _cache.RemoveAsync($"reports:summary:{currentUserId}");

            await _notificationService.CreateNotificationAsync(
                currentUserId,
                "ReportGenerated",
                "Report Generated",
                $"{filters.ReportType} is ready to download.");

            await _hubContext.Clients.Group(NotificationsHub.UserGroup(currentUserId.ToString()))
                .SendAsync("ReportGenerated", new { report.Id, report.Name, report.ReportType, summary });

            if (summary.HighRiskCount > 0 && canViewAll)
            {
                await _notificationService.NotifyAdminAsync(
                    "HighRiskReportGenerated",
                    "High-Risk Report Generated",
                    $"A report containing {summary.HighRiskCount} high-risk predictions was generated.");
            }

            return ToDto(report, includePayload: true);
        }

        public async Task<ExportFileDto?> DownloadAsync(Guid id, Guid currentUserId, bool canViewAll)
        {
            var export = await _context.ReportExports
                .Include(e => e.Report)
                .Include(e => e.FileRecord)
                .OrderByDescending(e => e.ExportedAt)
                .FirstOrDefaultAsync(e => e.ReportId == id);

            if (export?.Report == null || export.FileRecord == null) return null;
            if (!canViewAll && export.Report.CreatedByUserId != currentUserId) return null;

            if (!File.Exists(export.FileRecord.StoragePath)) return null;
            return new ExportFileDto
            {
                Content = await File.ReadAllBytesAsync(export.FileRecord.StoragePath),
                ContentType = export.FileRecord.ContentType,
                FileName = export.FileRecord.OriginalFileName
            };
        }

        public async Task<bool> DeleteAsync(Guid id, Guid currentUserId, bool canViewAll)
        {
            var report = await _context.Reports
                .Include(r => r.Exports)
                .ThenInclude(e => e.FileRecord)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (report == null || (!canViewAll && report.CreatedByUserId != currentUserId)) return false;

            foreach (var export in report.Exports)
            {
                if (export.FileRecord != null && File.Exists(export.FileRecord.StoragePath))
                {
                    File.Delete(export.FileRecord.StoragePath);
                }
            }

            _context.Reports.Remove(report);
            await _context.SaveChangesAsync();
            await _cache.RemoveAsync($"reports:summary:{currentUserId}");
            return true;
        }

        private async Task<ReportSummaryDto> BuildSummaryAsync(ReportFilterRequest filters)
        {
            var cacheKey = $"reports:summary:{JsonSerializer.Serialize(filters)}";
            var cached = await _cache.GetAsync<ReportSummaryDto>(cacheKey);
            if (cached != null) return cached;

            var query = _context.Predictions
                .Include(p => p.User)
                .AsNoTracking()
                .AsQueryable();

            if (filters.UserId.HasValue) query = query.Where(p => p.UserId == filters.UserId.Value);
            if (filters.DateFrom.HasValue) query = query.Where(p => p.CreatedAt >= filters.DateFrom.Value);
            if (filters.DateTo.HasValue) query = query.Where(p => p.CreatedAt <= filters.DateTo.Value);
            if (!string.IsNullOrWhiteSpace(filters.RiskLevel)) query = query.Where(p => p.RiskLevel == filters.RiskLevel);
            if (filters.MinRiskScore.HasValue) query = query.Where(p => p.RiskScore >= filters.MinRiskScore.Value);
            if (filters.MaxRiskScore.HasValue) query = query.Where(p => p.RiskScore <= filters.MaxRiskScore.Value);
            if (!string.IsNullOrWhiteSpace(filters.EmploymentStatus)) query = query.Where(p => p.EmploymentStatus == filters.EmploymentStatus);

            var predictions = await query.ToListAsync();
            var metrics = await _context.ModelMetrics
                .Include(m => m.ModelRun)
                .AsNoTracking()
                .OrderByDescending(m => m.CreatedAt)
                .Take(20)
                .Select(m => new ModelMetricDto
                {
                    MetricName = m.MetricName,
                    MetricValue = m.MetricValue,
                    ModelName = m.ModelRun.ModelName,
                    ModelVersion = m.ModelRun.ModelVersion
                })
                .ToListAsync();

            var summary = new ReportSummaryDto
            {
                TotalPredictions = predictions.Count,
                LowRiskCount = predictions.Count(p => p.RiskLevel == "Low"),
                MediumRiskCount = predictions.Count(p => p.RiskLevel == "Medium"),
                HighRiskCount = predictions.Count(p => p.RiskLevel == "High"),
                AverageRiskScore = predictions.Count == 0 ? 0 : Math.Round(predictions.Average(p => p.RiskScore), 2),
                HighestRiskScore = predictions.Count == 0 ? 0 : predictions.Max(p => p.RiskScore),
                LowestRiskScore = predictions.Count == 0 ? 0 : predictions.Min(p => p.RiskScore),
                RiskDistribution = predictions.GroupBy(p => p.RiskLevel).ToDictionary(g => g.Key, g => g.Count()),
                RecentPredictions = predictions.OrderByDescending(p => p.CreatedAt).Take(10).Select(ToListItem).ToList(),
                HighRiskPredictions = predictions.Where(p => p.RiskLevel == "High").OrderByDescending(p => p.RiskScore).Take(10).Select(ToListItem).ToList(),
                ModelMetrics = metrics
            };

            await _cache.SetAsync(cacheKey, summary, TimeSpan.FromMinutes(5));
            return summary;
        }

        private async Task<FileRecord> SaveReportFileAsync(ExportFileDto file, Guid userId)
        {
            var directory = Path.Combine(_environment.ContentRootPath, "exports", "reports");
            Directory.CreateDirectory(directory);
            var safeName = $"{Guid.NewGuid()}-{file.FileName}";
            var path = Path.Combine(directory, safeName);
            await File.WriteAllBytesAsync(path, file.Content);

            return new FileRecord
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                FileName = safeName,
                OriginalFileName = file.FileName,
                ContentType = file.ContentType,
                SizeBytes = file.Content.LongLength,
                StoragePath = path,
                Category = "ReportExport",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                CreatedBy = userId
            };
        }

        private static List<IDictionary<string, object?>> BuildReportRows(ReportSummaryDto summary, string reportType)
        {
            var rows = new List<IDictionary<string, object?>>
            {
                new Dictionary<string, object?>
                {
                    ["ReportType"] = reportType,
                    ["TotalPredictions"] = summary.TotalPredictions,
                    ["LowRiskCount"] = summary.LowRiskCount,
                    ["MediumRiskCount"] = summary.MediumRiskCount,
                    ["HighRiskCount"] = summary.HighRiskCount,
                    ["AverageRiskScore"] = summary.AverageRiskScore,
                    ["HighestRiskScore"] = summary.HighestRiskScore,
                    ["LowestRiskScore"] = summary.LowestRiskScore
                }
            };

            foreach (var prediction in summary.RecentPredictions)
            {
                rows.Add(new Dictionary<string, object?>
                {
                    ["PredictionId"] = prediction.Id,
                    ["UserEmail"] = prediction.UserEmail,
                    ["LoanAmount"] = prediction.LoanAmount,
                    ["RiskScore"] = prediction.RiskScore,
                    ["RiskLevel"] = prediction.RiskLevel,
                    ["CreatedAt"] = prediction.CreatedAt
                });
            }

            return rows;
        }

        private static PredictionListItemDto ToListItem(Prediction p) => new()
        {
            Id = p.Id,
            UserId = p.UserId,
            UserName = p.User?.FullName ?? string.Empty,
            UserEmail = p.User?.Email ?? string.Empty,
            LoanAmount = p.LoanAmount,
            Income = p.Income,
            EmploymentStatus = p.EmploymentStatus,
            CreditScore = p.CreditScore,
            RiskScore = p.RiskScore,
            RiskLevel = p.RiskLevel,
            LoanStatus = p.LoanStatus,
            ExplanationMessage = p.ExplanationMessage,
            CreatedAt = p.CreatedAt
        };

        private static ReportDto ToDto(Report report, bool includePayload)
        {
            StoredReportPayload? payload = null;
            if (includePayload)
            {
                payload = JsonSerializer.Deserialize<StoredReportPayload>(report.ParametersJson, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
            }

            return new ReportDto
            {
                Id = report.Id,
                Name = report.Name,
                ReportType = report.ReportType,
                CreatedByUserId = report.CreatedByUserId,
                CreatedBy = report.CreatedByUser?.Email ?? string.Empty,
                Format = report.Exports.OrderByDescending(e => e.ExportedAt).Select(e => e.Format).FirstOrDefault(),
                CreatedAt = report.CreatedAt,
                Filters = payload?.Filters,
                Summary = payload?.Summary
            };
        }

        private class StoredReportPayload
        {
            public ReportFilterRequest Filters { get; set; } = new();
            public ReportSummaryDto Summary { get; set; } = new();
        }
    }
}
