import apiClient from './api-client';

// ═══════════════════════════════════════════════════════════════════════
// INTERFACES - Providers
// ═══════════════════════════════════════════════════════════════════════

export interface CreateProviderRequest {
  providerType: string;
  documentType: string;
  documentNumber: string;
  businessName: string;
  contactName?: string;
  email?: string;
  phone?: string;
  address?: string;
  serviceType?: string;
  rutFilePath?: string;
  chamberOfCommerceFilePath?: string;
}

export interface UpdateProviderRequest {
  providerType?: string;
  documentType?: string;
  documentNumber?: string;
  businessName?: string;
  contactName?: string;
  email?: string;
  phone?: string;
  address?: string;
  serviceType?: string;
  rutFilePath?: string;
  chamberOfCommerceFilePath?: string;
  status?: string;
}

export interface ProviderListItem {
  id: string;
  providerType: string;
  documentNumber: string;
  businessName: string;
  contactName: string;
  email: string;
  phone: string;
  serviceType: string;
  status: string;
  contractCount: number;
  activeContractCount: number;
  averageEvaluationScore: number;
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
  daysUntilExpiration: number;
}

export interface ProviderInvoiceSummary {
  id: string;
  invoiceNumber: string;
  contractNumber: string;
  totalAmount: number;
  amountPaid: number;
  pendingAmount: number;
  dueDate: string;
  status: string;
  budgetItemName: string;
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
  businessName: string;
  contactName: string;
  email: string;
  phone: string;
  address: string;
  serviceType: string;
  rutFilePath: string;
  chamberOfCommerceFilePath: string;
  status: string;
  createdAt: string;
  updatedAt: string;
  contracts: ProviderContractSummary[];
  invoices: ProviderInvoiceSummary[];
  evaluations: ProviderEvaluationSummary[];
}

export interface CreateEvaluationRequest {
  evaluationPeriod: string;
  qualityScore: number;
  complianceScore: number;
  priceScore: number;
  attentionScore: number;
  comments?: string;
}

export interface ProviderIndicators {
  totalProviders: number;
  activeProviders: number;
  inactiveProviders: number;
  totalContracts: number;
  activeContracts: number;
  expiringContracts: number;
  totalContractValue: number;
  monthlyContractValue: number;
  pendingPaymentInvoices: number;
  pendingPaymentAmount: number;
  overdueInvoices: number;
  activeAlerts: number;
}

// ═══════════════════════════════════════════════════════════════════════
// INTERFACES - Contracts
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
  observations?: string;
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
  signedContractFilePath?: string;
  councilMeetingActNumber?: string;
  approvedInAssemblyId?: string;
  observations?: string;
}

export interface ChangeContractStatusRequest {
  newStatus: string;
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

export interface ContractAlertDto {
  id: string;
  alertType: string;
  message: string;
  generatedAt: string;
  isActive: boolean;
  resolvedAt: string;
  resolvedByUserId: string;
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
  totalAmount: number;
  amountPaid: number;
  pendingAmount: number;
  status: string;
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
  startDate: string;
  endDate: string;
  hasAutoRenewal: boolean;
  autoRenewalNoticeDays: number;
  approvalLevel: string;
  councilMeetingActNumber: string;
  approvedInAssemblyId?: string;
  approvedInAssemblyTitle: string;
  status: string;
  signedContractFilePath: string;
  observations: string;
  createdAt: string;
  updatedAt: string;
  daysUntilExpiration: number;
  alerts: ContractAlertDto[];
  invoices: ContractInvoiceDto[];
}

export interface ApprovalThreshold {
  id: string;
  approvalLevel: string;
  minValue: number;
  maxValue: number;
  description: string;
  isActive: boolean;
}

export interface CreateInvoiceRequest {
  providerId: string;
  contractId?: string;
  invoiceNumber: string;
  invoiceDate: string;
  dueDate: string;
  totalAmount: number;
  amountPaid: number;
  paymentDate?: string;
  paymentMethod?: string;
  paymentReferenceNumber?: string;
  budgetItemId?: string;
}

export interface CreatePaymentRequest {
  amount: number;
  paymentDate: string;
  paymentMethod: string;
  referenceNumber: string;
}

export interface PendingPaymentItem {
  invoiceId: string;
  invoiceNumber: string;
  providerName: string;
  providerDocumentNumber: string;
  contractId: string;
  contractNumber: string;
  totalAmount: number;
  amountPaid: number;
  pendingAmount: number;
  dueDate: string;
  daysOverdue: number;
  status: string;
}

export interface ContractExpirationReportItem {
  contractId: string;
  contractNumber: string;
  contractType: string;
  providerName: string;
  providerDocumentNumber: string;
  totalValue: number;
  startDate: string;
  endDate: string;
  daysUntilExpiration: number;
  hasAutoRenewal: boolean;
  autoRenewalNoticeDays: number;
  approvalLevel: string;
  status: string;
}

// ═══════════════════════════════════════════════════════════════════════
// SERVICE
// ═══════════════════════════════════════════════════════════════════════

const supplierService = {
  // ── Providers ────────────────────────────────────────────────────
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

  // ── Contracts ─────────────────────────────────────────────────────
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

  async getActiveAlerts(): Promise<ContractAlertDto[]> {
    const response = await apiClient.get<ContractAlertDto[]>('/contracts/alerts/active');
    return response.data;
  },

  async resolveAlert(alertId: string): Promise<void> {
    await apiClient.post(`/contracts/alerts/${alertId}/resolve`);
  },

  // ── Invoices & Payments ──────────────────────────────────────────
  async createInvoice(request: CreateInvoiceRequest): Promise<ContractInvoiceDto> {
    const response = await apiClient.post<ContractInvoiceDto>('/contracts/invoices', request);
    return response.data;
  },

  async registerPayment(invoiceId: string, request: CreatePaymentRequest): Promise<ContractPaymentDto> {
    const response = await apiClient.post<ContractPaymentDto>(`/contracts/invoices/${invoiceId}/payments`, request);
    return response.data;
  },

  async cancelInvoice(invoiceId: string): Promise<void> {
    await apiClient.post(`/contracts/invoices/${invoiceId}/cancel`);
  },

  async getPendingPayments(): Promise<PendingPaymentItem[]> {
    const response = await apiClient.get<PendingPaymentItem[]>('/contracts/payments-pending');
    return response.data;
  },

  async getExpiringContractsReport(daysAhead?: number): Promise<ContractExpirationReportItem[]> {
    const params = daysAhead ? `?daysAhead=${daysAhead}` : '';
    const response = await apiClient.get<ContractExpirationReportItem[]>(`/contracts/expiring-report${params}`);
    return response.data;
  },

  // ── Approval Thresholds ─────────────────────────────────────────
  async getApprovalThresholds(): Promise<ApprovalThreshold[]> {
    const response = await apiClient.get<ApprovalThreshold[]>('/contracts/approval-thresholds');
    return response.data;
  },
};

export default supplierService;
