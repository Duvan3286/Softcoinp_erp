"use client";

import React, { useState, useEffect, useCallback } from "react";
import { useRouter, useParams } from "next/navigation";
import assemblyService, {
  AssemblyDetail,
  ConvocationDto,
  AttendanceDto,
  AgendaItemDto,
  ConstancyDto,
  MinutesDto,
  DecisionPropagationDto,
  QuorumStatus,
  UnitWithOwnerInfo,
  CreateConvocationRequest,
  RegisterAttendanceRequest,
  CreateAgendaItemRequest,
  RegisterVoteRequest,
  CreateConstancyRequest,
  GenerateMinutesRequest,
} from "@/lib/assembly-service";


type TabId =
  | "informacion"
  | "convocatoria"
  | "asistencia"
  | "orden-del-dia"
  | "constancias"
  | "acta"
  | "propagacion";

interface Tab {
  id: TabId;
  label: string;
}

const TABS: Tab[] = [
  { id: "informacion", label: "Información" },
  { id: "convocatoria", label: "Convocatoria" },
  { id: "asistencia", label: "Asistencia" },
  { id: "orden-del-dia", label: "Orden del Día" },
  { id: "constancias", label: "Constancias" },
  { id: "acta", label: "Acta" },
  { id: "propagacion", label: "Propagación" },
];

const STATUS_LABELS: Record<string, string> = {
  Draft: "Borrador",
  Convoked: "Convocada",
  InSession: "En Sesión",
  Closed: "Cerrada",
};

const STATUS_COLORS: Record<string, string> = {
  Draft: "bg-muted text-muted-foreground",
  Convoked: "bg-blue-100 dark:bg-blue-950/30 text-blue-700 dark:text-blue-400",
  InSession: "bg-emerald-100 text-emerald-700",
  Closed: "bg-rose-100 dark:bg-rose-950/30 text-rose-700 dark:text-rose-400",
};

const ATTENDANCE_STATUS_LABELS: Record<string, string> = {
  Present: "Presente",
  Absent: "Ausente",
  Late: "Tardanza",
  Justified: "Justificado",
};

const MAJORITY_LABELS: Record<string, string> = {
  Simple: "Mayoría Simple",
  Qualified: "Mayoría Calificada",
  Unanimous: "Unanimidad",
};

const VOTING_MODE_LABELS: Record<string, string> = {
  ShowOfHands: "Aplaumanos",
  Written: "Escrito",
  Electronic: "Electrónico",
  Coefficient: "Por Coeficiente",
};

const PROPAGATION_STATUS_LABELS: Record<string, string> = {
  Pending: "Pendiente",
  Processing: "Procesando",
  Completed: "Completado",
  Failed: "Fallido",
};

const PROPAGATION_STATUS_COLORS: Record<string, string> = {
  Pending: "bg-yellow-100 text-yellow-700",
  Processing: "bg-blue-100 dark:bg-blue-950/30 text-blue-700 dark:text-blue-400",
  Completed: "bg-emerald-100 text-emerald-700",
  Failed: "bg-rose-100 dark:bg-rose-950/30 text-rose-700 dark:text-rose-400",
};

const MINUTES_STATUS_LABELS: Record<string, string> = {
  Draft: "Borrador",
  Generated: "Generada",
  UnderReview: "En Revisión",
  Approved: "Aprobada",
  Published: "Publicada",
};

