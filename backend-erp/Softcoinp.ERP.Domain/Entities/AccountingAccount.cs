using Softcoinp.ERP.Domain.Common;

namespace Softcoinp.ERP.Domain.Entities;

public enum AccountingAccountCategory
{
    Asset,       // Activo
    Liability,   // Pasivo
    Equity,      // Patrimonio (Fondo Social)
    Income,      // Ingreso
    Expense      // Gasto
}

public enum AccountingAccountNature
{
    Debit,       // Débito
    Credit       // Crédito
}

/// <summary>
/// Represents an accounting account in the financial system (Resolution 029 & Custom Auxiliaries).
/// </summary>
public class AccountingAccount : BaseEntity
{
    public string TenantId { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public AccountingAccountCategory Category { get; set; }
    public AccountingAccountNature Nature { get; set; }
    public bool IsGroup { get; set; }
    public bool IsActive { get; set; } = true;
    public bool IsOfficialStandard { get; set; }
}
