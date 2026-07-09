using System;
using System.Collections.Generic;

namespace Softcoinp.ERP.WebAPI.DTOs;

// ── Bienes Comunes ──────────────────────────────────────────────

public class CreateCommonAssetRequestDto
{
    public string Name { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public bool IsEssential { get; set; }
    public string? Brand { get; set; }
    public string? Model { get; set; }
    public string? SerialNumber { get; set; }
    public DateTime? AcquisitionDate { get; set; }
    public decimal? AcquisitionValue { get; set; }
    public int? EstimatedUsefulLifeMonths { get; set; }
    public Guid? ReferenceProviderId { get; set; }
    public string? Manufacturer { get; set; }
    public bool HasWarranty { get; set; }
    public DateTime? WarrantyEndDate { get; set; }
    public string? StatusNotes { get; set; }
}

public class UpdateCommonAssetRequestDto
{
    public string? Name { get; set; }
    public string? Category { get; set; }
    public string? Location { get; set; }
    public bool? IsEssential { get; set; }
    public string? Brand { get; set; }
    public string? Model { get; set; }
    public string? SerialNumber { get; set; }
    public DateTime? AcquisitionDate { get; set; }
    public decimal? AcquisitionValue { get; set; }
    public int? EstimatedUsefulLifeMonths { get; set; }
    public Guid? ReferenceProviderId { get; set; }
    public string? Manufacturer { get; set; }
    public bool? HasWarranty { get; set; }
    public DateTime? WarrantyEndDate { get; set; }
    public string? Status { get; set; }
    public string? StatusNotes { get; set; }
}

public class CommonAssetListDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public bool IsEssential { get; set; }
    public string Status { get; set; } = string.Empty;
    public string Brand { get; set; } = string.Empty;
    public string Model { get; set; } = string.Empty;
    public bool HasWarranty { get; set; }
    public DateTime? WarrantyEndDate { get; set; }
    public DateTime? NextMaintenanceDate { get; set; }
    public int PendingWorkOrders { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class CommonAssetDetailDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public bool IsEssential { get; set; }
    public string Brand { get; set; } = string.Empty;
    public string Model { get; set; } = string.Empty;
    public string SerialNumber { get; set; } = string.Empty;
    public DateTime? AcquisitionDate { get; set; }
    public decimal AcquisitionValue { get; set; }
    public int EstimatedUsefulLifeMonths { get; set; }
    public Guid? ReferenceProviderId { get; set; }
    public string ReferenceProviderName { get; set; } = string.Empty;
    public string Manufacturer { get; set; } = string.Empty;
    public bool HasWarranty { get; set; }
    public DateTime? WarrantyEndDate { get; set; }
    public string Status { get; set; } = string.Empty;
    public string StatusNotes { get; set; } = string.Empty;
    public string CreatedByUserId { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public List<AssetPhotoDto> Photos { get; set; } = new();
    public List<MaintenancePlanSummaryDto> MaintenancePlans { get; set; } = new();
    public List<WorkOrderSummaryDto> WorkOrders { get; set; } = new();
    public List<AssetStatusHistoryDto> StatusHistory { get; set; } = new();
}

public class AssetPhotoDto
{
    public Guid Id { get; set; }
    public string FilePath { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime CapturedAt { get; set; }
}

public class AssetStatusHistoryDto
{
    public Guid Id { get; set; }
    public string PreviousStatus { get; set; } = string.Empty;
    public string NewStatus { get; set; } = string.Empty;
    public string Reason { get; set; } = string.Empty;
    public string ChangedByUserName { get; set; } = string.Empty;
    public DateTime ChangedAt { get; set; }
}

// ── Planes de Mantenimiento ────────────────────────────────────

public class CreateMaintenancePlanRequestDto
{
    public Guid AssetId { get; set; }
    public string ActivityType { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int FrequencyDays { get; set; }
    public Guid? PreferredProviderId { get; set; }
    public decimal? EstimatedCost { get; set; }
    public bool RequiresServiceSuspension { get; set; }
    public int? EstimatedDowntimeHours { get; set; }
}

public class UpdateMaintenancePlanRequestDto
{
    public string? ActivityType { get; set; }
    public string? Description { get; set; }
    public int? FrequencyDays { get; set; }
    public Guid? PreferredProviderId { get; set; }
    public decimal? EstimatedCost { get; set; }
    public bool? RequiresServiceSuspension { get; set; }
    public int? EstimatedDowntimeHours { get; set; }
    public bool? IsActive { get; set; }
}

public class MaintenancePlanSummaryDto
{
    public Guid Id { get; set; }
    public string ActivityType { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int FrequencyDays { get; set; }
    public string PreferredProviderName { get; set; } = string.Empty;
    public decimal EstimatedCost { get; set; }
    public bool RequiresServiceSuspension { get; set; }
    public bool IsActive { get; set; }
    public DateTime? LastExecutionDate { get; set; }
    public DateTime? NextExecutionDate { get; set; }
}

// ── Órdenes de Trabajo ─────────────────────────────────────────

public class CreateWorkOrderRequestDto
{
    public string OrderType { get; set; } = string.Empty;
    public Guid AssetId { get; set; }
    public string Description { get; set; } = string.Empty;
    public string Priority { get; set; } = string.Empty;
    public string Origin { get; set; } = string.Empty;
    public Guid? RelatedPqrId { get; set; }
    public Guid? AssignedProviderId { get; set; }
    public DateTime? ScheduledDate { get; set; }
    public decimal? EstimatedCost { get; set; }
    public Guid? ExpenseItemId { get; set; }
}

public class UpdateWorkOrderRequestDto
{
    public string? Description { get; set; }
    public string? Priority { get; set; }
    public Guid? AssignedProviderId { get; set; }
    public DateTime? ScheduledDate { get; set; }
    public DateTime? ExecutionStartDate { get; set; }
    public DateTime? ExecutionEndDate { get; set; }
    public decimal? EstimatedCost { get; set; }
    public decimal? ActualCost { get; set; }
    public Guid? ExpenseItemId { get; set; }
    public string? Status { get; set; }
    public string? Outcome { get; set; }
    public string? OutcomeNotes { get; set; }
}

public class WorkOrderListDto
{
    public Guid Id { get; set; }
    public string OrderType { get; set; } = string.Empty;
    public string AssetName { get; set; } = string.Empty;
    public string AssetLocation { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Priority { get; set; } = string.Empty;
    public string Origin { get; set; } = string.Empty;
    public string AssignedProviderName { get; set; } = string.Empty;
    public DateTime? ScheduledDate { get; set; }
    public DateTime? ExecutionEndDate { get; set; }
    public decimal EstimatedCost { get; set; }
    public decimal ActualCost { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? Outcome { get; set; }
    public string RelatedPqrNumber { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}

public class WorkOrderDetailDto
{
    public Guid Id { get; set; }
    public string OrderType { get; set; } = string.Empty;
    public Guid AssetId { get; set; }
    public string AssetName { get; set; } = string.Empty;
    public string AssetLocation { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Priority { get; set; } = string.Empty;
    public string Origin { get; set; } = string.Empty;
    public Guid? RelatedPqrId { get; set; }
    public string RelatedPqrNumber { get; set; } = string.Empty;
    public Guid? AssignedProviderId { get; set; }
    public string AssignedProviderName { get; set; } = string.Empty;
    public DateTime? ScheduledDate { get; set; }
    public DateTime? ExecutionStartDate { get; set; }
    public DateTime? ExecutionEndDate { get; set; }
    public decimal EstimatedCost { get; set; }
    public decimal ActualCost { get; set; }
    public Guid? ExpenseItemId { get; set; }
    public string ExpenseItemName { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string? Outcome { get; set; }
    public string OutcomeNotes { get; set; } = string.Empty;
    public bool CostAlertSent { get; set; }
    public string CreatedByUserId { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public List<WorkOrderEvidenceDto> Evidences { get; set; } = new();
}

public class WorkOrderSummaryDto
{
    public Guid Id { get; set; }
    public string OrderType { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Priority { get; set; } = string.Empty;
    public string AssignedProviderName { get; set; } = string.Empty;
    public DateTime? ScheduledDate { get; set; }
    public DateTime? ExecutionEndDate { get; set; }
    public decimal ActualCost { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? Outcome { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class WorkOrderEvidenceDto
{
    public Guid Id { get; set; }
    public string FilePath { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public bool IsBeforeIntervention { get; set; }
    public DateTime CapturedAt { get; set; }
}

// ── Siniestros ─────────────────────────────────────────────────

public class CreateIncidentRequestDto
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string IncidentType { get; set; } = string.Empty;
    public DateTime OccurredAt { get; set; }
    public decimal? TotalDamageValue { get; set; }
    public string? InsurancePolicyNumber { get; set; }
    public string? InsuranceCompany { get; set; }
    public List<Guid>? WorkOrderIds { get; set; }
}

public class UpdateIncidentRequestDto
{
    public string? Name { get; set; }
    public string? Description { get; set; }
    public string? IncidentType { get; set; }
    public DateTime? OccurredAt { get; set; }
    public decimal? TotalDamageValue { get; set; }
    public string? InsurancePolicyNumber { get; set; }
    public string? InsuranceCompany { get; set; }
    public string? Status { get; set; }
}

public class IncidentListDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string IncidentType { get; set; } = string.Empty;
    public DateTime OccurredAt { get; set; }
    public decimal TotalDamageValue { get; set; }
    public string InsurancePolicyNumber { get; set; } = string.Empty;
    public string InsuranceCompany { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public int RelatedWorkOrders { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class IncidentDetailDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string IncidentType { get; set; } = string.Empty;
    public DateTime OccurredAt { get; set; }
    public decimal TotalDamageValue { get; set; }
    public string InsurancePolicyNumber { get; set; } = string.Empty;
    public string InsuranceCompany { get; set; } = string.Empty;
    public string PolicyFilePath { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string CreatedByUserId { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public List<WorkOrderSummaryDto> RelatedWorkOrders { get; set; } = new();
}

// ── Reportes ───────────────────────────────────────────────────

public class MaintenanceReportDto
{
    public int DaysAhead { get; set; }
    public decimal TotalEstimatedCost { get; set; }
    public decimal BudgetAvailable { get; set; }
    public List<ScheduledMaintenanceItemDto> ScheduledItems { get; set; } = new();
}

public class ScheduledMaintenanceItemDto
{
    public Guid AssetId { get; set; }
    public string AssetName { get; set; } = string.Empty;
    public string AssetLocation { get; set; } = string.Empty;
    public string ActivityType { get; set; } = string.Empty;
    public DateTime ScheduledDate { get; set; }
    public decimal EstimatedCost { get; set; }
    public string PreferredProviderName { get; set; } = string.Empty;
}

// ── Indicadores ────────────────────────────────────────────────

public class MaintenanceIndicatorsDto
{
    public int TotalAssets { get; set; }
    public int OperationalAssets { get; set; }
    public int OutOfServiceAssets { get; set; }
    public int EssentialAssets { get; set; }
    public int PendingWorkOrders { get; set; }
    public int InProgressWorkOrders { get; set; }
    public int CompletedWorkOrdersLast30Days { get; set; }
    public int UnassignedWorkOrders { get; set; }
    public decimal TotalCostLast30Days { get; set; }
    public int UpcomingMaintenances30Days { get; set; }
}

// ── Bienes Fuera de Servicio ───────────────────────────────────

public class OutOfServiceAssetDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public bool IsEssential { get; set; }
    public DateTime StatusChangedAt { get; set; }
    public int DaysOutOfService { get; set; }
    public string Reason { get; set; } = string.Empty;
}
