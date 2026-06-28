using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using CreditDefault.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CreditDefault.Api.Controllers
{
    [ApiController]
    [Route("api/notifications")]
    [Authorize]
    public class NotificationsController : ControllerBase
    {
        private readonly NotificationService _service;
        public NotificationsController(NotificationService service) => _service = service;

        [HttpGet]
        public async Task<IActionResult> Get([FromQuery] int page = 1, [FromQuery] int pageSize = 20) =>
            Ok(await _service.GetForUserAsync(GetUserId(), page, pageSize));

        [HttpGet("unread-count")]
        public async Task<IActionResult> GetUnreadCount() => Ok(new { count = await _service.GetUnreadCountAsync(GetUserId()) });

        [HttpPut("{id:guid}/read")]
        public async Task<IActionResult> MarkRead(Guid id) =>
            await _service.MarkReadAsync(GetUserId(), id) ? Ok(new { message = "Notification marked as read." }) : NotFound();

        [HttpPut("read-all")]
        public async Task<IActionResult> MarkAllRead()
        {
            await _service.MarkAllReadAsync(GetUserId());
            return Ok(new { message = "All notifications marked as read." });
        }

        private Guid GetUserId()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue(JwtRegisteredClaimNames.Sub);
            return Guid.Parse(userId!);
        }
    }
}
