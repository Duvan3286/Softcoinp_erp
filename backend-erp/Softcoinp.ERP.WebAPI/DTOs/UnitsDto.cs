using System;
using System.ComponentModel.DataAnnotations;
using Softcoinp.ERP.Domain.Enums;

namespace Softcoinp.ERP.WebAPI.DTOs;

public class UnitTypeDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public bool HasCustomLiquidationRules { get; set; }
}

public class CreateUnitTypeDto
{
    [Required]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;
    public bool HasCustomLiquidationRules { get; set; }
}

public class UnitDto
{
    public Guid Id { get; set; }
    public string Identifier { get; set; } = string.Empty;
    public Guid UnitTypeId { get; set; }
    public string UnitTypeName { get; set; } = string.Empty;
    public string TowerOrBlock { get; set; } = string.Empty;
    public int FloorLevel { get; set; }
    public decimal PrivateArea { get; set; }
    public decimal BalconyArea { get; set; }
    public decimal CoproprietyCoefficient { get; set; }
    public UnitStatus Status { get; set; }
    public bool HasPrivateParking { get; set; }
    public string ParkingIdentifier { get; set; } = string.Empty;
    public bool HasAssignedStorage { get; set; }
    public string StorageIdentifier { get; set; } = string.Empty;
    public DateTime? ConstructionDeliveryDate { get; set; }
    public string InternalObservations { get; set; } = string.Empty;
}

public class CreateUnitDto
{
    [Required]
    [StringLength(50)]
    public string Identifier { get; set; } = string.Empty;
    
    [Required]
    public Guid UnitTypeId { get; set; }
    
    [StringLength(50)]
    public string TowerOrBlock { get; set; } = string.Empty;
    
    public int FloorLevel { get; set; }
    
    [Range(0, 1000000)]
    public decimal PrivateArea { get; set; }
    
    [Range(0, 100000)]
    public decimal BalconyArea { get; set; }
    
    [Required]
    [Range(0.0001, 100.0000, ErrorMessage = "Coefficient must be greater than 0 and up to 100.")]
    public decimal CoproprietyCoefficient { get; set; }
    
    [Required]
    public UnitStatus Status { get; set; }
    
    public bool HasPrivateParking { get; set; }
    
    [StringLength(50)]
    public string ParkingIdentifier { get; set; } = string.Empty;
    
    public bool HasAssignedStorage { get; set; }
    
    [StringLength(50)]
    public string StorageIdentifier { get; set; } = string.Empty;
    
    public DateTime? ConstructionDeliveryDate { get; set; }
    
    [StringLength(1000)]
    public string InternalObservations { get; set; } = string.Empty;
}

public class UpdateUnitDto : CreateUnitDto
{
    public string ReasonForChange { get; set; } = string.Empty;
}

public class UnitCoefficientSummaryDto
{
    public decimal TotalCoefficient { get; set; }
    public decimal PendingCoefficient { get; set; }
    public decimal ExcessCoefficient { get; set; }
    public bool IsExactlyOneHundred { get; set; }
}

public class UnitIdentifierAvailabilityDto
{
    public bool IsAvailable { get; set; }
    public string? Message { get; set; }
}
