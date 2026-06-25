using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Softcoinp.ERP.Domain.Entities;
using Softcoinp.ERP.Domain.Enums;
using Softcoinp.ERP.Infrastructure.Persistence;

namespace Softcoinp.ERP.WebAPI.Services;

public class BankReconciliationService
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<BankReconciliationService> _logger;

    public BankReconciliationService(ApplicationDbContext context, ILogger<BankReconciliationService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<List<BankReconciliation>> GetReconciliationsAsync(string tenantId, Guid? bankAccountId = null)
    {
        var query = _context.BankReconciliations
            .Include(r => r.BankAccount)
            .Where(r => r.TenantId == tenantId);
        if (bankAccountId.HasValue)
        {
            query = query.Where(r => r.BankAccountId == bankAccountId.Value);
        }
        return await query.OrderByDescending(r => r.FiscalYear).ThenByDescending(r => r.Month).ToListAsync();
    }

    public async Task<BankReconciliation?> GetReconciliationByIdAsync(string tenantId, Guid id)
    {
        return await _context.BankReconciliations
            .Include(r => r.BankAccount)
            .Include(r => r.Items)
            .FirstOrDefaultAsync(r => r.Id == id && r.TenantId == tenantId);
    }

    public async Task<BankReconciliation> StartReconciliationAsync(string tenantId, Guid bankAccountId, int year, int month, string userId)
    {
        var bankAccount = await _context.BankAccounts
            .FirstOrDefaultAsync(ba => ba.Id == bankAccountId && ba.TenantId == tenantId);
        if (bankAccount == null)
        {
            throw new KeyNotFoundException("Cuenta bancaria no encontrada.");
        }

        var existing = await _context.BankReconciliations
            .AnyAsync(r => r.TenantId == tenantId && r.BankAccountId == bankAccountId
                        && r.FiscalYear == year && r.Month == month);
        if (existing)
        {
            throw new InvalidOperationException("Ya existe una conciliación para este período.");
        }

        var reconciliation = new BankReconciliation
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            BankAccountId = bankAccountId,
            FiscalYear = year,
            Month = month,
            PeriodLabel = $"{year:D4}-{month:D2}",
            BookBalance = bankAccount.CurrentBalance,
            StatementBalance = 0m,
            Difference = bankAccount.CurrentBalance,
            Status = ReconciliationStatus.InProgress,
            CreatedByUserId = userId,
            CreatedAt = DateTime.UtcNow
        };

        _context.BankReconciliations.Add(reconciliation);
        await _context.SaveChangesAsync();
        return reconciliation;
    }

    public async Task<ReconciliationItem> AddItemAsync(string tenantId, Guid reconciliationId, ReconciliationItem item)
    {
        var reconciliation = await _context.BankReconciliations
            .FirstOrDefaultAsync(r => r.Id == reconciliationId && r.TenantId == tenantId);
        if (reconciliation == null)
        {
            throw new KeyNotFoundException("Conciliación no encontrada.");
        }
        if (reconciliation.Status == ReconciliationStatus.Completed)
        {
            throw new InvalidOperationException("No se pueden agregar items a una conciliación cerrada.");
        }

        item.Id = Guid.NewGuid();
        item.BankReconciliationId = reconciliationId;
        _context.ReconciliationItems.Add(item);

        reconciliation.StatementBalance += item.IsInStatement && !item.IsInBooks ? item.Amount : 0m;
        reconciliation.Difference = reconciliation.BookBalance - reconciliation.StatementBalance;
        await _context.SaveChangesAsync();
        return item;
    }

    public async Task RemoveItemAsync(string tenantId, Guid reconciliationId, Guid itemId)
    {
        var reconciliation = await _context.BankReconciliations
            .FirstOrDefaultAsync(r => r.Id == reconciliationId && r.TenantId == tenantId);
        if (reconciliation == null)
        {
            throw new KeyNotFoundException("Conciliación no encontrada.");
        }

        var item = await _context.ReconciliationItems
            .FirstOrDefaultAsync(i => i.Id == itemId && i.BankReconciliationId == reconciliationId);
        if (item == null)
        {
            throw new KeyNotFoundException("Item no encontrado.");
        }

        if (item.IsInStatement && !item.IsInBooks)
        {
            reconciliation.StatementBalance -= item.Amount;
        }
        reconciliation.Difference = reconciliation.BookBalance - reconciliation.StatementBalance;

        _context.ReconciliationItems.Remove(item);
        await _context.SaveChangesAsync();
    }

    public async Task<ReconciliationItem> ClearItemAsync(string tenantId, Guid reconciliationId, Guid itemId)
    {
        var reconciliation = await _context.BankReconciliations
            .FirstOrDefaultAsync(r => r.Id == reconciliationId && r.TenantId == tenantId);
        if (reconciliation == null)
        {
            throw new KeyNotFoundException("Conciliación no encontrada.");
        }
        if (reconciliation.Status == ReconciliationStatus.Completed)
        {
            throw new InvalidOperationException("No se pueden modificar items de una conciliación cerrada.");
        }

        var item = await _context.ReconciliationItems
            .FirstOrDefaultAsync(i => i.Id == itemId && i.BankReconciliationId == reconciliationId);
        if (item == null)
        {
            throw new KeyNotFoundException("Item no encontrado.");
        }

        item.IsCleared = true;
        await _context.SaveChangesAsync();
        return item;
    }

    public async Task<BankReconciliation> CompleteReconciliationAsync(string tenantId, Guid reconciliationId, string userId)
    {
        var reconciliation = await _context.BankReconciliations
            .Include(r => r.Items)
            .FirstOrDefaultAsync(r => r.Id == reconciliationId && r.TenantId == tenantId);
        if (reconciliation == null)
        {
            throw new KeyNotFoundException("Conciliación no encontrada.");
        }

        if (Math.Abs(reconciliation.Difference) > 0.01m)
        {
            throw new InvalidOperationException(
                $"La diferencia es de {reconciliation.Difference:C}. Debe ser cero para cerrar.");
        }

        var unclearedItems = reconciliation.Items.Any(i => !i.IsCleared);
        if (unclearedItems)
        {
            throw new InvalidOperationException(
                "Existen items no conciliados. Todos los items deben estar marcados como conciliados para cerrar.");
        }

        reconciliation.Status = ReconciliationStatus.Completed;
        reconciliation.CompletedAt = DateTime.UtcNow;
        reconciliation.CompletedByUserId = userId;
        await _context.SaveChangesAsync();

        return reconciliation;
    }
}
