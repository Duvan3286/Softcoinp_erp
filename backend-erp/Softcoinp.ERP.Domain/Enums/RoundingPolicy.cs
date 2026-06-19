namespace Softcoinp.ERP.Domain.Enums;

/// <summary>
/// Política de redondeo para liquidaciones financieras
/// </summary>
public enum RoundingPolicy
{
    /// <summary>Redondea al peso más cercano (ej. 100.4 -> 100, 100.5 -> 101)</summary>
    Nearest,
    
    /// <summary>Redondea siempre hacia arriba al peso superior (ej. 100.1 -> 101)</summary>
    Up,
    
    /// <summary>Redondea siempre hacia abajo al peso inferior (ej. 100.9 -> 100)</summary>
    Down
}
