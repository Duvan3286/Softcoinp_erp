using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Softcoinp.ERP.Infrastructure.Persistence;

namespace Softcoinp.ERP.Infrastructure.Persistence;

public static class MasterDbInitializer
{
    public static async Task SeedTenantAsync(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<MasterDbContext>();

        // Ensure Master DB exists
        await context.Database.EnsureCreatedAsync();

        var testTenant = await context.Tenants.FirstOrDefaultAsync(t => t.Subdomain == "test");
        if (testTenant == null)
        {
            context.Tenants.Add(new Softcoinp.ERP.Domain.Interfaces.Tenant
            {
                Name = "Test Tenant",
                Subdomain = "test",
                ConnectionString = "Server=localhost;Port=3306;Database=erp_db;User=root;Password=1234;",
                IsActive = true
            });

            await context.SaveChangesAsync();
        }
        else if (testTenant.ConnectionString.Contains("Port=3307") || testTenant.ConnectionString.Contains("erp_user"))
        {
            testTenant.ConnectionString = "Server=localhost;Port=3306;Database=erp_db;User=root;Password=1234;";
            await context.SaveChangesAsync();
        }
    }
}
