using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Softcoinp.ERP.Domain.Interfaces;
using Softcoinp.ERP.Infrastructure.Persistence;

namespace Softcoinp.ERP.Infrastructure.Services;

/// <summary>
/// Implementation of ITenantResolver that uses the request subdomain to identify the tenant.
/// </summary>
public class SubdomainTenantResolver : ITenantResolver
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly MasterDbContext _masterDbContext;
    private readonly IMemoryCache _cache;
    private const string CacheKeyPrefix = "Tenant_";

    public SubdomainTenantResolver(IHttpContextAccessor httpContextAccessor, MasterDbContext masterDbContext, IMemoryCache cache)
    {
        _httpContextAccessor = httpContextAccessor;
        _masterDbContext = masterDbContext;
        _cache = cache;
    }

    public async Task<string?> GetConnectionStringAsync()
    {
        var tenant = await GetCurrentTenantAsync();
        return tenant?.ConnectionString;
    }

    public async Task<Tenant?> GetCurrentTenantAsync()
    {
        var host = _httpContextAccessor.HttpContext?.Request.Host.Host;
        if (string.IsNullOrEmpty(host)) return null;

        // Extract subdomain (e.g., client1.softcoinp.com -> client1)
        var segments = host.Split('.');
        if (segments.Length < 2) return null; // Or handle base domain logic

        var subdomain = segments[0].ToLower();
        var cacheKey = $"{CacheKeyPrefix}{subdomain}";

        if (!_cache.TryGetValue(cacheKey, out Tenant? tenant))
        {
            tenant = await _masterDbContext.Tenants
                .FirstOrDefaultAsync(t => t.Subdomain == subdomain && t.IsActive);

            if (tenant != null)
            {
                var cacheOptions = new MemoryCacheEntryOptions()
                    .SetSlidingExpiration(TimeSpan.FromMinutes(15))
                    .SetAbsoluteExpiration(TimeSpan.FromHours(1));
                
                _cache.Set(cacheKey, tenant, cacheOptions);
            }
        }

        return tenant;
    }
}
