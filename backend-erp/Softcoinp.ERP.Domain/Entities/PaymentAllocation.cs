using System;
using Softcoinp.ERP.Domain.Enums;

namespace Softcoinp.ERP.Domain.Entities;

public class PaymentAllocation
{
    public Guid Id { get; set; }
    public string TenantId { get; set; } = string.Empty;

    public Guid PaymentId { get; set; }
    public Payment? Payment { get; set; }

    public Guid? UnitFeeId { get; set; }
    public UnitFee? UnitFee { get; set; }

    public Guid? ExtraordinaryFeeDistributionId { get; set; }
    public ExtraordinaryFeeDistribution? ExtraordinaryFeeDistribution { get; set; }

    public Guid? IndividualChargeId { get; set; }
    public IndividualCharge? IndividualCharge { get; set; }

    public decimal Amount { get; set; }
    public PaymentAllocationType AllocationType { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
