using CreditDefault.Api.Data;
using CreditDefault.Api.DTOs;
using Microsoft.EntityFrameworkCore;

namespace CreditDefault.Api.Services
{
    public class SearchService
    {
        private readonly AppDbContext _context;

        public SearchService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<PagedResult<UserListItemDto>> SearchUsersAsync(UserSearchRequest request)
        {
            var query = _context.Users
                .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
                .AsNoTracking()
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(request.Keyword))
            {
                var keyword = request.Keyword.Trim();
                query = query.Where(u => u.FullName.Contains(keyword) || u.Email.Contains(keyword));
            }

            if (!string.IsNullOrWhiteSpace(request.Role))
            {
                query = query.Where(u => u.Role == request.Role || u.UserRoles.Any(ur => ur.Role.Name == request.Role));
            }

            if (request.IsActive.HasValue) query = query.Where(u => u.IsActive == request.IsActive.Value);
            if (request.CreatedFrom.HasValue) query = query.Where(u => u.CreatedAt >= request.CreatedFrom.Value);
            if (request.CreatedTo.HasValue) query = query.Where(u => u.CreatedAt <= request.CreatedTo.Value);

            query = (request.SortBy?.ToLowerInvariant(), IsAscending(request)) switch
            {
                ("email", true) => query.OrderBy(u => u.Email),
                ("email", false) => query.OrderByDescending(u => u.Email),
                ("name", true) or ("fullname", true) => query.OrderBy(u => u.FullName),
                ("name", false) or ("fullname", false) => query.OrderByDescending(u => u.FullName),
                ("role", true) => query.OrderBy(u => u.Role),
                ("role", false) => query.OrderByDescending(u => u.Role),
                ("createdat", true) => query.OrderBy(u => u.CreatedAt),
                _ => query.OrderByDescending(u => u.CreatedAt)
            };

            return await ToPagedResultAsync(query.Select(u => new UserListItemDto
            {
                Id = u.Id,
                FullName = u.FullName,
                Email = u.Email,
                PhoneNumber = u.PhoneNumber,
                Role = u.UserRoles.Select(ur => ur.Role.Name).FirstOrDefault() ?? u.Role,
                Roles = u.UserRoles.Select(ur => ur.Role.Name).ToList(),
                IsActive = u.IsActive,
                CreatedAt = u.CreatedAt
            }), request);
        }

        public async Task<PagedResult<PredictionListItemDto>> SearchPredictionsAsync(PredictionSearchRequest request, Guid currentUserId, bool canViewAll)
        {
            var query = _context.Predictions
                .Include(p => p.User)
                .AsNoTracking()
                .AsQueryable();

            if (!canViewAll)
            {
                query = query.Where(p => p.UserId == currentUserId);
            }
            else if (request.UserId.HasValue)
            {
                query = query.Where(p => p.UserId == request.UserId.Value);
            }

            if (!string.IsNullOrWhiteSpace(request.Keyword))
            {
                var keyword = request.Keyword.Trim();
                query = query.Where(p =>
                    p.RiskLevel.Contains(keyword) ||
                    p.EmploymentStatus.Contains(keyword) ||
                    p.PaymentHistory.Contains(keyword) ||
                    p.User.FullName.Contains(keyword) ||
                    p.User.Email.Contains(keyword));
            }

            if (!string.IsNullOrWhiteSpace(request.RiskLevel)) query = query.Where(p => p.RiskLevel == request.RiskLevel);
            if (request.MinRiskScore.HasValue) query = query.Where(p => p.RiskScore >= request.MinRiskScore.Value);
            if (request.MaxRiskScore.HasValue) query = query.Where(p => p.RiskScore <= request.MaxRiskScore.Value);
            if (request.DateFrom.HasValue) query = query.Where(p => p.CreatedAt >= request.DateFrom.Value);
            if (request.DateTo.HasValue) query = query.Where(p => p.CreatedAt <= request.DateTo.Value);

            query = (request.SortBy?.ToLowerInvariant(), IsAscending(request)) switch
            {
                ("riskscore", true) => query.OrderBy(p => p.RiskScore),
                ("riskscore", false) => query.OrderByDescending(p => p.RiskScore),
                ("risklevel", true) => query.OrderBy(p => p.RiskLevel),
                ("risklevel", false) => query.OrderByDescending(p => p.RiskLevel),
                ("loanamount", true) => query.OrderBy(p => p.LoanAmount),
                ("loanamount", false) => query.OrderByDescending(p => p.LoanAmount),
                ("createdat", true) => query.OrderBy(p => p.CreatedAt),
                _ => query.OrderByDescending(p => p.CreatedAt)
            };

            return await ToPagedResultAsync(query.Select(ToPredictionListItem()), request);
        }

