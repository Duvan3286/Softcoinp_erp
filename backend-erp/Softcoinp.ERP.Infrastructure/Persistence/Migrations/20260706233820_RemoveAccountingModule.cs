using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Softcoinp.ERP.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class RemoveAccountingModule : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_erp_bank_accounts_erp_accounting_accounts_AccountingAccountId",
                table: "erp_bank_accounts");

            migrationBuilder.DropForeignKey(
                name: "FK_erp_bank_movements_erp_accounting_entries_AccountingEntryId",
                table: "erp_bank_movements");

            migrationBuilder.DropForeignKey(
                name: "FK_erp_contracts_erp_accounting_accounts_BudgetAccountId",
                table: "erp_contracts");

            migrationBuilder.DropForeignKey(
                name: "FK_erp_fixed_assets_erp_accounting_accounts_AccountingAccountId",
                table: "erp_fixed_assets");

            migrationBuilder.DropForeignKey(
                name: "FK_erp_monthly_depreciations_erp_accounting_entries_AccountingE~",
                table: "erp_monthly_depreciations");

            migrationBuilder.DropForeignKey(
                name: "FK_erp_provider_invoices_erp_accounting_entries_AccountingEntry~",
                table: "erp_provider_invoices");

            migrationBuilder.DropForeignKey(
                name: "FK_erp_provider_payments_erp_accounting_entries_AccountingEntry~",
                table: "erp_provider_payments");

            migrationBuilder.DropForeignKey(
                name: "FK_erp_work_orders_erp_accounting_accounts_BudgetAccountId",
                table: "erp_work_orders");

            migrationBuilder.DropForeignKey(
                name: "FK_erp_work_orders_erp_accounting_entries_AccountingEntryId",
                table: "erp_work_orders");

            migrationBuilder.DropForeignKey(
                name: "FK_erp_accounting_entries_erp_accounting_periods_AccountingPeri~",
                table: "erp_accounting_entries");

            migrationBuilder.DropForeignKey(
                name: "FK_erp_accounting_entries_erp_entry_reversals_ReversalId",
                table: "erp_accounting_entries");

            migrationBuilder.DropTable(
                name: "erp_budget_details");

            migrationBuilder.DropTable(
                name: "erp_budget_movements");

            migrationBuilder.DropTable(
                name: "erp_contingency_fund_contributions");

            migrationBuilder.DropTable(
                name: "erp_contingency_funds");

            migrationBuilder.DropTable(
                name: "erp_entry_lines");

            migrationBuilder.DropTable(
                name: "erp_accounting_accounts");

            migrationBuilder.DropTable(
                name: "erp_accounting_periods");

            migrationBuilder.DropTable(
                name: "erp_entry_reversals");

            migrationBuilder.DropTable(
                name: "erp_accounting_entries");

            migrationBuilder.DropIndex(
                name: "IX_erp_work_orders_AccountingEntryId",
                table: "erp_work_orders");

            migrationBuilder.DropIndex(
                name: "IX_erp_work_orders_BudgetAccountId",
                table: "erp_work_orders");

            migrationBuilder.DropIndex(
                name: "IX_erp_provider_payments_AccountingEntryId",
                table: "erp_provider_payments");

            migrationBuilder.DropIndex(
                name: "IX_erp_provider_invoices_AccountingEntryId",
                table: "erp_provider_invoices");

            migrationBuilder.DropIndex(
                name: "IX_erp_monthly_depreciations_AccountingEntryId",
                table: "erp_monthly_depreciations");

            migrationBuilder.DropIndex(
                name: "IX_erp_fixed_assets_AccountingAccountId",
                table: "erp_fixed_assets");

            migrationBuilder.DropIndex(
                name: "IX_erp_contracts_BudgetAccountId",
                table: "erp_contracts");

            migrationBuilder.DropIndex(
                name: "IX_erp_budgets_TenantId_FiscalPeriod",
                table: "erp_budgets");

            migrationBuilder.DropIndex(
                name: "IX_erp_bank_movements_AccountingEntryId",
                table: "erp_bank_movements");

            migrationBuilder.DropIndex(
                name: "IX_erp_bank_accounts_AccountingAccountId",
                table: "erp_bank_accounts");

            migrationBuilder.DropColumn(
                name: "AccountingEntryId",
                table: "erp_work_orders");

            migrationBuilder.DropColumn(
                name: "BudgetAccountId",
                table: "erp_work_orders");

            migrationBuilder.DropColumn(
                name: "AccountingEntryId",
                table: "erp_provider_payments");

            migrationBuilder.DropColumn(
                name: "AccountingEntryId",
                table: "erp_provider_invoices");

            migrationBuilder.DropColumn(
                name: "AccountingEntryId",
                table: "erp_monthly_depreciations");

            migrationBuilder.DropColumn(
                name: "AccountingAccountId",
                table: "erp_fixed_assets");

            migrationBuilder.DropColumn(
                name: "BudgetAccountId",
                table: "erp_contracts");

            migrationBuilder.DropColumn(
                name: "ApprovalDate",
                table: "erp_contingency_fund_usages");

            migrationBuilder.DropColumn(
                name: "AccountingEntryId",
                table: "erp_bank_movements");

            migrationBuilder.DropColumn(
                name: "AccountingAccountId",
                table: "erp_bank_accounts");

            migrationBuilder.RenameColumn(
                name: "AccountingRecordId",
                table: "erp_contingency_fund_usages",
                newName: "ExecutedExpenseId");

            migrationBuilder.RenameColumn(
                name: "FiscalPeriod",
                table: "erp_budgets",
                newName: "FiscalYear");

            migrationBuilder.AddColumn<Guid>(
                name: "BudgetId",
                table: "erp_contingency_fund_usages",
                type: "char(36)",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                collation: "ascii_general_ci");

            migrationBuilder.AddColumn<string>(
                name: "Observations",
                table: "erp_budgets",
                type: "varchar(1000)",
                maxLength: 1000,
                nullable: false,
                defaultValue: "")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "erp_expense_items",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    BudgetId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    Name = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Description = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Category = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    AnnualValue = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    IsContingencyFund = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    ContingencyPercentage = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: false),
                    RequiresCouncilApproval = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    ApprovalThreshold = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    IsDeleted = table.Column<bool>(type: "tinyint(1)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_erp_expense_items", x => x.Id);
                    table.ForeignKey(
                        name: "FK_erp_expense_items_erp_budgets_BudgetId",
                        column: x => x.BudgetId,
                        principalTable: "erp_budgets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "erp_income_items",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    BudgetId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    Name = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Description = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    AnnualValue = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    IsDeleted = table.Column<bool>(type: "tinyint(1)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_erp_income_items", x => x.Id);
                    table.ForeignKey(
                        name: "FK_erp_income_items_erp_budgets_BudgetId",
                        column: x => x.BudgetId,
                        principalTable: "erp_budgets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "erp_executed_expenses",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    TenantId = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ExpenseItemId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    Description = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    ExpenseDate = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    ProviderId = table.Column<Guid>(type: "char(36)", nullable: true, collation: "ascii_general_ci"),
                    InvoiceReference = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CouncilApproved = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    CreatedByUserId = table.Column<string>(type: "varchar(450)", maxLength: 450, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    IsDeleted = table.Column<bool>(type: "tinyint(1)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_erp_executed_expenses", x => x.Id);
                    table.ForeignKey(
                        name: "FK_erp_executed_expenses_erp_expense_items_ExpenseItemId",
                        column: x => x.ExpenseItemId,
                        principalTable: "erp_expense_items",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_erp_executed_expenses_erp_providers_ProviderId",
                        column: x => x.ProviderId,
                        principalTable: "erp_providers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "erp_budget_modifications",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    TenantId = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    BudgetId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    ExpenseItemId = table.Column<Guid>(type: "char(36)", nullable: true, collation: "ascii_general_ci"),
                    IncomeItemId = table.Column<Guid>(type: "char(36)", nullable: true, collation: "ascii_general_ci"),
                    ModificationType = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    PreviousValue = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    NewValue = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
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
                    table.PrimaryKey("PK_erp_budget_modifications", x => x.Id);
                    table.ForeignKey(
                        name: "FK_erp_budget_modifications_erp_budgets_BudgetId",
                        column: x => x.BudgetId,
                        principalTable: "erp_budgets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_erp_budget_modifications_erp_expense_items_ExpenseItemId",
                        column: x => x.ExpenseItemId,
                        principalTable: "erp_expense_items",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_erp_budget_modifications_erp_income_items_IncomeItemId",
                        column: x => x.IncomeItemId,
                        principalTable: "erp_income_items",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_erp_contingency_fund_usages_BudgetId",
                table: "erp_contingency_fund_usages",
                column: "BudgetId");

            migrationBuilder.CreateIndex(
                name: "IX_erp_contingency_fund_usages_ExecutedExpenseId",
                table: "erp_contingency_fund_usages",
                column: "ExecutedExpenseId");

            migrationBuilder.CreateIndex(
                name: "IX_erp_budgets_TenantId_FiscalYear",
                table: "erp_budgets",
                columns: new[] { "TenantId", "FiscalYear" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_erp_budget_modifications_BudgetId",
                table: "erp_budget_modifications",
                column: "BudgetId");

            migrationBuilder.CreateIndex(
                name: "IX_erp_budget_modifications_ExpenseItemId",
                table: "erp_budget_modifications",
                column: "ExpenseItemId");

            migrationBuilder.CreateIndex(
                name: "IX_erp_budget_modifications_IncomeItemId",
                table: "erp_budget_modifications",
                column: "IncomeItemId");

            migrationBuilder.CreateIndex(
                name: "IX_erp_budget_modifications_TenantId_BudgetId_CreatedAt",
                table: "erp_budget_modifications",
                columns: new[] { "TenantId", "BudgetId", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_erp_executed_expenses_ExpenseItemId_ExpenseDate",
                table: "erp_executed_expenses",
                columns: new[] { "ExpenseItemId", "ExpenseDate" });

            migrationBuilder.CreateIndex(
                name: "IX_erp_executed_expenses_ProviderId",
                table: "erp_executed_expenses",
                column: "ProviderId");

            migrationBuilder.CreateIndex(
                name: "IX_erp_executed_expenses_TenantId_ExpenseDate",
                table: "erp_executed_expenses",
                columns: new[] { "TenantId", "ExpenseDate" });

            migrationBuilder.CreateIndex(
                name: "IX_erp_expense_items_BudgetId_Name",
                table: "erp_expense_items",
                columns: new[] { "BudgetId", "Name" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_erp_income_items_BudgetId_Name",
                table: "erp_income_items",
                columns: new[] { "BudgetId", "Name" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_erp_contingency_fund_usages_erp_budgets_BudgetId",
                table: "erp_contingency_fund_usages",
                column: "BudgetId",
                principalTable: "erp_budgets",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_erp_contingency_fund_usages_erp_executed_expenses_ExecutedEx~",
                table: "erp_contingency_fund_usages",
                column: "ExecutedExpenseId",
                principalTable: "erp_executed_expenses",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_erp_contingency_fund_usages_erp_budgets_BudgetId",
                table: "erp_contingency_fund_usages");

            migrationBuilder.DropForeignKey(
                name: "FK_erp_contingency_fund_usages_erp_executed_expenses_ExecutedEx~",
                table: "erp_contingency_fund_usages");

            migrationBuilder.DropTable(
                name: "erp_budget_modifications");

            migrationBuilder.DropTable(
                name: "erp_executed_expenses");

            migrationBuilder.DropTable(
                name: "erp_income_items");

            migrationBuilder.DropTable(
                name: "erp_expense_items");

            migrationBuilder.DropIndex(
                name: "IX_erp_contingency_fund_usages_BudgetId",
                table: "erp_contingency_fund_usages");

            migrationBuilder.DropIndex(
                name: "IX_erp_contingency_fund_usages_ExecutedExpenseId",
                table: "erp_contingency_fund_usages");

            migrationBuilder.DropIndex(
                name: "IX_erp_budgets_TenantId_FiscalYear",
                table: "erp_budgets");

            migrationBuilder.DropColumn(
                name: "BudgetId",
                table: "erp_contingency_fund_usages");

            migrationBuilder.DropColumn(
                name: "Observations",
                table: "erp_budgets");

            migrationBuilder.RenameColumn(
                name: "ExecutedExpenseId",
                table: "erp_contingency_fund_usages",
                newName: "AccountingRecordId");

            migrationBuilder.RenameColumn(
                name: "FiscalYear",
                table: "erp_budgets",
                newName: "FiscalPeriod");

            migrationBuilder.AddColumn<Guid>(
                name: "AccountingEntryId",
                table: "erp_work_orders",
                type: "char(36)",
                nullable: true,
                collation: "ascii_general_ci");

            migrationBuilder.AddColumn<Guid>(
                name: "BudgetAccountId",
                table: "erp_work_orders",
                type: "char(36)",
                nullable: true,
                collation: "ascii_general_ci");

            migrationBuilder.AddColumn<Guid>(
                name: "AccountingEntryId",
                table: "erp_provider_payments",
                type: "char(36)",
                nullable: true,
                collation: "ascii_general_ci");

            migrationBuilder.AddColumn<Guid>(
                name: "AccountingEntryId",
                table: "erp_provider_invoices",
                type: "char(36)",
                nullable: true,
                collation: "ascii_general_ci");

            migrationBuilder.AddColumn<Guid>(
                name: "AccountingEntryId",
                table: "erp_monthly_depreciations",
                type: "char(36)",
                nullable: true,
                collation: "ascii_general_ci");

            migrationBuilder.AddColumn<Guid>(
                name: "AccountingAccountId",
                table: "erp_fixed_assets",
                type: "char(36)",
                nullable: true,
                collation: "ascii_general_ci");

            migrationBuilder.AddColumn<Guid>(
                name: "BudgetAccountId",
                table: "erp_contracts",
                type: "char(36)",
                nullable: true,
                collation: "ascii_general_ci");

            migrationBuilder.AddColumn<DateTime>(
                name: "ApprovalDate",
                table: "erp_contingency_fund_usages",
                type: "datetime(6)",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<Guid>(
                name: "AccountingEntryId",
                table: "erp_bank_movements",
                type: "char(36)",
                nullable: true,
                collation: "ascii_general_ci");

            migrationBuilder.AddColumn<Guid>(
                name: "AccountingAccountId",
                table: "erp_bank_accounts",
                type: "char(36)",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                collation: "ascii_general_ci");

            migrationBuilder.CreateTable(
                name: "erp_accounting_accounts",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    Category = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Code = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    IsActive = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    IsDeleted = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    IsGroup = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    IsOfficialStandard = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    Name = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Nature = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    TenantId = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    UpdatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_erp_accounting_accounts", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "erp_accounting_periods",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    ClosedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    ClosedByUserId = table.Column<string>(type: "varchar(450)", maxLength: 450, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    FiscalYear = table.Column<int>(type: "int", nullable: false),
                    LastEntryNumber = table.Column<int>(type: "int", nullable: false),
                    Month = table.Column<int>(type: "int", nullable: false),
                    OpenedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    PeriodLabel = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Status = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    TenantId = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_erp_accounting_periods", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "erp_contingency_fund_contributions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    AccountingRecordId = table.Column<Guid>(type: "char(36)", nullable: true, collation: "ascii_general_ci"),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    ContributionDate = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    IncomeBase = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    IsDeleted = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    Percentage = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: false),
                    Period = table.Column<string>(type: "varchar(7)", maxLength: 7, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    TenantId = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    UpdatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_erp_contingency_fund_contributions", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "erp_contingency_funds",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    CurrentBalance = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    IsDeleted = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    TenantId = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    UpdatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_erp_contingency_funds", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "erp_budget_details",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    AccountingAccountId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    BudgetId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    ApprovedValue = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    IsDeleted = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    Observations = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    UpdatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true)
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
                    BudgetId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    DestinationAccountId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    SourceAccountId = table.Column<Guid>(type: "char(36)", nullable: true, collation: "ascii_general_ci"),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    ApprovalDate = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    ApprovalType = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    CreatedByUserId = table.Column<string>(type: "varchar(450)", maxLength: 450, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    IsDeleted = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    Justification = table.Column<string>(type: "varchar(1000)", maxLength: 1000, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    MeetingActNumber = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    MovementType = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    TenantId = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    UpdatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true)
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
                name: "erp_accounting_entries",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    AccountingPeriodId = table.Column<Guid>(type: "char(36)", nullable: true, collation: "ascii_general_ci"),
                    ReversalId = table.Column<Guid>(type: "char(36)", nullable: true, collation: "ascii_general_ci"),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    CreatedByUserId = table.Column<string>(type: "varchar(450)", maxLength: 450, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Description = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    EntryDate = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    EntryNumber = table.Column<int>(type: "int", nullable: false),
                    EntryType = table.Column<string>(type: "varchar(30)", maxLength: 30, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ExternalReference = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Status = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    TenantId = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    TotalCredit = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    TotalDebit = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    UpdatedByUserId = table.Column<string>(type: "varchar(450)", maxLength: 450, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_erp_accounting_entries", x => x.Id);
                    table.ForeignKey(
                        name: "FK_erp_accounting_entries_erp_accounting_periods_AccountingPeri~",
                        column: x => x.AccountingPeriodId,
                        principalTable: "erp_accounting_periods",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "erp_entry_lines",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    AccountingAccountId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    AccountingEntryId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    Credit = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Debit = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    ThirdPartyId = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_erp_entry_lines", x => x.Id);
                    table.ForeignKey(
                        name: "FK_erp_entry_lines_erp_accounting_accounts_AccountingAccountId",
                        column: x => x.AccountingAccountId,
                        principalTable: "erp_accounting_accounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_erp_entry_lines_erp_accounting_entries_AccountingEntryId",
                        column: x => x.AccountingEntryId,
                        principalTable: "erp_accounting_entries",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "erp_entry_reversals",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    OriginalEntryId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    ReversalEntryId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    Reason = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ReversedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    ReversedByUserId = table.Column<string>(type: "varchar(450)", maxLength: 450, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    TenantId = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_erp_entry_reversals", x => x.Id);
                    table.ForeignKey(
                        name: "FK_erp_entry_reversals_erp_accounting_entries_OriginalEntryId",
                        column: x => x.OriginalEntryId,
                        principalTable: "erp_accounting_entries",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_erp_entry_reversals_erp_accounting_entries_ReversalEntryId",
                        column: x => x.ReversalEntryId,
                        principalTable: "erp_accounting_entries",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_erp_work_orders_AccountingEntryId",
                table: "erp_work_orders",
                column: "AccountingEntryId");

            migrationBuilder.CreateIndex(
                name: "IX_erp_work_orders_BudgetAccountId",
                table: "erp_work_orders",
                column: "BudgetAccountId");

            migrationBuilder.CreateIndex(
                name: "IX_erp_provider_payments_AccountingEntryId",
                table: "erp_provider_payments",
                column: "AccountingEntryId");

            migrationBuilder.CreateIndex(
                name: "IX_erp_provider_invoices_AccountingEntryId",
                table: "erp_provider_invoices",
                column: "AccountingEntryId");

            migrationBuilder.CreateIndex(
                name: "IX_erp_monthly_depreciations_AccountingEntryId",
                table: "erp_monthly_depreciations",
                column: "AccountingEntryId");

            migrationBuilder.CreateIndex(
                name: "IX_erp_fixed_assets_AccountingAccountId",
                table: "erp_fixed_assets",
                column: "AccountingAccountId");

            migrationBuilder.CreateIndex(
                name: "IX_erp_contracts_BudgetAccountId",
                table: "erp_contracts",
                column: "BudgetAccountId");

            migrationBuilder.CreateIndex(
                name: "IX_erp_budgets_TenantId_FiscalPeriod",
                table: "erp_budgets",
                columns: new[] { "TenantId", "FiscalPeriod" });

            migrationBuilder.CreateIndex(
                name: "IX_erp_bank_movements_AccountingEntryId",
                table: "erp_bank_movements",
                column: "AccountingEntryId");

            migrationBuilder.CreateIndex(
                name: "IX_erp_bank_accounts_AccountingAccountId",
                table: "erp_bank_accounts",
                column: "AccountingAccountId");

            migrationBuilder.CreateIndex(
                name: "IX_accounts_tenant_code_group",
                table: "erp_accounting_accounts",
                columns: new[] { "TenantId", "Code", "IsGroup" });

            migrationBuilder.CreateIndex(
                name: "IX_erp_accounting_accounts_Code",
                table: "erp_accounting_accounts",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_entries_pagination",
                table: "erp_accounting_entries",
                columns: new[] { "TenantId", "EntryDate", "EntryNumber" });

            migrationBuilder.CreateIndex(
                name: "IX_entries_status_type",
                table: "erp_accounting_entries",
                columns: new[] { "TenantId", "Status", "EntryType" });

            migrationBuilder.CreateIndex(
                name: "IX_erp_accounting_entries_AccountingPeriodId",
                table: "erp_accounting_entries",
                column: "AccountingPeriodId");

            migrationBuilder.CreateIndex(
                name: "IX_erp_accounting_entries_ReversalId",
                table: "erp_accounting_entries",
                column: "ReversalId");

            migrationBuilder.CreateIndex(
                name: "IX_erp_accounting_entries_TenantId_EntryDate",
                table: "erp_accounting_entries",
                columns: new[] { "TenantId", "EntryDate" });

            migrationBuilder.CreateIndex(
                name: "IX_erp_accounting_entries_TenantId_EntryNumber",
                table: "erp_accounting_entries",
                columns: new[] { "TenantId", "EntryNumber" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_erp_accounting_entries_TenantId_Status",
                table: "erp_accounting_entries",
                columns: new[] { "TenantId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_accounting_periods_status_year_month",
                table: "erp_accounting_periods",
                columns: new[] { "TenantId", "Status", "FiscalYear", "Month" });

            migrationBuilder.CreateIndex(
                name: "IX_erp_accounting_periods_TenantId_FiscalYear_Month",
                table: "erp_accounting_periods",
                columns: new[] { "TenantId", "FiscalYear", "Month" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_budget_details_account_lookup",
                table: "erp_budget_details",
                columns: new[] { "AccountingAccountId", "BudgetId" });

            migrationBuilder.CreateIndex(
                name: "IX_budget_details_budget_account_value",
                table: "erp_budget_details",
                columns: new[] { "BudgetId", "AccountingAccountId", "ApprovedValue" });

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
                name: "IX_entry_lines_account_debit_credit",
                table: "erp_entry_lines",
                columns: new[] { "AccountingAccountId", "Debit", "Credit" });

            migrationBuilder.CreateIndex(
                name: "IX_entry_lines_account_entry",
                table: "erp_entry_lines",
                columns: new[] { "AccountingAccountId", "AccountingEntryId" });

            migrationBuilder.CreateIndex(
                name: "IX_entry_lines_entry_id",
                table: "erp_entry_lines",
                column: "AccountingEntryId");

            migrationBuilder.CreateIndex(
                name: "IX_erp_entry_reversals_OriginalEntryId",
                table: "erp_entry_reversals",
                column: "OriginalEntryId");

            migrationBuilder.CreateIndex(
                name: "IX_erp_entry_reversals_ReversalEntryId",
                table: "erp_entry_reversals",
                column: "ReversalEntryId");

            migrationBuilder.CreateIndex(
                name: "IX_erp_entry_reversals_TenantId_OriginalEntryId",
                table: "erp_entry_reversals",
                columns: new[] { "TenantId", "OriginalEntryId" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_erp_bank_accounts_erp_accounting_accounts_AccountingAccountId",
                table: "erp_bank_accounts",
                column: "AccountingAccountId",
                principalTable: "erp_accounting_accounts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_erp_bank_movements_erp_accounting_entries_AccountingEntryId",
                table: "erp_bank_movements",
                column: "AccountingEntryId",
                principalTable: "erp_accounting_entries",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_erp_contracts_erp_accounting_accounts_BudgetAccountId",
                table: "erp_contracts",
                column: "BudgetAccountId",
                principalTable: "erp_accounting_accounts",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_erp_fixed_assets_erp_accounting_accounts_AccountingAccountId",
                table: "erp_fixed_assets",
                column: "AccountingAccountId",
                principalTable: "erp_accounting_accounts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_erp_monthly_depreciations_erp_accounting_entries_AccountingE~",
                table: "erp_monthly_depreciations",
                column: "AccountingEntryId",
                principalTable: "erp_accounting_entries",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_erp_provider_invoices_erp_accounting_entries_AccountingEntry~",
                table: "erp_provider_invoices",
                column: "AccountingEntryId",
                principalTable: "erp_accounting_entries",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_erp_provider_payments_erp_accounting_entries_AccountingEntry~",
                table: "erp_provider_payments",
                column: "AccountingEntryId",
                principalTable: "erp_accounting_entries",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_erp_work_orders_erp_accounting_accounts_BudgetAccountId",
                table: "erp_work_orders",
                column: "BudgetAccountId",
                principalTable: "erp_accounting_accounts",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_erp_work_orders_erp_accounting_entries_AccountingEntryId",
                table: "erp_work_orders",
                column: "AccountingEntryId",
                principalTable: "erp_accounting_entries",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_erp_accounting_entries_erp_entry_reversals_ReversalId",
                table: "erp_accounting_entries",
                column: "ReversalId",
                principalTable: "erp_entry_reversals",
                principalColumn: "Id");
        }
    }
}
