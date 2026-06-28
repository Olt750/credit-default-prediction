using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using CreditDefault.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CreditDefault.Api.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/import")]
    public class ImportController : ControllerBase
    {
        private readonly DataImportService _importService;

        public ImportController(DataImportService importService)
        {
            _importService = importService;
        }

        [HttpPost("users")]
        [Authorize(Roles = "Admin")]
        public Task<IActionResult> Users(IFormFile file) => Import("users", file);

        [HttpPost("predictions")]
        public Task<IActionResult> Predictions(IFormFile file) => Import("predictions", file);

        [HttpPost("client-profiles")]
        public Task<IActionResult> ClientProfiles(IFormFile file) => Import("client-profiles", file);

        [HttpPost("notifications")]
        public Task<IActionResult> Notifications(IFormFile file) => Import("notifications", file);

        [HttpPost("reports")]
        public Task<IActionResult> Reports(IFormFile file) => Import("reports", file);

        private async Task<IActionResult> Import(string dataType, IFormFile file)
        {
            if (file == null) return BadRequest(new { error = "A CSV or JSON file is required." });

            try
            {
                return Ok(await _importService.ImportAsync(dataType, file, GetUserId(), User.IsInRole("Admin")));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
            catch (UnauthorizedAccessException ex)
            {
                return StatusCode(StatusCodes.Status403Forbidden, new { error = ex.Message });
            }
        }

        private Guid GetUserId()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue(JwtRegisteredClaimNames.Sub);
            return Guid.Parse(userId!);
        }
    }
}
