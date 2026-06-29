using System.Text.Json;
using CreditDefault.Api.Data;
using CreditDefault.Api.DTOs;
using CreditDefault.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace CreditDefault.Api.Services
{
    public class AdminUserService
    {
        private readonly AppDbContext _context;
        private readonly PasswordService _passwordService;

        public AdminUserService(AppDbContext context, PasswordService passwordService)
        {
            _context = context;
            _passwordService = passwordService;
        }

        public async Task<IReadOnlyList<UserListItemDto>> GetUsersAsync()
        {
            var users = await _context.Users
                .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
                .AsNoTracking()
                .OrderByDescending(u => u.CreatedAt)
                .ToListAsync();

            return users.Select(ToListItem).ToList();
        }

        public async Task<AdminUserDetailDto?> GetUserAsync(Guid id, Guid adminId, string performedBy, string ipAddress)
        {
            var user = await _context.Users
                .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
                .Include(u => u.Predictions)
                .Include(u => u.Notifications)
                .Include(u => u.ClientProfile)
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Id == id);

            if (user == null) return null;
            await AddAuditAsync(adminId, performedBy, "UserViewed", id, null, new { user.Email }, ipAddress);

            var item = ToListItem(user);
            return new AdminUserDetailDto
            {
                Id = item.Id,
                FullName = item.FullName,
                Email = item.Email,
                PhoneNumber = item.PhoneNumber,
                Role = item.Role,
                Roles = item.Roles,
                IsActive = item.IsActive,
                CreatedAt = item.CreatedAt,
                UpdatedAt = user.UpdatedAt,
                PredictionCount = user.Predictions.Count,
                NotificationCount = user.Notifications.Count,
                HasClientProfile = user.ClientProfile != null
            };
        }

        public async Task<UserListItemDto> InviteAsync(AdminUserInviteRequest request, Guid adminId, string performedBy, string ipAddress)
        {
            var email = request.Email.Trim().ToLowerInvariant();
            var exists = await _context.Users.AnyAsync(u => u.Email == email);
            if (exists) throw new InvalidOperationException("A user with this email already exists.");

            var now = DateTime.UtcNow;
            var user = new User
            {
                Id = Guid.NewGuid(),
                FullName = request.FullName.Trim(),
                Email = email,
                PhoneNumber = request.PhoneNumber?.Trim() ?? string.Empty,
                PasswordHash = _passwordService.HashPassword(string.IsNullOrWhiteSpace(request.Password) ? GenerateTemporaryPassword() : request.Password),
                Role = NormalizeRoles(request.Roles).First(),
                IsActive = true,
                CreatedAt = now,
                UpdatedAt = now
            };

            _context.Users.Add(user);
            await SetRolesAsync(user.Id, NormalizeRoles(request.Roles), saveChanges: false);
            await _context.SaveChangesAsync();
            await AddAuditAsync(adminId, performedBy, "UserInvited", user.Id, null, new { user.Email, Roles = request.Roles }, ipAddress);

            var saved = await _context.Users.Include(u => u.UserRoles).ThenInclude(ur => ur.Role).AsNoTracking().FirstAsync(u => u.Id == user.Id);
            return ToListItem(saved);
        }

        public async Task<UserListItemDto?> UpdateAsync(Guid id, AdminUserUpdateRequest request, Guid adminId, string performedBy, string ipAddress)
        {
            var user = await _context.Users.Include(u => u.UserRoles).ThenInclude(ur => ur.Role).FirstOrDefaultAsync(u => u.Id == id);
            if (user == null) return null;

            var newEmail = request.Email.Trim().ToLowerInvariant();
            var emailTaken = await _context.Users.AnyAsync(u => u.Id != id && u.Email == newEmail);
            if (emailTaken) throw new InvalidOperationException("A different user already uses this email.");

            var oldValues = new { user.FullName, user.Email, user.PhoneNumber };
            user.FullName = request.FullName.Trim();
            user.Email = newEmail;
            user.PhoneNumber = request.PhoneNumber?.Trim() ?? string.Empty;
            user.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            await AddAuditAsync(adminId, performedBy, "UserUpdated", id, oldValues, new { user.FullName, user.Email, user.PhoneNumber }, ipAddress);
            return ToListItem(user);
        }

        public async Task<UserListItemDto?> SetStatusAsync(Guid id, bool isActive, Guid adminId, string performedBy, string ipAddress)
        {
            var user = await _context.Users.Include(u => u.UserRoles).ThenInclude(ur => ur.Role).FirstOrDefaultAsync(u => u.Id == id);
            if (user == null) return null;
            if (user.Id == adminId && !isActive) throw new InvalidOperationException("You cannot deactivate your own admin account.");

            var oldValues = new { user.IsActive };
            user.IsActive = isActive;
            user.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            await AddAuditAsync(adminId, performedBy, isActive ? "UserActivated" : "UserDeactivated", id, oldValues, new { user.IsActive }, ipAddress);
            return ToListItem(user);
        }

        public async Task<UserListItemDto?> SetRolesAsync(Guid id, AdminUserRolesRequest request, Guid adminId, string performedBy, string ipAddress)
        {
            var user = await _context.Users.Include(u => u.UserRoles).ThenInclude(ur => ur.Role).FirstOrDefaultAsync(u => u.Id == id);
            if (user == null) return null;

            var roles = NormalizeRoles(request.Roles);
            if (user.Id == adminId && !roles.Contains("Admin")) throw new InvalidOperationException("You cannot remove your own Admin role.");

            var oldRoles = user.UserRoles.Select(ur => ur.Role.Name).ToList();
            await SetRolesAsync(id, roles, saveChanges: false);
            user.Role = roles.First();
            user.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            await AddAuditAsync(adminId, performedBy, "UserRolesChanged", id, new { Roles = oldRoles }, new { Roles = roles }, ipAddress);

            var refreshed = await _context.Users.Include(u => u.UserRoles).ThenInclude(ur => ur.Role).AsNoTracking().FirstAsync(u => u.Id == id);
            return ToListItem(refreshed);
        }

        private async Task SetRolesAsync(Guid userId, IReadOnlyList<string> roleNames, bool saveChanges)
        {
            var roles = await _context.Roles.Where(r => roleNames.Contains(r.Name)).ToListAsync();
            var missing = roleNames.Except(roles.Select(r => r.Name), StringComparer.OrdinalIgnoreCase).ToList();
            if (missing.Count > 0) throw new InvalidOperationException($"Unknown role: {string.Join(", ", missing)}");

            var existing = await _context.UserRoles.Where(ur => ur.UserId == userId).ToListAsync();
            _context.UserRoles.RemoveRange(existing);
            foreach (var role in roles)
            {
                _context.UserRoles.Add(new UserRole { UserId = userId, RoleId = role.Id, CreatedAt = DateTime.UtcNow });
            }

            if (saveChanges) await _context.SaveChangesAsync();
        }

        private static IReadOnlyList<string> NormalizeRoles(IReadOnlyList<string>? roles)
        {
            var normalized = (roles == null || roles.Count == 0 ? ["User"] : roles)
                .Select(role => role.Trim())
                .Where(role => !string.IsNullOrWhiteSpace(role))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();
            return normalized.Count == 0 ? ["User"] : normalized;
        }

        private static UserListItemDto ToListItem(User user)
        {
            var roles = user.UserRoles.Select(ur => ur.Role.Name).Distinct().ToList();
            return new UserListItemDto
            {
                Id = user.Id,
                FullName = user.FullName,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber,
                Role = roles.FirstOrDefault() ?? user.Role,
                Roles = roles,
                IsActive = user.IsActive,
                CreatedAt = user.CreatedAt
            };
        }

        private async Task AddAuditAsync(Guid adminId, string performedBy, string action, Guid entityId, object? oldValues, object? newValues, string ipAddress)
        {
            var now = DateTime.UtcNow;
            _context.AuditLogs.Add(new AuditLog
            {
                Id = Guid.NewGuid(),
                UserId = adminId,
                Action = action,
                EntityName = "Users",
                EntityId = entityId,
                OldValues = oldValues == null ? null : JsonSerializer.Serialize(oldValues),
                NewValues = newValues == null ? null : JsonSerializer.Serialize(newValues),
                IpAddress = string.IsNullOrWhiteSpace(ipAddress) ? "Unknown" : ipAddress,
                PerformedBy = string.IsNullOrWhiteSpace(performedBy) ? "UnknownUser" : performedBy,
                CreatedAt = now,
                Timestamp = now,
                Details = action
            });
            await _context.SaveChangesAsync();
        }

        private static string GenerateTemporaryPassword() => $"CreditIQ-{Guid.NewGuid():N}!a1"[..20];
    }
}
