"use client";

import React, { useEffect, useState } from "react";
import { ResidentsService, TenantResidentListItem } from "@/lib/residents-service";
import { useRouter } from "next/navigation";
import Link from "next/link";
import { Plus, Search, Users, Home, X } from "lucide-react";

const DOC_LABEL: Record<string, string> = {
  CitizenshipCard: "CC",
  ForeignerID: "CE",
  NIT: "NIT",
  Passport: "PAS",
  PEP: "PEP",
  PPT: "PPT",
};

export default function TenantsListPage() {
  const router = useRouter();
  const [tenants, setTenants] = useState<TenantResidentListItem[]>([]);
  const [loading, setLoading] = useState(true);
  const [apiError, setApiError] = useState("");
  const [search, setSearch] = useState("");
  const [includeInactive, setIncludeInactive] = useState(false);

  useEffect(() => {
    loadTenants();
  }, [includeInactive]);

  const loadTenants = async () => {
    setLoading(true);
    setApiError("");
    try {
      const data = await ResidentsService.getTenants(undefined, includeInactive);
      setTenants(data);
    } catch (err: any) {
      const msg =
        err?.response?.data?.message ||
        "Error al cargar los arrendatarios. Verifica que el servidor esté activo.";
      setApiError(msg);
      setTenants([]);
    } finally {
      setLoading(false);
    }
  };

  const filtered = tenants.filter((t) => {
    if (!search) return true;
    const lower = search.toLowerCase();
    return (
      t.fullName.toLowerCase().includes(lower) ||
      t.documentNumber.includes(lower) ||
      t.email.toLowerCase().includes(lower) ||
      t.unitIdentifier.toLowerCase().includes(lower)
    );
  });

  const activeCount = tenants.filter((t) => t.isActive).length;
  const inactiveCount = tenants.filter((t) => !t.isActive).length;

  const leaseDaysLabel = (days?: number): { text: string; cls: string } => {
    if (days === undefined || days === null) {
      return { text: "Sin fecha fin", cls: "bg-gray-100 text-gray-500" };
    }
    if (days < 0) {
      return { text: `Vencido hace ${Math.abs(days)}d`, cls: "bg-red-100 text-red-700" };
    }
    if (days <= 30) {
      return { text: `${days}d restantes`, cls: "bg-amber-100 text-amber-700" };
    }
    return { text: `${days}d restantes`, cls: "bg-emerald-100 text-emerald-700" };
  };

  return (
    <div className="space-y-6">
      {/* Header */}
      <div className="flex items-center justify-between gap-4 flex-wrap">
        <div>
          <h1 className="text-2xl font-bold text-gray-900 tracking-tight">Arrendatarios</h1>
          <p className="text-sm text-gray-500 mt-0.5">
            Personas que habitan las unidades bajo contrato de arrendamiento.
          </p>
        </div>
        <Link
          href="/residents/tenants/new"
          className="inline-flex items-center gap-2 px-5 py-2.5 bg-emerald-600 hover:bg-emerald-700 text-white font-bold rounded-xl shadow-sm shadow-emerald-200 transition-colors text-sm"
        >
          <Plus className="w-4 h-4" />
          Registrar Arrendatario
        </Link>
      </div>

      {/* Stats */}
      <div className="grid grid-cols-2 sm:grid-cols-3 gap-4">
        <div className="bg-white rounded-xl shadow-sm border border-gray-100 p-4">
          <div className="flex items-center gap-3">
            <div className="w-9 h-9 bg-emerald-100 rounded-lg flex items-center justify-center">
              <Users className="w-5 h-5 text-emerald-600" />
            </div>
            <div>
              <p className="text-2xl font-black text-gray-900">{activeCount}</p>
              <p className="text-xs font-bold text-gray-500 uppercase tracking-wide">Activos</p>
            </div>
          </div>
        </div>
        <div className="bg-white rounded-xl shadow-sm border border-gray-100 p-4">
          <div className="flex items-center gap-3">
            <div className="w-9 h-9 bg-gray-100 rounded-lg flex items-center justify-center">
              <Users className="w-5 h-5 text-gray-400" />
            </div>
            <div>
              <p className="text-2xl font-black text-gray-900">{inactiveCount}</p>
              <p className="text-xs font-bold text-gray-500 uppercase tracking-wide">Inactivos</p>
            </div>
          </div>
        </div>
        <div className="bg-white rounded-xl shadow-sm border border-gray-100 p-4 col-span-2 sm:col-span-1">
          <div className="flex items-center gap-3">
            <div className="w-9 h-9 bg-blue-100 rounded-lg flex items-center justify-center">
              <Home className="w-5 h-5 text-blue-600" />
            </div>
            <div>
              <p className="text-2xl font-black text-gray-900">{tenants.length}</p>
              <p className="text-xs font-bold text-gray-500 uppercase tracking-wide">Total</p>
            </div>
          </div>
        </div>
      </div>

      {/* Filters */}
      <div className="bg-white rounded-xl shadow-sm border border-gray-100 p-4 flex flex-col sm:flex-row gap-3">
        <div className="relative flex-1">
          <Search className="absolute left-3 top-1/2 -translate-y-1/2 w-4 h-4 text-gray-400" />
          <input
            type="text"
            placeholder="Buscar por nombre, documento, correo o unidad..."
            value={search}
            onChange={(e) => setSearch(e.target.value)}
            className="w-full pl-9 pr-9 py-2.5 bg-gray-50 border border-gray-200 rounded-xl text-sm focus:ring-2 focus:ring-emerald-500/20 focus:border-emerald-500 outline-none transition-all"
          />
          {search && (
            <button
              onClick={() => setSearch("")}
              className="absolute right-3 top-1/2 -translate-y-1/2 text-gray-400 hover:text-gray-600"
            >
              <X className="w-4 h-4" />
            </button>
          )}
        </div>
        <label className="flex items-center gap-2.5 cursor-pointer select-none shrink-0">
          <div
            onClick={() => setIncludeInactive(!includeInactive)}
            className={`w-10 h-6 rounded-full transition-colors flex items-center px-1 ${
              includeInactive ? "bg-emerald-500" : "bg-gray-200"
            }`}
          >
            <div
              className={`w-4 h-4 bg-white rounded-full shadow transition-transform ${
                includeInactive ? "translate-x-4" : "translate-x-0"
              }`}
            />
          </div>
          <span className="text-sm font-semibold text-gray-700">Incluir inactivos</span>
        </label>
      </div>

      {/* Error */}
      {apiError && (
        <div className="bg-red-50 border border-red-200 rounded-xl px-4 py-3 text-sm font-semibold text-red-700 flex items-center justify-between gap-4">
          <span>{apiError}</span>
          <button
            onClick={loadTenants}
            className="text-xs font-bold text-red-600 underline underline-offset-2 hover:text-red-800 shrink-0"
          >
            Reintentar
          </button>
        </div>
      )}

      {/* List */}
      <div className="bg-white rounded-xl shadow-sm border border-gray-100 overflow-hidden">
        {loading ? (
          <div className="flex justify-center py-16">
            <div className="animate-spin rounded-full h-8 w-8 border-b-2 border-emerald-600" />
          </div>
        ) : filtered.length === 0 ? (
          <div className="py-16 text-center">
            <Users className="w-12 h-12 mx-auto text-gray-200 mb-3" />
            <p className="text-base font-bold text-gray-500">
              {search ? "Sin resultados" : "Sin arrendatarios registrados"}
            </p>
            <p className="text-sm text-gray-400 mt-1">
              {search
                ? "Intenta con otros términos de búsqueda."
                : "Registra el primer arrendatario con el botón de arriba."}
            </p>
          </div>
        ) : (
          <>
            {/* Desktop table */}
            <div className="hidden md:block overflow-x-auto">
              <table className="w-full">
                <thead>
                  <tr className="border-b border-gray-100 bg-gray-50/50">
                    <th className="text-left px-5 py-3.5 text-xs font-bold text-gray-500 uppercase tracking-wider">
                      Arrendatario
                    </th>
                    <th className="text-left px-5 py-3.5 text-xs font-bold text-gray-500 uppercase tracking-wider">
                      Unidad
                    </th>
                    <th className="text-left px-5 py-3.5 text-xs font-bold text-gray-500 uppercase tracking-wider">
                      Contacto
                    </th>
                    <th className="text-left px-5 py-3.5 text-xs font-bold text-gray-500 uppercase tracking-wider">
                      Contrato
                    </th>
                    <th className="text-left px-5 py-3.5 text-xs font-bold text-gray-500 uppercase tracking-wider">
                      Estado
                    </th>
                    <th className="px-5 py-3.5" />
                  </tr>
                </thead>
                <tbody className="divide-y divide-gray-50">
                  {filtered.map((t) => {
                    const badge = leaseDaysLabel(t.daysUntilLeaseExpires);
                    return (
                      <tr
                        key={t.id}
                        className="hover:bg-gray-50/50 transition-colors cursor-pointer"
                        onClick={() => router.push(`/residents/tenants/${t.id}`)}
                      >
                        <td className="px-5 py-4">
                          <p className="text-sm font-bold text-gray-900">{t.fullName}</p>
                          <p className="text-xs text-gray-500 mt-0.5">
                            {DOC_LABEL[t.documentType] ?? t.documentType} {t.documentNumber}
                          </p>
                        </td>
                        <td className="px-5 py-4">
                          <span className="inline-flex items-center gap-1.5 px-2.5 py-1 bg-blue-50 text-blue-700 rounded-lg text-xs font-bold">
                            <Home className="w-3.5 h-3.5" />
                            {t.unitIdentifier}
                          </span>
                        </td>
                        <td className="px-5 py-4">
                          <p className="text-xs text-gray-700">{t.email}</p>
                          <p className="text-xs text-gray-500 mt-0.5">{t.phone}</p>
                        </td>
                        <td className="px-5 py-4">
                          <p className="text-xs text-gray-700">
                            Desde{" "}
                            {new Date(t.leaseStartDate).toLocaleDateString("es-CO", {
                              day: "numeric",
                              month: "short",
                              year: "numeric",
                            })}
                          </p>
                          {t.leaseEndDate && (
                            <span
                              className={`inline-block mt-1 px-2 py-0.5 rounded-full text-xs font-bold ${badge.cls}`}
                            >
                              {badge.text}
                            </span>
                          )}
                        </td>
                        <td className="px-5 py-4">
                          <span
                            className={`px-2.5 py-1 rounded-full text-xs font-bold ${
                              t.isActive
                                ? "bg-emerald-100 text-emerald-700"
                                : "bg-gray-100 text-gray-500"
                            }`}
                          >
                            {t.isActive ? "Activo" : "Inactivo"}
                          </span>
                        </td>
                        <td className="px-5 py-4 text-right">
                          <button
                            onClick={(e) => {
                              e.stopPropagation();
                              router.push(`/residents/tenants/${t.id}`);
                            }}
                            className="text-xs font-semibold text-blue-600 hover:text-blue-800 transition-colors"
                          >
                            Ver detalle →
                          </button>
                        </td>
                      </tr>
                    );
                  })}
                </tbody>
              </table>
            </div>

            {/* Mobile cards */}
            <div className="md:hidden divide-y divide-gray-100">
              {filtered.map((t) => {
                const badge = leaseDaysLabel(t.daysUntilLeaseExpires);
                return (
                  <div
                    key={t.id}
                    className="p-4 flex items-start gap-3 cursor-pointer hover:bg-gray-50 transition-colors"
                    onClick={() => router.push(`/residents/tenants/${t.id}`)}
                  >
                    <div className="w-10 h-10 rounded-xl bg-emerald-100 flex items-center justify-center shrink-0">
                      <Users className="w-5 h-5 text-emerald-600" />
                    </div>
                    <div className="flex-1 min-w-0">
                      <div className="flex items-center gap-2 flex-wrap">
                        <p className="text-sm font-bold text-gray-900">{t.fullName}</p>
                        {!t.isActive && (
                          <span className="px-1.5 py-0.5 bg-gray-100 text-gray-500 text-xs font-bold rounded-full">
                            Inactivo
                          </span>
                        )}
                      </div>
                      <p className="text-xs text-gray-500 mt-0.5">
                        {DOC_LABEL[t.documentType] ?? t.documentType} {t.documentNumber} · Unidad{" "}
                        {t.unitIdentifier}
                      </p>
                      {t.leaseEndDate && (
                        <span
                          className={`inline-block mt-1.5 px-2 py-0.5 rounded-full text-xs font-bold ${badge.cls}`}
                        >
                          {badge.text}
                        </span>
                      )}
                    </div>
                  </div>
                );
              })}
            </div>
          </>
        )}
      </div>
    </div>
  );
}
