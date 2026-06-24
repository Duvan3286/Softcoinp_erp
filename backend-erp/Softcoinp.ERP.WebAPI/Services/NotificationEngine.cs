using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Softcoinp.ERP.Domain.Entities;
using Softcoinp.ERP.Domain.Enums;
using Softcoinp.ERP.Infrastructure.Persistence;

namespace Softcoinp.ERP.WebAPI.Services;

public class NotificationEngine
{
    private readonly ApplicationDbContext _context;
    private readonly DeliveryTrackerEngine _deliveryTracker;

    public NotificationEngine(
        ApplicationDbContext context,
        DeliveryTrackerEngine deliveryTracker)
    {
        _context = context;
        _deliveryTracker = deliveryTracker;
    }

    public async Task<AutomaticNotification?> ProcessEventAsync(
        string tenantId,
        NotificationEventType eventType,
        string sourceModule,
        string sourceEntityId,
        string sourceEntityType,
        Guid? ownerId = null,
        Guid? tenantResidentId = null,
        Dictionary<string, string>? variables = null)
    {
        var template = await _context.NotificationTemplates
            .FirstOrDefaultAsync(t =>
                t.TenantId == tenantId &&
                t.EventType == eventType &&
                t.IsActive);

        if (template == null)
        {
            return null;
        }

        var recipients = new List<(Guid? OwnerId, Guid? TenantResidentId, string Email, string Phone)>();

        if (ownerId.HasValue)
        {
            var owner = await _context.Owners.FindAsync(ownerId.Value);
            if (owner != null)
            {
                recipients.Add((ownerId, null, owner.Email, owner.MainPhone));
            }
        }

        if (tenantResidentId.HasValue)
        {
            var tenantResident = await _context.TenantResidents.FindAsync(tenantResidentId.Value);
            if (tenantResident != null)
            {
                recipients.Add((null, tenantResidentId, tenantResident.Email, tenantResident.Phone));
            }
        }

        if (!ownerId.HasValue && !tenantResidentId.HasValue)
        {
            if (template.ForRecipientType == RecipientType.Owner || template.ForRecipientType == RecipientType.Both)
            {
                var owners = await _context.Owners
                    .Where(o => o.TenantId == tenantId && o.IsActive)
                    .ToListAsync();

                foreach (var owner in owners)
                {
                    recipients.Add((owner.Id, null, owner.Email, owner.MainPhone));
                }
            }

            if (template.ForRecipientType == RecipientType.Tenant || template.ForRecipientType == RecipientType.Both)
            {
                var tenants = await _context.TenantResidents
                    .Where(t => t.TenantId == tenantId && t.IsActive)
                    .ToListAsync();

                foreach (var tenant in tenants)
                {
                    recipients.Add((null, tenant.Id, tenant.Email, tenant.Phone));
                }
            }
        }

        var emailBody = ReplaceVariables(template.EmailBody, variables);
        var smsBody = ReplaceVariables(template.SmsBody, variables);

        AutomaticNotification? lastNotification = null;

        foreach (var (recOwnerId, recTenantId, email, phone) in recipients)
        {
            var pref = await _context.CommunicationPreferences
                .FirstOrDefaultAsync(p =>
                    p.TenantId == tenantId &&
                    ((recOwnerId.HasValue && p.OwnerId == recOwnerId.Value) ||
                     (recTenantId.HasValue && p.TenantResidentId == recTenantId.Value)));

            var channels = GetApplicableChannels(eventType, pref);
            var unsubscribedTypes = GetUnsubscribedTypes(pref);

            if (unsubscribedTypes.Contains(eventType.ToString()))
            {
                continue;
            }

            foreach (var channel in channels)
            {
                var notification = new AutomaticNotification
                {
                    TenantId = tenantId,
                    EventType = eventType,
                    OwnerId = recOwnerId,
                    TenantResidentId = recTenantId,
                    RecipientEmail = channel == CommunicationChannel.Email ? email : string.Empty,
                    RecipientPhone = channel == CommunicationChannel.Sms ? phone : string.Empty,
                    Channel = channel,
                    Status = DeliveryStatus.Pending,
                    SourceModule = sourceModule,
                    SourceEntityId = sourceEntityId,
                    SourceEntityType = sourceEntityType,
                };

                _context.AutomaticNotifications.Add(notification);
                await _context.SaveChangesAsync();

                await _deliveryTracker.ProcessAutomaticNotificationDeliveryAsync(notification.Id);

                lastNotification = notification;
            }
        }

        return lastNotification;
    }

    private static string ReplaceVariables(string text, Dictionary<string, string>? variables)
    {
        if (string.IsNullOrEmpty(text) || variables == null)
            return text;

        foreach (var kv in variables)
        {
            text = text.Replace("{" + kv.Key + "}", kv.Value);
        }

        return text;
    }

    private static List<CommunicationChannel> GetApplicableChannels(
        NotificationEventType eventType, CommunicationPreference? pref)
    {
        var isCritical = IsCriticalEvent(eventType);

        var channels = new List<CommunicationChannel>();

        if (isCritical)
        {
            channels.Add(CommunicationChannel.Email);
            channels.Add(CommunicationChannel.Sms);
            channels.Add(CommunicationChannel.Push);
            return channels;
        }

        if (pref == null || pref.AllowEmail)
            channels.Add(CommunicationChannel.Email);

        if (pref == null || pref.AllowSms)
            channels.Add(CommunicationChannel.Sms);

        if (pref == null || pref.AllowPush)
            channels.Add(CommunicationChannel.Push);

        if (channels.Count == 0)
            channels.Add(CommunicationChannel.Email);

        return channels;
    }

    private static bool IsCriticalEvent(NotificationEventType eventType)
    {
        return eventType switch
        {
            NotificationEventType.MaintenanceScheduled => true,
            NotificationEventType.OutOfService => true,
            NotificationEventType.PreLegalNotice => true,
            NotificationEventType.AssemblyConvocation => true,
            NotificationEventType.AssemblyReminder72h => true,
            _ => false
        };
    }

    private static List<string> GetUnsubscribedTypes(CommunicationPreference? pref)
    {
        if (pref == null || string.IsNullOrEmpty(pref.UnsubscribedEventTypes))
            return new List<string>();

        try
        {
            return JsonSerializer.Deserialize<List<string>>(pref.UnsubscribedEventTypes)
                ?? new List<string>();
        }
        catch
        {
            return new List<string>();
        }
    }
}
