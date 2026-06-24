using System;
using Softcoinp.ERP.Domain.Enums;

namespace Softcoinp.ERP.Domain.Entities;

public class SpaceBlock
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string TenantId { get; set; } = string.Empty;

    public Guid SpaceId { get; set; }
    public ReservableSpace? Space { get; set; }

    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public string StartTime { get; set; } = string.Empty;
    public string EndTime { get; set; } = string.Empty;

    public SpaceBlockOrigin Origin { get; set; } = SpaceBlockOrigin.Administrative;
    public string? Reason { get; set; }

    public Guid? RelatedWorkOrderId { get; set; }
    public string? RelatedWorkOrderNumber { get; set; }

    public bool NotifyAffectedResidents { get; set; }
    public bool NotificationSent { get; set; }

    public string CreatedByUserId { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
