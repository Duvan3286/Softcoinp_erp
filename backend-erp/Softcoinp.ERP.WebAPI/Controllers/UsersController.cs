using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Softcoinp.ERP.Domain.Entities;
using Softcoinp.ERP.Domain.Enums;
using Softcoinp.ERP.Domain.Interfaces;
using Softcoinp.ERP.Infrastructure.Persistence;
using System.Security.Claims;
using System.Text.Json;

namespace Softcoinp.ERP.WebAPI.Controllers;

[ApiController]
[Route("api/users")]
[Authorize]
public class UsersController : ControllerBase
{
    private readonly ApplicationDbContext _db;
    private readonly UserManager<User> _userManager;
    private readonly ITenantResolver _tenantResolver;
    private readonly ILogger<UsersController> _logger;

    public UsersController(
        ApplicationDbContext db,
        UserManager<User> userManager,
        ITenantResolver tenantResolver,
        ILogger<UsersController> logger)
    {
        _db = db;
        _userManager = userManager;
        _tenantResolver = tenantResolver;
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> GetUsers([FromQuery] string? role, [FromQuery] string? search)
    {
        var tenant = await _tenantResolver.GetCurrentTenantAsync();
        if (tenant == null) return BadRequest("No tenant active.");

        // Un admin solo puede ver los usuarios de su propio conjunto
        var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!await HasAdminPrivileges(currentUserId!, tenant.Id))
            return Forbid();

        var query = _db.UserTenantRoles
            .Include(r => r.User)
            .Where(r => r.TenantId == tenant.Id.ToString() && r.IsActive);

        if (!string.IsNullOrEmpty(role))
        {
            if (Enum.TryParse<AppRole>(role, true, out var roleEnum))
                query = query.Where(r => r.Role == roleEnum);
        }

        if (!string.IsNullOrEmpty(search))
        {
            query = query.Where(r => r.User!.FullName.Contains(search) || r.User!.Email!.Contains(search));
        }

        var users = await query.Select(r => new
        {
            id = r.User!.Id,
            fullName = r.User.FullName,
            email = r.User.Email,
            role = r.Role.ToString(),
            isActive = r.User.IsActive,
            isSuspended = r.User.IsSuspended,
            suspendedReason = r.User.SuspendedReason,
            assignedAt = r.AssignedAt,
            expiresAt = r.ExpiresAt
        }).ToListAsync();

        return Ok(users);
    }

    [HttpPost("{id}/suspend")]
    public async Task<IActionResult> SuspendUser(string id, [FromBody] SuspendRequest request)
    {
        var tenant = await _tenantResolver.GetCurrentTenantAsync();
        if (tenant == null) return BadRequest("No tenant active.");

        var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!await HasAdminPrivileges(currentUserId!, tenant.Id))
            return Forbid();

        var targetUser = await _userManager.FindByIdAsync(id);
        if (targetUser == null) return NotFound("Usuario no encontrado.");

        targetUser.IsSuspended = true;
        targetUser.SuspendedAt = DateTime.UtcNow;
        targetUser.SuspendedReason = request.Reason;
        await _userManager.UpdateAsync(targetUser);

        _db.AccessAuditLogs.Add(new AccessAuditLog
        {
            Timestamp = DateTime.UtcNow,
            UserId = targetUser.Id,
            Email = targetUser.Email!,
            TenantId = tenant.Id.ToString(),
            EventType = AuditEventType.AccountSuspended,
            IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString(),
            Details = JsonSerializer.Serialize(new { reason = request.Reason, suspendedBy = currentUserId })
        });
        await _db.SaveChangesAsync();

        return Ok(new { message = "Usuario suspendido correctamente." });
    }

    [HttpPost("{id}/activate")]
    public async Task<IActionResult> ActivateUser(string id)
    {
        var tenant = await _tenantResolver.GetCurrentTenantAsync();
        if (tenant == null) return BadRequest("No tenant active.");

        var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!await HasAdminPrivileges(currentUserId!, tenant.Id))
            return Forbid();

        var targetUser = await _userManager.FindByIdAsync(id);
        if (targetUser == null) return NotFound("Usuario no encontrado.");

        targetUser.IsSuspended = false;
        targetUser.SuspendedAt = null;
        targetUser.SuspendedReason = null;
        // Reiniciar contadores de bloqueo al activar
        targetUser.FailedLoginCount = 0;
        targetUser.LockoutUntil = null;
        targetUser.DailyLockoutCount = 0;
        await _userManager.UpdateAsync(targetUser);

        _db.AccessAuditLogs.Add(new AccessAuditLog
        {
            Timestamp = DateTime.UtcNow,
            UserId = targetUser.Id,
            Email = targetUser.Email!,
            TenantId = tenant.Id.ToString(),
            EventType = AuditEventType.AccountActivated,
            IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString(),
            Details = JsonSerializer.Serialize(new { activatedBy = currentUserId })
        });
        await _db.SaveChangesAsync();

        return Ok(new { message = "Usuario reactivado correctamente." });
    }

    private async Task<bool> HasAdminPrivileges(string userId, Guid tenantId)
    {
        var user = await _userManager.FindByIdAsync(userId);
        var roles = await _userManager.GetRolesAsync(user!);
        if (roles.Contains(nameof(AppRole.SuperAdmin))) return true;

        var tenantRole = await _db.UserTenantRoles
            .FirstOrDefaultAsync(r => r.UserId == userId && r.TenantId == tenantId.ToString() && r.IsActive);

        return tenantRole?.Role == AppRole.Admin;
    }
}

public record SuspendRequest(string Reason);
