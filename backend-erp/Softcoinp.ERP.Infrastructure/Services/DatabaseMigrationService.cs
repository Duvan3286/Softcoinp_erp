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
            // Use EnsureCreated first to make sure tables exist if migrations fail
            await _masterDbContext.Database.EnsureCreatedAsync();
            await _masterDbContext.Database.MigrateAsync();
            results.Add("MasterDB", "Success");
            _logger.LogInformation("Master database migrated successfully.");
        }
        catch (Exception ex)
        {
            results.Add("MasterDB", $"Partial Success/Warning: {ex.Message}");
            _logger.LogWarning(ex, "Error migrating Master database, but continuing...");
        }

        var tenants = await _masterDbContext.Tenants
            .Where(t => t.IsActive)
            .ToListAsync();
// ... (rest of the method)

        _logger.LogInformation("Found {Count} active tenants to migrate.", tenants.Count);

        foreach (var tenant in tenants)
        {
            _logger.LogInformation("Migrating database for tenant: {TenantName} ({Subdomain})", tenant.Name, tenant.Subdomain);
            
            try
            {
                using var scope = _scopeFactory.CreateScope();
                
                // We need to bypass the standard resolver for migration because we already have the connection string
                var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
                optionsBuilder.UseMySql(tenant.ConnectionString, ServerVersion.AutoDetect(tenant.ConnectionString));

                // Create a temporary context instance for migration
                // Note: ITenantResolver is still needed by the constructor but won't be used due to IsConfigured check in OnConfiguring
                var tenantResolver = scope.ServiceProvider.GetRequiredService<ITenantResolver>();
                using var tenantContext = new ApplicationDbContext(optionsBuilder.Options, tenantResolver);

                await tenantContext.Database.MigrateAsync();
                
                // CRITICAL: Set the current tenant in the resolver so that DI-resolved services (like UserManager) use the correct database
                tenantResolver.SetCurrentTenant(tenant);

                // Seed default users and roles
                var userManager = scope.ServiceProvider.GetRequiredService<UserManager<User>>();
                var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
                await DbInitializer.SeedUsersAsync(userManager, roleManager, _configuration);
                
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
