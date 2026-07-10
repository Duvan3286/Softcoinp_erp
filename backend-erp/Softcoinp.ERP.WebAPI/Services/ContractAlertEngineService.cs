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

public class ContractAlertEngineService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<ContractAlertEngineService> _logger;

    public ContractAlertEngineService(IServiceScopeFactory scopeFactory, ILogger<ContractAlertEngineService> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await TenantBackgroundRunner.ForEachTenantAsync(_scopeFactory, async (context, sp) =>
                {
                    await GenerateContractExpirationAlertsAsync(context);
                    await GenerateAutoRenewalAlertsAsync(context);
                    await UpdateExpiredContractStatusAsync(context);
                    await CleanupResolvedAlertsAsync(context);
                });

                await Task.Delay(TimeSpan.FromHours(6), stoppingToken);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                _logger.LogError(ex, "Error en el motor de alertas de contratos");
                await Task.Delay(TimeSpan.FromMinutes(30), stoppingToken);
            }
        }
    }

    private async Task GenerateContractExpirationAlertsAsync(ApplicationDbContext context)
    {
        var now = DateTime.UtcNow;
        var contracts90 = await context.Contracts
            .Where(c => c.Status == ContractStatus.Active &&
                c.EndDate > now &&
                c.EndDate <= now.AddDays(90))
            .ToListAsync();

        foreach (var contract in contracts90)
        {
            var daysUntilExpiration = (int)(contract.EndDate - now).TotalDays;
            ContractAlertType alertType;
            string message;

            if (daysUntilExpiration <= 15)
            {
                alertType = ContractAlertType.FifteenDaysToExpiration;
                message = $"El contrato {contract.ContractNumber} vence en {daysUntilExpiration} días ({contract.EndDate:dd/MM/yyyy}).";
            }
            else if (daysUntilExpiration <= 30)
            {
                alertType = ContractAlertType.ThirtyDaysToExpiration;
                message = $"El contrato {contract.ContractNumber} vence en {daysUntilExpiration} días ({contract.EndDate:dd/MM/yyyy}).";
            }
            else
            {
                alertType = ContractAlertType.NinetyDaysToExpiration;
                message = $"El contrato {contract.ContractNumber} vence en {daysUntilExpiration} días ({contract.EndDate:dd/MM/yyyy}). Requiere revisión.";
            }

            var existingAlert = await context.ContractAlerts
                .AnyAsync(a => a.ContractId == contract.Id &&
                    a.AlertType == alertType &&
                    a.IsActive);

            if (!existingAlert)
            {
                var alert = new ContractAlert
                {
                    Id = Guid.NewGuid(),
                    TenantId = contract.TenantId,
                    ContractId = contract.Id,
                    AlertType = alertType,
                    Message = message,
                    GeneratedAt = now,
                    IsActive = true
                };

                context.ContractAlerts.Add(alert);
            }
        }

        await context.SaveChangesAsync();
    }

    private async Task GenerateAutoRenewalAlertsAsync(ApplicationDbContext context)
    {
        var now = DateTime.UtcNow;
        var contractsForRenewal = await context.Contracts
            .Where(c => c.Status == ContractStatus.Active &&
                c.HasAutoRenewal &&
                c.EndDate > now &&
                c.EndDate <= now.AddDays(c.AutoRenewalNoticeDays))
            .ToListAsync();

        foreach (var contract in contractsForRenewal)
        {
            var daysToRenewal = (int)(contract.EndDate - now).TotalDays;

            var existingAlert = await context.ContractAlerts
                .AnyAsync(a => a.ContractId == contract.Id &&
                    a.AlertType == ContractAlertType.AutoRenewalWarning &&
                    a.IsActive);

            if (!existingAlert)
            {
                var alert = new ContractAlert
                {
                    Id = Guid.NewGuid(),
                    TenantId = contract.TenantId,
                    ContractId = contract.Id,
                    AlertType = ContractAlertType.AutoRenewalWarning,
                    Message = $"El contrato {contract.ContractNumber} se renovará automáticamente en {daysToRenewal} días ({contract.EndDate:dd/MM/yyyy}). " +
                              $"Si no desea renovarlo, notifique la no renovación dentro de los próximos {contract.AutoRenewalNoticeDays} días.",
                    GeneratedAt = now,
                    IsActive = true
                };

                context.ContractAlerts.Add(alert);
            }
        }

        await context.SaveChangesAsync();
    }

    private async Task UpdateExpiredContractStatusAsync(ApplicationDbContext context)
    {
        var now = DateTime.UtcNow;
        var expiredContracts = await context.Contracts
            .Where(c => c.Status == ContractStatus.Active && c.EndDate <= now)
            .ToListAsync();

        foreach (var contract in expiredContracts)
        {
            if (contract.HasAutoRenewal)
            {
                var newEndDate = contract.EndDate.AddYears(1);
                contract.StartDate = contract.EndDate;
                contract.EndDate = newEndDate;
            }
            else
            {
                contract.Status = ContractStatus.Expired;
            }
        }

        if (expiredContracts.Any())
        {
            await context.SaveChangesAsync();
        }
    }

    private async Task CleanupResolvedAlertsAsync(ApplicationDbContext context)
    {
        var thirtyDaysAgo = DateTime.UtcNow.AddDays(-30);
        var oldResolvedAlerts = await context.ContractAlerts
            .Where(a => !a.IsActive && a.ResolvedAt != null && a.ResolvedAt < thirtyDaysAgo)
            .ToListAsync();

        if (oldResolvedAlerts.Any())
        {
            context.ContractAlerts.RemoveRange(oldResolvedAlerts);
            await context.SaveChangesAsync();
        }
    }
}
