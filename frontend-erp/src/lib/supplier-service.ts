import apiClient from './api-client';

// ═══════════════════════════════════════════════════════════════════════
// INTERFACES - Proveedores
// ═══════════════════════════════════════════════════════════════════════

export interface CreateProviderRequest {
  providerType: string;
  documentType: string;
  documentNumber: string;
  verificationDigit?: string;
  businessName: string;
  tradeName?: string;
  contactName?: string;
  email?: string;
  phone?: string;
  address?: string;
  city?: string;
  economicActivity?: string;
  serviceType?: string;
  rutFilePath?: string;
  legalRepDocumentType?: string;
  legalRepDocumentNumber?: string;
  legalRepName?: string;
  legalRepEmail?: string;
  isPreferred?: boolean;
}

export interface UpdateProviderRequest {
  providerType?: string;
  documentType?: string;
  documentNumber?: string;
  verificationDigit?: string;
  businessName?: string;
  tradeName?: string;
  contactName?: string;
  email?: string;
  phone?: string;
  address?: string;
  city?: string;
  economicActivity?: string;
  serviceType?: string;
  rutFilePath?: string;
  legalRepDocumentType?: string;
  legalRepDocumentNumber?: string;
  legalRepName?: string;
  legalRepEmail?: string;
  isPreferred?: boolean;
  status?: string;
}

export interface ProviderListItem {
  id: string;
  providerType: string;
  documentNumber: string;
  businessName: string;
  tradeName: string;
  contactName: string;
  email: string;
  phone: string;
  city: string;
  serviceType: string;
  isPreferred: boolean;
  status: string;
  contractCount: number;
  activeContractCount: number;
  createdAt: string;
}

export interface ProviderContractSummary {
  id: string;
  contractNumber: string;
  contractType: string;
  totalValue: number;
  startDate: string;
  endDate: string;
  status: string;
}

export interface ProviderEvaluationSummary {
  id: string;
  evaluationPeriod: string;
  averageScore: number;
  recommendation: string;
  evaluatedByUserName: string;
  createdAt: string;
}

export interface ProviderDetail {
  id: string;
  providerType: string;
  documentType: string;
  documentNumber: string;
  verificationDigit: string;
  businessName: string;
  tradeName: string;
  contactName: string;
  email: string;
  phone: string;
  address: string;
  city: string;
  economicActivity: string;
  serviceType: string;
  rutFilePath: string;
  legalRepDocumentType: string;
  legalRepDocumentNumber: string;
  legalRepName: string;
  legalRepEmail: string;
  isPreferred: boolean;
  status: string;
  createdAt: string;
  updatedAt: string;
  contracts: ProviderContractSummary[];
  evaluations: ProviderEvaluationSummary[];
}

export interface CreateEvaluationRequest {
  contractId?: string;
  evaluationPeriod: string;
  serviceQualityScore: number;
  complianceScore: number;
  priceFairnessScore: number;
  afterSalesScore: number;
  comments?: string;
}

export interface ProviderIndicators {
  totalProviders: number;
  activeProviders: number;
  inactiveProviders: number;
  preferredProviders: number;
  totalContracts: number;
  activeContracts: number;
  expiringContracts: number;
  totalContractValue: number;
  monthlyContractValue: number;
  pendingInvoices: number;
  pendingInvoiceAmount: number;
  overdueInvoices: number;
  activeAlerts: number;
  expiringPolicies: number;
}

// ═══════════════════════════════════════════════════════════════════════
// INTERFACES - Contratos
// ═══════════════════════════════════════════════════════════════════════

export interface CreateContractRequest {
  providerId: string;
  contractNumber: string;
  contractType: string;
  objectDescription: string;
  totalValue: number;
  monthlyValue: number;
  isRecurrent: boolean;
  startDate: string;
  endDate: string;
  hasAutoRenewal: boolean;
  autoRenewalNoticeDays: number;
  budgetAccountId?: string;
}

