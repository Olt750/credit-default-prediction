using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using CreditDefault.Api.DTOs;
using CreditDefault.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CreditDefault.Api.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/reports")]
    public class ReportsController : ControllerBase
    {
        private readonly ReportService _reportService;
        private readonly SearchService _searchService;

        public ReportsController(ReportService reportService, SearchService searchService)
        {
            _reportService = reportService;
            _searchService = searchService;
        }

        [HttpGet]
        public async Task<IActionResult> Get([FromQuery] ReportSearchRequest request) =>
            Ok(await _reportService.GetReportsAsync(request, GetUserId(), CanViewAllReports(), _searchService));

        [HttpPost("generate")]
        public async Task<IActionResult> Generate(ReportFilterRequest request)
        {
            if (string.Equals(request.Format, "pdf", StringComparison.OrdinalIgnoreCase))
            {
                return BadRequest(new { error = "PDF report export is reserved for a future implementation. Use csv, xlsx, or json." });
            }

            return Ok(await _reportService.GenerateAsync(request, GetUserId(), CanViewAllReports()));
        }

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var report = await _reportService.GetReportAsync(id, GetUserId(), CanViewAllReports());
            return report == null ? NotFound() : Ok(report);
        }

        [HttpGet("{id:guid}/download")]
        public async Task<IActionResult> Download(Guid id)
        {
            var file = await _reportService.DownloadAsync(id, GetUserId(), CanViewAllReports());
            return file == null ? NotFound() : File(file.Content, file.ContentType, file.FileName);
        }

        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Delete(Guid id) =>
            await _reportService.DeleteAsync(id, GetUserId(), CanViewAllReports())
                ? Ok(new { message = "Report deleted." })
                : NotFound();

        private Guid GetUserId()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue(JwtRegisteredClaimNames.Sub);
            return Guid.Parse(userId!);
        }

        private bool CanViewAllReports() => User.IsInRole("Admin") || User.IsInRole("Manager");
    }
}
