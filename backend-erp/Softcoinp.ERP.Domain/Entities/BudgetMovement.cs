using System;
using Softcoinp.ERP.Domain.Common;
using Softcoinp.ERP.Domain.Enums;

namespace Softcoinp.ERP.Domain.Entities;

public class BudgetMovement : BaseEntity
{
    public string TenantId { get; set; } = string.Empty;
    public Guid BudgetId { get; set; }
    public Budget? Budget { get; set; }

    public BudgetMovementType MovementType { get; set; }

    // Source account is null for Additions, and required for Transfers
    public Guid? SourceAccountId { get; set; }
    public AccountingAccount? SourceAccount { get; set; }

    public Guid DestinationAccountId { get; set; }
    public AccountingAccount? DestinationAccount { get; set; }

    public decimal Amount { get; set; }
    public string Justification { get; set; } = string.Empty;

    public BudgetApprovalType ApprovalType { get; set; }
    public string MeetingActNumber { get; set; } = string.Empty;
    public DateTime ApprovalDate { get; set; }
    public string CreatedByUserId { get; set; } = string.Empty;
}
