using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Softcoinp.ERP.Domain.Enums;
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

    [HttpGet]
    public async Task<IActionResult> GetDashboard()
    {
        var tenantId = GetTenantId();
        var userId = GetUserId();
        var role = GetUserRole();

        if (string.IsNullOrEmpty(tenantId))
        {
            return Unauthorized("Tenant not resolved.");
        }

        if (string.IsNullOrEmpty(role))
        {
            return Forbid();
        }

        var data = await _dashboardService.GetDashboardAsync(tenantId, userId, role);
        return Ok(data);
    }

    [HttpPost("initialize-alerts")]
    [Authorize(Roles = "SuperAdmin,Admin")]
    public async Task<IActionResult> InitializeAlerts()
    {
        var tenantId = GetTenantId();

        if (string.IsNullOrEmpty(tenantId))
        {
            return Unauthorized("Tenant not resolved.");
        }

        await _dashboardService.InitializeDefaultAlertConfigurationsAsync(tenantId);
        return Ok(new { message = "Alert configurations initialized." });
    }

    [HttpPost("invalidate-cache")]
    [Authorize(Roles = "SuperAdmin,Admin,Accountant")]
    public async Task<IActionResult> InvalidateCache()
    {
        var tenantId = GetTenantId();

        if (string.IsNullOrEmpty(tenantId))
        {
            return Unauthorized("Tenant not resolved.");
        }

        await _dashboardService.InvalidateMoraMapCacheAsync(tenantId);
        return Ok(new { message = "Mora map cache invalidated." });
    }

    private string GetUserRole()
    {
        var roles = User.FindAll(ClaimTypes.Role).Select(r => r.Value).ToList();
        if (roles.Count == 0)
        {
            return string.Empty;
        }

        return roles.First();
    }
}
