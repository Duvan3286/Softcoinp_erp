"use client";

import React, { useEffect, useState } from "react";
import {
  ResidentsService,
  TenantResident,
  UpdateTenantResidentPayload,
} from "@/lib/residents-service";
import { useParams, useRouter } from "next/navigation";
import Link from "next/link";
import {
  Users,
  Home,
  Phone,
  Mail,
  CalendarDays,
  AlertTriangle,
  Edit2,
  Check,
  X,
} from "lucide-react";

const DOC_LABEL: Record<string, string> = {
  CitizenshipCard: "Cédula de Ciudadanía",
  ForeignerID: "Cédula de Extranjería",
  NIT: "NIT",
  Passport: "Pasaporte",
  PEP: "PEP",
  PPT: "PPT",
};

const inputClass =
  "w-full px-3 py-2.5 bg-muted/50 border border-border rounded-xl text-sm focus:border-emerald-400 focus:ring-2 focus:ring-emerald-500/20 outline-none transition-all";

const toTitleCase = (val: string): string =>
  val.toLowerCase().replace(/(?:^|\s)\S/g, (a) => a.toUpperCase());

const sanitizePhone = (val: string): string =>
  val.replace(/[^0-9\-\+\s]/g, "").slice(0, 20);

type Tab = "info" | "contract";

