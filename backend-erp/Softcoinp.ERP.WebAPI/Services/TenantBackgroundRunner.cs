using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using MySqlConnector;
using Softcoinp.ERP.Domain.Entities;
using Softcoinp.ERP.Domain.Interfaces;
using Softcoinp.ERP.Infrastructure.Persistence;

namespace Softcoinp.ERP.WebAPI.Services;

public static class TenantBackgroundRunner
{
    public static async Task ForEachTenantAsync(
        IServiceScopeFactory scopeFactory,
        Func<ApplicationDbContext, IServiceProvider, Task> action)
    {
        var tenants = await GetActiveTenantsAsync(scopeFactory);

        var serverVersion = new MySqlServerVersion(new Version(8, 0, 0));

        foreach (var tenant in tenants)
        {
            using var scope = scopeFactory.CreateScope();
            var tenantResolver = scope.ServiceProvider.GetRequiredService<ITenantResolver>();
            tenantResolver.SetCurrentTenant(tenant);

            var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
            optionsBuilder.UseMySql(tenant.ConnectionString, serverVersion);
            using var context = new ApplicationDbContext(optionsBuilder.Options, tenantResolver);

            await action(context, scope.ServiceProvider);
        }
    }

    public static async Task ForEachTenantScopedAsync(
        IServiceScopeFactory scopeFactory,
        Func<IServiceProvider, Task> action)
    {
        var tenants = await GetActiveTenantsAsync(scopeFactory);

        foreach (var tenant in tenants)
        {
            using var scope = scopeFactory.CreateScope();
            var tenantResolver = scope.ServiceProvider.GetRequiredService<ITenantResolver>();
            tenantResolver.SetCurrentTenant(tenant);

            await action(scope.ServiceProvider);
        }
    }

    private static async Task<List<Tenant>> GetActiveTenantsAsync(IServiceScopeFactory scopeFactory)
    {
        using var masterScope = scopeFactory.CreateScope();
        var masterContext = masterScope.ServiceProvider.GetRequiredService<MasterDbContext>();
        return await masterContext.Tenants
            .Where(t => t.IsActive)
            .ToListAsync();
    }
}
