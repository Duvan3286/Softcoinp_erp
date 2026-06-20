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

public class AccountingPeriodService
{
    private readonly ApplicationDbContext _context;

    public AccountingPeriodService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<AccountingPeriodDto>> GetPeriodsAsync(string tenantId)
    {
        return await _context.AccountingPeriods
            .Where(p => p.TenantId == tenantId)
            .OrderByDescending(p => p.FiscalYear)
            .ThenByDescending(p => p.Month)
            .Select(p => new AccountingPeriodDto
            {
                Id = p.Id,
                FiscalYear = p.FiscalYear,
                Month = p.Month,
                PeriodLabel = p.PeriodLabel,
                Status = p.Status.ToString(),
                OpenedAt = p.OpenedAt,
                ClosedAt = p.ClosedAt,
                ClosedByUserId = p.ClosedByUserId,
                LastEntryNumber = p.LastEntryNumber
            })
            .ToListAsync();
    }

    public async Task<AccountingPeriodDto?> GetCurrentPeriodAsync(string tenantId)
    {
        var now = DateTime.UtcNow;
        var period = await _context.AccountingPeriods
            .Where(p => p.TenantId == tenantId && p.FiscalYear == now.Year && p.Month == now.Month)
            .FirstOrDefaultAsync();

        if (period == null) return null;

        return new AccountingPeriodDto
        {
            Id = period.Id,
            FiscalYear = period.FiscalYear,
            Month = period.Month,
            PeriodLabel = period.PeriodLabel,
            Status = period.Status.ToString(),
            OpenedAt = period.OpenedAt,
            ClosedAt = period.ClosedAt,
            ClosedByUserId = period.ClosedByUserId,
            LastEntryNumber = period.LastEntryNumber
        };
    }

    public async Task<AccountingPeriodDto> OpenPeriodAsync(string tenantId, CreateAccountingPeriodDto dto, string userId)
    {
        var existing = await _context.AccountingPeriods
            .Where(p => p.TenantId == tenantId && p.FiscalYear == dto.FiscalYear && p.Month == dto.Month)
            .FirstOrDefaultAsync();

        if (existing != null)
        {
            throw new InvalidOperationException($"El período {dto.PeriodLabel} ({dto.FiscalYear}-{dto.Month:D2}) ya existe.");
        }

        var period = new AccountingPeriod
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            FiscalYear = dto.FiscalYear,
            Month = dto.Month,
            PeriodLabel = dto.PeriodLabel,
            Status = AccountingPeriodStatus.Open,
            OpenedAt = DateTime.UtcNow
        };

        _context.AccountingPeriods.Add(period);
        await _context.SaveChangesAsync();

        return new AccountingPeriodDto
        {
            Id = period.Id,
            FiscalYear = period.FiscalYear,
            Month = period.Month,
            PeriodLabel = period.PeriodLabel,
            Status = period.Status.ToString(),
            OpenedAt = period.OpenedAt,
            LastEntryNumber = period.LastEntryNumber
        };
    }

    public async Task<AccountingPeriodDto> ClosePeriodAsync(string tenantId, Guid periodId, string userId)
    {
        var period = await _context.AccountingPeriods
            .Where(p => p.Id == periodId && p.TenantId == tenantId)
            .FirstOrDefaultAsync();

        if (period == null)
        {
            throw new KeyNotFoundException("Período contable no encontrado.");
        }

        if (period.Status == AccountingPeriodStatus.Closed)
        {
            throw new InvalidOperationException("El período ya se encuentra cerrado.");
        }

        period.Status = AccountingPeriodStatus.Closed;
        period.ClosedAt = DateTime.UtcNow;
        period.ClosedByUserId = userId;

        await _context.SaveChangesAsync();

        return new AccountingPeriodDto
        {
            Id = period.Id,
            FiscalYear = period.FiscalYear,
            Month = period.Month,
            PeriodLabel = period.PeriodLabel,
            Status = period.Status.ToString(),
            OpenedAt = period.OpenedAt,
            ClosedAt = period.ClosedAt,
            ClosedByUserId = period.ClosedByUserId,
            LastEntryNumber = period.LastEntryNumber
        };
    }

    public async Task<int> GetNextEntryNumberAsync(string tenantId, Guid? periodId)
    {
        AccountingPeriod? period = null;

        if (periodId.HasValue)
        {
            period = await _context.AccountingPeriods
                .Where(p => p.Id == periodId.Value && p.TenantId == tenantId)
                .FirstOrDefaultAsync();
        }

        if (period == null)
        {
            var now = DateTime.UtcNow;
            period = await _context.AccountingPeriods
                .Where(p => p.TenantId == tenantId && p.FiscalYear == now.Year && p.Month == now.Month)
                .FirstOrDefaultAsync();
        }

        if (period == null)
        {
            throw new InvalidOperationException("No hay un período contable abierto. Abra un período antes de crear asientos.");
        }

        period.LastEntryNumber++;
        await _context.SaveChangesAsync();

        return period.LastEntryNumber;
    }
}
