import apiClient from './api-client';

// ── Reservable Space ──────────────────────────────────────────────

export interface ReservableSpaceListItem {
  id: string;
  name: string;
  description: string | null;
  location: string | null;
  maxCapacity: number;
  requiresDeposit: boolean;
  depositAmount: number;
  hasAdditionalCost: boolean;
  chargeType: string;
  hourlyRate: number;
  eventRate: number;
  approvalMode: string;
  arrearsPolicy: string;
  isActive: boolean;
  activeReservations: number;
  createdAt: string;
}

export interface SpaceSchedule {
  id: string;
  dayOfWeek: number;
  dayName: string;
  startTime: string;
  endTime: string;
  isActive: boolean;
}

export interface ReservableSpaceDetail {
  id: string;
  name: string;
  description: string | null;
  location: string | null;
  maxCapacity: number;
  minReservationHours: number;
  maxReservationHours: number;
  minAdvanceHours: number;
  maxAdvanceDays: number;
  maxSimultaneousReservationsPerUnit: number;
  requiresDeposit: boolean;
  depositAmount: number;
  hasAdditionalCost: boolean;
  chargeType: string;
  hourlyRate: number;
  eventRate: number;
  approvalMode: string;
  arrearsPolicy: string;
  isAvailableForMaintenance: boolean;
  isActive: boolean;
  rulesFilePath: string | null;
  imageFilePath: string | null;
  createdByUserId: string;
  createdAt: string;
  schedules: SpaceSchedule[];
}

export interface CreateReservableSpaceRequest {
  name: string;
  description?: string;
  location?: string;
  maxCapacity: number;
  minReservationHours?: number;
  maxReservationHours?: number;
  minAdvanceHours?: number;
  maxAdvanceDays?: number;
  maxSimultaneousReservationsPerUnit?: number;
  requiresDeposit?: boolean;
  depositAmount?: number;
  hasAdditionalCost?: boolean;
  chargeType?: string;
  hourlyRate?: number;
  eventRate?: number;
  approvalMode?: string;
  arrearsPolicy?: string;
  rulesFilePath?: string;
  imageFilePath?: string;
}

export interface UpdateReservableSpaceRequest {
  name?: string;
  description?: string;
  location?: string;
  maxCapacity?: number;
  minReservationHours?: number;
  maxReservationHours?: number;
  minAdvanceHours?: number;
  maxAdvanceDays?: number;
  maxSimultaneousReservationsPerUnit?: number;
  requiresDeposit?: boolean;
  depositAmount?: number;
  hasAdditionalCost?: boolean;
  chargeType?: string;
  hourlyRate?: number;
  eventRate?: number;
  approvalMode?: string;
  arrearsPolicy?: string;
  rulesFilePath?: string;
  imageFilePath?: string;
  isActive?: boolean;
}

export interface CreateSpaceScheduleRequest {
  dayOfWeek: number;
  startTime: string;
  endTime: string;
}

// ── Space Block ──────────────────────────────────────────────────

export interface SpaceBlock {
  id: string;
  spaceId: string;
  spaceName: string;
  startDate: string;
  endDate: string;
  startTime: string;
  endTime: string;
  origin: string;
  reason: string | null;
  relatedWorkOrderId: string | null;
  relatedWorkOrderNumber: string | null;
  notifyAffectedResidents: boolean;
  notificationSent: boolean;
  createdByUserId: string;
  createdAt: string;
}

export interface CreateSpaceBlockRequest {
  spaceId: string;
  startDate: string;
  endDate: string;
  startTime?: string;
  endTime?: string;
  origin?: string;
  reason?: string;
  relatedWorkOrderId?: string;
  notifyAffectedResidents?: boolean;
}

// ── Reservation ──────────────────────────────────────────────────

