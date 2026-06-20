using System;
using Softcoinp.ERP.Domain.Common;

namespace Softcoinp.ERP.Domain.Entities;

public class ContingencyFundUsage : BaseEntity
{
    public string TenantId { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string Justification { get; set; } = string.Empty;
    public string CouncilApprovalActNumber { get; set; } = string.Empty;
    public DateTime ApprovalDate { get; set; }
    public Guid? AccountingRecordId { get; set; }
    public string CreatedByUserId { get; set; } = string.Empty;
}
