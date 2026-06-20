namespace Softcoinp.ERP.Domain.Entities;

/// <summary>
/// Refresh token real para rotación segura de sesiones.
/// Reemplaza el token "dummy" del AuthController original.
/// </summary>
public class RefreshToken
{
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>Usuario propietario del token</summary>
    public string UserId { get; set; } = string.Empty;

    /// <summary>Tenant activo cuando se emitió el token</summary>
    public string TenantId { get; set; } = string.Empty;

    /// <summary>Token hasheado (SHA-256). El valor real solo viaja en la respuesta inicial.</summary>
    public string TokenHash { get; set; } = string.Empty;

    /// <summary>Fecha de expiración del refresh token</summary>
    public DateTime ExpiresAt { get; set; }

    /// <summary>Si el token ha sido revocado (logout, rotación o sospecha)</summary>
    public bool IsRevoked { get; set; } = false;

    /// <summary>Fecha de revocación</summary>
    public DateTime? RevokedAt { get; set; }

    /// <summary>Fecha de creación</summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Al rotar el token (refresh), se guarda el hash del nuevo token aquí
    /// para detectar reutilización del token anterior (señal de compromiso).
    /// </summary>
    public string? ReplacedByTokenHash { get; set; }

    /// <summary>IP desde la que se creó el token</summary>
    public string? CreatedFromIp { get; set; }

    // Navigation
    public User? User { get; set; }
}
