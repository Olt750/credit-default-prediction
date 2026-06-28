using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using CreditDefault.Api.Models;
using CreditDefault.Api.Data;
using CreditDefault.Api.Interfaces;

namespace CreditDefault.Api.Repositories
{
    public class AuditLogRepository : IAuditLogRepository
    {
        private readonly AppDbContext _context;
        public AuditLogRepository(AppDbContext context) => _context = context;

        public async Task<IEnumerable<AuditLog>> GetAllAsync() => await _context.AuditLogs.ToListAsync();
        public async Task AddAsync(AuditLog log)
        {
            log.Id = log.Id == Guid.Empty ? Guid.NewGuid() : log.Id;
            log.Action = string.IsNullOrWhiteSpace(log.Action) ? "UNKNOWN" : log.Action;
            log.EntityName = string.IsNullOrWhiteSpace(log.EntityName) ? "Unknown" : log.EntityName;
            log.IpAddress = string.IsNullOrWhiteSpace(log.IpAddress) ? "Unknown" : log.IpAddress;
            log.PerformedBy = string.IsNullOrWhiteSpace(log.PerformedBy) ? "System" : log.PerformedBy;
            log.Details = string.IsNullOrWhiteSpace(log.Details) ? $"{log.Action} {log.EntityName}" : log.Details;
            log.Timestamp = log.Timestamp == default ? DateTime.UtcNow : log.Timestamp;
            log.CreatedAt = log.CreatedAt == default ? log.Timestamp : log.CreatedAt;

            _context.AuditLogs.Add(log);
            await _context.SaveChangesAsync();
        }
    }
}
