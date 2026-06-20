using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Softcoinp.ERP.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddFeesAndPortfolioModule : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Type",
                table: "erp_accounting_accounts");

            migrationBuilder.AddColumn<string>(
                name: "Category",
                table: "erp_accounting_accounts",
                type: "varchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "erp_accounting_accounts",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsGroup",
                table: "erp_accounting_accounts",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsOfficialStandard",
                table: "erp_accounting_accounts",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "Nature",
                table: "erp_accounting_accounts",
                type: "varchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "TenantId",
                table: "erp_accounting_accounts",
                type: "varchar(255)",
                maxLength: 255,
                nullable: false,
                defaultValue: "")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "erp_accounting_entries",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    TenantId = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    AccountingAccountId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    Debit = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Credit = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    EntryDate = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    Description = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Reference = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    IsDeleted = table.Column<bool>(type: "tinyint(1)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_erp_accounting_entries", x => x.Id);
                    table.ForeignKey(
                        name: "FK_erp_accounting_entries_erp_accounting_accounts_AccountingAcc~",
                        column: x => x.AccountingAccountId,
                        principalTable: "erp_accounting_accounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "erp_billing_periods",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    TenantId = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Period = table.Column<string>(type: "varchar(7)", maxLength: 7, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    MonthlyBudgetTotal = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    CutoffDate = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    PaymentDueDate = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    Status = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ExecutedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    ExecutedByUserId = table.Column<string>(type: "varchar(450)", maxLength: 450, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    RoundingAdjustment = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Notes = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    CreatedByUserId = table.Column<string>(type: "varchar(450)", maxLength: 450, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    UpdatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_erp_billing_periods", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "erp_budgets",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    TenantId = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    FiscalPeriod = table.Column<int>(type: "int", nullable: false),
                    ApprovalDate = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    MeetingActNumber = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Status = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CreatedByUserId = table.Column<string>(type: "varchar(450)", maxLength: 450, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    IsDeleted = table.Column<bool>(type: "tinyint(1)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_erp_budgets", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "erp_clearance_certificates",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    TenantId = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    UnitId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    CertificateNumber = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    IssueDate = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    ExpirationDate = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    BalanceAtDate = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Status = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    IssuedByUserId = table.Column<string>(type: "varchar(450)", maxLength: 450, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    SignedByAdministratorName = table.Column<string>(type: "varchar(300)", maxLength: 300, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_erp_clearance_certificates", x => x.Id);
                    table.ForeignKey(
                        name: "FK_erp_clearance_certificates_erp_units_UnitId",
                        column: x => x.UnitId,
                        principalTable: "erp_units",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "erp_contingency_fund_contributions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    TenantId = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Period = table.Column<string>(type: "varchar(7)", maxLength: 7, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    IncomeBase = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Percentage = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: false),
                    ContributionDate = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    AccountingRecordId = table.Column<Guid>(type: "char(36)", nullable: true, collation: "ascii_general_ci"),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    IsDeleted = table.Column<bool>(type: "tinyint(1)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_erp_contingency_fund_contributions", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "erp_contingency_fund_usages",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    TenantId = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Justification = table.Column<string>(type: "varchar(1000)", maxLength: 1000, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CouncilApprovalActNumber = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ApprovalDate = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    AccountingRecordId = table.Column<Guid>(type: "char(36)", nullable: true, collation: "ascii_general_ci"),
                    CreatedByUserId = table.Column<string>(type: "varchar(450)", maxLength: 450, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    IsDeleted = table.Column<bool>(type: "tinyint(1)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_erp_contingency_fund_usages", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "erp_contingency_funds",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    TenantId = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CurrentBalance = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    IsDeleted = table.Column<bool>(type: "tinyint(1)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_erp_contingency_funds", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "erp_extraordinary_fees",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    TenantId = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Name = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Description = table.Column<string>(type: "varchar(1000)", maxLength: 1000, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    MeetingActNumber = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ApprovalDate = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    TotalAmount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    NumberOfInstallments = table.Column<int>(type: "int", nullable: false),
                    StartPeriod = table.Column<string>(type: "varchar(7)", maxLength: 7, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    DistributionType = table.Column<string>(type: "varchar(30)", maxLength: 30, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Status = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    CreatedByUserId = table.Column<string>(type: "varchar(450)", maxLength: 450, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    UpdatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_erp_extraordinary_fees", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "erp_individual_charges",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    TenantId = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    UnitId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    ChargeType = table.Column<string>(type: "varchar(30)", maxLength: 30, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Concept = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    ChargeDate = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    Description = table.Column<string>(type: "varchar(1000)", maxLength: 1000, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ReferenceActNumber = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    IsDisputed = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    DisputeReason = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Status = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    PaidAmount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    BalanceAmount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    CreatedByUserId = table.Column<string>(type: "varchar(450)", maxLength: 450, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    UpdatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_erp_individual_charges", x => x.Id);
                    table.ForeignKey(
                        name: "FK_erp_individual_charges_erp_units_UnitId",
                        column: x => x.UnitId,
                        principalTable: "erp_units",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "erp_payment_agreements",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    TenantId = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    UnitId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    TotalDebtIncluded = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    InstallmentAmount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    NumberOfInstallments = table.Column<int>(type: "int", nullable: false),
                    InterestForgivenessPercentage = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: false),
                    CouncilActNumber = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Status = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    StartedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    DefaultedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    DigitalAcceptance = table.Column<string>(type: "varchar(2000)", maxLength: 2000, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    CreatedByUserId = table.Column<string>(type: "varchar(450)", maxLength: 450, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    UpdatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_erp_payment_agreements", x => x.Id);
                    table.ForeignKey(
                        name: "FK_erp_payment_agreements_erp_units_UnitId",
                        column: x => x.UnitId,
                        principalTable: "erp_units",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "erp_payments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    TenantId = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    UnitId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    PaymentDate = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    PaymentMethod = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ReferenceNumber = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Notes = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ReceivedByUserId = table.Column<string>(type: "varchar(450)", maxLength: 450, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    AdvanceAmount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_erp_payments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_erp_payments_erp_units_UnitId",
                        column: x => x.UnitId,
                        principalTable: "erp_units",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "erp_unit_fees",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    TenantId = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    BillingPeriodId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    UnitId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    FeeValue = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    DueDate = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    Status = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    PaidAmount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    BalanceAmount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_erp_unit_fees", x => x.Id);
                    table.ForeignKey(
                        name: "FK_erp_unit_fees_erp_billing_periods_BillingPeriodId",
                        column: x => x.BillingPeriodId,
                        principalTable: "erp_billing_periods",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_erp_unit_fees_erp_units_UnitId",
                        column: x => x.UnitId,
                        principalTable: "erp_units",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "erp_budget_details",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    BudgetId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    AccountingAccountId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    ApprovedValue = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Observations = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    IsDeleted = table.Column<bool>(type: "tinyint(1)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_erp_budget_details", x => x.Id);
                    table.ForeignKey(
                        name: "FK_erp_budget_details_erp_accounting_accounts_AccountingAccount~",
                        column: x => x.AccountingAccountId,
                        principalTable: "erp_accounting_accounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_erp_budget_details_erp_budgets_BudgetId",
                        column: x => x.BudgetId,
                        principalTable: "erp_budgets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "erp_budget_movements",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    TenantId = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    BudgetId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    MovementType = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    SourceAccountId = table.Column<Guid>(type: "char(36)", nullable: true, collation: "ascii_general_ci"),
                    DestinationAccountId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Justification = table.Column<string>(type: "varchar(1000)", maxLength: 1000, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ApprovalType = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    MeetingActNumber = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ApprovalDate = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    CreatedByUserId = table.Column<string>(type: "varchar(450)", maxLength: 450, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    IsDeleted = table.Column<bool>(type: "tinyint(1)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_erp_budget_movements", x => x.Id);
                    table.ForeignKey(
                        name: "FK_erp_budget_movements_erp_accounting_accounts_DestinationAcco~",
                        column: x => x.DestinationAccountId,
                        principalTable: "erp_accounting_accounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_erp_budget_movements_erp_accounting_accounts_SourceAccountId",
                        column: x => x.SourceAccountId,
                        principalTable: "erp_accounting_accounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_erp_budget_movements_erp_budgets_BudgetId",
                        column: x => x.BudgetId,
                        principalTable: "erp_budgets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "erp_extraordinary_fee_distributions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    TenantId = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ExtraordinaryFeeId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    UnitId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    InstallmentNumber = table.Column<int>(type: "int", nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    DueDate = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    Status = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    PaidAmount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    BalanceAmount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_erp_extraordinary_fee_distributions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_erp_extraordinary_fee_distributions_erp_extraordinary_fees_E~",
                        column: x => x.ExtraordinaryFeeId,
                        principalTable: "erp_extraordinary_fees",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_erp_extraordinary_fee_distributions_erp_units_UnitId",
                        column: x => x.UnitId,
                        principalTable: "erp_units",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "erp_agreement_installments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    TenantId = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    PaymentAgreementId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    InstallmentNumber = table.Column<int>(type: "int", nullable: false),
                    DueDate = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    PaidAmount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Status = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    PaidAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_erp_agreement_installments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_erp_agreement_installments_erp_payment_agreements_PaymentAgr~",
                        column: x => x.PaymentAgreementId,
                        principalTable: "erp_payment_agreements",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "erp_late_interests",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    TenantId = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    UnitFeeId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    ExtraordinaryFeeDistributionId = table.Column<Guid>(type: "char(36)", nullable: true, collation: "ascii_general_ci"),
                    IndividualChargeId = table.Column<Guid>(type: "char(36)", nullable: true, collation: "ascii_general_ci"),
                    Period = table.Column<string>(type: "varchar(7)", maxLength: 7, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    BaseAmount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    DailyRate = table.Column<decimal>(type: "decimal(12,8)", precision: 12, scale: 8, nullable: false),
                    DaysOverdue = table.Column<int>(type: "int", nullable: false),
                    CalculatedAmount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    IsCapitalized = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_erp_late_interests", x => x.Id);
                    table.ForeignKey(
                        name: "FK_erp_late_interests_erp_extraordinary_fee_distributions_Extra~",
                        column: x => x.ExtraordinaryFeeDistributionId,
                        principalTable: "erp_extraordinary_fee_distributions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_erp_late_interests_erp_individual_charges_IndividualChargeId",
                        column: x => x.IndividualChargeId,
                        principalTable: "erp_individual_charges",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_erp_late_interests_erp_unit_fees_UnitFeeId",
                        column: x => x.UnitFeeId,
                        principalTable: "erp_unit_fees",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "erp_payment_allocations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    TenantId = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    PaymentId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    UnitFeeId = table.Column<Guid>(type: "char(36)", nullable: true, collation: "ascii_general_ci"),
                    ExtraordinaryFeeDistributionId = table.Column<Guid>(type: "char(36)", nullable: true, collation: "ascii_general_ci"),
                    IndividualChargeId = table.Column<Guid>(type: "char(36)", nullable: true, collation: "ascii_general_ci"),
                    LateInterestId = table.Column<Guid>(type: "char(36)", nullable: true, collation: "ascii_general_ci"),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    AllocationType = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_erp_payment_allocations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_erp_payment_allocations_erp_extraordinary_fee_distributions_~",
                        column: x => x.ExtraordinaryFeeDistributionId,
                        principalTable: "erp_extraordinary_fee_distributions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_erp_payment_allocations_erp_individual_charges_IndividualCha~",
                        column: x => x.IndividualChargeId,
                        principalTable: "erp_individual_charges",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_erp_payment_allocations_erp_late_interests_LateInterestId",
                        column: x => x.LateInterestId,
                        principalTable: "erp_late_interests",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_erp_payment_allocations_erp_payments_PaymentId",
                        column: x => x.PaymentId,
                        principalTable: "erp_payments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_erp_payment_allocations_erp_unit_fees_UnitFeeId",
                        column: x => x.UnitFeeId,
                        principalTable: "erp_unit_fees",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_erp_accounting_entries_AccountingAccountId",
                table: "erp_accounting_entries",
                column: "AccountingAccountId");

            migrationBuilder.CreateIndex(
                name: "IX_erp_accounting_entries_TenantId_EntryDate",
                table: "erp_accounting_entries",
                columns: new[] { "TenantId", "EntryDate" });

            migrationBuilder.CreateIndex(
                name: "IX_erp_agreement_installments_PaymentAgreementId_InstallmentNum~",
                table: "erp_agreement_installments",
                columns: new[] { "PaymentAgreementId", "InstallmentNumber" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_erp_agreement_installments_TenantId_Status_DueDate",
                table: "erp_agreement_installments",
                columns: new[] { "TenantId", "Status", "DueDate" });

            migrationBuilder.CreateIndex(
                name: "IX_erp_billing_periods_TenantId_Period",
                table: "erp_billing_periods",
                columns: new[] { "TenantId", "Period" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_erp_budget_details_AccountingAccountId",
                table: "erp_budget_details",
                column: "AccountingAccountId");

            migrationBuilder.CreateIndex(
                name: "IX_erp_budget_details_BudgetId_AccountingAccountId",
                table: "erp_budget_details",
                columns: new[] { "BudgetId", "AccountingAccountId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_erp_budget_movements_BudgetId",
                table: "erp_budget_movements",
                column: "BudgetId");

            migrationBuilder.CreateIndex(
                name: "IX_erp_budget_movements_DestinationAccountId",
                table: "erp_budget_movements",
                column: "DestinationAccountId");

            migrationBuilder.CreateIndex(
                name: "IX_erp_budget_movements_SourceAccountId",
                table: "erp_budget_movements",
                column: "SourceAccountId");

            migrationBuilder.CreateIndex(
                name: "IX_erp_budgets_TenantId_FiscalPeriod",
                table: "erp_budgets",
                columns: new[] { "TenantId", "FiscalPeriod" });

            migrationBuilder.CreateIndex(
                name: "IX_erp_clearance_certificates_TenantId_CertificateNumber",
                table: "erp_clearance_certificates",
                columns: new[] { "TenantId", "CertificateNumber" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_erp_clearance_certificates_TenantId_UnitId_Status",
                table: "erp_clearance_certificates",
                columns: new[] { "TenantId", "UnitId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_erp_clearance_certificates_UnitId",
                table: "erp_clearance_certificates",
                column: "UnitId");

            migrationBuilder.CreateIndex(
                name: "IX_erp_contingency_fund_contributions_TenantId_Period",
                table: "erp_contingency_fund_contributions",
                columns: new[] { "TenantId", "Period" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_erp_contingency_funds_TenantId",
                table: "erp_contingency_funds",
                column: "TenantId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_erp_extraordinary_fee_distributions_ExtraordinaryFeeId_UnitI~",
                table: "erp_extraordinary_fee_distributions",
                columns: new[] { "ExtraordinaryFeeId", "UnitId", "InstallmentNumber" });

            migrationBuilder.CreateIndex(
                name: "IX_erp_extraordinary_fee_distributions_TenantId_UnitId_Status",
                table: "erp_extraordinary_fee_distributions",
                columns: new[] { "TenantId", "UnitId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_erp_extraordinary_fee_distributions_UnitId",
                table: "erp_extraordinary_fee_distributions",
                column: "UnitId");

            migrationBuilder.CreateIndex(
                name: "IX_erp_extraordinary_fees_TenantId_Status",
                table: "erp_extraordinary_fees",
                columns: new[] { "TenantId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_erp_individual_charges_TenantId_Status_IsDisputed",
                table: "erp_individual_charges",
                columns: new[] { "TenantId", "Status", "IsDisputed" });

            migrationBuilder.CreateIndex(
                name: "IX_erp_individual_charges_TenantId_UnitId_Status",
                table: "erp_individual_charges",
                columns: new[] { "TenantId", "UnitId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_erp_individual_charges_UnitId",
                table: "erp_individual_charges",
                column: "UnitId");

            migrationBuilder.CreateIndex(
                name: "IX_erp_late_interests_ExtraordinaryFeeDistributionId",
                table: "erp_late_interests",
                column: "ExtraordinaryFeeDistributionId");

            migrationBuilder.CreateIndex(
                name: "IX_erp_late_interests_IndividualChargeId",
                table: "erp_late_interests",
                column: "IndividualChargeId");

            migrationBuilder.CreateIndex(
                name: "IX_erp_late_interests_TenantId_IsCapitalized",
                table: "erp_late_interests",
                columns: new[] { "TenantId", "IsCapitalized" });

            migrationBuilder.CreateIndex(
                name: "IX_erp_late_interests_TenantId_UnitFeeId_Period",
                table: "erp_late_interests",
                columns: new[] { "TenantId", "UnitFeeId", "Period" });

            migrationBuilder.CreateIndex(
                name: "IX_erp_late_interests_UnitFeeId",
                table: "erp_late_interests",
                column: "UnitFeeId");

            migrationBuilder.CreateIndex(
                name: "IX_erp_payment_agreements_TenantId_UnitId_Status",
                table: "erp_payment_agreements",
                columns: new[] { "TenantId", "UnitId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_erp_payment_agreements_UnitId",
                table: "erp_payment_agreements",
                column: "UnitId");

            migrationBuilder.CreateIndex(
                name: "IX_erp_payment_allocations_ExtraordinaryFeeDistributionId",
                table: "erp_payment_allocations",
                column: "ExtraordinaryFeeDistributionId");

            migrationBuilder.CreateIndex(
                name: "IX_erp_payment_allocations_IndividualChargeId",
                table: "erp_payment_allocations",
                column: "IndividualChargeId");

            migrationBuilder.CreateIndex(
                name: "IX_erp_payment_allocations_LateInterestId",
                table: "erp_payment_allocations",
                column: "LateInterestId");

            migrationBuilder.CreateIndex(
                name: "IX_erp_payment_allocations_PaymentId_AllocationType",
                table: "erp_payment_allocations",
                columns: new[] { "PaymentId", "AllocationType" });

            migrationBuilder.CreateIndex(
                name: "IX_erp_payment_allocations_UnitFeeId_AllocationType",
                table: "erp_payment_allocations",
                columns: new[] { "UnitFeeId", "AllocationType" });

            migrationBuilder.CreateIndex(
                name: "IX_erp_payments_TenantId_UnitId_PaymentDate",
                table: "erp_payments",
                columns: new[] { "TenantId", "UnitId", "PaymentDate" });

            migrationBuilder.CreateIndex(
                name: "IX_erp_payments_UnitId",
                table: "erp_payments",
                column: "UnitId");

            migrationBuilder.CreateIndex(
                name: "IX_erp_unit_fees_BillingPeriodId",
                table: "erp_unit_fees",
                column: "BillingPeriodId");

            migrationBuilder.CreateIndex(
                name: "IX_erp_unit_fees_TenantId_BillingPeriodId_UnitId",
                table: "erp_unit_fees",
                columns: new[] { "TenantId", "BillingPeriodId", "UnitId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_erp_unit_fees_TenantId_UnitId_Status",
                table: "erp_unit_fees",
                columns: new[] { "TenantId", "UnitId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_erp_unit_fees_UnitId",
                table: "erp_unit_fees",
                column: "UnitId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "erp_accounting_entries");

            migrationBuilder.DropTable(
                name: "erp_agreement_installments");

            migrationBuilder.DropTable(
                name: "erp_budget_details");

            migrationBuilder.DropTable(
                name: "erp_budget_movements");

            migrationBuilder.DropTable(
                name: "erp_clearance_certificates");

            migrationBuilder.DropTable(
                name: "erp_contingency_fund_contributions");

            migrationBuilder.DropTable(
                name: "erp_contingency_fund_usages");

            migrationBuilder.DropTable(
                name: "erp_contingency_funds");

            migrationBuilder.DropTable(
                name: "erp_payment_allocations");

            migrationBuilder.DropTable(
                name: "erp_payment_agreements");

            migrationBuilder.DropTable(
                name: "erp_budgets");

            migrationBuilder.DropTable(
                name: "erp_late_interests");

            migrationBuilder.DropTable(
                name: "erp_payments");

            migrationBuilder.DropTable(
                name: "erp_extraordinary_fee_distributions");

            migrationBuilder.DropTable(
                name: "erp_individual_charges");

            migrationBuilder.DropTable(
                name: "erp_unit_fees");

            migrationBuilder.DropTable(
                name: "erp_extraordinary_fees");

            migrationBuilder.DropTable(
                name: "erp_billing_periods");

            migrationBuilder.DropColumn(
                name: "Category",
                table: "erp_accounting_accounts");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "erp_accounting_accounts");

            migrationBuilder.DropColumn(
                name: "IsGroup",
                table: "erp_accounting_accounts");

            migrationBuilder.DropColumn(
                name: "IsOfficialStandard",
                table: "erp_accounting_accounts");

            migrationBuilder.DropColumn(
                name: "Nature",
                table: "erp_accounting_accounts");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "erp_accounting_accounts");

            migrationBuilder.AddColumn<int>(
                name: "Type",
                table: "erp_accounting_accounts",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }
    }
}
