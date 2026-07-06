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
[Route("api/maintenance")]
public class MaintenanceController : BaseController
{
    private readonly MaintenanceService _maintenanceService;

    public MaintenanceController(MaintenanceService maintenanceService)
    {
        _maintenanceService = maintenanceService;
    }

    // ── Bienes Comunes ──────────────────────────────────────────

    [HttpGet("assets")]
    [Authorize(Roles = "SuperAdmin,Admin,Accountant")]
    public async Task<ActionResult<List<CommonAssetListDto>>> GetAssets(
        [FromQuery] string? category = null,
        [FromQuery] string? status = null,
        [FromQuery] string? location = null,
        [FromQuery] string? search = null)
    {
        var tenantId = GetTenantId();
        var assets = await _maintenanceService.GetAssetsAsync(tenantId, category, status, location, search);
        return Ok(assets);
    }

    [HttpGet("assets/{id:guid}")]
    [Authorize(Roles = "SuperAdmin,Admin,Accountant")]
    public async Task<ActionResult<CommonAssetDetailDto>> GetAsset(Guid id)
    {
        var tenantId = GetTenantId();
        var asset = await _maintenanceService.GetAssetByIdAsync(tenantId, id);
        return Ok(asset);
    }

    [HttpPost("assets")]
    [Authorize(Roles = "SuperAdmin,Admin")]
    public async Task<ActionResult<CommonAssetDetailDto>> CreateAsset([FromBody] CreateCommonAssetRequestDto request)
    {
        var tenantId = GetTenantId();
        var userId = GetUserId();
        try
        {
            var asset = await _maintenanceService.CreateAssetAsync(tenantId, userId, request);
            return CreatedAtAction(nameof(GetAsset), new { id = asset.Id }, asset);
        }
        catch (ArgumentException ex) { return BadRequest(new { error = ex.Message }); }
    }

    [HttpPut("assets/{id:guid}")]
    [Authorize(Roles = "SuperAdmin,Admin")]
    public async Task<ActionResult<CommonAssetDetailDto>> UpdateAsset(Guid id, [FromBody] UpdateCommonAssetRequestDto request)
    {
        var tenantId = GetTenantId();
        var userId = GetUserId();
        try
        {
            var asset = await _maintenanceService.UpdateAssetAsync(tenantId, userId, id, request);
            return Ok(asset);
        }
        catch (KeyNotFoundException ex) { return NotFound(new { error = ex.Message }); }
        catch (ArgumentException ex) { return BadRequest(new { error = ex.Message }); }
    }

    [HttpDelete("assets/{id:guid}")]
    [Authorize(Roles = "SuperAdmin")]
    public async Task<IActionResult> DeleteAsset(Guid id)
    {
        var tenantId = GetTenantId();
        try
        {
            await _maintenanceService.DeleteAssetAsync(tenantId, id);
            return NoContent();
        }
        catch (KeyNotFoundException ex) { return NotFound(new { error = ex.Message }); }
        catch (InvalidOperationException ex) { return Conflict(new { error = ex.Message }); }
    }

    // ── Planes de Mantenimiento ─────────────────────────────────

    [HttpPost("plans")]
    [Authorize(Roles = "SuperAdmin,Admin")]
    public async Task<ActionResult<MaintenancePlanSummaryDto>> CreatePlan([FromBody] CreateMaintenancePlanRequestDto request)
    {
        var tenantId = GetTenantId();
        var userId = GetUserId();
        try
        {
            var plan = await _maintenanceService.CreateMaintenancePlanAsync(tenantId, userId, request);
            return Ok(plan);
        }
        catch (ArgumentException ex) { return BadRequest(new { error = ex.Message }); }
    }

    [HttpPut("plans/{id:guid}")]
    [Authorize(Roles = "SuperAdmin,Admin")]
    public async Task<ActionResult<MaintenancePlanSummaryDto>> UpdatePlan(Guid id, [FromBody] UpdateMaintenancePlanRequestDto request)
    {
        var tenantId = GetTenantId();
        var userId = GetUserId();
        try
        {
            var plan = await _maintenanceService.UpdateMaintenancePlanAsync(tenantId, userId, id, request);
            return Ok(plan);
        }
        catch (KeyNotFoundException ex) { return NotFound(new { error = ex.Message }); }
        catch (ArgumentException ex) { return BadRequest(new { error = ex.Message }); }
    }

    [HttpDelete("plans/{id:guid}")]
    [Authorize(Roles = "SuperAdmin,Admin")]
    public async Task<IActionResult> DeletePlan(Guid id)
    {
        var tenantId = GetTenantId();
        try
        {
            await _maintenanceService.DeleteMaintenancePlanAsync(tenantId, id);
            return NoContent();
        }
        catch (KeyNotFoundException ex) { return NotFound(new { error = ex.Message }); }
    }

    // ── Órdenes de Trabajo ──────────────────────────────────────

    [HttpGet("work-orders")]
    [Authorize(Roles = "SuperAdmin,Admin,Accountant")]
    public async Task<ActionResult<List<WorkOrderListDto>>> GetWorkOrders(
        [FromQuery] string? orderType = null,
        [FromQuery] string? status = null,
        [FromQuery] string? priority = null,
        [FromQuery] string? assignedProviderId = null,
        [FromQuery] string? search = null)
    {
        var tenantId = GetTenantId();
        var orders = await _maintenanceService.GetWorkOrdersAsync(tenantId, orderType, status, priority, assignedProviderId, search);
        return Ok(orders);
    }