        public async Task<PagedResult<ClientProfileListItemDto>> SearchClientProfilesAsync(ClientProfileSearchRequest request, Guid currentUserId, bool canViewAll)
        {
            var query = _context.ClientProfiles
                .Include(p => p.User)
                .AsNoTracking()
                .AsQueryable();

            if (!canViewAll) query = query.Where(p => p.UserId == currentUserId);

            if (!string.IsNullOrWhiteSpace(request.Keyword))
            {
                var keyword = request.Keyword.Trim();
                query = query.Where(p => p.User.FullName.Contains(keyword) || p.User.Email.Contains(keyword) || p.EmploymentStatus.Contains(keyword));
            }

            if (!string.IsNullOrWhiteSpace(request.EmploymentStatus)) query = query.Where(p => p.EmploymentStatus == request.EmploymentStatus);
            if (request.MinMonthlyIncome.HasValue) query = query.Where(p => p.AnnualIncome / 12 >= request.MinMonthlyIncome.Value);
            if (request.MaxMonthlyIncome.HasValue) query = query.Where(p => p.AnnualIncome / 12 <= request.MaxMonthlyIncome.Value);
            if (request.MinDebtToIncomeRatio.HasValue) query = query.Where(p => p.DebtToIncomeRatio >= request.MinDebtToIncomeRatio.Value);
            if (request.MaxDebtToIncomeRatio.HasValue) query = query.Where(p => p.DebtToIncomeRatio <= request.MaxDebtToIncomeRatio.Value);

            query = (request.SortBy?.ToLowerInvariant(), IsAscending(request)) switch
            {
                ("annualincome", true) => query.OrderBy(p => p.AnnualIncome),
                ("annualincome", false) => query.OrderByDescending(p => p.AnnualIncome),
                ("creditscore", true) => query.OrderBy(p => p.CreditScore),
                ("creditscore", false) => query.OrderByDescending(p => p.CreditScore),
                ("debtoincomeratio", true) => query.OrderBy(p => p.DebtToIncomeRatio),
                ("debtoincomeratio", false) => query.OrderByDescending(p => p.DebtToIncomeRatio),
                ("createdat", true) => query.OrderBy(p => p.CreatedAt),
                _ => query.OrderByDescending(p => p.CreatedAt)
            };

            return await ToPagedResultAsync(query.Select(p => new ClientProfileListItemDto
            {
                Id = p.Id,
                UserId = p.UserId,
                UserName = p.User.FullName,
                UserEmail = p.User.Email,
                Age = p.Age,
                AnnualIncome = p.AnnualIncome,
                MonthlyIncome = p.AnnualIncome / 12,
                LoanAmount = p.LoanAmount,
                CreditScore = p.CreditScore,
                EmploymentStatus = p.EmploymentStatus,
                DebtToIncomeRatio = p.DebtToIncomeRatio,
                CreatedAt = p.CreatedAt
            }), request);
        }

