using System;
using System.Collections.Generic;
using Softcoinp.ERP.Domain.Enums;

namespace Softcoinp.ERP.Domain.Entities;

public class PqrRecord
{
    public Guid Id { get; set; }
    public string TenantId { get; set; } = string.Empty;

    public string RadicadoNumber { get; set; } = string.Empty;
    public PQRType PQRType { get; set; }
    public PQRCategory Category { get; set; }
    public PQRStatus Status { get; set; } = PQRStatus.Filed;
    public PQRPriority Priority { get; set; } = PQRPriority.Medium;

    public string Subject { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;

    // Radiador (applicant) information
    public string RadiadorName { get; set; } = string.Empty;
    public string? RadiadorDocumentType { get; set; }
    public string? RadiadorDocumentNumber { get; set; }
    public string? RadiadorContact { get; set; }

    // Links to registered entities (optional – null if applicant is not registered)
    public Guid? OwnerId { get; set; }
    public Owner? Owner { get; set; }

    public Guid? TenantResidentId { get; set; }
    public TenantResident? TenantResident { get; set; }

    // Unit from which the PQR originates
    public Guid UnitId { get; set; }
    public Unit? Unit { get; set; }

    public PQRChannel Channel { get; set; }

    // Self-reference for related PQRs
    public Guid? RelatedPQRId { get; set; }
    public PqrRecord? RelatedPQR { get; set; }

    // Internal management
    public string? AssignedToUserId { get; set; }
    public DateTime? Deadline { get; set; }

    // Complaint-specific: involved resident (confidential)
    public string? InvolvedResidentName { get; set; }
    public Guid? InvolvedResidentUnitId { get; set; }
    public Unit? InvolvedResidentUnit { get; set; }

    public bool IsInternal { get; set; }

    // Claim linking to charges
    public bool IsLinkedToCharge { get; set; }
    public Guid? UnitFeeId { get; set; }
    public UnitFee? UnitFee { get; set; }
    public Guid? ExtraordinaryFeeDistributionId { get; set; }
    public ExtraordinaryFeeDistribution? ExtraordinaryFeeDistribution { get; set; }
    public Guid? IndividualChargeId { get; set; }
    public IndividualCharge? IndividualCharge { get; set; }

    // Claim resolution
    public bool? ClaimResolved { get; set; }
    public string? ClaimResolutionNote { get; set; }
    public bool CreditNoteGenerated { get; set; }

    // Dates
    public DateTime FiledAt { get; set; } = DateTime.UtcNow;
    public DateTime? ClosedAt { get; set; }
    public DateTime? ClosedDefinitivelyAt { get; set; }

    // Audit
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string CreatedByUserId { get; set; } = string.Empty;
    public DateTime? UpdatedAt { get; set; }

    // Navigation collections
    public ICollection<PqrFollowUp> FollowUps { get; set; } = new List<PqrFollowUp>();
    public ICollection<PqrResponse> Responses { get; set; } = new List<PqrResponse>();
    public ICollection<PqrInternalNote> InternalNotes { get; set; } = new List<PqrInternalNote>();
    public ICollection<PqrFile> Files { get; set; } = new List<PqrFile>();
    public ICollection<PqrAlert> Alerts { get; set; } = new List<PqrAlert>();
}
