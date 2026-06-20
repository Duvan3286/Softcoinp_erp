using System;
using Softcoinp.ERP.Domain.Entities;

namespace Softcoinp.ERP.Domain.Entities;

public class EntryLine
{
    public Guid Id { get; set; }

    public Guid AccountingEntryId { get; set; }
    public AccountingEntry? AccountingEntry { get; set; }

    public Guid AccountingAccountId { get; set; }
    public AccountingAccount? AccountingAccount { get; set; }

    public string? ThirdPartyId { get; set; }

    public decimal Debit { get; set; }
    public decimal Credit { get; set; }
}
