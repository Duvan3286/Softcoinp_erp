using Softcoinp.ERP.Domain.Common;
using Softcoinp.ERP.Domain.Enums;

namespace Softcoinp.ERP.Domain.Entities;

public class Provider : BaseEntity
{
    public string TenantId { get; set; } = string.Empty;

    public ProviderType ProviderType { get; set; }

    public string DocumentType { get; set; } = string.Empty;

    public string DocumentNumber { get; set; } = string.Empty;

    public string VerificationDigit { get; set; } = string.Empty;

    public string BusinessName { get; set; } = string.Empty;

    public string TradeName { get; set; } = string.Empty;

    public string ContactName { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string Phone { get; set; } = string.Empty;

    public string Address { get; set; } = string.Empty;

    public string City { get; set; } = string.Empty;

    public string EconomicActivity { get; set; } = string.Empty;

    public string ServiceType { get; set; } = string.Empty;

    public string RutFilePath { get; set; } = string.Empty;

    public string LegalRepDocumentType { get; set; } = string.Empty;

    public string LegalRepDocumentNumber { get; set; } = string.Empty;

    public string LegalRepName { get; set; } = string.Empty;

    public string LegalRepEmail { get; set; } = string.Empty;

    public bool IsPreferred { get; set; }

    public ProviderStatus Status { get; set; } = ProviderStatus.Active;

    public string CreatedByUserId { get; set; } = string.Empty;

    public ICollection<Contract> Contracts { get; set; } = new List<Contract>();

    public ICollection<ProviderInvoice> Invoices { get; set; } = new List<ProviderInvoice>();

    public ICollection<ProviderEvaluation> Evaluations { get; set; } = new List<ProviderEvaluation>();
}
