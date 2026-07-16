using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Softcoinp.ERP.Domain.Entities;
using Softcoinp.ERP.Domain.Enums;
using Softcoinp.ERP.Infrastructure.Persistence;
using Softcoinp.ERP.WebAPI.DTOs;

namespace Softcoinp.ERP.WebAPI.Services;

public class AssemblyService
{
    private readonly ApplicationDbContext _context;
    private readonly AssemblyQuorumEngine _quorumEngine;
    private readonly AssemblyVotingEngine _votingEngine;
    private readonly AssemblyMinutesGenerator _minutesGenerator;
    private readonly AssemblyDecisionPropagationService _propagationService;
    private readonly NotificationEngine _notificationEngine;
    private readonly BillingEngineService _billingEngineService;
    private readonly BudgetService _budgetService;

    public AssemblyService(
        ApplicationDbContext context,
        AssemblyQuorumEngine quorumEngine,
        AssemblyVotingEngine votingEngine,
        AssemblyMinutesGenerator minutesGenerator,
        AssemblyDecisionPropagationService propagationService,
        NotificationEngine notificationEngine,
        BillingEngineService billingEngineService,
        BudgetService budgetService)
    {
        _context = context;
        _quorumEngine = quorumEngine;
        _votingEngine = votingEngine;
        _minutesGenerator = minutesGenerator;
        _propagationService = propagationService;
        _notificationEngine = notificationEngine;
        _billingEngineService = billingEngineService;
        _budgetService = budgetService;
    }

    // ── Assembly CRUD ─────────────────────────────────────────────

    public async Task<List<AssemblyListDto>> GetAssembliesAsync(
        string tenantId, string? status = null, string? type = null,
        DateTime? fromDate = null, DateTime? toDate = null, string? search = null)
    {
        var query = _context.Assemblies.Where(a => a.TenantId == tenantId);

        if (!string.IsNullOrEmpty(status) && Enum.TryParse<AssemblyStatus>(status, true, out var statusEnum))
            query = query.Where(a => a.Status == statusEnum);

        if (!string.IsNullOrEmpty(type) && Enum.TryParse<AssemblyType>(type, true, out var typeEnum))
            query = query.Where(a => a.Type == typeEnum);

        if (fromDate.HasValue)
            query = query.Where(a => a.ScheduledDate >= fromDate.Value);

        if (toDate.HasValue)
            query = query.Where(a => a.ScheduledDate <= toDate.Value);

        if (!string.IsNullOrEmpty(search))
            query = query.Where(a => a.Title.Contains(search) || a.Location.Contains(search));

        return await query
            .OrderByDescending(a => a.ScheduledDate)
            .Select(a => new AssemblyListDto
            {
                Id = a.Id,
                Title = a.Title,
                Type = a.Type.ToString(),
                Status = a.Status.ToString(),
                ParticipationType = a.ParticipationType.ToString(),
                ScheduledDate = a.ScheduledDate,
                ScheduledTime = a.ScheduledTime,
                Location = a.Location,
                TotalCoefficients = a.TotalCoefficients,
                QuorumThresholdFirstCall = a.QuorumThresholdFirstCall,
                QuorumAchievedFirstCall = a.QuorumAchievedFirstCall,
                QuorumAchievedSecondCall = a.QuorumAchievedSecondCall,
                ConvocationNumber = a.ConvocationNumber,
                AttendanceCount = a.Attendances.Count(at =>
                    at.Status == AttendanceStatus.Present ||
                    at.Status == AttendanceStatus.Represented),
                AgendaItemCount = a.AgendaItems.Count,
                ApprovedItemsCount = a.AgendaItems.Count(ai => ai.IsApproved == true),
                PresidentName = a.PresidentName,
                SecretaryName = a.SecretaryName,
                CreatedByUserId = a.CreatedByUserId,
                CreatedAt = a.CreatedAt
            })
            .ToListAsync();
    }

    public async Task<AssemblyDetailDto> GetAssemblyByIdAsync(Guid id, string tenantId)
    {
        var assembly = await _context.Assemblies
            .Where(a => a.Id == id && a.TenantId == tenantId)
            .Select(a => new AssemblyDetailDto
            {
                Id = a.Id,
                Title = a.Title,
                Description = a.Description,
                Type = a.Type.ToString(),
                Status = a.Status.ToString(),
                ParticipationType = a.ParticipationType.ToString(),
                ScheduledDate = a.ScheduledDate,
                ScheduledTime = a.ScheduledTime,
                Location = a.Location,
                SecondConvocationDate = a.SecondConvocationDate,
                SecondConvocationTime = a.SecondConvocationTime,
                SecondConvocationLocation = a.SecondConvocationLocation,
                TotalCoefficients = a.TotalCoefficients,
                QuorumThresholdFirstCall = a.QuorumThresholdFirstCall,
                QuorumThresholdSecondCall = a.QuorumThresholdSecondCall,
                QuorumAchievedFirstCall = a.QuorumAchievedFirstCall,
                QuorumAchievedSecondCall = a.QuorumAchievedSecondCall,
                ConvocationNumber = a.ConvocationNumber,
                SessionStartTime = a.SessionStartTime,
                SessionEndTime = a.SessionEndTime,
                PresidentName = a.PresidentName,
                SecretaryName = a.SecretaryName,
                PresidentOwnerId = a.PresidentOwnerId,
                SecretaryOwnerId = a.SecretaryOwnerId,
                ConvocationSentAt = a.ConvocationSentAt,
                ConvocationDeadlineMet = a.ConvocationDeadlineMet,
                CreatedByUserId = a.CreatedByUserId,
                CreatedAt = a.CreatedAt
            })
            .FirstOrDefaultAsync();

        if (assembly == null)
            throw new InvalidOperationException("Assembly not found");

        assembly.Convocations = await GetConvocationsAsync(id, tenantId);
        assembly.AgendaItems = await GetAgendaItemsAsync(id, tenantId);
        assembly.Attendances = await GetAttendancesAsync(id, tenantId);
        assembly.Constancies = await GetConstanciesAsync(id, tenantId);

        var minutes = await _context.AssemblyMinutes
            .FirstOrDefaultAsync(m => m.AssemblyId == id && m.TenantId == tenantId);

        if (minutes != null)
        {
            assembly.Minutes = new AssemblyMinutesDto
            {
                Id = minutes.Id,
                Status = minutes.Status.ToString(),
                PresidentName = minutes.PresidentName,
                SecretaryName = minutes.SecretaryName,
                FullText = minutes.FullText,
                GeneratedAt = minutes.GeneratedAt,
                CommissionMemberNames = minutes.CommissionMemberNames,
                CommissionReviewDeadline = minutes.CommissionReviewDeadline,
                CommissionComments = minutes.CommissionComments,
                PresidentSignatureFilePath = minutes.PresidentSignatureFilePath,
                SecretarySignatureFilePath = minutes.SecretarySignatureFilePath,
                ApprovedAt = minutes.ApprovedAt,
                PublishedAt = minutes.PublishedAt,
                PublishNotificationCount = minutes.PublishNotificationCount,
                RevisionNotes = minutes.RevisionNotes
            };
        }

        return assembly;
    }

