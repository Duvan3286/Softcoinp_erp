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

public class BudgetExecutionService
{
    private readonly ApplicationDbContext _context;

    public BudgetExecutionService(ApplicationDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Calcula en tiempo real el estado de ejecución de cada cuenta del presupuesto activo del período.
    /// Incluye agregaciones jerárquicas para cuentas de agrupación y alertas automáticas.
    /// </summary>
    public async Task<BudgetExecutionReportDto> GetBudgetExecutionReportAsync(string tenantId, int fiscalPeriod)
    {
        // 1. Obtener el presupuesto del período (Activo o Borrador)
        var budget = await _context.Budgets
            .Include(b => b.BudgetDetails)
            .ThenInclude(d => d.AccountingAccount)
            .FirstOrDefaultAsync(b => b.TenantId == tenantId
                                   && b.FiscalPeriod == fiscalPeriod
                                   && (b.Status == BudgetStatus.Active || b.Status == BudgetStatus.Draft));

        if (budget == null)
        {
            throw new KeyNotFoundException($"No se encontró un presupuesto para el período fiscal {fiscalPeriod}.");
        }

        // 2. Obtener todos los movimientos presupuestales (adiciones/traslados)
        var movements = await _context.BudgetMovements
            .Where(m => m.TenantId == tenantId && m.BudgetId == budget.Id)
            .ToListAsync();

        // 3. Obtener todas las cuentas del catálogo del tenant
        var accounts = await _context.AccountingAccounts
            .Where(a => a.TenantId == tenantId)
            .ToListAsync();

        // 4. Obtener todas las transacciones contables del período fiscal
        var startDate = new DateTime(fiscalPeriod, 1, 1);
        var endDate = new DateTime(fiscalPeriod, 12, 31, 23, 59, 59);
        var journalEntries = await _context.EntryLines
            .Where(l => l.AccountingEntry!.TenantId == tenantId
                     && l.AccountingEntry.EntryDate >= startDate
                     && l.AccountingEntry.EntryDate <= endDate)
            .ToListAsync();

        // 5. Mapeo temporal para calcular valores de movimiento
        var reportItems = new List<BudgetExecutionItemDto>();
        var alertList = new List<BudgetAlertDto>();

        // Agrupar movimientos de diario por cuenta para optimizar sumas
        var entriesByAccount = journalEntries
            .GroupBy(e => e.AccountingAccountId)
            .ToDictionary(g => g.Key, g => g.ToList());

        // Agrupar movimientos presupuestales por cuenta origen y destino
        var additionsByAccount = movements
            .Where(m => m.MovementType == BudgetMovementType.Addition)
            .GroupBy(m => m.DestinationAccountId)
            .ToDictionary(g => g.Key, g => g.Sum(m => m.Amount));

        var transfersInByAccount = movements
            .Where(m => m.MovementType == BudgetMovementType.Transfer)
            .GroupBy(m => m.DestinationAccountId)
            .ToDictionary(g => g.Key, g => g.Sum(m => m.Amount));

        var transfersOutByAccount = movements
            .Where(m => m.MovementType == BudgetMovementType.Transfer && m.SourceAccountId.HasValue)
            .GroupBy(m => m.SourceAccountId!.Value)
            .ToDictionary(g => g.Key, g => g.Sum(m => m.Amount));

        var initialBudgetByAccount = budget.BudgetDetails
            .ToDictionary(d => d.AccountingAccountId, d => d.ApprovedValue);

        // 6. Calcular métricas para cuentas de movimiento
        var movementItems = new List<BudgetExecutionItemDto>();

        foreach (var acc in accounts.Where(a => a.IsGroup == false && a.Category == AccountingAccountCategory.Expense))
        {
            decimal approved = 0;
            if (initialBudgetByAccount.ContainsKey(acc.Id))
            {
                approved = initialBudgetByAccount[acc.Id];
            }

            decimal additions = 0;
            if (additionsByAccount.ContainsKey(acc.Id))
            {
                additions = additionsByAccount[acc.Id];
            }

            decimal txIn = 0;
            if (transfersInByAccount.ContainsKey(acc.Id))
            {
                txIn = transfersInByAccount[acc.Id];
            }

            decimal txOut = 0;
            if (transfersOutByAccount.ContainsKey(acc.Id))
            {
                txOut = transfersOutByAccount[acc.Id];
            }

            decimal adjustedBudget = approved + additions + txIn - txOut;

            decimal executed = 0;
            if (entriesByAccount.ContainsKey(acc.Id))
            {
                var accountEntries = entriesByAccount[acc.Id];
                var totalDebit = accountEntries.Sum(e => e.Debit);
                var totalCredit = accountEntries.Sum(e => e.Credit);

                if (acc.Nature == AccountingAccountNature.Debit)
                {
                    executed = totalDebit - totalCredit;
                }
                else
                {
                    executed = totalCredit - totalDebit;
                }
            }

            decimal available = adjustedBudget - executed;

            decimal percentage = 0;
            if (adjustedBudget != 0)
            {
                percentage = (executed / adjustedBudget) * 100m;
            }

            decimal closingProj = 0;
            if (DateTime.Today.Year == fiscalPeriod)
            {
                int elapsedMonths = DateTime.Today.Month;
                decimal average = executed / elapsedMonths;
                closingProj = average * 12m;
            }
            else if (DateTime.Today.Year > fiscalPeriod)
            {
                closingProj = executed;
            }
            else
            {
                closingProj = 0;
            }

            string trafficLight = "Green";
            int currentMonth = DateTime.Today.Month;
            decimal limitForMonth = (currentMonth / 12.0m) * 100m + 10m;

            if (percentage > 100m)
            {
                trafficLight = "Red";
            }
            else if (percentage > 80m && currentMonth < 10 && DateTime.Today.Year == fiscalPeriod)
            {
                trafficLight = "Yellow";
            }
            else if (percentage > limitForMonth && DateTime.Today.Year == fiscalPeriod)
            {
                trafficLight = "Yellow";
            }
            else
            {
                trafficLight = "Green";
            }

            var item = new BudgetExecutionItemDto
            {
                AccountId = acc.Id,
                AccountCode = acc.Code,
                AccountName = acc.Name,
                IsGroup = false,
                Category = acc.Category.ToString(),
                Nature = acc.Nature.ToString(),
                ApprovedValue = approved,
                Additions = additions,
                TransfersIn = txIn,
                TransfersOut = txOut,
                AdjustedBudget = adjustedBudget,
                ExecutedValue = executed,
                AvailableValue = available,
                ExecutionPercentage = percentage,
                ClosingProjection = closingProj,
                TrafficLight = trafficLight
            };

            movementItems.Add(item);

            if (adjustedBudget > 0 && closingProj > adjustedBudget)
            {
                var pctOver = (closingProj / adjustedBudget) * 100m;
                alertList.Add(new BudgetAlertDto
                {
                    AccountCode = acc.Code,
                    AccountName = acc.Name,
                    AdjustedBudget = adjustedBudget,
                    ClosingProjection = closingProj,
                    Message = $"Alerta: La cuenta de gasto '{acc.Code} - {acc.Name}' superara el limite aprobado. Proyeccion: {closingProj:C2} ({pctOver:N1}% de {adjustedBudget:C2})."
                });
            }
            else if (adjustedBudget == 0 && closingProj > 0)
            {
                alertList.Add(new BudgetAlertDto
                {
                    AccountCode = acc.Code,
                    AccountName = acc.Name,
                    AdjustedBudget = 0,
                    ClosingProjection = closingProj,
                    Message = $"Alerta: La cuenta de gasto '{acc.Code} - {acc.Name}' registra ejecucion de {closingProj:C2} pero no tiene presupuesto aprobado."
                });
            }
        }

        foreach (var acc in accounts.Where(a => a.IsGroup == true && a.Category == AccountingAccountCategory.Expense))
        {
            var descendants = movementItems
                .Where(m => m.AccountCode.StartsWith(acc.Code) && m.AccountCode != acc.Code)
                .ToList();

            decimal approved = descendants.Sum(d => d.ApprovedValue);
            decimal additions = descendants.Sum(d => d.Additions);
            decimal txIn = descendants.Sum(d => d.TransfersIn);
            decimal txOut = descendants.Sum(d => d.TransfersOut);
            decimal adjustedBudget = descendants.Sum(d => d.AdjustedBudget);
            decimal executed = descendants.Sum(d => d.ExecutedValue);
            decimal available = descendants.Sum(d => d.AvailableValue);
            decimal closingProj = descendants.Sum(d => d.ClosingProjection);

            decimal percentage = 0;
            if (adjustedBudget != 0)
            {
                percentage = (executed / adjustedBudget) * 100m;
            }

            string trafficLight = "Green";
            int currentMonth = DateTime.Today.Month;
            decimal limitForMonth = (currentMonth / 12.0m) * 100m + 10m;

            if (percentage > 100m)
            {
                trafficLight = "Red";
            }
            else if (percentage > 80m && currentMonth < 10 && DateTime.Today.Year == fiscalPeriod)
            {
                trafficLight = "Yellow";
            }
            else if (percentage > limitForMonth && DateTime.Today.Year == fiscalPeriod)
            {
                trafficLight = "Yellow";
            }
            else
            {
                trafficLight = "Green";
            }

            var item = new BudgetExecutionItemDto
            {
                AccountId = acc.Id,
                AccountCode = acc.Code,
                AccountName = acc.Name,
                IsGroup = true,
                Category = acc.Category.ToString(),
                Nature = acc.Nature.ToString(),
                ApprovedValue = approved,
                Additions = additions,
                TransfersIn = txIn,
                TransfersOut = txOut,
                AdjustedBudget = adjustedBudget,
                ExecutedValue = executed,
                AvailableValue = available,
                ExecutionPercentage = percentage,
                ClosingProjection = closingProj,
                TrafficLight = trafficLight
            };

            reportItems.Add(item);
        }

        // Unificar ítems y ordenar jerárquicamente por el código contable
        reportItems.AddRange(movementItems);
        var sortedItems = reportItems.OrderBy(i => i.AccountCode).ToList();

        var report = new BudgetExecutionReportDto
        {
            BudgetId = budget.Id,
            FiscalPeriod = budget.FiscalPeriod,
            MeetingActNumber = budget.MeetingActNumber,
            ApprovalDate = budget.ApprovalDate,
            Status = budget.Status.ToString(),
            Items = sortedItems,
            Alerts = alertList
        };

        return report;
    }
}
