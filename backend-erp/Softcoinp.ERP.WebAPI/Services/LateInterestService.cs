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

public class LateInterestService
{
    private readonly ApplicationDbContext _context;
    private readonly AccountingIntegrationService _accounting;
    private readonly IMemoryCache _cache;
    private readonly ILogger<LateInterestService> _logger;

    public LateInterestService(ApplicationDbContext context, AccountingIntegrationService accounting, IMemoryCache cache, ILogger<LateInterestService> logger)
    {
        _context = context;
        _accounting = accounting;
        _cache = cache;
        _logger = logger;
    }

    public async Task<decimal> GetMonthlyRateAsync(string tenantId)
    {
        var config = await _context.TenantConfigurations
            .FirstOrDefaultAsync();

        return config?.LatePaymentInterestRate ?? 0m;
    }

    public decimal GetDailyRate(decimal monthlyRate)
    {
        if (monthlyRate <= 0m) return 0m;
        var monthlyDecimal = monthlyRate / 100m;
        var dailyRate = Math.Pow((double)(1m + monthlyDecimal), 1.0 / 30.0) - 1.0;
        return Math.Round((decimal)dailyRate, 8);
    }

    public async Task<List<LateInterestPreviewDto>> PreviewUnitInterestAsync(
        string tenantId, Guid unitId, DateTime asOfDate)
    {
        var monthlyRate = await GetMonthlyRateAsync(tenantId);
        var dailyRate = GetDailyRate(monthlyRate);
        var result = new List<LateInterestPreviewDto>();

        if (dailyRate <= 0m) return result;

        var overdueFees = await _context.UnitFees
            .Where(uf => uf.TenantId == tenantId
                      && uf.UnitId == unitId
                      && uf.BalanceAmount > 0
                      && uf.DueDate < asOfDate)
            .ToListAsync();

        foreach (var fee in overdueFees)
        {
            var daysOverdue = Math.Max(0, (int)(asOfDate - fee.DueDate).TotalDays);
            if (daysOverdue <= 0) continue;

            var interest = Math.Round(fee.BalanceAmount * dailyRate * daysOverdue, 2);

            result.Add(new LateInterestPreviewDto
            {
                SourceType = "UnitFee",
                SourceId = fee.Id,
                BalanceAmount = fee.BalanceAmount,
                DaysOverdue = daysOverdue,
                DailyRate = dailyRate,
                CalculatedInterest = interest
            });
        }

        var overdueExtraordinary = await _context.ExtraordinaryFeeDistributions
            .Where(ed => ed.TenantId == tenantId
                      && ed.UnitId == unitId
                      && ed.BalanceAmount > 0
                      && ed.DueDate < asOfDate)
            .ToListAsync();

        foreach (var dist in overdueExtraordinary)
        {
            var daysOverdue = Math.Max(0, (int)(asOfDate - dist.DueDate).TotalDays);
            if (daysOverdue <= 0) continue;

            var interest = Math.Round(dist.BalanceAmount * dailyRate * daysOverdue, 2);

            result.Add(new LateInterestPreviewDto
            {
                SourceType = "ExtraordinaryFee",
                SourceId = dist.Id,
                BalanceAmount = dist.BalanceAmount,
                DaysOverdue = daysOverdue,
                DailyRate = dailyRate,
                CalculatedInterest = interest
            });
        }

        var disputedChargeIds = await _context.IndividualCharges
            .Where(ic => ic.TenantId == tenantId
                      && ic.UnitId == unitId
                      && ic.IsDisputed)
            .Select(ic => ic.Id)
            .ToListAsync();

        var overdueCharges = await _context.IndividualCharges
            .Where(ic => ic.TenantId == tenantId
                      && ic.UnitId == unitId
                      && ic.BalanceAmount > 0
                      && !ic.IsDisputed)
            .ToListAsync();

        foreach (var charge in overdueCharges)
        {
            var daysOverdue = Math.Max(0, (int)(asOfDate - charge.ChargeDate).TotalDays);
            if (daysOverdue <= 0) continue;

            var interest = Math.Round(charge.BalanceAmount * dailyRate * daysOverdue, 2);

            result.Add(new LateInterestPreviewDto
            {
                SourceType = "IndividualCharge",
                SourceId = charge.Id,
                BalanceAmount = charge.BalanceAmount,
                DaysOverdue = daysOverdue,
                DailyRate = dailyRate,
                CalculatedInterest = interest
            });
        }

        return result;
    }

