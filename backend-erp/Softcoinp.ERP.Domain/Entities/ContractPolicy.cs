namespace Softcoinp.ERP.Domain.Entities;

public class ContractPolicy
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string TenantId { get; set; } = string.Empty;

    public Guid ContractId { get; set; }
    public Contract? Contract { get; set; }

    public string PolicyNumber { get; set; } = string.Empty;

    public string InsuranceCompany { get; set; } = string.Empty;

    public string PolicyType { get; set; } = string.Empty;

    public decimal InsuredAmount { get; set; }

    public DateTime StartDate { get; set; }

    public DateTime EndDate { get; set; }

    public string FilePath { get; set; } = string.Empty;

    public bool IsActive { get; set; } = true;

    public string CreatedByUserId { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
