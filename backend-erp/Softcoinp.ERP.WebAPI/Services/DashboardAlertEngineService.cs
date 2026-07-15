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

/// <summary>
/// Evalúa en tiempo real las reglas de alerta operativa configurables (ConfiguracionAlerta)
/// y genera las alertas activas del conjunto. Ninguna regla consulta datos de Contabilidad.
/// </summary>
public class DashboardAlertEngineService
{
    private readonly ApplicationDbContext _context;
    private readonly ExecutionEngineService _executionEngineService;

    public DashboardAlertEngineService(ApplicationDbContext context, ExecutionEngineService executionEngineService)
    {
        _context = context;
        _executionEngineService = executionEngineService;
    }

    private static readonly Dictionary<AlertRuleType, AlertConfiguration> DefaultConfigurations = new()
    {
        [AlertRuleType.ProviderContractExpiring] = new AlertConfiguration
        {
            RuleType = AlertRuleType.ProviderContractExpiring,
            IsEnabled = true,
            ThresholdDays = 30,
            DefaultUrgency = AlertUrgency.High
        },
        [AlertRuleType.PreventiveMaintenanceDue] = new AlertConfiguration
        {
            RuleType = AlertRuleType.PreventiveMaintenanceDue,
            IsEnabled = true,
            ThresholdDays = 7,
            DefaultUrgency = AlertUrgency.Medium
        },
        [AlertRuleType.PqrOverdue] = new AlertConfiguration
        {
            RuleType = AlertRuleType.PqrOverdue,
            IsEnabled = true,
            DefaultUrgency = AlertUrgency.High
        },
        [AlertRuleType.BudgetItemExecutionExceeded] = new AlertConfiguration
        {
            RuleType = AlertRuleType.BudgetItemExecutionExceeded,
            IsEnabled = true,
            ThresholdPercentage = 90,
            DefaultUrgency = AlertUrgency.High
        },
        [AlertRuleType.AssetOutOfService] = new AlertConfiguration
        {
            RuleType = AlertRuleType.AssetOutOfService,
            IsEnabled = true,
            DefaultUrgency = AlertUrgency.Medium
        },
        [AlertRuleType.WorkOrderUnassigned] = new AlertConfiguration
        {
            RuleType = AlertRuleType.WorkOrderUnassigned,
            IsEnabled = true,
            ThresholdDays = 3,
            DefaultUrgency = AlertUrgency.High
        },
        [AlertRuleType.PaymentAgreementOverdue] = new AlertConfiguration
        {
            RuleType = AlertRuleType.PaymentAgreementOverdue,
            IsEnabled = false,
            DefaultUrgency = AlertUrgency.Medium
        },
        [AlertRuleType.ReservationNotCheckedIn] = new AlertConfiguration
        {
            RuleType = AlertRuleType.ReservationNotCheckedIn,
            IsEnabled = true,
            DefaultUrgency = AlertUrgency.Medium
        }
    };

    public async Task<List<AlertDto>> EvaluateActiveAlertsAsync(string tenantId)
    {
        var configurations = await LoadEffectiveConfigurationsAsync(tenantId);
        var now = DateTime.UtcNow;
        var alerts = new List<AlertDto>();

        await EvaluateProviderContractExpiringAsync(tenantId, now, configurations, alerts);
        await EvaluatePreventiveMaintenanceDueAsync(tenantId, now, configurations, alerts);
        await EvaluatePqrOverdueAsync(tenantId, now, configurations, alerts);
        await EvaluateBudgetItemExecutionExceededAsync(tenantId, configurations, alerts);
        await EvaluateAssetOutOfServiceAsync(tenantId, configurations, alerts);
        await EvaluateWorkOrderUnassignedAsync(tenantId, now, configurations, alerts);
        await EvaluateReservationNotCheckedInAsync(tenantId, now, configurations, alerts);

        // AlertRuleType.PaymentAgreementOverdue no tiene fuente de datos: el módulo de
        // Cuotas y Cartera eliminó la entidad de acuerdos de pago. La regla queda
        // definida y configurable, pero deshabilitada por defecto (ver entrega 8).

        return alerts.OrderByDescending(a => a.Urgency).ThenBy(a => a.CreatedAt).ToList();
    }

