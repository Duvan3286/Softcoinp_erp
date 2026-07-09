import apiClient from './api-client';

export interface BudgetSummary {
  id: string;
  fiscalYear: number;
  approvalDate?: string;
  meetingActNumber: string;
  status: string;
  observations: string;
  incomeItemsCount: number;
  expenseItemsCount: number;
  totalIncome: number;
  totalExpense: number;
  createdByUserId: string;
}

export interface IncomeItem {
  id: string;
  name: string;
  description: string;
  annualValue: number;
  monthlyValue: number;
}

export interface ExpenseItem {
  id: string;
  name: string;
  description: string;
  category: string;
  annualValue: number;
  monthlyValue: number;
  isContingencyFund: boolean;
  contingencyPercentage: number;
  requiresCouncilApproval: boolean;
  approvalThreshold: number;
}

export interface BudgetDetail {
  id: string;
  fiscalYear: number;
  approvalDate?: string;
  meetingActNumber: string;
  status: string;
  observations: string;
  incomeItems: IncomeItem[];
  expenseItems: ExpenseItem[];
}

export interface CreateIncomeItem {
  name: string;
  description: string;
  annualValue: number;
}

export interface CreateExpenseItem {
  name: string;
  description: string;
  category: string;
  annualValue: number;
  isContingencyFund: boolean;
  contingencyPercentage: number;
  requiresCouncilApproval: boolean;
  approvalThreshold: number;
}

export interface CreateBudgetRequest {
  fiscalYear: number;
  meetingActNumber: string;
  approvalDate?: string;
  observations: string;
  copyFromPrevious: boolean;
  globalPercentageAdjustment?: number;
  incomeItems?: CreateIncomeItem[];
  expenseItems?: CreateExpenseItem[];
}

export interface UpdateDraftBudgetRequest {
  incomeItems: CreateIncomeItem[];
  expenseItems: CreateExpenseItem[];
}

export interface ApproveBudgetRequest {
  meetingActNumber: string;
  approvalDate: string;
}

export interface ExpenseExecutionItem {
  id: string;
  name: string;
  category: string;
  annualValue: number;
  monthlyValue: number;
  proportionalToDate: number;
  executedValue: number;
  availableValue: number;
  executionPercentage: number;
  trafficLight: string;
  isContingencyFund: boolean;
  contingencyPercentage: number;
  requiresCouncilApproval: boolean;
  approvalThreshold: number;
}

export interface BudgetAlert {
  itemName: string;
  annualValue: number;
  executedValue: number;
  executionPercentage: number;
  message: string;
  severity: string;
}

export interface BudgetExecutionDashboard {
  budgetId: string;
  fiscalYear: number;
  status: string;
  totalApprovedIncome: number;
  totalApprovedExpense: number;
  totalExecutedExpense: number;
  totalAvailable: number;
  overallExecutionPercentage: number;
  expenseItems: ExpenseExecutionItem[];
  alerts: BudgetAlert[];
}

export interface RecordExpenseRequest {
  expenseItemId: string;
  description: string;
  amount: number;
  expenseDate: string;
  providerId?: string;
  invoiceReference: string;
}

export interface ExecutedExpense {
  id: string;
  expenseItemId: string;
  expenseItemName: string;
  description: string;
  amount: number;
  expenseDate: string;
  providerId?: string;
  providerName: string;
  invoiceReference: string;
  councilApproved: boolean;
  requiresCouncilApproval: boolean;
}

export interface CreateModificationRequest {
  budgetId: string;
  expenseItemId?: string;
  incomeItemId?: string;
  modificationType: string;
  amount: number;
  justification: string;
  approvalType: string;
  meetingActNumber: string;
  approvalDate: string;
}

export interface BudgetModification {
  id: string;
  budgetId: string;
  expenseItemId?: string;
  expenseItemName: string;
  incomeItemId?: string;
  incomeItemName: string;
  modificationType: string;
  amount: number;
  previousValue: number;
  newValue: number;
  justification: string;
  approvalType: string;
  meetingActNumber: string;
  approvalDate: string;
}

