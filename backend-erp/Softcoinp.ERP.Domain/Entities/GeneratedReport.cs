using System;
using Softcoinp.ERP.Domain.Enums;

namespace Softcoinp.ERP.Domain.Entities;

public class GeneratedReport
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string TenantId { get; set; } = string.Empty;

    public Guid ReportTypeId { get; set; }
    public ReportType? ReportType { get; set; }

    public ReportFormat Format { get; set; }
    public DateTime? PeriodFrom { get; set; }
    public DateTime? PeriodTo { get; set; }

    public string FileName { get; set; } = string.Empty;
    public string FilePath { get; set; } = string.Empty;
    public long FileSizeBytes { get; set; }

    public string GeneratedByUserId { get; set; } = string.Empty;
    public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;

    public string? Parameters { get; set; }
    public string? Notes { get; set; }

    public Guid? RecurringConfigId { get; set; }
    public RecurringReportConfig? RecurringConfig { get; set; }
}
