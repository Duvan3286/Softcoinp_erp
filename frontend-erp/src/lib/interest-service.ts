import apiClient from './api-client';

export interface MonthlyInterestRateDto {
  id: string;
  year: number;
  month: number;
  certifiedRate: number;
  appliedRate: number;
  maxAllowedRate: number;
  registeredAt: string;
  registeredByUserId: string;
}

export interface RegisterInterestRateRequest {
  year: number;
  month: number;
  certifiedRate: number;
  appliedRate: number;
}

export interface LateInterestConfigurationDto {
  id: string;
  interestStartDays: number;
  applyToAllUnitsByDefault: boolean;
  alertOnMissingMonthlyRate: boolean;
  createdAt: string;
  updatedAt?: string;
}

export interface UpdateInterestConfigurationRequest {
  interestStartDays: number;
  applyToAllUnitsByDefault: boolean;
  alertOnMissingMonthlyRate: boolean;
}

export interface UnitInterestExceptionDto {
  id: string;
  unitId: string;
  unitIdentifier: string;
  interestStartDays: number;
  reason: string;
  createdAt: string;
}

export interface UpsertInterestExceptionRequest {
  unitId: string;
  interestStartDays: number;
  reason: string;
}

export interface AccruedInterestDto {
  id: string;
  unitFeeId?: string;
  extraordinaryFeeDistributionId?: string;
  individualChargeId?: string;
  period: string;
  dailyRate: number;
  daysInPeriod: number;
  baseAmount: number;
  calculatedAmount: number;
  balanceAmount: number;
  status: string;
  interestStartDate: string;
  interestEndDate: string;
  monthlyInterestRateId: string;
  createdAt: string;
}

export interface InterestCheckResult {
  currentPeriod: string;
  hasRateForCurrentPeriod: boolean;
  alertEnabled: boolean;
  message: string;
}

export interface InterestCalculationResult {
  createdCount: number;
  updatedCount: number;
  hasMissingRates: boolean;
  alerts: string[];
  message: string;
}

export interface InterestReportLineDto {
  id: string;
  unitId: string;
  unitIdentifier: string;
  period: string;
  dailyRate: number;
  daysInPeriod: number;
  baseAmount: number;
  calculatedAmount: number;
  balanceAmount: number;
  status: string;
  interestStartDate: string;
  interestEndDate: string;
}

export interface InterestReportDto {
  lines: InterestReportLineDto[];
  totalCalculated: number;
  totalBalance: number;
  totalBaseAmount: number;
  pendingCount: number;
  paidCount: number;
  generatedAt: string;
}

const interestService = {
  async getRates(): Promise<MonthlyInterestRateDto[]> {
    const response = await apiClient.get<MonthlyInterestRateDto[]>('/billing/interest-rates');
    return response.data;
  },

  async getCurrentRate(): Promise<MonthlyInterestRateDto> {
    const response = await apiClient.get<MonthlyInterestRateDto>('/billing/interest-rates/current');
    return response.data;
  },

  async getRateByPeriod(year: number, month: number): Promise<MonthlyInterestRateDto> {
    const response = await apiClient.get<MonthlyInterestRateDto>(`/billing/interest-rates/by-period?year=${year}&month=${month}`);
    return response.data;
  },

  async getRateById(id: string): Promise<MonthlyInterestRateDto> {
    const response = await apiClient.get<MonthlyInterestRateDto>(`/billing/interest-rates/${id}`);
    return response.data;
  },

  async registerRate(request: RegisterInterestRateRequest): Promise<{ rate: MonthlyInterestRateDto; isUpdate: boolean; message: string }> {
    const response = await apiClient.post('/billing/interest-rates', request);
    return response.data;
  },

  async deleteRate(id: string): Promise<void> {
    await apiClient.delete(`/billing/interest-rates/${id}`);
  },

  async getConfiguration(): Promise<LateInterestConfigurationDto> {
    const response = await apiClient.get<LateInterestConfigurationDto>('/billing/interest-configuration');
    return response.data;
  },

  async updateConfiguration(request: UpdateInterestConfigurationRequest): Promise<void> {
    await apiClient.put('/billing/interest-configuration', request);
  },

  async getExceptions(): Promise<UnitInterestExceptionDto[]> {
    const response = await apiClient.get<UnitInterestExceptionDto[]>('/billing/interest-exceptions');
    return response.data;
  },

  async getExceptionForUnit(unitId: string): Promise<UnitInterestExceptionDto> {
    const response = await apiClient.get<UnitInterestExceptionDto>(`/billing/interest-exceptions/${unitId}`);
    return response.data;
  },

  async upsertException(request: UpsertInterestExceptionRequest): Promise<void> {
    await apiClient.post('/billing/interest-exceptions', request);
  },

  async deleteException(id: string): Promise<void> {
    await apiClient.delete(`/billing/interest-exceptions/${id}`);
  },

  async calculateInterests(unitId: string): Promise<InterestCalculationResult> {
    const response = await apiClient.post<InterestCalculationResult>('/billing/interest/calculate', { unitId });
    return response.data;
  },

  async checkMissingRates(): Promise<InterestCheckResult> {
    const response = await apiClient.get<InterestCheckResult>('/billing/interest/check-missing-rates');
    return response.data;
  },

  async getAccruedInterests(unitId: string): Promise<AccruedInterestDto[]> {
    const response = await apiClient.get<AccruedInterestDto[]>(`/billing/units/${unitId}/accrued-interests`);
    return response.data;
  },

  async getReport(unitId?: string, status?: string, from?: string, to?: string): Promise<InterestReportDto> {
    const params = new URLSearchParams();
    if (unitId) params.append('unitId', unitId);
    if (status) params.append('status', status);
    if (from) params.append('from', from);
    if (to) params.append('to', to);
    const response = await apiClient.get<InterestReportDto>(`/billing/reports/interest?${params.toString()}`);
    return response.data;
  },

  async exportReport(format: 'excel' | 'pdf', unitId?: string, status?: string, from?: string, to?: string): Promise<Blob> {
    const params = new URLSearchParams();
    params.append('format', format);
    if (unitId) params.append('unitId', unitId);
    if (status) params.append('status', status);
    if (from) params.append('from', from);
    if (to) params.append('to', to);
    const response = await apiClient.get(`/billing/reports/interest/export?${params.toString()}`, {
      responseType: 'blob',
    });
    return response.data;
  },
};

export default interestService;
