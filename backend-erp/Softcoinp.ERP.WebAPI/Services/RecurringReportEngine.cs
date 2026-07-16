using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
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
            var deliveryTracker = sp.GetRequiredService<DeliveryTrackerEngine>();

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

                    GeneratedReport generatedReport;
                    if (format == "Excel")
                    {
                        generatedReport = await excelEngine.GenerateExcelReportAsync(
                            tenantId, reportTypeCode, config.CreatedByUserId,
                            periodFrom, periodTo, null, null, config.Id);
                    }
                    else
                    {
                        generatedReport = await pdfEngine.GenerateReportAsync(
                            tenantId, reportTypeCode, format, config.CreatedByUserId,
                            periodFrom, periodTo, null, null, config.Id);
                    }

                    await SendGeneratedReportByEmailAsync(context, deliveryTracker, config, generatedReport);

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

    private async Task SendGeneratedReportByEmailAsync(
        ApplicationDbContext context, DeliveryTrackerEngine deliveryTracker,
        RecurringReportConfig config, GeneratedReport generatedReport)
    {
        var recipientEmails = new List<string>();
        if (!string.IsNullOrEmpty(config.RecipientEmails))
        {
            recipientEmails = config.RecipientEmails
                .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .ToList();
        }

        if (recipientEmails.Count == 0)
        {
            return;
        }

        var subject = config.SubjectTemplate;
        if (string.IsNullOrEmpty(subject))
        {
            subject = "Reporte automatico: " + config.Name;
        }

        var body = config.BodyTemplate;
        if (string.IsNullOrEmpty(body))
        {
            body = "Se adjunta el reporte '" + config.Name + "' generado automaticamente por el sistema. Archivo: " + generatedReport.FileName;
        }

        var communication = new Communication
        {
            TenantId = config.TenantId,
            Subject = subject,
            Body = body,
            Status = CommunicationStatus.Draft,
            AudienceType = AudienceType.CustomGroup,
            SelectedChannels = "Email",
            FilePaths = JsonSerializer.Serialize(new List<string> { generatedReport.FilePath }),
            CreatedByUserId = config.CreatedByUserId
        };

        context.Communications.Add(communication);
        await context.SaveChangesAsync();

        foreach (var email in recipientEmails)
        {
            context.CommunicationRecipients.Add(new CommunicationRecipient
            {
                TenantId = config.TenantId,
                CommunicationId = communication.Id,
                RecipientEmail = email
            });
        }
        await context.SaveChangesAsync();

        await deliveryTracker.ProcessCommunicationDeliveryAsync(communication.Id);
    }

    private static DateTime? GetPeriodFrom(ReportFrequency frequency)
    {
        var now = DateTime.UtcNow;
        return frequency switch
        {
            ReportFrequency.Weekly => now.AddDays(-7),
            ReportFrequency.Biweekly => now.AddDays(-14),
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
            ReportFrequency.Weekly => from.AddDays(7),
            ReportFrequency.Biweekly => from.AddDays(14),
            ReportFrequency.Monthly => from.AddMonths(1),
            ReportFrequency.Quarterly => from.AddMonths(3),
            ReportFrequency.Annual => from.AddYears(1),
            _ => from.AddMonths(1)
        };
    }
}
