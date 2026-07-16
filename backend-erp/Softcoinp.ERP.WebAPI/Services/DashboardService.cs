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

/// <summary>
/// Orquesta el Dashboard Principal: información operativa del conjunto para que el
/// administrador decida sin tener que navegar cada módulo por separado. Ningún método
/// de esta clase consulta datos del módulo de Contabilidad, que fue eliminado del
/// sistema.
/// Todos los indicadores se calculan en tiempo real, excepto el gráfico de recaudo
/// histórico y el mapa de estado de pago, que usan caché con invalidación por eventos
/// (ver PaymentStatusMapService y los puntos de invalidación en PaymentService y
/// BillingEngineService).
/// </summary>
public class DashboardService
{
    private readonly ApplicationDbContext _context;
    private readonly IndicatorCacheService _indicatorCache;
    private readonly ExecutionEngineService _executionEngineService;
    private readonly DashboardAlertEngineService _alertEngineService;
    private readonly PaymentStatusMapService _paymentStatusMapService;
    private readonly PortfolioAgingService _portfolioAgingService;

    public const string CollectionChartCacheKeyPrefix = "collection_chart_";

    public DashboardService(
        ApplicationDbContext context,
        IndicatorCacheService indicatorCache,
        ExecutionEngineService executionEngineService,
        DashboardAlertEngineService alertEngineService,
        PaymentStatusMapService paymentStatusMapService,
        PortfolioAgingService portfolioAgingService)
    {
        _context = context;
        _indicatorCache = indicatorCache;
        _executionEngineService = executionEngineService;
        _alertEngineService = alertEngineService;
        _paymentStatusMapService = paymentStatusMapService;
        _portfolioAgingService = portfolioAgingService;
    }

    // ── KPIs ──────────────────────────────────────────────────────────

    public async Task<DashboardKpisDto> GetKpisAsync(string tenantId)
    {
        var now = DateTime.UtcNow;
        var currentPeriod = $"{now.Year}-{now.Month:D2}";
        var previousDate = now.AddMonths(-1);
        var previousPeriod = $"{previousDate.Year}-{previousDate.Month:D2}";

        var daysInMonth = DateTime.DaysInMonth(now.Year, now.Month);
        var daysElapsed = Math.Min(now.Day, daysInMonth);

        var billingPeriods = await _context.BillingPeriods
            .Where(bp => bp.TenantId == tenantId
                && (bp.Period == currentPeriod || bp.Period == previousPeriod))
            .ToListAsync();

        var currentBillingPeriod = billingPeriods.FirstOrDefault(bp => bp.Period == currentPeriod);
        var previousBillingPeriod = billingPeriods.FirstOrDefault(bp => bp.Period == previousPeriod);

        var kpis = new DashboardKpisDto
        {
            DaysElapsedInPeriod = daysElapsed,
            TotalDaysInPeriod = daysInMonth
        };

        if (currentBillingPeriod != null)
        {
            var currentFeesAgg = await _context.UnitFees
                .Where(uf => uf.TenantId == tenantId && uf.BillingPeriodId == currentBillingPeriod.Id)
                .GroupBy(uf => 1)
                .Select(g => new { Billed = g.Sum(uf => uf.FeeValue), Collected = g.Sum(uf => uf.PaidAmount) })
                .FirstOrDefaultAsync();

            kpis.CurrentMonthBilled = currentFeesAgg?.Billed ?? 0m;
            kpis.CurrentMonthCollected = currentFeesAgg?.Collected ?? 0m;

            if (kpis.CurrentMonthBilled > 0)
            {
                kpis.CurrentMonthCollectionPercentage = Math.Round(kpis.CurrentMonthCollected / kpis.CurrentMonthBilled * 100, 1);
            }
        }

        if (previousBillingPeriod != null)
        {
            var prevFeesAgg = await _context.UnitFees
                .Where(uf => uf.TenantId == tenantId && uf.BillingPeriodId == previousBillingPeriod.Id)
                .GroupBy(uf => 1)
                .Select(g => new { Billed = g.Sum(uf => uf.FeeValue), Collected = g.Sum(uf => uf.PaidAmount) })
                .FirstOrDefaultAsync();

            var prevBilled = prevFeesAgg?.Billed ?? 0m;
            var prevCollected = prevFeesAgg?.Collected ?? 0m;

            if (prevBilled > 0)
            {
                kpis.PreviousMonthCollectionPercentage = Math.Round(prevCollected / prevBilled * 100, 1);
            }
        }

        var aging = await GetPortfolioAgingAsync(tenantId);
        kpis.TotalOverduePortfolio = aging.OverdueOneMonth + aging.OverdueTwoMonths + aging.OverdueThreeOrMoreMonths;
        kpis.OverdueOneMonth = aging.OverdueOneMonth;
        kpis.OverdueTwoMonths = aging.OverdueTwoMonths;
        kpis.OverdueThreeOrMoreMonths = aging.OverdueThreeOrMoreMonths;

        var budgetExecution = await GetBudgetExecutionSummaryAsync(tenantId, now.Year);
        kpis.BudgetExecutionPercentage = budgetExecution.OverallPercentage;
        kpis.BudgetExpectedExecutionPercentage = budgetExecution.ExpectedPercentage;

        kpis.OpenPqrCount = await _context.PqrRecords
            .CountAsync(p => p.TenantId == tenantId && p.Status != PQRStatus.Closed);

        kpis.OverduePqrCount = await _context.PqrRecords
            .CountAsync(p => p.TenantId == tenantId
                && p.Deadline != null
                && p.Deadline < now
                && p.Status != PQRStatus.Closed
                && p.Status != PQRStatus.Responded
                && p.Status != PQRStatus.Escalated);

        return kpis;
    }

