using System;
using Softcoinp.ERP.Domain.Enums;

namespace Softcoinp.ERP.Domain.Entities;

public class WorkOrder
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string TenantId { get; set; } = string.Empty;
    public WorkOrderType OrderType { get; set; }
    public Guid AssetId { get; set; }
    public CommonAsset? Asset { get; set; }
    public string Description { get; set; } = string.Empty;
    public WorkOrderPriority Priority { get; set; } = WorkOrderPriority.Medium;
    public WorkOrderOrigin Origin { get; set; }
    public Guid? RelatedPqrId { get; set; }
    public string? RelatedPqrNumber { get; set; }
    public Guid? AssignedProviderId { get; set; }
    public Provider? AssignedProvider { get; set; }
    public DateTime? ScheduledDate { get; set; }
    public DateTime? ExecutionStartDate { get; set; }
    public DateTime? ExecutionEndDate { get; set; }
    public decimal EstimatedCost { get; set; }
    public decimal ActualCost { get; set; }
    public WorkOrderStatus Status { get; set; } = WorkOrderStatus.PendingAssignment;
    public WorkOrderOutcome? Outcome { get; set; }
    public string OutcomeNotes { get; set; } = string.Empty;
    public bool CostAlertSent { get; set; }
    public string CreatedByUserId { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string? UpdatedByUserId { get; set; }
    public DateTime? UpdatedAt { get; set; }

    public ICollection<WorkOrderEvidence> Evidences { get; set; } = new List<WorkOrderEvidence>();
}
