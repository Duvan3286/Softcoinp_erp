import apiClient from './api-client';

export interface BillingPeriodSummary {
  id: string;
  period: string;
  monthlyBudgetTotal: number;
  cutoffDate: string;
  paymentDueDate: string;
  status: string;
  executedAt?: string;
  executedByUserId: string;
  roundingAdjustment: number;
  unitsCount: number;
  totalBilled: number;
}

export interface UnitFeeDto {
  id: string;
  unitId: string;
  unitIdentifier: string;
  unitTower: string;
  coefficient: number;
  feeValue: number;
  dueDate: string;
  status: string;
  paidAmount: number;
  balanceAmount: number;
}

export interface BillingPeriodDetail {
  id: string;
  period: string;
  monthlyBudgetTotal: number;
  cutoffDate: string;
  paymentDueDate: string;
  status: string;
  executedAt?: string;
  executedByUserId: string;
  roundingAdjustment: number;
  notes: string;
  unitFees: UnitFeeDto[];
}

export interface BillingChecklist {
  hasActiveBudget: boolean;
  coefficientSumIsHundred: boolean;
  coefficientSum: number;
  noExistingBillingForPeriod: boolean;
  activeUnitsCount: number;
  monthlyBudgetTotal: number;
  allChecksPass: boolean;
  warnings: string[];
}

export interface PortfolioSummary {
  totalBilled: number;
  totalCollected: number;
  totalOutstanding: number;
  collectionRate: number;
  unitsWithDebt: number;
  totalUnits: number;
  agingBuckets: AgingBucket[];
}

export interface AgingBucket {
  bucket: string;
  unitCount: number;
  totalDebt: number;
}

export interface LateInterestPreview {
  sourceType: string;
  sourceId: string;
  balanceAmount: number;
  daysOverdue: number;
  dailyRate: number;
  calculatedInterest: number;
}

export interface LateInterestRateConfig {
  monthlyRate: number;
  maxLegalRate: number;
  dailyRate: number;
}

export interface RegisterPaymentRequest {
  unitId: string;
  paymentDate: string;
  amount: number;
  paymentMethod: string;
  referenceNumber: string;
  notes: string;
}

export interface PaymentPreview {
  totalPayment: number;
  allocatedToInterest: number;
  allocatedToCapital: number;
  advanceAmount: number;
  allocations: PaymentAllocationPreview[];
}

export interface PaymentAllocationPreview {
  sourceType: string;
  sourceId: string;
  description: string;
  allocatedAmount: number;
}

export interface PaymentDto {
  id: string;
  unitId: string;
  unitIdentifier: string;
  paymentDate: string;
  amount: number;
  paymentMethod: string;
  referenceNumber: string;
  notes: string;
  advanceAmount: number;
  createdAt: string;
}

export interface PaymentDetail {
  id: string;
  unitId: string;
  unitIdentifier: string;
  paymentDate: string;
  amount: number;
  paymentMethod: string;
  referenceNumber: string;
  notes: string;
  advanceAmount: number;
  createdAt: string;
  allocations: PaymentAllocationItem[];
}

export interface PaymentAllocationItem {
  id: string;
  sourceType: string;
  sourceId?: string;
  amount: number;
  allocationType: string;
}

export interface UnitDebtSummary {
  unitId: string;
  unitIdentifier: string;
  totalDebt: number;
  totalOverdue: number;
  totalInterestAccrued: number;
  advanceBalance: number;
  items: DebtItem[];
}

export interface DebtItem {
  sourceType: string;
  sourceId: string;
  description: string;
  dueDate: string;
  amount: number;
  balance: number;
  isOverdue: boolean;
}

export interface CreatePaymentAgreementRequest {
  unitId: string;
  totalDebtIncluded: number;
  numberOfInstallments: number;
  interestForgivenessPercentage: number;
  councilActNumber: string;
  digitalAcceptance: string;
  startDate: string;
}

export interface AgreementSimulation {
  totalDebt: number;
  interestForgivenessPercentage: number;
  forgivenAmount: number;
  netDebt: number;
  numberOfInstallments: number;
  installmentAmount: number;
  installments: SimulatedInstallment[];
}

export interface SimulatedInstallment {
  number: number;
  dueDate: string;
  amount: number;
}

