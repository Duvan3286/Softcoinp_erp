using System;
using System.Collections.Generic;
using Softcoinp.ERP.Domain.Enums;

namespace Softcoinp.ERP.Domain.Entities;

public class BankReconciliation
{
    public Guid Id { get; set; }
    public string TenantId { get; set; } = string.Empty;

    public Guid BankAccountId { get; set; }
    public BankAccount? BankAccount { get; set; }

    public int FiscalYear { get; set; }
    public int Month { get; set; }
    public string PeriodLabel { get; set; } = string.Empty;

    public decimal BookBalance { get; set; }
    public decimal StatementBalance { get; set; }
    public decimal Difference { get; set; }
    public ReconciliationStatus Status { get; set; } = ReconciliationStatus.InProgress;

    public string CreatedByUserId { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? CompletedAt { get; set; }
    public string? CompletedByUserId { get; set; }

    public ICollection<ReconciliationItem> Items { get; set; } = new List<ReconciliationItem>();
}
