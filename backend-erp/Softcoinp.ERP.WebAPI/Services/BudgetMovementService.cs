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

public class BudgetMovementService
{
    private readonly ApplicationDbContext _context;

    public BudgetMovementService(ApplicationDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Registra un traslado o adición presupuestal aplicando validaciones diferenciadas de negocio.
    /// </summary>
    public async Task<BudgetMovement> CreateMovementAsync(
        string tenantId,
        Guid budgetId,
        BudgetMovementType movementType,
        Guid? sourceAccountId,
        Guid destinationAccountId,
        decimal amount,
        string justification,
        BudgetApprovalType approvalType,
        string meetingActNumber,
        DateTime approvalDate,
        string userId)
    {
        if (amount <= 0)
        {
            throw new ArgumentException("El monto del movimiento presupuestal debe ser mayor a cero.");
        }

        if (string.IsNullOrWhiteSpace(justification))
        {
            throw new ArgumentException("La justificación es obligatoria para registrar un cambio presupuestal.");
        }

        if (string.IsNullOrWhiteSpace(meetingActNumber))
        {
            throw new ArgumentException("El número de acta aprobatoria es obligatorio.");
        }

        // 1. Obtener presupuesto
        var budget = await _context.Budgets
            .Include(b => b.BudgetDetails)
            .FirstOrDefaultAsync(b => b.Id == budgetId && b.TenantId == tenantId);

        if (budget == null)
        {
            throw new KeyNotFoundException("No se encontró el presupuesto.");
        }

        if (budget.Status != BudgetStatus.Active)
        {
            throw new InvalidOperationException("Solo se pueden agregar traslados o adiciones a un presupuesto que esté en estado Activo.");
        }

        // 2. Obtener cuenta destino
        var destAccount = await _context.AccountingAccounts
            .FirstOrDefaultAsync(a => a.Id == destinationAccountId && a.TenantId == tenantId);

        if (destAccount == null)
        {
            throw new KeyNotFoundException("La cuenta destino no existe.");
        }

        if (!destAccount.IsActive)
        {
            throw new InvalidOperationException($"La cuenta destino {destAccount.Code} está inactiva.");
        }

        if (destAccount.IsGroup)
        {
            throw new InvalidOperationException($"La cuenta destino {destAccount.Code} es de agrupación y no puede recibir presupuesto directo.");
        }

        // 3. Validaciones de negocio específicas según tipo de movimiento
        if (movementType == BudgetMovementType.Addition)
        {
            // Regla de Negocio: Las adiciones presupuestales incrementan el total del gasto aprobado.
            // Requieren aprobación de asamblea extraordinaria (Asamblea).
            if (approvalType != BudgetApprovalType.Assembly)
            {
                throw new InvalidOperationException("Regla de negocio: Las adiciones presupuestales que aumentan el gasto total aprobado requieren aprobación de la Asamblea de Copropietarios.");
            }
        }
        else if (movementType == BudgetMovementType.Transfer)
        {
            if (approvalType != BudgetApprovalType.Council)
            {
                throw new InvalidOperationException("Los traslados presupuestales entre cuentas del mismo grupo requieren aprobación del Consejo de Administración.");
            }

            if (!sourceAccountId.HasValue)
            {
                throw new ArgumentException("Para realizar un traslado es obligatorio especificar la cuenta contable de origen.");
            }

            if (sourceAccountId.Value == destinationAccountId)
            {
                throw new InvalidOperationException("La cuenta de origen y de destino en un traslado no pueden ser la misma.");
            }

            // Obtener cuenta origen
            var sourceAccount = await _context.AccountingAccounts
                .FirstOrDefaultAsync(a => a.Id == sourceAccountId.Value && a.TenantId == tenantId);

            if (sourceAccount == null)
            {
                throw new KeyNotFoundException("La cuenta de origen no existe.");
            }

            if (!sourceAccount.IsActive)
            {
                throw new InvalidOperationException($"La cuenta contable de origen {sourceAccount.Code} está inactiva.");
            }

            if (sourceAccount.IsGroup)
            {
                throw new InvalidOperationException($"La cuenta contable de origen {sourceAccount.Code} es de agrupación y no acepta movimientos directos.");
            }

            // Regla de Negocio: Validar que origen y destino pertenezcan al mismo grupo (ej. Gasto con Gasto)
            if (sourceAccount.Category != destAccount.Category)
            {
                throw new InvalidOperationException($"Operación inválida: El traslado presupuestal solo está permitido entre cuentas del mismo grupo (Categorías: Origen={sourceAccount.Category}, Destino={destAccount.Category}).");
            }

            // Regla de Negocio: Validar saldo disponible en la cuenta origen para el traslado
            var initialSource = budget.BudgetDetails.FirstOrDefault(d => d.AccountingAccountId == sourceAccountId.Value)?.ApprovedValue ?? 0;
            
            var additionsSource = await _context.BudgetMovements
                .Where(m => m.BudgetId == budget.Id && m.DestinationAccountId == sourceAccountId.Value && m.MovementType == BudgetMovementType.Addition)
                .SumAsync(m => m.Amount);
                
            var transfersInSource = await _context.BudgetMovements
                .Where(m => m.BudgetId == budget.Id && m.DestinationAccountId == sourceAccountId.Value && m.MovementType == BudgetMovementType.Transfer)
                .SumAsync(m => m.Amount);
                
            var transfersOutSource = await _context.BudgetMovements
                .Where(m => m.BudgetId == budget.Id && m.SourceAccountId == sourceAccountId.Value && m.MovementType == BudgetMovementType.Transfer)
                .SumAsync(m => m.Amount);

            var adjustedBudgetSource = initialSource + additionsSource + transfersInSource - transfersOutSource;

            if (adjustedBudgetSource < amount)
            {
                throw new InvalidOperationException($"El traslado supera el presupuesto disponible en la cuenta origen ({sourceAccount.Code}). Presupuesto disponible: {adjustedBudgetSource:C2}, Solicitado: {amount:C2}.");
            }
        }

        // 4. Registrar movimiento
        var movement = new BudgetMovement
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            BudgetId = budget.Id,
            MovementType = movementType,
            SourceAccountId = sourceAccountId,
            DestinationAccountId = destinationAccountId,
            Amount = Math.Round(amount, 2),
            Justification = justification,
            ApprovalType = approvalType,
            MeetingActNumber = meetingActNumber,
            ApprovalDate = approvalDate,
            CreatedByUserId = userId
        };

        _context.BudgetMovements.Add(movement);
        await _context.SaveChangesAsync();

        return movement;
    }

