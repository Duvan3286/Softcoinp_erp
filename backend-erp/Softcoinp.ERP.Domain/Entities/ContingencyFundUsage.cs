using System;
using Softcoinp.ERP.Domain.Common;

namespace Softcoinp.ERP.Domain.Entities;

public class ContingencyFundUsage : BaseEntity
{
    public string TenantId { get; set; } = string.Empty;
    public Guid BudgetId { get; set; }
    public Budget? Budget { get; set; }
    public string Justification { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string CouncilApprovalActNumber { get; set; } = string.Empty;
    public Guid? ExecutedExpenseId { get; set; }
    public ExecutedExpense? ExecutedExpense { get; set; }
    public string CreatedByUserId { get; set; } = string.Empty;
}
