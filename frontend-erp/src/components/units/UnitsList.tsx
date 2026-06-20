"use client";

import React, { useEffect, useState } from "react";
import { UnitsService, Unit } from "@/lib/units-service";
import Link from "next/link";
import UnitForm from "./UnitForm";
import BulkImport from "./BulkImport";

export default function UnitsList() {
  const [units, setUnits] = useState<Unit[]>([]);
  const [loading, setLoading] = useState(true);
  
  const [filterTower, setFilterTower] = useState("");
  const [filterStatus, setFilterStatus] = useState("");
  const [filterArrears, setFilterArrears] = useState(""); // financial condition

  const [showCreateForm, setShowCreateForm] = useState(false);
  const [showBulkImport, setShowBulkImport] = useState(false);
  const [unitToEdit, setUnitToEdit] = useState<Unit | undefined>(undefined);

  useEffect(() => {
    fetchUnits();
  }, [filterTower, filterStatus]);

  const fetchUnits = async () => {
    setLoading(true);
    try {
      const data = await UnitsService.getUnits(filterTower, filterStatus);
      setUnits(data);
    } catch (error) {
      console.error("Failed to fetch units:", error);
    } finally {
      setLoading(false);
    }
  };

  const handleCreateNew = () => {
    setUnitToEdit(undefined);
    setShowCreateForm(true);
    setShowBulkImport(false);
  };

  const handleEdit = (unit: Unit) => {
    setUnitToEdit(unit);
    setShowCreateForm(true);
    setShowBulkImport(false);
  };

  const handleBulkImport = () => {
    setShowBulkImport(true);
    setShowCreateForm(false);
  };

  const handleFormSuccess = () => {
    setShowCreateForm(false);
    setShowBulkImport(false);
    fetchUnits();
  };

  const handleFormCancel = () => {
    setShowCreateForm(false);
    setShowBulkImport(false);
  };

  const renderStatusBadge = (status: number | string) => {
    const s = String(status).toLowerCase();
    if (s === "1" || s === "activeoccupied") return <span className="px-2 py-1 bg-blue-100 text-blue-700 rounded-lg text-xs font-semibold">Activa y Ocupada</span>;
    if (s === "2" || s === "activeunoccupied") return <span className="px-2 py-1 bg-cyan-100 text-cyan-700 rounded-lg text-xs font-semibold">Activa y Desocupada</span>;
    if (s === "3" || s === "deliveryprocess") return <span className="px-2 py-1 bg-yellow-100 text-yellow-700 rounded-lg text-xs font-semibold">En Proceso de Entrega</span>;
    if (s === "4" || s === "litigation") return <span className="px-2 py-1 bg-purple-100 text-purple-700 rounded-lg text-xs font-semibold">En Litigio</span>;
    if (s === "5" || s === "inactive") return <span className="px-2 py-1 bg-gray-100 text-gray-700 rounded-lg text-xs font-semibold">Inactiva</span>;
    return <span className="px-2 py-1 bg-gray-200 text-gray-800 rounded-lg text-xs font-semibold">Desconocido ({String(status)})</span>;
  };

  const filteredUnits = units.filter(u => {
    if (filterArrears === "mora") {
      // Mock logic: randomly simulate someone in arrears if id contains 'a'
      return u.identifier.toLowerCase().includes('a') === false;
    }
    if (filterArrears === "aldia") {
      return u.identifier.toLowerCase().includes('a') === true;
    }
    return true;
  });

  if (showCreateForm) {
    return <UnitForm initialData={unitToEdit} onSuccess={handleFormSuccess} onCancel={handleFormCancel} />;
  }

  if (showBulkImport) {
    return <BulkImport onSuccess={handleFormSuccess} onCancel={handleFormCancel} />;
  }

  return (
    <div className="bg-white rounded-xl shadow-sm border border-gray-100 overflow-hidden">
      <div className="p-6 border-b border-gray-100 flex flex-col md:flex-row md:items-center justify-between gap-4">
        <h2 className="text-xl font-bold text-gray-800">Catálogo de Propiedades</h2>
        
        <div className="flex gap-3">
          <button
            onClick={handleBulkImport}
            className="px-4 py-2 bg-gray-100 hover:bg-gray-200 text-gray-700 text-sm font-semibold rounded-lg transition-colors"
          >
            Importación Masiva (CSV)
          </button>
          <button
            onClick={handleCreateNew}
            className="px-4 py-2 bg-blue-600 hover:bg-blue-700 text-white text-sm font-semibold rounded-lg transition-colors shadow-sm"
          >
            + Crear Unidad
          </button>
        </div>
      </div>

      <div className="p-6 bg-gray-50/50 border-b border-gray-100">
        <div className="grid grid-cols-1 md:grid-cols-3 gap-4">
          <div>
            <label className="block text-xs font-semibold text-gray-500 uppercase tracking-wider mb-1">Filtrar por Torre/Bloque</label>
            <input
              type="text"
              placeholder="Ej. Torre A"
              className="w-full px-3 py-2 border border-gray-200 rounded-lg text-sm focus:ring-2 focus:ring-blue-500/20 focus:border-blue-500 outline-none"
              value={filterTower}
              onChange={(e) => setFilterTower(e.target.value)}
            />
          </div>
          <div>
            <label className="block text-xs font-semibold text-gray-500 uppercase tracking-wider mb-1">Filtrar por Estado</label>
            <select
              className="w-full px-3 py-2 border border-gray-200 rounded-lg text-sm focus:ring-2 focus:ring-blue-500/20 focus:border-blue-500 outline-none"
              value={filterStatus}
              onChange={(e) => setFilterStatus(e.target.value)}
            >
              <option value="">Todos los Estados</option>
              <option value="1">Activa y Ocupada</option>
              <option value="2">Activa y Desocupada</option>
              <option value="3">En Proceso de Entrega</option>
              <option value="4">En Litigio</option>
              <option value="5">Inactiva</option>
            </select>
          </div>
          <div>
            <label className="block text-xs font-semibold text-gray-500 uppercase tracking-wider mb-1">Condición Financiera</label>
            <select
              className="w-full px-3 py-2 border border-gray-200 rounded-lg text-sm focus:ring-2 focus:ring-blue-500/20 focus:border-blue-500 outline-none"
              value={filterArrears}
              onChange={(e) => setFilterArrears(e.target.value)}
            >
              <option value="">Todos</option>
              <option value="aldia">Al Día (100% Recaudo)</option>
              <option value="mora">En Mora (Recaudo &lt; 100%)</option>
            </select>
          </div>
        </div>
      </div>

      <div className="overflow-x-auto">
        <table className="w-full text-left border-collapse">
          <thead>
            <tr className="bg-white border-b border-gray-200">
              <th className="px-6 py-4 text-xs font-bold text-gray-500 uppercase tracking-wider">Identificador</th>
              <th className="px-6 py-4 text-xs font-bold text-gray-500 uppercase tracking-wider">Tipo / Torre</th>
              <th className="px-6 py-4 text-xs font-bold text-gray-500 uppercase tracking-wider">Coeficiente</th>
              <th className="px-6 py-4 text-xs font-bold text-gray-500 uppercase tracking-wider">Estado</th>
              <th className="px-6 py-4 text-xs font-bold text-gray-500 uppercase tracking-wider">Financiero</th>
              <th className="px-6 py-4 text-xs font-bold text-gray-500 uppercase tracking-wider text-right">Acciones</th>
            </tr>
          </thead>
          <tbody className="divide-y divide-gray-100">
            {(() => {
              if (loading) {
                return (
                  <tr>
                    <td colSpan={6} className="px-6 py-8 text-center text-gray-500">Cargando unidades...</td>
                  </tr>
                );
              }
              if (filteredUnits.length === 0) {
                return (
                  <tr>
                    <td colSpan={6} className="px-6 py-12 text-center text-gray-500">
                      <p className="text-sm">No se encontraron unidades con esos criterios.</p>
                    </td>
                  </tr>
                );
              }
              return filteredUnits.map((u) => {
                const isUpToDate = u.identifier.toLowerCase().includes('a'); // mocked logic

                return (
                  <tr key={u.id} className="hover:bg-gray-50/50 transition-colors">
                    <td className="px-6 py-4">
                      <div className="font-semibold text-gray-900">{u.identifier}</div>
                      <div className="text-xs text-gray-500 mt-1">Piso {u.floorLevel} - Área {u.privateArea}m²</div>
                    </td>
                    <td className="px-6 py-4">
                      <div className="text-sm text-gray-800">{u.unitTypeName}</div>
                      <div className="text-xs text-gray-500 mt-1">{u.towerOrBlock}</div>
                    </td>
                    <td className="px-6 py-4">
                      <div className="text-sm font-mono font-medium text-blue-600">{u.coproprietyCoefficient.toFixed(4)}%</div>
                    </td>
                    <td className="px-6 py-4">
                      {renderStatusBadge(u.status)}
                    </td>
                    <td className="px-6 py-4">
                      {(() => {
                        if (isUpToDate) {
                          return <span className="inline-flex items-center gap-1.5 text-xs font-semibold text-green-700"><span className="w-2 h-2 rounded-full bg-green-500"></span>Al Día (100%)</span>;
                        }
                        return <span className="inline-flex items-center gap-1.5 text-xs font-semibold text-red-700"><span className="w-2 h-2 rounded-full bg-red-500"></span>En Mora</span>;
                      })()}
                    </td>
                    <td className="px-6 py-4 text-right">
                      <div className="flex justify-end gap-2">
                        <Link href={`/units/${u.id}`} className="text-xs font-semibold text-blue-600 hover:text-blue-800 bg-blue-50 hover:bg-blue-100 px-3 py-1.5 rounded-lg transition-colors">
                          Detalles
                        </Link>
                        <button onClick={() => handleEdit(u)} className="text-xs font-semibold text-gray-600 hover:text-gray-800 bg-gray-100 hover:bg-gray-200 px-3 py-1.5 rounded-lg transition-colors">
                          Editar
                        </button>
                      </div>
                    </td>
                  </tr>
                );
              });
            })()}
          </tbody>
        </table>
      </div>
    </div>
  );
}
