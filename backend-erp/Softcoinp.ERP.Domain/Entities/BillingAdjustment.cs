using System;

namespace Softcoinp.ERP.Domain.Entities;

public class BillingAdjustment
{
    public Guid Id { get; set; }
    public string TenantId { get; set; } = string.Empty;

    public Guid UnitId { get; set; }
    public Unit? Unit { get; set; }

    public Guid? BillingPeriodId { get; set; }
    public BillingPeriod? BillingPeriod { get; set; }

    public Guid? UnitFeeId { get; set; }
    public UnitFee? UnitFee { get; set; }

    public decimal Amount { get; set; }
    public string Reason { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string CreatedByUserId { get; set; } = string.Empty;
}
