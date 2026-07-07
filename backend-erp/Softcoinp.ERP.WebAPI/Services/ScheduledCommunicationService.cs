using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Softcoinp.ERP.WebAPI.Services;

public class ScheduledCommunicationService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<ScheduledCommunicationService> _logger;
    private static readonly TimeSpan CheckInterval = TimeSpan.FromMinutes(5);

    public ScheduledCommunicationService(
        IServiceScopeFactory scopeFactory,
        ILogger<ScheduledCommunicationService> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation(
            "Servicio de comunicaciones programadas iniciado. Intervalo: {Interval} minutos.",
            CheckInterval.TotalMinutes);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var totalProcessed = 0;

                await TenantBackgroundRunner.ForEachTenantScopedAsync(_scopeFactory, async (sp) =>
                {
                    var communicationService = sp
                        .GetRequiredService<CommunicationService>();

                    var processed = await communicationService.GetPendingScheduledAsync();
                    totalProcessed += processed.Count;
                });

                if (totalProcessed > 0)
                {
                    _logger.LogInformation(
                        "Comunicados programados enviados: {Count}", totalProcessed);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Error al procesar comunicados programados");
            }

            await Task.Delay(CheckInterval, stoppingToken);
        }
    }
}
