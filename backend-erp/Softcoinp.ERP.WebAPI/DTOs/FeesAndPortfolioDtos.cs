using System;
using System.Collections.Generic;

namespace Softcoinp.ERP.WebAPI.DTOs;

public class ExecuteBillingRequestDto
{
    public string Period { get; set; } = string.Empty;
    public DateTime CutoffDate { get; set; }
    public DateTime PaymentDueDate { get; set; }
}

public class BillingPeriodSummaryDto
{
    public Guid Id { get; set; }
    public string Period { get; set; } = string.Empty;
    public decimal MonthlyBudgetTotal { get; set; }
    public DateTime CutoffDate { get; set; }
    public DateTime PaymentDueDate { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime? ExecutedAt { get; set; }
    public string ExecutedByUserId { get; set; } = string.Empty;
    public decimal RoundingAdjustment { get; set; }
    public int UnitsCount { get; set; }
    public decimal TotalBilled { get; set; }
}

public class BillingPeriodDetailDto
{
    public Guid Id { get; set; }
    public string Period { get; set; } = string.Empty;
    public decimal MonthlyBudgetTotal { get; set; }
    public DateTime CutoffDate { get; set; }
    public DateTime PaymentDueDate { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime? ExecutedAt { get; set; }
    public string ExecutedByUserId { get; set; } = string.Empty;
    public decimal RoundingAdjustment { get; set; }
    public string Notes { get; set; } = string.Empty;
    public List<UnitFeeDto> UnitFees { get; set; } = new();
}

public class UnitFeeDto
{
    public Guid Id { get; set; }
    public Guid UnitId { get; set; }
    public string UnitIdentifier { get; set; } = string.Empty;
    public string UnitTower { get; set; } = string.Empty;
    public decimal Coefficient { get; set; }
    public decimal FeeValue { get; set; }
    public DateTime DueDate { get; set; }
    public string Status { get; set; } = string.Empty;
    public decimal PaidAmount { get; set; }
    public decimal BalanceAmount { get; set; }
}

public class BillingChecklistDto
{
    public bool HasActiveBudget { get; set; }
    public bool CoeficientSumIsHundred { get; set; }
    public decimal CoeficientSum { get; set; }
    public bool NoExistingBillingForPeriod { get; set; }
    public int ActiveUnitsCount { get; set; }
    public decimal MonthlyBudgetTotal { get; set; }
    public bool AllChecksPass { get; set; }
    public List<string> Warnings { get; set; } = new();
}

public class PortfolioSummaryDto
{
    public decimal TotalBilled { get; set; }
    public decimal TotalCollected { get; set; }
    public decimal TotalOutstanding { get; set; }
    public decimal CollectionRate { get; set; }
    public int UnitsWithDebt { get; set; }
    public int TotalUnits { get; set; }
    public List<AgingBucketDto> AgingBuckets { get; set; } = new();
}

public class AgingBucketDto
{
    public string Bucket { get; set; } = string.Empty;
    public int UnitCount { get; set; }
    public decimal TotalDebt { get; set; }
}

public class UnitStatementDto
{
    public Guid UnitId { get; set; }
    public string UnitIdentifier { get; set; } = string.Empty;
    public string UnitTower { get; set; } = string.Empty;
    public decimal OpeningBalance { get; set; }
    public decimal TotalCharges { get; set; }
    public decimal TotalPayments { get; set; }
    public decimal TotalInterest { get; set; }
    public decimal ClosingBalance { get; set; }
    public List<StatementLineDto> Lines { get; set; } = new();
}

public class StatementLineDto
{
    public DateTime Date { get; set; }
    public string Description { get; set; } = string.Empty;
    public string Reference { get; set; } = string.Empty;
    public decimal Debit { get; set; }
    public decimal Credit { get; set; }
    public decimal Balance { get; set; }
}

public class LateInterestPreviewDto
{
    public string SourceType { get; set; } = string.Empty;
    public Guid SourceId { get; set; }
    public decimal BalanceAmount { get; set; }
    public int DaysOverdue { get; set; }
    public decimal DailyRate { get; set; }
    public decimal CalculatedInterest { get; set; }
}

public class LateInterestSummaryDto
{
    public Guid Id { get; set; }
    public string Period { get; set; } = string.Empty;
    public decimal BaseAmount { get; set; }
    public decimal DailyRate { get; set; }
    public int DaysOverdue { get; set; }
    public decimal CalculatedAmount { get; set; }
    public bool IsCapitalized { get; set; }
}

public class CapitalizeInterestRequestDto
{
    public Guid UnitFeeId { get; set; }
    public string Period { get; set; } = string.Empty;
}

public class CapitalizeAllInterestRequestDto
{
    public string Period { get; set; } = string.Empty;
}

public class LateInterestRateConfigDto
{
    public decimal MonthlyRate { get; set; }
    public decimal MaxLegalRate { get; set; }
    public decimal DailyRate { get; set; }
}

public class RegisterPaymentRequestDto
{
    public Guid UnitId { get; set; }
    public DateTime PaymentDate { get; set; }
    public decimal Amount { get; set; }
    public string PaymentMethod { get; set; } = string.Empty;
    public string ReferenceNumber { get; set; } = string.Empty;
    public string Notes { get; set; } = string.Empty;
}

public class PaymentPreviewDto
{
    public decimal TotalPayment { get; set; }
    public decimal AllocatedToInterest { get; set; }
    public decimal AllocatedToCapital { get; set; }
    public decimal AdvanceAmount { get; set; }
    public List<PaymentAllocationPreviewDto> Allocations { get; set; } = new();
}

public class PaymentAllocationPreviewDto
{
    public string SourceType { get; set; } = string.Empty;
    public Guid SourceId { get; set; }
    public string Description { get; set; } = string.Empty;
    public decimal AllocatedAmount { get; set; }
}

public class PaymentDto
{
    public Guid Id { get; set; }
    public Guid UnitId { get; set; }
    public string UnitIdentifier { get; set; } = string.Empty;
    public DateTime PaymentDate { get; set; }
    public decimal Amount { get; set; }
    public string PaymentMethod { get; set; } = string.Empty;
    public string ReferenceNumber { get; set; } = string.Empty;
    public string Notes { get; set; } = string.Empty;
    public decimal AdvanceAmount { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class PaymentDetailDto
{
    public Guid Id { get; set; }
    public Guid UnitId { get; set; }
    public string UnitIdentifier { get; set; } = string.Empty;
    public DateTime PaymentDate { get; set; }
    public decimal Amount { get; set; }
    public string PaymentMethod { get; set; } = string.Empty;
    public string ReferenceNumber { get; set; } = string.Empty;
    public string Notes { get; set; } = string.Empty;
    public decimal AdvanceAmount { get; set; }
    public DateTime CreatedAt { get; set; }
    public List<PaymentAllocationDto> Allocations { get; set; } = new();
}

public class PaymentAllocationDto
{
    public Guid Id { get; set; }
    public string SourceType { get; set; } = string.Empty;
    public Guid? SourceId { get; set; }
    public decimal Amount { get; set; }
    public string AllocationType { get; set; } = string.Empty;
}

public class UnitDebtSummaryDto
{
    public Guid UnitId { get; set; }
    public string UnitIdentifier { get; set; } = string.Empty;
    public decimal TotalDebt { get; set; }
    public decimal TotalOverdue { get; set; }
    public decimal TotalInterestAccrued { get; set; }
    public decimal AdvanceBalance { get; set; }
    public List<DebtItemDto> Items { get; set; } = new();
}

public class DebtItemDto
{
    public string SourceType { get; set; } = string.Empty;
    public Guid SourceId { get; set; }
    public string Description { get; set; } = string.Empty;
    public DateTime DueDate { get; set; }
    public decimal Amount { get; set; }
    public decimal Balance { get; set; }
    public bool IsOverdue { get; set; }
}

public class CreatePaymentAgreementRequestDto
{
    public Guid UnitId { get; set; }
    public decimal TotalDebtIncluded { get; set; }
    public int NumberOfInstallments { get; set; }
    public decimal InterestForgivenessPercentage { get; set; }
    public string CouncilActNumber { get; set; } = string.Empty;
    public string DigitalAcceptance { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
}

public class AgreementSimulationDto
{
    public decimal TotalDebt { get; set; }
    public decimal InterestForgivenessPercentage { get; set; }
    public decimal ForgivenAmount { get; set; }
    public decimal NetDebt { get; set; }
    public int NumberOfInstallments { get; set; }
    public decimal InstallmentAmount { get; set; }
    public List<SimulatedInstallmentDto> Installments { get; set; } = new();
}

public class SimulatedInstallmentDto
{
    public int Number { get; set; }
    public DateTime DueDate { get; set; }
    public decimal Amount { get; set; }
}

public class PaymentAgreementSummaryDto
{
    public Guid Id { get; set; }
    public Guid UnitId { get; set; }
    public string UnitIdentifier { get; set; } = string.Empty;
    public decimal TotalDebtIncluded { get; set; }
    public decimal InstallmentAmount { get; set; }
    public int NumberOfInstallments { get; set; }
    public decimal InterestForgivenessPercentage { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime StartedAt { get; set; }
    public DateTime? DefaultedAt { get; set; }
    public int PaidInstallments { get; set; }
    public int OverdueInstallments { get; set; }
}

public class PaymentAgreementDetailDto
{
    public Guid Id { get; set; }
    public Guid UnitId { get; set; }
    public string UnitIdentifier { get; set; } = string.Empty;
    public decimal TotalDebtIncluded { get; set; }
    public decimal InstallmentAmount { get; set; }
    public int NumberOfInstallments { get; set; }
    public decimal InterestForgivenessPercentage { get; set; }
    public string CouncilActNumber { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime StartedAt { get; set; }
    public DateTime? DefaultedAt { get; set; }
    public string DigitalAcceptance { get; set; } = string.Empty;
    public List<AgreementInstallmentDto> Installments { get; set; } = new();
}

public class AgreementInstallmentDto
{
    public Guid Id { get; set; }
    public int InstallmentNumber { get; set; }
    public DateTime DueDate { get; set; }
    public decimal Amount { get; set; }
    public decimal PaidAmount { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime? PaidAt { get; set; }
}

public class StatementRequestDto
{
    public Guid UnitId { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
}

public class ClearanceCertificateDto
{
    public Guid Id { get; set; }
    public Guid UnitId { get; set; }
    public string UnitIdentifier { get; set; } = string.Empty;
    public string CertificateNumber { get; set; } = string.Empty;
    public DateTime IssueDate { get; set; }
    public DateTime ExpirationDate { get; set; }
    public decimal BalanceAtDate { get; set; }
    public string Status { get; set; } = string.Empty;
    public string IssuedByUserId { get; set; } = string.Empty;
    public string SignedByAdministratorName { get; set; } = string.Empty;
}

public class IssueClearanceCertificateRequestDto
{
    public Guid UnitId { get; set; }
    public int ValidityDays { get; set; } = 30;
}

public class ClearanceCertificateSummaryDto
{
    public Guid Id { get; set; }
    public Guid UnitId { get; set; }
    public string UnitIdentifier { get; set; } = string.Empty;
    public string CertificateNumber { get; set; } = string.Empty;
    public DateTime IssueDate { get; set; }
    public DateTime ExpirationDate { get; set; }
    public string Status { get; set; } = string.Empty;
}

public class CreateExtraordinaryFeeRequestDto
{
    public string Name { get; set; } = string.Empty;
    public decimal TotalAmount { get; set; }
    public string DistributionType { get; set; } = string.Empty;
    public DateTime DueDate { get; set; }
    public string StartPeriod { get; set; } = string.Empty;
    public int NumberOfInstallments { get; set; } = 1;
    public string MeetingActNumber { get; set; } = string.Empty;
    public string Notes { get; set; } = string.Empty;
}

public class ExtraordinaryFeeDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal TotalAmount { get; set; }
    public string DistributionType { get; set; } = string.Empty;
    public DateTime DueDate { get; set; }
    public int NumberOfInstallments { get; set; }
    public decimal AmountPerUnit { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public decimal TotalCollected { get; set; }
    public decimal TotalOutstanding { get; set; }
    public int UnitsCount { get; set; }
}

public class ExtraordinaryFeeDetailDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal TotalAmount { get; set; }
    public string DistributionType { get; set; } = string.Empty;
    public DateTime DueDate { get; set; }
    public int NumberOfInstallments { get; set; }
    public decimal AmountPerUnit { get; set; }
    public string Status { get; set; } = string.Empty;
    public string Notes { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public List<ExtraordinaryFeeDistributionDto> Distributions { get; set; } = new();
}

public class ExtraordinaryFeeDistributionDto
{
    public Guid Id { get; set; }
    public Guid UnitId { get; set; }
    public string UnitIdentifier { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public int InstallmentNumber { get; set; }
    public DateTime DueDate { get; set; }
    public string Status { get; set; } = string.Empty;
    public decimal PaidAmount { get; set; }
    public decimal BalanceAmount { get; set; }
}

public class UpdateExtraordinaryFeeStatusRequestDto
{
    public string Status { get; set; } = string.Empty;
}

public class CreateIndividualChargeRequestDto
{
    public Guid UnitId { get; set; }
    public string ChargeType { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string Concept { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime ChargeDate { get; set; }
    public string ReferenceActNumber { get; set; } = string.Empty;
}

public class IndividualChargeDto
{
    public Guid Id { get; set; }
    public Guid UnitId { get; set; }
    public string UnitIdentifier { get; set; } = string.Empty;
    public string ChargeType { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public decimal BalanceAmount { get; set; }
    public string Concept { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime ChargeDate { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}

public class UpdateIndividualChargeStatusRequestDto
{
    public string Status { get; set; } = string.Empty;
    public string Notes { get; set; } = string.Empty;
}

public class CollectionStageDto
{
    public string Stage { get; set; } = string.Empty;
    public int UnitCount { get; set; }
    public decimal TotalDebt { get; set; }
    public decimal TotalOverdue { get; set; }
    public List<CollectionStageUnitDto> Units { get; set; } = new();
}

public class CollectionStageUnitDto
{
    public Guid UnitId { get; set; }
    public string UnitIdentifier { get; set; } = string.Empty;
    public decimal TotalDebt { get; set; }
    public decimal OverdueBalance { get; set; }
    public int LateDays { get; set; }
    public string LastPaymentDate { get; set; } = string.Empty;
}

public class PortfolioCollectionStagesDto
{
    public CollectionStageDto Preventive { get; set; } = new();
    public CollectionStageDto PreJudicial { get; set; } = new();
    public CollectionStageDto Judicial { get; set; } = new();
    public CollectionStageDto Agreement { get; set; } = new();
}

public class UnitPortfolioDetailDto
{
    public Guid UnitId { get; set; }
    public string UnitIdentifier { get; set; } = string.Empty;
    public string UnitTower { get; set; } = string.Empty;
    public string UnitType { get; set; } = string.Empty;
    public decimal OutstandingBalance { get; set; }
    public decimal OverdueBalance { get; set; }
    public decimal AdvanceBalance { get; set; }
    public decimal AccruedInterest { get; set; }
    public int LateDays { get; set; }
    public string CollectionStage { get; set; } = string.Empty;
    public List<PortfolioDebtItemDto> DebtItems { get; set; } = new();
    public List<RecentPaymentDto> RecentPayments { get; set; } = new();
}

public class PortfolioDebtItemDto
{
    public string SourceType { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime DueDate { get; set; }
    public decimal Amount { get; set; }
    public decimal Balance { get; set; }
    public int DaysOverdue { get; set; }
}

public class RecentPaymentDto
{
    public Guid PaymentId { get; set; }
    public DateTime PaymentDate { get; set; }
    public decimal Amount { get; set; }
    public string PaymentMethod { get; set; } = string.Empty;
}

public class PortfolioFiltersDto
{
    public string? Tower { get; set; }
    public string? CollectionStage { get; set; }
    public string? UnitType { get; set; }
    public string? Search { get; set; }
}
