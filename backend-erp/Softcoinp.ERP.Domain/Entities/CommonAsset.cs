using System;
using System.Collections.Generic;
using Softcoinp.ERP.Domain.Common;
using Softcoinp.ERP.Domain.Enums;

namespace Softcoinp.ERP.Domain.Entities;

public class CommonAsset : BaseEntity
{
    public string TenantId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public AssetCategory Category { get; set; }
    public string Location { get; set; } = string.Empty;
    public bool IsEssential { get; set; }
    public string Brand { get; set; } = string.Empty;
    public string Model { get; set; } = string.Empty;
    public string SerialNumber { get; set; } = string.Empty;
    public DateTime? AcquisitionDate { get; set; }
    public decimal AcquisitionValue { get; set; }
    public int EstimatedUsefulLifeMonths { get; set; }
    public Guid? ReferenceProviderId { get; set; }
    public Provider? ReferenceProvider { get; set; }
    public string Manufacturer { get; set; } = string.Empty;
    public bool HasWarranty { get; set; }
    public DateTime? WarrantyEndDate { get; set; }
    public AssetStatus Status { get; set; } = AssetStatus.Operational;
    public string StatusNotes { get; set; } = string.Empty;
    public string CreatedByUserId { get; set; } = string.Empty;

    public ICollection<AssetPhoto> Photos { get; set; } = new List<AssetPhoto>();
    public ICollection<MaintenancePlan> MaintenancePlans { get; set; } = new List<MaintenancePlan>();
    public ICollection<WorkOrder> WorkOrders { get; set; } = new List<WorkOrder>();
    public ICollection<AssetStatusHistory> StatusHistory { get; set; } = new List<AssetStatusHistory>();
}
