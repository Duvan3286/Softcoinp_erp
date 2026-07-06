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

public class ExpenseService
{
    private readonly ApplicationDbContext _context;

    public ExpenseService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ExecutedExpenseDto> RecordExpenseAsync(string tenantId, RecordExpenseRequestDto request, string userId)
    {
        var expenseItem = await _context.ExpenseItems
            .Include(e => e.Budget)
            .FirstOrDefaultAsync(e => e.Id == request.ExpenseItemId);

        if (expenseItem == null)
        {
            throw new KeyNotFoundException("El rubro de gasto especificado no existe.");
        }

        if (expenseItem.Budget == null || expenseItem.Budget.TenantId != tenantId)
        {
            throw new InvalidOperationException("El rubro de gasto no pertenece al conjunto actual.");
        }

        if (expenseItem.Budget.Status != BudgetStatus.Approved)
        {
            throw new InvalidOperationException("Solo se pueden registrar gastos contra un presupuesto aprobado.");
        }

        if (request.Amount <= 0)
        {
            throw new ArgumentException("El valor del gasto debe ser mayor a cero.");
        }

        var startDate = new DateTime(expenseItem.Budget.FiscalYear, 1, 1);
        var endDate = new DateTime(expenseItem.Budget.FiscalYear, 12, 31, 23, 59, 59);

        var executedTotal = await _context.ExecutedExpenses
            .Where(e => e.ExpenseItemId == expenseItem.Id && e.ExpenseDate >= startDate && e.ExpenseDate <= endDate)
            .SumAsync(e => e.Amount);

        var projectedTotal = executedTotal + request.Amount;
        if (projectedTotal > expenseItem.AnnualValue)
        {
            throw new InvalidOperationException(
                $"El gasto supera el presupuesto disponible para '{expenseItem.Name}'. " +
                $"Presupuesto: {expenseItem.AnnualValue:C2}, Ejecutado: {executedTotal:C2}, " +
                $"Nuevo gasto: {request.Amount:C2}. Debe aprobarse una modificacion presupuestal.");
        }

        var needsCouncil = false;
        if (expenseItem.RequiresCouncilApproval && expenseItem.ApprovalThreshold > 0 && request.Amount > expenseItem.ApprovalThreshold)
        {
            needsCouncil = true;
        }

        if (expenseItem.IsContingencyFund && string.IsNullOrWhiteSpace(request.InvoiceReference))
        {
            throw new InvalidOperationException("Los gastos del fondo de imprevistos requieren un comprobante de referencia.");
        }

        var expense = new ExecutedExpense
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            ExpenseItemId = expenseItem.Id,
            Description = request.Description,
            Amount = Math.Round(request.Amount, 2),
            ExpenseDate = request.ExpenseDate,
            ProviderId = request.ProviderId,
            InvoiceReference = request.InvoiceReference,
            CouncilApproved = needsCouncil,
            CreatedByUserId = userId
        };

        if (needsCouncil)
        {
            expense.CouncilApproved = false;
        }

        _context.ExecutedExpenses.Add(expense);
        await _context.SaveChangesAsync();