    private class PortfolioAgingResult
    {
        public decimal OverdueOneMonth { get; set; }
        public decimal OverdueTwoMonths { get; set; }
        public decimal OverdueThreeOrMoreMonths { get; set; }
    }

    private async Task<PortfolioAgingResult> GetPortfolioAgingAsync(string tenantId)
    {
        var overdueByUnit = await _portfolioAgingService.GetOverdueByUnitAsync(tenantId);

        var result = new PortfolioAgingResult();

        foreach (var unit in overdueByUnit.Values)
        {
            if (unit.MonthsOverdue <= 1)
            {
                result.OverdueOneMonth += unit.TotalDebt;
            }
            else if (unit.MonthsOverdue == 2)
            {
                result.OverdueTwoMonths += unit.TotalDebt;
            }
            else
            {
                result.OverdueThreeOrMoreMonths += unit.TotalDebt;
            }
        }

        return result;
    }

    private class BudgetExecutionSummary
    {
        public decimal OverallPercentage { get; set; }
        public decimal ExpectedPercentage { get; set; }
    }

    private async Task<BudgetExecutionSummary> GetBudgetExecutionSummaryAsync(string tenantId, int fiscalYear)
    {
        try
        {
            var execution = await _executionEngineService.GetExecutionDashboardAsync(tenantId, fiscalYear);

            var totalAnnual = execution.ExpenseItems.Sum(i => i.AnnualValue);
            var totalProportional = execution.ExpenseItems.Sum(i => i.ProportionalToDate);
            var expectedPercentage = 0m;

            if (totalAnnual > 0)
            {
                expectedPercentage = Math.Round(totalProportional / totalAnnual * 100m, 1);
            }

            return new BudgetExecutionSummary
            {
                OverallPercentage = execution.OverallExecutionPercentage,
                ExpectedPercentage = expectedPercentage
            };
        }
        catch (KeyNotFoundException)
        {
            return new BudgetExecutionSummary();
        }
    }

    // ── Alertas ───────────────────────────────────────────────────────

    public Task<List<AlertDto>> GetAlertsAsync(string tenantId)
    {
        return _alertEngineService.EvaluateActiveAlertsAsync(tenantId);
    }

    public Task<List<AlertConfigurationDto>> GetAlertConfigurationsAsync(string tenantId)
    {
        return _alertEngineService.GetConfigurationsAsync(tenantId);
    }

    public Task<AlertConfigurationDto> UpdateAlertConfigurationAsync(
        string tenantId, string ruleType, string userId, UpdateAlertConfigurationRequestDto request)
    {
        return _alertEngineService.UpdateConfigurationAsync(tenantId, ruleType, userId, request);
    }

    public Task InitializeDefaultAlertConfigurationsAsync(string tenantId)
    {
        return _alertEngineService.InitializeDefaultAlertConfigurationsAsync(tenantId);
    }

    // ── Gráfico de recaudo histórico (cacheado) ──────────────────────

