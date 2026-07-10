import apiClient from './api-client';

export interface CommonAssetListItem {
  id: string;
  name: string;
  category: string;
  location: string;
  isEssential: boolean;
  status: string;
  brand: string;
  model: string;
  hasWarranty: boolean;
  warrantyEndDate: string | null;
  nextMaintenanceDate: string | null;
  pendingWorkOrders: number;
  createdAt: string;
}

export interface AssetPhoto {
  id: string;
  filePath: string;
  description: string;
  capturedAt: string;
}

export interface MaintenancePlanSummary {
  id: string;
  activityType: string;
  description: string;
  frequencyDays: number;
  preferredProviderName: string;
  estimatedCost: number;
  requiresServiceSuspension: boolean;
  isActive: boolean;
  lastExecutionDate: string | null;
  nextExecutionDate: string | null;
}

export interface WorkOrderSummary {
  id: string;
  orderType: string;
  description: string;
  priority: string;
  assignedProviderName: string;
  scheduledDate: string | null;
  executionEndDate: string | null;
  actualCost: number;
  status: string;
  outcome: string | null;
  createdAt: string;
}

export interface AssetStatusHistory {
  id: string;
  previousStatus: string;
  newStatus: string;
  reason: string;
  changedByUserName: string;
  changedAt: string;
}

export interface CommonAssetDetail {
  id: string;
  name: string;
  category: string;
  location: string;
  isEssential: boolean;
  brand: string;
  model: string;
  serialNumber: string;
  acquisitionDate: string | null;
  acquisitionValue: number;
  estimatedUsefulLifeMonths: number;
  referenceProviderId: string | null;
  referenceProviderName: string;
  reservableSpaceId: string | null;
  reservableSpaceName: string;
  manufacturer: string;
  hasWarranty: boolean;
  warrantyEndDate: string | null;
  status: string;
  statusNotes: string;
  createdByUserId: string;
  createdAt: string;
  updatedAt: string | null;
  photos: AssetPhoto[];
  maintenancePlans: MaintenancePlanSummary[];
  workOrders: WorkOrderSummary[];
  statusHistory: AssetStatusHistory[];
}

export interface CreateCommonAssetRequest {
  name: string;
  category: string;
  location: string;
  isEssential: boolean;
  brand?: string;
  model?: string;
  serialNumber?: string;
  acquisitionDate?: string;
  acquisitionValue?: number;
  estimatedUsefulLifeMonths?: number;
  referenceProviderId?: string;
  reservableSpaceId?: string;
  manufacturer?: string;
  hasWarranty: boolean;
  warrantyEndDate?: string;
  statusNotes?: string;
}

export interface UpdateCommonAssetRequest {
  name?: string;
  category?: string;
  location?: string;
  isEssential?: boolean;
  brand?: string;
  model?: string;
  serialNumber?: string;
  acquisitionDate?: string;
  acquisitionValue?: number;
  estimatedUsefulLifeMonths?: number;
  referenceProviderId?: string;
  reservableSpaceId?: string;
  manufacturer?: string;
  hasWarranty?: boolean;
  warrantyEndDate?: string;
  status?: string;
  statusNotes?: string;
}

export interface WorkOrderListItem {
  id: string;
  orderType: string;
  assetName: string;
  assetLocation: string;
  description: string;
  priority: string;
  origin: string;
  assignedProviderName: string;
  scheduledDate: string | null;
  executionEndDate: string | null;
  estimatedCost: number;
  actualCost: number;
  status: string;
  outcome: string | null;
  relatedPqrNumber: string;
  createdAt: string;
}

export interface WorkOrderEvidence {
  id: string;
  filePath: string;
  description: string;
  isBeforeIntervention: boolean;
  capturedAt: string;
}

export interface WorkOrderDetail {
  id: string;
  orderType: string;
  assetId: string;
  assetName: string;
  assetLocation: string;
  description: string;
  priority: string;
  origin: string;
  relatedPqrId: string | null;
  relatedPqrNumber: string;
  assignedProviderId: string | null;
  assignedProviderName: string;
  scheduledDate: string | null;
  executionStartDate: string | null;
  executionEndDate: string | null;
  estimatedCost: number;
  actualCost: number;
  budgetItemId: string | null;
  budgetItemName: string;
  status: string;
  outcome: string | null;
  outcomeNotes: string;
  costAlertSent: boolean;
  createdByUserId: string;
  createdAt: string;
  updatedAt: string | null;
  evidences: WorkOrderEvidence[];
}

