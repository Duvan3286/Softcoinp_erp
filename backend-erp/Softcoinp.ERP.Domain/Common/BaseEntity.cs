using System;

namespace Softcoinp.ERP.Domain.Common;

/// <summary>
/// Base class for all domain entities, providing common audit properties.
/// </summary>
public abstract class BaseEntity
{
    /// <summary>
    /// Unique identifier for the entity.
    /// </summary>
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// Date and time when the entity was created.
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Date and time when the entity was last updated.
    /// </summary>
    public DateTime? UpdatedAt { get; set; }

    /// <summary>
    /// Indicates whether the entity has been logically deleted (Soft Delete).
    /// </summary>
    public bool IsDeleted { get; set; } = false;
}
