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

public class LateInterestService
{
    private readonly ApplicationDbContext _context;

    public LateInterestService(ApplicationDbContext context)
    {
        _context = context;
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
        return Math.Round(monthlyRate / 30m / 100m, 8);
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
        string tenantId, Guid unitFeeId, string period)
    {
        var monthlyRate = await GetMonthlyRateAsync(tenantId);
        var dailyRate = GetDailyRate(monthlyRate);
        var result = new List<LateInterest>();

        if (dailyRate <= 0m) return result;

        var fee = await _context.UnitFees
            .FirstOrDefaultAsync(uf => uf.Id == unitFeeId && uf.TenantId == tenantId);

        if (fee == null)
        {
            throw new KeyNotFoundException("No se encontró la cuota especificada.");
        }

        if (fee.BalanceAmount <= 0m)
        {
            throw new InvalidOperationException("La cuota no tiene saldo pendiente.");
        }

        var daysOverdue = Math.Max(0, (int)(DateTime.UtcNow - fee.DueDate).TotalDays);
        if (daysOverdue <= 0) return result;

        var interest = Math.Round(fee.BalanceAmount * dailyRate * daysOverdue, 2);

        var lateInterest = new LateInterest
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            UnitFeeId = unitFeeId,
            Period = period,
            BaseAmount = fee.BalanceAmount,
            DailyRate = dailyRate,
            DaysOverdue = daysOverdue,
            CalculatedAmount = interest,
            IsCapitalized = true
        };

        _context.LateInterests.Add(lateInterest);
        await _context.SaveChangesAsync();

        result.Add(lateInterest);
        return result;
    }

    public async Task<List<LateInterest>> CapitalizeAllOverdueInterestAsync(
        string tenantId, string period)
    {
        var monthlyRate = await GetMonthlyRateAsync(tenantId);
        var dailyRate = GetDailyRate(monthlyRate);
        var result = new List<LateInterest>();

        if (dailyRate <= 0m) return result;

        var now = DateTime.UtcNow;

        var overdueFees = await _context.UnitFees
            .Where(uf => uf.TenantId == tenantId
                      && uf.BalanceAmount > 0
                      && uf.DueDate < now)
            .ToListAsync();

        foreach (var fee in overdueFees)
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

        if (result.Count > 0)
        {
            _context.LateInterests.AddRange(result);
            await _context.SaveChangesAsync();
        }

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

            query = query.Where(li => unitFeeIds.Contains(li.UnitFeeId));
        }

        var interests = await query
            .OrderByDescending(li => li.Period)
            .Select(li => new LateInterestSummaryDto
            {
                Id = li.Id,
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
