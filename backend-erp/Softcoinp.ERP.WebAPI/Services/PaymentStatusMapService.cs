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
    private readonly PortfolioAgingService _portfolioAgingService;

    public const string CacheKeyPrefix = "payment_map_";

    public PaymentStatusMapService(
        ApplicationDbContext context, IndicatorCacheService indicatorCache, PortfolioAgingService portfolioAgingService)
    {
        _context = context;
        _indicatorCache = indicatorCache;
        _portfolioAgingService = portfolioAgingService;
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
        var overdueByUnit = await _portfolioAgingService.GetOverdueByUnitAsync(tenantId);

        var spokespersons = await _context.UnitOwners
            .Where(uo => uo.TenantId == tenantId && uo.IsActive && uo.IsSpokesperson)
            .Select(uo => new { uo.UnitId, OwnerName = uo.Owner!.FullNameOrCompanyName })
            .ToDictionaryAsync(x => x.UnitId, x => x.OwnerName);

        var units = await _context.Units
            .Where(u => u.TenantId == tenantId)
            .Select(u => new { u.Id, u.Identifier, u.TowerOrBlock, u.FloorLevel, Status = u.Status.ToString() })
            .ToListAsync();

        var rawUnits = new List<UnitPaymentStatusRawDto>();
        foreach (var unit in units)
        {
            var towerOrBlock = "Sin Torre";
            if (!string.IsNullOrEmpty(unit.TowerOrBlock))
            {
                towerOrBlock = unit.TowerOrBlock;
            }

            var ownerName = "Sin propietario";
            if (spokespersons.TryGetValue(unit.Id, out var spokespersonName))
            {
                ownerName = spokespersonName;
            }

            var overdueBalance = 0m;
            var monthsOverdue = 0;
            if (overdueByUnit.TryGetValue(unit.Id, out var overdue))
            {
                overdueBalance = overdue.TotalDebt;
                monthsOverdue = overdue.MonthsOverdue;
            }

            rawUnits.Add(new UnitPaymentStatusRawDto
            {
                UnitId = unit.Id,
                Identifier = unit.Identifier,
                TowerOrBlock = towerOrBlock,
                FloorLevel = unit.FloorLevel,
                OwnerName = ownerName,
                OverdueBalance = overdueBalance,
                MonthsOverdue = monthsOverdue,
                UnitStatus = unit.Status
            });
        }

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
