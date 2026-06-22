using System;
using System.Collections.Generic;

namespace Softcoinp.ERP.WebAPI.DTOs;

public class CreatePqrRequestDto
{
    public string PQRType { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string Subject { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;

    public Guid UnitId { get; set; }

    public string RadiadorName { get; set; } = string.Empty;
    public string? RadiadorDocumentType { get; set; }
    public string? RadiadorDocumentNumber { get; set; }
    public string? RadiadorContact { get; set; }
    public Guid? OwnerId { get; set; }
    public Guid? TenantResidentId { get; set; }

    public string Channel { get; set; } = string.Empty;

    public Guid? RelatedPQRId { get; set; }

    public bool IsInternal { get; set; }
    public string? InvolvedResidentName { get; set; }
    public Guid? InvolvedResidentUnitId { get; set; }

    public bool IsLinkedToCharge { get; set; }
    public Guid? UnitFeeId { get; set; }
    public Guid? ExtraordinaryFeeDistributionId { get; set; }
    public Guid? IndividualChargeId { get; set; }
}

public class PqrCreatedResponseDto
{
    public Guid Id { get; set; }
    public string RadicadoNumber { get; set; } = string.Empty;
    public string PQRType { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string Subject { get; set; } = string.Empty;
    public DateTime FiledAt { get; set; }
    public DateTime? Deadline { get; set; }
    public decimal ProgressPercent { get; set; }
}

public class PqrListDto
{
    public Guid Id { get; set; }
    public string RadicadoNumber { get; set; } = string.Empty;
    public string PQRType { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string Priority { get; set; } = string.Empty;
    public string Subject { get; set; } = string.Empty;
    public string UnitIdentifier { get; set; } = string.Empty;
    public string RadiadorName { get; set; } = string.Empty;
    public DateTime FiledAt { get; set; }
    public DateTime? Deadline { get; set; }
    public int ElapsedPercent { get; set; }
    public bool IsInternal { get; set; }
}

public class PqrDetailDto
{
    public Guid Id { get; set; }
    public string RadicadoNumber { get; set; } = string.Empty;
    public string PQRType { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string Priority { get; set; } = string.Empty;
    public string Subject { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;

    public string RadiadorName { get; set; } = string.Empty;
    public string? RadiadorDocumentType { get; set; }
    public string? RadiadorDocumentNumber { get; set; }
    public string? RadiadorContact { get; set; }

    public Guid UnitId { get; set; }
    public string UnitIdentifier { get; set; } = string.Empty;

    public string Channel { get; set; } = string.Empty;
    public Guid? RelatedPQRId { get; set; }
    public string? RelatedRadicadoNumber { get; set; }

    public string? AssignedToUserId { get; set; }
    public DateTime? Deadline { get; set; }
    public int ElapsedPercent { get; set; }

    public bool IsInternal { get; set; }
    public string? InvolvedResidentName { get; set; }
    public Guid? InvolvedResidentUnitId { get; set; }

    public bool IsLinkedToCharge { get; set; }
    public bool? ClaimResolved { get; set; }
    public string? ClaimResolutionNote { get; set; }
    public bool CreditNoteGenerated { get; set; }

    public DateTime FiledAt { get; set; }
    public DateTime? ClosedAt { get; set; }
    public DateTime? ClosedDefinitivelyAt { get; set; }

    public List<PqrFollowUpDto> FollowUps { get; set; } = new();
    public List<PqrResponseDto> Responses { get; set; } = new();
    public List<PqrInternalNoteDto> InternalNotes { get; set; } = new();
    public List<PqrFileDto> Files { get; set; } = new();
    public List<PqrAlertDto> Alerts { get; set; } = new();
}

public class PqrFollowUpDto
{
    public Guid Id { get; set; }
    public string PreviousStatus { get; set; } = string.Empty;
    public string NewStatus { get; set; } = string.Empty;
    public DateTime ChangedAt { get; set; }
    public string ChangedByUserName { get; set; } = string.Empty;
    public string Justification { get; set; } = string.Empty;
    public bool IsAutomatic { get; set; }
}

public class PqrResponseDto
{
    public Guid Id { get; set; }
    public string ResponseText { get; set; } = string.Empty;
    public bool IsDefinitive { get; set; }
    public bool IsPartialUpdate { get; set; }
    public DateTime SentAt { get; set; }
    public string SentByUserName { get; set; } = string.Empty;
    public bool RequiresConfirmation { get; set; }
    public bool? ConfirmedByRadiador { get; set; }
    public DateTime? ConfirmedAt { get; set; }
    public List<PqrFileDto> Files { get; set; } = new();
}

public class PqrInternalNoteDto
{
    public Guid Id { get; set; }
    public string NoteText { get; set; } = string.Empty;
    public string AuthorName { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}

public class PqrFileDto
{
    public Guid Id { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string OriginalFileName { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public long FileSize { get; set; }
    public string UploadedByUserName { get; set; } = string.Empty;
    public DateTime UploadedAt { get; set; }
    public bool IsFromApplicant { get; set; }
}

public class PqrAlertDto
{
    public Guid Id { get; set; }
    public string AlertType { get; set; } = string.Empty;
    public DateTime GeneratedAt { get; set; }
    public bool IsActive { get; set; }
    public DateTime? ResolvedAt { get; set; }
    public bool EscalatedToCouncil { get; set; }
}

public class PqrTimeConfigDto
{
    public string PQRType { get; set; } = string.Empty;
    public int BusinessDays { get; set; }
}

public class UpdatePqrTimeConfigRequestDto
{
    public string PQRType { get; set; } = string.Empty;
    public int BusinessDays { get; set; }
}

public class ResolveClaimRequestDto
{
    public bool Resolved { get; set; }
    public string ResolutionNote { get; set; } = string.Empty;
}

public class ChangePqrStatusRequestDto
{
    public string Status { get; set; } = string.Empty;
    public string Justification { get; set; } = string.Empty;
}

public class AssignPqrRequestDto
{
    public string AssignedToUserId { get; set; } = string.Empty;
    public string AssignedToUserName { get; set; } = string.Empty;
}

public class UpdatePqrPriorityRequestDto
{
    public string Priority { get; set; } = string.Empty;
}

public class AddPqrResponseRequestDto
{
    public string ResponseText { get; set; } = string.Empty;
    public bool IsDefinitive { get; set; }
    public bool IsPartialUpdate { get; set; }
    public bool RequiresConfirmation { get; set; }
}

public class AddPqrInternalNoteRequestDto
{
    public string NoteText { get; set; } = string.Empty;
}

public class ReopenPqrRequestDto
{
    public string Justification { get; set; } = string.Empty;
}

public class ConfirmResponseRequestDto
{
    public bool Confirmed { get; set; }
}
