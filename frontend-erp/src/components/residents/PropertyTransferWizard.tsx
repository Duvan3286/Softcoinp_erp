"use client";

import React, { useEffect, useState } from "react";
import {
  ResidentsService,
  UnitOccupants,
  OwnerSummary,
  DocumentType,
  TransferPropertyPayload,
  TransferPropertyResult,
  CreateNaturalPersonOwnerPayload,
  CreateLegalEntityOwnerPayload,
} from "@/lib/residents-service";

// ── Form helpers ─────────────────────────────────────────────────────────────

const toTitleCase = (val: string): string =>
  val.toLowerCase().replace(/(?:^|\s)\S/g, (a) => a.toUpperCase());

const sanitizePhone = (val: string): string =>
  val.replace(/[^0-9\-\+\s]/g, "").slice(0, 20);

const sanitizeDocNumber = (val: string, dt: DocumentType): string => {
  const isNumeric = dt === DocumentType.CitizenshipCard || dt === DocumentType.NIT;
  const maxLen = isNumeric ? 10 : 50;
  return isNumeric
    ? val.replace(/\D/g, "").slice(0, maxLen)
    : val.replace(/[^a-zA-Z0-9]/g, "").slice(0, maxLen);
};

const calculateDV = (nitVal: string): string => {
  if (nitVal.length === 0) return "";
  const vpri = [3, 7, 13, 17, 19, 23, 29, 37, 41, 43, 47, 53, 59, 67, 71];
  let x = 0;
  let y = 0;
  const z = nitVal.length;
  for (let i = 0; i < z; i++) {
    y = parseInt(nitVal.charAt(i));
    x += y * vpri[z - 1 - i];
  }
  y = x % 11;
  return y > 1 ? (11 - y).toString() : y.toString();
};

// ── STEP INDICATOR ────────────────────────────────────────────────────────────

const STEPS = [
  { number: 1, label: "Estado Actual" },
  { number: 2, label: "Verificar Deudas" },
  { number: 3, label: "Nuevo Propietario" },
  { number: 4, label: "Detalles" },
  { number: 5, label: "Confirmar" },
];

function StepIndicator({ current }: { current: number }) {
  return (
    <div className="flex items-center justify-between mb-8 px-2">
      {STEPS.map((step, idx) => {
        const isCompleted = current > step.number;
        const isActive = current === step.number;
        return (
          <React.Fragment key={step.number}>
            <div className="flex flex-col items-center gap-1.5">
              <div
                className={`w-9 h-9 rounded-full flex items-center justify-center text-sm font-bold border-2 transition-all ${
                  isCompleted
                    ? "bg-emerald-500 border-emerald-500 text-white"
                    : isActive
                    ? "bg-white border-blue-600 text-blue-600"
                    : "bg-white border-gray-200 text-gray-400"
                }`}
              >
                {isCompleted ? "✓" : step.number}
              </div>
              <span
                className={`text-xs font-semibold hidden sm:block ${
                  isActive ? "text-blue-600" : isCompleted ? "text-emerald-600" : "text-gray-400"
                }`}
              >
                {step.label}
              </span>
            </div>
            {idx < STEPS.length - 1 && (
              <div
                className={`flex-1 h-0.5 mx-2 transition-all ${
                  current > step.number ? "bg-emerald-400" : "bg-gray-200"
                }`}
              />
            )}
          </React.Fragment>
        );
      })}
    </div>
  );
}

// ── STEP 1: ESTADO ACTUAL ─────────────────────────────────────────────────────

