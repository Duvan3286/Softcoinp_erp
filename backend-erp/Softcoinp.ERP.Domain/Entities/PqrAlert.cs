using System;
using Softcoinp.ERP.Domain.Enums;

namespace Softcoinp.ERP.Domain.Entities;

public class PqrAlert
{
    public Guid Id { get; set; }

    public Guid PQRId { get; set; }
    public PqrRecord? PQR { get; set; }

    public PQRAlertType AlertType { get; set; }

    public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;
    public bool IsActive { get; set; } = true;
    public DateTime? ResolvedAt { get; set; }
    public bool EscalatedToCouncil { get; set; }
}
