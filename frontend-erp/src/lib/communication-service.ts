import apiClient from './api-client';

// ── Communication (Comunicado) ────────────────────────────────────

export interface CommunicationSummary {
  id: string;
  subject: string;
  status: string;
  audienceType: string;
  requiresReadConfirmation: boolean;
  publishToBulletinBoard: boolean;
  sendAt: string | null;
  sentAt: string | null;
  recipientCount: number;
  readConfirmedCount: number;
  createdAt: string;
}

export interface CommunicationRecipient {
  id: string;
  ownerId: string | null;
  ownerName: string | null;
  tenantResidentId: string | null;
  tenantResidentName: string | null;
  recipientEmail: string;
  recipientPhone: string;
  emailStatus: string;
  smsStatus: string;
  pushStatus: string;
  bulletinBoardStatus: string;
  readConfirmedAt: string | null;
  resentCount: number;
  errorMessage: string | null;
}

export interface CommunicationDetail {
  id: string;
  subject: string;
  body: string;
  status: string;
  audienceType: string;
  selectedChannels: string[];
  sendAt: string | null;
  sentAt: string | null;
  requiresReadConfirmation: boolean;
  publishToBulletinBoard: boolean;
  relatedCommunicationId: string | null;
  filePaths: string[];
  createdByUserId: string;
  createdAt: string;
  recipients: CommunicationRecipient[];
}

export interface CreateCommunicationRequest {
  subject: string;
  body: string;
  audienceType: string;
  specificUnitIds?: string[];
  specificTowers?: string[];
  selectedChannels: string[];
  sendAt?: string | null;
  requiresReadConfirmation: boolean;
  publishToBulletinBoard: boolean;
  filePaths?: string[];
}

export interface UpdateCommunicationRequest {
  subject?: string;
  body?: string;
  audienceType?: string;
  specificUnitIds?: string[];
  specificTowers?: string[];
  selectedChannels?: string[];
  sendAt?: string | null;
  requiresReadConfirmation?: boolean;
  publishToBulletinBoard?: boolean;
  filePaths?: string[];
}

// ── Notification Template ─────────────────────────────────────────

export interface NotificationTemplate {
  id: string;
  name: string;
  eventType: string;
  forRecipientType: string;
  emailSubject: string;
  emailBody: string;
  smsBody: string;
  dynamicVariables: string[];
  isActive: boolean;
  createdAt: string;
}

export interface CreateNotificationTemplateRequest {
  name: string;
  eventType: string;
  forRecipientType: string;
  emailSubject: string;
  emailBody: string;
  smsBody: string;
  dynamicVariables?: string[];
}

export interface UpdateNotificationTemplateRequest {
  name?: string;
  emailSubject?: string;
  emailBody?: string;
  smsBody?: string;
  dynamicVariables?: string[];
  isActive?: boolean;
}

// ── Bulletin Board ────────────────────────────────────────────────

export interface BulletinBoardPost {
  id: string;
  title: string;
  content: string;
  publishedAt: string;
  expiresAt: string | null;
  isPinned: boolean;
  category: string;
  createdAt: string;
}

export interface BulletinBoardPostAdmin {
  id: string;
  title: string;
  content: string;
  publishedAt: string;
  expiresAt: string | null;
  isPinned: boolean;
  category: string;
  isDeleted: boolean;
  createdByUserId: string;
  createdAt: string;
  updatedAt: string | null;
}

export interface CreateBulletinBoardPostRequest {
  title: string;
  content: string;
  publishedAt?: string;
  expiresAt?: string | null;
  isPinned: boolean;
  category: string;
}

export interface UpdateBulletinBoardPostRequest {
  title?: string;
  content?: string;
  publishedAt?: string;
  expiresAt?: string | null;
  isPinned?: boolean;
  category?: string;
}

// ── Communication Preferences ─────────────────────────────────────

