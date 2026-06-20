"use client";

import React, { useState, useEffect } from "react";
import { UnitsService, Unit, UnitType, UnitCoefficientSummary } from "@/lib/units-service";

interface UnitFormProps {
  initialData?: Unit;
  onSuccess: () => void;
  onCancel: () => void;
}

export default function UnitForm({ initialData, onSuccess, onCancel }: UnitFormProps) {
  const [unitTypes, setUnitTypes] = useState<UnitType[]>([]);
  const [summary, setSummary] = useState<UnitCoefficientSummary | null>(null);
  
  const [identifier, setIdentifier] = useState(initialData?.identifier || "");
  const [unitTypeId, setUnitTypeId] = useState(initialData?.unitTypeId || "");
  const [towerOrBlock, setTowerOrBlock] = useState(initialData?.towerOrBlock || "");
  const [floorLevel, setFloorLevel] = useState(initialData?.floorLevel || 1);
  const [privateArea, setPrivateArea] = useState(initialData?.privateArea || 0);
  const [balconyArea, setBalconyArea] = useState(initialData?.balconyArea || 0);
  const [coproprietyCoefficient, setCoproprietyCoefficient] = useState(initialData?.coproprietyCoefficient || 0);
  const mapStatusToInt = (st: string | number) => {
    const s = String(st).toLowerCase();
    if (s === "1" || s === "activeoccupied") return 1;
    if (s === "2" || s === "activeunoccupied") return 2;
    if (s === "3" || s === "deliveryprocess") return 3;
    if (s === "4" || s === "litigation") return 4;
    if (s === "5" || s === "inactive") return 5;
    return 3;
  };
  const [status, setStatus] = useState<number>(initialData?.status ? mapStatusToInt(initialData.status) : 3);
  const [hasPrivateParking, setHasPrivateParking] = useState(initialData?.hasPrivateParking || false);
  const [parkingIdentifier, setParkingIdentifier] = useState(initialData?.parkingIdentifier || "");
  const [hasAssignedStorage, setHasAssignedStorage] = useState(initialData?.hasAssignedStorage || false);
  const [storageIdentifier, setStorageIdentifier] = useState(initialData?.storageIdentifier || "");
  const [constructionDeliveryDate, setConstructionDeliveryDate] = useState(initialData?.constructionDeliveryDate?.substring(0, 10) || "");
  const [internalObservations, setInternalObservations] = useState(initialData?.internalObservations || "");
  const [reasonForChange, setReasonForChange] = useState("");
  
  const [error, setError] = useState<string | null>(null);
  const [isSubmitting, setIsSubmitting] = useState(false);

  useEffect(() => {
    loadDependencies();
  }, []);

  const loadDependencies = async () => {
    try {
      const types = await UnitsService.getTypes();
      setUnitTypes(types);
      if (!initialData && types.length > 0) {
        setUnitTypeId(types[0].id);
      }
      
      const sum = await UnitsService.getCoefficientSummary();
      setSummary(sum);
    } catch (err: any) {
      setError("Error loading dependencies from server.");
    }
  };

  const calculateRemainingCoefficient = () => {
    if (!summary) {
      return 0;
    }
    
    let baseTotal = summary.totalCoefficient;
    if (initialData) {
      baseTotal = baseTotal - initialData.coproprietyCoefficient;
    }
    
    let newPending = 100 - baseTotal - coproprietyCoefficient;
    return newPending;
  };

  const getCoefficientStatusMessage = () => {
    const remaining = calculateRemainingCoefficient();
    
    if (status === 5) {
      return "La unidad está inactiva. El coeficiente no afectará la suma total.";
    }

    if (remaining > 0) {
      return `Pendiente por asignar en el conjunto: ${remaining.toFixed(4)}%`;
    }
    
    if (remaining < 0) {
      const excess = Math.abs(remaining);
      return `Error: El coeficiente excede el 100% por ${excess.toFixed(4)}%`;
    }

    return "El coeficiente total es exactamente 100%.";
  };

  const isSubmitDisabled = () => {
    if (isSubmitting) {
      return true;
    }
    if (status !== 5) {
      const remaining = calculateRemainingCoefficient();
      if (remaining < 0) {
        return true;
      }
    }
    return false;
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setError(null);
    setIsSubmitting(true);

    try {
      const payload: Partial<Unit> = {
        identifier,
        unitTypeId,
        towerOrBlock,
        floorLevel,
        privateArea,
        balconyArea,
        coproprietyCoefficient,
        status,
        hasPrivateParking,
        parkingIdentifier,
        hasAssignedStorage,
        storageIdentifier,
        internalObservations
      };

      if (constructionDeliveryDate !== "") {
        payload.constructionDeliveryDate = new Date(constructionDeliveryDate).toISOString();
      }

      if (initialData) {
        await UnitsService.updateUnit(initialData.id, { ...payload, reasonForChange });
      } else {
        await UnitsService.createUnit(payload);
      }

      onSuccess();
    } catch (err: any) {
      if (err.response && err.response.data) {
        setError(err.response.data);
      } else {
        setError("An unexpected error occurred while saving the unit.");
      }
      setIsSubmitting(false);
    }
  };

  return (
    <div className="bg-white rounded-lg shadow-lg border border-gray-100">
      <div className="px-6 py-4 border-b border-gray-100 flex justify-between items-center bg-gray-50/50">
        <h3 className="text-lg font-semibold text-gray-800">
          {(() => {
            if (initialData) {
              return "Editar Unidad";
            }
            return "Crear Unidad";
          })()}
        </h3>
      </div>
      
      <form onSubmit={handleSubmit} className="p-6">
        {error && (
          <div className="mb-6 bg-red-50 text-red-700 p-4 rounded-xl text-sm font-medium border border-red-100 flex items-center gap-3 shadow-sm">
            <span>{error}</span>
          </div>
        )}

        <div className="grid grid-cols-1 md:grid-cols-2 gap-6 mb-8">
          <div>
            <label className="block text-sm font-semibold text-gray-700 mb-2">Identificador de la Unidad</label>
            <input
              type="text"
              required
              className="w-full px-4 py-2.5 bg-gray-50 border border-gray-200 rounded-xl focus:bg-white focus:ring-2 focus:ring-blue-500/20 focus:border-blue-500 transition-all text-gray-900"
              value={identifier}
              onChange={(e) => setIdentifier(e.target.value)}
              placeholder="Ej. Apto 101"
            />
          </div>

          <div>
            <label className="block text-sm font-semibold text-gray-700 mb-2">Tipo de Unidad</label>
            <div className="flex gap-2">
              <select
                required
                className="flex-1 px-4 py-2.5 bg-gray-50 border border-gray-200 rounded-xl focus:bg-white focus:ring-2 focus:ring-blue-500/20 focus:border-blue-500 transition-all text-gray-900"
                value={unitTypeId}
                onChange={(e) => setUnitTypeId(e.target.value)}
              >
                <option value="" disabled>Seleccione o cree uno nuevo</option>
                {unitTypes.map((t) => (
                  <option key={t.id} value={t.id}>{t.name}</option>
                ))}
              </select>
              <button
                type="button"
                onClick={async () => {
                  const name = window.prompt("Ingrese el nombre del nuevo tipo de unidad (ej. Apartamento, Local):");
                  if (name && name.trim()) {
                    try {
                      const newType = await UnitsService.createType(name.trim());
                      setUnitTypes([...unitTypes, newType]);
                      setUnitTypeId(newType.id);
                    } catch (err) {
                      alert("Error al crear el tipo de unidad.");
                    }
                  }
                }}
                className="px-4 py-2.5 bg-blue-50 text-blue-600 font-semibold rounded-xl border border-blue-100 hover:bg-blue-100 transition-colors"
                title="Añadir nuevo tipo"
              >
                +
              </button>
            </div>
          </div>

          <div>
            <label className="block text-sm font-semibold text-gray-700 mb-2">Torre o Bloque</label>
            <input
              type="text"
              className="w-full px-4 py-2.5 bg-gray-50 border border-gray-200 rounded-xl focus:bg-white focus:ring-2 focus:ring-blue-500/20 focus:border-blue-500 transition-all text-gray-900"
              value={towerOrBlock}
              onChange={(e) => setTowerOrBlock(e.target.value)}
              placeholder="Ej. Torre A"
            />
          </div>

          <div>
            <label className="block text-sm font-semibold text-gray-700 mb-2">Piso</label>
            <input
              type="number"
              required
              className="w-full px-4 py-2.5 bg-gray-50 border border-gray-200 rounded-xl focus:bg-white focus:ring-2 focus:ring-blue-500/20 focus:border-blue-500 transition-all text-gray-900"
              value={floorLevel}
              onChange={(e) => setFloorLevel(Number(e.target.value))}
            />
          </div>
        </div>

        <h4 className="text-md font-semibold text-gray-800 mb-4 pb-2 border-b border-gray-100">Áreas y Coeficientes</h4>
        
        <div className="grid grid-cols-1 md:grid-cols-3 gap-6 mb-8">
          <div>
            <label className="block text-sm font-semibold text-gray-700 mb-2">Área Privada (m2)</label>
            <input
              type="number"
              step="0.01"
              required
              className="w-full px-4 py-2.5 bg-gray-50 border border-gray-200 rounded-xl focus:bg-white focus:ring-2 focus:ring-blue-500/20 focus:border-blue-500 transition-all text-gray-900"
              value={privateArea}
              onChange={(e) => setPrivateArea(Number(e.target.value))}
            />
          </div>

          <div>
            <label className="block text-sm font-semibold text-gray-700 mb-2">Área de Balcón (m2)</label>
            <input
              type="number"
              step="0.01"
              required
              className="w-full px-4 py-2.5 bg-gray-50 border border-gray-200 rounded-xl focus:bg-white focus:ring-2 focus:ring-blue-500/20 focus:border-blue-500 transition-all text-gray-900"
              value={balconyArea}
              onChange={(e) => setBalconyArea(Number(e.target.value))}
            />
          </div>

          <div>
            <label className="block text-sm font-semibold text-gray-700 mb-2">Coeficiente de Copropiedad (%)</label>
            <input
              type="number"
              step="0.0001"
              required
              className="w-full px-4 py-2.5 bg-gray-50 border border-gray-200 rounded-xl focus:bg-white focus:ring-2 focus:ring-blue-500/20 focus:border-blue-500 transition-all text-gray-900"
              value={coproprietyCoefficient}
              onChange={(e) => setCoproprietyCoefficient(Number(e.target.value))}
            />
          </div>
        </div>

        {summary && (
          <div className="mb-8 p-4 bg-blue-50 rounded-xl border border-blue-100">
            <p className="text-sm font-medium text-blue-800">
              {getCoefficientStatusMessage()}
            </p>
          </div>
        )}

        <h4 className="text-md font-semibold text-gray-800 mb-4 pb-2 border-b border-gray-100">Estado y Complementos</h4>

        <div className="grid grid-cols-1 md:grid-cols-2 gap-6 mb-8">
          <div>
            <label className="block text-sm font-semibold text-gray-700 mb-2">Estado Actual</label>
            <select
              required
              className="w-full px-4 py-2.5 bg-gray-50 border border-gray-200 rounded-xl focus:bg-white focus:ring-2 focus:ring-blue-500/20 focus:border-blue-500 transition-all text-gray-900"
              value={status}
              onChange={(e) => setStatus(Number(e.target.value))}
            >
              <option value={1}>Activa y Ocupada</option>
              <option value={2}>Activa y Desocupada</option>
              <option value={3}>En Proceso de Entrega</option>
              <option value={4}>En Litigio</option>
              <option value={5}>Inactiva</option>
            </select>
          </div>

          <div>
            <label className="block text-sm font-semibold text-gray-700 mb-2">Fecha de Entrega</label>
            <input
              type="date"
              className="w-full px-4 py-2.5 bg-gray-50 border border-gray-200 rounded-xl focus:bg-white focus:ring-2 focus:ring-blue-500/20 focus:border-blue-500 transition-all text-gray-900"
              value={constructionDeliveryDate}
              onChange={(e) => setConstructionDeliveryDate(e.target.value)}
            />
          </div>
        </div>

        {initialData && (
          <div className="mb-8">
            <label className="block text-sm font-semibold text-gray-700 mb-2">Motivo del Cambio</label>
            <input
              type="text"
              required
              className="w-full px-4 py-2.5 bg-gray-50 border border-gray-200 rounded-xl focus:bg-white focus:ring-2 focus:ring-blue-500/20 focus:border-blue-500 transition-all text-gray-900"
              value={reasonForChange}
              onChange={(e) => setReasonForChange(e.target.value)}
              placeholder="Proporciona el motivo para actualizar esta unidad..."
            />
          </div>
        )}

        <div className="grid grid-cols-1 md:grid-cols-2 gap-6 mb-8">
          <div className="bg-gray-50 p-4 rounded-xl border border-gray-100">
            <div className="flex items-center mb-4">
              <input
                type="checkbox"
                id="hasPrivateParking"
                className="w-5 h-5 text-blue-600 bg-white border-gray-300 rounded focus:ring-blue-500"
                checked={hasPrivateParking}
                onChange={(e) => setHasPrivateParking(e.target.checked)}
              />
              <label htmlFor="hasPrivateParking" className="ml-3 text-sm font-medium text-gray-700">Tiene Parqueadero Privado</label>
            </div>
            
            {hasPrivateParking && (
              <div>
                <label className="block text-xs font-semibold text-gray-500 uppercase tracking-wider mb-2">Identificador de Parqueadero</label>
                <input
                  type="text"
                  required
                  className="w-full px-4 py-2.5 bg-white border border-gray-200 rounded-xl focus:ring-2 focus:ring-blue-500/20 focus:border-blue-500 transition-all text-gray-900 text-sm"
                  value={parkingIdentifier}
                  onChange={(e) => setParkingIdentifier(e.target.value)}
                  placeholder="Ej. P-12"
                />
              </div>
            )}
          </div>

          <div className="bg-gray-50 p-4 rounded-xl border border-gray-100">
            <div className="flex items-center mb-4">
              <input
                type="checkbox"
                id="hasAssignedStorage"
                className="w-5 h-5 text-blue-600 bg-white border-gray-300 rounded focus:ring-blue-500"
                checked={hasAssignedStorage}
                onChange={(e) => setHasAssignedStorage(e.target.checked)}
              />
              <label htmlFor="hasAssignedStorage" className="ml-3 text-sm font-medium text-gray-700">Tiene Bodega / Depósito</label>
            </div>
            
            {hasAssignedStorage && (
              <div>
                <label className="block text-xs font-semibold text-gray-500 uppercase tracking-wider mb-2">Identificador de Bodega</label>
                <input
                  type="text"
                  required
                  className="w-full px-4 py-2.5 bg-white border border-gray-200 rounded-xl focus:ring-2 focus:ring-blue-500/20 focus:border-blue-500 transition-all text-gray-900 text-sm"
                  value={storageIdentifier}
                  onChange={(e) => setStorageIdentifier(e.target.value)}
                  placeholder="Ej. B-05"
                />
              </div>
            )}
          </div>
        </div>

        <div className="mb-8">
          <label className="block text-sm font-semibold text-gray-700 mb-2">Observaciones Internas</label>
          <textarea
            rows={3}
            className="w-full px-4 py-2.5 bg-gray-50 border border-gray-200 rounded-xl focus:bg-white focus:ring-2 focus:ring-blue-500/20 focus:border-blue-500 transition-all text-gray-900 resize-none"
            value={internalObservations}
            onChange={(e) => setInternalObservations(e.target.value)}
            placeholder="Notas privadas visibles solo para administradores..."
          />
        </div>

        <div className="flex justify-end gap-3 pt-6 border-t border-gray-100">
          <button
            type="button"
            onClick={onCancel}
            className="px-6 py-2.5 text-sm font-semibold text-gray-600 bg-white border border-gray-200 rounded-xl hover:bg-gray-50 transition-colors"
            disabled={isSubmitting}
          >
            Cancelar
          </button>
          <button
            type="submit"
            disabled={isSubmitDisabled()}
            className="px-6 py-2.5 text-sm font-semibold text-white bg-blue-600 rounded-xl hover:bg-blue-700 transition-colors disabled:opacity-50 disabled:cursor-not-allowed shadow-sm shadow-blue-200"
          >
            {(() => {
              if (isSubmitting) {
                return "Guardando...";
              }
              return "Guardar Unidad";
            })()}
          </button>
        </div>
      </form>
    </div>
  );
}
