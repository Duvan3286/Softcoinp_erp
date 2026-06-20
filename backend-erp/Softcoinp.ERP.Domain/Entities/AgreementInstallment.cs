using System;
using Softcoinp.ERP.Domain.Enums;

namespace Softcoinp.ERP.Domain.Entities;

public class AgreementInstallment
{
    public Guid Id { get; set; }
    public string TenantId { get; set; } = string.Empty;

    public Guid PaymentAgreementId { get; set; }
    public PaymentAgreement? PaymentAgreement { get; set; }

    public int InstallmentNumber { get; set; }
    public DateTime DueDate { get; set; }
    public decimal Amount { get; set; }
    public decimal PaidAmount { get; set; }
    public AgreementInstallmentStatus Status { get; set; } = AgreementInstallmentStatus.Pending;
    public DateTime? PaidAt { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
}
