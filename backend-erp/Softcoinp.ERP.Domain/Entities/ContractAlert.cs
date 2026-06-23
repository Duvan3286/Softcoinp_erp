using Softcoinp.ERP.Domain.Enums;

namespace Softcoinp.ERP.Domain.Entities;

public class ContractAlert
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string TenantId { get; set; } = string.Empty;

    public Guid ContractId { get; set; }
    public Contract? Contract { get; set; }

    public ContractAlertType AlertType { get; set; }

    public string Message { get; set; } = string.Empty;

    public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;

    public bool IsActive { get; set; } = true;

    public DateTime? ResolvedAt { get; set; }

    public string? ResolvedByUserId { get; set; }

    public bool EscalatedToCouncil { get; set; }
}
