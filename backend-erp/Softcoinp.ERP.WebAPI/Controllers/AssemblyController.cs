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
[Route("api/assembly")]
public class AssemblyController : BaseController
{
    private readonly AssemblyService _assemblyService;

    public AssemblyController(AssemblyService assemblyService)
    {
        _assemblyService = assemblyService;
    }

    // ── Assembly CRUD ─────────────────────────────────────────────

    [HttpGet]
    [Authorize(Roles = "SuperAdmin,Admin,Accountant")]
    public async Task<ActionResult<List<AssemblyListDto>>> GetAssemblies(
        [FromQuery] string? status = null,
        [FromQuery] string? type = null,
        [FromQuery] DateTime? fromDate = null,
        [FromQuery] DateTime? toDate = null,
        [FromQuery] string? search = null)
    {
        var tenantId = GetTenantId();
        var assemblies = await _assemblyService.GetAssembliesAsync(tenantId, status, type, fromDate, toDate, search);
        return Ok(assemblies);
    }

    [HttpGet("{id:guid}")]
    [Authorize(Roles = "SuperAdmin,Admin,Accountant")]
    public async Task<ActionResult<AssemblyDetailDto>> GetAssembly(Guid id)
    {
        var tenantId = GetTenantId();
        var assembly = await _assemblyService.GetAssemblyByIdAsync(id, tenantId);
        return Ok(assembly);
    }

    [HttpPost]
    [Authorize(Roles = "SuperAdmin,Admin")]
    public async Task<ActionResult<AssemblyDetailDto>> CreateAssembly([FromBody] CreateAssemblyRequestDto request)
    {
        var tenantId = GetTenantId();
        var userId = GetUserId();
        var assembly = await _assemblyService.CreateAssemblyAsync(request, tenantId, userId);
        return CreatedAtAction(nameof(GetAssembly), new { id = assembly.Id }, assembly);
    }

