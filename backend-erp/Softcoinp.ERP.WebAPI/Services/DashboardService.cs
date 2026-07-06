using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Softcoinp.ERP.Domain.Entities;
using Softcoinp.ERP.Domain.Enums;
using Softcoinp.ERP.Infrastructure.Persistence;
using Softcoinp.ERP.WebAPI.DTOs;

namespace Softcoinp.ERP.WebAPI.Services;

public class DashboardService
{
    private readonly ApplicationDbContext _context;
    private readonly IMemoryCache _memoryCache;
    private readonly IndicatorCacheService _indicatorCache;

    public DashboardService(ApplicationDbContext context, IMemoryCache memoryCache, IndicatorCacheService indicatorCache)
    {
        _context = context;
        _memoryCache = memoryCache;
        _indicatorCache = indicatorCache;
    }

    public async Task<DashboardDataDto> GetDashboardAsync(
        string tenantId, string userId, string role)
    {
        var data = new DashboardDataDto();

        if (role == AppRole.Resident.ToString())
        {
            try { data.ResidentData = await GetResidentDataAsync(tenantId, userId); }
            catch (Exception ex) { _ = ex; }
            return data;
        }

        if (role == AppRole.Auditor.ToString())
        {
            return data;
        }

        bool isAdmin = role == AppRole.Admin.ToString()
            || role == AppRole.SuperAdmin.ToString();
        bool isCouncil = role == AppRole.Council.ToString();
        bool isAccountant = role == AppRole.Accountant.ToString();

        if (isAdmin || isCouncil)
        {
            try { data.Kpis = await GetKpisAsync(tenantId); }
            catch (Exception ex) { _ = ex; }

            try { data.MonthlyCollection = await GetMonthlyCollectionAsync(tenantId); }
            catch (Exception ex) { _ = ex; }

            try { data.UpcomingEvents = await GetUpcomingEventsAsync(tenantId); }
            catch (Exception ex) { _ = ex; }

            try { data.Alerts = await EvaluateAlertsAsync(tenantId, role); }
            catch (Exception ex) { _ = ex; }

            if (isAdmin)
            {
                try { data.MoraMap = await GetMoraMapAsync(tenantId); }
                catch (Exception ex) { _ = ex; }

                try { data.UnitSummaries = await GetUnitSummariesAsync(tenantId); }
                catch (Exception ex) { _ = ex; }

                try { data.RecentActivity = await GetRecentActivityAsync(tenantId); }
                catch (Exception ex) { _ = ex; }
            }

        }

        if (isAccountant)
        {
            try { data.Kpis = await GetKpisAsync(tenantId); }
            catch (Exception ex) { _ = ex; }

            try { data.MonthlyCollection = await GetMonthlyCollectionAsync(tenantId); }
            catch (Exception ex) { _ = ex; }
        }

        return data;
    }

    private async Task<DashboardKpisDto> GetKpisAsync(string tenantId)
    {
        var cacheKey = $"kpis_{tenantId}";
        var cached = await _indicatorCache.GetAsync<DashboardKpisDto>(tenantId, cacheKey);
        if (cached != null) return cached;

        var kpis = await ComputeKpisAsync(tenantId);
        await _indicatorCache.SetAsync(tenantId, cacheKey, kpis, expirationMinutes: 5);
        return kpis;
    }

    private async Task<DashboardKpisDto> ComputeKpisAsync(string tenantId)
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
                .Where(uf => uf.TenantId == tenantId
                    && uf.BillingPeriodId == currentBillingPeriod.Id)
                .GroupBy(uf => 1)
                .Select(g => new { Billed = g.Sum(uf => uf.FeeValue), Collected = g.Sum(uf => uf.PaidAmount) })
                .FirstOrDefaultAsync();

