using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Softcoinp.ERP.Domain.Entities;
using Softcoinp.ERP.Domain.Enums;
using Softcoinp.ERP.Infrastructure.Persistence;
using Softcoinp.ERP.WebAPI.DTOs;

namespace Softcoinp.ERP.WebAPI.Services;

public class BillingEngineService
{
    private readonly ApplicationDbContext _context;
    private readonly IMemoryCache _cache;
    private readonly ILogger<BillingEngineService> _logger;
    private readonly IndicatorCacheService _indicatorCache;

    public BillingEngineService(
        ApplicationDbContext context,
        IMemoryCache cache,
        ILogger<BillingEngineService> logger,
        IndicatorCacheService indicatorCache)
    {
        _context = context;
        _cache = cache;
        _logger = logger;
        _indicatorCache = indicatorCache;
    }

    public async Task<BillingChecklistDto> GetBillingChecklistAsync(string tenantId, string period)
    {
        var result = new BillingChecklistDto();

        var fiscalYear = int.Parse(period.Substring(0, 4));
        var month = int.Parse(period.Substring(5, 2));

        var activeBudget = await _context.Budgets
            .FirstOrDefaultAsync(b => b.TenantId == tenantId
                                   && b.FiscalYear == fiscalYear
                                   && b.Status == BudgetStatus.Approved);

        result.HasActiveBudget = activeBudget != null;
        result.MonthlyBudgetTotal = activeBudget != null
            ? Math.Round((activeBudget.IncomeItems.Sum(i => i.AnnualValue) + activeBudget.ExpenseItems.Sum(e => e.AnnualValue)) / 12m, 2)
            : 0m;

        var activeUnits = await _context.Units
            .Where(u => u.TenantId == tenantId
                     && (u.Status == UnitStatus.ActiveOccupied
                      || u.Status == UnitStatus.ActiveUnoccupied
                      || u.Status == UnitStatus.DeliveryProcess))
            .ToListAsync();

        result.ActiveUnitsCount = activeUnits.Count;
        result.CoeficientSum = Math.Round(activeUnits.Sum(u => u.CoproprietyCoefficient), 4);
        result.CoeficientSumIsHundred = Math.Abs(result.CoeficientSum - 100m) < 0.0001m;

        var existingBilling = await _context.BillingPeriods
            .AnyAsync(bp => bp.TenantId == tenantId && bp.Period == period);

        result.NoExistingBillingForPeriod = !existingBilling;

        result.Warnings = new List<string>();
        if (!result.HasActiveBudget)
        {
            result.Warnings.Add("No hay un presupuesto activo para el año fiscal " + fiscalYear + ".");
        }
        if (!result.CoeficientSumIsHundred)
        {
            result.Warnings.Add("La suma de coeficientes de unidades activas es " + result.CoeficientSum.ToString("F4") + "%. Debe ser exactamente 100.0000%.");
        }
        if (existingBilling)
        {
            result.Warnings.Add("Ya existe una liquidación para el período " + period + ".");
        }
        if (result.ActiveUnitsCount == 0)
        {
            result.Warnings.Add("No hay unidades activas para liquidar.");
        }

        result.AllChecksPass = result.HasActiveBudget
                            && result.CoeficientSumIsHundred
                            && result.NoExistingBillingForPeriod
                            && result.ActiveUnitsCount > 0;

        return result;
    }

