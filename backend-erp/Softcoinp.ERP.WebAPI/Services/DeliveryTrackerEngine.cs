using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Softcoinp.ERP.Domain.Entities;
using Softcoinp.ERP.Domain.Enums;
using Softcoinp.ERP.Infrastructure.Persistence;

namespace Softcoinp.ERP.WebAPI.Services;

public class DeliveryTrackerEngine
{
    private readonly ApplicationDbContext _context;

    public DeliveryTrackerEngine(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task ProcessCommunicationDeliveryAsync(Guid communicationId)
    {
        var communication = await _context.Communications
            .Include(c => c.Recipients)
            .FirstOrDefaultAsync(c => c.Id == communicationId);

        if (communication == null) return;

        var channels = DeserializeChannels(communication.SelectedChannels);

        foreach (var recipient in communication.Recipients)
        {
            foreach (var channel in channels)
            {
                DeliverToChannel(recipient, channel);
            }
        }

        communication.Status = CommunicationStatus.Sent;
        communication.SentAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
    }

    public async Task ProcessSingleRecipientAsync(Guid recipientId, CommunicationChannel channel)
    {
        var recipient = await _context.CommunicationRecipients
            .Include(r => r.Communication)
            .FirstOrDefaultAsync(r => r.Id == recipientId);

        if (recipient == null) return;

        DeliverToChannel(recipient, channel);
        await _context.SaveChangesAsync();
    }

    public async Task ProcessAutomaticNotificationDeliveryAsync(Guid notificationId)
    {
        var notification = await _context.AutomaticNotifications
            .FirstOrDefaultAsync(n => n.Id == notificationId);

        if (notification == null) return;

        var success = TryDeliver(notification.Channel, notification.RecipientEmail, notification.RecipientPhone);

        notification.Status = success ? DeliveryStatus.Delivered : DeliveryStatus.Failed;
        notification.SentAt = DateTime.UtcNow;
        if (!success) notification.ErrorMessage = "Fallo en el envío automático";

        await _context.SaveChangesAsync();
    }

    public async Task ResendToUnconfirmedAsync(Guid communicationId)
    {
        var unconfirmed = await _context.CommunicationRecipients
            .Where(r => r.CommunicationId == communicationId && r.ReadConfirmedAt == null)
            .ToListAsync();

        foreach (var recipient in unconfirmed)
        {
            recipient.ResentCount++;
            recipient.LastResentAt = DateTime.UtcNow;

            if (!string.IsNullOrEmpty(recipient.RecipientEmail))
            {
                recipient.EmailStatus = DeliveryStatus.Sent;
            }

            if (!string.IsNullOrEmpty(recipient.RecipientPhone))
            {
                recipient.SmsStatus = DeliveryStatus.Sent;
            }
        }

        await _context.SaveChangesAsync();
    }

    private void DeliverToChannel(CommunicationRecipient recipient, CommunicationChannel channel)
    {
        switch (channel)
        {
            case CommunicationChannel.Email:
                if (!string.IsNullOrEmpty(recipient.RecipientEmail))
                {
                    var success = TryDeliver(channel, recipient.RecipientEmail, null);
                    recipient.EmailStatus = success ? DeliveryStatus.Delivered : DeliveryStatus.Failed;
                    recipient.EmailSentAt = DateTime.UtcNow;
                    if (!success) recipient.ErrorMessage = "Fallo en envío de correo";
                }
                break;

            case CommunicationChannel.Sms:
                if (!string.IsNullOrEmpty(recipient.RecipientPhone))
                {
                    var success = TryDeliver(channel, null, recipient.RecipientPhone);
                    recipient.SmsStatus = success ? DeliveryStatus.Delivered : DeliveryStatus.Failed;
                    recipient.SmsSentAt = DateTime.UtcNow;
                    if (!success) recipient.ErrorMessage = "Fallo en envío de SMS";
                }
                break;

            case CommunicationChannel.Push:
                recipient.PushStatus = DeliveryStatus.Delivered;
                recipient.PushSentAt = DateTime.UtcNow;
                break;

            case CommunicationChannel.BulletinBoard:
                recipient.BulletinBoardStatus = DeliveryStatus.Sent;
                break;
        }
    }

    private static bool TryDeliver(CommunicationChannel channel, string? email, string? phone)
    {
        try
        {
            // En producción, aquí se integraría con:
            // - SMTP (SendGrid, Mailgun, etc.) para Email
            // - Proveedor SMS (Twilio, MessageBird, etc.) para SMS
            // - Firebase Cloud Messaging / SignalR para Push
            //
            // Simulación: el envío siempre es exitoso a menos que
            // el email o teléfono estén vacíos.

            if (channel == CommunicationChannel.Email && string.IsNullOrEmpty(email))
                return false;

            if (channel == CommunicationChannel.Sms && string.IsNullOrEmpty(phone))
                return false;

            return true;
        }
        catch
        {
            return false;
        }
    }

    private static List<CommunicationChannel> DeserializeChannels(string channelsJson)
    {
        if (string.IsNullOrEmpty(channelsJson))
            return new List<CommunicationChannel>();

        var items = channelsJson.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        var result = new List<CommunicationChannel>();

        foreach (var item in items)
        {
            if (Enum.TryParse<CommunicationChannel>(item, true, out var channel))
            {
                result.Add(channel);
            }
        }

        return result;
    }
}
