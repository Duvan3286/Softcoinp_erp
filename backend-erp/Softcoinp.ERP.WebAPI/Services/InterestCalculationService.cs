using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Softcoinp.ERP.Domain.Entities;
using Softcoinp.ERP.Domain.Enums;
using Softcoinp.ERP.Infrastructure.Persistence;

namespace Softcoinp.ERP.WebAPI.Services;

public class InterestCalculationService
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<InterestCalculationService> _logger;

    public InterestCalculationService(
        ApplicationDbContext context,
        ILogger<InterestCalculationService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public static decimal CalculateDailyRate(decimal monthlyRate)
    {
        var rateDecimal = monthlyRate / 100m;
        var result = (decimal)Math.Pow((double)(1m + rateDecimal), 1.0 / 30.0) - 1m;
        return Math.Round(result, 10, MidpointRounding.AwayFromZero);
    }

    public static decimal CalculateInterestAmount(decimal baseAmount, decimal dailyRate, int days)
    {
        var factor = (decimal)Math.Pow((double)(1m + dailyRate), days) - 1m;
        return Math.Round(baseAmount * factor, 2, MidpointRounding.AwayFromZero);
    }

    public async Task<InterestCalculationResult> CalculateAndSaveInterestsAsync(
        string tenantId, Guid unitId, string userId)
    {
        var result = new InterestCalculationResult();

        var config = await _context.LateInterestConfigurations
            .FirstOrDefaultAsync(c => c.TenantId == tenantId);

        if (config == null)
        {
            result.AddAlert("No hay configuración de intereses de mora para el conjunto.");
            return result;
        }

        var exception = await _context.UnitInterestExceptions
            .FirstOrDefaultAsync(e => e.TenantId == tenantId && e.UnitId == unitId);

        var interestStartDays = exception?.InterestStartDays ?? config.InterestStartDays;

        var now = DateTime.UtcNow;

        var overdueFees = await _context.UnitFees
            .Where(f => f.TenantId == tenantId
                     && f.UnitId == unitId
                     && f.BalanceAmount > 0
                     && f.DueDate < now)
            .OrderBy(f => f.DueDate)
            .ToListAsync();

        var overdueExtraordinary = await _context.ExtraordinaryFeeDistributions
            .Where(d => d.TenantId == tenantId
                     && d.UnitId == unitId
                     && d.BalanceAmount > 0
                     && d.DueDate < now)
            .OrderBy(d => d.DueDate)
            .ToListAsync();

        var overdueCharges = await _context.IndividualCharges
            .Where(c => c.TenantId == tenantId
                     && c.UnitId == unitId
                     && c.BalanceAmount > 0
                     && !c.IsDisputed
                     && c.ChargeDate < now)
            .OrderBy(c => c.ChargeDate)
            .ToListAsync();

        if (overdueFees.Count == 0 && overdueExtraordinary.Count == 0 && overdueCharges.Count == 0)
        {
            result.AddAlert("La unidad no tiene saldos vencidos.");
            return result;
        }

        var ratesByPeriod = await _context.MonthlyInterestRates
            .Where(r => r.TenantId == tenantId)
            .ToListAsync();

        var ratesLookup = ratesByPeriod
            .GroupBy(r => $"{r.Year}-{r.Month:D2}")
            .ToDictionary(g => g.Key, g => g.First());

        var missingPeriods = new HashSet<string>();

        foreach (var fee in overdueFees)
        {
            var interestStartDate = fee.DueDate.AddDays(interestStartDays + 1);
            if (interestStartDate > now) continue;

            var periods = GetPeriodsBetween(interestStartDate, now);
            foreach (var period in periods)
            {
                if (!ratesLookup.TryGetValue(period.Key, out var rate))
                {
                    missingPeriods.Add(period.Key);
                    continue;
                }

                var dailyRate = CalculateDailyRate(rate.AppliedRate);

                var existingInterest = await _context.AccruedInterests
                    .FirstOrDefaultAsync(ai => ai.TenantId == tenantId
                                            && ai.UnitId == unitId
                                            && ai.UnitFeeId == fee.Id
                                            && ai.Period == period.Key);

                if (existingInterest != null)
                {
                    if (existingInterest.Status == AccruedInterestStatus.Paid) continue;

                    existingInterest.BaseAmount = fee.BalanceAmount;
                    existingInterest.DailyRate = dailyRate;
                    existingInterest.DaysInPeriod = period.Days;
                    existingInterest.InterestStartDate = period.Start;
                    existingInterest.InterestEndDate = period.End;
                    existingInterest.CalculatedAmount = CalculateInterestAmount(fee.BalanceAmount, dailyRate, period.Days);
                    existingInterest.BalanceAmount = existingInterest.CalculatedAmount;
                    existingInterest.UpdatedAt = DateTime.UtcNow;
                    result.UpdatedCount++;
                    continue;
                }

                var interest = new AccruedInterest
                {
                    Id = Guid.NewGuid(),
                    TenantId = tenantId,
                    UnitId = unitId,
                    UnitFeeId = fee.Id,
                    Period = period.Key,
                    DailyRate = dailyRate,
                    DaysInPeriod = period.Days,
                    BaseAmount = fee.BalanceAmount,
                    CalculatedAmount = CalculateInterestAmount(fee.BalanceAmount, dailyRate, period.Days),
                    BalanceAmount = CalculateInterestAmount(fee.BalanceAmount, dailyRate, period.Days),
                    Status = AccruedInterestStatus.Pending,
                    InterestStartDate = period.Start,
                    InterestEndDate = period.End,
                    MonthlyInterestRateId = rate.Id,
                    CreatedAt = DateTime.UtcNow
                };

                _context.AccruedInterests.Add(interest);
                result.CreatedCount++;
            }
        }

        foreach (var dist in overdueExtraordinary)
        {
            var interestStartDate = dist.DueDate.AddDays(interestStartDays + 1);
            if (interestStartDate > now) continue;

            var periods = GetPeriodsBetween(interestStartDate, now);
            foreach (var period in periods)
            {
                if (!ratesLookup.TryGetValue(period.Key, out var rate))
                {
                    missingPeriods.Add(period.Key);
                    continue;
                }

                var dailyRate = CalculateDailyRate(rate.AppliedRate);

                var existingInterest = await _context.AccruedInterests
                    .FirstOrDefaultAsync(ai => ai.TenantId == tenantId
                                            && ai.UnitId == unitId
                                            && ai.ExtraordinaryFeeDistributionId == dist.Id
                                            && ai.Period == period.Key);

                if (existingInterest != null)
                {
                    if (existingInterest.Status == AccruedInterestStatus.Paid) continue;

                    existingInterest.BaseAmount = dist.BalanceAmount;
                    existingInterest.DailyRate = dailyRate;
                    existingInterest.DaysInPeriod = period.Days;
                    existingInterest.InterestStartDate = period.Start;
                    existingInterest.InterestEndDate = period.End;
                    existingInterest.CalculatedAmount = CalculateInterestAmount(dist.BalanceAmount, dailyRate, period.Days);
                    existingInterest.BalanceAmount = existingInterest.CalculatedAmount;
                    existingInterest.UpdatedAt = DateTime.UtcNow;
                    result.UpdatedCount++;
                    continue;
                }

                var interest = new AccruedInterest
                {
                    Id = Guid.NewGuid(),
                    TenantId = tenantId,
                    UnitId = unitId,
                    ExtraordinaryFeeDistributionId = dist.Id,
                    Period = period.Key,
                    DailyRate = dailyRate,
                    DaysInPeriod = period.Days,
                    BaseAmount = dist.BalanceAmount,
                    CalculatedAmount = CalculateInterestAmount(dist.BalanceAmount, dailyRate, period.Days),
                    BalanceAmount = CalculateInterestAmount(dist.BalanceAmount, dailyRate, period.Days),
                    Status = AccruedInterestStatus.Pending,
                    InterestStartDate = period.Start,
                    InterestEndDate = period.End,
                    MonthlyInterestRateId = rate.Id,
                    CreatedAt = DateTime.UtcNow
                };

                _context.AccruedInterests.Add(interest);
                result.CreatedCount++;
            }
        }

        foreach (var charge in overdueCharges)
        {
            var interestStartDate = charge.ChargeDate.AddDays(interestStartDays + 1);
            if (interestStartDate > now) continue;

            var periods = GetPeriodsBetween(interestStartDate, now);
            foreach (var period in periods)
            {
                if (!ratesLookup.TryGetValue(period.Key, out var rate))
                {
                    missingPeriods.Add(period.Key);
                    continue;
                }

                var dailyRate = CalculateDailyRate(rate.AppliedRate);

                var existingInterest = await _context.AccruedInterests
                    .FirstOrDefaultAsync(ai => ai.TenantId == tenantId
                                            && ai.UnitId == unitId
                                            && ai.IndividualChargeId == charge.Id
                                            && ai.Period == period.Key);

                if (existingInterest != null)
                {
                    if (existingInterest.Status == AccruedInterestStatus.Paid) continue;

                    existingInterest.BaseAmount = charge.BalanceAmount;
                    existingInterest.DailyRate = dailyRate;
                    existingInterest.DaysInPeriod = period.Days;
                    existingInterest.InterestStartDate = period.Start;
                    existingInterest.InterestEndDate = period.End;
                    existingInterest.CalculatedAmount = CalculateInterestAmount(charge.BalanceAmount, dailyRate, period.Days);
                    existingInterest.BalanceAmount = existingInterest.CalculatedAmount;
                    existingInterest.UpdatedAt = DateTime.UtcNow;
                    result.UpdatedCount++;
                    continue;
                }

                var interest = new AccruedInterest
                {
                    Id = Guid.NewGuid(),
                    TenantId = tenantId,
                    UnitId = unitId,
                    IndividualChargeId = charge.Id,
                    Period = period.Key,
                    DailyRate = dailyRate,
                    DaysInPeriod = period.Days,
                    BaseAmount = charge.BalanceAmount,
                    CalculatedAmount = CalculateInterestAmount(charge.BalanceAmount, dailyRate, period.Days),
                    BalanceAmount = CalculateInterestAmount(charge.BalanceAmount, dailyRate, period.Days),
                    Status = AccruedInterestStatus.Pending,
                    InterestStartDate = period.Start,
                    InterestEndDate = period.End,
                    MonthlyInterestRateId = rate.Id,
                    CreatedAt = DateTime.UtcNow
                };

                _context.AccruedInterests.Add(interest);
                result.CreatedCount++;
            }
        }

        foreach (var missing in missingPeriods)
        {
            result.AddAlert($"No hay tasa registrada para el período {missing}. Los intereses de ese período no se calcularon.");
        }

        result.HasMissingRates = missingPeriods.Count > 0;

        if (result.CreatedCount > 0 || result.UpdatedCount > 0)
        {
            await _context.SaveChangesAsync();
        }

        return result;
    }

    public async Task<InterestCheckResult> CheckMissingRatesAsync(string tenantId)
    {
        var config = await _context.LateInterestConfigurations
            .FirstOrDefaultAsync(c => c.TenantId == tenantId);

        var result = new InterestCheckResult
        {
            AlertEnabled = config?.AlertOnMissingMonthlyRate ?? true
        };

        var now = DateTime.UtcNow;
        var currentPeriod = $"{now.Year}-{now.Month:D2}";

        var existingRate = await _context.MonthlyInterestRates
            .FirstOrDefaultAsync(r => r.TenantId == tenantId && r.Year == now.Year && r.Month == now.Month);

        result.CurrentPeriod = currentPeriod;
        result.HasRateForCurrentPeriod = existingRate != null;

        if (existingRate == null)
        {
            result.Message = $"No se ha registrado la tasa de interés para el período {currentPeriod}.";
        }

        return result;
    }

    private static List<(string Key, DateTime Start, DateTime End, int Days)> GetPeriodsBetween(
        DateTime startDate, DateTime endDate)
    {
        var periods = new List<(string Key, DateTime Start, DateTime End, int Days)>();

        var current = new DateTime(startDate.Year, startDate.Month, 1);
        var final = new DateTime(endDate.Year, endDate.Month, 1);

        while (current <= final)
        {
            var periodStart = current;
            var periodEnd = current.AddMonths(1).AddDays(-1);
            if (periodEnd > endDate) periodEnd = endDate;
            if (periodStart < startDate) periodStart = startDate;

            var days = (periodEnd - periodStart).Days + 1;
            if (days <= 0) continue;

            var key = $"{current.Year}-{current.Month:D2}";
            periods.Add((key, periodStart, periodEnd, days));

            current = current.AddMonths(1);
        }

        return periods;
    }
}

public class InterestCalculationResult
{
    public int CreatedCount { get; set; }
    public int UpdatedCount { get; set; }
    public bool HasMissingRates { get; set; }
    public List<string> Alerts { get; set; } = new();

    public void AddAlert(string message)
    {
        Alerts.Add(message);
    }
}

public class InterestCheckResult
{
    public string CurrentPeriod { get; set; } = string.Empty;
    public bool HasRateForCurrentPeriod { get; set; }
    public bool AlertEnabled { get; set; }
    public string Message { get; set; } = string.Empty;
}
