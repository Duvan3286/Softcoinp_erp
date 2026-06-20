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

public class JournalEntryService
{
    private readonly ApplicationDbContext _context;
    private readonly AccountingPeriodService _periodService;

    public JournalEntryService(ApplicationDbContext context, AccountingPeriodService periodService)
    {
        _context = context;
        _periodService = periodService;
    }

    public async Task<JournalEntryDto> CreateEntryAsync(string tenantId, CreateJournalEntryDto dto, string userId)
    {
        // Validar que débitos = créditos
        var totalDebit = dto.Lines.Sum(l => l.Debit);
        var totalCredit = dto.Lines.Sum(l => l.Credit);

        if (totalDebit != totalCredit)
        {
            throw new InvalidOperationException($"La suma de débitos ({totalDebit}) debe ser igual a la suma de créditos ({totalCredit}).");
        }

        if (totalDebit == 0)
        {
            throw new InvalidOperationException("El asiento debe tener al menos un valor de débito o crédito.");
        }

        // Validar que cada línea tenga solo débito o crédito (no ambos)
        foreach (var line in dto.Lines)
        {
            if (line.Debit > 0 && line.Credit > 0)
            {
                throw new InvalidOperationException("Cada línea del asiento debe ser solo débito o solo crédito, no ambos.");
            }
            if (line.Debit == 0 && line.Credit == 0)
            {
                throw new InvalidOperationException("Cada línea del asiento debe tener un valor de débito o crédito.");
            }
        }

        // Validar cuentas contables
        var accountIds = dto.Lines.Select(l => l.AccountingAccountId).Distinct().ToList();
        var validAccounts = await _context.AccountingAccounts
            .Where(a => a.TenantId == tenantId && accountIds.Contains(a.Id) && a.IsGroup == false)
            .Select(a => a.Id)
            .ToListAsync();

        var invalidAccounts = accountIds.Except(validAccounts).ToList();
        if (invalidAccounts.Any())
        {
            throw new InvalidOperationException($"Las siguientes cuentas no existen, no pertenecen al tenant o son cuentas de grupo: {string.Join(", ", invalidAccounts)}");
        }

        // Validar que el período no esté cerrado
        if (dto.AccountingPeriodId.HasValue)
        {
            var period = await _context.AccountingPeriods
                .FirstOrDefaultAsync(p => p.Id == dto.AccountingPeriodId.Value && p.TenantId == tenantId);
            if (period == null)
            {
                throw new InvalidOperationException("El período contable especificado no existe.");
            }
            if (period.Status == AccountingPeriodStatus.Closed)
            {
                throw new InvalidOperationException($"El período {period.PeriodLabel} está cerrado. No se pueden crear asientos en períodos cerrados.");
            }
        }

        // Obtener número de asiento
        var entryNumber = await _periodService.GetNextEntryNumberAsync(tenantId, dto.AccountingPeriodId);

        // Crear asiento
        var entry = new AccountingEntry
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            AccountingPeriodId = null,
            EntryNumber = entryNumber,
            EntryType = dto.EntryType,
            Status = EntryStatus.Draft,
            EntryDate = dto.EntryDate,
            Description = dto.Description,
            ExternalReference = dto.ExternalReference ?? string.Empty,
            TotalDebit = totalDebit,
            TotalCredit = totalCredit,
            CreatedByUserId = userId
        };

        foreach (var lineDto in dto.Lines)
        {
            entry.Lines.Add(new EntryLine
            {
                AccountingEntryId = entry.Id,
                AccountingAccountId = lineDto.AccountingAccountId,
                ThirdPartyId = lineDto.ThirdPartyId,
                Debit = lineDto.Debit,
                Credit = lineDto.Credit
            });
        }

        _context.AccountingEntries.Add(entry);
        await _context.SaveChangesAsync();

