using System;
using Softcoinp.ERP.Domain.Common;

namespace Softcoinp.ERP.Domain.Entities;

public class BudgetDetail : BaseEntity
{
    public Guid BudgetId { get; set; }
    public Budget? Budget { get; set; }

    public Guid AccountingAccountId { get; set; }
    public AccountingAccount? AccountingAccount { get; set; }

    public decimal ApprovedValue { get; set; }
    public string Observations { get; set; } = string.Empty;
}
