using System;
using System.Collections.Generic;
using Softcoinp.ERP.Domain.Enums;

namespace Softcoinp.ERP.Domain.Entities;

public class AccountingEntry
{
    public Guid Id { get; set; }
    public string TenantId { get; set; } = string.Empty;

    public Guid? AccountingPeriodId { get; set; }
    public AccountingPeriod? AccountingPeriod { get; set; }

    public int EntryNumber { get; set; }
    public EntryType EntryType { get; set; }
    public EntryStatus Status { get; set; } = EntryStatus.Draft;
    public DateTime EntryDate { get; set; }
    public string Description { get; set; } = string.Empty;
    public string ExternalReference { get; set; } = string.Empty;
    public decimal TotalDebit { get; set; }
    public decimal TotalCredit { get; set; }

    public string CreatedByUserId { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string? UpdatedByUserId { get; set; }
    public DateTime? UpdatedAt { get; set; }

    public ICollection<EntryLine> Lines { get; set; } = new List<EntryLine>();
    public EntryReversal? Reversal { get; set; }
}
