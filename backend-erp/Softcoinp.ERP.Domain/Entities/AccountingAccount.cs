using Softcoinp.ERP.Domain.Common;

namespace Softcoinp.ERP.Domain.Entities;

public enum AccountType
{
    Debit,
    Credit
}

/// <summary>
/// Represents an accounting account in the financial system.
/// </summary>
public class AccountingAccount : BaseEntity
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public AccountType Type { get; set; }
}
