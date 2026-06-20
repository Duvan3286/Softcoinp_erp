using System;
using Softcoinp.ERP.Domain.Enums;

namespace Softcoinp.ERP.Domain.Entities;

public class AccountingPeriod
{
    public Guid Id { get; set; }
    public string TenantId { get; set; } = string.Empty;

    public int FiscalYear { get; set; }
    public int Month { get; set; }
    public string PeriodLabel { get; set; } = string.Empty;
    public AccountingPeriodStatus Status { get; set; } = AccountingPeriodStatus.Open;
    public DateTime OpenedAt { get; set; } = DateTime.UtcNow;
    public DateTime? ClosedAt { get; set; }
    public string? ClosedByUserId { get; set; }
    public int LastEntryNumber { get; set; }
}
