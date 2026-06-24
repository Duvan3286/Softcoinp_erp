using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Softcoinp.ERP.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddAssemblyAndVotingModule : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "erp_assemblies",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    TenantId = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Type = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Status = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ParticipationType = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Title = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Description = table.Column<string>(type: "varchar(4000)", maxLength: 4000, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ScheduledDate = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    ScheduledTime = table.Column<string>(type: "varchar(10)", maxLength: 10, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Location = table.Column<string>(type: "varchar(300)", maxLength: 300, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    SecondConvocationDate = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    SecondConvocationTime = table.Column<string>(type: "varchar(10)", maxLength: 10, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    SecondConvocationLocation = table.Column<string>(type: "varchar(300)", maxLength: 300, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    TotalCoefficients = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    QuorumThresholdFirstCall = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    QuorumThresholdSecondCall = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    QuorumAchievedFirstCall = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    QuorumAchievedSecondCall = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    ConvocationNumber = table.Column<int>(type: "int", nullable: false),
                    SessionStartTime = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    SessionEndTime = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    PresidentName = table.Column<string>(type: "varchar(300)", maxLength: 300, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    SecretaryName = table.Column<string>(type: "varchar(300)", maxLength: 300, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    PresidentOwnerId = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    SecretaryOwnerId = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ConvocationSentAt = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ConvocationDeadlineMet = table.Column<bool>(type: "tinyint(1)", nullable: false),
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
                    table.PrimaryKey("PK_erp_assemblies", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "erp_assembly_agenda_items",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    TenantId = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    AssemblyId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    SequenceNumber = table.Column<int>(type: "int", nullable: false),
                    Title = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Description = table.Column<string>(type: "varchar(4000)", maxLength: 4000, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    PresenterName = table.Column<string>(type: "varchar(300)", maxLength: 300, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    MajorityRequired = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    VotingMode = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    IsInformationOnly = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    RequiresVoting = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    TotalCoefficientsForVote = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    VotesInFavorCoefficients = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    VotesAgainstCoefficients = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    AbstentionCoefficients = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    VotesInFavorCount = table.Column<int>(type: "int", nullable: false),
                    VotesAgainstCount = table.Column<int>(type: "int", nullable: false),
                    AbstentionCount = table.Column<int>(type: "int", nullable: false),
                    IsApproved = table.Column<bool>(type: "tinyint(1)", nullable: true),
                    RejectionReason = table.Column<string>(type: "varchar(1000)", maxLength: 1000, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Observations = table.Column<string>(type: "varchar(4000)", maxLength: 4000, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    OwnerNotes = table.Column<string>(type: "varchar(4000)", maxLength: 4000, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    VoteRegistered = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    RegisteredByUserId = table.Column<string>(type: "varchar(450)", maxLength: 450, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    VoteRegisteredAt = table.Column<DateTime>(type: "datetime(6)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_erp_assembly_agenda_items", x => x.Id);
                    table.ForeignKey(
                        name: "FK_erp_assembly_agenda_items_erp_assemblies_AssemblyId",
                        column: x => x.AssemblyId,
                        principalTable: "erp_assemblies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "erp_assembly_attendances",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    TenantId = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    AssemblyId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    UnitId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    OwnerId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    Coefficient = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    Status = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    AttendsPersonally = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    RepresentativeOwnerId = table.Column<Guid>(type: "char(36)", nullable: true, collation: "ascii_general_ci"),
                    RepresentativeName = table.Column<string>(type: "varchar(300)", maxLength: 300, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    RepresentativeDocumentNumber = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    PowerOfAttorneyFilePath = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ArrivalTime = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    DepartureTime = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    HasDuesArrears = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    VotingRightRestricted = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    VotingRestrictionReason = table.Column<string>(type: "varchar(1000)", maxLength: 1000, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    VotingRestrictionLiftedByUserId = table.Column<string>(type: "varchar(450)", maxLength: 450, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    VotingRestrictionLiftedReason = table.Column<string>(type: "varchar(1000)", maxLength: 1000, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    VotingRestrictionLiftedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    IsCommissionMember = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    CommissionRole = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Notes = table.Column<string>(type: "varchar(2000)", maxLength: 2000, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    RegisteredByUserId = table.Column<string>(type: "varchar(450)", maxLength: 450, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_erp_assembly_attendances", x => x.Id);
                    table.ForeignKey(
                        name: "FK_erp_assembly_attendances_erp_assemblies_AssemblyId",
                        column: x => x.AssemblyId,
                        principalTable: "erp_assemblies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_erp_assembly_attendances_erp_owners_OwnerId",
                        column: x => x.OwnerId,
                        principalTable: "erp_owners",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_erp_assembly_attendances_erp_owners_RepresentativeOwnerId",
                        column: x => x.RepresentativeOwnerId,
                        principalTable: "erp_owners",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_erp_assembly_attendances_erp_units_UnitId",
                        column: x => x.UnitId,
                        principalTable: "erp_units",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "erp_assembly_convocations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    TenantId = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    AssemblyId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    ConvocationNumber = table.Column<int>(type: "int", nullable: false),
                    Subject = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Notes = table.Column<string>(type: "varchar(4000)", maxLength: 4000, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    SentAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    SentByUserId = table.Column<string>(type: "varchar(450)", maxLength: 450, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Channel = table.Column<string>(type: "varchar(30)", maxLength: 30, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    TotalRecipients = table.Column<int>(type: "int", nullable: false),
                    DeliveredCount = table.Column<int>(type: "int", nullable: false),
                    FailedCount = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_erp_assembly_convocations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_erp_assembly_convocations_erp_assemblies_AssemblyId",
                        column: x => x.AssemblyId,
                        principalTable: "erp_assemblies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "erp_assembly_minutes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    TenantId = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    AssemblyId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    Status = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    PresidentName = table.Column<string>(type: "varchar(300)", maxLength: 300, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    SecretaryName = table.Column<string>(type: "varchar(300)", maxLength: 300, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    FullText = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    GeneratedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    GeneratedByUserId = table.Column<string>(type: "varchar(450)", maxLength: 450, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CommissionMemberNames = table.Column<string>(type: "varchar(2000)", maxLength: 2000, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CommissionReviewDeadline = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    CommissionComments = table.Column<string>(type: "varchar(4000)", maxLength: 4000, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    PresidentSignatureFilePath = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    SecretarySignatureFilePath = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ApprovedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    ApprovedByUserId = table.Column<string>(type: "varchar(450)", maxLength: 450, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    PublishedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    PublishedByUserId = table.Column<string>(type: "varchar(450)", maxLength: 450, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    PublishNotificationCount = table.Column<int>(type: "int", nullable: true),
                    RevisionNotes = table.Column<string>(type: "varchar(4000)", maxLength: 4000, nullable: true)
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
                    table.PrimaryKey("PK_erp_assembly_minutes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_erp_assembly_minutes_erp_assemblies_AssemblyId",
                        column: x => x.AssemblyId,
                        principalTable: "erp_assemblies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "erp_assembly_constancies",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    TenantId = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    AssemblyId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    AgendaItemId = table.Column<Guid>(type: "char(36)", nullable: true, collation: "ascii_general_ci"),
                    OwnerId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    OwnerName = table.Column<string>(type: "varchar(300)", maxLength: 300, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Text = table.Column<string>(type: "varchar(4000)", maxLength: 4000, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    RegisteredByUserId = table.Column<string>(type: "varchar(450)", maxLength: 450, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_erp_assembly_constancies", x => x.Id);
                    table.ForeignKey(
                        name: "FK_erp_assembly_constancies_erp_assemblies_AssemblyId",
                        column: x => x.AssemblyId,
                        principalTable: "erp_assemblies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_erp_assembly_constancies_erp_assembly_agenda_items_AgendaIte~",
                        column: x => x.AgendaItemId,
                        principalTable: "erp_assembly_agenda_items",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_erp_assembly_constancies_erp_owners_OwnerId",
                        column: x => x.OwnerId,
                        principalTable: "erp_owners",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "erp_assembly_decision_propagations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    TenantId = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    AssemblyId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    AgendaItemId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    TargetModule = table.Column<string>(type: "varchar(30)", maxLength: 30, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Status = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Description = table.Column<string>(type: "varchar(2000)", maxLength: 2000, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    TargetEntityId = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    TargetEntityType = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ErrorMessage = table.Column<string>(type: "varchar(4000)", maxLength: 4000, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    RetryCount = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    PropagatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    PropagatedByUserId = table.Column<string>(type: "varchar(450)", maxLength: 450, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_erp_assembly_decision_propagations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_erp_assembly_decision_propagations_erp_assemblies_AssemblyId",
                        column: x => x.AssemblyId,
                        principalTable: "erp_assemblies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_erp_assembly_decision_propagations_erp_assembly_agenda_items~",
                        column: x => x.AgendaItemId,
                        principalTable: "erp_assembly_agenda_items",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "erp_convocation_documents",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    TenantId = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ConvocationId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    DocumentName = table.Column<string>(type: "varchar(300)", maxLength: 300, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    DocumentType = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    FilePath = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Description = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_erp_convocation_documents", x => x.Id);
                    table.ForeignKey(
                        name: "FK_erp_convocation_documents_erp_assembly_convocations_Convocat~",
                        column: x => x.ConvocationId,
                        principalTable: "erp_assembly_convocations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "erp_convocation_recipients",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    TenantId = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ConvocationId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    UnitId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    OwnerId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    OwnerName = table.Column<string>(type: "varchar(300)", maxLength: 300, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    OwnerEmail = table.Column<string>(type: "varchar(300)", maxLength: 300, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    OwnerPhone = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Delivered = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    DeliveredAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    DeliveryError = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_erp_convocation_recipients", x => x.Id);
                    table.ForeignKey(
                        name: "FK_erp_convocation_recipients_erp_assembly_convocations_Convoca~",
                        column: x => x.ConvocationId,
                        principalTable: "erp_assembly_convocations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_erp_convocation_recipients_erp_owners_OwnerId",
                        column: x => x.OwnerId,
                        principalTable: "erp_owners",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_erp_convocation_recipients_erp_units_UnitId",
                        column: x => x.UnitId,
                        principalTable: "erp_units",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_erp_assemblies_TenantId_ScheduledDate",
                table: "erp_assemblies",
                columns: new[] { "TenantId", "ScheduledDate" });

            migrationBuilder.CreateIndex(
                name: "IX_erp_assemblies_TenantId_Status",
                table: "erp_assemblies",
                columns: new[] { "TenantId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_erp_assemblies_TenantId_Type",
                table: "erp_assemblies",
                columns: new[] { "TenantId", "Type" });

            migrationBuilder.CreateIndex(
                name: "IX_erp_assembly_agenda_items_AssemblyId",
                table: "erp_assembly_agenda_items",
                column: "AssemblyId");

            migrationBuilder.CreateIndex(
                name: "IX_erp_assembly_agenda_items_TenantId_AssemblyId",
                table: "erp_assembly_agenda_items",
                columns: new[] { "TenantId", "AssemblyId" });

            migrationBuilder.CreateIndex(
                name: "IX_erp_assembly_agenda_items_TenantId_AssemblyId_SequenceNumber",
                table: "erp_assembly_agenda_items",
                columns: new[] { "TenantId", "AssemblyId", "SequenceNumber" });

            migrationBuilder.CreateIndex(
                name: "IX_erp_assembly_attendances_AssemblyId",
                table: "erp_assembly_attendances",
                column: "AssemblyId");

            migrationBuilder.CreateIndex(
                name: "IX_erp_assembly_attendances_OwnerId",
                table: "erp_assembly_attendances",
                column: "OwnerId");

            migrationBuilder.CreateIndex(
                name: "IX_erp_assembly_attendances_RepresentativeOwnerId",
                table: "erp_assembly_attendances",
                column: "RepresentativeOwnerId");

            migrationBuilder.CreateIndex(
                name: "IX_erp_assembly_attendances_TenantId_AssemblyId",
                table: "erp_assembly_attendances",
                columns: new[] { "TenantId", "AssemblyId" });

            migrationBuilder.CreateIndex(
                name: "IX_erp_assembly_attendances_TenantId_AssemblyId_UnitId",
                table: "erp_assembly_attendances",
                columns: new[] { "TenantId", "AssemblyId", "UnitId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_erp_assembly_attendances_UnitId",
                table: "erp_assembly_attendances",
                column: "UnitId");

            migrationBuilder.CreateIndex(
                name: "IX_erp_assembly_constancies_AgendaItemId",
                table: "erp_assembly_constancies",
                column: "AgendaItemId");

            migrationBuilder.CreateIndex(
                name: "IX_erp_assembly_constancies_AssemblyId",
                table: "erp_assembly_constancies",
                column: "AssemblyId");

            migrationBuilder.CreateIndex(
                name: "IX_erp_assembly_constancies_OwnerId",
                table: "erp_assembly_constancies",
                column: "OwnerId");

            migrationBuilder.CreateIndex(
                name: "IX_erp_assembly_constancies_TenantId_AssemblyId",
                table: "erp_assembly_constancies",
                columns: new[] { "TenantId", "AssemblyId" });

            migrationBuilder.CreateIndex(
                name: "IX_erp_assembly_convocations_AssemblyId",
                table: "erp_assembly_convocations",
                column: "AssemblyId");

            migrationBuilder.CreateIndex(
                name: "IX_erp_assembly_convocations_TenantId_AssemblyId",
                table: "erp_assembly_convocations",
                columns: new[] { "TenantId", "AssemblyId" });

            migrationBuilder.CreateIndex(
                name: "IX_erp_assembly_decision_propagations_AgendaItemId",
                table: "erp_assembly_decision_propagations",
                column: "AgendaItemId");

            migrationBuilder.CreateIndex(
                name: "IX_erp_assembly_decision_propagations_AssemblyId",
                table: "erp_assembly_decision_propagations",
                column: "AssemblyId");

            migrationBuilder.CreateIndex(
                name: "IX_erp_assembly_decision_propagations_TenantId_AssemblyId",
                table: "erp_assembly_decision_propagations",
                columns: new[] { "TenantId", "AssemblyId" });

            migrationBuilder.CreateIndex(
                name: "IX_erp_assembly_decision_propagations_TenantId_Status",
                table: "erp_assembly_decision_propagations",
                columns: new[] { "TenantId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_erp_assembly_decision_propagations_TenantId_TargetModule_Sta~",
                table: "erp_assembly_decision_propagations",
                columns: new[] { "TenantId", "TargetModule", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_erp_assembly_minutes_AssemblyId",
                table: "erp_assembly_minutes",
                column: "AssemblyId");

            migrationBuilder.CreateIndex(
                name: "IX_erp_assembly_minutes_TenantId_AssemblyId",
                table: "erp_assembly_minutes",
                columns: new[] { "TenantId", "AssemblyId" });

            migrationBuilder.CreateIndex(
                name: "IX_erp_assembly_minutes_TenantId_Status",
                table: "erp_assembly_minutes",
                columns: new[] { "TenantId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_erp_convocation_documents_ConvocationId",
                table: "erp_convocation_documents",
                column: "ConvocationId");

            migrationBuilder.CreateIndex(
                name: "IX_erp_convocation_documents_TenantId_ConvocationId",
                table: "erp_convocation_documents",
                columns: new[] { "TenantId", "ConvocationId" });

            migrationBuilder.CreateIndex(
                name: "IX_erp_convocation_recipients_ConvocationId",
                table: "erp_convocation_recipients",
                column: "ConvocationId");

            migrationBuilder.CreateIndex(
                name: "IX_erp_convocation_recipients_OwnerId",
                table: "erp_convocation_recipients",
                column: "OwnerId");

            migrationBuilder.CreateIndex(
                name: "IX_erp_convocation_recipients_TenantId_ConvocationId",
                table: "erp_convocation_recipients",
                columns: new[] { "TenantId", "ConvocationId" });

            migrationBuilder.CreateIndex(
                name: "IX_erp_convocation_recipients_TenantId_OwnerId",
                table: "erp_convocation_recipients",
                columns: new[] { "TenantId", "OwnerId" });

            migrationBuilder.CreateIndex(
                name: "IX_erp_convocation_recipients_UnitId",
                table: "erp_convocation_recipients",
                column: "UnitId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "erp_assembly_attendances");

            migrationBuilder.DropTable(
                name: "erp_assembly_constancies");

            migrationBuilder.DropTable(
                name: "erp_assembly_decision_propagations");

            migrationBuilder.DropTable(
                name: "erp_assembly_minutes");

            migrationBuilder.DropTable(
                name: "erp_convocation_documents");

            migrationBuilder.DropTable(
                name: "erp_convocation_recipients");

            migrationBuilder.DropTable(
                name: "erp_assembly_agenda_items");

            migrationBuilder.DropTable(
                name: "erp_assembly_convocations");

            migrationBuilder.DropTable(
                name: "erp_assemblies");
        }
    }
}
