using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Softcoinp.ERP.Domain.Entities;
using Softcoinp.ERP.Domain.Enums;
using Softcoinp.ERP.Infrastructure.Persistence;
using Softcoinp.ERP.WebAPI.DTOs;
using Softcoinp.ERP.WebAPI.Services;

namespace Softcoinp.ERP.WebAPI.Controllers;

[ApiController]
[Route("api/residents")]
[Authorize(Roles = "SuperAdmin,Admin,Council,Auditor")]
public class ResidentsController : BaseController
{
    private readonly ApplicationDbContext _context;
    private readonly IndicatorCacheService _indicatorCache;
    private readonly NotificationService _notificationService;

    public ResidentsController(ApplicationDbContext context, IndicatorCacheService indicatorCache, NotificationService notificationService)
    {
        _context = context;
        _indicatorCache = indicatorCache;
        _notificationService = notificationService;
    }

    // ── PROPIETARIOS — LISTADO Y DETALLE ─────────────────────────────────────

    [HttpGet("owners")]
    public async Task<IActionResult> GetOwners(
        [FromQuery] string? search,
        [FromQuery] bool includeInactive = false)
    {
        var tenantId = GetTenantId();

        var query = _context.Owners
            .Include(o => o.UnitOwners)
                .ThenInclude(uo => uo.Unit)
            .Where(o => o.TenantId == tenantId);

        if (!includeInactive)
        {
            query = query.Where(o => o.IsActive);
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(o =>
                o.FullNameOrCompanyName.Contains(search) ||
                o.DocumentNumber.Contains(search) ||
                o.Email.Contains(search));
        }

        var owners = await query
            .OrderBy(o => o.FullNameOrCompanyName)
            .Select(o => new OwnerSummaryDto
            {
                Id = o.Id,
                OwnerType = o.OwnerType.ToString(),
                DocumentType = o.DocumentType.ToString(),
                DocumentNumber = o.DocumentNumber,
                FullNameOrCompanyName = o.FullNameOrCompanyName,
                Email = o.Email,
                MainPhone = o.MainPhone,
                IsActive = o.IsActive,
                Units = o.UnitOwners
                    .Where(uo => uo.IsActive)
                    .Select(uo => new UnitOwnerSummaryDto
                    {
                        AssignmentId = uo.Id,
                        UnitId = uo.UnitId,
                        UnitIdentifier = uo.Unit != null ? uo.Unit.Identifier : string.Empty,
                        OwnerId = o.Id,
                        OwnerName = o.FullNameOrCompanyName,
                        OwnerDocumentNumber = o.DocumentNumber,
                        OwnerDocumentType = o.DocumentType.ToString(),
                        OwnershipPercentage = uo.OwnershipPercentage,
                        IsSpokesperson = uo.IsSpokesperson,
                        ResidesInUnit = uo.ResidesInUnit,
                        StartDate = uo.StartDate,
                        EndDate = uo.EndDate
                    }).ToList()
            })
            .ToListAsync();

        return Ok(owners);
    }

    [HttpGet("owners/{id:guid}")]
    public async Task<IActionResult> GetOwnerDetail(Guid id)
    {
        var tenantId = GetTenantId();

        var owner = await _context.Owners
            .Include(o => o.UnitOwners)
                .ThenInclude(uo => uo.Unit)
            .Include(o => o.ContactHistories)
            .Where(o => o.Id == id && o.TenantId == tenantId)
            .Select(o => new OwnerDetailDto
            {
                Id = o.Id,
                OwnerType = o.OwnerType.ToString(),
                DocumentType = o.DocumentType.ToString(),
                DocumentNumber = o.DocumentNumber,
                VerificationDigit = o.VerificationDigit,
                FullNameOrCompanyName = o.FullNameOrCompanyName,
                Email = o.Email,
                MainPhone = o.MainPhone,
                AlternativePhone = o.AlternativePhone,
                CorrespondenceAddress = o.CorrespondenceAddress,
                DateOfBirth = o.DateOfBirth,
                CivilStatus = o.CivilStatus,
                LegalRepresentativeName = o.LegalRepresentativeName,
                LegalRepresentativeDocumentType = o.LegalRepresentativeDocumentType.HasValue
                    ? o.LegalRepresentativeDocumentType.ToString()
                    : null,
                LegalRepresentativeDocument = o.LegalRepresentativeDocument,
                LegalRepresentativeRole = o.LegalRepresentativeRole,
                PowerOfAttorneyExpiration = o.PowerOfAttorneyExpiration,
                IsActive = o.IsActive,
                CreatedAt = o.CreatedAt,
                Units = o.UnitOwners
                    .Select(uo => new UnitOwnerSummaryDto
                    {
                        AssignmentId = uo.Id,
                        UnitId = uo.UnitId,
                        UnitIdentifier = uo.Unit != null ? uo.Unit.Identifier : string.Empty,
                        OwnerId = o.Id,
                        OwnerName = o.FullNameOrCompanyName,
                        OwnerDocumentNumber = o.DocumentNumber,
                        OwnerDocumentType = o.DocumentType.ToString(),
                        OwnershipPercentage = uo.OwnershipPercentage,
                        IsSpokesperson = uo.IsSpokesperson,
                        ResidesInUnit = uo.ResidesInUnit,
                        StartDate = uo.StartDate,
                        EndDate = uo.EndDate
                    }).ToList(),
                ContactHistory = o.ContactHistories
                    .OrderByDescending(ch => ch.ChangedAt)
                    .Select(ch => new ContactHistoryDto
                    {
                        Id = ch.Id,
                        FieldChanged = ch.FieldChanged,
                        OldValue = ch.OldValue,
                        NewValue = ch.NewValue,
                        ChangedAt = ch.ChangedAt,
                        ChangedByUserId = ch.ChangedByUserId
                    }).ToList()
            })
            .FirstOrDefaultAsync();

        if (owner == null)
        {
            return NotFound(new { message = "Propietario no encontrado." });
        }

        return Ok(owner);
    }

    // ── PROPIETARIOS — PERSONA NATURAL ───────────────────────────────────────

