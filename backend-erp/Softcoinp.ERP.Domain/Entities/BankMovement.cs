using System;
using Softcoinp.ERP.Domain.Enums;

namespace Softcoinp.ERP.Domain.Entities;

public class BankMovement
{
    public Guid Id { get; set; }
    public string TenantId { get; set; } = string.Empty;

    public Guid BankAccountId { get; set; }
    public BankAccount? BankAccount { get; set; }

    public Guid? AccountingEntryId { get; set; }
    public AccountingEntry? AccountingEntry { get; set; }

    public BankMovementType MovementType { get; set; }
    public decimal Amount { get; set; }
    public DateTime MovementDate { get; set; }
    public string Description { get; set; } = string.Empty;
    public string ReferenceNumber { get; set; } = string.Empty;
    public decimal RunningBalance { get; set; }

    public string CreatedByUserId { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
