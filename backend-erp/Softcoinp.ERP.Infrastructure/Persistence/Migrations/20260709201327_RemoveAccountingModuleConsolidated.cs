using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Softcoinp.ERP.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class RemoveAccountingModuleConsolidated : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_erp_payments_erp_bank_accounts_BankAccountId",
                table: "erp_payments");

            migrationBuilder.DropTable(
                name: "erp_monthly_depreciations");

            migrationBuilder.DropTable(
                name: "erp_reconciliation_items");

            migrationBuilder.DropTable(
                name: "erp_fixed_assets");

            migrationBuilder.DropTable(
                name: "erp_bank_movements");

            migrationBuilder.DropTable(
                name: "erp_bank_reconciliations");

            migrationBuilder.DropTable(
                name: "erp_bank_accounts");

            migrationBuilder.DropIndex(
                name: "IX_erp_payments_BankAccountId",
                table: "erp_payments");

            migrationBuilder.DropColumn(
                name: "BankAccountId",
                table: "erp_payments");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "BankAccountId",
                table: "erp_payments",
                type: "char(36)",
                nullable: true,
                collation: "ascii_general_ci");

            migrationBuilder.CreateTable(
                name: "erp_bank_accounts",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    AccountNumber = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    AccountType = table.Column<string>(type: "varchar(30)", maxLength: 30, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    BankName = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    CurrentBalance = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    IsActive = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    OpeningBalance = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    TenantId = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    UpdatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_erp_bank_accounts", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "erp_fixed_assets",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    AccumulatedDepreciation = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    AcquisitionDate = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    AcquisitionValue = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    BookValue = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    DepreciationMethod = table.Column<string>(type: "varchar(30)", maxLength: 30, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Description = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    DisposalDate = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    DisposalReason = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    DisposalValue = table.Column<decimal>(type: "decimal(65,30)", nullable: true),
                    Location = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Name = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ResidualValue = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    SerialNumber = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Status = table.Column<string>(type: "varchar(30)", maxLength: 30, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    TenantId = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    UpdatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    UsefulLifeMonths = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_erp_fixed_assets", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "erp_bank_movements",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    BankAccountId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    CreatedByUserId = table.Column<string>(type: "varchar(450)", maxLength: 450, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Description = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    MovementDate = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    MovementType = table.Column<string>(type: "varchar(30)", maxLength: 30, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ReferenceNumber = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    RunningBalance = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    TenantId = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_erp_bank_movements", x => x.Id);
                    table.ForeignKey(
                        name: "FK_erp_bank_movements_erp_bank_accounts_BankAccountId",
                        column: x => x.BankAccountId,
                        principalTable: "erp_bank_accounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "erp_bank_reconciliations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    BankAccountId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    BookBalance = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    CompletedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    CompletedByUserId = table.Column<string>(type: "varchar(450)", maxLength: 450, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    CreatedByUserId = table.Column<string>(type: "varchar(450)", maxLength: 450, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Difference = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    FiscalYear = table.Column<int>(type: "int", nullable: false),
                    Month = table.Column<int>(type: "int", nullable: false),
                    PeriodLabel = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    StatementBalance = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Status = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    TenantId = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_erp_bank_reconciliations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_erp_bank_reconciliations_erp_bank_accounts_BankAccountId",
                        column: x => x.BankAccountId,
                        principalTable: "erp_bank_accounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "erp_monthly_depreciations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    FixedAssetId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    AccumulatedAfter = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    BookValueAfter = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    DepreciationAmount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    FiscalYear = table.Column<int>(type: "int", nullable: false),
                    Month = table.Column<int>(type: "int", nullable: false),
                    PeriodLabel = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    TenantId = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_erp_monthly_depreciations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_erp_monthly_depreciations_erp_fixed_assets_FixedAssetId",
                        column: x => x.FixedAssetId,
                        principalTable: "erp_fixed_assets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "erp_reconciliation_items",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    BankMovementId = table.Column<Guid>(type: "char(36)", nullable: true, collation: "ascii_general_ci"),
                    BankReconciliationId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Description = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    IsCleared = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    IsInBooks = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    IsInStatement = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    MovementDate = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_erp_reconciliation_items", x => x.Id);
                    table.ForeignKey(
                        name: "FK_erp_reconciliation_items_erp_bank_movements_BankMovementId",
                        column: x => x.BankMovementId,
                        principalTable: "erp_bank_movements",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_erp_reconciliation_items_erp_bank_reconciliations_BankReconc~",
                        column: x => x.BankReconciliationId,
                        principalTable: "erp_bank_reconciliations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_erp_payments_BankAccountId",
                table: "erp_payments",
                column: "BankAccountId");

            migrationBuilder.CreateIndex(
                name: "IX_bank_accounts_active_balance",
                table: "erp_bank_accounts",
                columns: new[] { "TenantId", "IsActive", "CurrentBalance" });

            migrationBuilder.CreateIndex(
                name: "IX_erp_bank_accounts_TenantId_AccountNumber",
                table: "erp_bank_accounts",
                columns: new[] { "TenantId", "AccountNumber" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_erp_bank_movements_BankAccountId",
                table: "erp_bank_movements",
                column: "BankAccountId");

            migrationBuilder.CreateIndex(
                name: "IX_erp_bank_reconciliations_BankAccountId",
                table: "erp_bank_reconciliations",
                column: "BankAccountId");

            migrationBuilder.CreateIndex(
                name: "IX_erp_bank_reconciliations_TenantId_BankAccountId_FiscalYear_M~",
                table: "erp_bank_reconciliations",
                columns: new[] { "TenantId", "BankAccountId", "FiscalYear", "Month" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_erp_fixed_assets_TenantId_SerialNumber",
                table: "erp_fixed_assets",
                columns: new[] { "TenantId", "SerialNumber" },
                unique: true,
                filter: "[SerialNumber] IS NOT NULL AND [SerialNumber] <> ''");

            migrationBuilder.CreateIndex(
                name: "IX_erp_monthly_depreciations_FixedAssetId",
                table: "erp_monthly_depreciations",
                column: "FixedAssetId");

            migrationBuilder.CreateIndex(
                name: "IX_erp_monthly_depreciations_TenantId_FixedAssetId_FiscalYear_M~",
                table: "erp_monthly_depreciations",
                columns: new[] { "TenantId", "FixedAssetId", "FiscalYear", "Month" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_erp_reconciliation_items_BankMovementId",
                table: "erp_reconciliation_items",
                column: "BankMovementId");

            migrationBuilder.CreateIndex(
                name: "IX_erp_reconciliation_items_BankReconciliationId",
                table: "erp_reconciliation_items",
                column: "BankReconciliationId");

            migrationBuilder.AddForeignKey(
                name: "FK_erp_payments_erp_bank_accounts_BankAccountId",
                table: "erp_payments",
                column: "BankAccountId",
                principalTable: "erp_bank_accounts",
                principalColumn: "Id");
        }
    }
}
