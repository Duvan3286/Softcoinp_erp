using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Softcoinp.ERP.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddMaintenanceModuleIntegrationLinks : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "MaintenancePlanId",
                table: "erp_work_orders",
                type: "char(36)",
                nullable: true,
                collation: "ascii_general_ci");

            migrationBuilder.AddColumn<Guid>(
                name: "InsuranceContractId",
                table: "erp_incidents",
                type: "char(36)",
                nullable: true,
                collation: "ascii_general_ci");

            migrationBuilder.AddColumn<Guid>(
                name: "ReservableSpaceId",
                table: "erp_common_assets",
                type: "char(36)",
                nullable: true,
                collation: "ascii_general_ci");

            migrationBuilder.CreateIndex(
                name: "IX_erp_work_orders_MaintenancePlanId",
                table: "erp_work_orders",
                column: "MaintenancePlanId");

            migrationBuilder.CreateIndex(
                name: "IX_erp_incidents_InsuranceContractId",
                table: "erp_incidents",
                column: "InsuranceContractId");

            migrationBuilder.CreateIndex(
                name: "IX_erp_common_assets_ReservableSpaceId",
                table: "erp_common_assets",
                column: "ReservableSpaceId");

            migrationBuilder.AddForeignKey(
                name: "FK_erp_common_assets_erp_reservable_spaces_ReservableSpaceId",
                table: "erp_common_assets",
                column: "ReservableSpaceId",
                principalTable: "erp_reservable_spaces",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_erp_incidents_erp_contracts_InsuranceContractId",
                table: "erp_incidents",
                column: "InsuranceContractId",
                principalTable: "erp_contracts",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_erp_work_orders_erp_maintenance_plans_MaintenancePlanId",
                table: "erp_work_orders",
                column: "MaintenancePlanId",
                principalTable: "erp_maintenance_plans",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_erp_common_assets_erp_reservable_spaces_ReservableSpaceId",
                table: "erp_common_assets");

            migrationBuilder.DropForeignKey(
                name: "FK_erp_incidents_erp_contracts_InsuranceContractId",
                table: "erp_incidents");

            migrationBuilder.DropForeignKey(
                name: "FK_erp_work_orders_erp_maintenance_plans_MaintenancePlanId",
                table: "erp_work_orders");

            migrationBuilder.DropIndex(
                name: "IX_erp_work_orders_MaintenancePlanId",
                table: "erp_work_orders");

            migrationBuilder.DropIndex(
                name: "IX_erp_incidents_InsuranceContractId",
                table: "erp_incidents");

            migrationBuilder.DropIndex(
                name: "IX_erp_common_assets_ReservableSpaceId",
                table: "erp_common_assets");

            migrationBuilder.DropColumn(
                name: "MaintenancePlanId",
                table: "erp_work_orders");

            migrationBuilder.DropColumn(
                name: "InsuranceContractId",
                table: "erp_incidents");

            migrationBuilder.DropColumn(
                name: "ReservableSpaceId",
                table: "erp_common_assets");
        }
    }
}