    public async Task<AssemblyDetailDto> CreateAssemblyAsync(
        CreateAssemblyRequestDto request, string tenantId, string userId)
    {
        var totalCoefficients = await _quorumEngine.GetTotalCoefficientsAsync(tenantId);

        var assembly = new Assembly
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            Title = request.Title,
            Description = request.Description,
            Type = Enum.TryParse<AssemblyType>(request.Type, true, out var type) ? type : AssemblyType.Ordinary,
            Status = AssemblyStatus.Draft,
            ParticipationType = Enum.TryParse<AssemblyParticipationType>(request.ParticipationType, true, out var part) ? part : AssemblyParticipationType.InPerson,
            ScheduledDate = request.ScheduledDate,
            ScheduledTime = request.ScheduledTime,
            Location = request.Location,
            SecondConvocationDate = request.SecondConvocationDate,
            SecondConvocationTime = request.SecondConvocationTime,
            SecondConvocationLocation = request.SecondConvocationLocation,
            TotalCoefficients = totalCoefficients,
            QuorumThresholdFirstCall = totalCoefficients * 0.5m,
            QuorumThresholdSecondCall = 0,
            CreatedByUserId = userId,
            CreatedAt = DateTime.UtcNow
        };

        _context.Assemblies.Add(assembly);
        await _context.SaveChangesAsync();

