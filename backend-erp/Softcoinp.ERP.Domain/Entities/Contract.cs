using Softcoinp.ERP.Domain.Enums;

namespace Softcoinp.ERP.Domain.Entities;

public class Contract
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string TenantId { get; set; } = string.Empty;

    public Guid ProviderId { get; set; }
    public Provider? Provider { get; set; }

    public string ContractNumber { get; set; } = string.Empty;

    public ContractType ContractType { get; set; }

    public string ObjectDescription { get; set; } = string.Empty;

    public decimal TotalValue { get; set; }

    public decimal MonthlyValue { get; set; }

    public bool IsRecurrent { get; set; }

    public DateTime StartDate { get; set; }

    public DateTime EndDate { get; set; }

    public bool HasAutoRenewal { get; set; }

    public int AutoRenewalNoticeDays { get; set; }

    public ApprovalLevel ApprovalLevel { get; set; } = ApprovalLevel.Administrator;

    public string CouncilMeetingActNumber { get; set; } = string.Empty;

    public Guid? ApprovedInAssemblyId { get; set; }
    public Assembly? ApprovedInAssembly { get; set; }

    public ContractStatus Status { get; set; } = ContractStatus.Draft;

    public string SignedContractFilePath { get; set; } = string.Empty;

    public string Observations { get; set; } = string.Empty;

    public string CreatedByUserId { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string? UpdatedByUserId { get; set; }
    public DateTime? UpdatedAt { get; set; }

    public ICollection<ProviderInvoice> Invoices { get; set; } = new List<ProviderInvoice>();

    public ICollection<ContractAlert> Alerts { get; set; } = new List<ContractAlert>();
}
