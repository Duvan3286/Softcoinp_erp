using Microsoft.AspNetCore.Identity;

namespace Softcoinp.ERP.Domain.Entities;

/// <summary>
/// Usuario del sistema ERP. Extiende IdentityUser con campos de seguridad,
/// auditoría y soporte multi-tenant. Los usuarios NUNCA se eliminan,
/// solo se suspenden para preservar trazabilidad histórica.
/// </summary>
public class User : IdentityUser
{
    public string FullName { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>Los usuarios nunca se borran, solo se suspenden.</summary>
    public bool IsActive { get; set; } = true;

    // ── Auditoría de acceso ──────────────────────────────────────────
    public DateTime? LastLogin { get; set; }

    // ── Control de bloqueo por intentos fallidos ─────────────────────
    /// <summary>Intentos fallidos consecutivos (se resetea al login exitoso)</summary>
    public int FailedLoginCount { get; set; } = 0;

    /// <summary>Bloqueado temporalmente hasta esta fecha (15 min por regla de negocio)</summary>
    public DateTime? LockoutUntil { get; set; }

    /// <summary>Número de bloqueos temporales en el día actual</summary>
    public int DailyLockoutCount { get; set; } = 0;

    /// <summary>Fecha del último reset del contador diario de bloqueos</summary>
    public DateOnly? DailyLockoutResetDate { get; set; }

    // ── Suspensión manual ────────────────────────────────────────────
    /// <summary>
    /// Suspensión permanente hasta reactivación manual por un Admin.
    /// Se activa después de 3 bloqueos temporales en el mismo día.
    /// </summary>
    public bool IsSuspended { get; set; } = false;
    public DateTime? SuspendedAt { get; set; }
    public string? SuspendedReason { get; set; }

    // ── Navigation properties ────────────────────────────────────────
    public ICollection<UserTenantRole> TenantRoles { get; set; } = new List<UserTenantRole>();
    public ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();
    public ICollection<AccessAuditLog> AuditLogs { get; set; } = new List<AccessAuditLog>();
    public ICollection<Invitation> SentInvitations { get; set; } = new List<Invitation>();
}