export interface ContingencyFundUsage {
  id: string;
  justification: string;
  amount: number;
  councilApprovalActNumber: string;
  createdAt: string;
}

export interface ContingencyFundStatus {
  tenantId: string;
  totalContributed: number;
  totalUsed: number;
  availableBalance: number;
  contingencyPercentage: number;
  usages: ContingencyFundUsage[];
}

export interface RecordContingencyFundUsageRequest {
  budgetId: string;
  justification: string;
  amount: number;
  councilApprovalActNumber: string;
  executedExpenseId?: string;
}

const budgetService = {
  async getBudgets(year?: number): Promise<BudgetSummary[]> {
    const params = year != null ? `?year=${year}` : '';
    const response = await apiClient.get<BudgetSummary[]>(`/budgets${params}`);
    return response.data;
  },

  async getBudget(id: string): Promise<BudgetDetail> {
    const response = await apiClient.get<BudgetDetail>(`/budgets/${id}`);
    return response.data;
  },

  async createBudget(request: CreateBudgetRequest): Promise<{ id: string; fiscalYear: number; status: string }> {
    const response = await apiClient.post<{ id: string; fiscalYear: number; status: string }>('/budgets', request);
    return response.data;
  },

  async updateDraftBudget(id: string, request: UpdateDraftBudgetRequest): Promise<{ id: string; status: string }> {
    const response = await apiClient.put<{ id: string; status: string }>(`/budgets/${id}`, request);
    return response.data;
  },

  async approveBudget(id: string, request: ApproveBudgetRequest): Promise<{ id: string; status: string }> {
    const response = await apiClient.post<{ id: string; status: string }>(`/budgets/${id}/approve`, request);
    return response.data;
  },

  async generateNextBudget(id: string): Promise<{ id: string; fiscalYear: number; status: string }> {
    const response = await apiClient.post<{ id: string; fiscalYear: number; status: string }>(`/budgets/${id}/generate-next`);
    return response.data;
  },

  async getBudgetExecution(year: number): Promise<BudgetExecutionDashboard> {
    const response = await apiClient.get<BudgetExecutionDashboard>(`/budgets/execution/${year}`);
    return response.data;
  },

  async recordExpense(request: RecordExpenseRequest): Promise<{ id: string; amount: number; expenseItemName: string }> {
    const response = await apiClient.post<{ id: string; amount: number; expenseItemName: string }>('/budgets/expenses', request);
    return response.data;
  },

  async getExpenses(expenseItemId?: string, fromDate?: string, toDate?: string): Promise<ExecutedExpense[]> {
    const params = new URLSearchParams();
    if (expenseItemId) params.append('expenseItemId', expenseItemId);
    if (fromDate) params.append('fromDate', fromDate);
    if (toDate) params.append('toDate', toDate);
    const query = params.toString();
    const response = await apiClient.get<ExecutedExpense[]>(`/budgets/expenses${query ? `?${query}` : ''}`);
    return response.data;
  },

  async getModifications(budgetId: string): Promise<BudgetModification[]> {
    const response = await apiClient.get<BudgetModification[]>(`/budgets/modifications/${budgetId}`);
    return response.data;
  },

  async createModification(request: CreateModificationRequest): Promise<{ id: string; modificationType: string; amount: number }> {
    const response = await apiClient.post<{ id: string; modificationType: string; amount: number }>('/budgets/modifications', request);
    return response.data;
  },

  async getContingencyFundStatus(): Promise<ContingencyFundStatus> {
    const response = await apiClient.get<ContingencyFundStatus>('/budgets/contingency-fund');
    return response.data;
  },

  async recordContingencyFundUsage(request: RecordContingencyFundUsageRequest): Promise<{ id: string; amount: number; justification: string }> {
    const response = await apiClient.post<{ id: string; amount: number; justification: string }>('/budgets/contingency-fund/usage', request);
    return response.data;
  },

  async approveCouncilExpense(executedExpenseId: string): Promise<ExecutedExpense> {
    const response = await apiClient.post<ExecutedExpense>(`/budgets/expenses/${executedExpenseId}/approve-council`);
    return response.data;
  }
};

export default budgetService;