    public async Task<List<LateInterest>> CapitalizeInterestAsync(
        string tenantId, string sourceType, Guid sourceId, string period, string userId)
    {
        var monthlyRate = await GetMonthlyRateAsync(tenantId);
        var dailyRate = GetDailyRate(monthlyRate);
        var result = new List<LateInterest>();

        if (dailyRate <= 0m) return result;

        var now = DateTime.UtcNow;
        int daysOverdue;
        decimal balanceAmount;
        Guid? unitFeeId = null;
        Guid? extraordinaryFeeDistributionId = null;
        Guid? individualChargeId = null;

        switch (sourceType)
        {
            case "UnitFee":
                var fee = await _context.UnitFees
                    .FirstOrDefaultAsync(uf => uf.Id == sourceId && uf.TenantId == tenantId);
                if (fee == null)
                    throw new KeyNotFoundException("No se encontró la cuota ordinaria especificada.");
                if (fee.BalanceAmount <= 0m)
                    throw new InvalidOperationException("La cuota no tiene saldo pendiente.");
                balanceAmount = fee.BalanceAmount;
                daysOverdue = Math.Max(0, (int)(now - fee.DueDate).TotalDays);
                unitFeeId = fee.Id;
                break;

            case "ExtraordinaryFee":
                var ed = await _context.ExtraordinaryFeeDistributions
                    .FirstOrDefaultAsync(e => e.Id == sourceId && e.TenantId == tenantId);
                if (ed == null)
                    throw new KeyNotFoundException("No se encontró la cuota extraordinaria especificada.");
                if (ed.BalanceAmount <= 0m)
                    throw new InvalidOperationException("La cuota extraordinaria no tiene saldo pendiente.");
                balanceAmount = ed.BalanceAmount;
                daysOverdue = Math.Max(0, (int)(now - ed.DueDate).TotalDays);
                extraordinaryFeeDistributionId = ed.Id;
                break;

            case "IndividualCharge":
                var charge = await _context.IndividualCharges
                    .FirstOrDefaultAsync(ic => ic.Id == sourceId && ic.TenantId == tenantId);
                if (charge == null)
                    throw new KeyNotFoundException("No se encontró el cobro individual especificado.");
                if (charge.BalanceAmount <= 0m)
                    throw new InvalidOperationException("El cobro individual no tiene saldo pendiente.");
                if (charge.IsDisputed)
                    throw new InvalidOperationException("No se puede capitalizar intereses sobre un cobro individual disputado.");
                balanceAmount = charge.BalanceAmount;
                daysOverdue = Math.Max(0, (int)(now - charge.ChargeDate).TotalDays);
                individualChargeId = charge.Id;
                break;

            default:
                throw new ArgumentException($"Tipo de origen inválido: {sourceType}");
        }

        if (daysOverdue <= 0) return result;

        var interest = Math.Round(balanceAmount * dailyRate * daysOverdue, 2);

        var lateInterest = new LateInterest
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            UnitFeeId = unitFeeId,
            ExtraordinaryFeeDistributionId = extraordinaryFeeDistributionId,
            IndividualChargeId = individualChargeId,
            Period = period,
            BaseAmount = balanceAmount,
            DailyRate = dailyRate,
            DaysOverdue = daysOverdue,
            CalculatedAmount = interest,
            IsCapitalized = true
        };

        _context.LateInterests.Add(lateInterest);
        await _context.SaveChangesAsync();

        try
        {
            await _accounting.RecordLateInterestAsync(
                tenantId, lateInterest.Id, lateInterest.CalculatedAmount,
                $"Capitalización de intereses mora {sourceType} período {period}", userId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al registrar asiento contable de interés mora {InterestId} para tenant {TenantId}", lateInterest.Id, tenantId);
        }

        _cache.Remove($"mora_map_{tenantId}");
        result.Add(lateInterest);
        return result;
    }

    public async Task<List<LateInterest>> CapitalizeAllOverdueInterestAsync(
        string tenantId, string period, string userId)
    {
        var monthlyRate = await GetMonthlyRateAsync(tenantId);
        var dailyRate = GetDailyRate(monthlyRate);
        var result = new List<LateInterest>();

        if (dailyRate <= 0m) return result;

        var now = DateTime.UtcNow;

        await foreach (var fee in _context.UnitFees
            .Where(uf => uf.TenantId == tenantId
                      && uf.BalanceAmount > 0
                      && uf.DueDate < now)
            .AsAsyncEnumerable())
        {
            var daysOverdue = Math.Max(0, (int)(now - fee.DueDate).TotalDays);
            if (daysOverdue <= 0) continue;

            var interest = Math.Round(fee.BalanceAmount * dailyRate * daysOverdue, 2);
            if (interest <= 0m) continue;

            result.Add(new LateInterest
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                UnitFeeId = fee.Id,
                Period = period,
                BaseAmount = fee.BalanceAmount,
                DailyRate = dailyRate,
                DaysOverdue = daysOverdue,
                CalculatedAmount = interest,
                IsCapitalized = true
            });
        }