function Step1CurrentState({
  occupants,
  loading,
}: {
  occupants: UnitOccupants | null;
  loading: boolean;
}) {
  if (loading) {
    return (
      <div className="flex justify-center py-16">
        <div className="animate-spin rounded-full h-8 w-8 border-b-2 border-blue-600" />
      </div>
    );
  }

  if (!occupants) {
    return (
      <div className="bg-red-50 border border-red-200 rounded-xl p-5 text-red-700 text-sm">
        No se pudo cargar la información de la unidad.
      </div>
    );
  }

  const hasOwners = occupants.activeOwners.length > 0;

  return (
    <div className="space-y-5">
      <div className="bg-blue-50 border border-blue-200 rounded-xl p-4">
        <p className="text-sm font-semibold text-blue-800">
          Unidad: {occupants.unitIdentifier}
        </p>
        <p className="text-xs text-blue-600 mt-1">
          Este proceso registrará formalmente la transferencia de propiedad en el historial inmutable del conjunto.
        </p>
      </div>

      <div>
        <h4 className="text-sm font-bold text-gray-700 uppercase tracking-wider mb-3">
          Propietarios Actuales ({occupants.activeOwners.length})
        </h4>
        {hasOwners ? (
          <div className="space-y-2">
            {occupants.activeOwners.map((o) => (
              <div
                key={o.assignmentId}
                className="flex items-center justify-between bg-white border border-gray-200 rounded-xl px-4 py-3"
              >
                <div className="flex items-center gap-3">
                  <div className="w-8 h-8 rounded-full bg-gray-100 flex items-center justify-center text-gray-500 font-bold text-sm">
                    P
                  </div>
                  <div>
                    <p className="text-sm font-semibold text-gray-800">
                      {o.ownerName || "Propietario"}
                    </p>
                    <p className="text-xs text-gray-500">
                      {o.ownerDocumentType} {o.ownerDocumentNumber} · {o.ownershipPercentage}%
                    </p>
                  </div>
                </div>
                <div className="flex gap-2">
                  {o.isSpokesperson && (
                    <span className="px-2 py-0.5 bg-amber-100 text-amber-700 rounded-full text-xs font-semibold">
                      Vocero
                    </span>
                  )}
                </div>
              </div>
            ))}
          </div>
        ) : (
          <div className="bg-amber-50 border border-amber-200 rounded-xl p-4">
            <p className="text-sm font-semibold text-amber-800">⚠ Sin propietarios registrados</p>
            <p className="text-xs text-amber-600 mt-1">
              Debe haber al menos un propietario activo para realizar una transferencia.
            </p>
          </div>
        )}
      </div>

      {occupants.activeTenant && (
        <div>
          <h4 className="text-sm font-bold text-gray-700 uppercase tracking-wider mb-3">
            Arrendatario Activo
          </h4>
          <div className="bg-white border border-gray-200 rounded-xl px-4 py-3">
            <p className="text-sm font-semibold text-gray-800">{occupants.activeTenant.fullName}</p>
            <p className="text-xs text-gray-500">
              Contrato: {new Date(occupants.activeTenant.leaseStartDate).toLocaleDateString("es-CO")}
              {occupants.activeTenant.leaseEndDate &&
                ` → ${new Date(occupants.activeTenant.leaseEndDate).toLocaleDateString("es-CO")}`}
            </p>
          </div>
        </div>
      )}

      {!hasOwners && (
        <div className="bg-red-50 border border-red-200 rounded-xl p-4 text-sm text-red-700">
          No es posible continuar con la transferencia: la unidad no tiene propietarios activos.
        </div>
      )}
    </div>
  );
}

// ── STEP 2: VERIFICACIÓN DE DEUDAS ────────────────────────────────────────────

function Step2DebtVerification({
  debtConfirmed,
  onToggle,
}: {
  debtConfirmed: boolean;
  onToggle: () => void;
}) {
  return (
    <div className="space-y-5">
      <div className="bg-amber-50 border border-amber-200 rounded-xl p-4">
        <p className="text-sm font-bold text-amber-800 mb-1">
          Verificación de Deudas Pendientes
        </p>
        <p className="text-xs text-amber-700">
          El módulo financiero completará esta verificación automáticamente. Mientras tanto,
          el administrador debe confirmar manualmente que la unidad está al día.
        </p>
      </div>

      <div className="bg-white border border-gray-200 rounded-xl p-5 space-y-4">
        <div className="flex items-start gap-4">
          <div className="w-10 h-10 bg-blue-50 rounded-xl flex items-center justify-center shrink-0 text-blue-600 font-bold">
            $
          </div>
          <div>
            <p className="text-sm font-semibold text-gray-800">Estado de Cuenta</p>
            <p className="text-xs text-gray-500 mt-0.5">
              Módulo financiero pendiente de integración. El saldo real se mostrará aquí
              automáticamente una vez disponible.
            </p>
          </div>
          <span className="ml-auto px-2 py-1 bg-gray-100 text-gray-500 rounded-lg text-xs font-semibold shrink-0">
            Pendiente
          </span>
        </div>

        <div className="border-t border-gray-100 pt-4">
          <p className="text-xs font-bold text-gray-500 uppercase tracking-wider mb-3">
            Deudas Verificadas Manualmente
          </p>
          <div className="space-y-2 text-sm text-gray-600">
            <div className="flex justify-between">
              <span>Cuotas de administración atrasadas</span>
              <span className="font-semibold text-gray-400 italic text-xs">Sin datos</span>
            </div>
            <div className="flex justify-between">
              <span>Multas y sanciones pendientes</span>
              <span className="font-semibold text-gray-400 italic text-xs">Sin datos</span>
            </div>
            <div className="flex justify-between">
              <span>Cuotas extraordinarias</span>
              <span className="font-semibold text-gray-400 italic text-xs">Sin datos</span>
            </div>
          </div>
        </div>
      </div>

      <div className="bg-white border-2 border-dashed border-gray-200 rounded-xl p-4">
        <label className="flex items-start gap-3 cursor-pointer">
          <input
            type="checkbox"
            checked={debtConfirmed}
            onChange={onToggle}
            className="mt-0.5 w-5 h-5 rounded border-gray-300 text-blue-600 cursor-pointer"
          />
          <div>
            <p className="text-sm font-semibold text-gray-800">
              Confirmo que he verificado el estado de cuenta de esta unidad
            </p>
            <p className="text-xs text-gray-500 mt-1">
              Al marcar esta casilla, el administrador declara haber revisado el estado financiero
              y confirma que la unidad no presenta deudas activas, o que las partes han acordado
              su situación mediante paz y salvo.
            </p>
          </div>
        </label>
      </div>
    </div>
  );
}

// ── STEP 3: NUEVO PROPIETARIO ─────────────────────────────────────────────────

type OwnerSelectionMode = "search" | "create-natural" | "create-legal";

