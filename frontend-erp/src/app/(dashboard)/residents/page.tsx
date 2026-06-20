"use client";

import React, { useEffect, useState } from "react";
import { ResidentsService, OwnerSummary } from "@/lib/residents-service";
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

export default function ResidentsPage() {
  const [owners, setOwners] = useState<OwnerSummary[]>([]);
  const [loading, setLoading] = useState(true);
  const [search, setSearch] = useState("");
  const [includeInactive, setIncludeInactive] = useState(false);

  useEffect(() => {
    fetchOwners();
  }, [includeInactive]);

  const fetchOwners = async (query?: string) => {
    setLoading(true);
    try {
      const data = await ResidentsService.getOwners(query, includeInactive);
      setOwners(data);
    } catch {
      setOwners([]);
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

  return (
    <div className="space-y-6">
      <div className="flex flex-col sm:flex-row justify-between items-start sm:items-center gap-4">
        <div>
          <h1 className="text-2xl font-bold text-gray-900 tracking-tight">
            Residentes y Propietarios
          </h1>
          <p className="text-sm text-gray-500 mt-1">
            Gestiona propietarios, arrendatarios y grupos de convivencia del conjunto.
          </p>
        </div>
        <Link
          href="/residents/new"
          className="inline-flex items-center gap-2 bg-blue-600 hover:bg-blue-700 text-white px-4 py-2.5 rounded-xl text-sm font-semibold shadow-sm shadow-blue-200 transition-colors shrink-0"
        >
          <Plus className="w-5 h-5" />
          Registrar Propietario
        </Link>
      </div>

      <div className="grid grid-cols-1 sm:grid-cols-3 gap-4">
        <div className="bg-white border border-gray-100 rounded-xl shadow-sm p-5 flex items-center gap-4">
          <div className="w-10 h-10 bg-blue-50 rounded-xl flex items-center justify-center">
            <Users className="w-5 h-5 text-blue-600" />
          </div>
          <div>
            <p className="text-2xl font-bold text-gray-900">{naturalCount}</p>
            <p className="text-xs text-gray-500 font-medium">Personas Naturales</p>
          </div>
        </div>
        <div className="bg-white border border-gray-100 rounded-xl shadow-sm p-5 flex items-center gap-4">
          <div className="w-10 h-10 bg-indigo-50 rounded-xl flex items-center justify-center">
            <Building2 className="w-5 h-5 text-indigo-600" />
          </div>
          <div>
            <p className="text-2xl font-bold text-gray-900">{legalCount}</p>
            <p className="text-xs text-gray-500 font-medium">Personas Jurídicas</p>
          </div>
        </div>
        <div className="bg-white border border-gray-100 rounded-xl shadow-sm p-5 flex items-center gap-4">
          <div className="w-10 h-10 bg-emerald-50 rounded-xl flex items-center justify-center">
            <Star className="w-5 h-5 text-emerald-600" />
          </div>
          <div>
            <p className="text-2xl font-bold text-gray-900">{totalUnits}</p>
            <p className="text-xs text-gray-500 font-medium">Vinculaciones Activas</p>
          </div>
        </div>
      </div>

      <div className="bg-white p-4 rounded-xl shadow-sm border border-gray-100 flex flex-col sm:flex-row gap-4">
        <form onSubmit={handleSearch} className="relative flex-1 flex gap-2">
          <div className="relative flex-1">
            <div className="absolute inset-y-0 left-0 pl-3 flex items-center pointer-events-none">
              <Search className="h-5 w-5 text-gray-400" />
            </div>
            <input
              type="text"
              placeholder="Buscar por nombre, documento o correo..."
              value={search}
              onChange={(e) => setSearch(e.target.value)}
              className="block w-full pl-10 pr-3 py-2.5 border border-gray-200 rounded-xl focus:ring-2 focus:ring-blue-500/20 focus:border-blue-500 text-sm bg-gray-50 focus:bg-white transition-all outline-none"
            />
          </div>
          <button
            type="submit"
            className="px-4 py-2.5 bg-blue-600 hover:bg-blue-700 text-white rounded-xl text-sm font-semibold transition-colors shrink-0"
          >
            Buscar
          </button>
          {search && (
            <button
              type="button"
              onClick={handleClearSearch}
              className="px-4 py-2.5 bg-white border border-gray-200 text-gray-600 hover:bg-gray-50 rounded-xl text-sm font-semibold transition-colors shrink-0"
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
            className="w-4 h-4 rounded border-gray-300 text-blue-600 cursor-pointer"
          />
          <span className="text-sm text-gray-600 font-medium">Incluir inactivos</span>
        </label>
      </div>

      <div className="bg-white rounded-xl shadow-sm border border-gray-100 overflow-hidden">
        <div className="overflow-x-auto">
          <table className="min-w-full divide-y divide-gray-100">
            <thead className="bg-gray-50/50">
              <tr>
                <th className="px-6 py-4 text-left text-xs font-bold text-gray-500 uppercase tracking-wider">
                  Propietario
                </th>
                <th className="px-6 py-4 text-left text-xs font-bold text-gray-500 uppercase tracking-wider">
                  Contacto
                </th>
                <th className="px-6 py-4 text-left text-xs font-bold text-gray-500 uppercase tracking-wider">
                  Unidades
                </th>
                <th className="px-6 py-4 text-right text-xs font-bold text-gray-500 uppercase tracking-wider">
                  Acciones
                </th>
              </tr>
            </thead>
            <tbody className="divide-y divide-gray-100 bg-white">
              {loading ? (
                <tr>
                  <td colSpan={4} className="px-6 py-12 text-center">
                    <div className="animate-spin inline-block w-6 h-6 border-2 border-current border-t-transparent text-blue-600 rounded-full" />
                  </td>
                </tr>
              ) : owners.length === 0 ? (
                <tr>
                  <td colSpan={4} className="px-6 py-12 text-center text-gray-500">
                    <Users className="w-12 h-12 mx-auto text-gray-300 mb-3" />
                    <p className="font-semibold">No se encontraron propietarios</p>
                    <p className="text-sm mt-1 text-gray-400">
                      Prueba con otro término de búsqueda o registra un nuevo propietario.
                    </p>
                  </td>
                </tr>
              ) : (
                owners.map((owner) => {
                  const isLegal = owner.ownerType === "LegalEntity";
                  const docShort = DOC_SHORT[owner.documentType] ?? owner.documentType;
                  const isInactive = !owner.isActive;

                  return (
                    <tr
                      key={owner.id}
                      className={`hover:bg-gray-50/50 transition-colors ${isInactive ? "opacity-60" : ""}`}
                    >
                      <td className="px-6 py-4 whitespace-nowrap">
                        <div className="flex items-center gap-3">
                          <div
                            className={`w-10 h-10 rounded-full flex items-center justify-center shrink-0 ${
                              isLegal ? "bg-indigo-100 text-indigo-600" : "bg-blue-100 text-blue-600"
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
                              <p className="text-sm font-bold text-gray-900">
                                {owner.fullNameOrCompanyName}
                              </p>
                              {isInactive && (
                                <span className="px-1.5 py-0.5 bg-gray-100 text-gray-500 rounded text-xs font-semibold">
                                  Inactivo
                                </span>
                              )}
                            </div>
                            <p className="text-xs text-gray-500 mt-0.5">
                              {docShort} {owner.documentNumber} ·{" "}
                              {isLegal ? "Jurídica" : "Natural"}
                            </p>
                          </div>
                        </div>
                      </td>
                      <td className="px-6 py-4 whitespace-nowrap">
                        <p className="text-sm text-gray-900">{owner.email}</p>
                        <p className="text-xs text-gray-500 mt-0.5">{owner.mainPhone}</p>
                      </td>
                      <td className="px-6 py-4">
                        {owner.units.length > 0 ? (
                          <div className="flex flex-wrap gap-1.5">
                            {owner.units.slice(0, 3).map((u) => (
                              <Link key={u.assignmentId} href={`/units/${u.unitId}`}>
                                <span className="inline-flex items-center gap-1 px-2 py-0.5 bg-gray-100 text-gray-700 rounded-lg text-xs font-semibold hover:bg-blue-100 hover:text-blue-700 transition-colors">
                                  {u.unitIdentifier}
                                  {u.isSpokesperson && (
                                    <Star className="w-3 h-3 text-amber-500" />
                                  )}
                                </span>
                              </Link>
                            ))}
                            {owner.units.length > 3 && (
                              <span className="px-2 py-0.5 bg-gray-50 text-gray-500 rounded-lg text-xs font-semibold border border-gray-200">
                                +{owner.units.length - 3}
                              </span>
                            )}
                          </div>
                        ) : (
                          <span className="text-xs text-gray-400">Sin unidades</span>
                        )}
                      </td>
                      <td className="px-6 py-4 whitespace-nowrap text-right">
                        <Link
                          href={`/residents/${owner.id}`}
                          className="text-blue-600 hover:text-blue-900 text-sm font-semibold px-3 py-1.5 bg-blue-50 rounded-lg hover:bg-blue-100 transition-colors"
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
          <div className="px-6 py-3 border-t border-gray-100 bg-gray-50/30">
            <p className="text-xs text-gray-500">
              {owners.length} propietario{owners.length !== 1 ? "s" : ""} encontrado
              {owners.length !== 1 ? "s" : ""}
            </p>
          </div>
        )}
      </div>
    </div>
  );
}
