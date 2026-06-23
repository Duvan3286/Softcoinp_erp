using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Softcoinp.ERP.Domain.Entities;
using Softcoinp.ERP.Domain.Enums;
using Softcoinp.ERP.Domain.Interfaces;
using Softcoinp.ERP.Infrastructure.Persistence;

namespace Softcoinp.ERP.WebAPI.Services;

public class PQRAlertEngineService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<PQRAlertEngineService> _logger;
    private static readonly TimeSpan CheckInterval = TimeSpan.FromMinutes(15);

    public PQRAlertEngineService(IServiceScopeFactory scopeFactory, ILogger<PQRAlertEngineService> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("PQR Alert Engine iniciado. Intervalo de verificación: {Interval} minutos.", CheckInterval.TotalMinutes);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ProcessAllTenantsAsync(stoppingToken);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                _logger.LogError(ex, "Error al procesar alertas de PQR.");
            }

            await Task.Delay(CheckInterval, stoppingToken);
        }
    }

    private async Task ProcessAllTenantsAsync(CancellationToken stoppingToken)
    {
        List<Tenant> tenants;

        using (var masterScope = _scopeFactory.CreateScope())
        {
            var masterContext = masterScope.ServiceProvider.GetRequiredService<MasterDbContext>();
            tenants = await masterContext.Tenants
                .Where(t => t.IsActive)
                .ToListAsync(stoppingToken);
        }

        var serverVersion = new MySqlServerVersion(new Version(8, 0, 0));

        foreach (var tenant in tenants)
        {
            try
            {
                using var scope = _scopeFactory.CreateScope();
                var tenantResolver = scope.ServiceProvider.GetRequiredService<ITenantResolver>();
                tenantResolver.SetCurrentTenant(tenant);

                var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
                optionsBuilder.UseMySql(tenant.ConnectionString, serverVersion);
                using var context = new ApplicationDbContext(optionsBuilder.Options, tenantResolver);

                await ProcessTenantAsync(context, stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error al procesar alertas para tenant {Tenant}.", tenant.Subdomain);
            }
        }
    }

    private async Task ProcessTenantAsync(ApplicationDbContext context, CancellationToken stoppingToken)
    {
        var now = DateTime.UtcNow;

        var activePqrs = await context.PqrRecords
            .Include(p => p.Alerts)
            .Where(p => p.Deadline.HasValue
                     && p.Status != PQRStatus.Closed
                     && p.Status != PQRStatus.Escalated
                     && p.Status != PQRStatus.Responded)
            .ToListAsync(stoppingToken);

        if (activePqrs.Count == 0)
        {
            return;
        }

        var systemUserId = "system";
        var systemUserName = "Sistema";

        foreach (var pqr in activePqrs)
        {
            var elapsed = (now - pqr.FiledAt).TotalMinutes;
            var total = (pqr.Deadline.Value - pqr.FiledAt).TotalMinutes;

            if (total <= 0)
            {
                continue;
            }

            var percent = elapsed / total * 100.0;

            var hasFiftyAlert = pqr.Alerts.Any(a => a.AlertType == PQRAlertType.FiftyPercent);
            var hasEightyAlert = pqr.Alerts.Any(a => a.AlertType == PQRAlertType.EightyPercent);
            var hasOverdueAlert = pqr.Alerts.Any(a => a.AlertType == PQRAlertType.Overdue);

            bool statusChanged = false;

            if (percent >= 100.0 && !hasOverdueAlert)
            {
                var alert = new PqrAlert
                {
                    Id = Guid.NewGuid(),
                    PQRId = pqr.Id,
                    AlertType = PQRAlertType.Overdue,
                    GeneratedAt = now,
                    IsActive = true,
                    EscalatedToCouncil = true
                };

                context.PqrAlerts.Add(alert);

                var previousStatus = pqr.Status;
                pqr.Status = PQRStatus.Escalated;

                var followUp = new PqrFollowUp
                {
                    Id = Guid.NewGuid(),
                    PQRId = pqr.Id,
                    PreviousStatus = previousStatus,
                    NewStatus = PQRStatus.Escalated,
                    ChangedAt = now,
                    ChangedByUserId = systemUserId,
                    ChangedByUserName = systemUserName,
                    Justification = "Vencimiento del plazo límite de respuesta. La PQR ha sido escalada automáticamente al Consejo de Administración.",
                    IsAutomatic = true
                };

                context.PqrFollowUps.Add(followUp);
                statusChanged = true;

                _logger.LogWarning(
                    "PQR {Radicado} vencida. Escalada automáticamente al Consejo.",
                    pqr.RadicadoNumber);
            }
            else if (percent >= 80.0 && !hasEightyAlert)
            {
                var alert = new PqrAlert
                {
                    Id = Guid.NewGuid(),
                    PQRId = pqr.Id,
                    AlertType = PQRAlertType.EightyPercent,
                    GeneratedAt = now,
                    IsActive = true,
                    EscalatedToCouncil = true
                };

                context.PqrAlerts.Add(alert);

                _logger.LogInformation(
                    "PQR {Radicado} al 80% del plazo. Escalada al Consejo de Administración.",
                    pqr.RadicadoNumber);
            }
            else if (percent >= 50.0 && !hasFiftyAlert)
            {
                if (pqr.Status == PQRStatus.Filed || pqr.Status == PQRStatus.UnderReview)
                {
                    var alert = new PqrAlert
                    {
                        Id = Guid.NewGuid(),
                        PQRId = pqr.Id,
                        AlertType = PQRAlertType.FiftyPercent,
                        GeneratedAt = now,
                        IsActive = true
                    };

                    context.PqrAlerts.Add(alert);

                    _logger.LogInformation(
                        "PQR {Radicado} al 50% del plazo. Alerta interna generada.",
                        pqr.RadicadoNumber);
                }
            }

            if (statusChanged)
            {
                pqr.UpdatedAt = now;
            }
        }

        var changes = await context.SaveChangesAsync(stoppingToken);

        if (changes > 0)
        {
            _logger.LogInformation("Alertas de PQR procesadas: {Changes} cambios registrados.", changes);
        }
    }
}
