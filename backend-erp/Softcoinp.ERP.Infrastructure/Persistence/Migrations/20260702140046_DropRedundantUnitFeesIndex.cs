using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Softcoinp.ERP.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class DropRedundantUnitFeesIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_erp_unit_fees_TenantId_UnitId_Status",
                table: "erp_unit_fees");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_erp_unit_fees_TenantId_UnitId_Status",
                table: "erp_unit_fees",
                columns: new[] { "TenantId", "UnitId", "Status" });
        }
    }
}