export interface CommunicationPreference {
  id: string;
  ownerId: string | null;
  ownerName: string | null;
  tenantResidentId: string | null;
  tenantResidentName: string | null;
  allowEmail: boolean;
  allowSms: boolean;
  allowPush: boolean;
  criticalNotificationsOverride: boolean;
  unsubscribedEventTypes: string[];
  notes: string | null;
  changedAt: string;
}

export interface UpdateCommunicationPreferenceRequest {
  allowEmail?: boolean;
  allowSms?: boolean;
  allowPush?: boolean;
  criticalNotificationsOverride?: boolean;
  unsubscribedEventTypes?: string[];
  notes?: string;
}

// ── Delinquency Sequence ──────────────────────────────────────────

export interface DelinquencySequenceConfig {
  id: string;
  stepNumber: number;
  daysAfterDue: number;
  templateId: string;
  templateName: string;
  isActive: boolean;
}

export interface UpdateDelinquencySequenceConfigRequest {
  daysAfterDue: number;
  templateId: string;
  isActive: boolean;
}

export interface DelinquencySequencePause {
  id: string;
  unitId: string;
  unitIdentifier: string;
  startDate: string;
  endDate: string | null;
  reason: string;
  createdAt: string;
  createdByUserId: string;
}

export interface CreateDelinquencySequencePauseRequest {
  unitId: string;
  startDate: string;
  endDate?: string | null;
  reason: string;
}

// ── Reports ───────────────────────────────────────────────────────

export interface CommunicationEffectivenessReport {
  totalCommunications: number;
  totalRecipients: number;
  emailDelivered: number;
  emailOpened: number;
  emailBounced: number;
  smsDelivered: number;
  smsFailed: number;
  pushDelivered: number;
  readConfirmations: number;
  deliveryRate: number;
  openRate: number;
  readConfirmationRate: number;
}

// ── Trigger Notification Request ──────────────────────────────────

export interface TriggerNotificationRequest {
  eventType: string;
  sourceModule: string;
  sourceEntityId: string;
  sourceEntityType: string;
  ownerId?: string | null;
  tenantResidentId?: string | null;
  variables?: Record<string, string>;
}

// ── API Service ───────────────────────────────────────────────────

