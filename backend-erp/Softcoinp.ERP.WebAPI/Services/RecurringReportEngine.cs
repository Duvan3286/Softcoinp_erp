using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Softcoinp.ERP.Domain.Enums;
using Softcoinp.ERP.Infrastructure.Persistence;

namespace Softcoinp.ERP.WebAPI.Services;

public class RecurringReportEngine : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<RecurringReportEngine> _logger;

    public RecurringReportEngine(IServiceScopeFactory scopeFactory, ILogger<RecurringReportEngine> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("RecurringReportEngine started.");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ProcessDueRecurringReportsAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing recurring reports.");
            }

            await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);
        }
    }

    private async Task ProcessDueRecurringReportsAsync(CancellationToken ct)
    {
        var now = DateTime.UtcNow;

        await TenantBackgroundRunner.ForEachTenantAsync(_scopeFactory, async (context, sp) =>
        {
            var pdfEngine = sp.GetRequiredService<PDFGenerationEngine>();
            var excelEngine = sp.GetRequiredService<ExcelGenerationEngine>();

            var dueConfigs = await context.RecurringReportConfigs
                .Where(c => c.Status == ReportRecurrentStatus.Active
                         && c.NextExecutionAt.HasValue
                         && c.NextExecutionAt.Value <= now)
                .Include(c => c.ReportType)
                .ToListAsync(ct);

            foreach (var config in dueConfigs)
            {
                try
                {
                    var tenantId = config.TenantId;
                    var reportTypeCode = config.ReportType!.ReportTypeCode.ToString();
                    var format = config.Format.ToString();
                    var periodFrom = GetPeriodFrom(config.Frequency);
                    var periodTo = now;

                    if (format == "Excel")
                    {
                        await excelEngine.GenerateExcelReportAsync(
                            tenantId, reportTypeCode, config.CreatedByUserId,
                            periodFrom, periodTo, null, null, config.Id);
                    }
                    else
                    {
                        await pdfEngine.GenerateReportAsync(
                            tenantId, reportTypeCode, format, config.CreatedByUserId,
                            periodFrom, periodTo, null, null, config.Id);
                    }

                    config.LastExecutionAt = now;
                    config.NextExecutionAt = CalculateNextExecution(config.Frequency, now);
                    config.UpdatedAt = now;

                    _logger.LogInformation("Recurring report executed: {Name} ({Type})", config.Name, reportTypeCode);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to execute recurring report: {Name}", config.Name);
                    config.Status = ReportRecurrentStatus.Paused;
                    config.UpdatedAt = now;
                }
            }

            if (dueConfigs.Count > 0)
                await context.SaveChangesAsync(ct);
        });
    }

    private static DateTime? GetPeriodFrom(ReportFrequency frequency)
    {
        var now = DateTime.UtcNow;
        return frequency switch
        {
            ReportFrequency.Daily => now.AddDays(-1),
            ReportFrequency.Weekly => now.AddDays(-7),
            ReportFrequency.Monthly => now.AddMonths(-1),
            ReportFrequency.Quarterly => now.AddMonths(-3),
            ReportFrequency.Annual => now.AddYears(-1),
            _ => now.AddMonths(-1)
        };
    }

    private static DateTime CalculateNextExecution(ReportFrequency frequency, DateTime from)
    {
        return frequency switch
        {
            ReportFrequency.Daily => from.AddDays(1),
            ReportFrequency.Weekly => from.AddDays(7),
            ReportFrequency.Monthly => from.AddMonths(1),
            ReportFrequency.Quarterly => from.AddMonths(3),
            ReportFrequency.Annual => from.AddYears(1),
            _ => from.AddMonths(1)
        };
    }
}
