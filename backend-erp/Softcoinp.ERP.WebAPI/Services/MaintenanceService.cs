using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Softcoinp.ERP.Domain.Entities;
using Softcoinp.ERP.Domain.Enums;
using Softcoinp.ERP.Infrastructure.Persistence;
using Softcoinp.ERP.WebAPI.DTOs;

namespace Softcoinp.ERP.WebAPI.Services;

public class MaintenanceService
{
    private readonly ApplicationDbContext _context;

    public MaintenanceService(ApplicationDbContext context)
    {
        _context = context;
    }

    // ── Bienes Comunes ──────────────────────────────────────────────

    public async Task<List<CommonAssetListDto>> GetAssetsAsync(
        string tenantId, string? category = null, string? status = null,
        string? location = null, string? search = null)
    {
        var query = _context.CommonAssets.Where(a => a.TenantId == tenantId);

        if (!string.IsNullOrEmpty(category) && Enum.TryParse<AssetCategory>(category, true, out var catEnum))
            query = query.Where(a => a.Category == catEnum);

        if (!string.IsNullOrEmpty(status) && Enum.TryParse<AssetStatus>(status, true, out var stEnum))
            query = query.Where(a => a.Status == stEnum);

        if (!string.IsNullOrEmpty(location))
            query = query.Where(a => a.Location.Contains(location));

        if (!string.IsNullOrEmpty(search))
            query = query.Where(a => a.Name.Contains(search) || a.Brand.Contains(search) || a.Model.Contains(search));

        var now = DateTime.UtcNow;
        return await query
            .OrderByDescending(a => a.CreatedAt)
            .Select(a => new CommonAssetListDto
            {
                Id = a.Id,
                Name = a.Name,
                Category = a.Category.ToString(),
                Location = a.Location,
                IsEssential = a.IsEssential,
                Status = a.Status.ToString(),
                Brand = a.Brand,
                Model = a.Model,
                HasWarranty = a.HasWarranty,
                WarrantyEndDate = a.WarrantyEndDate,
                NextMaintenanceDate = a.MaintenancePlans
                    .Where(p => p.IsActive && p.NextExecutionDate != null)
                    .OrderBy(p => p.NextExecutionDate)
                    .Select(p => p.NextExecutionDate)
                    .FirstOrDefault(),
                PendingWorkOrders = a.WorkOrders.Count(w =>
                    w.Status == WorkOrderStatus.PendingAssignment ||
                    w.Status == WorkOrderStatus.Assigned ||
                    w.Status == WorkOrderStatus.InProgress),
                CreatedAt = a.CreatedAt
            })
            .ToListAsync();
    }

    public async Task<CommonAssetDetailDto> GetAssetByIdAsync(string tenantId, Guid assetId)
    {
        var asset = await _context.CommonAssets
            .Where(a => a.Id == assetId && a.TenantId == tenantId)
            .Select(a => new CommonAssetDetailDto
            {
                Id = a.Id,
                Name = a.Name,
                Category = a.Category.ToString(),
                Location = a.Location,
                IsEssential = a.IsEssential,
                Brand = a.Brand,
                Model = a.Model,
                SerialNumber = a.SerialNumber,
                AcquisitionDate = a.AcquisitionDate,
                AcquisitionValue = a.AcquisitionValue,
                EstimatedUsefulLifeMonths = a.EstimatedUsefulLifeMonths,
                ReferenceProviderId = a.ReferenceProviderId,
                ReferenceProviderName = a.ReferenceProvider != null ? a.ReferenceProvider.BusinessName : string.Empty,
                Manufacturer = a.Manufacturer,
                HasWarranty = a.HasWarranty,
                WarrantyEndDate = a.WarrantyEndDate,
                Status = a.Status.ToString(),
                StatusNotes = a.StatusNotes,
                CreatedByUserId = a.CreatedByUserId,
                CreatedAt = a.CreatedAt,
                UpdatedAt = a.UpdatedAt,
                Photos = a.Photos
                    .OrderByDescending(p => p.CapturedAt)
                    .Select(p => new AssetPhotoDto
                    {
                        Id = p.Id,
                        FilePath = p.FilePath,
                        Description = p.Description,
                        CapturedAt = p.CapturedAt
                    }).ToList(),
                MaintenancePlans = a.MaintenancePlans
                    .Select(p => new MaintenancePlanSummaryDto
                    {
                        Id = p.Id,
                        ActivityType = p.ActivityType.ToString(),
                        Description = p.Description,
                        FrequencyDays = p.FrequencyDays,
                        PreferredProviderName = p.PreferredProvider != null ? p.PreferredProvider.BusinessName : string.Empty,
                        EstimatedCost = p.EstimatedCost,
                        RequiresServiceSuspension = p.RequiresServiceSuspension,
                        IsActive = p.IsActive,
                        LastExecutionDate = p.LastExecutionDate,
                        NextExecutionDate = p.NextExecutionDate
                    }).ToList(),
                WorkOrders = a.WorkOrders
                    .OrderByDescending(w => w.CreatedAt)
                    .Take(20)
                    .Select(w => new WorkOrderSummaryDto
                    {
                        Id = w.Id,
                        OrderType = w.OrderType.ToString(),
                        Description = w.Description,
                        Priority = w.Priority.ToString(),
                        AssignedProviderName = w.AssignedProvider != null ? w.AssignedProvider.BusinessName : string.Empty,
                        ScheduledDate = w.ScheduledDate,
                        ExecutionEndDate = w.ExecutionEndDate,
                        ActualCost = w.ActualCost,
                        Status = w.Status.ToString(),
                        Outcome = w.Outcome != null ? w.Outcome.ToString() : null,
                        CreatedAt = w.CreatedAt
                    }).ToList(),
                StatusHistory = a.StatusHistory
                    .OrderByDescending(h => h.ChangedAt)
                    .Select(h => new AssetStatusHistoryDto
                    {
                        Id = h.Id,
                        PreviousStatus = h.PreviousStatus.ToString(),
                        NewStatus = h.NewStatus.ToString(),
                        Reason = h.Reason,
                        ChangedByUserName = h.ChangedByUserName,
                        ChangedAt = h.ChangedAt
                    }).ToList()
            })
            .FirstOrDefaultAsync();

        if (asset == null) throw new KeyNotFoundException("Bien común no encontrado.");
        return asset;
    }