const communicationService = {
  // ── Communications ──
  async getCommunications(status?: string, from?: string, to?: string): Promise<CommunicationSummary[]> {
    const params = new URLSearchParams();
    if (status) params.append('status', status);
    if (from) params.append('from', from);
    if (to) params.append('to', to);
    const qs = params.toString();
    return apiClient.get(`/api/communications${qs ? '?' + qs : ''}`);
  },

  async getCommunication(id: string): Promise<CommunicationDetail> {
    return apiClient.get(`/api/communications/${id}`);
  },

  async createCommunication(request: CreateCommunicationRequest): Promise<CommunicationDetail> {
    return apiClient.post('/api/communications', request);
  },

  async updateCommunication(id: string, request: UpdateCommunicationRequest): Promise<CommunicationDetail> {
    return apiClient.put(`/api/communications/${id}`, request);
  },

  async sendCommunication(id: string): Promise<CommunicationDetail> {
    return apiClient.post(`/api/communications/${id}/send`);
  },

  async cancelScheduled(id: string): Promise<void> {
    return apiClient.post(`/api/communications/${id}/cancel`);
  },

  async archiveCommunication(id: string): Promise<void> {
    return apiClient.post(`/api/communications/${id}/archive`);
  },

  async resendUnconfirmed(id: string): Promise<void> {
    return apiClient.post(`/api/communications/${id}/resend-unconfirmed`);
  },

  // ── Templates ──
  async getTemplates(eventType?: string): Promise<NotificationTemplate[]> {
    const params = eventType ? `?eventType=${eventType}` : '';
    return apiClient.get(`/api/communications/templates${params}`);
  },

  async getTemplate(id: string): Promise<NotificationTemplate> {
    return apiClient.get(`/api/communications/templates/${id}`);
  },

  async createTemplate(request: CreateNotificationTemplateRequest): Promise<NotificationTemplate> {
    return apiClient.post('/api/communications/templates', request);
  },

  async updateTemplate(id: string, request: UpdateNotificationTemplateRequest): Promise<NotificationTemplate> {
    return apiClient.put(`/api/communications/templates/${id}`, request);
  },

  async deleteTemplate(id: string): Promise<void> {
    return apiClient.delete(`/api/communications/templates/${id}`);
  },

  // ── Bulletin Board ──
  async getActiveBulletinPosts(): Promise<BulletinBoardPost[]> {
    return apiClient.get('/api/communications/bulletin-board');
  },

  async getAllBulletinPosts(includeArchived?: boolean): Promise<BulletinBoardPostAdmin[]> {
    const params = includeArchived ? '?includeArchived=true' : '';
    return apiClient.get(`/api/communications/bulletin-board/admin${params}`);
  },

  async getBulletinPost(id: string): Promise<BulletinBoardPostAdmin> {
    return apiClient.get(`/api/communications/bulletin-board/${id}`);
  },

  async createBulletinPost(request: CreateBulletinBoardPostRequest): Promise<BulletinBoardPostAdmin> {
    return apiClient.post('/api/communications/bulletin-board', request);
  },

  async updateBulletinPost(id: string, request: UpdateBulletinBoardPostRequest): Promise<BulletinBoardPostAdmin> {
    return apiClient.put(`/api/communications/bulletin-board/${id}`, request);
  },

  async archiveBulletinPost(id: string): Promise<void> {
    return apiClient.delete(`/api/communications/bulletin-board/${id}`);
  },

  // ── Preferences ──
  async getAllPreferences(): Promise<CommunicationPreference[]> {
    return apiClient.get('/api/communications/preferences');
  },

  async getOwnerPreferences(ownerId: string): Promise<CommunicationPreference> {
    return apiClient.get(`/api/communications/preferences/owner/${ownerId}`);
  },

  async getTenantPreferences(tenantResidentId: string): Promise<CommunicationPreference> {
    return apiClient.get(`/api/communications/preferences/tenant/${tenantResidentId}`);
  },

  async updateOwnerPreferences(ownerId: string, request: UpdateCommunicationPreferenceRequest): Promise<CommunicationPreference> {
    return apiClient.put(`/api/communications/preferences/owner/${ownerId}`, request);
  },

  async updateTenantPreferences(tenantResidentId: string, request: UpdateCommunicationPreferenceRequest): Promise<CommunicationPreference> {
    return apiClient.put(`/api/communications/preferences/tenant/${tenantResidentId}`, request);
  },

  // ── Delinquency Sequence ──
  async getDelinquencyConfig(): Promise<DelinquencySequenceConfig[]> {
    return apiClient.get('/api/communications/delinquency-config');
  },

  async updateDelinquencyConfig(stepNumber: number, request: UpdateDelinquencySequenceConfigRequest): Promise<void> {
    return apiClient.put(`/api/communications/delinquency-config/${stepNumber}`, request);
  },

  async getActiveDelinquencyPauses(): Promise<DelinquencySequencePause[]> {
    return apiClient.get('/api/communications/delinquency-pauses');
  },

  async createDelinquencyPause(request: CreateDelinquencySequencePauseRequest): Promise<void> {
    return apiClient.post('/api/communications/delinquency-pauses', request);
  },

  async removeDelinquencyPause(id: string): Promise<void> {
    return apiClient.delete(`/api/communications/delinquency-pauses/${id}`);
  },

  async runDelinquencyProcess(): Promise<string[]> {
    return apiClient.post('/api/communications/delinquency-process');
  },

  // ── Reports ──
  async getEffectivenessReport(from?: string, to?: string): Promise<CommunicationEffectivenessReport> {
    const params = new URLSearchParams();
    if (from) params.append('from', from);
    if (to) params.append('to', to);
    const qs = params.toString();
    return apiClient.get(`/api/communications/reports/effectiveness${qs ? '?' + qs : ''}`);
  },

  // ── Trigger Notification (admin) ──
  async triggerNotification(request: TriggerNotificationRequest): Promise<void> {
    return apiClient.post('/api/communications/notify', request);
  },
};

export default communicationService;
