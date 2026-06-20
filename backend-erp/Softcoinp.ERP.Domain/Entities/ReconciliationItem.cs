using System;

namespace Softcoinp.ERP.Domain.Entities;

public class ReconciliationItem
{
    public Guid Id { get; set; }

    public Guid BankReconciliationId { get; set; }
    public BankReconciliation? BankReconciliation { get; set; }

    public Guid? BankMovementId { get; set; }
    public BankMovement? BankMovement { get; set; }

    public string Description { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public DateTime MovementDate { get; set; }

    public bool IsInBooks { get; set; }
    public bool IsInStatement { get; set; }
    public bool IsCleared { get; set; }
}