export interface CreateWorkOrderRequest {
  orderType: string;
  assetId: string;
  description: string;
  priority: string;
  origin: string;
  relatedPqrId?: string;
  assignedProviderId?: string;
  scheduledDate?: string;
  estimatedCost?: number;
  budgetItemId?: string;
}

export interface UpdateWorkOrderRequest {
  description?: string;
  priority?: string;
  assignedProviderId?: string;
  scheduledDate?: string;
  executionStartDate?: string;
  executionEndDate?: string;
  estimatedCost?: number;
  actualCost?: number;
  confirmCostDeviation?: boolean;
  budgetItemId?: string;
  status?: string;
  outcome?: string;
  outcomeNotes?: string;
}

export interface IncidentListItem {
  id: string;
  name: string;
  incidentType: string;
  occurredAt: string;
  totalDamageValue: number;
  insurancePolicyNumber: string;
  insuranceCompany: string;
  status: string;
  relatedWorkOrders: number;
  createdAt: string;
}

export interface IncidentDetail {
  id: string;
  name: string;
  description: string;
  incidentType: string;
  occurredAt: string;
  totalDamageValue: number;
  insuranceContractId: string | null;
  insuranceContractNumber: string;
  insurancePolicyNumber: string;
  insuranceCompany: string;
  policyFilePath: string;
  status: string;
  createdByUserId: string;
  createdAt: string;
  updatedAt: string | null;
  relatedWorkOrders: WorkOrderSummary[];
}

export interface CreateIncidentRequest {
  name: string;
  description: string;
  incidentType: string;
  occurredAt: string;
  totalDamageValue?: number;
  insuranceContractId?: string;
  insurancePolicyNumber?: string;
  insuranceCompany?: string;
  workOrderIds?: string[];
}

export interface MaintenanceIndicators {
  totalAssets: number;
  operationalAssets: number;
  outOfServiceAssets: number;
  essentialAssets: number;
  pendingWorkOrders: number;
  inProgressWorkOrders: number;
  completedWorkOrdersLast30Days: number;
  unassignedWorkOrders: number;
  totalCostLast30Days: number;
  upcomingMaintenances30Days: number;
}

export interface ScheduledMaintenanceItem {
  assetId: string;
  assetName: string;
  assetLocation: string;
  activityType: string;
  scheduledDate: string;
  estimatedCost: number;
  preferredProviderName: string;
}

export interface MaintenanceReport {
  daysAhead: number;
  totalEstimatedCost: number;
  budgetAvailable: number;
  scheduledItems: ScheduledMaintenanceItem[];
}

export interface OutOfServiceAsset {
  id: string;
  name: string;
  category: string;
  location: string;
  isEssential: boolean;
  hasReservationBlock: boolean;
  statusChangedAt: string;
  daysOutOfService: number;
  reason: string;
}