    [HttpPost("owners/natural-person")]
    [Authorize(Roles = "SuperAdmin,Admin")]
    public async Task<IActionResult> CreateNaturalPersonOwner([FromBody] CreateNaturalPersonOwnerDto dto)
    {
        var tenantId = GetTenantId();

        var exists = await _context.Owners
            .AnyAsync(o => o.TenantId == tenantId && o.DocumentNumber == dto.DocumentNumber);

        if (exists)
        {
            return Conflict(new { message = "Ya existe un propietario con ese número de documento en este conjunto." });
        }

        var owner = new Owner
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            OwnerType = OwnerType.NaturalPerson,
            DocumentType = dto.DocumentType,
            DocumentNumber = dto.DocumentNumber,
            FullNameOrCompanyName = dto.FullName,
            Email = dto.Email,
            MainPhone = dto.MainPhone,
            AlternativePhone = dto.AlternativePhone,
            CorrespondenceAddress = dto.CorrespondenceAddress,
            DateOfBirth = dto.DateOfBirth,
            CivilStatus = dto.CivilStatus,
            IsActive = true,
            CreatedByUserId = GetUserId()
        };

        _context.Owners.Add(owner);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetOwnerDetail), new { id = owner.Id },
            new { owner.Id, owner.FullNameOrCompanyName, owner.DocumentNumber });
    }

    [HttpPut("owners/{id:guid}/natural-person")]
    [Authorize(Roles = "SuperAdmin,Admin")]
    public async Task<IActionResult> UpdateNaturalPersonOwner(Guid id, [FromBody] UpdateNaturalPersonOwnerDto dto)
    {
        var tenantId = GetTenantId();
        var userId = GetUserId();

        var owner = await _context.Owners
            .FirstOrDefaultAsync(o => o.Id == id && o.TenantId == tenantId && o.IsActive);

        if (owner == null)
        {
            return NotFound(new { message = "Propietario no encontrado o inactivo." });
        }

        if (owner.OwnerType != OwnerType.NaturalPerson)
        {
            return BadRequest(new { message = "Este endpoint es exclusivo para propietarios persona natural." });
        }

        var trackedFields = BuildContactChanges(owner, dto, userId, tenantId);

        owner.FullNameOrCompanyName = dto.FullName;
        owner.Email = dto.Email;
        owner.MainPhone = dto.MainPhone;
        owner.AlternativePhone = dto.AlternativePhone;
        owner.CorrespondenceAddress = dto.CorrespondenceAddress;
        owner.DateOfBirth = dto.DateOfBirth;
        owner.CivilStatus = dto.CivilStatus;
        owner.UpdatedAt = DateTime.UtcNow;

        _context.ContactHistories.AddRange(trackedFields);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    // ── PROPIETARIOS — PERSONA JURÍDICA ──────────────────────────────────────

    [HttpPost("owners/legal-entity")]
    [Authorize(Roles = "SuperAdmin,Admin")]
    public async Task<IActionResult> CreateLegalEntityOwner([FromBody] CreateLegalEntityOwnerDto dto)
    {
        var tenantId = GetTenantId();

        var exists = await _context.Owners
            .AnyAsync(o => o.TenantId == tenantId && o.DocumentNumber == dto.DocumentNumber);

        if (exists)
        {
            return Conflict(new { message = "Ya existe un propietario con ese NIT en este conjunto." });
        }

        var owner = new Owner
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            OwnerType = OwnerType.LegalEntity,
            DocumentType = DocumentType.NIT,
            DocumentNumber = dto.DocumentNumber,
            VerificationDigit = dto.VerificationDigit,
            FullNameOrCompanyName = dto.CompanyName,
            Email = dto.Email,
            MainPhone = dto.MainPhone,
            AlternativePhone = dto.AlternativePhone,
            CorrespondenceAddress = dto.FiscalAddress,
            LegalRepresentativeName = dto.LegalRepresentativeName,
            LegalRepresentativeDocumentType = dto.LegalRepresentativeDocumentType,
            LegalRepresentativeDocument = dto.LegalRepresentativeDocument,
            LegalRepresentativeRole = dto.LegalRepresentativeRole,
            PowerOfAttorneyExpiration = dto.PowerOfAttorneyExpiration,
            IsActive = true,
            CreatedByUserId = GetUserId()
        };

        _context.Owners.Add(owner);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetOwnerDetail), new { id = owner.Id },
            new { owner.Id, owner.FullNameOrCompanyName, owner.DocumentNumber });
    }

    [HttpPut("owners/{id:guid}/legal-entity")]
    [Authorize(Roles = "SuperAdmin,Admin")]
    public async Task<IActionResult> UpdateLegalEntityOwner(Guid id, [FromBody] UpdateLegalEntityOwnerDto dto)
    {
        var tenantId = GetTenantId();
        var userId = GetUserId();

        var owner = await _context.Owners
            .FirstOrDefaultAsync(o => o.Id == id && o.TenantId == tenantId && o.IsActive);

        if (owner == null)
        {
            return NotFound(new { message = "Propietario no encontrado o inactivo." });
        }

        if (owner.OwnerType != OwnerType.LegalEntity)
        {
            return BadRequest(new { message = "Este endpoint es exclusivo para propietarios persona jurídica." });
        }

        var trackedFields = BuildLegalEntityContactChanges(owner, dto, userId, tenantId);

        owner.Email = dto.Email;
        owner.MainPhone = dto.MainPhone;
        owner.AlternativePhone = dto.AlternativePhone;
        owner.CorrespondenceAddress = dto.FiscalAddress;
        owner.LegalRepresentativeName = dto.LegalRepresentativeName;
        owner.LegalRepresentativeDocumentType = dto.LegalRepresentativeDocumentType;
        owner.LegalRepresentativeDocument = dto.LegalRepresentativeDocument;
        owner.LegalRepresentativeRole = dto.LegalRepresentativeRole;
        owner.PowerOfAttorneyExpiration = dto.PowerOfAttorneyExpiration;
        owner.UpdatedAt = DateTime.UtcNow;

        _context.ContactHistories.AddRange(trackedFields);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    [HttpPost("owners/{id:guid}/deactivate")]
    [Authorize(Roles = "SuperAdmin,Admin")]
    public async Task<IActionResult> DeactivateOwner(Guid id, [FromBody] DeactivateOwnerDto dto)
    {
        var tenantId = GetTenantId();

        var owner = await _context.Owners
            .FirstOrDefaultAsync(o => o.Id == id && o.TenantId == tenantId && o.IsActive);

        if (owner == null)
        {
            return NotFound(new { message = "Propietario no encontrado o ya inactivo." });
        }

        var hasActiveUnitAssignments = await _context.UnitOwners
            .AnyAsync(uo => uo.OwnerId == id && uo.TenantId == tenantId && uo.IsActive);

        if (hasActiveUnitAssignments)
        {
            return BadRequest(new
            {
                message = "El propietario tiene unidades activas asignadas. Debe transferir o remover las asignaciones antes de inactivarlo."
            });
        }

        owner.IsActive = false;
        owner.UpdatedAt = DateTime.UtcNow;

        _context.ContactHistories.Add(new ContactHistory
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            OwnerId = id,
            FieldChanged = "IsActive",
            OldValue = "true",
            NewValue = $"false — {dto.Reason} (fecha: {dto.ExitDate:yyyy-MM-dd})",
            ChangedAt = DateTime.UtcNow,
            ChangedByUserId = GetUserId()
        });

        await _context.SaveChangesAsync();
        return NoContent();
    }

    // ── VINCULACIÓN UNIDAD-PROPIETARIO ────────────────────────────────────────

    [HttpGet("units/{unitId:guid}/owners")]
    public async Task<IActionResult> GetUnitOwners(Guid unitId)
    {
        var tenantId = GetTenantId();

        var unit = await _context.Units.FirstOrDefaultAsync(u => u.Id == unitId && u.TenantId == tenantId);
        if (unit == null)
        {
            return NotFound(new { message = "Unidad no encontrada." });
        }

        var owners = await _context.UnitOwners
            .Include(uo => uo.Owner)
            .Where(uo => uo.UnitId == unitId && uo.TenantId == tenantId && uo.IsActive)
            .Select(uo => new
            {
                uo.Id,
                uo.OwnerId,
                OwnerName = uo.Owner != null ? uo.Owner.FullNameOrCompanyName : string.Empty,
                OwnerDocument = uo.Owner != null ? uo.Owner.DocumentNumber : string.Empty,
                OwnerType = uo.Owner != null ? uo.Owner.OwnerType.ToString() : string.Empty,
                uo.OwnershipPercentage,
                uo.IsSpokesperson,
                uo.ResidesInUnit,
                uo.StartDate,
                uo.EndDate
            })
            .ToListAsync();

        return Ok(owners);
    }

    [HttpPost("units/{unitId:guid}/owners")]
    [Authorize(Roles = "SuperAdmin,Admin")]
    public async Task<IActionResult> AssignOwnerToUnit(Guid unitId, [FromBody] AssignOwnerToUnitDto dto)
    {
        var tenantId = GetTenantId();
        var userId = GetUserId();

        var unit = await _context.Units.FirstOrDefaultAsync(u => u.Id == unitId && u.TenantId == tenantId);
        if (unit == null)
        {
            return NotFound(new { message = "Unidad no encontrada." });
        }

        var owner = await _context.Owners
            .FirstOrDefaultAsync(o => o.Id == dto.OwnerId && o.TenantId == tenantId && o.IsActive);
        if (owner == null)
        {
            return NotFound(new { message = "Propietario no encontrado o inactivo." });
        }

        var alreadyAssigned = await _context.UnitOwners
            .AnyAsync(uo => uo.UnitId == unitId && uo.OwnerId == dto.OwnerId && uo.TenantId == tenantId && uo.IsActive);

        if (alreadyAssigned)
        {
            return Conflict(new { message = "Este propietario ya está asignado a esta unidad." });
        }

        if (dto.IsSpokesperson)
        {
            var currentSpokesperson = await _context.UnitOwners
                .FirstOrDefaultAsync(uo => uo.UnitId == unitId && uo.IsActive && uo.IsSpokesperson && uo.TenantId == tenantId);

            if (currentSpokesperson != null)
            {
                return BadRequest(new
                {
                    message = "Esta unidad ya tiene un vocero activo. Use el endpoint de designación de vocero para realizar el cambio.",
                    currentSpokespersonId = currentSpokesperson.OwnerId
                });
            }
        }

        var unitOwner = new UnitOwner
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            UnitId = unitId,
            OwnerId = dto.OwnerId,
            OwnershipPercentage = dto.OwnershipPercentage,
            IsSpokesperson = dto.IsSpokesperson,
            ResidesInUnit = dto.ResidesInUnit,
            StartDate = dto.StartDate,
            IsActive = true
        };

        _context.UnitOwners.Add(unitOwner);

        _context.OwnerHistories.Add(new OwnerHistory
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            UnitId = unitId,
            OwnerId = dto.OwnerId,
            StartDate = dto.StartDate,
            EndDate = null,
            TransferNotes = "Asignación inicial",
            RecordedAt = DateTime.UtcNow,
            RecordedByUserId = userId
        });

        if (dto.IsSpokesperson)
        {
            _context.SpokespersonHistories.Add(new SpokespersonHistory
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                UnitId = unitId,
                PreviousSpokespersonId = null,
                NewSpokespersonId = dto.OwnerId,
                ChangedAt = DateTime.UtcNow,
                ChangedByUserId = userId,
                ChangeReason = "Designación inicial de vocero"
            });
        }

        if (unit.Status == UnitStatus.DeliveryProcess)
        {
            var previousStatus = unit.Status;
            unit.Status = dto.ResidesInUnit ? UnitStatus.ActiveOccupied : UnitStatus.ActiveUnoccupied;

            _context.UnitStateHistories.Add(new UnitStateHistory
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                UnitId = unitId,
                PreviousStatus = previousStatus,
                NewStatus = unit.Status,
                Reason = "Asignación de primer propietario",
                ChangeDate = DateTime.UtcNow,
                ChangedByUserId = userId
            });
        }

        await _context.SaveChangesAsync();
        return Ok(new { unitOwner.Id, message = "Propietario asignado exitosamente." });
    }

    [HttpPost("units/{unitId:guid}/owners/spokesperson")]
    [Authorize(Roles = "SuperAdmin,Admin")]
    public async Task<IActionResult> DesignateSpokesperson(Guid unitId, [FromBody] DesignateSpokespersonDto dto)
    {
        var tenantId = GetTenantId();
        var userId = GetUserId();

        var newSpokespersonAssignment = await _context.UnitOwners
            .FirstOrDefaultAsync(uo => uo.UnitId == unitId && uo.OwnerId == dto.OwnerId && uo.TenantId == tenantId && uo.IsActive);

        if (newSpokespersonAssignment == null)
        {
            return NotFound(new { message = "El propietario indicado no está activamente asignado a esta unidad." });
        }

        var currentSpokesperson = await _context.UnitOwners
            .FirstOrDefaultAsync(uo => uo.UnitId == unitId && uo.IsActive && uo.IsSpokesperson && uo.TenantId == tenantId);

        if (currentSpokesperson != null && currentSpokesperson.OwnerId == dto.OwnerId)
        {
            return BadRequest(new { message = "Este propietario ya es el vocero activo de la unidad." });
        }

        Guid? previousSpokespersonId = null;

        if (currentSpokesperson != null)
        {
            currentSpokesperson.IsSpokesperson = false;
            previousSpokespersonId = currentSpokesperson.OwnerId;
        }

        newSpokespersonAssignment.IsSpokesperson = true;

        _context.SpokespersonHistories.Add(new SpokespersonHistory
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            UnitId = unitId,
            PreviousSpokespersonId = previousSpokespersonId,
            NewSpokespersonId = dto.OwnerId,
            ChangedAt = DateTime.UtcNow,
            ChangedByUserId = userId,
            ChangeReason = dto.Reason
        });

        await _context.SaveChangesAsync();
        return NoContent();
    }

    [HttpPost("units/{unitId:guid}/owners/{assignmentId:guid}/remove")]
    [Authorize(Roles = "SuperAdmin,Admin")]
    public async Task<IActionResult> RemoveOwnerFromUnit(Guid unitId, Guid assignmentId, [FromBody] RemoveOwnerFromUnitDto dto)
    {
        var tenantId = GetTenantId();
        var userId = GetUserId();

        var assignment = await _context.UnitOwners
            .FirstOrDefaultAsync(uo => uo.Id == assignmentId && uo.UnitId == unitId && uo.TenantId == tenantId && uo.IsActive);

        if (assignment == null)
        {
            return NotFound(new { message = "Asignación no encontrada." });
        }

        assignment.IsActive = false;
        assignment.EndDate = dto.EndDate;

        var historyEntry = await _context.OwnerHistories
            .Where(h => h.UnitId == unitId && h.OwnerId == assignment.OwnerId && h.TenantId == tenantId && h.EndDate == null)
            .OrderByDescending(h => h.StartDate)
            .FirstOrDefaultAsync();

        if (historyEntry != null)
        {
            historyEntry.EndDate = dto.EndDate;
        }

        if (assignment.IsSpokesperson)
        {
            assignment.IsSpokesperson = false;

            _context.SpokespersonHistories.Add(new SpokespersonHistory
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                UnitId = unitId,
                PreviousSpokespersonId = assignment.OwnerId,
                NewSpokespersonId = assignment.OwnerId,
                ChangedAt = DateTime.UtcNow,
                ChangedByUserId = userId,
                ChangeReason = $"Vocero removido de la unidad. {dto.Notes}"
            });
        }

        await _context.SaveChangesAsync();
        return NoContent();
    }

    // ── ARRENDATARIOS ─────────────────────────────────────────────────────────

    [HttpGet("units/{unitId:guid}/tenant")]
    public async Task<IActionResult> GetActiveTenant(Guid unitId)
    {
        var tenantId = GetTenantId();

        var resident = await _context.TenantResidents
            .Where(t => t.UnitId == unitId && t.TenantId == tenantId && t.IsActive)
            .Select(t => new TenantResidentDto
            {
                Id = t.Id,
                UnitId = t.UnitId,
                DocumentType = t.DocumentType.ToString(),
                DocumentNumber = t.DocumentNumber,
                FullName = t.FullName,
                Email = t.Email,
                Phone = t.Phone,
                LeaseStartDate = t.LeaseStartDate,
                LeaseEndDate = t.LeaseEndDate,
                RealEstateAgentName = t.RealEstateAgentName,
                RealEstateAgentPhone = t.RealEstateAgentPhone,
                AuthorizedToPayAdmin = t.AuthorizedToPayAdmin,
                IsActive = t.IsActive,
                DaysUntilLeaseExpires = t.LeaseEndDate.HasValue
                    ? (int)(t.LeaseEndDate.Value - DateTime.UtcNow).TotalDays
                    : (int?)null
            })
            .FirstOrDefaultAsync();

        if (resident == null)
        {
            return NotFound(new { message = "No hay arrendatario activo para esta unidad." });
        }

        return Ok(resident);
    }

    [HttpPost("units/{unitId:guid}/tenant")]
    [Authorize(Roles = "SuperAdmin,Admin")]
    public async Task<IActionResult> RegisterTenant(Guid unitId, [FromBody] CreateTenantResidentDto dto)
    {
        var tenantId = GetTenantId();
        var userId = GetUserId();

        var unit = await _context.Units.FirstOrDefaultAsync(u => u.Id == unitId && u.TenantId == tenantId);
        if (unit == null)
        {
            return NotFound(new { message = "Unidad no encontrada." });
        }

        var hasOwner = await _context.UnitOwners
            .AnyAsync(uo => uo.UnitId == unitId && uo.TenantId == tenantId && uo.IsActive);

        if (!hasOwner)
        {
            return BadRequest(new { message = "La unidad debe tener al menos un propietario activo antes de registrar un arrendatario." });
        }

        var existingActiveTenants = await _context.TenantResidents
            .Where(t => t.UnitId == unitId && t.TenantId == tenantId && t.IsActive)
            .ToListAsync();

        foreach (var existing in existingActiveTenants)
        {
            existing.IsActive = false;
            if (!existing.LeaseEndDate.HasValue)
            {
                existing.LeaseEndDate = dto.LeaseStartDate;
            }
            existing.UpdatedAt = DateTime.UtcNow;
        }

        var resident = new TenantResident
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            UnitId = unitId,
            DocumentType = dto.DocumentType,
            DocumentNumber = dto.DocumentNumber,
            FullName = dto.FullName,
            Email = dto.Email,
            Phone = dto.Phone,
            LeaseStartDate = dto.LeaseStartDate,
            LeaseEndDate = dto.LeaseEndDate,
            RealEstateAgentName = dto.RealEstateAgentName,
            RealEstateAgentPhone = dto.RealEstateAgentPhone,
            AuthorizedToPayAdmin = dto.AuthorizedToPayAdmin,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            CreatedByUserId = userId
        };

        _context.TenantResidents.Add(resident);

        var previousStatus = unit.Status;
        unit.Status = UnitStatus.ActiveOccupied;

        if (previousStatus != UnitStatus.ActiveOccupied)
        {
            _context.UnitStateHistories.Add(new UnitStateHistory
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                UnitId = unitId,
                PreviousStatus = previousStatus,
                NewStatus = UnitStatus.ActiveOccupied,
                Reason = "Registro de nuevo arrendatario",
                ChangeDate = DateTime.UtcNow,
                ChangedByUserId = userId
            });
        }

        await _context.SaveChangesAsync();
        return Ok(new { resident.Id, message = "Arrendatario registrado exitosamente." });
    }

    [HttpPut("units/{unitId:guid}/tenant/{residentId:guid}")]
    [Authorize(Roles = "SuperAdmin,Admin")]
    public async Task<IActionResult> UpdateTenant(Guid unitId, Guid residentId, [FromBody] UpdateTenantResidentDto dto)
    {
        var tenantId = GetTenantId();

        var resident = await _context.TenantResidents
            .FirstOrDefaultAsync(t => t.Id == residentId && t.UnitId == unitId && t.TenantId == tenantId && t.IsActive);

        if (resident == null)
        {
            return NotFound(new { message = "Arrendatario activo no encontrado." });
        }

        resident.Email = dto.Email;
        resident.Phone = dto.Phone;
        resident.LeaseStartDate = dto.LeaseStartDate;
        resident.LeaseEndDate = dto.LeaseEndDate;
        resident.RealEstateAgentName = dto.RealEstateAgentName;
        resident.RealEstateAgentPhone = dto.RealEstateAgentPhone;
        resident.AuthorizedToPayAdmin = dto.AuthorizedToPayAdmin;
        resident.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return NoContent();
    }

    [HttpPost("units/{unitId:guid}/tenant/{residentId:guid}/deactivate")]
    [Authorize(Roles = "SuperAdmin,Admin")]
    public async Task<IActionResult> DeactivateTenant(Guid unitId, Guid residentId)
    {
        var tenantId = GetTenantId();
        var userId = GetUserId();

        var resident = await _context.TenantResidents
            .FirstOrDefaultAsync(t => t.Id == residentId && t.UnitId == unitId && t.TenantId == tenantId && t.IsActive);

        if (resident == null)
        {
            return NotFound(new { message = "Arrendatario activo no encontrado." });
        }

        resident.IsActive = false;
        if (!resident.LeaseEndDate.HasValue)
        {
            resident.LeaseEndDate = DateTime.UtcNow;
        }
        resident.UpdatedAt = DateTime.UtcNow;

        var unit = await _context.Units.FirstOrDefaultAsync(u => u.Id == unitId && u.TenantId == tenantId);
        if (unit != null && unit.Status == UnitStatus.ActiveOccupied)
        {
            unit.Status = UnitStatus.ActiveUnoccupied;

            _context.UnitStateHistories.Add(new UnitStateHistory
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                UnitId = unitId,
                PreviousStatus = UnitStatus.ActiveOccupied,
                NewStatus = UnitStatus.ActiveUnoccupied,
                Reason = "Salida de arrendatario",
                ChangeDate = DateTime.UtcNow,
                ChangedByUserId = userId
            });
        }

        await _context.SaveChangesAsync();
        return NoContent();
    }

    // ── GRUPO DE CONVIVENCIA ──────────────────────────────────────────────────

    [HttpGet("units/{unitId:guid}/cohabitation")]
    public async Task<IActionResult> GetCohabitationMembers(Guid unitId)
    {
        var tenantId = GetTenantId();

        var members = await _context.CohabitationGroupMembers
            .Where(c => c.UnitId == unitId && c.TenantId == tenantId && c.IsActive)
            .Select(c => new CohabitationMemberDto
            {
                Id = c.Id,
                FullNameOrPetName = c.FullNameOrPetName,
                Relationship = c.Relationship,
                DateOfBirth = c.DateOfBirth,
                IsMinor = c.DateOfBirth.HasValue && c.DateOfBirth.Value > DateTime.UtcNow.AddYears(-18),
                IsPet = c.IsPet,
                PetSpecies = c.PetSpecies,
                PetBreed = c.PetBreed,
                PetSanitaryRegistration = c.PetSanitaryRegistration,
                IsActive = c.IsActive
            })
            .ToListAsync();

        return Ok(members);
    }

    [HttpPost("units/{unitId:guid}/cohabitation")]
    [Authorize(Roles = "SuperAdmin,Admin")]
    public async Task<IActionResult> AddCohabitationMember(Guid unitId, [FromBody] AddCohabitationMemberDto dto)
    {
        var tenantId = GetTenantId();

        var unit = await _context.Units.FirstOrDefaultAsync(u => u.Id == unitId && u.TenantId == tenantId);
        if (unit == null)
        {
            return NotFound(new { message = "Unidad no encontrada." });
        }

        if (dto.IsPet)
        {
            var petsInUnit = await _context.CohabitationGroupMembers
                .CountAsync(c => c.UnitId == unitId && c.TenantId == tenantId && c.IsActive && c.IsPet);

            var maxPets = 3;

            if (petsInUnit >= maxPets)
            {
                return BadRequest(new
                {
                    message = $"Se alcanzó el límite de {maxPets} mascotas por unidad.",
                    currentCount = petsInUnit,
                    limit = maxPets
                });
            }
        }

        var member = new CohabitationGroupMember
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            UnitId = unitId,
            FullNameOrPetName = dto.FullNameOrPetName,
            Relationship = dto.Relationship,
            DateOfBirth = dto.DateOfBirth,
            IsPet = dto.IsPet,
            PetSpecies = dto.PetSpecies,
            PetBreed = dto.PetBreed,
            PetSanitaryRegistration = dto.PetSanitaryRegistration,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            CreatedByUserId = GetUserId()
        };

        _context.CohabitationGroupMembers.Add(member);
        await _context.SaveChangesAsync();

        return Ok(new { member.Id, message = "Integrante registrado exitosamente." });
    }

    [HttpPost("units/{unitId:guid}/cohabitation/{memberId:guid}/deactivate")]
    [Authorize(Roles = "SuperAdmin,Admin")]
    public async Task<IActionResult> RemoveCohabitationMember(Guid unitId, Guid memberId)
    {
        var tenantId = GetTenantId();

        var member = await _context.CohabitationGroupMembers
            .FirstOrDefaultAsync(c => c.Id == memberId && c.UnitId == unitId && c.TenantId == tenantId && c.IsActive);

        if (member == null)
        {
            return NotFound(new { message = "Integrante no encontrado." });
        }

        member.IsActive = false;
        await _context.SaveChangesAsync();
        return NoContent();
    }

    // ── VISTAS CONSOLIDADAS ───────────────────────────────────────────────────

    [HttpGet("units/{unitId:guid}/occupants")]
    public async Task<IActionResult> GetUnitOccupants(Guid unitId)
    {
        var tenantId = GetTenantId();

        var unit = await _context.Units
            .FirstOrDefaultAsync(u => u.Id == unitId && u.TenantId == tenantId);

        if (unit == null)
        {
            return NotFound(new { message = "Unidad no encontrada." });
        }

        var activeOwners = await _context.UnitOwners
            .Include(uo => uo.Owner)
            .Where(uo => uo.UnitId == unitId && uo.TenantId == tenantId && uo.IsActive)
            .Select(uo => new UnitOwnerSummaryDto
            {
                AssignmentId = uo.Id,
                UnitId = uo.UnitId,
                UnitIdentifier = unit.Identifier,
                OwnerId = uo.OwnerId,
                OwnerName = uo.Owner != null ? uo.Owner.FullNameOrCompanyName : string.Empty,
                OwnerDocumentNumber = uo.Owner != null ? uo.Owner.DocumentNumber : string.Empty,
                OwnerDocumentType = uo.Owner != null ? uo.Owner.DocumentType.ToString() : string.Empty,
                OwnershipPercentage = uo.OwnershipPercentage,
                IsSpokesperson = uo.IsSpokesperson,
                ResidesInUnit = uo.ResidesInUnit,
                StartDate = uo.StartDate,
                EndDate = uo.EndDate
            })
            .ToListAsync();

        var activeTenant = await _context.TenantResidents
            .Where(t => t.UnitId == unitId && t.TenantId == tenantId && t.IsActive)
            .Select(t => new TenantResidentDto
            {
                Id = t.Id,
                UnitId = t.UnitId,
                DocumentType = t.DocumentType.ToString(),
                DocumentNumber = t.DocumentNumber,
                FullName = t.FullName,
                Email = t.Email,
                Phone = t.Phone,
                LeaseStartDate = t.LeaseStartDate,
                LeaseEndDate = t.LeaseEndDate,
                RealEstateAgentName = t.RealEstateAgentName,
                RealEstateAgentPhone = t.RealEstateAgentPhone,
                AuthorizedToPayAdmin = t.AuthorizedToPayAdmin,
                IsActive = t.IsActive,
                DaysUntilLeaseExpires = t.LeaseEndDate.HasValue
                    ? (int)(t.LeaseEndDate.Value - DateTime.UtcNow).TotalDays
                    : (int?)null
            })
            .FirstOrDefaultAsync();

        var cohabitationMembers = await _context.CohabitationGroupMembers
            .Where(c => c.UnitId == unitId && c.TenantId == tenantId && c.IsActive)
            .Select(c => new CohabitationMemberDto
            {
                Id = c.Id,
                FullNameOrPetName = c.FullNameOrPetName,
                Relationship = c.Relationship,
                DateOfBirth = c.DateOfBirth,
                IsMinor = c.DateOfBirth.HasValue && c.DateOfBirth.Value > DateTime.UtcNow.AddYears(-18),
                IsPet = c.IsPet,
                PetSpecies = c.PetSpecies,
                PetBreed = c.PetBreed,
                PetSanitaryRegistration = c.PetSanitaryRegistration,
                IsActive = c.IsActive
            })
            .ToListAsync();

        var spokesperson = activeOwners.FirstOrDefault(o => o.IsSpokesperson);

        var result = new UnitOccupantsDto
        {
            UnitId = unitId,
            UnitIdentifier = unit.Identifier,
            ActiveOwners = activeOwners,
            ActiveTenant = activeTenant,
            CohabitationMembers = cohabitationMembers,
            SpokespersonOwnerId = spokesperson?.AssignmentId
        };

        return Ok(result);
    }

    [HttpGet("units/{unitId:guid}/owner-history")]
    public async Task<IActionResult> GetOwnerHistory(Guid unitId)
    {
        var tenantId = GetTenantId();

        var unit = await _context.Units
            .FirstOrDefaultAsync(u => u.Id == unitId && u.TenantId == tenantId);

        if (unit == null)
        {
            return NotFound(new { message = "Unidad no encontrada." });
        }

        var history = await _context.OwnerHistories
            .Include(h => h.Owner)
            .Where(h => h.UnitId == unitId && h.TenantId == tenantId)
            .OrderByDescending(h => h.StartDate)
            .Select(h => new OwnerHistoryEntryDto
            {
                Id = h.Id,
                OwnerId = h.OwnerId,
                OwnerName = h.Owner != null ? h.Owner.FullNameOrCompanyName : string.Empty,
                OwnerDocument = h.Owner != null ? h.Owner.DocumentNumber : string.Empty,
                StartDate = h.StartDate,
                EndDate = h.EndDate,
                TransferNotes = h.TransferNotes,
                RecordedAt = h.RecordedAt
            })
            .ToListAsync();

        return Ok(history);
    }

    [HttpGet("owners/{ownerId:guid}/contact-history")]
    public async Task<IActionResult> GetOwnerContactHistory(Guid ownerId)
    {
        var tenantId = GetTenantId();

        var ownerExists = await _context.Owners
            .AnyAsync(o => o.Id == ownerId && o.TenantId == tenantId);

        if (!ownerExists)
        {
            return NotFound(new { message = "Propietario no encontrado." });
        }

        var history = await _context.ContactHistories
            .Where(ch => ch.OwnerId == ownerId && ch.TenantId == tenantId)
            .OrderByDescending(ch => ch.ChangedAt)
            .Select(ch => new ContactHistoryDto
            {
                Id = ch.Id,
                FieldChanged = ch.FieldChanged,
                OldValue = ch.OldValue,
                NewValue = ch.NewValue,
                ChangedAt = ch.ChangedAt,
                ChangedByUserId = ch.ChangedByUserId
            })
            .ToListAsync();

        return Ok(history);
    }

    // ── TRANSFERENCIA DE PROPIEDAD ────────────────────────────────────────────

    [HttpPost("units/{unitId:guid}/transfer")]
    [Authorize(Roles = "SuperAdmin,Admin")]
    public async Task<IActionResult> TransferProperty(Guid unitId, [FromBody] TransferPropertyDto dto)
    {
        var tenantId = GetTenantId();
        var userId = GetUserId();

        var unit = await _context.Units.FirstOrDefaultAsync(u => u.Id == unitId && u.TenantId == tenantId);
        if (unit == null)
        {
            return NotFound(new { message = "Unidad no encontrada." });
        }

        var newOwner = await _context.Owners
            .FirstOrDefaultAsync(o => o.Id == dto.NewOwnerId && o.TenantId == tenantId && o.IsActive);

        if (newOwner == null)
        {
            return NotFound(new { message = "El nuevo propietario no existe o está inactivo." });
        }

        var currentOwners = await _context.UnitOwners
            .Where(uo => uo.UnitId == unitId && uo.TenantId == tenantId && uo.IsActive)
            .ToListAsync();

        if (!currentOwners.Any())
        {
            return BadRequest(new { message = "La unidad no tiene propietarios activos para transferir." });
        }

        var alreadyOwner = currentOwners.Any(uo => uo.OwnerId == dto.NewOwnerId);
        if (alreadyOwner)
        {
            return Conflict(new { message = "El nuevo propietario ya figura como propietario activo de esta unidad." });
        }

        var outstandingDebt = await _context.UnitFees
            .Where(uf => uf.TenantId == tenantId && uf.UnitId == unitId && uf.BalanceAmount > 0)
            .SumAsync(uf => uf.BalanceAmount);

        var extraordinaryDebt = await _context.ExtraordinaryFeeDistributions
            .Where(ed => ed.TenantId == tenantId && ed.UnitId == unitId && ed.BalanceAmount > 0)
            .SumAsync(ed => ed.BalanceAmount);

        var chargesDebt = await _context.IndividualCharges
            .Where(ic => ic.TenantId == tenantId && ic.UnitId == unitId && ic.BalanceAmount > 0)
            .SumAsync(ic => ic.BalanceAmount);

        var totalDebt = outstandingDebt + extraordinaryDebt + chargesDebt;

        if (dto.GeneratePazYSalvo && totalDebt > 0)
        {
            return BadRequest(new
            {
                message = "No se puede generar paz y salvo. La unidad presenta saldos pendientes.",
                totalDebt
            });
        }

        foreach (var current in currentOwners)
        {
            current.IsActive = false;
            current.EndDate = dto.TransferDate;

            var openHistory = await _context.OwnerHistories
                .Where(h => h.UnitId == unitId && h.OwnerId == current.OwnerId && h.TenantId == tenantId && h.EndDate == null)
                .OrderByDescending(h => h.StartDate)
                .FirstOrDefaultAsync();

            if (openHistory != null)
            {
                openHistory.EndDate = dto.TransferDate;
            }
        }

        var newUnitOwner = new UnitOwner
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            UnitId = unitId,
            OwnerId = dto.NewOwnerId,
            OwnershipPercentage = dto.OwnershipPercentage,
            IsSpokesperson = dto.IsSpokesperson,
            ResidesInUnit = dto.ResidesInUnit,
            StartDate = dto.TransferDate,
            IsActive = true
        };

        _context.UnitOwners.Add(newUnitOwner);

        _context.OwnerHistories.Add(new OwnerHistory
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            UnitId = unitId,
            OwnerId = dto.NewOwnerId,
            StartDate = dto.TransferDate,
            EndDate = null,
            TransferNotes = dto.TransferNotes,
            RecordedAt = DateTime.UtcNow,
            RecordedByUserId = userId
        });

        if (dto.IsSpokesperson)
        {
            var previousSpokesperson = currentOwners.FirstOrDefault(o => o.IsSpokesperson);

            _context.SpokespersonHistories.Add(new SpokespersonHistory
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                UnitId = unitId,
                PreviousSpokespersonId = previousSpokesperson?.OwnerId,
                NewSpokespersonId = dto.NewOwnerId,
                ChangedAt = dto.TransferDate,
                ChangedByUserId = userId,
                ChangeReason = "Transferencia de propiedad"
            });
        }

        object pazYSalvoInfo;

        if (dto.GeneratePazYSalvo && totalDebt == 0)
        {
            pazYSalvoInfo = new
            {
                generated = true,
                certificateId = Guid.NewGuid(),
                unitId,
                unitIdentifier = unit.Identifier,
                ownerName = newOwner.FullNameOrCompanyName,
                transferDate = dto.TransferDate,
                generatedAt = DateTime.UtcNow,
                generatedByUserId = userId,
                message = "Paz y salvo generado. La unidad no presenta obligaciones pendientes."
            };
        }
        else
        {
            pazYSalvoInfo = new
            {
                generated = false,
                totalDebt,
                message = totalDebt > 0
                    ? "La transferencia se realizó con saldos pendientes. El nuevo propietario asume las deudas de la unidad."
                    : "Paz y salvo no solicitado."
            };
        }

        await _context.SaveChangesAsync();
        await _indicatorCache.InvalidateAsync(tenantId, PaymentStatusMapService.CacheKeyPrefix);

        await _notificationService.CreateAsync(
            tenantId, newOwner.Id,
            "Transferencia de propiedad",
            $"Se ha registrado la transferencia de la unidad {unit.Identifier} a su nombre. Fecha de transferencia: {dto.TransferDate:yyyy-MM-dd}.");

        return Ok(new
        {
            message = "Transferencia registrada exitosamente.",
            newAssignmentId = newUnitOwner.Id,
            pazYSalvo = pazYSalvoInfo
        });
    }

    // ── LISTADO GLOBAL DE ARRENDATARIOS ──────────────────────────────────────

    [HttpGet("tenants")]
    [Authorize(Roles = "SuperAdmin,Admin,Council,Auditor")]
    public async Task<IActionResult> GetTenants(
        [FromQuery] string? search,
        [FromQuery] bool includeInactive = false)
    {
        var tenantId = GetTenantId();

        var query = _context.TenantResidents
            .Include(t => t.Unit)
            .Where(t => t.TenantId == tenantId);

        if (!includeInactive)
        {
            query = query.Where(t => t.IsActive);
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(t =>
                t.FullName.Contains(search) ||
                t.DocumentNumber.Contains(search) ||
                t.Email.Contains(search) ||
                (t.Unit != null && t.Unit.Identifier.Contains(search)));
        }

        var rawList = await query
            .OrderByDescending(t => t.IsActive)
            .ThenBy(t => t.FullName)
            .Select(t => new
            {
                t.Id,
                t.UnitId,
                UnitIdentifier = t.Unit != null ? t.Unit.Identifier : string.Empty,
                DocumentType = t.DocumentType.ToString(),
                t.DocumentNumber,
                t.FullName,
                t.Email,
                t.Phone,
                t.LeaseStartDate,
                t.LeaseEndDate,
                t.AuthorizedToPayAdmin,
                t.IsActive,
            })
            .ToListAsync();

        var now = DateTime.UtcNow;
        var list = rawList.Select(t => new TenantResidentListItemDto
        {
            Id = t.Id,
            UnitId = t.UnitId,
            UnitIdentifier = t.UnitIdentifier,
            DocumentType = t.DocumentType,
            DocumentNumber = t.DocumentNumber,
            FullName = t.FullName,
            Email = t.Email,
            Phone = t.Phone,
            LeaseStartDate = t.LeaseStartDate,
            LeaseEndDate = t.LeaseEndDate,
            AuthorizedToPayAdmin = t.AuthorizedToPayAdmin,
            IsActive = t.IsActive,
            DaysUntilLeaseExpires = t.LeaseEndDate.HasValue
                ? (int?)((t.LeaseEndDate.Value - now).TotalDays)
                : null
        }).ToList();

        return Ok(list);
    }

    [HttpGet("tenants/{id:guid}")]
    [Authorize(Roles = "SuperAdmin,Admin,Council,Auditor")]
    public async Task<IActionResult> GetTenantDetail(Guid id)
    {
        var tenantId = GetTenantId();

        var tenant = await _context.TenantResidents
            .Include(t => t.Unit)
            .FirstOrDefaultAsync(t => t.Id == id && t.TenantId == tenantId);

        if (tenant == null)
            return NotFound(new { message = "Arrendatario no encontrado." });

        return Ok(new TenantResidentDto
        {
            Id = tenant.Id,
            UnitId = tenant.UnitId,
            UnitIdentifier = tenant.Unit != null ? tenant.Unit.Identifier : string.Empty,
            DocumentType = tenant.DocumentType.ToString(),
            DocumentNumber = tenant.DocumentNumber,
            FullName = tenant.FullName,
            Email = tenant.Email,
            Phone = tenant.Phone,
            LeaseStartDate = tenant.LeaseStartDate,
            LeaseEndDate = tenant.LeaseEndDate,
            RealEstateAgentName = tenant.RealEstateAgentName,
            RealEstateAgentPhone = tenant.RealEstateAgentPhone,
            AuthorizedToPayAdmin = tenant.AuthorizedToPayAdmin,
            IsActive = tenant.IsActive,
            DaysUntilLeaseExpires = tenant.LeaseEndDate.HasValue
                ? (int?)(tenant.LeaseEndDate.Value - DateTime.UtcNow).TotalDays
                : null
        });
    }

    [HttpPut("tenants/{id:guid}")]
    [Authorize(Roles = "SuperAdmin,Admin")]
    public async Task<IActionResult> UpdateTenant(Guid id, [FromBody] UpdateTenantResidentDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var tenantId = GetTenantId();

        var tenant = await _context.TenantResidents
            .FirstOrDefaultAsync(t => t.Id == id && t.TenantId == tenantId);

        if (tenant == null)
            return NotFound(new { message = "Arrendatario no encontrado." });

        if (!tenant.IsActive)
            return BadRequest(new { message = "No se puede modificar un arrendatario inactivo." });

        tenant.Email = dto.Email;
        tenant.Phone = dto.Phone;
        tenant.LeaseStartDate = dto.LeaseStartDate;
        tenant.LeaseEndDate = dto.LeaseEndDate;
        tenant.RealEstateAgentName = dto.RealEstateAgentName;
        tenant.RealEstateAgentPhone = dto.RealEstateAgentPhone;
        tenant.AuthorizedToPayAdmin = dto.AuthorizedToPayAdmin;

        await _context.SaveChangesAsync();

        return Ok(new { message = "Arrendatario actualizado exitosamente." });
    }

    // ── HELPERS PRIVADOS ──────────────────────────────────────────────────────

    private System.Collections.Generic.List<ContactHistory> BuildContactChanges(
        Owner current, UpdateNaturalPersonOwnerDto updated, string userId, string tenantId)
    {
        var changes = new System.Collections.Generic.List<ContactHistory>();
        var now = DateTime.UtcNow;

        void Track(string field, string? oldVal, string? newVal)
        {
            if (oldVal != newVal)
            {
                changes.Add(new ContactHistory
                {
                    Id = Guid.NewGuid(),
                    TenantId = tenantId,
                    OwnerId = current.Id,
                    FieldChanged = field,
                    OldValue = oldVal,
                    NewValue = newVal,
                    ChangedAt = now,
                    ChangedByUserId = userId
                });
            }
        }

        Track("FullName", current.FullNameOrCompanyName, updated.FullName);
        Track("Email", current.Email, updated.Email);
        Track("MainPhone", current.MainPhone, updated.MainPhone);
        Track("AlternativePhone", current.AlternativePhone, updated.AlternativePhone);
        Track("CorrespondenceAddress", current.CorrespondenceAddress, updated.CorrespondenceAddress);
        Track("CivilStatus", current.CivilStatus, updated.CivilStatus);
        Track("ChangeReason", null, updated.ChangeReason);

        return changes;
    }

    private System.Collections.Generic.List<ContactHistory> BuildLegalEntityContactChanges(
        Owner current, UpdateLegalEntityOwnerDto updated, string userId, string tenantId)
    {
        var changes = new System.Collections.Generic.List<ContactHistory>();
        var now = DateTime.UtcNow;

        void Track(string field, string? oldVal, string? newVal)
        {
            if (oldVal != newVal)
            {
                changes.Add(new ContactHistory
                {
                    Id = Guid.NewGuid(),
                    TenantId = tenantId,
                    OwnerId = current.Id,
                    FieldChanged = field,
                    OldValue = oldVal,
                    NewValue = newVal,
                    ChangedAt = now,
                    ChangedByUserId = userId
                });
            }
        }

        Track("Email", current.Email, updated.Email);
        Track("MainPhone", current.MainPhone, updated.MainPhone);
        Track("AlternativePhone", current.AlternativePhone, updated.AlternativePhone);
        Track("CorrespondenceAddress", current.CorrespondenceAddress, updated.FiscalAddress);
        Track("LegalRepresentativeName", current.LegalRepresentativeName, updated.LegalRepresentativeName);
        Track("LegalRepresentativeDocument", current.LegalRepresentativeDocument, updated.LegalRepresentativeDocument);
        Track("LegalRepresentativeRole", current.LegalRepresentativeRole, updated.LegalRepresentativeRole);
        Track("ChangeReason", null, updated.ChangeReason);

        return changes;
    }
}
