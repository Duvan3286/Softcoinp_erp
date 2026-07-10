using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Softcoinp.ERP.Domain.Entities;
using Softcoinp.ERP.Domain.Enums;
using Softcoinp.ERP.Infrastructure.Persistence;

namespace Softcoinp.ERP.WebAPI.Services;

public class DelinquencySequenceEngine
{
    private readonly ApplicationDbContext _context;
    private readonly NotificationEngine _notificationEngine;

    public DelinquencySequenceEngine(
        ApplicationDbContext context,
        NotificationEngine notificationEngine)
    {
        _context = context;
        _notificationEngine = notificationEngine;
    }

    public async Task<List<string>> ProcessDailyAsync(string tenantId)
    {
        var results = new List<string>();
        var today = DateTime.UtcNow.Date;

        var configs = await _context.DelinquencySequenceConfigs
            .Where(c => c.TenantId == tenantId && c.IsActive)
            .Include(c => c.Template)
            .OrderBy(c => c.StepNumber)
            .ToListAsync();

        if (configs.Count == 0)
        {
            results.Add("No hay configuración de secuencia de mora activa");
            return results;
        }

        var activePauses = await _context.DelinquencySequencePauses
            .Where(p => p.TenantId == tenantId &&
                        p.StartDate <= today &&
                        (p.EndDate == null || p.EndDate >= today))
            .Select(p => p.UnitId)
            .ToListAsync();

        // Buscar unidades con cuotas vencidas usando la entidad UnitFee
        // (no incluye cuotas ya pagadas totalmente) y DueDate menor a hoy
        var overdueUnits = await _context.Set<UnitFee>()
            .Where(f => f.TenantId == tenantId &&
                        f.Status != FeeStatus.FullyPaid &&
                        f.BalanceAmount > 0 &&
                        f.DueDate < today)
            .Select(f => f.UnitId)
            .Distinct()
            .ToListAsync();

        foreach (var unitId in overdueUnits)
        {
            if (activePauses.Contains(unitId))
            {
                results.Add($"Unidad {unitId} tiene pausa activa, se omite");
                continue;
            }

            var unit = await _context.Units
                .FirstOrDefaultAsync(u => u.Id == unitId && u.TenantId == tenantId);

            if (unit == null) continue;

            // Obtener día más antiguo de vencimiento para esta unidad
            var oldestDueDate = await _context.Set<UnitFee>()
                .Where(f => f.UnitId == unitId &&
                            f.Status != FeeStatus.FullyPaid &&
                            f.BalanceAmount > 0)
                .MinAsync(f => (DateTime?)f.DueDate);

            if (oldestDueDate == null) continue;

            var daysOverdue = (today - oldestDueDate.Value).Days;

            // Buscar la última notificación enviada para esta unidad
            var lastNotification = await _context.AutomaticNotifications
                .Where(n => n.TenantId == tenantId &&
                            n.SourceEntityId == unitId.ToString() &&
                            n.EventType == NotificationEventType.DelinquencyNotice1 ||
                            n.EventType == NotificationEventType.DelinquencyNotice2 ||
                            n.EventType == NotificationEventType.DelinquencyNotice3)
                .OrderByDescending(n => n.CreatedAt)
                .FirstOrDefaultAsync();

            NotificationEventType? lastStepType = null;
            if (lastNotification != null)
            {
                lastStepType = lastNotification.EventType;
            }

            // Determinar qué paso corresponde según los días de mora
            foreach (var config in configs)
            {
                if (daysOverdue < config.DaysAfterDue)
                    break;

                var stepEventType = GetEventTypeForStep(config.StepNumber);

                if (stepEventType == lastStepType)
                    break;

                // Verificar si ya se envió este paso
                var alreadySent = await _context.AutomaticNotifications
                    .AnyAsync(n => n.TenantId == tenantId &&
                                   n.SourceEntityId == unitId.ToString() &&
                                   n.EventType == stepEventType);

                if (alreadySent) continue;

                // Buscar propietarios de la unidad
                var unitOwners = await _context.Set<UnitOwner>()
                    .Where(uo => uo.UnitId == unitId && uo.IsActive)
                    .ToListAsync();

                foreach (var unitOwner in unitOwners)
                {
                    var owner = await _context.Owners.FindAsync(unitOwner.OwnerId);
                    if (owner == null) continue;

                    var variables = new Dictionary<string, string>
                    {
                        ["Propietario"] = owner.FullNameOrCompanyName,
                        ["Unidad"] = unit.Identifier,
                        ["DiasMora"] = daysOverdue.ToString(),
                        ["FechaVencimiento"] = oldestDueDate.Value.ToString("dd/MM/yyyy"),
                        ["Paso"] = config.StepNumber.ToString()
                    };

                    await _notificationEngine.ProcessEventAsync(
                        tenantId,
                        stepEventType,
                        "Billing",
                        unitId.ToString(),
                        "Unit",
                        ownerId: unitOwner.OwnerId,
                        variables: variables);
                }

                results.Add($"Enviado paso {config.StepNumber} a unidad {unit.Identifier} ({daysOverdue} días de mora)");
                break;
            }
        }

        return results;
    }

