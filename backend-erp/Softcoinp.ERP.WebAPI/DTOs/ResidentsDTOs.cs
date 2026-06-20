using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Softcoinp.ERP.Domain.Enums;

namespace Softcoinp.ERP.WebAPI.DTOs;

// ── READ DTOs ────────────────────────────────────────────────────────────────

public class OwnerSummaryDto
{
    public Guid Id { get; set; }
    public string OwnerType { get; set; } = string.Empty;
    public string DocumentType { get; set; } = string.Empty;
    public string DocumentNumber { get; set; } = string.Empty;
    public string FullNameOrCompanyName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string MainPhone { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public List<UnitOwnerSummaryDto> Units { get; set; } = new();
}

public class UnitOwnerSummaryDto
{
    public Guid AssignmentId { get; set; }
    public Guid UnitId { get; set; }
    public string UnitIdentifier { get; set; } = string.Empty;
    public Guid OwnerId { get; set; }
    public string OwnerName { get; set; } = string.Empty;
    public string OwnerDocumentNumber { get; set; } = string.Empty;
    public string OwnerDocumentType { get; set; } = string.Empty;
    public decimal OwnershipPercentage { get; set; }
    public bool IsSpokesperson { get; set; }
    public bool ResidesInUnit { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
}

public class OwnerDetailDto
{
    public Guid Id { get; set; }
    public string OwnerType { get; set; } = string.Empty;
    public string DocumentType { get; set; } = string.Empty;
    public string DocumentNumber { get; set; } = string.Empty;
    public string? VerificationDigit { get; set; }
    public string FullNameOrCompanyName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string MainPhone { get; set; } = string.Empty;
    public string? AlternativePhone { get; set; }
    public string? CorrespondenceAddress { get; set; }
    public DateTime? DateOfBirth { get; set; }
    public string? CivilStatus { get; set; }
    public string? LegalRepresentativeName { get; set; }
    public string? LegalRepresentativeDocumentType { get; set; }
    public string? LegalRepresentativeDocument { get; set; }
    public string? LegalRepresentativeRole { get; set; }
    public DateTime? PowerOfAttorneyExpiration { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public List<UnitOwnerSummaryDto> Units { get; set; } = new();
    public List<ContactHistoryDto> ContactHistory { get; set; } = new();
}

public class ContactHistoryDto
{
    public Guid Id { get; set; }
    public string FieldChanged { get; set; } = string.Empty;
    public string? OldValue { get; set; }
    public string? NewValue { get; set; }
    public DateTime ChangedAt { get; set; }
    public string ChangedByUserId { get; set; } = string.Empty;
}

public class TenantResidentDto
{
    public Guid Id { get; set; }
    public Guid UnitId { get; set; }
    public string UnitIdentifier { get; set; } = string.Empty;
    public string DocumentType { get; set; } = string.Empty;
    public string DocumentNumber { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public DateTime LeaseStartDate { get; set; }
    public DateTime? LeaseEndDate { get; set; }
    public string? RealEstateAgentName { get; set; }
    public string? RealEstateAgentPhone { get; set; }
    public bool AuthorizedToPayAdmin { get; set; }
    public bool IsActive { get; set; }
    public int? DaysUntilLeaseExpires { get; set; }
}

public class TenantResidentListItemDto
{
    public Guid Id { get; set; }
    public Guid UnitId { get; set; }
    public string UnitIdentifier { get; set; } = string.Empty;
    public string DocumentType { get; set; } = string.Empty;
    public string DocumentNumber { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public DateTime LeaseStartDate { get; set; }
    public DateTime? LeaseEndDate { get; set; }
    public bool AuthorizedToPayAdmin { get; set; }
    public bool IsActive { get; set; }
    public int? DaysUntilLeaseExpires { get; set; }
}

public class CohabitationMemberDto
{
    public Guid Id { get; set; }
    public string FullNameOrPetName { get; set; } = string.Empty;
    public string Relationship { get; set; } = string.Empty;
    public DateTime? DateOfBirth { get; set; }
    public bool IsMinor { get; set; }
    public bool IsPet { get; set; }
    public string? PetSpecies { get; set; }
    public string? PetBreed { get; set; }
    public string? PetSanitaryRegistration { get; set; }
    public bool IsActive { get; set; }
}

public class OwnerHistoryEntryDto
{
    public Guid Id { get; set; }
    public Guid OwnerId { get; set; }
    public string OwnerName { get; set; } = string.Empty;
    public string OwnerDocument { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public string? TransferNotes { get; set; }
    public DateTime RecordedAt { get; set; }
}

public class SpokespersonHistoryDto
{
    public Guid Id { get; set; }
    public string? PreviousSpokespersonName { get; set; }
    public string NewSpokespersonName { get; set; } = string.Empty;
    public DateTime ChangedAt { get; set; }
    public string ChangedByUserId { get; set; } = string.Empty;
    public string? ChangeReason { get; set; }
}

public class UnitOccupantsDto
{
    public Guid UnitId { get; set; }
    public string UnitIdentifier { get; set; } = string.Empty;
    public List<UnitOwnerSummaryDto> ActiveOwners { get; set; } = new();
    public TenantResidentDto? ActiveTenant { get; set; }
    public List<CohabitationMemberDto> CohabitationMembers { get; set; } = new();
    public string? SpokespersonName { get; set; }
    public Guid? SpokespersonOwnerId { get; set; }
}

// ── CREATE / UPDATE DTOs ─────────────────────────────────────────────────────

public class CreateNaturalPersonOwnerDto : IValidatableObject
{
    [Required(ErrorMessage = "El tipo de documento es obligatorio.")]
    public DocumentType DocumentType { get; set; }

