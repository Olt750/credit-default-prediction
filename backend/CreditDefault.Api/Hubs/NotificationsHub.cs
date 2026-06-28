using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace CreditDefault.Api.Hubs
{
    [Authorize]
    public class NotificationsHub : Hub
    {
        public override async Task OnConnectedAsync()
        {
            var userId = Context.User?.FindFirstValue(ClaimTypes.NameIdentifier)
                ?? Context.User?.FindFirstValue(JwtRegisteredClaimNames.Sub);

            if (!string.IsNullOrWhiteSpace(userId))
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, UserGroup(userId));
            }

            var roles = Context.User?.FindAll(ClaimTypes.Role).Select(c => c.Value).Distinct() ?? Enumerable.Empty<string>();
            foreach (var role in roles)
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, RoleGroup(role));
            }

            await base.OnConnectedAsync();
        }

        public static string UserGroup(string userId) => $"user:{userId}";
        public static string RoleGroup(string role) => $"role:{role}";
    }
}
