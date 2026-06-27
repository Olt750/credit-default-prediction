using System.Diagnostics;
using System.Text;
using System.Text.Json;
using CreditDefault.Api.DTOs;

namespace CreditDefault.Api.Services
{
    public class PythonCreditRiskPredictionService
    {
        private readonly IWebHostEnvironment _environment;
        private readonly IConfiguration _configuration;
        private readonly ILogger<PythonCreditRiskPredictionService> _logger;

        public PythonCreditRiskPredictionService(
            IWebHostEnvironment environment,
            IConfiguration configuration,
            ILogger<PythonCreditRiskPredictionService> logger)
        {
            _environment = environment;
            _configuration = configuration;
            _logger = logger;
        }

        public async Task<CreditRiskPredictionResponseDto> PredictAsync(CreditRiskPredictionRequestDto request)
        {
            var scriptPath = ResolvePredictionScriptPath();
            var pythonExecutable = Environment.GetEnvironmentVariable("PYTHON_EXECUTABLE_PATH")
                ?? _configuration["ML:PythonExecutable"]
                ?? "python";
            var payload = JsonSerializer.Serialize(request, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });

            using var process = new Process();
            process.StartInfo = new ProcessStartInfo
            {
                FileName = pythonExecutable,
                Arguments = $"\"{scriptPath}\"",
                WorkingDirectory = Path.GetDirectoryName(scriptPath)!,
                RedirectStandardInput = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true,
                StandardOutputEncoding = Encoding.UTF8,
                StandardErrorEncoding = Encoding.UTF8
            };

            process.Start();
            await process.StandardInput.WriteAsync(payload);
            process.StandardInput.Close();

            var outputTask = process.StandardOutput.ReadToEndAsync();
            var errorTask = process.StandardError.ReadToEndAsync();
            await process.WaitForExitAsync();

            var output = await outputTask;
            var error = await errorTask;

            if (process.ExitCode != 0)
            {
                _logger.LogError("Python ML prediction failed: {Error}", error);
                throw new InvalidOperationException("The ML prediction service failed to process the request.");
            }

            var result = JsonSerializer.Deserialize<CreditRiskPredictionResponseDto>(
                output,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            return result ?? throw new InvalidOperationException("The ML prediction service returned an empty response.");
        }

        private string ResolvePredictionScriptPath()
        {
            var configuredPath = Environment.GetEnvironmentVariable("ML_SCRIPT_PATH")
                ?? _configuration["ML:PredictionScriptPath"];
            if (!string.IsNullOrWhiteSpace(configuredPath))
            {
                return Path.GetFullPath(configuredPath);
            }

            var current = new DirectoryInfo(_environment.ContentRootPath);
            while (current != null)
            {
                var candidate = Path.Combine(current.FullName, "ml", "predict_credit_risk.py");
                if (File.Exists(candidate))
                {
                    return candidate;
                }

                current = current.Parent;
            }

            throw new FileNotFoundException("Could not find ml/predict_credit_risk.py from the backend content root.");
        }
    }
}
