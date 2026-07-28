"use client";

import React, { useEffect, useState } from "react";
import { ResidentsService, ResidentDetail } from "@/lib/residents-service";
import { formatUnitLabel } from "@/lib/units-service";
import { useParams } from "next/navigation";
import Link from "next/link";
import { Users, PawPrint, Home, Phone, IdCard, CalendarDays } from "lucide-react";

const DOC_LABEL: Record<string, string> = {
  CitizenshipCard: "Cédula de Ciudadanía",
  ForeignerID: "Cédula de Extranjería",
  NIT: "NIT",
  Passport: "Pasaporte",
  PEP: "PEP",
  PPT: "PPT",
  CivilRegistry: "Registro Civil",
  IdentityCard: "Tarjeta de Identidad",
};

export default function ResidentDetailPage() {
  const params = useParams();
  const rawId = params?.id;
  const id = Array.isArray(rawId) ? rawId[0] : rawId ?? "";

  const [resident, setResident] = useState<ResidentDetail | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState("");

  useEffect(() => {
    if (!id) return;
    setLoading(true);
    setError("");
    ResidentsService.getResidentDetail(id)
      .then((data) => setResident(data))
      .catch(() => setError("No se pudo cargar la información del residente."))
      .finally(() => setLoading(false));
  }, [id]);

  const formatDate = (dateStr?: string) => {
    if (!dateStr) return "Actualidad";
    return new Date(dateStr).toLocaleDateString("es-CO", {
      year: "numeric",
      month: "short",
      day: "numeric",
    });
  };

  if (loading) {
    return (
      <div className="flex justify-center py-16">
        <div className="animate-spin rounded-full h-8 w-8 border-b-2 border-emerald-600" />
      </div>
    );
  }

  if (error || !resident) {
    return (
      <div className="max-w-3xl mx-auto space-y-4">
        <Link
          href="/residents/directory"
          className="w-10 h-10 flex items-center justify-center bg-card border border-border rounded-xl hover:bg-muted/30 transition-colors shadow-sm text-muted-foreground font-bold"
        >
          ←
        </Link>
        <div className="bg-rose-50 dark:bg-rose-950/20 border border-rose-200 dark:border-rose-900 rounded-xl px-4 py-3 text-sm font-semibold text-rose-700 dark:text-rose-400">
          {error || "Residente no encontrado."}
        </div>
      </div>
    );
  }

  return (
    <div className="max-w-3xl mx-auto space-y-6">
      <div className="flex items-center gap-4">
        <Link
          href="/residents/directory"
          className="w-10 h-10 flex items-center justify-center bg-card border border-border rounded-xl hover:bg-muted/30 transition-colors shadow-sm text-muted-foreground font-bold"
        >
          ←
        </Link>
        <div className="flex-1 min-w-0">
          <div className="flex flex-wrap items-center gap-2">
            <h1 className="text-2xl font-bold text-foreground tracking-tight truncate">
              {resident.fullNameOrPetName}
            </h1>
            {resident.isPet && (
              <span className="inline-flex items-center gap-1 px-2 py-0.5 bg-amber-100 dark:bg-amber-950/30 text-amber-700 dark:text-amber-400 text-xs font-bold rounded-full">
                <PawPrint className="w-3 h-3" /> Mascota
              </span>
            )}
            {resident.isMinor && (
              <span className="px-2 py-0.5 bg-blue-100 dark:bg-blue-950/30 text-blue-700 dark:text-blue-400 text-xs font-bold rounded-full">
                Menor
              </span>
            )}
          </div>
          <p className="text-sm text-muted-foreground mt-0.5">
            {resident.isPet ? "Mascota del grupo de convivencia" : "Integrante del grupo de convivencia"}
          </p>
        </div>
      </div>

      <div className="bg-card rounded-2xl shadow-sm border border-border p-6 space-y-4">
        <div className="flex items-center gap-3 mb-2">
          <div className="w-9 h-9 bg-emerald-100 dark:bg-emerald-950/30 rounded-lg flex items-center justify-center">
            <Users className="w-5 h-5 text-emerald-600 dark:text-emerald-400" />
          </div>
          <h3 className="text-base font-bold text-foreground">Identificación</h3>
        </div>
        <div className="grid grid-cols-1 sm:grid-cols-2 gap-4">
          {resident.isPet ? (
            <>
              <div>
                <p className="text-xs font-bold text-muted-foreground uppercase tracking-wide mb-1">Especie</p>
                <p className="text-sm text-foreground">{resident.petSpecies || "—"}</p>
              </div>
              <div>
                <p className="text-xs font-bold text-muted-foreground uppercase tracking-wide mb-1">Raza</p>
                <p className="text-sm text-foreground">{resident.petBreed || "—"}</p>
              </div>
              <div>
                <p className="text-xs font-bold text-muted-foreground uppercase tracking-wide mb-1">
                  Registro Sanitario
                </p>
                <p className="text-sm text-foreground">{resident.petSanitaryRegistration || "—"}</p>
              </div>
            </>
          ) : (
            <>
              <div className="flex items-start gap-2">
                <IdCard className="w-4 h-4 text-muted-foreground mt-0.5 shrink-0" />
                <div>
                  <p className="text-xs font-bold text-muted-foreground uppercase tracking-wide mb-1">Documento</p>
                  <p className="text-sm text-foreground">
                    {resident.documentType ? DOC_LABEL[resident.documentType] ?? resident.documentType : ""}{" "}
                    {resident.documentNumber || "—"}
                  </p>
                </div>
              </div>
              <div className="flex items-start gap-2">
                <Phone className="w-4 h-4 text-muted-foreground mt-0.5 shrink-0" />
                <div>
                  <p className="text-xs font-bold text-muted-foreground uppercase tracking-wide mb-1">Teléfono</p>
                  <p className="text-sm text-foreground">{resident.phone || "—"}</p>
                </div>
              </div>
              <div className="flex items-start gap-2">
                <CalendarDays className="w-4 h-4 text-muted-foreground mt-0.5 shrink-0" />
                <div>
                  <p className="text-xs font-bold text-muted-foreground uppercase tracking-wide mb-1">
                    Fecha de Nacimiento
                  </p>
                  <p className="text-sm text-foreground">
                    {resident.dateOfBirth ? formatDate(resident.dateOfBirth) : "—"}
                  </p>
                </div>
              </div>
            </>
          )}
        </div>
      </div>

      <div className="bg-card rounded-2xl shadow-sm border border-border p-6 space-y-4">
        <div className="flex items-center gap-3 mb-2">
          <div className="w-9 h-9 bg-blue-100 dark:bg-blue-950/30 rounded-lg flex items-center justify-center">
            <Home className="w-5 h-5 text-blue-600 dark:text-blue-400" />
          </div>
          <h3 className="text-base font-bold text-foreground">Unidades</h3>
        </div>

        {resident.unitHistory.length === 0 ? (
          <p className="text-sm text-muted-foreground text-center py-3">Sin historial registrado.</p>
        ) : (
          <div className="relative border-l-2 border-border pl-4 ml-1 space-y-4">
            {resident.unitHistory.map((entry) => (
              <div key={entry.id} className="relative">
                <div
                  className={`absolute w-2.5 h-2.5 rounded-full -left-[1.3rem] top-1.5 border-2 border-card ${
                    entry.endDate ? "bg-muted-foreground/40" : "bg-emerald-500"
                  }`}
                />
                <div className="flex flex-wrap items-center gap-2">
                  <Link
                    href={`/units/${entry.unitId}`}
                    className="text-sm font-semibold text-foreground hover:text-emerald-600 hover:underline"
                  >
                    {formatUnitLabel(entry.unitIdentifier, entry.unitTowerOrBlock)}
                  </Link>
                  <span className="text-xs text-muted-foreground">· {entry.relationship}</span>
                  {!entry.endDate && (
                    <span className="px-1.5 py-0.5 bg-emerald-100 dark:bg-emerald-950/30 text-emerald-700 dark:text-emerald-400 text-xs font-bold rounded-full">
                      Actual
                    </span>
                  )}
                </div>
                <p className="text-xs text-muted-foreground mt-0.5">
                  {formatDate(entry.startDate)} → {formatDate(entry.endDate)}
                </p>
                {entry.transferNotes && (
                  <p className="text-xs text-muted-foreground/80 mt-0.5 italic">"{entry.transferNotes}"</p>
                )}
              </div>
            ))}
          </div>
        )}
      </div>
    </div>
  );
}
