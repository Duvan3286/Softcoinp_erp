using System;
using System.Collections.Generic;
using Softcoinp.ERP.Domain.Enums;

namespace Softcoinp.ERP.Domain.Entities;

public class Owner
{
    public Guid Id { get; set; }
    public string TenantId { get; set; } = string.Empty;

    public OwnerType OwnerType { get; set; }
    public DocumentType DocumentType { get; set; }
    public string DocumentNumber { get; set; } = string.Empty;
    public string? VerificationDigit { get; set; }

    public string FullNameOrCompanyName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string MainPhone { get; set; } = string.Empty;
    public string? AlternativePhone { get; set; }
    public string? CorrespondenceAddress { get; set; }

    // Natural Person specific
    public DateTime? DateOfBirth { get; set; }
    public string? CivilStatus { get; set; }

    // Legal Entity specific
    public string? LegalRepresentativeName { get; set; }
    public DocumentType? LegalRepresentativeDocumentType { get; set; }
    public string? LegalRepresentativeDocument { get; set; }
    public string? LegalRepresentativeRole { get; set; }
    public DateTime? PowerOfAttorneyExpiration { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    public string CreatedByUserId { get; set; } = string.Empty;

    // Navigation properties
    public ICollection<UnitOwner> UnitOwners { get; set; } = new List<UnitOwner>();
    public ICollection<ContactHistory> ContactHistories { get; set; } = new List<ContactHistory>();
    public ICollection<OwnerHistory> OwnerHistories { get; set; } = new List<OwnerHistory>();
    public ICollection<SpokespersonHistory> SpokespersonHistoriesAsPrevious { get; set; } = new List<SpokespersonHistory>();
    public ICollection<SpokespersonHistory> SpokespersonHistoriesAsNew { get; set; } = new List<SpokespersonHistory>();

    // Navigation properties for PQR module
    public ICollection<PqrRecord> PqrRecords { get; set; } = new List<PqrRecord>();
}
