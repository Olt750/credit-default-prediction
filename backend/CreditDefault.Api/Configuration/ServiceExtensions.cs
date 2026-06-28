using CreditDefault.Api.Data;
using CreditDefault.Api.Interfaces;
using CreditDefault.Api.Repositories;
using CreditDefault.Api.Services;
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
                throw new InvalidOperationException("Database connection string is not configured. Set DB_CONNECTION_STRING or ConnectionStrings:DefaultConnection.");
            }

            services.AddDbContext<AppDbContext>(options =>
                options.UseSqlServer(connectionString));
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
