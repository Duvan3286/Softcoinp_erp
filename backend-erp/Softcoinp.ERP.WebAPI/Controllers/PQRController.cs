using System;
using System.Linq;
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
[Route("api/pqr")]
[Authorize]
public class PQRController : BaseController
{
    private readonly PQRRadicationService _radicationService;
    private readonly ClaimResolutionService _claimResolutionService;
    private readonly ApplicationDbContext _context;

    public PQRController(
        PQRRadicationService radicationService,
        ClaimResolutionService claimResolutionService,
        ApplicationDbContext context)
    {
        _radicationService = radicationService;
        _claimResolutionService = claimResolutionService;
        _context = context;
    }

    [HttpPost]
    [Authorize(Roles = "SuperAdmin,Admin,Accountant")]
    public async Task<IActionResult> CreatePqr([FromBody] CreatePqrRequestDto request)
    {
        var tenantId = GetTenantId();
        var userId = GetUserId();

        try
        {
            var result = await _radicationService.RadicateAsync(tenantId, userId, request);
            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpGet]
    [Authorize(Roles = "SuperAdmin,Admin,Accountant,Council,Auditor")]
    public async Task<IActionResult> GetPqrList(
        [FromQuery] string? status = null,
        [FromQuery] string? type = null,
        [FromQuery] bool? isInternal = null)
    {
        var tenantId = GetTenantId();

        var query = _context.PqrRecords
            .Include(p => p.Unit)
            .Where(p => p.TenantId == tenantId);

        if (!string.IsNullOrEmpty(status) && Enum.TryParse<PQRStatus>(status, true, out var pqrStatus))
        {
            query = query.Where(p => p.Status == pqrStatus);
        }

        if (!string.IsNullOrEmpty(type) && Enum.TryParse<PQRType>(type, true, out var pqrType))
        {
            query = query.Where(p => p.PQRType == pqrType);
        }

        if (isInternal.HasValue)
        {
            query = query.Where(p => p.IsInternal == isInternal.Value);
        }
        else
        {
            query = query.Where(p => !p.IsInternal);
        }

        var now = DateTime.UtcNow;

        var list = await query
            .OrderByDescending(p => p.Priority == PQRPriority.High ? 0 :
                                    p.Priority == PQRPriority.Medium ? 1 : 2)
            .ThenBy(p => p.Deadline)
            .Select(p => new PqrListDto
            {
                Id = p.Id,
                RadicadoNumber = p.RadicadoNumber,
                PQRType = p.PQRType.ToString(),
                Category = p.Category.ToString(),
                Status = p.Status.ToString(),
                Priority = p.Priority.ToString(),
                Subject = p.Subject,
                UnitIdentifier = p.Unit.Identifier,
                RadiadorName = p.RadiadorName,
                FiledAt = p.FiledAt,
                Deadline = p.Deadline,
                ElapsedPercent = p.Deadline != null
                    ? (int)((now - p.FiledAt).TotalMinutes * 100 /
                        Math.Max(1, (p.Deadline.Value - p.FiledAt).TotalMinutes))
                    : 0,
                IsInternal = p.IsInternal
            })
            .ToListAsync();

        return Ok(list);
    }

    [HttpGet("{id}")]
    [Authorize(Roles = "SuperAdmin,Admin,Accountant,Council,Auditor")]
    public async Task<IActionResult> GetPqrDetail(Guid id)
    {
        var tenantId = GetTenantId();

        var pqr = await _context.PqrRecords
            .Include(p => p.Unit)
            .Include(p => p.RelatedPQR)
            .Include(p => p.FollowUps)
            .Include(p => p.Responses)
                .ThenInclude(r => r.Files)
            .Include(p => p.InternalNotes)
            .Include(p => p.Files)
            .Include(p => p.Alerts)
            .FirstOrDefaultAsync(p => p.Id == id && p.TenantId == tenantId);

        if (pqr == null)
        {
            return NotFound("PQR no encontrada.");
        }

        var now = DateTime.UtcNow;
        var userRole = User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value ?? string.Empty;
        var isAdminRole = userRole == "SuperAdmin" || userRole == "Admin" || userRole == "Accountant" || userRole == "Council";

        var detail = new PqrDetailDto
        {
            Id = pqr.Id,
            RadicadoNumber = pqr.RadicadoNumber,
            PQRType = pqr.PQRType.ToString(),
            Category = pqr.Category.ToString(),
            Status = pqr.Status.ToString(),
            Priority = pqr.Priority.ToString(),
            Subject = pqr.Subject,
            Description = pqr.Description,
            RadiadorName = pqr.RadiadorName,
            RadiadorDocumentType = pqr.RadiadorDocumentType,
            RadiadorDocumentNumber = pqr.RadiadorDocumentNumber,
            RadiadorContact = pqr.RadiadorContact,
            UnitId = pqr.UnitId,
            UnitIdentifier = pqr.Unit?.Identifier ?? string.Empty,
            Channel = pqr.Channel.ToString(),
            RelatedPQRId = pqr.RelatedPQRId,
            RelatedRadicadoNumber = pqr.RelatedPQR?.RadicadoNumber,
            AssignedToUserId = pqr.AssignedToUserId,
            Deadline = pqr.Deadline,
            ElapsedPercent = pqr.Deadline != null
                ? (int)((now - pqr.FiledAt).TotalMinutes * 100 /
                    Math.Max(1, (pqr.Deadline.Value - pqr.FiledAt).TotalMinutes))
                : 0,
            IsInternal = pqr.IsInternal,
            InvolvedResidentName = pqr.InvolvedResidentName,
            InvolvedResidentUnitId = pqr.InvolvedResidentUnitId,
            IsLinkedToCharge = pqr.IsLinkedToCharge,
            ClaimResolved = pqr.ClaimResolved,
            ClaimResolutionNote = pqr.ClaimResolutionNote,
            CreditNoteGenerated = pqr.CreditNoteGenerated,
            FiledAt = pqr.FiledAt,
            ClosedAt = pqr.ClosedAt,
            ClosedDefinitivelyAt = pqr.ClosedDefinitivelyAt,
            FollowUps = pqr.FollowUps.OrderBy(f => f.ChangedAt).Select(f => new PqrFollowUpDto
            {
                Id = f.Id,
                PreviousStatus = f.PreviousStatus.ToString(),
                NewStatus = f.NewStatus.ToString(),
                ChangedAt = f.ChangedAt,
                ChangedByUserName = f.ChangedByUserName,
                Justification = f.Justification,
                IsAutomatic = f.IsAutomatic
            }).ToList(),
            Responses = pqr.Responses.OrderByDescending(r => r.SentAt).Select(r => new PqrResponseDto
            {
                Id = r.Id,
                ResponseText = r.ResponseText,
                IsDefinitive = r.IsDefinitive,
                IsPartialUpdate = r.IsPartialUpdate,
                SentAt = r.SentAt,
                SentByUserName = r.SentByUserName,
                RequiresConfirmation = r.RequiresConfirmation,
                ConfirmedByRadiador = r.ConfirmedByRadiador,
                ConfirmedAt = r.ConfirmedAt,
                Files = r.Files.Select(f => new PqrFileDto
                {
                    Id = f.Id,
                    FileName = f.FileName,
                    OriginalFileName = f.OriginalFileName,
                    ContentType = f.ContentType,
                    FileSize = f.FileSize,
                    UploadedByUserName = f.UploadedByUserName,
                    UploadedAt = f.UploadedAt,
                    IsFromApplicant = f.IsFromApplicant
                }).ToList()
            }).ToList(),
            InternalNotes = isAdminRole
                ? pqr.InternalNotes.OrderByDescending(n => n.CreatedAt).Select(n => new PqrInternalNoteDto
                {
                    Id = n.Id,
                    NoteText = n.NoteText,
                    AuthorName = n.AuthorName,
                    CreatedAt = n.CreatedAt
                }).ToList()
                : new List<PqrInternalNoteDto>(),
            Files = pqr.Files.Select(f => new PqrFileDto
            {
                Id = f.Id,
                FileName = f.FileName,
                OriginalFileName = f.OriginalFileName,
                ContentType = f.ContentType,
                FileSize = f.FileSize,
                UploadedByUserName = f.UploadedByUserName,
                UploadedAt = f.UploadedAt,
                IsFromApplicant = f.IsFromApplicant
            }).ToList(),
            Alerts = pqr.Alerts.OrderByDescending(a => a.GeneratedAt).Select(a => new PqrAlertDto
            {
                Id = a.Id,
                AlertType = a.AlertType.ToString(),
                GeneratedAt = a.GeneratedAt,
                IsActive = a.IsActive,
                ResolvedAt = a.ResolvedAt,
                EscalatedToCouncil = a.EscalatedToCouncil
            }).ToList()
        };

        return Ok(detail);
    }

    [HttpGet("time-config")]
    [Authorize(Roles = "SuperAdmin,Admin")]
    public async Task<IActionResult> GetTimeConfig()
    {
        var tenantId = GetTenantId();

        var configs = await _context.PqrTimeConfigs
            .Where(c => c.TenantId == tenantId)
            .ToListAsync();

        var defaultConfigs = new List<PqrTimeConfigDto>
        {
            new() { PQRType = "Request", BusinessDays = 5 },
            new() { PQRType = "Complaint", BusinessDays = 3 },
            new() { PQRType = "Claim", BusinessDays = 10 }
        };

        foreach (var defaultConfig in defaultConfigs)
        {
            var existing = configs.FirstOrDefault(c =>
                c.PQRType.ToString() == defaultConfig.PQRType);

            if (existing != null)
            {
                defaultConfig.BusinessDays = existing.BusinessDays;
            }
        }

        return Ok(defaultConfigs);
    }

    [HttpPut("time-config")]
    [Authorize(Roles = "SuperAdmin,Admin")]
    public async Task<IActionResult> UpdateTimeConfig([FromBody] UpdatePqrTimeConfigRequestDto request)
    {
        var tenantId = GetTenantId();

        if (!Enum.TryParse<PQRType>(request.PQRType, true, out var pqrType))
        {
            return BadRequest("Tipo de PQR inválido.");
        }

        if (request.BusinessDays < 1)
        {
            return BadRequest("Los días hábiles deben ser al menos 1.");
        }

        var config = await _context.PqrTimeConfigs
            .FirstOrDefaultAsync(c => c.TenantId == tenantId && c.PQRType == pqrType);

        if (config == null)
        {
            config = new PqrTimeConfig
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                PQRType = pqrType,
                BusinessDays = request.BusinessDays
            };
            _context.PqrTimeConfigs.Add(config);
        }
        else
        {
            config.BusinessDays = request.BusinessDays;
        }

        config.UpdatedAt = DateTime.UtcNow;
        config.UpdatedByUserId = GetUserId();

        await _context.SaveChangesAsync();

        return Ok(new { message = "Configuración actualizada exitosamente." });
    }

