using System;
using Softcoinp.ERP.Domain.Enums;

namespace Softcoinp.ERP.Domain.Entities;

public class Unit
{
    public Guid Id { get; set; }
    public string TenantId { get; set; } = string.Empty;
    
    public string Identifier { get; set; } = string.Empty;
    
    public Guid UnitTypeId { get; set; }
    public UnitType? UnitType { get; set; }
    
    public string TowerOrBlock { get; set; } = string.Empty;
    public int FloorLevel { get; set; }
    
    public decimal PrivateArea { get; set; }
    public decimal BalconyArea { get; set; }
    public decimal CoproprietyCoefficient { get; set; }
    
    public UnitStatus Status { get; set; } = UnitStatus.DeliveryProcess;
    
    public bool HasPrivateParking { get; set; }
    public string ParkingIdentifier { get; set; } = string.Empty;
    
    public bool HasAssignedStorage { get; set; }
    public string StorageIdentifier { get; set; } = string.Empty;
    
    public DateTime? ConstructionDeliveryDate { get; set; }
    public string InternalObservations { get; set; } = string.Empty;
    
    // Audit fields
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string CreatedByUserId { get; set; } = string.Empty;
    public DateTime? UpdatedAt { get; set; }

    // Navigation properties for Residents and Owners module
    public System.Collections.Generic.ICollection<UnitOwner> UnitOwners { get; set; } = new System.Collections.Generic.List<UnitOwner>();
    public System.Collections.Generic.ICollection<TenantResident> TenantResidents { get; set; } = new System.Collections.Generic.List<TenantResident>();
    public System.Collections.Generic.ICollection<CohabitationGroupMember> CohabitationGroupMembers { get; set; } = new System.Collections.Generic.List<CohabitationGroupMember>();
    public System.Collections.Generic.ICollection<OwnerHistory> OwnerHistories { get; set; } = new System.Collections.Generic.List<OwnerHistory>();
    public System.Collections.Generic.ICollection<SpokespersonHistory> SpokespersonHistories { get; set; } = new System.Collections.Generic.List<SpokespersonHistory>();

    // Navigation properties for PQR module
    public System.Collections.Generic.ICollection<PqrRecord> PqrRecords { get; set; } = new System.Collections.Generic.List<PqrRecord>();
    public System.Collections.Generic.ICollection<PqrRecord> PqrRecordsAsInvolvedResident { get; set; } = new System.Collections.Generic.List<PqrRecord>();
}
