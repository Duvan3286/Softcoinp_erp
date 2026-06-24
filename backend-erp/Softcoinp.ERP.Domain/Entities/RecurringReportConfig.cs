using System;
using System.Collections.Generic;
using Softcoinp.ERP.Domain.Enums;

namespace Softcoinp.ERP.Domain.Entities;

public class RecurringReportConfig
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string TenantId { get; set; } = string.Empty;

    public Guid ReportTypeId { get; set; }
    public ReportType? ReportType { get; set; }

    public string Name { get; set; } = string.Empty;
    public ReportFrequency Frequency { get; set; }
    public ReportFormat Format { get; set; }

    public string RecipientEmails { get; set; } = string.Empty;
    public string? SubjectTemplate { get; set; }
    public string? BodyTemplate { get; set; }

    public DateTime? LastExecutionAt { get; set; }
    public DateTime? NextExecutionAt { get; set; }
    public ReportRecurrentStatus Status { get; set; } = ReportRecurrentStatus.Active;

    public string CreatedByUserId { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    public ICollection<GeneratedReport> GeneratedReports { get; set; } = new List<GeneratedReport>();
}
