using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Softcoinp.ERP.Domain.Entities;
using Softcoinp.ERP.Infrastructure.Persistence;

namespace Softcoinp.ERP.WebAPI.Services;

public class NotificationService
{
    private readonly ApplicationDbContext _context;

    public NotificationService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task CreateAsync(string tenantId, Guid ownerId, string title, string message)
    {
        _context.Notifications.Add(new Notification
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            OwnerId = ownerId,
            Title = title,
            Message = message,
            IsRead = false
        });
        await _context.SaveChangesAsync();
    }

    public async Task<List<Notification>> GetUnreadAsync(string tenantId, Guid ownerId)
    {
        return await _context.Notifications
            .Where(n => n.TenantId == tenantId && n.OwnerId == ownerId && !n.IsRead)
            .OrderByDescending(n => n.CreatedAt)
            .ToListAsync();
    }

    public async Task MarkAsReadAsync(Guid notificationId)
    {
        var notification = await _context.Notifications.FindAsync(notificationId);
        if (notification != null)
        {
            notification.IsRead = true;
            await _context.SaveChangesAsync();
        }
    }

    public async Task MarkAllAsReadAsync(string tenantId, Guid ownerId)
    {
        var unread = await _context.Notifications
            .Where(n => n.TenantId == tenantId && n.OwnerId == ownerId && !n.IsRead)
            .ToListAsync();

        foreach (var n in unread)
            n.IsRead = true;

        if (unread.Count > 0)
            await _context.SaveChangesAsync();
    }
}
