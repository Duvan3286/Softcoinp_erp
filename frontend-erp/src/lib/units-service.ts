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
    const res = await apiClient.get<UnitType[]>('/api/units/types');
    return res.data;
  },
  getCoefficientSummary: async (): Promise<UnitCoefficientSummary> => {
    const res = await apiClient.get<UnitCoefficientSummary>('/api/units/coefficient-summary');
    return res.data;
  },
  getUnits: async (tower?: string, status?: string): Promise<Unit[]> => {
    const params = new URLSearchParams();
    if (tower) {
      params.append('tower', tower);
    }
    if (status) {
      params.append('status', status);
    }
    const res = await apiClient.get<Unit[]>(`/api/units?${params.toString()}`);
    return res.data;
  },
  getUnit: async (id: string): Promise<Unit> => {
    const res = await apiClient.get<Unit>(`/api/units/${id}`);
    return res.data;
  },
  createUnit: async (data: Partial<Unit>): Promise<void> => {
    await apiClient.post('/api/units', data);
  },
  updateUnit: async (id: string, data: Partial<Unit> & { reasonForChange?: string }): Promise<void> => {
    await apiClient.put(`/api/units/${id}`, data);
  },
  bulkImport: async (file: File): Promise<{ message: string; errors?: string[] }> => {
    const formData = new FormData();
    formData.append('file', file);
    
    try {
      const res = await apiClient.post('/api/units/bulk-import', formData, {
        headers: {
          'Content-Type': 'multipart/form-data',
        },
      });
      return res.data;
    } catch (error: any) {
      if (error.response && error.response.data) {
        throw error.response.data;
      }
      throw new Error("A critical error occurred during upload.");
    }
  }
};
