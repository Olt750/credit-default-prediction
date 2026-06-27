using System;

namespace CreditDefault.Api.DTOs
{
    public class DashboardSummaryDto
    {
        public int TotalClients { get; set; }
        public int HighRiskClients { get; set; }
        public int ApprovedLoans { get; set; }
        public decimal AverageDefaultRisk { get; set; }
        public decimal AvgDefaultRisk
        {
            get => AverageDefaultRisk;
            set => AverageDefaultRisk = value;
        }
    }

    public class DashboardChartItemDto
    {
        public string Name { get; set; } = string.Empty;
        public int Value { get; set; }
        public string Color { get; set; } = string.Empty;
    }

    public class DashboardMonthlyActivityDto
    {
        public string Month { get; set; } = string.Empty;
        public int Predictions { get; set; }
        public int Approved { get; set; }
        public int Approvals
        {
            get => Approved;
            set => Approved = value;
        }
    }

    public class DashboardLoanStatusDto
    {
        public string Status { get; set; } = string.Empty;
        public int Count { get; set; }
    }

    public class DashboardPredictionDto
    {
        public Guid Id { get; set; }
        public string Client { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public int Score { get; set; }
        public string Level { get; set; } = string.Empty;
        public string Model { get; set; } = "Random Forest";
        public DateTime Date { get; set; }
        public string Status { get; set; } = string.Empty;
        public string Explanation { get; set; } = string.Empty;
    }

    public class DashboardDto
    {
        public DashboardSummaryDto Summary { get; set; } = new();
        public IReadOnlyList<DashboardChartItemDto> RiskDistribution { get; set; } = Array.Empty<DashboardChartItemDto>();
        public IReadOnlyList<DashboardMonthlyActivityDto> MonthlyActivity { get; set; } = Array.Empty<DashboardMonthlyActivityDto>();
        public IReadOnlyList<DashboardLoanStatusDto> LoanStatusSummary { get; set; } = Array.Empty<DashboardLoanStatusDto>();
        public IReadOnlyList<DashboardPredictionDto> RecentPredictions { get; set; } = Array.Empty<DashboardPredictionDto>();
    }
}