export interface UpdateContractRequest {
  contractType?: string;
  objectDescription?: string;
  totalValue?: number;
  monthlyValue?: number;
  isRecurrent?: boolean;
  startDate?: string;
  endDate?: string;
  hasAutoRenewal?: boolean;
  autoRenewalNoticeDays?: number;
  budgetAccountId?: string;
  signedContractFilePath?: string;
}

export interface ChangeContractStatusRequest {
  newStatus: string;
  justification: string;
}

export interface ContractListItem {
  id: string;
  contractNumber: string;
  contractType: string;
  providerBusinessName: string;
  totalValue: number;
  monthlyValue: number;
  startDate: string;
  endDate: string;
  hasAutoRenewal: boolean;
  approvalLevel: string;
  status: string;
  daysUntilExpiration: number;
  alertCount: number;
}

export interface ContractPolicyDto {
  id: string;
  policyNumber: string;
  insuranceCompany: string;
  policyType: string;
  insuredAmount: number;
  startDate: string;
  endDate: string;
  filePath: string;
  isActive: boolean;
  daysUntilExpiration: number;
}

export interface ContractAlertDto {
  id: string;
  alertType: string;
  message: string;
  generatedAt: string;
  isActive: boolean;
  resolvedAt: string;
  resolvedByUserId: string;
  escalatedToCouncil: boolean;
}

export interface ContractPaymentDto {
  id: string;
  amount: number;
  paymentDate: string;
  paymentMethod: string;
  referenceNumber: string;
  status: string;
}

export interface ContractInvoiceDto {
  id: string;
  invoiceNumber: string;
  invoiceDate: string;
  dueDate: string;
  subtotal: number;
  ivaAmount: number;
  retentionFuelAmount: number;
  retentionIcaAmount: number;
  netAmount: number;
  status: string;
  pendingAmount: number;
  payments: ContractPaymentDto[];
}

export interface ContractDetail {
  id: string;
  providerId: string;
  providerBusinessName: string;
  providerDocumentNumber: string;
  contractNumber: string;
  contractType: string;
  objectDescription: string;
  totalValue: number;
  monthlyValue: number;
  isRecurrent: boolean;
  startDate: string;
  endDate: string;
  hasAutoRenewal: boolean;
  autoRenewalNoticeDays: number;
  approvalLevel: string;
  councilMeetingActNumber: string;
  assemblyMeetingActNumber: string;
  budgetAccountId: string;
  status: string;
  signedContractFilePath: string;
  createdAt: string;
  updatedAt: string;
  daysUntilExpiration: number;
  policies: ContractPolicyDto[];
  alerts: ContractAlertDto[];
  invoices: ContractInvoiceDto[];
}

export interface CreateContractPolicyRequest {
  policyNumber: string;
  insuranceCompany: string;
  policyType: string;
  insuredAmount: number;
  startDate: string;
  endDate: string;
  filePath?: string;
}

export interface RetentionConfiguration {
  id: string;
  serviceType: string;
  serviceDescription: string;
  retentionFuelRate: number;
  retentionIcaRate: number;
  isActive: boolean;
}

export interface ApprovalThreshold {
  id: string;
  approvalLevel: string;
  minValue: number;
  maxValue: number;
  description: string;
  isActive: boolean;
}

export interface RetentionCalculation {
  subtotal: number;
  ivaAmount: number;
  retentionFuelAmount: number;
  retentionIcaAmount: number;
  totalRetentions: number;
  netAmount: number;
  details: { serviceType: string; rate: number; baseAmount: number; retentionAmount: number }[];
}

// ═══════════════════════════════════════════════════════════════════════
// SERVICE
// ═══════════════════════════════════════════════════════════════════════

