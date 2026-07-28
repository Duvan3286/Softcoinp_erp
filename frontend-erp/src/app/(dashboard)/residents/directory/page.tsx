"use client";

import React, { useEffect, useState } from "react";
import { ResidentsService, ResidentListItem } from "@/lib/residents-service";
import { formatUnitLabel } from "@/lib/units-service";
import Link from "next/link";
import { Users, Search, Home, Plus } from "lucide-react";

const PAGE_SIZE = 15;

export default function ResidentsDirectoryPage() {
  const [residents, setResidents] = useState<ResidentListItem[]>([]);
  const [loading, setLoading] = useState(true);
  const [apiError, setApiError] = useState("");
  const [search, setSearch] = useState("");
  const [currentPage, setCurrentPage] = useState(1);

  useEffect(() => {
    loadResidents();
  }, []);

  const loadResidents = async () => {
    setLoading(true);
    setApiError("");
    try {
      const data = await ResidentsService.getResidentsDirectory();
      setResidents(data);
      setCurrentPage(1);
    } catch {
      setApiError("Error al cargar los residentes. Verifica que el servidor esté activo.");
      setResidents([]);
    } finally {
      setLoading(false);
    }
  };

  const filtered = residents.filter((r) => {
    if (!search) {
      return true;
    }
    const lower = search.toLowerCase();
    return (
      r.fullName.toLowerCase().includes(lower) ||
      (r.documentNumber ?? "").toLowerCase().includes(lower) ||
      r.unitIdentifier.toLowerCase().includes(lower)
    );
  });

  useEffect(() => {
    setCurrentPage(1);
  }, [search]);

  const totalPages = Math.max(1, Math.ceil(filtered.length / PAGE_SIZE));
  const paginatedResidents = filtered.slice((currentPage - 1) * PAGE_SIZE, currentPage * PAGE_SIZE);
  const rangeStart = filtered.length === 0 ? 0 : (currentPage - 1) * PAGE_SIZE + 1;
  const rangeEnd = Math.min(currentPage * PAGE_SIZE, filtered.length);

  const goToPage = (page: number) => {
    let nextPage = page;
    if (nextPage < 1) {
      nextPage = 1;
    }
    if (nextPage > totalPages) {
      nextPage = totalPages;
    }
    setCurrentPage(nextPage);
  };

  const renderRoleBadge = (role: string) => {
    if (role === "Propietario") {
      return (
        <span className="px-2.5 py-1 rounded-full text-xs font-bold bg-indigo-100 dark:bg-indigo-950/30 text-indigo-700 dark:text-indigo-400">
          Propietario
        </span>
      );
    }
    if (role === "Arrendatario") {
      return (
        <span className="px-2.5 py-1 rounded-full text-xs font-bold bg-emerald-100 dark:bg-emerald-950/30 text-emerald-700 dark:text-emerald-400">
          Arrendatario
        </span>
      );
    }
    return (
      <span className="px-2.5 py-1 rounded-full text-xs font-bold bg-muted text-muted-foreground">
        {role}
      </span>
    );
  };

  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between gap-4 flex-wrap">
        <div>
          <h1 className="text-2xl font-bold text-foreground tracking-tight">Residentes</h1>
          <p className="text-sm text-muted-foreground mt-1">
            Todas las personas que viven en las unidades del conjunto: propietarios residentes, arrendatarios activos
            y su grupo de convivencia.
          </p>
        </div>
        <Link
          href="/residents/directory/new"
          className="inline-flex items-center gap-2 bg-emerald-600 hover:bg-emerald-700 text-white px-4 py-2.5 rounded-xl text-sm font-semibold shadow-sm shadow-emerald-200 transition-colors shrink-0"
        >
          <Plus className="w-5 h-5" />
          Registrar Residente
        </Link>
      </div>

      <div className="grid grid-cols-1 sm:grid-cols-3 gap-4">
        <div className="bg-card border border-border rounded-xl shadow-sm p-5 flex items-center gap-4">
          <div className="w-10 h-10 bg-blue-50 dark:bg-blue-950/20 rounded-xl flex items-center justify-center">
            <Users className="w-5 h-5 text-blue-600 dark:text-blue-400" />
          </div>
          <div>
            <p className="text-2xl font-bold text-foreground">{residents.length}</p>
            <p className="text-xs text-muted-foreground font-medium">Total Residentes</p>
          </div>
        </div>
        <div className="bg-card border border-border rounded-xl shadow-sm p-5 flex items-center gap-4">
          <div className="w-10 h-10 bg-indigo-50 dark:bg-indigo-950/20 rounded-xl flex items-center justify-center">
            <Home className="w-5 h-5 text-indigo-600 dark:text-indigo-400" />
          </div>
          <div>
            <p className="text-2xl font-bold text-foreground">
              {residents.filter((r) => r.role === "Propietario").length}
            </p>
            <p className="text-xs text-muted-foreground font-medium">Propietarios Residentes</p>
          </div>
        </div>
        <div className="bg-card border border-border rounded-xl shadow-sm p-5 flex items-center gap-4">
          <div className="w-10 h-10 bg-emerald-50 dark:bg-emerald-950/20 rounded-xl flex items-center justify-center">
            <Users className="w-5 h-5 text-emerald-600 dark:text-emerald-400" />
          </div>
          <div>
            <p className="text-2xl font-bold text-foreground">
              {residents.filter((r) => r.role !== "Propietario" && r.role !== "Arrendatario").length}
            </p>
            <p className="text-xs text-muted-foreground font-medium">Grupo de Convivencia</p>
          </div>
        </div>
      </div>

      <div className="bg-card p-4 rounded-xl shadow-sm border border-border">
        <div className="relative">
          <div className="absolute inset-y-0 left-0 pl-3 flex items-center pointer-events-none">
            <Search className="h-5 w-5 text-muted-foreground" />
          </div>
          <input
            type="text"
            placeholder="Buscar por nombre, documento o unidad..."
            value={search}
            onChange={(e) => setSearch(e.target.value)}
            className="block w-full pl-10 pr-3 py-2.5 border border-border rounded-xl focus:ring-2 focus:ring-emerald-500/20 focus:border-emerald-500 text-sm bg-muted/50 focus:bg-card transition-all outline-none"
          />
        </div>
      </div>

      {apiError && (
        <div className="bg-rose-50 dark:bg-rose-950/20 border border-rose-200 dark:border-rose-900 rounded-xl px-4 py-3 text-sm font-semibold text-rose-700 dark:text-rose-400 flex items-center justify-between gap-4">
          <span>{apiError}</span>
          <button
            onClick={loadResidents}
            className="text-xs font-bold text-rose-600 dark:text-rose-400 underline underline-offset-2 hover:text-rose-700 shrink-0"
          >
            Reintentar
          </button>
        </div>
      )}

      <div className="bg-card rounded-xl shadow-sm border border-border overflow-hidden">
        <div className="overflow-x-auto">
          <table className="min-w-full divide-y divide-border">
            <thead className="bg-muted/50">
              <tr>
                <th className="px-6 py-4 text-left text-xs font-bold text-muted-foreground uppercase tracking-wider">
                  Nombre
                </th>
                <th className="px-6 py-4 text-left text-xs font-bold text-muted-foreground uppercase tracking-wider">
                  Documento
                </th>
                <th className="px-6 py-4 text-left text-xs font-bold text-muted-foreground uppercase tracking-wider">
                  Teléfono
                </th>
                <th className="px-6 py-4 text-left text-xs font-bold text-muted-foreground uppercase tracking-wider">
                  Rol / Relación
                </th>
                <th className="px-6 py-4 text-left text-xs font-bold text-muted-foreground uppercase tracking-wider">
                  Unidad
                </th>
              </tr>
            </thead>
            <tbody className="divide-y divide-border bg-card">
              {(() => {
                if (loading) {
                  return (
                    <tr>
                      <td colSpan={5} className="px-6 py-12 text-center">
                        <div className="animate-spin inline-block w-6 h-6 border-2 border-current border-t-transparent text-emerald-600 dark:text-emerald-400 rounded-full" />
                      </td>
                    </tr>
                  );
                }
                if (paginatedResidents.length === 0) {
                  return (
                    <tr>
                      <td colSpan={5} className="px-6 py-12 text-center text-muted-foreground">
                        <Users className="w-12 h-12 mx-auto text-muted-foreground/40 mb-3" />
                        <p className="font-semibold">No se encontraron residentes</p>
                        <p className="text-sm mt-1">
                          Registra propietarios residentes, arrendatarios o grupo de convivencia desde cada unidad.
                        </p>
                      </td>
                    </tr>
                  );
                }
                return paginatedResidents.map((r) => (
                  <tr key={r.id} className="hover:bg-muted/30 transition-colors">
                    <td className="px-6 py-4">
                      {(() => {
                        if (r.residentId) {
                          return (
                            <Link
                              href={`/residents/directory/${r.residentId}`}
                              className="text-sm font-semibold text-foreground hover:text-emerald-600 hover:underline"
                            >
                              {r.fullName}
                            </Link>
                          );
                        }
                        return <p className="text-sm font-semibold text-foreground">{r.fullName}</p>;
                      })()}
                    </td>
                    <td className="px-6 py-4">
                      <p className="text-sm text-muted-foreground">
                        {r.documentType ?? ""} {r.documentNumber ?? "—"}
                      </p>
                    </td>
                    <td className="px-6 py-4">
                      <p className="text-sm text-muted-foreground">{r.phone || "—"}</p>
                    </td>
                    <td className="px-6 py-4">{renderRoleBadge(r.role)}</td>
                    <td className="px-6 py-4">
                      <Link
                        href={`/units/${r.unitId}`}
                        className="inline-flex items-center gap-1.5 text-xs font-bold text-emerald-600 dark:text-emerald-400 bg-emerald-50 dark:bg-emerald-950/20 px-2.5 py-1 rounded-lg hover:bg-emerald-100 dark:hover:bg-emerald-900/30 transition-colors"
                      >
                        <Home className="w-3.5 h-3.5" />
                        {formatUnitLabel(r.unitIdentifier, r.unitTowerOrBlock)}
                      </Link>
                    </td>
                  </tr>
                ));
              })()}
            </tbody>
          </table>
        </div>

        {!loading && filtered.length > 0 && (
          <div className="px-6 py-4 border-t border-border flex flex-col sm:flex-row items-center justify-between gap-3">
            <p className="text-xs text-muted-foreground">
              Mostrando {rangeStart}-{rangeEnd} de {filtered.length} residentes
            </p>
            <div className="flex items-center gap-2">
              <button
                onClick={() => goToPage(currentPage - 1)}
                disabled={currentPage === 1}
                className="px-3 py-1.5 text-xs font-semibold text-muted-foreground bg-card border border-border rounded-lg hover:bg-muted/30 transition-colors disabled:opacity-40 disabled:cursor-not-allowed"
              >
                Anterior
              </button>
              <span className="text-xs text-muted-foreground">
                Página {currentPage} de {totalPages}
              </span>
              <button
                onClick={() => goToPage(currentPage + 1)}
                disabled={currentPage === totalPages}
                className="px-3 py-1.5 text-xs font-semibold text-muted-foreground bg-card border border-border rounded-lg hover:bg-muted/30 transition-colors disabled:opacity-40 disabled:cursor-not-allowed"
              >
                Siguiente
              </button>
            </div>
          </div>
        )}
      </div>
    </div>
  );
}
