"use client";

import React, { useEffect, useState } from "react";
import {
  ResidentsService,
  Owner,
  ContactHistoryEntry,
  UnitOwnerSummary,
  AssignOwnerToUnitPayload,
} from "@/lib/residents-service";
import { UnitsService, Unit, formatUnitLabel } from "@/lib/units-service";
import { useParams, useRouter } from "next/navigation";
import Link from "next/link";
import {
  Users,
  Building2,
  Phone,
  Mail,
  Home,
  Plus,
  Star,
  Clock,
  ArrowRightLeft,
  History,
  AlertTriangle,
  X,
  Check,
} from "lucide-react";

const DOC_LABEL: Record<string, string> = {
  CitizenshipCard: "Cédula de Ciudadanía",
  ForeignerID: "Cédula de Extranjería",
  NIT: "NIT",
  Passport: "Pasaporte",
  PEP: "PEP",
  PPT: "PPT",
};

type Tab = "info" | "units" | "contact-history";

export default function OwnerDetailPage() {
  const params = useParams();
  const router = useRouter();
  const rawId = params?.id;
  const id = Array.isArray(rawId) ? rawId[0] : rawId ?? "";

  const [owner, setOwner] = useState<Owner | null>(null);
  const [contactHistory, setContactHistory] = useState<ContactHistoryEntry[]>([]);
  const [loading, setLoading] = useState(true);
  const [loadingHistory, setLoadingHistory] = useState(false);
  const [activeTab, setActiveTab] = useState<Tab>("info");
  const [deactivating, setDeactivating] = useState(false);
  const [showDeactivate, setShowDeactivate] = useState(false);
  const [deactivateReason, setDeactivateReason] = useState("");
  const [deactivateError, setDeactivateError] = useState("");

  // Assign unit form state
  const [showAssignUnit, setShowAssignUnit] = useState(false);
  const [availableUnits, setAvailableUnits] = useState<Unit[]>([]);
  const [loadingUnits, setLoadingUnits] = useState(false);
  const [assignUnitId, setAssignUnitId] = useState("");
  const [assignPercentage, setAssignPercentage] = useState("100");
  const [assignSpokesperson, setAssignSpokesperson] = useState(false);
  const [assignResides, setAssignResides] = useState(false);
  const [assignStartDate, setAssignStartDate] = useState(
    new Date().toISOString().split("T")[0]
  );
  const [assignSubmitting, setAssignSubmitting] = useState(false);
  const [assignError, setAssignError] = useState("");

  useEffect(() => {
    loadOwner();
  }, [id]);

  useEffect(() => {
    if (activeTab === "contact-history" && contactHistory.length === 0) {
      loadContactHistory();
    }
  }, [activeTab]);

  const loadOwner = async () => {
    setLoading(true);
    try {
      const data = await ResidentsService.getOwnerDetail(id);
      setOwner(data);
    } catch {
      setOwner(null);
    } finally {
      setLoading(false);
    }
  };

  const loadContactHistory = async () => {
    setLoadingHistory(true);
    try {
      const data = await ResidentsService.getOwnerContactHistory(id);
      setContactHistory(data);
    } catch {
      setContactHistory([]);
    } finally {
      setLoadingHistory(false);
    }
  };

  const handleOpenAssignUnit = async () => {
    setShowAssignUnit(true);
    setAssignError("");
    if (availableUnits.length > 0) return;
    setLoadingUnits(true);
    try {
      const units = await UnitsService.getUnits();
      setAvailableUnits(units);
      if (units.length > 0) setAssignUnitId(units[0].id);
    } catch {
      setAssignError("No se pudieron cargar las unidades.");
    } finally {
      setLoadingUnits(false);
    }
  };

  const handleAssignUnit = async () => {
    if (!assignUnitId) {
      setAssignError("Selecciona una unidad.");
      return;
    }
    const pct = parseFloat(assignPercentage);
    if (isNaN(pct) || pct <= 0 || pct > 100) {
      setAssignError("El porcentaje debe estar entre 0.01 y 100.");
      return;
    }
    setAssignSubmitting(true);
    setAssignError("");
    try {
      const payload: AssignOwnerToUnitPayload = {
        ownerId: id,
        ownershipPercentage: pct,
        isSpokesperson: assignSpokesperson,
        residesInUnit: assignResides,
        startDate: assignStartDate,
      };
      await ResidentsService.assignOwnerToUnit(assignUnitId, payload);
      setShowAssignUnit(false);
      setAssignUnitId("");
      setAssignPercentage("100");
      setAssignSpokesperson(false);
      setAssignResides(false);
      setAssignStartDate(new Date().toISOString().split("T")[0]);
      await loadOwner();
    } catch (err: any) {
      const msg =
        err?.response?.data?.message || "Error al vincular la unidad.";
      setAssignError(msg);
    } finally {
      setAssignSubmitting(false);
    }
  };

  const handleDeactivate = async () => {
    if (!deactivateReason.trim()) {
      setDeactivateError("El motivo es obligatorio.");
      return;
    }
    setDeactivating(true);
    setDeactivateError("");
    try {
      await ResidentsService.deactivateOwner(id, new Date().toISOString(), deactivateReason);
      router.push("/residents");
    } catch (err: any) {
      const msg = err?.response?.data?.message || "Error al inactivar el propietario.";
      setDeactivateError(msg);
    } finally {
      setDeactivating(false);
    }
  };

  const formatDate = (dateStr?: string) => {
    if (!dateStr) return "—";
    return new Date(dateStr).toLocaleDateString("es-CO", {
      year: "numeric",
      month: "long",
      day: "numeric",
    });
  };

  const formatDateTime = (dateStr?: string) => {
    if (!dateStr) return "—";
    return new Date(dateStr).toLocaleString("es-CO", {
      year: "numeric",
      month: "short",
      day: "numeric",
      hour: "2-digit",
      minute: "2-digit",
    });
  };

  if (loading) {
    return (
      <div className="flex justify-center py-20">
        <div className="animate-spin rounded-full h-8 w-8 border-b-2 border-emerald-600" />
      </div>
    );
  }

  if (!owner) {
    return (
      <div className="bg-card rounded-xl shadow-sm border border-border p-10 text-center">
        <h2 className="text-xl font-bold text-foreground mb-2">Propietario no encontrado</h2>
        <Link
          href="/residents"
          className="px-4 py-2 bg-emerald-600 hover:bg-emerald-700 text-white font-semibold rounded-xl transition-colors"
        >
          Volver al listado
        </Link>
      </div>
    );
  }

  const isLegal = String(owner.ownerType) === "LegalEntity";
  const docLabel = DOC_LABEL[String(owner.documentType)] ?? String(owner.documentType);
  const activeUnits = (owner.units ?? []).filter((u) => !u.endDate);

  const tabs: Array<{ key: Tab; label: string; icon: React.ReactNode }> = [
    { key: "info", label: "Información", icon: <Users className="w-4 h-4" /> },
    { key: "units", label: `Unidades (${activeUnits.length})`, icon: <Home className="w-4 h-4" /> },
    { key: "contact-history", label: "Historial de Cambios", icon: <History className="w-4 h-4" /> },
  ];

  return (
    <div className="space-y-6">
      {/* Header */}
      <div className="flex items-start gap-4">
        <Link
          href="/residents"
          className="w-10 h-10 flex items-center justify-center bg-card border border-border rounded-xl hover:bg-muted/30 transition-colors shadow-sm text-muted-foreground font-bold shrink-0 mt-1"
        >
          ←
        </Link>
        <div className="flex-1 flex items-start gap-4">
          <div
            className={`w-14 h-14 rounded-2xl flex items-center justify-center shrink-0 ${
              isLegal ? "bg-indigo-100 dark:bg-indigo-950/30" : "bg-blue-100 dark:bg-blue-950/30"
            }`}
          >
            {isLegal ? (
              <Building2 className="w-7 h-7 text-indigo-600 dark:text-indigo-400" />
            ) : (
              <Users className="w-7 h-7 text-blue-600 dark:text-blue-400" />
            )}
          </div>
          <div className="flex-1 min-w-0">
            <div className="flex flex-wrap items-center gap-2">
              <h1 className="text-2xl font-bold text-foreground tracking-tight truncate">
                {owner.fullNameOrCompanyName}
              </h1>
              {!owner.isActive && (
                <span className="px-2 py-0.5 bg-rose-100 dark:bg-rose-950/30 text-rose-700 dark:text-rose-400 rounded-full text-xs font-bold">
                  Inactivo
                </span>
              )}
            </div>
            <p className="text-sm text-muted-foreground mt-0.5">
              {docLabel}{" "}
              {owner.documentNumber}
              {owner.verificationDigit ? `-${owner.verificationDigit}` : ""} ·{" "}
              {isLegal ? "Persona Jurídica" : "Persona Natural"}
            </p>
          </div>
        </div>
      </div>

      <div className="grid grid-cols-1 lg:grid-cols-3 gap-6">
        {/* Main content */}
        <div className="lg:col-span-2 space-y-0">
          {/* Tabs */}
          <div className="bg-card rounded-t-2xl border border-border shadow-sm overflow-hidden">
            <div className="flex border-b border-border overflow-x-auto">
              {tabs.map((tab) => (
                <button
                  key={tab.key}
                  onClick={() => setActiveTab(tab.key)}
                  className={`flex items-center gap-2 px-5 py-4 text-sm font-semibold whitespace-nowrap transition-colors border-b-2 ${
                    activeTab === tab.key
                      ? "border-emerald-600 text-emerald-600 dark:text-emerald-400 bg-emerald-50 dark:bg-emerald-950/20"
                      : "border-transparent text-muted-foreground hover:text-muted-foreground hover:bg-muted/30"
                  }`}
                >
                  {tab.icon}
                  {tab.label}
                </button>
              ))}
            </div>

            {/* Tab: Info */}
            {activeTab === "info" && (
              <div className="divide-y divide-border">
                <div className="p-6 space-y-5">
                  <h3 className="text-sm font-bold text-muted-foreground uppercase tracking-wider">
                    Contacto
                  </h3>
                  <div className="grid grid-cols-1 md:grid-cols-2 gap-5">
                    <div className="flex items-start gap-3">
                      <Mail className="w-5 h-5 text-muted-foreground mt-0.5 shrink-0" />
                      <div>
                        <p className="text-xs font-bold text-muted-foreground uppercase tracking-wider">
                          Correo
                        </p>
                        <p className="text-sm font-semibold text-foreground mt-0.5">{owner.email}</p>
                      </div>
                    </div>
                    <div className="flex items-start gap-3">
                      <Phone className="w-5 h-5 text-muted-foreground mt-0.5 shrink-0" />
                      <div>
                        <p className="text-xs font-bold text-muted-foreground uppercase tracking-wider">
                          Teléfono Principal
                        </p>
                        <p className="text-sm font-semibold text-foreground mt-0.5">
                          {owner.mainPhone}
                        </p>
                      </div>
                    </div>
                    {owner.alternativePhone && (
                      <div className="flex items-start gap-3">
                        <Phone className="w-5 h-5 text-muted-foreground mt-0.5 shrink-0" />
                        <div>
                          <p className="text-xs font-bold text-muted-foreground uppercase tracking-wider">
                            Teléfono Alternativo
                          </p>
                          <p className="text-sm font-semibold text-foreground mt-0.5">
                            {owner.alternativePhone}
                          </p>
                        </div>
                      </div>
                    )}
                    {owner.correspondenceAddress && (
                      <div className="flex items-start gap-3">
                        <Home className="w-5 h-5 text-muted-foreground mt-0.5 shrink-0" />
                        <div>
                          <p className="text-xs font-bold text-muted-foreground uppercase tracking-wider">
                            Dirección de Correspondencia
                          </p>
                          <p className="text-sm font-semibold text-foreground mt-0.5">
                            {owner.correspondenceAddress}
                          </p>
                        </div>
                      </div>
                    )}
                  </div>
                </div>

                {isLegal ? (
                  <div className="p-6 space-y-5">
                    <h3 className="text-sm font-bold text-muted-foreground uppercase tracking-wider">
                      Representación Legal
                    </h3>
                    <div className="grid grid-cols-1 md:grid-cols-2 gap-5">
                      <div>
                        <p className="text-xs font-bold text-muted-foreground uppercase tracking-wider">
                          Nombre
                        </p>
                        <p className="text-sm font-semibold text-foreground mt-1">
                          {owner.legalRepresentativeName || "—"}
                        </p>
                      </div>
                      <div>
                        <p className="text-xs font-bold text-muted-foreground uppercase tracking-wider">
                          Documento
                        </p>
                        <p className="text-sm font-semibold text-foreground mt-1">
                          {owner.legalRepresentativeDocumentType && (
                            <span className="text-muted-foreground mr-1">
                              {owner.legalRepresentativeDocumentType}
                            </span>
                          )}
                          {owner.legalRepresentativeDocument || "—"}
                        </p>
                      </div>
                      <div>
                        <p className="text-xs font-bold text-muted-foreground uppercase tracking-wider">
                          Cargo
                        </p>
                        <p className="text-sm font-semibold text-foreground mt-1">
                          {owner.legalRepresentativeRole || "—"}
                        </p>
                      </div>
                      <div>
                        <p className="text-xs font-bold text-muted-foreground uppercase tracking-wider">
                          Vigencia del Poder
                        </p>
                        <p className="text-sm font-semibold text-foreground mt-1">
                          {formatDate(owner.powerOfAttorneyExpiration)}
                        </p>
                      </div>
                    </div>
                  </div>
                ) : (
                  <div className="p-6 space-y-5">
                    <h3 className="text-sm font-bold text-muted-foreground uppercase tracking-wider">
                      Datos Personales
                    </h3>
                    <div className="grid grid-cols-1 md:grid-cols-2 gap-5">
                      <div>
                        <p className="text-xs font-bold text-muted-foreground uppercase tracking-wider">
                          Fecha de Nacimiento
                        </p>
                        <p className="text-sm font-semibold text-foreground mt-1">
                          {formatDate(owner.dateOfBirth)}
                        </p>
                      </div>
                      <div>
                        <p className="text-xs font-bold text-muted-foreground uppercase tracking-wider">
                          Estado Civil
                        </p>
                        <p className="text-sm font-semibold text-foreground mt-1">
                          {owner.civilStatus || "—"}
                        </p>
                      </div>
                    </div>
                  </div>
                )}
              </div>
            )}

            {/* Tab: Unidades */}
            {activeTab === "units" && (
              <div className="divide-y divide-border">
                {(owner.units ?? []).length === 0 ? (
                  <div className="p-8 text-center">
                    <Home className="w-10 h-10 mx-auto text-muted-foreground/40 mb-3" />
                    <p className="text-sm font-semibold text-muted-foreground">
                      Sin unidades vinculadas
                    </p>
                    <p className="text-xs text-muted-foreground mt-1">
                      Usa "Vincular a Unidad" para asignarlo.
                    </p>
                  </div>
                ) : (
                  (owner.units ?? []).map((u: UnitOwnerSummary) => (
                    <div key={u.assignmentId} className="px-6 py-4 flex items-center gap-4">
                      <div className="w-10 h-10 rounded-xl bg-muted flex items-center justify-center shrink-0">
                        <Home className="w-5 h-5 text-muted-foreground" />
                      </div>
                      <div className="flex-1 min-w-0">
                        <div className="flex flex-wrap items-center gap-2">
                          <p className="text-sm font-bold text-foreground">
                            {(() => {
                              if (u.unitTowerOrBlock) {
                                return `${u.unitTowerOrBlock} ${u.unitTypeName} ${u.unitIdentifier}`;
                              }
                              return `${u.unitTypeName} ${u.unitIdentifier}`;
                            })()}
                          </p>
                          {u.isSpokesperson && (
                            <span className="inline-flex items-center gap-1 px-2 py-0.5 bg-amber-100 dark:bg-amber-950/30 text-amber-700 dark:text-amber-400 text-xs font-bold rounded-full border border-amber-200 dark:border-amber-900">
                              <Star className="w-3 h-3" /> Vocero
                            </span>
                          )}
                          {u.residesInUnit && (
                            <span className="px-2 py-0.5 bg-emerald-100 dark:bg-emerald-950/30 text-emerald-700 dark:text-emerald-400 text-xs font-bold rounded-full border border-emerald-200 dark:border-emerald-900">
                              Reside
                            </span>
                          )}
                          {u.endDate && (
                            <span className="px-2 py-0.5 bg-muted text-muted-foreground text-xs font-bold rounded-full">
                              Finalizado
                            </span>
                          )}
                        </div>
                        <p className="text-xs text-muted-foreground mt-0.5">
                          {u.ownershipPercentage}% · Desde {formatDate(u.startDate)}
                          {u.endDate && ` → ${formatDate(u.endDate)}`}
                        </p>
                      </div>
                      <div className="flex gap-2 shrink-0">
                        <Link
                          href={`/units/${u.unitId}`}
                          className="text-xs font-semibold text-emerald-600 dark:text-emerald-400 bg-emerald-50 dark:bg-emerald-950/20 px-3 py-1.5 rounded-lg hover:bg-emerald-100 dark:hover:bg-emerald-900/30 transition-colors"
                        >
                          Ver Unidad
                        </Link>
                        {!u.endDate && (
                          <Link
                            href={`/residents/transfer/${u.unitId}`}
                            className="text-xs font-semibold text-muted-foreground bg-muted/50 border border-border px-3 py-1.5 rounded-lg hover:bg-muted transition-colors"
                          >
                            Transferir
                          </Link>
                        )}
                      </div>
                    </div>
                  ))
                )}
              </div>
            )}

            {/* Tab: Historial de Cambios */}
            {activeTab === "contact-history" && (
              <div className="p-6">
                {loadingHistory ? (
                  <div className="flex justify-center py-8">
                    <div className="animate-spin rounded-full h-6 w-6 border-b-2 border-emerald-600" />
                  </div>
                ) : contactHistory.length === 0 ? (
                  <div className="text-center py-8">
                    <Clock className="w-10 h-10 mx-auto text-muted-foreground/40 mb-3" />
                    <p className="text-sm font-semibold text-muted-foreground">
                      Sin cambios registrados aún
                    </p>
                    <p className="text-xs text-muted-foreground mt-1">
                      Cada modificación de datos quedará registrada aquí.
                    </p>
                  </div>
                ) : (
                  <div className="relative border-l-2 border-border pl-5 ml-2 space-y-5">
                    {contactHistory.map((entry) => (
                      <div key={entry.id} className="relative">
                        <div className="absolute w-3 h-3 bg-emerald-400 rounded-full -left-[1.45rem] top-1 border-2 border-card" />
                        <p className="text-xs text-muted-foreground">{formatDateTime(entry.changedAt)}</p>
                        <p className="text-sm font-semibold text-foreground mt-0.5">
                          Campo modificado:{" "}
                          <span className="font-bold text-foreground">{entry.fieldChanged}</span>
                        </p>
                        {entry.oldValue !== null && entry.newValue !== null && (
                          <div className="mt-1 flex items-center gap-2 flex-wrap">
                            <span className="text-xs px-2 py-0.5 bg-rose-50 dark:bg-rose-950/20 text-rose-700 dark:text-rose-400 rounded line-through font-mono">
                              {entry.oldValue || "—"}
                            </span>
                            <span className="text-muted-foreground text-xs">→</span>
                            <span className="text-xs px-2 py-0.5 bg-emerald-50 dark:bg-emerald-950/20 text-emerald-700 dark:text-emerald-400 rounded font-mono font-semibold">
                              {entry.newValue || "—"}
                            </span>
                          </div>
                        )}
                        <p className="text-xs text-muted-foreground mt-0.5">
                          Por: {entry.changedByUserId}
                        </p>
                      </div>
                    ))}
                  </div>
                )}
              </div>
            )}
          </div>

          {/* Rounded bottom */}
          <div className="bg-card rounded-b-2xl border border-t-0 border-border shadow-sm h-2" />
        </div>

        {/* Sidebar */}
        <div className="space-y-5">
          {/* Acciones */}
          <div className="bg-card rounded-2xl shadow-sm border border-border overflow-hidden">
            <div className="px-5 py-4 border-b border-border bg-muted/50">
              <h3 className="font-bold text-foreground text-sm">Acciones</h3>
            </div>
            <div className="p-4 space-y-2">
              <button
                onClick={handleOpenAssignUnit}
                className="w-full flex items-center gap-3 px-4 py-3 rounded-xl bg-emerald-50 dark:bg-emerald-950/20 text-emerald-700 dark:text-emerald-400 hover:bg-emerald-100 dark:hover:bg-emerald-900/30 font-semibold text-sm transition-colors border border-emerald-100 dark:border-emerald-900"
              >
                <Plus className="w-5 h-5" />
                Vincular a Unidad
              </button>

              {activeUnits.length > 0 && (
                <Link
                  href={`/residents/transfer/${activeUnits[0].unitId}`}
                  className="w-full flex items-center gap-3 px-4 py-3 rounded-xl bg-amber-50 text-amber-700 dark:text-amber-400 hover:bg-amber-100 dark:bg-amber-950/30 font-semibold text-sm transition-colors border border-amber-100"
                >
                  <ArrowRightLeft className="w-5 h-5" />
                  Transferir Propiedad
                </Link>
              )}

              {owner.isActive && (
                <button
                  onClick={() => setShowDeactivate(true)}
                  className="w-full flex items-center gap-3 px-4 py-3 rounded-xl bg-rose-50 dark:bg-rose-950/20 text-rose-700 dark:text-rose-400 hover:bg-rose-100 dark:bg-rose-950/30 font-semibold text-sm transition-colors border border-rose-100 dark:border-rose-900"
                >
                  <AlertTriangle className="w-5 h-5" />
                  Inactivar Propietario
                </button>
              )}
            </div>
          </div>

          {/* Metadata */}
          <div className="bg-card rounded-2xl shadow-sm border border-border overflow-hidden">
            <div className="px-5 py-4 border-b border-border bg-muted/50">
              <h3 className="font-bold text-foreground text-sm">Registro</h3>
            </div>
            <div className="p-5 space-y-3">
              <div>
                <p className="text-xs font-bold text-muted-foreground uppercase tracking-wider">
                  Creado en el sistema
                </p>
                <p className="text-sm font-semibold text-foreground mt-0.5">
                  {formatDate(owner.createdAt)}
                </p>
              </div>
              <div>
                <p className="text-xs font-bold text-muted-foreground uppercase tracking-wider">Estado</p>
                <span
                  className={`inline-block mt-0.5 px-2 py-0.5 rounded-full text-xs font-bold ${
                    owner.isActive
                      ? "bg-emerald-100 text-emerald-700"
                      : "bg-rose-100 dark:bg-rose-950/30 text-rose-700 dark:text-rose-400"
                  }`}
                >
                  {owner.isActive ? "Activo" : "Inactivo"}
                </span>
              </div>
            </div>
          </div>

          {/* Assign Unit Panel */}
          {showAssignUnit && (
            <div className="bg-card rounded-2xl shadow-sm border border-emerald-200 dark:border-emerald-900 overflow-hidden">
              <div className="px-5 py-4 border-b border-emerald-100 dark:border-emerald-900 bg-emerald-50 dark:bg-emerald-950/20 flex items-center justify-between">
                <h3 className="font-bold text-emerald-800 dark:text-emerald-300 text-sm flex items-center gap-2">
                  <Plus className="w-4 h-4" />
                  Vincular a Unidad
                </h3>
                <button
                  onClick={() => {
                    setShowAssignUnit(false);
                    setAssignError("");
                  }}
                  className="text-muted-foreground hover:text-muted-foreground transition-colors"
                >
                  <X className="w-4 h-4" />
                </button>
              </div>
              <div className="p-5 space-y-4">
                {loadingUnits ? (
                  <div className="flex justify-center py-4">
                    <div className="animate-spin rounded-full h-5 w-5 border-b-2 border-emerald-600" />
                  </div>
                ) : (
                  <>
                    <div>
                      <label className="block text-xs font-bold text-muted-foreground mb-1.5 uppercase tracking-wide">
                        Unidad *
                      </label>
                      <select
                        value={assignUnitId}
                        onChange={(e) => setAssignUnitId(e.target.value)}
                        className="w-full px-3 py-2.5 bg-muted/50 border border-border rounded-xl text-sm focus:border-emerald-400 focus:ring-2 focus:ring-emerald-500/20 outline-none"
                      >
                        <option value="">Seleccione una unidad...</option>
                        {availableUnits.map((u) => (
                          <option key={u.id} value={u.id}>
                            {formatUnitLabel(u.identifier, u.towerOrBlock)}
                          </option>
                        ))}
                      </select>
                    </div>

                    <div>
                      <label className="block text-xs font-bold text-muted-foreground mb-1.5 uppercase tracking-wide">
                        % Copropiedad *
                      </label>
                      <input
                        type="number"
                        min="0.01"
                        max="100"
                        step="0.01"
                        value={assignPercentage}
                        onChange={(e) => setAssignPercentage(e.target.value)}
                        className="w-full px-3 py-2.5 bg-muted/50 border border-border rounded-xl text-sm focus:border-emerald-400 focus:ring-2 focus:ring-emerald-500/20 outline-none"
                      />
                    </div>

                    <div>
                      <label className="block text-xs font-bold text-muted-foreground mb-1.5 uppercase tracking-wide">
                        Fecha de Inicio *
                      </label>
                      <input
                        type="date"
                        value={assignStartDate}
                        onChange={(e) => setAssignStartDate(e.target.value)}
                        className="w-full px-3 py-2.5 bg-muted/50 border border-border rounded-xl text-sm focus:border-emerald-400 focus:ring-2 focus:ring-emerald-500/20 outline-none"
                      />
                    </div>

                    <div className="space-y-2">
                      <label className="flex items-center gap-3 cursor-pointer select-none">
                        <div
                          onClick={() => setAssignSpokesperson(!assignSpokesperson)}
                          className={`w-10 h-6 rounded-full transition-colors flex items-center px-1 ${
                            assignSpokesperson ? "bg-amber-500" : "bg-muted"
                          }`}
                        >
                          <div
                            className={`w-4 h-4 bg-card rounded-full shadow transition-transform ${
                              assignSpokesperson ? "translate-x-4" : "translate-x-0"
                            }`}
                          />
                        </div>
                        <span className="text-sm font-semibold text-muted-foreground">
                          Es vocero de la unidad
                        </span>
                      </label>

                      <label className="flex items-center gap-3 cursor-pointer select-none">
                        <div
                          onClick={() => setAssignResides(!assignResides)}
                          className={`w-10 h-6 rounded-full transition-colors flex items-center px-1 ${
                            assignResides ? "bg-emerald-500" : "bg-muted"
                          }`}
                        >
                          <div
                            className={`w-4 h-4 bg-card rounded-full shadow transition-transform ${
                              assignResides ? "translate-x-4" : "translate-x-0"
                            }`}
                          />
                        </div>
                        <span className="text-sm font-semibold text-muted-foreground">
                          Reside en la unidad
                        </span>
                      </label>
                    </div>

                    {assignError && (
                      <p className="text-sm text-rose-600 dark:text-rose-400 font-semibold bg-rose-50 dark:bg-rose-950/20 border border-rose-200 dark:border-rose-900 rounded-xl px-3 py-2">
                        {assignError}
                      </p>
                    )}

                    <button
                      onClick={handleAssignUnit}
                      disabled={assignSubmitting || !assignUnitId}
                      className="w-full flex items-center justify-center gap-2 py-2.5 px-4 bg-emerald-600 hover:bg-emerald-700 text-white font-bold rounded-xl text-sm disabled:opacity-50 transition-colors"
                    >
                      {assignSubmitting ? (
                        <div className="w-4 h-4 rounded-full border-2 border-white/30 border-t-white animate-spin" />
                      ) : (
                        <Check className="w-4 h-4" />
                      )}
                      Confirmar Vinculación
                    </button>
                  </>
                )}
              </div>
            </div>
          )}

          {/* Deactivate Modal */}
          {showDeactivate && (
            <div className="bg-card rounded-2xl shadow-sm border border-rose-200 dark:border-rose-900 overflow-hidden">
              <div className="px-5 py-4 border-b border-rose-100 dark:border-rose-900 bg-rose-50 dark:bg-rose-950/20">
                <h3 className="font-bold text-rose-700 dark:text-rose-400 text-sm flex items-center gap-2">
                  <AlertTriangle className="w-4 h-4" />
                  Confirmar Inactivación
                </h3>
              </div>
              <div className="p-5 space-y-4">
                <p className="text-sm text-muted-foreground">
                  El propietario no podrá seguir vinculado a unidades activas. Esta acción
                  queda registrada en el historial.
                </p>
                <div>
                  <label className="block text-xs font-bold text-muted-foreground mb-1.5">
                    Motivo *
                  </label>
                  <textarea
                    value={deactivateReason}
                    onChange={(e) => setDeactivateReason(e.target.value)}
                    rows={3}
                    className="w-full px-3 py-2.5 bg-muted/50 border border-border rounded-xl text-sm focus:border-red-400 focus:ring-2 focus:ring-red-500/20 outline-none resize-none"
                    placeholder="Ej: Venta de la unidad, fallecimiento, etc."
                  />
                </div>
                {deactivateError && (
                  <p className="text-sm text-rose-600 dark:text-rose-400 font-semibold">{deactivateError}</p>
                )}
                <div className="flex gap-2">
                  <button
                    onClick={() => {
                      setShowDeactivate(false);
                      setDeactivateReason("");
                      setDeactivateError("");
                    }}
                    className="flex-1 py-2 px-3 bg-card border border-border text-muted-foreground rounded-xl text-sm font-semibold hover:bg-muted/30 transition-colors"
                  >
                    Cancelar
                  </button>
                  <button
                    onClick={handleDeactivate}
                    disabled={deactivating}
                    className="flex-1 py-2 px-3 bg-red-600 hover:bg-red-700 text-white rounded-xl text-sm font-semibold disabled:opacity-50 transition-colors"
                  >
                    {deactivating ? "..." : "Inactivar"}
                  </button>
                </div>
              </div>
            </div>
          )}
        </div>
      </div>
    </div>
  );
}
