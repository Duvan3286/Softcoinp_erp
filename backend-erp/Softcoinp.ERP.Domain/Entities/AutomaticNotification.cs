using System;
using Softcoinp.ERP.Domain.Enums;

namespace Softcoinp.ERP.Domain.Entities;

public class AutomaticNotification
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string TenantId { get; set; } = string.Empty;

    public NotificationEventType EventType { get; set; }

    public Guid? CommunicationId { get; set; }
    public Communication? Communication { get; set; }

    public Guid? OwnerId { get; set; }
    public Owner? Owner { get; set; }

    public Guid? TenantResidentId { get; set; }
    public TenantResident? TenantResident { get; set; }

    public string RecipientEmail { get; set; } = string.Empty;
    public string RecipientPhone { get; set; } = string.Empty;

    public CommunicationChannel Channel { get; set; }
    public DeliveryStatus Status { get; set; } = DeliveryStatus.Pending;

    public DateTime? SentAt { get; set; }
    public DateTime? ReadAt { get; set; }

    public string SourceModule { get; set; } = string.Empty;
    public string SourceEntityId { get; set; } = string.Empty;
    public string SourceEntityType { get; set; } = string.Empty;

    public string? ErrorMessage { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
