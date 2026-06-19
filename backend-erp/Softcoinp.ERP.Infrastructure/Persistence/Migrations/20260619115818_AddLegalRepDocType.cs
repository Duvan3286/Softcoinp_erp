using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Softcoinp.ERP.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddLegalRepDocType : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "LegalRepresentativeDocumentType",
                table: "erp_tenant_configuration",
                type: "varchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "LegalRepresentativeDv",
                table: "erp_tenant_configuration",
                type: "varchar(1)",
                maxLength: 1,
                nullable: false,
                defaultValue: "")
                .Annotation("MySql:CharSet", "utf8mb4");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LegalRepresentativeDocumentType",
                table: "erp_tenant_configuration");

            migrationBuilder.DropColumn(
                name: "LegalRepresentativeDv",
                table: "erp_tenant_configuration");
        }
    }
}
