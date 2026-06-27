namespace CreditDefault.Api.Models
{
    public class ClientDocument
    {
        public Guid Id { get; set; }
        public Guid ClientProfileId { get; set; }
        public ClientProfile ClientProfile { get; set; }
        public Guid FileRecordId { get; set; }
        public FileRecord FileRecord { get; set; }
        public string DocumentType { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        public Guid? CreatedBy { get; set; }
        public Guid? UpdatedBy { get; set; }
    }
}
