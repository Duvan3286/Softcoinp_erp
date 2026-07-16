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
using Softcoinp.ERP.Domain.Interfaces;
using Softcoinp.ERP.Infrastructure.Persistence;

namespace Softcoinp.ERP.WebAPI.Services;

/// <summary>
/// Processes due reservation reminders (24h and 2h before the reservation start)
/// across every active tenant and sends them through the notification engine.
/// </summary>
public class ReservationReminderBackgroundService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<ReservationReminderBackgroundService> _logger;
    private static readonly TimeSpan CheckInterval = TimeSpan.FromMinutes(15);

    public ReservationReminderBackgroundService(
        IServiceScopeFactory scopeFactory, ILogger<ReservationReminderBackgroundService> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation(
            "Reservation Reminder Engine iniciado. Intervalo de verificación: {Interval} minutos.",
            CheckInterval.TotalMinutes);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ProcessAllTenantsAsync(stoppingToken);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                _logger.LogError(ex, "Error al procesar recordatorios de reserva.");
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

                var deliveryTracker = new DeliveryTrackerEngine(context);
                var notificationEngine = new NotificationEngine(context, deliveryTracker);
                var reminderEngine = new ReservationReminderEngine(context, notificationEngine);

                var processedCount = await reminderEngine.ProcessAllPendingRemindersAsync();
                if (processedCount > 0)
                {
                    _logger.LogInformation(
                        "Tenant {Tenant}: {Count} recordatorio(s) de reserva procesado(s).",
                        tenant.Subdomain, processedCount);
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error al procesar recordatorios de reserva para tenant {Tenant}.", tenant.Subdomain);
            }
        }
    }
}
