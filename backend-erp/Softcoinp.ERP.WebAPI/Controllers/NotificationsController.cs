using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Softcoinp.ERP.Infrastructure.Persistence;
using Softcoinp.ERP.WebAPI.Services;

namespace Softcoinp.ERP.WebAPI.Controllers;

[ApiController]
[Route("api/notifications")]
[Authorize(Roles = "SuperAdmin,Admin")]
public class NotificationsController : BaseController
{
    private readonly NotificationService _notificationService;
    private readonly ApplicationDbContext _context;

    public NotificationsController(NotificationService notificationService, ApplicationDbContext context)
    {
        _notificationService = notificationService;
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> GetUnread([FromQuery] Guid ownerId)
    {
        var tenantId = GetTenantId();
        var notifications = await _notificationService.GetUnreadAsync(tenantId, ownerId);
        return Ok(notifications.Select(n => new
        {
            n.Id,
            n.Title,
            n.Message,
            n.CreatedAt
        }));
    }

    [HttpPost("{id}/read")]
    public async Task<IActionResult> MarkAsRead(Guid id)
    {
        var tenantId = GetTenantId();
        var userId = GetUserId();

        var notification = await _context.Notifications
            .FirstOrDefaultAsync(n => n.Id == id && n.TenantId == tenantId);

        if (notification == null)
            return NotFound(new { message = "Notificación no encontrada." });

        await _notificationService.MarkAsReadAsync(id);
        return Ok();
    }

    [HttpPost("read-all")]
    public async Task<IActionResult> MarkAllAsRead([FromQuery] Guid ownerId)
    {
        var tenantId = GetTenantId();
        await _notificationService.MarkAllAsReadAsync(tenantId, ownerId);
        return Ok();
    }
}
