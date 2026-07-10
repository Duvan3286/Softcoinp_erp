namespace Softcoinp.ERP.Domain.Enums;

public enum NotificationEventType
{
    PaymentConfirmed,
    NewMonthlyBillingAvailable,
    DelinquencyNotice1,
    DelinquencyNotice2,
    DelinquencyNotice3,
    PreLegalNotice,
    PeaceAndSafetyIssued,
    PQRReceived,
    PQRStatusUpdated,
    PQRResponseAvailable,
    PQRClosed,
    ReservationApproved,
    ReservationRejected,
    ReservationReminder24h,
    ReservationReminder2h,
    DepositReturned,
    AssemblyConvocation,
    AssemblyReminder72h,
    AssemblyMinutesPublished,
    MaintenanceScheduled,
    OutOfService,
    WorkOrderResolved
}