export default function AssemblyDetailPage() {
  const router = useRouter();
  const params = useParams();
  const assemblyId = params.id as string;

  const [assembly, setAssembly] = useState<AssemblyDetail | null>(null);
  const [loading, setLoading] = useState(true);
  const [activeTab, setActiveTab] = useState<TabId>("informacion");
  const [actionLoading, setActionLoading] = useState(false);

  // Convocatoria state
  const [convocationSubject, setConvocationSubject] = useState("");
  const [convocationNotes, setConvocationNotes] = useState("");
  const [convocationChannel, setConvocationChannel] = useState("Email");
  const [convocationNumber, setConvocationNumber] = useState(1);

  // Asistencia state
  const [quorumStatus, setQuorumStatus] = useState<QuorumStatus | null>(null);
  const [units, setUnits] = useState<UnitWithOwnerInfo[]>([]);
  const [selectedUnitId, setSelectedUnitId] = useState("");
  const [selectedOwnerId, setSelectedOwnerId] = useState("");
  const [attendsPersonally, setAttendsPersonally] = useState(true);
  const [representativeName, setRepresentativeName] = useState("");
  const [representativeDocNumber, setRepresentativeDocNumber] = useState("");
  const [attendanceNotes, setAttendanceNotes] = useState("");

  // Orden del día state
  const [agendaTitle, setAgendaTitle] = useState("");
  const [agendaDescription, setAgendaDescription] = useState("");
  const [agendaPresenter, setAgendaPresenter] = useState("");
  const [agendaMajority, setAgendaMajority] = useState("Simple");
  const [agendaVotingMode, setAgendaVotingMode] = useState("Coefficient");
  const [agendaIsInformationOnly, setAgendaIsInformationOnly] = useState(false);

  // Vote registration state
  const [voteItemId, setVoteItemId] = useState<string | null>(null);
  const [votesInFavorCoeff, setVotesInFavorCoeff] = useState(0);
  const [votesAgainstCoeff, setVotesAgainstCoeff] = useState(0);
  const [abstentionCoeff, setAbstentionCoeff] = useState(0);
  const [votesInFavorCount, setVotesInFavorCount] = useState(0);
  const [votesAgainstCount, setVotesAgainstCount] = useState(0);
  const [abstentionCount, setAbstentionCount] = useState(0);
  const [voteObservations, setVoteObservations] = useState("");

  // Constancias state
  const [constancyOwnerId, setConstancyOwnerId] = useState("");
  const [constancyOwnerName, setConstancyOwnerName] = useState("");
  const [constancyText, setConstancyText] = useState("");
  const [constancyAgendaItemId, setConstancyAgendaItemId] = useState("");

  // Acta state
  const [minutesPresident, setMinutesPresident] = useState("");
  const [minutesSecretary, setMinutesSecretary] = useState("");
  const [minutesCommissionMembers, setMinutesCommissionMembers] = useState("");

  const fetchAssembly = useCallback(async () => {
    try {
      setLoading(true);
      const data = await assemblyService.getAssemblyById(assemblyId);
      setAssembly(data);
    } catch (error) {
      console.error("Error al cargar asamblea:", error);
    } finally {
      setLoading(false);
    }
  }, [assemblyId]);

  const fetchQuorum = useCallback(async () => {
    try {
      const data = await assemblyService.getQuorumStatus(assemblyId);
      setQuorumStatus(data);
    } catch (error) {
      console.error("Error al cargar quórum:", error);
    }
  }, [assemblyId]);

  const fetchUnits = useCallback(async () => {
    try {
      const data = await assemblyService.getUnitsForAttendance();
      setUnits(data);
    } catch (error) {
      console.error("Error al cargar unidades:", error);
    }
  }, []);

  useEffect(() => {
    fetchAssembly();
  }, [fetchAssembly]);

  useEffect(() => {
    if (activeTab === "asistencia") {
      fetchQuorum();
      fetchUnits();
    }
  }, [activeTab, fetchQuorum, fetchUnits]);

  const handleConvocate = async () => {
    if (!assembly) return;
    try {
      setActionLoading(true);
      await assemblyService.convocate(assembly.id);
      await fetchAssembly();
    } catch (error) {
      console.error("Error al convocar:", error);
    } finally {
      setActionLoading(false);
    }
  };

  const handleStartSession = async () => {
    if (!assembly) return;
    try {
      setActionLoading(true);
      await assemblyService.startSession(assembly.id, {
        convocationNumber: assembly.convocationNumber,
        presidentName: assembly.presidentName || undefined,
        secretaryName: assembly.secretaryName || undefined,
      });
      await fetchAssembly();
    } catch (error) {
      console.error("Error al iniciar sesión:", error);
    } finally {
      setActionLoading(false);
    }
  };

  const handleEndSession = async () => {
    if (!assembly) return;
    try {
      setActionLoading(true);
      await assemblyService.endSession(assembly.id);
      await fetchAssembly();
    } catch (error) {
      console.error("Error al cerrar sesión:", error);
    } finally {
      setActionLoading(false);
    }
  };

  const handleCreateConvocation = async () => {
    if (!assembly) return;
    try {
      setActionLoading(true);
      const request: CreateConvocationRequest = {
        convocationNumber,
        subject: convocationSubject,
        notes: convocationNotes || undefined,
        channel: convocationChannel,
      };
      await assemblyService.createConvocation(assembly.id, request);
      setConvocationSubject("");
      setConvocationNotes("");
      await fetchAssembly();
    } catch (error) {
      console.error("Error al crear convocatoria:", error);
    } finally {
      setActionLoading(false);
    }
  };

  const handleRegisterAttendance = async () => {
    if (!assembly || !selectedUnitId || !selectedOwnerId) return;
    try {
      setActionLoading(true);
      const request: RegisterAttendanceRequest = {
        unitId: selectedUnitId,
        ownerId: selectedOwnerId,
        attendsPersonally,
        representativeName: representativeName || undefined,
        representativeDocumentNumber: representativeDocNumber || undefined,
        notes: attendanceNotes || undefined,
      };
      await assemblyService.registerAttendance(assembly.id, request);
      setSelectedUnitId("");
      setSelectedOwnerId("");
      setRepresentativeName("");
      setRepresentativeDocNumber("");
      setAttendanceNotes("");
      await fetchQuorum();
      await fetchAssembly();
    } catch (error) {
      console.error("Error al registrar asistencia:", error);
    } finally {
      setActionLoading(false);
    }
  };

  const handleCreateAgendaItem = async () => {
    if (!assembly) return;
    try {
      setActionLoading(true);
      const request: CreateAgendaItemRequest = {
        sequenceNumber: (assembly.agendaItems?.length || 0) + 1,
        title: agendaTitle,
        description: agendaDescription || undefined,
        presenterName: agendaPresenter || undefined,
        majorityRequired: agendaMajority,
        votingMode: agendaVotingMode,
        isInformationOnly: agendaIsInformationOnly,
        requiresVoting: !agendaIsInformationOnly,
      };
      await assemblyService.createAgendaItem(assembly.id, request);
      setAgendaTitle("");
      setAgendaDescription("");
      setAgendaPresenter("");
      await fetchAssembly();
    } catch (error) {
      console.error("Error al crear punto de agenda:", error);
    } finally {
      setActionLoading(false);
    }
  };

  const handleRegisterVote = async () => {
    if (!voteItemId) return;
    try {
      setActionLoading(true);
      const request: RegisterVoteRequest = {
        votesInFavorCoefficients: votesInFavorCoeff,
        votesAgainstCoefficients: votesAgainstCoeff,
        abstentionCoefficients: abstentionCoeff,
        votesInFavorCount,
        votesAgainstCount,
        abstentionCount,
        observations: voteObservations || undefined,
      };
      await assemblyService.registerVote(voteItemId, request);
      setVoteItemId(null);
      setVotesInFavorCoeff(0);
      setVotesAgainstCoeff(0);
      setAbstentionCoeff(0);
      setVotesInFavorCount(0);
      setVotesAgainstCount(0);
      setAbstentionCount(0);
      setVoteObservations("");
      await fetchAssembly();
    } catch (error) {
      console.error("Error al registrar voto:", error);
    } finally {
      setActionLoading(false);
    }
  };

  const handleCreateConstancy = async () => {
    if (!assembly) return;
    try {
      setActionLoading(true);
      const request: CreateConstancyRequest = {
        agendaItemId: constancyAgendaItemId || undefined,
        ownerId: constancyOwnerId,
        ownerName: constancyOwnerName,
        text: constancyText,
      };
      await assemblyService.createConstancy(assembly.id, request);
      setConstancyOwnerId("");
      setConstancyOwnerName("");
      setConstancyText("");
      setConstancyAgendaItemId("");
      await fetchAssembly();
    } catch (error) {
      console.error("Error al crear constancia:", error);
    } finally {
      setActionLoading(false);
    }
  };

  const handleGenerateMinutes = async () => {
    if (!assembly) return;
    try {
      setActionLoading(true);
      const request: GenerateMinutesRequest = {
        presidentName: minutesPresident || undefined,
        secretaryName: minutesSecretary || undefined,
        commissionMemberNames: minutesCommissionMembers || undefined,
      };
      await assemblyService.generateMinutes(assembly.id, request);
      await fetchAssembly();
    } catch (error) {
      console.error("Error al generar acta:", error);
    } finally {
      setActionLoading(false);
    }
  };

  const handleApproveMinutes = async () => {
    if (!assembly) return;
    try {
      setActionLoading(true);
      await assemblyService.approveMinutes(assembly.id, {});
      await fetchAssembly();
    } catch (error) {
      console.error("Error al aprobar acta:", error);
    } finally {
      setActionLoading(false);
    }
  };

  const handlePublishMinutes = async () => {
    if (!assembly) return;
    try {
      setActionLoading(true);
      await assemblyService.publishMinutes(assembly.id);
      await fetchAssembly();
    } catch (error) {
      console.error("Error al publicar acta:", error);
    } finally {
      setActionLoading(false);
    }
  };

  const formatDate = (dateStr: string) => {
    try {
      return new Date(dateStr).toLocaleDateString('es-CO', { day: '2-digit', month: 'long', year: 'numeric' });
    } catch {
      return dateStr;
    }
  };

  const formatTime = (timeStr: string) => {
    return timeStr;
  };

  const formatDateTime = (dateStr: string) => {
    try {
      return new Date(dateStr).toLocaleString('es-CO', { day: '2-digit', month: '2-digit', year: 'numeric', hour: '2-digit', minute: '2-digit' });
    } catch {
      return dateStr;
    }
  };

  if (loading) {
    return (
      <main className="flex items-center justify-center">
        <div className="text-center">
          <div className="animate-spin rounded-full h-12 w-12 border-b-2 border-emerald-600 mx-auto mb-4"></div>
          <p className="text-muted-foreground text-sm font-medium">Cargando asamblea...</p>
        </div>
      </main>
    );
  }

  if (!assembly) {
    return (
      <main className="flex items-center justify-center">
        <div className="text-center">
          <p className="text-muted-foreground text-lg">Asamblea no encontrada</p>
          <button
            onClick={() => router.push("/assembly")}
            className="mt-4 text-emerald-600 hover:text-emerald-700 font-medium"
          >
            Volver a la lista
          </button>
        </div>
      </main>
    );
  }

  const quorumPercentage = quorumStatus?.percentagePresent || 0;
  const quorumMet = assembly.status === "Convoked"
    ? assembly.quorumAchievedFirstCall
    : assembly.quorumAchievedSecondCall;

  const selectedUnit = units.find((u) => u.unitId === selectedUnitId);

  return (
    <main>
      <div className="max-w-7xl mx-auto px-6 py-8">
          {/* Header */}
          <div className="mb-8">
            <div className="flex items-center gap-3 mb-2">
              <button
                onClick={() => router.push("/assembly")}
                className="text-muted-foreground hover:text-muted-foreground"
              >
                ← Volver
              </button>
            </div>
            <div className="flex items-start justify-between">
              <div>
                <h1 className="text-2xl font-bold text-foreground">{assembly.title}</h1>
                <p className="text-sm text-muted-foreground mt-1">{assembly.description}</p>
              </div>
              <span
                className={`px-3 py-1 rounded-full text-xs font-semibold ${STATUS_COLORS[assembly.status] || "bg-muted text-muted-foreground"}`}
              >
                {STATUS_LABELS[assembly.status] || assembly.status}
              </span>
            </div>

            {/* Action Buttons */}
            <div className="flex gap-3 mt-4">
              {assembly.status === "Draft" && (
                <button
                  onClick={handleConvocate}
                  disabled={actionLoading}
                  className="px-4 py-2 bg-emerald-600 text-white text-sm font-medium rounded-lg hover:bg-emerald-700 disabled:opacity-50"
                >
                  {actionLoading ? "Procesando..." : "Convocar"}
                </button>
              )}
              {assembly.status === "Convoked" && (
                <button
                  onClick={handleStartSession}
                  disabled={actionLoading}
                  className="px-4 py-2 bg-emerald-600 text-white text-sm font-medium rounded-lg hover:bg-emerald-700 disabled:opacity-50"
                >
                  {actionLoading ? "Procesando..." : "Iniciar Sesión"}
                </button>
              )}
              {assembly.status === "InSession" && (
                <button
                  onClick={handleEndSession}
                  disabled={actionLoading}
                  className="px-4 py-2 bg-emerald-600 text-white text-sm font-medium rounded-lg hover:bg-emerald-700 disabled:opacity-50"
                >
                  {actionLoading ? "Procesando..." : "Cerrar Sesión"}
                </button>
              )}
              {assembly.status === "Closed" && (
                <button
                  onClick={() => setActiveTab("acta")}
                  className="px-4 py-2 bg-emerald-600 text-white text-sm font-medium rounded-lg hover:bg-emerald-700"
                >
                  Generar Acta
                </button>
              )}
            </div>
          </div>

          {/* Tabs */}
          <div className="border-b border-border mb-6">
            <nav className="flex gap-1 -mb-px">
              {TABS.map((tab) => (
                <button
                  key={tab.id}
                  onClick={() => setActiveTab(tab.id)}
                  className={`px-4 py-3 text-sm font-medium border-b-2 transition-colors ${
                    activeTab === tab.id
                      ? "border-emerald-600 text-emerald-600"
                      : "border-transparent text-muted-foreground hover:text-muted-foreground hover:border-border"
                  }`}
                >
                  {tab.label}
                </button>
              ))}
            </nav>
          </div>

          {/* Tab Content */}
          <div className="bg-card rounded-xl shadow-sm border border-border p-6">
            {/* ════════════════════════════════════════════════════════════ */}
            {/* TAB: Información */}
            {/* ════════════════════════════════════════════════════════════ */}
            {activeTab === "informacion" && (
              <div className="space-y-6">
                <h2 className="text-lg font-semibold text-foreground">Información de la Asamblea</h2>
                <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
                  <div>
                    <label className="block text-xs font-medium text-muted-foreground uppercase tracking-wider mb-1">
                      Tipo
                    </label>
                    <p className="text-sm text-foreground">{assembly.type}</p>
                  </div>
                  <div>
                    <label className="block text-xs font-medium text-muted-foreground uppercase tracking-wider mb-1">
                      Tipo de Participación
                    </label>
                    <p className="text-sm text-foreground">{assembly.participationType}</p>
                  </div>
                  <div>
                    <label className="block text-xs font-medium text-muted-foreground uppercase tracking-wider mb-1">
                      Fecha Programada
                    </label>
                    <p className="text-sm text-foreground">{formatDate(assembly.scheduledDate)}</p>
                  </div>
                  <div>
                    <label className="block text-xs font-medium text-muted-foreground uppercase tracking-wider mb-1">
                      Hora Programada
                    </label>
                    <p className="text-sm text-foreground">{formatTime(assembly.scheduledTime)}</p>
                  </div>
                  <div>
                    <label className="block text-xs font-medium text-muted-foreground uppercase tracking-wider mb-1">
                      Lugar
                    </label>
                    <p className="text-sm text-foreground">{assembly.location}</p>
                  </div>
                  <div>
                    <label className="block text-xs font-medium text-muted-foreground uppercase tracking-wider mb-1">
                      Coeficiente Total
                    </label>
                    <p className="text-sm text-foreground">{assembly.totalCoefficients}</p>
                  </div>
                  <div>
                    <label className="block text-xs font-medium text-muted-foreground uppercase tracking-wider mb-1">
                      Umbral Quórum Primera Convocatoria
                    </label>
                    <p className="text-sm text-foreground">{assembly.quorumThresholdFirstCall}%</p>
                  </div>
                  <div>
                    <label className="block text-xs font-medium text-muted-foreground uppercase tracking-wider mb-1">
                      Umbral Quórum Segunda Convocatoria
                    </label>
                    <p className="text-sm text-foreground">{assembly.quorumThresholdSecondCall}%</p>
                  </div>
                  <div>
                    <label className="block text-xs font-medium text-muted-foreground uppercase tracking-wider mb-1">
                      Presidente
                    </label>
                    <p className="text-sm text-foreground">{assembly.presidentName || "—"}</p>
                  </div>
                  <div>
                    <label className="block text-xs font-medium text-muted-foreground uppercase tracking-wider mb-1">
                      Secretario
                    </label>
                    <p className="text-sm text-foreground">{assembly.secretaryName || "—"}</p>
                  </div>
                  {assembly.sessionStartTime && (
                    <div>
                      <label className="block text-xs font-medium text-muted-foreground uppercase tracking-wider mb-1">
                        Inicio de Sesión
                      </label>
                      <p className="text-sm text-foreground">
                        {formatDateTime(assembly.sessionStartTime)}
                      </p>
                    </div>
                  )}
                  {assembly.sessionEndTime && (
                    <div>
                      <label className="block text-xs font-medium text-muted-foreground uppercase tracking-wider mb-1">
                        Fin de Sesión
                      </label>
                      <p className="text-sm text-foreground">
                        {formatDateTime(assembly.sessionEndTime)}
                      </p>
                    </div>
                  )}
                </div>
              </div>
            )}

            {/* ════════════════════════════════════════════════════════════ */}
            {/* TAB: Convocatoria */}
            {/* ════════════════════════════════════════════════════════════ */}
            {activeTab === "convocatoria" && (
              <div className="space-y-6">
                <h2 className="text-lg font-semibold text-foreground">Convocatorias</h2>

                {/* Convocation List */}
                {assembly.convocations && assembly.convocations.length > 0 && (
                  <div className="space-y-4">
                    {assembly.convocations.map((conv) => (
                      <div
                        key={conv.id}
                        className="border border-border rounded-lg p-4"
                      >
                        <div className="flex items-start justify-between">
                          <div>
                            <h3 className="text-sm font-semibold text-foreground">
                              Convocatoria N.° {conv.convocationNumber}
                            </h3>
                            <p className="text-sm text-muted-foreground mt-1">{conv.subject}</p>
                            {conv.notes && (
                              <p className="text-xs text-muted-foreground mt-1">{conv.notes}</p>
                            )}
                          </div>
                          <div className="text-right">
                            <span className="text-xs text-muted-foreground">
                              {conv.sentAt ? formatDateTime(conv.sentAt) : "No enviada"}
                            </span>
                            <p className="text-xs text-muted-foreground mt-1">
                              Canal: {conv.channel}
                            </p>
                          </div>
                        </div>
                        <div className="mt-3 flex gap-4 text-xs text-muted-foreground">
                          <span>Destinatarios: {conv.totalRecipients}</span>
                          <span>Entregados: {conv.deliveredCount}</span>
                          <span>Fallidos: {conv.failedCount}</span>
                        </div>
                        {conv.recipients && conv.recipients.length > 0 && (
                          <div className="mt-3">
                            <p className="text-xs font-medium text-muted-foreground mb-1">Destinatarios:</p>
                            <div className="max-h-32 overflow-y-auto">
                              {conv.recipients.map((rec) => (
                                <div
                                  key={rec.id}
                                  className="flex items-center gap-2 text-xs text-muted-foreground py-1"
                                >
                                  <span
                                    className={`w-2 h-2 rounded-full ${
                                      rec.delivered ? "bg-emerald-500" : "bg-red-500"
                                    }`}
                                  ></span>
                                  <span>{rec.ownerName}</span>
                                  <span className="text-muted-foreground">({rec.unitIdentifier})</span>
                                </div>
                              ))}
                            </div>
                          </div>
                        )}
                      </div>
                    ))}
                  </div>
                )}

                {/* New Convocation Form */}
                <div className="border-t border-border pt-6">
                  <h3 className="text-sm font-semibold text-foreground mb-4">Nueva Convocatoria</h3>
                  <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
                    <div>
                      <label className="block text-xs font-medium text-muted-foreground mb-1">
                        Número de Convocatoria
                      </label>
                      <input
                        type="number"
                        value={convocationNumber}
                        onChange={(e) => setConvocationNumber(Number(e.target.value))}
                        className="border-b border-emerald-600/30 focus:border-emerald-600 text-sm font-medium py-2 outline-none w-full"
                      />
                    </div>
                    <div>
                      <label className="block text-xs font-medium text-muted-foreground mb-1">Canal</label>
                      <select
                        value={convocationChannel}
                        onChange={(e) => setConvocationChannel(e.target.value)}
                        className="border-b border-emerald-600/30 focus:border-emerald-600 text-sm font-medium py-2 outline-none w-full"
                      >
                        <option value="Email">Email</option>
                        <option value="WhatsApp">WhatsApp</option>
                        <option value="SMS">SMS</option>
                      </select>
                    </div>
                    <div className="md:col-span-2">
                      <label className="block text-xs font-medium text-muted-foreground mb-1">Asunto</label>
                      <input
                        type="text"
                        value={convocationSubject}
                        onChange={(e) => setConvocationSubject(e.target.value)}
                        className="border-b border-emerald-600/30 focus:border-emerald-600 text-sm font-medium py-2 outline-none w-full"
                        placeholder="Ingrese el asunto de la convocatoria"
                      />
                    </div>
                    <div className="md:col-span-2">
                      <label className="block text-xs font-medium text-muted-foreground mb-1">
                        Notas Adicionales
                      </label>
                      <textarea
                        value={convocationNotes}
                        onChange={(e) => setConvocationNotes(e.target.value)}
                        className="border-b border-emerald-600/30 focus:border-emerald-600 text-sm font-medium py-2 outline-none w-full resize-none"
                        rows={3}
                        placeholder="Notas opcionales"
                      ></textarea>
                    </div>
                  </div>
                  <button
                    onClick={handleCreateConvocation}
                    disabled={actionLoading || !convocationSubject}
                    className="mt-4 px-4 py-2 bg-emerald-600 text-white text-sm font-medium rounded-lg hover:bg-emerald-700 disabled:opacity-50"
                  >
                    {actionLoading ? "Creando..." : "Crear Convocatoria"}
                  </button>
                </div>
              </div>
            )}

            {/* ════════════════════════════════════════════════════════════ */}
            {/* TAB: Asistencia */}
            {/* ════════════════════════════════════════════════════════════ */}
            {activeTab === "asistencia" && (
              <div className="space-y-6">
                <h2 className="text-lg font-semibold text-foreground">Control de Asistencia</h2>

                {/* Quorum Indicator */}
                {quorumStatus && (
                  <div className="bg-muted/50 rounded-lg p-6 border border-border">
                    <h3 className="text-sm font-semibold text-foreground mb-4">Estado del Quórum</h3>
                    <div className="mb-4">
                      <div className="flex items-center justify-between mb-2">
                        <span className="text-sm text-muted-foreground">
                          Quórum: {quorumPercentage.toFixed(1)}% ({quorumStatus.presentCoefficients} coeficientes de{" "}
                          {quorumStatus.totalCoefficients} totales)
                        </span>
                        <span
                          className={`text-sm font-semibold ${
                            quorumMet ? "text-emerald-600" : "text-rose-600 dark:text-rose-400"
                          }`}
                        >
                          {quorumMet ? "Quórum Logrado" : "Quórum No Logrado"}
                        </span>
                      </div>
                      <div className="w-full bg-muted rounded-full h-3">
                        <div
                          className={`h-3 rounded-full transition-all ${
                            quorumMet ? "bg-emerald-600" : "bg-red-500"
                          }`}
                          style={{ width: `${Math.min(quorumPercentage, 100)}%` }}
                        ></div>
                      </div>
                    </div>
                    <div className="grid grid-cols-2 md:grid-cols-4 gap-4 text-xs">
                      <div>
                        <p className="text-muted-foreground">1ra Convocatoria</p>
                        <p
                          className={`font-semibold ${
                            quorumStatus.firstCallQuorumMet ? "text-emerald-600" : "text-rose-600 dark:text-rose-400"
                          }`}
                        >
                          {quorumStatus.quorumThresholdFirstCall}%
                          {quorumStatus.firstCallQuorumMet ? " ✓" : " ✗"}
                        </p>
                      </div>
                      <div>
                        <p className="text-muted-foreground">2da Convocatoria</p>
                        <p
                          className={`font-semibold ${
                            quorumStatus.secondCallQuorumMet ? "text-emerald-600" : "text-rose-600 dark:text-rose-400"
                          }`}
                        >
                          {quorumStatus.quorumThresholdSecondCall}%
                          {quorumStatus.secondCallQuorumMet ? " ✓" : " ✗"}
                        </p>
                      </div>
                      <div>
                        <p className="text-muted-foreground">Propietarios Presentes</p>
                        <p className="font-semibold text-foreground">
                          {quorumStatus.presentOwners} / {quorumStatus.totalOwners}
                        </p>
                      </div>
                      <div>
                        <p className="text-muted-foreground">Ausentes</p>
                        <p className="font-semibold text-foreground">{quorumStatus.absentOwners}</p>
                      </div>
                    </div>
                  </div>
                )}

                {/* Attendance List */}
                {assembly.attendances && assembly.attendances.length > 0 && (
                  <div>
                    <h3 className="text-sm font-semibold text-foreground mb-3">
                      Lista de Asistencia ({assembly.attendances.length})
                    </h3>
                    <div className="overflow-x-auto">
                      <table className="w-full text-sm">
                        <thead>
                          <tr className="border-b border-border">
                            <th className="text-left py-2 text-xs font-medium text-muted-foreground uppercase">
                              Unidad
                            </th>
                            <th className="text-left py-2 text-xs font-medium text-muted-foreground uppercase">
                              Propietario
                            </th>
                            <th className="text-left py-2 text-xs font-medium text-muted-foreground uppercase">
                              Coeficiente
                            </th>
                            <th className="text-left py-2 text-xs font-medium text-muted-foreground uppercase">
                              Estado
                            </th>
                            <th className="text-left py-2 text-xs font-medium text-muted-foreground uppercase">
                              Hora Llegada
                            </th>
                          </tr>
                        </thead>
                        <tbody>
                          {assembly.attendances.map((att) => (
                            <tr key={att.id} className="border-b border-border">
                              <td className="py-2 text-foreground">{att.unitIdentifier}</td>
                              <td className="py-2 text-foreground">{att.ownerName}</td>
                              <td className="py-2 text-foreground">{att.coefficient}</td>
                              <td className="py-2">
                                <span
                                  className={`px-2 py-0.5 rounded text-xs font-medium ${
                                    att.status === "Present"
                                      ? "bg-emerald-100 text-emerald-700"
                                      : att.status === "Absent"
                                        ? "bg-rose-100 dark:bg-rose-950/30 text-rose-700 dark:text-rose-400"
                                        : "bg-yellow-100 text-yellow-700"
                                  }`}
                                >
                                  {ATTENDANCE_STATUS_LABELS[att.status] || att.status}
                                </span>
                              </td>
                              <td className="py-2 text-muted-foreground">
                                {formatDateTime(att.arrivalTime)}
                              </td>
                            </tr>
                          ))}
                        </tbody>
                      </table>
                    </div>
                  </div>
                )}

                {/* Register Attendance Form */}
                <div className="border-t border-border pt-6">
                  <h3 className="text-sm font-semibold text-foreground mb-4">Registrar Asistencia</h3>
                  <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
                    <div>
                      <label className="block text-xs font-medium text-muted-foreground mb-1">Unidad</label>
                      <select
                        value={selectedUnitId}
                        onChange={(e) => {
                          setSelectedUnitId(e.target.value);
                          const unit = units.find((u) => u.unitId === e.target.value);
                          if (unit) {
                            setSelectedOwnerId(unit.ownerId || "");
                          }
                        }}
                        className="border-b border-emerald-600/30 focus:border-emerald-600 text-sm font-medium py-2 outline-none w-full"
                      >
                        <option value="">Seleccionar unidad...</option>
                        {units.map((unit) => (
                          <option key={unit.unitId} value={unit.unitId}>
                            {unit.unitIdentifier} - {unit.ownerName || "Sin propietario"} ({unit.coefficient})
                          </option>
                        ))}
                      </select>
                    </div>
                    <div>
                      <label className="block text-xs font-medium text-muted-foreground mb-1">
                        Asiste Personalmente
                      </label>
                      <select
                        value={attendsPersonally ? "true" : "false"}
                        onChange={(e) => setAttendsPersonally(e.target.value === "true")}
                        className="border-b border-emerald-600/30 focus:border-emerald-600 text-sm font-medium py-2 outline-none w-full"
                      >
                        <option value="true">Sí</option>
                        <option value="false">No (Representante)</option>
                      </select>
                    </div>
                    {!attendsPersonally && (
                      <>
                        <div>
                          <label className="block text-xs font-medium text-muted-foreground mb-1">
                            Nombre del Representante
                          </label>
                          <input
                            type="text"
                            value={representativeName}
                            onChange={(e) => setRepresentativeName(e.target.value)}
                            className="border-b border-emerald-600/30 focus:border-emerald-600 text-sm font-medium py-2 outline-none w-full"
                            placeholder="Nombre completo"
                          />
                        </div>
                        <div>
                          <label className="block text-xs font-medium text-muted-foreground mb-1">
                            Documento del Representante
                          </label>
                          <input
                            type="text"
                            value={representativeDocNumber}
                            onChange={(e) => setRepresentativeDocNumber(e.target.value)}
                            className="border-b border-emerald-600/30 focus:border-emerald-600 text-sm font-medium py-2 outline-none w-full"
                            placeholder="Número de documento"
                          />
                        </div>
                      </>
                    )}
                    <div className="md:col-span-2">
                      <label className="block text-xs font-medium text-muted-foreground mb-1">Notas</label>
                      <input
                        type="text"
                        value={attendanceNotes}
                        onChange={(e) => setAttendanceNotes(e.target.value)}
                        className="border-b border-emerald-600/30 focus:border-emerald-600 text-sm font-medium py-2 outline-none w-full"
                        placeholder="Notas opcionales"
                      />
                    </div>
                  </div>
                  <button
                    onClick={handleRegisterAttendance}
                    disabled={actionLoading || !selectedUnitId || !selectedOwnerId}
                    className="mt-4 px-4 py-2 bg-emerald-600 text-white text-sm font-medium rounded-lg hover:bg-emerald-700 disabled:opacity-50"
                  >
                    {actionLoading ? "Registrando..." : "Registrar Asistencia"}
                  </button>
                </div>
              </div>
            )}

            {/* ════════════════════════════════════════════════════════════ */}
            {/* TAB: Orden del Día */}
            {/* ════════════════════════════════════════════════════════════ */}
            {activeTab === "orden-del-dia" && (
              <div className="space-y-6">
                <h2 className="text-lg font-semibold text-foreground">Orden del Día</h2>

                {/* Agenda Items List */}
                {assembly.agendaItems && assembly.agendaItems.length > 0 && (
                  <div className="space-y-4">
                    {assembly.agendaItems.map((item) => (
                      <div
                        key={item.id}
                        className="border border-border rounded-lg p-4"
                      >
                        <div className="flex items-start justify-between">
                          <div className="flex-1">
                            <div className="flex items-center gap-2">
                              <span className="text-xs font-bold text-emerald-600 bg-emerald-50 px-2 py-0.5 rounded">
                                Punto {item.sequenceNumber}
                              </span>
                              {item.isInformationOnly && (
                                <span className="text-xs text-muted-foreground bg-muted px-2 py-0.5 rounded">
                                  Informativo
                                </span>
                              )}
                              {item.voteRegistered && (
                                <span
                                  className={`text-xs px-2 py-0.5 rounded ${
                                    item.isApproved
                                      ? "bg-emerald-100 text-emerald-700"
                                      : "bg-rose-100 dark:bg-rose-950/30 text-rose-700 dark:text-rose-400"
                                  }`}
                                >
                                  {item.isApproved ? "Aprobado" : "Rechazado"}
                                </span>
                              )}
                            </div>
                            <h3 className="text-sm font-semibold text-foreground mt-2">
                              {item.title}
                            </h3>
                            {item.description && (
                              <p className="text-xs text-muted-foreground mt-1">{item.description}</p>
                            )}
                            <div className="flex gap-4 mt-2 text-xs text-muted-foreground">
                              {item.presenterName && <span>Presenta: {item.presenterName}</span>}
                              <span>Mayoría: {MAJORITY_LABELS[item.majorityRequired] || item.majorityRequired}</span>
                              <span>Votación: {VOTING_MODE_LABELS[item.votingMode] || item.votingMode}</span>
                            </div>
                          </div>
                          {!item.voteRegistered && !item.isInformationOnly && (
                            <button
                              onClick={() => setVoteItemId(item.id)}
                              className="px-3 py-1 text-xs font-medium text-emerald-600 border border-emerald-600 rounded hover:bg-emerald-50"
                            >
                              Registrar Voto
                            </button>
                          )}
                        </div>

                        {/* Vote Results */}
                        {item.voteRegistered && (
                          <div className="mt-3 pt-3 border-t border-border">
                            <div className="grid grid-cols-3 gap-4 text-xs">
                              <div>
                                <p className="text-muted-foreground">A Favor</p>
                                <p className="font-semibold text-emerald-600">
                                  {item.votesInFavorCoefficients} coef. ({item.votesInFavorCount} votos)
                                </p>
                              </div>
                              <div>
                                <p className="text-muted-foreground">En Contra</p>
                                <p className="font-semibold text-rose-600 dark:text-rose-400">
                                  {item.votesAgainstCoefficients} coef. ({item.votesAgainstCount} votos)
                                </p>
                              </div>
                              <div>
                                <p className="text-muted-foreground">Abstenciones</p>
                                <p className="font-semibold text-yellow-600">
                                  {item.abstentionCoefficients} coef. ({item.abstentionCount} votos)
                                </p>
                              </div>
                            </div>
                          </div>
                        )}
                      </div>
                    ))}
                  </div>
                )}

                {/* Vote Registration Form */}
                {voteItemId && (
                  <div className="border-t border-border pt-6">
                    <h3 className="text-sm font-semibold text-foreground mb-4">Registrar Voto</h3>
                    <div className="grid grid-cols-1 md:grid-cols-3 gap-4">
                      <div>
                        <label className="block text-xs font-medium text-muted-foreground mb-1">
                          Votos a Favor (Coeficiente)
                        </label>
                        <input
                          type="number"
                          step="0.01"
                          value={votesInFavorCoeff}
                          onChange={(e) => setVotesInFavorCoeff(Number(e.target.value))}
                          className="border-b border-emerald-600/30 focus:border-emerald-600 text-sm font-medium py-2 outline-none w-full"
                        />
                      </div>
                      <div>
                        <label className="block text-xs font-medium text-muted-foreground mb-1">
                          Votos en Contra (Coeficiente)
                        </label>
                        <input
                          type="number"
                          step="0.01"
                          value={votesAgainstCoeff}
                          onChange={(e) => setVotesAgainstCoeff(Number(e.target.value))}
                          className="border-b border-emerald-600/30 focus:border-emerald-600 text-sm font-medium py-2 outline-none w-full"
                        />
                      </div>
                      <div>
                        <label className="block text-xs font-medium text-muted-foreground mb-1">
                          Abstenciones (Coeficiente)
                        </label>
                        <input
                          type="number"
                          step="0.01"
                          value={abstentionCoeff}
                          onChange={(e) => setAbstentionCoeff(Number(e.target.value))}
                          className="border-b border-emerald-600/30 focus:border-emerald-600 text-sm font-medium py-2 outline-none w-full"
                        />
                      </div>
                      <div>
                        <label className="block text-xs font-medium text-muted-foreground mb-1">
                          Cantidad Votos a Favor
                        </label>
                        <input
                          type="number"
                          value={votesInFavorCount}
                          onChange={(e) => setVotesInFavorCount(Number(e.target.value))}
                          className="border-b border-emerald-600/30 focus:border-emerald-600 text-sm font-medium py-2 outline-none w-full"
                        />
                      </div>
                      <div>
                        <label className="block text-xs font-medium text-muted-foreground mb-1">
                          Cantidad Votos en Contra
                        </label>
                        <input
                          type="number"
                          value={votesAgainstCount}
                          onChange={(e) => setVotesAgainstCount(Number(e.target.value))}
                          className="border-b border-emerald-600/30 focus:border-emerald-600 text-sm font-medium py-2 outline-none w-full"
                        />
                      </div>
                      <div>
                        <label className="block text-xs font-medium text-muted-foreground mb-1">
                          Cantidad Abstenciones
                        </label>
                        <input
                          type="number"
                          value={abstentionCount}
                          onChange={(e) => setAbstentionCount(Number(e.target.value))}
                          className="border-b border-emerald-600/30 focus:border-emerald-600 text-sm font-medium py-2 outline-none w-full"
                        />
                      </div>
                      <div className="md:col-span-3">
                        <label className="block text-xs font-medium text-muted-foreground mb-1">
                          Observaciones
                        </label>
                        <input
                          type="text"
                          value={voteObservations}
                          onChange={(e) => setVoteObservations(e.target.value)}
                          className="border-b border-emerald-600/30 focus:border-emerald-600 text-sm font-medium py-2 outline-none w-full"
                          placeholder="Observaciones opcionales"
                        />
                      </div>
                    </div>
                    <div className="flex gap-3 mt-4">
                      <button
                        onClick={handleRegisterVote}
                        disabled={actionLoading}
                        className="px-4 py-2 bg-emerald-600 text-white text-sm font-medium rounded-lg hover:bg-emerald-700 disabled:opacity-50"
                      >
                        {actionLoading ? "Registrando..." : "Registrar Voto"}
                      </button>
                      <button
                        onClick={() => setVoteItemId(null)}
                        className="px-4 py-2 text-sm font-medium text-muted-foreground border border-border rounded-lg hover:bg-muted/30"
                      >
                        Cancelar
                      </button>
                    </div>
                  </div>
                )}

                {/* New Agenda Item Form */}
                <div className="border-t border-border pt-6">
                  <h3 className="text-sm font-semibold text-foreground mb-4">Nuevo Punto</h3>
                  <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
                    <div className="md:col-span-2">
                      <label className="block text-xs font-medium text-muted-foreground mb-1">Título</label>
                      <input
                        type="text"
                        value={agendaTitle}
                        onChange={(e) => setAgendaTitle(e.target.value)}
                        className="border-b border-emerald-600/30 focus:border-emerald-600 text-sm font-medium py-2 outline-none w-full"
                        placeholder="Título del punto de agenda"
                      />
                    </div>
                    <div className="md:col-span-2">
                      <label className="block text-xs font-medium text-muted-foreground mb-1">
                        Descripción
                      </label>
                      <textarea
                        value={agendaDescription}
                        onChange={(e) => setAgendaDescription(e.target.value)}
                        className="border-b border-emerald-600/30 focus:border-emerald-600 text-sm font-medium py-2 outline-none w-full resize-none"
                        rows={3}
                        placeholder="Descripción del punto"
                      ></textarea>
                    </div>
                    <div>
                      <label className="block text-xs font-medium text-muted-foreground mb-1">
                        Presentador
                      </label>
                      <input
                        type="text"
                        value={agendaPresenter}
                        onChange={(e) => setAgendaPresenter(e.target.value)}
                        className="border-b border-emerald-600/30 focus:border-emerald-600 text-sm font-medium py-2 outline-none w-full"
                        placeholder="Nombre del presentador"
                      />
                    </div>
                    <div>
                      <label className="block text-xs font-medium text-muted-foreground mb-1">
                        Mayoría Requerida
                      </label>
                      <select
                        value={agendaMajority}
                        onChange={(e) => setAgendaMajority(e.target.value)}
                        className="border-b border-emerald-600/30 focus:border-emerald-600 text-sm font-medium py-2 outline-none w-full"
                      >
                        <option value="Simple">Mayoría Simple</option>
                        <option value="Qualified">Mayoría Calificada</option>
                        <option value="Unanimous">Unanimidad</option>
                      </select>
                    </div>
                    <div>
                      <label className="block text-xs font-medium text-muted-foreground mb-1">
                        Modo de Votación
                      </label>
                      <select
                        value={agendaVotingMode}
                        onChange={(e) => setAgendaVotingMode(e.target.value)}
                        className="border-b border-emerald-600/30 focus:border-emerald-600 text-sm font-medium py-2 outline-none w-full"
                      >
                        <option value="Coefficient">Por Coeficiente</option>
                        <option value="ShowOfHands">Aplaumanos</option>
                        <option value="Written">Escrito</option>
                        <option value="Electronic">Electrónico</option>
                      </select>
                    </div>
                    <div>
                      <label className="flex items-center gap-2 text-xs font-medium text-muted-foreground">
                        <input
                          type="checkbox"
                          checked={agendaIsInformationOnly}
                          onChange={(e) => setAgendaIsInformationOnly(e.target.checked)}
                          className="rounded border-border text-emerald-600 focus:ring-emerald-500"
                        />
                        Solo Informativo (sin votación)
                      </label>
                    </div>
                  </div>
                  <button
                    onClick={handleCreateAgendaItem}
                    disabled={actionLoading || !agendaTitle}
                    className="mt-4 px-4 py-2 bg-emerald-600 text-white text-sm font-medium rounded-lg hover:bg-emerald-700 disabled:opacity-50"
                  >
                    {actionLoading ? "Creando..." : "Nuevo Punto"}
                  </button>
                </div>
              </div>
            )}

            {/* ════════════════════════════════════════════════════════════ */}
            {/* TAB: Constancias */}
            {/* ════════════════════════════════════════════════════════════ */}
            {activeTab === "constancias" && (
              <div className="space-y-6">
                <h2 className="text-lg font-semibold text-foreground">Constancias</h2>

                {/* Constancies List */}
                {assembly.constancies && assembly.constancies.length > 0 && (
                  <div className="space-y-4">
                    {assembly.constancies.map((constancy) => (
                      <div
                        key={constancy.id}
                        className="border border-border rounded-lg p-4"
                      >
                        <div className="flex items-start justify-between">
                          <div>
                            <p className="text-sm font-semibold text-foreground">
                              {constancy.ownerName}
                            </p>
                            {constancy.agendaItemTitle && (
                              <p className="text-xs text-emerald-600 mt-1">
                                Punto: {constancy.agendaItemTitle}
                              </p>
                            )}
                          </div>
                          <span className="text-xs text-muted-foreground">
                            {formatDateTime(constancy.createdAt)}
                          </span>
                        </div>
                        <p className="text-sm text-muted-foreground mt-2">{constancy.text}</p>
                      </div>
                    ))}
                  </div>
                )}

                {/* New Constancy Form */}
                <div className="border-t border-border pt-6">
                  <h3 className="text-sm font-semibold text-foreground mb-4">Nueva Constancia</h3>
                  <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
                    <div>
                      <label className="block text-xs font-medium text-muted-foreground mb-1">
                        ID del Propietario
                      </label>
                      <input
                        type="text"
                        value={constancyOwnerId}
                        onChange={(e) => setConstancyOwnerId(e.target.value)}
                        className="border-b border-emerald-600/30 focus:border-emerald-600 text-sm font-medium py-2 outline-none w-full"
                        placeholder="ID del propietario"
                      />
                    </div>
                    <div>
                      <label className="block text-xs font-medium text-muted-foreground mb-1">
                        Nombre del Propietario
                      </label>
                      <input
                        type="text"
                        value={constancyOwnerName}
                        onChange={(e) => setConstancyOwnerName(e.target.value)}
                        className="border-b border-emerald-600/30 focus:border-emerald-600 text-sm font-medium py-2 outline-none w-full"
                        placeholder="Nombre completo"
                      />
                    </div>
                    <div>
                      <label className="block text-xs font-medium text-muted-foreground mb-1">
                        Punto de Agenda (Opcional)
                      </label>
                      <select
                        value={constancyAgendaItemId}
                        onChange={(e) => setConstancyAgendaItemId(e.target.value)}
                        className="border-b border-emerald-600/30 focus:border-emerald-600 text-sm font-medium py-2 outline-none w-full"
                      >
                        <option value="">Sin punto asociado</option>
                        {assembly.agendaItems?.map((item) => (
                          <option key={item.id} value={item.id}>
                            Punto {item.sequenceNumber}: {item.title}
                          </option>
                        ))}
                      </select>
                    </div>
                    <div className="md:col-span-2">
                      <label className="block text-xs font-medium text-muted-foreground mb-1">Texto</label>
                      <textarea
                        value={constancyText}
                        onChange={(e) => setConstancyText(e.target.value)}
                        className="border-b border-emerald-600/30 focus:border-emerald-600 text-sm font-medium py-2 outline-none w-full resize-none"
                        rows={4}
                        placeholder="Texto de la constancia"
                      ></textarea>
                    </div>
                  </div>
                  <button
                    onClick={handleCreateConstancy}
                    disabled={actionLoading || !constancyOwnerId || !constancyOwnerName || !constancyText}
                    className="mt-4 px-4 py-2 bg-emerald-600 text-white text-sm font-medium rounded-lg hover:bg-emerald-700 disabled:opacity-50"
                  >
                    {actionLoading ? "Creando..." : "Nueva Constancia"}
                  </button>
                </div>
              </div>
            )}

            {/* ════════════════════════════════════════════════════════════ */}
            {/* TAB: Acta */}
            {/* ════════════════════════════════════════════════════════════ */}
            {activeTab === "acta" && (
              <div className="space-y-6">
                <h2 className="text-lg font-semibold text-foreground">Acta de Asamblea</h2>

                {assembly.minutes ? (
                  <div>
                    {/* Minutes Status */}
                    <div className="flex items-center gap-3 mb-4">
                      <span className="text-xs font-medium text-muted-foreground">Estado:</span>
                      <span className="px-3 py-1 bg-emerald-100 text-emerald-700 rounded-full text-xs font-semibold">
                        {MINUTES_STATUS_LABELS[assembly.minutes.status] || assembly.minutes.status}
                      </span>
                    </div>

                    {/* Minutes Metadata */}
                    <div className="grid grid-cols-2 md:grid-cols-4 gap-4 text-xs mb-6">
                      <div>
                        <p className="text-muted-foreground">Generada</p>
                        <p className="font-semibold text-foreground">
                          {formatDateTime(assembly.minutes.generatedAt)}
                        </p>
                      </div>
                      <div>
                        <p className="text-muted-foreground">Presidente</p>
                        <p className="font-semibold text-foreground">
                          {assembly.minutes.presidentName || "—"}
                        </p>
                      </div>
                      <div>
                        <p className="text-muted-foreground">Secretario</p>
                        <p className="font-semibold text-foreground">
                          {assembly.minutes.secretaryName || "—"}
                        </p>
                      </div>
                      {assembly.minutes.approvedAt && (
                        <div>
                          <p className="text-muted-foreground">Aprobada</p>
                          <p className="font-semibold text-foreground">
                            {formatDateTime(assembly.minutes.approvedAt)}
                          </p>
                        </div>
                      )}
                      {assembly.minutes.publishedAt && (
                        <div>
                          <p className="text-muted-foreground">Publicada</p>
                          <p className="font-semibold text-foreground">
                            {formatDateTime(assembly.minutes.publishedAt)}
                          </p>
                        </div>
                      )}
                    </div>

                    {/* Minutes Full Text */}
                    <div className="bg-muted/50 rounded-lg p-6 border border-border">
                      <h3 className="text-sm font-semibold text-foreground mb-4">Texto Completo del Acta</h3>
                      <div className="text-sm text-muted-foreground whitespace-pre-wrap leading-relaxed">
                        {assembly.minutes.fullText}
                      </div>
                    </div>

                    {/* Action Buttons */}
                    <div className="flex gap-3 mt-6">
                      {assembly.minutes.status === "Generated" && (
                        <button
                          onClick={handleApproveMinutes}
                          disabled={actionLoading}
                          className="px-4 py-2 bg-emerald-600 text-white text-sm font-medium rounded-lg hover:bg-emerald-700 disabled:opacity-50"
                        >
                          {actionLoading ? "Aprobando..." : "Aprobar Acta"}
                        </button>
                      )}
                      {assembly.minutes.status === "Approved" && (
                        <button
                          onClick={handlePublishMinutes}
                          disabled={actionLoading}
                          className="px-4 py-2 bg-emerald-600 text-white text-sm font-medium rounded-lg hover:bg-emerald-700 disabled:opacity-50"
                        >
                          {actionLoading ? "Publicando..." : "Publicar Acta"}
                        </button>
                      )}
                    </div>
                  </div>
                ) : (
                  <div>
                    <p className="text-sm text-muted-foreground mb-6">
                      No se ha generado el acta para esta asamblea.
                    </p>

                    {/* Generate Minutes Form */}
                    <div className="bg-muted/50 rounded-lg p-6 border border-border">
                      <h3 className="text-sm font-semibold text-foreground mb-4">
                        Generar Acta
                      </h3>
                      <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
                        <div>
                          <label className="block text-xs font-medium text-muted-foreground mb-1">
                            Nombre del Presidente
                          </label>
                          <input
                            type="text"
                            value={minutesPresident}
                            onChange={(e) => setMinutesPresident(e.target.value)}
                            className="border-b border-emerald-600/30 focus:border-emerald-600 text-sm font-medium py-2 outline-none w-full"
                            placeholder="Nombre del presidente"
                          />
                        </div>
                        <div>
                          <label className="block text-xs font-medium text-muted-foreground mb-1">
                            Nombre del Secretario
                          </label>
                          <input
                            type="text"
                            value={minutesSecretary}
                            onChange={(e) => setMinutesSecretary(e.target.value)}
                            className="border-b border-emerald-600/30 focus:border-emerald-600 text-sm font-medium py-2 outline-none w-full"
                            placeholder="Nombre del secretario"
                          />
                        </div>
                        <div className="md:col-span-2">
                          <label className="block text-xs font-medium text-muted-foreground mb-1">
                            Miembros de Comisión (separados por coma)
                          </label>
                          <input
                            type="text"
                            value={minutesCommissionMembers}
                            onChange={(e) => setMinutesCommissionMembers(e.target.value)}
                            className="border-b border-emerald-600/30 focus:border-emerald-600 text-sm font-medium py-2 outline-none w-full"
                            placeholder="Nombre 1, Nombre 2, Nombre 3"
                          />
                        </div>
                      </div>
                      <button
                        onClick={handleGenerateMinutes}
                        disabled={actionLoading}
                        className="mt-4 px-4 py-2 bg-emerald-600 text-white text-sm font-medium rounded-lg hover:bg-emerald-700 disabled:opacity-50"
                      >
                        {actionLoading ? "Generando..." : "Generar Acta"}
                      </button>
                    </div>
                  </div>
                )}
              </div>
            )}

            {/* ════════════════════════════════════════════════════════════ */}
            {/* TAB: Propagación */}
            {/* ════════════════════════════════════════════════════════════ */}
            {activeTab === "propagacion" && (
              <div className="space-y-6">
                <h2 className="text-lg font-semibold text-foreground">Propagación de Decisiones</h2>

                {assembly.agendaItems && assembly.agendaItems.length > 0 && (
                  <div className="space-y-4">
                    {assembly.agendaItems
                      .filter((item) => item.voteRegistered && item.isApproved)
                      .map((item) => (
                        <div
                          key={item.id}
                          className="border border-border rounded-lg p-4"
                        >
                          <div className="flex items-start justify-between">
                            <div>
                              <h3 className="text-sm font-semibold text-foreground">
                                Punto {item.sequenceNumber}: {item.title}
                              </h3>
                              <p className="text-xs text-emerald-600 mt-1">Aprobado</p>
                            </div>
                          </div>
                          <div className="mt-3 flex gap-4 text-xs text-muted-foreground">
                            <span>
                              A Favor: {item.votesInFavorCoefficients} coef. ({item.votesInFavorCount})
                            </span>
                            <span>
                              En Contra: {item.votesAgainstCoefficients} coef. ({item.votesAgainstCount})
                            </span>
                            <span>
                              Abstenciones: {item.abstentionCoefficients} coef. ({item.abstentionCount})
                            </span>
                          </div>
                        </div>
                      ))}
                  </div>
                )}

                {assembly.agendaItems &&
                  assembly.agendaItems.filter((item) => item.voteRegistered && item.isApproved)
                    .length === 0 && (
                    <p className="text-sm text-muted-foreground">
                      No hay decisiones aprobadas para propagar.
                    </p>
                  )}
              </div>
            )}
          </div>
        </div>
      </main>
  );
}
