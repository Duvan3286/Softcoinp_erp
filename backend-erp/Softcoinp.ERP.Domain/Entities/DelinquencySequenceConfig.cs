using System;

namespace Softcoinp.ERP.Domain.Entities;

public class DelinquencySequenceConfig
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string TenantId { get; set; } = string.Empty;

    public int StepNumber { get; set; }
    public int DaysAfterDue { get; set; }

    public Guid TemplateId { get; set; }
    public NotificationTemplate? Template { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
}
