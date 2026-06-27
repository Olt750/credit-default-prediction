using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Threading.Tasks;
using CreditDefault.Api.Data;
using CreditDefault.Api.DTOs;
using CreditDefault.Api.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CreditDefault.Api.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/client-profile")]
    public class ClientProfileController : ControllerBase
    {
        private readonly AppDbContext _context;

        public ClientProfileController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet("me")]
        public async Task<IActionResult> GetMyProfile()
        {
            var userId = GetUserId();
            if (userId == null) return Unauthorized();

            var profile = await _context.ClientProfiles.AsNoTracking().FirstOrDefaultAsync(p => p.UserId == userId.Value);
            return profile == null ? NotFound() : Ok(ToDto(profile));
        }

        [HttpPost]
        public async Task<IActionResult> CreateProfile(CreateClientProfileDto dto)
        {
            if (!ModelState.IsValid) return ValidationProblem(ModelState);

            var userId = GetUserId();
            if (userId == null) return Unauthorized();

            var existing = await _context.ClientProfiles.FirstOrDefaultAsync(p => p.UserId == userId.Value);
            if (existing != null)
            {
                Apply(existing, dto);
                existing.UpdatedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();
                return Ok(ToDto(existing));
            }

            var profile = new ClientProfile
            {
                Id = Guid.NewGuid(),
                UserId = userId.Value,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            Apply(profile, dto);
            _context.ClientProfiles.Add(profile);
            await _context.SaveChangesAsync();

            return Ok(ToDto(profile));
        }

        [HttpPut("me")]
        public async Task<IActionResult> UpdateMyProfile(UpdateClientProfileDto dto)
        {
            if (!ModelState.IsValid) return ValidationProblem(ModelState);

            var userId = GetUserId();
            if (userId == null) return Unauthorized();

            var profile = await _context.ClientProfiles.FirstOrDefaultAsync(p => p.UserId == userId.Value);
            if (profile == null) return NotFound();

            Apply(profile, dto);
            profile.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return Ok(ToDto(profile));
        }

        private Guid? GetUserId()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue(JwtRegisteredClaimNames.Sub);
            return Guid.TryParse(userId, out var parsed) ? parsed : null;
        }

        private static void Apply(ClientProfile profile, UpsertClientProfileDto dto)
        {
            var totalMonthlyDebt =
                dto.MonthlyCarLoanPayment +
                dto.MonthlyMortgageOrRentPayment +
                dto.MonthlyPersonalLoanPayment +
                dto.MonthlyCreditCardPayment +
                dto.MonthlyOtherDebtPayment;

            profile.Age = dto.Age;
            profile.AnnualIncome = dto.AnnualIncome;
            profile.LoanAmount = dto.LoanAmount;
            profile.CreditScore = dto.CreditScore;
            profile.EmploymentStatus = dto.EmploymentStatus;
            profile.LoanTermMonths = dto.LoanTermMonths;
            profile.PreviousDefaults = dto.PreviousDefaults;
            profile.Education = dto.Education;
            profile.MaritalStatus = dto.MaritalStatus;
            profile.MonthlyCarLoanPayment = dto.MonthlyCarLoanPayment;
            profile.MonthlyMortgageOrRentPayment = dto.MonthlyMortgageOrRentPayment;
            profile.MonthlyPersonalLoanPayment = dto.MonthlyPersonalLoanPayment;
            profile.MonthlyCreditCardPayment = dto.MonthlyCreditCardPayment;
            profile.MonthlyOtherDebtPayment = dto.MonthlyOtherDebtPayment;
            profile.TotalMonthlyDebt = totalMonthlyDebt;
            profile.DebtToIncomeRatio = dto.AnnualIncome > 0 ? totalMonthlyDebt / (dto.AnnualIncome / 12) : 0;
        }

        private static ClientProfileDto ToDto(ClientProfile profile)
        {
            return new ClientProfileDto
            {
                Id = profile.Id,
                UserId = profile.UserId,
                Age = profile.Age,
                AnnualIncome = profile.AnnualIncome,
                LoanAmount = profile.LoanAmount,
                CreditScore = profile.CreditScore,
                EmploymentStatus = profile.EmploymentStatus,
                LoanTermMonths = profile.LoanTermMonths,
                PreviousDefaults = profile.PreviousDefaults,
                Education = profile.Education,
                MaritalStatus = profile.MaritalStatus,
                MonthlyCarLoanPayment = profile.MonthlyCarLoanPayment,
                MonthlyMortgageOrRentPayment = profile.MonthlyMortgageOrRentPayment,
                MonthlyPersonalLoanPayment = profile.MonthlyPersonalLoanPayment,
                MonthlyCreditCardPayment = profile.MonthlyCreditCardPayment,
                MonthlyOtherDebtPayment = profile.MonthlyOtherDebtPayment,
                TotalMonthlyDebt = profile.TotalMonthlyDebt,
                DebtToIncomeRatio = profile.DebtToIncomeRatio,
                CreatedAt = profile.CreatedAt,
                UpdatedAt = profile.UpdatedAt
            };
        }
    }
}
