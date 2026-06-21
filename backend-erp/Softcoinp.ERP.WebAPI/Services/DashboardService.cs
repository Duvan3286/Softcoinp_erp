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

    public DashboardService(ApplicationDbContext context, IMemoryCache memoryCache)
    {
        _context = context;
        _memoryCache = memoryCache;
    }

    public async Task<DashboardDataDto> GetDashboardAsync(
        string tenantId, string userId, string role)
    {
        var data = new DashboardDataDto();

        if (role == AppRole.Resident.ToString())
        {
            data.ResidentData = await GetResidentDataAsync(tenantId, userId);
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
            data.Kpis = await GetKpisAsync(tenantId);
            data.MonthlyCollection = await GetMonthlyCollectionAsync(tenantId);
            data.UpcomingEvents = await GetUpcomingEventsAsync(tenantId);
            data.Alerts = await EvaluateAlertsAsync(tenantId, role);

            if (isAdmin)
            {
                data.MoraMap = await GetMoraMapAsync(tenantId);
                data.UnitSummaries = await GetUnitSummariesAsync(tenantId);
                data.RecentActivity = await GetRecentActivityAsync(tenantId);
            }

            if (isCouncil)
            {
                data.ContingencyFund = await GetContingencyFundInfoAsync(tenantId);
                data.PendingCouncilApprovals = await GetPendingCouncilApprovalsAsync(tenantId);
            }
        }

        if (isAccountant)
        {
            data.AccountingStatus = await GetAccountingStatusAsync(tenantId);
            data.Kpis = await GetKpisAsync(tenantId);
            data.MonthlyCollection = await GetMonthlyCollectionAsync(tenantId);
        }

        return data;
    }

    private async Task<DashboardKpisDto> GetKpisAsync(string tenantId)
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
            var currentFees = await _context.UnitFees
                .Where(uf => uf.TenantId == tenantId
                    && uf.BillingPeriodId == currentBillingPeriod.Id)
                .ToListAsync();

            var totalBilled = currentFees.Sum(f => f.FeeValue);
            var totalCollected = currentFees.Sum(f => f.PaidAmount);

            kpis.CurrentMonthBilled = totalBilled;
            kpis.CurrentMonthCollected = totalCollected;
            kpis.CurrentMonthCollectionPercentage = totalBilled > 0
                ? Math.Round(totalCollected / totalBilled * 100, 1)
                : 0;
        }

        if (previousBillingPeriod != null)
        {
            var previousFees = await _context.UnitFees
                .Where(uf => uf.TenantId == tenantId
                    && uf.BillingPeriodId == previousBillingPeriod.Id)
                .ToListAsync();

            var prevBilled = previousFees.Sum(f => f.FeeValue);
            var prevCollected = previousFees.Sum(f => f.PaidAmount);

            kpis.PreviousMonthCollectionPercentage = prevBilled > 0
                ? Math.Round(prevCollected / prevBilled * 100, 1)
                : 0;
        }

        var allOverdueFees = await _context.UnitFees
            .Where(uf => uf.TenantId == tenantId
                && uf.BalanceAmount > 0
                && uf.Status != FeeStatus.FullyPaid)
            .Select(uf => new
            {
                uf.BalanceAmount,
                DaysOverdue = EF.Functions.DateDiffDay(uf.DueDate, DateTime.UtcNow)
            })
            .ToListAsync();

        var totalOverdue = allOverdueFees.Sum(f => f.BalanceAmount);

        var allOverdueExtraordinary = await _context.ExtraordinaryFeeDistributions
            .Where(efd => efd.TenantId == tenantId
                && efd.BalanceAmount > 0
                && efd.Status != FeeStatus.FullyPaid)
            .Select(efd => new
            {
                efd.BalanceAmount,
                DaysOverdue = EF.Functions.DateDiffDay(efd.DueDate, DateTime.UtcNow)
            })
            .ToListAsync();

        var allOverdueCharges = await _context.IndividualCharges
            .Where(ic => ic.TenantId == tenantId
                && ic.BalanceAmount > 0
                && ic.Status != IndividualChargeStatus.Paid)
            .Select(ic => new
            {
                ic.BalanceAmount,
                DaysOverdue = EF.Functions.DateDiffDay(ic.ChargeDate, DateTime.UtcNow)
            })
            .ToListAsync();

        foreach (var item in allOverdueExtraordinary)
        {
            allOverdueFees.Add(new { BalanceAmount = item.BalanceAmount, DaysOverdue = item.DaysOverdue });
        }

        foreach (var item in allOverdueCharges)
        {
            allOverdueFees.Add(new { BalanceAmount = item.BalanceAmount, DaysOverdue = item.DaysOverdue });
        }

        kpis.TotalOverduePortfolio = totalOverdue + allOverdueExtraordinary.Sum(x => x.BalanceAmount)
            + allOverdueCharges.Sum(x => x.BalanceAmount);

        kpis.EarlyOverdue = allOverdueFees
            .Where(x => x.DaysOverdue >= 1 && x.DaysOverdue <= 90)
            .Sum(x => x.BalanceAmount);

        kpis.MediumOverdue = allOverdueFees
            .Where(x => x.DaysOverdue >= 91 && x.DaysOverdue <= 180)
            .Sum(x => x.BalanceAmount);

        kpis.LegalOverdue = allOverdueFees
            .Where(x => x.DaysOverdue > 180)
            .Sum(x => x.BalanceAmount);

        var bankAccounts = await _context.BankAccounts
            .Where(ba => ba.TenantId == tenantId && ba.IsActive)
            .ToListAsync();

        var totalBankBalance = bankAccounts.Sum(ba => ba.CurrentBalance);

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
            var actualExpenses = await _context.EntryLines
                .Where(el => el.AccountingEntry!.TenantId == tenantId
                    && el.AccountingEntry.Status == EntryStatus.Final
                    && el.AccountingEntry.EntryDate.Year == currentYear
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

        var result = new List<MonthlyCollectionDto>();

        foreach (var period in billingPeriods)
        {
            var fees = await _context.UnitFees
                .Where(uf => uf.TenantId == tenantId
                    && uf.BillingPeriodId == period.Id)
                .ToListAsync();

            result.Add(new MonthlyCollectionDto
            {
                Period = period.Period,
                Billed = fees.Sum(f => f.FeeValue),
                Collected = fees.Sum(f => f.PaidAmount)
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

        var units = await _context.Units
            .Where(u => u.TenantId == tenantId
                && (u.Status == UnitStatus.ActiveOccupied
                    || u.Status == UnitStatus.ActiveUnoccupied))
            .ToListAsync();

        var unitIds = units.Select(u => u.Id).ToList();

        var unitOwners = await _context.UnitOwners
            .Where(uo => uo.TenantId == tenantId
                && unitIds.Contains(uo.UnitId)
                && uo.IsActive
                && uo.IsSpokesperson)
            .Join(_context.Owners,
                uo => uo.OwnerId,
                o => o.Id,
                (uo, o) => new { uo.UnitId, OwnerName = o.FullNameOrCompanyName })
            .ToDictionaryAsync(x => x.UnitId, x => x.OwnerName);

        var activeUnitFees = await _context.UnitFees
            .Where(uf => uf.TenantId == tenantId
                && unitIds.Contains(uf.UnitId)
                && uf.BalanceAmount > 0
                && uf.Status != FeeStatus.FullyPaid)
            .Select(uf => new
            {
                uf.UnitId,
                uf.BalanceAmount,
                uf.DueDate
            })
            .ToListAsync();

        var extraFees = await _context.ExtraordinaryFeeDistributions
            .Where(efd => efd.TenantId == tenantId
                && unitIds.Contains(efd.UnitId)
                && efd.BalanceAmount > 0
                && efd.Status != FeeStatus.FullyPaid)
            .Select(efd => new
            {
                efd.UnitId,
                efd.BalanceAmount,
                efd.DueDate
            })
            .ToListAsync();

        var charges = await _context.IndividualCharges
            .Where(ic => ic.TenantId == tenantId
                && unitIds.Contains(ic.UnitId)
                && ic.BalanceAmount > 0
                && ic.Status != IndividualChargeStatus.Paid)
            .Select(ic => new
            {
                ic.UnitId,
                ic.BalanceAmount,
                ic.ChargeDate
            })
            .ToListAsync();

        var result = new List<UnitMoraDto>();
        var now = DateTime.UtcNow;

        foreach (var unit in units)
        {
            var unitFeeDebt = activeUnitFees
                .Where(f => f.UnitId == unit.Id)
                .Sum(f => f.BalanceAmount);

            var extraDebt = extraFees
                .Where(f => f.UnitId == unit.Id)
                .Sum(f => f.BalanceAmount);

            var chargeDebt = charges
                .Where(c => c.UnitId == unit.Id)
                .Sum(c => c.BalanceAmount);

            var totalOverdue = unitFeeDebt + extraDebt + chargeDebt;

            var allDueDates = activeUnitFees
                .Where(f => f.UnitId == unit.Id)
                .Select(f => f.DueDate)
                .Concat(extraFees
                    .Where(f => f.UnitId == unit.Id)
                    .Select(f => f.DueDate))
                .Concat(charges
                    .Where(c => c.UnitId == unit.Id)
                    .Select(c => c.ChargeDate))
                .ToList();

            var oldestDate = allDueDates.Count > 0
                ? allDueDates.Min()
                : (DateTime?)null;

            var maxDaysOverdue = oldestDate.HasValue
                ? Math.Max(0, (int)(now - oldestDate.Value).TotalDays)
                : 0;

            string colorCode;
            string status;

            if (totalOverdue <= 0)
            {
                colorCode = "green";
                status = "Al día";
            }
            else if (maxDaysOverdue <= 30)
            {
                colorCode = "yellow";
                status = "Mora temprana";
            }
            else if (maxDaysOverdue <= 90)
            {
                colorCode = "orange";
                status = "Mora media";
            }
            else
            {
                colorCode = "red";
                status = "Mora crítica";
            }

            if (unit.Status == UnitStatus.ActiveUnoccupied)
            {
                colorCode = "gray";
                status = "Desocupada";
            }

            unitOwners.TryGetValue(unit.Id, out var ownerName);

            result.Add(new UnitMoraDto
            {
                UnitId = unit.Id,
                Identifier = unit.Identifier,
                TowerOrBlock = unit.TowerOrBlock,
                FloorLevel = unit.FloorLevel,
                OwnerName = ownerName ?? "Sin propietario",
                OverdueBalance = totalOverdue,
                DaysOverdue = maxDaysOverdue,
                ColorCode = colorCode,
                Status = status
            });
        }

        var cacheOptions = new MemoryCacheEntryOptions()
            .SetAbsoluteExpiration(TimeSpan.FromMinutes(5))
            .SetPriority(CacheItemPriority.Normal);

        _memoryCache.Set(cacheKey, result, cacheOptions);

        return result;
    }

    private async Task<List<UnitSummaryDto>> GetUnitSummariesAsync(string tenantId)
    {
        var units = await _context.Units
            .Where(u => u.TenantId == tenantId)
            .ToListAsync();

        var unitIds = units.Select(u => u.Id).ToList();

        var unitOwners = await _context.UnitOwners
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
            .ToListAsync();

        var overdueExtra = await _context.ExtraordinaryFeeDistributions
            .Where(efd => efd.TenantId == tenantId
                && unitIds.Contains(efd.UnitId)
                && efd.BalanceAmount > 0)
            .GroupBy(efd => efd.UnitId)
            .Select(g => new { UnitId = g.Key, Balance = g.Sum(efd => efd.BalanceAmount) })
            .ToListAsync();

        var overdueCharges = await _context.IndividualCharges
            .Where(ic => ic.TenantId == tenantId
                && unitIds.Contains(ic.UnitId)
                && ic.BalanceAmount > 0)
            .GroupBy(ic => ic.UnitId)
            .Select(g => new { UnitId = g.Key, Balance = g.Sum(ic => ic.BalanceAmount) })
            .ToListAsync();

        var feeDict = overdueFees.ToDictionary(x => x.UnitId, x => x.Balance);
        var extraDict = overdueExtra.ToDictionary(x => x.UnitId, x => x.Balance);
        var chargeDict = overdueCharges.ToDictionary(x => x.UnitId, x => x.Balance);

        var result = new List<UnitSummaryDto>();

        foreach (var unit in units)
        {
            var balance = (feeDict.GetValueOrDefault(unit.Id, 0)
                + extraDict.GetValueOrDefault(unit.Id, 0)
                + chargeDict.GetValueOrDefault(unit.Id, 0));

            string colorCode;
            string status;

            if (balance <= 0)
            {
                colorCode = "green";
                status = "Al día";
            }
            else if (unit.Status == UnitStatus.Inactive
                || unit.Status == UnitStatus.DeliveryProcess)
            {
                colorCode = "gray";
                status = "Inactiva";
            }
            else
            {
                colorCode = "red";
                status = "En mora";
            }

            unitOwners.TryGetValue(unit.Id, out var ownerName);

            result.Add(new UnitSummaryDto
            {
                UnitId = unit.Id,
                Identifier = unit.Identifier,
                TowerOrBlock = unit.TowerOrBlock,
                FloorLevel = unit.FloorLevel,
                OwnerName = ownerName ?? "Sin propietario",
                CurrentBalance = balance,
                ColorCode = colorCode,
                Status = status
            });
        }

        return result;
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
            var budgetDetails = await _context.BudgetDetails
                .Where(bd => bd.Budget!.TenantId == tenantId
                    && bd.Budget.FiscalPeriod == currentYear
                    && bd.Budget.Status == BudgetStatus.Active)
                .ToListAsync();

            foreach (var detail in budgetDetails)
            {
                var actualExpense = await _context.EntryLines
                    .Where(el => el.AccountingEntry!.TenantId == tenantId
                        && el.AccountingEntry.Status == EntryStatus.Final
                        && el.AccountingEntry.EntryDate.Year == currentYear
                        && el.Debit > 0)
                    .Join(_context.AccountingAccounts,
                        el => el.AccountingAccountId,
                        acc => acc.Id,
                        (el, acc) => new { el.Debit, acc.Id })
                    .Where(x => x.Id == detail.AccountingAccountId)
                    .SumAsync(x => x.Debit);

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

    public async Task InvalidateMoraMapCacheAsync(string tenantId)
    {
        _memoryCache.Remove($"mora_map_{tenantId}");
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
