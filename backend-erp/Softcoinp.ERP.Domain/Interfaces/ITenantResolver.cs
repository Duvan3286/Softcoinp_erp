using System;
using System.Threading.Tasks;

namespace Softcoinp.ERP.Domain.Interfaces;

public class Tenant
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = string.Empty;
    public string Subdomain { get; set; } = string.Empty;
    public string ConnectionString { get; set; } = string.Empty;
    public string? CoreApiUrl { get; set; }
    public bool IsActive { get; set; } = true;

    public int SessionTimeout { get; set; } = 480;
    public int MaxLoginAttempts { get; set; } = 5;
}

public interface ITenantResolver
{
    Task<string?> GetConnectionStringAsync();
    Task<Tenant?> GetCurrentTenantAsync();
    void SetCurrentTenant(Tenant tenant);
}
