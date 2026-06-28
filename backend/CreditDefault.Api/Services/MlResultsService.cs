namespace CreditDefault.Api.Services
{
    public class MlResultsService
    {
        private readonly IWebHostEnvironment _environment;
        private readonly ILogger<MlResultsService> _logger;

        private static readonly IReadOnlyDictionary<string, string> ResultFiles = new Dictionary<string, string>
        {
            ["model-comparison"] = "model_comparison_results.json",
            ["feature-importance"] = "feature_importance.json",
            ["feature-selection"] = "feature_selection_results.json",
            ["neural-network-results"] = "neural_network_results.json",
            ["clustering-results"] = "clustering_results.json",
            ["clustering-points"] = "clustering_points.json",
            ["model-metadata"] = "model_metadata.json",
            ["summary"] = "experiment_summary.json"
        };

        public MlResultsService(IWebHostEnvironment environment, ILogger<MlResultsService> logger)
        {
            _environment = environment;
            _logger = logger;
        }

        public async Task<(bool Found, string Content, string? Error)> ReadResultAsync(string key)
        {
            if (!ResultFiles.TryGetValue(key, out var fileName))
            {
                return (false, string.Empty, $"Unknown ML result '{key}'.");
            }

            var path = GetResultPath(fileName);
            if (!File.Exists(path))
            {
                _logger.LogWarning("ML result file missing: {Path}", path);
                return (false, string.Empty, $"ML result file '{fileName}' was not found. Run 'python train_model.py' from the ml folder.");
            }

            return (true, await File.ReadAllTextAsync(path), null);
        }

        public async Task<(bool Found, string Content, string? Error)> ReadFeatureImportanceAsync()
        {
            var jsonPath = GetResultPath("feature_importance.json");
            if (File.Exists(jsonPath))
            {
                return (true, await File.ReadAllTextAsync(jsonPath), null);
            }

            var csvPath = GetResultPath("feature_importance.csv");
            if (!File.Exists(csvPath))
            {
                return (false, string.Empty, "Feature importance results were not found. Run 'python train_model.py' from the ml folder.");
            }

            var lines = await File.ReadAllLinesAsync(csvPath);
            if (lines.Length <= 1) return (true, "[]", null);

            var headers = lines[0].Split(',');
            var rows = lines.Skip(1)
                .Select(line => line.Split(','))
                .Where(values => values.Length == headers.Length)
                .Select(values => headers
                    .Select((header, index) => new KeyValuePair<string, string>(header, values[index]))
                    .ToDictionary(pair => pair.Key, pair => pair.Value))
                .ToList();

            return (true, System.Text.Json.JsonSerializer.Serialize(rows), null);
        }

        private string GetResultPath(string fileName)
        {
            return Path.GetFullPath(Path.Combine(_environment.ContentRootPath, "..", "..", "ml", "results", fileName));
        }
    }
}
