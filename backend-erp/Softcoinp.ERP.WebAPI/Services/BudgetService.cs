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

public class BudgetService
{
    private readonly ApplicationDbContext _context;

    public BudgetService(ApplicationDbContext context)
    {
        _context = context;
    }

    private static ExpenseCategory ParseExpenseCategory(string category)
    {
        var parseSucceeded = Enum.TryParse<ExpenseCategory>(category, true, out var parsedCategory);
        if (!parseSucceeded)
        {
            throw new ArgumentException($"Categoria de gasto invalida: '{category}'. Los valores permitidos son 'Fixed' y 'Variable'.");
        }

        return parsedCategory;
    }

    public async Task<BudgetDetailDto> CreateBudgetAsync(
        string tenantId,
        CreateBudgetRequestDto request,
        string userId)
    {
        var activeBudget = await _context.Budgets
            .FirstOrDefaultAsync(b => b.TenantId == tenantId && b.FiscalYear == request.FiscalYear && b.Status == BudgetStatus.Approved);

        if (activeBudget != null)
        {
            throw new InvalidOperationException($"Ya existe un presupuesto aprobado para el ano fiscal {request.FiscalYear}.");
        }

        var existingDraft = await _context.Budgets
            .FirstOrDefaultAsync(b => b.TenantId == tenantId && b.FiscalYear == request.FiscalYear && b.Status == BudgetStatus.Draft);

        if (existingDraft != null)
        {
            _context.Budgets.Remove(existingDraft);
        }

        var budget = new Budget
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            FiscalYear = request.FiscalYear,
            MeetingActNumber = request.MeetingActNumber,
            ApprovalDate = request.ApprovalDate,
            Status = BudgetStatus.Draft,
            Observations = request.Observations,
            CreatedByUserId = userId
        };

        if (request.CopyFromPrevious)
        {
            var previousYear = request.FiscalYear - 1;
            var previousBudget = await _context.Budgets
                .Include(b => b.IncomeItems)
                .Include(b => b.ExpenseItems)
                .FirstOrDefaultAsync(b => b.TenantId == tenantId && b.FiscalYear == previousYear && b.Status == BudgetStatus.Approved);

            if (previousBudget == null)
            {
                throw new InvalidOperationException($"No se encontro un presupuesto aprobado para el periodo anterior ({previousYear}).");
            }

            var adjustment = request.GlobalPercentageAdjustment ?? 0m;

            foreach (var prev in previousBudget.IncomeItems)
            {
                var adjustedValue = Math.Round(prev.AnnualValue * (1m + adjustment / 100m), 2);
                budget.IncomeItems.Add(new IncomeItem
                {
                    Id = Guid.NewGuid(),
                    BudgetId = budget.Id,
                    Name = prev.Name,
                    Description = prev.Description,
                    AnnualValue = adjustedValue
                });
            }

            foreach (var prev in previousBudget.ExpenseItems)
            {
                var adjustedValue = Math.Round(prev.AnnualValue * (1m + adjustment / 100m), 2);
                budget.ExpenseItems.Add(new ExpenseItem
                {
                    Id = Guid.NewGuid(),
                    BudgetId = budget.Id,
                    Name = prev.Name,
                    Description = prev.Description,
                    Category = prev.Category,
                    AnnualValue = adjustedValue,
                    IsContingencyFund = prev.IsContingencyFund,
                    ContingencyPercentage = prev.ContingencyPercentage,
                    RequiresCouncilApproval = prev.RequiresCouncilApproval,
                    ApprovalThreshold = prev.ApprovalThreshold
                });
            }
        }
        else
        {
            if (request.IncomeItems != null)
            {
                foreach (var item in request.IncomeItems)
                {
                    budget.IncomeItems.Add(new IncomeItem
                    {
                        Id = Guid.NewGuid(),
                        BudgetId = budget.Id,
                        Name = item.Name,
                        Description = item.Description,
                        AnnualValue = Math.Round(item.AnnualValue, 2)
                    });
                }
            }

            if (request.ExpenseItems != null)
            {
                foreach (var item in request.ExpenseItems)
                {
                    budget.ExpenseItems.Add(new ExpenseItem
                    {
                        Id = Guid.NewGuid(),
                        BudgetId = budget.Id,
                        Name = item.Name,
                        Description = item.Description,
                        Category = ParseExpenseCategory(item.Category),
                        AnnualValue = Math.Round(item.AnnualValue, 2),
                        IsContingencyFund = item.IsContingencyFund,
                        ContingencyPercentage = item.ContingencyPercentage,
                        RequiresCouncilApproval = item.RequiresCouncilApproval,
                        ApprovalThreshold = item.ApprovalThreshold
                    });
                }
            }
        }

