using System;
using Softcoinp.ERP.Domain.Common;
using Softcoinp.ERP.Domain.Enums;

namespace Softcoinp.ERP.Domain.Entities;

public class BudgetModification : BaseEntity
{
    public string TenantId { get; set; } = string.Empty;
    public Guid BudgetId { get; set; }
    public Budget? Budget { get; set; }
    public Guid? ExpenseItemId { get; set; }
    public ExpenseItem? ExpenseItem { get; set; }
    public Guid? IncomeItemId { get; set; }
    public IncomeItem? IncomeItem { get; set; }
    public ModificationType ModificationType { get; set; }
    public decimal Amount { get; set; }
    public decimal PreviousValue { get; set; }
    public decimal NewValue { get; set; }
    public string Justification { get; set; } = string.Empty;
    public ApprovalType ApprovalType { get; set; }
    public string MeetingActNumber { get; set; } = string.Empty;
    public DateTime ApprovalDate { get; set; }
    public string CreatedByUserId { get; set; } = string.Empty;
}
