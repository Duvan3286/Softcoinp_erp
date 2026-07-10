using Softcoinp.ERP.Domain.Enums;

namespace Softcoinp.ERP.Domain.Entities;

public class ProviderEvaluation
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string TenantId { get; set; } = string.Empty;

    public Guid ProviderId { get; set; }
    public Provider? Provider { get; set; }

    public string EvaluationPeriod { get; set; } = string.Empty;

    public int QualityScore { get; set; }

    public int ComplianceScore { get; set; }

    public int PriceScore { get; set; }

    public int AttentionScore { get; set; }

    public decimal AverageScore { get; set; }

    public string Comments { get; set; } = string.Empty;

    public EvaluationRecommendation Recommendation { get; set; }

    public string EvaluatedByUserId { get; set; } = string.Empty;

    public string EvaluatedByUserName { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
