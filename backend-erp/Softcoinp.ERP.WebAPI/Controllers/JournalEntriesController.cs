using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Softcoinp.ERP.Domain.Enums;
using Softcoinp.ERP.WebAPI.DTOs;
using Softcoinp.ERP.WebAPI.Services;

namespace Softcoinp.ERP.WebAPI.Controllers;

[ApiController]
[Route("api/journal-entries")]
[Authorize]
public class JournalEntriesController : BaseController
{
    private readonly JournalEntryService _journalEntryService;

    public JournalEntriesController(JournalEntryService journalEntryService)
    {
        _journalEntryService = journalEntryService;
    }

    [HttpGet]
    [Authorize(Roles = "SuperAdmin,Admin,Accountant,Auditor")]
    public async Task<IActionResult> GetEntries(
        [FromQuery] DateTime? fromDate,
        [FromQuery] DateTime? toDate,
        [FromQuery] Guid? periodId,
        [FromQuery] EntryStatus? status,
        [FromQuery] EntryType? entryType,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        var tenantId = GetTenantId();
        var entries = await _journalEntryService.GetEntriesAsync(tenantId, fromDate, toDate, periodId, status, entryType, page, pageSize);
        return Ok(entries);
    }

    [HttpGet("{id:guid}")]
    [Authorize(Roles = "SuperAdmin,Admin,Accountant,Auditor")]
    public async Task<IActionResult> GetEntry(Guid id)
    {
        var tenantId = GetTenantId();
        var entry = await _journalEntryService.GetEntryAsync(tenantId, id);

        if (entry == null)
            return NotFound("Asiento contable no encontrado.");

        return Ok(entry);
    }

    [HttpPost]
    [Authorize(Roles = "SuperAdmin,Admin,Accountant")]
    public async Task<IActionResult> CreateEntry([FromBody] CreateJournalEntryDto dto)
    {
        var tenantId = GetTenantId();
        var userId = GetUserId();

        try
        {
            var entry = await _journalEntryService.CreateEntryAsync(tenantId, dto, userId);
            return CreatedAtAction(nameof(GetEntry), new { id = entry.Id }, entry);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPost("{id:guid}/post")]
    [Authorize(Roles = "SuperAdmin,Admin,Accountant")]
    public async Task<IActionResult> PostEntry(Guid id)
    {
        var tenantId = GetTenantId();

        try
        {
            var entry = await _journalEntryService.PostEntryAsync(tenantId, id);
            return Ok(entry);
        }
        catch (KeyNotFoundException)
        {
            return NotFound("Asiento contable no encontrado.");
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPost("{id:guid}/reverse")]
    [Authorize(Roles = "SuperAdmin,Admin,Accountant")]
    public async Task<IActionResult> ReverseEntry(Guid id, [FromBody] ReverseJournalEntryDto dto)
    {
        var tenantId = GetTenantId();
        var userId = GetUserId();

        try
        {
            var entry = await _journalEntryService.ReverseEntryAsync(tenantId, id, dto.Reason, userId);
            return Ok(entry);
        }
        catch (KeyNotFoundException)
        {
            return NotFound("Asiento contable no encontrado.");
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }
}
