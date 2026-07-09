using System;
using Softcoinp.ERP.Domain.Enums;

namespace Softcoinp.ERP.Domain.Entities;

public class MaintenancePlan
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string TenantId { get; set; } = string.Empty;
    public Guid AssetId { get; set; }
    public CommonAsset? Asset { get; set; }
    public MaintenanceActivityType ActivityType { get; set; }
    public string Description { get; set; } = string.Empty;
    public int FrequencyDays { get; set; }
    public Guid? PreferredProviderId { get; set; }
    public Provider? PreferredProvider { get; set; }
    public decimal EstimatedCost { get; set; }
    public Guid? ExpenseItemId { get; set; }
    public ExpenseItem? ExpenseItem { get; set; }
    public bool RequiresServiceSuspension { get; set; }
    public int EstimatedDowntimeHours { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime? LastExecutionDate { get; set; }
    public DateTime? NextExecutionDate { get; set; }
    public string CreatedByUserId { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string? UpdatedByUserId { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
