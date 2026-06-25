using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Softcoinp.ERP.Domain.Entities;
using Softcoinp.ERP.WebAPI.Services;

namespace Softcoinp.ERP.WebAPI.Controllers;

[ApiController]
[Route("api/bank-reconciliations")]
[Authorize]
public class BankReconciliationController : BaseController
{
    private readonly BankReconciliationService _reconciliationService;
    private readonly ILogger<BankReconciliationController> _logger;

    public BankReconciliationController(BankReconciliationService reconciliationService, ILogger<BankReconciliationController> logger)
    {
        _reconciliationService = reconciliationService;
        _logger = logger;
    }

    [HttpGet]
    [Authorize(Roles = "SuperAdmin,Admin,Accountant")]
    public async Task<IActionResult> GetReconciliations([FromQuery] Guid? bankAccountId)
    {
        var result = await _reconciliationService.GetReconciliationsAsync(GetTenantId(), bankAccountId);
        return Ok(result);
    }

    [HttpGet("{id}")]
    [Authorize(Roles = "SuperAdmin,Admin,Accountant")]
    public async Task<IActionResult> GetReconciliation(Guid id)
    {
        var result = await _reconciliationService.GetReconciliationByIdAsync(GetTenantId(), id);
        if (result == null) return NotFound(new { message = "Conciliación no encontrada." });
        return Ok(result);
    }

    [HttpPost]
    [Authorize(Roles = "SuperAdmin,Admin,Accountant")]
    public async Task<IActionResult> StartReconciliation([FromBody] StartReconciliationDto dto)
    {
        try
        {
            var result = await _reconciliationService.StartReconciliationAsync(
                GetTenantId(), dto.BankAccountId, dto.FiscalYear, dto.Month, GetUserId());
            return CreatedAtAction(nameof(GetReconciliation), new { id = result.Id }, result);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("{id}/items")]
    [Authorize(Roles = "SuperAdmin,Admin,Accountant")]
    public async Task<IActionResult> AddItem(Guid id, [FromBody] ReconciliationItem item)
    {
        try
        {
            var result = await _reconciliationService.AddItemAsync(GetTenantId(), id, item);
            return Ok(result);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpDelete("{id}/items/{itemId}")]
    [Authorize(Roles = "SuperAdmin,Admin,Accountant")]
    public async Task<IActionResult> RemoveItem(Guid id, Guid itemId)
    {
        try
        {
            await _reconciliationService.RemoveItemAsync(GetTenantId(), id, itemId);
            return NoContent();
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    [HttpPost("{id}/items/{itemId}/clear")]
    [Authorize(Roles = "SuperAdmin,Admin,Accountant")]
    public async Task<IActionResult> ClearItem(Guid id, Guid itemId)
    {
        try
        {
            var result = await _reconciliationService.ClearItemAsync(GetTenantId(), id, itemId);
            return Ok(result);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("{id}/complete")]
    [Authorize(Roles = "SuperAdmin,Admin,Accountant")]
    public async Task<IActionResult> CompleteReconciliation(Guid id)
    {
        try
        {
            var result = await _reconciliationService.CompleteReconciliationAsync(GetTenantId(), id, GetUserId());
            return Ok(result);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}

public class StartReconciliationDto
{
    public Guid BankAccountId { get; set; }
    public int FiscalYear { get; set; }
    public int Month { get; set; }
}
