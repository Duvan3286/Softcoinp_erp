using System;

namespace Softcoinp.ERP.Domain.Entities;

public class EntryReversal
{
    public Guid Id { get; set; }
    public string TenantId { get; set; } = string.Empty;

    public Guid OriginalEntryId { get; set; }
    public AccountingEntry? OriginalEntry { get; set; }

    public Guid ReversalEntryId { get; set; }
    public AccountingEntry? ReversalEntry { get; set; }

    public string Reason { get; set; } = string.Empty;
    public DateTime ReversedAt { get; set; } = DateTime.UtcNow;
    public string ReversedByUserId { get; set; } = string.Empty;
}
