import apiClient, { setAuthCookie, clearAuthCookie } from './api-client';

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
      sessionStorage.setItem('auth_token', token);
      if (refreshToken) {
        sessionStorage.setItem('refresh_token', refreshToken);
      }
      setAuthCookie(token);
    }

    return response.data;
  },

  async logout(): Promise<void> {
    try {
      const refreshToken = typeof window !== 'undefined' ? sessionStorage.getItem('refresh_token') : null;
      await apiClient.post('/auth/logout', { refreshToken });
    } catch (error) {
      console.warn('Backend logout failed, but clearing local session anyway', error);
    } finally {
      if (typeof window !== 'undefined') {
        sessionStorage.removeItem('auth_token');
        sessionStorage.removeItem('refresh_token');
        clearAuthCookie();
      }
    }
  },

  async getCurrentUser(): Promise<User> {
    const response = await apiClient.get<User>('/auth/me');
    return response.data;
  }
};

export default authService;
