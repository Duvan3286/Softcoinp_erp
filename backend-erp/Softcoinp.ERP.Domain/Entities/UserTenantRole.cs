using Softcoinp.ERP.Domain.Common;
using Softcoinp.ERP.Domain.Enums;

namespace Softcoinp.ERP.Domain.Entities;

/// <summary>
/// Relación M:N entre un usuario y un tenant con un rol específico.
/// Un usuario puede pertenecer a múltiples conjuntos con distintos roles.
/// </summary>
public class UserTenantRole : BaseEntity
{
    /// <summary>ID del usuario (FK → AspNetUsers)</summary>
    public string UserId { get; set; } = string.Empty;

    /// <summary>ID del tenant/conjunto residencial</summary>
    public string TenantId { get; set; } = string.Empty;

    /// <summary>Rol del usuario en este tenant</summary>
    public AppRole Role { get; set; }

    /// <summary>Si la asignación está activa</summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Fecha de expiración del rol. Usado para Council (miembros del consejo)
    /// y Auditor. Al vencer, el rol del Council pasa automáticamente a Resident.
    /// </summary>
    public DateTime? ExpiresAt { get; set; }

    /// <summary>Fecha en que se asignó el rol</summary>
    public DateTime AssignedAt { get; set; } = DateTime.UtcNow;

    /// <summary>ID del admin que asignó el rol</summary>
    public string AssignedByUserId { get; set; } = string.Empty;

    // Navigation properties
    public User? User { get; set; }
    public User? AssignedByUser { get; set; }
}
