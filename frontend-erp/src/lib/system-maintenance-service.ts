import apiClient from './api-client';

export interface CreateUserRequest {
  fullName: string;
  email: string;
  password: string;
}

export interface ResetPasswordRequest {
  newPassword: string;
}

export interface EditUserRequest {
  fullName?: string;
  email?: string;
}

export interface SuspendUserRequest {
  reason?: string;
}

export interface UserListItem {
  id: string;
  fullName: string;
  email: string;
  status: string;
  isSuspended: boolean;
  createdAt: string;
  lastLogin?: string;
}

export interface UserDetail {
  id: string;
  fullName: string;
  email: string;
  status: string;
  createdAt: string;
  updatedAt?: string;
  lastLogin?: string;
  isSuspended: boolean;
  suspendedAt?: string;
  suspendedReason?: string;
}

export interface UserChangeHistoryItem {
  id: string;
  changedField: string;
  oldValue?: string;
  newValue?: string;
  changedAt: string;
  changeType: string;
  changedByUserId: string;
}

export interface CreateUserResponse {
  userId: string;
  fullName: string;
  email: string;
  message: string;
}

const systemMaintenanceService = {
  async getUsers(search?: string, sortBy?: string, sortOrder?: string): Promise<UserListItem[]> {
    const params = new URLSearchParams();
    if (search) params.append('search', search);
    if (sortBy) params.append('sortBy', sortBy);
    if (sortOrder) params.append('sortOrder', sortOrder);
    const query = params.toString();
    const response = await apiClient.get<UserListItem[]>(`/v1/maintenance/users${query ? '?' + query : ''}`);
    return response.data;
  },

  async getUser(id: string): Promise<UserDetail> {
    const response = await apiClient.get<UserDetail>(`/v1/maintenance/users/${id}`);
    return response.data;
  },

  async createUser(request: CreateUserRequest): Promise<CreateUserResponse> {
    const response = await apiClient.post<CreateUserResponse>('/v1/maintenance/users', request);
    return response.data;
  },

  async editUser(id: string, request: EditUserRequest): Promise<UserDetail> {
    const response = await apiClient.put<UserDetail>(`/v1/maintenance/users/${id}`, request);
    return response.data;
  },

  async deleteUser(id: string): Promise<void> {
    await apiClient.delete(`/v1/maintenance/users/${id}`);
  },

  async suspendUser(id: string, request: SuspendUserRequest): Promise<void> {
    await apiClient.post(`/v1/maintenance/users/${id}/suspend`, request);
  },

  async reactivateUser(id: string): Promise<void> {
    await apiClient.post(`/v1/maintenance/users/${id}/reactivate`, {});
  },

  async resetPassword(id: string, request: ResetPasswordRequest): Promise<void> {
    await apiClient.post(`/v1/maintenance/users/${id}/reset-password`, request);
  },

  async getUserHistory(id: string): Promise<UserChangeHistoryItem[]> {
    const response = await apiClient.get<UserChangeHistoryItem[]>(`/v1/maintenance/users/${id}/history`);
    return response.data;
  },
};

export default systemMaintenanceService;
