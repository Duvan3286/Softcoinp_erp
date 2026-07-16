using System;

namespace Softcoinp.ERP.WebAPI.DTOs;

// ═══════════════════════════════════════════════════════════════════════
// REQUEST DTOs - Providers
// ═══════════════════════════════════════════════════════════════════════

public class CreateProviderRequestDto
{
    public string ProviderType { get; set; } = string.Empty;
    public string DocumentType { get; set; } = string.Empty;
    public string DocumentNumber { get; set; } = string.Empty;
    public string BusinessName { get; set; } = string.Empty;
    public string ContactName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string ServiceType { get; set; } = string.Empty;
    public string RutFilePath { get; set; } = string.Empty;
    public string ChamberOfCommerceFilePath { get; set; } = string.Empty;
}

public class UpdateProviderRequestDto
{
    public string? ProviderType { get; set; }
    public string? DocumentType { get; set; }
    public string? DocumentNumber { get; set; }
    public string? BusinessName { get; set; }
    public string? ContactName { get; set; }
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string? Address { get; set; }
    public string? ServiceType { get; set; }
    public string? RutFilePath { get; set; }
    public string? ChamberOfCommerceFilePath { get; set; }
    public string? Status { get; set; }
}

// ═══════════════════════════════════════════════════════════════════════
// REQUEST DTOs - Contracts
// ═══════════════════════════════════════════════════════════════════════