interface NewOwnerFormState {
  mode: OwnerSelectionMode;
  selectedOwnerId: string;
  selectedOwnerName: string;
  search: string;
  searchResults: OwnerSummary[];
  searching: boolean;
  // Create Natural Person
  docType: DocumentType;
  docNumber: string;
  fullName: string;
  email: string;
  mainPhone: string;
  dateOfBirth: string;
  // Create Legal Entity
  nit: string;
  dv: string;
  companyName: string;
  legalRepName: string;
  legalRepDocType: DocumentType;
  legalRepDoc: string;
  legalRepRole: string;
  companyEmail: string;
  companyPhone: string;
  creating: boolean;
  createError: string;
}

function Step3NewOwner({
  state,
  onChange,
  onCreated,
}: {
  state: NewOwnerFormState;
  onChange: (partial: Partial<NewOwnerFormState>) => void;
  onCreated: (id: string, name: string) => void;
}) {
  const handleSearch = async () => {
    if (!state.search.trim()) return;
    onChange({ searching: true });
    try {
      const results = await ResidentsService.getOwners(state.search);
      onChange({ searchResults: results, searching: false });
    } catch {
      onChange({ searching: false });
    }
  };

  const handleCreateNatural = async () => {
    onChange({ creating: true, createError: "" });
    try {
      const payload: CreateNaturalPersonOwnerPayload = {
        documentType: state.docType,
        documentNumber: state.docNumber,
        fullName: state.fullName,
        email: state.email,
        mainPhone: state.mainPhone,
        dateOfBirth: state.dateOfBirth || undefined,
      };
      const result = await ResidentsService.createNaturalPersonOwner(payload);
      onCreated(result.id, result.fullNameOrCompanyName);
      onChange({ creating: false });
    } catch (err: any) {
      const msg = err?.response?.data?.message || "Error al crear el propietario.";
      onChange({ creating: false, createError: msg });
    }
  };

  const handleCreateLegal = async () => {
    onChange({ creating: true, createError: "" });
    try {
      const payload: CreateLegalEntityOwnerPayload = {
        documentNumber: state.nit,
        verificationDigit: state.dv,
        companyName: state.companyName,
        email: state.companyEmail,
        mainPhone: state.companyPhone,
        legalRepresentativeName: state.legalRepName,
        legalRepresentativeDocumentType: state.legalRepDocType,
        legalRepresentativeDocument: state.legalRepDoc,
        legalRepresentativeRole: state.legalRepRole,
      };
      const result = await ResidentsService.createLegalEntityOwner(payload);
      onCreated(result.id, result.fullNameOrCompanyName);
      onChange({ creating: false });
    } catch (err: any) {
      const msg = err?.response?.data?.message || "Error al crear la empresa.";
      onChange({ creating: false, createError: msg });
    }
  };

  const inputClass =
    "w-full px-4 py-2.5 bg-gray-50 border border-gray-200 rounded-xl text-sm text-gray-900 focus:bg-white focus:ring-2 focus:ring-blue-500/20 focus:border-blue-500 transition-all outline-none";
  const labelClass = "block text-xs font-semibold text-gray-600 mb-1.5";

  return (
    <div className="space-y-5">
      {state.selectedOwnerId && (
        <div className="flex items-center gap-3 bg-emerald-50 border border-emerald-200 rounded-xl px-4 py-3">
          <div className="w-8 h-8 rounded-full bg-emerald-500 flex items-center justify-center text-white font-bold text-sm shrink-0">
            ✓
          </div>
          <div>
            <p className="text-sm font-semibold text-emerald-800">{state.selectedOwnerName}</p>
            <p className="text-xs text-emerald-600">Seleccionado como nuevo propietario</p>
          </div>
          <button
            onClick={() => onChange({ selectedOwnerId: "", selectedOwnerName: "" })}
            className="ml-auto text-xs font-semibold text-emerald-700 hover:text-emerald-900"
          >
            Cambiar
          </button>
        </div>
      )}

      {!state.selectedOwnerId && (
        <>
          <div className="flex gap-2">
            {(["search", "create-natural", "create-legal"] as OwnerSelectionMode[]).map((m) => {
              const labels: Record<OwnerSelectionMode, string> = {
                search: "Buscar existente",
                "create-natural": "Persona Natural",
                "create-legal": "Persona Jurídica",
              };
              return (
                <button
                  key={m}
                  onClick={() => onChange({ mode: m })}
                  className={`flex-1 py-2 px-3 rounded-xl text-xs font-semibold border transition-all ${
                    state.mode === m
                      ? "bg-blue-600 border-blue-600 text-white"
                      : "bg-white border-gray-200 text-gray-600 hover:bg-gray-50"
                  }`}
                >
                  {labels[m]}
                </button>
              );
            })}
          </div>

          {state.mode === "search" && (
            <div className="space-y-3">
              <div className="flex gap-2">
                <input
                  type="text"
                  placeholder="Nombre, documento o email..."
                  value={state.search}
                  onChange={(e) => onChange({ search: e.target.value })}
                  onKeyDown={(e) => { if (e.key === "Enter") handleSearch(); }}
                  className={inputClass}
                />
                <button
                  onClick={handleSearch}
                  disabled={state.searching}
                  className="px-4 py-2.5 bg-blue-600 text-white rounded-xl text-sm font-semibold hover:bg-blue-700 disabled:opacity-50 transition-all shrink-0"
                >
                  {state.searching ? "..." : "Buscar"}
                </button>
              </div>

              {state.searchResults.length > 0 && (
                <div className="space-y-2 max-h-60 overflow-y-auto">
                  {state.searchResults.map((owner) => (
                    <button
                      key={owner.id}
                      onClick={() => onCreated(owner.id, owner.fullNameOrCompanyName)}
                      className="w-full text-left bg-white border border-gray-200 rounded-xl px-4 py-3 hover:border-blue-400 hover:bg-blue-50/50 transition-all"
                    >
                      <div className="flex items-center justify-between">
                        <div>
                          <p className="text-sm font-semibold text-gray-800">{owner.fullNameOrCompanyName}</p>
                          <p className="text-xs text-gray-500 mt-0.5">
                            {owner.documentType} · {owner.documentNumber}
                          </p>
                        </div>
                        <span className="text-xs px-2 py-0.5 bg-gray-100 text-gray-600 rounded-full">
                          {owner.ownerType === "NaturalPerson" ? "Natural" : "Jurídica"}
                        </span>
                      </div>
                    </button>
                  ))}
                </div>
              )}

              {state.searchResults.length === 0 && state.search && !state.searching && (
                <p className="text-sm text-gray-500 text-center py-4">
                  Sin resultados. Prueba crear un nuevo propietario.
                </p>
              )}
            </div>
          )}

          {state.mode === "create-natural" && (
            <div className="space-y-4">
              {state.createError && (
                <div className="bg-red-50 border border-red-200 rounded-xl p-3 text-sm text-red-700">
                  {state.createError}
                </div>
              )}
              <div className="grid grid-cols-1 sm:grid-cols-2 gap-4">
                <div>
                  <label className={labelClass}>Tipo de Documento</label>
                  <select
                    value={state.docType}
                    onChange={(e) =>
                      onChange({ docType: Number(e.target.value) as DocumentType, docNumber: "" })
                    }
                    className={inputClass}
                  >
                    {[DocumentType.CitizenshipCard, DocumentType.ForeignerID, DocumentType.Passport, DocumentType.PEP, DocumentType.PPT].map((dt) => (
                      <option key={dt} value={dt}>{DOCUMENT_TYPE_LABELS_SHORT[dt]}</option>
                    ))}
                  </select>
                </div>
                <div>
                  <label className={labelClass}>Número de Documento *</label>
                  <input
                    type="text"
                    inputMode={state.docType === DocumentType.CitizenshipCard ? "numeric" : "text"}
                    value={state.docNumber}
                    onChange={(e) =>
                      onChange({ docNumber: sanitizeDocNumber(e.target.value, state.docType) })
                    }
                    className={inputClass}
                    placeholder={state.docType === DocumentType.CitizenshipCard ? "Ej: 1234567890" : "Ej: AB123456"}
                  />
                </div>
                <div className="sm:col-span-2">
                  <label className={labelClass}>Nombre Completo *</label>
                  <input
                    type="text"
                    maxLength={200}
                    value={state.fullName}
                    onChange={(e) => onChange({ fullName: toTitleCase(e.target.value) })}
                    className={inputClass}
                    placeholder="Ej: Juan Carlos Pérez Gómez"
                  />
                </div>
                <div>
                  <label className={labelClass}>Correo Electrónico *</label>
                  <input
                    type="email"
                    maxLength={256}
                    value={state.email}
                    onChange={(e) => onChange({ email: e.target.value })}
                    className={inputClass}
                    placeholder="correo@ejemplo.com"
                  />
                </div>
                <div>
                  <label className={labelClass}>Teléfono Principal *</label>
                  <input
                    type="tel"
                    value={state.mainPhone}
                    onChange={(e) => onChange({ mainPhone: sanitizePhone(e.target.value) })}
                    className={inputClass}
                    placeholder="Ej: 3001234567"
                  />
                </div>
                <div>
                  <label className={labelClass}>Fecha de Nacimiento</label>
                  <input
                    type="date"
                    value={state.dateOfBirth}
                    onChange={(e) => onChange({ dateOfBirth: e.target.value })}
                    className={inputClass}
                  />
                </div>
              </div>

              <button
                onClick={handleCreateNatural}
                disabled={state.creating || !state.docNumber || !state.fullName || !state.email || !state.mainPhone}
                className="w-full py-2.5 bg-emerald-600 hover:bg-emerald-700 text-white rounded-xl text-sm font-semibold disabled:opacity-50 transition-all"
              >
                {state.creating ? "Creando..." : "Crear y Seleccionar Propietario"}
              </button>
            </div>
          )}

          {state.mode === "create-legal" && (
            <div className="space-y-4">
              {state.createError && (
                <div className="bg-red-50 border border-red-200 rounded-xl p-3 text-sm text-red-700">
                  {state.createError}
                </div>
              )}
              <div className="grid grid-cols-1 sm:grid-cols-2 gap-4">
                <div>
                  <label className={labelClass}>NIT (Sin DV) *</label>
                  <input
                    type="text"
                    inputMode="numeric"
                    value={state.nit}
                    onChange={(e) => {
                      const cleaned = e.target.value.replace(/\D/g, "").slice(0, 10);
                      onChange({ nit: cleaned, dv: calculateDV(cleaned) });
                    }}
                    className={inputClass}
                    placeholder="Ej: 900123456"
                  />
                </div>
                <div>
                  <label className={labelClass}>DV</label>
                  <input
                    type="text"
                    value={state.dv}
                    readOnly
                    className="w-full px-4 py-2.5 bg-gray-100 border border-gray-200 rounded-xl text-sm text-gray-500 font-bold text-center cursor-not-allowed outline-none"
                  />
                </div>
                <div className="sm:col-span-2">
                  <label className={labelClass}>Razón Social *</label>
                  <input
                    type="text"
                    maxLength={200}
                    value={state.companyName}
                    onChange={(e) => onChange({ companyName: toTitleCase(e.target.value) })}
                    className={inputClass}
                    placeholder="Ej: Inversiones Xyz S.A.S."
                  />
                </div>
                <div>
                  <label className={labelClass}>Correo Corporativo *</label>
                  <input
                    type="email"
                    maxLength={256}
                    value={state.companyEmail}
                    onChange={(e) => onChange({ companyEmail: e.target.value })}
                    className={inputClass}
                  />
                </div>
                <div>
                  <label className={labelClass}>Teléfono *</label>
                  <input
                    type="tel"
                    value={state.companyPhone}
                    onChange={(e) => onChange({ companyPhone: sanitizePhone(e.target.value) })}
                    className={inputClass}
                  />
                </div>
                <div className="sm:col-span-2 border-t border-gray-100 pt-4">
                  <p className="text-xs font-bold text-gray-500 uppercase tracking-wider mb-3">
                    Representante Legal
                  </p>
                </div>
                <div className="sm:col-span-2">
                  <label className={labelClass}>Nombre Completo *</label>
                  <input
                    type="text"
                    maxLength={200}
                    value={state.legalRepName}
                    onChange={(e) => onChange({ legalRepName: toTitleCase(e.target.value) })}
                    className={inputClass}
                  />
                </div>
                <div>
                  <label className={labelClass}>Tipo de Documento *</label>
                  <select
                    value={state.legalRepDocType}
                    onChange={(e) =>
                      onChange({ legalRepDocType: Number(e.target.value) as DocumentType, legalRepDoc: "" })
                    }
                    className={inputClass}
                  >
                    {[DocumentType.CitizenshipCard, DocumentType.ForeignerID, DocumentType.Passport].map((dt) => (
                      <option key={dt} value={dt}>{DOCUMENT_TYPE_LABELS_SHORT[dt]}</option>
                    ))}
                  </select>
                </div>
                <div>
                  <label className={labelClass}>Número de Documento *</label>
                  <input
                    type="text"
                    inputMode={state.legalRepDocType === DocumentType.CitizenshipCard ? "numeric" : "text"}
                    value={state.legalRepDoc}
                    onChange={(e) =>
                      onChange({ legalRepDoc: sanitizeDocNumber(e.target.value, state.legalRepDocType) })
                    }
                    className={inputClass}
                  />
                </div>
                <div>
                  <label className={labelClass}>Cargo *</label>
                  <input
                    type="text"
                    maxLength={100}
                    value={state.legalRepRole}
                    onChange={(e) => onChange({ legalRepRole: toTitleCase(e.target.value) })}
                    className={inputClass}
                    placeholder="Ej: Gerente General"
                  />
                </div>
              </div>

              <button
                onClick={handleCreateLegal}
                disabled={
                  state.creating ||
                  !state.nit || !state.dv || !state.companyName ||
                  !state.companyEmail || !state.companyPhone ||
                  !state.legalRepName || !state.legalRepDoc || !state.legalRepRole
                }
                className="w-full py-2.5 bg-emerald-600 hover:bg-emerald-700 text-white rounded-xl text-sm font-semibold disabled:opacity-50 transition-all"
              >
                {state.creating ? "Creando..." : "Crear y Seleccionar Empresa"}
              </button>
            </div>
          )}
        </>
      )}
    </div>
  );
}

