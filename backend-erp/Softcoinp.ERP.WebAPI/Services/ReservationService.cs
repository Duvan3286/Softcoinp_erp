using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Softcoinp.ERP.Domain.Entities;
using Softcoinp.ERP.Domain.Enums;
using Softcoinp.ERP.Infrastructure.Persistence;
using Softcoinp.ERP.WebAPI.DTOs;namespace Softcoinp.ERP.WebAPI.Services;

public class ReservationService
{
    private readonly ApplicationDbContext _context;
    private readonly ReservationAvailabilityEngine _availabilityEngine;
    private readonly ReservationReminderEngine _reminderEngine;

    public ReservationService(
        ApplicationDbContext context,
        ReservationAvailabilityEngine availabilityEngine,
        ReservationReminderEngine reminderEngine)
    {
        _context = context;
        _availabilityEngine = availabilityEngine;
        _reminderEngine = reminderEngine;
    }

    // ── Reservable Spaces ────────────────────────────────────────

    public async Task<List<ReservableSpaceListDto>> GetSpacesAsync(string tenantId, bool? isActive = null)
    {
        var query = _context.ReservableSpaces.Where(s => s.TenantId == tenantId);

        if (isActive.HasValue)
            query = query.Where(s => s.IsActive == isActive.Value);

        return await query
            .OrderBy(s => s.Name)
            .Select(s => new ReservableSpaceListDto
            {
                Id = s.Id,
                Name = s.Name,
                Description = s.Description,
                Location = s.Location,
                MaxCapacity = s.MaxCapacity,
                RequiresDeposit = s.RequiresDeposit,
                DepositAmount = s.DepositAmount,
                HasAdditionalCost = s.HasAdditionalCost,
                ChargeType = s.ChargeType.ToString(),
                HourlyRate = s.HourlyRate,
                EventRate = s.EventRate,
                ApprovalMode = s.ApprovalMode.ToString(),
                ArrearsPolicy = s.ArrearsPolicy.ToString(),
                IsActive = s.IsActive,
                ActiveReservations = s.Reservations.Count(r =>
                    r.Status == ReservationStatus.Requested ||
                    r.Status == ReservationStatus.Approved ||
                    r.Status == ReservationStatus.InUse),
                CreatedAt = s.CreatedAt
            })
            .ToListAsync();
    }

    public async Task<ReservableSpaceDetailDto> GetSpaceByIdAsync(Guid id, string tenantId)
    {
        var space = await _context.ReservableSpaces
            .Where(s => s.Id == id && s.TenantId == tenantId)
            .Select(s => new ReservableSpaceDetailDto
            {
                Id = s.Id,
                Name = s.Name,
                Description = s.Description,
                Location = s.Location,
                MaxCapacity = s.MaxCapacity,
                MinReservationHours = s.MinReservationHours,
                MaxReservationHours = s.MaxReservationHours,
                MinAdvanceHours = s.MinAdvanceHours,
                MaxAdvanceDays = s.MaxAdvanceDays,
                MaxSimultaneousReservationsPerUnit = s.MaxSimultaneousReservationsPerUnit,
                RequiresDeposit = s.RequiresDeposit,
                DepositAmount = s.DepositAmount,
                HasAdditionalCost = s.HasAdditionalCost,
                ChargeType = s.ChargeType.ToString(),
                HourlyRate = s.HourlyRate,
                EventRate = s.EventRate,
                ApprovalMode = s.ApprovalMode.ToString(),
                ArrearsPolicy = s.ArrearsPolicy.ToString(),
                IsAvailableForMaintenance = s.IsAvailableForMaintenance,
                IsActive = s.IsActive,
                RulesFilePath = s.RulesFilePath,
                ImageFilePath = s.ImageFilePath,
                CreatedByUserId = s.CreatedByUserId,
                CreatedAt = s.CreatedAt
            })
            .FirstOrDefaultAsync();

        if (space == null)
            throw new InvalidOperationException("Space not found");

        space.Schedules = await GetSchedulesAsync(id, tenantId);
        return space;
    }

    public async Task<ReservableSpaceDetailDto> CreateSpaceAsync(
        CreateReservableSpaceRequestDto request, string tenantId, string userId)
    {
        var space = new ReservableSpace
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            Name = request.Name,
            Description = request.Description,
            Location = request.Location,
            MaxCapacity = request.MaxCapacity,
            MinReservationHours = request.MinReservationHours,
            MaxReservationHours = request.MaxReservationHours,
            MinAdvanceHours = request.MinAdvanceHours,
            MaxAdvanceDays = request.MaxAdvanceDays,
            MaxSimultaneousReservationsPerUnit = request.MaxSimultaneousReservationsPerUnit,
            RequiresDeposit = request.RequiresDeposit,
            DepositAmount = request.DepositAmount,
            HasAdditionalCost = request.HasAdditionalCost,
            ChargeType = Enum.TryParse<ChargeType>(request.ChargeType, true, out var ct) ? ct : ChargeType.Other,
            HourlyRate = request.HourlyRate,
            EventRate = request.EventRate,
            ApprovalMode = Enum.TryParse<ApprovalMode>(request.ApprovalMode, true, out var am) ? am : ApprovalMode.Automatic,
            ArrearsPolicy = Enum.TryParse<ArrearsPolicy>(request.ArrearsPolicy, true, out var ap) ? ap : ArrearsPolicy.Warn,
            RulesFilePath = request.RulesFilePath,
            ImageFilePath = request.ImageFilePath,
            CreatedByUserId = userId,
            CreatedAt = DateTime.UtcNow
        };

