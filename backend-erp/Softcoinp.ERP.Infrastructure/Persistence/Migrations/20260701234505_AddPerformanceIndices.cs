using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Softcoinp.ERP.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddPerformanceIndices : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // The replacement composite indexes are created BEFORE the single-column
            // indexes they replace are dropped. Both single-column indexes are the only
            // index backing a foreign key (AccountingAccountId), and MySQL refuses to
            // drop an index that is "needed in a foreign key constraint" unless another
            // index already covers that column as its leading column.
            migrationBuilder.CreateIndex(
                name: "IX_entry_lines_account_entry",
                table: "erp_entry_lines",
                columns: new[] { "AccountingAccountId", "AccountingEntryId" });

            migrationBuilder.DropIndex(
                name: "IX_erp_entry_lines_AccountingAccountId",
                table: "erp_entry_lines");

            migrationBuilder.CreateIndex(
                name: "IX_budget_details_account_lookup",
                table: "erp_budget_details",
                columns: new[] { "AccountingAccountId", "BudgetId" });

            migrationBuilder.DropIndex(
                name: "IX_erp_budget_details_AccountingAccountId",
                table: "erp_budget_details");

            migrationBuilder.RenameIndex(
                name: "IX_erp_payment_allocations_LateInterestId",
                table: "erp_payment_allocations",
                newName: "IX_payment_alloc_late_interest");

            migrationBuilder.RenameIndex(
                name: "IX_erp_entry_lines_AccountingEntryId",
                table: "erp_entry_lines",
                newName: "IX_entry_lines_entry_id");

            migrationBuilder.CreateIndex(
                name: "IX_unit_fees_overdue_balance",
                table: "erp_unit_fees",
                columns: new[] { "TenantId", "UnitId", "Status", "BalanceAmount", "DueDate" });

            migrationBuilder.CreateIndex(
                name: "IX_payments_advance_sum",
                table: "erp_payments",
                columns: new[] { "TenantId", "UnitId", "AdvanceAmount" });

            migrationBuilder.CreateIndex(
                name: "IX_charges_overdue_balance",
                table: "erp_individual_charges",
                columns: new[] { "TenantId", "UnitId", "Status", "BalanceAmount", "ChargeDate" });

            migrationBuilder.CreateIndex(
                name: "IX_extra_dist_overdue_balance",
                table: "erp_extraordinary_fee_distributions",
                columns: new[] { "TenantId", "UnitId", "Status", "BalanceAmount", "DueDate" });

            migrationBuilder.CreateIndex(
                name: "IX_entries_pagination",
                table: "erp_accounting_entries",
                columns: new[] { "TenantId", "EntryDate", "EntryNumber" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_unit_fees_overdue_balance",
                table: "erp_unit_fees");

            migrationBuilder.DropIndex(
                name: "IX_payments_advance_sum",
                table: "erp_payments");

            migrationBuilder.DropIndex(
                name: "IX_charges_overdue_balance",
                table: "erp_individual_charges");

            migrationBuilder.DropIndex(
                name: "IX_extra_dist_overdue_balance",
                table: "erp_extraordinary_fee_distributions");

            migrationBuilder.DropIndex(
                name: "IX_entries_pagination",
                table: "erp_accounting_entries");

            migrationBuilder.RenameIndex(
                name: "IX_payment_alloc_late_interest",
                table: "erp_payment_allocations",
                newName: "IX_erp_payment_allocations_LateInterestId");

            migrationBuilder.RenameIndex(
                name: "IX_entry_lines_entry_id",
                table: "erp_entry_lines",
                newName: "IX_erp_entry_lines_AccountingEntryId");

            // The old single-column indexes are recreated BEFORE the composite indexes
            // that currently back the foreign keys are dropped, for the same reason as
            // in Up(): the foreign key must always be covered by an existing index.
            migrationBuilder.CreateIndex(
                name: "IX_erp_entry_lines_AccountingAccountId",
                table: "erp_entry_lines",
                column: "AccountingAccountId");

            migrationBuilder.DropIndex(
                name: "IX_entry_lines_account_entry",
                table: "erp_entry_lines");

            migrationBuilder.CreateIndex(
                name: "IX_erp_budget_details_AccountingAccountId",
                table: "erp_budget_details",
                column: "AccountingAccountId");

            migrationBuilder.DropIndex(
                name: "IX_budget_details_account_lookup",
                table: "erp_budget_details");
        }
    }
}
