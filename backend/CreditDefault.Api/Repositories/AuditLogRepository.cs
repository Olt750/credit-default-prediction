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
        public async Task AddAsync(AuditLog log) { _context.AuditLogs.Add(log); await _context.SaveChangesAsync(); }
    }
}