const DOCUMENT_TYPE_LABELS_SHORT: Record<DocumentType, string> = {
  [DocumentType.CitizenshipCard]: "Cédula de Ciudadanía (CC)",
  [DocumentType.ForeignerID]: "Cédula de Extranjería (CE)",
  [DocumentType.NIT]: "NIT",
  [DocumentType.Passport]: "Pasaporte",
  [DocumentType.PEP]: "Persona Expuesta Políticamente (PEP)",
  [DocumentType.PPT]: "Pasaporte Temporal (PPT)",
};

// ── STEP 4: DETALLES DE TRANSFERENCIA ─────────────────────────────────────────

interface TransferDetails {
  transferDate: string;
  ownershipPercentage: string;
  isSpokesperson: boolean;
  residesInUnit: boolean;
  transferNotes: string;
  generatePazYSalvo: boolean;
}

function Step4Details({
  details,
  onChange,
  hasMultipleOwners,
}: {
  details: TransferDetails;
  onChange: (partial: Partial<TransferDetails>) => void;
  hasMultipleOwners: boolean;
}) {
  const inputClass =
    "w-full px-4 py-2.5 bg-gray-50 border border-gray-200 rounded-xl text-sm text-gray-900 focus:bg-white focus:ring-2 focus:ring-blue-500/20 focus:border-blue-500 transition-all outline-none";
  const labelClass = "block text-xs font-semibold text-gray-600 mb-1.5";

  const pct = parseFloat(details.ownershipPercentage);
  const pctValid = !isNaN(pct) && pct > 0 && pct <= 100;

  return (
    <div className="space-y-5">
      <div className="grid grid-cols-1 sm:grid-cols-2 gap-5">
        <div>
          <label className={labelClass}>Fecha de Transferencia *</label>
          <input
            type="date"
            value={details.transferDate}
            onChange={(e) => onChange({ transferDate: e.target.value })}
            max={new Date().toISOString().split("T")[0]}
            className={inputClass}
          />
          <p className="text-xs text-gray-400 mt-1">
            Fecha en que se formalizó la escritura pública.
          </p>
        </div>

        <div>
          <label className={labelClass}>Porcentaje de Copropiedad *</label>
          <div className="relative">
            <input
              type="number"
              value={details.ownershipPercentage}
              onChange={(e) => onChange({ ownershipPercentage: e.target.value })}
              min="0.01"
              max="100"
              step="0.01"
              className={`${inputClass} pr-10 ${!pctValid && details.ownershipPercentage ? "border-red-300" : ""}`}
              placeholder="100"
            />
            <span className="absolute right-4 top-1/2 -translate-y-1/2 text-gray-500 text-sm font-semibold">%</span>
          </div>
          {!pctValid && details.ownershipPercentage && (
            <p className="text-xs text-red-500 mt-1">Debe ser entre 0.01 y 100.</p>
          )}
          <p className="text-xs text-gray-400 mt-1">
            En copropiedad puede ser menos de 100%.
          </p>
        </div>
      </div>

      <div className="space-y-3">
        <label className="flex items-start gap-3 cursor-pointer bg-white border border-gray-200 rounded-xl p-4">
          <input
            type="checkbox"
            checked={details.isSpokesperson}
            onChange={(e) => onChange({ isSpokesperson: e.target.checked })}
            className="mt-0.5 w-4 h-4 rounded border-gray-300 text-blue-600 cursor-pointer"
          />
          <div>
            <p className="text-sm font-semibold text-gray-800">Designar como Vocero</p>
            <p className="text-xs text-gray-500 mt-0.5">
              El vocero es quien recibe notificaciones del conjunto y representa a los propietarios en asamblea.
              {hasMultipleOwners && " El vocero anterior perderá esta designación."}
            </p>
          </div>
        </label>

        <label className="flex items-start gap-3 cursor-pointer bg-white border border-gray-200 rounded-xl p-4">
          <input
            type="checkbox"
            checked={details.residesInUnit}
            onChange={(e) => onChange({ residesInUnit: e.target.checked })}
            className="mt-0.5 w-4 h-4 rounded border-gray-300 text-blue-600 cursor-pointer"
          />
          <div>
            <p className="text-sm font-semibold text-gray-800">Reside en la Unidad</p>
            <p className="text-xs text-gray-500 mt-0.5">
              El nuevo propietario habitará físicamente la unidad.
            </p>
          </div>
        </label>

        <label className="flex items-start gap-3 cursor-pointer bg-white border border-gray-200 rounded-xl p-4">
          <input
            type="checkbox"
            checked={details.generatePazYSalvo}
            onChange={(e) => onChange({ generatePazYSalvo: e.target.checked })}
            className="mt-0.5 w-4 h-4 rounded border-gray-300 text-blue-600 cursor-pointer"
          />
          <div>
            <p className="text-sm font-semibold text-gray-800">Generar Paz y Salvo</p>
            <p className="text-xs text-gray-500 mt-0.5">
              Certifica que la unidad estaba al día al momento de la venta.
              Disponible una vez el módulo financiero esté activo.
            </p>
          </div>
        </label>
      </div>

      <div>
        <label className={labelClass}>Notas de Transferencia</label>
        <textarea
          value={details.transferNotes}
          onChange={(e) => onChange({ transferNotes: e.target.value })}
          rows={3}
          maxLength={1000}
          placeholder="Escritura pública N°..., Notaría..., observaciones relevantes..."
          className={`${inputClass} resize-none`}
        />
        <p className="text-xs text-gray-400 mt-1 text-right">{details.transferNotes.length}/1000</p>
      </div>
    </div>
  );
}

