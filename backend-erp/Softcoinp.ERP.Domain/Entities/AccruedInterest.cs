using System;
using Softcoinp.ERP.Domain.Enums;

namespace Softcoinp.ERP.Domain.Entities;

public class AccruedInterest
{
    public Guid Id { get; set; }
    public string TenantId { get; set; } = string.Empty;

    public Guid UnitId { get; set; }
    public Unit? Unit { get; set; }

    public Guid? UnitFeeId { get; set; }
    public UnitFee? UnitFee { get; set; }

    public Guid? ExtraordinaryFeeDistributionId { get; set; }
    public ExtraordinaryFeeDistribution? ExtraordinaryFeeDistribution { get; set; }

    public Guid? IndividualChargeId { get; set; }
    public IndividualCharge? IndividualCharge { get; set; }

    public string Period { get; set; } = string.Empty;

    public decimal DailyRate { get; set; }

    public int DaysInPeriod { get; set; }

    public decimal BaseAmount { get; set; }

    public decimal CalculatedAmount { get; set; }

    public decimal BalanceAmount { get; set; }

    public AccruedInterestStatus Status { get; set; } = AccruedInterestStatus.Pending;

    public DateTime InterestStartDate { get; set; }
    public DateTime InterestEndDate { get; set; }

    public Guid MonthlyInterestRateId { get; set; }
    public MonthlyInterestRate? MonthlyInterestRate { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
}
