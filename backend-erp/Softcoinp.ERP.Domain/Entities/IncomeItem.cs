using Softcoinp.ERP.Domain.Common;

namespace Softcoinp.ERP.Domain.Entities;

public class IncomeItem : BaseEntity
{
    public Guid BudgetId { get; set; }
    public Budget? Budget { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal AnnualValue { get; set; }
}
