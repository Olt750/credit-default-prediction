namespace CreditDefault.Api.DTOs
{
    public class CreditRiskPredictionResponseDto
    {
        public string RiskLevel { get; set; } = string.Empty;
        public int RiskScore { get; set; }
        public int Prediction { get; set; }
        public string Explanation { get; set; } = string.Empty;
    }
}