const supplierService = {
  // ── Proveedores ──────────────────────────────────────────────────
  async getProviders(status?: string, providerType?: string, serviceType?: string, search?: string): Promise<ProviderListItem[]> {
    const params = new URLSearchParams();
    if (status) params.append('status', status);
    if (providerType) params.append('providerType', providerType);
    if (serviceType) params.append('serviceType', serviceType);
    if (search) params.append('search', search);
    const query = params.toString();
    const response = await apiClient.get<ProviderListItem[]>(`/providers${query ? '?' + query : ''}`);
    return response.data;
  },

  async getProviderById(id: string): Promise<ProviderDetail> {
    const response = await apiClient.get<ProviderDetail>(`/providers/${id}`);
    return response.data;
  },

  async createProvider(request: CreateProviderRequest): Promise<ProviderDetail> {
    const response = await apiClient.post<ProviderDetail>('/providers', request);
    return response.data;
  },

  async updateProvider(id: string, request: UpdateProviderRequest): Promise<ProviderDetail> {
    const response = await apiClient.put<ProviderDetail>(`/providers/${id}`, request);
    return response.data;
  },

  async deleteProvider(id: string): Promise<void> {
    await apiClient.delete(`/providers/${id}`);
  },

  async getProviderEvaluations(providerId: string): Promise<ProviderEvaluationSummary[]> {
    const response = await apiClient.get<ProviderEvaluationSummary[]>(`/providers/${providerId}/evaluations`);
    return response.data;
  },

  async createProviderEvaluation(providerId: string, request: CreateEvaluationRequest): Promise<ProviderEvaluationSummary> {
    const response = await apiClient.post<ProviderEvaluationSummary>(`/providers/${providerId}/evaluations`, request);
    return response.data;
  },

  async getIndicators(): Promise<ProviderIndicators> {
    const response = await apiClient.get<ProviderIndicators>('/providers/indicators');
    return response.data;
  },

  // ── Contratos ──────────────────────────────────────────────────────
  async getContracts(status?: string, contractType?: string, providerId?: string, search?: string): Promise<ContractListItem[]> {
    const params = new URLSearchParams();
    if (status) params.append('status', status);
    if (contractType) params.append('contractType', contractType);
    if (providerId) params.append('providerId', providerId);
    if (search) params.append('search', search);
    const query = params.toString();
    const response = await apiClient.get<ContractListItem[]>(`/contracts${query ? '?' + query : ''}`);
    return response.data;
  },

  async getContractById(id: string): Promise<ContractDetail> {
    const response = await apiClient.get<ContractDetail>(`/contracts/${id}`);
    return response.data;
  },

  async createContract(request: CreateContractRequest): Promise<ContractDetail> {
    const response = await apiClient.post<ContractDetail>('/contracts', request);
    return response.data;
  },

  async updateContract(id: string, request: UpdateContractRequest): Promise<ContractDetail> {
    const response = await apiClient.put<ContractDetail>(`/contracts/${id}`, request);
    return response.data;
  },

  async changeContractStatus(id: string, request: ChangeContractStatusRequest): Promise<ContractDetail> {
    const response = await apiClient.put<ContractDetail>(`/contracts/${id}/status`, request);
    return response.data;
  },

  async deleteContract(id: string): Promise<void> {
    await apiClient.delete(`/contracts/${id}`);
  },

  async addContractPolicy(contractId: string, request: CreateContractPolicyRequest): Promise<ContractPolicyDto> {
    const response = await apiClient.post<ContractPolicyDto>(`/contracts/${contractId}/policies`, request);
    return response.data;
  },

  async getActiveAlerts(): Promise<ContractAlertDto[]> {
    const response = await apiClient.get<ContractAlertDto[]>('/contracts/alerts/active');
    return response.data;
  },

  async resolveAlert(alertId: string): Promise<void> {
    await apiClient.post(`/contracts/alerts/${alertId}/resolve`);
  },

  // ── Configuración ──────────────────────────────────────────────────
  async getRetentionConfigurations(): Promise<RetentionConfiguration[]> {
    const response = await apiClient.get<RetentionConfiguration[]>('/contracts/retention-configurations');
    return response.data;
  },

  async getApprovalThresholds(): Promise<ApprovalThreshold[]> {
    const response = await apiClient.get<ApprovalThreshold[]>('/contracts/approval-thresholds');
    return response.data;
  },

  async calculateRetentions(serviceType: string, subtotal: number): Promise<RetentionCalculation> {
    const response = await apiClient.get<RetentionCalculation>(`/contracts/calculate-retentions?serviceType=${serviceType}&subtotal=${subtotal}`);
    return response.data;
  },
};

export default supplierService;
