using System;
using Softcoinp.ERP.Domain.Enums;

namespace Softcoinp.ERP.Domain.Entities;

public class UnitFee
{
    public Guid Id { get; set; }
    public string TenantId { get; set; } = string.Empty;

    public Guid BillingPeriodId { get; set; }
    public BillingPeriod? BillingPeriod { get; set; }

    public Guid UnitId { get; set; }
    public Unit? Unit { get; set; }

    public decimal FeeValue { get; set; }
    public DateTime DueDate { get; set; }
    public FeeStatus Status { get; set; } = FeeStatus.Pending;
    public decimal PaidAmount { get; set; }
    public decimal BalanceAmount { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    // Navigation properties for PQR module
    public System.Collections.Generic.ICollection<PqrRecord> PqrRecords { get; set; } = new System.Collections.Generic.List<PqrRecord>();
}
