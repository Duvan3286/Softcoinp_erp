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
    private readonly ApplicationDbContext _context;

    public PQRController(PQRRadicationService radicationService, ApplicationDbContext context)
    {
        _radicationService = radicationService;
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
}
