using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Softcoinp.ERP.Domain.Interfaces;

namespace Softcoinp.ERP.Infrastructure.Persistence;

public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
{
    public ApplicationDbContext CreateDbContext(string[] args)
    {
        var connectionString = Environment.GetEnvironmentVariable("ConnectionStrings__MasterConnection")
                            ?? "Server=127.0.0.1;Port=3307;Database=erp_master;User=root;Password=1234;";

        var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
        optionsBuilder.UseMySql(connectionString, new MySqlServerVersion(new Version(8, 0)));

        return new ApplicationDbContext(optionsBuilder.Options, new DesignTimeTenantResolver());
    }
}

public class DesignTimeTenantResolver : ITenantResolver
{
    public string GetCurrentTenantId()
    {
        return "design-time";
    }

    public Task<string?> GetConnectionStringAsync()
    {
        var connectionString = Environment.GetEnvironmentVariable("ConnectionStrings__MasterConnection")
                            ?? "Server=127.0.0.1;Port=3307;Database=erp_master;User=root;Password=1234;";
        return Task.FromResult<string?>(connectionString);
    }

    public async Task<Tenant?> GetCurrentTenantAsync()
    {
        return await Task.FromResult(new Tenant
        {
            Id = Guid.NewGuid(),
            Name = "DesignTime",
            Subdomain = "design-time",
            ConnectionString = "Server=127.0.0.1;Port=3307;Database=erp_master;User=root;Password=1234;"
        });
    }

    public void SetCurrentTenant(Tenant tenant)
    {
    }
}
