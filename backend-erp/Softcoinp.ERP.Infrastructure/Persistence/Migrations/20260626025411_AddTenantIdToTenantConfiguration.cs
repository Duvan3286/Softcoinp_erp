using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Softcoinp.ERP.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddTenantIdToTenantConfiguration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "TenantId",
                table: "erp_tenant_configuration",
                type: "varchar(255)",
                maxLength: 255,
                nullable: false,
                defaultValue: "")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<Guid>(
                name: "BankAccountId",
                table: "erp_payments",
                type: "char(36)",
                nullable: true,
                collation: "ascii_general_ci");

            migrationBuilder.AddColumn<decimal>(
                name: "TotalBilled",
                table: "erp_billing_periods",
                type: "decimal(65,30)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.CreateIndex(
                name: "IX_erp_tenant_configuration_TenantId",
                table: "erp_tenant_configuration",
                column: "TenantId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_erp_payments_BankAccountId",
                table: "erp_payments",
                column: "BankAccountId");

            migrationBuilder.AddForeignKey(
                name: "FK_erp_payments_erp_bank_accounts_BankAccountId",
                table: "erp_payments",
                column: "BankAccountId",
                principalTable: "erp_bank_accounts",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_erp_payments_erp_bank_accounts_BankAccountId",
                table: "erp_payments");

            migrationBuilder.DropIndex(
                name: "IX_erp_tenant_configuration_TenantId",
                table: "erp_tenant_configuration");

            migrationBuilder.DropIndex(
                name: "IX_erp_payments_BankAccountId",
                table: "erp_payments");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "erp_tenant_configuration");

            migrationBuilder.DropColumn(
                name: "BankAccountId",
                table: "erp_payments");

            migrationBuilder.DropColumn(
                name: "TotalBilled",
                table: "erp_billing_periods");
        }
    }
}
