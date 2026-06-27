using System.ComponentModel.DataAnnotations;

namespace CreditDefault.Api.DTOs
{
    public class CreditRiskPredictionRequestDto
    {
        [Range(1, int.MaxValue, ErrorMessage = "Age must be greater than 0.")]
        public int Age { get; set; }

        [Range(typeof(decimal), "0.01", "79228162514264337593543950335", ErrorMessage = "Annual income must be greater than 0.")]
        public decimal AnnualIncome { get; set; }

        [Range(typeof(decimal), "0.01", "79228162514264337593543950335", ErrorMessage = "Loan amount must be greater than 0.")]
        public decimal LoanAmount { get; set; }

        [Range(300, 850, ErrorMessage = "Credit score must be between 300 and 850.")]
        public int CreditScore { get; set; }

        public string EmploymentStatus { get; set; } = string.Empty;

        [Range(1, int.MaxValue, ErrorMessage = "Loan term must be greater than 0.")]
        public int LoanTerm { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "Previous defaults cannot be negative.")]
        public int PreviousDefaults { get; set; }

        [Range(typeof(decimal), "0", "79228162514264337593543950335", ErrorMessage = "Debt-to-income ratio cannot be negative.")]
        public decimal DebtToIncomeRatio { get; set; }

        public string Education { get; set; } = string.Empty;

        public string MaritalStatus { get; set; } = string.Empty;

        [Range(typeof(decimal), "0", "79228162514264337593543950335", ErrorMessage = "Monthly debt values cannot be negative.")]
        public decimal MonthlyCarLoanPayment { get; set; }

        [Range(typeof(decimal), "0", "79228162514264337593543950335", ErrorMessage = "Monthly debt values cannot be negative.")]
        public decimal MonthlyMortgageOrRentPayment { get; set; }

        [Range(typeof(decimal), "0", "79228162514264337593543950335", ErrorMessage = "Monthly debt values cannot be negative.")]
        public decimal MonthlyPersonalLoanPayment { get; set; }

        [Range(typeof(decimal), "0", "79228162514264337593543950335", ErrorMessage = "Monthly debt values cannot be negative.")]
        public decimal MonthlyCreditCardPayment { get; set; }

        [Range(typeof(decimal), "0", "79228162514264337593543950335", ErrorMessage = "Monthly debt values cannot be negative.")]
        public decimal MonthlyOtherDebtPayment { get; set; }

        [Range(typeof(decimal), "0", "79228162514264337593543950335", ErrorMessage = "Total monthly debt cannot be negative.")]
        public decimal TotalMonthlyDebt { get; set; }
    }
}
