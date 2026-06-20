import apiClient from "./api-client";
import { Unit } from "./units-service";

export enum OwnerType {
  NaturalPerson = 1,
  LegalEntity = 2,
}

export enum DocumentType {
  CitizenshipCard = 1,
  ForeignerID = 2,
  NIT = 3,
  Passport = 4,
  PEP = 5,
  PPT = 6,
}


export interface OwnerSummary {
  id: string;
  ownerType: string;
  documentType: string;
  documentNumber: string;
  fullNameOrCompanyName: string;
  email: string;
  mainPhone: string;
  isActive: boolean;
  units: UnitOwnerSummary[];
}

export interface UnitOwnerSummary {
  assignmentId: string;
  unitId: string;
  unitIdentifier: string;
  ownerId: string;
  ownerName: string;
  ownerDocumentNumber: string;
  ownerDocumentType: string;
  ownershipPercentage: number;
  isSpokesperson: boolean;
  residesInUnit: boolean;
  startDate: string;
  endDate?: string;
}

export interface Owner {
  id: string;
  tenantId: string;
  ownerType: OwnerType;
  documentType: DocumentType;
  documentNumber: string;
  verificationDigit?: string;
  fullNameOrCompanyName: string;
  email: string;
  mainPhone: string;
  alternativePhone?: string;
  correspondenceAddress?: string;
  dateOfBirth?: string;
  civilStatus?: string;
  legalRepresentativeName?: string;
  legalRepresentativeDocumentType?: string;
  legalRepresentativeDocument?: string;
  legalRepresentativeRole?: string;
  powerOfAttorneyExpiration?: string;
  isActive: boolean;
  createdAt?: string;
  units?: UnitOwnerSummary[];
  contactHistory?: ContactHistoryEntry[];
}


export interface TenantResident {
  id: string;
  unitId: string;
  unitIdentifier: string;
  documentType: DocumentType;
  documentNumber: string;
  fullName: string;
  email: string;
  phone: string;
  leaseStartDate: string;
  leaseEndDate?: string;
  realEstateAgentName?: string;
  realEstateAgentPhone?: string;
  authorizedToPayAdmin: boolean;
  isActive: boolean;
  daysUntilLeaseExpires?: number;
  unit?: Unit;
}

export interface TenantResidentListItem {
  id: string;
  unitId: string;
  unitIdentifier: string;
  documentType: string;
  documentNumber: string;
  fullName: string;
  email: string;
  phone: string;
  leaseStartDate: string;
  leaseEndDate?: string;
  authorizedToPayAdmin: boolean;
  isActive: boolean;
  daysUntilLeaseExpires?: number;
}

export interface UpdateTenantResidentPayload {
  email: string;
  phone: string;
  leaseStartDate: string;
  leaseEndDate?: string;
  realEstateAgentName?: string;
  realEstateAgentPhone?: string;
  authorizedToPayAdmin: boolean;
}

export interface CohabitationGroupMember {
  id: string;
  unitId: string;
  fullNameOrPetName: string;
  relationship: string;
  dateOfBirth?: string;
  isMinor?: boolean;
  isPet: boolean;
  petSpecies?: string;
  petBreed?: string;
  petSanitaryRegistration?: string;
  isActive: boolean;
}

export interface OwnerHistoryEntry {
  id: string;
  ownerId: string;
  ownerName: string;
  ownerDocument: string;
  startDate: string;
  endDate?: string;
  transferNotes?: string;
  recordedAt: string;
}

export interface ContactHistoryEntry {
  id: string;
  fieldChanged: string;
  oldValue?: string;
  newValue?: string;
  changedAt: string;
  changedByUserId: string;
}

export interface UnitOccupants {
  unitId: string;
  unitIdentifier: string;
  activeOwners: UnitOwnerSummary[];
  activeTenant?: TenantResident;
  cohabitationMembers: CohabitationGroupMember[];
  spokespersonName?: string;
  spokespersonOwnerId?: string;
}

export interface TransferPropertyPayload {
  newOwnerId: string;
  transferDate: string;
  ownershipPercentage: number;
  isSpokesperson: boolean;
  residesInUnit: boolean;
  transferNotes?: string;
  generatePazYSalvo: boolean;
}

export interface TransferPropertyResult {
  message: string;
  newAssignmentId: string;
  pazYSalvo: {
    generated: boolean;
    message?: string;
    unitId?: string;
    transferDate?: string;
  };
}

export interface CreateNaturalPersonOwnerPayload {
  documentType: DocumentType;
  documentNumber: string;
  fullName: string;
  email: string;
  mainPhone: string;
  alternativePhone?: string;
  correspondenceAddress?: string;
  dateOfBirth?: string;
  civilStatus?: string;
}

export interface CreateLegalEntityOwnerPayload {
  documentNumber: string;
  verificationDigit: string;
  companyName: string;
  email: string;
  mainPhone: string;
  alternativePhone?: string;
  fiscalAddress?: string;
  legalRepresentativeName: string;
  legalRepresentativeDocumentType: DocumentType;
  legalRepresentativeDocument: string;
  legalRepresentativeRole: string;
  powerOfAttorneyExpiration?: string;
}

export interface AssignOwnerToUnitPayload {
  ownerId: string;
  ownershipPercentage: number;
  isSpokesperson: boolean;
  residesInUnit: boolean;
  startDate: string;
}

