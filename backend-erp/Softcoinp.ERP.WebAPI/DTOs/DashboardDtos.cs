using System;
using System.Collections.Generic;
using Softcoinp.ERP.Domain.Enums;

namespace Softcoinp.ERP.WebAPI.DTOs;

// ═══════════════════════════════════════════════════════════════════════
// KPIs
// ═══════════════════════════════════════════════════════════════════════

public class DashboardKpisDto
{
    public decimal CurrentMonthCollectionPercentage { get; set; }
    public decimal PreviousMonthCollectionPercentage { get; set; }
    public int DaysElapsedInPeriod { get; set; }
    public int TotalDaysInPeriod { get; set; }
    public decimal CurrentMonthBilled { get; set; }
    public decimal CurrentMonthCollected { get; set; }

    public decimal TotalOverduePortfolio { get; set; }
    public decimal OverdueOneMonth { get; set; }
    public decimal OverdueTwoMonths { get; set; }
    public decimal OverdueThreeOrMoreMonths { get; set; }

    public decimal BudgetExecutionPercentage { get; set; }
    public decimal BudgetExpectedExecutionPercentage { get; set; }

    public int OpenPqrCount { get; set; }
    public int OverduePqrCount { get; set; }
}

// ═══════════════════════════════════════════════════════════════════════
// Alertas operativas
// ═══════════════════════════════════════════════════════════════════════

public class AlertDto
{
    public string Id { get; set; } = string.Empty;
    public string RuleType { get; set; } = string.Empty;
    public AlertUrgency Urgency { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string ModuleLink { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}

// ═══════════════════════════════════════════════════════════════════════
// ConfiguracionAlerta (AlertConfiguration) - administración
// ═══════════════════════════════════════════════════════════════════════

public class AlertConfigurationDto
{
    public Guid Id { get; set; }
    public string RuleType { get; set; } = string.Empty;
    public bool IsEnabled { get; set; }
    public int ThresholdDays { get; set; }
    public decimal ThresholdPercentage { get; set; }
    public string DefaultUrgency { get; set; } = string.Empty;
    public bool HasRealDataSource { get; set; }
}

public class UpdateAlertConfigurationRequestDto
{
    public bool IsEnabled { get; set; }
    public int ThresholdDays { get; set; }
    public decimal ThresholdPercentage { get; set; }
    public string DefaultUrgency { get; set; } = string.Empty;
}

// ═══════════════════════════════════════════════════════════════════════
// Recaudo histórico (gráfico de 12 meses) - cacheado
// ═══════════════════════════════════════════════════════════════════════

public class MonthlyCollectionDto
{
    public string Period { get; set; } = string.Empty;
    public decimal Billed { get; set; }
    public decimal Collected { get; set; }
}

// ═══════════════════════════════════════════════════════════════════════
// Mapa de estado de pago por unidad (torres/pisos) - cacheado
// ═══════════════════════════════════════════════════════════════════════

public class PaymentStatusMapDto
{
    public DateTime GeneratedAt { get; set; }
    public List<TowerGroupDto> Towers { get; set; } = new();
}

public class TowerGroupDto
{
    public string TowerOrBlock { get; set; } = string.Empty;
    public List<FloorGroupDto> Floors { get; set; } = new();
}

public class FloorGroupDto
{
    public int FloorLevel { get; set; }
    public List<UnitPaymentStatusDto> Units { get; set; } = new();
}

public class UnitPaymentStatusDto
{
    public Guid UnitId { get; set; }
    public string Identifier { get; set; } = string.Empty;
    public string OwnerName { get; set; } = string.Empty;
    public decimal OverdueBalance { get; set; }
    public int MonthsOverdue { get; set; }
    public string ColorCode { get; set; } = string.Empty;
    public string StatusLabel { get; set; } = string.Empty;
}

// ═══════════════════════════════════════════════════════════════════════
// Próximos eventos y actividad reciente
// ═══════════════════════════════════════════════════════════════════════

public class UpcomingEventDto
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime EventDate { get; set; }
    public string EventType { get; set; } = string.Empty;
    public string ModuleLink { get; set; } = string.Empty;
}

public class RecentActivityDto
{
    public string Action { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
    public string ModuleLink { get; set; } = string.Empty;
}

// ═══════════════════════════════════════════════════════════════════════
// Selector de conjunto
// ═══════════════════════════════════════════════════════════════════════

public class MyTenantOptionDto
{
    public Guid TenantId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Subdomain { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public bool IsCurrent { get; set; }
}