        await foreach (var ed in _context.ExtraordinaryFeeDistributions
            .Where(ed => ed.TenantId == tenantId
                      && ed.BalanceAmount > 0
                      && ed.DueDate < now)
            .AsAsyncEnumerable())
        {
            var daysOverdue = Math.Max(0, (int)(now - ed.DueDate).TotalDays);
            if (daysOverdue <= 0) continue;

            var interest = Math.Round(ed.BalanceAmount * dailyRate * daysOverdue, 2);
            if (interest <= 0m) continue;

            result.Add(new LateInterest
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                ExtraordinaryFeeDistributionId = ed.Id,
                Period = period,
                BaseAmount = ed.BalanceAmount,
                DailyRate = dailyRate,
                DaysOverdue = daysOverdue,
                CalculatedAmount = interest,
                IsCapitalized = true
            });
        }

        await foreach (var charge in _context.IndividualCharges
            .Where(ic => ic.TenantId == tenantId
                      && ic.BalanceAmount > 0
                      && !ic.IsDisputed)
            .AsAsyncEnumerable())
        {
            var daysOverdue = Math.Max(0, (int)(now - charge.ChargeDate).TotalDays);
            if (daysOverdue <= 0) continue;

            var interest = Math.Round(charge.BalanceAmount * dailyRate * daysOverdue, 2);
            if (interest <= 0m) continue;

            result.Add(new LateInterest
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                IndividualChargeId = charge.Id,
                Period = period,
                BaseAmount = charge.BalanceAmount,
                DailyRate = dailyRate,
                DaysOverdue = daysOverdue,
                CalculatedAmount = interest,
                IsCapitalized = true
            });
        }

        if (result.Count > 0)
        {
            _context.LateInterests.AddRange(result);
            await _context.SaveChangesAsync();

            foreach (var li in result)
            {
                try
                {
                    await _accounting.RecordLateInterestAsync(
                        tenantId, li.Id, li.CalculatedAmount,
                        $"Capitalización de intereses mora período {period}", userId);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error al registrar asiento contable de interés mora masivo {InterestId} para tenant {TenantId}", li.Id, tenantId);
                }
            }
        }

        _cache.Remove($"mora_map_{tenantId}");
        return result;
    }

    public async Task<List<LateInterestSummaryDto>> GetCapitalizedInterestsAsync(
        string tenantId, Guid? unitId = null)
    {
        var query = _context.LateInterests
            .Where(li => li.TenantId == tenantId && li.IsCapitalized);

        if (unitId.HasValue)
        {
            var unitFeeIds = await _context.UnitFees
                .Where(uf => uf.TenantId == tenantId && uf.UnitId == unitId.Value)
                .Select(uf => uf.Id)
                .ToListAsync();

            var extraordinaryIds = await _context.ExtraordinaryFeeDistributions
                .Where(ed => ed.TenantId == tenantId && ed.UnitId == unitId.Value)
                .Select(ed => ed.Id)
                .ToListAsync();

            var chargeIds = await _context.IndividualCharges
                .Where(ic => ic.TenantId == tenantId && ic.UnitId == unitId.Value)
                .Select(ic => ic.Id)
                .ToListAsync();

            query = query.Where(li =>
                (li.UnitFeeId.HasValue && unitFeeIds.Contains(li.UnitFeeId.Value)) ||
                (li.ExtraordinaryFeeDistributionId.HasValue && extraordinaryIds.Contains(li.ExtraordinaryFeeDistributionId.Value)) ||
                (li.IndividualChargeId.HasValue && chargeIds.Contains(li.IndividualChargeId.Value)));
        }

        var interests = await query
            .OrderByDescending(li => li.Period)
            .Select(li => new LateInterestSummaryDto
            {
                Id = li.Id,
                SourceType = li.UnitFeeId != null ? "UnitFee" :
                             li.ExtraordinaryFeeDistributionId != null ? "ExtraordinaryFee" : "IndividualCharge",
                SourceId = li.UnitFeeId ?? li.ExtraordinaryFeeDistributionId ?? li.IndividualChargeId,
                Period = li.Period,
                BaseAmount = li.BaseAmount,
                DailyRate = li.DailyRate,
                DaysOverdue = li.DaysOverdue,
                CalculatedAmount = li.CalculatedAmount,
                IsCapitalized = li.IsCapitalized
            })
            .ToListAsync();

        return interests;
    }
}
