using CreditDefault.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CreditDefault.Api.Controllers
{
    [ApiController]
    [Authorize]
    public class MlResultsController : ControllerBase
    {
        private readonly MlResultsService _mlResultsService;

        public MlResultsController(MlResultsService mlResultsService)
        {
            _mlResultsService = mlResultsService;
        }

        [HttpGet("api/ml/model-comparison")]
        public Task<IActionResult> ModelComparison() => JsonResult("model-comparison");

        [HttpGet("api/ml/feature-importance")]
        public async Task<IActionResult> FeatureImportance()
        {
            var result = await _mlResultsService.ReadFeatureImportanceAsync();
            return ToActionResult(result);
        }

        [HttpGet("api/ml/feature-selection")]
        public Task<IActionResult> FeatureSelection() => JsonResult("feature-selection");

        [HttpGet("api/ml/neural-network-results")]
        public Task<IActionResult> NeuralNetworkResults() => JsonResult("neural-network-results");

        [HttpGet("api/ml/clustering-results")]
        public Task<IActionResult> ClusteringResults() => JsonResult("clustering-results");

        [HttpGet("api/ml/clustering-points")]
        public Task<IActionResult> ClusteringPoints() => JsonResult("clustering-points");

        [HttpGet("api/ml/model-metadata")]
        public Task<IActionResult> ModelMetadata() => JsonResult("model-metadata");

        [HttpGet("api/ml/summary")]
        public Task<IActionResult> Summary() => JsonResult("summary");

        [HttpGet("api/admin/ml/model-comparison")]
        [Authorize(Roles = "Admin")]
        public Task<IActionResult> AdminModelComparison() => JsonResult("model-comparison");

        [HttpGet("api/admin/ml/summary")]
        [Authorize(Roles = "Admin")]
        public Task<IActionResult> AdminSummary() => JsonResult("summary");

        private async Task<IActionResult> JsonResult(string key)
        {
            var result = await _mlResultsService.ReadResultAsync(key);
            return ToActionResult(result);
        }

        private IActionResult ToActionResult((bool Found, string Content, string? Error) result)
        {
            if (!result.Found)
            {
                return NotFound(new { error = result.Error });
            }

            return Content(result.Content, "application/json");
        }
    }
}
