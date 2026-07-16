using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Identity;
using MySqlConnector;
using Softcoinp.ERP.Domain.Interfaces;
using Softcoinp.ERP.Domain.Entities;
using Softcoinp.ERP.Infrastructure.Persistence;
using Microsoft.Extensions.Configuration;

namespace Softcoinp.ERP.Infrastructure.Services;

/// <summary>
/// Service responsible for applying database migrations across all tenant databases.
/// </summary>
public class DatabaseMigrationService
{
    private readonly MasterDbContext _masterDbContext;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<DatabaseMigrationService> _logger;
    private readonly IConfiguration _configuration;

    public DatabaseMigrationService(
        MasterDbContext masterDbContext,
        IServiceScopeFactory scopeFactory,
        ILogger<DatabaseMigrationService> logger,
        IConfiguration configuration)
    {
        _masterDbContext = masterDbContext;
        _scopeFactory = scopeFactory;
        _logger = logger;
        _configuration = configuration;
    }

    /// <summary>
    /// Migrates the Master database and all active tenant databases.
    /// A tenant whose migration fails is reported as failed and is never silently
    /// baselined, so schema drift is always visible instead of hidden.
    /// </summary>
    public async Task<Dictionary<string, string>> MigrateAllAsync()
    {
        var results = new Dictionary<string, string>();

        _logger.LogInformation("Starting Master database migration...");
        try
        {
            await _masterDbContext.Database.EnsureCreatedAsync();
            await EnsureMasterSchemaAsync();
            results.Add("MasterDB", "Success");
            _logger.LogInformation("Master database initialized successfully.");
        }
        catch (Exception ex)
        {
            results.Add("MasterDB", $"Partial Success/Warning: {ex.Message}");
            _logger.LogWarning(ex, "Error initializing Master database, but continuing...");
        }

        var tenants = await _masterDbContext.Tenants
            .Where(t => t.IsActive)
            .ToListAsync();

        _logger.LogInformation("Found {Count} active tenants to migrate.", tenants.Count);

        foreach (var tenant in tenants)
        {
            _logger.LogInformation("Migrating database for tenant: {TenantName} ({Subdomain})", tenant.Name, tenant.Subdomain);

            try
            {
                using var scope = _scopeFactory.CreateScope();
                var tenantResolver = scope.ServiceProvider.GetRequiredService<ITenantResolver>();

                // CRITICAL: Set the current tenant FIRST so that any DI-resolved service
                // (ApplicationDbContext via OnConfiguring, UserManager, etc.) uses this tenant's DB.
                tenantResolver.SetCurrentTenant(tenant);

                // Use a fixed MySQL 8.0 server version to avoid AutoDetect connecting to
                // a database that might not exist yet (throws "Unknown database").
                var serverVersion = new MySqlServerVersion(new Version(8, 0, 0));
                var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
                optionsBuilder.UseMySql(tenant.ConnectionString, serverVersion);

                // Create a temporary context with EXPLICIT options for schema operations.
                // IsConfigured=true → OnConfiguring is skipped → uses the explicit connection string.
                using var tenantContext = new ApplicationDbContext(optionsBuilder.Options, tenantResolver);

                // First, create the database if it doesn't exist (without creating tables).
                // We connect to MySQL without specifying a database to run CREATE DATABASE.
                var connBuilder = new MySqlConnectionStringBuilder(tenant.ConnectionString);
                var dbName = connBuilder.Database;
                connBuilder.Database = null;
                using (var adminConn = new MySqlConnection(connBuilder.ConnectionString))
                {
                    await adminConn.OpenAsync();
                    using var createCmd = adminConn.CreateCommand();
                    createCmd.CommandText = $"CREATE DATABASE IF NOT EXISTS `{dbName}` CHARACTER SET utf8mb4";
                    await createCmd.ExecuteNonQueryAsync();
                }

                var historyTableExists = await HistoryTableExistsAsync(tenantContext);
                var hasApplicationTables = await HasApplicationTablesAsync(tenantContext);

                if (!historyTableExists && hasApplicationTables)
                {
                    // Genuine legacy scenario: schema was created with EnsureCreated before
                    // migrations were introduced. This is the only case where baselining
                    // the CURRENT model as fully applied is safe, because there is no
                    // migration history to reconcile against yet.
                    _logger.LogWarning(
                        "Tenant {Subdomain} has application tables but no migration history. Baselining current model as the starting point.",
                        tenant.Subdomain);
                    await BaselineFreshLegacySchemaAsync(tenantContext);
                }

                // Apply EF Core migrations for real. If this throws, the tenant is reported
                // as failed below instead of having its history silently rewritten.
                await tenantContext.Database.MigrateAsync();

                // Seed default users and roles using DI-resolved UserManager.
                // The ApplicationDbContext resolved from DI has IsConfigured=false →
                // OnConfiguring runs → resolver returns tenant's connection string (set above).
                var userManager = scope.ServiceProvider.GetRequiredService<UserManager<User>>();
                var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
                await DbInitializer.SeedUsersAsync(userManager, roleManager, _configuration);

                // Seed standard report types and default PDF templates
                await DbInitializer.SeedReportTypesAsync(tenantContext, tenant.Id.ToString());

                // Seed default automatic-notification templates (Comunicados)
                await DbInitializer.SeedNotificationTemplatesAsync(tenantContext, tenant.Id.ToString());

                results.Add(tenant.Subdomain, "Success");
                _logger.LogInformation("Successfully migrated tenant: {Subdomain}", tenant.Subdomain);
            }
            catch (Exception ex)
            {
                // Fail loudly. Never fabricate migration history to make an error disappear:
                // doing so leaves the physical schema out of sync with the EF model while
                // reporting everything as up to date, which is far worse than a visible failure.
                results.Add(tenant.Subdomain, $"Failed: {ex.Message}");
                _logger.LogError(ex, "Error migrating tenant database for {Subdomain}. Manual intervention required.", tenant.Subdomain);
            }
        }

        return results;
    }