        return await GetAssemblyByIdAsync(assembly.Id, tenantId);
    }

    public async Task<AssemblyDetailDto> UpdateAssemblyAsync(
        Guid id, UpdateAssemblyRequestDto request, string tenantId, string userId)
    {
        var assembly = await _context.Assemblies
            .FirstOrDefaultAsync(a => a.Id == id && a.TenantId == tenantId);

        if (assembly == null)
            throw new InvalidOperationException("Assembly not found");

        if (assembly.Status != AssemblyStatus.Draft)
            throw new InvalidOperationException("Only draft assemblies can be edited");

        if (request.Title != null) assembly.Title = request.Title;
        if (request.Description != null) assembly.Description = request.Description;
        if (request.ParticipationType != null &&
            Enum.TryParse<AssemblyParticipationType>(request.ParticipationType, true, out var part))
            assembly.ParticipationType = part;
        if (request.ScheduledDate.HasValue) assembly.ScheduledDate = request.ScheduledDate.Value;
        if (request.ScheduledTime != null) assembly.ScheduledTime = request.ScheduledTime;
        if (request.Location != null) assembly.Location = request.Location;
        if (request.SecondConvocationDate.HasValue) assembly.SecondConvocationDate = request.SecondConvocationDate;
        if (request.SecondConvocationTime != null) assembly.SecondConvocationTime = request.SecondConvocationTime;
        if (request.SecondConvocationLocation != null) assembly.SecondConvocationLocation = request.SecondConvocationLocation;

        assembly.UpdatedByUserId = userId;

        await _context.SaveChangesAsync();

        return await GetAssemblyByIdAsync(id, tenantId);
    }

    public async Task DeleteAssemblyAsync(Guid id, string tenantId)
    {
        var assembly = await _context.Assemblies
            .FirstOrDefaultAsync(a => a.Id == id && a.TenantId == tenantId);

        if (assembly == null)
            throw new InvalidOperationException("Assembly not found");

        if (assembly.Status != AssemblyStatus.Draft)
            throw new InvalidOperationException("Only draft assemblies can be deleted");

        assembly.IsDeleted = true;
        assembly.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
    }

    public async Task UpdateSessionInfoAsync(
        Guid id, UpdateSessionRequestDto request, string tenantId, string userId)
    {
        var assembly = await _context.Assemblies
            .FirstOrDefaultAsync(a => a.Id == id && a.TenantId == tenantId);

        if (assembly == null)
            throw new InvalidOperationException("Assembly not found");

        if (request.PresidentName != null) assembly.PresidentName = request.PresidentName;
        if (request.PresidentOwnerId != null) assembly.PresidentOwnerId = request.PresidentOwnerId;
        if (request.SecretaryName != null) assembly.SecretaryName = request.SecretaryName;
        if (request.SecretaryOwnerId != null) assembly.SecretaryOwnerId = request.SecretaryOwnerId;
        if (request.ConvocationNumber.HasValue) assembly.ConvocationNumber = request.ConvocationNumber.Value;

        assembly.UpdatedByUserId = userId;
        await _context.SaveChangesAsync();
    }

    public async Task StartSessionAsync(Guid id, StartSessionRequestDto request, string tenantId, string userId)
    {
        var assembly = await _context.Assemblies
            .FirstOrDefaultAsync(a => a.Id == id && a.TenantId == tenantId);

        if (assembly == null)
            throw new InvalidOperationException("Assembly not found");

        if (assembly.Status != AssemblyStatus.Convoked)
            throw new InvalidOperationException("Assembly must be in Convoked status to start session");

        assembly.Status = AssemblyStatus.InSession;
        assembly.SessionStartTime = DateTime.UtcNow;
        assembly.ConvocationNumber = request.ConvocationNumber;
        assembly.PresidentName = request.PresidentName;
        assembly.PresidentOwnerId = request.PresidentOwnerId;
        assembly.SecretaryName = request.SecretaryName;
        assembly.SecretaryOwnerId = request.SecretaryOwnerId;
        assembly.UpdatedByUserId = userId;

        if (string.IsNullOrEmpty(assembly.ActNumber))
        {
            assembly.ActNumber = await _minutesGenerator.GenerateActNumberAsync(tenantId);
        }

        await RefreshQuorumSnapshotAsync(assembly, tenantId);

        await _context.SaveChangesAsync();
    }

    public async Task EndSessionAsync(Guid id, string tenantId, string userId)
    {
        var assembly = await _context.Assemblies
            .FirstOrDefaultAsync(a => a.Id == id && a.TenantId == tenantId);

        if (assembly == null)
            throw new InvalidOperationException("Assembly not found");

        if (assembly.Status != AssemblyStatus.InSession)
            throw new InvalidOperationException("Assembly must be in InSession status to end session");

        assembly.Status = AssemblyStatus.Closed;
        assembly.SessionEndTime = DateTime.UtcNow;
        assembly.UpdatedByUserId = userId;

        await _context.SaveChangesAsync();
    }

    public async Task ConvocateAsync(Guid id, string tenantId, string userId)
    {
        var assembly = await _context.Assemblies
            .FirstOrDefaultAsync(a => a.Id == id && a.TenantId == tenantId);

        if (assembly == null)
            throw new InvalidOperationException("Assembly not found");

        if (assembly.Status != AssemblyStatus.Draft)
            throw new InvalidOperationException("Only draft assemblies can be convoked");

        var businessDaysRequired = assembly.Type == AssemblyType.Ordinary ? 15 : 5;
        var scheduledDate = assembly.ScheduledDate;
        var businessDaysBefore = CountBusinessDays(scheduledDate, DateTime.UtcNow);

        assembly.ConvocationDeadlineMet = businessDaysBefore >= businessDaysRequired;
        assembly.ConvocationSentAt = DateTime.UtcNow.ToString("o");

        assembly.Status = AssemblyStatus.Convoked;
        assembly.UpdatedByUserId = userId;

        await _context.SaveChangesAsync();
    }

    // ── Convocation ───────────────────────────────────────────────

    public async Task<List<AssemblyConvocationDto>> GetConvocationsAsync(Guid assemblyId, string tenantId)
    {
        return await _context.AssemblyConvocations
            .Where(c => c.AssemblyId == assemblyId && c.TenantId == tenantId)
            .OrderBy(c => c.ConvocationNumber)
            .Select(c => new AssemblyConvocationDto
            {
                Id = c.Id,
                ConvocationNumber = c.ConvocationNumber,
                Subject = c.Subject,
                Notes = c.Notes,
                Channel = c.Channel,
                SentAt = c.SentAt,
                TotalRecipients = c.TotalRecipients,
                DeliveredCount = c.DeliveredCount,
                FailedCount = c.FailedCount,
                CreatedAt = c.CreatedAt
            })
            .ToListAsync();
    }

    public async Task<AssemblyConvocationDto> CreateConvocationAsync(
        Guid assemblyId, CreateConvocationRequestDto request, string tenantId, string userId)
    {
        var convocation = new AssemblyConvocation
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            AssemblyId = assemblyId,
            ConvocationNumber = request.ConvocationNumber,
            Subject = request.Subject,
            Notes = request.Notes,
            Channel = request.Channel,
            SentByUserId = userId,
            CreatedAt = DateTime.UtcNow
        };

        _context.AssemblyConvocations.Add(convocation);

        if (request.Documents != null)
        {
            foreach (var doc in request.Documents)
            {
                var document = new ConvocationDocument
                {
                    Id = Guid.NewGuid(),
                    TenantId = tenantId,
                    ConvocationId = convocation.Id,
                    DocumentName = doc.DocumentName,
                    DocumentType = doc.DocumentType,
                    FilePath = doc.FilePath,
                    Description = doc.Description,
                    CreatedAt = DateTime.UtcNow
                };
                _context.ConvocationDocuments.Add(document);
            }
        }

        var owners = await _quorumEngine.GetAllUnitsWithOwnersAsync(tenantId);

        foreach (var owner in owners.Where(o => o.OwnerId.HasValue))
        {
            var recipient = new ConvocationRecipient
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                ConvocationId = convocation.Id,
                UnitId = owner.UnitId,
                OwnerId = owner.OwnerId!.Value,
                OwnerName = owner.OwnerName ?? string.Empty,
                OwnerEmail = owner.OwnerEmail ?? string.Empty,
                OwnerPhone = owner.OwnerPhone,
                CreatedAt = DateTime.UtcNow
            };
            _context.ConvocationRecipients.Add(recipient);
        }

        convocation.TotalRecipients = owners.Count(o => o.OwnerId.HasValue);

        await _context.SaveChangesAsync();

        return (await GetConvocationsAsync(assemblyId, tenantId))
            .First(c => c.Id == convocation.Id);
    }

    public async Task SendConvocationAsync(
        Guid convocationId, string tenantId, string userId)
    {
        var convocation = await _context.AssemblyConvocations
            .FirstOrDefaultAsync(c => c.Id == convocationId && c.TenantId == tenantId);

        if (convocation == null)
            throw new InvalidOperationException("Convocation not found");

        var assembly = await _context.Assemblies
            .FirstOrDefaultAsync(a => a.Id == convocation.AssemblyId && a.TenantId == tenantId);

        var variables = new Dictionary<string, string>
        {
            ["AssemblyDate"] = string.Empty,
            ["AssemblyTime"] = string.Empty,
            ["Location"] = string.Empty
        };

        if (assembly != null)
        {
            variables["AssemblyDate"] = assembly.ScheduledDate.ToString("dd/MM/yyyy");
            variables["AssemblyTime"] = assembly.ScheduledTime;
            variables["Location"] = assembly.Location;
        }

        convocation.SentAt = DateTime.UtcNow;
        convocation.SentByUserId = userId;

        var recipients = await _context.ConvocationRecipients
            .Where(r => r.ConvocationId == convocationId)
            .ToListAsync();

        var deliveredCount = 0;
        var failedCount = 0;

        foreach (var recipient in recipients)
        {
            var notification = await _notificationEngine.ProcessEventAsync(
                tenantId, NotificationEventType.AssemblyConvocation,
                "Assembly", convocation.Id.ToString(), "AssemblyConvocation",
                ownerId: recipient.OwnerId, variables: variables);

            if (notification != null)
            {
                recipient.Delivered = true;
                recipient.DeliveredAt = DateTime.UtcNow;
                deliveredCount++;
            }
            else
            {
                recipient.Delivered = false;
                failedCount++;
            }
        }

        convocation.DeliveredCount = deliveredCount;
        convocation.FailedCount = failedCount;

        await _context.SaveChangesAsync();
    }

    // ── Attendance ────────────────────────────────────────────────

    public async Task<List<AssemblyAttendanceDto>> GetAttendancesAsync(Guid assemblyId, string tenantId)
    {
        return await _context.AssemblyAttendances
            .Where(a => a.AssemblyId == assemblyId && a.TenantId == tenantId)
            .OrderBy(a => a.CreatedAt)
            .Select(a => new AssemblyAttendanceDto
            {
                Id = a.Id,
                UnitId = a.UnitId,
                UnitIdentifier = a.Unit != null ? a.Unit.Identifier : "",
                OwnerId = a.OwnerId,
                OwnerName = a.Owner != null ? a.Owner.FullNameOrCompanyName : "",
                Coefficient = a.Coefficient,
                Status = a.Status.ToString(),
                AttendsPersonally = a.AttendsPersonally,
                RepresentativeOwnerId = a.RepresentativeOwnerId,
                RepresentativeName = a.RepresentativeName,
                RepresentativeDocumentNumber = a.RepresentativeDocumentNumber,
                PowerOfAttorneyFilePath = a.PowerOfAttorneyFilePath,
                ArrivalTime = a.ArrivalTime,
                DepartureTime = a.DepartureTime,
                HasDuesArrears = a.HasDuesArrears,
                VotingRightRestricted = a.VotingRightRestricted,
                VotingRestrictionReason = a.VotingRestrictionReason,
                VotingRestrictionLiftedByUserId = a.VotingRestrictionLiftedByUserId,
                VotingRestrictionLiftedReason = a.VotingRestrictionLiftedReason,
                VotingRestrictionLiftedAt = a.VotingRestrictionLiftedAt,
                IsCommissionMember = a.IsCommissionMember,
                CommissionRole = a.CommissionRole,
                Notes = a.Notes
            })
            .ToListAsync();
    }

    public async Task<AssemblyAttendanceDto> RegisterAttendanceAsync(
        Guid assemblyId, RegisterAttendanceRequestDto request, string tenantId, string userId)
    {
        var assembly = await _context.Assemblies
            .FirstOrDefaultAsync(a => a.Id == assemblyId && a.TenantId == tenantId);

        if (assembly == null)
            throw new InvalidOperationException("Assembly not found");

        var existing = await _context.AssemblyAttendances
            .FirstOrDefaultAsync(a => a.AssemblyId == assemblyId &&
                                     a.TenantId == tenantId &&
                                     a.UnitId == request.UnitId);

        if (existing != null)
            throw new InvalidOperationException("Attendance already registered for this unit");

        var unit = await _context.Units
            .FirstOrDefaultAsync(u => u.Id == request.UnitId && u.TenantId == tenantId);

        if (unit == null)
            throw new InvalidOperationException("Unit not found");

        var hasArrears = await CheckDuesArrearsAsync(request.OwnerId, tenantId);

        var attendance = new AssemblyAttendance
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            AssemblyId = assemblyId,
            UnitId = request.UnitId,
            OwnerId = request.OwnerId,
            Coefficient = unit.CoproprietyCoefficient,
            Status = AttendanceStatus.Present,
            AttendsPersonally = request.AttendsPersonally,
            RepresentativeOwnerId = request.RepresentativeOwnerId,
            RepresentativeName = request.RepresentativeName,
            RepresentativeDocumentNumber = request.RepresentativeDocumentNumber,
            PowerOfAttorneyFilePath = request.PowerOfAttorneyFilePath,
            ArrivalTime = DateTime.UtcNow,
            HasDuesArrears = hasArrears,
            VotingRightRestricted = hasArrears,
            VotingRestrictionReason = hasArrears ? "Propietario con deuda según artículo 40 de la Ley 675 de 2001" : null,
            IsCommissionMember = request.IsCommissionMember,
            CommissionRole = request.CommissionRole,
            Notes = request.Notes,
            RegisteredByUserId = userId,
            CreatedAt = DateTime.UtcNow
        };

        _context.AssemblyAttendances.Add(attendance);
        await _context.SaveChangesAsync();

        await RefreshQuorumSnapshotAsync(assembly, tenantId);
        await _context.SaveChangesAsync();

        return (await GetAttendancesAsync(assemblyId, tenantId))
            .First(a => a.Id == attendance.Id);
    }

    private async Task RefreshQuorumSnapshotAsync(Assembly assembly, string tenantId)
    {
        var quorumStatus = await _quorumEngine.CalculateQuorumAsync(assembly.Id, tenantId);
        assembly.TotalCoefficients = quorumStatus.TotalCoefficients;
        assembly.QuorumAchievedFirstCall = quorumStatus.FirstCallQuorumMet;
        assembly.QuorumAchievedSecondCall = quorumStatus.SecondCallQuorumMet;
    }

    public async Task UpdateAttendanceAsync(
        Guid attendanceId, UpdateAttendanceRequestDto request, string tenantId, string userId)
    {
        var attendance = await _context.AssemblyAttendances
            .FirstOrDefaultAsync(a => a.Id == attendanceId && a.TenantId == tenantId);

        if (attendance == null)
            throw new InvalidOperationException("Attendance record not found");

        if (request.Status != null && Enum.TryParse<AttendanceStatus>(request.Status, true, out var status))
            attendance.Status = status;

        if (request.DepartureTime.HasValue)
            attendance.DepartureTime = request.DepartureTime;

        if (request.Notes != null)
            attendance.Notes = request.Notes;

        await _context.SaveChangesAsync();
    }

    public async Task LiftVotingRestrictionAsync(
        Guid attendanceId, LiftVotingRestrictionRequestDto request, string tenantId, string userId)
    {
        await _votingEngine.LiftVotingRestrictionAsync(attendanceId, tenantId, request.Reason, userId);
    }

    // ── Agenda Items ──────────────────────────────────────────────

    public async Task<List<AssemblyAgendaItemDto>> GetAgendaItemsAsync(Guid assemblyId, string tenantId)
    {
        return await _context.AssemblyAgendaItems
            .Where(ai => ai.AssemblyId == assemblyId && ai.TenantId == tenantId)
            .OrderBy(ai => ai.SequenceNumber)
            .Select(ai => new AssemblyAgendaItemDto
            {
                Id = ai.Id,
                SequenceNumber = ai.SequenceNumber,
                Title = ai.Title,
                Description = ai.Description,
                PresenterName = ai.PresenterName,
                MajorityRequired = ai.MajorityRequired.ToString(),
                VotingMode = ai.VotingMode.ToString(),
                IsInformationOnly = ai.IsInformationOnly,
                RequiresVoting = ai.RequiresVoting,
                TotalCoefficientsForVote = ai.TotalCoefficientsForVote,
                VotesInFavorCoefficients = ai.VotesInFavorCoefficients,
                VotesAgainstCoefficients = ai.VotesAgainstCoefficients,
                AbstentionCoefficients = ai.AbstentionCoefficients,
                VotesInFavorCount = ai.VotesInFavorCount,
                VotesAgainstCount = ai.VotesAgainstCount,
                AbstentionCount = ai.AbstentionCount,
                IsApproved = ai.IsApproved,
                RejectionReason = ai.RejectionReason,
                Observations = ai.Observations,
                OwnerNotes = ai.OwnerNotes,
                VoteRegistered = ai.VoteRegistered,
                RegisteredByUserId = ai.RegisteredByUserId,
                VoteRegisteredAt = ai.VoteRegisteredAt,
                PropagationTarget = ai.PropagationTarget.ToString(),
                TargetBudgetId = ai.TargetBudgetId
            })
            .ToListAsync();
    }

    public async Task<AssemblyAgendaItemDto> CreateAgendaItemAsync(
        Guid assemblyId, CreateAgendaItemRequestDto request, string tenantId, string userId)
    {
        var assembly = await _context.Assemblies
            .FirstOrDefaultAsync(a => a.Id == assemblyId && a.TenantId == tenantId);

        if (assembly == null)
            throw new InvalidOperationException("Assembly not found");

        var eligibleCoefficients = await _quorumEngine.GetEligibleVotingCoefficientsAsync(assemblyId, tenantId);

        DecisionPropagationTarget? propagationTarget = null;
        if (!string.IsNullOrEmpty(request.PropagationTarget)
            && Enum.TryParse<DecisionPropagationTarget>(request.PropagationTarget, true, out var parsedTarget))
        {
            propagationTarget = parsedTarget;
        }

        DistributionType? extraordinaryFeeDistributionType = null;
        if (!string.IsNullOrEmpty(request.ExtraordinaryFeeDistributionType)
            && Enum.TryParse<DistributionType>(request.ExtraordinaryFeeDistributionType, true, out var parsedDistribution))
        {
            extraordinaryFeeDistributionType = parsedDistribution;
        }

        var item = new AssemblyAgendaItem
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            AssemblyId = assemblyId,
            SequenceNumber = request.SequenceNumber,
            Title = request.Title,
            Description = request.Description,
            PresenterName = request.PresenterName,
            MajorityRequired = Enum.TryParse<MajorityType>(request.MajorityRequired, true, out var maj) ? maj : MajorityType.Simple,
            VotingMode = Enum.TryParse<VotingMode>(request.VotingMode, true, out var vot) ? vot : VotingMode.Public,
            IsInformationOnly = request.IsInformationOnly,
            RequiresVoting = request.RequiresVoting,
            TotalCoefficientsForVote = eligibleCoefficients,
            PropagationTarget = propagationTarget,
            ExtraordinaryFeeTotalAmount = request.ExtraordinaryFeeTotalAmount,
            ExtraordinaryFeeInstallments = request.ExtraordinaryFeeInstallments,
            ExtraordinaryFeeStartPeriod = request.ExtraordinaryFeeStartPeriod,
            ExtraordinaryFeeDueDate = request.ExtraordinaryFeeDueDate,
            ExtraordinaryFeeDistributionType = extraordinaryFeeDistributionType,
            TargetBudgetId = request.TargetBudgetId
        };

        _context.AssemblyAgendaItems.Add(item);
        await _context.SaveChangesAsync();

        return (await GetAgendaItemsAsync(assemblyId, tenantId))
            .First(ai => ai.Id == item.Id);
    }

    public async Task<AssemblyAgendaItemDto> UpdateAgendaItemAsync(
        Guid itemId, UpdateAgendaItemRequestDto request, string tenantId, string userId)
    {
        var item = await _context.AssemblyAgendaItems
            .FirstOrDefaultAsync(ai => ai.Id == itemId && ai.TenantId == tenantId);

        if (item == null)
            throw new InvalidOperationException("Agenda item not found");

        if (item.VoteRegistered)
            throw new InvalidOperationException("Cannot edit an agenda item with registered vote");

        if (request.Title != null) item.Title = request.Title;
        if (request.Description != null) item.Description = request.Description;
        if (request.PresenterName != null) item.PresenterName = request.PresenterName;
        if (request.MajorityRequired != null && Enum.TryParse<MajorityType>(request.MajorityRequired, true, out var maj))
            item.MajorityRequired = maj;
        if (request.VotingMode != null && Enum.TryParse<VotingMode>(request.VotingMode, true, out var vot))
            item.VotingMode = vot;
        if (request.IsInformationOnly.HasValue) item.IsInformationOnly = request.IsInformationOnly.Value;
        if (request.RequiresVoting.HasValue) item.RequiresVoting = request.RequiresVoting.Value;

        await _context.SaveChangesAsync();

        return (await GetAgendaItemsAsync(item.AssemblyId, tenantId))
            .First(ai => ai.Id == itemId);
    }

    public async Task DeleteAgendaItemAsync(Guid itemId, string tenantId)
    {
        var item = await _context.AssemblyAgendaItems
            .FirstOrDefaultAsync(ai => ai.Id == itemId && ai.TenantId == tenantId);

        if (item == null)
            throw new InvalidOperationException("Agenda item not found");

        if (item.VoteRegistered)
            throw new InvalidOperationException("Cannot delete an agenda item with registered vote");

        _context.AssemblyAgendaItems.Remove(item);
        await _context.SaveChangesAsync();
    }

    // ── Voting ────────────────────────────────────────────────────

    public async Task<AssemblyAgendaItemDto> RegisterVoteAsync(
        Guid itemId, RegisterVoteRequestDto request, string tenantId, string userId)
    {
        var item = await _context.AssemblyAgendaItems
            .FirstOrDefaultAsync(ai => ai.Id == itemId && ai.TenantId == tenantId);

        if (item == null)
            throw new InvalidOperationException("Agenda item not found");

        if (item.IsInformationOnly)
            throw new InvalidOperationException("Cannot register vote for information-only items");

        var votingResult = await _votingEngine.CalculateVotingResultAsync(
            item.AssemblyId, itemId, tenantId,
            request.VotesInFavorCoefficients,
            request.VotesAgainstCoefficients,
            request.AbstentionCoefficients);

        item.VotesInFavorCoefficients = request.VotesInFavorCoefficients;
        item.VotesAgainstCoefficients = request.VotesAgainstCoefficients;
        item.AbstentionCoefficients = request.AbstentionCoefficients;
        item.VotesInFavorCount = request.VotesInFavorCount;
        item.VotesAgainstCount = request.VotesAgainstCount;
        item.AbstentionCount = request.AbstentionCount;
        item.IsApproved = votingResult.IsApproved;
        item.RejectionReason = votingResult.RejectionReason;
        item.Observations = request.Observations;
        item.OwnerNotes = request.OwnerNotes;
        item.VoteRegistered = true;
        item.RegisteredByUserId = userId;
        item.VoteRegisteredAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        if (item.PropagationTarget.HasValue)
        {
            await PropagateDecisionAsync(item, tenantId, userId);
        }

        return (await GetAgendaItemsAsync(item.AssemblyId, tenantId))
            .First(ai => ai.Id == itemId);
    }

    private async Task PropagateDecisionAsync(AssemblyAgendaItem item, string tenantId, string userId)
    {
        var propagation = await _propagationService.CreatePropagationAsync(
            item.AssemblyId, item.Id, tenantId, item.PropagationTarget!.Value,
            $"Propagación automática del punto de agenda '{item.Title}'.");

        if (item.IsApproved != true)
        {
            await _propagationService.MarkAsFailedAsync(
                propagation.Id, tenantId, "El punto de agenda fue rechazado en la votación.");
            return;
        }

        var assembly = await _context.Assemblies
            .FirstOrDefaultAsync(a => a.Id == item.AssemblyId && a.TenantId == tenantId);
        var actNumber = string.Empty;
        if (assembly != null && assembly.ActNumber != null)
        {
            actNumber = assembly.ActNumber;
        }

        try
        {
            if (item.PropagationTarget == DecisionPropagationTarget.ExtraordinaryFee)
            {
                if (!item.ExtraordinaryFeeTotalAmount.HasValue
                    || !item.ExtraordinaryFeeInstallments.HasValue
                    || !item.ExtraordinaryFeeDueDate.HasValue
                    || !item.ExtraordinaryFeeDistributionType.HasValue
                    || string.IsNullOrEmpty(item.ExtraordinaryFeeStartPeriod))
                {
                    throw new InvalidOperationException(
                        "El punto de agenda no tiene los datos de la cuota extraordinaria completos.");
                }

                var fee = await _billingEngineService.CreateExtraordinaryFeeFromDecisionAsync(
                    tenantId, item.Title, item.Description ?? string.Empty, actNumber,
                    item.ExtraordinaryFeeTotalAmount.Value, item.ExtraordinaryFeeInstallments.Value,
                    item.ExtraordinaryFeeStartPeriod, item.ExtraordinaryFeeDistributionType.Value,
                    item.ExtraordinaryFeeDueDate.Value, userId);

                await _propagationService.MarkAsPropagatedAsync(
                    propagation.Id, tenantId, fee.Id.ToString(), "ExtraordinaryFee");
            }
            else if (item.PropagationTarget == DecisionPropagationTarget.Budget)
            {
                if (!item.TargetBudgetId.HasValue)
                {
                    throw new InvalidOperationException(
                        "El punto de agenda no tiene asociado el presupuesto a activar.");
                }

                var approveRequest = new ApproveBudgetRequestDto
                {
                    MeetingActNumber = actNumber,
                    ApprovalDate = DateTime.UtcNow
                };

                await _budgetService.ApproveBudgetAsync(tenantId, item.TargetBudgetId.Value, approveRequest);

                await _propagationService.MarkAsPropagatedAsync(
                    propagation.Id, tenantId, item.TargetBudgetId.Value.ToString(), "Budget");
            }
        }
        catch (Exception ex)
        {
            await _propagationService.MarkAsFailedAsync(propagation.Id, tenantId, ex.Message);
        }
    }

    // ── Constancies ───────────────────────────────────────────────

    public async Task<List<AssemblyConstancyDto>> GetConstanciesAsync(Guid assemblyId, string tenantId)
    {
        return await _context.AssemblyConstancies
            .Where(c => c.AssemblyId == assemblyId && c.TenantId == tenantId)
            .OrderBy(c => c.CreatedAt)
            .Select(c => new AssemblyConstancyDto
            {
                Id = c.Id,
                AgendaItemId = c.AgendaItemId,
                AgendaItemTitle = c.AgendaItem != null ? c.AgendaItem.Title : null,
                OwnerId = c.OwnerId,
                OwnerName = c.OwnerName,
                Text = c.Text,
                CreatedAt = c.CreatedAt
            })
            .ToListAsync();
    }

    public async Task<AssemblyConstancyDto> CreateConstancyAsync(
        Guid assemblyId, CreateConstancyRequestDto request, string tenantId, string userId)
    {
        var constancy = new AssemblyConstancy
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            AssemblyId = assemblyId,
            AgendaItemId = request.AgendaItemId,
            OwnerId = request.OwnerId,
            OwnerName = request.OwnerName,
            Text = request.Text,
            CreatedAt = DateTime.UtcNow,
            RegisteredByUserId = userId
        };

        _context.AssemblyConstancies.Add(constancy);
        await _context.SaveChangesAsync();

        return (await GetConstanciesAsync(assemblyId, tenantId))
            .First(c => c.Id == constancy.Id);
    }

    // ── Minutes ───────────────────────────────────────────────────

    public async Task<AssemblyMinutesDto> GenerateMinutesAsync(
        Guid assemblyId, GenerateMinutesRequestDto request, string tenantId, string userId)
    {
        var minutes = await _minutesGenerator.CreateMinutesAsync(
            assemblyId, tenantId, userId,
            request.PresidentName, request.SecretaryName,
            request.CommissionMemberNames);

        return new AssemblyMinutesDto
        {
            Id = minutes.Id,
            Status = minutes.Status.ToString(),
            PresidentName = minutes.PresidentName,
            SecretaryName = minutes.SecretaryName,
            FullText = minutes.FullText,
            GeneratedAt = minutes.GeneratedAt,
            CommissionMemberNames = minutes.CommissionMemberNames,
            CommissionReviewDeadline = minutes.CommissionReviewDeadline,
            CommissionComments = minutes.CommissionComments,
            PresidentSignatureFilePath = minutes.PresidentSignatureFilePath,
            SecretarySignatureFilePath = minutes.SecretarySignatureFilePath,
            ApprovedAt = minutes.ApprovedAt,
            PublishedAt = minutes.PublishedAt,
            PublishNotificationCount = minutes.PublishNotificationCount,
            RevisionNotes = minutes.RevisionNotes
        };
    }

    public async Task<AssemblyMinutesDto> ApproveMinutesAsync(
        Guid assemblyId, ApproveMinutesRequestDto request, string tenantId, string userId)
    {
        var minutes = await _context.AssemblyMinutes
            .FirstOrDefaultAsync(m => m.AssemblyId == assemblyId && m.TenantId == tenantId);

        if (minutes == null)
            throw new InvalidOperationException("Minutes not found");

        if (minutes.Status == MinutesStatus.Approved || minutes.Status == MinutesStatus.Published)
            throw new InvalidOperationException("Minutes are already approved or published");

        minutes.Status = MinutesStatus.Approved;
        minutes.PresidentSignatureFilePath = request.PresidentSignatureFilePath ?? minutes.PresidentSignatureFilePath;
        minutes.SecretarySignatureFilePath = request.SecretarySignatureFilePath ?? minutes.SecretarySignatureFilePath;
        minutes.CommissionComments = request.CommissionComments ?? minutes.CommissionComments;
        minutes.ApprovedAt = DateTime.UtcNow;
        minutes.ApprovedByUserId = userId;
        minutes.UpdatedAt = DateTime.UtcNow;
        minutes.UpdatedByUserId = userId;

        var assembly = await _context.Assemblies
            .FirstOrDefaultAsync(a => a.Id == assemblyId && a.TenantId == tenantId);

        if (assembly != null)
        {
            assembly.Status = AssemblyStatus.MinutesApproved;
            assembly.UpdatedByUserId = userId;
        }

        await _context.SaveChangesAsync();

        return new AssemblyMinutesDto
        {
            Id = minutes.Id,
            Status = minutes.Status.ToString(),
            PresidentName = minutes.PresidentName,
            SecretaryName = minutes.SecretaryName,
            FullText = minutes.FullText,
            GeneratedAt = minutes.GeneratedAt,
            CommissionMemberNames = minutes.CommissionMemberNames,
            CommissionReviewDeadline = minutes.CommissionReviewDeadline,
            CommissionComments = minutes.CommissionComments,
            PresidentSignatureFilePath = minutes.PresidentSignatureFilePath,
            SecretarySignatureFilePath = minutes.SecretarySignatureFilePath,
            ApprovedAt = minutes.ApprovedAt,
            PublishedAt = minutes.PublishedAt,
            PublishNotificationCount = minutes.PublishNotificationCount,
            RevisionNotes = minutes.RevisionNotes
        };
    }

    public async Task PublishMinutesAsync(Guid assemblyId, string tenantId, string userId)
    {
        var minutes = await _context.AssemblyMinutes
            .FirstOrDefaultAsync(m => m.AssemblyId == assemblyId && m.TenantId == tenantId);

        if (minutes == null)
            throw new InvalidOperationException("Minutes not found");

        if (minutes.Status != MinutesStatus.Approved)
            throw new InvalidOperationException("Minutes must be approved before publishing");

        minutes.Status = MinutesStatus.Published;
        minutes.PublishedAt = DateTime.UtcNow;
        minutes.PublishedByUserId = userId;
        minutes.UpdatedAt = DateTime.UtcNow;
        minutes.UpdatedByUserId = userId;

        var assembly = await _context.Assemblies
            .FirstOrDefaultAsync(a => a.Id == assemblyId && a.TenantId == tenantId);

        if (assembly != null)
        {
            assembly.Status = AssemblyStatus.Published;
            assembly.UpdatedByUserId = userId;
        }

        var recipients = await _context.ConvocationRecipients
            .Where(r => r.Convocation != null && r.Convocation.AssemblyId == assemblyId)
            .Select(r => r.OwnerId)
            .Distinct()
            .ToListAsync();

        var assemblyDate = string.Empty;
        if (assembly != null)
        {
            assemblyDate = assembly.ScheduledDate.ToString("dd/MM/yyyy");
        }

        var variables = new Dictionary<string, string>
        {
            ["AssemblyDate"] = assemblyDate,
            ["ActNumber"] = minutes.ActNumber ?? string.Empty
        };

        var notificationCount = 0;
        foreach (var ownerId in recipients)
        {
            var notification = await _notificationEngine.ProcessEventAsync(
                tenantId, NotificationEventType.AssemblyMinutesPublished,
                "Assembly", minutes.Id.ToString(), "AssemblyMinutes",
                ownerId: ownerId, variables: variables);

            if (notification != null)
            {
                notificationCount++;
            }
        }

        minutes.PublishNotificationCount = notificationCount;

        await _context.SaveChangesAsync();
    }

    // ── Decision Propagation ──────────────────────────────────────

    public async Task<List<AssemblyDecisionPropagationDto>> GetPropagationsAsync(
        Guid assemblyId, string tenantId)
    {
        var propagations = await _propagationService.GetPropagationsByAssemblyAsync(assemblyId, tenantId);

        return propagations.Select(p => new AssemblyDecisionPropagationDto
        {
            Id = p.Id,
            AgendaItemId = p.AgendaItemId,
            AgendaItemTitle = p.AgendaItem != null ? p.AgendaItem.Title : "",
            TargetModule = p.TargetModule.ToString(),
            Status = p.Status.ToString(),
            Description = p.Description,
            TargetEntityId = p.TargetEntityId,
            TargetEntityType = p.TargetEntityType,
            ErrorMessage = p.ErrorMessage,
            RetryCount = p.RetryCount,
            CreatedAt = p.CreatedAt,
            PropagatedAt = p.PropagatedAt
        }).ToList();
    }

    public async Task<AssemblyDecisionPropagationDto> CreatePropagationAsync(
        Guid assemblyId, CreateDecisionPropagationRequestDto request, string tenantId, string userId)
    {
        var targetModule = Enum.TryParse<DecisionPropagationTarget>(request.TargetModule, true, out var target)
            ? target
            : DecisionPropagationTarget.Other;

        var propagation = await _propagationService.CreatePropagationAsync(
            assemblyId, request.AgendaItemId, tenantId, targetModule, request.Description);

        return new AssemblyDecisionPropagationDto
        {
            Id = propagation.Id,
            AgendaItemId = propagation.AgendaItemId,
            TargetModule = propagation.TargetModule.ToString(),
            Status = propagation.Status.ToString(),
            Description = propagation.Description,
            CreatedAt = propagation.CreatedAt
        };
    }

    // ── Quorum ────────────────────────────────────────────────────

    public async Task<QuorumStatusDto> GetQuorumStatusAsync(Guid assemblyId, string tenantId)
    {
        var status = await _quorumEngine.CalculateQuorumAsync(assemblyId, tenantId);

        return new QuorumStatusDto
        {
            TotalCoefficients = status.TotalCoefficients,
            PresentCoefficients = status.PresentCoefficients,
            QuorumThresholdFirstCall = status.QuorumThresholdFirstCall,
            QuorumThresholdSecondCall = status.QuorumThresholdSecondCall,
            FirstCallQuorumMet = status.FirstCallQuorumMet,
            SecondCallQuorumMet = status.SecondCallQuorumMet,
            PercentagePresent = status.PercentagePresent,
            TotalOwners = status.TotalOwners,
            PresentOwners = status.PresentOwners,
            AbsentOwners = status.AbsentOwners,
            OwnersWithArrears = status.OwnersWithArrears,
            OwnersWithRestrictedVoting = status.OwnersWithRestrictedVoting
        };
    }

    public async Task<List<UnitWithOwnerInfo>> GetUnitsForAttendanceAsync(string tenantId)
    {
        return await _quorumEngine.GetAllUnitsWithOwnersAsync(tenantId);
    }

    // ── Reports ───────────────────────────────────────────────────

    public async Task<AssemblyReportDto> GetReportAsync(string tenantId)
    {
        var assemblies = await _context.Assemblies
            .Where(a => a.TenantId == tenantId)
            .ToListAsync();

        var nextScheduled = assemblies
            .Where(a => a.Status == AssemblyStatus.Draft || a.Status == AssemblyStatus.Convoked)
            .OrderBy(a => a.ScheduledDate)
            .FirstOrDefault();

        return new AssemblyReportDto
        {
            TotalAssemblies = assemblies.Count,
            OrdinaryAssemblies = assemblies.Count(a => a.Type == AssemblyType.Ordinary),
            ExtraordinaryAssemblies = assemblies.Count(a => a.Type == AssemblyType.Extraordinary),
            PublishedAssemblies = assemblies.Count(a => a.Status == AssemblyStatus.Published),
            PendingMinutesAssemblies = assemblies.Count(a => a.Status == AssemblyStatus.Closed),
            NextScheduledAssembly = nextScheduled?.ScheduledDate,
            NextAssemblyTitle = nextScheduled?.Title
        };
    }

    // ── Helpers ───────────────────────────────────────────────────

    private async Task<bool> CheckDuesArrearsAsync(Guid ownerId, string tenantId)
    {
        return await _context.UnitFees
            .Where(uf => uf.TenantId == tenantId &&
                        uf.Unit != null &&
                        uf.Unit.UnitOwners.Any(uo => uo.OwnerId == ownerId && uo.IsActive) &&
                        uf.Status == Domain.Enums.FeeStatus.Pending &&
                        uf.DueDate < DateTime.UtcNow)
            .AnyAsync();
    }

    private int CountBusinessDays(DateTime from, DateTime to)
    {
        int count = 0;
        var current = to.Date;
        var end = from.Date;

        while (current < end)
        {
            if (current.DayOfWeek != DayOfWeek.Saturday && current.DayOfWeek != DayOfWeek.Sunday)
                count++;

            current = current.AddDays(1);
        }

        return count;
    }
}
