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
            services.AddDbContext<AppDbContext>(options =>
                options.UseSqlServer(config.GetConnectionString("DefaultConnection")));
            services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped<IPredictionRepository, PredictionRepository>();
            services.AddScoped<IAuditLogRepository, AuditLogRepository>();
            services.AddScoped<JwtService>();
            services.AddScoped<PasswordService>();
            services.AddScoped<PredictionEngine>();
            services.AddScoped<PythonCreditRiskPredictionService>();
            return services;
        }
    }
}
