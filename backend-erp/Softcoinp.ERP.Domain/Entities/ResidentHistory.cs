using System;

namespace Softcoinp.ERP.Domain.Entities;

public class ResidentHistory
{
    public Guid Id { get; set; }
    public string TenantId { get; set; } = string.Empty;

    public Guid UnitId { get; set; }
    public Unit? Unit { get; set; }

    public Guid ResidentId { get; set; }
    public Resident? Resident { get; set; }

    public string Relationship { get; set; } = string.Empty;

    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public string? TransferNotes { get; set; }

    public DateTime RecordedAt { get; set; } = DateTime.UtcNow;
    public string RecordedByUserId { get; set; } = string.Empty;
}
