using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Softcoinp.ERP.Domain.Entities;
using Softcoinp.ERP.Domain.Enums;
using Softcoinp.ERP.Infrastructure.Persistence;

namespace Softcoinp.ERP.WebAPI.Services;

public class ReservationReminderEngine
{
    private readonly ApplicationDbContext _context;
    private readonly NotificationEngine _notificationEngine;

    public ReservationReminderEngine(ApplicationDbContext context, NotificationEngine notificationEngine)
    {
        _context = context;
        _notificationEngine = notificationEngine;
    }

    public async Task CreateRemindersForReservationAsync(Reservation reservation, string tenantId)
    {
        var existingReminders = await _context.ReservationReminders
            .Where(r => r.ReservationId == reservation.Id && r.TenantId == tenantId)
            .ToListAsync();

        if (existingReminders.Any())
            return;

        var owner = await _context.Owners
            .FirstOrDefaultAsync(o => o.Id == reservation.OwnerId);

        var unit = await _context.Units
            .FirstOrDefaultAsync(u => u.Id == reservation.UnitId);

        var reminder24h = new ReservationReminder
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            ReservationId = reservation.Id,
            ReminderType = ReminderType.TwentyFourHours,
            Status = ReminderStatus.Pending,
            ScheduledFor = reservation.StartDateTime.AddHours(-24),
            Channel = "Email",
            RecipientEmail = owner?.Email,
            CreatedAt = DateTime.UtcNow
        };

        var reminder2h = new ReservationReminder
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            ReservationId = reservation.Id,
            ReminderType = ReminderType.TwoHours,
            Status = ReminderStatus.Pending,
            ScheduledFor = reservation.StartDateTime.AddHours(-2),
            Channel = "Email",
            RecipientEmail = owner?.Email,
            CreatedAt = DateTime.UtcNow
        };

        _context.ReservationReminders.Add(reminder24h);
        _context.ReservationReminders.Add(reminder2h);
        await _context.SaveChangesAsync();
    }

    public async Task<List<ReservationReminder>> GetPendingRemindersAsync()
    {
        var now = DateTime.UtcNow;
        return await _context.ReservationReminders
            .Where(r => r.Status == ReminderStatus.Pending && r.ScheduledFor <= now)
            .Include(r => r.Reservation!)
            .ThenInclude(rv => rv!.Space)
            .OrderBy(r => r.ScheduledFor)
            .ToListAsync();
    }

    public async Task ProcessReminderAsync(Guid reminderId)
    {
        var reminder = await _context.ReservationReminders
            .Include(r => r.Reservation!)
            .ThenInclude(rv => rv!.Space)
            .FirstOrDefaultAsync(r => r.Id == reminderId);

        if (reminder == null || reminder.Status != ReminderStatus.Pending)
            return;

        try
        {
            var reservation = reminder.Reservation;
            if (reservation == null)
            {
                throw new InvalidOperationException("La reserva asociada al recordatorio no existe.");
            }

            var owner = await _context.Owners.FirstOrDefaultAsync(o => o.Id == reservation.OwnerId);
            var residentName = string.Empty;
            if (owner != null)
            {
                residentName = owner.FullNameOrCompanyName;
            }

            var spaceName = string.Empty;
            if (reservation.Space != null)
            {
                spaceName = reservation.Space.Name;
            }

            var variables = new Dictionary<string, string>
            {
                ["ResidentName"] = residentName,
                ["SpaceName"] = spaceName,
                ["ReservationDate"] = reservation.StartDateTime.ToString("dd/MM/yyyy"),
                ["ReservationTime"] = reservation.StartDateTime.ToString("HH:mm")
            };

            var eventType = NotificationEventType.ReservationReminder24h;
            if (reminder.ReminderType == ReminderType.TwoHours)
            {
                eventType = NotificationEventType.ReservationReminder2h;
            }

            await _notificationEngine.ProcessEventAsync(
                reminder.TenantId, eventType,
                "Reservations", reservation.Id.ToString(), "Reservation",
                ownerId: reservation.OwnerId, variables: variables);

            reminder.Status = ReminderStatus.Sent;
            reminder.SentAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            reminder.Status = ReminderStatus.Failed;
            reminder.ErrorMessage = ex.Message;
            reminder.RetryCount += 1;
            await _context.SaveChangesAsync();
        }
    }

    public async Task<int> ProcessAllPendingRemindersAsync()
    {
        var pendingReminders = await GetPendingRemindersAsync();
        var processedCount = 0;

        foreach (var reminder in pendingReminders)
        {
            await ProcessReminderAsync(reminder.Id);
            processedCount++;
        }

        return processedCount;
    }

    public async Task CancelRemindersForReservationAsync(Guid reservationId, string tenantId)
    {
        var reminders = await _context.ReservationReminders
            .Where(r => r.ReservationId == reservationId &&
                       r.TenantId == tenantId &&
                       r.Status == ReminderStatus.Pending)
            .ToListAsync();

        foreach (var reminder in reminders)
        {
            reminder.Status = ReminderStatus.Failed;
            reminder.ErrorMessage = "Reserva cancelada";
        }

        await _context.SaveChangesAsync();
    }
}