// ── STEP 5: CONFIRMAR ─────────────────────────────────────────────────────────

function Step5Confirm({
  unitIdentifier,
  newOwnerName,
  details,
  submitting,
  submitError,
}: {
  unitIdentifier: string;
  newOwnerName: string;
  details: TransferDetails;
  submitting: boolean;
  submitError: string;
}) {
  const rows: Array<{ label: string; value: string }> = [
    { label: "Unidad", value: unitIdentifier },
    { label: "Nuevo Propietario", value: newOwnerName },
    { label: "Fecha de Transferencia", value: new Date(details.transferDate + "T12:00:00").toLocaleDateString("es-CO", { year: "numeric", month: "long", day: "numeric" }) },
    { label: "Porcentaje de Copropiedad", value: `${details.ownershipPercentage}%` },
    { label: "Designado como Vocero", value: details.isSpokesperson ? "Sí" : "No" },
    { label: "Reside en la Unidad", value: details.residesInUnit ? "Sí" : "No" },
    { label: "Generar Paz y Salvo", value: details.generatePazYSalvo ? "Sí (pendiente módulo financiero)" : "No" },
  ];

  return (
    <div className="space-y-5">
      <div className="bg-blue-50 border border-blue-200 rounded-xl p-4">
        <p className="text-sm font-bold text-blue-800 mb-1">Resumen de la Transferencia</p>
        <p className="text-xs text-blue-600">
          Revise cuidadosamente los datos. Esta operación quedará registrada en el historial
          inmutable del conjunto y no puede deshacerse.
        </p>
      </div>

      <div className="bg-white border border-gray-200 rounded-xl overflow-hidden">
        {rows.map((row, idx) => (
          <div
            key={row.label}
            className={`flex justify-between items-center px-5 py-3 text-sm ${
              idx < rows.length - 1 ? "border-b border-gray-100" : ""
            }`}
          >
            <span className="text-gray-500 font-medium">{row.label}</span>
            <span className="font-semibold text-gray-900 text-right max-w-[60%]">{row.value}</span>
          </div>
        ))}
      </div>

      {details.transferNotes && (
        <div className="bg-gray-50 border border-gray-200 rounded-xl p-4">
          <p className="text-xs font-bold text-gray-500 uppercase tracking-wider mb-1.5">Notas</p>
          <p className="text-sm text-gray-700">{details.transferNotes}</p>
        </div>
      )}

      {submitError && (
        <div className="bg-red-50 border border-red-200 rounded-xl p-4 text-sm text-red-700">
          {submitError}
        </div>
      )}

      {submitting && (
        <div className="flex items-center gap-3 bg-blue-50 border border-blue-200 rounded-xl p-4">
          <div className="animate-spin rounded-full h-5 w-5 border-b-2 border-blue-600" />
          <p className="text-sm font-semibold text-blue-700">Registrando transferencia...</p>
        </div>
      )}
    </div>
  );
}

