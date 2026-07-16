using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Softcoinp.ERP.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddAssemblyDecisionPropagationFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ExtraordinaryFeeDistributionType",
                table: "erp_assembly_agenda_items",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ExtraordinaryFeeDueDate",
                table: "erp_assembly_agenda_items",
                type: "datetime(6)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ExtraordinaryFeeInstallments",
                table: "erp_assembly_agenda_items",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ExtraordinaryFeeStartPeriod",
                table: "erp_assembly_agenda_items",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<decimal>(
                name: "ExtraordinaryFeeTotalAmount",
                table: "erp_assembly_agenda_items",
                type: "decimal(65,30)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "PropagationTarget",
                table: "erp_assembly_agenda_items",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "TargetBudgetId",
                table: "erp_assembly_agenda_items",
                type: "char(36)",
                nullable: true,
                collation: "ascii_general_ci");

            migrationBuilder.AddColumn<string>(
                name: "ActNumber",
                table: "erp_assemblies",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ExtraordinaryFeeDistributionType",
                table: "erp_assembly_agenda_items");

            migrationBuilder.DropColumn(
                name: "ExtraordinaryFeeDueDate",
                table: "erp_assembly_agenda_items");

            migrationBuilder.DropColumn(
                name: "ExtraordinaryFeeInstallments",
                table: "erp_assembly_agenda_items");

            migrationBuilder.DropColumn(
                name: "ExtraordinaryFeeStartPeriod",
                table: "erp_assembly_agenda_items");

            migrationBuilder.DropColumn(
                name: "ExtraordinaryFeeTotalAmount",
                table: "erp_assembly_agenda_items");

            migrationBuilder.DropColumn(
                name: "PropagationTarget",
                table: "erp_assembly_agenda_items");

            migrationBuilder.DropColumn(
                name: "TargetBudgetId",
                table: "erp_assembly_agenda_items");

            migrationBuilder.DropColumn(
                name: "ActNumber",
                table: "erp_assemblies");
        }
    }
}