export interface ReservationListItem {
  id: string;
  reservationNumber: string;
  spaceId: string;
  spaceName: string;
  unitId: string;
  unitIdentifier: string;
  ownerId: string;
  ownerName: string;
  startDateTime: string;
  endDateTime: string;
  estimatedAttendees: number;
  eventDescription: string | null;
  status: string;
  totalCost: number;
  depositStatus: string;
  depositAmount: number;
  adminNotes: string | null;
  createdAt: string;
}

export interface ReservationDeposit {
  id: string;
  amount: number;
  status: string;
  paymentMethod: string | null;
  chargeNumber: string | null;
  returnChargeNumber: string | null;
  damageAmount: number | null;
  damageDescription: string | null;
  paidAt: string | null;
  returnedAt: string | null;
  appliedAt: string | null;
  notes: string | null;
  createdAt: string;
}

export interface ReservationIncident {
  id: string;
  description: string;
  severity: string;
  damageAmount: number;
  damageAssessed: boolean;
  depositAppliedToDamage: boolean;
  evidenceFilePath: string | null;
  reportedByName: string;
  createdAt: string;
}

export interface ReservationReminder {
  id: string;
  reminderType: string;
  status: string;
  scheduledFor: string;
  sentAt: string | null;
  channel: string;
  recipientEmail: string | null;
}

export interface ReservationDetail {
  id: string;
  reservationNumber: string;
  spaceId: string;
  spaceName: string;
  unitId: string;
  unitIdentifier: string;
  ownerId: string;
  ownerName: string;
  ownerEmail: string;
  ownerPhone: string;
  startDateTime: string;
  endDateTime: string;
  estimatedAttendees: number;
  eventDescription: string | null;
  hasMusic: boolean;
  musicEndTime: string | null;
  rulesAccepted: boolean;
  status: string;
  rejectionReason: string | null;
  totalCost: number;
  depositStatus: string;
  depositAmount: number;
  adminNotes: string | null;
  adminUserId: string | null;
  checkedInAt: string | null;
  checkedOutAt: string | null;
  checkoutSignaturePath: string | null;
  exceptionGranted: boolean;
  exceptionReason: string | null;
  createdByUserId: string;
  createdAt: string;
  deposits: ReservationDeposit[];
  incidents: ReservationIncident[];
  reminders: ReservationReminder[];
}

export interface CreateReservationRequest {
  spaceId: string;
  unitId: string;
  ownerId: string;
  startDateTime: string;
  endDateTime: string;
  estimatedAttendees: number;
  eventDescription?: string;
  hasMusic?: boolean;
  musicEndTime?: string;
  rulesAccepted: boolean;
}

export interface ApproveReservationRequest {
  adminNotes?: string;
}

export interface RejectReservationRequest {
  rejectionReason: string;
  adminNotes?: string;
}

export interface CheckInReservationRequest {
  adminNotes?: string;
}

export interface CheckOutReservationRequest {
  checkoutSignaturePath?: string;
  adminNotes?: string;
}

export interface ReportIncidentRequest {
  description: string;
  severity?: string;
  damageAmount?: number;
  evidenceFilePath?: string;
}

// ── Deposit ──────────────────────────────────────────────────────

export interface ProcessDepositPaymentRequest {
  paymentMethod?: string;
  notes?: string;
}

export interface ProcessDepositReturnRequest {
  notes?: string;
}

export interface ApplyDepositToDamageRequest {
  damageAmount: number;
  damageDescription: string;
  notes?: string;
}

// ── Availability ─────────────────────────────────────────────────

export interface AvailabilityCheck {
  isAvailable: boolean;
  reason: string | null;
  estimatedCost: number;
  depositAmount: number;
  hasArrears: boolean;
  arrearsWarning: string | null;
}

export interface AvailableSlot {
  startDateTime: string;
  endDateTime: string;
  durationHours: number;
}

export interface AlternativeSlot {
  startDateTime: string;
  endDateTime: string;
  durationHours: number;
  dayDifference: number;
}

// ── Calendar ─────────────────────────────────────────────────────

