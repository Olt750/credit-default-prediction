using CreditDefault.Api.Data;
using CreditDefault.Api.DTOs;
using Microsoft.EntityFrameworkCore;

namespace CreditDefault.Api.Services
{
    public class DataExportService
    {
        private readonly AppDbContext _context;
        private readonly TabularFileService _fileService;

        public DataExportService(AppDbContext context, TabularFileService fileService)
        {
            _context = context;
            _fileService = fileService;
        }

        public async Task<ExportFileDto> ExportAsync(string dataType, string format, Guid currentUserId, bool isAdmin)
        {
            var rows = dataType.ToLowerInvariant() switch
            {
                "users" when isAdmin => await ExportUsersAsync(),
                "predictions" => await ExportPredictionsAsync(currentUserId, isAdmin),
                "client-profiles" => await ExportClientProfilesAsync(currentUserId, isAdmin),
                "notifications" => await ExportNotificationsAsync(currentUserId, isAdmin),
                "reports" => await ExportReportsAsync(currentUserId, isAdmin),
                "audit-logs" when isAdmin => await ExportAuditLogsAsync(),
                _ => throw new UnauthorizedAccessException("You are not allowed to export this data type.")
            };

            return _fileService.CreateFile($"{dataType}-{DateTime.UtcNow:yyyyMMddHHmmss}", format, rows);
        }

        private async Task<List<IDictionary<string, object?>>> ExportUsersAsync()
        {
            var users = await _context.Users
                .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
                .AsNoTracking()
                .OrderByDescending(u => u.CreatedAt)
                .ToListAsync();

            return users.Select(u => (IDictionary<string, object?>)new Dictionary<string, object?>
            {
                ["Id"] = u.Id,
                ["FullName"] = u.FullName,
                ["Email"] = u.Email,
                ["PhoneNumber"] = u.PhoneNumber,
                ["Role"] = u.UserRoles.Select(ur => ur.Role.Name).FirstOrDefault() ?? u.Role,
                ["Roles"] = string.Join(";", u.UserRoles.Select(ur => ur.Role.Name)),
                ["CreatedAt"] = u.CreatedAt
            }).ToList();
        }

        private async Task<List<IDictionary<string, object?>>> ExportPredictionsAsync(Guid currentUserId, bool isAdmin)
        {
            var query = _context.Predictions.Include(p => p.User).AsNoTracking().AsQueryable();
            if (!isAdmin) query = query.Where(p => p.UserId == currentUserId);
            var predictions = await query.OrderByDescending(p => p.CreatedAt).ToListAsync();

            return predictions.Select(p => (IDictionary<string, object?>)new Dictionary<string, object?>
            {
                ["Id"] = p.Id,
                ["UserId"] = p.UserId,
                ["UserEmail"] = p.User.Email,
                ["LoanAmount"] = p.LoanAmount,
                ["Income"] = p.Income,
                ["EmploymentStatus"] = p.EmploymentStatus,
                ["CreditScore"] = p.CreditScore,
                ["RiskScore"] = p.RiskScore,
                ["RiskLevel"] = p.RiskLevel,
                ["CreatedAt"] = p.CreatedAt
            }).ToList();
        }

        private async Task<List<IDictionary<string, object?>>> ExportClientProfilesAsync(Guid currentUserId, bool isAdmin)
        {
            var query = _context.ClientProfiles.Include(p => p.User).AsNoTracking().AsQueryable();
            if (!isAdmin) query = query.Where(p => p.UserId == currentUserId);
            var profiles = await query.OrderByDescending(p => p.CreatedAt).ToListAsync();

            return profiles.Select(p => (IDictionary<string, object?>)new Dictionary<string, object?>
            {
                ["Id"] = p.Id,
                ["UserId"] = p.UserId,
                ["UserEmail"] = p.User.Email,
                ["Age"] = p.Age,
                ["AnnualIncome"] = p.AnnualIncome,
                ["LoanAmount"] = p.LoanAmount,
                ["CreditScore"] = p.CreditScore,
                ["EmploymentStatus"] = p.EmploymentStatus,
                ["DebtToIncomeRatio"] = p.DebtToIncomeRatio,
                ["CreatedAt"] = p.CreatedAt
            }).ToList();
        }

        private async Task<List<IDictionary<string, object?>>> ExportNotificationsAsync(Guid currentUserId, bool isAdmin)
        {
            var query = _context.Notifications.AsNoTracking().AsQueryable();
            if (!isAdmin) query = query.Where(n => n.UserId == currentUserId);
            var notifications = await query.OrderByDescending(n => n.CreatedAt).ToListAsync();

            return notifications.Select(n => (IDictionary<string, object?>)new Dictionary<string, object?>
            {
                ["Id"] = n.Id,
                ["UserId"] = n.UserId,
                ["Type"] = n.Type,
                ["Title"] = n.Title,
                ["Message"] = n.Message,
                ["IsRead"] = n.IsRead,
                ["CreatedAt"] = n.CreatedAt
            }).ToList();
        }

        private async Task<List<IDictionary<string, object?>>> ExportReportsAsync(Guid currentUserId, bool isAdmin)
        {
            var query = _context.Reports.Include(r => r.CreatedByUser).Include(r => r.Exports).AsNoTracking().AsQueryable();
            if (!isAdmin) query = query.Where(r => r.CreatedByUserId == currentUserId);
            var reports = await query.OrderByDescending(r => r.CreatedAt).ToListAsync();

            return reports.Select(r => (IDictionary<string, object?>)new Dictionary<string, object?>
            {
                ["Id"] = r.Id,
                ["Name"] = r.Name,
                ["ReportType"] = r.ReportType,
                ["CreatedByUserId"] = r.CreatedByUserId,
                ["CreatedByEmail"] = r.CreatedByUser.Email,
                ["Format"] = r.Exports.OrderByDescending(e => e.ExportedAt).Select(e => e.Format).FirstOrDefault(),
                ["CreatedAt"] = r.CreatedAt
            }).ToList();
        }

        private async Task<List<IDictionary<string, object?>>> ExportAuditLogsAsync()
        {
            var logs = await _context.AuditLogs.AsNoTracking().OrderByDescending(a => a.CreatedAt).Take(5000).ToListAsync();
            return logs.Select(a => (IDictionary<string, object?>)new Dictionary<string, object?>
            {
                ["Id"] = a.Id,
                ["UserId"] = a.UserId,
                ["PerformedBy"] = a.PerformedBy,
                ["Action"] = a.Action,
                ["EntityName"] = a.EntityName,
                ["EntityId"] = a.EntityId,
                ["IpAddress"] = a.IpAddress,
                ["CreatedAt"] = a.CreatedAt
            }).ToList();
        }
    }
}
