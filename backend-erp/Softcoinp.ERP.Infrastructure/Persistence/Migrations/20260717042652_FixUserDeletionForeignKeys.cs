using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Softcoinp.ERP.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class FixUserDeletionForeignKeys : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_erp_access_audit_log_AspNetUsers_UserId",
                table: "erp_access_audit_log");

            migrationBuilder.DropForeignKey(
                name: "FK_erp_invitations_AspNetUsers_AcceptedByUserId",
                table: "erp_invitations");

            migrationBuilder.DropForeignKey(
                name: "FK_erp_invitations_AspNetUsers_CreatedByUserId",
                table: "erp_invitations");

            migrationBuilder.DropForeignKey(
                name: "FK_erp_user_change_history_AspNetUsers_UserId",
                table: "erp_user_change_history");

            migrationBuilder.DropForeignKey(
                name: "FK_erp_user_tenant_roles_AspNetUsers_AssignedByUserId",
                table: "erp_user_tenant_roles");

            migrationBuilder.DropIndex(
                name: "IX_erp_user_tenant_roles_AssignedByUserId",
                table: "erp_user_tenant_roles");

            migrationBuilder.DropIndex(
                name: "IX_erp_invitations_CreatedByUserId",
                table: "erp_invitations");

            migrationBuilder.AddForeignKey(
                name: "FK_erp_access_audit_log_AspNetUsers_UserId",
                table: "erp_access_audit_log",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_erp_invitations_AspNetUsers_AcceptedByUserId",
                table: "erp_invitations",
                column: "AcceptedByUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_erp_access_audit_log_AspNetUsers_UserId",
                table: "erp_access_audit_log");

            migrationBuilder.DropForeignKey(
                name: "FK_erp_invitations_AspNetUsers_AcceptedByUserId",
                table: "erp_invitations");

            migrationBuilder.CreateIndex(
                name: "IX_erp_user_tenant_roles_AssignedByUserId",
                table: "erp_user_tenant_roles",
                column: "AssignedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_erp_invitations_CreatedByUserId",
                table: "erp_invitations",
                column: "CreatedByUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_erp_access_audit_log_AspNetUsers_UserId",
                table: "erp_access_audit_log",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_erp_invitations_AspNetUsers_AcceptedByUserId",
                table: "erp_invitations",
                column: "AcceptedByUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_erp_invitations_AspNetUsers_CreatedByUserId",
                table: "erp_invitations",
                column: "CreatedByUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_erp_user_change_history_AspNetUsers_UserId",
                table: "erp_user_change_history",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_erp_user_tenant_roles_AspNetUsers_AssignedByUserId",
                table: "erp_user_tenant_roles",
                column: "AssignedByUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