    private async Task<Dictionary<AlertRuleType, AlertConfiguration>> LoadEffectiveConfigurationsAsync(string tenantId)
    {
        var stored = await _context.AlertConfigurations
            .Where(ac => ac.TenantId == tenantId)
            .ToListAsync();

        var storedByType = stored.ToDictionary(c => c.RuleType);
        var effective = new Dictionary<AlertRuleType, AlertConfiguration>();

        foreach (var pair in DefaultConfigurations)
        {
            if (storedByType.TryGetValue(pair.Key, out var storedConfig))
            {
                effective[pair.Key] = storedConfig;
            }
            else
            {
                effective[pair.Key] = pair.Value;
            }
        }

        return effective;
    }

    private static bool IsRuleEnabled(Dictionary<AlertRuleType, AlertConfiguration> configurations, AlertRuleType ruleType)
    {
        if (!configurations.TryGetValue(ruleType, out var config))
        {
            return false;
        }

        return config.IsEnabled;
    }

    private async Task EvaluateProviderContractExpiringAsync(
        string tenantId, DateTime now, Dictionary<AlertRuleType, AlertConfiguration> configurations, List<AlertDto> alerts)
    {
        if (!IsRuleEnabled(configurations, AlertRuleType.ProviderContractExpiring))
        {
            return;
        }

        var config = configurations[AlertRuleType.ProviderContractExpiring];
        var limitDate = now.AddDays(config.ThresholdDays);

        var count = await _context.Contracts
            .Where(c => c.TenantId == tenantId
                && c.Status == ContractStatus.Active
                && c.EndDate > now
                && c.EndDate <= limitDate)
            .CountAsync();

        if (count > 0)
        {
            alerts.Add(new AlertDto
            {
                Id = Guid.NewGuid().ToString(),
                RuleType = AlertRuleType.ProviderContractExpiring.ToString(),
                Urgency = config.DefaultUrgency,
                Title = "Contratos próximos a vencer",
                Description = $"Hay {count} contrato(s) de proveedores que vencen en menos de {config.ThresholdDays} días.",
                ModuleLink = "/contracts",
                CreatedAt = now
            });
        }
    }

    private async Task EvaluatePreventiveMaintenanceDueAsync(
        string tenantId, DateTime now, Dictionary<AlertRuleType, AlertConfiguration> configurations, List<AlertDto> alerts)
    {
        if (!IsRuleEnabled(configurations, AlertRuleType.PreventiveMaintenanceDue))
        {
            return;
        }

        var config = configurations[AlertRuleType.PreventiveMaintenanceDue];
        var limitDate = now.AddDays(config.ThresholdDays);

        var count = await _context.WorkOrders
            .Where(w => w.TenantId == tenantId
                && w.OrderType == WorkOrderType.Preventive
                && w.Status == WorkOrderStatus.PendingAssignment
                && w.ScheduledDate != null
                && w.ScheduledDate <= limitDate)
            .CountAsync();

        if (count > 0)
        {
            alerts.Add(new AlertDto
            {
                Id = Guid.NewGuid().ToString(),
                RuleType = AlertRuleType.PreventiveMaintenanceDue.ToString(),
                Urgency = config.DefaultUrgency,
                Title = "Mantenimientos preventivos sin proveedor",
                Description = $"Hay {count} mantenimiento(s) preventivo(s) programado(s) para los próximos {config.ThresholdDays} días sin proveedor asignado.",
                ModuleLink = "/maintenance/work-orders",
                CreatedAt = now
            });
        }
    }

    private async Task EvaluatePqrOverdueAsync(
        string tenantId, DateTime now, Dictionary<AlertRuleType, AlertConfiguration> configurations, List<AlertDto> alerts)
    {
        if (!IsRuleEnabled(configurations, AlertRuleType.PqrOverdue))
        {
            return;
        }

        var config = configurations[AlertRuleType.PqrOverdue];

        var count = await _context.PqrRecords
            .Where(p => p.TenantId == tenantId
                && p.Deadline != null
                && p.Deadline < now
                && p.Status != PQRStatus.Closed
                && p.Status != PQRStatus.Responded
                && p.Status != PQRStatus.Escalated)
            .CountAsync();

        if (count > 0)
        {
            alerts.Add(new AlertDto
            {
                Id = Guid.NewGuid().ToString(),
                RuleType = AlertRuleType.PqrOverdue.ToString(),
                Urgency = config.DefaultUrgency,
                Title = "PQR sin respuesta a tiempo",
                Description = $"Hay {count} PQR que superaron el tiempo límite de respuesta.",
                ModuleLink = "/pqr",
                CreatedAt = now
            });
        }
    }

