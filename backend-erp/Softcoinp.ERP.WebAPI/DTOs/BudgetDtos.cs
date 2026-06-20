using System;
using System.Collections.Generic;

namespace Softcoinp.ERP.WebAPI.DTOs;

public class BudgetDto
{
    public Guid Id { get; set; }
    public int FiscalPeriod { get; set; }
    public DateTime? ApprovalDate { get; set; }
    public string MeetingActNumber { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public List<BudgetDetailDto> Details { get; set; } = new();
}

public class BudgetSummaryDto
{
    public Guid Id { get; set; }
    public int FiscalPeriod { get; set; }
    public DateTime? ApprovalDate { get; set; }
    public string MeetingActNumber { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public int DetailsCount { get; set; }
    public string CreatedByUserId { get; set; } = string.Empty;
}

public class BudgetDetailDto
{
    public Guid Id { get; set; }
    public Guid AccountingAccountId { get; set; }
    public string AccountCode { get; set; } = string.Empty;
    public string AccountName { get; set; } = string.Empty;
    public decimal ApprovedValue { get; set; }
    public string Observations { get; set; } = string.Empty;
}

public class CreateBudgetRequestDto
{
    public int FiscalPeriod { get; set; }
    public string MeetingActNumber { get; set; } = string.Empty;
    public DateTime? ApprovalDate { get; set; }
    public bool CopyFromPrevious { get; set; }
    public decimal? GlobalPercentageAdjustment { get; set; }
    public Dictionary<string, decimal>? AccountAdjustments { get; set; }
    public List<CreateBudgetDetailRequestDto>? ManualDetails { get; set; }
}

public class CreateBudgetDetailRequestDto
{
    public Guid AccountingAccountId { get; set; }
    public decimal ApprovedValue { get; set; }
    public string Observations { get; set; } = string.Empty;
}

public class ActivateBudgetRequestDto
{
    public string MeetingActNumber { get; set; } = string.Empty;
    public DateTime ApprovalDate { get; set; }
}

public class BudgetExecutionReportDto
{
    public Guid BudgetId { get; set; }
    public int FiscalPeriod { get; set; }
    public string MeetingActNumber { get; set; } = string.Empty;
    public DateTime? ApprovalDate { get; set; }
    public string Status { get; set; } = string.Empty;
    public List<BudgetExecutionItemDto> Items { get; set; } = new();
    public List<BudgetAlertDto> Alerts { get; set; } = new();
}

public class BudgetExecutionItemDto
{
    public Guid AccountId { get; set; }
    public string AccountCode { get; set; } = string.Empty;
    public string AccountName { get; set; } = string.Empty;
    public bool IsGroup { get; set; }
    public string Category { get; set; } = string.Empty;
    public string Nature { get; set; } = string.Empty;
    public decimal ApprovedValue { get; set; }
    public decimal Additions { get; set; }
    public decimal TransfersIn { get; set; }
    public decimal TransfersOut { get; set; }
    public decimal AdjustedBudget { get; set; }
    public decimal ExecutedValue { get; set; }
    public decimal AvailableValue { get; set; }
    public decimal ExecutionPercentage { get; set; }
    public decimal ClosingProjection { get; set; }
    public string TrafficLight { get; set; } = string.Empty;
}

public class BudgetAlertDto
{
    public string AccountCode { get; set; } = string.Empty;
    public string AccountName { get; set; } = string.Empty;
    public decimal AdjustedBudget { get; set; }
    public decimal ClosingProjection { get; set; }
    public string Message { get; set; } = string.Empty;
}

public class CreateBudgetMovementRequestDto
{
    public Guid BudgetId { get; set; }
    public string MovementType { get; set; } = string.Empty; // "Addition" or "Transfer"
    public Guid? SourceAccountId { get; set; }
    public Guid DestinationAccountId { get; set; }
    public decimal Amount { get; set; }
    public string Justification { get; set; } = string.Empty;
    public string ApprovalType { get; set; } = string.Empty; // "Council" or "Assembly"
    public string MeetingActNumber { get; set; } = string.Empty;
    public DateTime ApprovalDate { get; set; }
}

public class BudgetMovementDto
{
    public Guid Id { get; set; }
    public Guid BudgetId { get; set; }
    public string MovementType { get; set; } = string.Empty;
    public Guid? SourceAccountId { get; set; }
    public string SourceAccountCode { get; set; } = string.Empty;
    public string SourceAccountName { get; set; } = string.Empty;
    public Guid DestinationAccountId { get; set; }
    public string DestinationAccountCode { get; set; } = string.Empty;
    public string DestinationAccountName { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string Justification { get; set; } = string.Empty;
    public string ApprovalType { get; set; } = string.Empty;
    public string MeetingActNumber { get; set; } = string.Empty;
    public DateTime ApprovalDate { get; set; }
}

public class ContingencyFundDto
{
    public string TenantId { get; set; } = string.Empty;
    public decimal CurrentBalance { get; set; }
    public decimal ProjectedClosingBalance { get; set; }
    public List<ContingencyFundContributionDto> Contributions { get; set; } = new();
    public List<ContingencyFundUsageDto> Usages { get; set; } = new();
}

public class ContingencyFundContributionDto
{
    public Guid Id { get; set; }
    public string Period { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public decimal IncomeBase { get; set; }
    public decimal Percentage { get; set; }
    public DateTime ContributionDate { get; set; }
}

public class ContingencyFundUsageDto
{
    public Guid Id { get; set; }
    public decimal Amount { get; set; }
    public string Justification { get; set; } = string.Empty;
    public string CouncilApprovalActNumber { get; set; } = string.Empty;
    public DateTime ApprovalDate { get; set; }
    public string CreatedByUserId { get; set; } = string.Empty;
}

public class LiquidateMonthlyContributionRequestDto
{
    public int Year { get; set; }
    public int Month { get; set; }
}

public class RecordContingencyFundUsageRequestDto
{
    public decimal Amount { get; set; }
    public string Justification { get; set; } = string.Empty;
    public string CouncilApprovalActNumber { get; set; } = string.Empty;
    public DateTime ApprovalDate { get; set; }
}
