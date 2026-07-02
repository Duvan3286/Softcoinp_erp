using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Softcoinp.ERP.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddAdvancedPerformanceIndices : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_units_tenant_status",
                table: "erp_units",
                columns: new[] { "TenantId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_unit_fees_period_agg",
                table: "erp_unit_fees",
                columns: new[] { "TenantId", "BillingPeriodId", "FeeValue", "PaidAmount" });

            migrationBuilder.CreateIndex(
                name: "IX_payments_tenant_unit_created",
                table: "erp_payments",
                columns: new[] { "TenantId", "UnitId", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_owners_tenant_email",
                table: "erp_owners",
                columns: new[] { "TenantId", "Email" });

            migrationBuilder.CreateIndex(
                name: "IX_late_interests_fee_cap_amount",
                table: "erp_late_interests",
                columns: new[] { "TenantId", "UnitFeeId", "IsCapitalized", "CalculatedAmount" });

            migrationBuilder.CreateIndex(
                name: "IX_entry_lines_account_debit_credit",
                table: "erp_entry_lines",
                columns: new[] { "AccountingAccountId", "Debit", "Credit" });

            migrationBuilder.CreateIndex(
                name: "IX_budget_details_budget_account_value",
                table: "erp_budget_details",
                columns: new[] { "BudgetId", "AccountingAccountId", "ApprovedValue" });

            migrationBuilder.CreateIndex(
                name: "IX_bank_accounts_active_balance",
                table: "erp_bank_accounts",
                columns: new[] { "TenantId", "IsActive", "CurrentBalance" });

            migrationBuilder.CreateIndex(
                name: "IX_agreement_installments_overdue",
                table: "erp_agreement_installments",
                columns: new[] { "TenantId", "Status", "DueDate", "Amount" });

            migrationBuilder.CreateIndex(
                name: "IX_accounting_periods_status_year_month",
                table: "erp_accounting_periods",
                columns: new[] { "TenantId", "Status", "FiscalYear", "Month" });

            migrationBuilder.CreateIndex(
                name: "IX_entries_status_type",
                table: "erp_accounting_entries",
                columns: new[] { "TenantId", "Status", "EntryType" });

            migrationBuilder.CreateIndex(
                name: "IX_accounts_tenant_code_group",
                table: "erp_accounting_accounts",
                columns: new[] { "TenantId", "Code", "IsGroup" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_units_tenant_status",
                table: "erp_units");

            migrationBuilder.DropIndex(
                name: "IX_unit_fees_period_agg",
                table: "erp_unit_fees");

            migrationBuilder.DropIndex(
                name: "IX_payments_tenant_unit_created",
                table: "erp_payments");

            migrationBuilder.DropIndex(
                name: "IX_owners_tenant_email",
                table: "erp_owners");

            migrationBuilder.DropIndex(
                name: "IX_late_interests_fee_cap_amount",
                table: "erp_late_interests");

            migrationBuilder.DropIndex(
                name: "IX_entry_lines_account_debit_credit",
                table: "erp_entry_lines");

            migrationBuilder.DropIndex(
                name: "IX_budget_details_budget_account_value",
                table: "erp_budget_details");

            migrationBuilder.DropIndex(
                name: "IX_bank_accounts_active_balance",
                table: "erp_bank_accounts");

            migrationBuilder.DropIndex(
                name: "IX_agreement_installments_overdue",
                table: "erp_agreement_installments");

            migrationBuilder.DropIndex(
                name: "IX_accounting_periods_status_year_month",
                table: "erp_accounting_periods");

            migrationBuilder.DropIndex(
                name: "IX_entries_status_type",
                table: "erp_accounting_entries");

            migrationBuilder.DropIndex(
                name: "IX_accounts_tenant_code_group",
                table: "erp_accounting_accounts");
        }
    }
}
