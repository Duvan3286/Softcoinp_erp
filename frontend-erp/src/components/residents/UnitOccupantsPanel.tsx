"use client";

import React, { useEffect, useState } from "react";
import { ResidentsService, UnitOccupants } from "@/lib/residents-service";
import Link from "next/link";
import {
  Users,
  Building2,
  Star,
  UserCheck,
  PawPrint,
  ArrowRightLeft,
  Plus,
  CalendarDays,
  History,
  X,
} from "lucide-react";

interface UnitOccupantsPanelProps {
  unitId: string;
}

export default function UnitOccupantsPanel({ unitId }: UnitOccupantsPanelProps) {
  const [occupants, setOccupants] = useState<UnitOccupants | null>(null);
  const [loading, setLoading] = useState(true);
  const [ownerHistory, setOwnerHistory] = useState<
    Array<{
      id: string;
      ownerName: string;
      ownerDocument: string;
      startDate: string;
      endDate?: string;
      transferNotes?: string;
    }>
  >([]);
  const [showHistory, setShowHistory] = useState(false);
  const [loadingHistory, setLoadingHistory] = useState(false);
  const [togglingResidenceId, setTogglingResidenceId] = useState<string | null>(null);
  const [residenceError, setResidenceError] = useState("");

  const [removingMemberId, setRemovingMemberId] = useState<string | null>(null);
  const [memberPendingRemoval, setMemberPendingRemoval] = useState<{ id: string; name: string } | null>(null);
  const [residencePendingChange, setResidencePendingChange] = useState<{
    assignmentId: string;
    ownerName: string;
    newResidesInUnit: boolean;
  } | null>(null);

  useEffect(() => {
    loadOccupants();
  }, [unitId]);

  const loadOccupants = async () => {
    setLoading(true);
    try {
      const data = await ResidentsService.getUnitOccupants(unitId);
      setOccupants(data);
    } catch {
      setOccupants(null);
    } finally {
      setLoading(false);
    }
  };

  const handleToggleResidence = async (assignmentId: string, newResidesInUnit: boolean) => {
    setResidenceError("");
    setTogglingResidenceId(assignmentId);
    try {
      await ResidentsService.updateOwnerResidence(unitId, assignmentId, newResidesInUnit);
      await loadOccupants();
    } catch {
      setResidenceError("No se pudo actualizar si el propietario reside en la unidad.");
    } finally {
      setTogglingResidenceId(null);
      setResidencePendingChange(null);
    }
  };

  const handleRemoveMember = async (memberId: string) => {
    setRemovingMemberId(memberId);
    try {
      await ResidentsService.removeCohabitationMember(unitId, memberId);
      await loadOccupants();
    } catch {
      setResidenceError("No se pudo quitar el integrante del grupo de convivencia.");
    } finally {
      setRemovingMemberId(null);
      setMemberPendingRemoval(null);
    }
  };

  const handleToggleHistory = async () => {
    if (!showHistory && ownerHistory.length === 0) {
      setLoadingHistory(true);
      try {
        const data = await ResidentsService.getOwnerHistory(unitId);
        setOwnerHistory(data);
      } catch {
        setOwnerHistory([]);
      } finally {
        setLoadingHistory(false);
      }
    }
    setShowHistory((v) => !v);
  };

  const formatDate = (dateStr?: string) => {
    if (!dateStr) return "Actualidad";
    return new Date(dateStr).toLocaleDateString("es-CO", {
      year: "numeric",
      month: "short",
      day: "numeric",
    });
  };

  const leaseDaysLabel = (days?: number) => {
    if (days === undefined || days === null) return null;
    if (days < 0) return { text: "Vencido", cls: "bg-red-100 text-red-700" };
    if (days <= 30) return { text: `${days}d restantes`, cls: "bg-amber-100 text-amber-700" };
    return { text: `${days}d restantes`, cls: "bg-emerald-100 text-emerald-700" };
  };

  if (loading) {
    return (
      <div className="bg-white rounded-xl shadow-sm border border-gray-100 overflow-hidden">
        <div className="px-6 py-4 border-b border-gray-100 bg-gray-50/50 flex justify-between items-center">
          <h3 className="font-bold text-gray-800">Residentes y Propietarios</h3>
        </div>
        <div className="flex justify-center py-10">
          <div className="animate-spin rounded-full h-6 w-6 border-b-2 border-blue-600" />
        </div>
      </div>
    );
  }

  if (!occupants) {
    return (
      <div className="bg-white rounded-xl shadow-sm border border-gray-100 overflow-hidden">
        <div className="px-6 py-4 border-b border-gray-100 bg-gray-50/50">
          <h3 className="font-bold text-gray-800">Residentes y Propietarios</h3>
        </div>
        <div className="p-6 text-center text-sm text-gray-400">
          No se pudo cargar la información de ocupantes.
        </div>
      </div>
    );
  }

  const hasNoOccupants =
    occupants.activeOwners.length === 0 &&
    !occupants.activeTenant &&
    occupants.cohabitationMembers.length === 0;

  const humans = occupants.cohabitationMembers.filter((m) => !m.isPet);
  const pets = occupants.cohabitationMembers.filter((m) => m.isPet);

  return (
    <div className="bg-white rounded-xl shadow-sm border border-gray-100 overflow-hidden">
      <div className="px-6 py-4 border-b border-gray-100 bg-gray-50/50 flex justify-between items-center">
        <h3 className="font-bold text-gray-800">Residentes y Propietarios</h3>
        <div className="flex gap-2">
          <button
            onClick={handleToggleHistory}
            className="flex items-center gap-1.5 text-xs font-semibold text-gray-600 bg-white border border-gray-200 px-3 py-1.5 rounded-lg hover:bg-gray-50 transition-colors"
          >
            <History className="w-3.5 h-3.5" />
            {showHistory ? "Ocultar historial" : "Ver historial"}
          </button>
          <Link
            href={`/residents/transfer/${unitId}`}
            className="flex items-center gap-1.5 text-xs font-semibold text-amber-700 bg-amber-50 border border-amber-100 px-3 py-1.5 rounded-lg hover:bg-amber-100 transition-colors"
          >
            <ArrowRightLeft className="w-3.5 h-3.5" />
            Transferir
          </Link>
        </div>
      </div>

      {residenceError && (
        <div className="px-6 py-3 bg-red-50 border-b border-red-100 text-sm font-semibold text-red-700">
          {residenceError}
        </div>
      )}

      {hasNoOccupants ? (
        <div className="p-8 flex flex-col items-center text-center">
          <div className="w-12 h-12 bg-gray-100 rounded-full flex items-center justify-center mb-3">
            <Users className="w-6 h-6 text-gray-400" />
          </div>
          <p className="text-sm font-semibold text-gray-600">Sin ocupantes registrados</p>
          <p className="text-xs text-gray-400 mt-1">
            Registra propietarios y arrendatarios desde el módulo de Residentes.
          </p>
          <Link
            href="/residents"
            className="mt-4 inline-flex items-center gap-2 px-4 py-2 bg-blue-600 text-white text-sm font-semibold rounded-xl hover:bg-blue-700 transition-colors"
          >
            <Plus className="w-4 h-4" />
            Ir a Residentes
          </Link>
        </div>
      ) : (
        <div className="divide-y divide-gray-100">
          {/* Propietarios */}
          {occupants.activeOwners.length > 0 && (
            <div className="px-6 py-4 space-y-3">
              <p className="text-xs font-bold text-gray-500 uppercase tracking-wider">
                Propietarios Activos ({occupants.activeOwners.length})
              </p>
              {residencePendingChange && (
                <div className="flex flex-wrap items-center gap-3 bg-indigo-50 border border-indigo-100 rounded-xl px-3 py-2">
                  <p className="text-sm font-semibold text-indigo-700 flex-1 min-w-0">
                    {(() => {
                      if (residencePendingChange.newResidesInUnit) {
                        if (occupants.activeTenant) {
                          return `¿Marcar a ${residencePendingChange.ownerName} como residente de esta unidad? Esto finalizará automáticamente el contrato de ${occupants.activeTenant.fullName}.`;
                        }
                        return `¿Marcar a ${residencePendingChange.ownerName} como residente de esta unidad?`;
                      }
                      return `¿Marcar a ${residencePendingChange.ownerName} como que ya no reside en esta unidad?`;
                    })()}
                  </p>
                  <div className="flex gap-2 shrink-0">
                    <button
                      onClick={() => setResidencePendingChange(null)}
                      className="px-3 py-1 text-xs font-semibold text-gray-600 bg-white border border-gray-200 rounded-lg hover:bg-gray-50 transition-colors"
                    >
                      Cancelar
                    </button>
                    <button
                      onClick={() =>
                        handleToggleResidence(residencePendingChange.assignmentId, residencePendingChange.newResidesInUnit)
                      }
                      disabled={togglingResidenceId === residencePendingChange.assignmentId}
                      className="px-3 py-1 text-xs font-semibold text-white bg-indigo-600 hover:bg-indigo-700 rounded-lg transition-colors disabled:opacity-50"
                    >
                      {togglingResidenceId === residencePendingChange.assignmentId ? "Guardando..." : "Confirmar"}
                    </button>
                  </div>
                </div>
              )}
              {occupants.activeOwners.map((o) => {
                const isLegal = o.ownerDocumentType === "NIT";
                return (
                  <div key={o.assignmentId} className="flex items-center gap-3">
                    <div
                      className={`w-9 h-9 rounded-full flex items-center justify-center shrink-0 ${
                        isLegal ? "bg-indigo-100 text-indigo-600" : "bg-blue-100 text-blue-600"
                      }`}
                    >
                      {isLegal ? (
                        <Building2 className="w-4 h-4" />
                      ) : (
                        <Users className="w-4 h-4" />
                      )}
                    </div>
                    <div className="flex-1 min-w-0">
                      <div className="flex flex-wrap items-center gap-1.5">
                        <p className="text-sm font-semibold text-gray-800 truncate">
                          {o.ownerName}
                        </p>
                        {o.isSpokesperson && (
                          <span className="inline-flex items-center gap-0.5 px-1.5 py-0.5 bg-amber-100 text-amber-700 text-xs font-bold rounded-full">
                            <Star className="w-3 h-3" /> Vocero
                          </span>
                        )}
                        {o.residesInUnit && (
                          <span className="px-1.5 py-0.5 bg-green-100 text-green-700 text-xs font-bold rounded-full">
                            Reside
                          </span>
                        )}
                      </div>
                      <p className="text-xs text-gray-500 mt-0.5">
                        {o.ownerDocumentType} {o.ownerDocumentNumber} · {o.ownershipPercentage}%
                      </p>
                    </div>
                    <button
                      onClick={() =>
                        setResidencePendingChange({
                          assignmentId: o.assignmentId,
                          ownerName: o.ownerName,
                          newResidesInUnit: !o.residesInUnit,
                        })
                      }
                      disabled={togglingResidenceId === o.assignmentId}
                      className={(() => {
                        const base =
                          "text-xs font-semibold px-2.5 py-1 rounded-lg border transition-colors shrink-0 disabled:opacity-50";
                        if (o.residesInUnit) {
                          return `${base} text-amber-700 bg-amber-50 border-amber-200 hover:bg-amber-100`;
                        }
                        return `${base} text-emerald-700 bg-emerald-50 border-emerald-200 hover:bg-emerald-100`;
                      })()}
                    >
                      {(() => {
                        if (togglingResidenceId === o.assignmentId) {
                          return "...";
                        }
                        if (o.residesInUnit) {
                          return "Marcar que ya no reside";
                        }
                        return "Marcar que reside";
                      })()}
                    </button>
                    <Link
                      href={`/residents/${o.ownerId}`}
                      className="text-xs font-semibold text-blue-600 hover:text-blue-800 shrink-0"
                    >
                      Ver
                    </Link>
                  </div>
                );
              })}
            </div>
          )}

          {/* Arrendatario */}
          {occupants.activeTenant && (
            <div className="px-6 py-4 space-y-3">
              <p className="text-xs font-bold text-gray-500 uppercase tracking-wider">
                Arrendatario Activo
              </p>
              <div className="flex items-center gap-3">
                <div className="w-9 h-9 rounded-full bg-emerald-100 text-emerald-600 flex items-center justify-center shrink-0">
                  <UserCheck className="w-4 h-4" />
                </div>
                <div className="flex-1 min-w-0">
                  <div className="flex flex-wrap items-center gap-1.5">
                    <p className="text-sm font-semibold text-gray-800 truncate">
                      {occupants.activeTenant.fullName}
                    </p>
                    {(() => {
                      const badge = leaseDaysLabel(occupants.activeTenant!.daysUntilLeaseExpires);
                      if (!badge) return null;
                      return (
                        <span
                          className={`inline-flex items-center gap-0.5 px-1.5 py-0.5 text-xs font-bold rounded-full ${badge.cls}`}
                        >
                          <CalendarDays className="w-3 h-3" />
                          {badge.text}
                        </span>
                      );
                    })()}
                  </div>
                  <p className="text-xs text-gray-500 mt-0.5">
                    Contrato:{" "}
                    {new Date(occupants.activeTenant.leaseStartDate).toLocaleDateString("es-CO")}
                    {occupants.activeTenant.leaseEndDate &&
                      ` → ${new Date(occupants.activeTenant.leaseEndDate).toLocaleDateString("es-CO")}`}
                  </p>
                </div>
              </div>
            </div>
          )}

          {/* Grupo de Convivencia */}
          {occupants.cohabitationMembers.length > 0 && (
            <div className="px-6 py-4 space-y-2">
              <p className="text-xs font-bold text-gray-500 uppercase tracking-wider">
                Grupo de Convivencia
              </p>
              {memberPendingRemoval && (
                <div className="flex flex-wrap items-center gap-3 bg-red-50 border border-red-100 rounded-xl px-3 py-2">
                  <p className="text-sm font-semibold text-red-700 flex-1 min-w-0">
                    ¿Quitar a {memberPendingRemoval.name} del grupo de convivencia?
                  </p>
                  <div className="flex gap-2 shrink-0">
                    <button
                      onClick={() => setMemberPendingRemoval(null)}
                      className="px-3 py-1 text-xs font-semibold text-gray-600 bg-white border border-gray-200 rounded-lg hover:bg-gray-50 transition-colors"
                    >
                      Cancelar
                    </button>
                    <button
                      onClick={() => handleRemoveMember(memberPendingRemoval.id)}
                      disabled={removingMemberId === memberPendingRemoval.id}
                      className="px-3 py-1 text-xs font-semibold text-white bg-red-600 hover:bg-red-700 rounded-lg transition-colors disabled:opacity-50"
                    >
                      {removingMemberId === memberPendingRemoval.id ? "Quitando..." : "Quitar"}
                    </button>
                  </div>
                </div>
              )}
              {humans.length > 0 && (
                <div className="flex flex-wrap gap-2">
                  {humans.map((m) => (
                    <div
                      key={m.id}
                      className="flex items-center gap-1.5 px-3 py-1.5 bg-gray-50 border border-gray-200 rounded-xl"
                    >
                      <UserCheck className="w-3.5 h-3.5 text-gray-500 shrink-0" />
                      <div>
                        <Link
                          href={`/residents/directory/${m.residentId}`}
                          className="text-xs font-semibold text-gray-800 leading-tight hover:text-blue-600 hover:underline"
                        >
                          {m.fullNameOrPetName}
                        </Link>
                        <p className="text-xs text-gray-400 leading-tight">{m.relationship}</p>
                        {m.documentNumber && (
                          <p className="text-xs text-gray-400 leading-tight">
                            {m.documentNumber}
                            {m.phone && ` · ${m.phone}`}
                          </p>
                        )}
                      </div>
                      {m.isMinor && (
                        <span className="ml-1 text-xs px-1 bg-blue-50 text-blue-600 font-bold rounded">
                          Menor
                        </span>
                      )}
                      <button
                        onClick={() => setMemberPendingRemoval({ id: m.id, name: m.fullNameOrPetName })}
                        disabled={removingMemberId === m.id}
                        className="ml-1 text-gray-400 hover:text-red-600 disabled:opacity-50 shrink-0"
                        title="Quitar del grupo de convivencia"
                      >
                        <X className="w-3.5 h-3.5" />
                      </button>
                    </div>
                  ))}
                </div>
              )}
              {pets.length > 0 && (
                <div className="flex flex-wrap gap-2 mt-2">
                  {pets.map((p) => (
                    <div
                      key={p.id}
                      className="flex items-center gap-1.5 px-3 py-1.5 bg-amber-50 border border-amber-100 rounded-xl"
                    >
                      <PawPrint className="w-3.5 h-3.5 text-amber-500 shrink-0" />
                      <div>
                        <Link
                          href={`/residents/directory/${p.residentId}`}
                          className="text-xs font-semibold text-amber-800 leading-tight hover:text-amber-900 hover:underline"
                        >
                          {p.fullNameOrPetName}
                        </Link>
                        <p className="text-xs text-amber-600 leading-tight">
                          {p.petSpecies}
                          {p.petBreed && ` · ${p.petBreed}`}
                        </p>
                      </div>
                      <button
                        onClick={() => setMemberPendingRemoval({ id: p.id, name: p.fullNameOrPetName })}
                        disabled={removingMemberId === p.id}
                        className="ml-1 text-amber-400 hover:text-red-600 disabled:opacity-50 shrink-0"
                        title="Quitar del grupo de convivencia"
                      >
                        <X className="w-3.5 h-3.5" />
                      </button>
                    </div>
                  ))}
                </div>
              )}
            </div>
          )}
        </div>
      )}

      {/* Historial de Propietarios */}
      {showHistory && (
        <div className="border-t border-gray-100 px-6 py-4 bg-gray-50/30">
          <p className="text-xs font-bold text-gray-500 uppercase tracking-wider mb-3">
            Historial de Propietarios
          </p>
          {loadingHistory ? (
            <div className="flex justify-center py-4">
              <div className="animate-spin rounded-full h-5 w-5 border-b-2 border-blue-600" />
            </div>
          ) : ownerHistory.length === 0 ? (
            <p className="text-sm text-gray-400 text-center py-3">
              Sin historial registrado.
            </p>
          ) : (
            <div className="relative border-l-2 border-gray-200 pl-4 ml-1 space-y-4">
              {ownerHistory.map((entry) => (
                <div key={entry.id} className="relative">
                  <div
                    className={`absolute w-2.5 h-2.5 rounded-full -left-[1.3rem] top-1.5 border-2 border-white ${
                      entry.endDate ? "bg-gray-400" : "bg-blue-500"
                    }`}
                  />
                  <div className="flex flex-wrap items-center gap-2">
                    <p className="text-sm font-semibold text-gray-800">{entry.ownerName}</p>
                    {!entry.endDate && (
                      <span className="px-1.5 py-0.5 bg-blue-100 text-blue-700 text-xs font-bold rounded-full">
                        Actual
                      </span>
                    )}
                  </div>
                  <p className="text-xs text-gray-500 mt-0.5">
                    {entry.ownerDocument} · {formatDate(entry.startDate)} →{" "}
                    {formatDate(entry.endDate)}
                  </p>
                  {entry.transferNotes && (
                    <p className="text-xs text-gray-400 mt-0.5 italic">
                      "{entry.transferNotes}"
                    </p>
                  )}
                </div>
              ))}
            </div>
          )}
        </div>
      )}
    </div>
  );
}
