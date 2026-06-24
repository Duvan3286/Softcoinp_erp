using System;
using System.Collections.Generic;
using Softcoinp.ERP.Domain.Common;
using Softcoinp.ERP.Domain.Enums;

namespace Softcoinp.ERP.Domain.Entities;

public class ReservableSpace : BaseEntity
{
    public string TenantId { get; set; } = string.Empty;

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
    public ChargeType ChargeType { get; set; } = ChargeType.PerHour;
    public decimal HourlyRate { get; set; }
    public decimal EventRate { get; set; }

    public ApprovalMode ApprovalMode { get; set; } = ApprovalMode.Automatic;

    public ArrearsPolicy ArrearsPolicy { get; set; } = ArrearsPolicy.Warn;

    public bool IsAvailableForMaintenance { get; set; } = true;

    public bool IsActive { get; set; } = true;

    public string? RulesFilePath { get; set; }
    public string? ImageFilePath { get; set; }

    public string CreatedByUserId { get; set; } = string.Empty;
    public string? UpdatedByUserId { get; set; }

    public ICollection<SpaceSchedule> Schedules { get; set; } = new List<SpaceSchedule>();
    public ICollection<SpaceBlock> Blocks { get; set; } = new List<SpaceBlock>();
    public ICollection<Reservation> Reservations { get; set; } = new List<Reservation>();
}
