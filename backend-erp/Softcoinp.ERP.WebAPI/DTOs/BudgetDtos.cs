using System;
using System.Collections.Generic;

namespace Softcoinp.ERP.WebAPI.DTOs;

public class BudgetSummaryDto
{
    public Guid Id { get; set; }
    public int FiscalYear { get; set; }
    public DateTime? ApprovalDate { get; set; }
    public string MeetingActNumber { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string Observations { get; set; } = string.Empty;
    public int IncomeItemsCount { get; set; }
    public int ExpenseItemsCount { get; set; }
    public decimal TotalIncome { get; set; }
    public decimal TotalExpense { get; set; }
    public string CreatedByUserId { get; set; } = string.Empty;
}

public class BudgetDetailDto
{
    public Guid Id { get; set; }
    public int FiscalYear { get; set; }
    public DateTime? ApprovalDate { get; set; }
    public string MeetingActNumber { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string Observations { get; set; } = string.Empty;
    public List<IncomeItemDto> IncomeItems { get; set; } = new();
    public List<ExpenseItemDto> ExpenseItems { get; set; } = new();
}

public class IncomeItemDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal AnnualValue { get; set; }
    public decimal MonthlyValue => Math.Round(AnnualValue / 12m, 2);
}

public class ExpenseItemDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public decimal AnnualValue { get; set; }
    public decimal MonthlyValue => Math.Round(AnnualValue / 12m, 2);
    public bool IsContingencyFund { get; set; }
    public decimal ContingencyPercentage { get; set; }
    public bool RequiresCouncilApproval { get; set; }
    public decimal ApprovalThreshold { get; set; }
}

public class CreateBudgetRequestDto
{
    public int FiscalYear { get; set; }
    public string MeetingActNumber { get; set; } = string.Empty;
    public DateTime? ApprovalDate { get; set; }
    public string Observations { get; set; } = string.Empty;
    public bool CopyFromPrevious { get; set; }
    public decimal? GlobalPercentageAdjustment { get; set; }
    public List<CreateIncomeItemDto>? IncomeItems { get; set; }
    public List<CreateExpenseItemDto>? ExpenseItems { get; set; }
}

public class CreateIncomeItemDto
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal AnnualValue { get; set; }
}

public class CreateExpenseItemDto
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Category { get; set; } = "Variable";
    public decimal AnnualValue { get; set; }
    public bool IsContingencyFund { get; set; }
    public decimal ContingencyPercentage { get; set; }
    public bool RequiresCouncilApproval { get; set; }
    public decimal ApprovalThreshold { get; set; }
}

public class ApproveBudgetRequestDto
{
    public string MeetingActNumber { get; set; } = string.Empty;
    public DateTime ApprovalDate { get; set; }
}

public class BudgetExecutionDashboardDto
{
    public Guid BudgetId { get; set; }
    public int FiscalYear { get; set; }
    public string Status { get; set; } = string.Empty;
    public decimal TotalApprovedIncome { get; set; }
    public decimal TotalApprovedExpense { get; set; }
    public decimal TotalExecutedExpense { get; set; }
    public decimal TotalAvailable { get; set; }
    public decimal OverallExecutionPercentage { get; set; }
    public List<ExpenseExecutionItemDto> ExpenseItems { get; set; } = new();
    public List<BudgetAlertDto> Alerts { get; set; } = new();
}

public class ExpenseExecutionItemDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public decimal AnnualValue { get; set; }
    public decimal MonthlyValue => Math.Round(AnnualValue / 12m, 2);
    public decimal ProportionalToDate { get; set; }
    public decimal ExecutedValue { get; set; }
    public decimal AvailableValue { get; set; }
    public decimal ExecutionPercentage { get; set; }
    public string TrafficLight { get; set; } = "Green";
    public bool IsContingencyFund { get; set; }
    public decimal ContingencyPercentage { get; set; }
    public bool RequiresCouncilApproval { get; set; }
    public decimal ApprovalThreshold { get; set; }
}

public class BudgetAlertDto
{
    public string ItemName { get; set; } = string.Empty;
    public decimal AnnualValue { get; set; }
    public decimal ExecutedValue { get; set; }
    public decimal ExecutionPercentage { get; set; }
    public string Message { get; set; } = string.Empty;
    public string Severity { get; set; } = "Warning";
}

public class RecordExpenseRequestDto
{
    public Guid ExpenseItemId { get; set; }
    public string Description { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public DateTime ExpenseDate { get; set; }
    public Guid? ProviderId { get; set; }
    public string InvoiceReference { get; set; } = string.Empty;
}

public class ExecutedExpenseDto
{
    public Guid Id { get; set; }
    public Guid ExpenseItemId { get; set; }
    public string ExpenseItemName { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public DateTime ExpenseDate { get; set; }
    public Guid? ProviderId { get; set; }
    public string ProviderName { get; set; } = string.Empty;
    public string InvoiceReference { get; set; } = string.Empty;
    public bool CouncilApproved { get; set; }
    public bool RequiresCouncilApproval { get; set; }
}

public class CreateModificationRequestDto
{
    public Guid BudgetId { get; set; }
    public Guid? ExpenseItemId { get; set; }
    public Guid? IncomeItemId { get; set; }
    public string ModificationType { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string Justification { get; set; } = string.Empty;
    public string ApprovalType { get; set; } = string.Empty;
    public string MeetingActNumber { get; set; } = string.Empty;
    public DateTime ApprovalDate { get; set; }
}

public class BudgetModificationDto
{
    public Guid Id { get; set; }
    public Guid BudgetId { get; set; }
    public Guid? ExpenseItemId { get; set; }
    public string ExpenseItemName { get; set; } = string.Empty;
    public Guid? IncomeItemId { get; set; }
    public string IncomeItemName { get; set; } = string.Empty;
    public string ModificationType { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public decimal PreviousValue { get; set; }
    public decimal NewValue { get; set; }
    public string Justification { get; set; } = string.Empty;
    public string ApprovalType { get; set; } = string.Empty;
    public string MeetingActNumber { get; set; } = string.Empty;
    public DateTime ApprovalDate { get; set; }
}

public class ContingencyFundStatusDto
{
    public string TenantId { get; set; } = string.Empty;
    public decimal TotalContributed { get; set; }
    public decimal TotalUsed { get; set; }
    public decimal AvailableBalance { get; set; }
    public decimal ContingencyPercentage { get; set; }
    public List<ContingencyFundUsageDto> Usages { get; set; } = new();
}

public class ContingencyFundUsageDto
{
    public Guid Id { get; set; }
    public string Justification { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string CouncilApprovalActNumber { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}

public class RecordContingencyFundUsageRequestDto
{
    public Guid BudgetId { get; set; }
    public string Justification { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string CouncilApprovalActNumber { get; set; } = string.Empty;
    public Guid? ExecutedExpenseId { get; set; }
}
