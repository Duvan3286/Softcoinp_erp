import apiClient from './api-client';

export interface TenantConfiguration {
  officialName: string;
  nit: string;
  verificationDigit: string;
  address: string;
  municipality: string;
  department: string;
  phone: string;
  email: string;
  realEstateRegistration: string;
  constitutionDate: string;
  legalRepresentativeName: string;
  legalRepresentativeDocumentType: string;
  legalRepresentativeId: string;
  legalRepresentativeDv: string;
  
  billingCycleDay: number;
  gracePeriodDays: number;
  fiscalYearStartMonth: number;
  fiscalYearStartDay: number;
  annualBudget: number;
  
  totalUnits: number;
  totalTowers: number;
  roundingPolicy: number;
  maxActiveExtraordinaryQuotas: number;
  hasContingencyFund: boolean;
  contingencyFundPercentage: number;
  
  senderEmail: string;
  signatureFooterTemplate: string;
  autoSendLatePaymentNotifications: boolean;
  latePaymentNotificationFrequencyDays: number;
  
  logoUrl?: string;
}

export interface ConfigurationAuditLog {
  id: string;
  timestamp: string;
  changedByUserId: string;
  parameterName: string;
  oldValue: string;
  newValue: string;
}

export interface LegalRepresentativeHistory {
  id: string;
  fullName: string;
  identificationDocument: string;
  startDate: string;
  endDate?: string;
  recordedAt: string;
}

export interface TenantDocument {
  id: string;
  title: string;
  type: number;
  filePath?: string;
  contentType?: string;
  fileSize: number;
  uploadedAt: string;
}

const tenantConfigService = {
  async getConfig(): Promise<TenantConfiguration> {
    const response = await apiClient.get<TenantConfiguration>('/tenant-config');
    return response.data;
  },

  async updateConfig(config: TenantConfiguration): Promise<TenantConfiguration> {
    const response = await apiClient.put<TenantConfiguration>('/tenant-config', config);
    return response.data;
  },

  async uploadLogo(file: File): Promise<{ logoUrl: string }> {
    const formData = new FormData();
    formData.append('file', file);
    const response = await apiClient.post<{ logoUrl: string }>('/tenant-config/logo', formData, {
      headers: { 'Content-Type': 'multipart/form-data' }
    });
    return response.data;
  },

  async getAuditLogs(): Promise<ConfigurationAuditLog[]> {
    const response = await apiClient.get<ConfigurationAuditLog[]>('/tenant-config/audit');
    return response.data;
  },

  async getRepresentatives(): Promise<LegalRepresentativeHistory[]> {
    const response = await apiClient.get<LegalRepresentativeHistory[]>('/tenant-config/representatives');
    return response.data;
  },

  async getDocuments(): Promise<TenantDocument[]> {
    const response = await apiClient.get<TenantDocument[]>('/tenant-config/documents');
    return response.data;
  },

  async uploadDocument(file: File, type: number, title: string): Promise<TenantDocument> {
    const formData = new FormData();
    formData.append('file', file);
    formData.append('type', type.toString());
    formData.append('title', title);
    
    const response = await apiClient.post<TenantDocument>('/tenant-config/documents', formData, {
      headers: { 'Content-Type': 'multipart/form-data' }
    });
    return response.data;
  },
  
  async downloadDocument(id: string, defaultFileName: string): Promise<void> {
    const response = await apiClient.get(`/tenant-config/documents/${id}/download`, {
      responseType: 'blob'
    });
    
    const url = window.URL.createObjectURL(new Blob([response.data]));
    const link = document.createElement('a');
    link.href = url;
    
    let fileName = defaultFileName;
    const contentDisposition = response.headers['content-disposition'];
    if (contentDisposition) {
      const fileNameMatch = contentDisposition.match(/filename="?([^"]+)"?/);
      if (fileNameMatch && fileNameMatch.length === 2) {
        fileName = fileNameMatch[1];
      }
    }
    
    link.setAttribute('download', fileName);
    document.body.appendChild(link);
    link.click();
    link.remove();
    window.URL.revokeObjectURL(url);
  }
};

export default tenantConfigService;
