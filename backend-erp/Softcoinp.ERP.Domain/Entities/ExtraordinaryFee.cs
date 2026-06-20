using System;
using System.Collections.Generic;
using Softcoinp.ERP.Domain.Enums;

namespace Softcoinp.ERP.Domain.Entities;

public class ExtraordinaryFee
{
    public Guid Id { get; set; }
    public string TenantId { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string MeetingActNumber { get; set; } = string.Empty;
    public DateTime ApprovalDate { get; set; }
    public decimal TotalAmount { get; set; }
    public int NumberOfInstallments { get; set; }
    public string StartPeriod { get; set; } = string.Empty;
    public DistributionType DistributionType { get; set; }
    public ExtraordinaryFeeStatus Status { get; set; } = ExtraordinaryFeeStatus.Pending;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string CreatedByUserId { get; set; } = string.Empty;
    public DateTime? UpdatedAt { get; set; }

    public ICollection<ExtraordinaryFeeDistribution> Distributions { get; set; } = new List<ExtraordinaryFeeDistribution>();
}
