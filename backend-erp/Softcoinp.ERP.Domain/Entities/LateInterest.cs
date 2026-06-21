using System;
using Softcoinp.ERP.Domain.Enums;

namespace Softcoinp.ERP.Domain.Entities;

public class LateInterest
{
    public Guid Id { get; set; }
    public string TenantId { get; set; } = string.Empty;

    public Guid? UnitFeeId { get; set; }
    public UnitFee? UnitFee { get; set; }

    public Guid? ExtraordinaryFeeDistributionId { get; set; }
    public ExtraordinaryFeeDistribution? ExtraordinaryFeeDistribution { get; set; }

    public Guid? IndividualChargeId { get; set; }
    public IndividualCharge? IndividualCharge { get; set; }

    public string Period { get; set; } = string.Empty;
    public decimal BaseAmount { get; set; }
    public decimal DailyRate { get; set; }
    public int DaysOverdue { get; set; }
    public decimal CalculatedAmount { get; set; }
    public bool IsCapitalized { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
}