    [HttpGet("work-orders/{id:guid}")]
    [Authorize(Roles = "SuperAdmin,Admin,Accountant")]
    public async Task<ActionResult<WorkOrderDetailDto>> GetWorkOrder(Guid id)
    {
        var tenantId = GetTenantId();
        var order = await _maintenanceService.GetWorkOrderByIdAsync(tenantId, id);
        return Ok(order);
    }

    [HttpPost("work-orders")]
    [Authorize(Roles = "SuperAdmin,Admin")]
    public async Task<ActionResult<WorkOrderDetailDto>> CreateWorkOrder([FromBody] CreateWorkOrderRequestDto request)
    {
        var tenantId = GetTenantId();
        var userId = GetUserId();
        try
        {
            var order = await _maintenanceService.CreateWorkOrderAsync(tenantId, userId, request);
            return CreatedAtAction(nameof(GetWorkOrder), new { id = order.Id }, order);
        }
        catch (KeyNotFoundException ex) { return NotFound(new { error = ex.Message }); }
        catch (ArgumentException ex) { return BadRequest(new { error = ex.Message }); }
    }

    [HttpPut("work-orders/{id:guid}")]
    [Authorize(Roles = "SuperAdmin,Admin")]
    public async Task<ActionResult<WorkOrderDetailDto>> UpdateWorkOrder(Guid id, [FromBody] UpdateWorkOrderRequestDto request)
    {
        var tenantId = GetTenantId();
        var userId = GetUserId();
        try
        {
            var order = await _maintenanceService.UpdateWorkOrderAsync(tenantId, userId, id, request);
            return Ok(order);
        }
        catch (KeyNotFoundException ex) { return NotFound(new { error = ex.Message }); }
        catch (ArgumentException ex) { return BadRequest(new { error = ex.Message }); }
        catch (InvalidOperationException ex) { return Conflict(new { error = ex.Message }); }
    }

    [HttpDelete("work-orders/{id:guid}")]
    [Authorize(Roles = "SuperAdmin")]
    public async Task<IActionResult> DeleteWorkOrder(Guid id)
    {
        var tenantId = GetTenantId();
        try
        {
            await _maintenanceService.DeleteWorkOrderAsync(tenantId, id);
            return NoContent();
        }
        catch (KeyNotFoundException ex) { return NotFound(new { error = ex.Message }); }
        catch (InvalidOperationException ex) { return Conflict(new { error = ex.Message }); }
    }

    // ── Siniestros ──────────────────────────────────────────────

    [HttpGet("incidents")]
    [Authorize(Roles = "SuperAdmin,Admin,Accountant")]
    public async Task<ActionResult<List<IncidentListDto>>> GetIncidents([FromQuery] string? status = null)
    {
        var tenantId = GetTenantId();
        var incidents = await _maintenanceService.GetIncidentsAsync(tenantId, status);
        return Ok(incidents);
    }

    [HttpGet("incidents/{id:guid}")]
    [Authorize(Roles = "SuperAdmin,Admin,Accountant")]
    public async Task<ActionResult<IncidentDetailDto>> GetIncident(Guid id)
    {
        var tenantId = GetTenantId();
        var incident = await _maintenanceService.GetIncidentByIdAsync(tenantId, id);
        return Ok(incident);
    }

    [HttpPost("incidents")]
    [Authorize(Roles = "SuperAdmin,Admin")]
    public async Task<ActionResult<IncidentDetailDto>> CreateIncident([FromBody] CreateIncidentRequestDto request)
    {
        var tenantId = GetTenantId();
        var userId = GetUserId();
        try
        {
            var incident = await _maintenanceService.CreateIncidentAsync(tenantId, userId, request);
            return CreatedAtAction(nameof(GetIncident), new { id = incident.Id }, incident);
        }
        catch (ArgumentException ex) { return BadRequest(new { error = ex.Message }); }
    }

    [HttpPut("incidents/{id:guid}")]
    [Authorize(Roles = "SuperAdmin,Admin")]
    public async Task<ActionResult<IncidentDetailDto>> UpdateIncident(Guid id, [FromBody] UpdateIncidentRequestDto request)
    {
        var tenantId = GetTenantId();
        var userId = GetUserId();
        try
        {
            var incident = await _maintenanceService.UpdateIncidentAsync(tenantId, userId, id, request);
            return Ok(incident);
        }
        catch (KeyNotFoundException ex) { return NotFound(new { error = ex.Message }); }
        catch (ArgumentException ex) { return BadRequest(new { error = ex.Message }); }
    }

    // ── Reportes e Indicadores ──────────────────────────────────

    [HttpGet("reports/scheduled")]
    [Authorize(Roles = "SuperAdmin,Admin,Accountant")]
    public async Task<ActionResult<MaintenanceReportDto>> GetScheduledReport(
        [FromQuery] int daysAhead = 30)
    {
        var tenantId = GetTenantId();
        var report = await _maintenanceService.GetScheduledMaintenanceReportAsync(tenantId, daysAhead);
        return Ok(report);
    }

    [HttpGet("indicators")]
    [Authorize(Roles = "SuperAdmin,Admin,Accountant")]
    public async Task<ActionResult<MaintenanceIndicatorsDto>> GetIndicators()
    {
        var tenantId = GetTenantId();
        var indicators = await _maintenanceService.GetIndicatorsAsync(tenantId);
        return Ok(indicators);
    }

    [HttpGet("out-of-service")]
    [Authorize(Roles = "SuperAdmin,Admin")]
    public async Task<ActionResult<List<OutOfServiceAssetDto>>> GetOutOfServiceAssets()
    {
        var tenantId = GetTenantId();
        var assets = await _maintenanceService.GetOutOfServiceAssetsAsync(tenantId);
        return Ok(assets);
    }
}