    [HttpPut("{id:guid}")]
    [Authorize(Roles = "SuperAdmin,Admin")]
    public async Task<ActionResult<AssemblyDetailDto>> UpdateAssembly(Guid id, [FromBody] UpdateAssemblyRequestDto request)
    {
        var tenantId = GetTenantId();
        var userId = GetUserId();
        var assembly = await _assemblyService.UpdateAssemblyAsync(id, request, tenantId, userId);
        return Ok(assembly);
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "SuperAdmin,Admin")]
    public async Task<IActionResult> DeleteAssembly(Guid id)
    {
        var tenantId = GetTenantId();
        await _assemblyService.DeleteAssemblyAsync(id, tenantId);
        return NoContent();
    }

    [HttpPut("{id:guid}/session")]
    [Authorize(Roles = "SuperAdmin,Admin")]
    public async Task<IActionResult> UpdateSessionInfo(Guid id, [FromBody] UpdateSessionRequestDto request)
    {
        var tenantId = GetTenantId();
        var userId = GetUserId();
        await _assemblyService.UpdateSessionInfoAsync(id, request, tenantId, userId);
        return NoContent();
    }

    // ── Session Flow ──────────────────────────────────────────────

    [HttpPost("{id:guid}/convocate")]
    [Authorize(Roles = "SuperAdmin,Admin")]
    public async Task<IActionResult> Convocate(Guid id)
    {
        var tenantId = GetTenantId();
        var userId = GetUserId();
        await _assemblyService.ConvocateAsync(id, tenantId, userId);
        return Ok(new { message = "Assembly convoked successfully" });
    }

    [HttpPost("{id:guid}/start-session")]
    [Authorize(Roles = "SuperAdmin,Admin")]
    public async Task<IActionResult> StartSession(Guid id, [FromBody] StartSessionRequestDto request)
    {
        var tenantId = GetTenantId();
        var userId = GetUserId();
        await _assemblyService.StartSessionAsync(id, request, tenantId, userId);
        return Ok(new { message = "Session started successfully" });
    }

    [HttpPost("{id:guid}/end-session")]
    [Authorize(Roles = "SuperAdmin,Admin")]
    public async Task<IActionResult> EndSession(Guid id)
    {
        var tenantId = GetTenantId();
        var userId = GetUserId();
        await _assemblyService.EndSessionAsync(id, tenantId, userId);
        return Ok(new { message = "Session ended successfully" });
    }

    // ── Convocation ───────────────────────────────────────────────

    [HttpGet("{id:guid}/convocations")]
    [Authorize(Roles = "SuperAdmin,Admin,Accountant")]
    public async Task<ActionResult<List<AssemblyConvocationDto>>> GetConvocations(Guid id)
    {
        var tenantId = GetTenantId();
        var convocations = await _assemblyService.GetConvocationsAsync(id, tenantId);
        return Ok(convocations);
    }

    [HttpPost("{id:guid}/convocations")]
    [Authorize(Roles = "SuperAdmin,Admin")]
    public async Task<ActionResult<AssemblyConvocationDto>> CreateConvocation(
        Guid id, [FromBody] CreateConvocationRequestDto request)
    {
        var tenantId = GetTenantId();
        var userId = GetUserId();
        var convocation = await _assemblyService.CreateConvocationAsync(id, request, tenantId, userId);
        return Ok(convocation);
    }

    [HttpPost("convocations/{convocationId:guid}/send")]
    [Authorize(Roles = "SuperAdmin,Admin")]
    public async Task<IActionResult> SendConvocation(Guid convocationId)
    {
        var tenantId = GetTenantId();
        var userId = GetUserId();
        await _assemblyService.SendConvocationAsync(convocationId, tenantId, userId);
        return Ok(new { message = "Convocation sent successfully" });
    }

    // ── Attendance ────────────────────────────────────────────────

    [HttpGet("{id:guid}/attendances")]
    [Authorize(Roles = "SuperAdmin,Admin,Accountant")]
    public async Task<ActionResult<List<AssemblyAttendanceDto>>> GetAttendances(Guid id)
    {
        var tenantId = GetTenantId();
        var attendances = await _assemblyService.GetAttendancesAsync(id, tenantId);
        return Ok(attendances);
    }

    [HttpPost("{id:guid}/attendances")]
    [Authorize(Roles = "SuperAdmin,Admin")]
    public async Task<ActionResult<AssemblyAttendanceDto>> RegisterAttendance(
        Guid id, [FromBody] RegisterAttendanceRequestDto request)
    {
        var tenantId = GetTenantId();
        var userId = GetUserId();
        var attendance = await _assemblyService.RegisterAttendanceAsync(id, request, tenantId, userId);
        return Ok(attendance);
    }

    [HttpPut("attendances/{attendanceId:guid}")]
    [Authorize(Roles = "SuperAdmin,Admin")]
    public async Task<IActionResult> UpdateAttendance(
        Guid attendanceId, [FromBody] UpdateAttendanceRequestDto request)
    {
        var tenantId = GetTenantId();
        var userId = GetUserId();
        await _assemblyService.UpdateAttendanceAsync(attendanceId, request, tenantId, userId);
        return NoContent();
    }

    [HttpPost("attendances/{attendanceId:guid}/lift-restriction")]
    [Authorize(Roles = "SuperAdmin,Admin")]
    public async Task<IActionResult> LiftVotingRestriction(
        Guid attendanceId, [FromBody] LiftVotingRestrictionRequestDto request)
    {
        var tenantId = GetTenantId();
        var userId = GetUserId();
        await _assemblyService.LiftVotingRestrictionAsync(attendanceId, request, tenantId, userId);
        return Ok(new { message = "Voting restriction lifted successfully" });
    }

    // ── Agenda Items ──────────────────────────────────────────────

    [HttpGet("{id:guid}/agenda-items")]
    [Authorize(Roles = "SuperAdmin,Admin,Accountant")]
    public async Task<ActionResult<List<AssemblyAgendaItemDto>>> GetAgendaItems(Guid id)
    {
        var tenantId = GetTenantId();
        var items = await _assemblyService.GetAgendaItemsAsync(id, tenantId);
        return Ok(items);
    }

    [HttpPost("{id:guid}/agenda-items")]
    [Authorize(Roles = "SuperAdmin,Admin")]
    public async Task<ActionResult<AssemblyAgendaItemDto>> CreateAgendaItem(
        Guid id, [FromBody] CreateAgendaItemRequestDto request)
    {
        var tenantId = GetTenantId();
        var userId = GetUserId();
        var item = await _assemblyService.CreateAgendaItemAsync(id, request, tenantId, userId);
        return Ok(item);
    }

    [HttpPut("agenda-items/{itemId:guid}")]
    [Authorize(Roles = "SuperAdmin,Admin")]
    public async Task<ActionResult<AssemblyAgendaItemDto>> UpdateAgendaItem(
        Guid itemId, [FromBody] UpdateAgendaItemRequestDto request)
    {
        var tenantId = GetTenantId();
        var userId = GetUserId();
        var item = await _assemblyService.UpdateAgendaItemAsync(itemId, request, tenantId, userId);
        return Ok(item);
    }

    [HttpDelete("agenda-items/{itemId:guid}")]
    [Authorize(Roles = "SuperAdmin,Admin")]
    public async Task<IActionResult> DeleteAgendaItem(Guid itemId)
    {
        var tenantId = GetTenantId();
        await _assemblyService.DeleteAgendaItemAsync(itemId, tenantId);
        return NoContent();
    }

    // ── Voting ────────────────────────────────────────────────────

    [HttpPost("agenda-items/{itemId:guid}/vote")]
    [Authorize(Roles = "SuperAdmin,Admin")]
    public async Task<ActionResult<AssemblyAgendaItemDto>> RegisterVote(
        Guid itemId, [FromBody] RegisterVoteRequestDto request)
    {
        var tenantId = GetTenantId();
        var userId = GetUserId();
        var item = await _assemblyService.RegisterVoteAsync(itemId, request, tenantId, userId);
        return Ok(item);
    }

    // ── Constancies ───────────────────────────────────────────────

    [HttpGet("{id:guid}/constancies")]
    [Authorize(Roles = "SuperAdmin,Admin,Accountant")]
    public async Task<ActionResult<List<AssemblyConstancyDto>>> GetConstancies(Guid id)
    {
        var tenantId = GetTenantId();
        var constancies = await _assemblyService.GetConstanciesAsync(id, tenantId);
        return Ok(constancies);
    }

    [HttpPost("{id:guid}/constancies")]
    [Authorize(Roles = "SuperAdmin,Admin")]
    public async Task<ActionResult<AssemblyConstancyDto>> CreateConstancy(
        Guid id, [FromBody] CreateConstancyRequestDto request)
    {
        var tenantId = GetTenantId();
        var userId = GetUserId();
        var constancy = await _assemblyService.CreateConstancyAsync(id, request, tenantId, userId);
        return Ok(constancy);
    }

    // ── Minutes ───────────────────────────────────────────────────

    [HttpPost("{id:guid}/minutes/generate")]
    [Authorize(Roles = "SuperAdmin,Admin")]
    public async Task<ActionResult<AssemblyMinutesDto>> GenerateMinutes(
        Guid id, [FromBody] GenerateMinutesRequestDto request)
    {
        var tenantId = GetTenantId();
        var userId = GetUserId();
        var minutes = await _assemblyService.GenerateMinutesAsync(id, request, tenantId, userId);
        return Ok(minutes);
    }

    [HttpPost("{id:guid}/minutes/approve")]
    [Authorize(Roles = "SuperAdmin,Admin")]
    public async Task<ActionResult<AssemblyMinutesDto>> ApproveMinutes(
        Guid id, [FromBody] ApproveMinutesRequestDto request)
    {
        var tenantId = GetTenantId();
        var userId = GetUserId();
        var minutes = await _assemblyService.ApproveMinutesAsync(id, request, tenantId, userId);
        return Ok(minutes);
    }

    [HttpPost("{id:guid}/minutes/publish")]
    [Authorize(Roles = "SuperAdmin,Admin")]
    public async Task<IActionResult> PublishMinutes(Guid id)
    {
        var tenantId = GetTenantId();
        var userId = GetUserId();
        await _assemblyService.PublishMinutesAsync(id, tenantId, userId);
        return Ok(new { message = "Minutes published successfully" });
    }

    // ── Decision Propagation ──────────────────────────────────────

    [HttpGet("{id:guid}/propagations")]
    [Authorize(Roles = "SuperAdmin,Admin,Accountant")]
    public async Task<ActionResult<List<AssemblyDecisionPropagationDto>>> GetPropagations(Guid id)
    {
        var tenantId = GetTenantId();
        var propagations = await _assemblyService.GetPropagationsAsync(id, tenantId);
        return Ok(propagations);
    }

    [HttpPost("{id:guid}/propagations")]
    [Authorize(Roles = "SuperAdmin,Admin")]
    public async Task<ActionResult<AssemblyDecisionPropagationDto>> CreatePropagation(
        Guid id, [FromBody] CreateDecisionPropagationRequestDto request)
    {
        var tenantId = GetTenantId();
        var userId = GetUserId();
        var propagation = await _assemblyService.CreatePropagationAsync(id, request, tenantId, userId);
        return Ok(propagation);
    }

    // ── Quorum ────────────────────────────────────────────────────

    [HttpGet("{id:guid}/quorum")]
    [Authorize(Roles = "SuperAdmin,Admin,Accountant")]
    public async Task<ActionResult<QuorumStatusDto>> GetQuorumStatus(Guid id)
    {
        var tenantId = GetTenantId();
        var quorum = await _assemblyService.GetQuorumStatusAsync(id, tenantId);
        return Ok(quorum);
    }

    [HttpGet("units-for-attendance")]
    [Authorize(Roles = "SuperAdmin,Admin")]
    public async Task<ActionResult<List<UnitWithOwnerInfo>>> GetUnitsForAttendance()
    {
        var tenantId = GetTenantId();
        var units = await _assemblyService.GetUnitsForAttendanceAsync(tenantId);
        return Ok(units);
    }

    // ── Reports ───────────────────────────────────────────────────

    [HttpGet("report")]
    [Authorize(Roles = "SuperAdmin,Admin,Accountant")]
    public async Task<ActionResult<AssemblyReportDto>> GetReport()
    {
        var tenantId = GetTenantId();
        var report = await _assemblyService.GetReportAsync(tenantId);
        return Ok(report);
    }
}
