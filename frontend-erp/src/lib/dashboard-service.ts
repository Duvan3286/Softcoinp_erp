import apiClient from './api-client';

export interface DashboardData {
  kpis: DashboardKpis;
  alerts: AlertDto[];
  monthlyCollection: MonthlyCollectionDto[];
  moraMap: UnitMoraDto[];
  upcomingEvents: UpcomingEventDto[];
  recentActivity: RecentActivityDto[];
  unitSummaries: UnitSummaryDto[];
  contingencyFund: ContingencyFundInfoDto | null;
  pendingCouncilApprovals: CouncilApprovalDto[];
  accountingStatus: AccountingStatusDto | null;
  residentData: ResidentDashboardDto | null;
}

export interface DashboardKpis {
  currentMonthCollectionPercentage: number;
  previousMonthCollectionPercentage: number;
  daysElapsedInPeriod: number;
  totalDaysInPeriod: number;
  totalOverduePortfolio: number;
  earlyOverdue: number;
  mediumOverdue: number;
  legalOverdue: number;
  availableCash: number;
  budgetExecutionPercentage: number;
  yearProgressPercentage: number;
  openPqrCount: number;
  overduePqrCount: number;
  currentMonthBilled: number;
  currentMonthCollected: number;
}

export interface AlertDto {
  id: string;
  ruleType: string;
  urgency: number;
  title: string;
  description: string;
  moduleLink: string;
  createdAt: string;
}

export interface MonthlyCollectionDto {
  period: string;
  billed: number;
  collected: number;
}

export interface UnitMoraDto {
  unitId: string;
  identifier: string;
  towerOrBlock: string;
  floorLevel: number;
  ownerName: string;
  overdueBalance: number;
  daysOverdue: number;
  colorCode: string;
  status: string;
}

export interface UpcomingEventDto {
  title: string;
  description: string;
  eventDate: string;
  eventType: string;
  moduleLink: string;
}

export interface RecentActivityDto {
  action: string;
  description: string;
  userName: string;
  timestamp: string;
  moduleLink: string;
}

export interface UnitSummaryDto {
  unitId: string;
  identifier: string;
  towerOrBlock: string;
  floorLevel: number;
  ownerName: string;
  currentBalance: number;
  colorCode: string;
  status: string;
}

export interface ContingencyFundInfoDto {
  currentBalance: number;
  lastContributionAmount: number;
  lastContributionPeriod: string;
}

export interface CouncilApprovalDto {
  type: string;
  description: string;
  amount: number;
  requestedAt: string;
  moduleLink: string;
}

export interface AccountingStatusDto {
  currentPeriodLabel: string;
  periodStatus: string;
  unreconciledBankAccounts: number;
  draftEntryCount: number;
  daysSinceMonthEnd: number;
}

export interface ResidentDashboardDto {
  unitIdentifier: string;
  currentBalance: number;
  lateInterestAccrued: number;
  dailyInterestRate: number;
  daysOverdue: number;
  oldestDebtDate: string;
}

const dashboardService = {
  async getDashboard(): Promise<DashboardData> {
    const response = await apiClient.get<DashboardData>('/dashboard');
    return response.data;
  },

  async initializeAlerts(): Promise<void> {
    await apiClient.post('/dashboard/initialize-alerts');
  },

  async invalidateCache(): Promise<void> {
    await apiClient.post('/dashboard/invalidate-cache');
  }
};

export default dashboardService;
