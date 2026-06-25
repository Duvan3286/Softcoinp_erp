using System;
using System.Collections.Generic;
using Softcoinp.ERP.Domain.Enums;

namespace Softcoinp.ERP.Domain.Entities;

public class BillingPeriod
{
    public Guid Id { get; set; }
    public string TenantId { get; set; } = string.Empty;

    public string Period { get; set; } = string.Empty;
    public decimal MonthlyBudgetTotal { get; set; }
    public decimal TotalBilled { get; set; }
    public DateTime CutoffDate { get; set; }
    public DateTime PaymentDueDate { get; set; }
    public BillingPeriodStatus Status { get; set; } = BillingPeriodStatus.Pending;
    public DateTime? ExecutedAt { get; set; }
    public string ExecutedByUserId { get; set; } = string.Empty;
    public decimal RoundingAdjustment { get; set; }
    public string Notes { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string CreatedByUserId { get; set; } = string.Empty;
    public DateTime? UpdatedAt { get; set; }

    public ICollection<UnitFee> UnitFees { get; set; } = new List<UnitFee>();
}
