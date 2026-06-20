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
    private readonly AccountingIntegrationService _accounting;
    private readonly ContingencyFundService _contingencyFund;
    private readonly IMemoryCache _cache;
    private readonly ILogger<BillingEngineService> _logger;

    public BillingEngineService(
        ApplicationDbContext context,
        AccountingIntegrationService accounting,
        ContingencyFundService contingencyFund,
        IMemoryCache cache,
        ILogger<BillingEngineService> logger)
    {
        _context = context;
        _accounting = accounting;
        _contingencyFund = contingencyFund;
        _cache = cache;
        _logger = logger;
    }

    public async Task<BillingChecklistDto> GetBillingChecklistAsync(string tenantId, string period)
    {
        var result = new BillingChecklistDto();

        var fiscalYear = int.Parse(period.Substring(0, 4));
        var month = int.Parse(period.Substring(5, 2));

        var activeBudget = await _context.Budgets
            .FirstOrDefaultAsync(b => b.TenantId == tenantId
                                   && b.FiscalPeriod == fiscalYear
                                   && b.Status == BudgetStatus.Active);

        result.HasActiveBudget = activeBudget != null;
        result.MonthlyBudgetTotal = activeBudget != null
            ? Math.Round(activeBudget.BudgetDetails.Sum(d => d.ApprovedValue) / 12m, 2)
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
        var rawSum = 0m;
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

            rawSum += roundedFee;
        }

        var roundingAdjustment = Math.Round(monthlyTotal - rawSum, 2);

        var billingPeriod = new BillingPeriod
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            Period = period,
            MonthlyBudgetTotal = monthlyTotal,
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
        await _context.SaveChangesAsync();

        try
        {
            await _accounting.RecordBillingAsync(
                tenantId,
                billingPeriod.Id,
                billingPeriod.MonthlyBudgetTotal,
                $"Liquidación mensual {period}",
                userId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al registrar asiento contable de liquidación {Period} para tenant {TenantId}", period, tenantId);
        }

        try
        {
            var year = int.Parse(period.Substring(0, 4));
            var month = int.Parse(period.Substring(5, 2));
            await _contingencyFund.LiquidateMonthlyContributionAsync(tenantId, year, month);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al liquidar fondo de imprevistos para período {Period} tenant {TenantId}", period, tenantId);
        }

        _cache.Remove($"mora_map_{tenantId}");
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
            .Select(bp => new BillingPeriodSummaryDto
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
                UnitsCount = _context.UnitFees.Count(uf => uf.BillingPeriodId == bp.Id),
                TotalBilled = _context.UnitFees.Where(uf => uf.BillingPeriodId == bp.Id).Sum(uf => uf.FeeValue)
            })
            .ToListAsync();

        return periods;
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
