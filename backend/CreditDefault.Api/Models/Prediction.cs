using System;

namespace CreditDefault.Api.Models
{
    public class Prediction
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public User User { get; set; }
        public int Age { get; set; }
        public decimal Income { get; set; }
        public string EmploymentStatus { get; set; }
        public int CreditScore { get; set; }
        public decimal ExistingDebt { get; set; }
        public decimal LoanAmount { get; set; }
        public int LoanTerm { get; set; }
        public string PaymentHistory { get; set; }
        public int RiskScore { get; set; }
        public string RiskLevel { get; set; }
        public string ExplanationMessage { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}