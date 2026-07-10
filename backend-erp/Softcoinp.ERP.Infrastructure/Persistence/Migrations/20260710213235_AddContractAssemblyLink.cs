using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Softcoinp.ERP.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddContractAssemblyLink : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "ApprovedInAssemblyId",
                table: "erp_contracts",
                type: "char(36)",
                nullable: true,
                collation: "ascii_general_ci");

            migrationBuilder.CreateIndex(
                name: "IX_erp_contracts_ApprovedInAssemblyId",
                table: "erp_contracts",
                column: "ApprovedInAssemblyId");

            migrationBuilder.AddForeignKey(
                name: "FK_erp_contracts_erp_assemblies_ApprovedInAssemblyId",
                table: "erp_contracts",
                column: "ApprovedInAssemblyId",
                principalTable: "erp_assemblies",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_erp_contracts_erp_assemblies_ApprovedInAssemblyId",
                table: "erp_contracts");

            migrationBuilder.DropIndex(
                name: "IX_erp_contracts_ApprovedInAssemblyId",
                table: "erp_contracts");

            migrationBuilder.DropColumn(
                name: "ApprovedInAssemblyId",
                table: "erp_contracts");
        }
    }
}
