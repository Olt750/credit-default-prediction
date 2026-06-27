using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using CreditDefault.Api.Models;
using CreditDefault.Api.Data;
using CreditDefault.Api.Interfaces;

namespace CreditDefault.Api.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly AppDbContext _context;
        public UserRepository(AppDbContext context) => _context = context;

        public async Task<User> GetByEmailAsync(string email) => await IncludeSecurity(_context.Users).FirstOrDefaultAsync(u => u.Email == email);
        public async Task<User> GetByIdAsync(Guid id) => await IncludeSecurity(_context.Users).Include(u => u.Predictions).FirstOrDefaultAsync(u => u.Id == id);
        public async Task<IEnumerable<User>> GetAllAsync() => await _context.Users.ToListAsync();
        public async Task AddAsync(User user) { _context.Users.Add(user); await _context.SaveChangesAsync(); }
        public async Task UpdateAsync(User user) { _context.Users.Update(user); await _context.SaveChangesAsync(); }

        private static IQueryable<User> IncludeSecurity(IQueryable<User> query) =>
            query
                .Include(u => u.UserRoles)
                    .ThenInclude(ur => ur.Role)
                        .ThenInclude(r => r.RolePermissions)
                            .ThenInclude(rp => rp.Permission);
    }
}