    [Required(ErrorMessage = "El número de documento es obligatorio.")]
    [MaxLength(50, ErrorMessage = "El número de documento no puede superar 50 caracteres.")]
    public string DocumentNumber { get; set; } = string.Empty;

    [Required(ErrorMessage = "El nombre completo es obligatorio.")]
    [MaxLength(300, ErrorMessage = "El nombre no puede superar 300 caracteres.")]
    public string FullName { get; set; } = string.Empty;

    [Required(ErrorMessage = "El correo electrónico es obligatorio.")]
    [EmailAddress(ErrorMessage = "El formato del correo electrónico no es válido.")]
    [MaxLength(256)]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "El teléfono principal es obligatorio.")]
    [MaxLength(20, ErrorMessage = "El teléfono no puede superar 20 caracteres.")]
    public string MainPhone { get; set; } = string.Empty;

    [MaxLength(20)]
    public string? AlternativePhone { get; set; }

    [MaxLength(500)]
    public string? CorrespondenceAddress { get; set; }

    public DateTime? DateOfBirth { get; set; }

    [MaxLength(30)]
    public string? CivilStatus { get; set; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (DocumentType == DocumentType.NIT)
        {
            yield return new ValidationResult(
                "Una persona natural no puede usar NIT como tipo de documento.",
                new[] { nameof(DocumentType) });
        }

        if (DateOfBirth.HasValue && DateOfBirth.Value > DateTime.UtcNow)
        {
            yield return new ValidationResult(
                "La fecha de nacimiento no puede ser una fecha futura.",
                new[] { nameof(DateOfBirth) });
        }
    }
}

public class UpdateNaturalPersonOwnerDto : IValidatableObject
{
    [Required(ErrorMessage = "El nombre completo es obligatorio.")]
    [MaxLength(300)]
    public string FullName { get; set; } = string.Empty;

    [Required(ErrorMessage = "El correo electrónico es obligatorio.")]
    [EmailAddress]
    [MaxLength(256)]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "El teléfono principal es obligatorio.")]
    [MaxLength(20)]
    public string MainPhone { get; set; } = string.Empty;

    [MaxLength(20)]
    public string? AlternativePhone { get; set; }

    [MaxLength(500)]
    public string? CorrespondenceAddress { get; set; }

    public DateTime? DateOfBirth { get; set; }

    [MaxLength(30)]
    public string? CivilStatus { get; set; }

    [Required(ErrorMessage = "La razón del cambio es obligatoria para registrar la trazabilidad.")]
    [MaxLength(500)]
    public string ChangeReason { get; set; } = string.Empty;

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (DateOfBirth.HasValue && DateOfBirth.Value > DateTime.UtcNow)
        {
            yield return new ValidationResult(
                "La fecha de nacimiento no puede ser una fecha futura.",
                new[] { nameof(DateOfBirth) });
        }
    }
}

public class CreateLegalEntityOwnerDto
{
    [Required(ErrorMessage = "El NIT es obligatorio.")]
    [MaxLength(50, ErrorMessage = "El NIT no puede superar 50 caracteres.")]
    public string DocumentNumber { get; set; } = string.Empty;

    [Required(ErrorMessage = "El dígito de verificación es obligatorio.")]
    [MaxLength(2)]
    public string VerificationDigit { get; set; } = string.Empty;

