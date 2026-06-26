using System;
using System.Collections.Generic;

namespace Softcoinp.ERP.WebAPI.DTOs;

public class CreateBankAccountDto
{
    public Guid AccountingAccountId { get; set; }
    public string BankName { get; set; } = string.Empty;
    public string AccountNumber { get; set; } = string.Empty;
    public string AccountType { get; set; } = string.Empty;
    public decimal OpeningBalance { get; set; }
}

public class UpdateBankAccountDto
{
    public string BankName { get; set; } = string.Empty;
    public string AccountNumber { get; set; } = string.Empty;
    public string AccountType { get; set; } = string.Empty;
    public decimal OpeningBalance { get; set; }
    public bool IsActive { get; set; }
}

public class BankAccountDto
{
    public Guid Id { get; set; }
    public Guid AccountingAccountId { get; set; }
    public string AccountingAccountCode { get; set; } = string.Empty;
    public string AccountingAccountName { get; set; } = string.Empty;
    public string BankName { get; set; } = string.Empty;
    public string AccountNumber { get; set; } = string.Empty;
    public string AccountType { get; set; } = string.Empty;
    public decimal CurrentBalance { get; set; }
    public decimal OpeningBalance { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
}
