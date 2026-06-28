using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using CreditDefault.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CreditDefault.Api.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/export")]
    public class ExportController : ControllerBase
    {
        private readonly DataExportService _exportService;

        public ExportController(DataExportService exportService)
        {
            _exportService = exportService;
        }

        [HttpGet("users")]
        [Authorize(Roles = "Admin")]
        public Task<IActionResult> Users([FromQuery] string format = "csv") => Export("users", format);

        [HttpGet("predictions")]
        public Task<IActionResult> Predictions([FromQuery] string format = "csv") => Export("predictions", format);

        [HttpGet("client-profiles")]
        public Task<IActionResult> ClientProfiles([FromQuery] string format = "csv") => Export("client-profiles", format);

        [HttpGet("notifications")]
        public Task<IActionResult> Notifications([FromQuery] string format = "csv") => Export("notifications", format);

        [HttpGet("reports")]
        public Task<IActionResult> Reports([FromQuery] string format = "csv") => Export("reports", format);

        [HttpGet("audit-logs")]
        [Authorize(Roles = "Admin")]
        public Task<IActionResult> AuditLogs([FromQuery] string format = "csv") => Export("audit-logs", format);

        private async Task<IActionResult> Export(string dataType, string format)
        {
            var file = await _exportService.ExportAsync(dataType, format, GetUserId(), User.IsInRole("Admin"));
            return File(file.Content, file.ContentType, file.FileName);
        }

        private Guid GetUserId()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue(JwtRegisteredClaimNames.Sub);
            return Guid.Parse(userId!);
        }
    }
}
