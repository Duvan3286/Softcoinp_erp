import apiClient from './api-client';

export interface TenantMetrics {
  databaseName: string;
  sizeMb: number;
  tableCount: number;
  rowCount: number;
  latencyMs: number;
  activity24h: number;
}

export interface TenantDto {
  id: string;
  name: string;
  subdomain: string;
  isActive: boolean;
  createdAt: string;
  connectionString: string;
  metrics: TenantMetrics | null;
}

export interface PagedTenants {
  items: TenantDto[];
  totalCount: number;
  totalPages: number;
  page: number;
  pageSize: number;
}

export interface CreateTenantRequest {
  subdomain: string;
}

export interface CreateTenantResponse {
  tenant: TenantDto;
  initialization: string;
}

export interface ToggleTenantStatusResponse {
  id: string;
  isActive: boolean;
}

const tenantService = {
  async getAllTenants(page: number, pageSize: number): Promise<PagedTenants> {
    const response = await apiClient.get<PagedTenants>('/v1/admin/tenants', {
      params: { page, pageSize },
    });
    return response.data;
  },

  async createTenant(request: CreateTenantRequest): Promise<CreateTenantResponse> {
    const response = await apiClient.post<CreateTenantResponse>('/v1/admin/tenants', request);
    return response.data;
  },

  async toggleStatus(id: string): Promise<ToggleTenantStatusResponse> {
    const response = await apiClient.patch<ToggleTenantStatusResponse>(`/v1/admin/tenants/${id}/toggle-status`, {});
    return response.data;
  },
};

export default tenantService;
