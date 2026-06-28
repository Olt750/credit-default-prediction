using CreditDefault.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace CreditDefault.Api.Data
{
    public class AppDbContext : DbContext
    {
        public static readonly Guid AdminRoleId = Guid.Parse("11111111-1111-1111-1111-111111111111");
        public static readonly Guid UserRoleId = Guid.Parse("22222222-2222-2222-2222-222222222222");
        public static readonly Guid ManagerRoleId = Guid.Parse("33333333-3333-3333-3333-333333333333");

        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<User> Users { get; set; }
        public DbSet<Role> Roles { get; set; }
        public DbSet<UserRole> UserRoles { get; set; }
        public DbSet<Permission> Permissions { get; set; }
        public DbSet<RolePermission> RolePermissions { get; set; }
        public DbSet<RefreshToken> RefreshTokens { get; set; }
        public DbSet<AuditLog> AuditLogs { get; set; }
        public DbSet<Notification> Notifications { get; set; }
        public DbSet<Setting> Settings { get; set; }
        public DbSet<FileRecord> Files { get; set; }
        public DbSet<Prediction> Predictions { get; set; }
        public DbSet<ClientProfile> ClientProfiles { get; set; }
        public DbSet<LoanApplication> LoanApplications { get; set; }
        public DbSet<LoanApplicationStatus> LoanApplicationStatuses { get; set; }
        public DbSet<LoanType> LoanTypes { get; set; }
        public DbSet<EmploymentInfo> EmploymentInfos { get; set; }
        public DbSet<IncomeSource> IncomeSources { get; set; }
        public DbSet<DebtObligation> DebtObligations { get; set; }
        public DbSet<CreditScoreRecord> CreditScores { get; set; }
        public DbSet<RiskFactor> RiskFactors { get; set; }
        public DbSet<PredictionFactor> PredictionFactors { get; set; }
        public DbSet<ModelRun> ModelRuns { get; set; }
        public DbSet<ModelMetric> ModelMetrics { get; set; }
        public DbSet<Report> Reports { get; set; }
        public DbSet<ReportExport> ReportExports { get; set; }
        public DbSet<UserActivity> UserActivities { get; set; }
        public DbSet<ClientDocument> ClientDocuments { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            ConfigureUsers(modelBuilder);
            ConfigureSecurity(modelBuilder);
            ConfigureCreditRiskDomain(modelBuilder);
            ConfigureSystemTables(modelBuilder);
            SeedLabFoundation(modelBuilder);
        }

        private static void ConfigureUsers(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>(entity =>
            {
                entity.HasIndex(u => u.Email).IsUnique();
                entity.HasIndex(u => u.CreatedAt);
                entity.Property(u => u.FullName).HasMaxLength(160);
                entity.Property(u => u.Email).HasMaxLength(256);
                entity.Property(u => u.Role).HasMaxLength(64);

                entity.HasMany(u => u.Predictions)
                    .WithOne(p => p.User)
                    .HasForeignKey(p => p.UserId);

                entity.HasOne(u => u.ClientProfile)
                    .WithOne(p => p.User)
                    .HasForeignKey<ClientProfile>(p => p.UserId);
            });
        }

        private static void ConfigureSecurity(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Role>(entity =>
            {
                entity.HasIndex(r => r.Name).IsUnique();
                entity.Property(r => r.Name).HasMaxLength(64);
            });

            modelBuilder.Entity<Permission>(entity =>
            {
                entity.HasIndex(p => p.Name).IsUnique();
                entity.Property(p => p.Name).HasMaxLength(128);
            });

            modelBuilder.Entity<UserRole>(entity =>
            {
                entity.HasKey(ur => new { ur.UserId, ur.RoleId });
                entity.HasIndex(ur => ur.RoleId);
                entity.HasOne(ur => ur.User).WithMany(u => u.UserRoles).HasForeignKey(ur => ur.UserId);
                entity.HasOne(ur => ur.Role).WithMany(r => r.UserRoles).HasForeignKey(ur => ur.RoleId);
            });

            modelBuilder.Entity<RolePermission>(entity =>
            {
                entity.HasKey(rp => new { rp.RoleId, rp.PermissionId });
                entity.HasIndex(rp => rp.PermissionId);
                entity.HasOne(rp => rp.Role).WithMany(r => r.RolePermissions).HasForeignKey(rp => rp.RoleId);
                entity.HasOne(rp => rp.Permission).WithMany(p => p.RolePermissions).HasForeignKey(rp => rp.PermissionId);
            });

            modelBuilder.Entity<RefreshToken>(entity =>
            {
                entity.HasIndex(t => t.TokenHash).IsUnique();
                entity.HasIndex(t => t.UserId);
                entity.HasIndex(t => t.ExpiresAt);
                entity.Property(t => t.TokenHash).HasMaxLength(128);
                entity.HasOne(t => t.User).WithMany(u => u.RefreshTokens).HasForeignKey(t => t.UserId);
            });
        }

        private static void ConfigureCreditRiskDomain(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<ClientProfile>(entity =>
            {
                entity.HasIndex(p => p.UserId).IsUnique();
                entity.HasIndex(p => p.CreatedAt);
                entity.Property(p => p.AnnualIncome).HasColumnType("decimal(18,2)");
                entity.Property(p => p.LoanAmount).HasColumnType("decimal(18,2)");
                entity.Property(p => p.MonthlyCarLoanPayment).HasColumnType("decimal(18,2)");
                entity.Property(p => p.MonthlyMortgageOrRentPayment).HasColumnType("decimal(18,2)");
                entity.Property(p => p.MonthlyPersonalLoanPayment).HasColumnType("decimal(18,2)");
                entity.Property(p => p.MonthlyCreditCardPayment).HasColumnType("decimal(18,2)");
                entity.Property(p => p.MonthlyOtherDebtPayment).HasColumnType("decimal(18,2)");
                entity.Property(p => p.TotalMonthlyDebt).HasColumnType("decimal(18,2)");
                entity.Property(p => p.DebtToIncomeRatio).HasColumnType("decimal(18,4)");
            });

            modelBuilder.Entity<Prediction>(entity =>
            {
                entity.HasIndex(p => p.UserId);
                entity.HasIndex(p => p.CreatedAt);
                entity.Property(p => p.Income).HasColumnType("decimal(18,2)");
                entity.Property(p => p.ExistingDebt).HasColumnType("decimal(18,2)");
                entity.Property(p => p.LoanAmount).HasColumnType("decimal(18,2)");
                entity.Property(p => p.MonthlyCarLoanPayment).HasColumnType("decimal(18,2)");
                entity.Property(p => p.MonthlyMortgageOrRentPayment).HasColumnType("decimal(18,2)");
                entity.Property(p => p.MonthlyPersonalLoanPayment).HasColumnType("decimal(18,2)");
                entity.Property(p => p.MonthlyCreditCardPayment).HasColumnType("decimal(18,2)");
                entity.Property(p => p.MonthlyOtherDebtPayment).HasColumnType("decimal(18,2)");
                entity.Property(p => p.TotalMonthlyDebt).HasColumnType("decimal(18,2)");
                entity.Property(p => p.DebtToIncomeRatio).HasColumnType("decimal(18,4)");
            });

            modelBuilder.Entity<LoanApplication>(entity =>
            {
                entity.HasIndex(a => a.ClientProfileId);
                entity.HasIndex(a => a.StatusId);
                entity.HasIndex(a => a.CreatedAt);
                entity.Property(a => a.RequestedAmount).HasColumnType("decimal(18,2)");
            });
            modelBuilder.Entity<LoanApplicationStatus>().HasIndex(s => s.Name).IsUnique();
            modelBuilder.Entity<LoanType>().HasIndex(t => t.Name).IsUnique();
            modelBuilder.Entity<EmploymentInfo>().Property(e => e.MonthlyIncome).HasColumnType("decimal(18,2)");
            modelBuilder.Entity<IncomeSource>().Property(i => i.MonthlyAmount).HasColumnType("decimal(18,2)");
            modelBuilder.Entity<DebtObligation>().Property(d => d.OutstandingBalance).HasColumnType("decimal(18,2)");
            modelBuilder.Entity<DebtObligation>().Property(d => d.MonthlyPayment).HasColumnType("decimal(18,2)");
            modelBuilder.Entity<CreditScoreRecord>().HasIndex(c => new { c.ClientProfileId, c.ScoreDate });
            modelBuilder.Entity<RiskFactor>().HasIndex(r => r.Code).IsUnique();
            modelBuilder.Entity<RiskFactor>().Property(r => r.Weight).HasColumnType("decimal(10,4)");
            modelBuilder.Entity<PredictionFactor>().Property(f => f.Value).HasColumnType("decimal(18,4)");
            modelBuilder.Entity<PredictionFactor>().Property(f => f.ImpactScore).HasColumnType("decimal(18,4)");
            modelBuilder.Entity<ModelMetric>().Property(m => m.MetricValue).HasColumnType("decimal(18,6)");
        }

        private static void ConfigureSystemTables(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<AuditLog>(entity =>
            {
                entity.HasIndex(a => a.UserId);
                entity.HasIndex(a => a.CreatedAt);
                entity.HasIndex(a => a.EntityName);
                entity.Property(a => a.Action).IsRequired();
                entity.Property(a => a.EntityName).IsRequired();
                entity.Property(a => a.IpAddress).IsRequired().HasDefaultValue("Unknown");
                entity.Property(a => a.PerformedBy).IsRequired().HasDefaultValue("System");
                entity.Property(a => a.Details).IsRequired().HasDefaultValue(string.Empty);
            });

            modelBuilder.Entity<Notification>(entity =>
            {
                entity.HasIndex(n => n.UserId);
                entity.HasIndex(n => new { n.UserId, n.IsRead });
                entity.HasIndex(n => n.CreatedAt);
            });

            modelBuilder.Entity<Setting>(entity =>
            {
                entity.HasIndex(s => s.Key).IsUnique();
                entity.Property(s => s.Key).HasMaxLength(128);
            });

            modelBuilder.Entity<FileRecord>(entity =>
            {
                entity.HasIndex(f => f.UserId);
                entity.HasIndex(f => f.ClientProfileId);
                entity.HasIndex(f => f.CreatedAt);
                entity.ToTable("Files");
            });

            modelBuilder.Entity<Report>().HasIndex(r => r.CreatedByUserId);
            modelBuilder.Entity<ReportExport>().HasIndex(e => e.ReportId);
            modelBuilder.Entity<UserActivity>().HasIndex(a => a.UserId);
            modelBuilder.Entity<UserActivity>().HasIndex(a => a.CreatedAt);
            modelBuilder.Entity<ClientDocument>().HasIndex(d => d.ClientProfileId);
            modelBuilder.Entity<ClientDocument>().HasIndex(d => d.FileRecordId);
        }

        private static void SeedLabFoundation(ModelBuilder modelBuilder)
        {
            var seedTime = new DateTime(2026, 06, 27, 0, 0, 0, DateTimeKind.Utc);
            var permissionNames = new[]
            {
                "users.read", "users.manage", "predictions.create", "predictions.read", "predictions.manage",
                "reports.read", "reports.generate", "settings.manage", "notifications.read", "files.manage"
            };
            var permissionIds = permissionNames
                .Select((name, index) => new { name, id = Guid.Parse($"44444444-4444-4444-4444-{(index + 1).ToString().PadLeft(12, '0')}") })
                .ToArray();

            modelBuilder.Entity<Role>().HasData(
                new Role { Id = AdminRoleId, Name = "Admin", Description = "Full system administration", CreatedAt = seedTime, UpdatedAt = seedTime },
                new Role { Id = UserRoleId, Name = "User", Description = "Standard credit-risk applicant", CreatedAt = seedTime, UpdatedAt = seedTime },
                new Role { Id = ManagerRoleId, Name = "Manager", Description = "Credit-risk operations manager", CreatedAt = seedTime, UpdatedAt = seedTime });

            modelBuilder.Entity<Permission>().HasData(permissionIds.Select(p => new Permission
            {
                Id = p.id,
                Name = p.name,
                Description = p.name,
                CreatedAt = seedTime,
                UpdatedAt = seedTime
            }));

            modelBuilder.Entity<RolePermission>().HasData(permissionIds.Select(p => new RolePermission
            {
                RoleId = AdminRoleId,
                PermissionId = p.id,
                CreatedAt = seedTime
            }));

            modelBuilder.Entity<RolePermission>().HasData(permissionIds
                .Where(p => p.name is "predictions.create" or "predictions.read" or "notifications.read")
                .Select(p => new RolePermission { RoleId = UserRoleId, PermissionId = p.id, CreatedAt = seedTime }));

            modelBuilder.Entity<RolePermission>().HasData(permissionIds
                .Where(p => p.name is "users.read" or "predictions.read" or "predictions.manage" or "reports.read" or "reports.generate" or "notifications.read" or "files.manage")
                .Select(p => new RolePermission { RoleId = ManagerRoleId, PermissionId = p.id, CreatedAt = seedTime }));

            modelBuilder.Entity<LoanApplicationStatus>().HasData(
                new LoanApplicationStatus { Id = Guid.Parse("55555555-5555-5555-5555-000000000001"), Name = "Draft", Description = "Application is being prepared", CreatedAt = seedTime, UpdatedAt = seedTime },
                new LoanApplicationStatus { Id = Guid.Parse("55555555-5555-5555-5555-000000000002"), Name = "Submitted", Description = "Application submitted for review", CreatedAt = seedTime, UpdatedAt = seedTime },
                new LoanApplicationStatus { Id = Guid.Parse("55555555-5555-5555-5555-000000000003"), Name = "Approved", Description = "Application approved", CreatedAt = seedTime, UpdatedAt = seedTime },
                new LoanApplicationStatus { Id = Guid.Parse("55555555-5555-5555-5555-000000000004"), Name = "Rejected", Description = "Application rejected", CreatedAt = seedTime, UpdatedAt = seedTime });

            modelBuilder.Entity<LoanType>().HasData(
                new LoanType { Id = Guid.Parse("66666666-6666-6666-6666-000000000001"), Name = "Personal", Description = "Personal loan", CreatedAt = seedTime, UpdatedAt = seedTime },
                new LoanType { Id = Guid.Parse("66666666-6666-6666-6666-000000000002"), Name = "Auto", Description = "Vehicle financing", CreatedAt = seedTime, UpdatedAt = seedTime },
                new LoanType { Id = Guid.Parse("66666666-6666-6666-6666-000000000003"), Name = "Mortgage", Description = "Mortgage loan", CreatedAt = seedTime, UpdatedAt = seedTime });
        }
    }
}
