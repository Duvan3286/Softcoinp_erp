using System;
using System.Collections.Generic;

namespace Softcoinp.ERP.WebAPI.DTOs;

// ═══════════════════════════════════════════════════════════════════════
// REQUEST DTOs - Proveedores
// ═══════════════════════════════════════════════════════════════════════

public class CreateProviderRequestDto
{
    public string ProviderType { get; set; } = string.Empty;
    public string DocumentType { get; set; } = string.Empty;
    public string DocumentNumber { get; set; } = string.Empty;
    public string VerificationDigit { get; set; } = string.Empty;
    public string BusinessName { get; set; } = string.Empty;
    public string TradeName { get; set; } = string.Empty;
    public string ContactName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string EconomicActivity { get; set; } = string.Empty;
    public string ServiceType { get; set; } = string.Empty;
    public string RutFilePath { get; set; } = string.Empty;
    public string LegalRepDocumentType { get; set; } = string.Empty;
    public string LegalRepDocumentNumber { get; set; } = string.Empty;
    public string LegalRepName { get; set; } = string.Empty;
    public string LegalRepEmail { get; set; } = string.Empty;
    public bool IsPreferred { get; set; }
}

public class UpdateProviderRequestDto
{
    public string? ProviderType { get; set; }
    public string? DocumentType { get; set; }
    public string? DocumentNumber { get; set; }
    public string? VerificationDigit { get; set; }
    public string? BusinessName { get; set; }
    public string? TradeName { get; set; }
    public string? ContactName { get; set; }
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string? Address { get; set; }
    public string? City { get; set; }
    public string? EconomicActivity { get; set; }
    public string? ServiceType { get; set; }
    public string? RutFilePath { get; set; }
    public string? LegalRepDocumentType { get; set; }
    public string? LegalRepDocumentNumber { get; set; }
    public string? LegalRepName { get; set; }
    public string? LegalRepEmail { get; set; }
    public bool? IsPreferred { get; set; }
    public string? Status { get; set; }
}

// ═══════════════════════════════════════════════════════════════════════
// REQUEST DTOs - Contratos
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
    public Guid? BudgetAccountId { get; set; }
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
    public Guid? BudgetAccountId { get; set; }
    public string? SignedContractFilePath { get; set; }
}

public class ChangeContractStatusRequestDto
{
    public string NewStatus { get; set; } = string.Empty;
    public string Justification { get; set; } = string.Empty;
}

// ═══════════════════════════════════════════════════════════════════════
// REQUEST DTOs - Pólizas
// ═══════════════════════════════════════════════════════════════════════

