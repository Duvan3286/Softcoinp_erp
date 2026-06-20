using System;
using Softcoinp.ERP.Domain.Enums;

namespace Softcoinp.ERP.Domain.Entities;

public class IndividualCharge
{
    public Guid Id { get; set; }
    public string TenantId { get; set; } = string.Empty;

    public Guid UnitId { get; set; }
    public Unit? Unit { get; set; }

    public ChargeType ChargeType { get; set; }
    public string Concept { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public DateTime ChargeDate { get; set; }
    public string Description { get; set; } = string.Empty;
    public string ReferenceActNumber { get; set; } = string.Empty;
    public bool IsDisputed { get; set; }
    public string DisputeReason { get; set; } = string.Empty;
    public IndividualChargeStatus Status { get; set; } = IndividualChargeStatus.Pending;
    public decimal PaidAmount { get; set; }
    public decimal BalanceAmount { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string CreatedByUserId { get; set; } = string.Empty;
    public DateTime? UpdatedAt { get; set; }
}
