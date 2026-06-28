using CreditDefault.Api.Configuration;
using CreditDefault.Api.Data;
using CreditDefault.Api.Hubs;
using CreditDefault.Api.Middleware;
using CreditDefault.Api.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);
ApplyEnvironmentOverrides(builder.Configuration);

// Add services
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddAppServices(builder.Configuration);
builder.Services.AddSingleton<IAuthorizationPolicyProvider, PermissionAuthorizationPolicyProvider>();
builder.Services.AddSignalR();

// JWT Auth
var jwtSecret = builder.Configuration["Jwt:Key"];
if (string.IsNullOrWhiteSpace(jwtSecret))
{
    throw new InvalidOperationException("JWT secret is not configured. Set JWT_SECRET or Jwt:Key.");
}

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret))
        };
        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                var accessToken = context.Request.Query["access_token"];
                var path = context.HttpContext.Request.Path;
                if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/hubs/notifications"))
                {
                    context.Token = accessToken;
                }

                return Task.CompletedTask;
            }
        };
    });

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend",
        policy => policy.WithOrigins("http://localhost:8080", "http://localhost:5173")
                        .AllowAnyHeader()
                        .AllowAnyMethod()
                        .AllowCredentials());
});

var app = builder.Build();

// Middleware
app.UseMiddleware<ExceptionMiddleware>();
app.UseSwagger();
app.UseSwaggerUI();
app.UseHttpsRedirection();
app.UseCors("AllowFrontend");
app.UseAuthentication();
app.UseAuthorization();
app.UseMiddleware<AuditLogMiddleware>();
app.MapControllers();
app.MapHub<NotificationsHub>("/hubs/notifications");

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();
}

app.Run();

static void ApplyEnvironmentOverrides(IConfiguration configuration)
{
    SetIfPresent(configuration, "DB_CONNECTION_STRING", "ConnectionStrings:DefaultConnection");
    SetIfPresent(configuration, "JWT_SECRET", "Jwt:Key");
    SetIfPresent(configuration, "JWT_ISSUER", "Jwt:Issuer");
    SetIfPresent(configuration, "JWT_AUDIENCE", "Jwt:Audience");
    SetIfPresent(configuration, "PYTHON_EXECUTABLE_PATH", "ML:PythonExecutable");
    SetIfPresent(configuration, "ML_SCRIPT_PATH", "ML:PredictionScriptPath");
    SetIfPresent(configuration, "REDIS_CONNECTION_STRING", "Redis:ConnectionString");
    SetIfPresent(configuration, "REDIS_INSTANCE_NAME", "Redis:InstanceName");
}

static void SetIfPresent(IConfiguration configuration, string envName, string configKey)
{
    var value = Environment.GetEnvironmentVariable(envName);
    if (!string.IsNullOrWhiteSpace(value))
    {
        configuration[configKey] = value;
    }
}
