using System;
using Softcoinp.ERP.Domain.Enums;

namespace Softcoinp.ERP.Domain.Entities;

public class ReservationDeposit
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string TenantId { get; set; } = string.Empty;

    public Guid ReservationId { get; set; }
    public Reservation? Reservation { get; set; }

    public decimal Amount { get; set; }
    public DepositStatus Status { get; set; } = DepositStatus.Pending;
    public DepositPaymentMethod? PaymentMethod { get; set; }

    public Guid? ChargeId { get; set; }
    public string? ChargeNumber { get; set; }

    public Guid? ReturnChargeId { get; set; }
    public string? ReturnChargeNumber { get; set; }

    public decimal? DamageAmount { get; set; }
    public string? DamageDescription { get; set; }

    public DateTime? PaidAt { get; set; }
    public DateTime? ReturnedAt { get; set; }
    public DateTime? AppliedAt { get; set; }

    public string? ProcessedByUserId { get; set; }
    public string? Notes { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