public class CreateContractRequestDto
{
    public Guid ProviderId { get; set; }
    public string ContractNumber { get; set; } = string.Empty;
    public string ContractType { get; set; } = string.Empty;
    public string ObjectDescription { get; set; } = string.Empty;
    public decimal TotalValue { get; set; }
    public decimal MonthlyValue { get; set; }
    public bool IsRecurrent { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public bool HasAutoRenewal { get; set; }
    public int AutoRenewalNoticeDays { get; set; }
    public string Observations { get; set; } = string.Empty;
}

public class UpdateContractRequestDto
{
    public string? ContractType { get; set; }
    public string? ObjectDescription { get; set; }
    public decimal? TotalValue { get; set; }
    public decimal? MonthlyValue { get; set; }
    public bool? IsRecurrent { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public bool? HasAutoRenewal { get; set; }
    public int? AutoRenewalNoticeDays { get; set; }
    public string? SignedContractFilePath { get; set; }
    public Guid? ApprovedInAssemblyId { get; set; }
    public string? Observations { get; set; }
}

public class ChangeContractStatusRequestDto
{
    public string NewStatus { get; set; } = string.Empty;
}

// ═══════════════════════════════════════════════════════════════════════
// REQUEST DTOs - Invoices
// ═══════════════════════════════════════════════════════════════════════

public class CreateProviderInvoiceRequestDto
{
    public Guid ProviderId { get; set; }
    public Guid? ContractId { get; set; }
    public string InvoiceNumber { get; set; } = string.Empty;
    public DateTime InvoiceDate { get; set; }
    public DateTime DueDate { get; set; }
    public decimal TotalAmount { get; set; }
    public decimal AmountPaid { get; set; }
    public DateTime? PaymentDate { get; set; }
    public string? PaymentMethod { get; set; }
    public string? PaymentReferenceNumber { get; set; }
    public Guid? BudgetItemId { get; set; }
}

public class CreateProviderPaymentRequestDto
{
    public decimal Amount { get; set; }
    public DateTime PaymentDate { get; set; }
    public string PaymentMethod { get; set; } = string.Empty;
    public string ReferenceNumber { get; set; } = string.Empty;
}

// ═══════════════════════════════════════════════════════════════════════
// REQUEST DTOs - Evaluations
// ═══════════════════════════════════════════════════════════════════════

public class CreateProviderEvaluationRequestDto
{
    public string EvaluationPeriod { get; set; } = string.Empty;
    public int QualityScore { get; set; }
    public int ComplianceScore { get; set; }
    public int PriceScore { get; set; }
    public int AttentionScore { get; set; }
    public string Comments { get; set; } = string.Empty;
}

// ═══════════════════════════════════════════════════════════════════════
// REQUEST DTOs - Approval Thresholds
// ═══════════════════════════════════════════════════════════════════════

public class CreateApprovalThresholdRequestDto
{
    public decimal MinValue { get; set; }
    public decimal MaxValue { get; set; }
    public string Description { get; set; } = string.Empty;
}

public class UpdateApprovalThresholdRequestDto
{
    public decimal? MinValue { get; set; }
    public decimal? MaxValue { get; set; }
    public string? Description { get; set; }
    public bool? IsActive { get; set; }
}

// ═══════════════════════════════════════════════════════════════════════
// RESPONSE DTOs - Providers
// ═══════════════════════════════════════════════════════════════════════

public class ProviderListDto
{
    public Guid Id { get; set; }
    public string ProviderType { get; set; } = string.Empty;
    public string DocumentNumber { get; set; } = string.Empty;
    public string BusinessName { get; set; } = string.Empty;
    public string ContactName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string ServiceType { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public int ContractCount { get; set; }
    public int ActiveContractCount { get; set; }
    public decimal AverageEvaluationScore { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class ProviderDetailDto
{
    public Guid Id { get; set; }
    public string ProviderType { get; set; } = string.Empty;
    public string DocumentType { get; set; } = string.Empty;
    public string DocumentNumber { get; set; } = string.Empty;
    public string BusinessName { get; set; } = string.Empty;
    public string ContactName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string ServiceType { get; set; } = string.Empty;
    public string RutFilePath { get; set; } = string.Empty;
    public string ChamberOfCommerceFilePath { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public List<ProviderContractSummaryDto> Contracts { get; set; } = new();
    public List<ProviderInvoiceSummaryDto> Invoices { get; set; } = new();
    public List<ProviderEvaluationSummaryDto> Evaluations { get; set; } = new();
}

public class ProviderContractSummaryDto
{
    public Guid Id { get; set; }
    public string ContractNumber { get; set; } = string.Empty;
    public string ContractType { get; set; } = string.Empty;
    public decimal TotalValue { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public string Status { get; set; } = string.Empty;
    public int DaysUntilExpiration { get; set; }
}

public class ProviderInvoiceSummaryDto
{
    public Guid Id { get; set; }
    public string InvoiceNumber { get; set; } = string.Empty;
    public string ContractNumber { get; set; } = string.Empty;
    public decimal TotalAmount { get; set; }
    public decimal AmountPaid { get; set; }
    public decimal PendingAmount { get; set; }
    public DateTime DueDate { get; set; }
    public string Status { get; set; } = string.Empty;
    public string BudgetItemName { get; set; } = string.Empty;
}

public class ProviderEvaluationSummaryDto
{
    public Guid Id { get; set; }
    public string EvaluationPeriod { get; set; } = string.Empty;
    public decimal AverageScore { get; set; }
    public string Recommendation { get; set; } = string.Empty;
    public string EvaluatedByUserName { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}

// ═══════════════════════════════════════════════════════════════════════
// RESPONSE DTOs - Contracts
// ═══════════════════════════════════════════════════════════════════════

public class ContractListDto
{
    public Guid Id { get; set; }
    public string ContractNumber { get; set; } = string.Empty;
    public string ContractType { get; set; } = string.Empty;
    public string ProviderBusinessName { get; set; } = string.Empty;
    public decimal TotalValue { get; set; }
    public decimal MonthlyValue { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public bool HasAutoRenewal { get; set; }
    public string Status { get; set; } = string.Empty;
    public int DaysUntilExpiration { get; set; }
    public int AlertCount { get; set; }
}

public class ContractDetailDto
{
    public Guid Id { get; set; }
    public Guid ProviderId { get; set; }
    public string ProviderBusinessName { get; set; } = string.Empty;
    public string ProviderDocumentNumber { get; set; } = string.Empty;
    public string ContractNumber { get; set; } = string.Empty;
    public string ContractType { get; set; } = string.Empty;
    public string ObjectDescription { get; set; } = string.Empty;
    public decimal TotalValue { get; set; }
    public decimal MonthlyValue { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public bool HasAutoRenewal { get; set; }
    public int AutoRenewalNoticeDays { get; set; }
    public Guid? ApprovedInAssemblyId { get; set; }
    public string ApprovedInAssemblyTitle { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string SignedContractFilePath { get; set; } = string.Empty;
    public string Observations { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public int DaysUntilExpiration { get; set; }
    public List<ContractAlertDto> Alerts { get; set; } = new();
    public List<ContractInvoiceDto> Invoices { get; set; } = new();
}

public class ContractInvoiceDto
{
    public Guid Id { get; set; }
    public string InvoiceNumber { get; set; } = string.Empty;
    public DateTime InvoiceDate { get; set; }
    public DateTime DueDate { get; set; }
    public decimal TotalAmount { get; set; }
    public decimal AmountPaid { get; set; }
    public decimal PendingAmount { get; set; }
    public string Status { get; set; } = string.Empty;
    public List<ProviderPaymentDto> Payments { get; set; } = new();
}

public class ContractAlertDto
{
    public Guid Id { get; set; }
    public string AlertType { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public DateTime GeneratedAt { get; set; }
    public bool IsActive { get; set; }
    public DateTime? ResolvedAt { get; set; }
    public string? ResolvedByUserId { get; set; }
}

public class ProviderPaymentDto
{
    public Guid Id { get; set; }
    public decimal Amount { get; set; }
    public DateTime PaymentDate { get; set; }
    public string PaymentMethod { get; set; } = string.Empty;
    public string ReferenceNumber { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
}

// ═══════════════════════════════════════════════════════════════════════
// RESPONSE DTOs - Approval Thresholds
// ═══════════════════════════════════════════════════════════════════════

public class ApprovalThresholdDto
{
    public Guid Id { get; set; }
    public decimal MinValue { get; set; }
    public decimal MaxValue { get; set; }
    public string Description { get; set; } = string.Empty;
    public bool IsActive { get; set; }
}

// ═══════════════════════════════════════════════════════════════════════
// RESPONSE DTOs - Indicators / Dashboard
// ═══════════════════════════════════════════════════════════════════════

public class ProviderIndicatorsDto
{
    public int TotalProviders { get; set; }
    public int ActiveProviders { get; set; }
    public int InactiveProviders { get; set; }
    public int TotalContracts { get; set; }
    public int ActiveContracts { get; set; }
    public int ExpiringContracts { get; set; }
    public decimal TotalContractValue { get; set; }
    public decimal MonthlyContractValue { get; set; }
    public int PendingPaymentInvoices { get; set; }
    public decimal PendingPaymentAmount { get; set; }
    public int OverdueInvoices { get; set; }
    public int ActiveAlerts { get; set; }
}

// ═══════════════════════════════════════════════════════════════════════
// RESPONSE DTOs - Pending Payments Panel
// ═══════════════════════════════════════════════════════════════════════

public class PendingPaymentDto
{
    public Guid InvoiceId { get; set; }
    public string InvoiceNumber { get; set; } = string.Empty;
    public string ProviderName { get; set; } = string.Empty;
    public string ProviderDocumentNumber { get; set; } = string.Empty;
    public Guid? ContractId { get; set; }
    public string ContractNumber { get; set; } = string.Empty;
    public decimal TotalAmount { get; set; }
    public decimal AmountPaid { get; set; }
    public decimal PendingAmount { get; set; }
    public DateTime DueDate { get; set; }
    public int DaysOverdue { get; set; }
    public string Status { get; set; } = string.Empty;
}

// ═══════════════════════════════════════════════════════════════════════
// RESPONSE DTOs - Contract Expiration Report
// ═══════════════════════════════════════════════════════════════════════

public class ContractExpirationReportDto
{
    public Guid ContractId { get; set; }
    public string ContractNumber { get; set; } = string.Empty;
    public string ContractType { get; set; } = string.Empty;
    public string ProviderName { get; set; } = string.Empty;
    public string ProviderDocumentNumber { get; set; } = string.Empty;
    public decimal TotalValue { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public int DaysUntilExpiration { get; set; }
    public bool HasAutoRenewal { get; set; }
    public int AutoRenewalNoticeDays { get; set; }
    public string Status { get; set; } = string.Empty;
}
