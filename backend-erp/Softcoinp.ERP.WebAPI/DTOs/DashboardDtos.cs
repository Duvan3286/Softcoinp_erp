using System;
using System.Collections.Generic;
using Softcoinp.ERP.Domain.Enums;

namespace Softcoinp.ERP.WebAPI.DTOs;

public class DashboardDataDto
{
    public DashboardKpisDto Kpis { get; set; } = new();
    public List<AlertDto> Alerts { get; set; } = new();
    public List<MonthlyCollectionDto> MonthlyCollection { get; set; } = new();
    public List<UnitMoraDto> MoraMap { get; set; } = new();
    public List<UpcomingEventDto> UpcomingEvents { get; set; } = new();
    public List<RecentActivityDto> RecentActivity { get; set; } = new();
    public List<UnitSummaryDto> UnitSummaries { get; set; } = new();
    public ContingencyFundInfoDto? ContingencyFund { get; set; }
    public List<CouncilApprovalDto> PendingCouncilApprovals { get; set; } = new();
    public ResidentDashboardDto? ResidentData { get; set; }
}

public class DashboardKpisDto
{
    public decimal CurrentMonthCollectionPercentage { get; set; }
    public decimal PreviousMonthCollectionPercentage { get; set; }
    public int DaysElapsedInPeriod { get; set; }
    public int TotalDaysInPeriod { get; set; }
    public decimal TotalOverduePortfolio { get; set; }
    public decimal EarlyOverdue { get; set; }
    public decimal MediumOverdue { get; set; }
    public decimal LegalOverdue { get; set; }
    public decimal AvailableCash { get; set; }
    public decimal BudgetExecutionPercentage { get; set; }
    public decimal YearProgressPercentage { get; set; }
    public int OpenPqrCount { get; set; }
    public int OverduePqrCount { get; set; }
    public decimal CurrentMonthBilled { get; set; }
    public decimal CurrentMonthCollected { get; set; }
}

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

public class MonthlyCollectionDto
{
    public string Period { get; set; } = string.Empty;
    public decimal Billed { get; set; }
    public decimal Collected { get; set; }
}

public class UnitMoraDto
{
    public Guid UnitId { get; set; }
    public string Identifier { get; set; } = string.Empty;
    public string TowerOrBlock { get; set; } = string.Empty;
    public int FloorLevel { get; set; }
    public string OwnerName { get; set; } = string.Empty;
    public decimal OverdueBalance { get; set; }
    public int DaysOverdue { get; set; }
    public string ColorCode { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
}

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

public class UnitSummaryDto
{
    public Guid UnitId { get; set; }
    public string Identifier { get; set; } = string.Empty;
    public string TowerOrBlock { get; set; } = string.Empty;
    public int FloorLevel { get; set; }
    public string OwnerName { get; set; } = string.Empty;
    public decimal CurrentBalance { get; set; }
    public string ColorCode { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
}

public class ContingencyFundInfoDto
{
    public decimal CurrentBalance { get; set; }
    public decimal LastContributionAmount { get; set; }
    public string LastContributionPeriod { get; set; } = string.Empty;
}

public class CouncilApprovalDto
{
    public string Type { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public DateTime RequestedAt { get; set; }
    public string ModuleLink { get; set; } = string.Empty;
}

public class ResidentDashboardDto
{
    public string UnitIdentifier { get; set; } = string.Empty;
    public decimal CurrentBalance { get; set; }
    public int DaysOverdue { get; set; }
    public DateTime OldestDebtDate { get; set; }
    public List<ResidentOpenPqrDto> OpenPqrs { get; set; } = new();
    public List<ResidentReservationDto> ActiveReservations { get; set; } = new();
    public List<ResidentCircularDto> LatestCirculars { get; set; } = new();
}

public class ResidentOpenPqrDto
{
    public string Title { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public bool IsOverdue { get; set; }
}

public class ResidentReservationDto
{
    public string Title { get; set; } = string.Empty;
    public DateTime ReservationDate { get; set; }
    public string Status { get; set; } = string.Empty;
}

public class ResidentCircularDto
{
    public string Title { get; set; } = string.Empty;
    public DateTime PublishedAt { get; set; }
    public string DocumentUrl { get; set; } = string.Empty;
}
