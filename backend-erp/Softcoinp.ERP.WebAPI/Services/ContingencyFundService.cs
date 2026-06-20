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

public class ContingencyFundService
{
    private readonly ApplicationDbContext _context;

    public ContingencyFundService(ApplicationDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Retorna el estado actual del fondo de imprevistos, incluyendo la proyección de cierre y el historial completo.
    /// </summary>
    public async Task<ContingencyFundDto> GetContingencyFundStatusAsync(string tenantId)
    {
        // 1. Obtener registro del fondo
        var fund = await _context.ContingencyFunds
            .FirstOrDefaultAsync(f => f.TenantId == tenantId);

        decimal currentBalance = 0;
        if (fund != null)
        {
            currentBalance = fund.CurrentBalance;
        }

        // 2. Obtener historial completo de aportes y usos
        var contributions = await _context.ContingencyFundContributions
            .Where(c => c.TenantId == tenantId)
            .OrderByDescending(c => c.Period)
            .ToListAsync();

        var usages = await _context.ContingencyFundUsages
            .Where(u => u.TenantId == tenantId)
            .OrderByDescending(u => u.ApprovalDate)
            .ToListAsync();

        // 3. Proyección de saldo al cierre del período fiscal (año en curso)
        int currentYear = DateTime.Today.Year;
        var contributionsThisYear = contributions
            .Where(c => c.Period.StartsWith(currentYear.ToString()))
            .ToList();

        decimal averageMonthlyContribution = 0;
        if (contributionsThisYear.Count > 0)
        {
            averageMonthlyContribution = contributionsThisYear.Average(c => c.Amount);
        }

        int remainingMonths = 12 - DateTime.Today.Month;
        if (remainingMonths < 0)
        {
            remainingMonths = 0;
        }

        decimal projectedContributions = averageMonthlyContribution * remainingMonths;
        decimal projectedClosingBalance = currentBalance + projectedContributions;

        // 4. Mapear DTOs
        var contribDtos = new List<ContingencyFundContributionDto>();
        foreach (var c in contributions)
        {
            contribDtos.Add(new ContingencyFundContributionDto
            {
                Id = c.Id,
                Period = c.Period,
                Amount = c.Amount,
                IncomeBase = c.IncomeBase,
                Percentage = c.Percentage,
                ContributionDate = c.ContributionDate
            });
        }

        var usageDtos = new List<ContingencyFundUsageDto>();
        foreach (var u in usages)
        {
            usageDtos.Add(new ContingencyFundUsageDto
            {
                Id = u.Id,
                Amount = u.Amount,
                Justification = u.Justification,
                CouncilApprovalActNumber = u.CouncilApprovalActNumber,
                ApprovalDate = u.ApprovalDate,
                CreatedByUserId = u.CreatedByUserId
            });
        }

        return new ContingencyFundDto
        {
            TenantId = tenantId,
            CurrentBalance = currentBalance,
            ProjectedClosingBalance = Math.Round(projectedClosingBalance, 2),
            Contributions = contribDtos,
            Usages = usageDtos
        };
    }

    /// <summary>
    /// Calcula y registra automáticamente el aporte mensual al fondo de imprevistos basado en los ingresos totales del período.
    /// Genera la transacción contable Debit a cuenta de Gasto (5196) y Credit a cuenta de Reserva Patrimonio (3205).
    /// </summary>
    public async Task<ContingencyFundContribution> LiquidateMonthlyContributionAsync(string tenantId, int year, int month)
    {
        string period = $"{year:D4}-{month:D2}";

        // 1. Validar que no se haya liquidado previamente este periodo
        var existingContribution = await _context.ContingencyFundContributions
            .FirstOrDefaultAsync(c => c.TenantId == tenantId && c.Period == period);

        if (existingContribution != null)
        {
            throw new InvalidOperationException($"El aporte al fondo de imprevistos para el periodo {period} ya fue liquidado (ID aporte: {existingContribution.Id}).");
        }

        // 2. Obtener porcentaje del fondo de imprevistos de la configuración del tenant
        decimal pct = 1.00m; // Mínimo legal por defecto según la Ley 675
        var tenantConfig = await _context.TenantConfigurations.FirstOrDefaultAsync();
        if (tenantConfig != null)
        {
            pct = tenantConfig.ContingencyFundPercentage;
        }

        // 3. Calcular aporte basado en el presupuesto anual (Ley 675 de 2001)
        var activeBudget = await _context.Budgets
            .Where(b => b.TenantId == tenantId && b.FiscalPeriod == year && b.Status == BudgetStatus.Active)
            .Include(b => b.BudgetDetails)
            .FirstOrDefaultAsync();

        decimal monthlyBudget = 0m;
        if (activeBudget != null)
        {
            monthlyBudget = Math.Round(activeBudget.BudgetDetails.Sum(d => d.ApprovedValue) / 12m, 2);
        }

        decimal contributionAmount = Math.Round(monthlyBudget * (pct / 100m), 2);

        // 4. Iniciar transacción o registrar datos contables
        var contribution = new ContingencyFundContribution
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            Period = period,
            Amount = contributionAmount,
            IncomeBase = monthlyBudget,
            Percentage = pct,
            ContributionDate = DateTime.UtcNow
        };

        // 5. Actualizar el saldo actual en ContingencyFund
        var fund = await _context.ContingencyFunds.FirstOrDefaultAsync(f => f.TenantId == tenantId);
        if (fund == null)
        {
            fund = new ContingencyFund
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                CurrentBalance = 0
            };
            _context.ContingencyFunds.Add(fund);
        }
        fund.CurrentBalance += contributionAmount;

