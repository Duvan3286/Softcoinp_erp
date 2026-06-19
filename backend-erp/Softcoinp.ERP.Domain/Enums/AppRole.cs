namespace Softcoinp.ERP.Domain.Enums;

/// <summary>
/// Roles del sistema ERP para conjuntos residenciales en Colombia.
/// </summary>
public enum AppRole
{
    /// <summary>Acceso total a todos los tenants. Solo uso interno del equipo Softcoinp.</summary>
    SuperAdmin,

    /// <summary>Acceso completo a todos los módulos del conjunto asignado.</summary>
    Admin,

    /// <summary>Lectura de módulos financieros y capacidad de aprobar solicitudes.</summary>
    Council,

    /// <summary>Acceso completo al módulo contable y de reportes. Sin acceso a módulos operativos.</summary>
    Accountant,

    /// <summary>Acceso exclusivo al portal personal: estado de cuenta, reservas y PQR.</summary>
    Resident,

    /// <summary>Solo lectura de estados financieros con fecha de vencimiento configurable.</summary>
    Auditor
}