            kpis.CurrentMonthBilled = currentFeesAgg?.Billed ?? 0m;
            kpis.CurrentMonthCollected = currentFeesAgg?.Collected ?? 0m;
            kpis.CurrentMonthCollectionPercentage = kpis.CurrentMonthBilled > 0
                ? Math.Round(kpis.CurrentMonthCollected / kpis.CurrentMonthBilled * 100, 1)
                : 0;
        }

        if (previousBillingPeriod != null)
        {
            var prevFeesAgg = await _context.UnitFees
                .Where(uf => uf.TenantId == tenantId
                    && uf.BillingPeriodId == previousBillingPeriod.Id)
                .GroupBy(uf => 1)
                .Select(g => new { Billed = g.Sum(uf => uf.FeeValue), Collected = g.Sum(uf => uf.PaidAmount) })
                .FirstOrDefaultAsync();

            var prevBilled = prevFeesAgg?.Billed ?? 0m;
            var prevCollected = prevFeesAgg?.Collected ?? 0m;

            kpis.PreviousMonthCollectionPercentage = prevBilled > 0
                ? Math.Round(prevCollected / prevBilled * 100, 1)
                : 0;
        }

        var totalOverdue = await _context.UnitFees
            .Where(uf => uf.TenantId == tenantId
                && uf.BalanceAmount > 0
                && uf.Status != FeeStatus.FullyPaid)
            .SumAsync(uf => uf.BalanceAmount);

        var totalOverdueExtraordinary = await _context.ExtraordinaryFeeDistributions
            .Where(efd => efd.TenantId == tenantId
                && efd.BalanceAmount > 0
                && efd.Status != FeeStatus.FullyPaid)
            .SumAsync(efd => efd.BalanceAmount);

        var totalOverdueCharges = await _context.IndividualCharges
            .Where(ic => ic.TenantId == tenantId
                && ic.BalanceAmount > 0
                && ic.Status != IndividualChargeStatus.Paid)
            .SumAsync(ic => ic.BalanceAmount);

        kpis.TotalOverduePortfolio = totalOverdue + totalOverdueExtraordinary + totalOverdueCharges;

        var overdueItems = new List<(decimal Balance, int DaysOverdue)>();

        await foreach (var item in _context.UnitFees
            .Where(uf => uf.TenantId == tenantId
                && uf.BalanceAmount > 0
                && uf.Status != FeeStatus.FullyPaid)
            .Select(uf => new { uf.BalanceAmount, uf.DueDate })
            .AsAsyncEnumerable())
        {
            var days = (int)(DateTime.UtcNow - item.DueDate).TotalDays;
            overdueItems.Add((item.BalanceAmount, days));
        }

        await foreach (var item in _context.ExtraordinaryFeeDistributions
            .Where(efd => efd.TenantId == tenantId
                && efd.BalanceAmount > 0
                && efd.Status != FeeStatus.FullyPaid)
            .Select(efd => new { efd.BalanceAmount, efd.DueDate })
            .AsAsyncEnumerable())
        {
            var days = (int)(DateTime.UtcNow - item.DueDate).TotalDays;
            overdueItems.Add((item.BalanceAmount, days));
        }

        await foreach (var item in _context.IndividualCharges
            .Where(ic => ic.TenantId == tenantId
                && ic.BalanceAmount > 0
                && ic.Status != IndividualChargeStatus.Paid)
            .Select(ic => new { ic.BalanceAmount, ic.ChargeDate })
            .AsAsyncEnumerable())
        {
            var days = (int)(DateTime.UtcNow - item.ChargeDate).TotalDays;
            overdueItems.Add((item.BalanceAmount, days));
        }

        kpis.EarlyOverdue = overdueItems
            .Where(x => x.DaysOverdue >= 1 && x.DaysOverdue <= 90)
            .Sum(x => x.Balance);

        kpis.MediumOverdue = overdueItems
            .Where(x => x.DaysOverdue >= 91 && x.DaysOverdue <= 180)
            .Sum(x => x.Balance);

        kpis.LegalOverdue = overdueItems
            .Where(x => x.DaysOverdue > 180)
            .Sum(x => x.Balance);

        var totalBankBalance = await _context.BankAccounts
            .Where(ba => ba.TenantId == tenantId && ba.IsActive)
            .SumAsync(ba => ba.CurrentBalance);

        kpis.AvailableCash = totalBankBalance;

        var currentYear = now.Year;
        var yearStart = new DateTime(currentYear, 1, 1);
        var yearProgress = (now - yearStart).TotalDays / (DateTime.IsLeapYear(currentYear) ? 366 : 365);
        kpis.YearProgressPercentage = Math.Round((decimal)(yearProgress * 100), 1);

        return kpis;
    }

    private async Task<List<MonthlyCollectionDto>> GetMonthlyCollectionAsync(string tenantId)
    {
        var now = DateTime.UtcNow;
        var twelveMonthsAgo = now.AddMonths(-12);

        var billingPeriods = await _context.BillingPeriods
            .Where(bp => bp.TenantId == tenantId
                && string.Compare(bp.Period, $"{twelveMonthsAgo.Year}-{twelveMonthsAgo.Month:D2}") >= 0)
            .OrderBy(bp => bp.Period)
            .ToListAsync();

        var periodIds = billingPeriods.Select(bp => bp.Id).ToList();

        var feesByPeriod = await _context.UnitFees
            .Where(uf => uf.TenantId == tenantId
                && periodIds.Contains(uf.BillingPeriodId))
            .GroupBy(uf => uf.BillingPeriodId)
            .Select(g => new
            {
                BillingPeriodId = g.Key,
                Billed = g.Sum(uf => uf.FeeValue),
                Collected = g.Sum(uf => uf.PaidAmount)
            })
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

    private async Task<List<UnitMoraDto>> GetMoraMapAsync(string tenantId)
    {
        var cacheKey = $"mora_map_{tenantId}";

        if (_memoryCache.TryGetValue(cacheKey, out List<UnitMoraDto>? cached))
        {
            if (cached != null)
            {
                return cached;
            }
        }

        var now = DateTime.UtcNow;
        var nowStr = now.ToString("yyyy-MM-dd HH:mm:ss");

        var sql = $@"
WITH spokespersons AS (
    SELECT uo.UnitId, o.FullNameOrCompanyName AS OwnerName
    FROM erp_unit_owners uo
    INNER JOIN erp_owners o ON o.Id = uo.OwnerId
    WHERE uo.TenantId = @p0 AND uo.IsActive = TRUE AND uo.IsSpokesperson = TRUE
),
fee_debts AS (
    SELECT UnitId, SUM(BalanceAmount) AS TotalDebt, MIN(DueDate) AS OldestDate
    FROM erp_unit_fees
    WHERE TenantId = @p0 AND BalanceAmount > 0 AND Status <> 'FullyPaid'
    GROUP BY UnitId
),
extra_debts AS (
    SELECT UnitId, SUM(BalanceAmount) AS TotalDebt, MIN(DueDate) AS OldestDate
    FROM erp_extraordinary_fee_distributions
    WHERE TenantId = @p0 AND BalanceAmount > 0 AND Status <> 'FullyPaid'
    GROUP BY UnitId
),
charge_debts AS (
    SELECT UnitId, SUM(BalanceAmount) AS TotalDebt, MIN(ChargeDate) AS OldestDate
    FROM erp_individual_charges
    WHERE TenantId = @p0 AND BalanceAmount > 0 AND Status <> 'Paid'
    GROUP BY UnitId
),
unit_base AS (
    SELECT
        u.Id AS UnitId,
        u.Identifier,
        COALESCE(u.TowerOrBlock, '') AS TowerOrBlock,
        u.FloorLevel,
        u.Status,
        COALESCE(s.OwnerName, 'Sin propietario') AS OwnerName,
        COALESCE(fd.TotalDebt, 0) + COALESCE(ed.TotalDebt, 0) + COALESCE(cd.TotalDebt, 0) AS OverdueBalance,
        LEAST(
            COALESCE(fd.OldestDate, '9999-12-31'),
            COALESCE(ed.OldestDate, '9999-12-31'),
            COALESCE(cd.OldestDate, '9999-12-31')
        ) AS MinDebtDate
    FROM erp_units u
    LEFT JOIN spokespersons s ON s.UnitId = u.Id
    LEFT JOIN fee_debts fd ON fd.UnitId = u.Id
    LEFT JOIN extra_debts ed ON ed.UnitId = u.Id
    LEFT JOIN charge_debts cd ON cd.UnitId = u.Id
    WHERE u.TenantId = @p0
      AND u.Status IN ('ActiveOccupied', 'ActiveUnoccupied')
)
SELECT
    UnitId, Identifier, TowerOrBlock, FloorLevel, OwnerName, OverdueBalance,
    CASE WHEN MinDebtDate = '9999-12-31' THEN 0
         ELSE GREATEST(0, DATEDIFF(@p1, MinDebtDate))
    END AS DaysOverdue,
    CASE WHEN Status = 'ActiveUnoccupied' THEN 'gray'
         WHEN OverdueBalance <= 0 THEN 'green'
         WHEN MinDebtDate = '9999-12-31' THEN 'green'
         WHEN GREATEST(0, DATEDIFF(@p1, MinDebtDate)) <= 30 THEN 'yellow'
         WHEN GREATEST(0, DATEDIFF(@p1, MinDebtDate)) <= 90 THEN 'orange'
         ELSE 'red'
    END AS ColorCode,
    CASE WHEN Status = 'ActiveUnoccupied' THEN 'Desocupada'
         WHEN OverdueBalance <= 0 THEN 'Al dia'
         WHEN MinDebtDate = '9999-12-31' THEN 'Al dia'
         WHEN GREATEST(0, DATEDIFF(@p1, MinDebtDate)) <= 30 THEN 'Mora temprana'
         WHEN GREATEST(0, DATEDIFF(@p1, MinDebtDate)) <= 90 THEN 'Mora media'
         ELSE 'Mora critica'
    END AS Status
FROM unit_base
ORDER BY Identifier";

        var units = await _context.Database
            .SqlQueryRaw<UnitMoraDto>(sql, tenantId, nowStr)
            .ToListAsync();

        var cacheOptions = new MemoryCacheEntryOptions()
            .SetAbsoluteExpiration(TimeSpan.FromMinutes(5))
            .SetPriority(CacheItemPriority.Normal);

        _memoryCache.Set(cacheKey, units, cacheOptions);

        return units;
    }

    private async Task<List<UnitSummaryDto>> GetUnitSummariesAsync(string tenantId)
    {
        var sql = @"
WITH spokespersons AS (
    SELECT uo.UnitId, o.FullNameOrCompanyName AS OwnerName
    FROM erp_unit_owners uo
    INNER JOIN erp_owners o ON o.Id = uo.OwnerId
    WHERE uo.TenantId = @p0 AND uo.IsActive = TRUE AND uo.IsSpokesperson = TRUE
),
fee_balances AS (
    SELECT UnitId, SUM(BalanceAmount) AS Balance
    FROM erp_unit_fees
    WHERE TenantId = @p0 AND BalanceAmount > 0
    GROUP BY UnitId
),
extra_balances AS (
    SELECT UnitId, SUM(BalanceAmount) AS Balance
    FROM erp_extraordinary_fee_distributions
    WHERE TenantId = @p0 AND BalanceAmount > 0
    GROUP BY UnitId
),
charge_balances AS (
    SELECT UnitId, SUM(BalanceAmount) AS Balance
    FROM erp_individual_charges
    WHERE TenantId = @p0 AND BalanceAmount > 0
    GROUP BY UnitId
)
SELECT
    u.Id AS UnitId,
    u.Identifier,
    COALESCE(u.TowerOrBlock, '') AS TowerOrBlock,
    u.FloorLevel,
    COALESCE(s.OwnerName, 'Sin propietario') AS OwnerName,
    COALESCE(fb.Balance, 0) + COALESCE(eb.Balance, 0) + COALESCE(cb.Balance, 0) AS CurrentBalance,
    CASE
        WHEN u.Status IN ('Inactive', 'DeliveryProcess') THEN 'gray'
        WHEN COALESCE(fb.Balance, 0) + COALESCE(eb.Balance, 0) + COALESCE(cb.Balance, 0) > 0 THEN 'red'
        ELSE 'green'
    END AS ColorCode,
    CASE
        WHEN u.Status IN ('Inactive', 'DeliveryProcess') THEN 'Inactiva'
        WHEN COALESCE(fb.Balance, 0) + COALESCE(eb.Balance, 0) + COALESCE(cb.Balance, 0) > 0 THEN 'En mora'
        ELSE 'Al dia'
    END AS Status
FROM erp_units u
LEFT JOIN spokespersons s ON s.UnitId = u.Id
LEFT JOIN fee_balances fb ON fb.UnitId = u.Id
LEFT JOIN extra_balances eb ON eb.UnitId = u.Id
LEFT JOIN charge_balances cb ON cb.UnitId = u.Id
WHERE u.TenantId = @p0
ORDER BY u.Identifier";

        return await _context.Database
            .SqlQueryRaw<UnitSummaryDto>(sql, tenantId)
            .ToListAsync();
    }

    private async Task<List<AlertDto>> EvaluateAlertsAsync(string tenantId, string role)
    {
        var alerts = new List<AlertDto>();

        var configurations = await _context.AlertConfigurations
            .Where(ac => ac.TenantId == tenantId && ac.IsEnabled)
            .ToListAsync();

        var defaultConfigs = configurations.ToDictionary(c => c.RuleType);

        var now = DateTime.UtcNow;

        if (!defaultConfigs.ContainsKey(AlertRuleType.PaymentAgreementInstallmentOverdue))
        {
            defaultConfigs[AlertRuleType.PaymentAgreementInstallmentOverdue] = new AlertConfiguration
            {
                RuleType = AlertRuleType.PaymentAgreementInstallmentOverdue,
                ThresholdDays = 5,
                DefaultUrgency = AlertUrgency.High,
                UseDefaultThreshold = true
            };
        }

        if (!defaultConfigs.ContainsKey(AlertRuleType.BudgetAccountExceeded))
        {
            defaultConfigs[AlertRuleType.BudgetAccountExceeded] = new AlertConfiguration
            {
                RuleType = AlertRuleType.BudgetAccountExceeded,
                ThresholdPercentage = 90,
                DefaultUrgency = AlertUrgency.High,
                UseDefaultThreshold = true
            };
        }

        if (!defaultConfigs.ContainsKey(AlertRuleType.AccountingPeriodNotClosed))
        {
            defaultConfigs[AlertRuleType.AccountingPeriodNotClosed] = new AlertConfiguration
            {
                RuleType = AlertRuleType.AccountingPeriodNotClosed,
                ThresholdDays = 5,
                DefaultUrgency = AlertUrgency.Critical,
                UseDefaultThreshold = true
            };
        }

        var agreementConfig = defaultConfigs.GetValueOrDefault(AlertRuleType.PaymentAgreementInstallmentOverdue);
        if (agreementConfig != null)
        {
            var overdueInstallments = await _context.AgreementInstallments
                .Where(ai => ai.TenantId == tenantId
                    && ai.Status == AgreementInstallmentStatus.Overdue
                    && ai.DueDate <= now.AddDays(-agreementConfig.ThresholdDays))
                .Join(_context.PaymentAgreements,
                    ai => ai.PaymentAgreementId,
                    pa => pa.Id,
                    (ai, pa) => new
                    {
                        ai.Id,
                        pa.UnitId,
                        ai.InstallmentNumber,
                        ai.Amount,
                        ai.DueDate
                    })
                .ToListAsync();

            foreach (var installment in overdueInstallments)
            {
                alerts.Add(new AlertDto
                {
                    Id = $"agreement_{installment.Id}",
                    RuleType = AlertRuleType.PaymentAgreementInstallmentOverdue.ToString(),
                    Urgency = AlertUrgency.High,
                    Title = $"Cuota de acuerdo de pago vencida",
                    Description = $"La cuota No. {installment.InstallmentNumber} por {installment.Amount:N0} COP venció el {installment.DueDate:dd/MM/yyyy}.",
                    ModuleLink = "/portfolio",
                    CreatedAt = now
                });
            }
        }

        return alerts.OrderByDescending(a => a.Urgency).ThenBy(a => a.CreatedAt).ToList();
    }

    private async Task<List<UpcomingEventDto>> GetUpcomingEventsAsync(string tenantId)
    {
        var now = DateTime.UtcNow;
        var thirtyDaysFromNow = now.AddDays(30);
        var events = new List<UpcomingEventDto>();

        var overdueFees = await _context.UnitFees
            .Where(uf => uf.TenantId == tenantId
                && uf.Status == FeeStatus.Overdue
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

        return events.OrderBy(e => e.EventDate).ToList();
    }

    private async Task<List<RecentActivityDto>> GetRecentActivityAsync(string tenantId)
    {
        var activities = new List<RecentActivityDto>();

        var recentPayments = await _context.Payments
            .Where(p => p.TenantId == tenantId)
            .OrderByDescending(p => p.CreatedAt)
            .Take(10)
            .Join(_context.Units,
                p => p.UnitId,
                u => u.Id,
                (p, u) => new RecentActivityDto
                {
                    Action = "Pago registrado",
                    Description = $"Pago de {p.Amount:N0} COP de la unidad {u.Identifier}",
                    UserName = p.ReceivedByUserId,
                    Timestamp = p.CreatedAt,
                    ModuleLink = "/billing/payments/register"
                })
            .ToListAsync();

        activities.AddRange(recentPayments);

        var recentAgreements = await _context.PaymentAgreements
            .Where(pa => pa.TenantId == tenantId)
            .OrderByDescending(pa => pa.CreatedAt)
            .Take(5)
            .Join(_context.Units,
                pa => pa.UnitId,
                u => u.Id,
                (pa, u) => new RecentActivityDto
                {
                    Action = "Acuerdo de pago creado",
                    Description = $"Acuerdo por {pa.TotalDebtIncluded:N0} COP de {u.Identifier}",
                    UserName = pa.CreatedByUserId,
                    Timestamp = pa.CreatedAt,
                    ModuleLink = "/portfolio"
                })
            .ToListAsync();

        activities.AddRange(recentAgreements);

        return activities.OrderByDescending(a => a.Timestamp).Take(20).ToList();
    }

    private async Task<ResidentDashboardDto> GetResidentDataAsync(string tenantId, string userId)
    {
        var data = new ResidentDashboardDto();

        var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
        if (user == null)
        {
            return data;
        }

        var owner = await _context.Owners
            .FirstOrDefaultAsync(o => o.TenantId == tenantId && o.Email == user.Email);
        if (owner == null)
        {
            return data;
        }

        var unitOwner = await _context.UnitOwners
            .Where(uo => uo.TenantId == tenantId
                && uo.OwnerId == owner.Id
                && uo.IsActive)
            .Join(_context.Units,
                uo => uo.UnitId,
                u => u.Id,
                (uo, u) => new { uo.UnitId, u.Identifier })
            .FirstOrDefaultAsync();

        if (unitOwner == null)
        {
            return data;
        }

        data.UnitIdentifier = unitOwner.Identifier;

        var overdueFees = await _context.UnitFees
            .Where(uf => uf.TenantId == tenantId
                && uf.UnitId == unitOwner.UnitId
                && uf.BalanceAmount > 0)
            .ToListAsync();

        var overdueExtra = await _context.ExtraordinaryFeeDistributions
            .Where(efd => efd.TenantId == tenantId
                && efd.UnitId == unitOwner.UnitId
                && efd.BalanceAmount > 0)
            .ToListAsync();

        var overdueCharges = await _context.IndividualCharges
            .Where(ic => ic.TenantId == tenantId
                && ic.UnitId == unitOwner.UnitId
                && ic.BalanceAmount > 0)
            .ToListAsync();

        data.CurrentBalance = overdueFees.Sum(f => f.BalanceAmount)
            + overdueExtra.Sum(f => f.BalanceAmount)
            + overdueCharges.Sum(f => f.BalanceAmount);

        var unitFeeIds = overdueFees.Select(f => f.Id).ToList();
        var lateInterests = await _context.LateInterests
            .Where(li => li.TenantId == tenantId
                && li.UnitFeeId != null
                && unitFeeIds.Contains(li.UnitFeeId.Value)
                && !li.IsCapitalized)
            .ToListAsync();

        data.LateInterestAccrued = lateInterests.Sum(li => li.CalculatedAmount);
        data.DailyInterestRate = lateInterests.Count > 0
            ? lateInterests.Max(li => li.DailyRate)
            : 0;

        var allDueDates = overdueFees.Select(f => f.DueDate)
            .Concat(overdueExtra.Select(f => f.DueDate))
            .Concat(overdueCharges.Select(f => f.ChargeDate))
            .ToList();

        if (allDueDates.Count > 0)
        {
            var oldestDate = allDueDates.Min();
            data.OldestDebtDate = oldestDate;
            data.DaysOverdue = Math.Max(0, (int)(DateTime.UtcNow - oldestDate).TotalDays);
        }

        return data;
    }

    public Task InvalidateMoraMapCacheAsync(string tenantId)
    {
        _memoryCache?.Remove($"mora_map_{tenantId}");
        return Task.CompletedTask;
    }

    public async Task InitializeDefaultAlertConfigurationsAsync(string tenantId)
    {
        var existing = await _context.AlertConfigurations
            .Where(ac => ac.TenantId == tenantId)
            .Select(ac => ac.RuleType)
            .ToListAsync();

        var existingSet = new HashSet<AlertRuleType>(existing);

        var defaults = new List<AlertConfiguration>
        {
            new AlertConfiguration
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                RuleType = AlertRuleType.PaymentAgreementInstallmentOverdue,
                IsEnabled = true,
                ThresholdDays = 5,
                DefaultUrgency = AlertUrgency.High,
                UseDefaultThreshold = true
            },
            new AlertConfiguration
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                RuleType = AlertRuleType.BudgetAccountExceeded,
                IsEnabled = true,
                ThresholdPercentage = 90,
                DefaultUrgency = AlertUrgency.High,
                UseDefaultThreshold = true
            },
            new AlertConfiguration
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                RuleType = AlertRuleType.AccountingPeriodNotClosed,
                IsEnabled = true,
                ThresholdDays = 5,
                DefaultUrgency = AlertUrgency.Critical,
                UseDefaultThreshold = true
            }
        };

        foreach (var config in defaults)
        {
            if (!existingSet.Contains(config.RuleType))
            {
                _context.AlertConfigurations.Add(config);
            }
        }

        await _context.SaveChangesAsync();
    }
}