    [HttpGet("resident/{ownerId}")]
    [Authorize(Roles = "SuperAdmin,Admin,Resident")]
    public async Task<IActionResult> GetResidentPqrs(Guid ownerId)
    {
        var tenantId = GetTenantId();

        var pqrs = await _context.PqrRecords
            .Include(p => p.Unit)
            .Where(p => p.TenantId == tenantId && p.OwnerId == ownerId && !p.IsInternal)
            .OrderByDescending(p => p.FiledAt)
            .Select(p => new PqrListDto
            {
                Id = p.Id,
                RadicadoNumber = p.RadicadoNumber,
                PQRType = p.PQRType.ToString(),
                Category = p.Category.ToString(),
                Status = p.Status.ToString(),
                Priority = p.Priority.ToString(),
                Subject = p.Subject,
                UnitIdentifier = p.Unit.Identifier,
                FiledAt = p.FiledAt,
                Deadline = p.Deadline,
                IsInternal = p.IsInternal
            })
            .ToListAsync();

        return Ok(pqrs);
    }

    [HttpGet("alerts/active")]
    [Authorize(Roles = "SuperAdmin,Admin,Accountant,Council")]
    public async Task<IActionResult> GetActiveAlerts()
    {
        var tenantId = GetTenantId();

        var alerts = await _context.PqrAlerts
            .Include(a => a.PQR)
                .ThenInclude(p => p.Unit)
            .Where(a => a.IsActive && a.PQR.TenantId == tenantId)
            .OrderByDescending(a => a.AlertType == PQRAlertType.Overdue ? 0 :
                                    a.AlertType == PQRAlertType.EightyPercent ? 1 : 2)
            .ThenBy(a => a.GeneratedAt)
            .Select(a => new
            {
                a.Id,
                AlertType = a.AlertType.ToString(),
                a.GeneratedAt,
                a.EscalatedToCouncil,
                PQR = new
                {
                    a.PQR.Id,
                    a.PQR.RadicadoNumber,
                    a.PQR.PQRType,
                    a.PQR.Status,
                    a.PQR.Subject,
                    UnitIdentifier = a.PQR.Unit.Identifier,
                    a.PQR.Deadline,
                    a.PQR.FiledAt
                }
            })
            .ToListAsync();

        return Ok(alerts);
    }

