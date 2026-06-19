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

    // ── Configuración de sesión por tenant ───────────────────────────
    /// <summary>Timeout por inactividad para Admin/Consejo/Contador/Auditor (minutos). Default 8h.</summary>
    public int AdminSessionTimeout { get; set; } = 480;

    /// <summary>Timeout por inactividad para Residentes (minutos). Default 2h.</summary>
    public int ResidentSessionTimeout { get; set; } = 120;

    /// <summary>Intentos fallidos antes de bloquear la cuenta temporalmente. Default 5.</summary>
    public int MaxLoginAttempts { get; set; } = 5;
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

    /// <summary>
    /// Manually sets the current tenant (useful for migrations or background tasks).
    /// </summary>
    void SetCurrentTenant(Tenant tenant);
}
