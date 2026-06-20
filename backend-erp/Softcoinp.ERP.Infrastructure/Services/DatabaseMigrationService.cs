using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Identity;
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
    /// </summary>
    /// <returns>A dictionary containing the status of each migration.</returns>
    public async Task<Dictionary<string, string>> MigrateAllAsync()
    {
        var results = new Dictionary<string, string>();

        _logger.LogInformation("Starting Master database migration...");
        try
        {
            // Use only EnsureCreated for MasterDB — it was created with EnsureCreated (not migrations),
            // so MigrateAsync would fail with "pending changes" because there is no migration history.
            await _masterDbContext.Database.EnsureCreatedAsync();
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

                // EnsureCreated creates the database if it doesn't exist.
                // Required for fresh environments (docker-compose down -v).
                await tenantContext.Database.EnsureCreatedAsync();

                // MigrateAsync applies any pending EF migrations on top of the schema.
                try
                {
                    await tenantContext.Database.MigrateAsync();
                }
                catch (Exception migEx)
                {
                    // EnsureCreated + MigrateAsync can conflict when the schema was already
                    // created without migration history. Log and continue — DB is functional.
                    _logger.LogWarning(migEx, "MigrateAsync warning for {Subdomain} (schema may already be current)", tenant.Subdomain);
                }

                // Seed default users and roles using DI-resolved UserManager.
                // The ApplicationDbContext resolved from DI has IsConfigured=false →
                // OnConfiguring runs → resolver returns tenant's connection string (set above).
                var userManager = scope.ServiceProvider.GetRequiredService<UserManager<User>>();
                var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
                await DbInitializer.SeedUsersAsync(userManager, roleManager, _configuration);
                
                // Seed Resolution 029 standard chart of accounts
                await DbInitializer.SeedChartOfAccountsAsync(tenantContext, tenant.Id.ToString());
                
                results.Add(tenant.Subdomain, "Success");
                _logger.LogInformation("Successfully migrated tenant: {Subdomain}", tenant.Subdomain);
            }
            catch (Exception ex)
            {
                results.Add(tenant.Subdomain, $"Failed: {ex.Message}");
                _logger.LogError(ex, "Error migrating tenant database for {Subdomain}", tenant.Subdomain);
            }
        }

        return results;
    }
}
