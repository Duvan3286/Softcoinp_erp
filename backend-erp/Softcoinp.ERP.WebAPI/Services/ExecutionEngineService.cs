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

public class ExecutionEngineService
{
    private readonly ApplicationDbContext _context;

    public ExecutionEngineService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<BudgetExecutionDashboardDto> GetExecutionDashboardAsync(string tenantId, int fiscalYear)
    {
        var budget = await _context.Budgets
            .Include(b => b.IncomeItems)
            .Include(b => b.ExpenseItems)
            .FirstOrDefaultAsync(b => b.TenantId == tenantId && b.FiscalYear == fiscalYear && b.Status == BudgetStatus.Approved);

        if (budget == null)
        {
            throw new KeyNotFoundException($"No se encontro un presupuesto aprobado para el ano {fiscalYear}.");
        }

        var startDate = new DateTime(fiscalYear, 1, 1);
        var endDate = new DateTime(fiscalYear, 12, 31, 23, 59, 59);

        var executedExpenses = await _context.ExecutedExpenses
            .Where(e => e.TenantId == tenantId && e.ExpenseDate >= startDate && e.ExpenseDate <= endDate)
            .ToListAsync();

        var expenseExecution = executedExpenses
            .GroupBy(e => e.ExpenseItemId)
            .ToDictionary(g => g.Key, g => g.Sum(e => e.Amount));

        var totalApprovedIncome = budget.IncomeItems.Sum(i => i.AnnualValue);
        var totalApprovedExpense = budget.ExpenseItems.Sum(e => e.AnnualValue);
        var totalExecutedExpense = executedExpenses.Sum(e => e.Amount);
        var totalAvailable = totalApprovedExpense - totalExecutedExpense;
        var overallPercentage = totalApprovedExpense > 0
            ? Math.Round(totalExecutedExpense / totalApprovedExpense * 100m, 2)
            : 0m;

        var currentMonth = DateTime.Today.Month;
        var monthsElapsed = fiscalYear < DateTime.Today.Year ? 12
            : fiscalYear > DateTime.Today.Year ? 0
            : currentMonth;

        var items = new List<ExpenseExecutionItemDto>();
        var alerts = new List<BudgetAlertDto>();

        foreach (var expenseItem in budget.ExpenseItems)
        {
            var approved = expenseItem.AnnualValue;
            var monthlyValue = approved / 12m;
            var proportionalToDate = Math.Round(monthlyValue * monthsElapsed, 2);
            var executed = expenseExecution.TryGetValue(expenseItem.Id, out var val) ? val : 0m;
            var available = approved - executed;
            var percentage = approved > 0 ? Math.Round(executed / approved * 100m, 2) : 0m;

            var trafficLight = CalculateTrafficLight(percentage, approved, executed, monthsElapsed);

            items.Add(new ExpenseExecutionItemDto
            {
                Id = expenseItem.Id,
                Name = expenseItem.Name,
                Category = expenseItem.Category.ToString(),
                AnnualValue = approved,
                ProportionalToDate = proportionalToDate,
                ExecutedValue = executed,
                AvailableValue = available,
                ExecutionPercentage = percentage,
                TrafficLight = trafficLight,
                IsContingencyFund = expenseItem.IsContingencyFund,
                ContingencyPercentage = expenseItem.ContingencyPercentage,
                RequiresCouncilApproval = expenseItem.RequiresCouncilApproval,
                ApprovalThreshold = expenseItem.ApprovalThreshold
            });

            if (percentage >= 90m && percentage < 100m)
            {
                alerts.Add(new BudgetAlertDto
                {
                    ItemName = expenseItem.Name,
                    AnnualValue = approved,
                    ExecutedValue = executed,
                    ExecutionPercentage = percentage,
                    Message = $"Alerta: El rubro '{expenseItem.Name}' ha ejecutado el {percentage}% de su presupuesto.",
                    Severity = "Warning"
                });
            }

            if (percentage >= 100m)
            {
                alerts.Add(new BudgetAlertDto
                {
                    ItemName = expenseItem.Name,
                    AnnualValue = approved,
                    ExecutedValue = executed,
                    ExecutionPercentage = percentage,
                    Message = $"ALERTA CRITICA: El rubro '{expenseItem.Name}' ha superado el presupuesto ({percentage}%). Se bloquean nuevos gastos.",
                    Severity = "Critical"
                });
            }
        }

        return new BudgetExecutionDashboardDto
        {
            BudgetId = budget.Id,
            FiscalYear = budget.FiscalYear,
            Status = budget.Status.ToString(),
            TotalApprovedIncome = totalApprovedIncome,
            TotalApprovedExpense = totalApprovedExpense,
            TotalExecutedExpense = totalExecutedExpense,
            TotalAvailable = totalAvailable,
            OverallExecutionPercentage = overallPercentage,
            ExpenseItems = items,
            Alerts = alerts
        };
    }

    public async Task<ContingencyFundStatusDto> GetContingencyFundStatusAsync(string tenantId)
    {
        var currentYear = DateTime.Today.Year;

        var approvedBudget = await _context.Budgets
            .Include(b => b.IncomeItems)
            .Include(b => b.ExpenseItems)
            .FirstOrDefaultAsync(b => b.TenantId == tenantId && b.FiscalYear == currentYear && b.Status == BudgetStatus.Approved);

        if (approvedBudget == null)
        {
            var allBudgets = await _context.Budgets
                .Include(b => b.IncomeItems)
                .Include(b => b.ExpenseItems)
                .Where(b => b.TenantId == tenantId && b.Status == BudgetStatus.Approved)
                .OrderByDescending(b => b.FiscalYear)
                .ToListAsync();

            approvedBudget = allBudgets.FirstOrDefault();
        }

        decimal totalContributed = 0;
        decimal pct = 0;

        if (approvedBudget != null)
        {
            var contingencyItem = approvedBudget.ExpenseItems.FirstOrDefault(e => e.IsContingencyFund);
            if (contingencyItem != null)
            {
                totalContributed = contingencyItem.AnnualValue;
                pct = contingencyItem.ContingencyPercentage;
            }
        }

        var totalUsed = await _context.ContingencyFundUsages
            .Where(u => u.TenantId == tenantId)
            .SumAsync(u => u.Amount);

        var usages = await _context.ContingencyFundUsages
            .Where(u => u.TenantId == tenantId)
            .OrderByDescending(u => u.CreatedAt)
            .Select(u => new ContingencyFundUsageDto
            {
                Id = u.Id,
                Justification = u.Justification,
                Amount = u.Amount,
                CouncilApprovalActNumber = u.CouncilApprovalActNumber,
                CreatedAt = u.CreatedAt
            })
            .ToListAsync();

        return new ContingencyFundStatusDto
        {
            TenantId = tenantId,
            TotalContributed = totalContributed,
            TotalUsed = totalUsed,
            AvailableBalance = totalContributed - totalUsed,
            ContingencyPercentage = pct,
            Usages = usages
        };
    }

    private static string CalculateTrafficLight(decimal percentage, decimal approved, decimal executed, int monthsElapsed)
    {
        if (percentage >= 100m)
        {
            return "Red";
        }

        if (percentage >= 90m)
        {
            return "Yellow";
        }

        if (monthsElapsed > 0 && approved > 0)
        {
            var expectedMonthly = approved / 12m;
            var expectedToDate = expectedMonthly * monthsElapsed;
            if (executed > expectedToDate * 1.1m)
            {
                return "Yellow";
            }
        }

        return "Green";
    }
}
