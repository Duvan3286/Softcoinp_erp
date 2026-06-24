using System;
using Softcoinp.ERP.Domain.Enums;

namespace Softcoinp.ERP.Domain.Entities;

public class CommunicationRecipient
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string TenantId { get; set; } = string.Empty;

    public Guid CommunicationId { get; set; }
    public Communication? Communication { get; set; }

    public Guid? OwnerId { get; set; }
    public Owner? Owner { get; set; }

    public Guid? TenantResidentId { get; set; }
    public TenantResident? TenantResident { get; set; }

    public string RecipientEmail { get; set; } = string.Empty;
    public string RecipientPhone { get; set; } = string.Empty;

    public DeliveryStatus EmailStatus { get; set; } = DeliveryStatus.Pending;
    public DeliveryStatus SmsStatus { get; set; } = DeliveryStatus.Pending;
    public DeliveryStatus PushStatus { get; set; } = DeliveryStatus.Pending;
    public DeliveryStatus BulletinBoardStatus { get; set; } = DeliveryStatus.Pending;

    public DateTime? EmailSentAt { get; set; }
    public DateTime? SmsSentAt { get; set; }
    public DateTime? PushSentAt { get; set; }
    public DateTime? ReadConfirmedAt { get; set; }

    public int ResentCount { get; set; }
    public DateTime? LastResentAt { get; set; }
    public string? ErrorMessage { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
