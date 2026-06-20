using System;
using System.Collections.Generic;
using Softcoinp.ERP.Domain.Enums;

namespace Softcoinp.ERP.Domain.Entities;

public class FixedAsset
{
    public Guid Id { get; set; }
    public string TenantId { get; set; } = string.Empty;

    public Guid? AccountingAccountId { get; set; }
    public AccountingAccount? AccountingAccount { get; set; }

    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string SerialNumber { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;

    public decimal AcquisitionValue { get; set; }
    public DateTime AcquisitionDate { get; set; }
    public int UsefulLifeMonths { get; set; }
    public decimal ResidualValue { get; set; }
    public DepreciationMethod DepreciationMethod { get; set; } = DepreciationMethod.StraightLine;

    public decimal AccumulatedDepreciation { get; set; }
    public decimal BookValue { get; set; }
    public FixedAssetStatus Status { get; set; } = FixedAssetStatus.Active;

    public DateTime? DisposalDate { get; set; }
    public decimal? DisposalValue { get; set; }
    public string DisposalReason { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    public ICollection<MonthlyDepreciation> Depreciations { get; set; } = new List<MonthlyDepreciation>();
}
