using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Softcoinp.ERP.Domain.Entities;
using Softcoinp.ERP.Domain.Enums;
using Softcoinp.ERP.Infrastructure.Persistence;
using Softcoinp.ERP.WebAPI.DTOs;

namespace Softcoinp.ERP.WebAPI.Services;

public class AccountingReportService
{
    private readonly ApplicationDbContext _context;

    public AccountingReportService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<TrialBalanceItemDto>> GetTrialBalanceAsync(string tenantId, Guid? periodId = null, DateTime? fromDate = null, DateTime? toDate = null)
    {
        var linesQuery = _context.EntryLines
            .Where(l => l.AccountingEntry!.TenantId == tenantId && l.AccountingEntry.Status == EntryStatus.Final)
            .AsQueryable();

        if (periodId.HasValue)
            linesQuery = linesQuery.Where(l => l.AccountingEntry!.AccountingPeriodId == periodId.Value);

        if (fromDate.HasValue)
            linesQuery = linesQuery.Where(l => l.AccountingEntry!.EntryDate >= fromDate.Value);

        if (toDate.HasValue)
            linesQuery = linesQuery.Where(l => l.AccountingEntry!.EntryDate <= toDate.Value);

        var result = await linesQuery
            .GroupBy(l => new { l.AccountingAccountId, l.AccountingAccount!.Code, l.AccountingAccount.Name, l.AccountingAccount.Nature, l.AccountingAccount.Category })
            .Select(g => new TrialBalanceItemDto
            {
                AccountCode = g.Key.Code,
                AccountName = g.Key.Name,
                Nature = g.Key.Nature.ToString(),
                Category = g.Key.Category.ToString(),
                TotalDebit = g.Sum(l => l.Debit),
                TotalCredit = g.Sum(l => l.Credit),
                Balance = g.Key.Nature == AccountingAccountNature.Debit
                    ? g.Sum(l => l.Debit) - g.Sum(l => l.Credit)
                    : g.Sum(l => l.Credit) - g.Sum(l => l.Debit)
            })
            .OrderBy(r => r.AccountCode)
            .ToListAsync();

        return result;
    }

    public async Task<List<GeneralLedgerEntryDto>> GetGeneralLedgerAsync(
        string tenantId, Guid accountId, DateTime? fromDate = null, DateTime? toDate = null)
    {
        var linesQuery = _context.EntryLines
            .Where(l => l.AccountingAccountId == accountId
                     && l.AccountingEntry!.TenantId == tenantId
                     && l.AccountingEntry.Status == EntryStatus.Final)
            .AsQueryable();

        if (fromDate.HasValue)
            linesQuery = linesQuery.Where(l => l.AccountingEntry!.EntryDate >= fromDate.Value);

        if (toDate.HasValue)
            linesQuery = linesQuery.Where(l => l.AccountingEntry!.EntryDate <= toDate.Value);

        var lines = await linesQuery
            .OrderBy(l => l.AccountingEntry!.EntryDate)
            .ThenBy(l => l.AccountingEntry!.EntryNumber)
            .Select(l => new
            {
                l.AccountingEntry!.EntryDate,
                l.AccountingEntry.EntryNumber,
                l.AccountingEntry.Description,
                l.AccountingEntry.ExternalReference,
                l.Debit,
                l.Credit,
                l.AccountingEntry.AccountingPeriodId
            })
            .ToListAsync();

        var result = new List<GeneralLedgerEntryDto>();
        decimal runningBalance = 0;

        // Determinar saldo inicial según naturaleza de la cuenta
        var account = await _context.AccountingAccounts.FindAsync(accountId);
        if (account == null) return result;

        foreach (var line in lines)
        {
            if (account.Nature == AccountingAccountNature.Debit)
                runningBalance += line.Debit - line.Credit;
            else
                runningBalance += line.Credit - line.Debit;

            result.Add(new GeneralLedgerEntryDto
            {
                Date = line.EntryDate,
                EntryNumber = line.EntryNumber,
                Description = line.Description,
                ExternalReference = line.ExternalReference,
                Debit = line.Debit,
                Credit = line.Credit,
                RunningBalance = runningBalance
            });
        }

        return result;
    }

