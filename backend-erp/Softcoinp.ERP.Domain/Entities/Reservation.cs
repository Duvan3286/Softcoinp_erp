using System;
using Softcoinp.ERP.Domain.Enums;

namespace Softcoinp.ERP.Domain.Entities;

public class Reservation
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string TenantId { get; set; } = string.Empty;

    public string ReservationNumber { get; set; } = string.Empty;

    public Guid SpaceId { get; set; }
    public ReservableSpace? Space { get; set; }

    public Guid UnitId { get; set; }
    public Unit? Unit { get; set; }

    public Guid OwnerId { get; set; }
    public Owner? Owner { get; set; }

    public DateTime StartDateTime { get; set; }
    public DateTime EndDateTime { get; set; }
    public int EstimatedAttendees { get; set; }
    public string? EventDescription { get; set; }

    public bool HasMusic { get; set; }
    public string? MusicEndTime { get; set; }

    public bool RulesAccepted { get; set; }

    public ReservationStatus Status { get; set; } = ReservationStatus.Requested;
    public string? RejectionReason { get; set; }

    public decimal TotalCost { get; set; }
    public DepositStatus DepositStatus { get; set; } = DepositStatus.NotRequired;
    public decimal DepositAmount { get; set; }
    public Guid? DepositChargeId { get; set; }
    public Guid? DepositReturnChargeId { get; set; }

    public string? AdminNotes { get; set; }
    public string? AdminUserId { get; set; }

    public DateTime? CheckedInAt { get; set; }
    public DateTime? CheckedOutAt { get; set; }
    public string? CheckoutSignaturePath { get; set; }

    public bool ExceptionGranted { get; set; }
    public string? ExceptionReason { get; set; }
    public string? ExceptionGrantedByUserId { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string CreatedByUserId { get; set; } = string.Empty;
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedByUserId { get; set; }

    public ICollection<ReservationDeposit> Deposits { get; set; } = new List<ReservationDeposit>();
    public ICollection<ReservationIncident> Incidents { get; set; } = new List<ReservationIncident>();
    public ICollection<ReservationReminder> Reminders { get; set; } = new List<ReservationReminder>();
}
