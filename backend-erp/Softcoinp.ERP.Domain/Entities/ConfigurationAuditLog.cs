using System;

namespace Softcoinp.ERP.Domain.Entities;

/// <summary>
/// Registro inmutable del historial de cambios en los parámetros financieros o críticos
/// de la configuración del Conjunto.
/// </summary>
public class ConfigurationAuditLog
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public string TenantId { get; set; } = string.Empty;

    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    /// <summary>Usuario que realizó el cambio (Admin o SuperAdmin)</summary>
    public string ChangedByUserId { get; set; } = string.Empty;

    /// <summary>Nombre del parámetro modificado (ej. "LatePaymentInterestRate")</summary>
    public string ParameterName { get; set; } = string.Empty;

    /// <summary>Valor antes de la modificación (serializado a string)</summary>
    public string OldValue { get; set; } = string.Empty;

    /// <summary>Nuevo valor asignado (serializado a string)</summary>
    public string NewValue { get; set; } = string.Empty;

    /// <summary>Motivo del cambio (opcional)</summary>
    public string? Reason { get; set; }
}
