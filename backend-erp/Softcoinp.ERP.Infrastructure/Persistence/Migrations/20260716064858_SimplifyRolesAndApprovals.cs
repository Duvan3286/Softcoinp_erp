using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Softcoinp.ERP.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class SimplifyRolesAndApprovals : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_erp_executed_expenses_TenantId_CouncilApproved",
                table: "erp_executed_expenses");

            migrationBuilder.DropIndex(
                name: "IX_erp_contracts_TenantId_Status_ApprovalLevel",
                table: "erp_contracts");

            migrationBuilder.DropIndex(
                name: "IX_erp_approval_thresholds_TenantId_ApprovalLevel",
                table: "erp_approval_thresholds");

            migrationBuilder.DropColumn(
                name: "ExpiresAt",
                table: "erp_user_tenant_roles");

            migrationBuilder.DropColumn(
                name: "MinimumRoleRequired",
                table: "erp_tenant_documents");

            migrationBuilder.DropColumn(
                name: "AllowedRoles",
                table: "erp_report_types");

            migrationBuilder.DropColumn(
                name: "RequiresCouncilApproval",
                table: "erp_expense_items");

            migrationBuilder.DropColumn(
                name: "CouncilApproved",
                table: "erp_executed_expenses");

            migrationBuilder.DropColumn(
                name: "ApprovalLevel",
                table: "erp_contracts");

            migrationBuilder.DropColumn(
                name: "CouncilMeetingActNumber",
                table: "erp_contracts");

            migrationBuilder.DropColumn(
                name: "CouncilApprovalActNumber",
                table: "erp_contingency_fund_usages");

            migrationBuilder.DropColumn(
                name: "ApprovalLevel",
                table: "erp_approval_thresholds");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "ExpiresAt",
                table: "erp_user_tenant_roles",
                type: "datetime(6)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "MinimumRoleRequired",
                table: "erp_tenant_documents",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "AllowedRoles",
                table: "erp_report_types",
                type: "varchar(500)",
                maxLength: 500,
                nullable: false,
                defaultValue: "")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<bool>(
                name: "RequiresCouncilApproval",
                table: "erp_expense_items",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "CouncilApproved",
                table: "erp_executed_expenses",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "ApprovalLevel",
                table: "erp_contracts",
                type: "varchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "CouncilMeetingActNumber",
                table: "erp_contracts",
                type: "varchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "CouncilApprovalActNumber",
                table: "erp_contingency_fund_usages",
                type: "varchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "ApprovalLevel",
                table: "erp_approval_thresholds",
                type: "varchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_erp_executed_expenses_TenantId_CouncilApproved",
                table: "erp_executed_expenses",
                columns: new[] { "TenantId", "CouncilApproved" });

            migrationBuilder.CreateIndex(
                name: "IX_erp_contracts_TenantId_Status_ApprovalLevel",
                table: "erp_contracts",
                columns: new[] { "TenantId", "Status", "ApprovalLevel" });

            migrationBuilder.CreateIndex(
                name: "IX_erp_approval_thresholds_TenantId_ApprovalLevel",
                table: "erp_approval_thresholds",
                columns: new[] { "TenantId", "ApprovalLevel" },
                unique: true);
        }
    }
}
