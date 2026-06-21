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
    private readonly IndicatorCacheService _indicatorCache;

    public BudgetService(ApplicationDbContext context, IndicatorCacheService indicatorCache)
    {
        _context = context;
        _indicatorCache = indicatorCache;
    }

    /// <summary>
    /// Crea un presupuesto en estado Borrador (Draft) para el período fiscal.
    /// Si ya existe un presupuesto ACTIVO para ese período, bloquea la operación lanzando una excepción.
    /// </summary>
    public async Task<Budget> CreateBudgetAsync(
        string tenantId,
        int fiscalPeriod,
        string meetingActNumber,
        DateTime? approvalDate,
        bool copyFromPrevious,
        decimal? globalPercentageAdjustment,
        Dictionary<string, decimal>? accountAdjustments,
        List<CreateBudgetDetailRequestDto>? manualDetails,
        string userId)
    {
        // 1. Validar que no exista un presupuesto activo para el período
        var activeBudget = await _context.Budgets
            .FirstOrDefaultAsync(b => b.TenantId == tenantId && b.FiscalPeriod == fiscalPeriod && b.Status == BudgetStatus.Active);

        if (activeBudget != null)
        {
            throw new InvalidOperationException($"Operación bloqueada: Ya existe un presupuesto ACTIVO para el período fiscal {fiscalPeriod}.");
        }

        // Eliminar cualquier borrador previo para este periodo fiscal si se va a recrear
        var existingDraft = await _context.Budgets
            .FirstOrDefaultAsync(b => b.TenantId == tenantId && b.FiscalPeriod == fiscalPeriod && b.Status == BudgetStatus.Draft);
        if (existingDraft != null)
        {
            _context.Budgets.Remove(existingDraft);
        }

        var budget = new Budget
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            FiscalPeriod = fiscalPeriod,
            MeetingActNumber = meetingActNumber,
            ApprovalDate = approvalDate,
            Status = BudgetStatus.Draft,
            CreatedByUserId = userId
        };

        // Cargar cuentas del tenant para validación de tipo e inactividad
        var accounts = await _context.AccountingAccounts
            .Where(a => a.TenantId == tenantId && a.IsActive)
            .ToDictionaryAsync(a => a.Code);

        if (copyFromPrevious)
        {
            // 2. Copiar presupuesto del período anterior
            var previousPeriod = fiscalPeriod - 1;
            var previousBudget = await _context.Budgets
                .Include(b => b.BudgetDetails)
                .ThenInclude(d => d.AccountingAccount)
                .FirstOrDefaultAsync(b => b.TenantId == tenantId && b.FiscalPeriod == previousPeriod && (b.Status == BudgetStatus.Active || b.Status == BudgetStatus.Closed));

            if (previousBudget == null)
            {
                throw new InvalidOperationException($"No se encontró un presupuesto aprobado (Activo o Cerrado) para el período anterior ({previousPeriod}) para copiar.");
            }

            foreach (var prevDetail in previousBudget.BudgetDetails)
            {
                var accountCode = prevDetail.AccountingAccount!.Code;

                // Solo copiar si la cuenta está activa en el catálogo actual
                if (accounts.TryGetValue(accountCode, out var currentAccount))
                {
                    // No presupuestar cuentas de agrupación
                    if (currentAccount.IsGroup)
                    {
                        continue;
                    }

                    decimal adjustedValue = prevDetail.ApprovedValue;

                    // Aplicar ajuste cuenta por cuenta si existe
                    if (accountAdjustments != null && accountAdjustments.TryGetValue(accountCode, out var pct))
                    {
                        adjustedValue = prevDetail.ApprovedValue * (1m + (pct / 100m));
                    }
                    // O aplicar ajuste global
                    else if (globalPercentageAdjustment.HasValue)
                    {
                        adjustedValue = prevDetail.ApprovedValue * (1m + (globalPercentageAdjustment.Value / 100m));
                    }

                    // Redondear a 2 decimales
                    adjustedValue = Math.Round(adjustedValue, 2);

                    budget.BudgetDetails.Add(new BudgetDetail
                    {
                        Id = Guid.NewGuid(),
                        BudgetId = budget.Id,
                        AccountingAccountId = currentAccount.Id,
                        ApprovedValue = adjustedValue,
                        Observations = $"Copiado de período {previousPeriod}. Valor original: {prevDetail.ApprovedValue}"
                    });
                }
            }
        }
        else if (manualDetails != null)
        {
            // 3. Crear a partir de los detalles manuales proveídos
            foreach (var detail in manualDetails)
            {
                var account = await _context.AccountingAccounts
                    .FirstOrDefaultAsync(a => a.Id == detail.AccountingAccountId && a.TenantId == tenantId);

                if (account == null)
                {
                    throw new KeyNotFoundException($"La cuenta contable con ID {detail.AccountingAccountId} no existe.");
                }

                if (!account.IsActive)
                {
                    throw new InvalidOperationException($"La cuenta contable {account.Code} - {account.Name} está inactiva y no puede recibir presupuesto.");
                }

                if (account.IsGroup)
                {
                    throw new InvalidOperationException($"La cuenta contable {account.Code} es de agrupación y no puede recibir presupuesto directo. Debe presupuestar a nivel auxiliar.");
                }

                budget.BudgetDetails.Add(new BudgetDetail
                {
                    Id = Guid.NewGuid(),
                    BudgetId = budget.Id,
                    AccountingAccountId = account.Id,
                    ApprovedValue = Math.Round(detail.ApprovedValue, 2),
                    Observations = detail.Observations
                });
            }
        }

        _context.Budgets.Add(budget);
        await _context.SaveChangesAsync();

        return budget;
    }

    /// <summary>
    /// Modifica los rubros de un presupuesto que esté en estado Borrador (Draft).
    /// Si el presupuesto ya está Activo o Cerrado, bloquea la edición directa.
    /// </summary>
    public async Task<Budget> UpdateDraftBudgetDetailsAsync(
        string tenantId,
        Guid budgetId,
        List<CreateBudgetDetailRequestDto> details)
    {
        var budget = await _context.Budgets
            .Include(b => b.BudgetDetails)
            .FirstOrDefaultAsync(b => b.Id == budgetId && b.TenantId == tenantId);

        if (budget == null)
        {
            throw new KeyNotFoundException("No se encontró el presupuesto solicitado.");
        }

        if (budget.Status != BudgetStatus.Draft)
        {
            throw new InvalidOperationException("Un presupuesto aprobado/activo no puede editarse directamente. Los cambios deben realizarse mediante traslados o adiciones.");
        }

        // Limpiar detalles anteriores
        _context.BudgetDetails.RemoveRange(budget.BudgetDetails);
        budget.BudgetDetails.Clear();

        // Agregar nuevos detalles
        foreach (var d in details)
        {
            var account = await _context.AccountingAccounts
                .FirstOrDefaultAsync(a => a.Id == d.AccountingAccountId && a.TenantId == tenantId);

            if (account == null)
            {
                throw new KeyNotFoundException($"La cuenta con ID {d.AccountingAccountId} no existe.");
            }

            if (!account.IsActive)
            {
                throw new InvalidOperationException($"La cuenta contable {account.Code} está inactiva.");
            }

            if (account.IsGroup)
            {
                throw new InvalidOperationException($"La cuenta {account.Code} es de agrupación y no acepta movimientos directos.");
            }

            budget.BudgetDetails.Add(new BudgetDetail
            {
                Id = Guid.NewGuid(),
                BudgetId = budget.Id,
                AccountingAccountId = account.Id,
                ApprovedValue = Math.Round(d.ApprovedValue, 2),
                Observations = d.Observations
            });
        }

        await _context.SaveChangesAsync();
        return budget;
    }

    /// <summary>
    /// Activa un presupuesto en borrador exigiendo acta de asamblea y fecha de aprobación.
    /// </summary>
    public async Task<Budget> ActivateBudgetAsync(
        string tenantId,
        Guid budgetId,
        string meetingActNumber,
        DateTime approvalDate)
    {
        if (string.IsNullOrWhiteSpace(meetingActNumber))
        {
            throw new ArgumentException("El número de acta de asamblea es obligatorio para activar el presupuesto.");
        }

        var budget = await _context.Budgets
            .FirstOrDefaultAsync(b => b.Id == budgetId && b.TenantId == tenantId);

        if (budget == null)
        {
            throw new KeyNotFoundException("No se encontró el presupuesto a activar.");
        }

        if (budget.Status == BudgetStatus.Active)
        {
            return budget; // Ya está activo
        }

        if (budget.Status == BudgetStatus.Closed)
        {
            throw new InvalidOperationException("No se puede activar un presupuesto que ya se encuentra cerrado.");
        }

        // Validar que no exista otro presupuesto activo para el mismo período
        var activeBudget = await _context.Budgets
            .FirstOrDefaultAsync(b => b.TenantId == tenantId && b.FiscalPeriod == budget.FiscalPeriod && b.Status == BudgetStatus.Active);

        if (activeBudget != null)
        {
            throw new InvalidOperationException($"Operación bloqueada: Ya existe un presupuesto ACTIVO para el período fiscal {budget.FiscalPeriod} (Presupuesto ID: {activeBudget.Id}).");
        }

        // Validar que no existan periodos de facturación ya ejecutados para este año fiscal
        var yearPrefix = budget.FiscalPeriod.ToString() + "-";
        var hasBillingPeriods = await _context.BillingPeriods
            .AnyAsync(bp => bp.TenantId == tenantId && bp.Period.StartsWith(yearPrefix));

        if (hasBillingPeriods)
        {
            throw new InvalidOperationException($"Operación bloqueada: Ya existen periodos de facturación generados para el año fiscal {budget.FiscalPeriod}. No se puede activar un presupuesto después de haber ejecutado liquidaciones mensuales.");
        }

        // Activar el presupuesto y registrar los datos de aprobación de la asamblea
        budget.MeetingActNumber = meetingActNumber;
        budget.ApprovalDate = approvalDate;
        budget.Status = BudgetStatus.Active;

        await _context.SaveChangesAsync();
        await _indicatorCache.InvalidateAsync(tenantId, "kpis_");
        return budget;
    }

    /// <summary>
    /// Cierra un presupuesto activo al final del período fiscal.
    /// </summary>
    public async Task<Budget> CloseBudgetAsync(string tenantId, Guid budgetId)
    {
        var budget = await _context.Budgets
            .FirstOrDefaultAsync(b => b.Id == budgetId && b.TenantId == tenantId);

        if (budget == null)
        {
            throw new KeyNotFoundException("No se encontró el presupuesto.");
        }

        if (budget.Status != BudgetStatus.Active)
        {
            throw new InvalidOperationException("Solo se puede cerrar un presupuesto que esté actualmente Activo.");
        }

        budget.Status = BudgetStatus.Closed;
        await _context.SaveChangesAsync();

        return budget;
    }
}
