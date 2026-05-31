using System;

namespace CreditDefault.Api.Models
{
    public class AuditLog
    {
        public Guid Id { get; set; }
        public string Action { get; set; }
        public string PerformedBy { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
        public string Details { get; set; }
    }
}