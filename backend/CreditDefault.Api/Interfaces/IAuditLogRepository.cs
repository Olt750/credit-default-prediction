using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CreditDefault.Api.Models;

namespace CreditDefault.Api.Interfaces
{
    public interface IAuditLogRepository
    {
        Task<IEnumerable<AuditLog>> GetAllAsync();
        Task AddAsync(AuditLog log);
    }
}