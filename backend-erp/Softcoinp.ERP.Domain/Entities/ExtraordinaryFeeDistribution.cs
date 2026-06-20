using System;
using Softcoinp.ERP.Domain.Enums;

namespace Softcoinp.ERP.Domain.Entities;

public class ExtraordinaryFeeDistribution
{
    public Guid Id { get; set; }
    public string TenantId { get; set; } = string.Empty;

    public Guid ExtraordinaryFeeId { get; set; }
    public ExtraordinaryFee? ExtraordinaryFee { get; set; }

    public Guid UnitId { get; set; }
    public Unit? Unit { get; set; }

    public int InstallmentNumber { get; set; }
    public decimal Amount { get; set; }
    public DateTime DueDate { get; set; }
    public FeeStatus Status { get; set; } = FeeStatus.Pending;
    public decimal PaidAmount { get; set; }
    public decimal BalanceAmount { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
}
