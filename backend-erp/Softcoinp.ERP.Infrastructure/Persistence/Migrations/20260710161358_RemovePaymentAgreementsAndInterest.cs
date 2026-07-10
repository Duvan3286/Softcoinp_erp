using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Softcoinp.ERP.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class RemovePaymentAgreementsAndInterest : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_erp_payment_allocations_erp_late_interests_LateInterestId",
                table: "erp_payment_allocations");

            migrationBuilder.DropTable(
                name: "erp_agreement_debts");

            migrationBuilder.DropTable(
                name: "erp_agreement_installments");

            migrationBuilder.DropTable(
                name: "erp_late_interests");

            migrationBuilder.DropTable(
                name: "erp_payment_agreements");

            migrationBuilder.DropIndex(
                name: "IX_payment_alloc_late_interest",
                table: "erp_payment_allocations");

            migrationBuilder.DropColumn(
                name: "LatePaymentInterestRate",
                table: "erp_tenant_configuration");

            migrationBuilder.DropColumn(
                name: "MaxLegalInterestRate",
                table: "erp_tenant_configuration");

            migrationBuilder.DropColumn(
                name: "LateInterestId",
                table: "erp_payment_allocations");

            migrationBuilder.CreateTable(
                name: "erp_billing_adjustments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    TenantId = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    UnitId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    BillingPeriodId = table.Column<Guid>(type: "char(36)", nullable: true, collation: "ascii_general_ci"),
                    UnitFeeId = table.Column<Guid>(type: "char(36)", nullable: true, collation: "ascii_general_ci"),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Reason = table.Column<string>(type: "varchar(1000)", maxLength: 1000, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    CreatedByUserId = table.Column<string>(type: "varchar(450)", maxLength: 450, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_erp_billing_adjustments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_erp_billing_adjustments_erp_billing_periods_BillingPeriodId",
                        column: x => x.BillingPeriodId,
                        principalTable: "erp_billing_periods",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_erp_billing_adjustments_erp_unit_fees_UnitFeeId",
                        column: x => x.UnitFeeId,
                        principalTable: "erp_unit_fees",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_erp_billing_adjustments_erp_units_UnitId",
                        column: x => x.UnitId,
                        principalTable: "erp_units",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_erp_billing_adjustments_BillingPeriodId",
                table: "erp_billing_adjustments",
                column: "BillingPeriodId");

            migrationBuilder.CreateIndex(
                name: "IX_erp_billing_adjustments_TenantId_UnitId_CreatedAt",
                table: "erp_billing_adjustments",
                columns: new[] { "TenantId", "UnitId", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_erp_billing_adjustments_UnitFeeId",
                table: "erp_billing_adjustments",
                column: "UnitFeeId");

            migrationBuilder.CreateIndex(
                name: "IX_erp_billing_adjustments_UnitId",
                table: "erp_billing_adjustments",
                column: "UnitId");

            // Data cleanup: the "PaymentAgreements" report type was seeded for existing
            // tenants before this module removed payment agreements. It has no generated
            // reports or recurring configs pointing to it (verified before writing this
            // migration), so it is safe to delete outright instead of leaving it orphaned.
            migrationBuilder.Sql("DELETE FROM erp_pdf_templates WHERE ReportTypeCode = 'PaymentAgreements';");
            migrationBuilder.Sql("DELETE FROM erp_report_types WHERE ReportTypeCode = 'PaymentAgreements';");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "erp_billing_adjustments");

            migrationBuilder.AddColumn<decimal>(
                name: "LatePaymentInterestRate",
                table: "erp_tenant_configuration",
                type: "decimal(5,2)",
                precision: 5,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "MaxLegalInterestRate",
                table: "erp_tenant_configuration",
                type: "decimal(5,2)",
                precision: 5,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<Guid>(
                name: "LateInterestId",
                table: "erp_payment_allocations",
                type: "char(36)",
                nullable: true,
                collation: "ascii_general_ci");

            migrationBuilder.CreateTable(
                name: "erp_late_interests",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    ExtraordinaryFeeDistributionId = table.Column<Guid>(type: "char(36)", nullable: true, collation: "ascii_general_ci"),
                    IndividualChargeId = table.Column<Guid>(type: "char(36)", nullable: true, collation: "ascii_general_ci"),
                    UnitFeeId = table.Column<Guid>(type: "char(36)", nullable: true, collation: "ascii_general_ci"),
                    BaseAmount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    CalculatedAmount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    DailyRate = table.Column<decimal>(type: "decimal(12,8)", precision: 12, scale: 8, nullable: false),
                    DaysOverdue = table.Column<int>(type: "int", nullable: false),
                    IsCapitalized = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    Period = table.Column<string>(type: "varchar(7)", maxLength: 7, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    TenantId = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    UpdatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_erp_late_interests", x => x.Id);
                    table.ForeignKey(
                        name: "FK_erp_late_interests_erp_extraordinary_fee_distributions_Extra~",
                        column: x => x.ExtraordinaryFeeDistributionId,
                        principalTable: "erp_extraordinary_fee_distributions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_erp_late_interests_erp_individual_charges_IndividualChargeId",
                        column: x => x.IndividualChargeId,
                        principalTable: "erp_individual_charges",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_erp_late_interests_erp_unit_fees_UnitFeeId",
                        column: x => x.UnitFeeId,
                        principalTable: "erp_unit_fees",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "erp_payment_agreements",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    UnitId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    CouncilActNumber = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    CreatedByUserId = table.Column<string>(type: "varchar(450)", maxLength: 450, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    DefaultedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    DigitalAcceptance = table.Column<string>(type: "varchar(2000)", maxLength: 2000, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    InstallmentAmount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    InterestForgivenessPercentage = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: false),
                    NumberOfInstallments = table.Column<int>(type: "int", nullable: false),
                    StartedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    Status = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    TenantId = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    TotalDebtIncluded = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_erp_payment_agreements", x => x.Id);
                    table.ForeignKey(
                        name: "FK_erp_payment_agreements_erp_units_UnitId",
                        column: x => x.UnitId,
                        principalTable: "erp_units",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "erp_agreement_debts",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    PaymentAgreementId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    OriginalBalance = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    SourceId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    SourceType = table.Column<string>(type: "varchar(30)", maxLength: 30, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    TenantId = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_erp_agreement_debts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_erp_agreement_debts_erp_payment_agreements_PaymentAgreementId",
                        column: x => x.PaymentAgreementId,
                        principalTable: "erp_payment_agreements",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "erp_agreement_installments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    PaymentAgreementId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    DueDate = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    InstallmentNumber = table.Column<int>(type: "int", nullable: false),
                    PaidAmount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    PaidAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    Status = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    TenantId = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    UpdatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_erp_agreement_installments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_erp_agreement_installments_erp_payment_agreements_PaymentAgr~",
                        column: x => x.PaymentAgreementId,
                        principalTable: "erp_payment_agreements",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_payment_alloc_late_interest",
                table: "erp_payment_allocations",
                column: "LateInterestId");

            migrationBuilder.CreateIndex(
                name: "IX_erp_agreement_debts_PaymentAgreementId_SourceType_SourceId",
                table: "erp_agreement_debts",
                columns: new[] { "PaymentAgreementId", "SourceType", "SourceId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_erp_agreement_debts_TenantId_SourceType_SourceId",
                table: "erp_agreement_debts",
                columns: new[] { "TenantId", "SourceType", "SourceId" });

            migrationBuilder.CreateIndex(
                name: "IX_agreement_installments_overdue",
                table: "erp_agreement_installments",
                columns: new[] { "TenantId", "Status", "DueDate", "Amount" });

            migrationBuilder.CreateIndex(
                name: "IX_erp_agreement_installments_PaymentAgreementId_InstallmentNum~",
                table: "erp_agreement_installments",
                columns: new[] { "PaymentAgreementId", "InstallmentNumber" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_erp_agreement_installments_TenantId_Status_DueDate",
                table: "erp_agreement_installments",
                columns: new[] { "TenantId", "Status", "DueDate" });

            migrationBuilder.CreateIndex(
                name: "IX_erp_late_interests_ExtraordinaryFeeDistributionId",
                table: "erp_late_interests",
                column: "ExtraordinaryFeeDistributionId");

            migrationBuilder.CreateIndex(
                name: "IX_erp_late_interests_IndividualChargeId",
                table: "erp_late_interests",
                column: "IndividualChargeId");

            migrationBuilder.CreateIndex(
                name: "IX_erp_late_interests_TenantId_IsCapitalized",
                table: "erp_late_interests",
                columns: new[] { "TenantId", "IsCapitalized" });

            migrationBuilder.CreateIndex(
                name: "IX_erp_late_interests_TenantId_UnitFeeId_Period",
                table: "erp_late_interests",
                columns: new[] { "TenantId", "UnitFeeId", "Period" });

            migrationBuilder.CreateIndex(
                name: "IX_erp_late_interests_UnitFeeId",
                table: "erp_late_interests",
                column: "UnitFeeId");

            migrationBuilder.CreateIndex(
                name: "IX_late_interests_fee_cap_amount",
                table: "erp_late_interests",
                columns: new[] { "TenantId", "UnitFeeId", "IsCapitalized", "CalculatedAmount" });

            migrationBuilder.CreateIndex(
                name: "IX_erp_payment_agreements_TenantId_UnitId_Status",
                table: "erp_payment_agreements",
                columns: new[] { "TenantId", "UnitId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_erp_payment_agreements_UnitId",
                table: "erp_payment_agreements",
                column: "UnitId");

            migrationBuilder.AddForeignKey(
                name: "FK_erp_payment_allocations_erp_late_interests_LateInterestId",
                table: "erp_payment_allocations",
                column: "LateInterestId",
                principalTable: "erp_late_interests",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
