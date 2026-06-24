using System;
using Softcoinp.ERP.Domain.Common;
using Softcoinp.ERP.Domain.Enums;

namespace Softcoinp.ERP.Domain.Entities;

public class BulletinBoardPost : BaseEntity
{
    public string TenantId { get; set; } = string.Empty;

    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;

    public DateTime PublishedAt { get; set; } = DateTime.UtcNow;
    public DateTime? ExpiresAt { get; set; }

    public bool IsPinned { get; set; }
    public BulletinCategory Category { get; set; } = BulletinCategory.Administrative;

    public string CreatedByUserId { get; set; } = string.Empty;
    public string? UpdatedByUserId { get; set; }
}
