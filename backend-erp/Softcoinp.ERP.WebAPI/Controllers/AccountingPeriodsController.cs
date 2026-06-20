using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Softcoinp.ERP.WebAPI.DTOs;
using Softcoinp.ERP.WebAPI.Services;

namespace Softcoinp.ERP.WebAPI.Controllers;

[ApiController]
[Route("api/accounting-periods")]
[Authorize]
public class AccountingPeriodsController : BaseController
{
    private readonly AccountingPeriodService _periodService;

    public AccountingPeriodsController(AccountingPeriodService periodService)
    {
        _periodService = periodService;
    }

    [HttpGet]
    [Authorize(Roles = "SuperAdmin,Admin,Accountant,Auditor")]
    public async Task<IActionResult> GetPeriods()
    {
        var tenantId = GetTenantId();
        var periods = await _periodService.GetPeriodsAsync(tenantId);
        return Ok(periods);
    }

    [HttpGet("current")]
    [Authorize(Roles = "SuperAdmin,Admin,Accountant,Auditor")]
    public async Task<IActionResult> GetCurrentPeriod()
    {
        var tenantId = GetTenantId();
        var period = await _periodService.GetCurrentPeriodAsync(tenantId);

        if (period == null)
            return NotFound("No hay un período contable abierto para el mes actual.");

        return Ok(period);
    }

    [HttpPost]
    [Authorize(Roles = "SuperAdmin,Admin,Accountant")]
    public async Task<IActionResult> OpenPeriod([FromBody] CreateAccountingPeriodDto dto)
    {
        var tenantId = GetTenantId();
        var userId = GetUserId();

        try
        {
            var period = await _periodService.OpenPeriodAsync(tenantId, dto, userId);
            return Ok(period);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPost("{id:guid}/close")]
    [Authorize(Roles = "SuperAdmin,Admin,Accountant")]
    public async Task<IActionResult> ClosePeriod(Guid id)
    {
        var tenantId = GetTenantId();
        var userId = GetUserId();

        try
        {
            var period = await _periodService.ClosePeriodAsync(tenantId, id, userId);
            return Ok(period);
        }
        catch (KeyNotFoundException)
        {
            return NotFound("Período contable no encontrado.");
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }
}
