using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Softcoinp.ERP.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddExpenseItemLinkToMaintenance : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "ExpenseItemId",
                table: "erp_work_orders",
                type: "char(36)",
                nullable: true,
                collation: "ascii_general_ci");

            migrationBuilder.AddColumn<Guid>(
                name: "ExpenseItemId",
                table: "erp_maintenance_plans",
                type: "char(36)",
                nullable: true,
                collation: "ascii_general_ci");

            migrationBuilder.CreateIndex(
                name: "IX_erp_work_orders_ExpenseItemId",
                table: "erp_work_orders",
                column: "ExpenseItemId");

            migrationBuilder.CreateIndex(
                name: "IX_erp_maintenance_plans_ExpenseItemId",
                table: "erp_maintenance_plans",
                column: "ExpenseItemId");

            migrationBuilder.AddForeignKey(
                name: "FK_erp_maintenance_plans_erp_expense_items_ExpenseItemId",
                table: "erp_maintenance_plans",
                column: "ExpenseItemId",
                principalTable: "erp_expense_items",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_erp_work_orders_erp_expense_items_ExpenseItemId",
                table: "erp_work_orders",
                column: "ExpenseItemId",
                principalTable: "erp_expense_items",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_erp_maintenance_plans_erp_expense_items_ExpenseItemId",
                table: "erp_maintenance_plans");

            migrationBuilder.DropForeignKey(
                name: "FK_erp_work_orders_erp_expense_items_ExpenseItemId",
                table: "erp_work_orders");

            migrationBuilder.DropIndex(
                name: "IX_erp_work_orders_ExpenseItemId",
                table: "erp_work_orders");

            migrationBuilder.DropIndex(
                name: "IX_erp_maintenance_plans_ExpenseItemId",
                table: "erp_maintenance_plans");

            migrationBuilder.DropColumn(
                name: "ExpenseItemId",
                table: "erp_work_orders");

            migrationBuilder.DropColumn(
                name: "ExpenseItemId",
                table: "erp_maintenance_plans");
        }
    }
}
