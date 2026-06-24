using System;
using Softcoinp.ERP.Domain.Enums;

namespace Softcoinp.ERP.Domain.Entities;

public class NotificationTemplate
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string TenantId { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;
    public NotificationEventType EventType { get; set; }
    public RecipientType ForRecipientType { get; set; } = RecipientType.Owner;

    public string EmailSubject { get; set; } = string.Empty;
    public string EmailBody { get; set; } = string.Empty;
    public string SmsBody { get; set; } = string.Empty;

    public string DynamicVariables { get; set; } = string.Empty;

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    public string CreatedByUserId { get; set; } = string.Empty;
}
