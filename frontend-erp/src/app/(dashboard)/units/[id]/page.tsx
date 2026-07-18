"use client";

import React, { useEffect, useState } from "react";
import { UnitsService, Unit } from "@/lib/units-service";
import Link from "next/link";
import { useParams } from "next/navigation";
import UnitOccupantsPanel from "@/components/residents/UnitOccupantsPanel";

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

  const renderStatusBadge = (status: number | string) => {
    const s = String(status).toLowerCase();
    if (s === "1" || s === "activeoccupied") return <span className="px-3 py-1 bg-blue-100 dark:bg-blue-950/30 text-blue-800 dark:text-blue-300 rounded-full text-xs font-bold uppercase tracking-wider">Activa y Ocupada</span>;
    if (s === "2" || s === "activeunoccupied") return <span className="px-3 py-1 bg-cyan-100 dark:bg-cyan-950/30 text-cyan-800 dark:text-cyan-300 rounded-full text-xs font-bold uppercase tracking-wider">Activa y Desocupada</span>;
    if (s === "3" || s === "deliveryprocess") return <span className="px-3 py-1 bg-yellow-100 dark:bg-yellow-950/30 text-yellow-800 dark:text-yellow-300 rounded-full text-xs font-bold uppercase tracking-wider">En Proceso de Entrega</span>;
    if (s === "4" || s === "litigation") return <span className="px-3 py-1 bg-purple-100 dark:bg-purple-950/30 text-purple-800 dark:text-purple-300 rounded-full text-xs font-bold uppercase tracking-wider">En Litigio</span>;
    if (s === "5" || s === "inactive") return <span className="px-3 py-1 bg-muted text-foreground rounded-full text-xs font-bold uppercase tracking-wider">Inactiva</span>;
    return <span className="px-3 py-1 bg-muted text-muted-foreground rounded-full text-xs font-bold uppercase tracking-wider">Desconocido</span>;
  };

  if (loading) {
    return (
      <div className="flex justify-center py-20">
        <div className="animate-spin rounded-full h-8 w-8 border-b-2 border-emerald-600"></div>
      </div>
    );
  }

  if (!unit) {
    return (
      <div className="bg-card rounded-xl shadow-sm border border-border p-10 text-center">
        <h2 className="text-xl font-bold text-foreground mb-2">Unidad no encontrada</h2>
        <p className="text-muted-foreground mb-6">La propiedad solicitada no pudo ser localizada en el sistema.</p>
        <Link href="/units" className="px-4 py-2 bg-emerald-600 hover:bg-emerald-700 text-white font-semibold rounded-lg transition-colors">Volver al Catálogo</Link>
      </div>
    );
  }

  return (
    <div className="space-y-6">
      <div className="flex items-center gap-4">
        <Link href="/units" className="w-10 h-10 flex items-center justify-center bg-card border border-border rounded-xl hover:bg-muted/30 transition-colors shadow-sm text-muted-foreground">
          ←
        </Link>
        <div>
          <h1 className="text-2xl font-bold text-foreground tracking-tight">
            {(() => {
              if (unit.towerOrBlock) {
                return `${unit.towerOrBlock} ${unit.unitTypeName} ${unit.identifier}`;
              }
              return `${unit.unitTypeName} ${unit.identifier}`;
            })()}
          </h1>
        </div>
        <div className="ml-auto">
          {renderStatusBadge(unit.status)}
        </div>
      </div>

      <div className="grid grid-cols-1 lg:grid-cols-3 gap-6">
        
        {/* Left Column: Core Info */}
        <div className="lg:col-span-2 space-y-6">
          <div className="bg-card rounded-xl shadow-sm border border-border overflow-hidden">
            <div className="px-6 py-4 border-b border-border bg-muted/50">
              <h3 className="font-bold text-foreground">Características Físicas</h3>
            </div>
            <div className="p-6 grid grid-cols-2 md:grid-cols-4 gap-6">
              <div>
                <p className="text-xs font-bold text-muted-foreground uppercase tracking-wider mb-1">Área Privada</p>
                <p className="text-lg font-semibold text-foreground">{unit.privateArea} m²</p>
              </div>
              <div>
                <p className="text-xs font-bold text-muted-foreground uppercase tracking-wider mb-1">Balcón</p>
                <p className="text-lg font-semibold text-foreground">{unit.balconyArea} m²</p>
              </div>
              <div>
                <p className="text-xs font-bold text-muted-foreground uppercase tracking-wider mb-1">Piso</p>
                <p className="text-lg font-semibold text-foreground">{unit.floorLevel}</p>
              </div>
              <div>
                <p className="text-xs font-bold text-muted-foreground uppercase tracking-wider mb-1">Coeficiente</p>
                <p className="text-lg font-mono font-bold text-emerald-600 dark:text-emerald-400 bg-emerald-50 dark:bg-emerald-950/20 inline-block px-2 py-0.5 rounded-md border border-emerald-100 dark:border-emerald-900">
                  {unit.coproprietyCoefficient.toFixed(4)}%
                </p>
              </div>
            </div>
          </div>

          <UnitOccupantsPanel unitId={id} />

          {/* Financials */}
          <div className="bg-card rounded-xl shadow-sm border border-border overflow-hidden">
            <div className="px-6 py-4 border-b border-border bg-muted/50 flex justify-between items-center">
              <h3 className="font-bold text-foreground">Historial Financiero</h3>
              <Link
                href={`/billing/documents?unitId=${id}`}
                className="text-xs font-semibold text-muted-foreground bg-card px-3 py-1.5 rounded-lg border border-border hover:bg-muted/30"
              >
                Ver Estado de Cuenta
              </Link>
            </div>
            <div className="p-6">
              <div className="flex justify-end gap-2">
                <Link
                  href={`/billing/extraordinary-fees/new?unitId=${id}`}
                  className="text-xs font-semibold text-muted-foreground bg-card px-3 py-1.5 rounded-lg border border-border hover:bg-muted/30 transition-colors"
                >
                  Cargar Deuda / Cuota Extraordinaria
                </Link>
                <Link
                  href={`/billing/payments/register?unitId=${id}`}
                  className="text-xs font-semibold text-white bg-emerald-600 hover:bg-emerald-700 px-3 py-1.5 rounded-lg transition-colors"
                >
                  Registrar Pago
                </Link>
              </div>
              <p className="text-xs text-center text-muted-foreground italic mt-4">Consulta los movimientos y saldos detallados en el Estado de Cuenta.</p>
            </div>
          </div>
        </div>

        {/* Right Column: Dependencies & Audit */}
        <div className="space-y-6">
          <div className="bg-card rounded-xl shadow-sm border border-border overflow-hidden">
            <div className="px-6 py-4 border-b border-border bg-muted/50">
              <h3 className="font-bold text-foreground">Complementos Asignados</h3>
            </div>
            <div className="p-6 space-y-4">
              <div className="flex items-start gap-3">
                <div className="w-8 h-8 rounded-lg bg-blue-50 dark:bg-blue-950/20 border border-blue-100 dark:border-blue-900 flex items-center justify-center text-blue-600 dark:text-blue-400 font-bold shrink-0">P</div>
                <div>
                  <p className="text-sm font-semibold text-foreground">Parqueadero Privado</p>
                  {(() => {
                    if (unit.hasPrivateParking) {
                      return <p className="text-xs text-muted-foreground mt-0.5 font-mono bg-muted inline-block px-1.5 rounded border border-border">{unit.parkingIdentifier}</p>;
                    }
                    return <p className="text-xs text-muted-foreground mt-0.5">No asignado</p>;
                  })()}
                </div>
              </div>

              <div className="flex items-start gap-3 pt-4 border-t border-border">
                <div className="w-8 h-8 rounded-lg bg-amber-50 dark:bg-amber-950/20 border border-amber-100 dark:border-amber-900 flex items-center justify-center text-amber-600 dark:text-amber-400 font-bold shrink-0">B</div>
                <div>
                  <p className="text-sm font-semibold text-foreground">Bodega / Depósito</p>
                  {(() => {
                    if (unit.hasAssignedStorage) {
                      return <p className="text-xs text-muted-foreground mt-0.5 font-mono bg-muted inline-block px-1.5 rounded border border-border">{unit.storageIdentifier}</p>;
                    }
                    return <p className="text-xs text-muted-foreground mt-0.5">No asignado</p>;
                  })()}
                </div>
              </div>
            </div>
          </div>

          <div className="bg-card rounded-xl shadow-sm border border-border overflow-hidden">
            <div className="px-6 py-4 border-b border-border bg-muted/50 flex justify-between items-center">
              <h3 className="font-bold text-foreground">Historial de Estados</h3>
            </div>
            <div className="p-6">
               <div className="relative border-l-2 border-border pl-4 ml-2 space-y-6">
                 {/* Mock history since we didn't build the history endpoint yet */}
                 <div className="relative">
                   <div className="absolute w-3 h-3 bg-emerald-500 rounded-full -left-[1.35rem] top-1 border-2 border-card"></div>
                   <p className="text-xs font-bold text-foreground">Estado Actual</p>
                   <p className="text-xs text-muted-foreground mt-0.5">Basado en tu última actualización.</p>
                 </div>
                 <div className="relative">
                   <div className="absolute w-3 h-3 bg-muted-foreground/40 rounded-full -left-[1.35rem] top-1 border-2 border-card"></div>
                   <p className="text-xs font-bold text-foreground">Unidad Creada</p>
                   <p className="text-xs text-muted-foreground mt-0.5">Inicialización en el sistema.</p>
                 </div>
               </div>
            </div>
          </div>

          <div className="bg-card rounded-xl shadow-sm border border-border overflow-hidden">
            <div className="px-6 py-4 border-b border-border bg-muted/50">
              <h3 className="font-bold text-foreground">Observaciones Internas</h3>
            </div>
            <div className="p-6">
              {(() => {
                if (unit.internalObservations) {
                  return <p className="text-sm text-muted-foreground italic">"{unit.internalObservations}"</p>;
                }
                return <p className="text-sm text-muted-foreground italic">No hay observaciones internas registradas para esta unidad.</p>;
              })()}
            </div>
          </div>

        </div>
      </div>
    </div>
  );
}