    [Required(ErrorMessage = "La razón social es obligatoria.")]
    [MaxLength(300, ErrorMessage = "La razón social no puede superar 300 caracteres.")]
    public string CompanyName { get; set; } = string.Empty;

    [Required(ErrorMessage = "El correo electrónico corporativo es obligatorio.")]
    [EmailAddress(ErrorMessage = "El formato del correo electrónico no es válido.")]
    [MaxLength(256)]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "El teléfono principal es obligatorio.")]
    [MaxLength(20)]
    public string MainPhone { get; set; } = string.Empty;

    [MaxLength(20)]
    public string? AlternativePhone { get; set; }

    [MaxLength(500)]
    public string? FiscalAddress { get; set; }

    [Required(ErrorMessage = "El nombre del representante legal es obligatorio.")]
    [MaxLength(300)]
    public string LegalRepresentativeName { get; set; } = string.Empty;

    [Required(ErrorMessage = "El tipo de documento del representante legal es obligatorio.")]
    public DocumentType LegalRepresentativeDocumentType { get; set; }

    [Required(ErrorMessage = "El documento del representante legal es obligatorio.")]
    [MaxLength(50)]
    public string LegalRepresentativeDocument { get; set; } = string.Empty;

    [Required(ErrorMessage = "El cargo del representante legal es obligatorio.")]
    [MaxLength(100)]
    public string LegalRepresentativeRole { get; set; } = string.Empty;

    public DateTime? PowerOfAttorneyExpiration { get; set; }
}

public class UpdateLegalEntityOwnerDto
{
    [Required]
    [EmailAddress]
    [MaxLength(256)]
    public string Email { get; set; } = string.Empty;

    [Required]
    [MaxLength(20)]
    public string MainPhone { get; set; } = string.Empty;

    [MaxLength(20)]
    public string? AlternativePhone { get; set; }

    [MaxLength(500)]
    public string? FiscalAddress { get; set; }

    [Required]
    [MaxLength(300)]
    public string LegalRepresentativeName { get; set; } = string.Empty;

    [Required]
    public DocumentType LegalRepresentativeDocumentType { get; set; }

    [Required]
    [MaxLength(50)]
    public string LegalRepresentativeDocument { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    public string LegalRepresentativeRole { get; set; } = string.Empty;

    public DateTime? PowerOfAttorneyExpiration { get; set; }

    [Required(ErrorMessage = "La razón del cambio es obligatoria para registrar la trazabilidad.")]
    [MaxLength(500)]
    public string ChangeReason { get; set; } = string.Empty;
}

public class DeactivateOwnerDto
{
    [Required(ErrorMessage = "La fecha de salida es obligatoria.")]
    public DateTime ExitDate { get; set; }

    [Required(ErrorMessage = "El motivo de inactivación es obligatorio.")]
    [MaxLength(500)]
    public string Reason { get; set; } = string.Empty;
}

public class AssignOwnerToUnitDto : IValidatableObject
{
    [Required(ErrorMessage = "El ID del propietario es obligatorio.")]
    public Guid OwnerId { get; set; }

    [Required]
    [Range(0.01, 100.0, ErrorMessage = "El porcentaje de copropiedad debe estar entre 0.01 y 100.")]
    public decimal OwnershipPercentage { get; set; }

    public bool IsSpokesperson { get; set; }
    public bool ResidesInUnit { get; set; }

    [Required(ErrorMessage = "La fecha de inicio es obligatoria.")]
    public DateTime StartDate { get; set; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (StartDate > DateTime.UtcNow.AddDays(1))
        {
            yield return new ValidationResult(
                "La fecha de inicio no puede ser más de un día en el futuro.",
                new[] { nameof(StartDate) });
        }
    }
}

public class DesignateSpokespersonDto
{
    [Required(ErrorMessage = "El ID del propietario a designar como vocero es obligatorio.")]
    public Guid OwnerId { get; set; }

    [MaxLength(500)]
    public string? Reason { get; set; }
}

public class RemoveOwnerFromUnitDto
{
    [Required(ErrorMessage = "La fecha de salida es obligatoria.")]
    public DateTime EndDate { get; set; }

    [MaxLength(500)]
    public string? Notes { get; set; }
}

public class CreateTenantResidentDto : IValidatableObject
{
    [Required(ErrorMessage = "El tipo de documento es obligatorio.")]
    public DocumentType DocumentType { get; set; }

    [Required(ErrorMessage = "El número de documento es obligatorio.")]
    [MaxLength(50)]
    public string DocumentNumber { get; set; } = string.Empty;

