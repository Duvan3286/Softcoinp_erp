import apiClient from './api-client';

export interface BudgetDetail {
  id: string;
  accountingAccountId: string;
  accountCode: string;
  accountName: string;
  approvedValue: number;
  observations: string;
}

export interface Budget {
  id: string;
  fiscalPeriod: number;
  approvalDate?: string;
  meetingActNumber: string;
  status: string;
  details: BudgetDetail[];
}

export interface CreateBudgetDetailRequest {
  accountingAccountId: string;
  approvedValue: number;
  observations: string;
}

export interface CreateBudgetRequest {
  fiscalPeriod: number;
  meetingActNumber: string;
  approvalDate?: string;
  copyFromPrevious: boolean;
  globalPercentageAdjustment?: number;
  accountAdjustments?: Record<string, number>;
  manualDetails?: CreateBudgetDetailRequest[];
}

export interface ActivateBudgetRequest {
  meetingActNumber: string;
  approvalDate: string;
}

export interface BudgetExecutionItem {
  accountId: string;
  accountCode: string;
  accountName: string;
  isGroup: boolean;
  category: string;
  nature: string;
  approvedValue: number;
  additions: number;
  transfersIn: number;
  transfersOut: number;
  adjustedBudget: number;
  executedValue: number;
  availableValue: number;
  executionPercentage: number;
  closingProjection: number;
  trafficLight: string;
}

export interface BudgetAlert {
  accountCode: string;
  accountName: string;
  adjustedBudget: number;
  closingProjection: number;
  message: string;
}

export interface BudgetExecutionReport {
  budgetId: string;
  fiscalPeriod: number;
  meetingActNumber: string;
  approvalDate?: string;
  status: string;
  items: BudgetExecutionItem[];
  alerts: BudgetAlert[];
}

export interface CreateBudgetMovementRequest {
  budgetId: string;
  movementType: 'Addition' | 'Transfer';
  sourceAccountId?: string;
  destinationAccountId: string;
  amount: number;
  justification: string;
  approvalType: 'Council' | 'Assembly';
  meetingActNumber: string;
  approvalDate: string;
}

export interface BudgetMovement {
  id: string;
  budgetId: string;
  movementType: string;
  sourceAccountId?: string;
  sourceAccountCode: string;
  sourceAccountName: string;
  destinationAccountId: string;
  destinationAccountCode: string;
  destinationAccountName: string;
  amount: number;
  justification: string;
  approvalType: string;
  meetingActNumber: string;
  approvalDate: string;
}

export interface ContingencyFundContribution {
  id: string;
  period: string;
  amount: number;
  incomeBase: number;
  percentage: number;
  contributionDate: string;
}

export interface ContingencyFundUsage {
  id: string;
  amount: number;
  justification: string;
  councilApprovalActNumber: string;
  approvalDate: string;
  createdByUserId: string;
}

export interface ContingencyFundStatus {
  tenantId: string;
  currentBalance: number;
  projectedClosingBalance: number;
  contributions: ContingencyFundContribution[];
  usages: ContingencyFundUsage[];
}

export interface LiquidateMonthlyContributionRequest {
  year: number;
  month: number;
}

export interface RecordContingencyFundUsageRequest {
  amount: number;
  justification: string;
  councilApprovalActNumber: string;
  approvalDate: string;
}

export interface BudgetSummary {
  id: string;
  fiscalPeriod: number;
  approvalDate?: string;
  meetingActNumber: string;
  status: string;
  detailsCount: number;
  createdByUserId: string;
}

const budgetService = {
  async getBudgets(year?: number): Promise<BudgetSummary[]> {
    const params = year != null ? `?year=${year}` : '';
    const response = await apiClient.get<BudgetSummary[]>(`/budgets${params}`);
    return response.data;
  },

  async createBudget(request: CreateBudgetRequest): Promise<{ id: string; fiscalPeriod: number; status: string }> {
    const response = await apiClient.post<{ id: string; fiscalPeriod: number; status: string }>('/budgets', request);
    return response.data;
  },

  async updateDraftDetails(id: string, details: CreateBudgetDetailRequest[]): Promise<{ id: string; status: string; detailsCount: number }> {
    const response = await apiClient.put<{ id: string; status: string; detailsCount: number }>(`/budgets/${id}/details`, details);
    return response.data;
  },

  async activateBudget(id: string, request: ActivateBudgetRequest): Promise<{ id: string; status: string; meetingActNumber: string; approvalDate: string }> {
    const response = await apiClient.post<{ id: string; status: string; meetingActNumber: string; approvalDate: string }>(`/budgets/${id}/activate`, request);
    return response.data;
  },

  async closeBudget(id: string): Promise<{ id: string; status: string }> {
    const response = await apiClient.post<{ id: string; status: string }>(`/budgets/${id}/close`);
    return response.data;
  },

  async getExecutionReport(year: number): Promise<BudgetExecutionReport> {
    const response = await apiClient.get<BudgetExecutionReport>(`/budgets/execution/${year}`);
    return response.data;
  },

  async createMovement(request: CreateBudgetMovementRequest): Promise<{ id: string; type: string; amount: number }> {
    const response = await apiClient.post<{ id: string; type: string; amount: number }>('/budgets/movements', request);
    return response.data;
  },

  async getMovements(budgetId: string): Promise<BudgetMovement[]> {
    const response = await apiClient.get<BudgetMovement[]>(`/budgets/${budgetId}/movements`);
    return response.data;
  },

  async getContingencyFund(): Promise<ContingencyFundStatus> {
    const response = await apiClient.get<ContingencyFundStatus>('/budgets/contingency-fund');
    return response.data;
  },

  async liquidateContingencyContribution(request: LiquidateMonthlyContributionRequest): Promise<{ id: string; period: string; amount: number; incomeBase: number }> {
    const response = await apiClient.post<{ id: string; period: string; amount: number; incomeBase: number }>('/budgets/contingency-fund/liquidate', request);
    return response.data;
  },

  async recordContingencyUsage(request: RecordContingencyFundUsageRequest): Promise<{ id: string; amount: number; act: string }> {
    const response = await apiClient.post<{ id: string; amount: number; act: string }>('/budgets/contingency-fund/usage', request);
    return response.data;
  }
};

export default budgetService;
