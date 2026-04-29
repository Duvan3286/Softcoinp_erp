import apiClient from './api-client';

export interface User {
  id: string;
  name: string;
  email: string;
  role: string;
  habeasDataAccepted: boolean;
}

export interface LoginResponse {
  user: User;
  token: string;
  refreshToken: string;
}

export interface LoginCredentials {
  email: string;
  password: string;
  acceptHabeasData: boolean;
}

const authService = {
  async login(credentials: LoginCredentials): Promise<LoginResponse> {
    const response = await apiClient.post<LoginResponse>('/auth/login', credentials);
    const { token, refreshToken } = response.data;
    
    if (typeof window !== 'undefined') {
      localStorage.setItem('auth_token', token);
      localStorage.setItem('refresh_token', refreshToken);
    }
    
    return response.data;
  },

  async logout(): Promise<void> {
    try {
      await apiClient.post('/auth/logout');
    } finally {
      if (typeof window !== 'undefined') {
        localStorage.removeItem('auth_token');
        localStorage.removeItem('refresh_token');
      }
    }
  },

  async refreshToken(): Promise<string> {
    const refreshToken = localStorage.getItem('refresh_token');
    const response = await apiClient.post<{ token: string }>('/auth/refresh', { refreshToken });
    const { token } = response.data;
    
    if (typeof window !== 'undefined') {
      localStorage.setItem('auth_token', token);
    }
    
    return token;
  },

  async getCurrentUser(): Promise<User> {
    const response = await apiClient.get<User>('/auth/me');
    return response.data;
  }
};

export default authService;