    [Required(ErrorMessage = "El nombre completo es obligatorio.")]
    [MaxLength(300)]
    public string FullName { get; set; } = string.Empty;

    [Required(ErrorMessage = "El correo electrónico es obligatorio.")]
    [EmailAddress(ErrorMessage = "El formato del correo electrónico no es válido.")]
    [MaxLength(256)]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "El teléfono es obligatorio.")]
    [MaxLength(20)]
    public string Phone { get; set; } = string.Empty;

    [Required(ErrorMessage = "La fecha de inicio del arrendamiento es obligatoria.")]
    public DateTime LeaseStartDate { get; set; }

    public DateTime? LeaseEndDate { get; set; }

    [MaxLength(200)]
    public string? RealEstateAgentName { get; set; }

    [MaxLength(20)]
    public string? RealEstateAgentPhone { get; set; }

    public bool AuthorizedToPayAdmin { get; set; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (DocumentType == DocumentType.NIT)
        {
            yield return new ValidationResult(
                "Un arrendatario debe ser una persona natural. No se permite NIT.",
                new[] { nameof(DocumentType) });
        }

        if (LeaseEndDate.HasValue && LeaseEndDate.Value <= LeaseStartDate)
        {
            yield return new ValidationResult(
                "La fecha de terminación del contrato debe ser posterior a la fecha de inicio.",
                new[] { nameof(LeaseEndDate) });
        }
    }
}

public class UpdateTenantResidentDto : IValidatableObject
{
    [Required]
    [EmailAddress]
    [MaxLength(256)]
    public string Email { get; set; } = string.Empty;

    [Required]
    [MaxLength(20)]
    public string Phone { get; set; } = string.Empty;

    public DateTime? LeaseEndDate { get; set; }

    [MaxLength(200)]
    public string? RealEstateAgentName { get; set; }

    [MaxLength(20)]
    public string? RealEstateAgentPhone { get; set; }

    public bool AuthorizedToPayAdmin { get; set; }

    [Required]
    public DateTime LeaseStartDate { get; set; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (LeaseEndDate.HasValue && LeaseEndDate.Value <= LeaseStartDate)
        {
            yield return new ValidationResult(
                "La fecha de terminación debe ser posterior a la fecha de inicio.",
                new[] { nameof(LeaseEndDate) });
        }
    }
}

public class AddCohabitationMemberDto : IValidatableObject
{
    [Required(ErrorMessage = "El nombre es obligatorio.")]
    [MaxLength(200)]
    public string FullNameOrPetName { get; set; } = string.Empty;

    [Required(ErrorMessage = "El parentesco o relación es obligatorio.")]
    [MaxLength(100)]
    public string Relationship { get; set; } = string.Empty;

    public DateTime? DateOfBirth { get; set; }

    public bool IsPet { get; set; }

    [MaxLength(100)]
    public string? PetSpecies { get; set; }

    [MaxLength(100)]
    public string? PetBreed { get; set; }

    [MaxLength(100)]
    public string? PetSanitaryRegistration { get; set; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (IsPet && string.IsNullOrWhiteSpace(PetSpecies))
        {
            yield return new ValidationResult(
                "La especie es obligatoria para mascotas.",
                new[] { nameof(PetSpecies) });
        }

        if (!IsPet && DateOfBirth.HasValue && DateOfBirth.Value > DateTime.UtcNow)
        {
            yield return new ValidationResult(
                "La fecha de nacimiento no puede ser una fecha futura.",
                new[] { nameof(DateOfBirth) });
        }
    }
}

public class TransferPropertyDto : IValidatableObject
{
    [Required(ErrorMessage = "El ID del nuevo propietario es obligatorio.")]
    public Guid NewOwnerId { get; set; }

    [Required(ErrorMessage = "La fecha de transferencia es obligatoria.")]
    public DateTime TransferDate { get; set; }

    [Required]
    [Range(0.01, 100.0)]
    public decimal OwnershipPercentage { get; set; }

    public bool IsSpokesperson { get; set; }
    public bool ResidesInUnit { get; set; }

    [MaxLength(1000)]
    public string? TransferNotes { get; set; }

    public bool GeneratePazYSalvo { get; set; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (TransferDate > DateTime.UtcNow.AddDays(1))
        {
            yield return new ValidationResult(
                "La fecha de transferencia no puede ser más de un día en el futuro.",
                new[] { nameof(TransferDate) });
        }
    }
}
