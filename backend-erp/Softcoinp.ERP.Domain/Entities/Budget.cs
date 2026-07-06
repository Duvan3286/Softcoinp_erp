using System;
using System.Collections.Generic;
using Softcoinp.ERP.Domain.Common;
using Softcoinp.ERP.Domain.Enums;

namespace Softcoinp.ERP.Domain.Entities;

public class Budget : BaseEntity
{
    public string TenantId { get; set; } = string.Empty;
    public int FiscalYear { get; set; }
    public DateTime? ApprovalDate { get; set; }
    public string MeetingActNumber { get; set; } = string.Empty;
    public BudgetStatus Status { get; set; } = BudgetStatus.Draft;
    public string Observations { get; set; } = string.Empty;
    public string CreatedByUserId { get; set; } = string.Empty;

    public ICollection<IncomeItem> IncomeItems { get; set; } = new List<IncomeItem>();
    public ICollection<ExpenseItem> ExpenseItems { get; set; } = new List<ExpenseItem>();
}