    private async Task EvaluateBudgetItemExecutionExceededAsync(
        string tenantId, Dictionary<AlertRuleType, AlertConfiguration> configurations, List<AlertDto> alerts)
    {
        if (!IsRuleEnabled(configurations, AlertRuleType.BudgetItemExecutionExceeded))
        {
            return;
        }

        var config = configurations[AlertRuleType.BudgetItemExecutionExceeded];
        var currentYear = DateTime.UtcNow.Year;

        BudgetExecutionDashboardDto execution;
        try
        {
            execution = await _executionEngineService.GetExecutionDashboardAsync(tenantId, currentYear);
        }
        catch (KeyNotFoundException)
        {
            return;
        }

        var criticalItems = execution.Alerts.Where(a => a.Severity == "Critical").ToList();
        var warningItems = execution.Alerts.Where(a => a.Severity == "Warning").ToList();

        if (criticalItems.Count > 0)
        {
            alerts.Add(new AlertDto
            {
                Id = Guid.NewGuid().ToString(),
                RuleType = AlertRuleType.BudgetItemExecutionExceeded.ToString(),
                Urgency = AlertUrgency.Critical,
                Title = "Rubros presupuestales al 100% o más",
                Description = $"{criticalItems.Count} rubro(s) del presupuesto superaron el 100% de ejecución: {string.Join(", ", criticalItems.Select(a => a.ItemName))}.",
                ModuleLink = "/budgets",
                CreatedAt = DateTime.UtcNow
            });
        }

        if (warningItems.Count > 0)
        {
            alerts.Add(new AlertDto
            {
                Id = Guid.NewGuid().ToString(),
                RuleType = AlertRuleType.BudgetItemExecutionExceeded.ToString(),
                Urgency = config.DefaultUrgency,
                Title = $"Rubros presupuestales sobre el {config.ThresholdPercentage:N0}% de ejecución",
                Description = $"{warningItems.Count} rubro(s) del presupuesto superaron el {config.ThresholdPercentage:N0}% de ejecución: {string.Join(", ", warningItems.Select(a => a.ItemName))}.",
                ModuleLink = "/budgets",
                CreatedAt = DateTime.UtcNow
            });
        }
    }

    private async Task EvaluateAssetOutOfServiceAsync(
        string tenantId, Dictionary<AlertRuleType, AlertConfiguration> configurations, List<AlertDto> alerts)
    {
        if (!IsRuleEnabled(configurations, AlertRuleType.AssetOutOfService))
        {
            return;
        }

        var config = configurations[AlertRuleType.AssetOutOfService];

        var outOfServiceAssets = await _context.CommonAssets
            .Where(a => a.TenantId == tenantId && a.Status == AssetStatus.OutOfService)
            .ToListAsync();

        if (outOfServiceAssets.Count == 0)
        {
            return;
        }

        var essentialCount = outOfServiceAssets.Count(a => a.IsEssential);
        var urgency = config.DefaultUrgency;
        if (essentialCount > 0)
        {
            urgency = AlertUrgency.Critical;
        }

        alerts.Add(new AlertDto
        {
            Id = Guid.NewGuid().ToString(),
            RuleType = AlertRuleType.AssetOutOfService.ToString(),
            Urgency = urgency,
            Title = "Bienes fuera de servicio",
            Description = $"Hay {outOfServiceAssets.Count} bien(es) fuera de servicio, {essentialCount} esencial(es).",
            ModuleLink = "/maintenance/out-of-service",
            CreatedAt = DateTime.UtcNow
        });
    }

    private async Task EvaluateWorkOrderUnassignedAsync(
        string tenantId, DateTime now, Dictionary<AlertRuleType, AlertConfiguration> configurations, List<AlertDto> alerts)
    {
        if (!IsRuleEnabled(configurations, AlertRuleType.WorkOrderUnassigned))
        {
            return;
        }

        var config = configurations[AlertRuleType.WorkOrderUnassigned];
        var limitDate = now.AddDays(config.ThresholdDays);

        var count = await _context.WorkOrders
            .Where(w => w.TenantId == tenantId
                && w.Status == WorkOrderStatus.PendingAssignment
                && w.ScheduledDate != null
                && w.ScheduledDate <= limitDate)
            .CountAsync();

        if (count > 0)
        {
            alerts.Add(new AlertDto
            {
                Id = Guid.NewGuid().ToString(),
                RuleType = AlertRuleType.WorkOrderUnassigned.ToString(),
                Urgency = config.DefaultUrgency,
                Title = "Órdenes de trabajo sin asignar",
                Description = $"Hay {count} orden(es) sin asignar próximas a su fecha de ejecución.",
                ModuleLink = "/maintenance/work-orders",
                CreatedAt = now
            });
        }
    }

