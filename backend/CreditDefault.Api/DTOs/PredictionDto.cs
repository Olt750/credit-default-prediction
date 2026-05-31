namespace CreditDefault.Api.DTOs
{
    public class PredictionDto
    {
        public int Age { get; set; }
        public decimal Income { get; set; }
        public string EmploymentStatus { get; set; }
        public int CreditScore { get; set; }
        public decimal ExistingDebt { get; set; }
        public decimal LoanAmount { get; set; }
        public int LoanTerm { get; set; }
        public string PaymentHistory { get; set; }
    }
}