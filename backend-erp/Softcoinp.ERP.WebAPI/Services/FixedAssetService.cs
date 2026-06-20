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

public class FixedAssetService
{
    private readonly ApplicationDbContext _context;
    private readonly AccountingIntegrationService _accounting;
    private readonly ILogger<FixedAssetService> _logger;

    public FixedAssetService(ApplicationDbContext context, AccountingIntegrationService accounting, ILogger<FixedAssetService> logger)
    {
        _context = context;
        _accounting = accounting;
        _logger = logger;
    }

    public async Task<List<FixedAsset>> GetAssetsAsync(string tenantId, bool includeInactive = false)
    {
        var query = _context.FixedAssets.Where(fa => fa.TenantId == tenantId);
        if (!includeInactive)
        {
            query = query.Where(fa => fa.Status == FixedAssetStatus.Active);
        }
        return await query.OrderBy(fa => fa.Name).ToListAsync();
    }

    public async Task<FixedAsset?> GetAssetByIdAsync(string tenantId, Guid id)
    {
        return await _context.FixedAssets
            .FirstOrDefaultAsync(fa => fa.Id == id && fa.TenantId == tenantId);
    }

    public async Task<FixedAsset> CreateAssetAsync(string tenantId, FixedAsset asset, string userId)
    {
        asset.Id = Guid.NewGuid();
        asset.TenantId = tenantId;
        asset.AccumulatedDepreciation = 0m;
        asset.BookValue = asset.AcquisitionValue;
        asset.Status = FixedAssetStatus.Active;
        asset.CreatedAt = DateTime.UtcNow;

        _context.FixedAssets.Add(asset);
        await _context.SaveChangesAsync();
        return asset;
    }

    public async Task<FixedAsset> UpdateAssetAsync(string tenantId, FixedAsset asset, string userId)
    {
        var existing = await _context.FixedAssets
            .FirstOrDefaultAsync(fa => fa.Id == asset.Id && fa.TenantId == tenantId);
        if (existing == null)
        {
            throw new KeyNotFoundException("Activo fijo no encontrado.");
        }

        existing.Name = asset.Name;
        existing.Description = asset.Description;
        existing.SerialNumber = asset.SerialNumber;
        existing.Location = asset.Location;
        existing.AcquisitionValue = asset.AcquisitionValue;
        existing.AcquisitionDate = asset.AcquisitionDate;
        existing.UsefulLifeMonths = asset.UsefulLifeMonths;
        existing.ResidualValue = asset.ResidualValue;
        existing.AccountingAccountId = asset.AccountingAccountId;
        existing.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return existing;
    }

    public async Task<FixedAsset> DisposeAssetAsync(string tenantId, Guid id, DateTime disposalDate, decimal? disposalValue, string reason, string userId)
    {
        var asset = await _context.FixedAssets
            .FirstOrDefaultAsync(fa => fa.Id == id && fa.TenantId == tenantId);
        if (asset == null)
        {
            throw new KeyNotFoundException("Activo fijo no encontrado.");
        }

        asset.Status = FixedAssetStatus.Disposed;
        asset.DisposalDate = disposalDate;
        asset.DisposalValue = disposalValue;
        asset.DisposalReason = reason;
        asset.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return asset;
    }

    public async Task<int> CalculateMonthlyDepreciationAsync(string tenantId, int year, int month, string userId)
    {
        var periodLabel = $"{year:D4}-{month:D2}";
        var assets = await _context.FixedAssets
            .Where(fa => fa.TenantId == tenantId && fa.Status == FixedAssetStatus.Active
                      && fa.AcquisitionDate <= new DateTime(year, month, 1))
            .ToListAsync();

        var count = 0;
        foreach (var asset in assets)
        {
            var existing = await _context.MonthlyDepreciations
                .AnyAsync(md => md.TenantId == tenantId && md.FixedAssetId == asset.Id
                             && md.FiscalYear == year && md.Month == month);
            if (existing) continue;

            var monthlyDep = Math.Round((asset.AcquisitionValue - asset.ResidualValue) / asset.UsefulLifeMonths, 2);
            asset.AccumulatedDepreciation += monthlyDep;
            asset.BookValue = asset.AcquisitionValue - asset.AccumulatedDepreciation;
            if (asset.BookValue <= 0m)
            {
                asset.Status = FixedAssetStatus.FullyDepreciated;
            }

            var depreciation = new MonthlyDepreciation
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                FixedAssetId = asset.Id,
                FiscalYear = year,
                Month = month,
                PeriodLabel = periodLabel,
                DepreciationAmount = monthlyDep,
                AccumulatedAfter = asset.AccumulatedDepreciation,
                BookValueAfter = Math.Max(0, asset.BookValue),
                CreatedAt = DateTime.UtcNow
            };

            _context.MonthlyDepreciations.Add(depreciation);
            count++;
        }

        await _context.SaveChangesAsync();
        return count;
    }

    public async Task<List<MonthlyDepreciation>> GetDepreciationHistoryAsync(string tenantId, Guid assetId)
    {
        return await _context.MonthlyDepreciations
            .Where(md => md.TenantId == tenantId && md.FixedAssetId == assetId)
            .OrderBy(md => md.FiscalYear).ThenBy(md => md.Month)
            .ToListAsync();
    }
}
