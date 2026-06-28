using CreditDefault.Api.Data;
using CreditDefault.Api.DTOs;
using CreditDefault.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace CreditDefault.Api.Services
{
    public class DataImportService
    {
        private readonly AppDbContext _context;
        private readonly TabularFileService _fileService;
        private readonly PasswordService _passwordService;
        private readonly NotificationService _notificationService;

        public DataImportService(
            AppDbContext context,
            TabularFileService fileService,
            PasswordService passwordService,
            NotificationService notificationService)
        {
            _context = context;
            _fileService = fileService;
            _passwordService = passwordService;
            _notificationService = notificationService;
        }

        public async Task<ImportResultDto> ImportAsync(string dataType, IFormFile file, Guid currentUserId, bool isAdmin)
        {
            if (file.Length == 0) throw new InvalidOperationException("Import file is empty.");
            if (Path.GetExtension(file.FileName).Equals(".xlsx", StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException("Excel import is not enabled yet. Use CSV or JSON for imports.");
            }

            var rows = await _fileService.ReadRowsAsync(file);
            var result = dataType.ToLowerInvariant() switch
            {
                "users" when isAdmin => await ImportUsersAsync(rows),
                "predictions" => await ImportPredictionsAsync(rows, currentUserId, isAdmin),
                "client-profiles" => await ImportClientProfilesAsync(rows, currentUserId, isAdmin),
                "notifications" => await ImportNotificationsAsync(rows, currentUserId, isAdmin),
                "reports" => await ImportReportsAsync(rows, currentUserId, isAdmin),
                _ => throw new UnauthorizedAccessException("You are not allowed to import this data type.")
            };

            await _notificationService.CreateNotificationAsync(
                currentUserId,
                "ImportCompleted",
                "Import Completed",
                $"{dataType} import completed: {result.InsertedRows} inserted, {result.SkippedRows} skipped, {result.FailedRows} failed.");

            return result;
        }

        private async Task<ImportResultDto> ImportUsersAsync(List<Dictionary<string, string>> rows)
        {
            var result = NewResult(rows);
            var userRole = await _context.Roles.FirstOrDefaultAsync(r => r.Name == "User");

            foreach (var row in rows)
            {
                try
                {
                    var email = Value(row, "Email");
                    var password = Value(row, "Password");
                    if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
                    {
                        Fail(result, "User row requires Email and Password. Password is hashed before storage.");
                        continue;
                    }

                    if (await _context.Users.AnyAsync(u => u.Email == email))
                    {
                        result.SkippedRows++;
                        continue;
                    }

                    var now = DateTime.UtcNow;
                    var user = new User
                    {
                        Id = Guid.NewGuid(),
                        FullName = Value(row, "FullName") ?? Value(row, "Name") ?? email,
                        Email = email,
                        PhoneNumber = Value(row, "PhoneNumber") ?? string.Empty,
                        Role = Value(row, "Role") ?? "User",
                        PasswordHash = _passwordService.HashPassword(password),
                        CreatedAt = now,
                        UpdatedAt = now
                    };
                    _context.Users.Add(user);
                    if (userRole != null)
                    {
                        user.UserRoles.Add(new UserRole { UserId = user.Id, RoleId = userRole.Id, CreatedAt = now });
                    }
                    result.InsertedRows++;
                }
                catch (Exception ex)
                {
                    Fail(result, ex.Message);
                }
            }

            await _context.SaveChangesAsync();
            return result;
        }

        private async Task<ImportResultDto> ImportPredictionsAsync(List<Dictionary<string, string>> rows, Guid currentUserId, bool isAdmin)
        {
            var result = NewResult(rows);

            foreach (var row in rows)
            {
                try
                {
                    var userId = await ResolveUserIdAsync(row, currentUserId, isAdmin);
                    if (userId == null)
                    {
                        Fail(result, "Prediction row user was not found or is not allowed.");
                        continue;
                    }

                    if (TryGuid(Value(row, "Id"), out var id) && await _context.Predictions.AnyAsync(p => p.Id == id))
                    {
                        result.SkippedRows++;
                        continue;
                    }

                    _context.Predictions.Add(new Prediction
                    {
                        Id = id == Guid.Empty ? Guid.NewGuid() : id,
                        UserId = userId.Value,
                        Age = Int(row, "Age"),
                        Income = Decimal(row, "Income"),
                        EmploymentStatus = Value(row, "EmploymentStatus") ?? "Unknown",
                        CreditScore = Int(row, "CreditScore"),
                        ExistingDebt = Decimal(row, "ExistingDebt"),
                        LoanAmount = Decimal(row, "LoanAmount"),
                        LoanTerm = Int(row, "LoanTerm"),
                        PaymentHistory = Value(row, "PaymentHistory") ?? "Imported",
                        RiskScore = Int(row, "RiskScore"),
                        RiskLevel = Value(row, "RiskLevel") ?? "Medium",
                        ExplanationMessage = Value(row, "ExplanationMessage") ?? "Imported prediction.",
                        CreatedAt = DateTime.UtcNow
                    });
                    result.InsertedRows++;
                }
                catch (Exception ex)
                {
                    Fail(result, ex.Message);
                }
            }

            await _context.SaveChangesAsync();
            return result;
        }

        private async Task<ImportResultDto> ImportClientProfilesAsync(List<Dictionary<string, string>> rows, Guid currentUserId, bool isAdmin)
        {
            var result = NewResult(rows);

            foreach (var row in rows)
            {
                try
                {
                    var userId = await ResolveUserIdAsync(row, currentUserId, isAdmin);
                    if (userId == null)
                    {
                        Fail(result, "Client profile row user was not found or is not allowed.");
                        continue;
                    }

                    if (await _context.ClientProfiles.AnyAsync(p => p.UserId == userId.Value))
                    {
                        result.SkippedRows++;
                        continue;
                    }

                    var annualIncome = Decimal(row, "AnnualIncome");
                    var totalDebt = Decimal(row, "TotalMonthlyDebt");
                    _context.ClientProfiles.Add(new ClientProfile
                    {
                        Id = Guid.NewGuid(),
                        UserId = userId.Value,
                        Age = Int(row, "Age"),
                        AnnualIncome = annualIncome,
                        LoanAmount = Decimal(row, "LoanAmount"),
                        CreditScore = Int(row, "CreditScore"),
                        EmploymentStatus = Value(row, "EmploymentStatus") ?? "Unknown",
                        LoanTermMonths = Int(row, "LoanTermMonths"),
                        PreviousDefaults = Int(row, "PreviousDefaults"),
                        Education = Value(row, "Education") ?? string.Empty,
                        MaritalStatus = Value(row, "MaritalStatus") ?? string.Empty,
                        TotalMonthlyDebt = totalDebt,
                        DebtToIncomeRatio = annualIncome > 0 ? totalDebt / (annualIncome / 12) : 0,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    });
                    result.InsertedRows++;
                }
                catch (Exception ex)
                {
                    Fail(result, ex.Message);
                }
            }

            await _context.SaveChangesAsync();
            return result;
        }

        private async Task<ImportResultDto> ImportNotificationsAsync(List<Dictionary<string, string>> rows, Guid currentUserId, bool isAdmin)
        {
            var result = NewResult(rows);

            foreach (var row in rows)
            {
                try
                {
                    var userId = await ResolveUserIdAsync(row, currentUserId, isAdmin) ?? currentUserId;
                    if (!isAdmin && userId != currentUserId)
                    {
                        Fail(result, "Users can only import their own notifications.");
                        continue;
                    }

                    _context.Notifications.Add(new Notification
                    {
                        Id = Guid.NewGuid(),
                        UserId = userId,
                        Type = Value(row, "Type") ?? "Info",
                        Title = Value(row, "Title") ?? "Imported Notification",
                        Message = Value(row, "Message") ?? string.Empty,
                        IsRead = Bool(row, "IsRead"),
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    });
                    result.InsertedRows++;
                }
                catch (Exception ex)
                {
                    Fail(result, ex.Message);
                }
            }

            await _context.SaveChangesAsync();
            return result;
        }

        private async Task<ImportResultDto> ImportReportsAsync(List<Dictionary<string, string>> rows, Guid currentUserId, bool isAdmin)
        {
            var result = NewResult(rows);

            foreach (var row in rows)
            {
                try
                {
                    if (TryGuid(Value(row, "Id"), out var id) && await _context.Reports.AnyAsync(r => r.Id == id))
                    {
                        result.SkippedRows++;
                        continue;
                    }

                    var createdBy = isAdmin && TryGuid(Value(row, "CreatedByUserId"), out var parsedUserId) ? parsedUserId : currentUserId;
                    _context.Reports.Add(new Report
                    {
                        Id = id == Guid.Empty ? Guid.NewGuid() : id,
                        CreatedByUserId = createdBy,
                        Name = Value(row, "Name") ?? "Imported Report",
                        ReportType = Value(row, "ReportType") ?? "Prediction Summary Report",
                        ParametersJson = "{}",
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    });
                    result.InsertedRows++;
                }
                catch (Exception ex)
                {
                    Fail(result, ex.Message);
                }
            }

            await _context.SaveChangesAsync();
            return result;
        }

        private async Task<Guid?> ResolveUserIdAsync(Dictionary<string, string> row, Guid currentUserId, bool isAdmin)
        {
            if (!isAdmin) return currentUserId;
            if (TryGuid(Value(row, "UserId"), out var userId) && await _context.Users.AnyAsync(u => u.Id == userId)) return userId;
            var email = Value(row, "UserEmail") ?? Value(row, "Email");
            if (!string.IsNullOrWhiteSpace(email))
            {
                return await _context.Users.Where(u => u.Email == email).Select(u => (Guid?)u.Id).FirstOrDefaultAsync();
            }
            return null;
        }

        private static ImportResultDto NewResult(List<Dictionary<string, string>> rows) => new() { TotalRows = rows.Count };

        private static void Fail(ImportResultDto result, string error)
        {
            result.FailedRows++;
            result.Errors.Add(error);
        }

        private static string? Value(Dictionary<string, string> row, string key) =>
            row.TryGetValue(key, out var value) && !string.IsNullOrWhiteSpace(value) ? value.Trim() : null;

        private static int Int(Dictionary<string, string> row, string key) =>
            int.TryParse(Value(row, key), out var value) ? value : 0;

        private static decimal Decimal(Dictionary<string, string> row, string key) =>
            decimal.TryParse(Value(row, key), out var value) ? value : 0;

        private static bool Bool(Dictionary<string, string> row, string key) =>
            bool.TryParse(Value(row, key), out var value) && value;

        private static bool TryGuid(string? value, out Guid id) =>
            Guid.TryParse(value, out id);
    }
}
