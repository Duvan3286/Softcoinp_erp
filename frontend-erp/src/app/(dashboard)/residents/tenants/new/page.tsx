"use client";

import React, { useEffect, useState } from "react";
import {
  ResidentsService,
  DocumentType,
  CreateTenantResidentPayload,
} from "@/lib/residents-service";
import { UnitsService, Unit } from "@/lib/units-service";
import { useRouter } from "next/navigation";
import Link from "next/link";
import { Home, FileCheck } from "lucide-react";

const inputClass =
  "w-full px-4 py-2.5 bg-gray-50 border border-gray-200 rounded-xl focus:ring-2 focus:ring-emerald-500/20 focus:border-emerald-500 focus:bg-white text-sm text-gray-900 transition-all outline-none";
const labelClass =
  "block text-xs font-bold text-gray-700 uppercase tracking-wide mb-2";

// ── Helpers ──────────────────────────────────────────────────────────────────

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

// ── Component ─────────────────────────────────────────────────────────────────

export default function NewTenantPage() {
  const router = useRouter();
  const [submitting, setSubmitting] = useState(false);
  const [error, setError] = useState("");

  // Unit selector
  const [units, setUnits] = useState<Unit[]>([]);
  const [loadingUnits, setLoadingUnits] = useState(true);
  const [selectedUnitId, setSelectedUnitId] = useState("");

  // Tenant fields
  const [docType, setDocType] = useState<DocumentType>(DocumentType.CitizenshipCard);
  const [docNumber, setDocNumber] = useState("");
  const [fullName, setFullName] = useState("");
  const [email, setEmail] = useState("");
  const [phone, setPhone] = useState("");
  const [leaseStart, setLeaseStart] = useState(new Date().toISOString().split("T")[0]);
  const [leaseEnd, setLeaseEnd] = useState("");
  const [agentName, setAgentName] = useState("");
  const [agentPhone, setAgentPhone] = useState("");
  const [authorizedToPay, setAuthorizedToPay] = useState(false);

  useEffect(() => {
    UnitsService.getUnits()
      .then((data) => {
        setUnits(data);
        if (data.length > 0) setSelectedUnitId(data[0].id);
      })
      .catch(() => setUnits([]))
      .finally(() => setLoadingUnits(false));
  }, []);

  const handleDocTypeChange = (dt: DocumentType) => {
    setDocType(dt);
    setDocNumber("");
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!selectedUnitId) {
      setError("Debes seleccionar una unidad.");
      return;
    }
    setSubmitting(true);
    setError("");
    try {
      const payload: CreateTenantResidentPayload = {
        documentType: docType,
        documentNumber: docNumber,
        fullName: fullName,
        email: email,
        phone: phone,
        leaseStartDate: leaseStart,
        leaseEndDate: leaseEnd || undefined,
        realEstateAgentName: agentName || undefined,
        realEstateAgentPhone: agentPhone || undefined,
        authorizedToPayAdmin: authorizedToPay,
      };
      const result = await ResidentsService.registerTenant(selectedUnitId, payload);
      router.push(`/residents/tenants/${result.id}`);
    } catch (err: any) {
      const msg =
        err?.response?.data?.message ||
        err?.response?.data?.errors?.[
          Object.keys(err?.response?.data?.errors ?? {})[0]
        ]?.[0] ||
        "Ocurrió un error al registrar el arrendatario.";
      setError(msg);
    } finally {
      setSubmitting(false);
    }
  };

  return (
    <div className="max-w-3xl mx-auto space-y-6">
      <div className="flex items-center gap-4">
        <Link
          href="/residents/tenants"
          className="w-10 h-10 flex items-center justify-center bg-white border border-gray-200 rounded-xl hover:bg-gray-50 transition-colors shadow-sm text-gray-500 font-bold"
        >
          ←
        </Link>
        <div>
          <h1 className="text-2xl font-bold text-gray-900 tracking-tight">
            Registrar Arrendatario
          </h1>
          <p className="text-sm text-gray-500 mt-0.5">
            Vincula un arrendatario a una unidad del conjunto.
          </p>
        </div>
      </div>

      <form onSubmit={handleSubmit} className="space-y-6">
        {/* Unidad */}
        <div className="bg-white rounded-2xl shadow-sm border border-gray-100 p-6 space-y-4">
          <div className="flex items-center gap-3 mb-2">
            <div className="w-9 h-9 bg-blue-100 rounded-lg flex items-center justify-center">
              <Home className="w-5 h-5 text-blue-600" />
            </div>
            <h3 className="text-base font-bold text-gray-800">Unidad a Arrendar</h3>
          </div>
          {loadingUnits ? (
            <div className="flex items-center gap-2 py-2">
              <div className="animate-spin rounded-full h-4 w-4 border-b-2 border-blue-600" />
              <span className="text-sm text-gray-500">Cargando unidades...</span>
            </div>
          ) : units.length === 0 ? (
            <p className="text-sm text-amber-700 bg-amber-50 border border-amber-200 rounded-xl px-4 py-3 font-semibold">
              No hay unidades disponibles. Crea unidades primero en el módulo de Unidades.
            </p>
          ) : (
            <div>
              <label className={labelClass}>Unidad *</label>
              <select
                value={selectedUnitId}
                onChange={(e) => setSelectedUnitId(e.target.value)}
                className={inputClass}
                required
              >
                <option value="">Seleccione una unidad...</option>
                {units.map((u) => (
                  <option key={u.id} value={u.id}>
                    {u.identifier}
                    {u.towerOrBlock ? ` — Torre/Bloque ${u.towerOrBlock}` : ""}
                  </option>
                ))}
              </select>
            </div>
          )}
        </div>

        {/* Identificación */}
        <div className="bg-white rounded-2xl shadow-sm border border-gray-100 p-6 space-y-5">
          <h3 className="text-base font-bold text-gray-800">Identificación</h3>
          <div className="grid grid-cols-1 md:grid-cols-12 gap-5">
            <div className="col-span-12 md:col-span-5">
              <label className={labelClass}>Tipo de Documento</label>
              <select
                value={docType}
                onChange={(e) => handleDocTypeChange(Number(e.target.value) as DocumentType)}
                className={inputClass}
              >
                <option value={DocumentType.CitizenshipCard}>Cédula de Ciudadanía (CC)</option>
                <option value={DocumentType.ForeignerID}>Cédula de Extranjería (CE)</option>
                <option value={DocumentType.Passport}>Pasaporte</option>
                <option value={DocumentType.PEP}>Persona Expuesta Políticamente (PEP)</option>
                <option value={DocumentType.PPT}>Pasaporte Temporal (PPT)</option>
              </select>
            </div>
            <div className="col-span-12 md:col-span-7">
              <label className={labelClass}>Número de Documento *</label>
              <input
                type="text"
                inputMode={docType === DocumentType.CitizenshipCard ? "numeric" : "text"}
                required
                value={docNumber}
                onChange={(e) => setDocNumber(sanitizeDocNumber(e.target.value, docType))}
                className={inputClass}
                placeholder={
                  docType === DocumentType.CitizenshipCard ? "Ej: 1020304050" : "Ej: AB123456"
                }
              />
            </div>
            <div className="col-span-12">
              <label className={labelClass}>Nombre Completo *</label>
              <input
                type="text"
                required
                maxLength={200}
                value={fullName}
                onChange={(e) => setFullName(toTitleCase(e.target.value))}
                className={inputClass}
                placeholder="Ej: María Alejandra Gómez Pérez"
              />
            </div>
          </div>
        </div>

        {/* Contacto */}
        <div className="bg-white rounded-2xl shadow-sm border border-gray-100 p-6 space-y-5">
          <h3 className="text-base font-bold text-gray-800">Contacto</h3>
          <div className="grid grid-cols-1 md:grid-cols-2 gap-5">
            <div className="md:col-span-2">
              <label className={labelClass}>Correo Electrónico *</label>
              <input
                type="email"
                required
                maxLength={256}
                value={email}
                onChange={(e) => setEmail(e.target.value)}
                className={inputClass}
                placeholder="arrendatario@ejemplo.com"
              />
            </div>
            <div>
              <label className={labelClass}>Teléfono *</label>
              <input
                type="tel"
                required
                value={phone}
                onChange={(e) => setPhone(sanitizePhone(e.target.value))}
                className={inputClass}
                placeholder="3001234567"
              />
            </div>
          </div>
        </div>

        {/* Contrato */}
        <div className="bg-white rounded-2xl shadow-sm border border-gray-100 p-6 space-y-5">
          <h3 className="text-base font-bold text-gray-800">Contrato de Arrendamiento</h3>
          <div className="grid grid-cols-1 md:grid-cols-2 gap-5">
            <div>
              <label className={labelClass}>Fecha de Inicio *</label>
              <input
                type="date"
                required
                value={leaseStart}
                onChange={(e) => setLeaseStart(e.target.value)}
                className={inputClass}
              />
            </div>
            <div>
              <label className={labelClass}>
                Fecha de Terminación{" "}
                <span className="font-normal normal-case text-gray-400">(opcional)</span>
              </label>
              <input
                type="date"
                value={leaseEnd}
                onChange={(e) => setLeaseEnd(e.target.value)}
                min={leaseStart}
                className={inputClass}
              />
            </div>
            <div>
              <label className={labelClass}>
                Nombre Inmobiliaria / Intermediario{" "}
                <span className="font-normal normal-case text-gray-400">(opcional)</span>
              </label>
              <input
                type="text"
                maxLength={200}
                value={agentName}
                onChange={(e) => setAgentName(toTitleCase(e.target.value))}
                className={inputClass}
                placeholder="Ej: Inmobiliaria Abc"
              />
            </div>
            <div>
              <label className={labelClass}>
                Teléfono Intermediario{" "}
                <span className="font-normal normal-case text-gray-400">(opcional)</span>
              </label>
              <input
                type="tel"
                value={agentPhone}
                onChange={(e) => setAgentPhone(sanitizePhone(e.target.value))}
                className={inputClass}
                placeholder="3009876543"
              />
            </div>
            <div className="md:col-span-2">
              <label className="flex items-center gap-3 cursor-pointer select-none">
                <div
                  onClick={() => setAuthorizedToPay(!authorizedToPay)}
                  className={`w-11 h-6 rounded-full transition-colors flex items-center px-1 ${
                    authorizedToPay ? "bg-emerald-500" : "bg-gray-200"
                  }`}
                >
                  <div
                    className={`w-4 h-4 bg-white rounded-full shadow transition-transform ${
                      authorizedToPay ? "translate-x-5" : "translate-x-0"
                    }`}
                  />
                </div>
                <div>
                  <span className="block text-sm font-semibold text-gray-800">
                    Autorizado a pagar administración
                  </span>
                  <span className="block text-xs text-gray-400">
                    El arrendatario puede realizar pagos de cuotas de administración directamente.
                  </span>
                </div>
              </label>
            </div>
          </div>
        </div>

        {/* Footer */}
        <div className="bg-white rounded-2xl shadow-sm border border-gray-100 p-5 flex flex-col sm:flex-row justify-between items-center gap-4">
          {error ? (
            <div className="text-sm font-semibold text-red-600 bg-red-50 border border-red-200 rounded-xl px-4 py-2 w-full sm:w-auto">
              {error}
            </div>
          ) : (
            <div className="flex items-center gap-2 text-sm text-gray-500">
              <FileCheck className="w-4 h-4" />
              Los datos podrán actualizarse después del registro.
            </div>
          )}

          <div className="flex gap-3 w-full sm:w-auto">
            <Link
              href="/residents/tenants"
              className="px-5 py-2.5 text-gray-700 font-semibold rounded-xl hover:bg-gray-100 transition-colors text-center flex-1 sm:flex-none"
            >
              Cancelar
            </Link>
            <button
              type="submit"
              disabled={submitting || loadingUnits || units.length === 0}
              className="px-6 py-2.5 bg-emerald-600 hover:bg-emerald-700 text-white font-bold rounded-xl shadow-sm shadow-emerald-200 transition-colors flex-1 sm:flex-none flex items-center justify-center gap-2 disabled:opacity-50"
            >
              {submitting && (
                <div className="w-4 h-4 rounded-full border-2 border-white/30 border-t-white animate-spin" />
              )}
              Guardar Arrendatario
            </button>
          </div>
        </div>
      </form>
    </div>
  );
}
