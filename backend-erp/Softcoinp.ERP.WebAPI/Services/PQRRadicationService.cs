using System;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Softcoinp.ERP.Domain.Entities;
using Softcoinp.ERP.Domain.Enums;
using Softcoinp.ERP.Infrastructure.Persistence;
using Softcoinp.ERP.WebAPI.DTOs;

namespace Softcoinp.ERP.WebAPI.Services;

public class PQRRadicationService
{
    private readonly ApplicationDbContext _context;
    private readonly NotificationEngine _notificationEngine;

    public PQRRadicationService(ApplicationDbContext context, NotificationEngine notificationEngine)
    {
        _context = context;
        _notificationEngine = notificationEngine;
    }

    public async Task<PqrCreatedResponseDto> RadicateAsync(string tenantId, string userId, CreatePqrRequestDto request)
    {
        if (!Enum.TryParse<PQRType>(request.PQRType, true, out var pqrType))
        {
            throw new ArgumentException("Tipo de PQR inválido. Use: Request, Complaint o Claim.");
        }

        if (!Enum.TryParse<PQRCategory>(request.Category, true, out var category))
        {
            throw new ArgumentException("Categoría inválida.");
        }

        if (!Enum.TryParse<PQRChannel>(request.Channel, true, out var channel))
        {
            throw new ArgumentException("Canal inválido. Use: WebPortal, Email, InPerson o Verbal.");
        }

        if (request.RelatedPQRId.HasValue)
        {
            var relatedExists = await _context.PqrRecords
                .AnyAsync(p => p.Id == request.RelatedPQRId.Value && p.TenantId == tenantId);

            if (!relatedExists)
            {
                throw new ArgumentException("La PQR relacionada no existe.");
            }
        }

        var radicadoNumber = await GenerateRadicadoNumberAsync(tenantId);
        var deadline = await CalculateDeadlineAsync(tenantId, pqrType);

        var pqr = new PqrRecord
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            RadicadoNumber = radicadoNumber,
            PQRType = pqrType,
            Category = category,
            Status = PQRStatus.Filed,
            Priority = PQRPriority.Medium,
            Subject = request.Subject,
            Description = request.Description,
            RadiadorName = request.RadiadorName,
            RadiadorDocumentType = request.RadiadorDocumentType,
            RadiadorDocumentNumber = request.RadiadorDocumentNumber,
            RadiadorContact = request.RadiadorContact,
            OwnerId = request.OwnerId,
            TenantResidentId = request.TenantResidentId,
            UnitId = request.UnitId,
            Channel = channel,
            RelatedPQRId = request.RelatedPQRId,
            Deadline = deadline,
            InvolvedResidentName = request.InvolvedResidentName,
            InvolvedResidentUnitId = request.InvolvedResidentUnitId,
            IsInternal = request.IsInternal,
            IsLinkedToCharge = request.IsLinkedToCharge,
            UnitFeeId = request.UnitFeeId,
            ExtraordinaryFeeDistributionId = request.ExtraordinaryFeeDistributionId,
            IndividualChargeId = request.IndividualChargeId,
            FiledAt = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow,
            CreatedByUserId = userId
        };

        var followUp = new PqrFollowUp
        {
            Id = Guid.NewGuid(),
            PQRId = pqr.Id,
            PreviousStatus = PQRStatus.Filed,
            NewStatus = PQRStatus.Filed,
            ChangedAt = DateTime.UtcNow,
            ChangedByUserId = userId,
            ChangedByUserName = "Sistema",
            Justification = "Radicación inicial de la PQR.",
            IsAutomatic = true
        };

        _context.PqrRecords.Add(pqr);
        _context.PqrFollowUps.Add(followUp);
        await _context.SaveChangesAsync();

        await SendRadicationNotificationAsync(tenantId, pqr);

        return new PqrCreatedResponseDto
        {
            Id = pqr.Id,
            RadicadoNumber = pqr.RadicadoNumber,
            PQRType = pqr.PQRType.ToString(),
            Status = pqr.Status.ToString(),
            Subject = pqr.Subject,
            FiledAt = pqr.FiledAt,
            Deadline = pqr.Deadline,
            ProgressPercent = 0m
        };
    }

    private async Task<string> GenerateRadicadoNumberAsync(string tenantId)
    {
        var now = DateTime.UtcNow;
        var year = now.Year;
        var month = now.Month.ToString("00");
        var prefix = $"PQR-{year}-{month}-";

        var lastPqr = await _context.PqrRecords
            .Where(p => p.TenantId == tenantId && p.RadicadoNumber.StartsWith(prefix))
            .OrderByDescending(p => p.RadicadoNumber)
            .FirstOrDefaultAsync();

        int nextSequence = 1;

        if (lastPqr != null)
        {
            var lastPart = lastPqr.RadicadoNumber.Split('-').Last();
            if (int.TryParse(lastPart, out var lastSequence))
            {
                nextSequence = lastSequence + 1;
            }
        }

        return $"{prefix}{nextSequence:D5}";
    }

    private async Task<DateTime> CalculateDeadlineAsync(string tenantId, PQRType pqrType)
    {
        var config = await _context.PqrTimeConfigs
            .FirstOrDefaultAsync(c => c.TenantId == tenantId && c.PQRType == pqrType);

        int businessDays;

        if (config != null)
        {
            businessDays = config.BusinessDays;
        }
        else
        {
            businessDays = pqrType switch
            {
                PQRType.Request => 5,
                PQRType.Complaint => 3,
                PQRType.Claim => 10,
                _ => 5
            };
        }

        return AddBusinessDays(DateTime.UtcNow, businessDays);
    }

    private static DateTime AddBusinessDays(DateTime date, int businessDays)
    {
        if (businessDays <= 0)
        {
            return date;
        }

        var current = date;
        var added = 0;

        while (added < businessDays)
        {
            current = current.AddDays(1);

            if (current.DayOfWeek != DayOfWeek.Saturday && current.DayOfWeek != DayOfWeek.Sunday)
            {
                added++;
            }
        }

        return current;
    }

    private async Task SendRadicationNotificationAsync(string tenantId, PqrRecord pqr)
    {
        if (!pqr.OwnerId.HasValue && !pqr.TenantResidentId.HasValue)
        {
            return;
        }

        var deadlineText = "Por definir";
        if (pqr.Deadline.HasValue)
        {
            deadlineText = pqr.Deadline.Value.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture);
        }

        var variables = new System.Collections.Generic.Dictionary<string, string>
        {
            ["ResidentName"] = pqr.RadiadorName,
            ["RadicadoNumber"] = pqr.RadicadoNumber,
            ["Deadline"] = deadlineText
        };

        await _notificationEngine.ProcessEventAsync(
            tenantId, NotificationEventType.PQRReceived,
            "PQR", pqr.Id.ToString(), "PqrRecord",
            ownerId: pqr.OwnerId, tenantResidentId: pqr.TenantResidentId, variables: variables);
    }
}
