using System;
using System.Threading.Tasks;

namespace Softcoinp.ERP.Domain.Interfaces;

/// <summary>
/// Domain entity representing a tenant in the system.
/// </summary>
public class Tenant
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = string.Empty;
    public string Subdomain { get; set; } = string.Empty;
    public string ConnectionString { get; set; } = string.Empty;
    public string? CoreApiUrl { get; set; }
    public bool IsActive { get; set; } = true;
}

/// <summary>
/// Interface for resolving the current tenant based on the request context.
/// </summary>
public interface ITenantResolver
{
    /// <summary>
    /// Gets the connection string for the current tenant.
    /// </summary>
    Task<string?> GetConnectionStringAsync();

    /// <summary>
    /// Gets the current tenant metadata.
    /// </summary>
    Task<Tenant?> GetCurrentTenantAsync();
}
