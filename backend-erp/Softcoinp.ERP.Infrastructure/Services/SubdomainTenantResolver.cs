using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
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
    private readonly Microsoft.Extensions.Logging.ILogger<SubdomainTenantResolver> _logger;
    private const string CacheKeyPrefix = "Tenant_";
    private Tenant? _currentTenantOverride;

    public SubdomainTenantResolver(IHttpContextAccessor httpContextAccessor, MasterDbContext masterDbContext, IMemoryCache cache, Microsoft.Extensions.Logging.ILogger<SubdomainTenantResolver> logger)
    {
        _httpContextAccessor = httpContextAccessor;
        _masterDbContext = masterDbContext;
        _cache = cache;
        _logger = logger;
    }

    public void SetCurrentTenant(Tenant tenant)
    {
        _logger.LogInformation("Tenant override set manually to: {Subdomain}", tenant.Subdomain);
        _currentTenantOverride = tenant;
    }

    public async Task<string?> GetConnectionStringAsync()
    {
        var tenant = await GetCurrentTenantAsync();
        return tenant?.ConnectionString;
    }

    public async Task<Tenant?> GetCurrentTenantAsync()
    {
        // 0. Manual Override
        if (_currentTenantOverride != null) return _currentTenantOverride;

        // 1. Try to get tenant from Header (useful for API calls from different origins)
        if (_httpContextAccessor.HttpContext?.Request.Headers.TryGetValue("X-Tenant-Id", out var tenantIdHeader) == true)
        {
            var subdomainFromHeader = tenantIdHeader.ToString().ToLower();
            _logger.LogInformation("Tenant detected from header X-Tenant-Id: {Subdomain}", subdomainFromHeader);
            if (!string.IsNullOrEmpty(subdomainFromHeader))
            {
                return await GetTenantFromCacheOrDb(subdomainFromHeader);
            }
        }

        // 2. Fallback to Subdomain from Host
        var host = _httpContextAccessor.HttpContext?.Request.Host.Host;
        _logger.LogInformation("Resolving tenant for Host: {Host}", host);
        if (string.IsNullOrEmpty(host)) return null;

        var segments = host.Split('.');
        string subdomain;

        if (segments.Length >= 2)
        {
            // test.softcoinp.com -> test
            // test.localhost -> test
            subdomain = segments[0].ToLower();
            _logger.LogInformation("Subdomain extracted from host: {Subdomain}", subdomain);
        }
        else
        {
            _logger.LogWarning("No subdomain found in host: {Host}", host);
            // localhost or direct IP -> no tenant detected by subdomain
            return null;
        }

        return await GetTenantFromCacheOrDb(subdomain);
    }

    private async Task<Tenant?> GetTenantFromCacheOrDb(string subdomain)
    {
        var cacheKey = $"{CacheKeyPrefix}{subdomain}";

        if (!_cache.TryGetValue(cacheKey, out Tenant? tenant))
        {
            _logger.LogInformation("Tenant {Subdomain} not in cache, fetching from Master DB", subdomain);
            tenant = await _masterDbContext.Tenants
                .FirstOrDefaultAsync(t => t.Subdomain == subdomain && t.IsActive);

            if (tenant != null)
            {
                var cacheOptions = new MemoryCacheEntryOptions()
                    .SetSlidingExpiration(TimeSpan.FromMinutes(15))
                    .SetAbsoluteExpiration(TimeSpan.FromHours(1));
                
                _cache.Set(cacheKey, tenant, cacheOptions);
                _logger.LogInformation("Tenant {Subdomain} found in Master DB and cached", subdomain);
            }
            else
            {
                _logger.LogWarning("Tenant {Subdomain} NOT found in Master DB", subdomain);
            }
        }

        return tenant;
    }
}
