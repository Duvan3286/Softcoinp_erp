using System;

namespace Softcoinp.ERP.Domain.Entities;

public class MonthlyInterestRate
{
    public Guid Id { get; set; }
    public string TenantId { get; set; } = string.Empty;

    public int Year { get; set; }
    public int Month { get; set; }

    public decimal CertifiedRate { get; set; }

    public decimal AppliedRate { get; set; }

    public DateTime RegisteredAt { get; set; } = DateTime.UtcNow;
    public string RegisteredByUserId { get; set; } = string.Empty;
}
