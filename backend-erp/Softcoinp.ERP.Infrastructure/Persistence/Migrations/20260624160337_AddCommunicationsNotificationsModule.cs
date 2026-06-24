using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Softcoinp.ERP.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddCommunicationsNotificationsModule : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "erp_bulletin_board_posts",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    TenantId = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Title = table.Column<string>(type: "varchar(300)", maxLength: 300, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Content = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    PublishedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    ExpiresAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    IsPinned = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    Category = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false)
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
                    table.PrimaryKey("PK_erp_bulletin_board_posts", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "erp_communication_preferences",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    TenantId = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    OwnerId = table.Column<Guid>(type: "char(36)", nullable: true, collation: "ascii_general_ci"),
                    TenantResidentId = table.Column<Guid>(type: "char(36)", nullable: true, collation: "ascii_general_ci"),
                    AllowEmail = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    AllowSms = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    AllowPush = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    CriticalNotificationsOverride = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    UnsubscribedEventTypes = table.Column<string>(type: "varchar(2000)", maxLength: 2000, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Notes = table.Column<string>(type: "varchar(2000)", maxLength: 2000, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ChangedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    ChangedByUserId = table.Column<string>(type: "varchar(450)", maxLength: 450, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_erp_communication_preferences", x => x.Id);
                    table.ForeignKey(
                        name: "FK_erp_communication_preferences_erp_owners_OwnerId",
                        column: x => x.OwnerId,
                        principalTable: "erp_owners",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_erp_communication_preferences_erp_tenant_residents_TenantRes~",
                        column: x => x.TenantResidentId,
                        principalTable: "erp_tenant_residents",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "erp_communications",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    TenantId = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Subject = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Body = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Status = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    AudienceType = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    SelectedChannels = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    SendAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    SentAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    RequiresReadConfirmation = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    PublishToBulletinBoard = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    RelatedCommunicationId = table.Column<Guid>(type: "char(36)", nullable: true, collation: "ascii_general_ci"),
                    FilePaths = table.Column<string>(type: "varchar(2000)", maxLength: 2000, nullable: false)
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
                    table.PrimaryKey("PK_erp_communications", x => x.Id);
                    table.ForeignKey(
                        name: "FK_erp_communications_erp_communications_RelatedCommunicationId",
                        column: x => x.RelatedCommunicationId,
                        principalTable: "erp_communications",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "erp_delinquency_sequence_pauses",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    TenantId = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    UnitId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    StartDate = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    EndDate = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    Reason = table.Column<string>(type: "varchar(1000)", maxLength: 1000, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    CreatedByUserId = table.Column<string>(type: "varchar(450)", maxLength: 450, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_erp_delinquency_sequence_pauses", x => x.Id);
                    table.ForeignKey(
                        name: "FK_erp_delinquency_sequence_pauses_erp_units_UnitId",
                        column: x => x.UnitId,
                        principalTable: "erp_units",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "erp_notification_templates",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    TenantId = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Name = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    EventType = table.Column<string>(type: "varchar(40)", maxLength: 40, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ForRecipientType = table.Column<string>(type: "varchar(10)", maxLength: 10, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    EmailSubject = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    EmailBody = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    SmsBody = table.Column<string>(type: "varchar(160)", maxLength: 160, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    DynamicVariables = table.Column<string>(type: "varchar(2000)", maxLength: 2000, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    IsActive = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    CreatedByUserId = table.Column<string>(type: "varchar(450)", maxLength: 450, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_erp_notification_templates", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "erp_automatic_notifications",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    TenantId = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    EventType = table.Column<string>(type: "varchar(40)", maxLength: 40, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CommunicationId = table.Column<Guid>(type: "char(36)", nullable: true, collation: "ascii_general_ci"),
                    OwnerId = table.Column<Guid>(type: "char(36)", nullable: true, collation: "ascii_general_ci"),
                    TenantResidentId = table.Column<Guid>(type: "char(36)", nullable: true, collation: "ascii_general_ci"),
                    RecipientEmail = table.Column<string>(type: "varchar(300)", maxLength: 300, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    RecipientPhone = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Channel = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Status = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    SentAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    ReadAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    SourceModule = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    SourceEntityId = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    SourceEntityType = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ErrorMessage = table.Column<string>(type: "varchar(1000)", maxLength: 1000, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_erp_automatic_notifications", x => x.Id);
                    table.ForeignKey(
                        name: "FK_erp_automatic_notifications_erp_communications_Communication~",
                        column: x => x.CommunicationId,
                        principalTable: "erp_communications",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_erp_automatic_notifications_erp_owners_OwnerId",
                        column: x => x.OwnerId,
                        principalTable: "erp_owners",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_erp_automatic_notifications_erp_tenant_residents_TenantResid~",
                        column: x => x.TenantResidentId,
                        principalTable: "erp_tenant_residents",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "erp_communication_recipients",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    TenantId = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CommunicationId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    OwnerId = table.Column<Guid>(type: "char(36)", nullable: true, collation: "ascii_general_ci"),
                    TenantResidentId = table.Column<Guid>(type: "char(36)", nullable: true, collation: "ascii_general_ci"),
                    RecipientEmail = table.Column<string>(type: "varchar(300)", maxLength: 300, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    RecipientPhone = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    EmailStatus = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    SmsStatus = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    PushStatus = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    BulletinBoardStatus = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    EmailSentAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    SmsSentAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    PushSentAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    ReadConfirmedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    ResentCount = table.Column<int>(type: "int", nullable: false),
                    LastResentAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    ErrorMessage = table.Column<string>(type: "varchar(1000)", maxLength: 1000, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_erp_communication_recipients", x => x.Id);
                    table.ForeignKey(
                        name: "FK_erp_communication_recipients_erp_communications_Communicatio~",
                        column: x => x.CommunicationId,
                        principalTable: "erp_communications",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_erp_communication_recipients_erp_owners_OwnerId",
                        column: x => x.OwnerId,
                        principalTable: "erp_owners",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_erp_communication_recipients_erp_tenant_residents_TenantResi~",
                        column: x => x.TenantResidentId,
                        principalTable: "erp_tenant_residents",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "erp_delinquency_sequence_configs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    TenantId = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    StepNumber = table.Column<int>(type: "int", nullable: false),
                    DaysAfterDue = table.Column<int>(type: "int", nullable: false),
                    TemplateId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    IsActive = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_erp_delinquency_sequence_configs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_erp_delinquency_sequence_configs_erp_notification_templates_~",
                        column: x => x.TemplateId,
                        principalTable: "erp_notification_templates",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_erp_automatic_notifications_CommunicationId",
                table: "erp_automatic_notifications",
                column: "CommunicationId");

            migrationBuilder.CreateIndex(
                name: "IX_erp_automatic_notifications_OwnerId",
                table: "erp_automatic_notifications",
                column: "OwnerId");

            migrationBuilder.CreateIndex(
                name: "IX_erp_automatic_notifications_TenantId_CreatedAt",
                table: "erp_automatic_notifications",
                columns: new[] { "TenantId", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_erp_automatic_notifications_TenantId_EventType",
                table: "erp_automatic_notifications",
                columns: new[] { "TenantId", "EventType" });

            migrationBuilder.CreateIndex(
                name: "IX_erp_automatic_notifications_TenantId_SourceModule_SourceEnti~",
                table: "erp_automatic_notifications",
                columns: new[] { "TenantId", "SourceModule", "SourceEntityId" });

            migrationBuilder.CreateIndex(
                name: "IX_erp_automatic_notifications_TenantId_Status",
                table: "erp_automatic_notifications",
                columns: new[] { "TenantId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_erp_automatic_notifications_TenantResidentId",
                table: "erp_automatic_notifications",
                column: "TenantResidentId");

            migrationBuilder.CreateIndex(
                name: "IX_erp_bulletin_board_posts_TenantId_Category",
                table: "erp_bulletin_board_posts",
                columns: new[] { "TenantId", "Category" });

            migrationBuilder.CreateIndex(
                name: "IX_erp_bulletin_board_posts_TenantId_ExpiresAt",
                table: "erp_bulletin_board_posts",
                columns: new[] { "TenantId", "ExpiresAt" });

            migrationBuilder.CreateIndex(
                name: "IX_erp_bulletin_board_posts_TenantId_IsPinned_PublishedAt",
                table: "erp_bulletin_board_posts",
                columns: new[] { "TenantId", "IsPinned", "PublishedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_erp_communication_preferences_OwnerId",
                table: "erp_communication_preferences",
                column: "OwnerId");

            migrationBuilder.CreateIndex(
                name: "IX_erp_communication_preferences_TenantId_OwnerId",
                table: "erp_communication_preferences",
                columns: new[] { "TenantId", "OwnerId" },
                unique: true,
                filter: "[OwnerId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_erp_communication_preferences_TenantId_TenantResidentId",
                table: "erp_communication_preferences",
                columns: new[] { "TenantId", "TenantResidentId" },
                unique: true,
                filter: "[TenantResidentId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_erp_communication_preferences_TenantResidentId",
                table: "erp_communication_preferences",
                column: "TenantResidentId");

            migrationBuilder.CreateIndex(
                name: "IX_erp_communication_recipients_CommunicationId",
                table: "erp_communication_recipients",
                column: "CommunicationId");

            migrationBuilder.CreateIndex(
                name: "IX_erp_communication_recipients_OwnerId",
                table: "erp_communication_recipients",
                column: "OwnerId");

            migrationBuilder.CreateIndex(
                name: "IX_erp_communication_recipients_TenantId_CommunicationId",
                table: "erp_communication_recipients",
                columns: new[] { "TenantId", "CommunicationId" });

            migrationBuilder.CreateIndex(
                name: "IX_erp_communication_recipients_TenantId_EmailStatus",
                table: "erp_communication_recipients",
                columns: new[] { "TenantId", "EmailStatus" });

            migrationBuilder.CreateIndex(
                name: "IX_erp_communication_recipients_TenantId_ReadConfirmedAt",
                table: "erp_communication_recipients",
                columns: new[] { "TenantId", "ReadConfirmedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_erp_communication_recipients_TenantResidentId",
                table: "erp_communication_recipients",
                column: "TenantResidentId");

            migrationBuilder.CreateIndex(
                name: "IX_erp_communications_RelatedCommunicationId",
                table: "erp_communications",
                column: "RelatedCommunicationId");

            migrationBuilder.CreateIndex(
                name: "IX_erp_communications_TenantId_CreatedAt",
                table: "erp_communications",
                columns: new[] { "TenantId", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_erp_communications_TenantId_SendAt",
                table: "erp_communications",
                columns: new[] { "TenantId", "SendAt" });

            migrationBuilder.CreateIndex(
                name: "IX_erp_communications_TenantId_Status",
                table: "erp_communications",
                columns: new[] { "TenantId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_erp_delinquency_sequence_configs_TemplateId",
                table: "erp_delinquency_sequence_configs",
                column: "TemplateId");

            migrationBuilder.CreateIndex(
                name: "IX_erp_delinquency_sequence_configs_TenantId_IsActive",
                table: "erp_delinquency_sequence_configs",
                columns: new[] { "TenantId", "IsActive" });

            migrationBuilder.CreateIndex(
                name: "IX_erp_delinquency_sequence_configs_TenantId_StepNumber",
                table: "erp_delinquency_sequence_configs",
                columns: new[] { "TenantId", "StepNumber" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_erp_delinquency_sequence_pauses_TenantId_StartDate_EndDate",
                table: "erp_delinquency_sequence_pauses",
                columns: new[] { "TenantId", "StartDate", "EndDate" });

            migrationBuilder.CreateIndex(
                name: "IX_erp_delinquency_sequence_pauses_TenantId_UnitId",
                table: "erp_delinquency_sequence_pauses",
                columns: new[] { "TenantId", "UnitId" });

            migrationBuilder.CreateIndex(
                name: "IX_erp_delinquency_sequence_pauses_UnitId",
                table: "erp_delinquency_sequence_pauses",
                column: "UnitId");

            migrationBuilder.CreateIndex(
                name: "IX_erp_notification_templates_TenantId_EventType_IsActive",
                table: "erp_notification_templates",
                columns: new[] { "TenantId", "EventType", "IsActive" });

            migrationBuilder.CreateIndex(
                name: "IX_erp_notification_templates_TenantId_Name",
                table: "erp_notification_templates",
                columns: new[] { "TenantId", "Name" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "erp_automatic_notifications");

            migrationBuilder.DropTable(
                name: "erp_bulletin_board_posts");

            migrationBuilder.DropTable(
                name: "erp_communication_preferences");

            migrationBuilder.DropTable(
                name: "erp_communication_recipients");

            migrationBuilder.DropTable(
                name: "erp_delinquency_sequence_configs");

            migrationBuilder.DropTable(
                name: "erp_delinquency_sequence_pauses");

            migrationBuilder.DropTable(
                name: "erp_communications");

            migrationBuilder.DropTable(
                name: "erp_notification_templates");
        }
    }
}
