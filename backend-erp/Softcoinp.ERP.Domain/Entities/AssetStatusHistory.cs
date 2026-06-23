using System;
using Softcoinp.ERP.Domain.Enums;

namespace Softcoinp.ERP.Domain.Entities;

public class AssetStatusHistory
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string TenantId { get; set; } = string.Empty;
    public Guid AssetId { get; set; }
    public CommonAsset? Asset { get; set; }
    public AssetStatus PreviousStatus { get; set; }
    public AssetStatus NewStatus { get; set; }
    public string Reason { get; set; } = string.Empty;
    public string ChangedByUserId { get; set; } = string.Empty;
    public string ChangedByUserName { get; set; } = string.Empty;
    public DateTime ChangedAt { get; set; } = DateTime.UtcNow;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