        public async Task<PagedResult<NotificationListItemDto>> SearchNotificationsAsync(NotificationSearchRequest request, Guid currentUserId, bool canViewAll)
        {
            var query = _context.Notifications.AsNoTracking().AsQueryable();
            if (!canViewAll) query = query.Where(n => n.UserId == currentUserId);

            if (!string.IsNullOrWhiteSpace(request.Keyword))
            {
                var keyword = request.Keyword.Trim();
                query = query.Where(n => n.Title.Contains(keyword) || n.Message.Contains(keyword) || n.Type.Contains(keyword));
            }

            if (!string.IsNullOrWhiteSpace(request.Type)) query = query.Where(n => n.Type == request.Type);
            if (request.IsRead.HasValue) query = query.Where(n => n.IsRead == request.IsRead.Value);
            if (request.DateFrom.HasValue) query = query.Where(n => n.CreatedAt >= request.DateFrom.Value);
            if (request.DateTo.HasValue) query = query.Where(n => n.CreatedAt <= request.DateTo.Value);

            query = (request.SortBy?.ToLowerInvariant(), IsAscending(request)) switch
            {
                ("type", true) => query.OrderBy(n => n.Type),
                ("type", false) => query.OrderByDescending(n => n.Type),
                ("isread", true) => query.OrderBy(n => n.IsRead),
                ("isread", false) => query.OrderByDescending(n => n.IsRead),
                ("createdat", true) => query.OrderBy(n => n.CreatedAt),
                _ => query.OrderByDescending(n => n.CreatedAt)
            };

            return await ToPagedResultAsync(query.Select(n => new NotificationListItemDto
            {
                Id = n.Id,
                UserId = n.UserId,
                Title = n.Title,
                Message = n.Message,
                Type = n.Type,
                IsRead = n.IsRead,
                CreatedAt = n.CreatedAt
            }), request);
        }

        public async Task<PagedResult<ReportDto>> SearchReportsAsync(ReportSearchRequest request, Guid currentUserId, bool canViewAll)
        {
            var query = _context.Reports
                .Include(r => r.CreatedByUser)
                .Include(r => r.Exports)
                .AsNoTracking()
                .AsQueryable();

            if (!canViewAll) query = query.Where(r => r.CreatedByUserId == currentUserId);
            else if (request.GeneratedBy.HasValue) query = query.Where(r => r.CreatedByUserId == request.GeneratedBy.Value);

            if (!string.IsNullOrWhiteSpace(request.Keyword))
            {
                var keyword = request.Keyword.Trim();
                query = query.Where(r => r.Name.Contains(keyword) || r.ReportType.Contains(keyword) || r.CreatedByUser.Email.Contains(keyword));
            }

            if (!string.IsNullOrWhiteSpace(request.ReportType)) query = query.Where(r => r.ReportType == request.ReportType);
            if (!string.IsNullOrWhiteSpace(request.Format)) query = query.Where(r => r.Exports.Any(e => e.Format == request.Format));
            if (request.DateFrom.HasValue) query = query.Where(r => r.CreatedAt >= request.DateFrom.Value);
            if (request.DateTo.HasValue) query = query.Where(r => r.CreatedAt <= request.DateTo.Value);

            query = (request.SortBy?.ToLowerInvariant(), IsAscending(request)) switch
            {
                ("name", true) => query.OrderBy(r => r.Name),
                ("name", false) => query.OrderByDescending(r => r.Name),
                ("reporttype", true) => query.OrderBy(r => r.ReportType),
                ("reporttype", false) => query.OrderByDescending(r => r.ReportType),
                ("createdat", true) => query.OrderBy(r => r.CreatedAt),
                _ => query.OrderByDescending(r => r.CreatedAt)
            };

            return await ToPagedResultAsync(query.Select(r => new ReportDto
            {
                Id = r.Id,
                Name = r.Name,
                ReportType = r.ReportType,
                CreatedByUserId = r.CreatedByUserId,
                CreatedBy = r.CreatedByUser.Email,
                Format = r.Exports.OrderByDescending(e => e.ExportedAt).Select(e => e.Format).FirstOrDefault(),
                CreatedAt = r.CreatedAt
            }), request);
        }

        public static System.Linq.Expressions.Expression<Func<Models.Prediction, PredictionListItemDto>> ToPredictionListItem() =>
            p => new PredictionListItemDto
            {
                Id = p.Id,
                UserId = p.UserId,
                UserName = p.User.FullName,
                UserEmail = p.User.Email,
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

        private static async Task<PagedResult<T>> ToPagedResultAsync<T>(IQueryable<T> query, SearchRequest request)
        {
            var page = Math.Max(1, request.Page);
            var pageSize = Math.Clamp(request.PageSize, 1, 100);
            var total = await query.CountAsync();
            var items = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();
            return new PagedResult<T>
            {
                Items = items,
                Page = page,
                PageSize = pageSize,
                TotalCount = total
            };
        }

        private static bool IsAscending(SearchRequest request) =>
            string.Equals(request.SortDirection, "asc", StringComparison.OrdinalIgnoreCase);
    }
}
