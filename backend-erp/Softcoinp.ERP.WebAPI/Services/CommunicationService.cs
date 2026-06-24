using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Softcoinp.ERP.Domain.Entities;
using Softcoinp.ERP.Domain.Enums;
using Softcoinp.ERP.Infrastructure.Persistence;
using Softcoinp.ERP.WebAPI.DTOs;

namespace Softcoinp.ERP.WebAPI.Services;

public class CommunicationService
{
    private readonly ApplicationDbContext _context;
    private readonly DeliveryTrackerEngine _deliveryTracker;

    public CommunicationService(
        ApplicationDbContext context,
        DeliveryTrackerEngine deliveryTracker)
    {
        _context = context;
        _deliveryTracker = deliveryTracker;
    }

    public async Task<List<CommunicationSummaryDto>> GetListAsync(
        string tenantId, string? status = null, DateTime? from = null, DateTime? to = null)
    {
        var query = _context.Communications.Where(c => c.TenantId == tenantId && !c.IsDeleted);

        if (!string.IsNullOrEmpty(status))
            query = query.Where(c => c.Status.ToString() == status);

        if (from.HasValue)
            query = query.Where(c => c.CreatedAt >= from.Value);

        if (to.HasValue)
            query = query.Where(c => c.CreatedAt <= to.Value);

        return await query
            .OrderByDescending(c => c.CreatedAt)
            .Select(c => new CommunicationSummaryDto
            {
                Id = c.Id,
                Subject = c.Subject,
                Status = c.Status.ToString(),
                AudienceType = c.AudienceType.ToString(),
                RequiresReadConfirmation = c.RequiresReadConfirmation,
                PublishToBulletinBoard = c.PublishToBulletinBoard,
                SendAt = c.SendAt,
                SentAt = c.SentAt,
                RecipientCount = c.Recipients.Count,
                ReadConfirmedCount = c.Recipients.Count(r => r.ReadConfirmedAt != null),
                CreatedAt = c.CreatedAt
            })
            .ToListAsync();
    }

    public async Task<CommunicationDetailDto?> GetByIdAsync(Guid id, string tenantId)
    {
        var communication = await _context.Communications
            .Include(c => c.Recipients)
                .ThenInclude(r => r.Owner)
            .Include(c => c.Recipients)
                .ThenInclude(r => r.TenantResident)
            .FirstOrDefaultAsync(c => c.Id == id && c.TenantId == tenantId && !c.IsDeleted);

        if (communication == null) return null;

        var channels = string.IsNullOrEmpty(communication.SelectedChannels)
            ? new List<string>()
            : communication.SelectedChannels.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).ToList();

        var filePaths = string.IsNullOrEmpty(communication.FilePaths)
            ? new List<string>()
            : JsonSerializer.Deserialize<List<string>>(communication.FilePaths) ?? new List<string>();

