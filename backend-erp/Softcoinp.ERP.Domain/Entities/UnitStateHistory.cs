using System;
using Softcoinp.ERP.Domain.Enums;

namespace Softcoinp.ERP.Domain.Entities;

public class UnitStateHistory
{
    public Guid Id { get; set; }
    public string TenantId { get; set; } = string.Empty;
    
    public Guid UnitId { get; set; }
    public Unit? Unit { get; set; }
    
    public UnitStatus PreviousStatus { get; set; }
    public UnitStatus NewStatus { get; set; }
    
    public DateTime ChangeDate { get; set; } = DateTime.UtcNow;
    public string ChangedByUserId { get; set; } = string.Empty;
    public string Reason { get; set; } = string.Empty;
}