        return await GetEntryAsync(tenantId, entry.Id)
            ?? throw new InvalidOperationException("Error al crear el asiento contable.");
    }

    public async Task<JournalEntryDto?> GetEntryAsync(string tenantId, Guid entryId)
    {
        return await _context.AccountingEntries
            .Where(e => e.Id == entryId && e.TenantId == tenantId)
            .Include(e => e.Lines)
                .ThenInclude(l => l.AccountingAccount)
            .Select(e => new JournalEntryDto
            {
                Id = e.Id,
                TenantId = e.TenantId,
                AccountingPeriodId = e.AccountingPeriodId,
                PeriodLabel = e.AccountingPeriod != null ? e.AccountingPeriod.PeriodLabel : null,
                EntryNumber = e.EntryNumber,
                EntryType = e.EntryType.ToString(),
                Status = e.Status.ToString(),
                EntryDate = e.EntryDate,
                Description = e.Description,
                ExternalReference = e.ExternalReference,
                TotalDebit = e.TotalDebit,
                TotalCredit = e.TotalCredit,
                CreatedByUserId = e.CreatedByUserId,
                CreatedAt = e.CreatedAt,
                Lines = e.Lines.Select(l => new JournalEntryLineDto
                {
                    Id = l.Id,
                    AccountingAccountId = l.AccountingAccountId,
                    AccountCode = l.AccountingAccount.Code,
                    AccountName = l.AccountingAccount.Name,
                    ThirdPartyId = l.ThirdPartyId,
                    Debit = l.Debit,
                    Credit = l.Credit
                }).ToList()
            })
            .FirstOrDefaultAsync();
    }

    public async Task<List<JournalEntryDto>> GetEntriesAsync(
        string tenantId,
        DateTime? fromDate = null,
        DateTime? toDate = null,
        Guid? periodId = null,
        EntryStatus? status = null,
        EntryType? entryType = null,
        int page = 1,
        int pageSize = 20)
    {
        var query = _context.AccountingEntries
            .Where(e => e.TenantId == tenantId)
            .AsQueryable();

        if (fromDate.HasValue)
            query = query.Where(e => e.EntryDate >= fromDate.Value);

        if (toDate.HasValue)
            query = query.Where(e => e.EntryDate <= toDate.Value);

        if (periodId.HasValue)
            query = query.Where(e => e.AccountingPeriodId == periodId.Value);

        if (status.HasValue)
            query = query.Where(e => e.Status == status.Value);

        if (entryType.HasValue)
            query = query.Where(e => e.EntryType == entryType.Value);

        return await query
            .OrderByDescending(e => e.EntryDate)
            .ThenByDescending(e => e.EntryNumber)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Include(e => e.Lines)
                .ThenInclude(l => l.AccountingAccount)
            .Select(e => new JournalEntryDto
            {
                Id = e.Id,
                TenantId = e.TenantId,
                AccountingPeriodId = e.AccountingPeriodId,
                PeriodLabel = e.AccountingPeriod != null ? e.AccountingPeriod.PeriodLabel : null,
                EntryNumber = e.EntryNumber,
                EntryType = e.EntryType.ToString(),
                Status = e.Status.ToString(),
                EntryDate = e.EntryDate,
                Description = e.Description,
                ExternalReference = e.ExternalReference,
                TotalDebit = e.TotalDebit,
                TotalCredit = e.TotalCredit,
                CreatedByUserId = e.CreatedByUserId,
                CreatedAt = e.CreatedAt,
                Lines = e.Lines.Select(l => new JournalEntryLineDto
                {
                    Id = l.Id,
                    AccountingAccountId = l.AccountingAccountId,
                    AccountCode = l.AccountingAccount.Code,
                    AccountName = l.AccountingAccount.Name,
                    ThirdPartyId = l.ThirdPartyId,
                    Debit = l.Debit,
                    Credit = l.Credit
                }).ToList()
            })
            .ToListAsync();
    }

    public async Task<JournalEntryDto> PostEntryAsync(string tenantId, Guid entryId)
    {
        var entry = await _context.AccountingEntries
            .Where(e => e.Id == entryId && e.TenantId == tenantId)
            .FirstOrDefaultAsync();

        if (entry == null)
        {
            throw new KeyNotFoundException("Asiento contable no encontrado.");
        }

        if (entry.Status != EntryStatus.Draft)
        {
            throw new InvalidOperationException($"El asiento no puede ser contabilizado porque su estado es {entry.Status}.");
        }

        entry.Status = EntryStatus.Final;
        await _context.SaveChangesAsync();

        return await GetEntryAsync(tenantId, entryId)
            ?? throw new InvalidOperationException("Error al obtener el asiento contable.");
    }

    public async Task<JournalEntryDto> ReverseEntryAsync(string tenantId, Guid entryId, string reason, string userId)
    {
        var originalEntry = await _context.AccountingEntries
            .Where(e => e.Id == entryId && e.TenantId == tenantId)
            .Include(e => e.Lines)
            .FirstOrDefaultAsync();

        if (originalEntry == null)
        {
            throw new KeyNotFoundException("Asiento contable original no encontrado.");
        }

        if (originalEntry.Status != EntryStatus.Final)
        {
            throw new InvalidOperationException("Solo se pueden revertir asientos en estado Final.");
        }

        if (originalEntry.EntryType == EntryType.Automatic)
        {
            throw new InvalidOperationException("No se pueden revertir asientos generados automáticamente por el sistema. Cree un asiento manual de ajuste en su lugar.");
        }

        // Verificar que no esté ya revertido
        var alreadyReversed = await _context.EntryReversals
            .AnyAsync(r => r.OriginalEntryId == entryId && r.TenantId == tenantId);

        if (alreadyReversed)
        {
            throw new InvalidOperationException("Este asiento ya fue revertido anteriormente.");
        }

        // Obtener número de asiento para la reversión
        var entryNumber = await _periodService.GetNextEntryNumberAsync(tenantId, null);

        // Crear asiento de reversión (débitos ↔ créditos)
        var reversalEntry = new AccountingEntry
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            EntryNumber = entryNumber,
            EntryType = EntryType.Automatic,
            Status = EntryStatus.Final,
            EntryDate = DateTime.UtcNow,
            Description = $"REVERSIÓN: {originalEntry.Description}",
            ExternalReference = $"REV-{originalEntry.EntryNumber}",
            TotalDebit = originalEntry.TotalCredit,
            TotalCredit = originalEntry.TotalDebit,
            CreatedByUserId = userId
        };

        foreach (var originalLine in originalEntry.Lines)
        {
            reversalEntry.Lines.Add(new EntryLine
            {
                AccountingEntryId = reversalEntry.Id,
                AccountingAccountId = originalLine.AccountingAccountId,
                ThirdPartyId = originalLine.ThirdPartyId,
                Debit = originalLine.Credit,
                Credit = originalLine.Debit
            });
        }

        _context.AccountingEntries.Add(reversalEntry);

        // Registrar la reversión
        var reversalRecord = new EntryReversal
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            OriginalEntryId = entryId,
            ReversalEntryId = reversalEntry.Id,
            Reason = reason,
            ReversedAt = DateTime.UtcNow,
            ReversedByUserId = userId
        };

        _context.EntryReversals.Add(reversalRecord);

        await _context.SaveChangesAsync();

        return await GetEntryAsync(tenantId, reversalEntry.Id)
            ?? throw new InvalidOperationException("Error al crear el asiento de reversión.");
    }
}
