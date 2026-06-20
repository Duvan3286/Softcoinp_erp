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

// ── Accounting Periods ─────────────────────────────────────────────

export interface AccountingPeriod {
  id: string;
  fiscalYear: number;
  month: number;
  periodLabel: string;
  status: string;
  openedAt: string;
  closedAt?: string;
  closedByUserId?: string;
  lastEntryNumber: number;
}

export interface CreateAccountingPeriodRequest {
  fiscalYear: number;
  month: number;
  periodLabel: string;
}

// ── Journal Entries ────────────────────────────────────────────────

export interface JournalEntryLine {
  id: string;
  accountingAccountId: string;
  accountCode: string;
  accountName: string;
  thirdPartyId?: string;
  debit: number;
  credit: number;
}

export interface CreateJournalEntryLine {
  accountingAccountId: string;
  thirdPartyId?: string;
  debit: number;
  credit: number;
}

export interface JournalEntry {
  id: string;
  tenantId: string;
  accountingPeriodId?: string;
  periodLabel?: string;
  entryNumber: number;
  entryType: string;
  status: string;
  entryDate: string;
  description: string;
  externalReference?: string;
  totalDebit: number;
  totalCredit: number;
  createdByUserId: string;
  createdAt: string;
  lines: JournalEntryLine[];
}

export interface CreateJournalEntryRequest {
  entryDate: string;
  description: string;
  externalReference?: string;
  accountingPeriodId?: string;
  entryType: string;
  lines: CreateJournalEntryLine[];
}

export interface ReverseJournalEntryRequest {
  reason: string;
}

// ── Reports ────────────────────────────────────────────────────────

export interface TrialBalanceItem {
  accountCode: string;
  accountName: string;
  nature: string;
  category: string;
  totalDebit: number;
  totalCredit: number;
  balance: number;
}

export interface GeneralLedgerEntry {
  date: string;
  entryNumber: number;
  description: string;
  externalReference?: string;
  debit: number;
  credit: number;
  runningBalance: number;
}

export interface IncomeStatementItem {
  accountCode: string;
  accountName: string;
  balance: number;
}

export interface BalanceSheetItem {
  accountCode: string;
  accountName: string;
  balance: number;
}

const accountingService = {
  // Accounts
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
  },

  // Periods
  async getPeriods(): Promise<AccountingPeriod[]> {
    const response = await apiClient.get<AccountingPeriod[]>('/accounting-periods');
    return response.data;
  },

  async getCurrentPeriod(): Promise<AccountingPeriod> {
    const response = await apiClient.get<AccountingPeriod>('/accounting-periods/current');
    return response.data;
  },

  async openPeriod(request: CreateAccountingPeriodRequest): Promise<AccountingPeriod> {
    const response = await apiClient.post<AccountingPeriod>('/accounting-periods', request);
    return response.data;
  },

  async closePeriod(id: string): Promise<AccountingPeriod> {
    const response = await apiClient.post<AccountingPeriod>(`/accounting-periods/${id}/close`);
    return response.data;
  },

  // Journal Entries
  async getEntries(params?: {
    fromDate?: string; toDate?: string; periodId?: string;
    status?: string; entryType?: string; page?: number; pageSize?: number;
  }): Promise<JournalEntry[]> {
    const response = await apiClient.get<JournalEntry[]>('/journal-entries', { params });
    return response.data;
  },

  async getEntry(id: string): Promise<JournalEntry> {
    const response = await apiClient.get<JournalEntry>(`/journal-entries/${id}`);
    return response.data;
  },

  async createEntry(request: CreateJournalEntryRequest): Promise<JournalEntry> {
    const response = await apiClient.post<JournalEntry>('/journal-entries', request);
    return response.data;
  },

  async postEntry(id: string): Promise<JournalEntry> {
    const response = await apiClient.post<JournalEntry>(`/journal-entries/${id}/post`);
    return response.data;
  },

  async reverseEntry(id: string, request: ReverseJournalEntryRequest): Promise<JournalEntry> {
    const response = await apiClient.post<JournalEntry>(`/journal-entries/${id}/reverse`, request);
    return response.data;
  },

  // Reports
  async getTrialBalance(params?: { periodId?: string; fromDate?: string; toDate?: string }): Promise<TrialBalanceItem[]> {
    const response = await apiClient.get<TrialBalanceItem[]>('/accounting-reports/trial-balance', { params });
    return response.data;
  },

  async getGeneralLedger(accountId: string, params?: { fromDate?: string; toDate?: string }): Promise<GeneralLedgerEntry[]> {
    const response = await apiClient.get<GeneralLedgerEntry[]>(`/accounting-reports/general-ledger/${accountId}`, { params });
    return response.data;
  },

  async getIncomeStatement(params?: { periodId?: string }): Promise<IncomeStatementItem[]> {
    const response = await apiClient.get<IncomeStatementItem[]>('/accounting-reports/income-statement', { params });
    return response.data;
  },

  async getBalanceSheet(params?: { periodId?: string }): Promise<BalanceSheetItem[]> {
    const response = await apiClient.get<BalanceSheetItem[]>('/accounting-reports/balance-sheet', { params });
    return response.data;
  }
};

export default accountingService;
