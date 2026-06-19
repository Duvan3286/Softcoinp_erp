"use client";

import React, { useState } from "react";
import { UnitsService } from "@/lib/units-service";

interface BulkImportProps {
  onSuccess: () => void;
  onCancel: () => void;
}

export default function BulkImport({ onSuccess, onCancel }: BulkImportProps) {
  const [file, setFile] = useState<File | null>(null);
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [errors, setErrors] = useState<string[]>([]);
  const [globalMessage, setGlobalMessage] = useState<string | null>(null);

  const handleFileChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    if (e.target.files && e.target.files.length > 0) {
      setFile(e.target.files[0]);
      setErrors([]);
      setGlobalMessage(null);
    }
  };

  const handleUpload = async () => {
    if (!file) {
      setGlobalMessage("Por favor, selecciona un archivo CSV primero.");
      return;
    }

    setIsSubmitting(true);
    setErrors([]);
    setGlobalMessage(null);

    try {
      const response = await UnitsService.bulkImport(file);
      setGlobalMessage(response.message);
      
      // Delay before closing on success
      setTimeout(() => {
        onSuccess();
      }, 2000);
      
    } catch (err: any) {
      setIsSubmitting(false);
      
      if (err.message) {
        setGlobalMessage(err.message);
      } else {
        setGlobalMessage("Ocurrió un error inesperado.");
      }

      if (err.errors && Array.isArray(err.errors)) {
        setErrors(err.errors);
      }
    }
  };

  return (
    <div className="bg-white rounded-lg shadow-lg border border-gray-100">
      <div className="px-6 py-4 border-b border-gray-100 flex justify-between items-center bg-gray-50/50">
        <h3 className="text-lg font-semibold text-gray-800">Carga Masiva de Unidades</h3>
      </div>
      
      <div className="p-6">
        <div className="mb-6">
          <p className="text-sm text-gray-600 mb-4">
            Sube un archivo CSV con los datos de las unidades. El archivo debe contener una fila de encabezado.
            Las columnas deben estar en este orden exacto:
          </p>
          <ul className="list-disc pl-5 text-sm text-gray-600 mb-4 space-y-1">
            <li>Identifier (texto)</li>
            <li>UnitTypeName (texto, debe existir)</li>
            <li>TowerOrBlock (texto)</li>
            <li>FloorLevel (entero)</li>
            <li>PrivateArea (decimal)</li>
            <li>BalconyArea (decimal)</li>
            <li>CoproprietyCoefficient (decimal, la suma debe ser exactamente 100)</li>
            <li>Status (ActiveOccupied, ActiveUnoccupied, DeliveryProcess, Litigation, Inactive)</li>
            <li>HasPrivateParking (true/false)</li>
            <li>ParkingIdentifier (texto)</li>
            <li>HasAssignedStorage (true/false)</li>
            <li>StorageIdentifier (texto)</li>
          </ul>
        </div>

        <div className="mb-6">
          <label className="block text-sm font-semibold text-gray-700 mb-2">Seleccionar Archivo CSV</label>
          <input
            type="file"
            accept=".csv"
            onChange={handleFileChange}
            disabled={isSubmitting}
            className="w-full px-4 py-2.5 bg-gray-50 border border-gray-200 rounded-xl focus:bg-white focus:ring-2 focus:ring-blue-500/20 focus:border-blue-500 transition-all text-gray-900"
          />
        </div>

        {globalMessage && (
          <div className={`mb-6 p-4 rounded-xl text-sm font-medium border shadow-sm ${errors.length > 0 ? 'bg-red-50 text-red-700 border-red-100' : 'bg-blue-50 text-blue-700 border-blue-100'}`}>
            {globalMessage}
          </div>
        )}

        {errors.length > 0 && (
          <div className="mb-6 bg-red-50 rounded-xl border border-red-100 overflow-hidden">
            <div className="px-4 py-2 bg-red-100 border-b border-red-200">
              <h4 className="text-sm font-bold text-red-800">Errores de Validación</h4>
            </div>
            <div className="p-4 max-h-64 overflow-y-auto">
              <ul className="list-disc pl-5 text-sm text-red-700 space-y-1">
                {errors.map((errorMsg, idx) => (
                  <li key={idx}>{errorMsg}</li>
                ))}
              </ul>
            </div>
          </div>
        )}

        <div className="flex justify-end gap-3 pt-6 border-t border-gray-100">
          <button
            type="button"
            onClick={onCancel}
            disabled={isSubmitting}
            className="px-6 py-2.5 text-sm font-semibold text-gray-600 bg-white border border-gray-200 rounded-xl hover:bg-gray-50 transition-colors"
          >
            Cancelar
          </button>
          
          <button
            type="button"
            onClick={handleUpload}
            disabled={!file || isSubmitting}
            className="px-6 py-2.5 text-sm font-semibold text-white bg-blue-600 rounded-xl hover:bg-blue-700 transition-colors disabled:opacity-50 disabled:cursor-not-allowed shadow-sm shadow-blue-200"
          >
            {(() => {
              if (isSubmitting) {
                return "Procesando...";
              }
              return "Subir y Validar";
            })()}
          </button>
        </div>
      </div>
    </div>
  );
}