    public async Task<List<MonthlyCollectionDto>> GetCollectionChartAsync(string tenantId)
    {
        var cacheKey = $"{CollectionChartCacheKeyPrefix}{tenantId}";
        var cached = await _indicatorCache.GetAsync<List<MonthlyCollectionDto>>(tenantId, cacheKey);
        if (cached != null)
        {
            return cached;
        }

        var chart = await ComputeCollectionChartAsync(tenantId);
        await _indicatorCache.SetAsync(tenantId, cacheKey, chart, expirationMinutes: 15);
        return chart;
    }

    private async Task<List<MonthlyCollectionDto>> ComputeCollectionChartAsync(string tenantId)
    {
        var now = DateTime.UtcNow;
        var twelveMonthsAgo = now.AddMonths(-12);
        var earliestPeriod = $"{twelveMonthsAgo.Year}-{twelveMonthsAgo.Month:D2}";

        var billingPeriods = await _context.BillingPeriods
            .Where(bp => bp.TenantId == tenantId && string.Compare(bp.Period, earliestPeriod) >= 0)
            .OrderBy(bp => bp.Period)
            .ToListAsync();

        var periodIds = billingPeriods.Select(bp => bp.Id).ToList();

        var feesByPeriod = await _context.UnitFees
            .Where(uf => uf.TenantId == tenantId && periodIds.Contains(uf.BillingPeriodId))
            .GroupBy(uf => uf.BillingPeriodId)
            .Select(g => new { BillingPeriodId = g.Key, Billed = g.Sum(uf => uf.FeeValue), Collected = g.Sum(uf => uf.PaidAmount) })
            .ToDictionaryAsync(g => g.BillingPeriodId);

        var result = new List<MonthlyCollectionDto>();

        foreach (var period in billingPeriods)
        {
            feesByPeriod.TryGetValue(period.Id, out var fees);

            result.Add(new MonthlyCollectionDto
            {
                Period = period.Period,
                Billed = fees?.Billed ?? 0m,
                Collected = fees?.Collected ?? 0m
            });
        }

        return result;
    }

    // ── Mapa de estado de pago (cacheado, delegado) ──────────────────

    public Task<PaymentStatusMapDto> GetPaymentStatusMapAsync(string tenantId)
    {
        return _paymentStatusMapService.GetPaymentStatusMapAsync(tenantId);
    }

    // ── Próximos eventos ──────────────────────────────────────────────

    public async Task<List<UpcomingEventDto>> GetUpcomingEventsAsync(string tenantId)
    {
        var now = DateTime.UtcNow;
        var thirtyDaysFromNow = now.AddDays(30);
        var events = new List<UpcomingEventDto>();

        var overdueFees = await _context.UnitFees
            .Where(uf => uf.TenantId == tenantId
                && uf.Status != FeeStatus.FullyPaid
                && uf.DueDate >= now
                && uf.DueDate <= thirtyDaysFromNow)
            .GroupBy(uf => uf.DueDate)
            .Select(g => new { DueDate = g.Key, Count = g.Count(), Total = g.Sum(f => f.BalanceAmount) })
            .ToListAsync();

        foreach (var fee in overdueFees)
        {
            events.Add(new UpcomingEventDto
            {
                Title = $"Vencimiento de cuotas ({fee.Count} unidades)",
                Description = $"{fee.Count} cuotas por {fee.Total:N0} COP vencen el {fee.DueDate:dd/MM/yyyy}.",
                EventDate = fee.DueDate,
                EventType = "FeeDueDate",
                ModuleLink = "/billing"
            });
        }

        var expiringContracts = await _context.Contracts
            .Include(c => c.Provider)
            .Where(c => c.TenantId == tenantId
                && c.Status == ContractStatus.Active
                && c.EndDate >= now
                && c.EndDate <= thirtyDaysFromNow)
            .ToListAsync();

        foreach (var contract in expiringContracts)
        {
            var providerName = string.Empty;
            if (contract.Provider != null)
            {
                providerName = contract.Provider.BusinessName;
            }

            events.Add(new UpcomingEventDto
            {
                Title = $"Vence contrato {contract.ContractNumber}",
                Description = $"Contrato con {providerName} vence el {contract.EndDate:dd/MM/yyyy}.",
                EventDate = contract.EndDate,
                EventType = "ContractExpiration",
                ModuleLink = $"/contracts/{contract.Id}"
            });
        }

        var upcomingMaintenance = await _context.MaintenancePlans
            .Include(p => p.Asset)
            .Where(p => p.TenantId == tenantId
                && p.IsActive
                && p.NextExecutionDate != null
                && p.NextExecutionDate >= now
                && p.NextExecutionDate <= thirtyDaysFromNow)
            .ToListAsync();

        foreach (var plan in upcomingMaintenance)
        {
            var assetName = string.Empty;
            if (plan.Asset != null)
            {
                assetName = plan.Asset.Name;
            }

            events.Add(new UpcomingEventDto
            {
                Title = $"Mantenimiento programado: {assetName}",
                Description = plan.Description,
                EventDate = plan.NextExecutionDate!.Value,
                EventType = "MaintenanceScheduled",
                ModuleLink = "/maintenance/work-orders"
            });
        }

        var upcomingReservations = await _context.Reservations
            .Include(r => r.Space)
            .Where(r => r.TenantId == tenantId
                && r.StartDateTime >= now
                && r.StartDateTime <= thirtyDaysFromNow
                && (r.Status == ReservationStatus.Approved || r.Status == ReservationStatus.InUse))
            .OrderBy(r => r.StartDateTime)
            .Take(20)
            .ToListAsync();

        foreach (var reservation in upcomingReservations)
        {
            var spaceName = string.Empty;
            if (reservation.Space != null)
            {
                spaceName = reservation.Space.Name;
            }

            events.Add(new UpcomingEventDto
            {
                Title = $"Reserva: {spaceName}",
                Description = $"Reserva {reservation.ReservationNumber} el {reservation.StartDateTime:dd/MM/yyyy HH:mm}.",
                EventDate = reservation.StartDateTime,
                EventType = "Reservation",
                ModuleLink = "/reservation/admin"
            });
        }

        return events.OrderBy(e => e.EventDate).ToList();
    }

