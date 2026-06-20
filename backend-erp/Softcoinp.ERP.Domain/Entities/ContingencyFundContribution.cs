using System;
using Softcoinp.ERP.Domain.Common;

namespace Softcoinp.ERP.Domain.Entities;

public class ContingencyFundContribution : BaseEntity
{
    public string TenantId { get; set; } = string.Empty;
    public string Period { get; set; } = string.Empty; // format: "YYYY-MM"
    public decimal Amount { get; set; }
    public decimal IncomeBase { get; set; }
    public decimal Percentage { get; set; }
    public DateTime ContributionDate { get; set; }
    public Guid? AccountingRecordId { get; set; }
}
