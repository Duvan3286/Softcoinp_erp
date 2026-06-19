using System;
using Softcoinp.ERP.Domain.Enums;

namespace Softcoinp.ERP.Domain.Entities;

public class BulkImportLog
{
    public Guid Id { get; set; }
    public string TenantId { get; set; } = string.Empty;
    
    public DateTime ExecutedAt { get; set; } = DateTime.UtcNow;
    public string ExecutedByUserId { get; set; } = string.Empty;
    
    public BulkImportStatus Status { get; set; }
    public int ProcessedRecordsCount { get; set; }
    public int ErrorCount { get; set; }
    
    public string ErrorReport { get; set; } = string.Empty; // JSON array of errors
}
