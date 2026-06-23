using System;

namespace Softcoinp.ERP.Domain.Entities;

public class IncidentWorkOrder
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string TenantId { get; set; } = string.Empty;
    public Guid IncidentId { get; set; }
    public Incident? Incident { get; set; }
    public Guid WorkOrderId { get; set; }
    public WorkOrder? WorkOrder { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
