using System;
using Softcoinp.ERP.Domain.Enums;

namespace Softcoinp.ERP.Domain.Entities;

public class AlertConfiguration
{
    public Guid Id { get; set; }
    public string TenantId { get; set; } = string.Empty;

    public AlertRuleType RuleType { get; set; }
    public bool IsEnabled { get; set; } = true;

    public int ThresholdDays { get; set; }
    public decimal ThresholdPercentage { get; set; }
    public AlertUrgency DefaultUrgency { get; set; } = AlertUrgency.Medium;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    public bool UseDefaultThreshold { get; set; } = true;
}