export interface CalendarEvent {
  reservationId: string;
  reservationNumber: string;
  spaceName: string;
  unitIdentifier: string;
  ownerName: string;
  startDateTime: string;
  endDateTime: string;
  status: string;
  color: string;
}

// ── Report ───────────────────────────────────────────────────────

export interface TopUnit {
  unitId: string;
  unitIdentifier: string;
  reservationCount: number;
}

export interface PeakHour {
  hour: number;
  reservationCount: number;
}

export interface ReservationReport {
  spaceId: string;
  spaceName: string;
  totalReservations: number;
  completedReservations: number;
  cancelledReservations: number;
  incidentReservations: number;
  occupancyPercentage: number;
  totalRevenue: number;
  totalDeposits: number;
  topUnits: TopUnit[];
  peakHours: PeakHour[];
}

// ── Unit ─────────────────────────────────────────────────────────

export interface UnitListItem {
  id: string;
  identifier: string;
}

export interface OwnerListItem {
  id: string;
  fullNameOrCompanyName: string;
}

const reservationService = {
  // ── Reservable Spaces ────────────────────────────────────────

  async getSpaces(isActive?: boolean): Promise<ReservableSpaceListItem[]> {
    const params = new URLSearchParams();
    if (isActive !== undefined) params.append('isActive', isActive.toString());
    const response = await apiClient.get<ReservableSpaceListItem[]>('/reservation/spaces', { params });
    return response.data;
  },

  async getSpace(id: string): Promise<ReservableSpaceDetail> {
    const response = await apiClient.get<ReservableSpaceDetail>(`/reservation/spaces/${id}`);
    return response.data;
  },

  async createSpace(request: CreateReservableSpaceRequest): Promise<ReservableSpaceDetail> {
    const response = await apiClient.post<ReservableSpaceDetail>('/reservation/spaces', request);
    return response.data;
  },

  async updateSpace(id: string, request: UpdateReservableSpaceRequest): Promise<ReservableSpaceDetail> {
    const response = await apiClient.put<ReservableSpaceDetail>(`/reservation/spaces/${id}`, request);
    return response.data;
  },

  // ── Schedules ────────────────────────────────────────────────

  async getSchedules(spaceId: string): Promise<SpaceSchedule[]> {
    const response = await apiClient.get<SpaceSchedule[]>(`/reservation/spaces/${spaceId}/schedules`);
    return response.data;
  },

  async createSchedule(spaceId: string, request: CreateSpaceScheduleRequest): Promise<SpaceSchedule> {
    const response = await apiClient.post<SpaceSchedule>(`/reservation/spaces/${spaceId}/schedules`, request);
    return response.data;
  },

  async deleteSchedule(scheduleId: string): Promise<void> {
    await apiClient.delete(`/reservation/schedules/${scheduleId}`);
  },

  // ── Space Blocks ─────────────────────────────────────────────

  async getBlocks(spaceId?: string): Promise<SpaceBlock[]> {
    const params = new URLSearchParams();
    if (spaceId) params.append('spaceId', spaceId);
    const response = await apiClient.get<SpaceBlock[]>('/reservation/blocks', { params });
    return response.data;
  },

  async createBlock(request: CreateSpaceBlockRequest): Promise<SpaceBlock> {
    const response = await apiClient.post<SpaceBlock>('/reservation/blocks', request);
    return response.data;
  },

  // ── Reservations ─────────────────────────────────────────────

  async getReservations(filters?: {
    status?: string;
    spaceId?: string;
    unitId?: string;
    fromDate?: string;
    toDate?: string;
  }): Promise<ReservationListItem[]> {
    const params = new URLSearchParams();
    if (filters?.status) params.append('status', filters.status);
    if (filters?.spaceId) params.append('spaceId', filters.spaceId);
    if (filters?.unitId) params.append('unitId', filters.unitId);
    if (filters?.fromDate) params.append('fromDate', filters.fromDate);
    if (filters?.toDate) params.append('toDate', filters.toDate);
    const response = await apiClient.get<ReservationListItem[]>('/reservation', { params });
    return response.data;
  },

  async getReservation(id: string): Promise<ReservationDetail> {
    const response = await apiClient.get<ReservationDetail>(`/reservation/${id}`);
    return response.data;
  },

  async createReservation(request: CreateReservationRequest): Promise<ReservationDetail> {
    const response = await apiClient.post<ReservationDetail>('/reservation', request);
    return response.data;
  },

  async approveReservation(id: string, request?: ApproveReservationRequest): Promise<void> {
    await apiClient.post(`/reservation/${id}/approve`, request || {});
  },

  async rejectReservation(id: string, request: RejectReservationRequest): Promise<void> {
    await apiClient.post(`/reservation/${id}/reject`, request);
  },

  async cancelReservation(id: string): Promise<void> {
    await apiClient.post(`/reservation/${id}/cancel`);
  },

  async checkIn(id: string, request?: CheckInReservationRequest): Promise<void> {
    await apiClient.post(`/reservation/${id}/check-in`, request || {});
  },

  async checkOut(id: string, request?: CheckOutReservationRequest): Promise<void> {
    await apiClient.post(`/reservation/${id}/check-out`, request || {});
  },

  async reportIncident(reservationId: string, request: ReportIncidentRequest): Promise<void> {
    await apiClient.post(`/reservation/${reservationId}/incidents`, request);
  },

  // ── Deposits ─────────────────────────────────────────────────

  async processDepositPayment(reservationId: string, request?: ProcessDepositPaymentRequest): Promise<void> {
    await apiClient.post(`/reservation/${reservationId}/deposits/pay`, request || {});
  },

  async processDepositReturn(reservationId: string, request?: ProcessDepositReturnRequest): Promise<void> {
    await apiClient.post(`/reservation/${reservationId}/deposits/return`, request || {});
  },

  async applyDepositToDamage(reservationId: string, request: ApplyDepositToDamageRequest): Promise<void> {
    await apiClient.post(`/reservation/${reservationId}/deposits/apply-damage`, request);
  },

  // ── Availability ─────────────────────────────────────────────

  async checkAvailability(spaceId: string, start: string, end: string, unitId: string): Promise<AvailabilityCheck> {
    const params = new URLSearchParams();
    params.append('spaceId', spaceId);
    params.append('start', start);
    params.append('end', end);
    params.append('unitId', unitId);
    const response = await apiClient.get<AvailabilityCheck>('/reservation/availability', { params });
    return response.data;
  },

  async getAvailableSlots(spaceId: string, date: string): Promise<AvailableSlot[]> {
    const params = new URLSearchParams();
    params.append('spaceId', spaceId);
    params.append('date', date);
    const response = await apiClient.get<AvailableSlot[]>('/reservation/availability/slots', { params });
    return response.data;
  },

  async getAlternatives(spaceId: string, start: string, end: string): Promise<AlternativeSlot[]> {
    const params = new URLSearchParams();
    params.append('spaceId', spaceId);
    params.append('start', start);
    params.append('end', end);
    const response = await apiClient.get<AlternativeSlot[]>('/reservation/availability/alternatives', { params });
    return response.data;
  },

  // ── Calendar ─────────────────────────────────────────────────

  async getCalendarEvents(spaceId: string, monthStart: string, monthEnd: string): Promise<CalendarEvent[]> {
    const params = new URLSearchParams();
    params.append('monthStart', monthStart);
    params.append('monthEnd', monthEnd);
    const response = await apiClient.get<CalendarEvent[]>(`/reservation/calendar/${spaceId}`, { params });
    return response.data;
  },

  // ── Reports ──────────────────────────────────────────────────

  async getReport(spaceId: string, fromDate: string, toDate: string): Promise<ReservationReport> {
    const params = new URLSearchParams();
    params.append('fromDate', fromDate);
    params.append('toDate', toDate);
    const response = await apiClient.get<ReservationReport>(`/reservation/reports/${spaceId}`, { params });
    return response.data;
  },
};

export default reservationService;