export interface PaymentAgreementSummary {
  id: string;
  unitId: string;
  unitIdentifier: string;
  totalDebtIncluded: number;
  installmentAmount: number;
  numberOfInstallments: number;
  interestForgivenessPercentage: number;
  status: string;
  startedAt: string;
  defaultedAt?: string;
  paidInstallments: number;
  overdueInstallments: number;
}

export interface PaymentAgreementDetail {
  id: string;
  unitId: string;
  unitIdentifier: string;
  totalDebtIncluded: number;
  installmentAmount: number;
  numberOfInstallments: number;
  interestForgivenessPercentage: number;
  councilActNumber: string;
  status: string;
  startedAt: string;
  defaultedAt?: string;
  digitalAcceptance: string;
  installments: AgreementInstallment[];
}

export interface AgreementInstallment {
  id: string;
  installmentNumber: number;
  dueDate: string;
  amount: number;
  paidAmount: number;
  status: string;
  paidAt?: string;
}

export interface StatementRequest {
  unitId: string;
  startDate?: string;
  endDate?: string;
}

export interface UnitStatement {
  unitId: string;
  unitIdentifier: string;
  unitTower: string;
  openingBalance: number;
  totalCharges: number;
  totalPayments: number;
  totalInterest: number;
  closingBalance: number;
  lines: StatementLine[];
}

export interface StatementLine {
  date: string;
  description: string;
  reference: string;
  debit: number;
  credit: number;
  balance: number;
}

export interface ClearanceCertificate {
  id: string;
  unitId: string;
  unitIdentifier: string;
  certificateNumber: string;
  issueDate: string;
  expirationDate: string;
  balanceAtDate: number;
  status: string;
  issuedByUserId: string;
  signedByAdministratorName: string;
}

export interface IssueClearanceCertificateRequest {
  unitId: string;
  validityDays: number;
}

export interface ClearanceCertificateSummary {
  id: string;
  unitId: string;
  unitIdentifier: string;
  certificateNumber: string;
  issueDate: string;
  expirationDate: string;
  status: string;
}

export interface ExtraordinaryFeeDto {
  id: string;
  name: string;
  totalAmount: number;
  distributionType: string;
  dueDate: string;
  numberOfInstallments: number;
  amountPerUnit: number;
  status: string;
  createdAt: string;
  totalCollected: number;
  totalOutstanding: number;
  unitsCount: number;
}

export interface ExtraordinaryFeeDetail {
  id: string;
  name: string;
  totalAmount: number;
  distributionType: string;
  dueDate: string;
  numberOfInstallments: number;
  amountPerUnit: number;
  status: string;
  notes: string;
  createdAt: string;
  distributions: ExtraordinaryFeeDistribution[];
}

export interface ExtraordinaryFeeDistribution {
  id: string;
  unitId: string;
  unitIdentifier: string;
  amount: number;
  installmentNumber: number;
  dueDate: string;
  status: string;
  paidAmount: number;
  balanceAmount: number;
}

export interface CreateExtraordinaryFeeRequest {
  name: string;
  totalAmount: number;
  distributionType: string;
  dueDate: string;
  startPeriod: string;
  numberOfInstallments: number;
  notes: string;
}

export interface IndividualChargeDto {
  id: string;
  unitId: string;
  unitIdentifier: string;
  chargeType: string;
  amount: number;
  balanceAmount: number;
  concept: string;
  description: string;
  chargeDate: string;
  status: string;
  createdAt: string;
}

export interface CreateIndividualChargeRequest {
  unitId: string;
  chargeType: string;
  amount: number;
  concept: string;
  description: string;
  chargeDate: string;
  referenceActNumber: string;
}

export interface PortfolioCollectionStages {
  preventive: CollectionStage;
  preJudicial: CollectionStage;
  judicial: CollectionStage;
  agreement: CollectionStage;
}

export interface CollectionStage {
  stage: string;
  unitCount: number;
  totalDebt: number;
  totalOverdue: number;
  units: CollectionStageUnit[];
}

export interface CollectionStageUnit {
  unitId: string;
  unitIdentifier: string;
  totalDebt: number;
  overdueBalance: number;
  lateDays: number;
  lastPaymentDate: string;
}

