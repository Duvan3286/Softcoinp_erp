import apiClient from './api-client';

export interface UnitType {
  id: string;
  name: string;
  hasCustomLiquidationRules: boolean;
}

export interface UnitCoefficientSummary {
  totalCoefficient: number;
  pendingCoefficient: number;
  excessCoefficient: number;
  isExactlyOneHundred: boolean;
}

export interface UnitIdentifierAvailability {
  isAvailable: boolean;
  message: string | null;
}

export interface UnitFinancialStatus {
  unitId: string;
  identifier: string;
  ownerName: string;
  overdueBalance: number;
  monthsOverdue: number;
  colorCode: string;
  statusLabel: string;
}

export function formatUnitLabel(identifier: string, towerOrBlock?: string | null): string {
  if (!towerOrBlock) {
    return identifier;
  }
  const trimmedTowerOrBlock = towerOrBlock.trim();
  if (trimmedTowerOrBlock === "") {
    return identifier;
  }
  return `${identifier} - ${trimmedTowerOrBlock}`;
}

export interface Unit {
  id: string;
  identifier: string;
  unitTypeId: string;
  unitTypeName: string;
  towerOrBlock: string;
  floorLevel: number;
  privateArea: number;
  balconyArea: number;
  coproprietyCoefficient: number;
  status: number;
  hasPrivateParking: boolean;
  parkingIdentifier: string;
  hasAssignedStorage: boolean;
  storageIdentifier: string;
  constructionDeliveryDate: string | null;
  internalObservations: string;
}

export const UnitsService = {
  getTypes: async (): Promise<UnitType[]> => {
    const res = await apiClient.get<UnitType[]>('/units/types');
    return res.data;
  },
  createType: async (name: string): Promise<UnitType> => {
    const res = await apiClient.post<UnitType>('/units/types', { name, hasCustomLiquidationRules: false });
    return res.data;
  },
  getCoefficientSummary: async (): Promise<UnitCoefficientSummary> => {
    const res = await apiClient.get<UnitCoefficientSummary>('/units/coefficient-summary');
    return res.data;
  },
  getPaymentStatus: async (): Promise<UnitFinancialStatus[]> => {
    const res = await apiClient.get<UnitFinancialStatus[]>('/units/payment-status');
    return res.data;
  },
  getUnits: async (tower?: string, status?: string, identifier?: string): Promise<Unit[]> => {
    const params = new URLSearchParams();
    if (tower) {
      params.append('tower', tower);
    }
    if (status) {
      params.append('status', status);
    }
    if (identifier) {
      params.append('identifier', identifier);
    }
    const res = await apiClient.get<Unit[]>(`/units?${params.toString()}`);
    return res.data;
  },
  getUnit: async (id: string): Promise<Unit> => {
    const res = await apiClient.get<Unit>(`/units/${id}`);
    return res.data;
  },
  checkIdentifierAvailability: async (
    identifier: string,
    towerOrBlock: string,
    excludeUnitId?: string
  ): Promise<UnitIdentifierAvailability> => {
    const params = new URLSearchParams();
    params.append('identifier', identifier);
    params.append('towerOrBlock', towerOrBlock);
    if (excludeUnitId) {
      params.append('excludeUnitId', excludeUnitId);
    }
    const res = await apiClient.get<UnitIdentifierAvailability>(`/units/check-identifier?${params.toString()}`);
    return res.data;
  },
  createUnit: async (data: Partial<Unit>): Promise<void> => {
    await apiClient.post('/units', data);
  },
  updateUnit: async (id: string, data: Partial<Unit> & { reasonForChange?: string }): Promise<void> => {
    await apiClient.put(`/units/${id}`, data);
  },
  bulkImport: async (file: File): Promise<{ message: string; errors?: string[] }> => {
    const formData = new FormData();
    formData.append('file', file);
    
    try {
      const res = await apiClient.post('/units/bulk-import', formData, {
        headers: {
          'Content-Type': 'multipart/form-data',
        },
      });
      return res.data;
    } catch (error: any) {
      console.error("Error en bulkImport:", error);
      if (error.response && error.response.data) {
        throw error.response.data;
      }
      throw new Error(error.message || "Ocurrió un error crítico durante la subida.");
    }
  }
};
