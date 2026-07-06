using Softcoinp.ERP.Domain.Common;
using Softcoinp.ERP.Domain.Enums;

namespace Softcoinp.ERP.Domain.Entities;

public class ExpenseItem : BaseEntity
{
    public Guid BudgetId { get; set; }
    public Budget? Budget { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public ExpenseCategory Category { get; set; }
    public decimal AnnualValue { get; set; }
    public bool IsContingencyFund { get; set; }
    public decimal ContingencyPercentage { get; set; }
    public bool RequiresCouncilApproval { get; set; }
    public decimal ApprovalThreshold { get; set; }
}
