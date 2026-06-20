using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Softcoinp.ERP.Domain.Enums;

namespace Softcoinp.ERP.WebAPI.DTOs;

public class AccountingPeriodDto
{
    public Guid Id { get; set; }
    public int FiscalYear { get; set; }
    public int Month { get; set; }
    public string PeriodLabel { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime OpenedAt { get; set; }
    public DateTime? ClosedAt { get; set; }
    public string? ClosedByUserId { get; set; }
    public int LastEntryNumber { get; set; }
}

public class CreateAccountingPeriodDto
{
    [Required]
    [Range(2000, 2100)]
    public int FiscalYear { get; set; }

    [Required]
    [Range(1, 12)]
    public int Month { get; set; }

    [Required]
    [StringLength(20)]
    public string PeriodLabel { get; set; } = string.Empty;
}

public class JournalEntryLineDto
{
    public Guid Id { get; set; }
    public Guid AccountingAccountId { get; set; }
    public string AccountCode { get; set; } = string.Empty;
    public string AccountName { get; set; } = string.Empty;
    public string? ThirdPartyId { get; set; }
    public decimal Debit { get; set; }
    public decimal Credit { get; set; }
}

public class CreateJournalEntryLineDto
{
    [Required]
    public Guid AccountingAccountId { get; set; }

    public string? ThirdPartyId { get; set; }

    [Range(0, double.MaxValue)]
    public decimal Debit { get; set; }

    [Range(0, double.MaxValue)]
    public decimal Credit { get; set; }
}

public class JournalEntryDto
{
    public Guid Id { get; set; }
    public string TenantId { get; set; } = string.Empty;
    public Guid? AccountingPeriodId { get; set; }
    public string? PeriodLabel { get; set; }
    public int EntryNumber { get; set; }
    public string EntryType { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime EntryDate { get; set; }
    public string Description { get; set; } = string.Empty;
    public string? ExternalReference { get; set; }
    public decimal TotalDebit { get; set; }
    public decimal TotalCredit { get; set; }
    public string CreatedByUserId { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public List<JournalEntryLineDto> Lines { get; set; } = new();
}

public class CreateJournalEntryDto
{
    [Required]
    public DateTime EntryDate { get; set; }

    [Required]
    [StringLength(500)]
    public string Description { get; set; } = string.Empty;

    [StringLength(100)]
    public string? ExternalReference { get; set; }

    public Guid? AccountingPeriodId { get; set; }

    public EntryType EntryType { get; set; } = EntryType.Manual;

    [Required]
    [MinLength(2, ErrorMessage = "Una entrada contable debe tener al menos 2 líneas (débito y crédito)")]
    public List<CreateJournalEntryLineDto> Lines { get; set; } = new();
}

public class ReverseJournalEntryDto
{
    [Required]
    [StringLength(500)]
    public string Reason { get; set; } = string.Empty;
}

public class TrialBalanceItemDto
{
    public string AccountCode { get; set; } = string.Empty;
    public string AccountName { get; set; } = string.Empty;
    public string Nature { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public decimal TotalDebit { get; set; }
    public decimal TotalCredit { get; set; }
    public decimal Balance { get; set; }
}

public class GeneralLedgerEntryDto
{
    public DateTime Date { get; set; }
    public int EntryNumber { get; set; }
    public string Description { get; set; } = string.Empty;
    public string? ExternalReference { get; set; }
    public decimal Debit { get; set; }
    public decimal Credit { get; set; }
    public decimal RunningBalance { get; set; }
}

public class IncomeStatementItemDto
{
    public string AccountCode { get; set; } = string.Empty;
    public string AccountName { get; set; } = string.Empty;
    public decimal Balance { get; set; }
}

public class BalanceSheetItemDto
{
    public string AccountCode { get; set; } = string.Empty;
    public string AccountName { get; set; } = string.Empty;
    public decimal Balance { get; set; }
}
