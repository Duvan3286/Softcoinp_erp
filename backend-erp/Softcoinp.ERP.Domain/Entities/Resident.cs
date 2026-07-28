using System;
using System.Collections.Generic;
using Softcoinp.ERP.Domain.Enums;

namespace Softcoinp.ERP.Domain.Entities;

public class Resident
{
    public Guid Id { get; set; }
    public string TenantId { get; set; } = string.Empty;

    public string FullNameOrPetName { get; set; } = string.Empty;
    public DateTime? DateOfBirth { get; set; }

    public DocumentType? DocumentType { get; set; }
    public string? DocumentNumber { get; set; }
    public string? Phone { get; set; }

    public bool IsPet { get; set; }
    public string? PetSpecies { get; set; }
    public string? PetBreed { get; set; }
    public string? PetSanitaryRegistration { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string CreatedByUserId { get; set; } = string.Empty;

    public ICollection<UnitResident> UnitResidents { get; set; } = new List<UnitResident>();
    public ICollection<ResidentHistory> ResidentHistories { get; set; } = new List<ResidentHistory>();
}