    [HttpPost("alerts/{alertId}/resolve")]
    [Authorize(Roles = "SuperAdmin,Admin,Accountant")]
    public async Task<IActionResult> ResolveAlert(Guid alertId)
    {
        var tenantId = GetTenantId();

        var alert = await _context.PqrAlerts
            .Include(a => a.PQR)
            .FirstOrDefaultAsync(a => a.Id == alertId && a.PQR.TenantId == tenantId);

        if (alert == null)
        {
            return NotFound("Alerta no encontrada.");
        }

        alert.IsActive = false;
        alert.ResolvedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return Ok(new { message = "Alerta resuelta exitosamente." });
    }

    [HttpGet("indicators")]
    [Authorize(Roles = "SuperAdmin,Admin,Accountant,Council,Auditor")]
    public async Task<IActionResult> GetPQRIndicators()
    {
        var tenantId = GetTenantId();

        var pqrs = await _context.PqrRecords
            .Include(p => p.FollowUps)
            .Where(p => p.TenantId == tenantId && !p.IsInternal)
            .ToListAsync();

        var now = DateTime.UtcNow;

        var openPqrs = pqrs.Where(p => p.Status != PQRStatus.Closed && p.Status != PQRStatus.Responded).ToList();
        var closedPqrs = pqrs.Where(p => p.Status == PQRStatus.Closed).ToList();
        var escalatedPqrs = pqrs.Where(p => p.Status == PQRStatus.Escalated).ToList();
        var activeAlerts = await _context.PqrAlerts
            .CountAsync(a => a.IsActive && a.PQR.TenantId == tenantId);

        var byType = pqrs.GroupBy(p => p.PQRType)
            .Select(g => new
            {
                Type = g.Key.ToString(),
                Count = g.Count(),
                OpenCount = g.Count(p => p.Status != PQRStatus.Closed && p.Status != PQRStatus.Responded)
            }).ToList();

        var byCategory = pqrs.GroupBy(p => p.Category)
            .Select(g => new
            {
                Category = g.Key.ToString(),
                Count = g.Count()
            }).ToList();

        var byStatus = pqrs.GroupBy(p => p.Status)
            .Select(g => new
            {
                Status = g.Key.ToString(),
                Count = g.Count()
            }).ToList();

        var monthlyTrend = pqrs
            .GroupBy(p => new { p.FiledAt.Year, p.FiledAt.Month })
            .OrderBy(g => g.Key.Year).ThenBy(g => g.Key.Month)
            .Select(g => new
            {
                Period = $"{g.Key.Year}-{g.Key.Month:D2}",
                Count = g.Count()
            }).ToList();

        var responded = pqrs.Where(p =>
            p.Status == PQRStatus.Responded || p.Status == PQRStatus.Closed);

        var avgResponseHours = responded.Any()
            ? Math.Round(responded
                .Select(p =>
                {
                    var firstResponse = p.FollowUps
                        .Where(f => f.NewStatus == PQRStatus.Responded)
                        .OrderBy(f => f.ChangedAt)
                        .FirstOrDefault();
                    if (firstResponse != null)
                    {
                        return (firstResponse.ChangedAt - p.FiledAt).TotalHours;
                    }
                    if (p.ClosedAt.HasValue)
                    {
                        return (p.ClosedAt.Value - p.FiledAt).TotalHours;
                    }
                    return 0d;
                })
                .DefaultIfEmpty(0)
                .Average(), 1)
            : 0d;

        var avgResponseByType = responded
            .GroupBy(p => p.PQRType)
            .Select(g =>
            {
                var avg = g
                    .Select(p =>
                    {
                        var firstResponse = p.FollowUps
                            .Where(f => f.NewStatus == PQRStatus.Responded)
                            .OrderBy(f => f.ChangedAt)
                            .FirstOrDefault();
                        if (firstResponse != null)
                        {
                            return (firstResponse.ChangedAt - p.FiledAt).TotalHours;
                        }
                        if (p.ClosedAt.HasValue)
                        {
                            return (p.ClosedAt.Value - p.FiledAt).TotalHours;
                        }
                        return 0d;
                    })
                    .DefaultIfEmpty(0)
                    .Average();

                return new
                {
                    Type = g.Key.ToString(),
                    AverageResponseHours = Math.Round(avg, 1),
                    Count = g.Count()
                };
            }).ToList();

        return Ok(new
        {
            TotalPQRs = pqrs.Count,
            OpenPQRs = openPqrs.Count,
            ClosedPQRs = closedPqrs.Count,
            EscalatedPQRs = escalatedPqrs.Count,
            ActiveAlerts = activeAlerts,
            AverageResponseHours = avgResponseHours,
            ByType = byType,
            ByCategory = byCategory,
            ByStatus = byStatus,
            MonthlyTrend = monthlyTrend,
            AverageResponseByType = avgResponseByType
        });
    }

