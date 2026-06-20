using System;

namespace Softcoinp.ERP.Domain.Entities;

public class SpokespersonHistory
{
    public Guid Id { get; set; }
    public string TenantId { get; set; } = string.Empty;

    public Guid UnitId { get; set; }
    public Unit? Unit { get; set; }

    public Guid? PreviousSpokespersonId { get; set; }
    public Owner? PreviousSpokesperson { get; set; }

    public Guid NewSpokespersonId { get; set; }
    public Owner? NewSpokesperson { get; set; }

    public DateTime ChangedAt { get; set; } = DateTime.UtcNow;
    public string ChangedByUserId { get; set; } = string.Empty;
    public string? ChangeReason { get; set; }
}
