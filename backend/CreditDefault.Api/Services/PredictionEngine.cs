using System;

namespace CreditDefault.Api.Services
{
    public class PredictionEngine
    {
        public (int riskScore, string riskLevel, string explanation) Predict(int creditScore, decimal income, decimal existingDebt, string employmentStatus)
        {
            // Rule-based logic
            if (creditScore < 550 && existingDebt > income * 0.5m && income < 20000)
                return (85, "High", "Low credit score and high debt increase default risk.");
            if (creditScore >= 700 && existingDebt < income * 0.2m && employmentStatus == "Employed")
                return (20, "Low", "Good credit score, low debt, and stable income.");
            if (creditScore >= 600 && creditScore < 700 && existingDebt < income * 0.35m)
                return (50, "Medium", "Average metrics, moderate risk.");
            return (65, "Medium", "Default risk based on provided data.");
        }
    }
}