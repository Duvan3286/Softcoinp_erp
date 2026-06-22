using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Softcoinp.ERP.Domain.Entities;
using Softcoinp.ERP.Domain.Enums;
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
                await ProcessAlertsAsync(stoppingToken);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                _logger.LogError(ex, "Error al procesar alertas de PQR.");
            }

            await Task.Delay(CheckInterval, stoppingToken);
        }
    }

    private async Task ProcessAlertsAsync(CancellationToken stoppingToken)
    {
        using var scope = _scopeFactory.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

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

            var percent = elapsed / total * 100m;

            var hasFiftyAlert = pqr.Alerts.Any(a => a.AlertType == PQRAlertType.FiftyPercent);
            var hasEightyAlert = pqr.Alerts.Any(a => a.AlertType == PQRAlertType.EightyPercent);
            var hasOverdueAlert = pqr.Alerts.Any(a => a.AlertType == PQRAlertType.Overdue);

            bool statusChanged = false;

            if (percent >= 100m && !hasOverdueAlert)
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
            else if (percent >= 80m && !hasEightyAlert)
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
            else if (percent >= 50m && !hasFiftyAlert)
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
