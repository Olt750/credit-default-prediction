using CreditDefault.Api.Services;

namespace CreditDefault.Api.Middleware
{
    public class AuditLogMiddleware
    {
        private static readonly HashSet<string> MutatingMethods = new(StringComparer.OrdinalIgnoreCase)
        {
            "POST", "PUT", "PATCH", "DELETE"
        };

        private readonly RequestDelegate _next;
        private readonly ILogger<AuditLogMiddleware> _logger;

        public AuditLogMiddleware(RequestDelegate next, ILogger<AuditLogMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context, AuditLogService auditLogService)
        {
            string? body = null;

            if (MutatingMethods.Contains(context.Request.Method) && context.Request.ContentLength is > 0)
            {
                context.Request.EnableBuffering();
                using var reader = new StreamReader(context.Request.Body, leaveOpen: true);
                body = await reader.ReadToEndAsync();
                context.Request.Body.Position = 0;
            }

            var shouldAudit = MutatingMethods.Contains(context.Request.Method) && !context.Request.Path.StartsWithSegments("/swagger");
            try
            {
                await _next(context);
            }
            finally
            {
                if (shouldAudit)
                {
                    try
                    {
                        await auditLogService.LogRequestAsync(context, body);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Audit logging failed for {Method} {Path}. The request will not be failed because of audit logging.", context.Request.Method, context.Request.Path);
                    }
                }
            }
        }
    }
}