        return new CommunicationDetailDto
        {
            Id = communication.Id,
            Subject = communication.Subject,
            Body = communication.Body,
            Status = communication.Status.ToString(),
            AudienceType = communication.AudienceType.ToString(),
            SelectedChannels = channels,
            SendAt = communication.SendAt,
            SentAt = communication.SentAt,
            RequiresReadConfirmation = communication.RequiresReadConfirmation,
            PublishToBulletinBoard = communication.PublishToBulletinBoard,
            RelatedCommunicationId = communication.RelatedCommunicationId,
            FilePaths = filePaths,
            CreatedByUserId = communication.CreatedByUserId,
            CreatedAt = communication.CreatedAt,
            Recipients = communication.Recipients.Select(r => new CommunicationRecipientDto
            {
                Id = r.Id,
                OwnerId = r.OwnerId,
                OwnerName = r.Owner != null ? r.Owner.FullNameOrCompanyName : null,
                TenantResidentId = r.TenantResidentId,
                TenantResidentName = r.TenantResident != null ? r.TenantResident.FullName : null,
                RecipientEmail = r.RecipientEmail,
                RecipientPhone = r.RecipientPhone,
                EmailStatus = r.EmailStatus.ToString(),
                SmsStatus = r.SmsStatus.ToString(),
                PushStatus = r.PushStatus.ToString(),
                BulletinBoardStatus = r.BulletinBoardStatus.ToString(),
                ReadConfirmedAt = r.ReadConfirmedAt,
                ResentCount = r.ResentCount,
                ErrorMessage = r.ErrorMessage
            }).ToList()
        };
    }

    public async Task<CommunicationDetailDto> CreateAsync(CreateCommunicationRequest request, string tenantId, string userId)
    {
        var audienceType = (AudienceType)Enum.Parse(typeof(AudienceType), request.AudienceType);
        var channels = string.Join(",", request.SelectedChannels);
        var filePaths = request.FilePaths != null ? JsonSerializer.Serialize(request.FilePaths) : string.Empty;

        var communication = new Communication
        {
            TenantId = tenantId,
            Subject = request.Subject,
            Body = request.Body,
            Status = request.SendAt.HasValue ? CommunicationStatus.Scheduled : CommunicationStatus.Draft,
            AudienceType = audienceType,
            SelectedChannels = channels,
            SendAt = request.SendAt,
            RequiresReadConfirmation = request.RequiresReadConfirmation,
            PublishToBulletinBoard = request.PublishToBulletinBoard,
            FilePaths = filePaths,
            CreatedByUserId = userId
        };

        _context.Communications.Add(communication);
        await _context.SaveChangesAsync();

        return await GetByIdAsync(communication.Id, tenantId)
            ?? throw new InvalidOperationException("Error al crear el comunicado");
    }

    public async Task<CommunicationDetailDto?> UpdateAsync(Guid id, UpdateCommunicationRequest request, string tenantId)
    {
        var communication = await _context.Communications
            .FirstOrDefaultAsync(c => c.Id == id && c.TenantId == tenantId && !c.IsDeleted);

        if (communication == null) return null;

        if (communication.Status == CommunicationStatus.Sent)
            throw new InvalidOperationException("No se puede modificar un comunicado ya enviado");

        if (request.Subject != null) communication.Subject = request.Subject;
        if (request.Body != null) communication.Body = request.Body;
        if (request.AudienceType != null)
            communication.AudienceType = (AudienceType)Enum.Parse(typeof(AudienceType), request.AudienceType);
        if (request.SelectedChannels != null)
            communication.SelectedChannels = string.Join(",", request.SelectedChannels);
        if (request.SendAt != null) communication.SendAt = request.SendAt;
        if (request.RequiresReadConfirmation.HasValue)
            communication.RequiresReadConfirmation = request.RequiresReadConfirmation.Value;
        if (request.PublishToBulletinBoard.HasValue)
            communication.PublishToBulletinBoard = request.PublishToBulletinBoard.Value;
        if (request.FilePaths != null)
            communication.FilePaths = JsonSerializer.Serialize(request.FilePaths);

        if (request.SendAt == null && communication.Status == CommunicationStatus.Scheduled)
            communication.Status = CommunicationStatus.Draft;

        await _context.SaveChangesAsync();

        return await GetByIdAsync(id, tenantId);
    }

    public async Task<CommunicationDetailDto?> PrepareAndSendAsync(Guid id, string tenantId)
    {
        var communication = await _context.Communications
            .FirstOrDefaultAsync(c => c.Id == id && c.TenantId == tenantId && !c.IsDeleted);

        if (communication == null) return null;

        if (communication.Status != CommunicationStatus.Draft &&
            communication.Status != CommunicationStatus.Scheduled)
            throw new InvalidOperationException("El comunicado ya fue enviado o archivado");

        await ResolveRecipientsAsync(communication);

        if (communication.PublishToBulletinBoard &&
            communication.SelectedChannels.Contains("BulletinBoard"))
        {
            var boardService = new BulletinBoardService(_context);
            await boardService.CreateAsync(new CreateBulletinBoardPostRequest
            {
                Title = communication.Subject,
                Content = communication.Body,
                PublishedAt = DateTime.UtcNow,
                Category = "Administrative"
            }, tenantId, communication.CreatedByUserId);
        }

        await _deliveryTracker.ProcessCommunicationDeliveryAsync(communication.Id);

        return await GetByIdAsync(id, tenantId);
    }

    public async Task<bool> ArchiveAsync(Guid id, string tenantId)
    {
        var communication = await _context.Communications
            .FirstOrDefaultAsync(c => c.Id == id && c.TenantId == tenantId);

        if (communication == null) return false;

        communication.IsDeleted = true;
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> CancelScheduledAsync(Guid id, string tenantId)
    {
        var communication = await _context.Communications
            .FirstOrDefaultAsync(c => c.Id == id && c.TenantId == tenantId && !c.IsDeleted);

        if (communication == null) return false;

        if (communication.Status != CommunicationStatus.Scheduled)
            throw new InvalidOperationException("Solo se pueden cancelar comunicados programados");

        communication.Status = CommunicationStatus.Draft;
        communication.SendAt = null;
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task ResendToUnconfirmedAsync(Guid id, string tenantId)
    {
        var communication = await _context.Communications
            .FirstOrDefaultAsync(c => c.Id == id && c.TenantId == tenantId && !c.IsDeleted);

        if (communication == null)
            throw new InvalidOperationException("Comunicado no encontrado");

        await _deliveryTracker.ResendToUnconfirmedAsync(id);
    }

    public async Task<List<CommunicationSummaryDto>> GetPendingScheduledAsync()
    {
        var now = DateTime.UtcNow;
        var communications = await _context.Communications
            .Where(c => c.Status == CommunicationStatus.Scheduled && c.SendAt <= now)
            .ToListAsync();

        var result = new List<CommunicationSummaryDto>();

        foreach (var communication in communications)
        {
            try
            {
                await ResolveRecipientsAsync(communication);

                if (communication.PublishToBulletinBoard)
                {
                    var boardService = new BulletinBoardService(_context);
                    await boardService.CreateAsync(new CreateBulletinBoardPostRequest
                    {
                        Title = communication.Subject,
                        Content = communication.Body,
                        PublishedAt = DateTime.UtcNow,
                        Category = "Administrative"
                    }, communication.TenantId, communication.CreatedByUserId);
                }

                await _deliveryTracker.ProcessCommunicationDeliveryAsync(communication.Id);
                result.Add(new CommunicationSummaryDto
                {
                    Id = communication.Id,
                    Subject = communication.Subject,
                    Status = CommunicationStatus.Sent.ToString(),
                    SentAt = DateTime.UtcNow
                });
            }
            catch
            {
                // Log error but continue processing others
            }
        }

        return result;
    }

    public async Task<CommunicationEffectivenessReportDto> GetEffectivenessReportAsync(
        string tenantId, DateTime? from = null, DateTime? to = null)
    {
        var query = _context.Communications
            .Include(c => c.Recipients)
            .Where(c => c.TenantId == tenantId && c.Status == CommunicationStatus.Sent && !c.IsDeleted);

        if (from.HasValue)
            query = query.Where(c => c.SentAt >= from.Value);

        if (to.HasValue)
            query = query.Where(c => c.SentAt <= to.Value);

        var communications = await query.ToListAsync();

        var totalRecipients = communications.Sum(c => c.Recipients.Count);
        var emailDelivered = communications.Sum(c => c.Recipients.Count(r => r.EmailStatus == DeliveryStatus.Delivered));
        var emailOpened = communications.Sum(c => c.Recipients.Count(r => r.EmailStatus == DeliveryStatus.Read));
        var emailBounced = communications.Sum(c => c.Recipients.Count(r => r.EmailStatus == DeliveryStatus.Bounced));
        var smsDelivered = communications.Sum(c => c.Recipients.Count(r => r.SmsStatus == DeliveryStatus.Delivered));
        var smsFailed = communications.Sum(c => c.Recipients.Count(r => r.SmsStatus == DeliveryStatus.Failed));
        var pushDelivered = communications.Sum(c => c.Recipients.Count(r => r.PushStatus == DeliveryStatus.Delivered));
        var readConfirmations = communications.Sum(c => c.Recipients.Count(r => r.ReadConfirmedAt != null));

        return new CommunicationEffectivenessReportDto
        {
            TotalCommunications = communications.Count,
            TotalRecipients = totalRecipients,
            EmailDelivered = emailDelivered,
            EmailOpened = emailOpened,
            EmailBounced = emailBounced,
            SmsDelivered = smsDelivered,
            SmsFailed = smsFailed,
            PushDelivered = pushDelivered,
            ReadConfirmations = readConfirmations,
            DeliveryRate = totalRecipients > 0
                ? Math.Round((double)(emailDelivered + smsDelivered + pushDelivered) / (totalRecipients * 3) * 100, 2)
                : 0,
            OpenRate = emailDelivered > 0
                ? Math.Round((double)emailOpened / emailDelivered * 100, 2)
                : 0,
            ReadConfirmationRate = totalRecipients > 0
                ? Math.Round((double)readConfirmations / totalRecipients * 100, 2)
                : 0
        };
    }

    private async Task ResolveRecipientsAsync(Communication communication)
    {
        var tenantId = communication.TenantId;
        var channels = string.IsNullOrEmpty(communication.SelectedChannels)
            ? new List<string>()
            : communication.SelectedChannels.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).ToList();

        _context.CommunicationRecipients.RemoveRange(
            _context.CommunicationRecipients.Where(r => r.CommunicationId == communication.Id));

        var recipients = new List<CommunicationRecipient>();

        switch (communication.AudienceType)
        {
            case AudienceType.AllOwners:
                var owners = await _context.Owners
                    .Where(o => o.TenantId == tenantId && o.IsActive)
                    .ToListAsync();

                foreach (var owner in owners)
                {
                    recipients.Add(CreateRecipient(communication.Id, ownerId: owner.Id,
                        email: owner.Email, phone: owner.MainPhone));
                }
                break;

            case AudienceType.AllResidents:
                var allOwners = await _context.Owners
                    .Where(o => o.TenantId == tenantId && o.IsActive)
                    .ToListAsync();

                foreach (var owner in allOwners)
                {
                    recipients.Add(CreateRecipient(communication.Id, ownerId: owner.Id,
                        email: owner.Email, phone: owner.MainPhone));
                }

                var tenants = await _context.TenantResidents
                    .Where(t => t.TenantId == tenantId && t.IsActive)
                    .ToListAsync();

                foreach (var tenant in tenants)
                {
                    recipients.Add(CreateRecipient(communication.Id, tenantResidentId: tenant.Id,
                        email: tenant.Email, phone: tenant.Phone));
                }
                break;

            case AudienceType.SpecificUnits:
                var unitsWithOwners = await _context.Set<UnitOwner>()
                    .Where(uo => uo.TenantId == tenantId && uo.IsActive)
                    .Include(uo => uo.Owner)
                    .ToListAsync();

                foreach (var uo in unitsWithOwners)
                {
                    if (uo.Owner == null) continue;

                    recipients.Add(CreateRecipient(communication.Id, ownerId: uo.OwnerId,
                        email: uo.Owner.Email, phone: uo.Owner.MainPhone));
                }

                var unitTenants = await _context.TenantResidents
                    .Where(t => t.TenantId == tenantId && t.IsActive)
                    .ToListAsync();

                foreach (var tenant in unitTenants)
                {
                    recipients.Add(CreateRecipient(communication.Id, tenantResidentId: tenant.Id,
                        email: tenant.Email, phone: tenant.Phone));
                }
                break;

            case AudienceType.SpecificTowers:
                var towerOwners = await _context.Set<UnitOwner>()
                    .Where(uo => uo.TenantId == tenantId && uo.IsActive)
                    .Include(uo => uo.Unit)
                    .Include(uo => uo.Owner)
                    .ToListAsync();

                foreach (var uo in towerOwners)
                {
                    if (uo.Owner == null) continue;

                    recipients.Add(CreateRecipient(communication.Id, ownerId: uo.OwnerId,
                        email: uo.Owner.Email, phone: uo.Owner.MainPhone));
                }

                var towerTenants = await _context.TenantResidents
                    .Where(t => t.TenantId == tenantId && t.IsActive)
                    .ToListAsync();

                foreach (var tenant in towerTenants)
                {
                    recipients.Add(CreateRecipient(communication.Id, tenantResidentId: tenant.Id,
                        email: tenant.Email, phone: tenant.Phone));
                }
                break;
        }

        _context.CommunicationRecipients.AddRange(recipients);
        await _context.SaveChangesAsync();
    }

    private static CommunicationRecipient CreateRecipient(
        Guid communicationId,
        Guid? ownerId = null,
        Guid? tenantResidentId = null,
        string email = "",
        string phone = "")
    {
        return new CommunicationRecipient
        {
            CommunicationId = communicationId,
            OwnerId = ownerId,
            TenantResidentId = tenantResidentId,
            RecipientEmail = email,
            RecipientPhone = phone
        };
    }
}