        _context.Budgets.Add(budget);
        await _context.SaveChangesAsync();

        return await GetBudgetDetailAsync(tenantId, budget.Id)
            ?? throw new InvalidOperationException("Error al crear el presupuesto.");
    }

    public async Task<List<BudgetSummaryDto>> GetBudgetsAsync(string tenantId, int? year = null)
    {
        var query = _context.Budgets
            .Include(b => b.IncomeItems)
            .Include(b => b.ExpenseItems)
            .Where(b => b.TenantId == tenantId);

        if (year.HasValue)
        {
            query = query.Where(b => b.FiscalYear == year.Value);
        }

        return await query
            .OrderByDescending(b => b.FiscalYear)
            .Select(b => new BudgetSummaryDto
            {
                Id = b.Id,
                FiscalYear = b.FiscalYear,
                ApprovalDate = b.ApprovalDate,
                MeetingActNumber = b.MeetingActNumber,
                Status = b.Status.ToString(),
                Observations = b.Observations,
                IncomeItemsCount = b.IncomeItems.Count,
                ExpenseItemsCount = b.ExpenseItems.Count,
                TotalIncome = b.IncomeItems.Sum(i => i.AnnualValue),
                TotalExpense = b.ExpenseItems.Sum(e => e.AnnualValue),
                CreatedByUserId = b.CreatedByUserId
            })
            .ToListAsync();
    }

    public async Task<BudgetDetailDto?> GetBudgetDetailAsync(string tenantId, Guid budgetId)
    {
        return await _context.Budgets
            .Include(b => b.IncomeItems)
            .Include(b => b.ExpenseItems)
            .Where(b => b.Id == budgetId && b.TenantId == tenantId)
            .Select(b => new BudgetDetailDto
            {
                Id = b.Id,
                FiscalYear = b.FiscalYear,
                ApprovalDate = b.ApprovalDate,
                MeetingActNumber = b.MeetingActNumber,
                Status = b.Status.ToString(),
                Observations = b.Observations,
                IncomeItems = b.IncomeItems.Select(i => new IncomeItemDto
                {
                    Id = i.Id,
                    Name = i.Name,
                    Description = i.Description,
                    AnnualValue = i.AnnualValue
                }).ToList(),
                ExpenseItems = b.ExpenseItems.Select(e => new ExpenseItemDto
                {
                    Id = e.Id,
                    Name = e.Name,
                    Description = e.Description,
                    Category = e.Category.ToString(),
                    AnnualValue = e.AnnualValue,
                    IsContingencyFund = e.IsContingencyFund,
                    ContingencyPercentage = e.ContingencyPercentage,
                    RequiresCouncilApproval = e.RequiresCouncilApproval,
                    ApprovalThreshold = e.ApprovalThreshold
                }).ToList()
            })
            .FirstOrDefaultAsync();
    }

    public async Task<BudgetDetailDto> UpdateDraftBudgetAsync(
        string tenantId,
        Guid budgetId,
        List<CreateIncomeItemDto> incomeItems,
        List<CreateExpenseItemDto> expenseItems)
    {
        var budget = await _context.Budgets
            .Include(b => b.IncomeItems)
            .Include(b => b.ExpenseItems)
            .FirstOrDefaultAsync(b => b.Id == budgetId && b.TenantId == tenantId);

        if (budget == null)
        {
            throw new KeyNotFoundException("Presupuesto no encontrado.");
        }

        if (budget.Status != BudgetStatus.Draft)
        {
            throw new InvalidOperationException("Un presupuesto aprobado no puede editarse. Use modificaciones formales.");
        }

        _context.RemoveRange(budget.IncomeItems);
        _context.RemoveRange(budget.ExpenseItems);

        var newIncomeItems = new List<IncomeItem>();
        foreach (var item in incomeItems)
        {
            newIncomeItems.Add(new IncomeItem
            {
                Id = Guid.NewGuid(),
                BudgetId = budget.Id,
                Name = item.Name,
                Description = item.Description,
                AnnualValue = Math.Round(item.AnnualValue, 2)
            });
        }

        var newExpenseItems = new List<ExpenseItem>();
        foreach (var item in expenseItems)
        {
            newExpenseItems.Add(new ExpenseItem
            {
                Id = Guid.NewGuid(),
                BudgetId = budget.Id,
                Name = item.Name,
                Description = item.Description,
                Category = ParseExpenseCategory(item.Category),
                AnnualValue = Math.Round(item.AnnualValue, 2),
                IsContingencyFund = item.IsContingencyFund,
                ContingencyPercentage = item.ContingencyPercentage,
                RequiresCouncilApproval = item.RequiresCouncilApproval,
                ApprovalThreshold = item.ApprovalThreshold
            });
        }

        await _context.IncomeItems.AddRangeAsync(newIncomeItems);
        await _context.ExpenseItems.AddRangeAsync(newExpenseItems);
        await _context.SaveChangesAsync();

        return (await GetBudgetDetailAsync(tenantId, budgetId))!;
    }

    public async Task<BudgetDetailDto> ApproveBudgetAsync(
        string tenantId,
        Guid budgetId,
        ApproveBudgetRequestDto request)
    {
        if (string.IsNullOrWhiteSpace(request.MeetingActNumber))
        {
            throw new ArgumentException("El numero de acta de asamblea es obligatorio para aprobar el presupuesto.");
        }

        var budget = await _context.Budgets
            .Include(b => b.IncomeItems)
            .Include(b => b.ExpenseItems)
            .FirstOrDefaultAsync(b => b.Id == budgetId && b.TenantId == tenantId);

        if (budget == null)
        {
            throw new KeyNotFoundException("Presupuesto no encontrado.");
        }

        if (budget.Status == BudgetStatus.Approved)
        {
            return (await GetBudgetDetailAsync(tenantId, budgetId))!;
        }

        var conflict = await _context.Budgets
            .AnyAsync(b => b.TenantId == tenantId && b.FiscalYear == budget.FiscalYear && b.Status == BudgetStatus.Approved);

        if (conflict)
        {
            throw new InvalidOperationException($"Ya existe un presupuesto aprobado para el ano fiscal {budget.FiscalYear}.");
        }

        var contingencyFundItem = budget.ExpenseItems.FirstOrDefault(e => e.IsContingencyFund);
        if (contingencyFundItem == null)
        {
            throw new InvalidOperationException("El presupuesto debe incluir un rubro de fondo de imprevistos (Ley 675 de 2001).");
        }

        var totalIncome = budget.IncomeItems.Sum(i => i.AnnualValue);
        if (totalIncome <= 0)
        {
            throw new InvalidOperationException("El presupuesto debe tener al menos un ingreso con valor positivo.");
        }

        if (contingencyFundItem.ContingencyPercentage > 0)
        {
            var minContingency = totalIncome * (contingencyFundItem.ContingencyPercentage / 100m);
            if (contingencyFundItem.AnnualValue < minContingency)
            {
                throw new InvalidOperationException(
                    $"El valor del fondo de imprevistos ({contingencyFundItem.AnnualValue:C2}) es inferior al " +
                    $"{contingencyFundItem.ContingencyPercentage}% del total de ingresos ({minContingency:C2}).");
            }
        }

        budget.MeetingActNumber = request.MeetingActNumber;
        budget.ApprovalDate = request.ApprovalDate;
        budget.Status = BudgetStatus.Approved;

        await _context.SaveChangesAsync();

        return (await GetBudgetDetailAsync(tenantId, budgetId))!;
    }

    public async Task<BudgetDetailDto> GenerateNextPeriodBudgetAsync(string tenantId, Guid currentBudgetId)
    {
        var current = await _context.Budgets
            .Include(b => b.IncomeItems)
            .Include(b => b.ExpenseItems)
            .FirstOrDefaultAsync(b => b.Id == currentBudgetId && b.TenantId == tenantId);

        if (current == null)
        {
            throw new KeyNotFoundException("Presupuesto actual no encontrado.");
        }

        var nextYear = current.FiscalYear + 1;

        var request = new CreateBudgetRequestDto
        {
            FiscalYear = nextYear,
            CopyFromPrevious = true,
            Observations = $"Generado a partir del presupuesto {current.FiscalYear}"
        };

        return await CreateBudgetAsync(tenantId, request, "SYSTEM");
    }
}
