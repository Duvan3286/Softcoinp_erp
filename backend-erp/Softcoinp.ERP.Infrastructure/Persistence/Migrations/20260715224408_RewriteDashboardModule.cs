using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Softcoinp.ERP.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class RewriteDashboardModule : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                "UPDATE `erp_alert_configurations` SET `RuleType` = 'BudgetItemExecutionExceeded' WHERE `RuleType` = 'BudgetAccountExceeded';");

            migrationBuilder.CreateIndex(
                name: "IX_erp_work_orders_TenantId_OrderType_Status_ScheduledDate",
                table: "erp_work_orders",
                columns: new[] { "TenantId", "OrderType", "Status", "ScheduledDate" });

            migrationBuilder.CreateIndex(
                name: "IX_erp_reservations_TenantId_OwnerId_StartDateTime",
                table: "erp_reservations",
                columns: new[] { "TenantId", "OwnerId", "StartDateTime" });

            migrationBuilder.CreateIndex(
                name: "IX_erp_reservations_TenantId_Status_StartDateTime",
                table: "erp_reservations",
                columns: new[] { "TenantId", "Status", "StartDateTime" });

            migrationBuilder.CreateIndex(
                name: "IX_erp_executed_expenses_TenantId_CouncilApproved",
                table: "erp_executed_expenses",
                columns: new[] { "TenantId", "CouncilApproved" });

            migrationBuilder.CreateIndex(
                name: "IX_erp_contracts_TenantId_Status_ApprovalLevel",
                table: "erp_contracts",
                columns: new[] { "TenantId", "Status", "ApprovalLevel" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_erp_work_orders_TenantId_OrderType_Status_ScheduledDate",
                table: "erp_work_orders");

            migrationBuilder.DropIndex(
                name: "IX_erp_reservations_TenantId_OwnerId_StartDateTime",
                table: "erp_reservations");

            migrationBuilder.DropIndex(
                name: "IX_erp_reservations_TenantId_Status_StartDateTime",
                table: "erp_reservations");

            migrationBuilder.DropIndex(
                name: "IX_erp_executed_expenses_TenantId_CouncilApproved",
                table: "erp_executed_expenses");

            migrationBuilder.DropIndex(
                name: "IX_erp_contracts_TenantId_Status_ApprovalLevel",
                table: "erp_contracts");

            migrationBuilder.Sql(
                "UPDATE `erp_alert_configurations` SET `RuleType` = 'BudgetAccountExceeded' WHERE `RuleType` = 'BudgetItemExecutionExceeded';");
        }
    }
}
