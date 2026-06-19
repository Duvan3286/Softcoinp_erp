import apiClient from './api-client';

export interface User {
  id: string;
  name: string;
  email: string;
  role: string;
  isSuspended?: boolean;
  lastLogin?: string;
  tenantId?: string;
  tenantName?: string;
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

const authService = {
  async login(credentials: LoginCredentials): Promise<LoginResponse> {
    const response = await apiClient.post<LoginResponse>('/auth/login', credentials);
    const { token, refreshToken } = response.data;
    
    if (typeof window !== 'undefined') {
      localStorage.setItem('auth_token', token);
      localStorage.setItem('refresh_token', refreshToken);
      // Set cookie for middleware
      document.cookie = `auth_token=${token}; path=/; max-age=86400; samesite=lax`;
    }
    
    return response.data;
  },

  async logout(): Promise<void> {
    try {
      await apiClient.post('/auth/logout');
    } catch (error) {
      console.warn('Backend logout failed, but clearing local session anyway', error);
    } finally {
      if (typeof window !== 'undefined') {
        localStorage.removeItem('auth_token');
        localStorage.removeItem('refresh_token');
        document.cookie = 'auth_token=; path=/; expires=Thu, 01 Jan 1970 00:00:00 GMT';
      }
    }
  },

  async refreshToken(): Promise<string> {
    const refreshToken = localStorage.getItem('refresh_token');
    const response = await apiClient.post<{ token: string, refreshToken: string }>('/auth/refresh', { refreshToken });
    const { token, refreshToken: newRefresh } = response.data;
    
    if (typeof window !== 'undefined') {
      localStorage.setItem('auth_token', token);
      if (newRefresh) {
        localStorage.setItem('refresh_token', newRefresh);
      }
      document.cookie = `auth_token=${token}; path=/; max-age=86400; samesite=lax`;
    }
    
    return token;
  },

  async getCurrentUser(): Promise<User> {
    const response = await apiClient.get<User>('/auth/me');
    return response.data;
  }
};

export default authService;