    public async Task<BillingPeriod> ExecuteMonthlyBillingAsync(
        string tenantId,
        string period,
        DateTime cutoffDate,
        DateTime paymentDueDate,
        string userId)
    {
        var checklist = await GetBillingChecklistAsync(tenantId, period);
        if (!checklist.AllChecksPass)
        {
            var errors = string.Join(" ", checklist.Warnings);
            throw new InvalidOperationException("No se puede ejecutar la liquidación. " + errors);
        }

        var activeUnits = await _context.Units
            .Where(u => u.TenantId == tenantId
                     && (u.Status == UnitStatus.ActiveOccupied
                      || u.Status == UnitStatus.ActiveUnoccupied
                      || u.Status == UnitStatus.DeliveryProcess))
            .OrderBy(u => u.Identifier)
            .ToListAsync();

        var tenantConfig = await _context.TenantConfigurations
            .FirstOrDefaultAsync(tc => tc.Id != Guid.Empty);

        var roundingPolicy = tenantConfig?.RoundingPolicy ?? RoundingPolicy.Nearest;

        var monthlyTotal = checklist.MonthlyBudgetTotal;
        var coefficientSum = checklist.CoeficientSum;
        var roundedSum = 0m;
        var unitFees = new List<UnitFee>();

        foreach (var unit in activeUnits)
        {
            var rawFee = monthlyTotal * (unit.CoproprietyCoefficient / 100m);
            var roundedFee = ApplyRounding(rawFee, roundingPolicy);

            unitFees.Add(new UnitFee
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                BillingPeriodId = Guid.Empty,
                UnitId = unit.Id,
                FeeValue = roundedFee,
                DueDate = paymentDueDate,
                Status = FeeStatus.Pending,
                PaidAmount = 0m,
                BalanceAmount = roundedFee
            });

            roundedSum += roundedFee;
        }

        var roundingAdjustment = Math.Round(monthlyTotal - roundedSum, 2);
        var totalBilled = roundedSum;

