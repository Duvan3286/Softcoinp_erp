using System;
using Softcoinp.ERP.Domain.Enums;

namespace Softcoinp.ERP.Domain.Entities;

public class IndicatorCache
{
    public Guid Id { get; set; }
    public string TenantId { get; set; } = string.Empty;

    public string CacheKey { get; set; } = string.Empty;
    public string CacheValue { get; set; } = string.Empty;
    public CacheStatus Status { get; set; } = CacheStatus.Invalid;

    public DateTime LastUpdatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? NextInvalidationAt { get; set; }

    public int HitCount { get; set; }
    public int InvalidationCount { get; set; }
}
