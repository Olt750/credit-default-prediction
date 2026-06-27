namespace CreditDefault.Api.Models
{
    public class PredictionFactor
    {
        public Guid Id { get; set; }
        public Guid PredictionId { get; set; }
        public Prediction Prediction { get; set; }
        public Guid RiskFactorId { get; set; }
        public RiskFactor RiskFactor { get; set; }
        public decimal Value { get; set; }
        public decimal ImpactScore { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        public Guid? CreatedBy { get; set; }
        public Guid? UpdatedBy { get; set; }
    }
}