    public async Task<CommonAssetDetailDto> CreateAssetAsync(string tenantId, string userId, CreateCommonAssetRequestDto request)
    {
        if (!Enum.TryParse<AssetCategory>(request.Category, true, out var category))
            throw new ArgumentException("Categoría inválida.");

        var asset = new CommonAsset
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            Name = request.Name,
            Category = category,
            Location = request.Location,
            IsEssential = request.IsEssential,
            Brand = request.Brand ?? string.Empty,
            Model = request.Model ?? string.Empty,
            SerialNumber = request.SerialNumber ?? string.Empty,
            AcquisitionDate = request.AcquisitionDate,
            AcquisitionValue = request.AcquisitionValue ?? 0,
            EstimatedUsefulLifeMonths = request.EstimatedUsefulLifeMonths ?? 0,
            ReferenceProviderId = request.ReferenceProviderId,
            Manufacturer = request.Manufacturer ?? string.Empty,
            HasWarranty = request.HasWarranty,
            WarrantyEndDate = request.WarrantyEndDate,
            Status = AssetStatus.Operational,
            StatusNotes = request.StatusNotes ?? string.Empty,
            CreatedByUserId = userId,
            CreatedAt = DateTime.UtcNow
        };

        _context.CommonAssets.Add(asset);
        await _context.SaveChangesAsync();
        return await GetAssetByIdAsync(tenantId, asset.Id);
    }

    public async Task<CommonAssetDetailDto> UpdateAssetAsync(string tenantId, string userId, Guid assetId, UpdateCommonAssetRequestDto request)
    {
        var asset = await _context.CommonAssets
            .FirstOrDefaultAsync(a => a.Id == assetId && a.TenantId == tenantId);
        if (asset == null) throw new KeyNotFoundException("Bien común no encontrado.");

        if (request.Name != null) asset.Name = request.Name;
        if (request.Category != null && Enum.TryParse<AssetCategory>(request.Category, true, out var cat))
            asset.Category = cat;
        if (request.Location != null) asset.Location = request.Location;
        if (request.IsEssential != null) asset.IsEssential = request.IsEssential.Value;
        if (request.Brand != null) asset.Brand = request.Brand;
        if (request.Model != null) asset.Model = request.Model;
        if (request.SerialNumber != null) asset.SerialNumber = request.SerialNumber;
        if (request.AcquisitionDate != null) asset.AcquisitionDate = request.AcquisitionDate;
        if (request.AcquisitionValue != null) asset.AcquisitionValue = request.AcquisitionValue.Value;
        if (request.EstimatedUsefulLifeMonths != null) asset.EstimatedUsefulLifeMonths = request.EstimatedUsefulLifeMonths.Value;
        if (request.ReferenceProviderId != null) asset.ReferenceProviderId = request.ReferenceProviderId;
        if (request.Manufacturer != null) asset.Manufacturer = request.Manufacturer;
        if (request.HasWarranty != null) asset.HasWarranty = request.HasWarranty.Value;
        if (request.WarrantyEndDate != null) asset.WarrantyEndDate = request.WarrantyEndDate;
        if (request.StatusNotes != null) asset.StatusNotes = request.StatusNotes;

        if (request.Status != null && Enum.TryParse<AssetStatus>(request.Status, true, out var newStatus))
        {
            if (asset.Status != newStatus)
            {
                var history = new AssetStatusHistory
                {
                    Id = Guid.NewGuid(),
                    TenantId = tenantId,
                    AssetId = assetId,
                    PreviousStatus = asset.Status,
                    NewStatus = newStatus,
                    Reason = request.StatusNotes ?? "Cambio de estado",
                    ChangedByUserId = userId,
                    ChangedByUserName = userId,
                    ChangedAt = DateTime.UtcNow,
                    CreatedAt = DateTime.UtcNow
                };
                _context.AssetStatusHistories.Add(history);
                asset.Status = newStatus;
            }
        }

        asset.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        return await GetAssetByIdAsync(tenantId, asset.Id);
    }

    public async Task DeleteAssetAsync(string tenantId, Guid assetId)
    {
        var asset = await _context.CommonAssets
            .FirstOrDefaultAsync(a => a.Id == assetId && a.TenantId == tenantId);
        if (asset == null) throw new KeyNotFoundException("Bien común no encontrado.");

        var hasActiveOrders = await _context.WorkOrders
            .AnyAsync(w => w.AssetId == assetId &&
                (w.Status == WorkOrderStatus.PendingAssignment ||
                 w.Status == WorkOrderStatus.Assigned ||
                 w.Status == WorkOrderStatus.InProgress));
        if (hasActiveOrders)
            throw new InvalidOperationException("No se puede eliminar el bien porque tiene órdenes de trabajo activas.");

        asset.Status = AssetStatus.Decommissioned;
        asset.IsDeleted = true;
        asset.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
    }

    // ── Planes de Mantenimiento ─────────────────────────────────────

    public async Task<MaintenancePlanSummaryDto> CreateMaintenancePlanAsync(string tenantId, string userId, CreateMaintenancePlanRequestDto request)
    {
        if (!Enum.TryParse<MaintenanceActivityType>(request.ActivityType, true, out var activityType))
            throw new ArgumentException("Tipo de actividad inválido.");

        var plan = new MaintenancePlan
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            AssetId = request.AssetId,
            ActivityType = activityType,
            Description = request.Description,
            FrequencyDays = request.FrequencyDays,
            PreferredProviderId = request.PreferredProviderId,
            EstimatedCost = request.EstimatedCost ?? 0,
            RequiresServiceSuspension = request.RequiresServiceSuspension,
            EstimatedDowntimeHours = request.EstimatedDowntimeHours ?? 0,
            IsActive = true,
            NextExecutionDate = DateTime.UtcNow.AddDays(request.FrequencyDays),
            CreatedByUserId = userId,
            CreatedAt = DateTime.UtcNow
        };

        _context.MaintenancePlans.Add(plan);
        await _context.SaveChangesAsync();

        return new MaintenancePlanSummaryDto
        {
            Id = plan.Id,
            ActivityType = plan.ActivityType.ToString(),
            Description = plan.Description,
            FrequencyDays = plan.FrequencyDays,
            EstimatedCost = plan.EstimatedCost,
            RequiresServiceSuspension = plan.RequiresServiceSuspension,
            IsActive = plan.IsActive,
            NextExecutionDate = plan.NextExecutionDate
        };
    }

    public async Task<MaintenancePlanSummaryDto> UpdateMaintenancePlanAsync(string tenantId, string userId, Guid planId, UpdateMaintenancePlanRequestDto request)
    {
        var plan = await _context.MaintenancePlans
            .FirstOrDefaultAsync(p => p.Id == planId && p.TenantId == tenantId);
        if (plan == null) throw new KeyNotFoundException("Plan de mantenimiento no encontrado.");

        if (request.ActivityType != null && Enum.TryParse<MaintenanceActivityType>(request.ActivityType, true, out var act))
            plan.ActivityType = act;
        if (request.Description != null) plan.Description = request.Description;
        if (request.FrequencyDays != null) plan.FrequencyDays = request.FrequencyDays.Value;
        if (request.PreferredProviderId != null) plan.PreferredProviderId = request.PreferredProviderId;
        if (request.EstimatedCost != null) plan.EstimatedCost = request.EstimatedCost.Value;
        if (request.RequiresServiceSuspension != null) plan.RequiresServiceSuspension = request.RequiresServiceSuspension.Value;
        if (request.EstimatedDowntimeHours != null) plan.EstimatedDowntimeHours = request.EstimatedDowntimeHours.Value;
        if (request.IsActive != null) plan.IsActive = request.IsActive.Value;

        plan.UpdatedByUserId = userId;
        plan.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return new MaintenancePlanSummaryDto
        {
            Id = plan.Id,
            ActivityType = plan.ActivityType.ToString(),
            Description = plan.Description,
            FrequencyDays = plan.FrequencyDays,
            EstimatedCost = plan.EstimatedCost,
            RequiresServiceSuspension = plan.RequiresServiceSuspension,
            IsActive = plan.IsActive,
            LastExecutionDate = plan.LastExecutionDate,
            NextExecutionDate = plan.NextExecutionDate
        };
    }

    public async Task DeleteMaintenancePlanAsync(string tenantId, Guid planId)
    {
        var plan = await _context.MaintenancePlans
            .FirstOrDefaultAsync(p => p.Id == planId && p.TenantId == tenantId);
        if (plan == null) throw new KeyNotFoundException("Plan de mantenimiento no encontrado.");

        _context.MaintenancePlans.Remove(plan);
        await _context.SaveChangesAsync();
    }

    // ── Órdenes de Trabajo ─────────────────────────────────────────

    public async Task<List<WorkOrderListDto>> GetWorkOrdersAsync(
        string tenantId, string? orderType = null, string? status = null,
        string? priority = null, string? assignedProviderId = null, string? search = null)
    {
        var query = _context.WorkOrders.Where(w => w.TenantId == tenantId);

        if (!string.IsNullOrEmpty(orderType) && Enum.TryParse<WorkOrderType>(orderType, true, out var typeEnum))
            query = query.Where(w => w.OrderType == typeEnum);

        if (!string.IsNullOrEmpty(status) && Enum.TryParse<WorkOrderStatus>(status, true, out var stEnum))
            query = query.Where(w => w.Status == stEnum);

        if (!string.IsNullOrEmpty(priority) && Enum.TryParse<WorkOrderPriority>(priority, true, out var priEnum))
            query = query.Where(w => w.Priority == priEnum);

        if (!string.IsNullOrEmpty(assignedProviderId) && Guid.TryParse(assignedProviderId, out var provId))
            query = query.Where(w => w.AssignedProviderId == provId);

        if (!string.IsNullOrEmpty(search))
            query = query.Where(w => w.Description.Contains(search) || w.Asset!.Name.Contains(search));

        return await query
            .OrderByDescending(w => w.CreatedAt)
            .Select(w => new WorkOrderListDto
            {
                Id = w.Id,
                OrderType = w.OrderType.ToString(),
                AssetName = w.Asset != null ? w.Asset.Name : string.Empty,
                AssetLocation = w.Asset != null ? w.Asset.Location : string.Empty,
                Description = w.Description,
                Priority = w.Priority.ToString(),
                Origin = w.Origin.ToString(),
                AssignedProviderName = w.AssignedProvider != null ? w.AssignedProvider.BusinessName : string.Empty,
                ScheduledDate = w.ScheduledDate,
                ExecutionEndDate = w.ExecutionEndDate,
                EstimatedCost = w.EstimatedCost,
                ActualCost = w.ActualCost,
                Status = w.Status.ToString(),
                Outcome = w.Outcome != null ? w.Outcome.ToString() : null,
                RelatedPqrNumber = w.RelatedPqrNumber ?? string.Empty,
                CreatedAt = w.CreatedAt
            })
            .ToListAsync();
    }

    public async Task<WorkOrderDetailDto> GetWorkOrderByIdAsync(string tenantId, Guid workOrderId)
    {
        var order = await _context.WorkOrders
            .Where(w => w.Id == workOrderId && w.TenantId == tenantId)
            .Select(w => new WorkOrderDetailDto
            {
                Id = w.Id,
                OrderType = w.OrderType.ToString(),
                AssetId = w.AssetId,
                AssetName = w.Asset != null ? w.Asset.Name : string.Empty,
                AssetLocation = w.Asset != null ? w.Asset.Location : string.Empty,
                Description = w.Description,
                Priority = w.Priority.ToString(),
                Origin = w.Origin.ToString(),
                RelatedPqrId = w.RelatedPqrId,
                RelatedPqrNumber = w.RelatedPqrNumber ?? string.Empty,
                AssignedProviderId = w.AssignedProviderId,
                AssignedProviderName = w.AssignedProvider != null ? w.AssignedProvider.BusinessName : string.Empty,
                ScheduledDate = w.ScheduledDate,
                ExecutionStartDate = w.ExecutionStartDate,
                ExecutionEndDate = w.ExecutionEndDate,
                EstimatedCost = w.EstimatedCost,
                ActualCost = w.ActualCost,
                BudgetAccountId = w.BudgetAccountId,
                BudgetAccountName = w.BudgetAccount != null ? w.BudgetAccount.Name : string.Empty,
                Status = w.Status.ToString(),
                Outcome = w.Outcome != null ? w.Outcome.ToString() : null,
                OutcomeNotes = w.OutcomeNotes,
                CostAlertSent = w.CostAlertSent,
                CreatedByUserId = w.CreatedByUserId,
                CreatedAt = w.CreatedAt,
                UpdatedAt = w.UpdatedAt,
                Evidences = w.Evidences
                    .Select(e => new WorkOrderEvidenceDto
                    {
                        Id = e.Id,
                        FilePath = e.FilePath,
                        Description = e.Description,
                        IsBeforeIntervention = e.IsBeforeIntervention,
                        CapturedAt = e.CapturedAt
                    }).ToList()
            })
            .FirstOrDefaultAsync();

        if (order == null) throw new KeyNotFoundException("Orden de trabajo no encontrada.");
        return order;
    }

    public async Task<WorkOrderDetailDto> CreateWorkOrderAsync(string tenantId, string userId, CreateWorkOrderRequestDto request)
    {
        if (!Enum.TryParse<WorkOrderType>(request.OrderType, true, out var orderType))
            throw new ArgumentException("Tipo de orden inválido.");
        if (!Enum.TryParse<WorkOrderPriority>(request.Priority, true, out var priority))
            throw new ArgumentException("Prioridad inválida.");
        if (!Enum.TryParse<WorkOrderOrigin>(request.Origin, true, out var origin))
            throw new ArgumentException("Origen inválido.");

        var asset = await _context.CommonAssets
            .FirstOrDefaultAsync(a => a.Id == request.AssetId && a.TenantId == tenantId);
        if (asset == null) throw new KeyNotFoundException("Bien común no encontrado.");

        string? relatedPqrNumber = null;
        if (request.RelatedPqrId.HasValue)
        {
            var pqr = await _context.PqrRecords
                .FirstOrDefaultAsync(p => p.Id == request.RelatedPqrId.Value && p.TenantId == tenantId);
            if (pqr != null) relatedPqrNumber = pqr.RadicadoNumber;
        }

        var order = new WorkOrder
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            OrderType = orderType,
            AssetId = request.AssetId,
            Description = request.Description,
            Priority = priority,
            Origin = origin,
            RelatedPqrId = request.RelatedPqrId,
            RelatedPqrNumber = relatedPqrNumber,
            AssignedProviderId = request.AssignedProviderId,
            ScheduledDate = request.ScheduledDate,
            EstimatedCost = request.EstimatedCost ?? 0,
            BudgetAccountId = request.BudgetAccountId,
            Status = request.AssignedProviderId.HasValue ? WorkOrderStatus.Assigned : WorkOrderStatus.PendingAssignment,
            CreatedByUserId = userId,
            CreatedAt = DateTime.UtcNow
        };

        _context.WorkOrders.Add(order);
        await _context.SaveChangesAsync();
        return await GetWorkOrderByIdAsync(tenantId, order.Id);
    }

    public async Task<WorkOrderDetailDto> UpdateWorkOrderAsync(string tenantId, string userId, Guid workOrderId, UpdateWorkOrderRequestDto request)
    {
        var order = await _context.WorkOrders
            .Include(w => w.Asset)
            .FirstOrDefaultAsync(w => w.Id == workOrderId && w.TenantId == tenantId);
        if (order == null) throw new KeyNotFoundException("Orden de trabajo no encontrada.");

        var previousStatus = order.Status;

        if (request.Description != null) order.Description = request.Description;
        if (request.Priority != null && Enum.TryParse<WorkOrderPriority>(request.Priority, true, out var pri))
            order.Priority = pri;
        if (request.AssignedProviderId != null)
        {
            order.AssignedProviderId = request.AssignedProviderId;
            if (order.Status == WorkOrderStatus.PendingAssignment)
                order.Status = WorkOrderStatus.Assigned;
        }
        if (request.ScheduledDate != null) order.ScheduledDate = request.ScheduledDate;
        if (request.ExecutionStartDate != null) order.ExecutionStartDate = request.ExecutionStartDate;
        if (request.EstimatedCost != null) order.EstimatedCost = request.EstimatedCost.Value;
        if (request.ActualCost != null) order.ActualCost = request.ActualCost.Value;
        if (request.BudgetAccountId != null) order.BudgetAccountId = request.BudgetAccountId;
        if (request.Outcome != null && Enum.TryParse<WorkOrderOutcome>(request.Outcome, true, out var outcome))
            order.Outcome = outcome;
        if (request.OutcomeNotes != null) order.OutcomeNotes = request.OutcomeNotes;

        if (request.Status != null && Enum.TryParse<WorkOrderStatus>(request.Status, true, out var newStatus))
        {
            order.Status = newStatus;

            if (newStatus == WorkOrderStatus.InProgress && order.ExecutionStartDate == null)
                order.ExecutionStartDate = DateTime.UtcNow;

            if (newStatus == WorkOrderStatus.Completed)
            {
                order.ExecutionEndDate = DateTime.UtcNow;

                if (order.ActualCost > 0 && order.EstimatedCost > 0)
                {
                    var deviation = Math.Abs(order.ActualCost - order.EstimatedCost) / order.EstimatedCost;
                    if (deviation > 0.20m && !order.CostAlertSent)
                    {
                        order.CostAlertSent = true;
                    }
                }

                if (order.AssetId != Guid.Empty)
                {
                    var plans = await _context.MaintenancePlans
                        .Where(p => p.AssetId == order.AssetId && p.IsActive)
                        .ToListAsync();
                    foreach (var plan in plans)
                    {
                        plan.LastExecutionDate = DateTime.UtcNow;
                        plan.NextExecutionDate = DateTime.UtcNow.AddDays(plan.FrequencyDays);
                    }
                }

                if (order.Origin == WorkOrderOrigin.ResidentPqr && order.RelatedPqrId.HasValue)
                {
                    var pqr = await _context.PqrRecords
                        .FirstOrDefaultAsync(p => p.Id == order.RelatedPqrId.Value);
                    if (pqr != null && pqr.Status != PQRStatus.Closed)
                    {
                        pqr.Status = PQRStatus.Responded;
                        pqr.UpdatedAt = DateTime.UtcNow;
                    }
                }
            }
        }

        order.UpdatedByUserId = userId;
        order.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        return await GetWorkOrderByIdAsync(tenantId, order.Id);
    }

    public async Task DeleteWorkOrderAsync(string tenantId, Guid workOrderId)
    {
        var order = await _context.WorkOrders
            .FirstOrDefaultAsync(w => w.Id == workOrderId && w.TenantId == tenantId);
        if (order == null) throw new KeyNotFoundException("Orden de trabajo no encontrada.");

        if (order.Status == WorkOrderStatus.InProgress)
            throw new InvalidOperationException("No se puede eliminar una orden de trabajo en ejecución.");

        _context.WorkOrders.Remove(order);
        await _context.SaveChangesAsync();
    }

    // ── Siniestros ─────────────────────────────────────────────────

    public async Task<List<IncidentListDto>> GetIncidentsAsync(string tenantId, string? status = null)
    {
        var query = _context.Incidents.Where(i => i.TenantId == tenantId);

        if (!string.IsNullOrEmpty(status))
            query = query.Where(i => i.Status == status);

        return await query
            .OrderByDescending(i => i.CreatedAt)
            .Select(i => new IncidentListDto
            {
                Id = i.Id,
                Name = i.Name,
                IncidentType = i.IncidentType.ToString(),
                OccurredAt = i.OccurredAt,
                TotalDamageValue = i.TotalDamageValue,
                InsurancePolicyNumber = i.InsurancePolicyNumber,
                InsuranceCompany = i.InsuranceCompany,
                Status = i.Status,
                RelatedWorkOrders = i.IncidentWorkOrders.Count,
                CreatedAt = i.CreatedAt
            })
            .ToListAsync();
    }

    public async Task<IncidentDetailDto> GetIncidentByIdAsync(string tenantId, Guid incidentId)
    {
        var incident = await _context.Incidents
            .Where(i => i.Id == incidentId && i.TenantId == tenantId)
            .Select(i => new IncidentDetailDto
            {
                Id = i.Id,
                Name = i.Name,
                Description = i.Description,
                IncidentType = i.IncidentType.ToString(),
                OccurredAt = i.OccurredAt,
                TotalDamageValue = i.TotalDamageValue,
                InsurancePolicyNumber = i.InsurancePolicyNumber,
                InsuranceCompany = i.InsuranceCompany,
                PolicyFilePath = i.PolicyFilePath,
                Status = i.Status,
                CreatedByUserId = i.CreatedByUserId,
                CreatedAt = i.CreatedAt,
                UpdatedAt = i.UpdatedAt,
                RelatedWorkOrders = i.IncidentWorkOrders
                    .Select(iw => iw.WorkOrder)
                    .Where(w => w != null)
                    .Select(w => new WorkOrderSummaryDto
                    {
                        Id = w!.Id,
                        OrderType = w.OrderType.ToString(),
                        Description = w.Description,
                        Priority = w.Priority.ToString(),
                        AssignedProviderName = w.AssignedProvider != null ? w.AssignedProvider.BusinessName : string.Empty,
                        ScheduledDate = w.ScheduledDate,
                        ExecutionEndDate = w.ExecutionEndDate,
                        ActualCost = w.ActualCost,
                        Status = w.Status.ToString(),
                        Outcome = w.Outcome != null ? w.Outcome.ToString() : null,
                        CreatedAt = w.CreatedAt
                    }).ToList()
            })
            .FirstOrDefaultAsync();

        if (incident == null) throw new KeyNotFoundException("Siniestro no encontrado.");
        return incident;
    }

    public async Task<IncidentDetailDto> CreateIncidentAsync(string tenantId, string userId, CreateIncidentRequestDto request)
    {
        if (!Enum.TryParse<IncidentType>(request.IncidentType, true, out var incidentType))
            throw new ArgumentException("Tipo de siniestro inválido.");

        var incident = new Incident
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            Name = request.Name,
            Description = request.Description,
            IncidentType = incidentType,
            OccurredAt = request.OccurredAt,
            TotalDamageValue = request.TotalDamageValue ?? 0,
            InsurancePolicyNumber = request.InsurancePolicyNumber ?? string.Empty,
            InsuranceCompany = request.InsuranceCompany ?? string.Empty,
            Status = "Open",
            CreatedByUserId = userId,
            CreatedAt = DateTime.UtcNow
        };

        _context.Incidents.Add(incident);

        if (request.WorkOrderIds != null && request.WorkOrderIds.Count > 0)
        {
            foreach (var woId in request.WorkOrderIds)
            {
                _context.IncidentWorkOrders.Add(new IncidentWorkOrder
                {
                    Id = Guid.NewGuid(),
                    TenantId = tenantId,
                    IncidentId = incident.Id,
                    WorkOrderId = woId,
                    CreatedAt = DateTime.UtcNow
                });
            }
        }

        await _context.SaveChangesAsync();
        return await GetIncidentByIdAsync(tenantId, incident.Id);
    }

    public async Task<IncidentDetailDto> UpdateIncidentAsync(string tenantId, string userId, Guid incidentId, UpdateIncidentRequestDto request)
    {
        var incident = await _context.Incidents
            .FirstOrDefaultAsync(i => i.Id == incidentId && i.TenantId == tenantId);
        if (incident == null) throw new KeyNotFoundException("Siniestro no encontrado.");

        if (request.Name != null) incident.Name = request.Name;
        if (request.Description != null) incident.Description = request.Description;
        if (request.IncidentType != null && Enum.TryParse<IncidentType>(request.IncidentType, true, out var it))
            incident.IncidentType = it;
        if (request.OccurredAt != null) incident.OccurredAt = request.OccurredAt.Value;
        if (request.TotalDamageValue != null) incident.TotalDamageValue = request.TotalDamageValue.Value;
        if (request.InsurancePolicyNumber != null) incident.InsurancePolicyNumber = request.InsurancePolicyNumber;
        if (request.InsuranceCompany != null) incident.InsuranceCompany = request.InsuranceCompany;
        if (request.Status != null) incident.Status = request.Status;

        incident.UpdatedByUserId = userId;
        incident.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        return await GetIncidentByIdAsync(tenantId, incident.Id);
    }

    // ── Reportes ───────────────────────────────────────────────────

    public async Task<MaintenanceReportDto> GetScheduledMaintenanceReportAsync(string tenantId, int daysAhead, Guid? budgetAccountId = null)
    {
        var now = DateTime.UtcNow;
        var cutoff = now.AddDays(daysAhead);

        var items = await _context.MaintenancePlans
            .Where(p => p.TenantId == tenantId &&
                p.IsActive &&
                p.NextExecutionDate != null &&
                p.NextExecutionDate >= now &&
                p.NextExecutionDate <= cutoff)
            .OrderBy(p => p.NextExecutionDate)
            .Select(p => new ScheduledMaintenanceItemDto
            {
                AssetId = p.Asset!.Id,
                AssetName = p.Asset.Name,
                AssetLocation = p.Asset.Location,
                ActivityType = p.ActivityType.ToString(),
                ScheduledDate = p.NextExecutionDate!.Value,
                EstimatedCost = p.EstimatedCost,
                PreferredProviderName = p.PreferredProvider != null ? p.PreferredProvider.BusinessName : string.Empty
            })
            .ToListAsync();

        var totalEstimated = items.Sum(i => i.EstimatedCost);

        decimal budgetAvailable = 0;
        if (budgetAccountId.HasValue)
        {
            var exists = await _context.AccountingAccounts
                .AnyAsync(a => a.Id == budgetAccountId.Value);
            if (exists) budgetAvailable = 0;
        }

        return new MaintenanceReportDto
        {
            DaysAhead = daysAhead,
            TotalEstimatedCost = totalEstimated,
            BudgetAvailable = budgetAvailable,
            ScheduledItems = items
        };
    }

    // ── Indicadores ────────────────────────────────────────────────

    public async Task<MaintenanceIndicatorsDto> GetIndicatorsAsync(string tenantId)
    {
        var now = DateTime.UtcNow;
        var thirtyDaysAgo = now.AddDays(-30);

        var totalAssets = await _context.CommonAssets
            .CountAsync(a => a.TenantId == tenantId);

        var operationalAssets = await _context.CommonAssets
            .CountAsync(a => a.TenantId == tenantId &&
                (a.Status == AssetStatus.Operational || a.Status == AssetStatus.OperationalWithObservations));

        var outOfServiceAssets = await _context.CommonAssets
            .CountAsync(a => a.TenantId == tenantId && a.Status == AssetStatus.OutOfService);

        var essentialAssets = await _context.CommonAssets
            .CountAsync(a => a.TenantId == tenantId && a.IsEssential);

        var pendingWorkOrders = await _context.WorkOrders
            .CountAsync(w => w.TenantId == tenantId && w.Status == WorkOrderStatus.PendingAssignment);

        var inProgressWorkOrders = await _context.WorkOrders
            .CountAsync(w => w.TenantId == tenantId && w.Status == WorkOrderStatus.InProgress);

        var completedLast30 = await _context.WorkOrders
            .CountAsync(w => w.TenantId == tenantId &&
                w.Status == WorkOrderStatus.Completed &&
                w.ExecutionEndDate != null &&
                w.ExecutionEndDate >= thirtyDaysAgo);

        var unassignedOrders = await _context.WorkOrders
            .CountAsync(w => w.TenantId == tenantId &&
                w.Status == WorkOrderStatus.PendingAssignment &&
                w.ScheduledDate != null &&
                w.ScheduledDate <= now);

        var totalCostLast30 = await _context.WorkOrders
            .Where(w => w.TenantId == tenantId &&
                w.Status == WorkOrderStatus.Completed &&
                w.ExecutionEndDate != null &&
                w.ExecutionEndDate >= thirtyDaysAgo)
            .SumAsync(w => w.ActualCost);

        var upcomingMaintenances = await _context.MaintenancePlans
            .CountAsync(p => p.TenantId == tenantId &&
                p.IsActive &&
                p.NextExecutionDate != null &&
                p.NextExecutionDate >= now &&
                p.NextExecutionDate <= now.AddDays(30));

        return new MaintenanceIndicatorsDto
        {
            TotalAssets = totalAssets,
            OperationalAssets = operationalAssets,
            OutOfServiceAssets = outOfServiceAssets,
            EssentialAssets = essentialAssets,
            PendingWorkOrders = pendingWorkOrders,
            InProgressWorkOrders = inProgressWorkOrders,
            CompletedWorkOrdersLast30Days = completedLast30,
            UnassignedWorkOrders = unassignedOrders,
            TotalCostLast30Days = totalCostLast30,
            UpcomingMaintenances30Days = upcomingMaintenances
        };
    }

    // ── Bienes Fuera de Servicio ───────────────────────────────────

    public async Task<List<OutOfServiceAssetDto>> GetOutOfServiceAssetsAsync(string tenantId)
    {
        var assets = await _context.CommonAssets
            .Where(a => a.TenantId == tenantId && a.Status == AssetStatus.OutOfService)
            .ToListAsync();

        var assetIds = assets.Select(a => a.Id).ToList();

        var lastHistories = await _context.AssetStatusHistories
            .Where(h => assetIds.Contains(h.AssetId) && h.NewStatus == AssetStatus.OutOfService)
            .GroupBy(h => h.AssetId)
            .Select(g => g.OrderByDescending(h => h.ChangedAt).First())
            .ToDictionaryAsync(h => h.AssetId);

        var result = new List<OutOfServiceAssetDto>();
        foreach (var asset in assets)
        {
            lastHistories.TryGetValue(asset.Id, out var lastHistory);

            var statusChangedAt = lastHistory?.ChangedAt ?? asset.CreatedAt;
            var daysOut = (int)(DateTime.UtcNow - statusChangedAt).TotalDays;

            result.Add(new OutOfServiceAssetDto
            {
                Id = asset.Id,
                Name = asset.Name,
                Category = asset.Category.ToString(),
                Location = asset.Location,
                IsEssential = asset.IsEssential,
                StatusChangedAt = statusChangedAt,
                DaysOutOfService = daysOut,
                Reason = lastHistory?.Reason ?? string.Empty
            });
        }

        return result.OrderByDescending(a => a.DaysOutOfService).ToList();
    }
}
