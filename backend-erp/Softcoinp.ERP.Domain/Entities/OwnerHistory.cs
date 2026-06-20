using System;

namespace Softcoinp.ERP.Domain.Entities;

public class OwnerHistory
{
    public Guid Id { get; set; }
    public string TenantId { get; set; } = string.Empty;

    public Guid UnitId { get; set; }
    public Unit? Unit { get; set; }

    public Guid OwnerId { get; set; }
    public Owner? Owner { get; set; }

    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public string? TransferNotes { get; set; }

    public DateTime RecordedAt { get; set; } = DateTime.UtcNow;
    public string RecordedByUserId { get; set; } = string.Empty;
}
