using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Softcoinp.ERP.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class SimplifySuppliersContractsModule : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_erp_provider_evaluations_erp_contracts_ContractId",
                table: "erp_provider_evaluations");

            migrationBuilder.DropTable(
                name: "erp_contract_policies");

            migrationBuilder.DropTable(
                name: "erp_retention_configurations");

            migrationBuilder.DropIndex(
                name: "IX_erp_provider_evaluations_ContractId",
                table: "erp_provider_evaluations");

            migrationBuilder.DropColumn(
                name: "City",
                table: "erp_providers");

            migrationBuilder.DropColumn(
                name: "EconomicActivity",
                table: "erp_providers");

            migrationBuilder.DropColumn(
                name: "IsPreferred",
                table: "erp_providers");

            migrationBuilder.DropColumn(
                name: "LegalRepDocumentNumber",
                table: "erp_providers");

            migrationBuilder.DropColumn(
                name: "LegalRepDocumentType",
                table: "erp_providers");

            migrationBuilder.DropColumn(
                name: "LegalRepEmail",
                table: "erp_providers");

            migrationBuilder.DropColumn(
                name: "LegalRepName",
                table: "erp_providers");

            migrationBuilder.DropColumn(
                name: "TradeName",
                table: "erp_providers");

            migrationBuilder.DropColumn(
                name: "VerificationDigit",
                table: "erp_providers");

            migrationBuilder.DropColumn(
                name: "BankAccount",
                table: "erp_provider_payments");

            migrationBuilder.DropColumn(
                name: "Notes",
                table: "erp_provider_payments");

            migrationBuilder.DropColumn(
                name: "ReceiptFilePath",
                table: "erp_provider_payments");

            migrationBuilder.DropColumn(
                name: "Description",
                table: "erp_provider_invoices");

            migrationBuilder.DropColumn(
                name: "InvoiceFilePath",
                table: "erp_provider_invoices");

            migrationBuilder.DropColumn(
                name: "IvaAmount",
                table: "erp_provider_invoices");

            migrationBuilder.DropColumn(
                name: "NetAmount",
                table: "erp_provider_invoices");

            migrationBuilder.DropColumn(
                name: "RetentionFuelAmount",
                table: "erp_provider_invoices");

            migrationBuilder.DropColumn(
                name: "ContractId",
                table: "erp_provider_evaluations");

            migrationBuilder.DropColumn(
                name: "AssemblyMeetingActNumber",
                table: "erp_contracts");

            migrationBuilder.RenameColumn(
                name: "Subtotal",
                table: "erp_provider_invoices",
                newName: "TotalAmount");

            migrationBuilder.RenameColumn(
                name: "RetentionIcaAmount",
                table: "erp_provider_invoices",
                newName: "AmountPaid");

            migrationBuilder.RenameColumn(
                name: "ServiceQualityScore",
                table: "erp_provider_evaluations",
                newName: "QualityScore");

            migrationBuilder.RenameColumn(
                name: "PriceFairnessScore",
                table: "erp_provider_evaluations",
                newName: "PriceScore");

            migrationBuilder.RenameColumn(
                name: "AfterSalesScore",
                table: "erp_provider_evaluations",
                newName: "AttentionScore");

            migrationBuilder.AddColumn<string>(
                name: "ChamberOfCommerceFilePath",
                table: "erp_providers",
                type: "varchar(1000)",
                maxLength: 1000,
                nullable: false,
                defaultValue: "")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<Guid>(
                name: "BudgetItemId",
                table: "erp_provider_invoices",
                type: "char(36)",
                nullable: true,
                collation: "ascii_general_ci");

            migrationBuilder.AddColumn<DateTime>(
                name: "PaymentDate",
                table: "erp_provider_invoices",
                type: "datetime(6)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PaymentMethod",
                table: "erp_provider_invoices",
                type: "varchar(20)",
                maxLength: 20,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "PaymentReferenceNumber",
                table: "erp_provider_invoices",
                type: "varchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "Observations",
                table: "erp_contracts",
                type: "varchar(2000)",
                maxLength: 2000,
                nullable: false,
                defaultValue: "")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_erp_provider_invoices_BudgetItemId",
                table: "erp_provider_invoices",
                column: "BudgetItemId");

            migrationBuilder.AddForeignKey(
                name: "FK_erp_provider_invoices_erp_expense_items_BudgetItemId",
                table: "erp_provider_invoices",
                column: "BudgetItemId",
                principalTable: "erp_expense_items",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_erp_provider_invoices_erp_expense_items_BudgetItemId",
                table: "erp_provider_invoices");

            migrationBuilder.DropIndex(
                name: "IX_erp_provider_invoices_BudgetItemId",
                table: "erp_provider_invoices");

            migrationBuilder.DropColumn(
                name: "ChamberOfCommerceFilePath",
                table: "erp_providers");

            migrationBuilder.DropColumn(
                name: "BudgetItemId",
                table: "erp_provider_invoices");

            migrationBuilder.DropColumn(
                name: "PaymentDate",
                table: "erp_provider_invoices");

            migrationBuilder.DropColumn(
                name: "PaymentMethod",
                table: "erp_provider_invoices");

            migrationBuilder.DropColumn(
                name: "PaymentReferenceNumber",
                table: "erp_provider_invoices");

            migrationBuilder.DropColumn(
                name: "Observations",
                table: "erp_contracts");

            migrationBuilder.RenameColumn(
                name: "TotalAmount",
                table: "erp_provider_invoices",
                newName: "Subtotal");

            migrationBuilder.RenameColumn(
                name: "AmountPaid",
                table: "erp_provider_invoices",
                newName: "RetentionIcaAmount");

            migrationBuilder.RenameColumn(
                name: "QualityScore",
                table: "erp_provider_evaluations",
                newName: "ServiceQualityScore");

            migrationBuilder.RenameColumn(
                name: "PriceScore",
                table: "erp_provider_evaluations",
                newName: "PriceFairnessScore");

            migrationBuilder.RenameColumn(
                name: "AttentionScore",
                table: "erp_provider_evaluations",
                newName: "AfterSalesScore");

            migrationBuilder.AddColumn<string>(
                name: "City",
                table: "erp_providers",
                type: "varchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "EconomicActivity",
                table: "erp_providers",
                type: "varchar(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<bool>(
                name: "IsPreferred",
                table: "erp_providers",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "LegalRepDocumentNumber",
                table: "erp_providers",
                type: "varchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "LegalRepDocumentType",
                table: "erp_providers",
                type: "varchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "LegalRepEmail",
                table: "erp_providers",
                type: "varchar(256)",
                maxLength: 256,
                nullable: false,
                defaultValue: "")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "LegalRepName",
                table: "erp_providers",
                type: "varchar(300)",
                maxLength: 300,
                nullable: false,
                defaultValue: "")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "TradeName",
                table: "erp_providers",
                type: "varchar(300)",
                maxLength: 300,
                nullable: false,
                defaultValue: "")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "VerificationDigit",
                table: "erp_providers",
                type: "varchar(2)",
                maxLength: 2,
                nullable: false,
                defaultValue: "")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "BankAccount",
                table: "erp_provider_payments",
                type: "varchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "Notes",
                table: "erp_provider_payments",
                type: "varchar(1000)",
                maxLength: 1000,
                nullable: false,
                defaultValue: "")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "ReceiptFilePath",
                table: "erp_provider_payments",
                type: "varchar(1000)",
                maxLength: 1000,
                nullable: false,
                defaultValue: "")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "erp_provider_invoices",
                type: "varchar(2000)",
                maxLength: 2000,
                nullable: false,
                defaultValue: "")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "InvoiceFilePath",
                table: "erp_provider_invoices",
                type: "varchar(1000)",
                maxLength: 1000,
                nullable: false,
                defaultValue: "")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<decimal>(
                name: "IvaAmount",
                table: "erp_provider_invoices",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "NetAmount",
                table: "erp_provider_invoices",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "RetentionFuelAmount",
                table: "erp_provider_invoices",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<Guid>(
                name: "ContractId",
                table: "erp_provider_evaluations",
                type: "char(36)",
                nullable: true,
                collation: "ascii_general_ci");

            migrationBuilder.AddColumn<string>(
                name: "AssemblyMeetingActNumber",
                table: "erp_contracts",
                type: "varchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "erp_contract_policies",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    ContractId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    CreatedByUserId = table.Column<string>(type: "varchar(450)", maxLength: 450, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    EndDate = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    FilePath = table.Column<string>(type: "varchar(1000)", maxLength: 1000, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    InsuranceCompany = table.Column<string>(type: "varchar(300)", maxLength: 300, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    InsuredAmount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    IsActive = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    PolicyNumber = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    PolicyType = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    StartDate = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    TenantId = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_erp_contract_policies", x => x.Id);
                    table.ForeignKey(
                        name: "FK_erp_contract_policies_erp_contracts_ContractId",
                        column: x => x.ContractId,
                        principalTable: "erp_contracts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "erp_retention_configurations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    CreatedByUserId = table.Column<string>(type: "varchar(450)", maxLength: 450, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    IsActive = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    RetentionFuelRate = table.Column<decimal>(type: "decimal(5,4)", precision: 5, scale: 4, nullable: false),
                    RetentionIcaRate = table.Column<decimal>(type: "decimal(5,4)", precision: 5, scale: 4, nullable: false),
                    ServiceDescription = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ServiceType = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    TenantId = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    UpdatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    UpdatedByUserId = table.Column<string>(type: "varchar(450)", maxLength: 450, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_erp_retention_configurations", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_erp_provider_evaluations_ContractId",
                table: "erp_provider_evaluations",
                column: "ContractId");

            migrationBuilder.CreateIndex(
                name: "IX_erp_contract_policies_ContractId",
                table: "erp_contract_policies",
                column: "ContractId");

            migrationBuilder.CreateIndex(
                name: "IX_erp_contract_policies_TenantId_ContractId",
                table: "erp_contract_policies",
                columns: new[] { "TenantId", "ContractId" });

            migrationBuilder.CreateIndex(
                name: "IX_erp_contract_policies_TenantId_EndDate_IsActive",
                table: "erp_contract_policies",
                columns: new[] { "TenantId", "EndDate", "IsActive" });

            migrationBuilder.CreateIndex(
                name: "IX_erp_retention_configurations_TenantId_ServiceType",
                table: "erp_retention_configurations",
                columns: new[] { "TenantId", "ServiceType" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_erp_provider_evaluations_erp_contracts_ContractId",
                table: "erp_provider_evaluations",
                column: "ContractId",
                principalTable: "erp_contracts",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }
    }
}
