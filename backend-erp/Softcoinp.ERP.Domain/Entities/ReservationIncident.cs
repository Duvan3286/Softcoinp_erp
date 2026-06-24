using System;
using Softcoinp.ERP.Domain.Enums;

namespace Softcoinp.ERP.Domain.Entities;

public class ReservationIncident
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string TenantId { get; set; } = string.Empty;

    public Guid ReservationId { get; set; }
    public Reservation? Reservation { get; set; }

    public string Description { get; set; } = string.Empty;
    public IncidentSeverity Severity { get; set; } = IncidentSeverity.Minor;
    public decimal DamageAmount { get; set; }

    public bool DamageAssessed { get; set; }
    public bool DepositAppliedToDamage { get; set; }

    public string? EvidenceFilePath { get; set; }

    public string ReportedByUserId { get; set; } = string.Empty;
    public string? ReportedByName { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
