using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Softcoinp.ERP.Domain.Entities;
using Softcoinp.ERP.Domain.Enums;
using Softcoinp.ERP.Infrastructure.Persistence;
using Softcoinp.ERP.WebAPI.DTOs;

namespace Softcoinp.ERP.WebAPI.Controllers;

[ApiController]
[Route("api/units")]
[Authorize]
public class UnitsController : BaseController
{
    private readonly ApplicationDbContext _context;
    private readonly IMemoryCache _cache;

    public UnitsController(ApplicationDbContext context, IMemoryCache cache)
    {
        _context = context;
        _cache = cache;
    }

    [HttpGet("types")]
    public async Task<IActionResult> GetUnitTypes()
    {
        var tenantId = GetTenantId();
        var types = await _context.UnitTypes
            .Where(t => t.TenantId == tenantId)
            .Select(t => new UnitTypeDto
            {
                Id = t.Id,
                Name = t.Name,
                HasCustomLiquidationRules = t.HasCustomLiquidationRules
            })
            .ToListAsync();

        return Ok(types);
    }

    [HttpPost("types")]
    [Authorize(Roles = "SuperAdmin,Admin")]
    public async Task<IActionResult> CreateUnitType([FromBody] CreateUnitTypeDto dto)
    {
        var tenantId = GetTenantId();
        var type = new UnitType
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            Name = dto.Name,
            HasCustomLiquidationRules = dto.HasCustomLiquidationRules,
            CreatedByUserId = GetUserId()
        };

        _context.UnitTypes.Add(type);
        await _context.SaveChangesAsync();

