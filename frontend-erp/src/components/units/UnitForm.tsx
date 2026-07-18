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

  const [identifierAvailability, setIdentifierAvailability] = useState<{
    checked: boolean;
    isAvailable: boolean;
    message: string | null;
  }>({ checked: false, isAvailable: true, message: null });

  useEffect(() => {
    loadDependencies();
  }, []);

  useEffect(() => {
    if (identifier.trim() === "") {
      setIdentifierAvailability({ checked: false, isAvailable: true, message: null });
      return;
    }

    let isCancelled = false;

    const timeoutId = setTimeout(async () => {
      try {
        let excludeUnitId: string | undefined = undefined;
        if (initialData) {
          excludeUnitId = initialData.id;
        }

        const availability = await UnitsService.checkIdentifierAvailability(identifier, towerOrBlock, excludeUnitId);
        if (!isCancelled) {
          setIdentifierAvailability({
            checked: true,
            isAvailable: availability.isAvailable,
            message: availability.message
          });
        }
      } catch (err) {
        if (!isCancelled) {
          setIdentifierAvailability({ checked: false, isAvailable: true, message: null });
        }
      }
    }, 500);

    return () => {
      isCancelled = true;
      clearTimeout(timeoutId);
    };
  }, [identifier, towerOrBlock, initialData]);

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
    if (identifierAvailability.checked && !identifierAvailability.isAvailable) {
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

  const getIdentifierAvailabilityMessage = () => {
    if (!identifierAvailability.checked) {
      return null;
    }
    if (identifierAvailability.isAvailable) {
      return null;
    }
    return identifierAvailability.message;
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
    <div className="bg-card rounded-lg shadow-lg border border-border">
      <div className="px-6 py-4 border-b border-border flex justify-between items-center bg-muted/50">
        <h3 className="text-lg font-semibold text-foreground">
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
          <div className="mb-6 bg-rose-50 dark:bg-rose-950/20 text-rose-700 dark:text-rose-400 p-4 rounded-xl text-sm font-medium border border-rose-100 dark:border-rose-900 flex items-center gap-3 shadow-sm">
            <span>{error}</span>
          </div>
        )}

        <div className="grid grid-cols-1 md:grid-cols-2 gap-6 mb-8">
          <div>
            <label className="block text-sm font-semibold text-muted-foreground mb-2">Identificador de la Unidad</label>
            <input
              type="text"
              required
              className="w-full px-4 py-2.5 bg-muted/50 border border-border rounded-xl focus:bg-card focus:ring-2 focus:ring-emerald-500/20 focus:border-emerald-500 transition-all text-foreground"
              value={identifier}
              onChange={(e) => setIdentifier(e.target.value)}
              placeholder="Ej. Apto 101"
            />
            {getIdentifierAvailabilityMessage() && (
              <p className="mt-2 text-xs font-medium text-rose-600 dark:text-rose-400">
                {getIdentifierAvailabilityMessage()}
              </p>
            )}
          </div>

          <div>
            <label className="block text-sm font-semibold text-muted-foreground mb-2">Tipo de Unidad</label>
            <div className="flex gap-2">
              <select
                required
                className="flex-1 pl-4 pr-9 py-2.5 bg-muted/50 border border-border rounded-xl focus:bg-card focus:ring-2 focus:ring-emerald-500/20 focus:border-emerald-500 transition-all text-foreground"
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
                className="px-4 py-2.5 bg-emerald-50 dark:bg-emerald-950/20 text-emerald-600 dark:text-emerald-400 font-semibold rounded-xl border border-emerald-100 dark:border-emerald-900 hover:bg-emerald-100 dark:hover:bg-emerald-900/30 transition-colors"
                title="Añadir nuevo tipo"
              >
                +
              </button>
            </div>
          </div>

          <div>
            <label className="block text-sm font-semibold text-muted-foreground mb-2">Torre o Bloque</label>
            <input
              type="text"
              className="w-full px-4 py-2.5 bg-muted/50 border border-border rounded-xl focus:bg-card focus:ring-2 focus:ring-emerald-500/20 focus:border-emerald-500 transition-all text-foreground"
              value={towerOrBlock}
              onChange={(e) => setTowerOrBlock(e.target.value)}
              placeholder="Ej. Torre A"
            />
          </div>

          <div>
            <label className="block text-sm font-semibold text-muted-foreground mb-2">Piso</label>
            <input
              type="number"
              required
              className="w-full px-4 py-2.5 bg-muted/50 border border-border rounded-xl focus:bg-card focus:ring-2 focus:ring-emerald-500/20 focus:border-emerald-500 transition-all text-foreground"
              value={floorLevel}
              onChange={(e) => setFloorLevel(Number(e.target.value))}
            />
          </div>
        </div>

        <h4 className="text-md font-semibold text-foreground mb-4 pb-2 border-b border-border">Áreas y Coeficientes</h4>
        
        <div className="grid grid-cols-1 md:grid-cols-3 gap-6 mb-8">
          <div>
            <label className="block text-sm font-semibold text-muted-foreground mb-2">Área Privada (m2)</label>
            <input
              type="number"
              step="0.01"
              required
              className="w-full px-4 py-2.5 bg-muted/50 border border-border rounded-xl focus:bg-card focus:ring-2 focus:ring-emerald-500/20 focus:border-emerald-500 transition-all text-foreground"
              value={privateArea}
              onChange={(e) => setPrivateArea(Number(e.target.value))}
            />
          </div>

          <div>
            <label className="block text-sm font-semibold text-muted-foreground mb-2">Área de Balcón (m2)</label>
            <input
              type="number"
              step="0.01"
              required
              className="w-full px-4 py-2.5 bg-muted/50 border border-border rounded-xl focus:bg-card focus:ring-2 focus:ring-emerald-500/20 focus:border-emerald-500 transition-all text-foreground"
              value={balconyArea}
              onChange={(e) => setBalconyArea(Number(e.target.value))}
            />
          </div>

          <div>
            <label className="block text-sm font-semibold text-muted-foreground mb-2">Coeficiente de Copropiedad (%)</label>
            <input
              type="number"
              step="0.0001"
              required
              className="w-full px-4 py-2.5 bg-muted/50 border border-border rounded-xl focus:bg-card focus:ring-2 focus:ring-emerald-500/20 focus:border-emerald-500 transition-all text-foreground"
              value={coproprietyCoefficient}
              onChange={(e) => setCoproprietyCoefficient(Number(e.target.value))}
            />
          </div>
        </div>

        {summary && (
          <div className="mb-8 p-4 bg-blue-50 dark:bg-blue-950/20 rounded-xl border border-blue-100 dark:border-blue-900">
            <p className="text-sm font-medium text-blue-800 dark:text-blue-300">
              {getCoefficientStatusMessage()}
            </p>
          </div>
        )}

        <h4 className="text-md font-semibold text-foreground mb-4 pb-2 border-b border-border">Estado y Complementos</h4>

        <div className="grid grid-cols-1 md:grid-cols-2 gap-6 mb-8">
          <div>
            <label className="block text-sm font-semibold text-muted-foreground mb-2">Estado Actual</label>
            <select
              required
              className="w-full pl-4 pr-9 py-2.5 bg-muted/50 border border-border rounded-xl focus:bg-card focus:ring-2 focus:ring-emerald-500/20 focus:border-emerald-500 transition-all text-foreground"
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
            <label className="block text-sm font-semibold text-muted-foreground mb-2">Fecha de Entrega</label>
            <input
              type="date"
              className="w-full px-4 py-2.5 bg-muted/50 border border-border rounded-xl focus:bg-card focus:ring-2 focus:ring-emerald-500/20 focus:border-emerald-500 transition-all text-foreground"
              value={constructionDeliveryDate}
              onChange={(e) => setConstructionDeliveryDate(e.target.value)}
            />
          </div>
        </div>

        {initialData && (
          <div className="mb-8">
            <label className="block text-sm font-semibold text-muted-foreground mb-2">Motivo del Cambio</label>
            <input
              type="text"
              required
              className="w-full px-4 py-2.5 bg-muted/50 border border-border rounded-xl focus:bg-card focus:ring-2 focus:ring-emerald-500/20 focus:border-emerald-500 transition-all text-foreground"
              value={reasonForChange}
              onChange={(e) => setReasonForChange(e.target.value)}
              placeholder="Proporciona el motivo para actualizar esta unidad..."
            />
          </div>
        )}

        <div className="grid grid-cols-1 md:grid-cols-2 gap-6 mb-8">
          <div className="bg-muted/50 p-4 rounded-xl border border-border">
            <div className="flex items-center mb-4">
              <input
                type="checkbox"
                id="hasPrivateParking"
                className="w-5 h-5 text-emerald-600 dark:text-emerald-400 bg-card border-border rounded focus:ring-emerald-500"
                checked={hasPrivateParking}
                onChange={(e) => setHasPrivateParking(e.target.checked)}
              />
              <label htmlFor="hasPrivateParking" className="ml-3 text-sm font-medium text-muted-foreground">Tiene Parqueadero Privado</label>
            </div>
            
            {hasPrivateParking && (
              <div>
                <label className="block text-xs font-semibold text-muted-foreground uppercase tracking-wider mb-2">Identificador de Parqueadero</label>
                <input
                  type="text"
                  required
                  className="w-full px-4 py-2.5 bg-card border border-border rounded-xl focus:ring-2 focus:ring-emerald-500/20 focus:border-emerald-500 transition-all text-foreground text-sm"
                  value={parkingIdentifier}
                  onChange={(e) => setParkingIdentifier(e.target.value)}
                  placeholder="Ej. P-12"
                />
              </div>
            )}
          </div>

          <div className="bg-muted/50 p-4 rounded-xl border border-border">
            <div className="flex items-center mb-4">
              <input
                type="checkbox"
                id="hasAssignedStorage"
                className="w-5 h-5 text-emerald-600 dark:text-emerald-400 bg-card border-border rounded focus:ring-emerald-500"
                checked={hasAssignedStorage}
                onChange={(e) => setHasAssignedStorage(e.target.checked)}
              />
              <label htmlFor="hasAssignedStorage" className="ml-3 text-sm font-medium text-muted-foreground">Tiene Bodega / Depósito</label>
            </div>
            
            {hasAssignedStorage && (
              <div>
                <label className="block text-xs font-semibold text-muted-foreground uppercase tracking-wider mb-2">Identificador de Bodega</label>
                <input
                  type="text"
                  required
                  className="w-full px-4 py-2.5 bg-card border border-border rounded-xl focus:ring-2 focus:ring-emerald-500/20 focus:border-emerald-500 transition-all text-foreground text-sm"
                  value={storageIdentifier}
                  onChange={(e) => setStorageIdentifier(e.target.value)}
                  placeholder="Ej. B-05"
                />
              </div>
            )}
          </div>
        </div>

        <div className="mb-8">
          <label className="block text-sm font-semibold text-muted-foreground mb-2">Observaciones Internas</label>
          <textarea
            rows={3}
            className="w-full px-4 py-2.5 bg-muted/50 border border-border rounded-xl focus:bg-card focus:ring-2 focus:ring-emerald-500/20 focus:border-emerald-500 transition-all text-foreground resize-none"
            value={internalObservations}
            onChange={(e) => setInternalObservations(e.target.value)}
            placeholder="Notas privadas visibles solo para administradores..."
          />
        </div>

        <div className="flex justify-end gap-3 pt-6 border-t border-border">
          <button
            type="button"
            onClick={onCancel}
            className="px-6 py-2.5 text-sm font-semibold text-muted-foreground bg-card border border-border rounded-xl hover:bg-muted/30 transition-colors"
            disabled={isSubmitting}
          >
            Cancelar
          </button>
          <button
            type="submit"
            disabled={isSubmitDisabled()}
            className="px-6 py-2.5 text-sm font-semibold text-white bg-emerald-600 rounded-xl hover:bg-emerald-700 transition-colors disabled:opacity-50 disabled:cursor-not-allowed shadow-sm shadow-emerald-200"
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