// ── SUCCESS SCREEN ────────────────────────────────────────────────────────────

function SuccessScreen({
  result,
  unitIdentifier,
  onViewHistory,
}: {
  result: TransferPropertyResult;
  unitIdentifier: string;
  onViewHistory: () => void;
}) {
  return (
    <div className="text-center py-8 space-y-6">
      <div className="w-16 h-16 bg-emerald-100 rounded-full flex items-center justify-center mx-auto">
        <span className="text-3xl">✓</span>
      </div>
      <div>
        <h3 className="text-xl font-bold text-gray-900">Transferencia Registrada</h3>
        <p className="text-sm text-gray-500 mt-2">
          La transferencia de la unidad <strong>{unitIdentifier}</strong> ha sido
          registrada exitosamente en el historial del conjunto.
        </p>
      </div>

      {result.pazYSalvo?.generated && (
        <div className="bg-amber-50 border border-amber-200 rounded-xl p-4 text-left">
          <p className="text-sm font-semibold text-amber-800">Paz y Salvo</p>
          <p className="text-xs text-amber-600 mt-1">{result.pazYSalvo.message}</p>
        </div>
      )}

      <div className="flex gap-3 justify-center">
        <button
          onClick={onViewHistory}
          className="px-6 py-2.5 bg-blue-600 hover:bg-blue-700 text-white rounded-xl text-sm font-semibold transition-all"
        >
          Ver Historial de la Unidad
        </button>
      </div>
    </div>
  );
}

