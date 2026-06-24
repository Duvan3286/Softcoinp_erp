using System;
using Softcoinp.ERP.Domain.Enums;

namespace Softcoinp.ERP.Domain.Entities;

public class CommunicationPreference
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string TenantId { get; set; } = string.Empty;

    public Guid? OwnerId { get; set; }
    public Owner? Owner { get; set; }

    public Guid? TenantResidentId { get; set; }
    public TenantResident? TenantResident { get; set; }

    public bool AllowEmail { get; set; } = true;
    public bool AllowSms { get; set; } = true;
    public bool AllowPush { get; set; } = true;

    public bool CriticalNotificationsOverride { get; set; } = true;

    public string UnsubscribedEventTypes { get; set; } = string.Empty;

    public string? Notes { get; set; }

    public DateTime ChangedAt { get; set; } = DateTime.UtcNow;
    public string ChangedByUserId { get; set; } = string.Empty;
}
