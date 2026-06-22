using System;
using Softcoinp.ERP.Domain.Enums;

namespace Softcoinp.ERP.Domain.Entities;

public class PqrFollowUp
{
    public Guid Id { get; set; }

    public Guid PQRId { get; set; }
    public PqrRecord? PQR { get; set; }

    public PQRStatus PreviousStatus { get; set; }
    public PQRStatus NewStatus { get; set; }

    public DateTime ChangedAt { get; set; } = DateTime.UtcNow;
    public string ChangedByUserId { get; set; } = string.Empty;
    public string ChangedByUserName { get; set; } = string.Empty;
    public string Justification { get; set; } = string.Empty;
    public bool IsAutomatic { get; set; }
}
