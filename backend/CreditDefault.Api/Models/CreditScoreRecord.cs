namespace CreditDefault.Api.Models
{
    public class CreditScoreRecord
    {
        public Guid Id { get; set; }
        public Guid ClientProfileId { get; set; }
        public ClientProfile ClientProfile { get; set; }
        public int Score { get; set; }
        public string Provider { get; set; } = string.Empty;
        public DateTime ScoreDate { get; set; } = DateTime.UtcNow;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        public Guid? CreatedBy { get; set; }
        public Guid? UpdatedBy { get; set; }
    }
}
