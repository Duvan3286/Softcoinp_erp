using System;

namespace Softcoinp.ERP.Domain.Entities;

public class UnitComplement
{
    public Guid Id { get; set; }
    public string TenantId { get; set; } = string.Empty;
    
    public Guid ParentUnitId { get; set; }
    public Unit? ParentUnit { get; set; }
    
    public Guid ComplementUnitId { get; set; }
    public Unit? ComplementUnit { get; set; }
    
    public string ComplementType { get; set; } = string.Empty; // e.g., Parking, Storage
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string CreatedByUserId { get; set; } = string.Empty;
}
