using System;

namespace Softcoinp.ERP.Domain.Entities;

public class LateInterestConfiguration
{
    public Guid Id { get; set; }
    public string TenantId { get; set; } = string.Empty;

    public int InterestStartDays { get; set; }

    public bool ApplyToAllUnitsByDefault { get; set; } = true;

    public bool AlertOnMissingMonthlyRate { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    public string CreatedByUserId { get; set; } = string.Empty;
}