        var billingPeriod = new BillingPeriod
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            Period = period,
            MonthlyBudgetTotal = monthlyTotal,
            TotalBilled = totalBilled,
            CutoffDate = cutoffDate,
            PaymentDueDate = paymentDueDate,
            Status = BillingPeriodStatus.Executed,
            ExecutedAt = DateTime.UtcNow,
            ExecutedByUserId = userId,
            RoundingAdjustment = roundingAdjustment,
            Notes = string.Empty,
            CreatedByUserId = userId
        };

        _context.BillingPeriods.Add(billingPeriod);

        foreach (var uf in unitFees)
        {
            uf.BillingPeriodId = billingPeriod.Id;
        }

        _context.UnitFees.AddRange(unitFees);

        using var tx = await _context.Database.BeginTransactionAsync();
        try
        {
            await _context.SaveChangesAsync();

            var unitIds = unitFees.Select(uf => uf.UnitId).ToList();
            var advanceBalances = await _context.Payments
                .Where(p => p.TenantId == tenantId && unitIds.Contains(p.UnitId))
                .GroupBy(p => p.UnitId)
                .Select(g => new { UnitId = g.Key, TotalAdvance = g.Sum(p => p.AdvanceAmount) })
                .ToDictionaryAsync(g => g.UnitId, g => g.TotalAdvance);

            foreach (var uf in unitFees)
            {
                advanceBalances.TryGetValue(uf.UnitId, out var advanceBalance);

                if (advanceBalance <= 0m || uf.BalanceAmount <= 0m)
                    continue;

                var applied = Math.Min(advanceBalance, uf.BalanceAmount);
                if (applied <= 0m)
                    continue;

                uf.PaidAmount += applied;
                uf.BalanceAmount -= applied;
                uf.UpdatedAt = DateTime.UtcNow;

                if (uf.BalanceAmount <= 0m)
                {
                    uf.Status = FeeStatus.FullyPaid;
                    uf.BalanceAmount = 0m;
                }
                else
                {
                    uf.Status = FeeStatus.PartiallyPaid;
                }

                var advancePayment = new Payment
                {
                    Id = Guid.NewGuid(),
                    TenantId = tenantId,
                    UnitId = uf.UnitId,
                    PaymentDate = DateTime.UtcNow,
                    Amount = 0m,
                    PaymentMethod = PaymentMethod.Cash,
                    ReferenceNumber = $"ADV-{billingPeriod.Id:N}",
                    Notes = "Aplicacion automatica de saldo a favor",
                    ReceivedByUserId = "SYSTEM",
                    AdvanceAmount = -applied
                };

                _context.Payments.Add(advancePayment);

                _context.PaymentAllocations.Add(new PaymentAllocation
                {
                    Id = Guid.NewGuid(),
                    TenantId = tenantId,
                    PaymentId = advancePayment.Id,
                    UnitFeeId = uf.Id,
                    Amount = applied,
                    AllocationType = PaymentAllocationType.Advance
                });
            }

            await _context.SaveChangesAsync();

            await tx.CommitAsync();
        }
        catch
        {
            await tx.RollbackAsync();
            throw;
        }

        _cache.Remove($"mora_map_{tenantId}");
        await _indicatorCache.InvalidateAsync(tenantId, "kpis_");
        return billingPeriod;
    }

    public async Task<BillingPeriodDetailDto> GetBillingPeriodDetailAsync(string tenantId, Guid billingPeriodId)
    {
        var billingPeriod = await _context.BillingPeriods
            .FirstOrDefaultAsync(bp => bp.Id == billingPeriodId && bp.TenantId == tenantId);

        if (billingPeriod == null)
        {
            throw new KeyNotFoundException("No se encontró el período de liquidación.");
        }

        var unitFees = await _context.UnitFees
            .Where(uf => uf.BillingPeriodId == billingPeriodId)
            .Join(_context.Units,
                  uf => uf.UnitId,
                  u => u.Id,
                  (uf, u) => new UnitFeeDto
                  {
                      Id = uf.Id,
                      UnitId = uf.UnitId,
                      UnitIdentifier = u.Identifier,
                      UnitTower = u.TowerOrBlock,
                      Coefficient = u.CoproprietyCoefficient,
                      FeeValue = uf.FeeValue,
                      DueDate = uf.DueDate,
                      Status = uf.Status.ToString(),
                      PaidAmount = uf.PaidAmount,
                      BalanceAmount = uf.BalanceAmount
                  })
            .OrderBy(d => d.UnitTower)
            .ThenBy(d => d.UnitIdentifier)
            .ToListAsync();

        return new BillingPeriodDetailDto
        {
            Id = billingPeriod.Id,
            Period = billingPeriod.Period,
            MonthlyBudgetTotal = billingPeriod.MonthlyBudgetTotal,
            CutoffDate = billingPeriod.CutoffDate,
            PaymentDueDate = billingPeriod.PaymentDueDate,
            Status = billingPeriod.Status.ToString(),
            ExecutedAt = billingPeriod.ExecutedAt,
            ExecutedByUserId = billingPeriod.ExecutedByUserId,
            RoundingAdjustment = billingPeriod.RoundingAdjustment,
            Notes = billingPeriod.Notes,
            UnitFees = unitFees
        };
    }

    public async Task<List<BillingPeriodSummaryDto>> GetBillingPeriodsAsync(string tenantId)
    {
        var periods = await _context.BillingPeriods
            .Where(bp => bp.TenantId == tenantId)
            .OrderByDescending(bp => bp.Period)
            .ToListAsync();

        var periodIds = periods.Select(bp => bp.Id).ToList();

        var feeStats = await _context.UnitFees
            .Where(uf => periodIds.Contains(uf.BillingPeriodId))
            .GroupBy(uf => uf.BillingPeriodId)
            .Select(g => new
            {
                BillingPeriodId = g.Key,
                UnitsCount = g.Count(),
                TotalBilled = g.Sum(uf => uf.FeeValue)
            })
            .ToDictionaryAsync(g => g.BillingPeriodId);

        return periods.Select(bp =>
        {
            feeStats.TryGetValue(bp.Id, out var stats);

            return new BillingPeriodSummaryDto
            {
                Id = bp.Id,
                Period = bp.Period,
                MonthlyBudgetTotal = bp.MonthlyBudgetTotal,
                CutoffDate = bp.CutoffDate,
                PaymentDueDate = bp.PaymentDueDate,
                Status = bp.Status.ToString(),
                ExecutedAt = bp.ExecutedAt,
                ExecutedByUserId = bp.ExecutedByUserId,
                RoundingAdjustment = bp.RoundingAdjustment,
                UnitsCount = stats?.UnitsCount ?? 0,
                TotalBilled = stats?.TotalBilled ?? 0m
            };
        }).ToList();
    }

    private static decimal ApplyRounding(decimal value, RoundingPolicy policy)
    {
        return policy switch
        {
            RoundingPolicy.Up => Math.Ceiling(value * 100m) / 100m,
            RoundingPolicy.Down => Math.Floor(value * 100m) / 100m,
            _ => Math.Round(value, 2, MidpointRounding.AwayFromZero)
        };
    }
}
