using System.Collections.Generic;
using Softcoinp.ERP.Domain.Enums;

namespace Softcoinp.ERP.Domain.Entities;

public class ReportType
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string TenantId { get; set; } = string.Empty;

    public ReportTypeEnum ReportTypeCode { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public ReportCategory Category { get; set; }
    public string SourceModules { get; set; } = string.Empty;
    public bool ContainsPersonalData { get; set; }
    public bool IsActive { get; set; } = true;

    public ICollection<GeneratedReport> GeneratedReports { get; set; } = new List<GeneratedReport>();
}
