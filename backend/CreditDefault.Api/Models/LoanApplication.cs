namespace CreditDefault.Api.Models
{
    public class LoanApplication
    {
        public Guid Id { get; set; }
        public Guid ClientProfileId { get; set; }
        public ClientProfile ClientProfile { get; set; }
        public Guid StatusId { get; set; }
        public LoanApplicationStatus Status { get; set; }
        public Guid LoanTypeId { get; set; }
        public LoanType LoanType { get; set; }
        public decimal RequestedAmount { get; set; }
        public int TermMonths { get; set; }
        public string Purpose { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        public Guid? CreatedBy { get; set; }
        public Guid? UpdatedBy { get; set; }
    }
}
