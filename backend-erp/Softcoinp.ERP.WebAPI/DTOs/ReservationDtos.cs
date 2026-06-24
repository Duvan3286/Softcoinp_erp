using System;
using System.Collections.Generic;

namespace Softcoinp.ERP.WebAPI.DTOs;

// ── Reservable Space ─────────────────────────────────────────────

public class ReservableSpaceListDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Location { get; set; }
    public int MaxCapacity { get; set; }
    public bool RequiresDeposit { get; set; }
    public decimal DepositAmount { get; set; }
    public bool HasAdditionalCost { get; set; }
    public string ChargeType { get; set; } = string.Empty;
    public decimal HourlyRate { get; set; }
    public decimal EventRate { get; set; }
    public string ApprovalMode { get; set; } = string.Empty;
    public string ArrearsPolicy { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public int ActiveReservations { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class ReservableSpaceDetailDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Location { get; set; }
    public int MaxCapacity { get; set; }
    public int MinReservationHours { get; set; }
    public int MaxReservationHours { get; set; }
    public int MinAdvanceHours { get; set; }
    public int MaxAdvanceDays { get; set; }
    public int MaxSimultaneousReservationsPerUnit { get; set; }
    public bool RequiresDeposit { get; set; }
    public decimal DepositAmount { get; set; }
    public bool HasAdditionalCost { get; set; }
    public string ChargeType { get; set; } = string.Empty;
    public decimal HourlyRate { get; set; }
    public decimal EventRate { get; set; }
    public string ApprovalMode { get; set; } = string.Empty;
    public string ArrearsPolicy { get; set; } = string.Empty;
    public bool IsAvailableForMaintenance { get; set; }
    public bool IsActive { get; set; }
    public string? RulesFilePath { get; set; }
    public string? ImageFilePath { get; set; }
    public string CreatedByUserId { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public List<SpaceScheduleDto> Schedules { get; set; } = new();
}

public class CreateReservableSpaceRequestDto
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Location { get; set; }
    public int MaxCapacity { get; set; }
    public int MinReservationHours { get; set; } = 1;
    public int MaxReservationHours { get; set; } = 8;
    public int MinAdvanceHours { get; set; } = 2;
    public int MaxAdvanceDays { get; set; } = 30;
    public int MaxSimultaneousReservationsPerUnit { get; set; } = 2;
    public bool RequiresDeposit { get; set; }
    public decimal DepositAmount { get; set; }
    public bool HasAdditionalCost { get; set; }
    public string? ChargeType { get; set; }
    public decimal HourlyRate { get; set; }
    public decimal EventRate { get; set; }
    public string? ApprovalMode { get; set; }
    public string? ArrearsPolicy { get; set; }
    public string? RulesFilePath { get; set; }
    public string? ImageFilePath { get; set; }
}

public class UpdateReservableSpaceRequestDto
{
    public string? Name { get; set; }
    public string? Description { get; set; }
    public string? Location { get; set; }
    public int? MaxCapacity { get; set; }
    public int? MinReservationHours { get; set; }
    public int? MaxReservationHours { get; set; }
    public int? MinAdvanceHours { get; set; }
    public int? MaxAdvanceDays { get; set; }
    public int? MaxSimultaneousReservationsPerUnit { get; set; }
    public bool? RequiresDeposit { get; set; }
    public decimal? DepositAmount { get; set; }
    public bool? HasAdditionalCost { get; set; }
    public string? ChargeType { get; set; }
    public decimal? HourlyRate { get; set; }
    public decimal? EventRate { get; set; }
    public string? ApprovalMode { get; set; }
    public string? ArrearsPolicy { get; set; }
    public string? RulesFilePath { get; set; }
    public string? ImageFilePath { get; set; }
    public bool? IsActive { get; set; }
}

// ── Space Schedule ───────────────────────────────────────────────

public class SpaceScheduleDto
{
    public Guid Id { get; set; }
    public int DayOfWeek { get; set; }
    public string DayName { get; set; } = string.Empty;
    public string StartTime { get; set; } = string.Empty;
    public string EndTime { get; set; } = string.Empty;
    public bool IsActive { get; set; }
}

public class CreateSpaceScheduleRequestDto
{
    public int DayOfWeek { get; set; }
    public string StartTime { get; set; } = string.Empty;
    public string EndTime { get; set; } = string.Empty;
}

// ── Space Block ──────────────────────────────────────────────────

public class SpaceBlockDto
{
    public Guid Id { get; set; }
    public Guid SpaceId { get; set; }
    public string SpaceName { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public string StartTime { get; set; } = string.Empty;
    public string EndTime { get; set; } = string.Empty;
    public string Origin { get; set; } = string.Empty;
    public string? Reason { get; set; }
    public Guid? RelatedWorkOrderId { get; set; }
    public string? RelatedWorkOrderNumber { get; set; }
    public bool NotifyAffectedResidents { get; set; }
    public bool NotificationSent { get; set; }
    public string CreatedByUserId { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}

public class CreateSpaceBlockRequestDto
{
    public Guid SpaceId { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public string StartTime { get; set; } = "00:00";
    public string EndTime { get; set; } = "23:59";
    public string Origin { get; set; } = "Administrative";
    public string? Reason { get; set; }
    public Guid? RelatedWorkOrderId { get; set; }
    public bool NotifyAffectedResidents { get; set; } = true;
}

// ── Reservation ──────────────────────────────────────────────────

public class ReservationListDto
{
    public Guid Id { get; set; }
    public string ReservationNumber { get; set; } = string.Empty;
    public Guid SpaceId { get; set; }
    public string SpaceName { get; set; } = string.Empty;
    public Guid UnitId { get; set; }
    public string UnitIdentifier { get; set; } = string.Empty;
    public Guid OwnerId { get; set; }
    public string OwnerName { get; set; } = string.Empty;
    public DateTime StartDateTime { get; set; }
    public DateTime EndDateTime { get; set; }
    public int EstimatedAttendees { get; set; }
    public string? EventDescription { get; set; }
    public string Status { get; set; } = string.Empty;
    public decimal TotalCost { get; set; }
    public string DepositStatus { get; set; } = string.Empty;
    public decimal DepositAmount { get; set; }
    public string? AdminNotes { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class ReservationDetailDto
{
    public Guid Id { get; set; }
    public string ReservationNumber { get; set; } = string.Empty;
    public Guid SpaceId { get; set; }
    public string SpaceName { get; set; } = string.Empty;
    public Guid UnitId { get; set; }
    public string UnitIdentifier { get; set; } = string.Empty;
    public Guid OwnerId { get; set; }
    public string OwnerName { get; set; } = string.Empty;
    public string OwnerEmail { get; set; } = string.Empty;
    public string OwnerPhone { get; set; } = string.Empty;
    public DateTime StartDateTime { get; set; }
    public DateTime EndDateTime { get; set; }
    public int EstimatedAttendees { get; set; }
    public string? EventDescription { get; set; }
    public bool HasMusic { get; set; }
    public string? MusicEndTime { get; set; }
    public bool RulesAccepted { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? RejectionReason { get; set; }
    public decimal TotalCost { get; set; }
    public string DepositStatus { get; set; } = string.Empty;
    public decimal DepositAmount { get; set; }
    public string? AdminNotes { get; set; }
    public string? AdminUserId { get; set; }
    public DateTime? CheckedInAt { get; set; }
    public DateTime? CheckedOutAt { get; set; }
    public string? CheckoutSignaturePath { get; set; }
    public bool ExceptionGranted { get; set; }
    public string? ExceptionReason { get; set; }
    public string CreatedByUserId { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public List<ReservationDepositDto> Deposits { get; set; } = new();
    public List<ReservationIncidentDto> Incidents { get; set; } = new();
    public List<ReservationReminderDto> Reminders { get; set; } = new();
}

public class CreateReservationRequestDto
{
    public Guid SpaceId { get; set; }
    public Guid UnitId { get; set; }
    public Guid OwnerId { get; set; }
    public DateTime StartDateTime { get; set; }
    public DateTime EndDateTime { get; set; }
    public int EstimatedAttendees { get; set; }
    public string? EventDescription { get; set; }
    public bool HasMusic { get; set; }
    public string? MusicEndTime { get; set; }
    public bool RulesAccepted { get; set; }
}

public class ApproveReservationRequestDto
{
    public string? AdminNotes { get; set; }
}

public class RejectReservationRequestDto
{
    public string RejectionReason { get; set; } = string.Empty;
    public string? AdminNotes { get; set; }
}

public class CheckInReservationRequestDto
{
    public string? AdminNotes { get; set; }
}

public class CheckOutReservationRequestDto
{
    public string? CheckoutSignaturePath { get; set; }
    public string? AdminNotes { get; set; }
}

public class ReportIncidentRequestDto
{
    public string Description { get; set; } = string.Empty;
    public string Severity { get; set; } = "Minor";
    public decimal DamageAmount { get; set; }
    public string? EvidenceFilePath { get; set; }
}

// ── Deposit ──────────────────────────────────────────────────────

public class ReservationDepositDto
{
    public Guid Id { get; set; }
    public decimal Amount { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? PaymentMethod { get; set; }
    public string? ChargeNumber { get; set; }
    public string? ReturnChargeNumber { get; set; }
    public decimal? DamageAmount { get; set; }
    public string? DamageDescription { get; set; }
    public DateTime? PaidAt { get; set; }
    public DateTime? ReturnedAt { get; set; }
    public DateTime? AppliedAt { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class ProcessDepositPaymentRequestDto
{
    public string PaymentMethod { get; set; } = "Cash";
    public string? Notes { get; set; }
}

public class ProcessDepositReturnRequestDto
{
    public string? Notes { get; set; }
}

public class ApplyDepositToDamageRequestDto
{
    public decimal DamageAmount { get; set; }
    public string DamageDescription { get; set; } = string.Empty;
    public string? Notes { get; set; }
}

// ── Incident ─────────────────────────────────────────────────────

public class ReservationIncidentDto
{
    public Guid Id { get; set; }
    public string Description { get; set; } = string.Empty;
    public string Severity { get; set; } = string.Empty;
    public decimal DamageAmount { get; set; }
    public bool DamageAssessed { get; set; }
    public bool DepositAppliedToDamage { get; set; }
    public string? EvidenceFilePath { get; set; }
    public string ReportedByName { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}

// ── Reminder ─────────────────────────────────────────────────────

public class ReservationReminderDto
{
    public Guid Id { get; set; }
    public string ReminderType { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime ScheduledFor { get; set; }
    public DateTime? SentAt { get; set; }
    public string Channel { get; set; } = string.Empty;
    public string? RecipientEmail { get; set; }
}

// ── Availability ─────────────────────────────────────────────────

public class AvailabilityCheckDto
{
    public bool IsAvailable { get; set; }
    public string? Reason { get; set; }
    public decimal EstimatedCost { get; set; }
    public decimal DepositAmount { get; set; }
    public bool HasArrears { get; set; }
    public string? ArrearsWarning { get; set; }
}

public class AvailableSlotDto
{
    public DateTime StartDateTime { get; set; }
    public DateTime EndDateTime { get; set; }
    public double DurationHours { get; set; }
}

public class AlternativeSlotDto
{
    public DateTime StartDateTime { get; set; }
    public DateTime EndDateTime { get; set; }
    public double DurationHours { get; set; }
    public int DayDifference { get; set; }
}

// ── Report ───────────────────────────────────────────────────────

public class ReservationReportDto
{
    public Guid SpaceId { get; set; }
    public string SpaceName { get; set; } = string.Empty;
    public int TotalReservations { get; set; }
    public int CompletedReservations { get; set; }
    public int CancelledReservations { get; set; }
    public int IncidentReservations { get; set; }
    public decimal OccupancyPercentage { get; set; }
    public decimal TotalRevenue { get; set; }
    public decimal TotalDeposits { get; set; }
    public List<TopUnitDto> TopUnits { get; set; } = new();
    public List<PeakHourDto> PeakHours { get; set; } = new();
}

public class TopUnitDto
{
    public Guid UnitId { get; set; }
    public string UnitIdentifier { get; set; } = string.Empty;
    public int ReservationCount { get; set; }
}

public class PeakHourDto
{
    public int Hour { get; set; }
    public int ReservationCount { get; set; }
}

// ── Calendar ─────────────────────────────────────────────────────

public class CalendarEventDto
{
    public Guid ReservationId { get; set; }
    public string ReservationNumber { get; set; } = string.Empty;
    public string SpaceName { get; set; } = string.Empty;
    public string UnitIdentifier { get; set; } = string.Empty;
    public string OwnerName { get; set; } = string.Empty;
    public DateTime StartDateTime { get; set; }
    public DateTime EndDateTime { get; set; }
    public string Status { get; set; } = string.Empty;
    public string Color { get; set; } = string.Empty;
}
