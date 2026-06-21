using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Softcoinp.ERP.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class StandardizeDecimalPrecision : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<decimal>(
                name: "OwnershipPercentage",
                table: "erp_unit_owners",
                type: "decimal(7,4)",
                precision: 7,
                scale: 4,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(5,2)");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<decimal>(
                name: "OwnershipPercentage",
                table: "erp_unit_owners",
                type: "decimal(5,2)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(7,4)",
                oldPrecision: 7,
                oldScale: 4);
        }
    }
}
