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

public class NotificationTemplateService
{
    private readonly ApplicationDbContext _context;

    public NotificationTemplateService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<NotificationTemplateDto>> GetAllAsync(string tenantId, string? eventType = null)
    {
        var query = _context.NotificationTemplates.Where(t => t.TenantId == tenantId);

        if (!string.IsNullOrEmpty(eventType))
            query = query.Where(t => t.EventType.ToString() == eventType);

        var templates = await query
            .OrderBy(t => t.EventType.ToString())
            .ThenBy(t => t.Name)
            .ToListAsync();

        return templates.Select(t => new NotificationTemplateDto
        {
            Id = t.Id,
            Name = t.Name,
            EventType = t.EventType.ToString(),
            ForRecipientType = t.ForRecipientType.ToString(),
            EmailSubject = t.EmailSubject,
            EmailBody = t.EmailBody,
            SmsBody = t.SmsBody,
            DynamicVariables = string.IsNullOrEmpty(t.DynamicVariables)
                ? new List<string>()
                : JsonSerializer.Deserialize<List<string>>(t.DynamicVariables) ?? new List<string>(),
            IsActive = t.IsActive,
            CreatedAt = t.CreatedAt
        }).ToList();
    }

    public async Task<NotificationTemplateDto?> GetByIdAsync(Guid id, string tenantId)
    {
        var template = await _context.NotificationTemplates
            .FirstOrDefaultAsync(t => t.Id == id && t.TenantId == tenantId);

        if (template == null) return null;

        return new NotificationTemplateDto
        {
            Id = template.Id,
            Name = template.Name,
            EventType = template.EventType.ToString(),
            ForRecipientType = template.ForRecipientType.ToString(),
            EmailSubject = template.EmailSubject,
            EmailBody = template.EmailBody,
            SmsBody = template.SmsBody,
            DynamicVariables = string.IsNullOrEmpty(template.DynamicVariables)
                ? new List<string>()
                : JsonSerializer.Deserialize<List<string>>(template.DynamicVariables) ?? new List<string>(),
            IsActive = template.IsActive,
            CreatedAt = template.CreatedAt
        };
    }

    public async Task<NotificationTemplateDto> CreateAsync(CreateNotificationTemplateRequest request, string tenantId, string userId)
    {
        var eventType = (NotificationEventType)Enum.Parse(typeof(NotificationEventType), request.EventType);
        var forRecipientType = (RecipientType)Enum.Parse(typeof(RecipientType), request.ForRecipientType);

        var template = new NotificationTemplate
        {
            TenantId = tenantId,
            Name = request.Name,
            EventType = eventType,
            ForRecipientType = forRecipientType,
            EmailSubject = request.EmailSubject,
            EmailBody = request.EmailBody,
            SmsBody = request.SmsBody,
            DynamicVariables = request.DynamicVariables != null
                ? JsonSerializer.Serialize(request.DynamicVariables)
                : string.Empty,
            IsActive = true,
            CreatedByUserId = userId
        };

        _context.NotificationTemplates.Add(template);
        await _context.SaveChangesAsync();

        return new NotificationTemplateDto
        {
            Id = template.Id,
            Name = template.Name,
            EventType = template.EventType.ToString(),
            ForRecipientType = template.ForRecipientType.ToString(),
            EmailSubject = template.EmailSubject,
            EmailBody = template.EmailBody,
            SmsBody = template.SmsBody,
            DynamicVariables = request.DynamicVariables ?? new List<string>(),
            IsActive = template.IsActive,
            CreatedAt = template.CreatedAt
        };
    }

    public async Task<NotificationTemplateDto?> UpdateAsync(Guid id, UpdateNotificationTemplateRequest request, string tenantId)
    {
        var template = await _context.NotificationTemplates
            .FirstOrDefaultAsync(t => t.Id == id && t.TenantId == tenantId);

        if (template == null) return null;

        if (request.Name != null) template.Name = request.Name;
        if (request.EmailSubject != null) template.EmailSubject = request.EmailSubject;
        if (request.EmailBody != null) template.EmailBody = request.EmailBody;
        if (request.SmsBody != null) template.SmsBody = request.SmsBody;
        if (request.DynamicVariables != null)
            template.DynamicVariables = JsonSerializer.Serialize(request.DynamicVariables);
        if (request.IsActive.HasValue) template.IsActive = request.IsActive.Value;

        template.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return await GetByIdAsync(id, tenantId);
    }

    public async Task<bool> DeleteAsync(Guid id, string tenantId)
    {
        var template = await _context.NotificationTemplates
            .FirstOrDefaultAsync(t => t.Id == id && t.TenantId == tenantId);

        if (template == null) return false;

        _context.NotificationTemplates.Remove(template);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<string?> PreviewAsync(string eventType, Dictionary<string, string> variables)
    {
        var type = (NotificationEventType)Enum.Parse(typeof(NotificationEventType), eventType);
        var template = await _context.NotificationTemplates
            .FirstOrDefaultAsync(t => t.EventType == type && t.IsActive);

        if (template == null) return null;

        var result = template.EmailBody;
        foreach (var kv in variables)
        {
            result = result.Replace("{" + kv.Key + "}", kv.Value);
        }

        return result;
    }
}
