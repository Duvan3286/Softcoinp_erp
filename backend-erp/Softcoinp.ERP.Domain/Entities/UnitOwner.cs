using System;

namespace Softcoinp.ERP.Domain.Entities;

public class UnitOwner
{
    public Guid Id { get; set; }
    public string TenantId { get; set; } = string.Empty;
    
    public Guid UnitId { get; set; }
    public Unit? Unit { get; set; }
    
    public Guid OwnerId { get; set; }
    public Owner? Owner { get; set; }
    
    public decimal OwnershipPercentage { get; set; }
    public bool IsSpokesperson { get; set; }
    public bool ResidesInUnit { get; set; }
    
    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public bool IsActive { get; set; } = true;
}
