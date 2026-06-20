using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Softcoinp.ERP.Domain.Entities;
using Softcoinp.ERP.Domain.Enums;
using Softcoinp.ERP.Infrastructure.Persistence;

namespace Softcoinp.ERP.WebAPI.Services;

public class AccountingIntegrationService
{
    private readonly ApplicationDbContext _context;

    public AccountingIntegrationService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<AccountingEntry> RecordBillingAsync(string tenantId, Guid billingPeriodId, decimal totalBilled, string description, string userId)
    {
        var receivableAccount = await GetAccountOrThrowAsync(tenantId, "1305");
        var incomeAccount = await GetAccountOrThrowAsync(tenantId, "4405");

        var entry = NewEntry(tenantId, description, $"FAC-{billingPeriodId:N}", totalBilled, userId);
        entry.Lines.Add(MakeLine(entry.Id, receivableAccount.Id, totalBilled, 0));
        entry.Lines.Add(MakeLine(entry.Id, incomeAccount.Id, 0, totalBilled));

        _context.AccountingEntries.Add(entry);
        await _context.SaveChangesAsync();
        return entry;
    }

    public async Task<AccountingEntry> RecordPaymentAsync(string tenantId, Guid paymentId, decimal amount, string description, string userId)
    {
        var bankAccount = await _context.AccountingAccounts
            .Where(a => a.TenantId == tenantId && a.Code.StartsWith("1110") && !a.IsGroup)
            .OrderBy(a => a.Code)
            .FirstOrDefaultAsync() ?? await GetAccountOrThrowAsync(tenantId, "1110");

        var receivableAccount = await GetAccountOrThrowAsync(tenantId, "1305");

        var entry = NewEntry(tenantId, description, $"PAG-{paymentId:N}", amount, userId);
        entry.Lines.Add(MakeLine(entry.Id, bankAccount.Id, amount, 0));
        entry.Lines.Add(MakeLine(entry.Id, receivableAccount.Id, 0, amount));

        _context.AccountingEntries.Add(entry);
        await _context.SaveChangesAsync();
        return entry;
    }

    public async Task<AccountingEntry> RecordExtraordinaryFeeAsync(string tenantId, Guid feeId, decimal totalAmount, string description, string userId)
    {
        var receivableAccount = await GetAccountOrThrowAsync(tenantId, "1305");
        var extraordinaryIncomeAccount = await _context.AccountingAccounts
            .Where(a => a.TenantId == tenantId && a.Code.StartsWith("4295") && !a.IsGroup)
            .OrderBy(a => a.Code)
            .FirstOrDefaultAsync() ?? await GetAccountOrThrowAsync(tenantId, "4405");

        var entry = NewEntry(tenantId, description, $"EXT-{feeId:N}", totalAmount, userId);
        entry.Lines.Add(MakeLine(entry.Id, receivableAccount.Id, totalAmount, 0));
        entry.Lines.Add(MakeLine(entry.Id, extraordinaryIncomeAccount.Id, 0, totalAmount));

        _context.AccountingEntries.Add(entry);
        await _context.SaveChangesAsync();
        return entry;
    }

    public async Task<AccountingEntry> RecordIndividualChargeAsync(string tenantId, Guid chargeId, decimal amount, string description, string userId)
    {
        var receivableAccount = await GetAccountOrThrowAsync(tenantId, "1305");
        var otherIncomeAccount = await _context.AccountingAccounts
            .Where(a => a.TenantId == tenantId && a.Code.StartsWith("4205") && !a.IsGroup)
            .OrderBy(a => a.Code)
            .FirstOrDefaultAsync() ?? await GetAccountOrThrowAsync(tenantId, "4405");

        var entry = NewEntry(tenantId, description, $"CGO-{chargeId:N}", amount, userId);
        entry.Lines.Add(MakeLine(entry.Id, receivableAccount.Id, amount, 0));
        entry.Lines.Add(MakeLine(entry.Id, otherIncomeAccount.Id, 0, amount));

        _context.AccountingEntries.Add(entry);
        await _context.SaveChangesAsync();
        return entry;
    }

    public async Task<AccountingEntry> RecordLateInterestAsync(string tenantId, Guid interestId, decimal amount, string description, string userId)
    {
        var receivableAccount = await GetAccountOrThrowAsync(tenantId, "1305");
        var financialIncomeAccount = await _context.AccountingAccounts
            .Where(a => a.TenantId == tenantId && a.Code.StartsWith("5305") && !a.IsGroup)
            .OrderBy(a => a.Code)
            .FirstOrDefaultAsync() ?? await GetAccountOrThrowAsync(tenantId, "4405");

        var entry = NewEntry(tenantId, description, $"INT-{interestId:N}", amount, userId);
        entry.Lines.Add(MakeLine(entry.Id, receivableAccount.Id, amount, 0));
        entry.Lines.Add(MakeLine(entry.Id, financialIncomeAccount.Id, 0, amount));

        _context.AccountingEntries.Add(entry);
        await _context.SaveChangesAsync();
        return entry;
    }

    private async Task<AccountingAccount> GetAccountOrThrowAsync(string tenantId, string code)
    {
        return await _context.AccountingAccounts
            .Where(a => a.TenantId == tenantId && a.Code == code)
            .FirstOrDefaultAsync()
            ?? throw new InvalidOperationException($"Cuenta contable {code} no encontrada. Configure el plan de cuentas estándar antes de generar asientos automáticos.");
    }

    private static AccountingEntry NewEntry(string tenantId, string description, string externalRef, decimal total, string userId)
    {
        return new AccountingEntry
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            EntryDate = DateTime.UtcNow,
            Description = description,
            ExternalReference = externalRef,
            EntryType = EntryType.Automatic,
            Status = EntryStatus.Final,
            TotalDebit = total,
            TotalCredit = total,
            CreatedByUserId = userId
        };
    }

    private static EntryLine MakeLine(Guid entryId, Guid accountId, decimal debit, decimal credit)
    {
        return new EntryLine
        {
            AccountingEntryId = entryId,
            AccountingAccountId = accountId,
            Debit = debit,
            Credit = credit
        };
    }
}
