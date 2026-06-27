namespace CreditDefault.Api.Models
{
    public class DebtObligation
    {
        public Guid Id { get; set; }
        public Guid ClientProfileId { get; set; }
        public ClientProfile ClientProfile { get; set; }
        public string DebtType { get; set; } = string.Empty;
        public string CreditorName { get; set; } = string.Empty;
        public decimal OutstandingBalance { get; set; }
        public decimal MonthlyPayment { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        public Guid? CreatedBy { get; set; }
        public Guid? UpdatedBy { get; set; }
    }
}
