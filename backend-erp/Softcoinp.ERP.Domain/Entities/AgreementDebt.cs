using System;

namespace Softcoinp.ERP.Domain.Entities;

public class AgreementDebt
{
    public Guid Id { get; set; }
    public string TenantId { get; set; } = string.Empty;

    public Guid PaymentAgreementId { get; set; }
    public PaymentAgreement? PaymentAgreement { get; set; }

    public string SourceType { get; set; } = string.Empty;
    public Guid SourceId { get; set; }
    public decimal OriginalBalance { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
