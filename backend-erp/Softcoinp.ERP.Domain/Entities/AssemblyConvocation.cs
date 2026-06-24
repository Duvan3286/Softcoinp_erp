using System;
using System.Collections.Generic;

namespace Softcoinp.ERP.Domain.Entities;

public class AssemblyConvocation
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string TenantId { get; set; } = string.Empty;

    public Guid AssemblyId { get; set; }
    public Assembly? Assembly { get; set; }

    public int ConvocationNumber { get; set; }
    public string Subject { get; set; } = string.Empty;
    public string? Notes { get; set; }

    public DateTime? SentAt { get; set; }
    public string SentByUserId { get; set; } = string.Empty;
    public string Channel { get; set; } = string.Empty;

    public int TotalRecipients { get; set; }
    public int DeliveredCount { get; set; }
    public int FailedCount { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<ConvocationDocument> Documents { get; set; } = new List<ConvocationDocument>();
    public ICollection<ConvocationRecipient> Recipients { get; set; } = new List<ConvocationRecipient>();
}
