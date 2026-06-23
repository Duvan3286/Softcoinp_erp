using Softcoinp.ERP.Domain.Enums;

namespace Softcoinp.ERP.Domain.Entities;

public class ProviderInvoice
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string TenantId { get; set; } = string.Empty;

    public Guid ProviderId { get; set; }
    public Provider? Provider { get; set; }

    public Guid? ContractId { get; set; }
    public Contract? Contract { get; set; }

    public string InvoiceNumber { get; set; } = string.Empty;

    public DateTime InvoiceDate { get; set; }

    public DateTime DueDate { get; set; }

    public decimal Subtotal { get; set; }

    public decimal IvaAmount { get; set; }

    public decimal RetentionFuelAmount { get; set; }

    public decimal RetentionIcaAmount { get; set; }

    public decimal NetAmount { get; set; }

    public InvoiceStatus Status { get; set; } = InvoiceStatus.Pending;

    public string Description { get; set; } = string.Empty;

    public string InvoiceFilePath { get; set; } = string.Empty;

    public Guid? AccountingEntryId { get; set; }
    public AccountingEntry? AccountingEntry { get; set; }

    public string CreatedByUserId { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string? UpdatedByUserId { get; set; }
    public DateTime? UpdatedAt { get; set; }

    public ICollection<ProviderPayment> Payments { get; set; } = new List<ProviderPayment>();
}
