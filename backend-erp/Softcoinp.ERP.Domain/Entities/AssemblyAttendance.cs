using System;
using Softcoinp.ERP.Domain.Enums;

namespace Softcoinp.ERP.Domain.Entities;

public class AssemblyAttendance
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string TenantId { get; set; } = string.Empty;

    public Guid AssemblyId { get; set; }
    public Assembly? Assembly { get; set; }

    public Guid UnitId { get; set; }
    public Unit? Unit { get; set; }

    public Guid OwnerId { get; set; }
    public Owner? Owner { get; set; }

    public decimal Coefficient { get; set; }

    public AttendanceStatus Status { get; set; } = AttendanceStatus.Absent;
    public bool AttendsPersonally { get; set; } = true;

    public Guid? RepresentativeOwnerId { get; set; }
    public Owner? RepresentativeOwner { get; set; }
    public string? RepresentativeName { get; set; }
    public string? RepresentativeDocumentNumber { get; set; }
    public string? PowerOfAttorneyFilePath { get; set; }

    public DateTime ArrivalTime { get; set; }
    public DateTime? DepartureTime { get; set; }

    public bool HasDuesArrears { get; set; }
    public bool VotingRightRestricted { get; set; }
    public string? VotingRestrictionReason { get; set; }
    public string? VotingRestrictionLiftedByUserId { get; set; }
    public string? VotingRestrictionLiftedReason { get; set; }
    public DateTime? VotingRestrictionLiftedAt { get; set; }

    public bool IsCommissionMember { get; set; }
    public string? CommissionRole { get; set; }

    public string? Notes { get; set; }
    public string RegisteredByUserId { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
