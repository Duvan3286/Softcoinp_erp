namespace Softcoinp.ERP.Domain.Utils;

/// <summary>
/// Constants related to tenant database naming and configuration.
/// </summary>
public static class TenantDatabaseConstants
{
    public const string TenantDatabasePrefix = "erp_tenant_";
    public const string MasterDatabaseName = "erp_master";
    public const string DefaultMigrationTable = "__EFMigrationsHistory";
}
