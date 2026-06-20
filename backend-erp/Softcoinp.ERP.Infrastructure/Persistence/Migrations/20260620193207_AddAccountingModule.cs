using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Softcoinp.ERP.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddAccountingModule : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_erp_accounting_entries_erp_accounting_accounts_AccountingAcc~",
                table: "erp_accounting_entries");

            migrationBuilder.DropIndex(
                name: "IX_erp_accounting_entries_AccountingAccountId",
                table: "erp_accounting_entries");

            migrationBuilder.DropColumn(
                name: "AccountingAccountId",
                table: "erp_accounting_entries");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "erp_accounting_entries");

            migrationBuilder.RenameColumn(
                name: "Reference",
                table: "erp_accounting_entries",
                newName: "ExternalReference");

            migrationBuilder.RenameColumn(
                name: "Debit",
                table: "erp_accounting_entries",
                newName: "TotalDebit");

            migrationBuilder.RenameColumn(
                name: "Credit",
                table: "erp_accounting_entries",
                newName: "TotalCredit");

            migrationBuilder.AddColumn<Guid>(
                name: "AccountingPeriodId",
                table: "erp_accounting_entries",
                type: "char(36)",
                nullable: true,
                collation: "ascii_general_ci");

            migrationBuilder.AddColumn<string>(
                name: "CreatedByUserId",
                table: "erp_accounting_entries",
                type: "varchar(450)",
                maxLength: 450,
                nullable: false,
                defaultValue: "")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<int>(
                name: "EntryNumber",
                table: "erp_accounting_entries",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "EntryType",
                table: "erp_accounting_entries",
                type: "varchar(30)",
                maxLength: 30,
                nullable: false,
                defaultValue: "")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<Guid>(
                name: "ReversalId",
                table: "erp_accounting_entries",
                type: "char(36)",
                nullable: true,
                collation: "ascii_general_ci");

            migrationBuilder.AddColumn<string>(
                name: "Status",
                table: "erp_accounting_entries",
                type: "varchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "UpdatedByUserId",
                table: "erp_accounting_entries",
                type: "varchar(450)",
                maxLength: 450,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "erp_accounting_periods",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    TenantId = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    FiscalYear = table.Column<int>(type: "int", nullable: false),
                    Month = table.Column<int>(type: "int", nullable: false),
                    PeriodLabel = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Status = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    OpenedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    ClosedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    ClosedByUserId = table.Column<string>(type: "varchar(450)", maxLength: 450, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    LastEntryNumber = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_erp_accounting_periods", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "erp_entry_lines",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    AccountingEntryId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    AccountingAccountId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    ThirdPartyId = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Debit = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Credit = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false)
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
                    TenantId = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    OriginalEntryId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    ReversalEntryId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    Reason = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ReversedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    ReversedByUserId = table.Column<string>(type: "varchar(450)", maxLength: 450, nullable: false)
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
                name: "IX_erp_accounting_entries_AccountingPeriodId",
                table: "erp_accounting_entries",
                column: "AccountingPeriodId");

            migrationBuilder.CreateIndex(
                name: "IX_erp_accounting_entries_ReversalId",
                table: "erp_accounting_entries",
                column: "ReversalId");

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
                name: "IX_erp_accounting_periods_TenantId_FiscalYear_Month",
                table: "erp_accounting_periods",
                columns: new[] { "TenantId", "FiscalYear", "Month" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_erp_entry_lines_AccountingAccountId",
                table: "erp_entry_lines",
                column: "AccountingAccountId");

            migrationBuilder.CreateIndex(
                name: "IX_erp_entry_lines_AccountingEntryId",
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
                name: "FK_erp_accounting_entries_erp_accounting_periods_AccountingPeri~",
                table: "erp_accounting_entries",
                column: "AccountingPeriodId",
                principalTable: "erp_accounting_periods",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_erp_accounting_entries_erp_entry_reversals_ReversalId",
                table: "erp_accounting_entries",
                column: "ReversalId",
                principalTable: "erp_entry_reversals",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_erp_accounting_entries_erp_accounting_periods_AccountingPeri~",
                table: "erp_accounting_entries");

            migrationBuilder.DropForeignKey(
                name: "FK_erp_accounting_entries_erp_entry_reversals_ReversalId",
                table: "erp_accounting_entries");

            migrationBuilder.DropTable(
                name: "erp_accounting_periods");

            migrationBuilder.DropTable(
                name: "erp_entry_lines");

            migrationBuilder.DropTable(
                name: "erp_entry_reversals");

            migrationBuilder.DropIndex(
                name: "IX_erp_accounting_entries_AccountingPeriodId",
                table: "erp_accounting_entries");

            migrationBuilder.DropIndex(
                name: "IX_erp_accounting_entries_ReversalId",
                table: "erp_accounting_entries");

            migrationBuilder.DropIndex(
                name: "IX_erp_accounting_entries_TenantId_EntryNumber",
                table: "erp_accounting_entries");

            migrationBuilder.DropIndex(
                name: "IX_erp_accounting_entries_TenantId_Status",
                table: "erp_accounting_entries");

            migrationBuilder.DropColumn(
                name: "AccountingPeriodId",
                table: "erp_accounting_entries");

            migrationBuilder.DropColumn(
                name: "CreatedByUserId",
                table: "erp_accounting_entries");

            migrationBuilder.DropColumn(
                name: "EntryNumber",
                table: "erp_accounting_entries");

            migrationBuilder.DropColumn(
                name: "EntryType",
                table: "erp_accounting_entries");

            migrationBuilder.DropColumn(
                name: "ReversalId",
                table: "erp_accounting_entries");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "erp_accounting_entries");

            migrationBuilder.DropColumn(
                name: "UpdatedByUserId",
                table: "erp_accounting_entries");

            migrationBuilder.RenameColumn(
                name: "TotalDebit",
                table: "erp_accounting_entries",
                newName: "Debit");

            migrationBuilder.RenameColumn(
                name: "TotalCredit",
                table: "erp_accounting_entries",
                newName: "Credit");

            migrationBuilder.RenameColumn(
                name: "ExternalReference",
                table: "erp_accounting_entries",
                newName: "Reference");

            migrationBuilder.AddColumn<Guid>(
                name: "AccountingAccountId",
                table: "erp_accounting_entries",
                type: "char(36)",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                collation: "ascii_general_ci");

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "erp_accounting_entries",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateIndex(
                name: "IX_erp_accounting_entries_AccountingAccountId",
                table: "erp_accounting_entries",
                column: "AccountingAccountId");

            migrationBuilder.AddForeignKey(
                name: "FK_erp_accounting_entries_erp_accounting_accounts_AccountingAcc~",
                table: "erp_accounting_entries",
                column: "AccountingAccountId",
                principalTable: "erp_accounting_accounts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
