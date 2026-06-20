using System;
using Softcoinp.ERP.Domain.Enums;

namespace Softcoinp.ERP.Domain.Entities;

public class ClearanceCertificate
{
    public Guid Id { get; set; }
    public string TenantId { get; set; } = string.Empty;

    public Guid UnitId { get; set; }
    public Unit? Unit { get; set; }

    public string CertificateNumber { get; set; } = string.Empty;
    public DateTime IssueDate { get; set; }
    public DateTime ExpirationDate { get; set; }
    public decimal BalanceAtDate { get; set; }
    public ClearanceCertificateStatus Status { get; set; } = ClearanceCertificateStatus.Active;
    public string IssuedByUserId { get; set; } = string.Empty;
    public string SignedByAdministratorName { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
}