    private async Task<bool> HistoryTableExistsAsync(ApplicationDbContext context)
    {
        var connection = context.Database.GetDbConnection();
        var wasClosed = connection.State != System.Data.ConnectionState.Open;
        if (wasClosed)
        {
            await connection.OpenAsync();
        }

        using var command = connection.CreateCommand();
        command.CommandText = "SELECT COUNT(*) FROM information_schema.tables WHERE table_schema = DATABASE() AND table_name = '__EFMigrationsHistory'";
        var count = Convert.ToInt32(await command.ExecuteScalarAsync());

        if (wasClosed)
        {
            await connection.CloseAsync();
        }

        return count > 0;
    }

    private async Task<bool> HasApplicationTablesAsync(ApplicationDbContext context)
    {
        var connection = context.Database.GetDbConnection();
        var wasClosed = connection.State != System.Data.ConnectionState.Open;
        if (wasClosed)
        {
            await connection.OpenAsync();
        }

        using var command = connection.CreateCommand();
        command.CommandText = "SELECT COUNT(*) FROM information_schema.tables WHERE table_schema = DATABASE() AND table_name != '__EFMigrationsHistory'";
        var count = Convert.ToInt32(await command.ExecuteScalarAsync());

        if (wasClosed)
        {
            await connection.CloseAsync();
        }

        return count > 0;
    }

    private async Task BaselineFreshLegacySchemaAsync(ApplicationDbContext context)
    {
        var migrationsAssembly = context.Database.GetService<Microsoft.EntityFrameworkCore.Migrations.IMigrationsAssembly>();
        var allMigrations = migrationsAssembly.Migrations
            .OrderBy(kvp => kvp.Key)
            .ToList();

        if (allMigrations.Count == 0)
        {
            _logger.LogWarning("No migrations found in assembly for baselining.");
            return;
        }

        foreach (var (migrationId, _) in allMigrations)
        {
            await context.Database.ExecuteSqlRawAsync(
                "INSERT IGNORE INTO `__EFMigrationsHistory` (MigrationId, ProductVersion) VALUES ({0}, {1})",
                migrationId, "8.0.10");
        }

        _logger.LogInformation("Migration history baselined with {Count} entries for a fresh legacy schema.", allMigrations.Count);
    }

    private async Task EnsureMasterSchemaAsync()
    {
        var connection = _masterDbContext.Database.GetDbConnection();
        var wasClosed = connection.State != System.Data.ConnectionState.Open;
        if (wasClosed)
        {
            await connection.OpenAsync();
        }

        var dbName = connection.Database;

        // Check if SessionTimeout column exists
        using (var cmd = connection.CreateCommand())
        {
            cmd.CommandText = $"SELECT COUNT(*) FROM information_schema.columns WHERE table_schema = '{dbName}' AND table_name = 'erp_master_tenants' AND column_name = 'SessionTimeout'";
            var exists = Convert.ToInt32(await cmd.ExecuteScalarAsync()) > 0;
            if (!exists)
            {
                _logger.LogInformation("Adding SessionTimeout column to erp_master_tenants...");
                using var alter = connection.CreateCommand();
                alter.CommandText = "ALTER TABLE erp_master_tenants ADD COLUMN SessionTimeout int NOT NULL DEFAULT 480";
                await alter.ExecuteNonQueryAsync();
            }
        }

        // Check if MaxLoginAttempts column exists
        using (var cmd = connection.CreateCommand())
        {
            cmd.CommandText = $"SELECT COUNT(*) FROM information_schema.columns WHERE table_schema = '{dbName}' AND table_name = 'erp_master_tenants' AND column_name = 'MaxLoginAttempts'";
            var exists = Convert.ToInt32(await cmd.ExecuteScalarAsync()) > 0;
            if (!exists)
            {
                _logger.LogInformation("Adding MaxLoginAttempts column to erp_master_tenants...");
                using var alter = connection.CreateCommand();
                alter.CommandText = "ALTER TABLE erp_master_tenants ADD COLUMN MaxLoginAttempts int NOT NULL DEFAULT 5";
                await alter.ExecuteNonQueryAsync();
            }
        }

        if (wasClosed)
        {
            await connection.CloseAsync();
        }
    }
}