public class CreateContractPolicyRequestDto
{
    public string PolicyNumber { get; set; } = string.Empty;
    public string InsuranceCompany { get; set; } = string.Empty;
    public string PolicyType { get; set; } = string.Empty;
    public decimal InsuredAmount { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public string FilePath { get; set; } = string.Empty;
}

// ═══════════════════════════════════════════════════════════════════════
// REQUEST DTOs - Facturas
// ═══════════════════════════════════════════════════════════════════════

public class CreateProviderInvoiceRequestDto
{
    public Guid ProviderId { get; set; }
    public Guid? ContractId { get; set; }
    public string InvoiceNumber { get; set; } = string.Empty;
    public DateTime InvoiceDate { get; set; }
    public DateTime DueDate { get; set; }
    public decimal Subtotal { get; set; }
    public decimal IvaAmount { get; set; }
    public decimal RetentionFuelAmount { get; set; }
    public decimal RetentionIcaAmount { get; set; }
    public string Description { get; set; } = string.Empty;
    public string InvoiceFilePath { get; set; } = string.Empty;
}

// ═══════════════════════════════════════════════════════════════════════
// REQUEST DTOs - Pagos
// ═══════════════════════════════════════════════════════════════════════

public class CreateProviderPaymentRequestDto
{
    public decimal Amount { get; set; }
    public DateTime PaymentDate { get; set; }
    public string PaymentMethod { get; set; } = string.Empty;
    public string ReferenceNumber { get; set; } = string.Empty;
    public string BankAccount { get; set; } = string.Empty;
    public string Notes { get; set; } = string.Empty;
    public string ReceiptFilePath { get; set; } = string.Empty;
}

// ═══════════════════════════════════════════════════════════════════════
// REQUEST DTOs - Evaluaciones
// ═══════════════════════════════════════════════════════════════════════

public class CreateProviderEvaluationRequestDto
{
    public Guid? ContractId { get; set; }
    public string EvaluationPeriod { get; set; } = string.Empty;
    public int ServiceQualityScore { get; set; }
    public int ComplianceScore { get; set; }
    public int PriceFairnessScore { get; set; }
    public int AfterSalesScore { get; set; }
    public string Comments { get; set; } = string.Empty;
}

// ═══════════════════════════════════════════════════════════════════════
// REQUEST DTOs - Configuración
// ═══════════════════════════════════════════════════════════════════════

public class CreateRetentionConfigurationRequestDto
{
    public string ServiceType { get; set; } = string.Empty;
    public string ServiceDescription { get; set; } = string.Empty;
    public decimal RetentionFuelRate { get; set; }
    public decimal RetentionIcaRate { get; set; }
}

public class UpdateRetentionConfigurationRequestDto
{
    public string? ServiceDescription { get; set; }
    public decimal? RetentionFuelRate { get; set; }
    public decimal? RetentionIcaRate { get; set; }
    public bool? IsActive { get; set; }
}

public class CreateApprovalThresholdRequestDto
{
    public string ApprovalLevel { get; set; } = string.Empty;
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
// RESPONSE DTOs - Proveedores
// ═══════════════════════════════════════════════════════════════════════

public class ProviderListDto
{
    public Guid Id { get; set; }
    public string ProviderType { get; set; } = string.Empty;
    public string DocumentNumber { get; set; } = string.Empty;
    public string BusinessName { get; set; } = string.Empty;
    public string TradeName { get; set; } = string.Empty;
    public string ContactName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string ServiceType { get; set; } = string.Empty;
    public bool IsPreferred { get; set; }
    public string Status { get; set; } = string.Empty;
    public int ContractCount { get; set; }
    public int ActiveContractCount { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class ProviderDetailDto
{
    public Guid Id { get; set; }
    public string ProviderType { get; set; } = string.Empty;
    public string DocumentType { get; set; } = string.Empty;
    public string DocumentNumber { get; set; } = string.Empty;
    public string VerificationDigit { get; set; } = string.Empty;
    public string BusinessName { get; set; } = string.Empty;
    public string TradeName { get; set; } = string.Empty;
    public string ContactName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string EconomicActivity { get; set; } = string.Empty;
    public string ServiceType { get; set; } = string.Empty;
    public string RutFilePath { get; set; } = string.Empty;
    public string LegalRepDocumentType { get; set; } = string.Empty;
    public string LegalRepDocumentNumber { get; set; } = string.Empty;
    public string LegalRepName { get; set; } = string.Empty;
    public string LegalRepEmail { get; set; } = string.Empty;
    public bool IsPreferred { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public List<ProviderContractSummaryDto> Contracts { get; set; } = new();
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
// RESPONSE DTOs - Contratos
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
    public string ApprovalLevel { get; set; } = string.Empty;
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
    public bool IsRecurrent { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public bool HasAutoRenewal { get; set; }
    public int AutoRenewalNoticeDays { get; set; }
    public string ApprovalLevel { get; set; } = string.Empty;
    public string CouncilMeetingActNumber { get; set; } = string.Empty;
    public string AssemblyMeetingActNumber { get; set; } = string.Empty;
    public string BudgetAccountId { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string SignedContractFilePath { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public int DaysUntilExpiration { get; set; }
    public List<ContractPolicyDto> Policies { get; set; } = new();
    public List<ContractAlertDto> Alerts { get; set; } = new();
    public List<ContractInvoiceDto> Invoices { get; set; } = new();
}

public class ContractPolicyDto
{
    public Guid Id { get; set; }
    public string PolicyNumber { get; set; } = string.Empty;
    public string InsuranceCompany { get; set; } = string.Empty;
    public string PolicyType { get; set; } = string.Empty;
    public decimal InsuredAmount { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public string FilePath { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public int DaysUntilExpiration { get; set; }
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
    public bool EscalatedToCouncil { get; set; }
}

public class ContractInvoiceDto
{
    public Guid Id { get; set; }
    public string InvoiceNumber { get; set; } = string.Empty;
    public DateTime InvoiceDate { get; set; }
    public DateTime DueDate { get; set; }
    public decimal Subtotal { get; set; }
    public decimal IvaAmount { get; set; }
    public decimal RetentionFuelAmount { get; set; }
    public decimal RetentionIcaAmount { get; set; }
    public decimal NetAmount { get; set; }
    public string Status { get; set; } = string.Empty;
    public decimal PendingAmount { get; set; }
    public List<ProviderPaymentDto> Payments { get; set; } = new();
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
// RESPONSE DTOs - Configuración
// ═══════════════════════════════════════════════════════════════════════

public class RetentionConfigurationDto
{
    public Guid Id { get; set; }
    public string ServiceType { get; set; } = string.Empty;
    public string ServiceDescription { get; set; } = string.Empty;
    public decimal RetentionFuelRate { get; set; }
    public decimal RetentionIcaRate { get; set; }
    public bool IsActive { get; set; }
}

public class ApprovalThresholdDto
{
    public Guid Id { get; set; }
    public string ApprovalLevel { get; set; } = string.Empty;
    public decimal MinValue { get; set; }
    public decimal MaxValue { get; set; }
    public string Description { get; set; } = string.Empty;
    public bool IsActive { get; set; }
}

// ═══════════════════════════════════════════════════════════════════════
// RESPONSE DTOs - Indicadores / Dashboard
// ═══════════════════════════════════════════════════════════════════════

public class ProviderIndicatorsDto
{
    public int TotalProviders { get; set; }
    public int ActiveProviders { get; set; }
    public int InactiveProviders { get; set; }
    public int PreferredProviders { get; set; }
    public int TotalContracts { get; set; }
    public int ActiveContracts { get; set; }
    public int ExpiringContracts { get; set; }
    public decimal TotalContractValue { get; set; }
    public decimal MonthlyContractValue { get; set; }
    public int PendingInvoices { get; set; }
    public decimal PendingInvoiceAmount { get; set; }
    public int OverdueInvoices { get; set; }
    public int ActiveAlerts { get; set; }
    public int ExpiringPolicies { get; set; }
}

public class RetentionCalculationDto
{
    public decimal Subtotal { get; set; }
    public decimal IvaAmount { get; set; }
    public decimal RetentionFuelAmount { get; set; }
    public decimal RetentionIcaAmount { get; set; }
    public decimal TotalRetentions { get; set; }
    public decimal NetAmount { get; set; }
    public List<RetentionDetailDto> Details { get; set; } = new();
}

public class RetentionDetailDto
{
    public string ServiceType { get; set; } = string.Empty;
    public decimal Rate { get; set; }
    public decimal BaseAmount { get; set; }
    public decimal RetentionAmount { get; set; }
}
