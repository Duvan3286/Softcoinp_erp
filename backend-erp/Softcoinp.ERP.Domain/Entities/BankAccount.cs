using System;
using System.Collections.Generic;
using Softcoinp.ERP.Domain.Enums;

namespace Softcoinp.ERP.Domain.Entities;

public class BankAccount
{
    public Guid Id { get; set; }
    public string TenantId { get; set; } = string.Empty;

    public Guid AccountingAccountId { get; set; }
    public AccountingAccount? AccountingAccount { get; set; }

    public string BankName { get; set; } = string.Empty;
    public string AccountNumber { get; set; } = string.Empty;
    public BankAccountType AccountType { get; set; }
    public decimal CurrentBalance { get; set; }
    public decimal OpeningBalance { get; set; }
    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    public ICollection<BankMovement> Movements { get; set; } = new List<BankMovement>();
    public ICollection<BankReconciliation> Reconciliations { get; set; } = new List<BankReconciliation>();
}
