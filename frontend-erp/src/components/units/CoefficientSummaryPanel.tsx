"use client";

import React, { useEffect, useState } from "react";
import { UnitsService, UnitCoefficientSummary } from "@/lib/units-service";

export default function CoefficientSummaryPanel() {
  const [summary, setSummary] = useState<UnitCoefficientSummary | null>(null);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    fetchSummary();
  }, []);

  const fetchSummary = async () => {
    try {
      const data = await UnitsService.getCoefficientSummary();
      setSummary(data);
    } catch (error) {
      console.error("Error fetching summary:", error);
    } finally {
      setLoading(false);
    }
  };

  if (loading) {
    return (
      <div className="bg-card p-6 rounded-xl shadow-sm border border-border flex justify-center items-center h-32">
        <div className="animate-pulse flex space-x-2">
          <div className="w-3 h-3 bg-emerald-400 rounded-full"></div>
          <div className="w-3 h-3 bg-emerald-400 rounded-full"></div>
          <div className="w-3 h-3 bg-emerald-400 rounded-full"></div>
        </div>
      </div>
    );
  }

  if (!summary) {
    return null;
  }

  const { totalCoefficient, pendingCoefficient, excessCoefficient, isExactlyOneHundred } = summary;

  return (
    <div className="bg-card rounded-xl shadow-sm border border-border overflow-hidden mb-6">
      <div className="px-6 py-4 border-b border-border flex items-center justify-between bg-muted/50">
        <h3 className="text-lg font-semibold text-foreground">Resumen de Coeficientes</h3>
        {(() => {
          if (isExactlyOneHundred) {
            return (
              <span className="px-3 py-1 bg-emerald-100 dark:bg-emerald-950/30 text-emerald-700 dark:text-emerald-400 text-xs font-bold rounded-full">
                Perfectamente Balanceado
              </span>
            );
          }
          return (
            <span className="px-3 py-1 bg-amber-100 dark:bg-amber-950/30 text-amber-700 dark:text-amber-400 text-xs font-bold rounded-full">
              Requiere Atención
            </span>
          );
        })()}
      </div>

      <div className="p-6 grid grid-cols-1 md:grid-cols-3 gap-6">
        <div className="bg-emerald-50 dark:bg-emerald-950/20 rounded-xl p-5 border border-emerald-100 dark:border-emerald-900 flex flex-col justify-center items-center text-center">
          <p className="text-sm font-semibold text-emerald-600 dark:text-emerald-400 uppercase tracking-wide mb-1">Total Activo</p>
          <p className="text-4xl font-bold text-foreground">{totalCoefficient.toFixed(4)}%</p>
        </div>

        {(() => {
          if (pendingCoefficient > 0) {
            return (
              <div className="bg-amber-50 dark:bg-amber-950/20 rounded-xl p-5 border border-amber-100 dark:border-amber-900 flex flex-col justify-center items-center text-center">
                <p className="text-sm font-semibold text-amber-600 dark:text-amber-400 uppercase tracking-wide mb-1">Faltante</p>
                <p className="text-4xl font-bold text-foreground">{pendingCoefficient.toFixed(4)}%</p>
                <p className="text-xs text-amber-700 dark:text-amber-400 mt-2">Deben crearse unidades para llegar al 100%</p>
              </div>
            );
          }

          if (excessCoefficient > 0) {
            return (
              <div className="bg-rose-50 dark:bg-rose-950/20 rounded-xl p-5 border border-rose-100 dark:border-rose-900 flex flex-col justify-center items-center text-center">
                <p className="text-sm font-semibold text-rose-600 dark:text-rose-400 uppercase tracking-wide mb-1">Exceso</p>
                <p className="text-4xl font-bold text-foreground">{excessCoefficient.toFixed(4)}%</p>
                <p className="text-xs text-rose-700 dark:text-rose-400 mt-2">Por favor corrige la distribución de coeficientes</p>
              </div>
            );
          }

          return (
            <div className="bg-emerald-50 dark:bg-emerald-950/20 rounded-xl p-5 border border-emerald-100 dark:border-emerald-900 flex flex-col justify-center items-center text-center col-span-2">
              <p className="text-sm font-semibold text-emerald-600 dark:text-emerald-400 uppercase tracking-wide mb-1">Invariante Cumplido</p>
              <p className="text-2xl font-bold text-foreground mt-2">Todas las unidades están sincronizadas matemáticamente.</p>
            </div>
          );
        })()}
      </div>
    </div>
  );
}
