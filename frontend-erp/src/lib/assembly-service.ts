import apiClient from './api-client';

// ── Assembly ──────────────────────────────────────────────────────

export interface AssemblyListItem {
  id: string;
  title: string;
  type: string;
  status: string;
  participationType: string;
  scheduledDate: string;
  scheduledTime: string;
  location: string;
  totalCoefficients: number;
  quorumThresholdFirstCall: number;
  quorumAchievedFirstCall: boolean;
  quorumAchievedSecondCall: boolean;
  convocationNumber: number;
  attendanceCount: number;
  agendaItemCount: number;
  approvedItemsCount: number;
  presidentName: string | null;
  secretaryName: string | null;
  createdByUserId: string;
  createdAt: string;
}

export interface AssemblyDetail {
  id: string;
  title: string;
  description: string | null;
  type: string;
  status: string;
  participationType: string;
  scheduledDate: string;
  scheduledTime: string;
  location: string;
  secondConvocationDate: string | null;
  secondConvocationTime: string | null;
  secondConvocationLocation: string | null;
  totalCoefficients: number;
  quorumThresholdFirstCall: number;
  quorumThresholdSecondCall: number;
  quorumAchievedFirstCall: boolean;
  quorumAchievedSecondCall: boolean;
  convocationNumber: number;
  sessionStartTime: string | null;
  sessionEndTime: string | null;
  presidentName: string | null;
  secretaryName: string | null;
  presidentOwnerId: string | null;
  secretaryOwnerId: string | null;
  convocationSentAt: string | null;
  convocationDeadlineMet: boolean;
  createdByUserId: string;
  createdAt: string;
  convocations: ConvocationDto[];
  agendaItems: AgendaItemDto[];
  attendances: AttendanceDto[];
  constancies: ConstancyDto[];
  minutes: MinutesDto | null;
}

export interface CreateAssemblyRequest {
  title: string;
  description?: string;
  type: string;
  participationType?: string;
  scheduledDate: string;
  scheduledTime: string;
  location: string;
  secondConvocationDate?: string;
  secondConvocationTime?: string;
  secondConvocationLocation?: string;
}

export interface UpdateAssemblyRequest {
  title?: string;
  description?: string;
  participationType?: string;
  scheduledDate?: string;
  scheduledTime?: string;
  location?: string;
  secondConvocationDate?: string;
  secondConvocationTime?: string;
  secondConvocationLocation?: string;
}

// ── Convocation ──────────────────────────────────────────────────

export interface ConvocationDto {
  id: string;
  convocationNumber: number;
  subject: string;
  notes: string | null;
  channel: string;
  sentAt: string | null;
  totalRecipients: number;
  deliveredCount: number;
  failedCount: number;
  createdAt: string;
  documents: ConvocationDocumentDto[];
  recipients: ConvocationRecipientDto[];
}

export interface ConvocationDocumentDto {
  id: string;
  documentName: string;
  documentType: string;
  filePath: string;
  description: string | null;
}

export interface ConvocationRecipientDto {
  id: string;
  unitId: string;
  unitIdentifier: string;
  ownerId: string;
  ownerName: string;
  ownerEmail: string;
  ownerPhone: string | null;
  delivered: boolean;
  deliveredAt: string | null;
  deliveryError: string | null;
}

export interface CreateConvocationRequest {
  convocationNumber: number;
  subject: string;
  notes?: string;
  channel: string;
  documents?: ConvocationDocumentInput[];
}

export interface ConvocationDocumentInput {
  documentName: string;
  documentType: string;
  filePath: string;
  description?: string;
}

// ── Attendance ───────────────────────────────────────────────────

export interface AttendanceDto {
  id: string;
  unitId: string;
  unitIdentifier: string;
  ownerId: string;
  ownerName: string;
  coefficient: number;
  status: string;
  attendsPersonally: boolean;
  representativeOwnerId: string | null;
  representativeName: string | null;
  representativeDocumentNumber: string | null;
  powerOfAttorneyFilePath: string | null;
  arrivalTime: string;
  departureTime: string | null;
  hasDuesArrears: boolean;
  votingRightRestricted: boolean;
  votingRestrictionReason: string | null;
  votingRestrictionLiftedByUserId: string | null;
  votingRestrictionLiftedReason: string | null;
  votingRestrictionLiftedAt: string | null;
  isCommissionMember: boolean;
  commissionRole: string | null;
  notes: string | null;
}

