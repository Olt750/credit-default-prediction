using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CreditDefault.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddLabCourseTechnicalFoundation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "UserId",
                table: "AuditLogs",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "EntityName",
                table: "AuditLogs",
                type: "nvarchar(256)",
                maxLength: 256,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<Guid>(
                name: "EntityId",
                table: "AuditLogs",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "OldValues",
                table: "AuditLogs",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NewValues",
                table: "AuditLogs",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "IpAddress",
                table: "AuditLogs",
                type: "nvarchar(64)",
                maxLength: 64,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "AuditLogs",
                type: "datetime2",
                nullable: false,
                defaultValueSql: "SYSUTCDATETIME()");

            migrationBuilder.CreateTable(
                name: "Roles",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table => table.PrimaryKey("PK_Roles", x => x.Id));

            migrationBuilder.CreateTable(
                name: "Permissions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table => table.PrimaryKey("PK_Permissions", x => x.Id));

            migrationBuilder.CreateTable(
                name: "Settings",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Key = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    Value = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsSensitive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table => table.PrimaryKey("PK_Settings", x => x.Id));

            migrationBuilder.CreateTable(
                name: "LoanApplicationStatuses",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table => table.PrimaryKey("PK_LoanApplicationStatuses", x => x.Id));

            migrationBuilder.CreateTable(
                name: "LoanTypes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table => table.PrimaryKey("PK_LoanTypes", x => x.Id));

            migrationBuilder.CreateTable(
                name: "RiskFactors",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Code = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Weight = table.Column<decimal>(type: "decimal(10,4)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table => table.PrimaryKey("PK_RiskFactors", x => x.Id));

            migrationBuilder.CreateTable(
                name: "ModelRuns",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ModelName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ModelVersion = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DatasetName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    StartedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CompletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table => table.PrimaryKey("PK_ModelRuns", x => x.Id));

            migrationBuilder.CreateTable(
                name: "UserRoles",
                columns: table => new
                {
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RoleId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserRoles", x => new { x.UserId, x.RoleId });
                    table.ForeignKey("FK_UserRoles_Roles_RoleId", x => x.RoleId, "Roles", "Id", onDelete: ReferentialAction.Cascade);
                    table.ForeignKey("FK_UserRoles_Users_UserId", x => x.UserId, "Users", "Id", onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RolePermissions",
                columns: table => new
                {
                    RoleId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PermissionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RolePermissions", x => new { x.RoleId, x.PermissionId });
                    table.ForeignKey("FK_RolePermissions_Permissions_PermissionId", x => x.PermissionId, "Permissions", "Id", onDelete: ReferentialAction.Cascade);
                    table.ForeignKey("FK_RolePermissions_Roles_RoleId", x => x.RoleId, "Roles", "Id", onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RefreshTokens",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TokenHash = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    ExpiresAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    RevokedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedByIp = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RevokedByIp = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ReplacedByTokenHash = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RefreshTokens", x => x.Id);
                    table.ForeignKey("FK_RefreshTokens_Users_UserId", x => x.UserId, "Users", "Id", onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Notifications",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Message = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Type = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsRead = table.Column<bool>(type: "bit", nullable: false),
                    ReadAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Notifications", x => x.Id);
                    table.ForeignKey("FK_Notifications_Users_UserId", x => x.UserId, "Users", "Id", onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Files",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ClientProfileId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    FileName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    OriginalFileName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ContentType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SizeBytes = table.Column<long>(type: "bigint", nullable: false),
                    StoragePath = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Category = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Files", x => x.Id);
                    table.ForeignKey("FK_Files_ClientProfiles_ClientProfileId", x => x.ClientProfileId, "ClientProfiles", "Id");
                    table.ForeignKey("FK_Files_Users_UserId", x => x.UserId, "Users", "Id");
                });

            migrationBuilder.CreateTable(
                name: "EmploymentInfos",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ClientProfileId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EmployerName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    EmploymentStatus = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    JobTitle = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    MonthlyIncome = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    StartDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmploymentInfos", x => x.Id);
                    table.ForeignKey("FK_EmploymentInfos_ClientProfiles_ClientProfileId", x => x.ClientProfileId, "ClientProfiles", "Id", onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "IncomeSources",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ClientProfileId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SourceType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    MonthlyAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    IsPrimary = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IncomeSources", x => x.Id);
                    table.ForeignKey("FK_IncomeSources_ClientProfiles_ClientProfileId", x => x.ClientProfileId, "ClientProfiles", "Id", onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DebtObligations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ClientProfileId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DebtType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreditorName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    OutstandingBalance = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    MonthlyPayment = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DebtObligations", x => x.Id);
                    table.ForeignKey("FK_DebtObligations_ClientProfiles_ClientProfileId", x => x.ClientProfileId, "ClientProfiles", "Id", onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CreditScores",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ClientProfileId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Score = table.Column<int>(type: "int", nullable: false),
                    Provider = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ScoreDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CreditScores", x => x.Id);
                    table.ForeignKey("FK_CreditScores_ClientProfiles_ClientProfileId", x => x.ClientProfileId, "ClientProfiles", "Id", onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "LoanApplications",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ClientProfileId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    StatusId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    LoanTypeId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RequestedAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TermMonths = table.Column<int>(type: "int", nullable: false),
                    Purpose = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LoanApplications", x => x.Id);
                    table.ForeignKey("FK_LoanApplications_ClientProfiles_ClientProfileId", x => x.ClientProfileId, "ClientProfiles", "Id", onDelete: ReferentialAction.Cascade);
                    table.ForeignKey("FK_LoanApplications_LoanApplicationStatuses_StatusId", x => x.StatusId, "LoanApplicationStatuses", "Id", onDelete: ReferentialAction.Cascade);
                    table.ForeignKey("FK_LoanApplications_LoanTypes_LoanTypeId", x => x.LoanTypeId, "LoanTypes", "Id", onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PredictionFactors",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PredictionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RiskFactorId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Value = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    ImpactScore = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PredictionFactors", x => x.Id);
                    table.ForeignKey("FK_PredictionFactors_Predictions_PredictionId", x => x.PredictionId, "Predictions", "Id", onDelete: ReferentialAction.Cascade);
                    table.ForeignKey("FK_PredictionFactors_RiskFactors_RiskFactorId", x => x.RiskFactorId, "RiskFactors", "Id", onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ModelMetrics",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ModelRunId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    MetricName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    MetricValue = table.Column<decimal>(type: "decimal(18,6)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ModelMetrics", x => x.Id);
                    table.ForeignKey("FK_ModelMetrics_ModelRuns_ModelRunId", x => x.ModelRunId, "ModelRuns", "Id", onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Reports",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ReportType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ParametersJson = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Reports", x => x.Id);
                    table.ForeignKey("FK_Reports_Users_CreatedByUserId", x => x.CreatedByUserId, "Users", "Id", onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserActivities",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ActivityType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    EntityName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    EntityId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    IpAddress = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserActivities", x => x.Id);
                    table.ForeignKey("FK_UserActivities_Users_UserId", x => x.UserId, "Users", "Id");
                });

            migrationBuilder.CreateTable(
                name: "ClientDocuments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ClientProfileId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FileRecordId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DocumentType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClientDocuments", x => x.Id);
                    table.ForeignKey("FK_ClientDocuments_ClientProfiles_ClientProfileId", x => x.ClientProfileId, "ClientProfiles", "Id", onDelete: ReferentialAction.Cascade);
                    table.ForeignKey("FK_ClientDocuments_Files_FileRecordId", x => x.FileRecordId, "Files", "Id", onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ReportExports",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ReportId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FileRecordId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Format = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ExportedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReportExports", x => x.Id);
                    table.ForeignKey("FK_ReportExports_Files_FileRecordId", x => x.FileRecordId, "Files", "Id");
                    table.ForeignKey("FK_ReportExports_Reports_ReportId", x => x.ReportId, "Reports", "Id", onDelete: ReferentialAction.Cascade);
                });

            CreateIndexes(migrationBuilder);
            SeedData(migrationBuilder);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable("ClientDocuments");
            migrationBuilder.DropTable("ReportExports");
            migrationBuilder.DropTable("UserActivities");
            migrationBuilder.DropTable("Reports");
            migrationBuilder.DropTable("ModelMetrics");
            migrationBuilder.DropTable("PredictionFactors");
            migrationBuilder.DropTable("LoanApplications");
            migrationBuilder.DropTable("CreditScores");
            migrationBuilder.DropTable("DebtObligations");
            migrationBuilder.DropTable("IncomeSources");
            migrationBuilder.DropTable("EmploymentInfos");
            migrationBuilder.DropTable("Files");
            migrationBuilder.DropTable("Notifications");
            migrationBuilder.DropTable("RefreshTokens");
            migrationBuilder.DropTable("RolePermissions");
            migrationBuilder.DropTable("UserRoles");
            migrationBuilder.DropTable("ModelRuns");
            migrationBuilder.DropTable("RiskFactors");
            migrationBuilder.DropTable("LoanTypes");
            migrationBuilder.DropTable("LoanApplicationStatuses");
            migrationBuilder.DropTable("Settings");
            migrationBuilder.DropTable("Permissions");
            migrationBuilder.DropTable("Roles");

            migrationBuilder.DropColumn("UserId", "AuditLogs");
            migrationBuilder.DropColumn("EntityName", "AuditLogs");
            migrationBuilder.DropColumn("EntityId", "AuditLogs");
            migrationBuilder.DropColumn("OldValues", "AuditLogs");
            migrationBuilder.DropColumn("NewValues", "AuditLogs");
            migrationBuilder.DropColumn("IpAddress", "AuditLogs");
            migrationBuilder.DropColumn("CreatedAt", "AuditLogs");
        }

        private static void CreateIndexes(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex("IX_AuditLogs_UserId", "AuditLogs", "UserId");
            migrationBuilder.CreateIndex("IX_AuditLogs_CreatedAt", "AuditLogs", "CreatedAt");
            migrationBuilder.CreateIndex("IX_AuditLogs_EntityName", "AuditLogs", "EntityName");
            migrationBuilder.CreateIndex("IX_Users_CreatedAt", "Users", "CreatedAt");
            migrationBuilder.CreateIndex("IX_Predictions_CreatedAt", "Predictions", "CreatedAt");
            migrationBuilder.CreateIndex("IX_ClientProfiles_CreatedAt", "ClientProfiles", "CreatedAt");
            migrationBuilder.CreateIndex("IX_Roles_Name", "Roles", "Name", unique: true);
            migrationBuilder.CreateIndex("IX_Permissions_Name", "Permissions", "Name", unique: true);
            migrationBuilder.CreateIndex("IX_Settings_Key", "Settings", "Key", unique: true);
            migrationBuilder.CreateIndex("IX_LoanApplicationStatuses_Name", "LoanApplicationStatuses", "Name", unique: true);
            migrationBuilder.CreateIndex("IX_LoanTypes_Name", "LoanTypes", "Name", unique: true);
            migrationBuilder.CreateIndex("IX_RiskFactors_Code", "RiskFactors", "Code", unique: true);
            migrationBuilder.CreateIndex("IX_UserRoles_RoleId", "UserRoles", "RoleId");
            migrationBuilder.CreateIndex("IX_RolePermissions_PermissionId", "RolePermissions", "PermissionId");
            migrationBuilder.CreateIndex("IX_RefreshTokens_TokenHash", "RefreshTokens", "TokenHash", unique: true);
            migrationBuilder.CreateIndex("IX_RefreshTokens_UserId", "RefreshTokens", "UserId");
            migrationBuilder.CreateIndex("IX_RefreshTokens_ExpiresAt", "RefreshTokens", "ExpiresAt");
            migrationBuilder.CreateIndex("IX_Notifications_UserId", "Notifications", "UserId");
            migrationBuilder.CreateIndex("IX_Notifications_UserId_IsRead", "Notifications", new[] { "UserId", "IsRead" });
            migrationBuilder.CreateIndex("IX_Notifications_CreatedAt", "Notifications", "CreatedAt");
            migrationBuilder.CreateIndex("IX_Files_UserId", "Files", "UserId");
            migrationBuilder.CreateIndex("IX_Files_ClientProfileId", "Files", "ClientProfileId");
            migrationBuilder.CreateIndex("IX_Files_CreatedAt", "Files", "CreatedAt");
            migrationBuilder.CreateIndex("IX_EmploymentInfos_ClientProfileId", "EmploymentInfos", "ClientProfileId");
            migrationBuilder.CreateIndex("IX_IncomeSources_ClientProfileId", "IncomeSources", "ClientProfileId");
            migrationBuilder.CreateIndex("IX_DebtObligations_ClientProfileId", "DebtObligations", "ClientProfileId");
            migrationBuilder.CreateIndex("IX_CreditScores_ClientProfileId_ScoreDate", "CreditScores", new[] { "ClientProfileId", "ScoreDate" });
            migrationBuilder.CreateIndex("IX_LoanApplications_ClientProfileId", "LoanApplications", "ClientProfileId");
            migrationBuilder.CreateIndex("IX_LoanApplications_StatusId", "LoanApplications", "StatusId");
            migrationBuilder.CreateIndex("IX_LoanApplications_LoanTypeId", "LoanApplications", "LoanTypeId");
            migrationBuilder.CreateIndex("IX_LoanApplications_CreatedAt", "LoanApplications", "CreatedAt");
            migrationBuilder.CreateIndex("IX_PredictionFactors_PredictionId", "PredictionFactors", "PredictionId");
            migrationBuilder.CreateIndex("IX_PredictionFactors_RiskFactorId", "PredictionFactors", "RiskFactorId");
            migrationBuilder.CreateIndex("IX_ModelMetrics_ModelRunId", "ModelMetrics", "ModelRunId");
            migrationBuilder.CreateIndex("IX_Reports_CreatedByUserId", "Reports", "CreatedByUserId");
            migrationBuilder.CreateIndex("IX_ReportExports_ReportId", "ReportExports", "ReportId");
            migrationBuilder.CreateIndex("IX_ReportExports_FileRecordId", "ReportExports", "FileRecordId");
            migrationBuilder.CreateIndex("IX_UserActivities_UserId", "UserActivities", "UserId");
            migrationBuilder.CreateIndex("IX_UserActivities_CreatedAt", "UserActivities", "CreatedAt");
            migrationBuilder.CreateIndex("IX_ClientDocuments_ClientProfileId", "ClientDocuments", "ClientProfileId");
            migrationBuilder.CreateIndex("IX_ClientDocuments_FileRecordId", "ClientDocuments", "FileRecordId");
        }

        private static void SeedData(MigrationBuilder migrationBuilder)
        {
            var seedTime = new DateTime(2026, 06, 27, 0, 0, 0, DateTimeKind.Utc);
            var adminRoleId = Guid.Parse("11111111-1111-1111-1111-111111111111");
            var userRoleId = Guid.Parse("22222222-2222-2222-2222-222222222222");
            var managerRoleId = Guid.Parse("33333333-3333-3333-3333-333333333333");
            var permissionNames = new[] { "users.read", "users.manage", "predictions.create", "predictions.read", "predictions.manage", "reports.read", "reports.generate", "settings.manage", "notifications.read", "files.manage" };
            var permissionIds = permissionNames.Select((_, index) => Guid.Parse($"44444444-4444-4444-4444-{(index + 1).ToString().PadLeft(12, '0')}")).ToArray();

            migrationBuilder.InsertData("Roles", new[] { "Id", "Name", "Description", "CreatedAt", "UpdatedAt" }, new object[,]
            {
                { adminRoleId, "Admin", "Full system administration", seedTime, seedTime },
                { userRoleId, "User", "Standard credit-risk applicant", seedTime, seedTime },
                { managerRoleId, "Manager", "Credit-risk operations manager", seedTime, seedTime }
            });

            for (var i = 0; i < permissionNames.Length; i++)
            {
                migrationBuilder.InsertData("Permissions", new[] { "Id", "Name", "Description", "CreatedAt", "UpdatedAt" }, new object[] { permissionIds[i], permissionNames[i], permissionNames[i], seedTime, seedTime });
                migrationBuilder.InsertData("RolePermissions", new[] { "RoleId", "PermissionId", "CreatedAt", "CreatedBy" }, new object[] { adminRoleId, permissionIds[i], seedTime, null });
            }

            AddRolePermission(migrationBuilder, userRoleId, permissionIds[2], seedTime);
            AddRolePermission(migrationBuilder, userRoleId, permissionIds[3], seedTime);
            AddRolePermission(migrationBuilder, userRoleId, permissionIds[8], seedTime);

            foreach (var index in new[] { 0, 3, 4, 5, 6, 8, 9 })
            {
                AddRolePermission(migrationBuilder, managerRoleId, permissionIds[index], seedTime);
            }

            migrationBuilder.InsertData("LoanApplicationStatuses", new[] { "Id", "Name", "Description", "CreatedAt", "UpdatedAt" }, new object[,]
            {
                { Guid.Parse("55555555-5555-5555-5555-000000000001"), "Draft", "Application is being prepared", seedTime, seedTime },
                { Guid.Parse("55555555-5555-5555-5555-000000000002"), "Submitted", "Application submitted for review", seedTime, seedTime },
                { Guid.Parse("55555555-5555-5555-5555-000000000003"), "Approved", "Application approved", seedTime, seedTime },
                { Guid.Parse("55555555-5555-5555-5555-000000000004"), "Rejected", "Application rejected", seedTime, seedTime }
            });

            migrationBuilder.InsertData("LoanTypes", new[] { "Id", "Name", "Description", "CreatedAt", "UpdatedAt" }, new object[,]
            {
                { Guid.Parse("66666666-6666-6666-6666-000000000001"), "Personal", "Personal loan", seedTime, seedTime },
                { Guid.Parse("66666666-6666-6666-6666-000000000002"), "Auto", "Vehicle financing", seedTime, seedTime },
                { Guid.Parse("66666666-6666-6666-6666-000000000003"), "Mortgage", "Mortgage loan", seedTime, seedTime }
            });
        }

        private static void AddRolePermission(MigrationBuilder migrationBuilder, Guid roleId, Guid permissionId, DateTime seedTime)
        {
            migrationBuilder.InsertData("RolePermissions", new[] { "RoleId", "PermissionId", "CreatedAt", "CreatedBy" }, new object[] { roleId, permissionId, seedTime, null });
        }
    }
}
