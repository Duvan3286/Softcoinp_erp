using System;

namespace Softcoinp.ERP.Domain.Entities;

public class UnitResident
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
    public bool IsActive { get; set; } = true;
}
