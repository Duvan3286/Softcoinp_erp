using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Softcoinp.ERP.WebAPI.DTOs;
using Softcoinp.ERP.WebAPI.Services;

namespace Softcoinp.ERP.WebAPI.Controllers;

[ApiController]
[Authorize]
[Route("api/communications")]
public class CommunicationController : BaseController
{
    private readonly CommunicationService _communicationService;
    private readonly NotificationTemplateService _notificationTemplateService;
    private readonly BulletinBoardService _bulletinBoardService;
    private readonly CommunicationPreferenceService _communicationPreferenceService;
    private readonly DelinquencySequenceEngine _delinquencySequenceEngine;
    private readonly NotificationEngine _notificationEngine;

    public CommunicationController(
        CommunicationService communicationService,
        NotificationTemplateService notificationTemplateService,
        BulletinBoardService bulletinBoardService,
        CommunicationPreferenceService communicationPreferenceService,
        DelinquencySequenceEngine delinquencySequenceEngine,
        NotificationEngine notificationEngine)
    {
        _communicationService = communicationService;
        _notificationTemplateService = notificationTemplateService;
        _bulletinBoardService = bulletinBoardService;
        _communicationPreferenceService = communicationPreferenceService;
        _delinquencySequenceEngine = delinquencySequenceEngine;
        _notificationEngine = notificationEngine;
    }

    // ── Communications ────────────────────────────────────────────

