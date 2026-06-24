using System;
using System.Collections.Generic;

namespace Softcoinp.ERP.WebAPI.DTOs;

// ── Communication (Comunicado) ────────────────────────────────────

public class CommunicationSummaryDto
{
    public Guid Id { get; set; }
    public string Subject { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string AudienceType { get; set; } = string.Empty;
    public bool RequiresReadConfirmation { get; set; }
    public bool PublishToBulletinBoard { get; set; }
    public DateTime? SendAt { get; set; }
    public DateTime? SentAt { get; set; }
    public int RecipientCount { get; set; }
    public int ReadConfirmedCount { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class CommunicationDetailDto
{
    public Guid Id { get; set; }
    public string Subject { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string AudienceType { get; set; } = string.Empty;
    public List<string> SelectedChannels { get; set; } = new();
    public DateTime? SendAt { get; set; }
    public DateTime? SentAt { get; set; }
    public bool RequiresReadConfirmation { get; set; }
    public bool PublishToBulletinBoard { get; set; }
    public Guid? RelatedCommunicationId { get; set; }
    public List<string> FilePaths { get; set; } = new();
    public string CreatedByUserId { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public List<CommunicationRecipientDto> Recipients { get; set; } = new();
}

public class CommunicationRecipientDto
{
    public Guid Id { get; set; }
    public Guid? OwnerId { get; set; }
    public string? OwnerName { get; set; }
    public Guid? TenantResidentId { get; set; }
    public string? TenantResidentName { get; set; }
    public string RecipientEmail { get; set; } = string.Empty;
    public string RecipientPhone { get; set; } = string.Empty;
    public string EmailStatus { get; set; } = string.Empty;
    public string SmsStatus { get; set; } = string.Empty;
    public string PushStatus { get; set; } = string.Empty;
    public string BulletinBoardStatus { get; set; } = string.Empty;
    public DateTime? ReadConfirmedAt { get; set; }
    public int ResentCount { get; set; }
    public string? ErrorMessage { get; set; }
}

public class CreateCommunicationRequest
{
    public string Subject { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;
    public string AudienceType { get; set; } = string.Empty;
    public List<Guid>? SpecificUnitIds { get; set; }
    public List<string>? SpecificTowers { get; set; }
    public List<string> SelectedChannels { get; set; } = new();
    public DateTime? SendAt { get; set; }
    public bool RequiresReadConfirmation { get; set; }
    public bool PublishToBulletinBoard { get; set; }
    public List<string>? FilePaths { get; set; }
}

public class UpdateCommunicationRequest
{
    public string? Subject { get; set; }
    public string? Body { get; set; }
    public string? AudienceType { get; set; }
    public List<Guid>? SpecificUnitIds { get; set; }
    public List<string>? SpecificTowers { get; set; }
    public List<string>? SelectedChannels { get; set; }
    public DateTime? SendAt { get; set; }
    public bool? RequiresReadConfirmation { get; set; }
    public bool? PublishToBulletinBoard { get; set; }
    public List<string>? FilePaths { get; set; }
}

// ── Notification Template (Plantilla) ─────────────────────────────

public class NotificationTemplateDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string EventType { get; set; } = string.Empty;
    public string ForRecipientType { get; set; } = string.Empty;
    public string EmailSubject { get; set; } = string.Empty;
    public string EmailBody { get; set; } = string.Empty;
    public string SmsBody { get; set; } = string.Empty;
    public List<string> DynamicVariables { get; set; } = new();
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class CreateNotificationTemplateRequest
{
    public string Name { get; set; } = string.Empty;
    public string EventType { get; set; } = string.Empty;
    public string ForRecipientType { get; set; } = string.Empty;
    public string EmailSubject { get; set; } = string.Empty;
    public string EmailBody { get; set; } = string.Empty;
    public string SmsBody { get; set; } = string.Empty;
    public List<string>? DynamicVariables { get; set; }
}

public class UpdateNotificationTemplateRequest
{
    public string? Name { get; set; }
    public string? EmailSubject { get; set; }
    public string? EmailBody { get; set; }
    public string? SmsBody { get; set; }
    public List<string>? DynamicVariables { get; set; }
    public bool? IsActive { get; set; }
}

// ── Automatic Notification ────────────────────────────────────────

public class AutomaticNotificationDto
{
    public Guid Id { get; set; }
    public string EventType { get; set; } = string.Empty;
    public string RecipientName { get; set; } = string.Empty;
    public string RecipientEmail { get; set; } = string.Empty;
    public string RecipientPhone { get; set; } = string.Empty;
    public string Channel { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime? SentAt { get; set; }
    public DateTime? ReadAt { get; set; }
    public string SourceModule { get; set; } = string.Empty;
    public string? ErrorMessage { get; set; }
    public DateTime CreatedAt { get; set; }
}

// ── Bulletin Board ────────────────────────────────────────────────

public class BulletinBoardPostDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public DateTime PublishedAt { get; set; }
    public DateTime? ExpiresAt { get; set; }
    public bool IsPinned { get; set; }
    public string Category { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}

public class BulletinBoardPostAdminDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public DateTime PublishedAt { get; set; }
    public DateTime? ExpiresAt { get; set; }
    public bool IsPinned { get; set; }
    public string Category { get; set; } = string.Empty;
    public bool IsDeleted { get; set; }
    public string CreatedByUserId { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

public class CreateBulletinBoardPostRequest
{
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public DateTime? PublishedAt { get; set; }
    public DateTime? ExpiresAt { get; set; }
    public bool IsPinned { get; set; }
    public string Category { get; set; } = string.Empty;
}

public class UpdateBulletinBoardPostRequest
{
    public string? Title { get; set; }
    public string? Content { get; set; }
    public DateTime? PublishedAt { get; set; }
    public DateTime? ExpiresAt { get; set; }
    public bool? IsPinned { get; set; }
    public string? Category { get; set; }
}

// ── Communication Preferences ─────────────────────────────────────

public class CommunicationPreferenceDto
{
    public Guid Id { get; set; }
    public Guid? OwnerId { get; set; }
    public string? OwnerName { get; set; }
    public Guid? TenantResidentId { get; set; }
    public string? TenantResidentName { get; set; }
    public bool AllowEmail { get; set; }
    public bool AllowSms { get; set; }
    public bool AllowPush { get; set; }
    public bool CriticalNotificationsOverride { get; set; }
    public List<string> UnsubscribedEventTypes { get; set; } = new();
    public string? Notes { get; set; }
    public DateTime ChangedAt { get; set; }
}

public class UpdateCommunicationPreferenceRequest
{
    public bool? AllowEmail { get; set; }
    public bool? AllowSms { get; set; }
    public bool? AllowPush { get; set; }
    public bool? CriticalNotificationsOverride { get; set; }
    public List<string>? UnsubscribedEventTypes { get; set; }
    public string? Notes { get; set; }
}

// ── Delinquency Sequence ──────────────────────────────────────────

public class DelinquencySequenceConfigDto
{
    public Guid Id { get; set; }
    public int StepNumber { get; set; }
    public int DaysAfterDue { get; set; }
    public Guid TemplateId { get; set; }
    public string TemplateName { get; set; } = string.Empty;
    public bool IsActive { get; set; }
}

public class UpdateDelinquencySequenceConfigRequest
{
    public int DaysAfterDue { get; set; }
    public Guid TemplateId { get; set; }
    public bool IsActive { get; set; }
}

public class DelinquencySequencePauseDto
{
    public Guid Id { get; set; }
    public Guid UnitId { get; set; }
    public string UnitIdentifier { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public string Reason { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public string CreatedByUserId { get; set; } = string.Empty;
}

public class CreateDelinquencySequencePauseRequest
{
    public Guid UnitId { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public string Reason { get; set; } = string.Empty;
}

// ── Dashboard / Reports ───────────────────────────────────────────

public class CommunicationEffectivenessReportDto
{
    public int TotalCommunications { get; set; }
    public int TotalRecipients { get; set; }
    public int EmailDelivered { get; set; }
    public int EmailOpened { get; set; }
    public int EmailBounced { get; set; }
    public int SmsDelivered { get; set; }
    public int SmsFailed { get; set; }
    public int PushDelivered { get; set; }
    public int ReadConfirmations { get; set; }
    public double DeliveryRate { get; set; }
    public double OpenRate { get; set; }
    public double ReadConfirmationRate { get; set; }
}
