using System;
using Softcoinp.ERP.Domain.Common;

namespace Softcoinp.ERP.Domain.Entities;

/// <summary>
/// Represents a double-entry journal movement (movimiento contable) in the general ledger.
/// Used to compute real-time budget execution by summing Debits/Credits.
/// </summary>
public class AccountingEntry : BaseEntity
{
    public string TenantId { get; set; } = string.Empty;
    
    public Guid AccountingAccountId { get; set; }
    public AccountingAccount? AccountingAccount { get; set; }

    public decimal Debit { get; set; }
    public decimal Credit { get; set; }
    public DateTime EntryDate { get; set; }
    public string Description { get; set; } = string.Empty;
    public string Reference { get; set; } = string.Empty; // Comprobante de egreso, ingreso, etc.
}
