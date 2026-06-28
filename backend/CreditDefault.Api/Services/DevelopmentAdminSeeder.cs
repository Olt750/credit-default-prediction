using CreditDefault.Api.Data;
using CreditDefault.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace CreditDefault.Api.Services
{
    public class DevelopmentAdminSeeder
    {
        private const string DefaultAdminEmail = "admin@credit.com";
        private const string DefaultAdminPassword = "Admin123!";

        private readonly AppDbContext _context;
        private readonly PasswordService _passwordService;
        private readonly IConfiguration _configuration;
        private readonly ILogger<DevelopmentAdminSeeder> _logger;

        public DevelopmentAdminSeeder(
            AppDbContext context,
            PasswordService passwordService,
            IConfiguration configuration,
            ILogger<DevelopmentAdminSeeder> logger)
        {
            _context = context;
            _passwordService = passwordService;
            _configuration = configuration;
            _logger = logger;
        }

        public async Task SeedAsync()
        {
            var email = _configuration["AdminSeed:Email"] ?? DefaultAdminEmail;
            var password = _configuration["AdminSeed:Password"] ?? DefaultAdminPassword;

            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
            {
                _logger.LogWarning("Development admin seed skipped because ADMIN_EMAIL or ADMIN_PASSWORD is empty.");
                return;
            }

            var adminRole = await _context.Roles.FirstOrDefaultAsync(r => r.Name == "Admin");
            if (adminRole == null)
            {
                _logger.LogWarning("Development admin seed skipped because the Admin role was not found.");
                return;
            }

            var normalizedEmail = email.Trim();
            var adminUser = await _context.Users
                .Include(u => u.UserRoles)
                .FirstOrDefaultAsync(u => u.Email == normalizedEmail);

            if (adminUser == null)
            {
                var now = DateTime.UtcNow;
                adminUser = new User
                {
                    Id = Guid.NewGuid(),
                    FullName = "Development Admin",
                    Email = normalizedEmail,
                    PasswordHash = _passwordService.HashPassword(password),
                    PhoneNumber = "000-000-0000",
                    Role = "Admin",
                    CreatedAt = now,
                    UpdatedAt = now
                };

                _context.Users.Add(adminUser);
                _logger.LogInformation("Seeded local development admin user {Email}.", normalizedEmail);
            }
            else if (!string.Equals(adminUser.Role, "Admin", StringComparison.OrdinalIgnoreCase))
            {
                adminUser.Role = "Admin";
                adminUser.UpdatedAt = DateTime.UtcNow;
            }

            var hasAdminRole = adminUser.UserRoles.Any(ur => ur.RoleId == adminRole.Id);
            if (!hasAdminRole)
            {
                adminUser.UserRoles.Add(new UserRole
                {
                    UserId = adminUser.Id,
                    RoleId = adminRole.Id,
                    CreatedAt = DateTime.UtcNow
                });
            }

            await _context.SaveChangesAsync();
        }
    }
}
