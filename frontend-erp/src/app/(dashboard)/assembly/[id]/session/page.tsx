"use client";

import React, { useState, useEffect, useCallback } from "react";
import { useRouter, useParams } from "next/navigation";
import assemblyService, {
  AssemblyDetail,
  AgendaItemDto,
  ConstancyDto,
  AttendanceDto,
  RegisterVoteRequest,
  CreateConstancyRequest,
} from "@/lib/assembly-service";


const MAJORITY_LABELS: Record<string, string> = {
  Simple: "Mayoría Simple",
  Qualified: "Mayoría Calificada",
  Unanimous: "Unanimidad",
};

const MAJORITY_DESCRIPTIONS: Record<string, string> = {
  Simple: "Requiere más de la mitad de los votos emitidos para ser aprobada.",
  Qualified:
    "Requiere al menos dos tercios de los coeficientes de los asistentes a favor para ser aprobada.",
  Unanimous:
    "Requiere la totalidad de los votos emitidos a favor para ser aprobada.",
};

const VOTING_MODE_LABELS: Record<string, string> = {
  ShowOfHands: "Pública (Aplaumanos)",
  Written: "Escrita",
  Electronic: "Electrónica",
  Coefficient: "Por Coeficiente",
};

export default function SessionPage() {
  const router = useRouter();
  const params = useParams();
  const assemblyId = params.id as string;

  const [assembly, setAssembly] = useState<AssemblyDetail | null>(null);
  const [attendances, setAttendances] = useState<AttendanceDto[]>([]);
  const [constancies, setConstancies] = useState<ConstancyDto[]>([]);
  const [loading, setLoading] = useState(true);
  const [actionLoading, setActionLoading] = useState(false);

  const [activeVoteItemId, setActiveVoteItemId] = useState<string | null>(null);
  const [votesInFavorCoeff, setVotesInFavorCoeff] = useState(0);
  const [votesAgainstCoeff, setVotesAgainstCoeff] = useState(0);
  const [abstentionCoeff, setAbstentionCoeff] = useState(0);
  const [votesInFavorCount, setVotesInFavorCount] = useState(0);
  const [votesAgainstCount, setVotesAgainstCount] = useState(0);
  const [abstentionCount, setAbstentionCount] = useState(0);
  const [voteObservations, setVoteObservations] = useState("");

  const [constancyOwnerId, setConstancyOwnerId] = useState("");
  const [constancyOwnerName, setConstancyOwnerName] = useState("");
  const [constancyText, setConstancyText] = useState("");

  const loadData = useCallback(async () => {
    try {
      setLoading(true);
      const [assemblyData, attendancesData, constanciesData] = await Promise.all([
        assemblyService.getAssemblyById(assemblyId),
        assemblyService.getAttendances(assemblyId),
        assemblyService.getConstancies(assemblyId),
      ]);
      setAssembly(assemblyData);
      setAttendances(attendancesData);
      setConstancies(constanciesData);
    } catch (error) {
      console.error("Error al cargar datos de sesión:", error);
    } finally {
      setLoading(false);
    }
  }, [assemblyId]);

  useEffect(() => {
    loadData();
  }, [loadData]);

  const formatDateTime = (dateStr: string) => {
    try {
      return new Date(dateStr).toLocaleString('es-CO', { day: '2-digit', month: '2-digit', year: 'numeric', hour: '2-digit', minute: '2-digit' });
    } catch {
      return dateStr;
    }
  };

  const formatDate = (dateStr: string) => {
    try {
      return new Date(dateStr).toLocaleDateString('es-CO', { day: '2-digit', month: 'long', year: 'numeric' });
    } catch {
      return dateStr;
    }
  };

  const resetVoteForm = () => {
    setActiveVoteItemId(null);
    setVotesInFavorCoeff(0);
    setVotesAgainstCoeff(0);
    setAbstentionCoeff(0);
    setVotesInFavorCount(0);
    setVotesAgainstCount(0);
    setAbstentionCount(0);
    setVoteObservations("");
  };

  const handleOpenVoteForm = (itemId: string) => {
    resetVoteForm();
    setActiveVoteItemId(itemId);
  };

  const handleRegisterVote = async () => {
    if (!activeVoteItemId) return;
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
      await assemblyService.registerVote(activeVoteItemId, request);
      resetVoteForm();
      await loadData();
    } catch (error) {
      console.error("Error al registrar voto:", error);
    } finally {
      setActionLoading(false);
    }
  };

  const handleCreateConstancy = async () => {
    if (!assembly || !constancyOwnerId || !constancyOwnerName || !constancyText) return;
    try {
      setActionLoading(true);
      const request: CreateConstancyRequest = {
        ownerId: constancyOwnerId,
        ownerName: constancyOwnerName,
        text: constancyText,
      };
      await assemblyService.createConstancy(assembly.id, request);
      setConstancyOwnerId("");
      setConstancyOwnerName("");
      setConstancyText("");
      await loadData();
    } catch (error) {
      console.error("Error al crear constancia:", error);
    } finally {
      setActionLoading(false);
    }
  };

  const handleOwnerSelect = (ownerId: string) => {
    setConstancyOwnerId(ownerId);
    const attendance = attendances.find((a) => a.ownerId === ownerId);
    if (attendance) {
      setConstancyOwnerName(attendance.ownerName);
    } else {
      setConstancyOwnerName("");
    }
  };

  const getVoteTotalCoefficients = (item: AgendaItemDto) => {
    return (
      item.votesInFavorCoefficients +
      item.votesAgainstCoefficients +
      item.abstentionCoefficients
    );
  };

  const getVotePercentage = (value: number, total: number) => {
    if (total === 0) return 0;
    return (value / total) * 100;
  };

  const getMajorityThreshold = (item: AgendaItemDto, totalCoeff: number) => {
    if (item.majorityRequired === "Simple") {
      return totalCoeff / 2;
    }
    if (item.majorityRequired === "Qualified") {
      return (totalCoeff * 2) / 3;
    }
    return totalCoeff;
  };

  if (loading) {
    return (
      <main className="flex items-center justify-center">
        <div className="text-center">
          <div className="animate-spin rounded-full h-12 w-12 border-b-2 border-emerald-600 mx-auto mb-4"></div>
          <p className="text-gray-500 text-sm font-medium">Cargando datos de sesión...</p>
        </div>
      </main>
    );
  }

  if (!assembly) {
    return (
      <main className="flex items-center justify-center">
        <div className="text-center">
          <p className="text-gray-500 text-lg">Asamblea no encontrada</p>
          <button
            onClick={() => router.push("/assembly")}
            className="mt-4 text-emerald-600 hover:text-emerald-700 font-medium text-sm"
          >
            Volver a la lista
          </button>
        </div>
      </main>
    );
  }

  const agendaItems = assembly.agendaItems || [];
  const sortedItems = [...agendaItems].sort((a, b) => a.sequenceNumber - b.sequenceNumber);

  return (
    <main className="p-6 space-y-6">
        <div className="flex items-center gap-4">
          <button
            onClick={() => router.push(`/assembly/${assemblyId}`)}
            className="text-emerald-600 hover:text-emerald-800 text-sm font-semibold"
          >
            &larr; Volver
          </button>
          <h1 className="text-2xl font-bold text-gray-900 tracking-tight">
            Gestión de Sesión - {assembly.title}
          </h1>
        </div>

        <div className="bg-white border border-gray-200 rounded-lg p-5">
          <h2 className="text-lg font-bold text-gray-900 mb-4">Información de la Sesión</h2>
          <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-5">
            <div>
              <label className="block text-xs font-medium text-gray-500 uppercase tracking-wider mb-1">
                Fecha de la Asamblea
              </label>
              <p className="text-sm font-semibold text-gray-900">
                {formatDate(assembly.scheduledDate)}
              </p>
            </div>
            <div>
              <label className="block text-xs font-medium text-gray-500 uppercase tracking-wider mb-1">
                Hora Programada
              </label>
              <p className="text-sm font-semibold text-gray-900">{assembly.scheduledTime}</p>
            </div>
            <div>
              <label className="block text-xs font-medium text-gray-500 uppercase tracking-wider mb-1">
                Presidente
              </label>
              <p className="text-sm font-semibold text-gray-900">
                {assembly.presidentName || "—"}
              </p>
            </div>
            <div>
              <label className="block text-xs font-medium text-gray-500 uppercase tracking-wider mb-1">
                Secretario
              </label>
              <p className="text-sm font-semibold text-gray-900">
                {assembly.secretaryName || "—"}
              </p>
            </div>
            <div>
              <label className="block text-xs font-medium text-gray-500 uppercase tracking-wider mb-1">
                Número de Convocatoria
              </label>
              <p className="text-sm font-semibold text-gray-900">
                {assembly.convocationNumber}
              </p>
            </div>
            {assembly.sessionStartTime && (
              <div>
                <label className="block text-xs font-medium text-gray-500 uppercase tracking-wider mb-1">
                  Inicio de Sesión
                </label>
                <p className="text-sm font-semibold text-gray-900">
                  {formatDateTime(assembly.sessionStartTime)}
                </p>
              </div>
            )}
            {assembly.sessionEndTime && (
              <div>
                <label className="block text-xs font-medium text-gray-500 uppercase tracking-wider mb-1">
                  Fin de Sesión
                </label>
                <p className="text-sm font-semibold text-gray-900">
                  {formatDateTime(assembly.sessionEndTime)}
                </p>
              </div>
            )}
            <div>
              <label className="block text-xs font-medium text-gray-500 uppercase tracking-wider mb-1">
                Lugar
              </label>
              <p className="text-sm font-semibold text-gray-900">{assembly.location}</p>
            </div>
          </div>
        </div>

        <div className="bg-white border border-gray-200 rounded-lg p-5">
          <h2 className="text-lg font-bold text-gray-900 mb-6">Orden del Día y Votaciones</h2>

          {sortedItems.length === 0 && (
            <p className="text-sm text-gray-500 text-center py-8">
              No hay puntos en el orden del día.
            </p>
          )}

          <div className="space-y-6">
            {sortedItems.map((item) => {
              const totalCoeff = getVoteTotalCoefficients(item);
              const favorPct = getVotePercentage(item.votesInFavorCoefficients, totalCoeff);
              const againstPct = getVotePercentage(item.votesAgainstCoefficients, totalCoeff);
              const abstentionPct = getVotePercentage(item.abstentionCoefficients, totalCoeff);
              const threshold = getMajorityThreshold(item, totalCoeff);
              const showForm = activeVoteItemId === item.id;

              return (
                <div
                  key={item.id}
                  className="border border-gray-200 rounded-lg p-5"
                >
                  <div className="flex items-start justify-between">
                    <div className="flex-1">
                      <div className="flex items-center gap-2 flex-wrap">
                        <span className="text-xs font-bold text-emerald-600 bg-emerald-50 px-2 py-0.5 rounded">
                          Punto {item.sequenceNumber}
                        </span>
                        {item.isInformationOnly && (
                          <span className="text-xs text-gray-500 bg-gray-100 px-2 py-0.5 rounded">
                            Informativo
                          </span>
                        )}
                        {item.voteRegistered && (
                          <span
                            className={`text-xs font-semibold px-2 py-0.5 rounded ${
                              item.isApproved
                                ? "bg-emerald-100 text-emerald-700"
                                : "bg-red-100 text-red-700"
                            }`}
                          >
                            {item.isApproved ? "Aprobada" : "No Aprobada"}
                          </span>
                        )}
                      </div>

                      <h3 className="text-sm font-semibold text-gray-900 mt-2">
                        {item.title}
                      </h3>

                      {item.description && (
                        <p className="text-xs text-gray-500 mt-1">{item.description}</p>
                      )}

                      <div className="flex gap-4 mt-2 text-xs text-gray-500 flex-wrap">
                        {item.presenterName && (
                          <span>Presenta: {item.presenterName}</span>
                        )}
                      </div>

                      <div className="mt-3 bg-gray-50 rounded-md p-3 space-y-1">
                        <div className="flex items-center gap-2">
                          <span className="text-xs font-semibold text-gray-700">
                            Mayoría requerida:
                          </span>
                          <span className="text-xs text-emerald-700 font-medium">
                            {MAJORITY_LABELS[item.majorityRequired] || item.majorityRequired}
                          </span>
                        </div>
                        <p className="text-xs text-gray-500">
                          {MAJORITY_DESCRIPTIONS[item.majorityRequired] || ""}
                        </p>
                        <div className="flex items-center gap-2 mt-1">
                          <span className="text-xs font-semibold text-gray-700">
                            Modo de votación:
                          </span>
                          <span className="text-xs text-gray-600">
                            {VOTING_MODE_LABELS[item.votingMode] || item.votingMode}
                          </span>
                        </div>
                      </div>
                    </div>

                    {!item.voteRegistered && !item.isInformationOnly && !showForm && (
                      <button
                        onClick={() => handleOpenVoteForm(item.id)}
                        className="px-3 py-1 text-xs font-medium text-emerald-600 border border-emerald-600 rounded hover:bg-emerald-50 ml-4 flex-shrink-0"
                      >
                        Registrar Voto
                      </button>
                    )}
                  </div>

                  {item.voteRegistered && (
                    <div className="mt-4 pt-4 border-t border-gray-100">
                      <div className="grid grid-cols-3 gap-4 text-xs mb-3">
                        <div>
                          <p className="text-gray-500 font-medium">A Favor</p>
                          <p className="font-bold text-emerald-600 text-sm">
                            {item.votesInFavorCoefficients} coef.
                          </p>
                          <p className="text-gray-500">{item.votesInFavorCount} votos</p>
                        </div>
                        <div>
                          <p className="text-gray-500 font-medium">En Contra</p>
                          <p className="font-bold text-red-600 text-sm">
                            {item.votesAgainstCoefficients} coef.
                          </p>
                          <p className="text-gray-500">{item.votesAgainstCount} votos</p>
                        </div>
                        <div>
                          <p className="text-gray-500 font-medium">Abstenciones</p>
                          <p className="font-bold text-yellow-600 text-sm">
                            {item.abstentionCoefficients} coef.
                          </p>
                          <p className="text-gray-500">{item.abstentionCount} votos</p>
                        </div>
                      </div>

                      {totalCoeff > 0 && (
                        <div className="space-y-2">
                          <div>
                            <div className="flex items-center justify-between mb-1">
                              <span className="text-xs text-gray-500">A Favor</span>
                              <span className="text-xs text-gray-500">
                                {favorPct.toFixed(1)}%
                              </span>
                            </div>
                            <div className="w-full bg-gray-200 rounded-full h-2">
                              <div
                                className="h-2 rounded-full bg-emerald-500 transition-all"
                                style={{ width: `${favorPct}%` }}
                              ></div>
                            </div>
                          </div>
                          <div>
                            <div className="flex items-center justify-between mb-1">
                              <span className="text-xs text-gray-500">En Contra</span>
                              <span className="text-xs text-gray-500">
                                {againstPct.toFixed(1)}%
                              </span>
                            </div>
                            <div className="w-full bg-gray-200 rounded-full h-2">
                              <div
                                className="h-2 rounded-full bg-red-500 transition-all"
                                style={{ width: `${againstPct}%` }}
                              ></div>
                            </div>
                          </div>
                          <div>
                            <div className="flex items-center justify-between mb-1">
                              <span className="text-xs text-gray-500">Abstenciones</span>
                              <span className="text-xs text-gray-500">
                                {abstentionPct.toFixed(1)}%
                              </span>
                            </div>
                            <div className="w-full bg-gray-200 rounded-full h-2">
                              <div
                                className="h-2 rounded-full bg-yellow-500 transition-all"
                                style={{ width: `${abstentionPct}%` }}
                              ></div>
                            </div>
                          </div>
                        </div>
                      )}

                      {item.majorityRequired === "Qualified" && totalCoeff > 0 && (
                        <p className="text-xs text-gray-500 mt-2">
                          Umbral requerido (2/3): {threshold.toFixed(2)} coef. —{" "}
                          {item.votesInFavorCoefficients >= threshold
                            ? "Resultado alcanza la mayoría calificada."
                            : "Resultado no alcanza la mayoría calificada."}
                        </p>
                      )}

                      {item.observations && (
                        <p className="text-xs text-gray-500 mt-2">
                          <span className="font-medium">Observaciones:</span>{" "}
                          {item.observations}
                        </p>
                      )}
                    </div>
                  )}

                  {showForm && (
                    <div className="mt-4 pt-4 border-t border-gray-200">
                      <h4 className="text-xs font-semibold text-gray-900 mb-3">
                        Registrar Voto — Punto {item.sequenceNumber}
                      </h4>
                      <div className="grid grid-cols-1 md:grid-cols-3 gap-4">
                        <div>
                          <label className="block text-xs font-medium text-gray-700 mb-1">
                            Coeficientes a Favor
                          </label>
                          <input
                            type="number"
                            step="0.01"
                            min="0"
                            value={votesInFavorCoeff}
                            onChange={(e) => setVotesInFavorCoeff(Number(e.target.value))}
                            className="border-b border-emerald-600/30 focus:border-emerald-600 text-sm font-medium py-2 outline-none w-full"
                          />
                        </div>
                        <div>
                          <label className="block text-xs font-medium text-gray-700 mb-1">
                            Coeficientes en Contra
                          </label>
                          <input
                            type="number"
                            step="0.01"
                            min="0"
                            value={votesAgainstCoeff}
                            onChange={(e) => setVotesAgainstCoeff(Number(e.target.value))}
                            className="border-b border-emerald-600/30 focus:border-emerald-600 text-sm font-medium py-2 outline-none w-full"
                          />
                        </div>
                        <div>
                          <label className="block text-xs font-medium text-gray-700 mb-1">
                            Coeficientes Abstenciones
                          </label>
                          <input
                            type="number"
                            step="0.01"
                            min="0"
                            value={abstentionCoeff}
                            onChange={(e) => setAbstentionCoeff(Number(e.target.value))}
                            className="border-b border-emerald-600/30 focus:border-emerald-600 text-sm font-medium py-2 outline-none w-full"
                          />
                        </div>
                        <div>
                          <label className="block text-xs font-medium text-gray-700 mb-1">
                            Votos a Favor
                          </label>
                          <input
                            type="number"
                            min="0"
                            value={votesInFavorCount}
                            onChange={(e) => setVotesInFavorCount(Number(e.target.value))}
                            className="border-b border-emerald-600/30 focus:border-emerald-600 text-sm font-medium py-2 outline-none w-full"
                          />
                        </div>
                        <div>
                          <label className="block text-xs font-medium text-gray-700 mb-1">
                            Votos en Contra
                          </label>
                          <input
                            type="number"
                            min="0"
                            value={votesAgainstCount}
                            onChange={(e) => setVotesAgainstCount(Number(e.target.value))}
                            className="border-b border-emerald-600/30 focus:border-emerald-600 text-sm font-medium py-2 outline-none w-full"
                          />
                        </div>
                        <div>
                          <label className="block text-xs font-medium text-gray-700 mb-1">
                            Abstenciones
                          </label>
                          <input
                            type="number"
                            min="0"
                            value={abstentionCount}
                            onChange={(e) => setAbstentionCount(Number(e.target.value))}
                            className="border-b border-emerald-600/30 focus:border-emerald-600 text-sm font-medium py-2 outline-none w-full"
                          />
                        </div>
                        <div className="md:col-span-3">
                          <label className="block text-xs font-medium text-gray-700 mb-1">
                            Observaciones
                          </label>
                          <textarea
                            value={voteObservations}
                            onChange={(e) => setVoteObservations(e.target.value)}
                            className="border-b border-emerald-600/30 focus:border-emerald-600 text-sm font-medium py-2 outline-none w-full resize-none"
                            rows={2}
                            placeholder="Observaciones opcionales"
                          ></textarea>
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
                          onClick={resetVoteForm}
                          className="px-4 py-2 text-sm font-medium text-gray-600 border border-gray-300 rounded-lg hover:bg-gray-50"
                        >
                          Cancelar
                        </button>
                      </div>
                    </div>
                  )}
                </div>
              );
            })}
          </div>
        </div>

        <div className="bg-white border border-gray-200 rounded-lg p-5">
          <h2 className="text-lg font-bold text-gray-900 mb-4">Constancias</h2>

          {constancies.length > 0 && (
            <div className="space-y-3 mb-6">
              {constancies.map((constancy) => (
                <div
                  key={constancy.id}
                  className="border border-gray-200 rounded-lg p-4"
                >
                  <div className="flex items-start justify-between">
                    <div>
                      <p className="text-sm font-semibold text-gray-900">
                        {constancy.ownerName}
                      </p>
                      {constancy.agendaItemTitle && (
                        <p className="text-xs text-emerald-600 mt-1">
                          Punto: {constancy.agendaItemTitle}
                        </p>
                      )}
                    </div>
                    <span className="text-xs text-gray-500">
                      {formatDateTime(constancy.createdAt)}
                    </span>
                  </div>
                  <p className="text-sm text-gray-600 mt-2">{constancy.text}</p>
                </div>
              ))}
            </div>
          )}

          {constancies.length === 0 && (
            <p className="text-sm text-gray-500 mb-6">No hay constancias registradas.</p>
          )}

          <div className="border-t border-gray-200 pt-5">
            <h3 className="text-sm font-semibold text-gray-900 mb-4">Agregar Constancia</h3>
            <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
              <div>
                <label className="block text-xs font-medium text-gray-700 mb-1">
                  Propietario
                </label>
                <select
                  value={constancyOwnerId}
                  onChange={(e) => handleOwnerSelect(e.target.value)}
                  className="border-b border-emerald-600/30 focus:border-emerald-600 text-sm font-medium py-2 outline-none w-full"
                >
                  <option value="">Seleccionar propietario...</option>
                  {attendances.map((att) => (
                    <option key={att.ownerId} value={att.ownerId}>
                      {att.ownerName} — {att.unitIdentifier} (Coef. {att.coefficient})
                    </option>
                  ))}
                </select>
              </div>
              <div className="md:col-span-2">
                <label className="block text-xs font-medium text-gray-700 mb-1">Texto</label>
                <textarea
                  value={constancyText}
                  onChange={(e) => setConstancyText(e.target.value)}
                  className="border-b border-emerald-600/30 focus:border-emerald-600 text-sm font-medium py-2 outline-none w-full resize-none"
                  rows={3}
                  placeholder="Texto de la constancia"
                ></textarea>
              </div>
            </div>
            <button
              onClick={handleCreateConstancy}
              disabled={actionLoading || !constancyOwnerId || !constancyText}
              className="mt-4 px-4 py-2 bg-emerald-600 text-white text-sm font-medium rounded-lg hover:bg-emerald-700 disabled:opacity-50"
            >
              {actionLoading ? "Agregando..." : "Agregar Constancia"}
            </button>
          </div>
        </div>
    </main>
  );
}
