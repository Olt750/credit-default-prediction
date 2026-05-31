using System;

namespace CreditDefault.Api.DTOs
{
    public class PredictionResultDto
    {
        public int RiskScore { get; set; }
        public string RiskLevel { get; set; }
        public string ExplanationMessage { get; set; }
        public DateTime CreatedAt { get; set; }
        public Guid UserId { get; set; }
    }
}