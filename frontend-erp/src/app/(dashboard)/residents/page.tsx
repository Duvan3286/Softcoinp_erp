"use client";

import React, { useEffect, useState } from "react";
import { ResidentsService, OwnerSummary } from "@/lib/residents-service";
import { formatUnitLabel } from "@/lib/units-service";
import Link from "next/link";
import { Plus, Search, Users, Building2, Star } from "lucide-react";

const DOC_SHORT: Record<string, string> = {
  CitizenshipCard: "CC",
  ForeignerID: "CE",
  NIT: "NIT",
  Passport: "Pas.",
  PEP: "PEP",
  PPT: "PPT",
};

const PAGE_SIZE = 15;

export default function ResidentsPage() {
  const [owners, setOwners] = useState<OwnerSummary[]>([]);
  const [loading, setLoading] = useState(true);
  const [search, setSearch] = useState("");
  const [includeInactive, setIncludeInactive] = useState(false);
  const [currentPage, setCurrentPage] = useState(1);

  useEffect(() => {
    fetchOwners();
  }, [includeInactive]);

  const fetchOwners = async (query?: string) => {
    setLoading(true);
    try {
      const data = await ResidentsService.getOwners(query, includeInactive);
      setOwners(data);
      setCurrentPage(1);
    } catch {
      setOwners([]);
      setCurrentPage(1);
    } finally {
      setLoading(false);
    }
  };

  const handleSearch = (e: React.FormEvent) => {
    e.preventDefault();
    fetchOwners(search || undefined);
  };

  const handleClearSearch = () => {
    setSearch("");
    fetchOwners(undefined);
  };

  const naturalCount = owners.filter((o) => o.ownerType === "NaturalPerson").length;
  const legalCount = owners.filter((o) => o.ownerType === "LegalEntity").length;
  const totalUnits = owners.reduce((acc, o) => acc + o.units.length, 0);

  const totalPages = Math.max(1, Math.ceil(owners.length / PAGE_SIZE));
  const paginatedOwners = owners.slice((currentPage - 1) * PAGE_SIZE, currentPage * PAGE_SIZE);
  const rangeStart = owners.length === 0 ? 0 : (currentPage - 1) * PAGE_SIZE + 1;
  const rangeEnd = Math.min(currentPage * PAGE_SIZE, owners.length);

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

  return (
    <div className="space-y-6">
      <div className="flex flex-col sm:flex-row justify-between items-start sm:items-center gap-4">
        <div>
          <h1 className="text-2xl font-bold text-foreground tracking-tight">
            Residentes y Propietarios
          </h1>
          <p className="text-sm text-muted-foreground mt-1">
            Gestiona propietarios, arrendatarios y grupos de convivencia del conjunto.
          </p>
        </div>
        <Link
          href="/residents/new"
          className="inline-flex items-center gap-2 bg-emerald-600 hover:bg-emerald-700 text-white px-4 py-2.5 rounded-xl text-sm font-semibold shadow-sm shadow-emerald-200 transition-colors shrink-0"
        >
          <Plus className="w-5 h-5" />
          Registrar Propietario
        </Link>
      </div>

      <div className="grid grid-cols-1 sm:grid-cols-3 gap-4">
        <div className="bg-card border border-border rounded-xl shadow-sm p-5 flex items-center gap-4">
          <div className="w-10 h-10 bg-blue-50 dark:bg-blue-950/20 rounded-xl flex items-center justify-center">
            <Users className="w-5 h-5 text-blue-600 dark:text-blue-400" />
          </div>
          <div>
            <p className="text-2xl font-bold text-foreground">{naturalCount}</p>
            <p className="text-xs text-muted-foreground font-medium">Personas Naturales</p>
          </div>
        </div>
        <div className="bg-card border border-border rounded-xl shadow-sm p-5 flex items-center gap-4">
          <div className="w-10 h-10 bg-indigo-50 dark:bg-indigo-950/20 rounded-xl flex items-center justify-center">
            <Building2 className="w-5 h-5 text-indigo-600 dark:text-indigo-400" />
          </div>
          <div>
            <p className="text-2xl font-bold text-foreground">{legalCount}</p>
            <p className="text-xs text-muted-foreground font-medium">Personas Jurídicas</p>
          </div>
        </div>
        <div className="bg-card border border-border rounded-xl shadow-sm p-5 flex items-center gap-4">
          <div className="w-10 h-10 bg-emerald-50 rounded-xl flex items-center justify-center">
            <Star className="w-5 h-5 text-emerald-600" />
          </div>
          <div>
            <p className="text-2xl font-bold text-foreground">{totalUnits}</p>
            <p className="text-xs text-muted-foreground font-medium">Vinculaciones Activas</p>
          </div>
        </div>
      </div>

      <div className="bg-card p-4 rounded-xl shadow-sm border border-border flex flex-col sm:flex-row gap-4">
        <form onSubmit={handleSearch} className="relative flex-1 flex gap-2">
          <div className="relative flex-1">
            <div className="absolute inset-y-0 left-0 pl-3 flex items-center pointer-events-none">
              <Search className="h-5 w-5 text-muted-foreground" />
            </div>
            <input
              type="text"
              placeholder="Buscar por nombre, documento o correo..."
              value={search}
              onChange={(e) => setSearch(e.target.value)}
              className="block w-full pl-10 pr-3 py-2.5 border border-border rounded-xl focus:ring-2 focus:ring-emerald-500/20 focus:border-emerald-500 text-sm bg-muted/50 focus:bg-card transition-all outline-none"
            />
          </div>
          <button
            type="submit"
            className="px-4 py-2.5 bg-emerald-600 hover:bg-emerald-700 text-white rounded-xl text-sm font-semibold transition-colors shrink-0"
          >
            Buscar
          </button>
          {search && (
            <button
              type="button"
              onClick={handleClearSearch}
              className="px-4 py-2.5 bg-card border border-border text-muted-foreground hover:bg-muted/30 rounded-xl text-sm font-semibold transition-colors shrink-0"
            >
              Limpiar
            </button>
          )}
        </form>

        <label className="flex items-center gap-2 cursor-pointer shrink-0 select-none">
          <input
            type="checkbox"
            checked={includeInactive}
            onChange={(e) => setIncludeInactive(e.target.checked)}
            className="w-4 h-4 rounded border-border text-emerald-600 dark:text-emerald-400 cursor-pointer"
          />
          <span className="text-sm text-muted-foreground font-medium">Incluir inactivos</span>
        </label>
      </div>

      <div className="bg-card rounded-xl shadow-sm border border-border overflow-hidden">
        <div className="overflow-x-auto">
          <table className="min-w-full divide-y divide-border">
            <thead className="bg-muted/50">
              <tr>
                <th className="px-6 py-4 text-left text-xs font-bold text-muted-foreground uppercase tracking-wider">
                  Propietario
                </th>
                <th className="px-6 py-4 text-left text-xs font-bold text-muted-foreground uppercase tracking-wider">
                  Contacto
                </th>
                <th className="px-6 py-4 text-left text-xs font-bold text-muted-foreground uppercase tracking-wider">
                  Unidades
                </th>
                <th className="px-6 py-4 text-right text-xs font-bold text-muted-foreground uppercase tracking-wider">
                  Acciones
                </th>
              </tr>
            </thead>
            <tbody className="divide-y divide-border bg-card">
              {loading ? (
                <tr>
                  <td colSpan={4} className="px-6 py-12 text-center">
                    <div className="animate-spin inline-block w-6 h-6 border-2 border-current border-t-transparent text-emerald-600 dark:text-emerald-400 rounded-full" />
                  </td>
                </tr>
              ) : owners.length === 0 ? (
                <tr>
                  <td colSpan={4} className="px-6 py-12 text-center text-muted-foreground">
                    <Users className="w-12 h-12 mx-auto text-muted-foreground/40 mb-3" />
                    <p className="font-semibold">No se encontraron propietarios</p>
                    <p className="text-sm mt-1 text-muted-foreground">
                      Prueba con otro término de búsqueda o registra un nuevo propietario.
                    </p>
                  </td>
                </tr>
              ) : (
                paginatedOwners.map((owner) => {
                  const isLegal = owner.ownerType === "LegalEntity";
                  const docShort = DOC_SHORT[owner.documentType] ?? owner.documentType;
                  const isInactive = !owner.isActive;

                  return (
                    <tr
                      key={owner.id}
                      className={`hover:bg-muted/30 transition-colors ${isInactive ? "opacity-60" : ""}`}
                    >
                      <td className="px-6 py-4 whitespace-nowrap">
                        <div className="flex items-center gap-3">
                          <div
                            className={`w-10 h-10 rounded-full flex items-center justify-center shrink-0 ${
                              isLegal ? "bg-indigo-100 dark:bg-indigo-950/30 text-indigo-600 dark:text-indigo-400" : "bg-blue-100 dark:bg-blue-950/30 text-blue-600 dark:text-blue-400"
                            }`}
                          >
                            {isLegal ? (
                              <Building2 className="w-5 h-5" />
                            ) : (
                              <Users className="w-5 h-5" />
                            )}
                          </div>
                          <div>
                            <div className="flex items-center gap-2">
                              <p className="text-sm font-bold text-foreground">
                                {owner.fullNameOrCompanyName}
                              </p>
                              {isInactive && (
                                <span className="px-1.5 py-0.5 bg-muted text-muted-foreground rounded text-xs font-semibold">
                                  Inactivo
                                </span>
                              )}
                            </div>
                            <p className="text-xs text-muted-foreground mt-0.5">
                              {docShort} {owner.documentNumber} ·{" "}
                              {isLegal ? "Jurídica" : "Natural"}
                            </p>
                          </div>
                        </div>
                      </td>
                      <td className="px-6 py-4 whitespace-nowrap">
                        <p className="text-sm text-foreground">{owner.email}</p>
                        <p className="text-xs text-muted-foreground mt-0.5">{owner.mainPhone}</p>
                      </td>
                      <td className="px-6 py-4">
                        {owner.units.length > 0 ? (
                          <div className="flex flex-wrap gap-1.5">
                            {owner.units.slice(0, 3).map((u) => (
                              <Link key={u.assignmentId} href={`/units/${u.unitId}`}>
                                <span className="inline-flex items-center gap-1 px-2 py-0.5 bg-muted text-muted-foreground rounded-lg text-xs font-semibold hover:bg-emerald-100 dark:hover:bg-emerald-900/30 hover:text-emerald-700 dark:hover:text-emerald-400 transition-colors">
                                  {formatUnitLabel(u.unitIdentifier, u.unitTowerOrBlock)}
                                  {u.isSpokesperson && (
                                    <Star className="w-3 h-3 text-amber-500" />
                                  )}
                                </span>
                              </Link>
                            ))}
                            {owner.units.length > 3 && (
                              <span className="px-2 py-0.5 bg-muted/50 text-muted-foreground rounded-lg text-xs font-semibold border border-border">
                                +{owner.units.length - 3}
                              </span>
                            )}
                          </div>
                        ) : (
                          <span className="text-xs text-muted-foreground">Sin unidades</span>
                        )}
                      </td>
                      <td className="px-6 py-4 whitespace-nowrap text-right">
                        <Link
                          href={`/residents/${owner.id}`}
                          className="text-emerald-600 dark:text-emerald-400 hover:text-emerald-900 dark:hover:text-emerald-300 text-sm font-semibold px-3 py-1.5 bg-emerald-50 dark:bg-emerald-950/20 rounded-lg hover:bg-emerald-100 dark:hover:bg-emerald-900/30 transition-colors"
                        >
                          Ver Detalle
                        </Link>
                      </td>
                    </tr>
                  );
                })
              )}
            </tbody>
          </table>
        </div>

        {!loading && owners.length > 0 && (
          <div className="px-6 py-4 border-t border-border flex flex-col sm:flex-row items-center justify-between gap-3">
            <p className="text-xs text-muted-foreground">
              Mostrando {rangeStart}-{rangeEnd} de {owners.length} propietarios
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
