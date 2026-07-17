import apiClient, { setAuthCookie, clearAuthCookie } from './api-client';

const API_URL = process.env.NEXT_PUBLIC_API_URL || '/api';
const isSameOrigin = API_URL.startsWith('/');

export type AppRole = 'SuperAdmin' | 'Admin';

export interface User {
  id: string;
  name: string;
  email: string;
  role: AppRole;
  isSuspended?: boolean;
  lastLogin?: string;
  tenantId?: string;
  tenantName?: string;
  tenantSubdomain?: string;
}

export interface LoginResponse {
  user: User;
  token: string;
  refreshToken: string;
}

export interface LoginCredentials {
  email: string;
  password: string;
}

export interface TenantOption {
  tenantId: string;
  name: string;
  subdomain: string;
  role: AppRole;
  isCurrent: boolean;
}

export interface SwitchTenantResponse {
  token: string;
  tokenExpiry: string;
  refreshToken: string;
  tenantId: string;
  role: AppRole;
}

const authService = {
  async login(credentials: LoginCredentials): Promise<LoginResponse> {
    const response = await apiClient.post<LoginResponse>('/auth/login', credentials);
    const { token, refreshToken } = response.data;

    // Modo cross-origen: guardar en sessionStorage + cookie para middleware
    if (!isSameOrigin && typeof window !== 'undefined') {
      sessionStorage.setItem('auth_token', token);
      if (refreshToken) {
        sessionStorage.setItem('refresh_token', refreshToken);
      }
      setAuthCookie(token);
    }
    // Modo mismo origen: la cookie httpOnly la maneja el backend

    return response.data;
  },

  async logout(): Promise<void> {
    try {
      if (!isSameOrigin) {
        const refreshToken = typeof window !== 'undefined' ? sessionStorage.getItem('refresh_token') : null;
        await apiClient.post('/auth/logout', { refreshToken });
      } else {
        await apiClient.post('/auth/logout', {});
      }
    } catch (error) {
      console.warn('Backend logout failed, but clearing local session anyway', error);
    } finally {
      if (!isSameOrigin && typeof window !== 'undefined') {
        sessionStorage.removeItem('auth_token');
        sessionStorage.removeItem('refresh_token');
        clearAuthCookie();
      }
    }
  },

  async getCurrentUser(): Promise<User> {
    const response = await apiClient.get<User>('/auth/me');
    return response.data;
  },

  async getMyTenants(): Promise<TenantOption[]> {
    const response = await apiClient.get<TenantOption[]>('/auth/my-tenants');
    return response.data;
  },

  async switchTenant(tenantId: string): Promise<SwitchTenantResponse> {
    const response = await apiClient.post<SwitchTenantResponse>('/auth/switch-tenant', { tenantId });
    const { token, refreshToken } = response.data;

    if (!isSameOrigin && typeof window !== 'undefined') {
      sessionStorage.setItem('auth_token', token);
      if (refreshToken) {
        sessionStorage.setItem('refresh_token', refreshToken);
      }
      setAuthCookie(token);
    }

    return response.data;
  }
};

export default authService;
