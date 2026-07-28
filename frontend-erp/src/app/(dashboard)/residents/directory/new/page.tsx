"use client";

import React, { useEffect, useState } from "react";
import {
  ResidentsService,
  DocumentType,
  AddCohabitationMemberPayload,
} from "@/lib/residents-service";
import { UnitsService, Unit, formatUnitLabel } from "@/lib/units-service";
import { useRouter } from "next/navigation";
import Link from "next/link";
import { Home, Users } from "lucide-react";

const inputClass =
  "w-full px-4 py-2.5 bg-muted/50 border border-border rounded-xl focus:ring-2 focus:ring-emerald-500/20 focus:border-emerald-500 focus:bg-card text-sm text-foreground transition-all outline-none";
const labelClass =
  "block text-xs font-bold text-muted-foreground uppercase tracking-wide mb-2";

const toTitleCase = (val: string): string =>
  val.toLowerCase().replace(/(?:^|\s)\S/g, (a) => a.toUpperCase());

const sanitizePhone = (val: string): string =>
  val.replace(/[^0-9\-\+\s]/g, "").slice(0, 20);

export default function NewResidentPage() {
  const router = useRouter();
  const [submitting, setSubmitting] = useState(false);
  const [error, setError] = useState("");

  const [units, setUnits] = useState<Unit[]>([]);
  const [loadingUnits, setLoadingUnits] = useState(true);
  const [selectedUnitId, setSelectedUnitId] = useState("");

  const [fullName, setFullName] = useState("");
  const [documentType, setDocumentType] = useState<DocumentType>(DocumentType.CitizenshipCard);
  const [documentNumber, setDocumentNumber] = useState("");
  const [phone, setPhone] = useState("");

  useEffect(() => {
    UnitsService.getUnits()
      .then((data) => {
        setUnits(data);
        if (data.length > 0) setSelectedUnitId(data[0].id);
      })
      .catch(() => setUnits([]))
      .finally(() => setLoadingUnits(false));
  }, []);

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setError("");

    if (!selectedUnitId) {
      setError("Debes seleccionar una unidad.");
      return;
    }
    if (!fullName.trim()) {
      setError("El nombre es obligatorio.");
      return;
    }
    if (!documentNumber.trim()) {
      setError("El número de documento es obligatorio.");
      return;
    }
    if (!phone.trim()) {
      setError("El teléfono es obligatorio.");
      return;
    }

    setSubmitting(true);
    try {
      const payload: AddCohabitationMemberPayload = {
        fullNameOrPetName: fullName,
        documentType,
        documentNumber,
        phone,
        isPet: false,
      };
      await ResidentsService.addCohabitationMember(selectedUnitId, payload);
      router.push("/residents/directory");
    } catch (err: any) {
      const msg =
        err?.response?.data?.message ||
        err?.response?.data?.errors?.[
          Object.keys(err?.response?.data?.errors ?? {})[0]
        ]?.[0] ||
        "Ocurrió un error al registrar el residente.";
      setError(msg);
    } finally {
      setSubmitting(false);
    }
  };

  return (
    <div className="max-w-3xl mx-auto space-y-6">
      <div className="flex items-center gap-4">
        <Link
          href="/residents/directory"
          className="w-10 h-10 flex items-center justify-center bg-card border border-border rounded-xl hover:bg-muted/30 transition-colors shadow-sm text-muted-foreground font-bold"
        >
          ←
        </Link>
        <div>
          <h1 className="text-2xl font-bold text-foreground tracking-tight">Agregar Residente</h1>
          <p className="text-sm text-muted-foreground mt-0.5">
            Registra una persona del grupo de convivencia (esposa, hijos, etc.) en una unidad.
          </p>
        </div>
      </div>

      <form onSubmit={handleSubmit} className="space-y-6">
        {/* Unidad */}
        <div className="bg-card rounded-2xl shadow-sm border border-border p-6 space-y-4">
          <div className="flex items-center gap-3 mb-2">
            <div className="w-9 h-9 bg-blue-100 dark:bg-blue-950/30 rounded-lg flex items-center justify-center">
              <Home className="w-5 h-5 text-blue-600 dark:text-blue-400" />
            </div>
            <h3 className="text-base font-bold text-foreground">Unidad</h3>
          </div>
          {loadingUnits ? (
            <div className="flex items-center gap-2 py-2">
              <div className="animate-spin rounded-full h-4 w-4 border-b-2 border-emerald-600" />
              <span className="text-sm text-muted-foreground">Cargando unidades...</span>
            </div>
          ) : units.length === 0 ? (
            <p className="text-sm text-amber-700 dark:text-amber-400 bg-amber-50 border border-amber-200 dark:border-amber-900 rounded-xl px-4 py-3 font-semibold">
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
                    {formatUnitLabel(u.identifier, u.towerOrBlock)}
                  </option>
                ))}
              </select>
            </div>
          )}
        </div>

        {/* Identificación */}
        <div className="bg-card rounded-2xl shadow-sm border border-border p-6 space-y-5">
          <div className="flex items-center gap-3 mb-2">
            <div className="w-9 h-9 bg-emerald-100 dark:bg-emerald-950/30 rounded-lg flex items-center justify-center">
              <Users className="w-5 h-5 text-emerald-600 dark:text-emerald-400" />
            </div>
            <h3 className="text-base font-bold text-foreground">Identificación</h3>
          </div>
          <div className="grid grid-cols-1 md:grid-cols-12 gap-5">
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
            <div className="col-span-12 md:col-span-4">
              <label className={labelClass}>Tipo de Documento *</label>
              <select
                value={documentType}
                onChange={(e) => setDocumentType(Number(e.target.value) as DocumentType)}
                className={inputClass}
              >
                <option value={DocumentType.CitizenshipCard}>Cédula de Ciudadanía (CC)</option>
                <option value={DocumentType.IdentityCard}>Tarjeta de Identidad</option>
                <option value={DocumentType.CivilRegistry}>Registro Civil</option>
                <option value={DocumentType.ForeignerID}>Cédula de Extranjería (CE)</option>
                <option value={DocumentType.Passport}>Pasaporte</option>
                <option value={DocumentType.PEP}>Persona Expuesta Políticamente (PEP)</option>
                <option value={DocumentType.PPT}>Pasaporte Temporal (PPT)</option>
              </select>
            </div>
            <div className="col-span-12 md:col-span-4">
              <label className={labelClass}>Número de Documento *</label>
              <input
                type="text"
                required
                value={documentNumber}
                onChange={(e) => setDocumentNumber(e.target.value)}
                className={inputClass}
                placeholder="Ej: 1020304050"
              />
            </div>
            <div className="col-span-12 md:col-span-4">
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

        {/* Footer */}
        <div className="bg-card rounded-2xl shadow-sm border border-border p-5 flex flex-col sm:flex-row justify-between items-center gap-4">
          {error && (
            <div className="text-sm font-semibold text-rose-600 dark:text-rose-400 bg-rose-50 dark:bg-rose-950/20 border border-rose-200 dark:border-rose-900 rounded-xl px-4 py-2 w-full sm:w-auto">
              {error}
            </div>
          )}

          <div className="flex gap-3 w-full sm:w-auto sm:ml-auto">
            <Link
              href="/residents/directory"
              className="px-5 py-2.5 text-muted-foreground font-semibold rounded-xl hover:bg-muted transition-colors text-center flex-1 sm:flex-none"
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
              Guardar Residente
            </button>
          </div>
        </div>
      </form>
    </div>
  );
}
