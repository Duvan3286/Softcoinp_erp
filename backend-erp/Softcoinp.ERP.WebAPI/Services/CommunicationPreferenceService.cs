using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Softcoinp.ERP.Domain.Entities;
using Softcoinp.ERP.Infrastructure.Persistence;
using Softcoinp.ERP.WebAPI.DTOs;

namespace Softcoinp.ERP.WebAPI.Services;

public class CommunicationPreferenceService
{
    private readonly ApplicationDbContext _context;

    public CommunicationPreferenceService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<CommunicationPreferenceDto?> GetByOwnerAsync(Guid ownerId, string tenantId)
    {
        var pref = await _context.CommunicationPreferences
            .Include(p => p.Owner)
            .FirstOrDefaultAsync(p => p.OwnerId == ownerId && p.TenantId == tenantId);

        if (pref == null) return null;

        return MapToDto(pref);
    }

    public async Task<CommunicationPreferenceDto?> GetByTenantResidentAsync(Guid tenantResidentId, string tenantId)
    {
        var pref = await _context.CommunicationPreferences
            .Include(p => p.TenantResident)
            .FirstOrDefaultAsync(p => p.TenantResidentId == tenantResidentId && p.TenantId == tenantId);

        if (pref == null) return null;

        return MapToDto(pref);
    }

    public async Task<List<CommunicationPreferenceDto>> GetAllAsync(string tenantId)
    {
        var prefs = await _context.CommunicationPreferences
            .Include(p => p.Owner)
            .Include(p => p.TenantResident)
            .Where(p => p.TenantId == tenantId)
            .OrderBy(p => p.ChangedAt)
            .ToListAsync();

        return prefs.Select(p => new CommunicationPreferenceDto
        {
            Id = p.Id,
            OwnerId = p.OwnerId,
            OwnerName = p.Owner?.FullNameOrCompanyName,
            TenantResidentId = p.TenantResidentId,
            TenantResidentName = p.TenantResident?.FullName,
            AllowEmail = p.AllowEmail,
            AllowSms = p.AllowSms,
            AllowPush = p.AllowPush,
            CriticalNotificationsOverride = p.CriticalNotificationsOverride,
            UnsubscribedEventTypes = string.IsNullOrEmpty(p.UnsubscribedEventTypes)
                ? new List<string>()
                : JsonSerializer.Deserialize<List<string>>(p.UnsubscribedEventTypes) ?? new List<string>(),
            Notes = p.Notes,
            ChangedAt = p.ChangedAt
        }).ToList();
    }

    public async Task<CommunicationPreferenceDto> CreateOrUpdateAsync(
        UpdateCommunicationPreferenceRequest request, Guid? ownerId, Guid? tenantResidentId, string tenantId, string userId)
    {
        var pref = await _context.CommunicationPreferences
            .FirstOrDefaultAsync(p =>
                p.TenantId == tenantId &&
                ((ownerId.HasValue && p.OwnerId == ownerId.Value) ||
                 (tenantResidentId.HasValue && p.TenantResidentId == tenantResidentId.Value)));

        if (pref == null)
        {
            pref = new CommunicationPreference
            {
                TenantId = tenantId,
                OwnerId = ownerId,
                TenantResidentId = tenantResidentId,
                ChangedByUserId = userId
            };
            _context.CommunicationPreferences.Add(pref);
        }

        if (request.AllowEmail.HasValue) pref.AllowEmail = request.AllowEmail.Value;
        if (request.AllowSms.HasValue) pref.AllowSms = request.AllowSms.Value;
        if (request.AllowPush.HasValue) pref.AllowPush = request.AllowPush.Value;
        if (request.CriticalNotificationsOverride.HasValue)
            pref.CriticalNotificationsOverride = request.CriticalNotificationsOverride.Value;
        if (request.UnsubscribedEventTypes != null)
            pref.UnsubscribedEventTypes = JsonSerializer.Serialize(request.UnsubscribedEventTypes);
        if (request.Notes != null) pref.Notes = request.Notes;

        pref.ChangedAt = DateTime.UtcNow;
        pref.ChangedByUserId = userId;
        await _context.SaveChangesAsync();

        return await GetByOwnerAsync(pref.OwnerId.GetValueOrDefault(), tenantId)
            ?? await GetByTenantResidentAsync(pref.TenantResidentId.GetValueOrDefault(), tenantId)
            ?? MapToDto(pref);
    }

    private static CommunicationPreferenceDto MapToDto(CommunicationPreference pref)
    {
        return new CommunicationPreferenceDto
        {
            Id = pref.Id,
            OwnerId = pref.OwnerId,
            OwnerName = pref.Owner?.FullNameOrCompanyName,
            TenantResidentId = pref.TenantResidentId,
            TenantResidentName = pref.TenantResident?.FullName,
            AllowEmail = pref.AllowEmail,
            AllowSms = pref.AllowSms,
            AllowPush = pref.AllowPush,
            CriticalNotificationsOverride = pref.CriticalNotificationsOverride,
            UnsubscribedEventTypes = string.IsNullOrEmpty(pref.UnsubscribedEventTypes)
                ? new List<string>()
                : JsonSerializer.Deserialize<List<string>>(pref.UnsubscribedEventTypes) ?? new List<string>(),
            Notes = pref.Notes,
            ChangedAt = pref.ChangedAt
        };
    }
}
