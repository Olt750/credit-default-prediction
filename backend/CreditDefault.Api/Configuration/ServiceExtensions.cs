using CreditDefault.Api.Data;
using CreditDefault.Api.Interfaces;
using CreditDefault.Api.Repositories;
using CreditDefault.Api.Services;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore;

namespace CreditDefault.Api.Configuration
{
    public static class ServiceExtensions
    {
        public static IServiceCollection AddAppServices(this IServiceCollection services, IConfiguration config)
        {
            var connectionString = Environment.GetEnvironmentVariable("DB_CONNECTION_STRING")
                ?? config.GetConnectionString("DefaultConnection");

            if (string.IsNullOrWhiteSpace(connectionString))
            {
                throw new InvalidOperationException(
                    "Database connection string is not configured. For local development, add ConnectionStrings:DefaultConnection to backend/CreditDefault.Api/appsettings.Development.json, or set the DB_CONNECTION_STRING environment variable before running dotnet run. Example: Server=(localdb)\\MSSQLLocalDB;Database=CreditDefaultDb;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True");
            }

            services.AddDbContext<AppDbContext>(options =>
                options
                    .UseSqlServer(connectionString)
                    .ConfigureWarnings(warnings => warnings.Ignore(RelationalEventId.PendingModelChangesWarning)));
            services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
            services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped<IPredictionRepository, PredictionRepository>();
            services.AddScoped<IAuditLogRepository, AuditLogRepository>();
            services.AddScoped<IRoleRepository, RoleRepository>();
            services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();
            services.AddScoped<INotificationRepository, NotificationRepository>();
            services.AddSingleton<ICacheService, RedisCacheService>();
            services.AddScoped<JwtService>();
            services.AddScoped<PasswordService>();
            services.AddScoped<AuthService>();
            services.AddScoped<DevelopmentAdminSeeder>();
            services.AddScoped<RefreshTokenService>();
            services.AddScoped<PredictionEngine>();
            services.AddScoped<PredictionWorkflowService>();
            services.AddScoped<PythonCreditRiskPredictionService>();
            services.AddScoped<DashboardService>();
            services.AddScoped<AuditLogService>();
            services.AddScoped<NotificationService>();
            services.AddScoped<SettingService>();
            services.AddScoped<FileRecordService>();
            return services;
        }
    }
}
