using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Softcoinp.ERP.Domain.Entities;
using Softcoinp.ERP.Domain.Enums;
using Softcoinp.ERP.Infrastructure.Persistence;

namespace Softcoinp.ERP.WebAPI.Services;

public class PreventiveMaintenanceEngineService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<PreventiveMaintenanceEngineService> _logger;
    private const int DefaultAdvanceDays = 7;

    public PreventiveMaintenanceEngineService(IServiceScopeFactory scopeFactory, ILogger<PreventiveMaintenanceEngineService> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await TenantBackgroundRunner.ForEachTenantAsync(_scopeFactory, async (context, sp) =>
                {
                    await GeneratePreventiveWorkOrdersAsync(context);
                    await DetectUnassignedOrdersAsync(context);
                    await DetectOutOfServiceEssentialAssetsAsync(context);
                    await CleanupOldHistoryAsync(context);
                });

                await Task.Delay(TimeSpan.FromHours(6), stoppingToken);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                _logger.LogError(ex, "Error en el motor de mantenimiento preventivo");
                await Task.Delay(TimeSpan.FromMinutes(30), stoppingToken);
            }
        }
    }

    private async Task GeneratePreventiveWorkOrdersAsync(ApplicationDbContext context)
    {
        var now = DateTime.UtcNow;
        var advanceDate = now.AddDays(DefaultAdvanceDays);

        var plansDue = await context.MaintenancePlans
            .Where(p => p.IsActive &&
                p.NextExecutionDate != null &&
                p.NextExecutionDate <= advanceDate &&
                p.NextExecutionDate > now.AddDays(-1))
            .Include(p => p.Asset)
            .ToListAsync();

        foreach (var plan in plansDue)
        {
            if (plan.Asset == null) continue;

            var existingOrder = await context.WorkOrders
                .AnyAsync(w => w.AssetId == plan.AssetId &&
                    w.OrderType == WorkOrderType.Preventive &&
                    w.Status != WorkOrderStatus.Cancelled &&
                    w.ScheduledDate != null &&
                    w.ScheduledDate.Value.Date == plan.NextExecutionDate!.Value.Date);

            if (existingOrder) continue;

            var workOrder = new WorkOrder
            {
                Id = Guid.NewGuid(),
                TenantId = plan.TenantId,
                OrderType = WorkOrderType.Preventive,
                AssetId = plan.AssetId,
                Description = $"Mantenimiento preventivo programado: {plan.Description}",
                Priority = plan.Asset.IsEssential ? WorkOrderPriority.High : WorkOrderPriority.Medium,
                Origin = WorkOrderOrigin.AutomaticScheduling,
                AssignedProviderId = plan.PreferredProviderId,
                ScheduledDate = plan.NextExecutionDate,
                EstimatedCost = plan.EstimatedCost,
                Status = plan.PreferredProviderId.HasValue
                    ? WorkOrderStatus.Assigned
                    : WorkOrderStatus.PendingAssignment,
                CreatedByUserId = "system",
                CreatedAt = now
            };

            context.WorkOrders.Add(workOrder);

            _logger.LogInformation(
                "Orden de trabajo preventivo generada para bien {AssetName} (plan {PlanId}), programada para {Date}",
                plan.Asset.Name, plan.Id, plan.NextExecutionDate);
        }

        await context.SaveChangesAsync();
    }

    private async Task DetectUnassignedOrdersAsync(ApplicationDbContext context)
    {
        var now = DateTime.UtcNow;

        var unassignedPastDue = await context.WorkOrders
            .Where(w => w.Status == WorkOrderStatus.PendingAssignment &&
                w.ScheduledDate != null &&
                w.ScheduledDate <= now)
            .Include(w => w.Asset)
            .ToListAsync();

        foreach (var order in unassignedPastDue)
        {
            _logger.LogWarning(
                "ALERTA CRÍTICA: Orden de trabajo {OrderId} para bien {AssetName} pasó su fecha de ejecución ({Date}) sin asignar",
                order.Id, order.Asset?.Name ?? "N/A", order.ScheduledDate);
        }
    }

    private async Task DetectOutOfServiceEssentialAssetsAsync(ApplicationDbContext context)
    {
        var outOfServiceEssential = await context.CommonAssets
            .Where(a => a.Status == AssetStatus.OutOfService && a.IsEssential)
            .ToListAsync();

        foreach (var asset in outOfServiceEssential)
        {
            _logger.LogWarning(
                "ALERTA: Bien esencial {AssetName} está fuera de servicio. Requiere atención del consejo de administración.",
                asset.Name);
        }
    }

    private async Task CleanupOldHistoryAsync(ApplicationDbContext context)
    {
        var twoYearsAgo = DateTime.UtcNow.AddYears(-2);
        var oldHistories = await context.AssetStatusHistories
            .Where(h => h.ChangedAt < twoYearsAgo)
            .ToListAsync();

        if (oldHistories.Count > 100)
        {
            context.AssetStatusHistories.RemoveRange(oldHistories.Take(oldHistories.Count - 100));
            await context.SaveChangesAsync();
        }
    }
}
