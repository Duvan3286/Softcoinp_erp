using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Softcoinp.ERP.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddLateInterestMoraModule : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ImputationType",
                table: "erp_payments",
                type: "varchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "ManualJustification",
                table: "erp_payments",
                type: "varchar(2000)",
                maxLength: 2000,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<Guid>(
                name: "AccruedInterestId",
                table: "erp_payment_allocations",
                type: "char(36)",
                nullable: true,
                collation: "ascii_general_ci");

            migrationBuilder.CreateTable(
                name: "erp_late_interest_configurations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    TenantId = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    InterestStartDays = table.Column<int>(type: "int", nullable: false),
                    ApplyToAllUnitsByDefault = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    AlertOnMissingMonthlyRate = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    CreatedByUserId = table.Column<string>(type: "varchar(450)", maxLength: 450, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_erp_late_interest_configurations", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "erp_monthly_interest_rates",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    TenantId = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Year = table.Column<int>(type: "int", nullable: false),
                    Month = table.Column<int>(type: "int", nullable: false),
                    CertifiedRate = table.Column<decimal>(type: "decimal(8,4)", precision: 8, scale: 4, nullable: false),
                    AppliedRate = table.Column<decimal>(type: "decimal(8,4)", precision: 8, scale: 4, nullable: false),
                    RegisteredAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    RegisteredByUserId = table.Column<string>(type: "varchar(450)", maxLength: 450, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_erp_monthly_interest_rates", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "erp_unit_interest_exceptions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    TenantId = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    UnitId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    InterestStartDays = table.Column<int>(type: "int", nullable: false),
                    Reason = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    CreatedByUserId = table.Column<string>(type: "varchar(450)", maxLength: 450, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_erp_unit_interest_exceptions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_erp_unit_interest_exceptions_erp_units_UnitId",
                        column: x => x.UnitId,
                        principalTable: "erp_units",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "erp_accrued_interests",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    TenantId = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    UnitId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    UnitFeeId = table.Column<Guid>(type: "char(36)", nullable: true, collation: "ascii_general_ci"),
                    ExtraordinaryFeeDistributionId = table.Column<Guid>(type: "char(36)", nullable: true, collation: "ascii_general_ci"),
                    IndividualChargeId = table.Column<Guid>(type: "char(36)", nullable: true, collation: "ascii_general_ci"),
                    Period = table.Column<string>(type: "varchar(7)", maxLength: 7, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    DailyRate = table.Column<decimal>(type: "decimal(14,10)", precision: 14, scale: 10, nullable: false),
                    DaysInPeriod = table.Column<int>(type: "int", nullable: false),
                    BaseAmount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    CalculatedAmount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Status = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    InterestStartDate = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    InterestEndDate = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    MonthlyInterestRateId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_erp_accrued_interests", x => x.Id);
                    table.ForeignKey(
                        name: "FK_erp_accrued_interests_erp_extraordinary_fee_distributions_Ex~",
                        column: x => x.ExtraordinaryFeeDistributionId,
                        principalTable: "erp_extraordinary_fee_distributions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_erp_accrued_interests_erp_individual_charges_IndividualCharg~",
                        column: x => x.IndividualChargeId,
                        principalTable: "erp_individual_charges",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_erp_accrued_interests_erp_monthly_interest_rates_MonthlyInte~",
                        column: x => x.MonthlyInterestRateId,
                        principalTable: "erp_monthly_interest_rates",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_erp_accrued_interests_erp_unit_fees_UnitFeeId",
                        column: x => x.UnitFeeId,
                        principalTable: "erp_unit_fees",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_erp_accrued_interests_erp_units_UnitId",
                        column: x => x.UnitId,
                        principalTable: "erp_units",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_erp_payments_TenantId_ImputationType",
                table: "erp_payments",
                columns: new[] { "TenantId", "ImputationType" });

            migrationBuilder.CreateIndex(
                name: "IX_erp_payment_allocations_AccruedInterestId_AllocationType",
                table: "erp_payment_allocations",
                columns: new[] { "AccruedInterestId", "AllocationType" });

            migrationBuilder.CreateIndex(
                name: "IX_erp_accrued_interests_ExtraordinaryFeeDistributionId",
                table: "erp_accrued_interests",
                column: "ExtraordinaryFeeDistributionId");

            migrationBuilder.CreateIndex(
                name: "IX_erp_accrued_interests_IndividualChargeId",
                table: "erp_accrued_interests",
                column: "IndividualChargeId");

            migrationBuilder.CreateIndex(
                name: "IX_erp_accrued_interests_MonthlyInterestRateId",
                table: "erp_accrued_interests",
                column: "MonthlyInterestRateId");

            migrationBuilder.CreateIndex(
                name: "IX_erp_accrued_interests_TenantId_Period_Status",
                table: "erp_accrued_interests",
                columns: new[] { "TenantId", "Period", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_erp_accrued_interests_TenantId_Status",
                table: "erp_accrued_interests",
                columns: new[] { "TenantId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_erp_accrued_interests_TenantId_UnitId_Period_UnitFeeId",
                table: "erp_accrued_interests",
                columns: new[] { "TenantId", "UnitId", "Period", "UnitFeeId" });

            migrationBuilder.CreateIndex(
                name: "IX_erp_accrued_interests_TenantId_UnitId_Status",
                table: "erp_accrued_interests",
                columns: new[] { "TenantId", "UnitId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_erp_accrued_interests_UnitFeeId",
                table: "erp_accrued_interests",
                column: "UnitFeeId");

            migrationBuilder.CreateIndex(
                name: "IX_erp_accrued_interests_UnitId",
                table: "erp_accrued_interests",
                column: "UnitId");

            migrationBuilder.CreateIndex(
                name: "IX_erp_late_interest_configurations_TenantId",
                table: "erp_late_interest_configurations",
                column: "TenantId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_erp_monthly_interest_rates_TenantId_Year_Month",
                table: "erp_monthly_interest_rates",
                columns: new[] { "TenantId", "Year", "Month" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_erp_monthly_interest_rates_TenantId_Year_Month_AppliedRate",
                table: "erp_monthly_interest_rates",
                columns: new[] { "TenantId", "Year", "Month", "AppliedRate" });

            migrationBuilder.CreateIndex(
                name: "IX_erp_unit_interest_exceptions_TenantId_UnitId",
                table: "erp_unit_interest_exceptions",
                columns: new[] { "TenantId", "UnitId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_erp_unit_interest_exceptions_UnitId",
                table: "erp_unit_interest_exceptions",
                column: "UnitId");

            migrationBuilder.AddForeignKey(
                name: "FK_erp_payment_allocations_erp_accrued_interests_AccruedInteres~",
                table: "erp_payment_allocations",
                column: "AccruedInterestId",
                principalTable: "erp_accrued_interests",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_erp_payment_allocations_erp_accrued_interests_AccruedInteres~",
                table: "erp_payment_allocations");

            migrationBuilder.DropTable(
                name: "erp_accrued_interests");

            migrationBuilder.DropTable(
                name: "erp_late_interest_configurations");

            migrationBuilder.DropTable(
                name: "erp_unit_interest_exceptions");

            migrationBuilder.DropTable(
                name: "erp_monthly_interest_rates");

            migrationBuilder.DropIndex(
                name: "IX_erp_payments_TenantId_ImputationType",
                table: "erp_payments");

            migrationBuilder.DropIndex(
                name: "IX_erp_payment_allocations_AccruedInterestId_AllocationType",
                table: "erp_payment_allocations");

            migrationBuilder.DropColumn(
                name: "ImputationType",
                table: "erp_payments");

            migrationBuilder.DropColumn(
                name: "ManualJustification",
                table: "erp_payments");

            migrationBuilder.DropColumn(
                name: "AccruedInterestId",
                table: "erp_payment_allocations");
        }
    }
}
