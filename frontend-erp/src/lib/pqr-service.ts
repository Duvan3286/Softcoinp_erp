import apiClient from './api-client';

export interface CreatePqrRequest {
  pqrType: string;
  category: string;
  subject: string;
  description: string;
  unitId: string;
  radiadorName: string;
  radiadorDocumentType?: string;
  radiadorDocumentNumber?: string;
  radiadorContact?: string;
  ownerId?: string;
  tenantResidentId?: string;
  channel: string;
  relatedPQRId?: string;
  isInternal: boolean;
  involvedResidentName?: string;
  involvedResidentUnitId?: string;
  isLinkedToCharge: boolean;
  unitFeeId?: string;
  extraordinaryFeeDistributionId?: string;
  individualChargeId?: string;
}

export interface PqrCreatedResponse {
  id: string;
  radicadoNumber: string;
  pqrType: string;
  status: string;
  subject: string;
  filedAt: string;
  deadline: string;
  progressPercent: number;
}

export interface PqrListItem {
  id: string;
  radicadoNumber: string;
  pqrType: string;
  category: string;
  status: string;
  priority: string;
  subject: string;
  unitIdentifier: string;
  radiadorName: string;
  filedAt: string;
  deadline: string;
  elapsedPercent: number;
  isInternal: boolean;
}

export interface PqrDetail {
  id: string;
  radicadoNumber: string;
  pqrType: string;
  category: string;
  status: string;
  priority: string;
  subject: string;
  description: string;
  radiadorName: string;
  radiadorDocumentType: string;
  radiadorDocumentNumber: string;
  radiadorContact: string;
  unitId: string;
  unitIdentifier: string;
  channel: string;
  relatedPQRId: string;
  relatedRadicadoNumber: string;
  assignedToUserId: string;
  deadline: string;
  elapsedPercent: number;
  isInternal: boolean;
  involvedResidentName: string;
  involvedResidentUnitId: string;
  isLinkedToCharge: boolean;
  claimResolved: boolean;
  claimResolutionNote: string;
  creditNoteGenerated: boolean;
  filedAt: string;
  closedAt: string;
  closedDefinitivelyAt: string;
  followUps: PqrFollowUpItem[];
  responses: PqrResponseItem[];
  internalNotes: PqrInternalNoteItem[];
  files: PqrFileItem[];
  alerts: PqrAlertItem[];
}

export interface PqrFollowUpItem {
  id: string;
  previousStatus: string;
  newStatus: string;
  changedAt: string;
  changedByUserName: string;
  justification: string;
  isAutomatic: boolean;
}

export interface PqrResponseItem {
  id: string;
  responseText: string;
  isDefinitive: boolean;
  isPartialUpdate: boolean;
  sentAt: string;
  sentByUserName: string;
  requiresConfirmation: boolean;
  confirmedByRadiador: boolean;
  confirmedAt: string;
  files: PqrFileItem[];
}

export interface PqrInternalNoteItem {
  id: string;
  noteText: string;
  authorName: string;
  createdAt: string;
}

export interface PqrFileItem {
  id: string;
  fileName: string;
  originalFileName: string;
  contentType: string;
  fileSize: number;
  uploadedByUserName: string;
  uploadedAt: string;
  isFromApplicant: boolean;
}

export interface PqrAlertItem {
  id: string;
  alertType: string;
  generatedAt: string;
  isActive: boolean;
  resolvedAt: string;
  escalatedToCouncil: boolean;
}

export interface PqrTimeConfig {
  pqrType: string;
  businessDays: number;
}

export interface ResolveClaimRequest {
  resolved: boolean;
  resolutionNote: string;
}

export interface ChangeStatusRequest {
  status: string;
  justification: string;
}

export interface AssignPqrRequest {
  assignedToUserId: string;
  assignedToUserName: string;
}

export interface UpdatePriorityRequest {
  priority: string;
}

export interface AddResponseRequest {
  responseText: string;
  isDefinitive: boolean;
  isPartialUpdate: boolean;
  requiresConfirmation: boolean;
}

export interface AddInternalNoteRequest {
  noteText: string;
}

export interface ReopenPqrRequest {
  justification: string;
}

