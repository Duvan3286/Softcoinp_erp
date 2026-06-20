import apiClient from './api-client';

export interface AccountingAccount {
  id: string;
  code: string;
  name: string;
  category: string;
  nature: string;
  isGroup: boolean;
  isActive: boolean;
  isOfficialStandard: boolean;
}

export interface CreateAuxiliaryAccountRequest {
  parentCode: string;
  subCode: string;
  name: string;
  isGroup: boolean;
}

export interface UpdateAccountingAccountRequest {
  name: string;
  isActive: boolean;
}

const accountingService = {
  async getAccounts(): Promise<AccountingAccount[]> {
    const response = await apiClient.get<AccountingAccount[]>('/accounting-accounts');
    return response.data;
  },

  async createAuxiliaryAccount(request: CreateAuxiliaryAccountRequest): Promise<AccountingAccount> {
    const response = await apiClient.post<AccountingAccount>('/accounting-accounts', request);
    return response.data;
  },

  async updateAccount(id: string, request: UpdateAccountingAccountRequest): Promise<AccountingAccount> {
    const response = await apiClient.put<AccountingAccount>(`/accounting-accounts/${id}`, request);
    return response.data;
  },

  async deleteAccount(id: string): Promise<void> {
    await apiClient.delete(`/accounting-accounts/${id}`);
  }
};

export default accountingService;