    public async Task<List<DelinquencySequenceConfig>> GetConfigAsync(string tenantId)
    {
        return await _context.DelinquencySequenceConfigs
            .Where(c => c.TenantId == tenantId)
            .Include(c => c.Template)
            .OrderBy(c => c.StepNumber)
            .ToListAsync();
    }

    public async Task<DelinquencySequenceConfig> UpsertConfigAsync(
        string tenantId, int stepNumber, int daysAfterDue, Guid templateId, bool isActive)
    {
        var config = await _context.DelinquencySequenceConfigs
            .FirstOrDefaultAsync(c => c.TenantId == tenantId && c.StepNumber == stepNumber);

        if (config == null)
        {
            config = new DelinquencySequenceConfig
            {
                TenantId = tenantId,
                StepNumber = stepNumber,
                DaysAfterDue = daysAfterDue,
                TemplateId = templateId,
                IsActive = isActive
            };
            _context.DelinquencySequenceConfigs.Add(config);
        }
        else
        {
            config.DaysAfterDue = daysAfterDue;
            config.TemplateId = templateId;
            config.IsActive = isActive;
            config.UpdatedAt = DateTime.UtcNow;
        }

        await _context.SaveChangesAsync();
        return config;
    }

    public async Task<bool> PauseForUnitAsync(
        string tenantId, Guid unitId, DateTime startDate, DateTime? endDate, string reason, string userId)
    {
        var existing = await _context.DelinquencySequencePauses
            .FirstOrDefaultAsync(p =>
                p.TenantId == tenantId &&
                p.UnitId == unitId &&
                p.StartDate <= DateTime.UtcNow &&
                (p.EndDate == null || p.EndDate >= DateTime.UtcNow));

        if (existing != null)
        {
            existing.EndDate = endDate;
            existing.Reason = reason;
        }
        else
        {
            var pause = new DelinquencySequencePause
            {
                TenantId = tenantId,
                UnitId = unitId,
                StartDate = startDate,
                EndDate = endDate,
                Reason = reason,
                CreatedByUserId = userId
            };
            _context.DelinquencySequencePauses.Add(pause);
        }

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> RemovePauseAsync(Guid pauseId, string tenantId)
    {
        var pause = await _context.DelinquencySequencePauses
            .FirstOrDefaultAsync(p => p.Id == pauseId && p.TenantId == tenantId);

        if (pause == null) return false;

        _context.DelinquencySequencePauses.Remove(pause);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<List<DelinquencySequencePause>> GetActivePausesAsync(string tenantId)
    {
        var now = DateTime.UtcNow;
        return await _context.DelinquencySequencePauses
            .Where(p => p.TenantId == tenantId &&
                        p.StartDate <= now &&
                        (p.EndDate == null || p.EndDate >= now))
            .Include(p => p.Unit)
            .OrderBy(p => p.StartDate)
            .ToListAsync();
    }

    private static NotificationEventType GetEventTypeForStep(int stepNumber)
    {
        return stepNumber switch
        {
            1 => NotificationEventType.DelinquencyNotice1,
            2 => NotificationEventType.DelinquencyNotice2,
            3 => NotificationEventType.DelinquencyNotice3,
            4 => NotificationEventType.PreLegalNotice,
            _ => NotificationEventType.DelinquencyNotice1
        };
    }
}