export interface ConfirmResponseRequest {
  confirmed: boolean;
}

export interface PqrIndicators {
  totalPQRs: number;
  openPQRs: number;
  closedPQRs: number;
  escalatedPQRs: number;
  activeAlerts: number;
  averageResponseHours: number;
  byType: { type: string; count: number; openCount: number }[];
  byCategory: { category: string; count: number }[];
  byStatus: { status: string; count: number }[];
  monthlyTrend: { period: string; count: number }[];
  averageResponseByType: { type: string; averageResponseHours: number; count: number }[];
}

export interface ActiveAlert {
  id: string;
  alertType: string;
  generatedAt: string;
  escalatedToCouncil: boolean;
  pqr: {
    id: string;
    radicadoNumber: string;
    pqrType: string;
    status: string;
    subject: string;
    unitIdentifier: string;
    deadline: string;
    filedAt: string;
  };
}

const pqrService = {
  async createPqr(request: CreatePqrRequest): Promise<PqrCreatedResponse> {
    const response = await apiClient.post<PqrCreatedResponse>('/pqr', request);
    return response.data;
  },

  async getPqrList(status?: string, type?: string, isInternal?: boolean): Promise<PqrListItem[]> {
    const params = new URLSearchParams();
    if (status) params.append('status', status);
    if (type) params.append('type', type);
    if (isInternal !== undefined) params.append('isInternal', String(isInternal));
    const query = params.toString();
    const response = await apiClient.get<PqrListItem[]>(`/pqr${query ? '?' + query : ''}`);
    return response.data;
  },

  async getPqrDetail(id: string): Promise<PqrDetail> {
    const response = await apiClient.get<PqrDetail>(`/pqr/${id}`);
    return response.data;
  },

  async changeStatus(id: string, request: ChangeStatusRequest): Promise<void> {
    await apiClient.put(`/pqr/${id}/status`, request);
  },

  async assignPqr(id: string, request: AssignPqrRequest): Promise<void> {
    await apiClient.put(`/pqr/${id}/assign`, request);
  },

  async updatePriority(id: string, request: UpdatePriorityRequest): Promise<void> {
    await apiClient.put(`/pqr/${id}/priority`, request);
  },

  async addResponse(id: string, request: AddResponseRequest): Promise<{ id: string; message: string }> {
    const response = await apiClient.post<{ id: string; message: string }>(`/pqr/${id}/responses`, request);
    return response.data;
  },

  async confirmResponse(id: string, responseId: string, request: ConfirmResponseRequest): Promise<void> {
    await apiClient.post(`/pqr/${id}/responses/${responseId}/confirm`, request);
  },

  async addInternalNote(id: string, request: AddInternalNoteRequest): Promise<void> {
    await apiClient.post(`/pqr/${id}/internal-notes`, request);
  },

  async reopenPqr(id: string, request: ReopenPqrRequest): Promise<void> {
    await apiClient.post(`/pqr/${id}/reopen`, request);
  },

  async resolveClaim(id: string, request: ResolveClaimRequest): Promise<{ message: string }> {
    const response = await apiClient.post<{ message: string }>(`/pqr/${id}/resolve-claim`, request);
    return response.data;
  },

  async getTimeConfig(): Promise<PqrTimeConfig[]> {
    const response = await apiClient.get<PqrTimeConfig[]>('/pqr/time-config');
    return response.data;
  },

  async updateTimeConfig(request: PqrTimeConfig): Promise<void> {
    await apiClient.put('/pqr/time-config', request);
  },

  async getActiveAlerts(): Promise<ActiveAlert[]> {
    const response = await apiClient.get<ActiveAlert[]>('/pqr/alerts/active');
    return response.data;
  },

  async resolveAlert(alertId: string): Promise<void> {
    await apiClient.post(`/pqr/alerts/${alertId}/resolve`);
  },

  async getIndicators(): Promise<PqrIndicators> {
    const response = await apiClient.get<PqrIndicators>('/pqr/indicators');
    return response.data;
  },

  async getResidentPqrs(ownerId: string): Promise<PqrListItem[]> {
    const response = await apiClient.get<PqrListItem[]>(`/pqr/resident/${ownerId}`);
    return response.data;
  },
};

export default pqrService;