    /// <summary>
    /// Lista todos los movimientos del presupuesto para un tenant.
    /// </summary>
    public async Task<List<BudgetMovementDto>> GetMovementsByBudgetAsync(string tenantId, Guid budgetId)
    {
        var movements = await _context.BudgetMovements
            .Include(m => m.SourceAccount)
            .Include(m => m.DestinationAccount)
            .Where(m => m.TenantId == tenantId && m.BudgetId == budgetId)
            .OrderByDescending(m => m.ApprovalDate)
            .ToListAsync();

        var list = new List<BudgetMovementDto>();
        foreach (var m in movements)
        {
            string srcCode = "";
            string srcName = "";
            if (m.SourceAccount != null)
            {
                srcCode = m.SourceAccount.Code;
                srcName = m.SourceAccount.Name;
            }

            list.Add(new BudgetMovementDto
            {
                Id = m.Id,
                BudgetId = m.BudgetId,
                MovementType = m.MovementType.ToString(),
                SourceAccountId = m.SourceAccountId,
                SourceAccountCode = srcCode,
                SourceAccountName = srcName,
                DestinationAccountId = m.DestinationAccountId,
                DestinationAccountCode = m.DestinationAccount!.Code,
                DestinationAccountName = m.DestinationAccount.Name,
                Amount = m.Amount,
                Justification = m.Justification,
                ApprovalType = m.ApprovalType.ToString(),
                MeetingActNumber = m.MeetingActNumber,
                ApprovalDate = m.ApprovalDate
            });
        }

        return list;
    }
}
