using Softcoinp.ERP.Domain.Enums;

namespace Softcoinp.ERP.Domain.Entities;

/// <summary>
/// Immutable record of all changes made to a user by the superuser via the Maintenance module.
/// Records are preserved even after the user is deleted.
/// </summary>
public class UserChangeHistory
{
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>ID of the user that was changed. Preserved after user deletion.</summary>
    public string UserId { get; set; } = string.Empty;

    /// <summary>Name of the field that was changed.</summary>
    public string ChangedField { get; set; } = string.Empty;

    /// <summary>Previous value before the change.</summary>
    public string? OldValue { get; set; }

    /// <summary>New value after the change.</summary>
    public string? NewValue { get; set; }

    /// <summary>Timestamp of the change in UTC.</summary>
    public DateTime ChangedAt { get; set; } = DateTime.UtcNow;

    /// <summary>Type of change performed.</summary>
    public UserChangeType ChangeType { get; set; }

    /// <summary>ID of the superuser who performed the change.</summary>
    public string ChangedByUserId { get; set; } = string.Empty;
}
