using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Softcoinp.ERP.Domain.Enums;
using Softcoinp.ERP.Infrastructure.Persistence;

namespace Softcoinp.ERP.WebAPI.Services;

/// <summary>
/// Fuente única de la mora/cartera vencida por unidad. Antes de este servicio, el
/// Dashboard, el mapa de estado de pago y el resumen de cartera calculaban esto por
/// separado con filtros distintos (algunos contaban cargos en disputa, otros no
/// filtraban por fecha de vencimiento, y "meses de mora" se definía de dos formas
/// distintas). Todos los consumidores deben usar este servicio para que la cifra de
/// cartera sea idéntica en cualquier pantalla que la muestre.
/// </summary>
public class PortfolioAgingService
{
    private readonly ApplicationDbContext _context;

    public PortfolioAgingService(ApplicationDbContext context)
    {
        _context = context;
    }

    public static int CalculateMonthsOverdue(DateTime dueDate, DateTime now)
    {
        var months = ((now.Year - dueDate.Year) * 12) + (now.Month - dueDate.Month);
        if (now.Day < dueDate.Day)
        {
            months -= 1;
        }
        return Math.Max(0, months);
    }

    /// <summary>
    /// Calcula, para cada unidad con saldo vencido, la deuda total vencida y los meses
    /// de mora (medidos en meses calendario desde la fecha de vencimiento más antigua
    /// aún pendiente). Excluye cargos individuales en disputa. Unidades sin mora no
    /// aparecen en el resultado.
    /// </summary>
    public async Task<Dictionary<Guid, UnitOverdueSummary>> GetOverdueByUnitAsync(string tenantId)
    {
        var now = DateTime.UtcNow;

        var overdueFees = await _context.UnitFees
            .Where(f => f.TenantId == tenantId && f.Status != FeeStatus.FullyPaid && f.DueDate < now)
            .Select(f => new OverdueRow { UnitId = f.UnitId, Balance = f.BalanceAmount, ReferenceDate = f.DueDate })
            .ToListAsync();

        var overdueExtraordinary = await _context.ExtraordinaryFeeDistributions
            .Where(d => d.TenantId == tenantId && d.Status != FeeStatus.FullyPaid && d.DueDate < now)
            .Select(d => new OverdueRow { UnitId = d.UnitId, Balance = d.BalanceAmount, ReferenceDate = d.DueDate })
            .ToListAsync();

        var overdueCharges = await _context.IndividualCharges
            .Where(c => c.TenantId == tenantId && c.Status != IndividualChargeStatus.Paid && !c.IsDisputed && c.ChargeDate < now)
            .Select(c => new OverdueRow { UnitId = c.UnitId, Balance = c.BalanceAmount, ReferenceDate = c.ChargeDate })
            .ToListAsync();

        var positiveAdjustments = await _context.BillingAdjustments
            .Where(a => a.TenantId == tenantId && a.Amount > 0)
            .Select(a => new OverdueRow { UnitId = a.UnitId, Balance = a.Amount, ReferenceDate = a.CreatedAt })
            .ToListAsync();

        var allRows = new List<OverdueRow>();
        allRows.AddRange(overdueFees);
        allRows.AddRange(overdueExtraordinary);
        allRows.AddRange(overdueCharges);
        allRows.AddRange(positiveAdjustments);

        return BuildSummary(allRows, now);
    }

    /// <summary>
    /// Misma lógica que <see cref="GetOverdueByUnitAsync"/>, en versión síncrona para los
    /// motores de generación de reportes (PDFGenerationEngine/ExcelGenerationEngine), cuya
    /// composición del documento corre en callbacks síncronos y no puede usar await.
    /// </summary>
    public Dictionary<Guid, UnitOverdueSummary> GetOverdueByUnit(string tenantId)
    {
        var now = DateTime.UtcNow;

        var overdueFees = _context.UnitFees
            .Where(f => f.TenantId == tenantId && f.Status != FeeStatus.FullyPaid && f.DueDate < now)
            .Select(f => new OverdueRow { UnitId = f.UnitId, Balance = f.BalanceAmount, ReferenceDate = f.DueDate })
            .ToList();

        var overdueExtraordinary = _context.ExtraordinaryFeeDistributions
            .Where(d => d.TenantId == tenantId && d.Status != FeeStatus.FullyPaid && d.DueDate < now)
            .Select(d => new OverdueRow { UnitId = d.UnitId, Balance = d.BalanceAmount, ReferenceDate = d.DueDate })
            .ToList();

        var overdueCharges = _context.IndividualCharges
            .Where(c => c.TenantId == tenantId && c.Status != IndividualChargeStatus.Paid && !c.IsDisputed && c.ChargeDate < now)
            .Select(c => new OverdueRow { UnitId = c.UnitId, Balance = c.BalanceAmount, ReferenceDate = c.ChargeDate })
            .ToList();

        var positiveAdjustments = _context.BillingAdjustments
            .Where(a => a.TenantId == tenantId && a.Amount > 0)
            .Select(a => new OverdueRow { UnitId = a.UnitId, Balance = a.Amount, ReferenceDate = a.CreatedAt })
            .ToList();

        var allRows = new List<OverdueRow>();
        allRows.AddRange(overdueFees);
        allRows.AddRange(overdueExtraordinary);
        allRows.AddRange(overdueCharges);
        allRows.AddRange(positiveAdjustments);

        return BuildSummary(allRows, now);
    }

    private static Dictionary<Guid, UnitOverdueSummary> BuildSummary(List<OverdueRow> allRows, DateTime now)
    {
        var result = new Dictionary<Guid, UnitOverdueSummary>();

        foreach (var group in allRows.GroupBy(r => r.UnitId))
        {
            var totalDebt = group.Sum(r => r.Balance);
            if (totalDebt <= 0)
            {
                continue;
            }

            var oldestReferenceDate = group.Min(r => r.ReferenceDate);
            var monthsOverdue = CalculateMonthsOverdue(oldestReferenceDate, now);
            if (monthsOverdue <= 0)
            {
                continue;
            }

            result[group.Key] = new UnitOverdueSummary
            {
                UnitId = group.Key,
                TotalDebt = totalDebt,
                MonthsOverdue = monthsOverdue
            };
        }

        return result;
    }

    private class OverdueRow
    {
        public Guid UnitId { get; set; }
        public decimal Balance { get; set; }
        public DateTime ReferenceDate { get; set; }
    }
}

public class UnitOverdueSummary
{
    public Guid UnitId { get; set; }
    public decimal TotalDebt { get; set; }
    public int MonthsOverdue { get; set; }
}
