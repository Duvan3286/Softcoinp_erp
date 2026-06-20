using System;

namespace Softcoinp.ERP.Domain.Entities;

public class ContactHistory
{
    public Guid Id { get; set; }
    public string TenantId { get; set; } = string.Empty;
    
    public Guid OwnerId { get; set; }
    public Owner? Owner { get; set; }
    
    public string FieldChanged { get; set; } = string.Empty;
    public string? OldValue { get; set; }
    public string? NewValue { get; set; }
    
    public DateTime ChangedAt { get; set; } = DateTime.UtcNow;
    public string ChangedByUserId { get; set; } = string.Empty;
}
