using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CreditDefault.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddClientFinancialProfile : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "DebtToIncomeRatio",
                table: "Predictions",
                type: "decimal(18,4)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "Education",
                table: "Predictions",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "LoanStatus",
                table: "Predictions",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "MaritalStatus",
                table: "Predictions",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<decimal>(
                name: "MonthlyCarLoanPayment",
                table: "Predictions",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "MonthlyCreditCardPayment",
                table: "Predictions",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "MonthlyMortgageOrRentPayment",
                table: "Predictions",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "MonthlyOtherDebtPayment",
                table: "Predictions",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "MonthlyPersonalLoanPayment",
                table: "Predictions",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<int>(
                name: "PreviousDefaults",
                table: "Predictions",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<decimal>(
                name: "TotalMonthlyDebt",
                table: "Predictions",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.CreateTable(
                name: "ClientProfiles",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Age = table.Column<int>(type: "int", nullable: false),
                    AnnualIncome = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    LoanAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    CreditScore = table.Column<int>(type: "int", nullable: false),
                    EmploymentStatus = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    LoanTermMonths = table.Column<int>(type: "int", nullable: false),
                    PreviousDefaults = table.Column<int>(type: "int", nullable: false),
                    Education = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    MaritalStatus = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    MonthlyCarLoanPayment = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    MonthlyMortgageOrRentPayment = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    MonthlyPersonalLoanPayment = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    MonthlyCreditCardPayment = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    MonthlyOtherDebtPayment = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TotalMonthlyDebt = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    DebtToIncomeRatio = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClientProfiles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ClientProfiles_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ClientProfiles_UserId",
                table: "ClientProfiles",
                column: "UserId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ClientProfiles");

            migrationBuilder.DropColumn(
                name: "DebtToIncomeRatio",
                table: "Predictions");

            migrationBuilder.DropColumn(
                name: "Education",
                table: "Predictions");

            migrationBuilder.DropColumn(
                name: "LoanStatus",
                table: "Predictions");

            migrationBuilder.DropColumn(
                name: "MaritalStatus",
                table: "Predictions");

            migrationBuilder.DropColumn(
                name: "MonthlyCarLoanPayment",
                table: "Predictions");

            migrationBuilder.DropColumn(
                name: "MonthlyCreditCardPayment",
                table: "Predictions");

            migrationBuilder.DropColumn(
                name: "MonthlyMortgageOrRentPayment",
                table: "Predictions");

            migrationBuilder.DropColumn(
                name: "MonthlyOtherDebtPayment",
                table: "Predictions");

            migrationBuilder.DropColumn(
                name: "MonthlyPersonalLoanPayment",
                table: "Predictions");

            migrationBuilder.DropColumn(
                name: "PreviousDefaults",
                table: "Predictions");

            migrationBuilder.DropColumn(
                name: "TotalMonthlyDebt",
                table: "Predictions");
        }
    }
}
