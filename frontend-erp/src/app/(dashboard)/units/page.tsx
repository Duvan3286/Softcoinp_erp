import React from "react";
import CoefficientSummaryPanel from "@/components/units/CoefficientSummaryPanel";
import UnitsList from "@/components/units/UnitsList";

export const metadata = {
  title: "Properties Catalog - Softcoinp ERP",
};

export default function UnitsPage() {
  return (
    <div className="space-y-6">
      <div>
        <h1 className="text-2xl font-bold text-gray-900 tracking-tight">Catálogo de Unidades</h1>
        <p className="text-sm text-gray-500 mt-1">
          Gestiona todas las propiedades, sus coeficientes y visualiza el estado matemático del conjunto.
        </p>
      </div>

      <CoefficientSummaryPanel />
      
      <UnitsList />
    </div>
  );
}