    // ── Actividad reciente ────────────────────────────────────────────

    public async Task<List<RecentActivityDto>> GetRecentActivityAsync(string tenantId)
    {
        var activities = new List<RecentActivityDto>();

        var recentPayments = await _context.Payments
            .Where(p => p.TenantId == tenantId)
            .OrderByDescending(p => p.CreatedAt)
            .Take(20)
            .Join(_context.Units, p => p.UnitId, u => u.Id, (p, u) => new RecentActivityDto
            {
                Action = "Pago registrado",
                Description = $"Pago de {p.Amount:N0} COP de la unidad {u.Identifier}",
                UserName = p.ReceivedByUserId,
                Timestamp = p.CreatedAt,
                ModuleLink = "/billing/payments/register"
            })
            .ToListAsync();

        activities.AddRange(recentPayments);

        var recentWorkOrders = await _context.WorkOrders
            .Include(w => w.Asset)
            .Where(w => w.TenantId == tenantId && w.Status == WorkOrderStatus.Completed && w.ExecutionEndDate != null)
            .OrderByDescending(w => w.ExecutionEndDate)
            .Take(20)
            .ToListAsync();

        foreach (var order in recentWorkOrders)
        {
            var assetName = string.Empty;
            if (order.Asset != null)
            {
                assetName = order.Asset.Name;
            }

            activities.Add(new RecentActivityDto
            {
                Action = "Orden de trabajo completada",
                Description = $"{assetName}: {order.Description}",
                UserName = order.UpdatedByUserId ?? order.CreatedByUserId,
                Timestamp = order.ExecutionEndDate!.Value,
                ModuleLink = "/maintenance/work-orders"
            });
        }

        var recentPqrResponses = await _context.PqrRecords
            .Where(p => p.TenantId == tenantId && p.Status == PQRStatus.Responded)
            .OrderByDescending(p => p.UpdatedAt)
            .Take(20)
            .ToListAsync();

        foreach (var pqr in recentPqrResponses)
        {
            var timestamp = pqr.CreatedAt;
            if (pqr.UpdatedAt.HasValue)
            {
                timestamp = pqr.UpdatedAt.Value;
            }

            activities.Add(new RecentActivityDto
            {
                Action = "PQR respondida",
                Description = $"{pqr.RadicadoNumber}: {pqr.Subject}",
                UserName = pqr.AssignedToUserId ?? string.Empty,
                Timestamp = timestamp,
                ModuleLink = "/pqr"
            });
        }

        return activities.OrderByDescending(a => a.Timestamp).Take(20).ToList();
    }

    // ── Invalidación de caché ─────────────────────────────────────────

    public async Task InvalidateDashboardCacheAsync(string tenantId)
    {
        await _indicatorCache.InvalidateAsync(tenantId, CollectionChartCacheKeyPrefix);
        await _indicatorCache.InvalidateAsync(tenantId, PaymentStatusMapService.CacheKeyPrefix);
    }
}