export interface UnitPortfolioDetail {
  unitId: string;
  unitIdentifier: string;
  unitTower: string;
  unitType: string;
  outstandingBalance: number;
  overdueBalance: number;
  advanceBalance: number;
  accruedInterest: number;
  lateDays: number;
  collectionStage: string;
  debtItems: PortfolioDebtItem[];
  recentPayments: RecentPaymentItem[];
}

export interface PortfolioDebtItem {
  sourceType: string;
  description: string;
  dueDate: string;
  amount: number;
  balance: number;
  daysOverdue: number;
}

export interface RecentPaymentItem {
  paymentId: string;
  paymentDate: string;
  amount: number;
  paymentMethod: string;
}

export interface ExecuteBillingRequest {
  period: string;
  cutoffDate: string;
  paymentDueDate: string;
}

const feesPortfolioService = {
  // Billing periods
  async getBillingPeriods(): Promise<BillingPeriodSummary[]> {
    const response = await apiClient.get<BillingPeriodSummary[]>('/billing/periods');
    return response.data;
  },

  async getBillingPeriod(id: string): Promise<BillingPeriodDetail> {
    const response = await apiClient.get<BillingPeriodDetail>(`/billing/periods/${id}`);
    return response.data;
  },

  async getBillingChecklist(period: string, cutoffDate: string, paymentDueDate: string): Promise<BillingChecklist> {
    const response = await apiClient.get<BillingChecklist>('/billing/periods/checklist', {
      params: { period, cutoffDate, paymentDueDate }
    });
    return response.data;
  },

  async executeBilling(request: ExecuteBillingRequest): Promise<{ id: string; period: string; status: string }> {
    const response = await apiClient.post<{ id: string; period: string; status: string }>('/billing/periods', request);
    return response.data;
  },

  async processBilling(periodId: string): Promise<{ id: string; status: string; unitFeesCreated: number }> {
    const response = await apiClient.post<{ id: string; status: string; unitFeesCreated: number }>(`/billing/periods/${periodId}/process`);
    return response.data;
  },

  // Late interest
  async calculateLateInterest(periodId: string): Promise<{ lateInterestRecordsCreated: number }> {
    const response = await apiClient.post<{ lateInterestRecordsCreated: number }>(`/billing/periods/${periodId}/late-interest`);
    return response.data;
  },

  // Portfolio
  async getPortfolioSummary(): Promise<PortfolioSummary> {
    const response = await apiClient.get<PortfolioSummary>('/billing/portfolio/summary');
    return response.data;
  },

  async getCollectionStages(): Promise<PortfolioCollectionStages> {
    const response = await apiClient.get<PortfolioCollectionStages>('/billing/portfolio/collection-stages');
    return response.data;
  },

  // Unit
  async getUnitFees(unitId: string): Promise<UnitFeeDto[]> {
    const response = await apiClient.get<UnitFeeDto[]>(`/billing/units/${unitId}/fees`);
    return response.data;
  },

  async getUnitBalance(unitId: string): Promise<UnitDebtSummary> {
    const response = await apiClient.get<UnitDebtSummary>(`/billing/units/${unitId}/balance`);
    return response.data;
  },

  async getUnitPortfolioDetail(unitId: string): Promise<UnitPortfolioDetail> {
    const response = await apiClient.get<UnitPortfolioDetail>(`/billing/units/${unitId}/portfolio-detail`);
    return response.data;
  },

  // Payments
  async previewPayment(unitId: string, amount: number, paymentDate: string): Promise<PaymentPreview> {
    const response = await apiClient.post<PaymentPreview>('/billing/payments/preview', { unitId, amount, paymentDate });
    return response.data;
  },

  async registerPayment(request: RegisterPaymentRequest): Promise<{ paymentId: string; amount: number; advanceAmount: number }> {
    const response = await apiClient.post<{ paymentId: string; amount: number; advanceAmount: number }>('/billing/payments/register', request);
    return response.data;
  },

  async getPaymentDetail(paymentId: string): Promise<PaymentDetail> {
    const response = await apiClient.get<PaymentDetail>(`/billing/payments/${paymentId}`);
    return response.data;
  },

  async getUnitPayments(unitId: string): Promise<PaymentDto[]> {
    const response = await apiClient.get<PaymentDto[]>(`/billing/units/${unitId}/payments`);
    return response.data;
  },

  // Extraordinary fees
  async getExtraordinaryFees(): Promise<ExtraordinaryFeeDto[]> {
    const response = await apiClient.get<ExtraordinaryFeeDto[]>('/billing/extraordinary-fees');
    return response.data;
  },

  async getExtraordinaryFeeDetail(id: string): Promise<ExtraordinaryFeeDetail> {
    const response = await apiClient.get<ExtraordinaryFeeDetail>(`/billing/extraordinary-fees/${id}`);
    return response.data;
  },

  async createExtraordinaryFee(request: CreateExtraordinaryFeeRequest): Promise<{ id: string; name: string; amountPerUnit: number; distributionsCount: number }> {
    const response = await apiClient.post<{ id: string; name: string; amountPerUnit: number; distributionsCount: number }>('/billing/extraordinary-fees', request);
    return response.data;
  },

  async updateExtraordinaryFeeStatus(id: string, status: string): Promise<void> {
    await apiClient.put(`/billing/extraordinary-fees/${id}/status`, { status });
  },

  // Individual charges
  async getIndividualCharges(status?: string): Promise<IndividualChargeDto[]> {
    const params = status ? `?status=${status}` : '';
    const response = await apiClient.get<IndividualChargeDto[]>(`/billing/individual-charges${params}`);
    return response.data;
  },

  async getUnitIndividualCharges(unitId: string): Promise<IndividualChargeDto[]> {
    const response = await apiClient.get<IndividualChargeDto[]>(`/billing/units/${unitId}/individual-charges`);
    return response.data;
  },

  async createIndividualCharge(request: CreateIndividualChargeRequest): Promise<{ id: string; unitId: string; amount: number; status: string }> {
    const response = await apiClient.post<{ id: string; unitId: string; amount: number; status: string }>('/billing/individual-charges', request);
    return response.data;
  },

  async updateIndividualChargeStatus(id: string, status: string, notes?: string): Promise<void> {
    await apiClient.put(`/billing/individual-charges/${id}/status`, { status, notes });
  },

  // Agreements
  async simulateAgreement(unitId: string, totalDebt: number, forgivenessPercentage: number, installments: number, startDate: string): Promise<AgreementSimulation> {
    const response = await apiClient.get<AgreementSimulation>('/billing/agreements/simulate', {
      params: { unitId, totalDebt, forgivenessPercentage, installments, startDate }
    });
    return response.data;
  },

  async getAgreements(): Promise<PaymentAgreementSummary[]> {
    const response = await apiClient.get<PaymentAgreementSummary[]>('/billing/agreements');
    return response.data;
  },

  async getAgreementDetail(id: string): Promise<PaymentAgreementDetail> {
    const response = await apiClient.get<PaymentAgreementDetail>(`/billing/agreements/${id}`);
    return response.data;
  },

  async createAgreement(request: CreatePaymentAgreementRequest): Promise<{ id: string; status: string; installmentAmount: number }> {
    const response = await apiClient.post<{ id: string; status: string; installmentAmount: number }>('/billing/agreements', request);
    return response.data;
  },

  // Statement & Certificates
  async getUnitStatement(request: StatementRequest): Promise<UnitStatement> {
    const response = await apiClient.post<UnitStatement>('/billing/statement', request);
    return response.data;
  },

  async issueClearanceCertificate(request: IssueClearanceCertificateRequest): Promise<{ id: string; certificateNumber: string; issueDate: string; expirationDate: string; status: string }> {
    const response = await apiClient.post<{ id: string; certificateNumber: string; issueDate: string; expirationDate: string; status: string }>('/billing/clearance-certificate/issue', request);
    return response.data;
  },

  async getUnitCertificates(unitId: string): Promise<ClearanceCertificateSummary[]> {
    const response = await apiClient.get<ClearanceCertificateSummary[]>(`/billing/units/${unitId}/clearance-certificates`);
    return response.data;
  },

  async getCertificateDetail(id: string): Promise<ClearanceCertificate> {
    const response = await apiClient.get<ClearanceCertificate>(`/billing/clearance-certificates/${id}`);
    return response.data;
  },

  async revokeCertificate(id: string): Promise<void> {
    await apiClient.post(`/billing/clearance-certificates/${id}/revoke`);
  }
};

export default feesPortfolioService;
