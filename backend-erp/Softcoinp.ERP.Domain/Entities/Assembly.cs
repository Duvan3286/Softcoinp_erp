using System;
using System.Collections.Generic;
using Softcoinp.ERP.Domain.Common;
using Softcoinp.ERP.Domain.Enums;

namespace Softcoinp.ERP.Domain.Entities;

public class Assembly : BaseEntity
{
    public string TenantId { get; set; } = string.Empty;

    public AssemblyType Type { get; set; } = AssemblyType.Ordinary;
    public AssemblyStatus Status { get; set; } = AssemblyStatus.Draft;
    public AssemblyParticipationType ParticipationType { get; set; } = AssemblyParticipationType.InPerson;

    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }

    public DateTime ScheduledDate { get; set; }
    public string ScheduledTime { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;

    public DateTime? SecondConvocationDate { get; set; }
    public string? SecondConvocationTime { get; set; }
    public string? SecondConvocationLocation { get; set; }

    public decimal TotalCoefficients { get; set; }
    public decimal QuorumThresholdFirstCall { get; set; }
    public decimal QuorumThresholdSecondCall { get; set; }

    public bool QuorumAchievedFirstCall { get; set; }
    public bool QuorumAchievedSecondCall { get; set; }
    public int ConvocationNumber { get; set; } = 1;

    public DateTime? SessionStartTime { get; set; }
    public DateTime? SessionEndTime { get; set; }
    public string? ActNumber { get; set; }

    public string? PresidentName { get; set; }
    public string? SecretaryName { get; set; }
    public string? PresidentOwnerId { get; set; }
    public string? SecretaryOwnerId { get; set; }

    public string? ConvocationSentAt { get; set; }
    public bool ConvocationDeadlineMet { get; set; }

    public string CreatedByUserId { get; set; } = string.Empty;
    public string? UpdatedByUserId { get; set; }

    public ICollection<AssemblyConvocation> Convocations { get; set; } = new List<AssemblyConvocation>();
    public ICollection<AssemblyAttendance> Attendances { get; set; } = new List<AssemblyAttendance>();
    public ICollection<AssemblyAgendaItem> AgendaItems { get; set; } = new List<AssemblyAgendaItem>();
    public ICollection<AssemblyConstancy> Constancies { get; set; } = new List<AssemblyConstancy>();
    public ICollection<AssemblyMinutes> Minutes { get; set; } = new List<AssemblyMinutes>();
    public ICollection<AssemblyDecisionPropagation> DecisionPropagations { get; set; } = new List<AssemblyDecisionPropagation>();
}