export interface CreateTenantResidentPayload {
  documentType: DocumentType;
  documentNumber: string;
  fullName: string;
  email: string;
  phone: string;
  leaseStartDate: string;
  leaseEndDate?: string;
  realEstateAgentName?: string;
  realEstateAgentPhone?: string;
  authorizedToPayAdmin: boolean;
}

export const ResidentsService = {
  // ── PROPIETARIOS ─────────────────────────────────────────────────────

  async getOwners(search?: string, includeInactive?: boolean): Promise<OwnerSummary[]> {
    const params = new URLSearchParams();
    if (search) params.append("search", search);
    if (includeInactive) params.append("includeInactive", "true");
    const response = await apiClient.get(`/residents/owners?${params.toString()}`);
    return response.data;
  },

  async getOwnerDetail(id: string): Promise<Owner> {
    const response = await apiClient.get(`/residents/owners/${id}`);
    return response.data;
  },

  async createNaturalPersonOwner(data: CreateNaturalPersonOwnerPayload): Promise<{ id: string; fullNameOrCompanyName: string }> {
    const response = await apiClient.post("/residents/owners/natural-person", data);
    return response.data;
  },

  async createLegalEntityOwner(data: CreateLegalEntityOwnerPayload): Promise<{ id: string; fullNameOrCompanyName: string }> {
    const response = await apiClient.post("/residents/owners/legal-entity", data);
    return response.data;
  },

  async deactivateOwner(id: string, exitDate: string, reason: string): Promise<void> {
    await apiClient.post(`/residents/owners/${id}/deactivate`, { exitDate, reason });
  },

  // ── VINCULACIÓN UNIDAD-PROPIETARIO ────────────────────────────────────

  async assignOwnerToUnit(unitId: string, data: AssignOwnerToUnitPayload): Promise<{ id: string }> {
    const response = await apiClient.post(`/residents/units/${unitId}/owners`, data);
    return response.data;
  },

  async designateSpokesperson(unitId: string, ownerId: string, reason?: string): Promise<void> {
    await apiClient.post(`/residents/units/${unitId}/owners/spokesperson`, { ownerId, reason });
  },

  async removeOwnerFromUnit(unitId: string, assignmentId: string, endDate: string, notes?: string): Promise<void> {
    await apiClient.post(`/residents/units/${unitId}/owners/${assignmentId}/remove`, { endDate, notes });
  },

  // ── ARRENDATARIOS ─────────────────────────────────────────────────────

  async getActiveTenant(unitId: string): Promise<TenantResident | null> {
    try {
      const response = await apiClient.get(`/residents/units/${unitId}/tenant`);
      return response.data;
    } catch {
      return null;
    }
  },

  async registerTenant(unitId: string, data: CreateTenantResidentPayload): Promise<{ id: string }> {
    const response = await apiClient.post(`/residents/units/${unitId}/tenant`, data);
    return response.data;
  },

  async deactivateTenant(unitId: string, residentId: string): Promise<void> {
    await apiClient.post(`/residents/units/${unitId}/tenant/${residentId}/deactivate`);
  },

  // ── GRUPO DE CONVIVENCIA ──────────────────────────────────────────────

  async getCohabitationMembers(unitId: string): Promise<CohabitationGroupMember[]> {
    const response = await apiClient.get(`/residents/units/${unitId}/cohabitation`);
    return response.data;
  },

  async addCohabitationMember(unitId: string, data: Partial<CohabitationGroupMember>): Promise<{ id: string }> {
    const response = await apiClient.post(`/residents/units/${unitId}/cohabitation`, data);
    return response.data;
  },

  async removeCohabitationMember(unitId: string, memberId: string): Promise<void> {
    await apiClient.post(`/residents/units/${unitId}/cohabitation/${memberId}/deactivate`);
  },

  // ── ARRENDATARIOS (GLOBAL) ───────────────────────────────────────────────

  async getTenants(search?: string, includeInactive?: boolean): Promise<TenantResidentListItem[]> {
    const params = new URLSearchParams();
    if (search) params.append("search", search);
    if (includeInactive) params.append("includeInactive", "true");
    const response = await apiClient.get(`/residents/tenants?${params.toString()}`);
    return response.data;
  },

  async getTenantDetail(id: string): Promise<TenantResident> {
    const response = await apiClient.get(`/residents/tenants/${id}`);
    return response.data;
  },

  async updateTenant(id: string, data: UpdateTenantResidentPayload): Promise<void> {
    await apiClient.put(`/residents/tenants/${id}`, data);
  },

  // ── VISTAS CONSOLIDADAS ───────────────────────────────────────────────

  async getUnitOccupants(unitId: string): Promise<UnitOccupants> {
    const response = await apiClient.get(`/residents/units/${unitId}/occupants`);
    return response.data;
  },

  async getOwnerHistory(unitId: string): Promise<OwnerHistoryEntry[]> {
    const response = await apiClient.get(`/residents/units/${unitId}/owner-history`);
    return response.data;
  },

  async getOwnerContactHistory(ownerId: string): Promise<ContactHistoryEntry[]> {
    const response = await apiClient.get(`/residents/owners/${ownerId}/contact-history`);
    return response.data;
  },

  // ── TRANSFERENCIA DE PROPIEDAD ────────────────────────────────────────

  async transferProperty(unitId: string, data: TransferPropertyPayload): Promise<TransferPropertyResult> {
    const response = await apiClient.post(`/residents/units/${unitId}/transfer`, data);
    return response.data;
  },


};
