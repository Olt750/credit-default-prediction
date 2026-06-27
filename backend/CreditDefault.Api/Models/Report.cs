namespace CreditDefault.Api.Models
{
    public class Report
    {
        public Guid Id { get; set; }
        public Guid CreatedByUserId { get; set; }
        public User CreatedByUser { get; set; }
        public string Name { get; set; } = string.Empty;
        public string ReportType { get; set; } = string.Empty;
        public string ParametersJson { get; set; } = "{}";
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        public Guid? UpdatedBy { get; set; }
        public ICollection<ReportExport> Exports { get; set; } = new List<ReportExport>();
    }
}
