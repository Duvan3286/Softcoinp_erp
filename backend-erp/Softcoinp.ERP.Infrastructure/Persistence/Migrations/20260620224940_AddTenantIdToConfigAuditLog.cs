using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Softcoinp.ERP.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddTenantIdToConfigAuditLog : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "TenantId",
                table: "erp_configuration_audit_logs",
                type: "varchar(255)",
                maxLength: 255,
                nullable: false,
                defaultValue: "")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_erp_configuration_audit_logs_TenantId_ParameterName_Timestamp",
                table: "erp_configuration_audit_logs",
                columns: new[] { "TenantId", "ParameterName", "Timestamp" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_erp_configuration_audit_logs_TenantId_ParameterName_Timestamp",
                table: "erp_configuration_audit_logs");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "erp_configuration_audit_logs");
        }
    }
}