    public async Task<List<IncomeStatementItemDto>> GetIncomeStatementAsync(string tenantId, Guid? periodId = null)
    {
        var linesQuery = _context.EntryLines
            .Where(l => l.AccountingEntry!.TenantId == tenantId
                     && l.AccountingEntry.Status == EntryStatus.Final
                     && l.AccountingAccount!.Category == AccountingAccountCategory.Income)
            .AsQueryable();

        if (periodId.HasValue)
            linesQuery = linesQuery.Where(l => l.AccountingEntry!.AccountingPeriodId == periodId.Value);

        var incomeData = await linesQuery
            .GroupBy(l => new { l.AccountingAccountId, l.AccountingAccount!.Code, l.AccountingAccount.Name })
            .Select(g => new IncomeStatementItemDto
            {
                AccountCode = g.Key.Code,
                AccountName = g.Key.Name,
                Balance = g.Sum(l => l.Credit) - g.Sum(l => l.Debit)
            })
            .OrderBy(r => r.AccountCode)
            .ToListAsync();

        return incomeData;
    }

    public async Task<List<BalanceSheetItemDto>> GetBalanceSheetAsync(string tenantId, Guid? periodId = null)
    {
        var linesQuery = _context.EntryLines
            .Where(l => l.AccountingEntry!.TenantId == tenantId
                     && l.AccountingEntry.Status == EntryStatus.Final
                     && (l.AccountingAccount!.Category == AccountingAccountCategory.Asset
                      || l.AccountingAccount.Category == AccountingAccountCategory.Liability
                      || l.AccountingAccount.Category == AccountingAccountCategory.Equity))
            .AsQueryable();

        if (periodId.HasValue)
            linesQuery = linesQuery.Where(l => l.AccountingEntry!.AccountingPeriodId == periodId.Value);

        var balanceData = await linesQuery
            .GroupBy(l => new { l.AccountingAccountId, l.AccountingAccount!.Code, l.AccountingAccount.Name, l.AccountingAccount.Nature })
            .Select(g => new BalanceSheetItemDto
            {
                AccountCode = g.Key.Code,
                AccountName = g.Key.Name,
                Balance = g.Key.Nature == AccountingAccountNature.Debit
                    ? g.Sum(l => l.Debit) - g.Sum(l => l.Credit)
                    : g.Sum(l => l.Credit) - g.Sum(l => l.Debit)
            })
            .OrderBy(r => r.AccountCode)
            .ToListAsync();

        return balanceData;
    }

    public async Task<List<ComparativeIncomeStatementItemDto>> GetComparativeIncomeStatementAsync(
        string tenantId, Guid currentPeriodId, Guid previousPeriodId)
    {
        var current = await GetIncomeStatementAsync(tenantId, currentPeriodId);
        var previous = await GetIncomeStatementAsync(tenantId, previousPeriodId);

        var prevDict = previous.ToDictionary(p => p.AccountCode);
        return current.Select(c => new ComparativeIncomeStatementItemDto
        {
            AccountCode = c.AccountCode,
            AccountName = c.AccountName,
            CurrentBalance = c.Balance,
            PreviousBalance = prevDict.TryGetValue(c.AccountCode, out var p) ? p.Balance : 0m
        }).ToList();
    }

    public async Task<List<ComparativeBalanceSheetItemDto>> GetComparativeBalanceSheetAsync(
        string tenantId, Guid currentPeriodId, Guid previousPeriodId)
    {
        var current = await GetBalanceSheetAsync(tenantId, currentPeriodId);
        var previous = await GetBalanceSheetAsync(tenantId, previousPeriodId);

        var prevDict = previous.ToDictionary(p => p.AccountCode);
        return current.Select(c => new ComparativeBalanceSheetItemDto
        {
            AccountCode = c.AccountCode,
            AccountName = c.AccountName,
            CurrentBalance = c.Balance,
            PreviousBalance = prevDict.TryGetValue(c.AccountCode, out var p) ? p.Balance : 0m
        }).ToList();
    }
}
