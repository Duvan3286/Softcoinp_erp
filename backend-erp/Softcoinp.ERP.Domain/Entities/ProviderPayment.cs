using Softcoinp.ERP.Domain.Enums;

namespace Softcoinp.ERP.Domain.Entities;

public class ProviderPayment
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string TenantId { get; set; } = string.Empty;

    public Guid InvoiceId { get; set; }
    public ProviderInvoice? Invoice { get; set; }

    public decimal Amount { get; set; }

    public DateTime PaymentDate { get; set; }

    public PaymentMethod PaymentMethod { get; set; }

    public string ReferenceNumber { get; set; } = string.Empty;

    public string BankAccount { get; set; } = string.Empty;

    public string Notes { get; set; } = string.Empty;

    public string ReceiptFilePath { get; set; } = string.Empty;

    public PaymentStatus Status { get; set; } = PaymentStatus.Completed;

    public string CreatedByUserId { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
