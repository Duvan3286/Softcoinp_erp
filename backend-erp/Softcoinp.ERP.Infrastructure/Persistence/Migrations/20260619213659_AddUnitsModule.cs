using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Softcoinp.ERP.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddUnitsModule : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "erp_bulk_import_logs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    TenantId = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ExecutedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    ExecutedByUserId = table.Column<string>(type: "varchar(450)", maxLength: 450, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Status = table.Column<string>(type: "varchar(30)", maxLength: 30, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ProcessedRecordsCount = table.Column<int>(type: "int", nullable: false),
                    ErrorCount = table.Column<int>(type: "int", nullable: false),
                    ErrorReport = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_erp_bulk_import_logs", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "erp_unit_types",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    TenantId = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Name = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    HasCustomLiquidationRules = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    CreatedByUserId = table.Column<string>(type: "varchar(450)", maxLength: 450, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    UpdatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_erp_unit_types", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "erp_units",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    TenantId = table.Column<string>(type: "varchar(255)", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Identifier = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    UnitTypeId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    TowerOrBlock = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    FloorLevel = table.Column<int>(type: "int", nullable: false),
                    PrivateArea = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    BalconyArea = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    CoproprietyCoefficient = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    Status = table.Column<string>(type: "varchar(30)", maxLength: 30, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    HasPrivateParking = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    ParkingIdentifier = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    HasAssignedStorage = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    StorageIdentifier = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ConstructionDeliveryDate = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    InternalObservations = table.Column<string>(type: "varchar(1000)", maxLength: 1000, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    CreatedByUserId = table.Column<string>(type: "varchar(450)", maxLength: 450, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    UpdatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_erp_units", x => x.Id);
                    table.CheckConstraint("CK_Unit_CoproprietyCoefficient_Positive", "`CoproprietyCoefficient` > 0");
                    table.ForeignKey(
                        name: "FK_erp_units_erp_unit_types_UnitTypeId",
                        column: x => x.UnitTypeId,
                        principalTable: "erp_unit_types",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "erp_unit_complements",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    TenantId = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ParentUnitId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    ComplementUnitId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    ComplementType = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    CreatedByUserId = table.Column<string>(type: "varchar(450)", maxLength: 450, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_erp_unit_complements", x => x.Id);
                    table.ForeignKey(
                        name: "FK_erp_unit_complements_erp_units_ComplementUnitId",
                        column: x => x.ComplementUnitId,
                        principalTable: "erp_units",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_erp_unit_complements_erp_units_ParentUnitId",
                        column: x => x.ParentUnitId,
                        principalTable: "erp_units",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "erp_unit_state_history",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    TenantId = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    UnitId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    PreviousStatus = table.Column<string>(type: "varchar(30)", maxLength: 30, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    NewStatus = table.Column<string>(type: "varchar(30)", maxLength: 30, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ChangeDate = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    ChangedByUserId = table.Column<string>(type: "varchar(450)", maxLength: 450, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Reason = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_erp_unit_state_history", x => x.Id);
                    table.ForeignKey(
                        name: "FK_erp_unit_state_history_erp_units_UnitId",
                        column: x => x.UnitId,
                        principalTable: "erp_units",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_erp_unit_complements_ComplementUnitId",
                table: "erp_unit_complements",
                column: "ComplementUnitId");

            migrationBuilder.CreateIndex(
                name: "IX_erp_unit_complements_ParentUnitId",
                table: "erp_unit_complements",
                column: "ParentUnitId");

            migrationBuilder.CreateIndex(
                name: "IX_erp_unit_state_history_UnitId",
                table: "erp_unit_state_history",
                column: "UnitId");

            migrationBuilder.CreateIndex(
                name: "IX_erp_units_TenantId_Identifier",
                table: "erp_units",
                columns: new[] { "TenantId", "Identifier" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_erp_units_UnitTypeId",
                table: "erp_units",
                column: "UnitTypeId");

            migrationBuilder.Sql(@"
                CREATE TRIGGER TRG_Units_CheckCoefficient_Insert
                BEFORE INSERT ON erp_units
                FOR EACH ROW
                BEGIN
                    DECLARE total DECIMAL(18,4);
                    SELECT IFNULL(SUM(CoproprietyCoefficient), 0) INTO total 
                    FROM erp_units 
                    WHERE TenantId = NEW.TenantId AND Status != 'Inactive';
                    
                    IF (NEW.Status != 'Inactive' AND total + NEW.CoproprietyCoefficient > 100.0000) THEN
                        SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'The total copropriety coefficient for active units cannot exceed 100%.';
                    END IF;
                END;
            ");

            migrationBuilder.Sql(@"
                CREATE TRIGGER TRG_Units_CheckCoefficient_Update
                BEFORE UPDATE ON erp_units
                FOR EACH ROW
                BEGIN
                    DECLARE total DECIMAL(18,4);
                    SELECT IFNULL(SUM(CoproprietyCoefficient), 0) INTO total 
                    FROM erp_units 
                    WHERE TenantId = NEW.TenantId AND Status != 'Inactive' AND Id != NEW.Id;
                    
                    IF (NEW.Status != 'Inactive' AND total + NEW.CoproprietyCoefficient > 100.0000) THEN
                        SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'The total copropriety coefficient for active units cannot exceed 100%.';
                    END IF;
                END;
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP TRIGGER IF EXISTS TRG_Units_CheckCoefficient_Insert;");
            migrationBuilder.Sql("DROP TRIGGER IF EXISTS TRG_Units_CheckCoefficient_Update;");

            migrationBuilder.DropTable(
                name: "erp_bulk_import_logs");

            migrationBuilder.DropTable(
                name: "erp_unit_complements");

            migrationBuilder.DropTable(
                name: "erp_unit_state_history");

            migrationBuilder.DropTable(
                name: "erp_units");

            migrationBuilder.DropTable(
                name: "erp_unit_types");
        }
    }
}
