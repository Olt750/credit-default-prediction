using System;
using System.ComponentModel.DataAnnotations;

namespace CreditDefault.Api.DTOs
{
    public class UpsertClientProfileDto
    {
        [Range(18, int.MaxValue, ErrorMessage = "Age must be at least 18.")]
        public int Age { get; set; }

        [Range(typeof(decimal), "0.01", "79228162514264337593543950335", ErrorMessage = "Annual income must be greater than 0.")]
        public decimal AnnualIncome { get; set; }

        [Range(typeof(decimal), "0.01", "79228162514264337593543950335", ErrorMessage = "Loan amount must be greater than 0.")]
        public decimal LoanAmount { get; set; }

        [Range(300, 850, ErrorMessage = "Credit score must be between 300 and 850.")]
        public int CreditScore { get; set; }

        public string EmploymentStatus { get; set; } = string.Empty;

        [Range(1, int.MaxValue, ErrorMessage = "Loan term must be greater than 0.")]
        public int LoanTermMonths { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "Previous defaults cannot be negative.")]
        public int PreviousDefaults { get; set; }

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
    }

    public class CreateClientProfileDto : UpsertClientProfileDto { }

    public class UpdateClientProfileDto : UpsertClientProfileDto { }

    public class ClientProfileDto : UpsertClientProfileDto
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public decimal TotalMonthlyDebt { get; set; }
        public decimal DebtToIncomeRatio { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
