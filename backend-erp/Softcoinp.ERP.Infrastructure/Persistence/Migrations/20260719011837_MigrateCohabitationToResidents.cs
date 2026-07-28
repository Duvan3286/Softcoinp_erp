using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Softcoinp.ERP.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class MigrateCohabitationToResidents : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "erp_residents",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    TenantId = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    FullNameOrPetName = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    DateOfBirth = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    DocumentType = table.Column<string>(type: "varchar(30)", maxLength: 30, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    DocumentNumber = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Phone = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    IsPet = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    PetSpecies = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    PetBreed = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    PetSanitaryRegistration = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    IsActive = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    CreatedByUserId = table.Column<string>(type: "varchar(450)", maxLength: 450, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_erp_residents", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "erp_resident_histories",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    TenantId = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    UnitId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    ResidentId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    Relationship = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    StartDate = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    EndDate = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    TransferNotes = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    RecordedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    RecordedByUserId = table.Column<string>(type: "varchar(450)", maxLength: 450, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_erp_resident_histories", x => x.Id);
                    table.ForeignKey(
                        name: "FK_erp_resident_histories_erp_residents_ResidentId",
                        column: x => x.ResidentId,
                        principalTable: "erp_residents",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_erp_resident_histories_erp_units_UnitId",
                        column: x => x.UnitId,
                        principalTable: "erp_units",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "erp_unit_residents",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    TenantId = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    UnitId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    ResidentId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    Relationship = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    StartDate = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    EndDate = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    IsActive = table.Column<bool>(type: "tinyint(1)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_erp_unit_residents", x => x.Id);
                    table.ForeignKey(
                        name: "FK_erp_unit_residents_erp_residents_ResidentId",
                        column: x => x.ResidentId,
                        principalTable: "erp_residents",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_erp_unit_residents_erp_units_UnitId",
                        column: x => x.UnitId,
                        principalTable: "erp_units",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_erp_resident_histories_ResidentId",
                table: "erp_resident_histories",
                column: "ResidentId");

            migrationBuilder.CreateIndex(
                name: "IX_erp_resident_histories_TenantId_ResidentId",
                table: "erp_resident_histories",
                columns: new[] { "TenantId", "ResidentId" });

            migrationBuilder.CreateIndex(
                name: "IX_erp_resident_histories_UnitId_EndDate",
                table: "erp_resident_histories",
                columns: new[] { "UnitId", "EndDate" });

            migrationBuilder.CreateIndex(
                name: "IX_erp_residents_TenantId_DocumentNumber",
                table: "erp_residents",
                columns: new[] { "TenantId", "DocumentNumber" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_erp_unit_residents_ResidentId",
                table: "erp_unit_residents",
                column: "ResidentId");

            migrationBuilder.CreateIndex(
                name: "IX_erp_unit_residents_TenantId_UnitId_IsActive",
                table: "erp_unit_residents",
                columns: new[] { "TenantId", "UnitId", "IsActive" });

            migrationBuilder.CreateIndex(
                name: "IX_erp_unit_residents_UnitId_IsActive",
                table: "erp_unit_residents",
                columns: new[] { "UnitId", "IsActive" });

            // Migra los datos existentes de erp_cohabitation_group_members (fila plana persona+unidad)
            // hacia el nuevo modelo de identidad (erp_residents) + asignación vigente (erp_unit_residents)
            // + bitácora histórica (erp_resident_histories). Se reutiliza el mismo Id como ResidentId.
            migrationBuilder.Sql(@"
                INSERT INTO erp_residents (Id, TenantId, FullNameOrPetName, DateOfBirth, DocumentType, DocumentNumber, Phone, IsPet, PetSpecies, PetBreed, PetSanitaryRegistration, IsActive, CreatedAt, CreatedByUserId)
                SELECT Id, TenantId, FullNameOrPetName, DateOfBirth, DocumentType, DocumentNumber, Phone, IsPet, PetSpecies, PetBreed, PetSanitaryRegistration, IsActive, CreatedAt, CreatedByUserId
                FROM erp_cohabitation_group_members;
            ");

            migrationBuilder.Sql(@"
                INSERT INTO erp_unit_residents (Id, TenantId, UnitId, ResidentId, Relationship, StartDate, EndDate, IsActive)
                SELECT UUID(), TenantId, UnitId, Id, Relationship, CreatedAt, NULL, IsActive
                FROM erp_cohabitation_group_members;
            ");

            migrationBuilder.Sql(@"
                INSERT INTO erp_resident_histories (Id, TenantId, UnitId, ResidentId, Relationship, StartDate, EndDate, TransferNotes, RecordedAt, RecordedByUserId)
                SELECT UUID(), TenantId, UnitId, Id, Relationship, CreatedAt, NULL, 'Migración desde grupo de convivencia', CreatedAt, CreatedByUserId
                FROM erp_cohabitation_group_members;
            ");

            migrationBuilder.DropTable(
                name: "erp_cohabitation_group_members");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "erp_resident_histories");

            migrationBuilder.DropTable(
                name: "erp_unit_residents");

            migrationBuilder.DropTable(
                name: "erp_residents");

            migrationBuilder.CreateTable(
                name: "erp_cohabitation_group_members",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    UnitId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    CreatedByUserId = table.Column<string>(type: "varchar(450)", maxLength: 450, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    DateOfBirth = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    DocumentNumber = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    DocumentType = table.Column<string>(type: "varchar(30)", maxLength: 30, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    FullNameOrPetName = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    IsActive = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    IsPet = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    PetBreed = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    PetSanitaryRegistration = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    PetSpecies = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Phone = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Relationship = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    TenantId = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_erp_cohabitation_group_members", x => x.Id);
                    table.ForeignKey(
                        name: "FK_erp_cohabitation_group_members_erp_units_UnitId",
                        column: x => x.UnitId,
                        principalTable: "erp_units",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_erp_cohabitation_group_members_UnitId_IsActive_IsPet",
                table: "erp_cohabitation_group_members",
                columns: new[] { "UnitId", "IsActive", "IsPet" });
        }
    }
}
