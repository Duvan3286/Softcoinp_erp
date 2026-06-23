using Softcoinp.ERP.Domain.Enums;

namespace Softcoinp.ERP.Domain.Entities;

public class ProviderEvaluation
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string TenantId { get; set; } = string.Empty;

    public Guid ProviderId { get; set; }
    public Provider? Provider { get; set; }

    public Guid? ContractId { get; set; }
    public Contract? Contract { get; set; }

    public string EvaluationPeriod { get; set; } = string.Empty;

    public int ServiceQualityScore { get; set; }

    public int ComplianceScore { get; set; }

    public int PriceFairnessScore { get; set; }

    public int AfterSalesScore { get; set; }

    public decimal AverageScore { get; set; }

    public string Comments { get; set; } = string.Empty;

    public EvaluationRecommendation Recommendation { get; set; }

    public string EvaluatedByUserId { get; set; } = string.Empty;

    public string EvaluatedByUserName { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
