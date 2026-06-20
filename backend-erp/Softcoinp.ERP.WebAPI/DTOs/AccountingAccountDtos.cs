using System;

namespace Softcoinp.ERP.WebAPI.DTOs;

public class CreateAuxiliaryAccountRequestDto
{
    public string ParentCode { get; set; } = string.Empty;
    public string SubCode { get; set; } = string.Empty; // e.g., "01" (becomes ParentCode + SubCode)
    public string Name { get; set; } = string.Empty;
    public bool IsGroup { get; set; } // Si acepta subcuentas adicionales
}

public class UpdateAccountingAccountRequestDto
{
    public string Name { get; set; } = string.Empty;
    public bool IsActive { get; set; }
}

public class AccountingAccountDto
{
    public Guid Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string Nature { get; set; } = string.Empty;
    public bool IsGroup { get; set; }
    public bool IsActive { get; set; }
    public bool IsOfficialStandard { get; set; }
}
