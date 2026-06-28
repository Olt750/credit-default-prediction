using System.Text.Json;
using System.Security.Claims;
using CreditDefault.Api.Interfaces;
using CreditDefault.Api.Models;

namespace CreditDefault.Api.Services
{
    public class AuditLogService
    {
        private static readonly HashSet<string> SensitiveKeys = new(StringComparer.OrdinalIgnoreCase)
        {
            "password", "confirmPassword", "token", "accessToken", "refreshToken", "jwt", "secret", "key"
        };

        private readonly IAuditLogRepository _repository;

        public AuditLogService(IAuditLogRepository repository)
        {
            _repository = repository;
        }

        public Task LogRequestAsync(HttpContext context, string? requestBody)
        {
            var routeValues = context.Request.RouteValues;
            var controller = routeValues.TryGetValue("controller", out var c) ? c?.ToString() : context.Request.Path.Value;
            var idValue = routeValues.TryGetValue("id", out var id) ? id?.ToString() : null;
            Guid? entityId = Guid.TryParse(idValue, out var parsedId) ? parsedId : null;
            var userIdValue = context.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            Guid? userId = Guid.TryParse(userIdValue, out var parsedUserId) ? parsedUserId : null;
            var now = DateTime.UtcNow;

            return _repository.AddAsync(new AuditLog
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                Action = string.IsNullOrWhiteSpace(context.Request.Method) ? "UNKNOWN" : context.Request.Method,
                EntityName = string.IsNullOrWhiteSpace(controller) ? "Unknown" : controller,
                EntityId = entityId,
                NewValues = SanitizeJson(requestBody),
                IpAddress = context.Connection.RemoteIpAddress?.ToString() ?? "Unknown",
                CreatedAt = now,
                Timestamp = now,
                PerformedBy = ResolvePerformedBy(context, userIdValue),
                Details = $"{context.Request.Method} {context.Request.Path}".Trim()
            });
        }

        private static string ResolvePerformedBy(HttpContext context, string? userIdValue)
        {
            if (context.User?.Identity?.IsAuthenticated != true)
            {
                return "Anonymous";
            }

            var identityName = context.User.Identity?.Name;
            if (!string.IsNullOrWhiteSpace(identityName))
            {
                return identityName;
            }

            var email = context.User.FindFirst(ClaimTypes.Email)?.Value
                ?? context.User.FindFirst("email")?.Value;
            if (!string.IsNullOrWhiteSpace(email))
            {
                return email;
            }

            if (!string.IsNullOrWhiteSpace(userIdValue))
            {
                return userIdValue;
            }

            return "UnknownUser";
        }

        private static string? SanitizeJson(string? body)
        {
            if (string.IsNullOrWhiteSpace(body)) return null;

            try
            {
                using var document = JsonDocument.Parse(body);
                var sanitized = SanitizeElement(document.RootElement);
                return JsonSerializer.Serialize(sanitized);
            }
            catch (JsonException)
            {
                return null;
            }
        }

        private static object? SanitizeElement(JsonElement element)
        {
            return element.ValueKind switch
            {
                JsonValueKind.Object => element.EnumerateObject().ToDictionary(
                    property => property.Name,
                    property => SensitiveKeys.Any(key => property.Name.Contains(key, StringComparison.OrdinalIgnoreCase))
                        ? "***"
                        : SanitizeElement(property.Value)),
                JsonValueKind.Array => element.EnumerateArray().Select(SanitizeElement).ToList(),
                JsonValueKind.String => element.GetString(),
                JsonValueKind.Number => element.TryGetDecimal(out var value) ? value : element.GetRawText(),
                JsonValueKind.True => true,
                JsonValueKind.False => false,
                _ => null
            };
        }
    }
}
