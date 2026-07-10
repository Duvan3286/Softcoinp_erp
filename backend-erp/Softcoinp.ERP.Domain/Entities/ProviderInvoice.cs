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

    public decimal TotalAmount { get; set; }

    public decimal AmountPaid { get; set; }

    public DateTime? PaymentDate { get; set; }

    public PaymentMethod? PaymentMethod { get; set; }

    public string PaymentReferenceNumber { get; set; } = string.Empty;

    public Guid? BudgetItemId { get; set; }

    public InvoiceStatus Status { get; set; } = InvoiceStatus.PendingPayment;

    public string CreatedByUserId { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string? UpdatedByUserId { get; set; }
    public DateTime? UpdatedAt { get; set; }

    public ICollection<ProviderPayment> Payments { get; set; } = new List<ProviderPayment>();
}
