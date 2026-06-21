using System;

namespace Softcoinp.ERP.Domain.Entities;

public class Notification
{
    public Guid Id { get; set; }
    public string TenantId { get; set; } = string.Empty;

    public Guid OwnerId { get; set; }
    public Owner? Owner { get; set; }

    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public bool IsRead { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
