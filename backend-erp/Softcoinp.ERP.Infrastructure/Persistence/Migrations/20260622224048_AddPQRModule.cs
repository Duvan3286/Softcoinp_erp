using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Softcoinp.ERP.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddPQRModule : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "erp_pqr_records",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    TenantId = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    RadicadoNumber = table.Column<string>(type: "varchar(30)", maxLength: 30, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    PQRType = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Category = table.Column<string>(type: "varchar(30)", maxLength: 30, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Status = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Priority = table.Column<string>(type: "varchar(10)", maxLength: 10, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Subject = table.Column<string>(type: "varchar(300)", maxLength: 300, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Description = table.Column<string>(type: "varchar(4000)", maxLength: 4000, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    RadiadorName = table.Column<string>(type: "varchar(300)", maxLength: 300, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    RadiadorDocumentType = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    RadiadorDocumentNumber = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    RadiadorContact = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    OwnerId = table.Column<Guid>(type: "char(36)", nullable: true, collation: "ascii_general_ci"),
                    TenantResidentId = table.Column<Guid>(type: "char(36)", nullable: true, collation: "ascii_general_ci"),
                    UnitId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    Channel = table.Column<string>(type: "varchar(15)", maxLength: 15, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    RelatedPQRId = table.Column<Guid>(type: "char(36)", nullable: true, collation: "ascii_general_ci"),
                    AssignedToUserId = table.Column<string>(type: "varchar(450)", maxLength: 450, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Deadline = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    InvolvedResidentName = table.Column<string>(type: "varchar(300)", maxLength: 300, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    InvolvedResidentUnitId = table.Column<Guid>(type: "char(36)", nullable: true, collation: "ascii_general_ci"),
                    IsInternal = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    IsLinkedToCharge = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    UnitFeeId = table.Column<Guid>(type: "char(36)", nullable: true, collation: "ascii_general_ci"),
                    ExtraordinaryFeeDistributionId = table.Column<Guid>(type: "char(36)", nullable: true, collation: "ascii_general_ci"),
                    IndividualChargeId = table.Column<Guid>(type: "char(36)", nullable: true, collation: "ascii_general_ci"),
                    ClaimResolved = table.Column<bool>(type: "tinyint(1)", nullable: true),
                    ClaimResolutionNote = table.Column<string>(type: "varchar(2000)", maxLength: 2000, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CreditNoteGenerated = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    FiledAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    ClosedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    ClosedDefinitivelyAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    CreatedByUserId = table.Column<string>(type: "varchar(450)", maxLength: 450, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    UpdatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_erp_pqr_records", x => x.Id);
                    table.ForeignKey(
                        name: "FK_erp_pqr_records_erp_extraordinary_fee_distributions_Extraord~",
                        column: x => x.ExtraordinaryFeeDistributionId,
                        principalTable: "erp_extraordinary_fee_distributions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_erp_pqr_records_erp_individual_charges_IndividualChargeId",
                        column: x => x.IndividualChargeId,
                        principalTable: "erp_individual_charges",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_erp_pqr_records_erp_owners_OwnerId",
                        column: x => x.OwnerId,
                        principalTable: "erp_owners",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_erp_pqr_records_erp_pqr_records_RelatedPQRId",
                        column: x => x.RelatedPQRId,
                        principalTable: "erp_pqr_records",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_erp_pqr_records_erp_tenant_residents_TenantResidentId",
                        column: x => x.TenantResidentId,
                        principalTable: "erp_tenant_residents",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_erp_pqr_records_erp_unit_fees_UnitFeeId",
                        column: x => x.UnitFeeId,
                        principalTable: "erp_unit_fees",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_erp_pqr_records_erp_units_InvolvedResidentUnitId",
                        column: x => x.InvolvedResidentUnitId,
                        principalTable: "erp_units",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_erp_pqr_records_erp_units_UnitId",
                        column: x => x.UnitId,
                        principalTable: "erp_units",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "erp_pqr_time_configs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    TenantId = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    PQRType = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    BusinessDays = table.Column<int>(type: "int", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    UpdatedByUserId = table.Column<string>(type: "varchar(450)", maxLength: 450, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_erp_pqr_time_configs", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "erp_pqr_alerts",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    PQRId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    AlertType = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    GeneratedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    IsActive = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    ResolvedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    EscalatedToCouncil = table.Column<bool>(type: "tinyint(1)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_erp_pqr_alerts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_erp_pqr_alerts_erp_pqr_records_PQRId",
                        column: x => x.PQRId,
                        principalTable: "erp_pqr_records",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "erp_pqr_follow_ups",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    PQRId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    PreviousStatus = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    NewStatus = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ChangedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    ChangedByUserId = table.Column<string>(type: "varchar(450)", maxLength: 450, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ChangedByUserName = table.Column<string>(type: "varchar(300)", maxLength: 300, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Justification = table.Column<string>(type: "varchar(2000)", maxLength: 2000, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    IsAutomatic = table.Column<bool>(type: "tinyint(1)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_erp_pqr_follow_ups", x => x.Id);
                    table.ForeignKey(
                        name: "FK_erp_pqr_follow_ups_erp_pqr_records_PQRId",
                        column: x => x.PQRId,
                        principalTable: "erp_pqr_records",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "erp_pqr_internal_notes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    PQRId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    NoteText = table.Column<string>(type: "varchar(4000)", maxLength: 4000, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    AuthorName = table.Column<string>(type: "varchar(300)", maxLength: 300, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    CreatedByUserId = table.Column<string>(type: "varchar(450)", maxLength: 450, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_erp_pqr_internal_notes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_erp_pqr_internal_notes_erp_pqr_records_PQRId",
                        column: x => x.PQRId,
                        principalTable: "erp_pqr_records",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "erp_pqr_responses",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    PQRId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    ResponseText = table.Column<string>(type: "varchar(4000)", maxLength: 4000, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    IsDefinitive = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    IsPartialUpdate = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    SentAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    SentByUserId = table.Column<string>(type: "varchar(450)", maxLength: 450, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    SentByUserName = table.Column<string>(type: "varchar(300)", maxLength: 300, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    RequiresConfirmation = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    ConfirmedByRadiador = table.Column<bool>(type: "tinyint(1)", nullable: true),
                    ConfirmedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_erp_pqr_responses", x => x.Id);
                    table.ForeignKey(
                        name: "FK_erp_pqr_responses_erp_pqr_records_PQRId",
                        column: x => x.PQRId,
                        principalTable: "erp_pqr_records",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "erp_pqr_files",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    PQRId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    PqrResponseId = table.Column<Guid>(type: "char(36)", nullable: true, collation: "ascii_general_ci"),
                    PqrInternalNoteId = table.Column<Guid>(type: "char(36)", nullable: true, collation: "ascii_general_ci"),
                    FileName = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    OriginalFileName = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ContentType = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    FileSize = table.Column<long>(type: "bigint", nullable: false),
                    FilePath = table.Column<string>(type: "varchar(1000)", maxLength: 1000, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    UploadedByUserId = table.Column<string>(type: "varchar(450)", maxLength: 450, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    UploadedByUserName = table.Column<string>(type: "varchar(300)", maxLength: 300, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    UploadedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    IsFromApplicant = table.Column<bool>(type: "tinyint(1)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_erp_pqr_files", x => x.Id);
                    table.ForeignKey(
                        name: "FK_erp_pqr_files_erp_pqr_internal_notes_PqrInternalNoteId",
                        column: x => x.PqrInternalNoteId,
                        principalTable: "erp_pqr_internal_notes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_erp_pqr_files_erp_pqr_records_PQRId",
                        column: x => x.PQRId,
                        principalTable: "erp_pqr_records",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_erp_pqr_files_erp_pqr_responses_PqrResponseId",
                        column: x => x.PqrResponseId,
                        principalTable: "erp_pqr_responses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_erp_pqr_alerts_IsActive_GeneratedAt",
                table: "erp_pqr_alerts",
                columns: new[] { "IsActive", "GeneratedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_erp_pqr_alerts_PQRId_AlertType_IsActive",
                table: "erp_pqr_alerts",
                columns: new[] { "PQRId", "AlertType", "IsActive" });

            migrationBuilder.CreateIndex(
                name: "IX_erp_pqr_files_PQRId",
                table: "erp_pqr_files",
                column: "PQRId");

            migrationBuilder.CreateIndex(
                name: "IX_erp_pqr_files_PqrInternalNoteId",
                table: "erp_pqr_files",
                column: "PqrInternalNoteId");

            migrationBuilder.CreateIndex(
                name: "IX_erp_pqr_files_PqrResponseId",
                table: "erp_pqr_files",
                column: "PqrResponseId");

            migrationBuilder.CreateIndex(
                name: "IX_erp_pqr_follow_ups_PQRId_ChangedAt",
                table: "erp_pqr_follow_ups",
                columns: new[] { "PQRId", "ChangedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_erp_pqr_internal_notes_PQRId",
                table: "erp_pqr_internal_notes",
                column: "PQRId");

            migrationBuilder.CreateIndex(
                name: "IX_erp_pqr_records_ExtraordinaryFeeDistributionId",
                table: "erp_pqr_records",
                column: "ExtraordinaryFeeDistributionId");

            migrationBuilder.CreateIndex(
                name: "IX_erp_pqr_records_FiledAt",
                table: "erp_pqr_records",
                column: "FiledAt");

            migrationBuilder.CreateIndex(
                name: "IX_erp_pqr_records_IndividualChargeId",
                table: "erp_pqr_records",
                column: "IndividualChargeId");

            migrationBuilder.CreateIndex(
                name: "IX_erp_pqr_records_InvolvedResidentUnitId",
                table: "erp_pqr_records",
                column: "InvolvedResidentUnitId");

            migrationBuilder.CreateIndex(
                name: "IX_erp_pqr_records_OwnerId",
                table: "erp_pqr_records",
                column: "OwnerId");

            migrationBuilder.CreateIndex(
                name: "IX_erp_pqr_records_RelatedPQRId",
                table: "erp_pqr_records",
                column: "RelatedPQRId");

            migrationBuilder.CreateIndex(
                name: "IX_erp_pqr_records_TenantId_PQRType_Status",
                table: "erp_pqr_records",
                columns: new[] { "TenantId", "PQRType", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_erp_pqr_records_TenantId_RadicadoNumber",
                table: "erp_pqr_records",
                columns: new[] { "TenantId", "RadicadoNumber" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_erp_pqr_records_TenantId_Status_Priority_Deadline",
                table: "erp_pqr_records",
                columns: new[] { "TenantId", "Status", "Priority", "Deadline" });

            migrationBuilder.CreateIndex(
                name: "IX_erp_pqr_records_TenantId_UnitId_Status",
                table: "erp_pqr_records",
                columns: new[] { "TenantId", "UnitId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_erp_pqr_records_TenantResidentId",
                table: "erp_pqr_records",
                column: "TenantResidentId");

            migrationBuilder.CreateIndex(
                name: "IX_erp_pqr_records_UnitFeeId",
                table: "erp_pqr_records",
                column: "UnitFeeId");

            migrationBuilder.CreateIndex(
                name: "IX_erp_pqr_records_UnitId",
                table: "erp_pqr_records",
                column: "UnitId");

            migrationBuilder.CreateIndex(
                name: "IX_erp_pqr_responses_PQRId_SentAt",
                table: "erp_pqr_responses",
                columns: new[] { "PQRId", "SentAt" });

            migrationBuilder.CreateIndex(
                name: "IX_erp_pqr_time_configs_TenantId_PQRType",
                table: "erp_pqr_time_configs",
                columns: new[] { "TenantId", "PQRType" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "erp_pqr_alerts");

            migrationBuilder.DropTable(
                name: "erp_pqr_files");

            migrationBuilder.DropTable(
                name: "erp_pqr_follow_ups");

            migrationBuilder.DropTable(
                name: "erp_pqr_time_configs");

            migrationBuilder.DropTable(
                name: "erp_pqr_internal_notes");

            migrationBuilder.DropTable(
                name: "erp_pqr_responses");

            migrationBuilder.DropTable(
                name: "erp_pqr_records");
        }
    }
}
