namespace Softcoinp.ERP.Domain.Entities;

public enum AuditEventType
{
    LoginSuccess,
    LoginFailed,
    Logout,
    PasswordChanged,
    TokenRefreshed,
    SessionExpired,
    ContextSwitched,
    AccountLocked,
    AccountSuspended,
    AccountActivated,
    InvitationSent,
    InvitationAccepted,
    InvitationRevoked
}

/// <summary>
/// Registro de auditoría de accesos. INMUTABLE: ningún rol puede actualizar
/// ni eliminar registros de esta tabla, ni siquiera el SuperAdmin.
/// Solo se permiten operaciones INSERT.
/// </summary>
public class AccessAuditLog
{
    /// <summary>Identificador único del evento</summary>
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>Timestamp UTC del evento</summary>
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    /// <summary>ID del usuario involucrado (null si el email no existe en el sistema)</summary>
    public string? UserId { get; set; }

    /// <summary>Email usado en el intento (siempre presente)</summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>Tenant en el que ocurrió el evento</summary>
    public string? TenantId { get; set; }

    /// <summary>Tipo de evento de auditoría</summary>
    public AuditEventType EventType { get; set; }

    /// <summary>IP de origen de la solicitud</summary>
    public string? IpAddress { get; set; }

    /// <summary>User-Agent del navegador/cliente</summary>
    public string? UserAgent { get; set; }

    /// <summary>Información adicional en formato JSON</summary>
    public string? Details { get; set; }

    // Navigation
    public User? User { get; set; }
}
