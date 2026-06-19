using System;
using Softcoinp.ERP.Domain.Common;
using Softcoinp.ERP.Domain.Enums;

namespace Softcoinp.ERP.Domain.Entities;

/// <summary>
/// Configuración legal, financiera y operativa del Conjunto Residencial.
/// Bajo la Ley 675 de 2001 (Colombia). Esta entidad es única por base de datos (tenant).
/// </summary>
public class TenantConfiguration : BaseEntity
{
    // ── 1. DATOS LEGALES ───────────────────────────────────────────────
    public string OfficialName { get; set; } = string.Empty;
    
    /// <summary>NIT sin dígito de verificación</summary>
    public string Nit { get; set; } = string.Empty;
    
    /// <summary>Dígito de verificación calculado (DIAN)</summary>
    public string VerificationDigit { get; set; } = string.Empty;
    
    public string Address { get; set; } = string.Empty;
    public string Municipality { get; set; } = string.Empty;
    public string Department { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    
    /// <summary>Número de Matrícula Inmobiliaria matriz</summary>
    public string RealEstateRegistration { get; set; } = string.Empty;
    
    /// <summary>Fecha de constitución de la propiedad horizontal</summary>
    public DateTime ConstitutionDate { get; set; }

    /// <summary>Nombre del Representante Legal actual</summary>
    public string LegalRepresentativeName { get; set; } = string.Empty;

    /// <summary>Tipo de Documento del Representante Legal actual</summary>
    public IdentityDocumentType LegalRepresentativeDocumentType { get; set; } = IdentityDocumentType.CC;

    /// <summary>Documento del Representante Legal actual</summary>
    public string LegalRepresentativeId { get; set; } = string.Empty;

    /// <summary>Dígito de verificación del Representante Legal actual (solo si el tipo es NIT)</summary>
    public string LegalRepresentativeDv { get; set; } = string.Empty;

    // ── 2. PARÁMETROS FINANCIEROS ──────────────────────────────────────
    /// <summary>Día del mes (1-28) para generar la liquidación mensual</summary>
    public int BillingCycleDay { get; set; } = 1;

    /// <summary>Días de gracia después del BillingCycleDay para pago sin mora</summary>
    public int GracePeriodDays { get; set; } = 10;

    /// <summary>Tasa Mensual de Interés de Mora (%) que aplica el conjunto</summary>
    public decimal LatePaymentInterestRate { get; set; } = 0m;

    /// <summary>Tasa Máxima Legal Vigente (%) según Superfinanciera (Control/Validación)</summary>
    public decimal MaxLegalInterestRate { get; set; } = 0m;

    /// <summary>Mes de inicio del período fiscal (1 = Enero)</summary>
    public int FiscalYearStartMonth { get; set; } = 1;

    /// <summary>Día de inicio del período fiscal</summary>
    public int FiscalYearStartDay { get; set; } = 1;

    /// <summary>Valor total presupuestado anual (Base para coeficientes y fondo)</summary>
    public decimal AnnualBudget { get; set; } = 0m;


    // ── 3. PARÁMETROS OPERATIVOS ───────────────────────────────────────
    public int TotalUnits { get; set; } = 0;
    public int TotalTowers { get; set; } = 1;
    
    public RoundingPolicy RoundingPolicy { get; set; } = RoundingPolicy.Nearest;

    /// <summary>Máximo de cuotas extraordinarias activas al mismo tiempo</summary>
    public int MaxActiveExtraordinaryQuotas { get; set; } = 3;

    /// <summary>Si el conjunto recauda Fondo de Imprevistos (Art. 35 Ley 675)</summary>
    public bool HasContingencyFund { get; set; } = true;

    /// <summary>Porcentaje del presupuesto destinado a Imprevistos (Mínimo legal 1%)</summary>
    public decimal ContingencyFundPercentage { get; set; } = 1m;


    // ── 4. NOTIFICACIONES & BRANDING ───────────────────────────────────
    public string SenderEmail { get; set; } = string.Empty;
    public string SignatureFooterTemplate { get; set; } = string.Empty;
    
    public bool AutoSendLatePaymentNotifications { get; set; } = false;
    public int LatePaymentNotificationFrequencyDays { get; set; } = 30;

    /// <summary>Ruta del archivo PNG/SVG en el storage local</summary>
    public string? LogoUrl { get; set; }
}
