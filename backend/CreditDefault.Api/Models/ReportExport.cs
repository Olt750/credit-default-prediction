namespace CreditDefault.Api.Models
{
    public class ReportExport
    {
        public Guid Id { get; set; }
        public Guid ReportId { get; set; }
        public Report Report { get; set; }
        public Guid? FileRecordId { get; set; }
        public FileRecord? FileRecord { get; set; }
        public string Format { get; set; } = string.Empty;
        public DateTime ExportedAt { get; set; } = DateTime.UtcNow;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        public Guid? CreatedBy { get; set; }
        public Guid? UpdatedBy { get; set; }
    }
}
