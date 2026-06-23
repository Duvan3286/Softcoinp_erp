namespace Softcoinp.ERP.Domain.Entities;

public class RetentionConfiguration
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string TenantId { get; set; } = string.Empty;

    public string ServiceType { get; set; } = string.Empty;

    public string ServiceDescription { get; set; } = string.Empty;

    public decimal RetentionFuelRate { get; set; }

    public decimal RetentionIcaRate { get; set; }

    public bool IsActive { get; set; } = true;

    public string CreatedByUserId { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string? UpdatedByUserId { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
