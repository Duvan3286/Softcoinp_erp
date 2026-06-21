using System;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Softcoinp.ERP.Domain.Entities;
using Softcoinp.ERP.Domain.Enums;
using Softcoinp.ERP.Infrastructure.Persistence;

namespace Softcoinp.ERP.WebAPI.Services;

public class IndicatorCacheService
{
    private readonly ApplicationDbContext _context;

    public IndicatorCacheService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<T?> GetAsync<T>(string tenantId, string cacheKey) where T : class
    {
        var entry = await _context.IndicatorCaches
            .FirstOrDefaultAsync(c => c.TenantId == tenantId && c.CacheKey == cacheKey);

        if (entry == null || entry.Status != CacheStatus.Valid) return null;

        if (entry.NextInvalidationAt.HasValue && entry.NextInvalidationAt.Value <= DateTime.UtcNow)
        {
            entry.Status = CacheStatus.Invalid;
            await _context.SaveChangesAsync();
            return null;
        }

        entry.HitCount++;
        await _context.SaveChangesAsync();

        return JsonSerializer.Deserialize<T>(entry.CacheValue);
    }

    public async Task SetAsync<T>(string tenantId, string cacheKey, T value, int expirationMinutes = 5) where T : class
    {
        var entry = await _context.IndicatorCaches
            .FirstOrDefaultAsync(c => c.TenantId == tenantId && c.CacheKey == cacheKey);

        var json = JsonSerializer.Serialize(value);

        if (entry == null)
        {
            entry = new IndicatorCache
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                CacheKey = cacheKey,
                CacheValue = json,
                Status = CacheStatus.Valid,
                LastUpdatedAt = DateTime.UtcNow,
                NextInvalidationAt = DateTime.UtcNow.AddMinutes(expirationMinutes),
                InvalidationCount = 0
            };
            _context.IndicatorCaches.Add(entry);
        }
        else
        {
            entry.CacheValue = json;
            entry.Status = CacheStatus.Valid;
            entry.LastUpdatedAt = DateTime.UtcNow;
            entry.NextInvalidationAt = DateTime.UtcNow.AddMinutes(expirationMinutes);
        }

        await _context.SaveChangesAsync();
    }

    public async Task InvalidateAsync(string tenantId, string cacheKeyPrefix)
    {
        var entries = await _context.IndicatorCaches
            .Where(c => c.TenantId == tenantId && c.CacheKey.StartsWith(cacheKeyPrefix))
            .ToListAsync();

        foreach (var entry in entries)
        {
            entry.Status = CacheStatus.Invalid;
            entry.InvalidationCount++;
        }

        if (entries.Count > 0)
            await _context.SaveChangesAsync();
    }
}
