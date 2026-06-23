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
[Route("api/providers")]
public class ProviderController : BaseController
{
    private readonly ProviderService _providerService;

    public ProviderController(ProviderService providerService)
    {
        _providerService = providerService;
    }

    [HttpGet]
    [Authorize(Roles = "SuperAdmin,Admin,Accountant")]
    public async Task<ActionResult<List<ProviderListDto>>> GetProviders(
        [FromQuery] string? status = null,
        [FromQuery] string? providerType = null,
        [FromQuery] string? serviceType = null,
        [FromQuery] string? search = null)
    {
        var tenantId = GetTenantId();
        var providers = await _providerService.GetProvidersAsync(tenantId, status, providerType, serviceType, search);
        return Ok(providers);
    }

    [HttpGet("{id:guid}")]
    [Authorize(Roles = "SuperAdmin,Admin,Accountant")]
    public async Task<ActionResult<ProviderDetailDto>> GetProvider(Guid id)
    {
        var tenantId = GetTenantId();
        var provider = await _providerService.GetProviderByIdAsync(tenantId, id);
        return Ok(provider);
    }

    [HttpPost]
    [Authorize(Roles = "SuperAdmin,Admin")]
    public async Task<ActionResult<ProviderDetailDto>> CreateProvider([FromBody] CreateProviderRequestDto request)
    {
        var tenantId = GetTenantId();
        var userId = GetUserId();

        try
        {
            var provider = await _providerService.CreateProviderAsync(tenantId, userId, request);
            return CreatedAtAction(nameof(GetProvider), new { id = provider.Id }, provider);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { error = ex.Message });
        }
    }

    [HttpPut("{id:guid}")]
    [Authorize(Roles = "SuperAdmin,Admin")]
    public async Task<ActionResult<ProviderDetailDto>> UpdateProvider(Guid id, [FromBody] UpdateProviderRequestDto request)
    {
        var tenantId = GetTenantId();
        var userId = GetUserId();

        try
        {
            var provider = await _providerService.UpdateProviderAsync(tenantId, userId, id, request);
            return Ok(provider);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { error = ex.Message });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "SuperAdmin")]
    public async Task<IActionResult> DeleteProvider(Guid id)
    {
        var tenantId = GetTenantId();

        try
        {
            await _providerService.DeleteProviderAsync(tenantId, id);
            return NoContent();
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { error = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { error = ex.Message });
        }
    }

    [HttpGet("{providerId:guid}/evaluations")]
    [Authorize(Roles = "SuperAdmin,Admin,Accountant")]
    public async Task<ActionResult<List<ProviderEvaluationSummaryDto>>> GetProviderEvaluations(Guid providerId)
    {
        var tenantId = GetTenantId();

        try
        {
            var evaluations = await _providerService.GetProviderEvaluationsAsync(tenantId, providerId);
            return Ok(evaluations);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { error = ex.Message });
        }
    }

    [HttpPost("{providerId:guid}/evaluations")]
    [Authorize(Roles = "SuperAdmin,Admin")]
    public async Task<ActionResult<ProviderEvaluationSummaryDto>> CreateProviderEvaluation(
        Guid providerId, [FromBody] CreateProviderEvaluationRequestDto request)
    {
        var tenantId = GetTenantId();
        var userId = GetUserId();
        var userName = User.Identity?.Name ?? string.Empty;

        try
        {
            var evaluation = await _providerService.CreateProviderEvaluationAsync(tenantId, userId, userName, providerId, request);
            return Ok(evaluation);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { error = ex.Message });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpGet("indicators")]
    [Authorize(Roles = "SuperAdmin,Admin,Accountant")]
    public async Task<ActionResult<ProviderIndicatorsDto>> GetIndicators()
    {
        var tenantId = GetTenantId();
        var indicators = await _providerService.GetIndicatorsAsync(tenantId);
        return Ok(indicators);
    }
}
