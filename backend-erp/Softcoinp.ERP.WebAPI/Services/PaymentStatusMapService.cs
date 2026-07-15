using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Softcoinp.ERP.Domain.Enums;
using Softcoinp.ERP.Infrastructure.Persistence;
using Softcoinp.ERP.WebAPI.DTOs;

namespace Softcoinp.ERP.WebAPI.Services;

/// <summary>
/// Calcula el mapa visual de estado de pago por unidad, agrupado por torre y piso,
/// usando únicamente datos del módulo de Cuotas y Cartera y del módulo de Unidades.
/// </summary>
public class PaymentStatusMapService
{
    private readonly ApplicationDbContext _context;
    private readonly IndicatorCacheService _indicatorCache;

    public const string CacheKeyPrefix = "payment_map_";

    public PaymentStatusMapService(ApplicationDbContext context, IndicatorCacheService indicatorCache)
    {
        _context = context;
        _indicatorCache = indicatorCache;
    }

    public async Task<PaymentStatusMapDto> GetPaymentStatusMapAsync(string tenantId)
    {
        var cacheKey = $"{CacheKeyPrefix}{tenantId}";
        var cached = await _indicatorCache.GetAsync<PaymentStatusMapDto>(tenantId, cacheKey);
        if (cached != null)
        {
            return cached;
        }

        var map = await ComputePaymentStatusMapAsync(tenantId);
        await _indicatorCache.SetAsync(tenantId, cacheKey, map, expirationMinutes: 15);
        return map;
    }

    private async Task<PaymentStatusMapDto> ComputePaymentStatusMapAsync(string tenantId)
    {
        var sql = @"
WITH spokespersons AS (
    SELECT uo.UnitId, o.FullNameOrCompanyName AS OwnerName
    FROM erp_unit_owners uo
    INNER JOIN erp_owners o ON o.Id = uo.OwnerId
    WHERE uo.TenantId = @p0 AND uo.IsActive = TRUE AND uo.IsSpokesperson = TRUE
),
fee_debts AS (
    SELECT UnitId,
           SUM(BalanceAmount) AS TotalDebt,
           COUNT(DISTINCT BillingPeriodId) AS MonthsOverdue
    FROM erp_unit_fees
    WHERE TenantId = @p0 AND BalanceAmount > 0 AND Status <> 'FullyPaid'
    GROUP BY UnitId
),
extra_debts AS (
    SELECT UnitId, SUM(BalanceAmount) AS TotalDebt
    FROM erp_extraordinary_fee_distributions
    WHERE TenantId = @p0 AND BalanceAmount > 0 AND Status <> 'FullyPaid'
    GROUP BY UnitId
),
charge_debts AS (
    SELECT UnitId, SUM(BalanceAmount) AS TotalDebt
    FROM erp_individual_charges
    WHERE TenantId = @p0 AND BalanceAmount > 0 AND Status <> 'Paid'
    GROUP BY UnitId
)
SELECT
    u.Id AS UnitId,
    u.Identifier AS Identifier,
    COALESCE(NULLIF(u.TowerOrBlock, ''), 'Sin Torre') AS TowerOrBlock,
    u.FloorLevel AS FloorLevel,
    COALESCE(s.OwnerName, 'Sin propietario') AS OwnerName,
    COALESCE(fd.TotalDebt, 0) + COALESCE(ed.TotalDebt, 0) + COALESCE(cd.TotalDebt, 0) AS OverdueBalance,
    COALESCE(fd.MonthsOverdue, 0) AS MonthsOverdue,
    u.Status AS UnitStatus
FROM erp_units u
LEFT JOIN spokespersons s ON s.UnitId = u.Id
LEFT JOIN fee_debts fd ON fd.UnitId = u.Id
LEFT JOIN extra_debts ed ON ed.UnitId = u.Id
LEFT JOIN charge_debts cd ON cd.UnitId = u.Id
WHERE u.TenantId = @p0
ORDER BY TowerOrBlock, u.FloorLevel, u.Identifier";

        var rawUnits = await _context.Database
            .SqlQueryRaw<UnitPaymentStatusRawDto>(sql, tenantId)
            .ToListAsync();

        var map = new PaymentStatusMapDto
        {
            GeneratedAt = DateTime.UtcNow
        };

        var towerGroups = rawUnits.GroupBy(u => u.TowerOrBlock).OrderBy(g => g.Key);

        foreach (var towerGroup in towerGroups)
        {
            var tower = new TowerGroupDto
            {
                TowerOrBlock = towerGroup.Key
            };

            var floorGroups = towerGroup.GroupBy(u => u.FloorLevel).OrderBy(g => g.Key);

            foreach (var floorGroup in floorGroups)
            {
                var floor = new FloorGroupDto
                {
                    FloorLevel = floorGroup.Key
                };

                foreach (var unit in floorGroup.OrderBy(u => u.Identifier))
                {
                    floor.Units.Add(BuildUnitPaymentStatus(unit));
                }

                tower.Floors.Add(floor);
            }

            map.Towers.Add(tower);
        }

        return map;
    }

    private static UnitPaymentStatusDto BuildUnitPaymentStatus(UnitPaymentStatusRawDto raw)
    {
        var colorCode = DetermineColorCode(raw.UnitStatus, raw.OverdueBalance, raw.MonthsOverdue);
        var statusLabel = DetermineStatusLabel(colorCode, raw.MonthsOverdue);

        return new UnitPaymentStatusDto
        {
            UnitId = raw.UnitId,
            Identifier = raw.Identifier,
            OwnerName = raw.OwnerName,
            OverdueBalance = raw.OverdueBalance,
            MonthsOverdue = raw.MonthsOverdue,
            ColorCode = colorCode,
            StatusLabel = statusLabel
        };
    }

    private static string DetermineColorCode(string unitStatus, decimal overdueBalance, int monthsOverdue)
    {
        var isUnoccupiedOrInactive = unitStatus == UnitStatus.ActiveUnoccupied.ToString()
            || unitStatus == UnitStatus.Inactive.ToString()
            || unitStatus == UnitStatus.DeliveryProcess.ToString()
            || unitStatus == UnitStatus.Litigation.ToString();

        if (isUnoccupiedOrInactive)
        {
            return "gray";
        }

        if (overdueBalance <= 0)
        {
            return "green";
        }

        if (monthsOverdue <= 1)
        {
            return "yellow";
        }

        if (monthsOverdue == 2)
        {
            return "orange";
        }

        return "red";
    }

    private static string DetermineStatusLabel(string colorCode, int monthsOverdue)
    {
        if (colorCode == "gray")
        {
            return "Desocupada / Inactiva";
        }

        if (colorCode == "green")
        {
            return "Al día";
        }

        if (colorCode == "yellow")
        {
            return "1 mes pendiente";
        }

        if (colorCode == "orange")
        {
            return "2 meses pendientes";
        }

        return $"{monthsOverdue} meses pendientes";
    }
}

internal class UnitPaymentStatusRawDto
{
    public Guid UnitId { get; set; }
    public string Identifier { get; set; } = string.Empty;
    public string TowerOrBlock { get; set; } = string.Empty;
    public int FloorLevel { get; set; }
    public string OwnerName { get; set; } = string.Empty;
    public decimal OverdueBalance { get; set; }
    public int MonthsOverdue { get; set; }
    public string UnitStatus { get; set; } = string.Empty;
}
