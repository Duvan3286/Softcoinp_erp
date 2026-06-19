using Softcoinp.ERP.Domain.Common;
using Softcoinp.ERP.Domain.Enums;

namespace Softcoinp.ERP.Domain.Entities;

public enum InvitationStatus { Pending, Accepted, Expired, Revoked }

/// <summary>
/// Invitación enviada por un admin para que un usuario nuevo se una al tenant.
/// Los usuarios nunca son creados directamente con contraseña por el admin;
/// siempre establecen su propia contraseña al aceptar la invitación.
/// </summary>
public class Invitation : BaseEntity
{
    /// <summary>Correo electrónico del invitado</summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>ID del tenant al que se invita</summary>
    public Guid TenantId { get; set; }

    /// <summary>Rol que tendrá el usuario al aceptar</summary>
    public AppRole Role { get; set; }

    /// <summary>Token hasheado (SHA-256). El token real solo se envía por email una sola vez.</summary>
    public string TokenHash { get; set; } = string.Empty;

    /// <summary>Estado actual de la invitación</summary>
    public InvitationStatus Status { get; set; } = InvitationStatus.Pending;

    /// <summary>Expira 48 horas después de la creación</summary>
    public DateTime ExpiresAt { get; set; }

    /// <summary>Admin que creó la invitación</summary>
    public string CreatedByUserId { get; set; } = string.Empty;

    /// <summary>Fecha y hora en que fue aceptada</summary>
    public DateTime? AcceptedAt { get; set; }

    /// <summary>ID del usuario que aceptó (creado al aceptar)</summary>
    public string? AcceptedByUserId { get; set; }

    // Navigation properties
    public User? CreatedByUser { get; set; }
    public User? AcceptedByUser { get; set; }
}
