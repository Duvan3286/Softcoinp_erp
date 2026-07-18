using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Softcoinp.ERP.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class FixUnitIdentifierUniquePerTower : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_erp_units_TenantId_Identifier",
                table: "erp_units");

            migrationBuilder.CreateIndex(
                name: "IX_erp_units_TenantId_TowerOrBlock_Identifier",
                table: "erp_units",
                columns: new[] { "TenantId", "TowerOrBlock", "Identifier" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_erp_units_TenantId_TowerOrBlock_Identifier",
                table: "erp_units");

            migrationBuilder.CreateIndex(
                name: "IX_erp_units_TenantId_Identifier",
                table: "erp_units",
                columns: new[] { "TenantId", "Identifier" },
                unique: true);
        }
    }
}
