using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Softcoinp.ERP.WebAPI.DTOs;
using Softcoinp.ERP.WebAPI.Services;

namespace Softcoinp.ERP.WebAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class DashboardController : BaseController
{
    private readonly DashboardService _dashboardService;

    public DashboardController(DashboardService dashboardService)
    {
        _dashboardService = dashboardService;
    }

    [HttpGet("kpis")]
    [Authorize(Roles = "SuperAdmin,Admin")]
    public async Task<ActionResult<DashboardKpisDto>> GetKpis()
    {
        var tenantId = GetTenantId();
        var kpis = await _dashboardService.GetKpisAsync(tenantId);
        return Ok(kpis);
    }

    [HttpGet("alerts")]
    [Authorize(Roles = "SuperAdmin,Admin")]
    public async Task<ActionResult> GetAlerts()
    {
        var tenantId = GetTenantId();
        var alerts = await _dashboardService.GetAlertsAsync(tenantId);
        return Ok(alerts);
    }

    [HttpGet("alerts/configurations")]
    [Authorize(Roles = "SuperAdmin,Admin")]
    public async Task<ActionResult> GetAlertConfigurations()
    {
        var tenantId = GetTenantId();
        var configurations = await _dashboardService.GetAlertConfigurationsAsync(tenantId);
        return Ok(configurations);
    }

    [HttpPut("alerts/configurations/{ruleType}")]
    [Authorize(Roles = "SuperAdmin,Admin")]
    public async Task<ActionResult<AlertConfigurationDto>> UpdateAlertConfiguration(
        string ruleType, [FromBody] UpdateAlertConfigurationRequestDto request)
    {
        var tenantId = GetTenantId();
        var userId = GetUserId();

        try
        {
            var updated = await _dashboardService.UpdateAlertConfigurationAsync(tenantId, ruleType, userId, request);
            return Ok(updated);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPost("alerts/initialize")]
    [Authorize(Roles = "SuperAdmin,Admin")]
    public async Task<IActionResult> InitializeAlerts()
    {
        var tenantId = GetTenantId();
        await _dashboardService.InitializeDefaultAlertConfigurationsAsync(tenantId);
        return Ok(new { message = "Configuración de alertas inicializada." });
    }

    [HttpGet("collection-chart")]
    [Authorize(Roles = "SuperAdmin,Admin")]
    public async Task<ActionResult> GetCollectionChart()
    {
        var tenantId = GetTenantId();
        var chart = await _dashboardService.GetCollectionChartAsync(tenantId);
        return Ok(chart);
    }

    [HttpGet("payment-status-map")]
    [Authorize(Roles = "SuperAdmin,Admin")]
    public async Task<ActionResult<PaymentStatusMapDto>> GetPaymentStatusMap()
    {
        var tenantId = GetTenantId();
        var map = await _dashboardService.GetPaymentStatusMapAsync(tenantId);
        return Ok(map);
    }

    [HttpGet("upcoming-events")]
    [Authorize(Roles = "SuperAdmin,Admin")]
    public async Task<ActionResult> GetUpcomingEvents()
    {
        var tenantId = GetTenantId();
        var events = await _dashboardService.GetUpcomingEventsAsync(tenantId);
        return Ok(events);
    }

    [HttpGet("recent-activity")]
    [Authorize(Roles = "SuperAdmin,Admin")]
    public async Task<ActionResult> GetRecentActivity()
    {
        var tenantId = GetTenantId();
        var activity = await _dashboardService.GetRecentActivityAsync(tenantId);
        return Ok(activity);
    }

    [HttpPost("invalidate-cache")]
    [Authorize(Roles = "SuperAdmin,Admin")]
    public async Task<IActionResult> InvalidateCache()
    {
        var tenantId = GetTenantId();
        await _dashboardService.InvalidateDashboardCacheAsync(tenantId);
        return Ok(new { message = "Caché del dashboard invalidada." });
    }
}
