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

            if (isCouncil)
            {
                try { data.ContingencyFund = await GetContingencyFundInfoAsync(tenantId); }
                catch (Exception ex) { _ = ex; }

                try { data.PendingCouncilApprovals = await GetPendingCouncilApprovalsAsync(tenantId); }
                catch (Exception ex) { _ = ex; }
            }
        }

        if (isAccountant)
        {
            try { data.AccountingStatus = await GetAccountingStatusAsync(tenantId); }
            catch (Exception ex) { _ = ex; }

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

        var pendingPayables = await _context.AccountingEntries
            .Where(ae => ae.TenantId == tenantId
                && ae.Status == EntryStatus.Final
                && ae.EntryType == EntryType.Manual)
            .Join(_context.EntryLines,
                ae => ae.Id,
                el => el.AccountingEntryId,
                (ae, el) => new { el.AccountingAccountId, el.Credit })
            .Join(_context.AccountingAccounts,
                el => el.AccountingAccountId,
                acc => acc.Id,
                (el, acc) => new { acc.Code, el.Credit })
            .Where(x => x.Code.StartsWith("2335"))
            .SumAsync(x => x.Credit);

        kpis.AvailableCash = totalBankBalance - pendingPayables;

        var currentYear = now.Year;
        var yearStart = new DateTime(currentYear, 1, 1);
        var yearProgress = (now - yearStart).TotalDays / (DateTime.IsLeapYear(currentYear) ? 366 : 365);
        kpis.YearProgressPercentage = Math.Round((decimal)(yearProgress * 100), 1);

        var totalBudgetApproved = await _context.BudgetDetails
            .Where(bd => bd.Budget!.TenantId == tenantId
                && bd.Budget.FiscalPeriod == currentYear
                && bd.Budget.Status == BudgetStatus.Active)
            .Join(_context.AccountingAccounts,
                bd => bd.AccountingAccountId,
                acc => acc.Id,
                (bd, acc) => new { bd.ApprovedValue, acc.Category })
            .Where(x => x.Category == AccountingAccountCategory.Expense)
            .SumAsync(x => x.ApprovedValue);

        if (totalBudgetApproved > 0)
        {
            var yearEnd = new DateTime(currentYear + 1, 1, 1);
            var actualExpenses = await _context.EntryLines
                .Where(el => el.AccountingEntry!.TenantId == tenantId
                    && el.AccountingEntry.Status == EntryStatus.Final
                    && el.AccountingEntry.EntryDate >= yearStart
                    && el.AccountingEntry.EntryDate < yearEnd
                    && el.Debit > 0)
                .Join(_context.AccountingAccounts,
                    el => el.AccountingAccountId,
                    acc => acc.Id,
                    (el, acc) => new { acc.Category, el.Debit })
                .Where(x => x.Category == AccountingAccountCategory.Expense)
                .SumAsync(x => x.Debit);

            kpis.BudgetExecutionPercentage = Math.Round(actualExpenses / totalBudgetApproved * 100, 1);
        }

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

        var units = await _context.Units
            .Where(u => u.TenantId == tenantId
                && (u.Status == UnitStatus.ActiveOccupied
                    || u.Status == UnitStatus.ActiveUnoccupied))
            .Select(u => new UnitMoraDto
            {
                UnitId = u.Id,
                Identifier = u.Identifier,
                TowerOrBlock = u.TowerOrBlock,
                FloorLevel = u.FloorLevel,
                OwnerName = "Sin propietario",
                OverdueBalance = 0m,
                DaysOverdue = 0,
                ColorCode = u.Status == UnitStatus.ActiveUnoccupied ? "gray" : "green",
                Status = u.Status == UnitStatus.ActiveUnoccupied ? "Desocupada" : "Al día"
            })
            .ToListAsync();

        if (units.Count == 0)
        {
            return units;
        }

        var unitIds = units.Select(u => u.UnitId).ToList();

        var spokespersons = await _context.UnitOwners
            .Where(uo => uo.TenantId == tenantId
                && unitIds.Contains(uo.UnitId)
                && uo.IsActive
                && uo.IsSpokesperson)
            .Join(_context.Owners,
                uo => uo.OwnerId,
                o => o.Id,
                (uo, o) => new { uo.UnitId, OwnerName = o.FullNameOrCompanyName })
            .ToDictionaryAsync(x => x.UnitId, x => x.OwnerName);

        var now_date = now;
        var unitFeeDebts = await _context.UnitFees
            .Where(uf => uf.TenantId == tenantId
                && unitIds.Contains(uf.UnitId)
                && uf.BalanceAmount > 0
                && uf.Status != FeeStatus.FullyPaid)
            .GroupBy(uf => uf.UnitId)
            .Select(g => new
            {
                UnitId = g.Key,
                TotalDebt = g.Sum(uf => uf.BalanceAmount),
                OldestDate = g.Min(uf => uf.DueDate)
            })
            .ToListAsync();

        var extraDebts = await _context.ExtraordinaryFeeDistributions
            .Where(efd => efd.TenantId == tenantId
                && unitIds.Contains(efd.UnitId)
                && efd.BalanceAmount > 0
                && efd.Status != FeeStatus.FullyPaid)
            .GroupBy(efd => efd.UnitId)
            .Select(g => new
            {
                UnitId = g.Key,
                TotalDebt = g.Sum(efd => efd.BalanceAmount),
                OldestDate = g.Min(efd => efd.DueDate)
            })
            .ToListAsync();

        var chargeDebts = await _context.IndividualCharges
            .Where(ic => ic.TenantId == tenantId
                && unitIds.Contains(ic.UnitId)
                && ic.BalanceAmount > 0
                && ic.Status != IndividualChargeStatus.Paid)
            .GroupBy(ic => ic.UnitId)
            .Select(g => new
            {
                UnitId = g.Key,
                TotalDebt = g.Sum(ic => ic.BalanceAmount),
                OldestDate = g.Min(ic => ic.ChargeDate)
            })
            .ToListAsync();

        var feeDict = unitFeeDebts.ToDictionary(x => x.UnitId);
        var extraDict = extraDebts.ToDictionary(x => x.UnitId);
        var chargeDict = chargeDebts.ToDictionary(x => x.UnitId);

        foreach (var unit in units)
        {
            spokespersons.TryGetValue(unit.UnitId, out var ownerName);
            if (ownerName != null) unit.OwnerName = ownerName;

            feeDict.TryGetValue(unit.UnitId, out var feeDebt);
            extraDict.TryGetValue(unit.UnitId, out var extraDebt);
            chargeDict.TryGetValue(unit.UnitId, out var chargeDebt);

            var totalOverdue = (feeDebt?.TotalDebt ?? 0m) + (extraDebt?.TotalDebt ?? 0m) + (chargeDebt?.TotalDebt ?? 0m);
            unit.OverdueBalance = totalOverdue;

            var allDates = new List<DateTime>();
            if (feeDebt != null && feeDebt.OldestDate != default) allDates.Add(feeDebt.OldestDate);
            if (extraDebt != null && extraDebt.OldestDate != default) allDates.Add(extraDebt.OldestDate);
            if (chargeDebt != null && chargeDebt.OldestDate != default) allDates.Add(chargeDebt.OldestDate);

            var oldestDate = allDates.Count > 0 ? allDates.Min() : (DateTime?)null;
            var maxDaysOverdue = oldestDate.HasValue
                ? Math.Max(0, (int)(now - oldestDate.Value).TotalDays)
                : 0;

            unit.DaysOverdue = maxDaysOverdue;

            if (unit.ColorCode == "gray")
            {
                // Already set for ActiveUnoccupied
            }
            else if (totalOverdue <= 0)
            {
                unit.ColorCode = "green";
                unit.Status = "Al día";
            }
            else if (maxDaysOverdue <= 30)
            {
                unit.ColorCode = "yellow";
                unit.Status = "Mora temprana";
            }
            else if (maxDaysOverdue <= 90)
            {
                unit.ColorCode = "orange";
                unit.Status = "Mora media";
            }
            else
            {
                unit.ColorCode = "red";
                unit.Status = "Mora crítica";
            }
        }

        var cacheOptions = new MemoryCacheEntryOptions()
            .SetAbsoluteExpiration(TimeSpan.FromMinutes(5))
            .SetPriority(CacheItemPriority.Normal);

        _memoryCache.Set(cacheKey, units, cacheOptions);

        return units;
    }

    private async Task<List<UnitSummaryDto>> GetUnitSummariesAsync(string tenantId)
    {
        var units = await _context.Units
            .Where(u => u.TenantId == tenantId)
            .Select(u => new UnitSummaryDto
            {
                UnitId = u.Id,
                Identifier = u.Identifier,
                TowerOrBlock = u.TowerOrBlock,
                FloorLevel = u.FloorLevel,
                OwnerName = "Sin propietario",
                CurrentBalance = 0m,
                ColorCode = (u.Status == UnitStatus.Inactive || u.Status == UnitStatus.DeliveryProcess) ? "gray" : "green",
                Status = (u.Status == UnitStatus.Inactive || u.Status == UnitStatus.DeliveryProcess) ? "Inactiva" : "Al día"
            })
            .ToListAsync();

        if (units.Count == 0)
        {
            return units;
        }

        var unitIds = units.Select(u => u.UnitId).ToList();

        var spokespersons = await _context.UnitOwners
            .Where(uo => uo.TenantId == tenantId
                && unitIds.Contains(uo.UnitId)
                && uo.IsActive
                && uo.IsSpokesperson)
            .Join(_context.Owners,
                uo => uo.OwnerId,
                o => o.Id,
                (uo, o) => new { uo.UnitId, OwnerName = o.FullNameOrCompanyName })
            .ToDictionaryAsync(x => x.UnitId, x => x.OwnerName);

        var overdueFees = await _context.UnitFees
            .Where(uf => uf.TenantId == tenantId
                && unitIds.Contains(uf.UnitId)
                && uf.BalanceAmount > 0)
            .GroupBy(uf => uf.UnitId)
            .Select(g => new { UnitId = g.Key, Balance = g.Sum(uf => uf.BalanceAmount) })
            .ToDictionaryAsync(g => g.UnitId, g => g.Balance);

        var overdueExtra = await _context.ExtraordinaryFeeDistributions
            .Where(efd => efd.TenantId == tenantId
                && unitIds.Contains(efd.UnitId)
                && efd.BalanceAmount > 0)
            .GroupBy(efd => efd.UnitId)
            .Select(g => new { UnitId = g.Key, Balance = g.Sum(efd => efd.BalanceAmount) })
            .ToDictionaryAsync(g => g.UnitId, g => g.Balance);

        var overdueCharges = await _context.IndividualCharges
            .Where(ic => ic.TenantId == tenantId
                && unitIds.Contains(ic.UnitId)
                && ic.BalanceAmount > 0)
            .GroupBy(ic => ic.UnitId)
            .Select(g => new { UnitId = g.Key, Balance = g.Sum(ic => ic.BalanceAmount) })
            .ToDictionaryAsync(g => g.UnitId, g => g.Balance);

        foreach (var unit in units)
        {
            spokespersons.TryGetValue(unit.UnitId, out var ownerName);
            if (ownerName != null) unit.OwnerName = ownerName;

            overdueFees.TryGetValue(unit.UnitId, out var feeBalance);
            overdueExtra.TryGetValue(unit.UnitId, out var extraBalance);
            overdueCharges.TryGetValue(unit.UnitId, out var chargeBalance);

            var balance = feeBalance + extraBalance + chargeBalance;
            unit.CurrentBalance = balance;

            if (unit.ColorCode == "gray")
            {
                // Already set for Inactive/DeliveryProcess
            }
            else if (balance > 0)
            {
                unit.ColorCode = "red";
                unit.Status = "En mora";
            }
        }

        return units;
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

        var budgetConfig = defaultConfigs.GetValueOrDefault(AlertRuleType.BudgetAccountExceeded);
        if (budgetConfig != null)
        {
            var currentYear = now.Year;
            var budgetYearStart = new DateTime(currentYear, 1, 1);
            var budgetYearEnd = new DateTime(currentYear + 1, 1, 1);
            var budgetDetails = await _context.BudgetDetails
                .Where(bd => bd.Budget!.TenantId == tenantId
                    && bd.Budget.FiscalPeriod == currentYear
                    && bd.Budget.Status == BudgetStatus.Active)
                .ToListAsync();

            var budgetAccountIds = budgetDetails.Select(bd => bd.AccountingAccountId).Distinct().ToList();

            var actualExpensesByAccount = await _context.EntryLines
                .Where(el => el.AccountingEntry!.TenantId == tenantId
                    && el.AccountingEntry.Status == EntryStatus.Final
                    && el.AccountingEntry.EntryDate >= budgetYearStart
                    && el.AccountingEntry.EntryDate < budgetYearEnd
                    && el.Debit > 0
                    && budgetAccountIds.Contains(el.AccountingAccountId))
                .GroupBy(el => el.AccountingAccountId)
                .Select(g => new { AccountId = g.Key, TotalDebit = g.Sum(el => el.Debit) })
                .ToDictionaryAsync(g => g.AccountId, g => g.TotalDebit);

            foreach (var detail in budgetDetails)
            {
                actualExpensesByAccount.TryGetValue(detail.AccountingAccountId, out var actualExpense);

                var executionPercentage = detail.ApprovedValue > 0
                    ? Math.Round(actualExpense / detail.ApprovedValue * 100, 1)
                    : 0;

                if (executionPercentage >= budgetConfig.ThresholdPercentage)
                {
                    var account = await _context.AccountingAccounts
                        .FirstOrDefaultAsync(a => a.Id == detail.AccountingAccountId);

                    alerts.Add(new AlertDto
                    {
                        Id = $"budget_{detail.Id}",
                        RuleType = AlertRuleType.BudgetAccountExceeded.ToString(),
                        Urgency = AlertUrgency.High,
                        Title = $"Presupuesto de cuenta excedido",
                        Description = $"La cuenta {account?.Name ?? "N/A"} ha ejecutado el {executionPercentage}% de su presupuesto anual.",
                        ModuleLink = "/budgets",
                        CreatedAt = now
                    });
                }
            }
        }

        var periodConfig = defaultConfigs.GetValueOrDefault(AlertRuleType.AccountingPeriodNotClosed);
        if (periodConfig != null)
        {
            var previousMonthDate = now.AddMonths(-1);
            var previousMonthPeriod = await _context.AccountingPeriods
                .FirstOrDefaultAsync(ap =>
                    ap.TenantId == tenantId
                    && ap.FiscalYear == previousMonthDate.Year
                    && ap.Month == previousMonthDate.Month
                    && ap.Status == AccountingPeriodStatus.Open);

            if (previousMonthPeriod != null)
            {
                var daysSinceMonthEnd = (int)(now - new DateTime(previousMonthDate.Year, previousMonthDate.Month, 1).AddMonths(1)).TotalDays;

                if (daysSinceMonthEnd >= periodConfig.ThresholdDays)
                {
                    alerts.Add(new AlertDto
                    {
                        Id = $"period_{previousMonthPeriod.Id}",
                        RuleType = AlertRuleType.AccountingPeriodNotClosed.ToString(),
                        Urgency = AlertUrgency.Critical,
                        Title = $"Período contable sin cerrar",
                        Description = $"El período {previousMonthPeriod.PeriodLabel} lleva {daysSinceMonthEnd} días del mes siguiente sin cerrarse.",
                        ModuleLink = "/accounting/periods",
                        CreatedAt = now
                    });
                }
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

        var recentEntries = await _context.AccountingEntries
            .Where(ae => ae.TenantId == tenantId)
            .OrderByDescending(ae => ae.CreatedAt)
            .Take(5)
            .Select(ae => new RecentActivityDto
            {
                Action = ae.EntryType == EntryType.Automatic
                    ? "Asiento automático generado"
                    : "Asiento contable creado",
                Description = $"Asiento No. {ae.EntryNumber}: {ae.Description}",
                UserName = ae.CreatedByUserId,
                Timestamp = ae.CreatedAt,
                ModuleLink = $"/accounting/journal-entries/{ae.Id}"
            })
            .ToListAsync();

        activities.AddRange(recentEntries);

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

    private async Task<ContingencyFundInfoDto?> GetContingencyFundInfoAsync(string tenantId)
    {
        var fund = await _context.ContingencyFunds
            .FirstOrDefaultAsync(cf => cf.TenantId == tenantId);

        if (fund == null)
        {
            return null;
        }

        var lastContribution = await _context.ContingencyFundContributions
            .Where(cc => cc.TenantId == tenantId)
            .OrderByDescending(cc => cc.ContributionDate)
            .FirstOrDefaultAsync();

        return new ContingencyFundInfoDto
        {
            CurrentBalance = fund.CurrentBalance,
            LastContributionAmount = lastContribution?.Amount ?? 0,
            LastContributionPeriod = lastContribution?.Period ?? string.Empty
        };
    }

    private async Task<List<CouncilApprovalDto>> GetPendingCouncilApprovalsAsync(string tenantId)
    {
        var approvals = new List<CouncilApprovalDto>();

        var pendingTransfers = await _context.BudgetMovements
            .Where(bm => bm.TenantId == tenantId
                && bm.Budget!.TenantId == tenantId
                && bm.MovementType == BudgetMovementType.Transfer) // Using Transfer as a placeholder filter; adjust if enum differs
            .OrderByDescending(bm => bm.CreatedAt)
            .Take(5)
            .Select(bm => new CouncilApprovalDto
            {
                Type = "Traslado presupuestal",
                Description = bm.Justification,
                Amount = bm.Amount,
                RequestedAt = bm.CreatedAt,
                ModuleLink = "/budgets"
            })
            .ToListAsync();

        approvals.AddRange(pendingTransfers);

        return approvals;
    }

    private async Task<AccountingStatusDto> GetAccountingStatusAsync(string tenantId)
    {
        var now = DateTime.UtcNow;
        var currentPeriod = await _context.AccountingPeriods
            .FirstOrDefaultAsync(ap =>
                ap.TenantId == tenantId
                && ap.FiscalYear == now.Year
                && ap.Month == now.Month);

        var daysSinceMonthEnd = (int)(now - new DateTime(now.Year, now.Month, 1).AddMonths(1)).TotalDays;

        var unreconciledCount = await _context.BankReconciliations
            .CountAsync(br => br.TenantId == tenantId
                && br.Status == ReconciliationStatus.InProgress);

        var draftCount = await _context.AccountingEntries
            .CountAsync(ae => ae.TenantId == tenantId
                && ae.Status == EntryStatus.Draft);

        return new AccountingStatusDto
        {
            CurrentPeriodLabel = currentPeriod?.PeriodLabel ?? $"{now.Year}-{now.Month:D2}",
            PeriodStatus = currentPeriod?.Status.ToString() ?? "Open",
            UnreconciledBankAccounts = unreconciledCount,
            DraftEntryCount = draftCount,
            DaysSinceMonthEnd = Math.Max(0, daysSinceMonthEnd)
        };
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
