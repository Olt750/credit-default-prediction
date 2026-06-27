namespace CreditDefault.Api.Models
{
    public class EmploymentInfo
    {
        public Guid Id { get; set; }
        public Guid ClientProfileId { get; set; }
        public ClientProfile ClientProfile { get; set; }
        public string EmployerName { get; set; } = string.Empty;
        public string EmploymentStatus { get; set; } = string.Empty;
        public string JobTitle { get; set; } = string.Empty;
        public decimal MonthlyIncome { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        public Guid? CreatedBy { get; set; }
        public Guid? UpdatedBy { get; set; }
    }
}
