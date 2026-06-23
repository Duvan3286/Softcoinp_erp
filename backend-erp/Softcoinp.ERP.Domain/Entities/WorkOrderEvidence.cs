using System;

namespace Softcoinp.ERP.Domain.Entities;

public class WorkOrderEvidence
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string TenantId { get; set; } = string.Empty;
    public Guid WorkOrderId { get; set; }
    public WorkOrder? WorkOrder { get; set; }
    public string FilePath { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public bool IsBeforeIntervention { get; set; }
    public DateTime CapturedAt { get; set; } = DateTime.UtcNow;
    public string CapturedByUserId { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