        return Ok(new UnitTypeDto
        {
            Id = type.Id,
            Name = type.Name,
            HasCustomLiquidationRules = type.HasCustomLiquidationRules
        });
    }

    [HttpGet("coefficient-summary")]
    public async Task<IActionResult> GetCoefficientSummary()
    {
        var tenantId = GetTenantId();
        
        var total = await _context.Units
            .Where(u => u.TenantId == tenantId && u.Status != UnitStatus.Inactive)
            .SumAsync(u => u.CoproprietyCoefficient);

        var pending = 100m - total;
        if (pending < 0) pending = 0;
        
        var excess = total - 100m;
        if (excess < 0) excess = 0;

        return Ok(new UnitCoefficientSummaryDto
        {
            TotalCoefficient = total,
            PendingCoefficient = pending,
            ExcessCoefficient = excess,
            IsExactlyOneHundred = total == 100m
        });
    }

    [HttpGet]
    public async Task<IActionResult> GetUnits([FromQuery] string? tower, [FromQuery] string? status)
    {
        var tenantId = GetTenantId();
        var query = _context.Units.Where(u => u.TenantId == tenantId).AsQueryable();

        if (!string.IsNullOrEmpty(tower))
        {
            query = query.Where(u => u.TowerOrBlock == tower);
        }

        if (!string.IsNullOrEmpty(status) && Enum.TryParse<UnitStatus>(status, out var parsedStatus))
        {
            query = query.Where(u => u.Status == parsedStatus);
        }

        var units = await query.Select(u => new UnitDto
        {
            Id = u.Id,
            Identifier = u.Identifier,
            UnitTypeId = u.UnitTypeId,
            UnitTypeName = u.UnitType!.Name,
            TowerOrBlock = u.TowerOrBlock,
            FloorLevel = u.FloorLevel,
            PrivateArea = u.PrivateArea,
            BalconyArea = u.BalconyArea,
            CoproprietyCoefficient = u.CoproprietyCoefficient,
            Status = u.Status,
            HasPrivateParking = u.HasPrivateParking,
            ParkingIdentifier = u.ParkingIdentifier,
            HasAssignedStorage = u.HasAssignedStorage,
            StorageIdentifier = u.StorageIdentifier,
            ConstructionDeliveryDate = u.ConstructionDeliveryDate,
            InternalObservations = u.InternalObservations
        }).ToListAsync();

        return Ok(units);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetUnit(Guid id)
    {
        var tenantId = GetTenantId();
        var u = await _context.Units.Include(un => un.UnitType).FirstOrDefaultAsync(un => un.Id == id && un.TenantId == tenantId);
        
        if (u == null) return NotFound();

        return Ok(new UnitDto
        {
            Id = u.Id,
            Identifier = u.Identifier,
            UnitTypeId = u.UnitTypeId,
            UnitTypeName = u.UnitType!.Name,
            TowerOrBlock = u.TowerOrBlock,
            FloorLevel = u.FloorLevel,
            PrivateArea = u.PrivateArea,
            BalconyArea = u.BalconyArea,
            CoproprietyCoefficient = u.CoproprietyCoefficient,
            Status = u.Status,
            HasPrivateParking = u.HasPrivateParking,
            ParkingIdentifier = u.ParkingIdentifier,
            HasAssignedStorage = u.HasAssignedStorage,
            StorageIdentifier = u.StorageIdentifier,
            ConstructionDeliveryDate = u.ConstructionDeliveryDate,
            InternalObservations = u.InternalObservations
        });
    }

    [HttpPost]
    [Authorize(Roles = "SuperAdmin,Admin")]
    public async Task<IActionResult> CreateUnit([FromBody] CreateUnitDto dto)
    {
        var tenantId = GetTenantId();
        
        // Validate UnitType exists
        var unitTypeExists = await _context.UnitTypes.AnyAsync(t => t.Id == dto.UnitTypeId && t.TenantId == tenantId);
        if (!unitTypeExists)
        {
            return BadRequest("El tipo de unidad seleccionado no existe en este conjunto.");
        }

        // Validate Identifier Uniqueness
        var exists = await _context.Units.AnyAsync(u => u.TenantId == tenantId && u.Identifier == dto.Identifier);
        if (exists)
        {
            return BadRequest("El identificador de la unidad ya existe en este conjunto.");
        }

        // Validate total coefficient sum logic
        var currentTotal = await _context.Units
            .Where(u => u.TenantId == tenantId && u.Status != UnitStatus.Inactive)
            .SumAsync(u => u.CoproprietyCoefficient);
        
        var willBeActive = dto.Status != UnitStatus.Inactive;
        if (willBeActive && Math.Abs(100m - (currentTotal + dto.CoproprietyCoefficient)) > 0.0001m)
        {
            var expected = 100m - currentTotal;
            return BadRequest($"La suma de coeficientes debe ser exactamente 100%. Coeficiente actual: {currentTotal:F4}%, esperado para esta unidad: {expected:F4}%.");
        }

        var unit = new Unit
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            Identifier = dto.Identifier,
            UnitTypeId = dto.UnitTypeId,
            TowerOrBlock = dto.TowerOrBlock,
            FloorLevel = dto.FloorLevel,
            PrivateArea = dto.PrivateArea,
            BalconyArea = dto.BalconyArea,
            CoproprietyCoefficient = dto.CoproprietyCoefficient,
            Status = dto.Status,
            HasPrivateParking = dto.HasPrivateParking,
            ParkingIdentifier = dto.ParkingIdentifier,
            HasAssignedStorage = dto.HasAssignedStorage,
            StorageIdentifier = dto.StorageIdentifier,
            ConstructionDeliveryDate = dto.ConstructionDeliveryDate,
            InternalObservations = dto.InternalObservations,
            CreatedByUserId = GetUserId()
        };

        _context.Units.Add(unit);

        // Record State History
        _context.UnitStateHistories.Add(new UnitStateHistory
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            UnitId = unit.Id,
            PreviousStatus = unit.Status,
            NewStatus = unit.Status,
            ChangedByUserId = GetUserId(),
            Reason = "Creación inicial de la unidad."
        });

        await _context.SaveChangesAsync();
        _cache.Remove($"mora_map_{tenantId}");
        return Ok();
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "SuperAdmin,Admin")]
    public async Task<IActionResult> UpdateUnit(Guid id, [FromBody] UpdateUnitDto dto)
    {
        var tenantId = GetTenantId();
        var unit = await _context.Units.FirstOrDefaultAsync(u => u.Id == id && u.TenantId == tenantId);
        if (unit == null) return NotFound();

        // Validate Identifier Uniqueness
        if (unit.Identifier != dto.Identifier)
        {
            var exists = await _context.Units.AnyAsync(u => u.TenantId == tenantId && u.Identifier == dto.Identifier);
            if (exists)
            {
                return BadRequest("El identificador de la unidad ya existe en este conjunto.");
            }
        }

        // Validate total coefficient sum logic
        var currentTotalOtherUnits = await _context.Units
            .Where(u => u.TenantId == tenantId && u.Status != UnitStatus.Inactive && u.Id != id)
            .SumAsync(u => u.CoproprietyCoefficient);
        
        var willBeActive = dto.Status != UnitStatus.Inactive;
        if (willBeActive && Math.Abs(100m - (currentTotalOtherUnits + dto.CoproprietyCoefficient)) > 0.0001m)
        {
            var expected = 100m - currentTotalOtherUnits;
            return BadRequest($"La suma de coeficientes debe ser exactamente 100%. Coeficiente actual sin esta unidad: {currentTotalOtherUnits:F4}%, esperado para esta unidad: {expected:F4}%.");
        }

        if (unit.Status != dto.Status)
        {
            _context.UnitStateHistories.Add(new UnitStateHistory
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                UnitId = unit.Id,
                PreviousStatus = unit.Status,
                NewStatus = dto.Status,
                ChangedByUserId = GetUserId(),
                Reason = string.IsNullOrWhiteSpace(dto.ReasonForChange) ? "Actualización de estado." : dto.ReasonForChange
            });
        }

        unit.Identifier = dto.Identifier;
        unit.UnitTypeId = dto.UnitTypeId;
        unit.TowerOrBlock = dto.TowerOrBlock;
        unit.FloorLevel = dto.FloorLevel;
        unit.PrivateArea = dto.PrivateArea;
        unit.BalconyArea = dto.BalconyArea;
        unit.CoproprietyCoefficient = dto.CoproprietyCoefficient;
        unit.Status = dto.Status;
        unit.HasPrivateParking = dto.HasPrivateParking;
        unit.ParkingIdentifier = dto.ParkingIdentifier;
        unit.HasAssignedStorage = dto.HasAssignedStorage;
        unit.StorageIdentifier = dto.StorageIdentifier;
        unit.ConstructionDeliveryDate = dto.ConstructionDeliveryDate;
        unit.InternalObservations = dto.InternalObservations;

        await _context.SaveChangesAsync();
        _cache.Remove($"mora_map_{tenantId}");
        return Ok();
    }

    [HttpPost("bulk-import")]
    [Authorize(Roles = "SuperAdmin,Admin")]
    public async Task<IActionResult> BulkImport([FromForm] Microsoft.AspNetCore.Http.IFormFile file)
    {
        var tenantId = GetTenantId();
        
        if (file == null || file.Length == 0)
        {
            return BadRequest("El archivo está vacío o no fue proporcionado.");
        }

        var errors = new System.Collections.Generic.List<string>();
        var newUnits = new System.Collections.Generic.List<Unit>();
        var unitTypes = await _context.UnitTypes.Where(t => t.TenantId == tenantId).ToListAsync();

        using var stream = file.OpenReadStream();
        using var reader = new System.IO.StreamReader(stream);
        
        // Read header
        var headerLine = await reader.ReadLineAsync();
        if (string.IsNullOrWhiteSpace(headerLine))
        {
            return BadRequest("El archivo no tiene cabecera.");
        }

        int rowNumber = 1;
        decimal totalNewCoefficient = 0;

        while (!reader.EndOfStream)
        {
            var line = await reader.ReadLineAsync();
            rowNumber++;
            
            if (string.IsNullOrWhiteSpace(line)) continue;

            // Detect separator (comma or semicolon)
            char separator = line.Contains(';') ? ';' : ',';
            var columns = line.Split(separator);

            if (columns.Length < 12)
            {
                errors.Add($"Fila {rowNumber}: El archivo debe tener al menos 12 columnas. Tiene {columns.Length}.");
                continue;
            }

            var identifier = columns[0].Trim();
            var unitTypeName = columns[1].Trim();
            var towerOrBlock = columns[2].Trim();
            var floorLevelStr = columns[3].Trim();
            // Allow comma as decimal separator by replacing it with dot
            var privateAreaStr = columns[4].Trim().Replace(',', '.');
            var balconyAreaStr = columns[5].Trim().Replace(',', '.');
            var coefficientStr = columns[6].Trim().Replace(',', '.');
            var statusStr = columns[7].Trim();
            var hasPrivateParkingStr = columns[8].Trim();
            var parkingIdentifier = columns[9].Trim();
            var hasAssignedStorageStr = columns[10].Trim();
            var storageIdentifier = columns[11].Trim();

            if (string.IsNullOrWhiteSpace(identifier))
            {
                errors.Add($"Fila {rowNumber}: El identificador es obligatorio.");
            }

            var unitType = unitTypes.FirstOrDefault(t => t.Name.Equals(unitTypeName, StringComparison.OrdinalIgnoreCase));
            if (unitType == null)
            {
                unitType = new UnitType
                {
                    Id = Guid.NewGuid(),
                    TenantId = tenantId,
                    Name = unitTypeName,
                    HasCustomLiquidationRules = false,
                    CreatedByUserId = GetUserId()
                };
                unitTypes.Add(unitType);
                _context.UnitTypes.Add(unitType);
            }

            if (!int.TryParse(floorLevelStr, out int floorLevel))
            {
                errors.Add($"Fila {rowNumber}: El piso debe ser un número entero.");
            }

            if (!decimal.TryParse(privateAreaStr, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out decimal privateArea))
            {
                errors.Add($"Fila {rowNumber}: El área privada debe ser un número.");
            }

            if (!decimal.TryParse(balconyAreaStr, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out decimal balconyArea))
            {
                errors.Add($"Fila {rowNumber}: El área de balcón debe ser un número.");
            }

            if (!decimal.TryParse(coefficientStr, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out decimal coefficient))
            {
                errors.Add($"Fila {rowNumber}: El coeficiente debe ser un número.");
            }
            else if (coefficient <= 0)
            {
                errors.Add($"Fila {rowNumber}: El coeficiente debe ser mayor a 0.");
            }

            // Parse status with Spanish and English mappings
            UnitStatus status = UnitStatus.Inactive;
            var normalizedStatus = statusStr.Replace(" ", "").ToLowerInvariant();
            if (normalizedStatus == "activayocupada" || normalizedStatus == "activaocupada" || normalizedStatus == "activeoccupied")
            {
                status = UnitStatus.ActiveOccupied;
            }
            else if (normalizedStatus == "activaydesocupada" || normalizedStatus == "activadesocupada" || normalizedStatus == "activeunoccupied")
            {
                status = UnitStatus.ActiveUnoccupied;
            }
            else if (normalizedStatus == "enprocesodeentrega" || normalizedStatus == "procesodeentrega" || normalizedStatus == "procesoentrega" || normalizedStatus == "deliveryprocess")
            {
                status = UnitStatus.DeliveryProcess;
            }
            else if (normalizedStatus == "enlitigio" || normalizedStatus == "litigio" || normalizedStatus == "litigation")
            {
                status = UnitStatus.Litigation;
            }
            else if (normalizedStatus == "inactiva" || normalizedStatus == "inactive")
            {
                status = UnitStatus.Inactive;
            }
            else
            {
                errors.Add($"Fila {rowNumber}: Estado inválido '{statusStr}'. Valores permitidos: Activa y Ocupada, Activa y Desocupada, En Proceso de Entrega, En Litigio, Inactiva.");
            }

            // Parse Spanish and English boolean representation
            bool ParseBoolean(string val)
            {
                var v = val.Trim().ToLowerInvariant();
                return v == "true" || v == "sí" || v == "si" || v == "verdadero" || v == "1";
            }

            bool hasPrivateParking = ParseBoolean(hasPrivateParkingStr);
            bool hasAssignedStorage = ParseBoolean(hasAssignedStorageStr);

            if (errors.Count == 0)
            {
                if (status != UnitStatus.Inactive)
                {
                    totalNewCoefficient += coefficient;
                }

                newUnits.Add(new Unit
                {
                    Id = Guid.NewGuid(),
                    TenantId = tenantId,
                    Identifier = identifier,
                    UnitTypeId = unitType!.Id,
                    TowerOrBlock = towerOrBlock,
                    FloorLevel = floorLevel,
                    PrivateArea = privateArea,
                    BalconyArea = balconyArea,
                    CoproprietyCoefficient = coefficient,
                    Status = status,
                    HasPrivateParking = hasPrivateParking,
                    ParkingIdentifier = parkingIdentifier,
                    HasAssignedStorage = hasAssignedStorage,
                    StorageIdentifier = storageIdentifier,
                    CreatedByUserId = GetUserId()
                });
            }
        }

        if (errors.Any())
        {
            await LogBulkImport(tenantId, BulkImportStatus.Failed, 0, errors.Count, System.Text.Json.JsonSerializer.Serialize(errors));
            return BadRequest(new { Message = "Se encontraron errores en el archivo. Ninguna unidad fue importada.", Errors = errors });
        }

        // Validate Identifiers uniqueness in memory
        var duplicateIdentifiers = newUnits.GroupBy(u => u.Identifier).Where(g => g.Count() > 1).Select(g => g.Key).ToList();
        if (duplicateIdentifiers.Any())
        {
            errors.Add($"Hay identificadores duplicados en el archivo: {string.Join(", ", duplicateIdentifiers)}");
            await LogBulkImport(tenantId, BulkImportStatus.Failed, 0, errors.Count, System.Text.Json.JsonSerializer.Serialize(errors));
            return BadRequest(new { Message = "Error de validación", Errors = errors });
        }

        // Database checks
        var existingIdentifiers = await _context.Units.Where(u => u.TenantId == tenantId).Select(u => u.Identifier).ToListAsync();
        var duplicatesInDb = newUnits.Where(u => existingIdentifiers.Contains(u.Identifier)).Select(u => u.Identifier).ToList();
        
        if (duplicatesInDb.Any())
        {
            errors.Add($"Los siguientes identificadores ya existen en el sistema: {string.Join(", ", duplicatesInDb)}");
            await LogBulkImport(tenantId, BulkImportStatus.Failed, 0, errors.Count, System.Text.Json.JsonSerializer.Serialize(errors));
            return BadRequest(new { Message = "Error de validación", Errors = errors });
        }

        // Coefficient validation
        var currentTotal = await _context.Units
            .Where(u => u.TenantId == tenantId && u.Status != UnitStatus.Inactive)
            .SumAsync(u => u.CoproprietyCoefficient);
        
        if (Math.Abs(100m - (currentTotal + totalNewCoefficient)) > 0.0001m)
        {
            var expected = 100m - currentTotal;
            errors.Add($"La suma de coeficientes debe ser exactamente 100%. El sistema tiene {currentTotal:F4}%, el archivo agrega {totalNewCoefficient:F4}%. Se esperaba {expected:F4}% en el archivo para completar el 100%.");
            await LogBulkImport(tenantId, BulkImportStatus.Failed, 0, errors.Count, System.Text.Json.JsonSerializer.Serialize(errors));
            return BadRequest(new { Message = "Error de validación de coeficientes", Errors = errors });
        }

        // Transactional insert
        using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            _context.Units.AddRange(newUnits);

            var histories = newUnits.Select(u => new UnitStateHistory
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                UnitId = u.Id,
                PreviousStatus = u.Status,
                NewStatus = u.Status,
                ChangedByUserId = GetUserId(),
                Reason = "Carga masiva"
            });
            _context.UnitStateHistories.AddRange(histories);

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();
            _cache.Remove($"mora_map_{tenantId}");

            await LogBulkImport(tenantId, BulkImportStatus.Success, newUnits.Count, 0, "[]");

            return Ok(new { Message = $"Se importaron {newUnits.Count} unidades exitosamente." });
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            errors.Add($"Error de sistema: {ex.Message}");
            await LogBulkImport(tenantId, BulkImportStatus.Failed, 0, 1, System.Text.Json.JsonSerializer.Serialize(errors));
            return StatusCode(500, new { Message = "Error crítico durante la transacción.", Errors = errors });
        }
    }

    private async Task LogBulkImport(string tenantId, BulkImportStatus status, int processed, int errorsCount, string errorReport)
    {
        _context.BulkImportLogs.Add(new BulkImportLog
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            ExecutedAt = DateTime.UtcNow,
            ExecutedByUserId = GetUserId(),
            Status = status,
            ProcessedRecordsCount = processed,
            ErrorCount = errorsCount,
            ErrorReport = errorReport
        });
        await _context.SaveChangesAsync();
    }
}
