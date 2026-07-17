using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Softcoinp.ERP.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class CleanupLegacyRoleData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                UPDATE erp_user_tenant_roles
                SET IsActive = 0
                WHERE Role NOT IN ('SuperAdmin', 'Admin');
            ");

            migrationBuilder.Sql(@"
                UPDATE erp_invitations
                SET Status = 'Revoked'
                WHERE Role NOT IN ('SuperAdmin', 'Admin')
                AND Status = 'Pending';
            ");

            migrationBuilder.Sql(@"
                DELETE FROM AspNetUserRoles
                WHERE RoleId IN (
                    SELECT Id FROM (
                        SELECT Id FROM AspNetRoles WHERE Name NOT IN ('SuperAdmin', 'Admin')
                    ) AS obsolete_roles
                );
            ");

            migrationBuilder.Sql(@"
                DELETE FROM AspNetRoles
                WHERE Name NOT IN ('SuperAdmin', 'Admin');
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
