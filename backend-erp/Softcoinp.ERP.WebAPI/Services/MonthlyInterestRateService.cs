using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Softcoinp.ERP.Domain.Entities;
using Softcoinp.ERP.Infrastructure.Persistence;

namespace Softcoinp.ERP.WebAPI.Services;

public class MonthlyInterestRateService
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<MonthlyInterestRateService> _logger;

    public MonthlyInterestRateService(
        ApplicationDbContext context,
        ILogger<MonthlyInterestRateService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<List<MonthlyInterestRateDto>> GetRatesAsync(string tenantId)
    {
        var rates = await _context.MonthlyInterestRates
            .Where(r => r.TenantId == tenantId)
            .OrderByDescending(r => r.Year)
            .ThenByDescending(r => r.Month)
            .ToListAsync();

        return rates.Select(r => MapToDto(r)).ToList();
    }

    public async Task<MonthlyInterestRateDto?> GetRateByIdAsync(string tenantId, Guid rateId)
    {
        var rate = await _context.MonthlyInterestRates
            .FirstOrDefaultAsync(r => r.Id == rateId && r.TenantId == tenantId);

        return rate == null ? null : MapToDto(rate);
    }

    public async Task<MonthlyInterestRateDto?> GetRateForPeriodAsync(string tenantId, int year, int month)
    {
        var rate = await _context.MonthlyInterestRates
            .FirstOrDefaultAsync(r => r.TenantId == tenantId && r.Year == year && r.Month == month);

        return rate == null ? null : MapToDto(rate);
    }

    public async Task<RateRegistrationResult> RegisterRateAsync(
        string tenantId, int year, int month, decimal certifiedRate, decimal appliedRate, string userId)
    {
        var result = new RateRegistrationResult();

        if (year < 2000 || year > 2100)
        {
            result.AddError("El año debe estar entre 2000 y 2100.");
            return result;
        }

        if (month < 1 || month > 12)
        {
            result.AddError("El mes debe estar entre 1 y 12.");
            return result;
        }

        if (certifiedRate <= 0)
        {
            result.AddError("La tasa certificada debe ser mayor a cero.");
            return result;
        }

        if (appliedRate <= 0)
        {
            result.AddError("La tasa aplicada debe ser mayor a cero.");
            return result;
        }

        var maxAllowedRate = Math.Round(certifiedRate * 1.5m, 4, MidpointRounding.AwayFromZero);

        if (appliedRate > maxAllowedRate)
        {
            result.AddError(
                $"La tasa aplicada ({appliedRate:F4}%) supera el límite legal de 1.5 veces la tasa certificada. " +
                $"El valor máximo permitido es {maxAllowedRate:F4}%.");
            return result;
        }

        var existingRate = await _context.MonthlyInterestRates
            .FirstOrDefaultAsync(r => r.TenantId == tenantId && r.Year == year && r.Month == month);

        if (existingRate != null)
        {
            existingRate.CertifiedRate = certifiedRate;
            existingRate.AppliedRate = appliedRate;
            existingRate.RegisteredAt = DateTime.UtcNow;
            existingRate.RegisteredByUserId = userId;

            await _context.SaveChangesAsync();

            result.Rate = MapToDto(existingRate);
            result.IsUpdate = true;
            return result;
        }

        var rate = new MonthlyInterestRate
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            Year = year,
            Month = month,
            CertifiedRate = certifiedRate,
            AppliedRate = appliedRate,
            RegisteredAt = DateTime.UtcNow,
            RegisteredByUserId = userId
        };

        _context.MonthlyInterestRates.Add(rate);
        await _context.SaveChangesAsync();

        result.Rate = MapToDto(rate);
        result.IsUpdate = false;
        return result;
    }

    public async Task<bool> DeleteRateAsync(string tenantId, Guid rateId)
    {
        var rate = await _context.MonthlyInterestRates
            .FirstOrDefaultAsync(r => r.Id == rateId && r.TenantId == tenantId);

        if (rate == null) return false;

        var hasInterests = await _context.AccruedInterests
            .AnyAsync(ai => ai.TenantId == tenantId && ai.MonthlyInterestRateId == rateId);

        if (hasInterests) return false;

        _context.MonthlyInterestRates.Remove(rate);
        await _context.SaveChangesAsync();
        return true;
    }

    private static MonthlyInterestRateDto MapToDto(MonthlyInterestRate rate)
    {
        return new MonthlyInterestRateDto
        {
            Id = rate.Id,
            Year = rate.Year,
            Month = rate.Month,
            CertifiedRate = rate.CertifiedRate,
            AppliedRate = rate.AppliedRate,
            MaxAllowedRate = Math.Round(rate.CertifiedRate * 1.5m, 4, MidpointRounding.AwayFromZero),
            RegisteredAt = rate.RegisteredAt,
            RegisteredByUserId = rate.RegisteredByUserId
        };
    }
}

public class MonthlyInterestRateDto
{
    public Guid Id { get; set; }
    public int Year { get; set; }
    public int Month { get; set; }
    public decimal CertifiedRate { get; set; }
    public decimal AppliedRate { get; set; }
    public decimal MaxAllowedRate { get; set; }
    public DateTime RegisteredAt { get; set; }
    public string RegisteredByUserId { get; set; } = string.Empty;
}

public class RateRegistrationResult
{
    public MonthlyInterestRateDto? Rate { get; set; }
    public bool IsUpdate { get; set; }
    public bool HasErrors => Errors.Count > 0;
    public List<string> Errors { get; set; } = new();

    public void AddError(string message)
    {
        Errors.Add(message);
    }
}
