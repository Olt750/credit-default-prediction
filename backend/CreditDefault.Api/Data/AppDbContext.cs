using Microsoft.EntityFrameworkCore;
using CreditDefault.Api.Models;

namespace CreditDefault.Api.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<User> Users { get; set; }
        public DbSet<Prediction> Predictions { get; set; }
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
        }
    }
}