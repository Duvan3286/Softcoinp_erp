using System;

namespace Softcoinp.ERP.Domain.Entities;

/// <summary>
/// Historial de Representantes Legales del conjunto para trazabilidad.
/// Se registra inmutablemente cuando hay un cambio en el representante.
/// </summary>
public class LegalRepresentativeHistory
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public string FullName { get; set; } = string.Empty;
    
    public string IdentificationDocument { get; set; } = string.Empty;

    public DateTime StartDate { get; set; }
    
    public DateTime? EndDate { get; set; }

    /// <summary>El usuario (Admin) que registró este cambio</summary>
    public string RecordedByUserId { get; set; } = string.Empty;

    public DateTime RecordedAt { get; set; } = DateTime.UtcNow;
}