    private async Task EvaluateReservationNotCheckedInAsync(
        string tenantId, DateTime now, Dictionary<AlertRuleType, AlertConfiguration> configurations, List<AlertDto> alerts)
    {
        if (!IsRuleEnabled(configurations, AlertRuleType.ReservationNotCheckedIn))
        {
            return;
        }

        var config = configurations[AlertRuleType.ReservationNotCheckedIn];
        var todayStart = now.Date;
        var todayEnd = todayStart.AddDays(1);

        var count = await _context.Reservations
            .Where(r => r.TenantId == tenantId
                && r.StartDateTime >= todayStart
                && r.StartDateTime < todayEnd
                && r.StartDateTime <= now
                && r.CheckedInAt == null
                && (r.Status == ReservationStatus.Approved || r.Status == ReservationStatus.InUse))
            .CountAsync();

        if (count > 0)
        {
            alerts.Add(new AlertDto
            {
                Id = Guid.NewGuid().ToString(),
                RuleType = AlertRuleType.ReservationNotCheckedIn.ToString(),
                Urgency = config.DefaultUrgency,
                Title = "Reservas del día sin confirmar entrega",
                Description = $"Hay {count} reserva(s) de hoy sin confirmación de entrega del espacio.",
                ModuleLink = "/reservation/admin",
                CreatedAt = now
            });
        }
    }

    public async Task<List<AlertConfigurationDto>> GetConfigurationsAsync(string tenantId)
    {
        var effective = await LoadEffectiveConfigurationsAsync(tenantId);

        return effective.Select(pair => new AlertConfigurationDto
        {
            Id = pair.Value.Id,
            RuleType = pair.Key.ToString(),
            IsEnabled = pair.Value.IsEnabled,
            ThresholdDays = pair.Value.ThresholdDays,
            ThresholdPercentage = pair.Value.ThresholdPercentage,
            DefaultUrgency = pair.Value.DefaultUrgency.ToString(),
            HasRealDataSource = pair.Key != AlertRuleType.PaymentAgreementOverdue
        }).ToList();
    }

    public async Task InitializeDefaultAlertConfigurationsAsync(string tenantId)
    {
        var existingRuleTypes = await _context.AlertConfigurations
            .Where(ac => ac.TenantId == tenantId)
            .Select(ac => ac.RuleType)
            .ToListAsync();

        var existingSet = new HashSet<AlertRuleType>(existingRuleTypes);

        foreach (var pair in DefaultConfigurations)
        {
            if (existingSet.Contains(pair.Key))
            {
                continue;
            }

            _context.AlertConfigurations.Add(new AlertConfiguration
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                RuleType = pair.Key,
                IsEnabled = pair.Value.IsEnabled,
                ThresholdDays = pair.Value.ThresholdDays,
                ThresholdPercentage = pair.Value.ThresholdPercentage,
                DefaultUrgency = pair.Value.DefaultUrgency,
                UseDefaultThreshold = true
            });
        }

        await _context.SaveChangesAsync();
    }

    public async Task<AlertConfigurationDto> UpdateConfigurationAsync(
        string tenantId, string ruleType, string userId, UpdateAlertConfigurationRequestDto request)
    {
        if (!Enum.TryParse<AlertRuleType>(ruleType, true, out var parsedRuleType))
        {
            throw new ArgumentException("Tipo de regla de alerta inválido.");
        }

        if (!Enum.TryParse<AlertUrgency>(request.DefaultUrgency, true, out var parsedUrgency))
        {
            throw new ArgumentException("Nivel de urgencia inválido.");
        }

        var config = await _context.AlertConfigurations
            .FirstOrDefaultAsync(ac => ac.TenantId == tenantId && ac.RuleType == parsedRuleType);

        if (config == null)
        {
            config = new AlertConfiguration
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                RuleType = parsedRuleType,
                CreatedAt = DateTime.UtcNow
            };
            _context.AlertConfigurations.Add(config);
        }

        config.IsEnabled = request.IsEnabled;
        config.ThresholdDays = request.ThresholdDays;
        config.ThresholdPercentage = request.ThresholdPercentage;
        config.DefaultUrgency = parsedUrgency;
        config.UseDefaultThreshold = false;
        config.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return new AlertConfigurationDto
        {
            Id = config.Id,
            RuleType = config.RuleType.ToString(),
            IsEnabled = config.IsEnabled,
            ThresholdDays = config.ThresholdDays,
            ThresholdPercentage = config.ThresholdPercentage,
            DefaultUrgency = config.DefaultUrgency.ToString(),
            HasRealDataSource = parsedRuleType != AlertRuleType.PaymentAgreementOverdue
        };
    }
}
