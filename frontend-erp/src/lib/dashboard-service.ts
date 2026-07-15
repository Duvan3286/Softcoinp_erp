import apiClient from './api-client';

// ═══════════════════════════════════════════════════════════════════════
// KPIs
// ═══════════════════════════════════════════════════════════════════════

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

// ═══════════════════════════════════════════════════════════════════════
// Alertas operativas
// ═══════════════════════════════════════════════════════════════════════

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

// ═══════════════════════════════════════════════════════════════════════
// Recaudo histórico
// ═══════════════════════════════════════════════════════════════════════

export interface MonthlyCollectionItem {
  period: string;
  billed: number;
  collected: number;
}

// ═══════════════════════════════════════════════════════════════════════
// Mapa de estado de pago
// ═══════════════════════════════════════════════════════════════════════

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

// ═══════════════════════════════════════════════════════════════════════
// Próximos eventos y actividad reciente
// ═══════════════════════════════════════════════════════════════════════

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

// ═══════════════════════════════════════════════════════════════════════
// Vista de Consejo
// ═══════════════════════════════════════════════════════════════════════

export interface CouncilApprovalItem {
  type: string;
  description: string;
  amount: number;
  requestedAt: string;
  moduleLink: string;
}

export interface ContingencyFundUsageSummary {
  justification: string;
  amount: number;
  councilApprovalActNumber: string;
  createdAt: string;
}

export interface ContingencyFundInfo {
  availableBalance: number;
  totalContributed: number;
  totalUsed: number;
  recentUsages: ContingencyFundUsageSummary[];
}

export interface CouncilDashboard {
  pendingApprovals: CouncilApprovalItem[];
  contingencyFund: ContingencyFundInfo;
}

// ═══════════════════════════════════════════════════════════════════════
// Vista de Contador y Auditor
// ═══════════════════════════════════════════════════════════════════════

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
  requiresCouncilApproval: boolean;
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

export interface ReportLinkItem {
  reportTypeCode: string;
  name: string;
  moduleLink: string;
}

export interface AccountantBudgetPanel {
  execution: BudgetExecutionDashboard;
  reportLinks: ReportLinkItem[];
}

export interface AuditorDashboard {
  currentFiscalYear: number;
  availableReports: ReportLinkItem[];
}

// ═══════════════════════════════════════════════════════════════════════
// Vista de Residente
// ═══════════════════════════════════════════════════════════════════════

export interface ResidentOpenPqr {
  radicadoNumber: string;
  subject: string;
  status: string;
  createdAt: string;
  isOverdue: boolean;
}

export interface ResidentReservationItem {
  spaceName: string;
  startDateTime: string;
  endDateTime: string;
  status: string;
}

export interface ResidentCircularItem {
  title: string;
  publishedAt: string;
}

export interface ResidentDashboard {
  unitIdentifier: string;
  currentBalance: number;
  daysOverdue: number;
  oldestDebtDate: string | null;
  openPqrs: ResidentOpenPqr[];
  activeReservations: ResidentReservationItem[];
  latestCirculars: ResidentCircularItem[];
}

// ═══════════════════════════════════════════════════════════════════════
// SERVICE
// ═══════════════════════════════════════════════════════════════════════

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

  async getCouncilDashboard(): Promise<CouncilDashboard> {
    const response = await apiClient.get<CouncilDashboard>('/dashboard/council');
    return response.data;
  },

  async getAccountantPanel(): Promise<AccountantBudgetPanel> {
    const response = await apiClient.get<AccountantBudgetPanel>('/dashboard/accountant');
    return response.data;
  },

  async getAuditorDashboard(): Promise<AuditorDashboard> {
    const response = await apiClient.get<AuditorDashboard>('/dashboard/auditor');
    return response.data;
  },

  async getResidentDashboard(): Promise<ResidentDashboard> {
    const response = await apiClient.get<ResidentDashboard>('/dashboard/resident');
    return response.data;
  },

  async invalidateCache(): Promise<void> {
    await apiClient.post('/dashboard/invalidate-cache');
  }
};

export default dashboardService;