        return await GetExpenseAsync(tenantId, expense.Id)
            ?? throw new InvalidOperationException("Error al registrar el gasto.");
    }

    public async Task<ExecutedExpenseDto?> GetExpenseAsync(string tenantId, Guid expenseId)
    {
        return await _context.ExecutedExpenses
            .Include(e => e.ExpenseItem)
            .Include(e => e.Provider)
            .Where(e => e.Id == expenseId && e.TenantId == tenantId)
            .Select(e => new ExecutedExpenseDto
            {
                Id = e.Id,
                ExpenseItemId = e.ExpenseItemId,
                ExpenseItemName = e.ExpenseItem != null ? e.ExpenseItem.Name : string.Empty,
                Description = e.Description,
                Amount = e.Amount,
                ExpenseDate = e.ExpenseDate,
                ProviderId = e.ProviderId,
                ProviderName = e.Provider != null ? e.Provider.BusinessName : string.Empty,
                InvoiceReference = e.InvoiceReference,
                CouncilApproved = e.CouncilApproved
            })
            .FirstOrDefaultAsync();
    }

    public async Task<List<ExecutedExpenseDto>> GetExpensesAsync(string tenantId, Guid? expenseItemId = null, DateTime? fromDate = null, DateTime? toDate = null)
    {
        var query = _context.ExecutedExpenses
            .Include(e => e.ExpenseItem)
            .Include(e => e.Provider)
            .Where(e => e.TenantId == tenantId)
            .AsQueryable();

        if (expenseItemId.HasValue)
        {
            query = query.Where(e => e.ExpenseItemId == expenseItemId.Value);
        }

        if (fromDate.HasValue)
        {
            query = query.Where(e => e.ExpenseDate >= fromDate.Value);
        }

        if (toDate.HasValue)
        {
            query = query.Where(e => e.ExpenseDate <= toDate.Value);
        }

        return await query
            .OrderByDescending(e => e.ExpenseDate)
            .Select(e => new ExecutedExpenseDto
            {
                Id = e.Id,
                ExpenseItemId = e.ExpenseItemId,
                ExpenseItemName = e.ExpenseItem != null ? e.ExpenseItem.Name : string.Empty,
                Description = e.Description,
                Amount = e.Amount,
                ExpenseDate = e.ExpenseDate,
                ProviderId = e.ProviderId,
                ProviderName = e.Provider != null ? e.Provider.BusinessName : string.Empty,
                InvoiceReference = e.InvoiceReference,
                CouncilApproved = e.CouncilApproved
            })
            .ToListAsync();
    }

    public async Task<BudgetModificationDto> CreateModificationAsync(
        string tenantId, CreateModificationRequestDto request, string userId)
    {
        if (request.Amount <= 0)
        {
            throw new ArgumentException("El valor de la modificacion debe ser mayor a cero.");
        }

        if (string.IsNullOrWhiteSpace(request.Justification))
        {
            throw new ArgumentException("La justificacion es obligatoria.");
        }

        if (string.IsNullOrWhiteSpace(request.MeetingActNumber))
        {
            throw new ArgumentException("El numero de acta aprobatoria es obligatorio.");
        }

        var budget = await _context.Budgets
            .Include(b => b.ExpenseItems)
            .Include(b => b.IncomeItems)
            .FirstOrDefaultAsync(b => b.Id == request.BudgetId && b.TenantId == tenantId);

        if (budget == null)
        {
            throw new KeyNotFoundException("Presupuesto no encontrado.");
        }

        if (budget.Status != BudgetStatus.Approved)
        {
            throw new InvalidOperationException("Solo se pueden modificar presupuestos aprobados.");
        }

        if (!Enum.TryParse<ModificationType>(request.ModificationType, true, out var modType))
        {
            throw new ArgumentException("Tipo de modificacion invalido (Increase o Reduction).");
        }

        if (!Enum.TryParse<ApprovalType>(request.ApprovalType, true, out var approvalType))
        {
            throw new ArgumentException("Tipo de aprobacion invalido (Council o Assembly).");
        }

        ExpenseItem? expenseItem = null;
        IncomeItem? incomeItem = null;
        decimal previousValue = 0;

        if (request.ExpenseItemId.HasValue)
        {
            expenseItem = budget.ExpenseItems.FirstOrDefault(e => e.Id == request.ExpenseItemId.Value);
            if (expenseItem == null)
            {
                throw new KeyNotFoundException("Rubro de gasto no encontrado en este presupuesto.");
            }
            previousValue = expenseItem.AnnualValue;

            expenseItem.AnnualValue = modType == ModificationType.Increase
                ? expenseItem.AnnualValue + request.Amount
                : expenseItem.AnnualValue - request.Amount;

            if (expenseItem.AnnualValue < 0)
            {
                throw new InvalidOperationException("El valor del rubro no puede ser negativo.");
            }
        }
        else if (request.IncomeItemId.HasValue)
        {
            incomeItem = budget.IncomeItems.FirstOrDefault(i => i.Id == request.IncomeItemId.Value);
            if (incomeItem == null)
            {
                throw new KeyNotFoundException("Rubro de ingreso no encontrado en este presupuesto.");
            }
            previousValue = incomeItem.AnnualValue;

            incomeItem.AnnualValue = modType == ModificationType.Increase
                ? incomeItem.AnnualValue + request.Amount
                : incomeItem.AnnualValue - request.Amount;

            if (incomeItem.AnnualValue < 0)
            {
                throw new InvalidOperationException("El valor del rubro no puede ser negativo.");
            }
        }
        else
        {
            throw new ArgumentException("Debe especificar un rubro de gasto o ingreso a modificar.");
        }

        var mod = new BudgetModification
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            BudgetId = budget.Id,
            ExpenseItemId = request.ExpenseItemId,
            IncomeItemId = request.IncomeItemId,
            ModificationType = modType,
            Amount = Math.Round(request.Amount, 2),
            PreviousValue = Math.Round(previousValue, 2),
            NewValue = Math.Round(previousValue + (modType == ModificationType.Increase ? request.Amount : -request.Amount), 2),
            Justification = request.Justification,
            ApprovalType = approvalType,
            MeetingActNumber = request.MeetingActNumber,
            ApprovalDate = request.ApprovalDate,
            CreatedByUserId = userId
        };

        _context.BudgetModifications.Add(mod);
        await _context.SaveChangesAsync();

        return new BudgetModificationDto
        {
            Id = mod.Id,
            BudgetId = mod.BudgetId,
            ExpenseItemId = mod.ExpenseItemId,
            ExpenseItemName = expenseItem?.Name ?? string.Empty,
            IncomeItemId = mod.IncomeItemId,
            IncomeItemName = incomeItem?.Name ?? string.Empty,
            ModificationType = mod.ModificationType.ToString(),
            Amount = mod.Amount,
            PreviousValue = mod.PreviousValue,
            NewValue = mod.NewValue,
            Justification = mod.Justification,
            ApprovalType = mod.ApprovalType.ToString(),
            MeetingActNumber = mod.MeetingActNumber,
            ApprovalDate = mod.ApprovalDate
        };
    }

    public async Task<List<BudgetModificationDto>> GetModificationsAsync(string tenantId, Guid budgetId)
    {
        return await _context.BudgetModifications
            .Include(m => m.ExpenseItem)
            .Include(m => m.IncomeItem)
            .Where(m => m.TenantId == tenantId && m.BudgetId == budgetId)
            .OrderByDescending(m => m.CreatedAt)
            .Select(m => new BudgetModificationDto
            {
                Id = m.Id,
                BudgetId = m.BudgetId,
                ExpenseItemId = m.ExpenseItemId,
                ExpenseItemName = m.ExpenseItem != null ? m.ExpenseItem.Name : string.Empty,
                IncomeItemId = m.IncomeItemId,
                IncomeItemName = m.IncomeItem != null ? m.IncomeItem.Name : string.Empty,
                ModificationType = m.ModificationType.ToString(),
                Amount = m.Amount,
                PreviousValue = m.PreviousValue,
                NewValue = m.NewValue,
                Justification = m.Justification,
                ApprovalType = m.ApprovalType.ToString(),
                MeetingActNumber = m.MeetingActNumber,
                ApprovalDate = m.ApprovalDate
            })
            .ToListAsync();
    }

    public async Task<ContingencyFundUsageDto> RecordContingencyFundUsageAsync(
        string tenantId, RecordContingencyFundUsageRequestDto request, string userId)
    {
        if (request.Amount <= 0)
        {
            throw new ArgumentException("El monto debe ser mayor a cero.");
        }

        if (string.IsNullOrWhiteSpace(request.CouncilApprovalActNumber))
        {
            throw new ArgumentException("La aprobacion del consejo (numero de acta) es obligatoria para usar el fondo de imprevistos.");
        }

        var budget = await _context.Budgets
            .Include(b => b.ExpenseItems)
            .FirstOrDefaultAsync(b => b.Id == request.BudgetId && b.TenantId == tenantId);

        if (budget == null)
        {
            throw new KeyNotFoundException("Presupuesto no encontrado.");
        }

        if (budget.Status != BudgetStatus.Approved)
        {
            throw new InvalidOperationException("El presupuesto debe estar aprobado.");
        }

        var contingencyItem = budget.ExpenseItems.FirstOrDefault(e => e.IsContingencyFund);
        if (contingencyItem == null)
        {
            throw new InvalidOperationException("El presupuesto no tiene configurado un rubro de fondo de imprevistos.");
        }

        var totalContributed = contingencyItem.AnnualValue;
        var totalUsed = await _context.ContingencyFundUsages
            .Where(u => u.TenantId == tenantId && u.BudgetId == request.BudgetId)
            .SumAsync(u => u.Amount);

        var available = totalContributed - totalUsed;
        if (request.Amount > available)
        {
            throw new InvalidOperationException(
                $"Fondos insuficientes en el fondo de imprevistos. Disponible: {available:C2}, Solicitado: {request.Amount:C2}.");
        }

        var usage = new ContingencyFundUsage
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            BudgetId = request.BudgetId,
            Justification = request.Justification,
            Amount = Math.Round(request.Amount, 2),
            CouncilApprovalActNumber = request.CouncilApprovalActNumber,
            ExecutedExpenseId = request.ExecutedExpenseId,
            CreatedByUserId = userId
        };

        _context.ContingencyFundUsages.Add(usage);
        await _context.SaveChangesAsync();

        return new ContingencyFundUsageDto
        {
            Id = usage.Id,
            Justification = usage.Justification,
            Amount = usage.Amount,
            CouncilApprovalActNumber = usage.CouncilApprovalActNumber,
            CreatedAt = usage.CreatedAt
        };
    }
}
