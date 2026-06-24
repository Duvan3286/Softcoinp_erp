using System;
using Softcoinp.ERP.Domain.Enums;

namespace Softcoinp.ERP.Domain.Entities;

public class AssemblyDecisionPropagation
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string TenantId { get; set; } = string.Empty;

    public Guid AssemblyId { get; set; }
    public Assembly? Assembly { get; set; }

    public Guid AgendaItemId { get; set; }
    public AssemblyAgendaItem? AgendaItem { get; set; }

    public DecisionPropagationTarget TargetModule { get; set; }
    public DecisionPropagationStatus Status { get; set; } = DecisionPropagationStatus.Pending;

    public string Description { get; set; } = string.Empty;
    public string? TargetEntityId { get; set; }
    public string? TargetEntityType { get; set; }

    public string? ErrorMessage { get; set; }
    public int RetryCount { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? PropagatedAt { get; set; }
    public string? PropagatedByUserId { get; set; }
}
