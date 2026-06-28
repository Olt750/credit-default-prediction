using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace CreditDefault.Api.DTOs
{
    public class PagedResult<T>
    {
        public IReadOnlyList<T> Items { get; set; } = [];
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int TotalCount { get; set; }
        public int TotalPages => PageSize <= 0 ? 0 : (int)Math.Ceiling(TotalCount / (double)PageSize);
    }

    public class SearchRequest
    {
        public string? Keyword { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public string? SortBy { get; set; }
        public string? SortDirection { get; set; } = "desc";
    }

    public class UserSearchRequest : SearchRequest
    {
        public string? Role { get; set; }
        public bool? IsActive { get; set; }
        public DateTime? CreatedFrom { get; set; }
        public DateTime? CreatedTo { get; set; }
    }

    public class PredictionSearchRequest : SearchRequest
    {
        public string? RiskLevel { get; set; }
        public int? MinRiskScore { get; set; }
        public int? MaxRiskScore { get; set; }
        public DateTime? DateFrom { get; set; }
        public DateTime? DateTo { get; set; }
        public Guid? UserId { get; set; }
    }

    public class ClientProfileSearchRequest : SearchRequest
    {
        public string? EmploymentStatus { get; set; }
        public decimal? MinMonthlyIncome { get; set; }
        public decimal? MaxMonthlyIncome { get; set; }
        public decimal? MinDebtToIncomeRatio { get; set; }
        public decimal? MaxDebtToIncomeRatio { get; set; }
    }

    public class NotificationSearchRequest : SearchRequest
    {
        public string? Type { get; set; }
        public bool? IsRead { get; set; }
        public DateTime? DateFrom { get; set; }
        public DateTime? DateTo { get; set; }
    }

    public class ReportSearchRequest : SearchRequest
    {
        public string? ReportType { get; set; }
        public string? Format { get; set; }
        public DateTime? DateFrom { get; set; }
        public DateTime? DateTo { get; set; }
        public Guid? GeneratedBy { get; set; }
    }

    public class ReportFilterRequest
    {
        public DateTime? DateFrom { get; set; }
        public DateTime? DateTo { get; set; }
        public string? RiskLevel { get; set; }
        public int? MinRiskScore { get; set; }
        public int? MaxRiskScore { get; set; }
        public string? LoanType { get; set; }
        public string? EmploymentStatus { get; set; }
        public Guid? UserId { get; set; }
        public string ReportType { get; set; } = "Prediction Summary Report";
        public string Format { get; set; } = "csv";
    }

    public class ReportSummaryDto
    {
        public int TotalPredictions { get; set; }
        public int LowRiskCount { get; set; }
        public int MediumRiskCount { get; set; }
        public int HighRiskCount { get; set; }
        public double AverageRiskScore { get; set; }
        public int HighestRiskScore { get; set; }
        public int LowestRiskScore { get; set; }
        public Dictionary<string, int> RiskDistribution { get; set; } = new();
        public IReadOnlyList<PredictionListItemDto> RecentPredictions { get; set; } = [];
        public IReadOnlyList<PredictionListItemDto> HighRiskPredictions { get; set; } = [];
        public IReadOnlyList<ModelMetricDto> ModelMetrics { get; set; } = [];
    }

    public class ReportDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string ReportType { get; set; } = string.Empty;
        public Guid CreatedByUserId { get; set; }
        public string CreatedBy { get; set; } = string.Empty;
        public string? Format { get; set; }
        public DateTime CreatedAt { get; set; }
        public ReportFilterRequest? Filters { get; set; }
        public ReportSummaryDto? Summary { get; set; }
    }

    public class UserListItemDto
    {
        public Guid Id { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public IReadOnlyList<string> Roles { get; set; } = [];
        public DateTime CreatedAt { get; set; }
    }

    public class PredictionListItemDto
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string UserEmail { get; set; } = string.Empty;
        public decimal LoanAmount { get; set; }
        public decimal Income { get; set; }
        public string EmploymentStatus { get; set; } = string.Empty;
        public int CreditScore { get; set; }
        public int RiskScore { get; set; }
        public string RiskLevel { get; set; } = string.Empty;
        public string LoanStatus { get; set; } = string.Empty;
        public string ExplanationMessage { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }

    public class ClientProfileListItemDto
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string UserEmail { get; set; } = string.Empty;
        public int Age { get; set; }
        public decimal AnnualIncome { get; set; }
        public decimal MonthlyIncome { get; set; }
        public decimal LoanAmount { get; set; }
        public int CreditScore { get; set; }
        public string EmploymentStatus { get; set; } = string.Empty;
        public decimal DebtToIncomeRatio { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class NotificationListItemDto
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public bool IsRead { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class ModelMetricDto
    {
        public string MetricName { get; set; } = string.Empty;
        public decimal MetricValue { get; set; }
        public string ModelName { get; set; } = string.Empty;
        public string ModelVersion { get; set; } = string.Empty;
    }

    public class ImportResultDto
    {
        public int TotalRows { get; set; }
        public int InsertedRows { get; set; }
        public int SkippedRows { get; set; }
        public int FailedRows { get; set; }
        public List<string> Errors { get; set; } = [];
    }

    public class ExportFileDto
    {
        public byte[] Content { get; set; } = [];
        public string ContentType { get; set; } = "application/octet-stream";
        public string FileName { get; set; } = "export.dat";
    }
}