export interface RegisterAttendanceRequest {
  unitId: string;
  ownerId: string;
  attendsPersonally: boolean;
  representativeOwnerId?: string;
  representativeName?: string;
  representativeDocumentNumber?: string;
  powerOfAttorneyFilePath?: string;
  isCommissionMember?: boolean;
  commissionRole?: string;
  notes?: string;
}

export interface UnitWithOwnerInfo {
  unitId: string;
  unitIdentifier: string;
  coefficient: number;
  ownerId: string | null;
  ownerName: string | null;
  ownerEmail: string | null;
  ownerPhone: string | null;
}

// ── Agenda Item ──────────────────────────────────────────────────

export interface AgendaItemDto {
  id: string;
  sequenceNumber: number;
  title: string;
  description: string | null;
  presenterName: string | null;
  majorityRequired: string;
  votingMode: string;
  isInformationOnly: boolean;
  requiresVoting: boolean;
  totalCoefficientsForVote: number;
  votesInFavorCoefficients: number;
  votesAgainstCoefficients: number;
  abstentionCoefficients: number;
  votesInFavorCount: number;
  votesAgainstCount: number;
  abstentionCount: number;
  isApproved: boolean | null;
  rejectionReason: string | null;
  observations: string | null;
  ownerNotes: string | null;
  voteRegistered: boolean;
  registeredByUserId: string | null;
  voteRegisteredAt: string | null;
}

export interface CreateAgendaItemRequest {
  sequenceNumber: number;
  title: string;
  description?: string;
  presenterName?: string;
  majorityRequired?: string;
  votingMode?: string;
  isInformationOnly?: boolean;
  requiresVoting?: boolean;
}

export interface RegisterVoteRequest {
  votesInFavorCoefficients: number;
  votesAgainstCoefficients: number;
  abstentionCoefficients: number;
  votesInFavorCount: number;
  votesAgainstCount: number;
  abstentionCount: number;
  observations?: string;
  ownerNotes?: string;
}

// ── Constancy ────────────────────────────────────────────────────

export interface ConstancyDto {
  id: string;
  agendaItemId: string | null;
  agendaItemTitle: string | null;
  ownerId: string;
  ownerName: string;
  text: string;
  createdAt: string;
}

export interface CreateConstancyRequest {
  agendaItemId?: string;
  ownerId: string;
  ownerName: string;
  text: string;
}

// ── Minutes ──────────────────────────────────────────────────────

export interface MinutesDto {
  id: string;
  status: string;
  presidentName: string | null;
  secretaryName: string | null;
  fullText: string;
  generatedAt: string;
  commissionMemberNames: string | null;
  commissionReviewDeadline: string | null;
  commissionComments: string | null;
  presidentSignatureFilePath: string | null;
  secretarySignatureFilePath: string | null;
  approvedAt: string | null;
  publishedAt: string | null;
  publishNotificationCount: number | null;
  revisionNotes: string | null;
}

export interface GenerateMinutesRequest {
  presidentName?: string;
  secretaryName?: string;
  commissionMemberNames?: string;
}

export interface ApproveMinutesRequest {
  presidentSignatureFilePath?: string;
  secretarySignatureFilePath?: string;
  commissionComments?: string;
}

// ── Decision Propagation ─────────────────────────────────────────

export interface DecisionPropagationDto {
  id: string;
  agendaItemId: string;
  agendaItemTitle: string;
  targetModule: string;
  status: string;
  description: string;
  targetEntityId: string | null;
  targetEntityType: string | null;
  errorMessage: string | null;
  retryCount: number;
  createdAt: string;
  propagatedAt: string | null;
}

// ── Quorum ───────────────────────────────────────────────────────