        _context.ReservableSpaces.Add(space);
        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateException ex) when (ex.InnerException?.Message.Contains("Duplicate entry") == true ||
                                            ex.InnerException?.Message.Contains("IX_erp_reservable_spaces") == true)
        {
            throw new InvalidOperationException($"Ya existe un espacio reservable con el nombre '{request.Name}'. Por favor elige un nombre diferente.");
        }

        return await GetSpaceByIdAsync(space.Id, tenantId);
    }

    public async Task<ReservableSpaceDetailDto> UpdateSpaceAsync(
        Guid id, UpdateReservableSpaceRequestDto request, string tenantId, string userId)
    {
        var space = await _context.ReservableSpaces
            .FirstOrDefaultAsync(s => s.Id == id && s.TenantId == tenantId);

        if (space == null)
            throw new InvalidOperationException("Space not found");

        if (request.Name != null) space.Name = request.Name;
        if (request.Description != null) space.Description = request.Description;
        if (request.Location != null) space.Location = request.Location;
        if (request.MaxCapacity.HasValue) space.MaxCapacity = request.MaxCapacity.Value;
        if (request.MinReservationHours.HasValue) space.MinReservationHours = request.MinReservationHours.Value;
        if (request.MaxReservationHours.HasValue) space.MaxReservationHours = request.MaxReservationHours.Value;
        if (request.MinAdvanceHours.HasValue) space.MinAdvanceHours = request.MinAdvanceHours.Value;
        if (request.MaxAdvanceDays.HasValue) space.MaxAdvanceDays = request.MaxAdvanceDays.Value;
        if (request.MaxSimultaneousReservationsPerUnit.HasValue) space.MaxSimultaneousReservationsPerUnit = request.MaxSimultaneousReservationsPerUnit.Value;
        if (request.RequiresDeposit.HasValue) space.RequiresDeposit = request.RequiresDeposit.Value;
        if (request.DepositAmount.HasValue) space.DepositAmount = request.DepositAmount.Value;
        if (request.HasAdditionalCost.HasValue) space.HasAdditionalCost = request.HasAdditionalCost.Value;
        if (request.ChargeType != null && Enum.TryParse<ChargeType>(request.ChargeType, true, out var ct)) space.ChargeType = ct;
        if (request.HourlyRate.HasValue) space.HourlyRate = request.HourlyRate.Value;
        if (request.EventRate.HasValue) space.EventRate = request.EventRate.Value;
        if (request.ApprovalMode != null && Enum.TryParse<ApprovalMode>(request.ApprovalMode, true, out var am)) space.ApprovalMode = am;
        if (request.ArrearsPolicy != null && Enum.TryParse<ArrearsPolicy>(request.ArrearsPolicy, true, out var ap)) space.ArrearsPolicy = ap;
        if (request.RulesFilePath != null) space.RulesFilePath = request.RulesFilePath;
        if (request.ImageFilePath != null) space.ImageFilePath = request.ImageFilePath;
        if (request.IsActive.HasValue) space.IsActive = request.IsActive.Value;

        space.UpdatedByUserId = userId;
        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateException ex) when (ex.InnerException?.Message.Contains("Duplicate entry") == true ||
                                            ex.InnerException?.Message.Contains("IX_erp_reservable_spaces") == true)
        {
            throw new InvalidOperationException($"Ya existe un espacio reservable con el nombre '{space.Name}'. Por favor elige un nombre diferente.");
        }

        return await GetSpaceByIdAsync(id, tenantId);
    }

    // ── Schedules ────────────────────────────────────────────────

    public async Task<List<SpaceScheduleDto>> GetSchedulesAsync(Guid spaceId, string tenantId)
    {
        return await _context.SpaceSchedules
            .Where(s => s.SpaceId == spaceId && s.TenantId == tenantId)
            .OrderBy(s => s.DayOfWeek)
            .ThenBy(s => s.StartTime)
            .Select(s => new SpaceScheduleDto
            {
                Id = s.Id,
                DayOfWeek = s.DayOfWeek,
                DayName = GetDayName(s.DayOfWeek),
                StartTime = s.StartTime,
                EndTime = s.EndTime,
                IsActive = s.IsActive
            })
            .ToListAsync();
    }

    public async Task<SpaceScheduleDto> CreateScheduleAsync(
        Guid spaceId, CreateSpaceScheduleRequestDto request, string tenantId)
    {
        var schedule = new SpaceSchedule
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            SpaceId = spaceId,
            DayOfWeek = request.DayOfWeek,
            StartTime = request.StartTime,
            EndTime = request.EndTime,
            CreatedAt = DateTime.UtcNow
        };

        _context.SpaceSchedules.Add(schedule);
        await _context.SaveChangesAsync();

        return new SpaceScheduleDto
        {
            Id = schedule.Id,
            DayOfWeek = schedule.DayOfWeek,
            DayName = GetDayName(schedule.DayOfWeek),
            StartTime = schedule.StartTime,
            EndTime = schedule.EndTime,
            IsActive = schedule.IsActive
        };
    }

    public async Task DeleteScheduleAsync(Guid scheduleId, string tenantId)
    {
        var schedule = await _context.SpaceSchedules
            .FirstOrDefaultAsync(s => s.Id == scheduleId && s.TenantId == tenantId);

        if (schedule == null)
            throw new InvalidOperationException("Schedule not found");

        _context.SpaceSchedules.Remove(schedule);
        await _context.SaveChangesAsync();
    }

    // ── Space Blocks ─────────────────────────────────────────────

    public async Task<List<SpaceBlockDto>> GetBlocksAsync(string tenantId, Guid? spaceId = null)
    {
        var query = _context.SpaceBlocks
            .Include(b => b.Space)
            .Where(b => b.TenantId == tenantId);

        if (spaceId.HasValue)
            query = query.Where(b => b.SpaceId == spaceId.Value);

        return await query
            .OrderByDescending(b => b.StartDate)
            .Select(b => new SpaceBlockDto
            {
                Id = b.Id,
                SpaceId = b.SpaceId,
                SpaceName = b.Space != null ? b.Space.Name : "",
                StartDate = b.StartDate,
                EndDate = b.EndDate,
                StartTime = b.StartTime,
                EndTime = b.EndTime,
                Origin = b.Origin.ToString(),
                Reason = b.Reason,
                RelatedWorkOrderId = b.RelatedWorkOrderId,
                RelatedWorkOrderNumber = b.RelatedWorkOrderNumber,
                NotifyAffectedResidents = b.NotifyAffectedResidents,
                NotificationSent = b.NotificationSent,
                CreatedByUserId = b.CreatedByUserId,
                CreatedAt = b.CreatedAt
            })
            .ToListAsync();
    }

    public async Task<SpaceBlockDto> CreateBlockAsync(
        CreateSpaceBlockRequestDto request, string tenantId, string userId)
    {
        var space = await _context.ReservableSpaces
            .FirstOrDefaultAsync(s => s.Id == request.SpaceId && s.TenantId == tenantId);

        if (space == null)
            throw new InvalidOperationException("Space not found");

        var block = new SpaceBlock
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            SpaceId = request.SpaceId,
            StartDate = request.StartDate,
            EndDate = request.EndDate,
            StartTime = request.StartTime,
            EndTime = request.EndTime,
            Origin = Enum.TryParse<SpaceBlockOrigin>(request.Origin, true, out var origin) ? origin : SpaceBlockOrigin.Administrative,
            Reason = request.Reason,
            RelatedWorkOrderId = request.RelatedWorkOrderId,
            NotifyAffectedResidents = request.NotifyAffectedResidents,
            CreatedByUserId = userId,
            CreatedAt = DateTime.UtcNow
        };

        _context.SpaceBlocks.Add(block);
        await _context.SaveChangesAsync();

        return new SpaceBlockDto
        {
            Id = block.Id,
            SpaceId = block.SpaceId,
            SpaceName = space.Name,
            StartDate = block.StartDate,
            EndDate = block.EndDate,
            StartTime = block.StartTime,
            EndTime = block.EndTime,
            Origin = block.Origin.ToString(),
            Reason = block.Reason,
            RelatedWorkOrderId = block.RelatedWorkOrderId,
            NotifyAffectedResidents = block.NotifyAffectedResidents,
            CreatedByUserId = block.CreatedByUserId,
            CreatedAt = block.CreatedAt
        };
    }

    // ── Reservations ─────────────────────────────────────────────

    public async Task<List<ReservationListDto>> GetReservationsAsync(
        string tenantId, string? status = null, Guid? spaceId = null,
        Guid? unitId = null, DateTime? fromDate = null, DateTime? toDate = null)
    {
        var query = _context.Reservations
            .Include(r => r.Space)
            .Include(r => r.Unit)
            .Include(r => r.Owner)
            .Where(r => r.TenantId == tenantId);

        if (!string.IsNullOrEmpty(status) && Enum.TryParse<ReservationStatus>(status, true, out var statusEnum))
            query = query.Where(r => r.Status == statusEnum);

        if (spaceId.HasValue)
            query = query.Where(r => r.SpaceId == spaceId.Value);

        if (unitId.HasValue)
            query = query.Where(r => r.UnitId == unitId.Value);

        if (fromDate.HasValue)
            query = query.Where(r => r.StartDateTime >= fromDate.Value);

        if (toDate.HasValue)
            query = query.Where(r => r.StartDateTime <= toDate.Value);

        return await query
            .OrderByDescending(r => r.StartDateTime)
            .Select(r => new ReservationListDto
            {
                Id = r.Id,
                ReservationNumber = r.ReservationNumber,
                SpaceId = r.SpaceId,
                SpaceName = r.Space != null ? r.Space.Name : "",
                UnitId = r.UnitId,
                UnitIdentifier = r.Unit != null ? r.Unit.Identifier : "",
                OwnerId = r.OwnerId,
                OwnerName = r.Owner != null ? r.Owner.FullNameOrCompanyName : "",
                StartDateTime = r.StartDateTime,
                EndDateTime = r.EndDateTime,
                EstimatedAttendees = r.EstimatedAttendees,
                EventDescription = r.EventDescription,
                Status = r.Status.ToString(),
                TotalCost = r.TotalCost,
                DepositStatus = r.DepositStatus.ToString(),
                DepositAmount = r.DepositAmount,
                AdminNotes = r.AdminNotes,
                CreatedAt = r.CreatedAt
            })
            .ToListAsync();
    }

    public async Task<ReservationDetailDto> GetReservationByIdAsync(Guid id, string tenantId)
    {
        var reservation = await _context.Reservations
            .Include(r => r.Space)
            .Include(r => r.Unit)
            .Include(r => r.Owner)
            .Include(r => r.Deposits)
            .Include(r => r.Incidents)
            .Include(r => r.Reminders)
            .FirstOrDefaultAsync(r => r.Id == id && r.TenantId == tenantId);

        if (reservation == null)
            throw new InvalidOperationException("Reservation not found");

        return new ReservationDetailDto
        {
            Id = reservation.Id,
            ReservationNumber = reservation.ReservationNumber,
            SpaceId = reservation.SpaceId,
            SpaceName = reservation.Space?.Name ?? "",
            UnitId = reservation.UnitId,
            UnitIdentifier = reservation.Unit?.Identifier ?? "",
            OwnerId = reservation.OwnerId,
            OwnerName = reservation.Owner?.FullNameOrCompanyName ?? "",
            OwnerEmail = reservation.Owner?.Email ?? "",
            OwnerPhone = reservation.Owner?.MainPhone ?? "",
            StartDateTime = reservation.StartDateTime,
            EndDateTime = reservation.EndDateTime,
            EstimatedAttendees = reservation.EstimatedAttendees,
            EventDescription = reservation.EventDescription,
            HasMusic = reservation.HasMusic,
            MusicEndTime = reservation.MusicEndTime,
            RulesAccepted = reservation.RulesAccepted,
            Status = reservation.Status.ToString(),
            RejectionReason = reservation.RejectionReason,
            TotalCost = reservation.TotalCost,
            DepositStatus = reservation.DepositStatus.ToString(),
            DepositAmount = reservation.DepositAmount,
            AdminNotes = reservation.AdminNotes,
            AdminUserId = reservation.AdminUserId,
            CheckedInAt = reservation.CheckedInAt,
            CheckedOutAt = reservation.CheckedOutAt,
            CheckoutSignaturePath = reservation.CheckoutSignaturePath,
            ExceptionGranted = reservation.ExceptionGranted,
            ExceptionReason = reservation.ExceptionReason,
            CreatedByUserId = reservation.CreatedByUserId,
            CreatedAt = reservation.CreatedAt,
            Deposits = reservation.Deposits.Select(d => new ReservationDepositDto
            {
                Id = d.Id,
                Amount = d.Amount,
                Status = d.Status.ToString(),
                PaymentMethod = d.PaymentMethod?.ToString(),
                ChargeNumber = d.ChargeNumber,
                ReturnChargeNumber = d.ReturnChargeNumber,
                DamageAmount = d.DamageAmount,
                DamageDescription = d.DamageDescription,
                PaidAt = d.PaidAt,
                ReturnedAt = d.ReturnedAt,
                AppliedAt = d.AppliedAt,
                Notes = d.Notes,
                CreatedAt = d.CreatedAt
            }).ToList(),
            Incidents = reservation.Incidents.Select(i => new ReservationIncidentDto
            {
                Id = i.Id,
                Description = i.Description,
                Severity = i.Severity.ToString(),
                DamageAmount = i.DamageAmount,
                DamageAssessed = i.DamageAssessed,
                DepositAppliedToDamage = i.DepositAppliedToDamage,
                EvidenceFilePath = i.EvidenceFilePath,
                ReportedByName = i.ReportedByName ?? "",
                CreatedAt = i.CreatedAt
            }).ToList(),
            Reminders = reservation.Reminders.Select(rm => new ReservationReminderDto
            {
                Id = rm.Id,
                ReminderType = rm.ReminderType.ToString(),
                Status = rm.Status.ToString(),
                ScheduledFor = rm.ScheduledFor,
                SentAt = rm.SentAt,
                Channel = rm.Channel,
                RecipientEmail = rm.RecipientEmail
            }).ToList()
        };
    }

    public async Task<ReservationDetailDto> CreateReservationAsync(
        CreateReservationRequestDto request, string tenantId, string userId)
    {
        var availability = await _availabilityEngine.CheckAvailabilityAsync(
            request.SpaceId, request.StartDateTime, request.EndDateTime, tenantId);

        if (!availability.IsAvailable)
            throw new InvalidOperationException(availability.Reason);

        var maxActive = await _availabilityEngine.GetActiveReservationCountAsync(
            request.UnitId, request.SpaceId, tenantId);

        if (maxActive >= availability.Space!.MaxSimultaneousReservationsPerUnit)
            throw new InvalidOperationException(
                $"La unidad ha alcanzado el límite máximo de {availability.Space.MaxSimultaneousReservationsPerUnit} reservas activas para este espacio");

        var hasArrears = await _availabilityEngine.HasOverdueBalanceAsync(request.UnitId, tenantId);
        if (hasArrears && availability.Space.ArrearsPolicy == ArrearsPolicy.Block && !request.RulesAccepted)
            throw new InvalidOperationException(
                "La unidad tiene saldo vencido. No es posible realizar reservas hasta regularizar la situación.");

        var duration = request.EndDateTime - request.StartDateTime;
        decimal totalCost = 0;
        if (availability.Space.HasAdditionalCost)
        {
            if (availability.Space.ChargeType == ChargeType.PerHour)
                totalCost = availability.Space.HourlyRate * (decimal)duration.TotalHours;
            else
                totalCost = availability.Space.EventRate;
        }

        var number = await GenerateReservationNumberAsync(tenantId);

        var reservation = new Reservation
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            ReservationNumber = number,
            SpaceId = request.SpaceId,
            UnitId = request.UnitId,
            OwnerId = request.OwnerId,
            StartDateTime = request.StartDateTime,
            EndDateTime = request.EndDateTime,
            EstimatedAttendees = request.EstimatedAttendees,
            EventDescription = request.EventDescription,
            HasMusic = request.HasMusic,
            MusicEndTime = request.MusicEndTime,
            RulesAccepted = request.RulesAccepted,
            Status = availability.Space.ApprovalMode == ApprovalMode.Automatic
                ? ReservationStatus.Approved
                : ReservationStatus.Requested,
            TotalCost = totalCost,
            DepositStatus = availability.Space.RequiresDeposit
                ? DepositStatus.Pending
                : DepositStatus.NotRequired,
            DepositAmount = availability.Space.RequiresDeposit ? availability.Space.DepositAmount : 0,
            CreatedByUserId = userId,
            CreatedAt = DateTime.UtcNow
        };

        _context.Reservations.Add(reservation);
        await _context.SaveChangesAsync();

        if (availability.Space.RequiresDeposit)
        {
            var deposit = new ReservationDeposit
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                ReservationId = reservation.Id,
                Amount = availability.Space.DepositAmount,
                Status = DepositStatus.Pending,
                CreatedAt = DateTime.UtcNow
            };
            _context.ReservationDeposits.Add(deposit);
            await _context.SaveChangesAsync();
        }

        await _reminderEngine.CreateRemindersForReservationAsync(reservation, tenantId);

        return await GetReservationByIdAsync(reservation.Id, tenantId);
    }

    public async Task ApproveReservationAsync(
        Guid id, ApproveReservationRequestDto request, string tenantId, string userId)
    {
        var reservation = await _context.Reservations
            .Include(r => r.Space)
            .FirstOrDefaultAsync(r => r.Id == id && r.TenantId == tenantId);

        if (reservation == null)
            throw new InvalidOperationException("Reservation not found");

        if (reservation.Status != ReservationStatus.Requested)
            throw new InvalidOperationException("Only requested reservations can be approved");

        reservation.Status = ReservationStatus.Approved;
        reservation.AdminNotes = request.AdminNotes ?? reservation.AdminNotes;
        reservation.AdminUserId = userId;
        reservation.UpdatedAt = DateTime.UtcNow;
        reservation.UpdatedByUserId = userId;

        await _context.SaveChangesAsync();
    }

    public async Task RejectReservationAsync(
        Guid id, RejectReservationRequestDto request, string tenantId, string userId)
    {
        var reservation = await _context.Reservations
            .FirstOrDefaultAsync(r => r.Id == id && r.TenantId == tenantId);

        if (reservation == null)
            throw new InvalidOperationException("Reservation not found");

        if (reservation.Status != ReservationStatus.Requested)
            throw new InvalidOperationException("Only requested reservations can be rejected");

        reservation.Status = ReservationStatus.Rejected;
        reservation.RejectionReason = request.RejectionReason;
        reservation.AdminNotes = request.AdminNotes ?? reservation.AdminNotes;
        reservation.AdminUserId = userId;
        reservation.UpdatedAt = DateTime.UtcNow;
        reservation.UpdatedByUserId = userId;

        await _reminderEngine.CancelRemindersForReservationAsync(id, tenantId);
        await _context.SaveChangesAsync();
    }

    public async Task CancelReservationAsync(Guid id, string tenantId, string userId)
    {
        var reservation = await _context.Reservations
            .Include(r => r.Space)
            .FirstOrDefaultAsync(r => r.Id == id && r.TenantId == tenantId);

        if (reservation == null)
            throw new InvalidOperationException("Reservation not found");

        if (reservation.Status == ReservationStatus.Completed ||
            reservation.Status == ReservationStatus.Cancelled)
            throw new InvalidOperationException("Cannot cancel a completed or already cancelled reservation");

        reservation.Status = ReservationStatus.Cancelled;
        reservation.UpdatedAt = DateTime.UtcNow;
        reservation.UpdatedByUserId = userId;

        await _reminderEngine.CancelRemindersForReservationAsync(id, tenantId);
        await _context.SaveChangesAsync();
    }

    public async Task CheckInReservationAsync(
        Guid id, CheckInReservationRequestDto request, string tenantId, string userId)
    {
        var reservation = await _context.Reservations
            .FirstOrDefaultAsync(r => r.Id == id && r.TenantId == tenantId);

        if (reservation == null)
            throw new InvalidOperationException("Reservation not found");

        if (reservation.Status != ReservationStatus.Approved)
            throw new InvalidOperationException("Only approved reservations can be checked in");

        reservation.Status = ReservationStatus.InUse;
        reservation.CheckedInAt = DateTime.UtcNow;
        reservation.AdminNotes = request.AdminNotes ?? reservation.AdminNotes;
        reservation.AdminUserId = userId;
        reservation.UpdatedAt = DateTime.UtcNow;
        reservation.UpdatedByUserId = userId;

        await _context.SaveChangesAsync();
    }

    public async Task CheckOutReservationAsync(
        Guid id, CheckOutReservationRequestDto request, string tenantId, string userId)
    {
        var reservation = await _context.Reservations
            .FirstOrDefaultAsync(r => r.Id == id && r.TenantId == tenantId);

        if (reservation == null)
            throw new InvalidOperationException("Reservation not found");

        if (reservation.Status != ReservationStatus.InUse)
            throw new InvalidOperationException("Only in-use reservations can be checked out");

        var hasIncidents = await _context.ReservationIncidents
            .AnyAsync(i => i.ReservationId == id && i.TenantId == tenantId);

        reservation.Status = hasIncidents
            ? ReservationStatus.WithIncident
            : ReservationStatus.Completed;

        reservation.CheckedOutAt = DateTime.UtcNow;
        reservation.CheckoutSignaturePath = request.CheckoutSignaturePath ?? reservation.CheckoutSignaturePath;
        reservation.AdminNotes = request.AdminNotes ?? reservation.AdminNotes;
        reservation.AdminUserId = userId;
        reservation.UpdatedAt = DateTime.UtcNow;
        reservation.UpdatedByUserId = userId;

        await _context.SaveChangesAsync();
    }

    public async Task ReportIncidentAsync(
        Guid reservationId, ReportIncidentRequestDto request, string tenantId, string userId)
    {
        var reservation = await _context.Reservations
            .FirstOrDefaultAsync(r => r.Id == reservationId && r.TenantId == tenantId);

        if (reservation == null)
            throw new InvalidOperationException("Reservation not found");

        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Id == userId);

        var incident = new ReservationIncident
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            ReservationId = reservationId,
            Description = request.Description,
            Severity = Enum.TryParse<IncidentSeverity>(request.Severity, true, out var sev) ? sev : IncidentSeverity.Minor,
            DamageAmount = request.DamageAmount,
            DamageAssessed = request.DamageAmount > 0,
            EvidenceFilePath = request.EvidenceFilePath,
            ReportedByUserId = userId,
            ReportedByName = user?.UserName ?? "",
            CreatedAt = DateTime.UtcNow
        };

        _context.ReservationIncidents.Add(incident);
        await _context.SaveChangesAsync();
    }

    // ── Deposits ─────────────────────────────────────────────────

    public async Task ProcessDepositPaymentAsync(
        Guid reservationId, ProcessDepositPaymentRequestDto request, string tenantId, string userId)
    {
        var deposit = await _context.ReservationDeposits
            .FirstOrDefaultAsync(d => d.ReservationId == reservationId && d.TenantId == tenantId);

        if (deposit == null)
            throw new InvalidOperationException("Deposit not found");

        if (deposit.Status != DepositStatus.Pending)
            throw new InvalidOperationException("Deposit is not in pending status");

        deposit.Status = DepositStatus.Paid;
        deposit.PaymentMethod = Enum.TryParse<DepositPaymentMethod>(request.PaymentMethod, true, out var pm) ? pm : DepositPaymentMethod.Cash;
        deposit.PaidAt = DateTime.UtcNow;
        deposit.ProcessedByUserId = userId;
        deposit.Notes = request.Notes ?? deposit.Notes;

        var reservation = await _context.Reservations
            .FirstOrDefaultAsync(r => r.Id == reservationId);

        if (reservation != null)
        {
            reservation.DepositStatus = DepositStatus.Paid;
            reservation.UpdatedAt = DateTime.UtcNow;
        }

        await _context.SaveChangesAsync();
    }

    public async Task ProcessDepositReturnAsync(
        Guid reservationId, ProcessDepositReturnRequestDto request, string tenantId, string userId)
    {
        var deposit = await _context.ReservationDeposits
            .FirstOrDefaultAsync(d => d.ReservationId == reservationId && d.TenantId == tenantId);

        if (deposit == null)
            throw new InvalidOperationException("Deposit not found");

        if (deposit.Status != DepositStatus.Paid)
            throw new InvalidOperationException("Only paid deposits can be returned");

        deposit.Status = DepositStatus.Returned;
        deposit.ReturnedAt = DateTime.UtcNow;
        deposit.ProcessedByUserId = userId;
        deposit.Notes = request.Notes ?? deposit.Notes;

        var reservation = await _context.Reservations
            .FirstOrDefaultAsync(r => r.Id == reservationId);

        if (reservation != null)
        {
            reservation.DepositStatus = DepositStatus.Returned;
            reservation.UpdatedAt = DateTime.UtcNow;
        }

        await _context.SaveChangesAsync();
    }

    public async Task ApplyDepositToDamageAsync(
        Guid reservationId, ApplyDepositToDamageRequestDto request, string tenantId, string userId)
    {
        var deposit = await _context.ReservationDeposits
            .FirstOrDefaultAsync(d => d.ReservationId == reservationId && d.TenantId == tenantId);

        if (deposit == null)
            throw new InvalidOperationException("Deposit not found");

        if (deposit.Status != DepositStatus.Paid)
            throw new InvalidOperationException("Only paid deposits can be applied to damage");

        deposit.Status = DepositStatus.AppliedToDamage;
        deposit.DamageAmount = request.DamageAmount;
        deposit.DamageDescription = request.DamageDescription;
        deposit.AppliedAt = DateTime.UtcNow;
        deposit.ProcessedByUserId = userId;
        deposit.Notes = request.Notes ?? deposit.Notes;

        var reservation = await _context.Reservations
            .FirstOrDefaultAsync(r => r.Id == reservationId);

        if (reservation != null)
        {
            reservation.DepositStatus = DepositStatus.AppliedToDamage;
            reservation.UpdatedAt = DateTime.UtcNow;
        }

        await _context.SaveChangesAsync();
    }

    // ── Availability ─────────────────────────────────────────────

    public async Task<AvailabilityCheckDto> CheckAvailabilityAsync(
        Guid spaceId, DateTime start, DateTime end, Guid unitId, string tenantId)
    {
        var result = await _availabilityEngine.CheckAvailabilityAsync(spaceId, start, end, tenantId);

        var space = await _context.ReservableSpaces
            .FirstOrDefaultAsync(s => s.Id == spaceId);

        decimal estimatedCost = 0;
        decimal depositAmount = 0;

        if (space != null && result.IsAvailable)
        {
            var duration = end - start;
            if (space.HasAdditionalCost)
            {
                if (space.ChargeType == ChargeType.PerHour)
                    estimatedCost = space.HourlyRate * (decimal)duration.TotalHours;
                else
                    estimatedCost = space.EventRate;
            }
            depositAmount = space.RequiresDeposit ? space.DepositAmount : 0;
        }

        var hasArrears = await _availabilityEngine.HasOverdueBalanceAsync(unitId, tenantId);
        string? arrearsWarning = null;
        if (hasArrears && space != null)
        {
            if (space.ArrearsPolicy == ArrearsPolicy.Block)
                arrearsWarning = "La unidad tiene saldo vencido. No es posible realizar reservas.";
            else
                arrearsWarning = "La unidad tiene saldo vencido. Se recomienda regularizar antes de reservar.";
        }

        return new AvailabilityCheckDto
        {
            IsAvailable = result.IsAvailable,
            Reason = result.Reason,
            EstimatedCost = estimatedCost,
            DepositAmount = depositAmount,
            HasArrears = hasArrears,
            ArrearsWarning = arrearsWarning
        };
    }

    public async Task<List<AvailableSlotDto>> GetAvailableSlotsAsync(
        Guid spaceId, DateTime date, string tenantId)
    {
        var slots = await _availabilityEngine.GetAvailableSlotsAsync(spaceId, date, tenantId);
        return slots.Select(s => new AvailableSlotDto
        {
            StartDateTime = s.StartDateTime,
            EndDateTime = s.EndDateTime,
            DurationHours = s.DurationHours
        }).ToList();
    }

    public async Task<List<AlternativeSlotDto>> GetAlternativesAsync(
        Guid spaceId, DateTime start, DateTime end, string tenantId)
    {
        var alternatives = await _availabilityEngine.SuggestAlternativesAsync(spaceId, start, end, tenantId);
        return alternatives.Select(a => new AlternativeSlotDto
        {
            StartDateTime = a.StartDateTime,
            EndDateTime = a.EndDateTime,
            DurationHours = a.DurationHours,
            DayDifference = a.DayDifference
        }).ToList();
    }

    // ── Calendar ─────────────────────────────────────────────────

    public async Task<List<CalendarEventDto>> GetCalendarEventsAsync(
        Guid spaceId, DateTime monthStart, DateTime monthEnd, string tenantId)
    {
        return await _context.Reservations
            .Include(r => r.Space)
            .Include(r => r.Unit)
            .Include(r => r.Owner)
            .Where(r => r.SpaceId == spaceId &&
                       r.TenantId == tenantId &&
                       r.Status != ReservationStatus.Cancelled &&
                       r.Status != ReservationStatus.Rejected &&
                       r.StartDateTime <= monthEnd &&
                       r.EndDateTime >= monthStart)
            .OrderBy(r => r.StartDateTime)
            .Select(r => new CalendarEventDto
            {
                ReservationId = r.Id,
                ReservationNumber = r.ReservationNumber,
                SpaceName = r.Space != null ? r.Space.Name : "",
                UnitIdentifier = r.Unit != null ? r.Unit.Identifier : "",
                OwnerName = r.Owner != null ? r.Owner.FullNameOrCompanyName : "",
                StartDateTime = r.StartDateTime,
                EndDateTime = r.EndDateTime,
                Status = r.Status.ToString(),
                Color = r.Status == ReservationStatus.Approved ? "#10B981" :
                        r.Status == ReservationStatus.InUse ? "#3B82F6" :
                        r.Status == ReservationStatus.Requested ? "#F59E0B" : "#6B7280"
            })
            .ToListAsync();
    }

    // ── Reports ──────────────────────────────────────────────────

    public async Task<ReservationReportDto> GetReportAsync(
        Guid spaceId, DateTime fromDate, DateTime toDate, string tenantId)
    {
        var space = await _context.ReservableSpaces
            .FirstOrDefaultAsync(s => s.Id == spaceId && s.TenantId == tenantId);

        if (space == null)
            throw new InvalidOperationException("Space not found");

        var reservations = await _context.Reservations
            .Where(r => r.SpaceId == spaceId &&
                       r.TenantId == tenantId &&
                       r.StartDateTime >= fromDate &&
                       r.StartDateTime <= toDate)
            .ToListAsync();

        var totalHoursAvailable = (toDate - fromDate).TotalDays * 14;
        var totalHoursUsed = reservations
            .Where(r => r.Status != ReservationStatus.Cancelled && r.Status != ReservationStatus.Rejected)
            .Sum(r => (r.EndDateTime - r.StartDateTime).TotalHours);

        var topUnits = reservations
            .Where(r => r.Status != ReservationStatus.Cancelled && r.Status != ReservationStatus.Rejected)
            .GroupBy(r => r.UnitId)
            .Select(g => new TopUnitDto
            {
                UnitId = g.Key,
                UnitIdentifier = g.First().Unit?.Identifier ?? "",
                ReservationCount = g.Count()
            })
            .OrderByDescending(u => u.ReservationCount)
            .Take(10)
            .ToList();

        var peakHours = reservations
            .Where(r => r.Status != ReservationStatus.Cancelled && r.Status != ReservationStatus.Rejected)
            .GroupBy(r => r.StartDateTime.Hour)
            .Select(g => new PeakHourDto
            {
                Hour = g.Key,
                ReservationCount = g.Count()
            })
            .OrderByDescending(p => p.ReservationCount)
            .Take(10)
            .ToList();

        return new ReservationReportDto
        {
            SpaceId = spaceId,
            SpaceName = space.Name,
            TotalReservations = reservations.Count,
            CompletedReservations = reservations.Count(r => r.Status == ReservationStatus.Completed),
            CancelledReservations = reservations.Count(r => r.Status == ReservationStatus.Cancelled),
            IncidentReservations = reservations.Count(r => r.Status == ReservationStatus.WithIncident),
            OccupancyPercentage = totalHoursAvailable > 0
                ? Math.Round((decimal)(totalHoursUsed / totalHoursAvailable) * 100, 2)
                : 0,
            TotalRevenue = reservations
                .Where(r => r.Status != ReservationStatus.Cancelled)
                .Sum(r => r.TotalCost),
            TotalDeposits = reservations
                .Where(r => r.DepositStatus == DepositStatus.Paid || r.DepositStatus == DepositStatus.Returned)
                .Sum(r => r.DepositAmount),
            TopUnits = topUnits,
            PeakHours = peakHours
        };
    }

    // ── Helpers ───────────────────────────────────────────────────

    private async Task<string> GenerateReservationNumberAsync(string tenantId)
    {
        var count = await _context.Reservations
            .CountAsync(r => r.TenantId == tenantId);

        var nextNumber = count + 1;
        return $"RES-{DateTime.UtcNow:yyyy-MM}-{nextNumber:D5}";
    }

    private string GetDayName(int dayOfWeek)
    {
        var days = new[] { "Domingo", "Lunes", "Martes", "Miércoles", "Jueves", "Viernes", "Sábado" };
        return days[dayOfWeek];
    }
}
