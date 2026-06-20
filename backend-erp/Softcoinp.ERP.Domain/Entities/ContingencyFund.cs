using Softcoinp.ERP.Domain.Common;

namespace Softcoinp.ERP.Domain.Entities;

public class ContingencyFund : BaseEntity
{
    public string TenantId { get; set; } = string.Empty;
    public decimal CurrentBalance { get; set; }
}
