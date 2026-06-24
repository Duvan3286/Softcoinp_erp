using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Softcoinp.ERP.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddReservationsModule : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "erp_reservable_spaces",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    TenantId = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Name = table.Column<string>(type: "varchar(300)", maxLength: 300, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Description = table.Column<string>(type: "varchar(2000)", maxLength: 2000, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Location = table.Column<string>(type: "varchar(300)", maxLength: 300, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    MaxCapacity = table.Column<int>(type: "int", nullable: false),
                    MinReservationHours = table.Column<int>(type: "int", nullable: false),
                    MaxReservationHours = table.Column<int>(type: "int", nullable: false),
                    MinAdvanceHours = table.Column<int>(type: "int", nullable: false),
                    MaxAdvanceDays = table.Column<int>(type: "int", nullable: false),
                    MaxSimultaneousReservationsPerUnit = table.Column<int>(type: "int", nullable: false),
                    RequiresDeposit = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    DepositAmount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    HasAdditionalCost = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    ChargeType = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    HourlyRate = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    EventRate = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    ApprovalMode = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ArrearsPolicy = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    IsAvailableForMaintenance = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    IsActive = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    RulesFilePath = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ImageFilePath = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CreatedByUserId = table.Column<string>(type: "varchar(450)", maxLength: 450, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    UpdatedByUserId = table.Column<string>(type: "varchar(450)", maxLength: 450, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    IsDeleted = table.Column<bool>(type: "tinyint(1)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_erp_reservable_spaces", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "erp_reservations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    TenantId = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ReservationNumber = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    SpaceId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    UnitId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    OwnerId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    StartDateTime = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    EndDateTime = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    EstimatedAttendees = table.Column<int>(type: "int", nullable: false),
                    EventDescription = table.Column<string>(type: "varchar(2000)", maxLength: 2000, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    HasMusic = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    MusicEndTime = table.Column<string>(type: "varchar(10)", maxLength: 10, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    RulesAccepted = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    Status = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    RejectionReason = table.Column<string>(type: "varchar(1000)", maxLength: 1000, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    TotalCost = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    DepositStatus = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    DepositAmount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    DepositChargeId = table.Column<Guid>(type: "char(36)", nullable: true, collation: "ascii_general_ci"),
                    DepositReturnChargeId = table.Column<Guid>(type: "char(36)", nullable: true, collation: "ascii_general_ci"),
                    AdminNotes = table.Column<string>(type: "varchar(2000)", maxLength: 2000, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    AdminUserId = table.Column<string>(type: "varchar(450)", maxLength: 450, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CheckedInAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    CheckedOutAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    CheckoutSignaturePath = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ExceptionGranted = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    ExceptionReason = table.Column<string>(type: "varchar(1000)", maxLength: 1000, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ExceptionGrantedByUserId = table.Column<string>(type: "varchar(450)", maxLength: 450, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    CreatedByUserId = table.Column<string>(type: "varchar(450)", maxLength: 450, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    UpdatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    UpdatedByUserId = table.Column<string>(type: "varchar(450)", maxLength: 450, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_erp_reservations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_erp_reservations_erp_owners_OwnerId",
                        column: x => x.OwnerId,
                        principalTable: "erp_owners",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_erp_reservations_erp_reservable_spaces_SpaceId",
                        column: x => x.SpaceId,
                        principalTable: "erp_reservable_spaces",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_erp_reservations_erp_units_UnitId",
                        column: x => x.UnitId,
                        principalTable: "erp_units",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "erp_space_blocks",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    TenantId = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    SpaceId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    StartDate = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    EndDate = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    StartTime = table.Column<string>(type: "varchar(10)", maxLength: 10, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    EndTime = table.Column<string>(type: "varchar(10)", maxLength: 10, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Origin = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Reason = table.Column<string>(type: "varchar(1000)", maxLength: 1000, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    RelatedWorkOrderId = table.Column<Guid>(type: "char(36)", nullable: true, collation: "ascii_general_ci"),
                    RelatedWorkOrderNumber = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    NotifyAffectedResidents = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    NotificationSent = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    CreatedByUserId = table.Column<string>(type: "varchar(450)", maxLength: 450, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_erp_space_blocks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_erp_space_blocks_erp_reservable_spaces_SpaceId",
                        column: x => x.SpaceId,
                        principalTable: "erp_reservable_spaces",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "erp_space_schedules",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    TenantId = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    SpaceId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    DayOfWeek = table.Column<int>(type: "int", nullable: false),
                    StartTime = table.Column<string>(type: "varchar(10)", maxLength: 10, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    EndTime = table.Column<string>(type: "varchar(10)", maxLength: 10, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    IsActive = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_erp_space_schedules", x => x.Id);
                    table.ForeignKey(
                        name: "FK_erp_space_schedules_erp_reservable_spaces_SpaceId",
                        column: x => x.SpaceId,
                        principalTable: "erp_reservable_spaces",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "erp_reservation_deposits",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    TenantId = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ReservationId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Status = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    PaymentMethod = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ChargeId = table.Column<Guid>(type: "char(36)", nullable: true, collation: "ascii_general_ci"),
                    ChargeNumber = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ReturnChargeId = table.Column<Guid>(type: "char(36)", nullable: true, collation: "ascii_general_ci"),
                    ReturnChargeNumber = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    DamageAmount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    DamageDescription = table.Column<string>(type: "varchar(2000)", maxLength: 2000, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    PaidAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    ReturnedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    AppliedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    ProcessedByUserId = table.Column<string>(type: "varchar(450)", maxLength: 450, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Notes = table.Column<string>(type: "varchar(2000)", maxLength: 2000, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_erp_reservation_deposits", x => x.Id);
                    table.ForeignKey(
                        name: "FK_erp_reservation_deposits_erp_reservations_ReservationId",
                        column: x => x.ReservationId,
                        principalTable: "erp_reservations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "erp_reservation_incidents",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    TenantId = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ReservationId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    Description = table.Column<string>(type: "varchar(4000)", maxLength: 4000, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Severity = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    DamageAmount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    DamageAssessed = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    DepositAppliedToDamage = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    EvidenceFilePath = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ReportedByUserId = table.Column<string>(type: "varchar(450)", maxLength: 450, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ReportedByName = table.Column<string>(type: "varchar(300)", maxLength: 300, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_erp_reservation_incidents", x => x.Id);
                    table.ForeignKey(
                        name: "FK_erp_reservation_incidents_erp_reservations_ReservationId",
                        column: x => x.ReservationId,
                        principalTable: "erp_reservations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "erp_reservation_reminders",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    TenantId = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ReservationId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    ReminderType = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Status = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ScheduledFor = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    SentAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    Channel = table.Column<string>(type: "varchar(30)", maxLength: 30, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    RecipientEmail = table.Column<string>(type: "varchar(300)", maxLength: 300, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    RecipientPhone = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ErrorMessage = table.Column<string>(type: "varchar(1000)", maxLength: 1000, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    RetryCount = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_erp_reservation_reminders", x => x.Id);
                    table.ForeignKey(
                        name: "FK_erp_reservation_reminders_erp_reservations_ReservationId",
                        column: x => x.ReservationId,
                        principalTable: "erp_reservations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_erp_reservable_spaces_TenantId_IsActive",
                table: "erp_reservable_spaces",
                columns: new[] { "TenantId", "IsActive" });

            migrationBuilder.CreateIndex(
                name: "IX_erp_reservable_spaces_TenantId_Name",
                table: "erp_reservable_spaces",
                columns: new[] { "TenantId", "Name" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_erp_reservation_deposits_ReservationId",
                table: "erp_reservation_deposits",
                column: "ReservationId");

            migrationBuilder.CreateIndex(
                name: "IX_erp_reservation_deposits_TenantId_ReservationId",
                table: "erp_reservation_deposits",
                columns: new[] { "TenantId", "ReservationId" });

            migrationBuilder.CreateIndex(
                name: "IX_erp_reservation_deposits_TenantId_Status",
                table: "erp_reservation_deposits",
                columns: new[] { "TenantId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_erp_reservation_incidents_ReservationId",
                table: "erp_reservation_incidents",
                column: "ReservationId");

            migrationBuilder.CreateIndex(
                name: "IX_erp_reservation_incidents_TenantId_ReservationId",
                table: "erp_reservation_incidents",
                columns: new[] { "TenantId", "ReservationId" });

            migrationBuilder.CreateIndex(
                name: "IX_erp_reservation_reminders_ReservationId",
                table: "erp_reservation_reminders",
                column: "ReservationId");

            migrationBuilder.CreateIndex(
                name: "IX_erp_reservation_reminders_TenantId_ReservationId",
                table: "erp_reservation_reminders",
                columns: new[] { "TenantId", "ReservationId" });

            migrationBuilder.CreateIndex(
                name: "IX_erp_reservation_reminders_TenantId_Status_ScheduledFor",
                table: "erp_reservation_reminders",
                columns: new[] { "TenantId", "Status", "ScheduledFor" });

            migrationBuilder.CreateIndex(
                name: "IX_erp_reservations_OwnerId",
                table: "erp_reservations",
                column: "OwnerId");

            migrationBuilder.CreateIndex(
                name: "IX_erp_reservations_SpaceId",
                table: "erp_reservations",
                column: "SpaceId");

            migrationBuilder.CreateIndex(
                name: "IX_erp_reservations_TenantId_ReservationNumber",
                table: "erp_reservations",
                columns: new[] { "TenantId", "ReservationNumber" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_erp_reservations_TenantId_SpaceId_StartDateTime_EndDateTime",
                table: "erp_reservations",
                columns: new[] { "TenantId", "SpaceId", "StartDateTime", "EndDateTime" });

            migrationBuilder.CreateIndex(
                name: "IX_erp_reservations_TenantId_Status",
                table: "erp_reservations",
                columns: new[] { "TenantId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_erp_reservations_TenantId_UnitId_Status",
                table: "erp_reservations",
                columns: new[] { "TenantId", "UnitId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_erp_reservations_UnitId",
                table: "erp_reservations",
                column: "UnitId");

            migrationBuilder.CreateIndex(
                name: "IX_erp_space_blocks_SpaceId",
                table: "erp_space_blocks",
                column: "SpaceId");

            migrationBuilder.CreateIndex(
                name: "IX_erp_space_blocks_TenantId_SpaceId",
                table: "erp_space_blocks",
                columns: new[] { "TenantId", "SpaceId" });

            migrationBuilder.CreateIndex(
                name: "IX_erp_space_blocks_TenantId_SpaceId_StartDate_EndDate",
                table: "erp_space_blocks",
                columns: new[] { "TenantId", "SpaceId", "StartDate", "EndDate" });

            migrationBuilder.CreateIndex(
                name: "IX_erp_space_schedules_SpaceId",
                table: "erp_space_schedules",
                column: "SpaceId");

            migrationBuilder.CreateIndex(
                name: "IX_erp_space_schedules_TenantId_SpaceId",
                table: "erp_space_schedules",
                columns: new[] { "TenantId", "SpaceId" });

            migrationBuilder.CreateIndex(
                name: "IX_erp_space_schedules_TenantId_SpaceId_DayOfWeek",
                table: "erp_space_schedules",
                columns: new[] { "TenantId", "SpaceId", "DayOfWeek" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "erp_reservation_deposits");

            migrationBuilder.DropTable(
                name: "erp_reservation_incidents");

            migrationBuilder.DropTable(
                name: "erp_reservation_reminders");

            migrationBuilder.DropTable(
                name: "erp_space_blocks");

            migrationBuilder.DropTable(
                name: "erp_space_schedules");

            migrationBuilder.DropTable(
                name: "erp_reservations");

            migrationBuilder.DropTable(
                name: "erp_reservable_spaces");
        }
    }
}