export default function TenantDetailPage() {
  const params = useParams();
  const router = useRouter();
  const rawId = params?.id;
  const id = Array.isArray(rawId) ? rawId[0] : rawId ?? "";

  const [tenant, setTenant] = useState<TenantResident | null>(null);
  const [loading, setLoading] = useState(true);
  const [activeTab, setActiveTab] = useState<Tab>("info");

  // Edit mode
  const [editing, setEditing] = useState(false);
  const [editEmail, setEditEmail] = useState("");
  const [editPhone, setEditPhone] = useState("");
  const [editLeaseStart, setEditLeaseStart] = useState("");
  const [editLeaseEnd, setEditLeaseEnd] = useState("");
  const [editAgentName, setEditAgentName] = useState("");
  const [editAgentPhone, setEditAgentPhone] = useState("");
  const [editAuthorizedToPay, setEditAuthorizedToPay] = useState(false);
  const [editSubmitting, setEditSubmitting] = useState(false);
  const [editError, setEditError] = useState("");

  // Deactivate
  const [showDeactivate, setShowDeactivate] = useState(false);
  const [deactivating, setDeactivating] = useState(false);
  const [deactivateError, setDeactivateError] = useState("");

  useEffect(() => {
    loadTenant();
  }, [id]);

  const loadTenant = async () => {
    setLoading(true);
    try {
      const data = await ResidentsService.getTenantDetail(id);
      setTenant(data);
    } catch {
      setTenant(null);
    } finally {
      setLoading(false);
    }
  };

  const handleStartEdit = () => {
    if (!tenant) return;
    setEditEmail(tenant.email);
    setEditPhone(tenant.phone);
    setEditLeaseStart(tenant.leaseStartDate.split("T")[0]);
    setEditLeaseEnd(tenant.leaseEndDate ? tenant.leaseEndDate.split("T")[0] : "");
    setEditAgentName(tenant.realEstateAgentName ?? "");
    setEditAgentPhone(tenant.realEstateAgentPhone ?? "");
    setEditAuthorizedToPay(tenant.authorizedToPayAdmin);
    setEditError("");
    setEditing(true);
  };

  const handleSaveEdit = async () => {
    setEditSubmitting(true);
    setEditError("");
    try {
      const payload: UpdateTenantResidentPayload = {
        email: editEmail,
        phone: editPhone,
        leaseStartDate: editLeaseStart,
        leaseEndDate: editLeaseEnd || undefined,
        realEstateAgentName: editAgentName || undefined,
        realEstateAgentPhone: editAgentPhone || undefined,
        authorizedToPayAdmin: editAuthorizedToPay,
      };
      await ResidentsService.updateTenant(id, payload);
      setEditing(false);
      await loadTenant();
    } catch (err: any) {
      const msg = err?.response?.data?.message || "Error al actualizar el arrendatario.";
      setEditError(msg);
    } finally {
      setEditSubmitting(false);
    }
  };

  const handleDeactivate = async () => {
    setDeactivating(true);
    setDeactivateError("");
    try {
      await ResidentsService.deactivateTenant(tenant!.unitId, id);
      router.push("/residents/tenants");
    } catch (err: any) {
      const msg = err?.response?.data?.message || "Error al inactivar el arrendatario.";
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

  const leaseBadge = (days?: number): { text: string; cls: string } => {
    if (days === undefined || days === null) {
      return { text: "Sin fecha de terminación", cls: "bg-muted text-muted-foreground" };
    }
    if (days < 0) {
      return {
        text: `Contrato vencido hace ${Math.abs(days)} días`,
        cls: "bg-rose-100 dark:bg-rose-950/30 text-rose-700 dark:text-rose-400",
      };
    }
    if (days === 0) {
      return { text: "Vence hoy", cls: "bg-rose-100 dark:bg-rose-950/30 text-rose-700 dark:text-rose-400" };
    }
    if (days <= 30) {
      return { text: `Vence en ${days} días`, cls: "bg-amber-100 dark:bg-amber-950/30 text-amber-700 dark:text-amber-400" };
    }
    return { text: `${days} días restantes`, cls: "bg-emerald-100 text-emerald-700" };
  };

  if (loading) {
    return (
      <div className="flex justify-center py-20">
        <div className="animate-spin rounded-full h-8 w-8 border-b-2 border-emerald-600" />
      </div>
    );
  }

  if (!tenant) {
    return (
      <div className="bg-card rounded-xl shadow-sm border border-border p-10 text-center">
        <h2 className="text-xl font-bold text-foreground mb-2">Arrendatario no encontrado</h2>
        <Link
          href="/residents/tenants"
          className="px-4 py-2 bg-emerald-600 text-white font-semibold rounded-xl"
        >
          Volver al listado
        </Link>
      </div>
    );
  }

  const docLabel = DOC_LABEL[String(tenant.documentType)] ?? String(tenant.documentType);
  const badge = leaseBadge(tenant.daysUntilLeaseExpires);

  const tabs: Array<{ key: Tab; label: string }> = [
    { key: "info", label: "Información" },
    { key: "contract", label: "Contrato" },
  ];

  return (
    <div className="space-y-6">
      {/* Header */}
      <div className="flex items-start gap-4">
        <Link
          href="/residents/tenants"
          className="w-10 h-10 flex items-center justify-center bg-card border border-border rounded-xl hover:bg-muted/30 transition-colors shadow-sm text-muted-foreground font-bold shrink-0 mt-1"
        >
          ←
        </Link>
        <div className="flex-1 flex items-start gap-4">
          <div className="w-14 h-14 rounded-2xl bg-emerald-100 flex items-center justify-center shrink-0">
            <Users className="w-7 h-7 text-emerald-600" />
          </div>
          <div className="flex-1 min-w-0">
            <div className="flex flex-wrap items-center gap-2">
              <h1 className="text-2xl font-bold text-foreground tracking-tight truncate">
                {tenant.fullName}
              </h1>
              {!tenant.isActive && (
                <span className="px-2 py-0.5 bg-rose-100 dark:bg-rose-950/30 text-rose-700 dark:text-rose-400 rounded-full text-xs font-bold">
                  Inactivo
                </span>
              )}
            </div>
            <div className="flex flex-wrap items-center gap-3 mt-1">
              <p className="text-sm text-muted-foreground">
                {docLabel} {tenant.documentNumber}
              </p>
              <Link
                href={`/units/${tenant.unitId}`}
                className="inline-flex items-center gap-1.5 px-2.5 py-1 bg-emerald-50 dark:bg-emerald-950/20 text-emerald-700 dark:text-emerald-400 rounded-lg text-xs font-bold hover:bg-emerald-100 dark:hover:bg-emerald-900/30 transition-colors"
              >
                <Home className="w-3.5 h-3.5" />
                Unidad {tenant.unitIdentifier}
              </Link>
            </div>
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
                      ? "border-emerald-600 text-emerald-600 bg-emerald-50/30"
                      : "border-transparent text-muted-foreground hover:text-muted-foreground hover:bg-muted/30"
                  }`}
                >
                  {tab.label}
                </button>
              ))}
            </div>

            {/* Tab: Info */}
            {activeTab === "info" && (
              <div className="p-6 space-y-5">
                {editing ? (
                  <div className="space-y-4">
                    <h3 className="text-sm font-bold text-muted-foreground uppercase tracking-wider">
                      Editando información de contacto
                    </h3>
                    <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
                      <div className="md:col-span-2">
                        <label className="block text-xs font-bold text-muted-foreground mb-1.5 uppercase tracking-wide">
                          Correo Electrónico *
                        </label>
                        <input
                          type="email"
                          required
                          maxLength={256}
                          value={editEmail}
                          onChange={(e) => setEditEmail(e.target.value)}
                          className={inputClass}
                        />
                      </div>
                      <div>
                        <label className="block text-xs font-bold text-muted-foreground mb-1.5 uppercase tracking-wide">
                          Teléfono *
                        </label>
                        <input
                          type="tel"
                          required
                          value={editPhone}
                          onChange={(e) => setEditPhone(sanitizePhone(e.target.value))}
                          className={inputClass}
                        />
                      </div>
                    </div>
                    {editError && (
                      <p className="text-sm text-rose-600 dark:text-rose-400 font-semibold bg-rose-50 dark:bg-rose-950/20 border border-rose-200 dark:border-rose-900 rounded-xl px-3 py-2">
                        {editError}
                      </p>
                    )}
                    <div className="flex gap-2 pt-2">
                      <button
                        onClick={() => setEditing(false)}
                        className="flex items-center gap-1.5 px-4 py-2 bg-card border border-border text-muted-foreground rounded-xl text-sm font-semibold hover:bg-muted/30 transition-colors"
                      >
                        <X className="w-4 h-4" />
                        Cancelar
                      </button>
                      <button
                        onClick={handleSaveEdit}
                        disabled={editSubmitting}
                        className="flex items-center gap-1.5 px-4 py-2 bg-emerald-600 hover:bg-emerald-700 text-white rounded-xl text-sm font-semibold disabled:opacity-50 transition-colors"
                      >
                        {editSubmitting ? (
                          <div className="w-4 h-4 rounded-full border-2 border-white/30 border-t-white animate-spin" />
                        ) : (
                          <Check className="w-4 h-4" />
                        )}
                        Guardar
                      </button>
                    </div>
                  </div>
                ) : (
                  <div className="space-y-5">
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
                          <p className="text-sm font-semibold text-foreground mt-0.5">
                            {tenant.email}
                          </p>
                        </div>
                      </div>
                      <div className="flex items-start gap-3">
                        <Phone className="w-5 h-5 text-muted-foreground mt-0.5 shrink-0" />
                        <div>
                          <p className="text-xs font-bold text-muted-foreground uppercase tracking-wider">
                            Teléfono
                          </p>
                          <p className="text-sm font-semibold text-foreground mt-0.5">
                            {tenant.phone}
                          </p>
                        </div>
                      </div>
                    </div>
                  </div>
                )}
              </div>
            )}

            {/* Tab: Contrato */}
            {activeTab === "contract" && (
              <div className="p-6 space-y-6">
                {/* Lease dates badge */}
                <div className={`inline-flex items-center gap-2 px-3 py-2 rounded-xl text-sm font-bold ${badge.cls}`}>
                  <CalendarDays className="w-4 h-4" />
                  {badge.text}
                </div>

                {editing ? (
                  <div className="space-y-4">
                    <h3 className="text-sm font-bold text-muted-foreground uppercase tracking-wider">
                      Editando contrato
                    </h3>
                    <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
                      <div>
                        <label className="block text-xs font-bold text-muted-foreground mb-1.5 uppercase tracking-wide">
                          Fecha de Inicio *
                        </label>
                        <input
                          type="date"
                          value={editLeaseStart}
                          onChange={(e) => setEditLeaseStart(e.target.value)}
                          className={inputClass}
                        />
                      </div>
                      <div>
                        <label className="block text-xs font-bold text-muted-foreground mb-1.5 uppercase tracking-wide">
                          Fecha de Terminación
                        </label>
                        <input
                          type="date"
                          value={editLeaseEnd}
                          onChange={(e) => setEditLeaseEnd(e.target.value)}
                          min={editLeaseStart}
                          className={inputClass}
                        />
                      </div>
                      <div>
                        <label className="block text-xs font-bold text-muted-foreground mb-1.5 uppercase tracking-wide">
                          Intermediario / Inmobiliaria
                        </label>
                        <input
                          type="text"
                          maxLength={200}
                          value={editAgentName}
                          onChange={(e) => setEditAgentName(toTitleCase(e.target.value))}
                          className={inputClass}
                        />
                      </div>
                      <div>
                        <label className="block text-xs font-bold text-muted-foreground mb-1.5 uppercase tracking-wide">
                          Teléfono Intermediario
                        </label>
                        <input
                          type="tel"
                          value={editAgentPhone}
                          onChange={(e) => setEditAgentPhone(sanitizePhone(e.target.value))}
                          className={inputClass}
                        />
                      </div>
                      <div className="md:col-span-2">
                        <label className="flex items-center gap-3 cursor-pointer select-none">
                          <div
                            onClick={() => setEditAuthorizedToPay(!editAuthorizedToPay)}
                            className={`w-10 h-6 rounded-full transition-colors flex items-center px-1 ${
                              editAuthorizedToPay ? "bg-emerald-500" : "bg-muted"
                            }`}
                          >
                            <div
                              className={`w-4 h-4 bg-card rounded-full shadow transition-transform ${
                                editAuthorizedToPay ? "translate-x-4" : "translate-x-0"
                              }`}
                            />
                          </div>
                          <span className="text-sm font-semibold text-muted-foreground">
                            Autorizado a pagar administración
                          </span>
                        </label>
                      </div>
                    </div>
                    {editError && (
                      <p className="text-sm text-rose-600 dark:text-rose-400 font-semibold bg-rose-50 dark:bg-rose-950/20 border border-rose-200 dark:border-rose-900 rounded-xl px-3 py-2">
                        {editError}
                      </p>
                    )}
                    <div className="flex gap-2 pt-2">
                      <button
                        onClick={() => setEditing(false)}
                        className="flex items-center gap-1.5 px-4 py-2 bg-card border border-border text-muted-foreground rounded-xl text-sm font-semibold hover:bg-muted/30 transition-colors"
                      >
                        <X className="w-4 h-4" />
                        Cancelar
                      </button>
                      <button
                        onClick={handleSaveEdit}
                        disabled={editSubmitting}
                        className="flex items-center gap-1.5 px-4 py-2 bg-emerald-600 hover:bg-emerald-700 text-white rounded-xl text-sm font-semibold disabled:opacity-50 transition-colors"
                      >
                        {editSubmitting ? (
                          <div className="w-4 h-4 rounded-full border-2 border-white/30 border-t-white animate-spin" />
                        ) : (
                          <Check className="w-4 h-4" />
                        )}
                        Guardar
                      </button>
                    </div>
                  </div>
                ) : (
                  <div className="grid grid-cols-1 md:grid-cols-2 gap-5">
                    <div>
                      <p className="text-xs font-bold text-muted-foreground uppercase tracking-wider">
                        Inicio del Contrato
                      </p>
                      <p className="text-sm font-semibold text-foreground mt-1">
                        {formatDate(tenant.leaseStartDate)}
                      </p>
                    </div>
                    <div>
                      <p className="text-xs font-bold text-muted-foreground uppercase tracking-wider">
                        Terminación del Contrato
                      </p>
                      <p className="text-sm font-semibold text-foreground mt-1">
                        {formatDate(tenant.leaseEndDate)}
                      </p>
                    </div>
                    {tenant.realEstateAgentName && (
                      <div>
                        <p className="text-xs font-bold text-muted-foreground uppercase tracking-wider">
                          Intermediario
                        </p>
                        <p className="text-sm font-semibold text-foreground mt-1">
                          {tenant.realEstateAgentName}
                        </p>
                      </div>
                    )}
                    {tenant.realEstateAgentPhone && (
                      <div>
                        <p className="text-xs font-bold text-muted-foreground uppercase tracking-wider">
                          Teléfono Intermediario
                        </p>
                        <p className="text-sm font-semibold text-foreground mt-1">
                          {tenant.realEstateAgentPhone}
                        </p>
                      </div>
                    )}
                    <div className="md:col-span-2">
                      <p className="text-xs font-bold text-muted-foreground uppercase tracking-wider">
                        Pago de Administración
                      </p>
                      <span
                        className={`inline-block mt-1 px-2.5 py-1 rounded-full text-xs font-bold ${
                          tenant.authorizedToPayAdmin
                            ? "bg-emerald-100 text-emerald-700"
                            : "bg-muted text-muted-foreground"
                        }`}
                      >
                        {tenant.authorizedToPayAdmin
                          ? "Autorizado a pagar"
                          : "No autorizado a pagar"}
                      </span>
                    </div>
                  </div>
                )}
              </div>
            )}
          </div>

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
              {tenant.isActive && !editing && (
                <button
                  onClick={handleStartEdit}
                  className="w-full flex items-center gap-3 px-4 py-3 rounded-xl bg-emerald-50 dark:bg-emerald-950/20 text-emerald-700 dark:text-emerald-400 hover:bg-emerald-100 dark:hover:bg-emerald-900/30 font-semibold text-sm transition-colors border border-emerald-100 dark:border-emerald-900"
                >
                  <Edit2 className="w-5 h-5" />
                  Editar Información
                </button>
              )}

              {tenant.isActive && (
                <button
                  onClick={() => setShowDeactivate(true)}
                  className="w-full flex items-center gap-3 px-4 py-3 rounded-xl bg-rose-50 dark:bg-rose-950/20 text-rose-700 dark:text-rose-400 hover:bg-rose-100 dark:bg-rose-950/30 font-semibold text-sm transition-colors border border-rose-100 dark:border-rose-900"
                >
                  <AlertTriangle className="w-5 h-5" />
                  Finalizar Contrato
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
                <p className="text-xs font-bold text-muted-foreground uppercase tracking-wider">Estado</p>
                <span
                  className={`inline-block mt-0.5 px-2 py-0.5 rounded-full text-xs font-bold ${
                    tenant.isActive
                      ? "bg-emerald-100 text-emerald-700"
                      : "bg-rose-100 dark:bg-rose-950/30 text-rose-700 dark:text-rose-400"
                  }`}
                >
                  {tenant.isActive ? "Activo" : "Inactivo"}
                </span>
              </div>
              <div>
                <p className="text-xs font-bold text-muted-foreground uppercase tracking-wider">
                  Unidad
                </p>
                <Link
                  href={`/units/${tenant.unitId}`}
                  className="inline-flex items-center gap-1.5 mt-0.5 px-2.5 py-1 bg-emerald-50 dark:bg-emerald-950/20 text-emerald-700 dark:text-emerald-400 rounded-lg text-xs font-bold hover:bg-emerald-100 dark:hover:bg-emerald-900/30 transition-colors"
                >
                  <Home className="w-3.5 h-3.5" />
                  {tenant.unitIdentifier}
                </Link>
              </div>
            </div>
          </div>

          {/* Deactivate panel */}
          {showDeactivate && (
            <div className="bg-card rounded-2xl shadow-sm border border-rose-200 dark:border-rose-900 overflow-hidden">
              <div className="px-5 py-4 border-b border-rose-100 dark:border-rose-900 bg-rose-50 dark:bg-rose-950/20">
                <h3 className="font-bold text-rose-700 dark:text-rose-400 text-sm flex items-center gap-2">
                  <AlertTriangle className="w-4 h-4" />
                  Confirmar Finalización
                </h3>
              </div>
              <div className="p-5 space-y-4">
                <p className="text-sm text-muted-foreground">
                  Se registrará la salida del arrendatario de la unidad{" "}
                  <strong>{tenant.unitIdentifier}</strong>. Esta acción no se puede deshacer.
                </p>
                {deactivateError && (
                  <p className="text-sm text-rose-600 dark:text-rose-400 font-semibold">{deactivateError}</p>
                )}
                <div className="flex gap-2">
                  <button
                    onClick={() => {
                      setShowDeactivate(false);
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
                    {deactivating ? "..." : "Finalizar"}
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