        // 6. Registrar el movimiento contable correspondiente (Resolución 029)
        // Cuenta de Gasto de Imprevistos (5196) vs Cuenta de Reserva de Imprevistos en Patrimonio (3205)
        var expenseAccount = await _context.AccountingAccounts.FirstOrDefaultAsync(a => a.TenantId == tenantId && a.Code == "5196");
        var reserveAccount = await _context.AccountingAccounts.FirstOrDefaultAsync(a => a.TenantId == tenantId && a.Code == "3205");

        if (expenseAccount != null && reserveAccount != null)
        {
            var entryId = Guid.NewGuid();
            var entry = new AccountingEntry
            {
                Id = entryId,
                TenantId = tenantId,
                EntryDate = DateTime.UtcNow,
                Description = $"Aporte fondo de imprevistos mensual {pct}% sobre presupuesto de {monthlyBudget:C2} ({period})",
                ExternalReference = $"LIQ-{period}",
                EntryType = EntryType.Automatic,
                Status = EntryStatus.Final,
                TotalDebit = contributionAmount,
                TotalCredit = contributionAmount,
                CreatedByUserId = "SYSTEM"
            };
            entry.Lines.Add(new EntryLine
            {
                AccountingEntryId = entryId,
                AccountingAccountId = expenseAccount.Id,
                Debit = contributionAmount,
                Credit = 0
            });
            entry.Lines.Add(new EntryLine
            {
                AccountingEntryId = entryId,
                AccountingAccountId = reserveAccount.Id,
                Debit = 0,
                Credit = contributionAmount
            });

            _context.AccountingEntries.Add(entry);

            // Asociar comprobante
            contribution.AccountingRecordId = entryId;
        }

        _context.ContingencyFundContributions.Add(contribution);
        await _context.SaveChangesAsync();

        return contribution;
    }

    /// <summary>
    /// Registra el uso del fondo de imprevistos con la aprobación obligatoria del consejo de administración.
    /// Genera la transacción contable Debit a Reserva Patrimonio (3205) y Credit a Bancos (1110).
    /// </summary>
    public async Task<ContingencyFundUsage> RecordUsageAsync(
        string tenantId,
        decimal amount,
        string justification,
        string councilApprovalActNumber,
        DateTime approvalDate,
        string userId)
    {
        if (amount <= 0)
        {
            throw new ArgumentException("El monto de retiro del fondo de imprevistos debe ser mayor a cero.");
        }

        if (string.IsNullOrWhiteSpace(justification))
        {
            throw new ArgumentException("La justificación técnica de urgencia es obligatoria para retirar fondos.");
        }

        if (string.IsNullOrWhiteSpace(councilApprovalActNumber))
        {
            throw new ArgumentException("Debe registrar la aprobación del Consejo de Administración (Número de acta obligatorio).");
        }

        // 1. Obtener y validar saldo suficiente del fondo
        var fund = await _context.ContingencyFunds.FirstOrDefaultAsync(f => f.TenantId == tenantId);
        if (fund == null)
        {
            throw new InvalidOperationException("No se ha inicializado el fondo de imprevistos para este tenant.");
        }

        if (fund.CurrentBalance < amount)
        {
            throw new InvalidOperationException($"Operación rechazada: Fondos insuficientes en el Fondo de Imprevistos. Disponible: {fund.CurrentBalance:C2}, Solicitado: {amount:C2}.");
        }

        // 2. Descontar saldo
        fund.CurrentBalance -= Math.Round(amount, 2);

        // 3. Crear registro de uso
        var usage = new ContingencyFundUsage
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            Amount = Math.Round(amount, 2),
            Justification = justification,
            CouncilApprovalActNumber = councilApprovalActNumber,
            ApprovalDate = approvalDate,
            CreatedByUserId = userId
        };

        // 4. Registrar movimientos contables
        // Debit Cuenta Patrimonio (3205) vs Credit Cuenta Activo Bancos (1110)
        var reserveAccount = await _context.AccountingAccounts.FirstOrDefaultAsync(a => a.TenantId == tenantId && a.Code == "3205");
        var bankAccount = await _context.AccountingAccounts
            .Where(a => a.TenantId == tenantId && a.Code.StartsWith("1110") && a.IsGroup == false)
            .OrderBy(a => a.Code)
            .FirstOrDefaultAsync();

        if (bankAccount == null)
        {
            bankAccount = await _context.AccountingAccounts.FirstOrDefaultAsync(a => a.TenantId == tenantId && a.Code == "1110");
        }

        if (reserveAccount != null && bankAccount != null)
        {
            var entryId = Guid.NewGuid();
            var entry = new AccountingEntry
            {
                Id = entryId,
                TenantId = tenantId,
                EntryDate = DateTime.UtcNow,
                Description = $"Retiro del fondo de imprevistos por acta del consejo {councilApprovalActNumber}. Justificación: {justification}",
                ExternalReference = $"CON-{councilApprovalActNumber}",
                EntryType = EntryType.Automatic,
                Status = EntryStatus.Final,
                TotalDebit = amount,
                TotalCredit = amount,
                CreatedByUserId = userId
            };
            entry.Lines.Add(new EntryLine
            {
                AccountingEntryId = entryId,
                AccountingAccountId = reserveAccount.Id,
                Debit = amount,
                Credit = 0
            });
            entry.Lines.Add(new EntryLine
            {
                AccountingEntryId = entryId,
                AccountingAccountId = bankAccount.Id,
                Debit = 0,
                Credit = amount
            });

            _context.AccountingEntries.Add(entry);

            usage.AccountingRecordId = entryId;
        }

        _context.ContingencyFundUsages.Add(usage);
        await _context.SaveChangesAsync();

        return usage;
    }
}