    [HttpPost("{id}/resolve-claim")]
    [Authorize(Roles = "SuperAdmin,Admin,Accountant")]
    public async Task<IActionResult> ResolveClaim(Guid id, [FromBody] ResolveClaimRequestDto request)
    {
        var tenantId = GetTenantId();
        var userId = GetUserId();

        try
        {
            await _claimResolutionService.ResolveClaimAsync(
                tenantId, id, request.Resolved, request.ResolutionNote, userId);

            var message = request.Resolved
                ? "Reclamo declarado procedente. Nota de crédito generada automáticamente en el módulo de cuotas."
                : "Reclamo declarado improcedente. No se generaron ajustes.";

            return Ok(new { message });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPut("{id}/status")]
    [Authorize(Roles = "SuperAdmin,Admin,Accountant")]
    public async Task<IActionResult> ChangeStatus(Guid id, [FromBody] ChangePqrStatusRequestDto request)
    {
        var tenantId = GetTenantId();
        var userId = GetUserId();
        var userName = User.Identity?.Name ?? "Administrador";

        if (!Enum.TryParse<PQRStatus>(request.Status, true, out var newStatus))
        {
            return BadRequest("Estado inválido.");
        }

        if (string.IsNullOrWhiteSpace(request.Justification))
        {
            return BadRequest("La justificación es obligatoria para cambiar el estado.");
        }

        var pqr = await _context.PqrRecords
            .FirstOrDefaultAsync(p => p.Id == id && p.TenantId == tenantId);

        if (pqr == null)
        {
            return NotFound("PQR no encontrada.");
        }

        var previousStatus = pqr.Status;
        pqr.Status = newStatus;
        pqr.UpdatedAt = DateTime.UtcNow;

        if (newStatus == PQRStatus.Closed)
        {
            pqr.ClosedAt = DateTime.UtcNow;
            pqr.ClosedDefinitivelyAt = DateTime.UtcNow.AddDays(10);
        }

        var followUp = new PqrFollowUp
        {
            Id = Guid.NewGuid(),
            PQRId = pqr.Id,
            PreviousStatus = previousStatus,
            NewStatus = newStatus,
            ChangedAt = DateTime.UtcNow,
            ChangedByUserId = userId,
            ChangedByUserName = userName,
            Justification = request.Justification,
            IsAutomatic = false
        };

        _context.PqrFollowUps.Add(followUp);
        await _context.SaveChangesAsync();

        return Ok(new { message = $"Estado actualizado a {newStatus}.", previousStatus = previousStatus.ToString(), newStatus = newStatus.ToString() });
    }

    [HttpPut("{id}/assign")]
    [Authorize(Roles = "SuperAdmin,Admin,Accountant")]
    public async Task<IActionResult> AssignPqr(Guid id, [FromBody] AssignPqrRequestDto request)
    {
        var tenantId = GetTenantId();
        var userId = GetUserId();

        if (string.IsNullOrWhiteSpace(request.AssignedToUserId))
        {
            return BadRequest("El ID del usuario asignado es obligatorio.");
        }

        var pqr = await _context.PqrRecords
            .FirstOrDefaultAsync(p => p.Id == id && p.TenantId == tenantId);

        if (pqr == null)
        {
            return NotFound("PQR no encontrada.");
        }

        pqr.AssignedToUserId = request.AssignedToUserId;

        if (pqr.Status == PQRStatus.Filed || pqr.Status == PQRStatus.UnderReview)
        {
            var previousStatus = pqr.Status;
            pqr.Status = PQRStatus.InManagement;
            pqr.UpdatedAt = DateTime.UtcNow;

            var followUp = new PqrFollowUp
            {
                Id = Guid.NewGuid(),
                PQRId = pqr.Id,
                PreviousStatus = previousStatus,
                NewStatus = PQRStatus.InManagement,
                ChangedAt = DateTime.UtcNow,
                ChangedByUserId = userId,
                ChangedByUserName = User.Identity?.Name ?? "Administrador",
                Justification = $"PQR asignada a {request.AssignedToUserName}. Gestión en curso.",
                IsAutomatic = false
            };

            _context.PqrFollowUps.Add(followUp);
        }

        await _context.SaveChangesAsync();

        return Ok(new { message = "PQR asignada exitosamente.", assignedToUserId = request.AssignedToUserId });
    }

    [HttpPut("{id}/priority")]
    [Authorize(Roles = "SuperAdmin,Admin,Accountant")]
    public async Task<IActionResult> UpdatePriority(Guid id, [FromBody] UpdatePqrPriorityRequestDto request)
    {
        var tenantId = GetTenantId();

        if (!Enum.TryParse<PQRPriority>(request.Priority, true, out var priority))
        {
            return BadRequest("Prioridad inválida. Use: High, Medium o Low.");
        }

        var pqr = await _context.PqrRecords
            .FirstOrDefaultAsync(p => p.Id == id && p.TenantId == tenantId);

        if (pqr == null)
        {
            return NotFound("PQR no encontrada.");
        }

        pqr.Priority = priority;
        pqr.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return Ok(new { message = $"Prioridad actualizada a {priority}.", priority = priority.ToString() });
    }

    [HttpPost("{id}/responses")]
    [Authorize(Roles = "SuperAdmin,Admin,Accountant")]
    public async Task<IActionResult> AddResponse(Guid id, [FromBody] AddPqrResponseRequestDto request)
    {
        var tenantId = GetTenantId();
        var userId = GetUserId();
        var userName = User.Identity?.Name ?? "Administrador";

        if (string.IsNullOrWhiteSpace(request.ResponseText))
        {
            return BadRequest("El texto de la respuesta es obligatorio.");
        }

        var pqr = await _context.PqrRecords
            .FirstOrDefaultAsync(p => p.Id == id && p.TenantId == tenantId);

        if (pqr == null)
        {
            return NotFound("PQR no encontrada.");
        }

        var response = new PqrResponse
        {
            Id = Guid.NewGuid(),
            PQRId = pqr.Id,
            ResponseText = request.ResponseText,
            IsDefinitive = request.IsDefinitive,
            IsPartialUpdate = request.IsPartialUpdate,
            SentAt = DateTime.UtcNow,
            SentByUserId = userId,
            SentByUserName = userName,
            RequiresConfirmation = request.RequiresConfirmation
        };

        _context.PqrResponses.Add(response);

        var previousStatus = pqr.Status;
        pqr.Status = PQRStatus.Responded;
        pqr.UpdatedAt = DateTime.UtcNow;

        var followUp = new PqrFollowUp
        {
            Id = Guid.NewGuid(),
            PQRId = pqr.Id,
            PreviousStatus = previousStatus,
            NewStatus = PQRStatus.Responded,
            ChangedAt = DateTime.UtcNow,
            ChangedByUserId = userId,
            ChangedByUserName = userName,
            Justification = request.IsDefinitive
                ? "Respuesta definitiva emitida al radicante."
                : "Actualización parcial enviada al radicante.",
            IsAutomatic = false
        };

        _context.PqrFollowUps.Add(followUp);

        if (request.IsDefinitive)
        {
            pqr.ClosedAt = DateTime.UtcNow;
        }

        await _context.SaveChangesAsync();

        return Ok(new
        {
            id = response.Id,
            message = "Respuesta registrada exitosamente.",
            isDefinitive = request.IsDefinitive,
            requiresConfirmation = request.RequiresConfirmation
        });
    }

    [HttpPost("{id}/responses/{responseId}/confirm")]
    [Authorize(Roles = "SuperAdmin,Admin,Resident")]
    public async Task<IActionResult> ConfirmResponse(Guid id, Guid responseId, [FromBody] ConfirmResponseRequestDto request)
    {
        var tenantId = GetTenantId();

        var response = await _context.PqrResponses
            .Include(r => r.PQR)
            .FirstOrDefaultAsync(r => r.Id == responseId && r.PQRId == id && r.PQR.TenantId == tenantId);

        if (response == null)
        {
            return NotFound("Respuesta no encontrada.");
        }

        response.ConfirmedByRadiador = request.Confirmed;
        response.ConfirmedAt = DateTime.UtcNow;

        if (request.Confirmed && response.IsDefinitive)
        {
            response.PQR.Status = PQRStatus.Closed;
            response.PQR.ClosedAt = DateTime.UtcNow;
            response.PQR.ClosedDefinitivelyAt = DateTime.UtcNow.AddDays(10);
            response.PQR.UpdatedAt = DateTime.UtcNow;
        }

        await _context.SaveChangesAsync();

        var message = request.Confirmed
            ? "Respuesta confirmada. PQR cerrada exitosamente."
            : "Respuesta marcada como no conforme.";

        return Ok(new { message });
    }

    [HttpPost("{id}/internal-notes")]
    [Authorize(Roles = "SuperAdmin,Admin,Accountant,Council")]
    public async Task<IActionResult> AddInternalNote(Guid id, [FromBody] AddPqrInternalNoteRequestDto request)
    {
        var tenantId = GetTenantId();
        var userId = GetUserId();
        var userName = User.Identity?.Name ?? "Administrador";

        if (string.IsNullOrWhiteSpace(request.NoteText))
        {
            return BadRequest("El texto de la nota es obligatorio.");
        }

        var pqr = await _context.PqrRecords
            .FirstOrDefaultAsync(p => p.Id == id && p.TenantId == tenantId);

        if (pqr == null)
        {
            return NotFound("PQR no encontrada.");
        }

        var note = new PqrInternalNote
        {
            Id = Guid.NewGuid(),
            PQRId = pqr.Id,
            NoteText = request.NoteText,
            AuthorName = userName,
            CreatedAt = DateTime.UtcNow,
            CreatedByUserId = userId
        };

        _context.PqrInternalNotes.Add(note);
        await _context.SaveChangesAsync();

        return Ok(new { id = note.Id, message = "Nota interna agregada exitosamente." });
    }

    [HttpPost("{id}/reopen")]
    [Authorize(Roles = "SuperAdmin,Admin,Resident")]
    public async Task<IActionResult> ReopenPqr(Guid id, [FromBody] ReopenPqrRequestDto request)
    {
        var tenantId = GetTenantId();

        if (string.IsNullOrWhiteSpace(request.Justification))
        {
            return BadRequest("La justificación es obligatoria para reabrir la PQR.");
        }

        var pqr = await _context.PqrRecords
            .FirstOrDefaultAsync(p => p.Id == id && p.TenantId == tenantId);

        if (pqr == null)
        {
            return NotFound("PQR no encontrada.");
        }

        if (pqr.Status != PQRStatus.Closed)
        {
            return BadRequest("Solo las PQR en estado Cerrado pueden ser reabiertas.");
        }

        if (pqr.ClosedDefinitivelyAt.HasValue && pqr.ClosedDefinitivelyAt.Value < DateTime.UtcNow)
        {
            return BadRequest("El plazo de 10 días para reabrir la PQR ha vencido. Debe radicar una nueva PQR.");
        }

        var previousStatus = pqr.Status;
        pqr.Status = PQRStatus.Reopened;
        pqr.ClosedAt = null;
        pqr.UpdatedAt = DateTime.UtcNow;

        var followUp = new PqrFollowUp
        {
            Id = Guid.NewGuid(),
            PQRId = pqr.Id,
            PreviousStatus = previousStatus,
            NewStatus = PQRStatus.Reopened,
            ChangedAt = DateTime.UtcNow,
            ChangedByUserId = GetUserId(),
            ChangedByUserName = User.Identity?.Name ?? "Radicante",
            Justification = request.Justification,
            IsAutomatic = false
        };

        _context.PqrFollowUps.Add(followUp);
        await _context.SaveChangesAsync();

        return Ok(new { message = "PQR reabierta exitosamente. La administración revisará su caso." });
    }
}
