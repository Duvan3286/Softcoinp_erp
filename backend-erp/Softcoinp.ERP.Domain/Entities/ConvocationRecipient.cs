using System;

namespace Softcoinp.ERP.Domain.Entities;

public class ConvocationRecipient
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string TenantId { get; set; } = string.Empty;

    public Guid ConvocationId { get; set; }
    public AssemblyConvocation? Convocation { get; set; }

    public Guid UnitId { get; set; }
    public Unit? Unit { get; set; }

    public Guid OwnerId { get; set; }
    public Owner? Owner { get; set; }

    public string OwnerName { get; set; } = string.Empty;
    public string OwnerEmail { get; set; } = string.Empty;
    public string? OwnerPhone { get; set; }

    public bool Delivered { get; set; }
    public DateTime? DeliveredAt { get; set; }
    public string? DeliveryError { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
