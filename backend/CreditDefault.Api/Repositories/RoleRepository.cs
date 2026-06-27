using CreditDefault.Api.Data;
using CreditDefault.Api.Interfaces;
using CreditDefault.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace CreditDefault.Api.Repositories
{
    public class RoleRepository : IRoleRepository
    {
        private readonly AppDbContext _context;
        public RoleRepository(AppDbContext context) => _context = context;

        public async Task<Role?> GetByNameAsync(string name) =>
            await _context.Roles.FirstOrDefaultAsync(r => r.Name == name);

        public async Task<List<string>> GetUserRoleNamesAsync(Guid userId) =>
            await _context.UserRoles
                .Where(ur => ur.UserId == userId)
                .Select(ur => ur.Role.Name)
                .ToListAsync();

        public async Task<List<string>> GetUserPermissionsAsync(Guid userId) =>
            await _context.UserRoles
                .Where(ur => ur.UserId == userId)
                .SelectMany(ur => ur.Role.RolePermissions.Select(rp => rp.Permission.Name))
                .Distinct()
                .ToListAsync();

        public async Task AddUserToRoleAsync(Guid userId, string roleName)
        {
            var role = await GetByNameAsync(roleName) ?? throw new InvalidOperationException($"Role '{roleName}' was not found.");
            var exists = await _context.UserRoles.AnyAsync(ur => ur.UserId == userId && ur.RoleId == role.Id);
            if (exists) return;

            _context.UserRoles.Add(new UserRole
            {
                UserId = userId,
                RoleId = role.Id,
                CreatedAt = DateTime.UtcNow
            });
            await _context.SaveChangesAsync();
        }
    }
}
