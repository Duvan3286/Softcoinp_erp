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
    private readonly NotificationEngine _notificationEngine;

    public MaintenanceService(ApplicationDbContext context, NotificationEngine notificationEngine)
    {
        _context = context;
        _notificationEngine = notificationEngine;
    }

    // ── Common Assets ─────────────────────────────────────────────

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
                ReservableSpaceId = a.ReservableSpaceId,
                ReservableSpaceName = a.ReservableSpace != null ? a.ReservableSpace.Name : string.Empty,
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

        if (asset == null) throw new KeyNotFoundException("Bien comun no encontrado.");
        return asset;
    }

    public async Task<CommonAssetDetailDto> CreateAssetAsync(string tenantId, string userId, CreateCommonAssetRequestDto request)
    {
        if (!Enum.TryParse<AssetCategory>(request.Category, true, out var category))
            throw new ArgumentException("Categoria invalida.");

        if (request.ReservableSpaceId.HasValue)
        {
            var spaceExists = await _context.ReservableSpaces
                .AnyAsync(s => s.Id == request.ReservableSpaceId.Value && s.TenantId == tenantId);
            if (!spaceExists)
                throw new KeyNotFoundException("El espacio reservable especificado no existe.");
        }

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
            ReservableSpaceId = request.ReservableSpaceId,
            Manufacturer = request.Manufacturer ?? string.Empty,
            HasWarranty = request.HasWarranty,
            WarrantyEndDate = request.WarrantyEndDate,
            Status = AssetStatus.Operational,
            StatusNotes = request.StatusNotes ?? string.Empty,
            CreatedByUserId = userId,
            CreatedAt = DateTime.UtcNow
        };

        _context.CommonAssets.Add(asset);

        _context.AssetStatusHistories.Add(new AssetStatusHistory
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            AssetId = asset.Id,
            PreviousStatus = AssetStatus.Operational,
            NewStatus = AssetStatus.Operational,
            Reason = "Bien registrado en el inventario.",
            ChangedByUserId = userId,
            ChangedByUserName = userId,
            ChangedAt = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow
        });

        await _context.SaveChangesAsync();
        return await GetAssetByIdAsync(tenantId, asset.Id);
    }

    public async Task<CommonAssetDetailDto> UpdateAssetAsync(string tenantId, string userId, Guid assetId, UpdateCommonAssetRequestDto request)
    {
        var asset = await _context.CommonAssets
            .FirstOrDefaultAsync(a => a.Id == assetId && a.TenantId == tenantId);
        if (asset == null) throw new KeyNotFoundException("Bien comun no encontrado.");

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

        if (request.ReservableSpaceId.HasValue)
        {
            var spaceExists = await _context.ReservableSpaces
                .AnyAsync(s => s.Id == request.ReservableSpaceId.Value && s.TenantId == tenantId);
            if (!spaceExists)
                throw new KeyNotFoundException("El espacio reservable especificado no existe.");
            asset.ReservableSpaceId = request.ReservableSpaceId;
        }

        if (request.Manufacturer != null) asset.Manufacturer = request.Manufacturer;
        if (request.HasWarranty != null) asset.HasWarranty = request.HasWarranty.Value;
        if (request.WarrantyEndDate != null) asset.WarrantyEndDate = request.WarrantyEndDate;
        if (request.StatusNotes != null) asset.StatusNotes = request.StatusNotes;

        if (request.Status != null && Enum.TryParse<AssetStatus>(request.Status, true, out var newStatus))
        {
            if (asset.Status != newStatus)
            {
                var previousStatus = asset.Status;
                asset.Status = newStatus;

                _context.AssetStatusHistories.Add(new AssetStatusHistory
                {
                    Id = Guid.NewGuid(),
                    TenantId = tenantId,
                    AssetId = assetId,
                    PreviousStatus = previousStatus,
                    NewStatus = newStatus,
                    Reason = request.StatusNotes ?? "Cambio de estado",
                    ChangedByUserId = userId,
                    ChangedByUserName = userId,
                    ChangedAt = DateTime.UtcNow,
                    CreatedAt = DateTime.UtcNow
                });

                if (newStatus == AssetStatus.OutOfService)
                {
                    await BlockAssociatedReservableSpacesAsync(tenantId, userId, asset);
                }
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
        if (asset == null) throw new KeyNotFoundException("Bien comun no encontrado.");

        var hasActiveOrders = await _context.WorkOrders
            .AnyAsync(w => w.AssetId == assetId && w.TenantId == tenantId &&
                (w.Status == WorkOrderStatus.PendingAssignment ||
                 w.Status == WorkOrderStatus.Assigned ||
                 w.Status == WorkOrderStatus.InProgress));
        if (hasActiveOrders)
            throw new InvalidOperationException("No se puede eliminar el bien porque tiene ordenes de trabajo activas.");

        asset.Status = AssetStatus.Decommissioned;
        asset.IsDeleted = true;
        asset.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
    }

    // ── Maintenance Plans ─────────────────────────────────────────

    public async Task<MaintenancePlanSummaryDto> CreateMaintenancePlanAsync(string tenantId, string userId, CreateMaintenancePlanRequestDto request)
    {
        if (!Enum.TryParse<MaintenanceActivityType>(request.ActivityType, true, out var activityType))
            throw new ArgumentException("Tipo de actividad invalido.");

        var assetExists = await _context.CommonAssets
            .AnyAsync(a => a.Id == request.AssetId && a.TenantId == tenantId);
        if (!assetExists)
            throw new KeyNotFoundException("El bien comun especificado no existe.");

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

    // ── Work Orders ───────────────────────────────────────────────

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
                BudgetItemId = w.BudgetItemId,
                BudgetItemName = w.BudgetItem != null ? w.BudgetItem.Name : string.Empty,
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
            throw new ArgumentException("Tipo de orden invalido.");
        if (!Enum.TryParse<WorkOrderPriority>(request.Priority, true, out var priority))
            throw new ArgumentException("Prioridad invalida.");
        if (!Enum.TryParse<WorkOrderOrigin>(request.Origin, true, out var origin))
            throw new ArgumentException("Origen invalido.");

        var asset = await _context.CommonAssets
            .FirstOrDefaultAsync(a => a.Id == request.AssetId && a.TenantId == tenantId);
        if (asset == null) throw new KeyNotFoundException("Bien comun no encontrado.");

        if (request.AssignedProviderId.HasValue)
        {
            var providerExists = await _context.Providers
                .AnyAsync(p => p.Id == request.AssignedProviderId.Value && p.TenantId == tenantId);
            if (!providerExists)
                throw new KeyNotFoundException("El proveedor especificado no existe.");
        }

        string? relatedPqrNumber = null;
        if (request.RelatedPqrId.HasValue)
        {
            var pqr = await _context.PqrRecords
                .FirstOrDefaultAsync(p => p.Id == request.RelatedPqrId.Value && p.TenantId == tenantId);
            if (pqr != null) relatedPqrNumber = pqr.RadicadoNumber;
        }

        if (request.BudgetItemId.HasValue)
        {
            var budgetItemExists = await _context.ExpenseItems
                .AnyAsync(e => e.Id == request.BudgetItemId.Value && e.Budget != null && e.Budget.TenantId == tenantId);
            if (!budgetItemExists)
                throw new KeyNotFoundException("El rubro presupuestal especificado no existe.");
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
            BudgetItemId = request.BudgetItemId,
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

        if (request.Description != null) order.Description = request.Description;
        if (request.Priority != null && Enum.TryParse<WorkOrderPriority>(request.Priority, true, out var pri))
            order.Priority = pri;
        if (request.AssignedProviderId != null)
        {
            var providerExists = await _context.Providers
                .AnyAsync(p => p.Id == request.AssignedProviderId.Value && p.TenantId == tenantId);
            if (!providerExists)
                throw new KeyNotFoundException("El proveedor especificado no existe.");

            order.AssignedProviderId = request.AssignedProviderId;
            if (order.Status == WorkOrderStatus.PendingAssignment)
                order.Status = WorkOrderStatus.Assigned;
        }
        if (request.ScheduledDate != null) order.ScheduledDate = request.ScheduledDate;
        if (request.ExecutionStartDate != null) order.ExecutionStartDate = request.ExecutionStartDate;
        if (request.EstimatedCost != null) order.EstimatedCost = request.EstimatedCost.Value;

        if (request.ActualCost != null)
        {
            var actualCostValue = request.ActualCost.Value;

            if (order.EstimatedCost > 0)
            {
                var deviation = Math.Abs(actualCostValue - order.EstimatedCost) / order.EstimatedCost;

                if (deviation > 0.20m && !request.ConfirmCostDeviation)
                {
                    throw new InvalidOperationException(
                        $"El costo real ({actualCostValue:C}) se desvia en {(deviation * 100):N0}% del costo estimado ({order.EstimatedCost:C}), superando el 20% permitido. " +
                        "Confirme explicitamente para continuar con el registro.");
                }

                if (deviation > 0.20m)
                {
                    order.CostAlertSent = true;
                }
            }

            order.ActualCost = actualCostValue;
        }

        if (request.BudgetItemId != null)
        {
            var budgetItemExists = await _context.ExpenseItems
                .AnyAsync(e => e.Id == request.BudgetItemId.Value && e.Budget != null && e.Budget.TenantId == tenantId);
            if (!budgetItemExists)
                throw new KeyNotFoundException("El rubro presupuestal especificado no existe.");
            order.BudgetItemId = request.BudgetItemId;
        }

        if (request.Outcome != null && Enum.TryParse<WorkOrderOutcome>(request.Outcome, true, out var outcome))
            order.Outcome = outcome;
        if (request.OutcomeNotes != null) order.OutcomeNotes = request.OutcomeNotes;

        if (request.Status != null && Enum.TryParse<WorkOrderStatus>(request.Status, true, out var newStatus))
        {
            var wasAlreadyCompleted = order.Status == WorkOrderStatus.Completed;
            order.Status = newStatus;

            if (newStatus == WorkOrderStatus.InProgress && order.ExecutionStartDate == null)
                order.ExecutionStartDate = DateTime.UtcNow;

            if (newStatus == WorkOrderStatus.Completed && !wasAlreadyCompleted)
            {
                order.ExecutionEndDate = DateTime.UtcNow;

                if (order.Origin == WorkOrderOrigin.ResidentPqr && order.RelatedPqrId.HasValue)
                {
                    var pqr = await _context.PqrRecords
                        .FirstOrDefaultAsync(p => p.Id == order.RelatedPqrId.Value && p.TenantId == tenantId);

                    if (pqr != null && pqr.Status != PQRStatus.Closed)
                    {
                        var previousPqrStatus = pqr.Status;
                        pqr.Status = PQRStatus.Responded;
                        pqr.UpdatedAt = DateTime.UtcNow;

                        _context.PqrFollowUps.Add(new PqrFollowUp
                        {
                            Id = Guid.NewGuid(),
                            PQRId = pqr.Id,
                            PreviousStatus = previousPqrStatus,
                            NewStatus = PQRStatus.Responded,
                            ChangedAt = DateTime.UtcNow,
                            ChangedByUserId = userId,
                            ChangedByUserName = userId,
                            Justification = $"Orden de trabajo {order.Id} completada.",
                            IsAutomatic = true
                        });

                        await _notificationEngine.ProcessEventAsync(
                            tenantId,
                            NotificationEventType.WorkOrderResolved,
                            "Maintenance",
                            order.Id.ToString(),
                            "WorkOrder",
                            ownerId: pqr.OwnerId,
                            tenantResidentId: pqr.TenantResidentId);
                    }
                }

                if (order.OrderType == WorkOrderType.Preventive && order.MaintenancePlanId.HasValue)
                {
                    var plan = await _context.MaintenancePlans
                        .FirstOrDefaultAsync(p => p.Id == order.MaintenancePlanId.Value && p.IsActive);

                    if (plan != null)
                    {
                        plan.LastExecutionDate = DateTime.UtcNow;
                        plan.NextExecutionDate = DateTime.UtcNow.AddDays(plan.FrequencyDays);
                    }
                }

                if (order.BudgetItemId.HasValue && order.ActualCost > 0)
                {
                    await RecordBudgetExecutionAsync(tenantId, userId, order);
                }
            }
        }

        order.UpdatedByUserId = userId;
        order.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        return await GetWorkOrderByIdAsync(tenantId, order.Id);
    }

    private Task RecordBudgetExecutionAsync(string tenantId, string userId, WorkOrder order)
    {
        var executedExpense = new ExecutedExpense
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            ExpenseItemId = order.BudgetItemId!.Value,
            Amount = order.ActualCost,
            Description = $"OT {order.OrderType}: {order.Description}".Substring(0, Math.Min(500, $"OT {order.OrderType}: {order.Description}".Length)),
            ExpenseDate = DateTime.UtcNow,
            ProviderId = order.AssignedProviderId,
            CreatedByUserId = userId,
            CreatedAt = DateTime.UtcNow
        };

        _context.ExecutedExpenses.Add(executedExpense);
        return Task.CompletedTask;
    }

    private async Task BlockAssociatedReservableSpacesAsync(string tenantId, string userId, CommonAsset asset)
    {
        if (!asset.ReservableSpaceId.HasValue)
        {
            return;
        }

        var space = await _context.ReservableSpaces
            .FirstOrDefaultAsync(s => s.Id == asset.ReservableSpaceId.Value && s.TenantId == tenantId);

        if (space == null)
        {
            return;
        }

        space.IsActive = false;

        var blockStart = DateTime.UtcNow;
        var blockEnd = DateTime.UtcNow.AddYears(1);

        _context.SpaceBlocks.Add(new SpaceBlock
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            SpaceId = space.Id,
            StartDate = blockStart,
            EndDate = blockEnd,
            Origin = SpaceBlockOrigin.Maintenance,
            Reason = $"Bien {asset.Name} fuera de servicio.",
            NotifyAffectedResidents = true,
            CreatedByUserId = userId,
            CreatedAt = DateTime.UtcNow
        });

        var affectedReservations = await _context.Reservations
            .Where(r => r.TenantId == tenantId
                && r.SpaceId == space.Id
                && r.EndDateTime >= blockStart
                && (r.Status == ReservationStatus.Approved || r.Status == ReservationStatus.InUse))
            .ToListAsync();

        foreach (var reservation in affectedReservations)
        {
            reservation.Status = ReservationStatus.Cancelled;
            reservation.RejectionReason = $"Cancelada: el bien {asset.Name} quedo fuera de servicio.";
            reservation.UpdatedAt = DateTime.UtcNow;
            reservation.UpdatedByUserId = userId;

            await _notificationEngine.ProcessEventAsync(
                tenantId,
                NotificationEventType.ReservationRejected,
                "Maintenance",
                reservation.Id.ToString(),
                "Reservation",
                ownerId: reservation.OwnerId);
        }
    }

    public async Task DeleteWorkOrderAsync(string tenantId, Guid workOrderId)
    {
        var order = await _context.WorkOrders
            .FirstOrDefaultAsync(w => w.Id == workOrderId && w.TenantId == tenantId);
        if (order == null) throw new KeyNotFoundException("Orden de trabajo no encontrada.");

        if (order.Status == WorkOrderStatus.InProgress)
            throw new InvalidOperationException("No se puede eliminar una orden de trabajo en ejecucion.");

        _context.WorkOrders.Remove(order);
        await _context.SaveChangesAsync();
    }

    // ── Incidents ─────────────────────────────────────────────────

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
                InsuranceContractId = i.InsuranceContractId,
                InsuranceContractNumber = i.InsuranceContract != null ? i.InsuranceContract.ContractNumber : string.Empty,
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
            throw new ArgumentException("Tipo de siniestro invalido.");

        if (request.InsuranceContractId.HasValue)
        {
            var contractExists = await _context.Contracts
                .AnyAsync(c => c.Id == request.InsuranceContractId.Value && c.TenantId == tenantId);
            if (!contractExists)
                throw new KeyNotFoundException("El contrato de seguros especificado no existe.");
        }

        var incident = new Incident
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            Name = request.Name,
            Description = request.Description,
            IncidentType = incidentType,
            OccurredAt = request.OccurredAt,
            TotalDamageValue = request.TotalDamageValue ?? 0,
            InsuranceContractId = request.InsuranceContractId,
            InsurancePolicyNumber = request.InsurancePolicyNumber ?? string.Empty,
            InsuranceCompany = request.InsuranceCompany ?? string.Empty,
            Status = "Open",
            CreatedByUserId = userId,
            CreatedAt = DateTime.UtcNow
        };

        _context.Incidents.Add(incident);

        if (request.WorkOrderIds != null && request.WorkOrderIds.Count > 0)
        {
            var validWorkOrderIds = await _context.WorkOrders
                .Where(w => request.WorkOrderIds.Contains(w.Id) && w.TenantId == tenantId)
                .Select(w => w.Id)
                .ToListAsync();

            var invalidCount = request.WorkOrderIds.Count - validWorkOrderIds.Count;
            if (invalidCount > 0)
            {
                throw new InvalidOperationException(
                    $"{invalidCount} orden(es) de trabajo especificada(s) no existen o no pertenecen a este conjunto.");
            }

            foreach (var woId in validWorkOrderIds)
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

        if (request.InsuranceContractId.HasValue)
        {
            var contractExists = await _context.Contracts
                .AnyAsync(c => c.Id == request.InsuranceContractId.Value && c.TenantId == tenantId);
            if (!contractExists)
                throw new KeyNotFoundException("El contrato de seguros especificado no existe.");
            incident.InsuranceContractId = request.InsuranceContractId;
        }

        if (request.InsurancePolicyNumber != null) incident.InsurancePolicyNumber = request.InsurancePolicyNumber;
        if (request.InsuranceCompany != null) incident.InsuranceCompany = request.InsuranceCompany;
        if (request.Status != null) incident.Status = request.Status;

        incident.UpdatedByUserId = userId;
        incident.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        return await GetIncidentByIdAsync(tenantId, incident.Id);
    }

    // ── Reports ───────────────────────────────────────────────────

    public async Task<MaintenanceReportDto> GetScheduledMaintenanceReportAsync(string tenantId, int daysAhead)
    {
        var now = DateTime.UtcNow;
        var cutoff = now.AddDays(daysAhead);

        var plans = await _context.MaintenancePlans
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

        var totalEstimated = plans.Sum(i => i.EstimatedCost);

        var maintenanceBudgetAvailable = await GetMaintenanceBudgetAvailableAsync(tenantId);

        return new MaintenanceReportDto
        {
            DaysAhead = daysAhead,
            TotalEstimatedCost = totalEstimated,
            BudgetAvailable = maintenanceBudgetAvailable,
            ScheduledItems = plans
        };
    }

    private async Task<decimal> GetMaintenanceBudgetAvailableAsync(string tenantId)
    {
        var currentYear = DateTime.Today.Year;
        var startDate = new DateTime(currentYear, 1, 1);
        var endDate = new DateTime(currentYear, 12, 31, 23, 59, 59);

        var expenseItems = await _context.ExpenseItems
            .Where(e => e.Budget != null && e.Budget.TenantId == tenantId && e.Budget.Status == BudgetStatus.Approved)
            .ToListAsync();

        if (expenseItems.Count == 0) return 0m;

        var expenseItemIds = expenseItems.Select(e => e.Id).ToList();

        var executedByItem = await _context.ExecutedExpenses
            .Where(e => e.TenantId == tenantId && expenseItemIds.Contains(e.ExpenseItemId) && e.ExpenseDate >= startDate && e.ExpenseDate <= endDate)
            .GroupBy(e => e.ExpenseItemId)
            .Select(g => new { ExpenseItemId = g.Key, Total = g.Sum(e => e.Amount) })
            .ToDictionaryAsync(x => x.ExpenseItemId, x => x.Total);

        var totalAvailable = 0m;
        foreach (var expenseItem in expenseItems)
        {
            var executed = executedByItem.TryGetValue(expenseItem.Id, out var val) ? val : 0m;
            totalAvailable += expenseItem.AnnualValue - executed;
        }

        return totalAvailable;
    }

    // ── Dashboard Indicators ─────────────────────────────────────

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

    // ── Out-of-Service Assets ─────────────────────────────────────

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
                HasReservationBlock = asset.ReservableSpaceId.HasValue,
                StatusChangedAt = statusChangedAt,
                DaysOutOfService = daysOut,
                Reason = lastHistory?.Reason ?? string.Empty
            });
        }

        return result.OrderByDescending(a => a.DaysOutOfService).ToList();
    }
}
