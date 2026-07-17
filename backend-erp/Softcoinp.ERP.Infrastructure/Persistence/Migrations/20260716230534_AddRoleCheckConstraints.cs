using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Softcoinp.ERP.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddRoleCheckConstraints : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                ALTER TABLE erp_user_tenant_roles
                ADD CONSTRAINT CK_erp_user_tenant_roles_Role
                CHECK (Role IN ('SuperAdmin', 'Admin'));
            ");

            migrationBuilder.Sql(@"
                ALTER TABLE erp_invitations
                ADD CONSTRAINT CK_erp_invitations_Role
                CHECK (Role IN ('SuperAdmin', 'Admin'));
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("ALTER TABLE erp_user_tenant_roles DROP CHECK CK_erp_user_tenant_roles_Role;");
            migrationBuilder.Sql("ALTER TABLE erp_invitations DROP CHECK CK_erp_invitations_Role;");
        }
    }
}
