using System;

namespace CreditDefault.Api.Models
{
    public class ClientProfile
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public User User { get; set; }
        public int Age { get; set; }
        public decimal AnnualIncome { get; set; }
        public decimal LoanAmount { get; set; }
        public int CreditScore { get; set; }
        public string EmploymentStatus { get; set; } = string.Empty;
        public int LoanTermMonths { get; set; }
        public int PreviousDefaults { get; set; }
        public string Education { get; set; } = string.Empty;
        public string MaritalStatus { get; set; } = string.Empty;
        public decimal MonthlyCarLoanPayment { get; set; }
        public decimal MonthlyMortgageOrRentPayment { get; set; }
        public decimal MonthlyPersonalLoanPayment { get; set; }
        public decimal MonthlyCreditCardPayment { get; set; }
        public decimal MonthlyOtherDebtPayment { get; set; }
        public decimal TotalMonthlyDebt { get; set; }
        public decimal DebtToIncomeRatio { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        public ICollection<LoanApplication> LoanApplications { get; set; } = new List<LoanApplication>();
        public ICollection<EmploymentInfo> EmploymentInfos { get; set; } = new List<EmploymentInfo>();
        public ICollection<IncomeSource> IncomeSources { get; set; } = new List<IncomeSource>();
        public ICollection<DebtObligation> DebtObligations { get; set; } = new List<DebtObligation>();
        public ICollection<CreditScoreRecord> CreditScores { get; set; } = new List<CreditScoreRecord>();
        public ICollection<ClientDocument> ClientDocuments { get; set; } = new List<ClientDocument>();
    }
}