export interface QuorumStatus {
  totalCoefficients: number;
  presentCoefficients: number;
  quorumThresholdFirstCall: number;
  quorumThresholdSecondCall: number;
  firstCallQuorumMet: boolean;
  secondCallQuorumMet: boolean;
  percentagePresent: number;
  totalOwners: number;
  presentOwners: number;
  absentOwners: number;
  ownersWithArrears: number;
  ownersWithRestrictedVoting: number;
}

// ── Report ───────────────────────────────────────────────────────

export interface AssemblyReport {
  totalAssemblies: number;
  ordinaryAssemblies: number;
  extraordinaryAssemblies: number;
  publishedAssemblies: number;
  pendingMinutesAssemblies: number;
  nextScheduledAssembly: string | null;
  nextAssemblyTitle: string | null;
}

// ── API Calls ────────────────────────────────────────────────────

const assemblyService = {
  async getAssemblies(status?: string, type?: string, fromDate?: string, toDate?: string, search?: string): Promise<AssemblyListItem[]> {
    const params = new URLSearchParams();
    if (status) params.append('status', status);
    if (type) params.append('type', type);
    if (fromDate) params.append('fromDate', fromDate);
    if (toDate) params.append('toDate', toDate);
    if (search) params.append('search', search);
    const query = params.toString();
    const response = await apiClient.get<AssemblyListItem[]>(`/assembly${query ? '?' + query : ''}`);
    return response.data;
  },

  async getAssemblyById(id: string): Promise<AssemblyDetail> {
    const response = await apiClient.get<AssemblyDetail>(`/assembly/${id}`);
    return response.data;
  },

  async createAssembly(request: CreateAssemblyRequest): Promise<AssemblyDetail> {
    const response = await apiClient.post<AssemblyDetail>('/assembly', request);
    return response.data;
  },

  async updateAssembly(id: string, request: UpdateAssemblyRequest): Promise<AssemblyDetail> {
    const response = await apiClient.put<AssemblyDetail>(`/assembly/${id}`, request);
    return response.data;
  },

  async deleteAssembly(id: string): Promise<void> {
    await apiClient.delete(`/assembly/${id}`);
  },

  async convocate(id: string): Promise<void> {
    await apiClient.post(`/assembly/${id}/convocate`);
  },

  async startSession(id: string, request: { convocationNumber: number; presidentName?: string; presidentOwnerId?: string; secretaryName?: string; secretaryOwnerId?: string }): Promise<void> {
    await apiClient.post(`/assembly/${id}/start-session`, request);
  },

  async endSession(id: string): Promise<void> {
    await apiClient.post(`/assembly/${id}/end-session`);
  },

  async updateSessionInfo(id: string, request: { presidentName?: string; presidentOwnerId?: string; secretaryName?: string; secretaryOwnerId?: string; convocationNumber?: number }): Promise<void> {
    await apiClient.put(`/assembly/${id}/session`, request);
  },

  // ── Convocation ──────────────────────────────────────────────

  async getConvocations(assemblyId: string): Promise<ConvocationDto[]> {
    const response = await apiClient.get<ConvocationDto[]>(`/assembly/${assemblyId}/convocations`);
    return response.data;
  },

  async createConvocation(assemblyId: string, request: CreateConvocationRequest): Promise<ConvocationDto> {
    const response = await apiClient.post<ConvocationDto>(`/assembly/${assemblyId}/convocations`, request);
    return response.data;
  },

  async sendConvocation(convocationId: string): Promise<void> {
    await apiClient.post(`/assembly/convocations/${convocationId}/send`);
  },

  // ── Attendance ───────────────────────────────────────────────

  async getAttendances(assemblyId: string): Promise<AttendanceDto[]> {
    const response = await apiClient.get<AttendanceDto[]>(`/assembly/${assemblyId}/attendances`);
    return response.data;
  },

  async registerAttendance(assemblyId: string, request: RegisterAttendanceRequest): Promise<AttendanceDto> {
    const response = await apiClient.post<AttendanceDto>(`/assembly/${assemblyId}/attendances`, request);
    return response.data;
  },

  async updateAttendance(attendanceId: string, request: { status?: string; departureTime?: string; notes?: string }): Promise<void> {
    await apiClient.put(`/assembly/attendances/${attendanceId}`, request);
  },

  async liftVotingRestriction(attendanceId: string, reason: string): Promise<void> {
    await apiClient.post(`/assembly/attendances/${attendanceId}/lift-restriction`, { reason });
  },

  async getUnitsForAttendance(): Promise<UnitWithOwnerInfo[]> {
    const response = await apiClient.get<UnitWithOwnerInfo[]>('/assembly/units-for-attendance');
    return response.data;
  },

  // ── Agenda Items ─────────────────────────────────────────────

  async getAgendaItems(assemblyId: string): Promise<AgendaItemDto[]> {
    const response = await apiClient.get<AgendaItemDto[]>(`/assembly/${assemblyId}/agenda-items`);
    return response.data;
  },

  async createAgendaItem(assemblyId: string, request: CreateAgendaItemRequest): Promise<AgendaItemDto> {
    const response = await apiClient.post<AgendaItemDto>(`/assembly/${assemblyId}/agenda-items`, request);
    return response.data;
  },

  async updateAgendaItem(itemId: string, request: Partial<CreateAgendaItemRequest>): Promise<AgendaItemDto> {
    const response = await apiClient.put<AgendaItemDto>(`/assembly/agenda-items/${itemId}`, request);
    return response.data;
  },

  async deleteAgendaItem(itemId: string): Promise<void> {
    await apiClient.delete(`/assembly/agenda-items/${itemId}`);
  },

  async registerVote(itemId: string, request: RegisterVoteRequest): Promise<AgendaItemDto> {
    const response = await apiClient.post<AgendaItemDto>(`/assembly/agenda-items/${itemId}/vote`, request);
    return response.data;
  },

  // ── Constancies ──────────────────────────────────────────────

  async getConstancies(assemblyId: string): Promise<ConstancyDto[]> {
    const response = await apiClient.get<ConstancyDto[]>(`/assembly/${assemblyId}/constancies`);
    return response.data;
  },

  async createConstancy(assemblyId: string, request: CreateConstancyRequest): Promise<ConstancyDto> {
    const response = await apiClient.post<ConstancyDto>(`/assembly/${assemblyId}/constancies`, request);
    return response.data;
  },

  // ── Minutes ──────────────────────────────────────────────────

  async generateMinutes(assemblyId: string, request: GenerateMinutesRequest): Promise<MinutesDto> {
    const response = await apiClient.post<MinutesDto>(`/assembly/${assemblyId}/minutes/generate`, request);
    return response.data;
  },

  async approveMinutes(assemblyId: string, request: ApproveMinutesRequest): Promise<MinutesDto> {
    const response = await apiClient.post<MinutesDto>(`/assembly/${assemblyId}/minutes/approve`, request);
    return response.data;
  },

  async publishMinutes(assemblyId: string): Promise<void> {
    await apiClient.post(`/assembly/${assemblyId}/minutes/publish`);
  },

  // ── Decision Propagation ─────────────────────────────────────

  async getPropagations(assemblyId: string): Promise<DecisionPropagationDto[]> {
    const response = await apiClient.get<DecisionPropagationDto[]>(`/assembly/${assemblyId}/propagations`);
    return response.data;
  },

  async createPropagation(assemblyId: string, request: { agendaItemId: string; targetModule: string; description: string }): Promise<DecisionPropagationDto> {
    const response = await apiClient.post<DecisionPropagationDto>(`/assembly/${assemblyId}/propagations`, request);
    return response.data;
  },

  // ── Quorum ───────────────────────────────────────────────────

  async getQuorumStatus(assemblyId: string): Promise<QuorumStatus> {
    const response = await apiClient.get<QuorumStatus>(`/assembly/${assemblyId}/quorum`);
    return response.data;
  },

  // ── Reports ──────────────────────────────────────────────────

  async getReport(): Promise<AssemblyReport> {
    const response = await apiClient.get<AssemblyReport>('/assembly/report');
    return response.data;
  },
};

export default assemblyService;
