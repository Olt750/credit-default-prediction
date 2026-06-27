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

        public AuditLogMiddleware(RequestDelegate next)
        {
            _next = next;
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

            await _next(context);

            if (MutatingMethods.Contains(context.Request.Method) && !context.Request.Path.StartsWithSegments("/swagger"))
            {
                await auditLogService.LogRequestAsync(context, body);
            }
        }
    }
}
