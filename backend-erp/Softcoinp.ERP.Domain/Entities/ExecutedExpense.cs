using System;
using Softcoinp.ERP.Domain.Common;

namespace Softcoinp.ERP.Domain.Entities;

public class ExecutedExpense : BaseEntity
{
    public string TenantId { get; set; } = string.Empty;
    public Guid ExpenseItemId { get; set; }
    public ExpenseItem? ExpenseItem { get; set; }
    public string Description { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public DateTime ExpenseDate { get; set; }
    public Guid? ProviderId { get; set; }
    public Provider? Provider { get; set; }
    public string InvoiceReference { get; set; } = string.Empty;
    public bool CouncilApproved { get; set; }
    public string CreatedByUserId { get; set; } = string.Empty;
}
