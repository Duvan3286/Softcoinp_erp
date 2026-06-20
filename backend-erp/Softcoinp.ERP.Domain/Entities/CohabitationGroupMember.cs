using System;

namespace Softcoinp.ERP.Domain.Entities;

public class CohabitationGroupMember
{
    public Guid Id { get; set; }
    public string TenantId { get; set; } = string.Empty;

    public Guid UnitId { get; set; }
    public Unit? Unit { get; set; }

    public string FullNameOrPetName { get; set; } = string.Empty;
    public string Relationship { get; set; } = string.Empty;
    public DateTime? DateOfBirth { get; set; }

    public bool IsPet { get; set; }
    public string? PetSpecies { get; set; }
    public string? PetBreed { get; set; }
    public string? PetSanitaryRegistration { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string CreatedByUserId { get; set; } = string.Empty;
}
