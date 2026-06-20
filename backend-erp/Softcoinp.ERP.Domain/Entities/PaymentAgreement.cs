using System;
using System.Collections.Generic;
using Softcoinp.ERP.Domain.Enums;

namespace Softcoinp.ERP.Domain.Entities;

public class PaymentAgreement
{
    public Guid Id { get; set; }
    public string TenantId { get; set; } = string.Empty;

    public Guid UnitId { get; set; }
    public Unit? Unit { get; set; }

    public decimal TotalDebtIncluded { get; set; }
    public decimal InstallmentAmount { get; set; }
    public int NumberOfInstallments { get; set; }
    public decimal InterestForgivenessPercentage { get; set; }
    public string CouncilActNumber { get; set; } = string.Empty;
    public AgreementStatus Status { get; set; } = AgreementStatus.Active;
    public DateTime StartedAt { get; set; }
    public DateTime? DefaultedAt { get; set; }
    public string DigitalAcceptance { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string CreatedByUserId { get; set; } = string.Empty;
    public DateTime? UpdatedAt { get; set; }

    public ICollection<AgreementInstallment> Installments { get; set; } = new List<AgreementInstallment>();
}
