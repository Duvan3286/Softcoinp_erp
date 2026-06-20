using System;

namespace Softcoinp.ERP.Domain.Entities;

public class MonthlyDepreciation
{
    public Guid Id { get; set; }
    public string TenantId { get; set; } = string.Empty;

    public Guid FixedAssetId { get; set; }
    public FixedAsset? FixedAsset { get; set; }

    public Guid? AccountingEntryId { get; set; }
    public AccountingEntry? AccountingEntry { get; set; }

    public int FiscalYear { get; set; }
    public int Month { get; set; }
    public string PeriodLabel { get; set; } = string.Empty;

    public decimal DepreciationAmount { get; set; }
    public decimal AccumulatedAfter { get; set; }
    public decimal BookValueAfter { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
