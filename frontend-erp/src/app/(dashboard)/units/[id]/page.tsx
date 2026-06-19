"use client";

import React, { useEffect, useState } from "react";
import { UnitsService, Unit } from "@/lib/units-service";
import Link from "next/link";
import { useParams } from "next/navigation";

export default function UnitDetailsPage() {
  const params = useParams();
  const id = params.id as string;
  const [unit, setUnit] = useState<Unit | null>(null);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    fetchUnitDetails();
  }, [id]);

  const fetchUnitDetails = async () => {
    try {
      const data = await UnitsService.getUnit(id);
      setUnit(data);
    } catch (error) {
      console.error("Failed to fetch unit details", error);
    } finally {
      setLoading(false);
    }
  };

  const renderStatusBadge = (status: number) => {
    if (status === 1) return <span className="px-3 py-1 bg-blue-100 text-blue-800 rounded-full text-xs font-bold uppercase tracking-wider">Activa y Ocupada</span>;
    if (status === 2) return <span className="px-3 py-1 bg-cyan-100 text-cyan-800 rounded-full text-xs font-bold uppercase tracking-wider">Activa y Desocupada</span>;
    if (status === 3) return <span className="px-3 py-1 bg-yellow-100 text-yellow-800 rounded-full text-xs font-bold uppercase tracking-wider">En Proceso de Entrega</span>;
    if (status === 4) return <span className="px-3 py-1 bg-purple-100 text-purple-800 rounded-full text-xs font-bold uppercase tracking-wider">En Litigio</span>;
    if (status === 5) return <span className="px-3 py-1 bg-gray-100 text-gray-800 rounded-full text-xs font-bold uppercase tracking-wider">Inactiva</span>;
    return <span>Desconocido</span>;
  };

  if (loading) {
    return (
      <div className="flex justify-center py-20">
        <div className="animate-spin rounded-full h-8 w-8 border-b-2 border-blue-600"></div>
      </div>
    );
  }

  if (!unit) {
    return (
      <div className="bg-white rounded-xl shadow-sm border border-gray-100 p-10 text-center">
        <h2 className="text-xl font-bold text-gray-800 mb-2">Unidad no encontrada</h2>
        <p className="text-gray-500 mb-6">La propiedad solicitada no pudo ser localizada en el sistema.</p>
        <Link href="/units" className="px-4 py-2 bg-blue-600 text-white font-semibold rounded-lg">Volver al Catálogo</Link>
      </div>
    );
  }

  return (
    <div className="space-y-6">
      <div className="flex items-center gap-4">
        <Link href="/units" className="w-10 h-10 flex items-center justify-center bg-white border border-gray-200 rounded-xl hover:bg-gray-50 transition-colors shadow-sm text-gray-500">
          ←
        </Link>
        <div>
          <h1 className="text-2xl font-bold text-gray-900 tracking-tight">Unidad {unit.identifier}</h1>
          <p className="text-sm text-gray-500 mt-0.5">{unit.unitTypeName} • {unit.towerOrBlock}</p>
        </div>
        <div className="ml-auto">
          {renderStatusBadge(unit.status)}
        </div>
      </div>

      <div className="grid grid-cols-1 lg:grid-cols-3 gap-6">
        
        {/* Left Column: Core Info */}
        <div className="lg:col-span-2 space-y-6">
          <div className="bg-white rounded-xl shadow-sm border border-gray-100 overflow-hidden">
            <div className="px-6 py-4 border-b border-gray-100 bg-gray-50/50">
              <h3 className="font-bold text-gray-800">Características Físicas</h3>
            </div>
            <div className="p-6 grid grid-cols-2 md:grid-cols-4 gap-6">
              <div>
                <p className="text-xs font-bold text-gray-500 uppercase tracking-wider mb-1">Área Privada</p>
                <p className="text-lg font-semibold text-gray-900">{unit.privateArea} m²</p>
              </div>
              <div>
                <p className="text-xs font-bold text-gray-500 uppercase tracking-wider mb-1">Balcón</p>
                <p className="text-lg font-semibold text-gray-900">{unit.balconyArea} m²</p>
              </div>
              <div>
                <p className="text-xs font-bold text-gray-500 uppercase tracking-wider mb-1">Piso</p>
                <p className="text-lg font-semibold text-gray-900">{unit.floorLevel}</p>
              </div>
              <div>
                <p className="text-xs font-bold text-gray-500 uppercase tracking-wider mb-1">Coeficiente</p>
                <p className="text-lg font-mono font-bold text-blue-600 bg-blue-50 inline-block px-2 py-0.5 rounded-md border border-blue-100">
                  {unit.coproprietyCoefficient.toFixed(4)}%
                </p>
              </div>
            </div>
          </div>

          {/* Residents Mock */}
          <div className="bg-white rounded-xl shadow-sm border border-gray-100 overflow-hidden">
            <div className="px-6 py-4 border-b border-gray-100 bg-gray-50/50 flex justify-between items-center">
              <h3 className="font-bold text-gray-800">Residentes y Propietarios</h3>
              <button className="text-xs font-semibold text-blue-600 bg-blue-50 px-3 py-1.5 rounded-lg border border-blue-100 hover:bg-blue-100">Gestionar</button>
            </div>
            <div className="p-6 flex flex-col items-center justify-center text-center">
              <div className="w-12 h-12 bg-gray-100 rounded-full flex items-center justify-center mb-3">
                <span className="text-gray-400">👥</span>
              </div>
              <p className="text-sm font-semibold text-gray-800">Sin residentes vinculados</p>
              <p className="text-xs text-gray-500 mt-1">Este módulo mostrará a los propietarios e inquilinos una vez se integre el CRM.</p>
            </div>
          </div>

          {/* Financials Mock */}
          <div className="bg-white rounded-xl shadow-sm border border-gray-100 overflow-hidden">
            <div className="px-6 py-4 border-b border-gray-100 bg-gray-50/50 flex justify-between items-center">
              <h3 className="font-bold text-gray-800">Historial Financiero (Resumen)</h3>
              <button className="text-xs font-semibold text-gray-600 bg-white px-3 py-1.5 rounded-lg border border-gray-200 hover:bg-gray-50">Ver Estado de Cuenta</button>
            </div>
            <div className="p-6">
              <div className="bg-green-50 border border-green-100 rounded-xl p-4 flex justify-between items-center mb-4">
                <div>
                  <p className="text-sm font-bold text-green-800">Balance Actual</p>
                  <p className="text-xs text-green-700 mt-0.5">Al día (100% Recaudo)</p>
                </div>
                <div className="text-xl font-bold text-green-700">$0.00</div>
              </div>
              <p className="text-xs text-center text-gray-500 italic mt-4">El historial de transacciones está deshabilitado hasta finalizar el Módulo Financiero.</p>
            </div>
          </div>
        </div>

        {/* Right Column: Dependencies & Audit */}
        <div className="space-y-6">
          <div className="bg-white rounded-xl shadow-sm border border-gray-100 overflow-hidden">
            <div className="px-6 py-4 border-b border-gray-100 bg-gray-50/50">
              <h3 className="font-bold text-gray-800">Complementos Asignados</h3>
            </div>
            <div className="p-6 space-y-4">
              <div className="flex items-start gap-3">
                <div className="w-8 h-8 rounded-lg bg-blue-50 border border-blue-100 flex items-center justify-center text-blue-600 font-bold shrink-0">P</div>
                <div>
                  <p className="text-sm font-semibold text-gray-800">Parqueadero Privado</p>
                  {(() => {
                    if (unit.hasPrivateParking) {
                      return <p className="text-xs text-gray-600 mt-0.5 font-mono bg-gray-100 inline-block px-1.5 rounded border border-gray-200">{unit.parkingIdentifier}</p>;
                    }
                    return <p className="text-xs text-gray-400 mt-0.5">No asignado</p>;
                  })()}
                </div>
              </div>

              <div className="flex items-start gap-3 pt-4 border-t border-gray-100">
                <div className="w-8 h-8 rounded-lg bg-amber-50 border border-amber-100 flex items-center justify-center text-amber-600 font-bold shrink-0">B</div>
                <div>
                  <p className="text-sm font-semibold text-gray-800">Bodega / Depósito</p>
                  {(() => {
                    if (unit.hasAssignedStorage) {
                      return <p className="text-xs text-gray-600 mt-0.5 font-mono bg-gray-100 inline-block px-1.5 rounded border border-gray-200">{unit.storageIdentifier}</p>;
                    }
                    return <p className="text-xs text-gray-400 mt-0.5">No asignado</p>;
                  })()}
                </div>
              </div>
            </div>
          </div>

          <div className="bg-white rounded-xl shadow-sm border border-gray-100 overflow-hidden">
            <div className="px-6 py-4 border-b border-gray-100 bg-gray-50/50 flex justify-between items-center">
              <h3 className="font-bold text-gray-800">Historial de Estados</h3>
            </div>
            <div className="p-6">
               <div className="relative border-l-2 border-gray-100 pl-4 ml-2 space-y-6">
                 {/* Mock history since we didn't build the history endpoint yet */}
                 <div className="relative">
                   <div className="absolute w-3 h-3 bg-blue-500 rounded-full -left-[1.35rem] top-1 border-2 border-white"></div>
                   <p className="text-xs font-bold text-gray-800">Estado Actual</p>
                   <p className="text-xs text-gray-500 mt-0.5">Basado en tu última actualización.</p>
                 </div>
                 <div className="relative">
                   <div className="absolute w-3 h-3 bg-gray-300 rounded-full -left-[1.35rem] top-1 border-2 border-white"></div>
                   <p className="text-xs font-bold text-gray-800">Unidad Creada</p>
                   <p className="text-xs text-gray-500 mt-0.5">Inicialización en el sistema.</p>
                 </div>
               </div>
            </div>
          </div>

          <div className="bg-white rounded-xl shadow-sm border border-gray-100 overflow-hidden">
            <div className="px-6 py-4 border-b border-gray-100 bg-gray-50/50">
              <h3 className="font-bold text-gray-800">Observaciones Internas</h3>
            </div>
            <div className="p-6">
              {(() => {
                if (unit.internalObservations) {
                  return <p className="text-sm text-gray-600 italic">"{unit.internalObservations}"</p>;
                }
                return <p className="text-sm text-gray-400 italic">No hay observaciones internas registradas para esta unidad.</p>;
              })()}
            </div>
          </div>

        </div>
      </div>
    </div>
  );
}
