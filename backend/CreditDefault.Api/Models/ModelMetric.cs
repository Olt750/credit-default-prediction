namespace CreditDefault.Api.Models
{
    public class ModelMetric
    {
        public Guid Id { get; set; }
        public Guid ModelRunId { get; set; }
        public ModelRun ModelRun { get; set; }
        public string MetricName { get; set; } = string.Empty;
        public decimal MetricValue { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        public Guid? CreatedBy { get; set; }
        public Guid? UpdatedBy { get; set; }
    }
}
