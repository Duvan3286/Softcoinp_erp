using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Softcoinp.ERP.Domain.Interfaces;
using Softcoinp.ERP.Infrastructure.Persistence;
using System.Security.Claims;

namespace Softcoinp.ERP.WebAPI.Controllers;

[ApiController]
[Route("api/audit")]
[Authorize(Roles = "SuperAdmin,Admin")]
public class AuditController : ControllerBase
{
    private readonly ApplicationDbContext _db;
    private readonly ITenantResolver _tenantResolver;

    public AuditController(ApplicationDbContext db, ITenantResolver tenantResolver)
    {
        _db = db;
        _tenantResolver = tenantResolver;
    }

    [HttpGet("logs")]
    public async Task<IActionResult> GetLogs(
        [FromQuery] DateTime? from,
        [FromQuery] DateTime? to,
        [FromQuery] string? eventType,
        [FromQuery] int page = 1,
        [FromQuery] int limit = 50)
    {
        var tenant = await _tenantResolver.GetCurrentTenantAsync();
        if (tenant == null) return BadRequest("No tenant active.");

        var query = _db.AccessAuditLogs
            .Where(l => l.TenantId == tenant.Id.ToString())
            .OrderByDescending(l => l.Timestamp)
            .AsQueryable();

        if (from.HasValue)
            query = query.Where(l => l.Timestamp >= from.Value);

        if (to.HasValue)
            query = query.Where(l => l.Timestamp <= to.Value);

        if (!string.IsNullOrEmpty(eventType))
            query = query.Where(l => l.EventType.ToString().Equals(eventType, StringComparison.OrdinalIgnoreCase));

        var total = await query.CountAsync();
        var logs = await query
            .Skip((page - 1) * limit)
            .Take(limit)
            .Select(l => new
            {
                id = l.Id,
                timestamp = l.Timestamp,
                email = l.Email,
                eventType = l.EventType.ToString(),
                ipAddress = l.IpAddress,
                userAgent = l.UserAgent,
                details = l.Details
            })
            .ToListAsync();

        return Ok(new
        {
            total,
            page,
            limit,
            data = logs
        });
    }
}
