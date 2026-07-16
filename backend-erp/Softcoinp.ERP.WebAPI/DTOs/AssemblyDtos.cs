using System;
using System.Collections.Generic;

namespace Softcoinp.ERP.WebAPI.DTOs;

// ── Assembly ──────────────────────────────────────────────────────

public class AssemblyListDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string ParticipationType { get; set; } = string.Empty;
    public DateTime ScheduledDate { get; set; }
    public string ScheduledTime { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public decimal TotalCoefficients { get; set; }
    public decimal QuorumThresholdFirstCall { get; set; }
    public bool QuorumAchievedFirstCall { get; set; }
    public bool QuorumAchievedSecondCall { get; set; }
    public int ConvocationNumber { get; set; }
    public int AttendanceCount { get; set; }
    public int AgendaItemCount { get; set; }
    public int ApprovedItemsCount { get; set; }
    public string? PresidentName { get; set; }
    public string? SecretaryName { get; set; }
    public string CreatedByUserId { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}

public class AssemblyDetailDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Type { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string ParticipationType { get; set; } = string.Empty;
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
    public int ConvocationNumber { get; set; }
    public DateTime? SessionStartTime { get; set; }
    public DateTime? SessionEndTime { get; set; }
    public string? PresidentName { get; set; }
    public string? SecretaryName { get; set; }
    public string? PresidentOwnerId { get; set; }
    public string? SecretaryOwnerId { get; set; }
    public string? ConvocationSentAt { get; set; }
    public bool ConvocationDeadlineMet { get; set; }
    public string CreatedByUserId { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public List<AssemblyConvocationDto> Convocations { get; set; } = new();
    public List<AssemblyAgendaItemDto> AgendaItems { get; set; } = new();
    public List<AssemblyAttendanceDto> Attendances { get; set; } = new();
    public List<AssemblyConstancyDto> Constancies { get; set; } = new();
    public AssemblyMinutesDto? Minutes { get; set; }
}

public class CreateAssemblyRequestDto
{
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Type { get; set; } = "Ordinary";
    public string ParticipationType { get; set; } = "InPerson";
    public DateTime ScheduledDate { get; set; }
    public string ScheduledTime { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public DateTime? SecondConvocationDate { get; set; }
    public string? SecondConvocationTime { get; set; }
    public string? SecondConvocationLocation { get; set; }
}

public class UpdateAssemblyRequestDto
{
    public string? Title { get; set; }
    public string? Description { get; set; }
    public string? ParticipationType { get; set; }
    public DateTime? ScheduledDate { get; set; }
    public string? ScheduledTime { get; set; }
    public string? Location { get; set; }
    public DateTime? SecondConvocationDate { get; set; }
    public string? SecondConvocationTime { get; set; }
    public string? SecondConvocationLocation { get; set; }
}

public class UpdateSessionRequestDto
{
    public string? PresidentName { get; set; }
    public string? PresidentOwnerId { get; set; }
    public string? SecretaryName { get; set; }
    public string? SecretaryOwnerId { get; set; }
    public int? ConvocationNumber { get; set; }
}

public class StartSessionRequestDto
{
    public int ConvocationNumber { get; set; } = 1;
    public string? PresidentName { get; set; }
    public string? PresidentOwnerId { get; set; }
    public string? SecretaryName { get; set; }
    public string? SecretaryOwnerId { get; set; }
}

// ── Convocation ──────────────────────────────────────────────────

public class AssemblyConvocationDto
{
    public Guid Id { get; set; }
    public int ConvocationNumber { get; set; }
    public string Subject { get; set; } = string.Empty;
    public string? Notes { get; set; }
    public string Channel { get; set; } = string.Empty;
    public DateTime? SentAt { get; set; }
    public int TotalRecipients { get; set; }
    public int DeliveredCount { get; set; }
    public int FailedCount { get; set; }
    public DateTime CreatedAt { get; set; }
    public List<ConvocationDocumentDto> Documents { get; set; } = new();
    public List<ConvocationRecipientDto> Recipients { get; set; } = new();
}

public class CreateConvocationRequestDto
{
    public int ConvocationNumber { get; set; } = 1;
    public string Subject { get; set; } = string.Empty;
    public string? Notes { get; set; }
    public string Channel { get; set; } = "Email";
    public List<ConvocationDocumentInputDto>? Documents { get; set; }
}

public class ConvocationDocumentDto
{
    public Guid Id { get; set; }
    public string DocumentName { get; set; } = string.Empty;
    public string DocumentType { get; set; } = string.Empty;
    public string FilePath { get; set; } = string.Empty;
    public string? Description { get; set; }
}

public class ConvocationDocumentInputDto
{
    public string DocumentName { get; set; } = string.Empty;
    public string DocumentType { get; set; } = string.Empty;
    public string FilePath { get; set; } = string.Empty;
    public string? Description { get; set; }
}

public class ConvocationRecipientDto
{
    public Guid Id { get; set; }
    public Guid UnitId { get; set; }
    public string UnitIdentifier { get; set; } = string.Empty;
    public Guid OwnerId { get; set; }
    public string OwnerName { get; set; } = string.Empty;
    public string OwnerEmail { get; set; } = string.Empty;
    public string? OwnerPhone { get; set; }
    public bool Delivered { get; set; }
    public DateTime? DeliveredAt { get; set; }
    public string? DeliveryError { get; set; }
}

public class SendConvocationRequestDto
{
    public string Channel { get; set; } = "Email";
}

// ── Attendance ───────────────────────────────────────────────────

public class AssemblyAttendanceDto
{
    public Guid Id { get; set; }
    public Guid UnitId { get; set; }
    public string UnitIdentifier { get; set; } = string.Empty;
    public Guid OwnerId { get; set; }
    public string OwnerName { get; set; } = string.Empty;
    public decimal Coefficient { get; set; }
    public string Status { get; set; } = string.Empty;
    public bool AttendsPersonally { get; set; }
    public Guid? RepresentativeOwnerId { get; set; }
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
}

public class RegisterAttendanceRequestDto
{
    public Guid UnitId { get; set; }
    public Guid OwnerId { get; set; }
    public bool AttendsPersonally { get; set; } = true;
    public Guid? RepresentativeOwnerId { get; set; }
    public string? RepresentativeName { get; set; }
    public string? RepresentativeDocumentNumber { get; set; }
    public string? PowerOfAttorneyFilePath { get; set; }
    public bool IsCommissionMember { get; set; }
    public string? CommissionRole { get; set; }
    public string? Notes { get; set; }
}

public class UpdateAttendanceRequestDto
{
    public string? Status { get; set; }
    public DateTime? DepartureTime { get; set; }
    public string? Notes { get; set; }
}

public class LiftVotingRestrictionRequestDto
{
    public string Reason { get; set; } = string.Empty;
}

// ── Agenda Item ──────────────────────────────────────────────────

public class AssemblyAgendaItemDto
{
    public Guid Id { get; set; }
    public int SequenceNumber { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? PresenterName { get; set; }
    public string MajorityRequired { get; set; } = string.Empty;
    public string VotingMode { get; set; } = string.Empty;
    public bool IsInformationOnly { get; set; }
    public bool RequiresVoting { get; set; }
    public decimal TotalCoefficientsForVote { get; set; }
    public decimal VotesInFavorCoefficients { get; set; }
    public decimal VotesAgainstCoefficients { get; set; }
    public decimal AbstentionCoefficients { get; set; }
    public int VotesInFavorCount { get; set; }
    public int VotesAgainstCount { get; set; }
    public int AbstentionCount { get; set; }
    public bool? IsApproved { get; set; }
    public string? RejectionReason { get; set; }
    public string? Observations { get; set; }
    public string? OwnerNotes { get; set; }
    public bool VoteRegistered { get; set; }
    public string? RegisteredByUserId { get; set; }
    public DateTime? VoteRegisteredAt { get; set; }
    public string? PropagationTarget { get; set; }
    public Guid? TargetBudgetId { get; set; }
}

public class CreateAgendaItemRequestDto
{
    public int SequenceNumber { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? PresenterName { get; set; }
    public string MajorityRequired { get; set; } = "Simple";
    public string VotingMode { get; set; } = "Public";
    public bool IsInformationOnly { get; set; }
    public bool RequiresVoting { get; set; } = true;

    // Propagación automática opcional: "ExtraordinaryFee" o "Budget"
    public string? PropagationTarget { get; set; }

    // Requerido si PropagationTarget == "ExtraordinaryFee"
    public decimal? ExtraordinaryFeeTotalAmount { get; set; }
    public int? ExtraordinaryFeeInstallments { get; set; }
    public string? ExtraordinaryFeeStartPeriod { get; set; }
    public DateTime? ExtraordinaryFeeDueDate { get; set; }
    public string? ExtraordinaryFeeDistributionType { get; set; }

    // Requerido si PropagationTarget == "Budget": Id de un presupuesto en estado Draft
    public Guid? TargetBudgetId { get; set; }
}

public class UpdateAgendaItemRequestDto
{
    public string? Title { get; set; }
    public string? Description { get; set; }
    public string? PresenterName { get; set; }
    public string? MajorityRequired { get; set; }
    public string? VotingMode { get; set; }
    public bool? IsInformationOnly { get; set; }
    public bool? RequiresVoting { get; set; }
}

public class RegisterVoteRequestDto
{
    public decimal VotesInFavorCoefficients { get; set; }
    public decimal VotesAgainstCoefficients { get; set; }
    public decimal AbstentionCoefficients { get; set; }
    public int VotesInFavorCount { get; set; }
    public int VotesAgainstCount { get; set; }
    public int AbstentionCount { get; set; }
    public string? Observations { get; set; }
    public string? OwnerNotes { get; set; }
}

// ── Constancy ────────────────────────────────────────────────────

public class AssemblyConstancyDto
{
    public Guid Id { get; set; }
    public Guid? AgendaItemId { get; set; }
    public string? AgendaItemTitle { get; set; }
    public Guid OwnerId { get; set; }
    public string OwnerName { get; set; } = string.Empty;
    public string Text { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}

public class CreateConstancyRequestDto
{
    public Guid? AgendaItemId { get; set; }
    public Guid OwnerId { get; set; }
    public string OwnerName { get; set; } = string.Empty;
    public string Text { get; set; } = string.Empty;
}

// ── Minutes ──────────────────────────────────────────────────────

public class AssemblyMinutesDto
{
    public Guid Id { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? PresidentName { get; set; }
    public string? SecretaryName { get; set; }
    public string FullText { get; set; } = string.Empty;
    public DateTime GeneratedAt { get; set; }
    public string? CommissionMemberNames { get; set; }
    public DateTime? CommissionReviewDeadline { get; set; }
    public string? CommissionComments { get; set; }
    public string? PresidentSignatureFilePath { get; set; }
    public string? SecretarySignatureFilePath { get; set; }
    public DateTime? ApprovedAt { get; set; }
    public DateTime? PublishedAt { get; set; }
    public int? PublishNotificationCount { get; set; }
    public string? RevisionNotes { get; set; }
}

public class GenerateMinutesRequestDto
{
    public string? PresidentName { get; set; }
    public string? SecretaryName { get; set; }
    public string? CommissionMemberNames { get; set; }
}

public class ApproveMinutesRequestDto
{
    public string? PresidentSignatureFilePath { get; set; }
    public string? SecretarySignatureFilePath { get; set; }
    public string? CommissionComments { get; set; }
}

// ── Decision Propagation ─────────────────────────────────────────

public class AssemblyDecisionPropagationDto
{
    public Guid Id { get; set; }
    public Guid AgendaItemId { get; set; }
    public string AgendaItemTitle { get; set; } = string.Empty;
    public string TargetModule { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string? TargetEntityId { get; set; }
    public string? TargetEntityType { get; set; }
    public string? ErrorMessage { get; set; }
    public int RetryCount { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? PropagatedAt { get; set; }
}

public class CreateDecisionPropagationRequestDto
{
    public Guid AgendaItemId { get; set; }
    public string TargetModule { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
}

// ── Quorum ───────────────────────────────────────────────────────

public class QuorumStatusDto
{
    public decimal TotalCoefficients { get; set; }
    public decimal PresentCoefficients { get; set; }
    public decimal QuorumThresholdFirstCall { get; set; }
    public decimal QuorumThresholdSecondCall { get; set; }
    public bool FirstCallQuorumMet { get; set; }
    public bool SecondCallQuorumMet { get; set; }
    public decimal PercentagePresent { get; set; }
    public int TotalOwners { get; set; }
    public int PresentOwners { get; set; }
    public int AbsentOwners { get; set; }
    public int OwnersWithArrears { get; set; }
    public int OwnersWithRestrictedVoting { get; set; }
}

// ── Report ───────────────────────────────────────────────────────

public class AssemblyReportDto
{
    public int TotalAssemblies { get; set; }
    public int OrdinaryAssemblies { get; set; }
    public int ExtraordinaryAssemblies { get; set; }
    public int PublishedAssemblies { get; set; }
    public int PendingMinutesAssemblies { get; set; }
    public DateTime? NextScheduledAssembly { get; set; }
    public string? NextAssemblyTitle { get; set; }
}