    [HttpGet]
    [Authorize(Roles = "SuperAdmin,Admin")]
    public async Task<ActionResult<List<CommunicationSummaryDto>>> GetCommunications(
        [FromQuery] string? status = null,
        [FromQuery] DateTime? from = null,
        [FromQuery] DateTime? to = null)
    {
        var tenantId = GetTenantId();
        var result = await _communicationService.GetListAsync(tenantId, status, from, to);
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    [Authorize(Roles = "SuperAdmin,Admin")]
    public async Task<ActionResult<CommunicationDetailDto>> GetCommunication(Guid id)
    {
        var tenantId = GetTenantId();
        var communication = await _communicationService.GetByIdAsync(id, tenantId);

        if (communication == null)
            return NotFound(new { message = "Comunicado no encontrado" });

        return Ok(communication);
    }

    [HttpPost]
    [Authorize(Roles = "SuperAdmin,Admin")]
    public async Task<ActionResult<CommunicationDetailDto>> CreateCommunication(
        [FromBody] CreateCommunicationRequest request)
    {
        var tenantId = GetTenantId();
        var userId = GetUserId();

        try
        {
            var communication = await _communicationService.CreateAsync(request, tenantId, userId);
            return CreatedAtAction(nameof(GetCommunication), new { id = communication.Id }, communication);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPut("{id:guid}")]
    [Authorize(Roles = "SuperAdmin,Admin")]
    public async Task<ActionResult<CommunicationDetailDto>> UpdateCommunication(
        Guid id, [FromBody] UpdateCommunicationRequest request)
    {
        var tenantId = GetTenantId();

        try
        {
            var communication = await _communicationService.UpdateAsync(id, request, tenantId);

            if (communication == null)
                return NotFound(new { message = "Comunicado no encontrado" });

            return Ok(communication);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("{id:guid}/send")]
    [Authorize(Roles = "SuperAdmin,Admin")]
    public async Task<ActionResult<CommunicationDetailDto>> SendCommunication(Guid id)
    {
        var tenantId = GetTenantId();

        try
        {
            var communication = await _communicationService.PrepareAndSendAsync(id, tenantId);

            if (communication == null)
                return NotFound(new { message = "Comunicado no encontrado" });

            return Ok(communication);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("{id:guid}/cancel")]
    [Authorize(Roles = "SuperAdmin,Admin")]
    public async Task<ActionResult> CancelScheduled(Guid id)
    {
        var tenantId = GetTenantId();

        try
        {
            var result = await _communicationService.CancelScheduledAsync(id, tenantId);

            if (!result)
                return NotFound(new { message = "Comunicado no encontrado" });

            return Ok(new { message = "Comunicado cancelado exitosamente" });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("{id:guid}/archive")]
    [Authorize(Roles = "SuperAdmin,Admin")]
    public async Task<ActionResult> ArchiveCommunication(Guid id)
    {
        var tenantId = GetTenantId();
        var result = await _communicationService.ArchiveAsync(id, tenantId);

        if (!result)
            return NotFound(new { message = "Comunicado no encontrado" });

        return Ok(new { message = "Comunicado archivado exitosamente" });
    }

    [HttpPost("{id:guid}/resend-unconfirmed")]
    [Authorize(Roles = "SuperAdmin,Admin")]
    public async Task<ActionResult> ResendUnconfirmed(Guid id)
    {
        var tenantId = GetTenantId();

        try
        {
            await _communicationService.ResendToUnconfirmedAsync(id, tenantId);
            return Ok(new { message = "Reenvío iniciado para destinatarios sin confirmación" });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    // ── Notification Templates ────────────────────────────────────

    [HttpGet("templates")]
    [Authorize(Roles = "SuperAdmin,Admin")]
    public async Task<ActionResult<List<NotificationTemplateDto>>> GetTemplates(
        [FromQuery] string? eventType = null)
    {
        var tenantId = GetTenantId();
        var templates = await _notificationTemplateService.GetAllAsync(tenantId, eventType);
        return Ok(templates);
    }

    [HttpGet("templates/{id:guid}")]
    [Authorize(Roles = "SuperAdmin,Admin")]
    public async Task<ActionResult<NotificationTemplateDto>> GetTemplate(Guid id)
    {
        var tenantId = GetTenantId();
        var template = await _notificationTemplateService.GetByIdAsync(id, tenantId);

        if (template == null)
            return NotFound(new { message = "Plantilla no encontrada" });

        return Ok(template);
    }

    [HttpPost("templates")]
    [Authorize(Roles = "SuperAdmin,Admin")]
    public async Task<ActionResult<NotificationTemplateDto>> CreateTemplate(
        [FromBody] CreateNotificationTemplateRequest request)
    {
        var tenantId = GetTenantId();
        var userId = GetUserId();

        try
        {
            var template = await _notificationTemplateService.CreateAsync(request, tenantId, userId);
            return CreatedAtAction(nameof(GetTemplate), new { id = template.Id }, template);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPut("templates/{id:guid}")]
    [Authorize(Roles = "SuperAdmin,Admin")]
    public async Task<ActionResult<NotificationTemplateDto>> UpdateTemplate(
        Guid id, [FromBody] UpdateNotificationTemplateRequest request)
    {
        var tenantId = GetTenantId();

        try
        {
            var template = await _notificationTemplateService.UpdateAsync(id, request, tenantId);

            if (template == null)
                return NotFound(new { message = "Plantilla no encontrada" });

            return Ok(template);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpDelete("templates/{id:guid}")]
    [Authorize(Roles = "SuperAdmin,Admin")]
    public async Task<ActionResult> DeleteTemplate(Guid id)
    {
        var tenantId = GetTenantId();
        var result = await _notificationTemplateService.DeleteAsync(id, tenantId);

        if (!result)
            return NotFound(new { message = "Plantilla no encontrada" });

        return Ok(new { message = "Plantilla eliminada exitosamente" });
    }

    // ── Bulletin Board ────────────────────────────────────────────

    [HttpGet("bulletin-board")]
    [Authorize(Roles = "SuperAdmin,Admin,Owner,Tenant")]
    public async Task<ActionResult<List<BulletinBoardPostDto>>> GetActiveBulletinPosts()
    {
        var tenantId = GetTenantId();
        var posts = await _bulletinBoardService.GetActivePostsAsync(tenantId);
        return Ok(posts);
    }

    [HttpGet("bulletin-board/admin")]
    [Authorize(Roles = "SuperAdmin,Admin")]
    public async Task<ActionResult<List<BulletinBoardPostAdminDto>>> GetAllBulletinPosts(
        [FromQuery] bool includeArchived = false)
    {
        var tenantId = GetTenantId();
        var posts = await _bulletinBoardService.GetAllPostsAsync(tenantId, includeArchived);
        return Ok(posts);
    }

    [HttpGet("bulletin-board/{id:guid}")]
    [Authorize(Roles = "SuperAdmin,Admin,Owner,Tenant")]
    public async Task<ActionResult<BulletinBoardPostAdminDto>> GetBulletinPost(Guid id)
    {
        var tenantId = GetTenantId();
        var post = await _bulletinBoardService.GetByIdAsync(id, tenantId);

        if (post == null)
            return NotFound(new { message = "Publicación no encontrada" });

        return Ok(post);
    }

    [HttpPost("bulletin-board")]
    [Authorize(Roles = "SuperAdmin,Admin")]
    public async Task<ActionResult<BulletinBoardPostAdminDto>> CreateBulletinPost(
        [FromBody] CreateBulletinBoardPostRequest request)
    {
        var tenantId = GetTenantId();
        var userId = GetUserId();

        try
        {
            var post = await _bulletinBoardService.CreateAsync(request, tenantId, userId);
            return CreatedAtAction(nameof(GetBulletinPost), new { id = post.Id }, post);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPut("bulletin-board/{id:guid}")]
    [Authorize(Roles = "SuperAdmin,Admin")]
    public async Task<ActionResult<BulletinBoardPostAdminDto>> UpdateBulletinPost(
        Guid id, [FromBody] UpdateBulletinBoardPostRequest request)
    {
        var tenantId = GetTenantId();

        try
        {
            var post = await _bulletinBoardService.UpdateAsync(id, request, tenantId);

            if (post == null)
                return NotFound(new { message = "Publicación no encontrada" });

            return Ok(post);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpDelete("bulletin-board/{id:guid}")]
    [Authorize(Roles = "SuperAdmin,Admin")]
    public async Task<ActionResult> ArchiveBulletinPost(Guid id)
    {
        var tenantId = GetTenantId();
        var result = await _bulletinBoardService.ArchiveAsync(id, tenantId);

        if (!result)
            return NotFound(new { message = "Publicación no encontrada" });

        return Ok(new { message = "Publicación archivada exitosamente" });
    }

    // ── Communication Preferences ─────────────────────────────────

    [HttpGet("preferences")]
    [Authorize(Roles = "SuperAdmin,Admin")]
    public async Task<ActionResult<List<CommunicationPreferenceDto>>> GetAllPreferences()
    {
        var tenantId = GetTenantId();
        var prefs = await _communicationPreferenceService.GetAllAsync(tenantId);
        return Ok(prefs);
    }

    [HttpGet("preferences/owner/{ownerId:guid}")]
    [Authorize(Roles = "SuperAdmin,Admin,Owner")]
    public async Task<ActionResult<CommunicationPreferenceDto>> GetOwnerPreferences(Guid ownerId)
    {
        var tenantId = GetTenantId();
        var pref = await _communicationPreferenceService.GetByOwnerAsync(ownerId, tenantId);

        if (pref == null)
            return NotFound(new { message = "Preferencias no encontradas" });

        return Ok(pref);
    }

    [HttpGet("preferences/tenant/{tenantResidentId:guid}")]
    [Authorize(Roles = "SuperAdmin,Admin,Tenant")]
    public async Task<ActionResult<CommunicationPreferenceDto>> GetTenantPreferences(Guid tenantResidentId)
    {
        var tenantId = GetTenantId();
        var pref = await _communicationPreferenceService.GetByTenantResidentAsync(tenantResidentId, tenantId);

        if (pref == null)
            return NotFound(new { message = "Preferencias no encontradas" });

        return Ok(pref);
    }

    [HttpPut("preferences/owner/{ownerId:guid}")]
    [Authorize(Roles = "SuperAdmin,Admin,Owner")]
    public async Task<ActionResult<CommunicationPreferenceDto>> UpdateOwnerPreferences(
        Guid ownerId, [FromBody] UpdateCommunicationPreferenceRequest request)
    {
        var tenantId = GetTenantId();
        var userId = GetUserId();

        try
        {
            var pref = await _communicationPreferenceService.CreateOrUpdateAsync(
                request, ownerId, null, tenantId, userId);
            return Ok(pref);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPut("preferences/tenant/{tenantResidentId:guid}")]
    [Authorize(Roles = "SuperAdmin,Admin,Tenant")]
    public async Task<ActionResult<CommunicationPreferenceDto>> UpdateTenantPreferences(
        Guid tenantResidentId, [FromBody] UpdateCommunicationPreferenceRequest request)
    {
        var tenantId = GetTenantId();
        var userId = GetUserId();

        try
        {
            var pref = await _communicationPreferenceService.CreateOrUpdateAsync(
                request, null, tenantResidentId, tenantId, userId);
            return Ok(pref);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    // ── Delinquency Sequence ──────────────────────────────────────

    [HttpGet("delinquency-config")]
    [Authorize(Roles = "SuperAdmin,Admin")]
    public async Task<ActionResult<List<DelinquencySequenceConfigDto>>> GetDelinquencyConfig()
    {
        var tenantId = GetTenantId();
        var configs = await _delinquencySequenceEngine.GetConfigAsync(tenantId);

        var result = configs.Select(c => new DelinquencySequenceConfigDto
        {
            Id = c.Id,
            StepNumber = c.StepNumber,
            DaysAfterDue = c.DaysAfterDue,
            TemplateId = c.TemplateId,
            TemplateName = c.Template?.Name ?? string.Empty,
            IsActive = c.IsActive
        }).ToList();

        return Ok(result);
    }

    [HttpPut("delinquency-config/{stepNumber:int}")]
    [Authorize(Roles = "SuperAdmin,Admin")]
    public async Task<ActionResult> UpdateDelinquencyConfig(
        int stepNumber, [FromBody] UpdateDelinquencySequenceConfigRequest request)
    {
        var tenantId = GetTenantId();

        try
        {
            var config = await _delinquencySequenceEngine.UpsertConfigAsync(
                tenantId, stepNumber, request.DaysAfterDue, request.TemplateId, request.IsActive);
            return Ok(new { message = $"Configuración del paso {stepNumber} actualizada" });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpGet("delinquency-pauses")]
    [Authorize(Roles = "SuperAdmin,Admin")]
    public async Task<ActionResult<List<DelinquencySequencePauseDto>>> GetActiveDelinquencyPauses()
    {
        var tenantId = GetTenantId();
        var pauses = await _delinquencySequenceEngine.GetActivePausesAsync(tenantId);

        var result = pauses.Select(p => new DelinquencySequencePauseDto
        {
            Id = p.Id,
            UnitId = p.UnitId,
            UnitIdentifier = p.Unit?.Identifier ?? string.Empty,
            StartDate = p.StartDate,
            EndDate = p.EndDate,
            Reason = p.Reason,
            CreatedAt = p.CreatedAt,
            CreatedByUserId = p.CreatedByUserId
        }).ToList();

        return Ok(result);
    }

    [HttpPost("delinquency-pauses")]
    [Authorize(Roles = "SuperAdmin,Admin")]
    public async Task<ActionResult> CreateDelinquencyPause(
        [FromBody] CreateDelinquencySequencePauseRequest request)
    {
        var tenantId = GetTenantId();
        var userId = GetUserId();

        try
        {
            var result = await _delinquencySequenceEngine.PauseForUnitAsync(
                tenantId, request.UnitId, request.StartDate, request.EndDate, request.Reason, userId);
            return Ok(new { message = "Pausa registrada exitosamente" });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpDelete("delinquency-pauses/{id:guid}")]
    [Authorize(Roles = "SuperAdmin,Admin")]
    public async Task<ActionResult> RemoveDelinquencyPause(Guid id)
    {
        var tenantId = GetTenantId();
        var result = await _delinquencySequenceEngine.RemovePauseAsync(id, tenantId);

        if (!result)
            return NotFound(new { message = "Pausa no encontrada" });

        return Ok(new { message = "Pausa eliminada exitosamente" });
    }

    [HttpPost("delinquency-process")]
    [Authorize(Roles = "SuperAdmin,Admin")]
    public async Task<ActionResult<List<string>>> RunDelinquencyProcess()
    {
        var tenantId = GetTenantId();

        try
        {
            var results = await _delinquencySequenceEngine.ProcessDailyAsync(tenantId);
            return Ok(results);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    // ── Notification Engine (event trigger) ───────────────────────

    [HttpPost("notify")]
    [Authorize(Roles = "SuperAdmin,Admin")]
    public async Task<ActionResult> TriggerNotification(
        [FromBody] TriggerNotificationRequest request)
    {
        var tenantId = GetTenantId();

        try
        {
            var eventType = (Domain.Enums.NotificationEventType)
                Enum.Parse(typeof(Domain.Enums.NotificationEventType), request.EventType);

            var result = await _notificationEngine.ProcessEventAsync(
                tenantId, eventType, request.SourceModule, request.SourceEntityId,
                request.SourceEntityType, request.OwnerId, request.TenantResidentId, request.Variables);

            return Ok(new { notificationId = result?.Id, message = "Notificación procesada" });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    // ── Reports ───────────────────────────────────────────────────

    [HttpGet("reports/effectiveness")]
    [Authorize(Roles = "SuperAdmin,Admin")]
    public async Task<ActionResult<CommunicationEffectivenessReportDto>> GetEffectivenessReport(
        [FromQuery] DateTime? from = null, [FromQuery] DateTime? to = null)
    {
        var tenantId = GetTenantId();
        var report = await _communicationService.GetEffectivenessReportAsync(tenantId, from, to);
        return Ok(report);
    }
}

public class TriggerNotificationRequest
{
    public string EventType { get; set; } = string.Empty;
    public string SourceModule { get; set; } = string.Empty;
    public string SourceEntityId { get; set; } = string.Empty;
    public string SourceEntityType { get; set; } = string.Empty;
    public Guid? OwnerId { get; set; }
    public Guid? TenantResidentId { get; set; }
    public Dictionary<string, string>? Variables { get; set; }
}
