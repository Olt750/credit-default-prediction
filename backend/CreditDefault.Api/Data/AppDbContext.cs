using Microsoft.EntityFrameworkCore;
using CreditDefault.Api.Models;

namespace CreditDefault.Api.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<User> Users { get; set; }
        public DbSet<Prediction> Predictions { get; set; }
        public DbSet<ClientProfile> ClientProfiles { get; set; }
        public DbSet<AuditLog> AuditLogs { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>()
                .HasIndex(u => u.Email)
                .IsUnique();
            modelBuilder.Entity<User>()
                .HasMany(u => u.Predictions)
                .WithOne(p => p.User)
                .HasForeignKey(p => p.UserId);
            modelBuilder.Entity<User>()
                .HasOne(u => u.ClientProfile)
                .WithOne(p => p.User)
                .HasForeignKey<ClientProfile>(p => p.UserId);

            modelBuilder.Entity<ClientProfile>(entity =>
            {
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
        }
    }
}