const maintenanceService = {
  async getAssets(category?: string, status?: string, location?: string, search?: string): Promise<CommonAssetListItem[]> {
    const params = new URLSearchParams();
    if (category) params.append('category', category);
    if (status) params.append('status', status);
    if (location) params.append('location', location);
    if (search) params.append('search', search);
    const query = params.toString();
    const response = await apiClient.get<CommonAssetListItem[]>(`/maintenance/assets${query ? '?' + query : ''}`);
    return response.data;
  },

  async getAssetById(id: string): Promise<CommonAssetDetail> {
    const response = await apiClient.get<CommonAssetDetail>(`/maintenance/assets/${id}`);
    return response.data;
  },

  async createAsset(request: CreateCommonAssetRequest): Promise<CommonAssetDetail> {
    const response = await apiClient.post<CommonAssetDetail>('/maintenance/assets', request);
    return response.data;
  },

  async updateAsset(id: string, request: UpdateCommonAssetRequest): Promise<CommonAssetDetail> {
    const response = await apiClient.put<CommonAssetDetail>(`/maintenance/assets/${id}`, request);
    return response.data;
  },

  async deleteAsset(id: string): Promise<void> {
    await apiClient.delete(`/maintenance/assets/${id}`);
  },

  async createMaintenancePlan(request: CreateCommonAssetRequest): Promise<MaintenancePlanSummary> {
    const response = await apiClient.post<MaintenancePlanSummary>('/maintenance/plans', request);
    return response.data;
  },

  async updateMaintenancePlan(id: string, request: Partial<CreateCommonAssetRequest>): Promise<MaintenancePlanSummary> {
    const response = await apiClient.put<MaintenancePlanSummary>(`/maintenance/plans/${id}`, request);
    return response.data;
  },

  async deleteMaintenancePlan(id: string): Promise<void> {
    await apiClient.delete(`/maintenance/plans/${id}`);
  },

  async getWorkOrders(orderType?: string, status?: string, priority?: string, assignedProviderId?: string, search?: string): Promise<WorkOrderListItem[]> {
    const params = new URLSearchParams();
    if (orderType) params.append('orderType', orderType);
    if (status) params.append('status', status);
    if (priority) params.append('priority', priority);
    if (assignedProviderId) params.append('assignedProviderId', assignedProviderId);
    if (search) params.append('search', search);
    const query = params.toString();
    const response = await apiClient.get<WorkOrderListItem[]>(`/maintenance/work-orders${query ? '?' + query : ''}`);
    return response.data;
  },

  async getWorkOrderById(id: string): Promise<WorkOrderDetail> {
    const response = await apiClient.get<WorkOrderDetail>(`/maintenance/work-orders/${id}`);
    return response.data;
  },

  async createWorkOrder(request: CreateWorkOrderRequest): Promise<WorkOrderDetail> {
    const response = await apiClient.post<WorkOrderDetail>('/maintenance/work-orders', request);
    return response.data;
  },

  async updateWorkOrder(id: string, request: UpdateWorkOrderRequest): Promise<WorkOrderDetail> {
    const response = await apiClient.put<WorkOrderDetail>(`/maintenance/work-orders/${id}`, request);
    return response.data;
  },

  async deleteWorkOrder(id: string): Promise<void> {
    await apiClient.delete(`/maintenance/work-orders/${id}`);
  },

  async getIncidents(status?: string): Promise<IncidentListItem[]> {
    const params = new URLSearchParams();
    if (status) params.append('status', status);
    const query = params.toString();
    const response = await apiClient.get<IncidentListItem[]>(`/maintenance/incidents${query ? '?' + query : ''}`);
    return response.data;
  },

  async getIncidentById(id: string): Promise<IncidentDetail> {
    const response = await apiClient.get<IncidentDetail>(`/maintenance/incidents/${id}`);
    return response.data;
  },

  async createIncident(request: CreateIncidentRequest): Promise<IncidentDetail> {
    const response = await apiClient.post<IncidentDetail>('/maintenance/incidents', request);
    return response.data;
  },

  async updateIncident(id: string, request: Partial<CreateIncidentRequest>): Promise<IncidentDetail> {
    const response = await apiClient.put<IncidentDetail>(`/maintenance/incidents/${id}`, request);
    return response.data;
  },

  async getIndicators(): Promise<MaintenanceIndicators> {
    const response = await apiClient.get<MaintenanceIndicators>('/maintenance/indicators');
    return response.data;
  },

  async getScheduledReport(daysAhead?: number): Promise<MaintenanceReport> {
    const params = new URLSearchParams();
    if (daysAhead) params.append('daysAhead', daysAhead.toString());
    const query = params.toString();
    const response = await apiClient.get<MaintenanceReport>(`/maintenance/reports/scheduled${query ? '?' + query : ''}`);
    return response.data;
  },

  async getOutOfServiceAssets(): Promise<OutOfServiceAsset[]> {
    const response = await apiClient.get<OutOfServiceAsset[]>('/maintenance/out-of-service');
    return response.data;
  },
};

export default maintenanceService;
