import apiClient from './api-client';

export interface DashboardKpis {
  currentMonthCollectionPercentage: number;
  previousMonthCollectionPercentage: number;
  daysElapsedInPeriod: number;
  totalDaysInPeriod: number;
  currentMonthBilled: number;
  currentMonthCollected: number;
  totalOverduePortfolio: number;
  overdueOneMonth: number;
  overdueTwoMonths: number;
  overdueThreeOrMoreMonths: number;
  budgetExecutionPercentage: number;
  budgetExpectedExecutionPercentage: number;
  openPqrCount: number;
  overduePqrCount: number;
}

export interface AlertItem {
  id: string;
  ruleType: string;
  urgency: string;
  title: string;
  description: string;
  moduleLink: string;
  createdAt: string;
}

export interface AlertConfigurationItem {
  id: string;
  ruleType: string;
  isEnabled: boolean;
  thresholdDays: number;
  thresholdPercentage: number;
  defaultUrgency: string;
  hasRealDataSource: boolean;
}

export interface UpdateAlertConfigurationRequest {
  isEnabled: boolean;
  thresholdDays: number;
  thresholdPercentage: number;
  defaultUrgency: string;
}

export interface MonthlyCollectionItem {
  period: string;
  billed: number;
  collected: number;
}

export interface UnitPaymentStatus {
  unitId: string;
  identifier: string;
  ownerName: string;
  overdueBalance: number;
  monthsOverdue: number;
  colorCode: string;
  statusLabel: string;
}

export interface FloorGroup {
  floorLevel: number;
  units: UnitPaymentStatus[];
}

export interface TowerGroup {
  towerOrBlock: string;
  floors: FloorGroup[];
}

export interface PaymentStatusMap {
  generatedAt: string;
  towers: TowerGroup[];
}

export interface UpcomingEventItem {
  title: string;
  description: string;
  eventDate: string;
  eventType: string;
  moduleLink: string;
}

export interface RecentActivityItem {
  action: string;
  description: string;
  userName: string;
  timestamp: string;
  moduleLink: string;
}

export interface ExpenseExecutionItem {
  id: string;
  name: string;
  category: string;
  annualValue: number;
  proportionalToDate: number;
  executedValue: number;
  availableValue: number;
  executionPercentage: number;
  trafficLight: string;
  isContingencyFund: boolean;
  contingencyPercentage: number;
  approvalThreshold: number;
}

export interface BudgetAlertItem {
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
  alerts: BudgetAlertItem[];
}

const dashboardService = {
  async getKpis(): Promise<DashboardKpis> {
    const response = await apiClient.get<DashboardKpis>('/dashboard/kpis');
    return response.data;
  },

  async getAlerts(): Promise<AlertItem[]> {
    const response = await apiClient.get<AlertItem[]>('/dashboard/alerts');
    return response.data;
  },

  async getAlertConfigurations(): Promise<AlertConfigurationItem[]> {
    const response = await apiClient.get<AlertConfigurationItem[]>('/dashboard/alerts/configurations');
    return response.data;
  },

  async updateAlertConfiguration(ruleType: string, request: UpdateAlertConfigurationRequest): Promise<AlertConfigurationItem> {
    const response = await apiClient.put<AlertConfigurationItem>(`/dashboard/alerts/configurations/${ruleType}`, request);
    return response.data;
  },

  async initializeAlerts(): Promise<void> {
    await apiClient.post('/dashboard/alerts/initialize');
  },

  async getCollectionChart(): Promise<MonthlyCollectionItem[]> {
    const response = await apiClient.get<MonthlyCollectionItem[]>('/dashboard/collection-chart');
    return response.data;
  },

  async getPaymentStatusMap(): Promise<PaymentStatusMap> {
    const response = await apiClient.get<PaymentStatusMap>('/dashboard/payment-status-map');
    return response.data;
  },

  async getUpcomingEvents(): Promise<UpcomingEventItem[]> {
    const response = await apiClient.get<UpcomingEventItem[]>('/dashboard/upcoming-events');
    return response.data;
  },

  async getRecentActivity(): Promise<RecentActivityItem[]> {
    const response = await apiClient.get<RecentActivityItem[]>('/dashboard/recent-activity');
    return response.data;
  },

  async invalidateCache(): Promise<void> {
    await apiClient.post('/dashboard/invalidate-cache');
  }
};

export default dashboardService;
