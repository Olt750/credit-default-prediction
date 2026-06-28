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
    [Route("api/search")]
    public class SearchController : ControllerBase
    {
        private readonly SearchService _searchService;

        public SearchController(SearchService searchService)
        {
            _searchService = searchService;
        }

        [HttpGet("users")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Users([FromQuery] UserSearchRequest request) =>
            Ok(await _searchService.SearchUsersAsync(request));

        [HttpGet("predictions")]
        public async Task<IActionResult> Predictions([FromQuery] PredictionSearchRequest request) =>
            Ok(await _searchService.SearchPredictionsAsync(request, GetUserId(), CanViewAll()));

        [HttpGet("client-profiles")]
        public async Task<IActionResult> ClientProfiles([FromQuery] ClientProfileSearchRequest request) =>
            Ok(await _searchService.SearchClientProfilesAsync(request, GetUserId(), CanViewAll()));

        [HttpGet("notifications")]
        public async Task<IActionResult> Notifications([FromQuery] NotificationSearchRequest request) =>
            Ok(await _searchService.SearchNotificationsAsync(request, GetUserId(), User.IsInRole("Admin")));

        [HttpGet("reports")]
        public async Task<IActionResult> Reports([FromQuery] ReportSearchRequest request) =>
            Ok(await _searchService.SearchReportsAsync(request, GetUserId(), CanViewAll()));

        private Guid GetUserId()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue(JwtRegisteredClaimNames.Sub);
            return Guid.Parse(userId!);
        }

        private bool CanViewAll() => User.IsInRole("Admin") || User.IsInRole("Manager");
    }
}