// ── WIZARD PRINCIPAL ──────────────────────────────────────────────────────────

interface PropertyTransferWizardProps {
  unitId: string;
  onClose?: () => void;
  onSuccess?: (unitId: string) => void;
}

export default function PropertyTransferWizard({
  unitId,
  onClose,
  onSuccess,
}: PropertyTransferWizardProps) {
  const [step, setStep] = useState(1);
  const [occupants, setOccupants] = useState<UnitOccupants | null>(null);
  const [loadingOccupants, setLoadingOccupants] = useState(true);
  const [debtConfirmed, setDebtConfirmed] = useState(false);
  const [result, setResult] = useState<TransferPropertyResult | null>(null);
  const [submitting, setSubmitting] = useState(false);
  const [submitError, setSubmitError] = useState("");

  const [ownerState, setOwnerState] = useState<NewOwnerFormState>({
    mode: "search",
    selectedOwnerId: "",
    selectedOwnerName: "",
    search: "",
    searchResults: [],
    searching: false,
    docType: DocumentType.CitizenshipCard,
    docNumber: "",
    fullName: "",
    email: "",
    mainPhone: "",
    dateOfBirth: "",
    nit: "",
    dv: "",
    companyName: "",
    legalRepName: "",
    legalRepDocType: DocumentType.CitizenshipCard,
    legalRepDoc: "",
    legalRepRole: "Gerente General",
    companyEmail: "",
    companyPhone: "",
    creating: false,
    createError: "",
  });

  const [details, setDetails] = useState<TransferDetails>({
    transferDate: new Date().toISOString().split("T")[0],
    ownershipPercentage: "100",
    isSpokesperson: true,
    residesInUnit: false,
    transferNotes: "",
    generatePazYSalvo: false,
  });

  useEffect(() => {
    loadOccupants();
  }, [unitId]);

  const loadOccupants = async () => {
    setLoadingOccupants(true);
    try {
      const data = await ResidentsService.getUnitOccupants(unitId);
      setOccupants(data);
    } catch {
      setOccupants(null);
    } finally {
      setLoadingOccupants(false);
    }
  };

  const canAdvance = (): boolean => {
    if (step === 1) {
      return !loadingOccupants && occupants !== null && occupants.activeOwners.length > 0;
    }
    if (step === 2) {
      return debtConfirmed;
    }
    if (step === 3) {
      return !!ownerState.selectedOwnerId;
    }
    if (step === 4) {
      const pct = parseFloat(details.ownershipPercentage);
      return !!details.transferDate && !isNaN(pct) && pct > 0 && pct <= 100;
    }
    return true;
  };

  const handleNext = () => {
    if (canAdvance() && step < 5) {
      setStep(step + 1);
    }
  };

  const handleBack = () => {
    if (step > 1) setStep(step - 1);
  };

  const handleOwnerCreated = (id: string, name: string) => {
    setOwnerState((prev) => ({ ...prev, selectedOwnerId: id, selectedOwnerName: name }));
  };

  const handleSubmit = async () => {
    setSubmitting(true);
    setSubmitError("");
    try {
      const payload: TransferPropertyPayload = {
        newOwnerId: ownerState.selectedOwnerId,
        transferDate: new Date(details.transferDate + "T12:00:00").toISOString(),
        ownershipPercentage: parseFloat(details.ownershipPercentage),
        isSpokesperson: details.isSpokesperson,
        residesInUnit: details.residesInUnit,
        transferNotes: details.transferNotes || undefined,
        generatePazYSalvo: details.generatePazYSalvo,
      };
      const res = await ResidentsService.transferProperty(unitId, payload);
      setResult(res);
    } catch (err: any) {
      const msg = err?.response?.data?.message || "Error al registrar la transferencia. Intente nuevamente.";
      setSubmitError(msg);
    } finally {
      setSubmitting(false);
    }
  };

  const handleViewHistory = () => {
    if (onSuccess) onSuccess(unitId);
  };

  if (result) {
    return (
      <div className="bg-white rounded-2xl shadow-sm border border-gray-100 p-8">
        <SuccessScreen
          result={result}
          unitIdentifier={occupants?.unitIdentifier ?? unitId}
          onViewHistory={handleViewHistory}
        />
      </div>
    );
  }

  return (
    <div className="bg-white rounded-2xl shadow-sm border border-gray-100 overflow-hidden">
      <div className="px-8 pt-8 pb-4 border-b border-gray-100 bg-gray-50/50">
        <div className="flex items-center justify-between mb-6">
          <div>
            <h2 className="text-xl font-bold text-gray-900">Transferencia de Propiedad</h2>
            <p className="text-sm text-gray-500 mt-1">
              Unidad {occupants?.unitIdentifier ?? "..."}
            </p>
          </div>
          {onClose && (
            <button
              onClick={onClose}
              className="w-9 h-9 flex items-center justify-center rounded-xl hover:bg-gray-100 text-gray-400 hover:text-gray-600 transition-all"
            >
              ✕
            </button>
          )}
        </div>
        <StepIndicator current={step} />
      </div>

      <div className="p-8">
        {step === 1 && (
          <Step1CurrentState
            occupants={occupants}
            loading={loadingOccupants}
          />
        )}
        {step === 2 && (
          <Step2DebtVerification
            debtConfirmed={debtConfirmed}
            onToggle={() => setDebtConfirmed((v) => !v)}
          />
        )}
        {step === 3 && (
          <Step3NewOwner
            state={ownerState}
            onChange={(partial) => setOwnerState((prev) => ({ ...prev, ...partial }))}
            onCreated={handleOwnerCreated}
          />
        )}
        {step === 4 && (
          <Step4Details
            details={details}
            onChange={(partial) => setDetails((prev) => ({ ...prev, ...partial }))}
            hasMultipleOwners={(occupants?.activeOwners.length ?? 0) > 1}
          />
        )}
        {step === 5 && (
          <Step5Confirm
            unitIdentifier={occupants?.unitIdentifier ?? ""}
            newOwnerName={ownerState.selectedOwnerName}
            details={details}
            submitting={submitting}
            submitError={submitError}
          />
        )}
      </div>

      <div className="px-8 py-5 border-t border-gray-100 bg-gray-50/50 flex justify-between items-center">
        <button
          onClick={handleBack}
          disabled={step === 1}
          className="px-5 py-2.5 bg-white border border-gray-200 text-gray-600 rounded-xl text-sm font-semibold hover:bg-gray-50 disabled:opacity-40 disabled:cursor-not-allowed transition-all"
        >
          ← Anterior
        </button>

        <span className="text-xs text-gray-400 font-medium">
          Paso {step} de {STEPS.length}
        </span>

        {step < 5 ? (
          <button
            onClick={handleNext}
            disabled={!canAdvance()}
            className="px-5 py-2.5 bg-blue-600 hover:bg-blue-700 text-white rounded-xl text-sm font-semibold disabled:opacity-40 disabled:cursor-not-allowed transition-all"
          >
            Siguiente →
          </button>
        ) : (
          <button
            onClick={handleSubmit}
            disabled={submitting}
            className="px-6 py-2.5 bg-emerald-600 hover:bg-emerald-700 text-white rounded-xl text-sm font-semibold disabled:opacity-50 transition-all"
          >
            {submitting ? "Registrando..." : "Confirmar Transferencia"}
          </button>
        )}
      </div>
    </div>
  );
}
