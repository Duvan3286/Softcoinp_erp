using System;
using Softcoinp.ERP.Domain.Enums;

namespace Softcoinp.ERP.Domain.Entities;

public class ReservationReminder
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string TenantId { get; set; } = string.Empty;

    public Guid ReservationId { get; set; }
    public Reservation? Reservation { get; set; }

    public ReminderType ReminderType { get; set; } = ReminderType.TwentyFourHours;
    public ReminderStatus Status { get; set; } = ReminderStatus.Pending;

    public DateTime ScheduledFor { get; set; }
    public DateTime? SentAt { get; set; }

    public string Channel { get; set; } = string.Empty;
    public string? RecipientEmail { get; set; }
    public string? RecipientPhone { get; set; }

    public string? ErrorMessage { get; set; }
    public int RetryCount { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
