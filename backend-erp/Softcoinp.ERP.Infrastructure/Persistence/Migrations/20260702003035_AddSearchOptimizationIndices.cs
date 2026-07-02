using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Softcoinp.ERP.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddSearchOptimizationIndices : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_tenant_residents_search_order",
                table: "erp_tenant_residents",
                columns: new[] { "TenantId", "IsActive", "FullName" });

            migrationBuilder.CreateIndex(
                name: "IX_tenant_residents_unit_active_fullname",
                table: "erp_tenant_residents",
                columns: new[] { "UnitId", "IsActive", "FullName" });

            migrationBuilder.CreateIndex(
                name: "IX_owners_search_order",
                table: "erp_owners",
                columns: new[] { "TenantId", "IsActive", "FullNameOrCompanyName" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_tenant_residents_search_order",
                table: "erp_tenant_residents");

            migrationBuilder.DropIndex(
                name: "IX_tenant_residents_unit_active_fullname",
                table: "erp_tenant_residents");

            migrationBuilder.DropIndex(
                name: "IX_owners_search_order",
                table: "erp_owners");
        }
    }
}
