using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Softcoinp.ERP.Domain.Entities;
using Softcoinp.ERP.Domain.Enums;
using Softcoinp.ERP.Infrastructure.Persistence;

namespace Softcoinp.ERP.WebAPI.Services;

public class ReservationAvailabilityEngine
{
    private readonly ApplicationDbContext _context;

    public ReservationAvailabilityEngine(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<AvailabilityResult> CheckAvailabilityAsync(
        Guid spaceId, DateTime startDateTime, DateTime endDateTime, string tenantId,
        Guid? excludeReservationId = null)
    {
        var space = await _context.ReservableSpaces
            .FirstOrDefaultAsync(s => s.Id == spaceId && s.TenantId == tenantId && s.IsActive);

        if (space == null)
            return new AvailabilityResult { IsAvailable = false, Reason = "Espacio no encontrado o inactivo" };

        var blockConflict = await CheckBlockConflictAsync(spaceId, startDateTime, endDateTime, tenantId);
        if (blockConflict != null)
            return new AvailabilityResult { IsAvailable = false, Reason = blockConflict };

        var overlapConflict = await CheckOverlapConflictAsync(spaceId, startDateTime, endDateTime, tenantId, excludeReservationId);
        if (overlapConflict != null)
            return new AvailabilityResult { IsAvailable = false, Reason = overlapConflict };

        var scheduleConflict = await CheckScheduleConflictAsync(spaceId, startDateTime, endDateTime, tenantId);
        if (scheduleConflict != null)
            return new AvailabilityResult { IsAvailable = false, Reason = scheduleConflict };

        var durationHours = (endDateTime - startDateTime).TotalHours;
        if (durationHours < space.MinReservationHours)
            return new AvailabilityResult { IsAvailable = false, Reason = $"La duración mínima de reserva es {space.MinReservationHours} horas" };

        if (durationHours > space.MaxReservationHours)
            return new AvailabilityResult { IsAvailable = false, Reason = $"La duración máxima de reserva es {space.MaxReservationHours} horas" };

        var advanceHours = (startDateTime - DateTime.UtcNow).TotalHours;
        if (advanceHours < space.MinAdvanceHours)
            return new AvailabilityResult { IsAvailable = false, Reason = $"La reserva debe hacerse con al menos {space.MinAdvanceHours} horas de anticipación" };

        var advanceDays = (startDateTime - DateTime.UtcNow).TotalDays;
        if (advanceDays > space.MaxAdvanceDays)
            return new AvailabilityResult { IsAvailable = false, Reason = $"La reserva no puede hacerse con más de {space.MaxAdvanceDays} días de anticipación" };

        return new AvailabilityResult { IsAvailable = true, Space = space };
    }

    public async Task<List<AvailableTimeSlot>> GetAvailableSlotsAsync(
        Guid spaceId, DateTime date, string tenantId)
    {
        var space = await _context.ReservableSpaces
            .FirstOrDefaultAsync(s => s.Id == spaceId && s.TenantId == tenantId && s.IsActive);

        if (space == null)
            return new List<AvailableTimeSlot>();

        var dayOfWeek = (int)date.DayOfWeek;
        var schedules = await _context.SpaceSchedules
            .Where(s => s.SpaceId == spaceId && s.DayOfWeek == dayOfWeek && s.IsActive)
            .ToListAsync();

        if (!schedules.Any())
            return new List<AvailableTimeSlot>();

        var existingReservations = await _context.Reservations
            .Where(r => r.SpaceId == spaceId &&
                       r.TenantId == tenantId &&
                       r.Status != ReservationStatus.Cancelled &&
                       r.Status != ReservationStatus.Rejected &&
                       r.StartDateTime.Date == date.Date)
            .ToListAsync();

        var blocks = await _context.SpaceBlocks
            .Where(b => b.SpaceId == spaceId &&
                       b.TenantId == tenantId &&
                       b.StartDate.Date <= date.Date &&
                       b.EndDate.Date >= date.Date)
            .ToListAsync();

        var availableSlots = new List<AvailableTimeSlot>();

        foreach (var schedule in schedules)
        {
            var scheduleStart = DateTime.Parse(schedule.StartTime);
            var scheduleEnd = DateTime.Parse(schedule.EndTime);

            var currentSlotStart = new DateTime(date.Year, date.Month, date.Day,
                scheduleStart.Hour, scheduleStart.Minute, 0);

            var scheduleEndToday = new DateTime(date.Year, date.Month, date.Day,
                scheduleEnd.Hour, scheduleEnd.Minute, 0);

            while (currentSlotStart.AddHours(space.MinReservationHours) <= scheduleEndToday)
            {
                var slotEnd = currentSlotStart.AddHours(space.MinReservationHours);
                if (slotEnd > scheduleEndToday)
                    slotEnd = scheduleEndToday;

                var isBlocked = blocks.Any(b =>
                    currentSlotStart < b.EndDate.Add(TimeSpan.Parse(b.EndTime)) &&
                    slotEnd > b.StartDate.Add(TimeSpan.Parse(b.StartTime)));

                var hasOverlap = existingReservations.Any(r =>
                    currentSlotStart < r.EndDateTime && slotEnd > r.StartDateTime);

                if (!isBlocked && !hasOverlap)
                {
                    availableSlots.Add(new AvailableTimeSlot
                    {
                        StartDateTime = currentSlotStart,
                        EndDateTime = slotEnd,
                        DurationHours = (slotEnd - currentSlotStart).TotalHours
                    });
                }

                currentSlotStart = currentSlotStart.AddMinutes(30);
            }
        }

        return availableSlots;
    }

    public async Task<List<AlternativeSlot>> SuggestAlternativesAsync(
        Guid spaceId, DateTime requestedStart, DateTime requestedEnd, string tenantId, int maxSuggestions = 5)
    {
        var space = await _context.ReservableSpaces
            .FirstOrDefaultAsync(s => s.Id == spaceId && s.TenantId == tenantId && s.IsActive);

        if (space == null)
            return new List<AlternativeSlot>();

        var duration = requestedEnd - requestedStart;
        var alternatives = new List<AlternativeSlot>();

        for (int dayOffset = 0; dayOffset <= 14 && alternatives.Count < maxSuggestions; dayOffset++)
        {
            var checkDate = requestedStart.Date.AddDays(dayOffset);
            var slots = await GetAvailableSlotsAsync(spaceId, checkDate, tenantId);

            foreach (var slot in slots)
            {
                if (slot.DurationHours >= duration.TotalHours && alternatives.Count < maxSuggestions)
                {
                    var alreadyExists = alternatives.Any(a =>
                        a.StartDateTime == slot.StartDateTime && a.EndDateTime == slot.EndDateTime);

                    if (!alreadyExists)
                    {
                        alternatives.Add(new AlternativeSlot
                        {
                            StartDateTime = slot.StartDateTime,
                            EndDateTime = slot.StartDateTime.Add(duration),
                            DurationHours = duration.TotalHours,
                            DayDifference = dayOffset
                        });
                    }
                }
            }
        }

        return alternatives;
    }

    public async Task<bool> HasActiveReservationsAsync(Guid unitId, Guid spaceId, string tenantId)
    {
        var space = await _context.ReservableSpaces
            .FirstOrDefaultAsync(s => s.Id == spaceId && s.TenantId == tenantId);

        if (space == null)
            return false;

        var activeCount = await _context.Reservations
            .Where(r => r.UnitId == unitId &&
                       r.SpaceId == spaceId &&
                       r.TenantId == tenantId &&
                       (r.Status == ReservationStatus.Requested ||
                        r.Status == ReservationStatus.Approved))
            .CountAsync();

        return activeCount >= space.MaxSimultaneousReservationsPerUnit;
    }

    public async Task<int> GetActiveReservationCountAsync(Guid unitId, Guid spaceId, string tenantId)
    {
        return await _context.Reservations
            .Where(r => r.UnitId == unitId &&
                       r.SpaceId == spaceId &&
                       r.TenantId == tenantId &&
                       (r.Status == ReservationStatus.Requested ||
                        r.Status == ReservationStatus.Approved))
            .CountAsync();
    }

    public async Task<bool> HasOverdueBalanceAsync(Guid unitId, string tenantId)
    {
        return await _context.UnitFees
            .Where(uf => uf.TenantId == tenantId &&
                        uf.Unit != null &&
                        uf.Unit.Id == unitId &&
                        uf.Status == FeeStatus.Pending &&
                        uf.DueDate < DateTime.UtcNow)
            .AnyAsync();
    }

    private async Task<string?> CheckBlockConflictAsync(
        Guid spaceId, DateTime start, DateTime end, string tenantId)
    {
        var blocks = await _context.SpaceBlocks
            .Where(b => b.SpaceId == spaceId && b.TenantId == tenantId &&
                       b.StartDate <= end && b.EndDate >= start)
            .ToListAsync();

        if (!blocks.Any())
            return null;

        var block = blocks.First();
        return $"Espacio bloqueado del {block.StartDate:dd/MM/yyyy} al {block.EndDate:dd/MM/yyyy} ({block.Reason ?? "Sin motivo"})";
    }

    private async Task<string?> CheckOverlapConflictAsync(
        Guid spaceId, DateTime start, DateTime end, string tenantId, Guid? excludeReservationId)
    {
        var query = _context.Reservations
            .Where(r => r.SpaceId == spaceId &&
                       r.TenantId == tenantId &&
                       r.Status != ReservationStatus.Cancelled &&
                       r.Status != ReservationStatus.Rejected &&
                       r.StartDateTime < end &&
                       r.EndDateTime > start);

        if (excludeReservationId.HasValue)
            query = query.Where(r => r.Id != excludeReservationId.Value);

        var conflict = await query.FirstOrDefaultAsync();

        if (conflict == null)
            return null;

        return $"Horario ocupado por reserva {conflict.ReservationNumber} ({conflict.StartDateTime:HH:mm} - {conflict.EndDateTime:HH:mm})";
    }

    private async Task<string?> CheckScheduleConflictAsync(
        Guid spaceId, DateTime start, DateTime end, string tenantId)
    {
        var startDayOfWeek = (int)start.DayOfWeek;
        var endDayOfWeek = (int)end.DayOfWeek;

        var schedules = await _context.SpaceSchedules
            .Where(s => s.SpaceId == spaceId && s.IsActive)
            .ToListAsync();

        if (startDayOfWeek == endDayOfWeek)
        {
            var daySchedule = schedules.FirstOrDefault(s => s.DayOfWeek == startDayOfWeek);
            if (daySchedule == null)
                return $"El espacio no está disponible los días {GetDayName(startDayOfWeek)}";

            var scheduleStart = DateTime.Parse(daySchedule.StartTime);
            var scheduleEnd = DateTime.Parse(daySchedule.EndTime);

            if (start.TimeOfDay < scheduleStart.TimeOfDay || end.TimeOfDay > scheduleEnd.TimeOfDay)
                return $"El horario disponible los días {GetDayName(startDayOfWeek)} es de {daySchedule.StartTime} a {daySchedule.EndTime}";
        }
        else
        {
            var startSchedule = schedules.FirstOrDefault(s => s.DayOfWeek == startDayOfWeek);
            if (startSchedule == null)
                return $"El espacio no está disponible los días {GetDayName(startDayOfWeek)}";

            var endSchedule = schedules.FirstOrDefault(s => s.DayOfWeek == endDayOfWeek);
            if (endSchedule == null)
                return $"El espacio no está disponible los días {GetDayName(endDayOfWeek)}";
        }

        return null;
    }

    private string GetDayName(int dayOfWeek)
    {
        var days = new[] { "Domingo", "Lunes", "Martes", "Miércoles", "Jueves", "Viernes", "Sábado" };
        return days[dayOfWeek];
    }
}

public class AvailabilityResult
{
    public bool IsAvailable { get; set; }
    public string? Reason { get; set; }
    public ReservableSpace? Space { get; set; }
}

public class AvailableTimeSlot
{
    public DateTime StartDateTime { get; set; }
    public DateTime EndDateTime { get; set; }
    public double DurationHours { get; set; }
}

public class AlternativeSlot
{
    public DateTime StartDateTime { get; set; }
    public DateTime EndDateTime { get; set; }
    public double DurationHours { get; set; }
    public int DayDifference { get; set; }
}
