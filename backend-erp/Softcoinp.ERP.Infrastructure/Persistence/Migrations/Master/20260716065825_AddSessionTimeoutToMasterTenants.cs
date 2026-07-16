using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Softcoinp.ERP.Infrastructure.Persistence.Migrations.Master
{
    /// <inheritdoc />
    public partial class AddSessionTimeoutToMasterTenants : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "MaxLoginAttempts",
                table: "erp_master_tenants",
                type: "int",
                nullable: false,
                defaultValue: 5);

            migrationBuilder.AddColumn<int>(
                name: "SessionTimeout",
                table: "erp_master_tenants",
                type: "int",
                nullable: false,
                defaultValue: 480);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MaxLoginAttempts",
                table: "erp_master_tenants");

            migrationBuilder.DropColumn(
                name: "SessionTimeout",
                table: "erp_master_tenants");
        }
    }
}
