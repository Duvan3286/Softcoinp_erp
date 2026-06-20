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
      console.error("Error capturado en BulkImport:", err);
      
      if (typeof err === "string") {
        setGlobalMessage(err);
      } else if (err.message) {
        setGlobalMessage(err.message);
      } else if (err.Message) { // ASP.NET Core capitalized property
        setGlobalMessage(err.Message);
      } else {
        setGlobalMessage("Ocurrió un error inesperado.");
      }

      if (err.errors && Array.isArray(err.errors)) {
        setErrors(err.errors);
      } else if (err.Errors && Array.isArray(err.Errors)) { // ASP.NET Core capitalized property
        setErrors(err.Errors);
      }
    }
  };

  const downloadTemplate = () => {
    const headers = [
      "Identificador",
      "Tipo de Unidad",
      "Torre o Bloque",
      "Piso",
      "Área Privada",
      "Área Balcón",
      "Coeficiente",
      "Estado",
      "Tiene Parqueadero",
      "Identificador Parqueadero",
      "Tiene Depósito",
      "Identificador Depósito"
    ];
    const sampleRow = [
      "101",
      "Apartamento",
      "Torre A",
      "1",
      "75.5",
      "5.2",
      "1.25",
      "Activa y Ocupada",
      "sí",
      "P-101",
      "sí",
      "D-101"
    ];
    const csvContent = "\uFEFF" + [headers.join(";"), sampleRow.join(";")].join("\r\n");
    const blob = new Blob([csvContent], { type: "text/csv;charset=utf-8;" });
    const url = URL.createObjectURL(blob);
    const link = document.createElement("a");
    link.setAttribute("href", url);
    link.setAttribute("download", "plantilla_carga_masiva_unidades.csv");
    link.style.visibility = "hidden";
    document.body.appendChild(link);
    link.click();
    document.body.removeChild(link);
  };

  return (
    <div className="bg-white rounded-lg shadow-lg border border-gray-100">
      <div className="px-6 py-4 border-b border-gray-100 flex justify-between items-center bg-gray-50/50">
        <h3 className="text-lg font-semibold text-gray-800">Carga Masiva de Unidades</h3>
      </div>
      
      <div className="p-6">
        <div className="mb-6">
          <p className="text-sm text-gray-600 mb-4">
            Para facilitar la carga masiva, descarga nuestra plantilla prediseñada en español, diligénciala y súbela a continuación.
          </p>
          <div className="mb-6 p-4 bg-blue-50 border border-blue-100 rounded-xl flex items-center justify-between">
            <div>
              <h4 className="text-sm font-semibold text-blue-900">Plantilla Oficial de Carga</h4>
              <p className="text-xs text-blue-700 mt-0.5">El archivo incluye los encabezados requeridos y un ejemplo de uso.</p>
            </div>
            <button
              type="button"
              onClick={downloadTemplate}
              className="px-4 py-2 text-xs font-bold text-white bg-blue-600 hover:bg-blue-700 rounded-lg transition-colors flex items-center gap-1.5 shadow-sm"
            >
              Descargar Plantilla
            </button>
          </div>
          
          <h4 className="text-xs font-semibold text-gray-500 uppercase tracking-wider mb-2">Columnas Requeridas:</h4>
          <ul className="list-disc pl-5 text-sm text-gray-600 mb-4 space-y-1">
            <li><strong>Identificador:</strong> Texto (Ej: 101, Apartamento 101).</li>
            <li><strong>Tipo de Unidad:</strong> Debe coincidir con un tipo creado (Ej: Apartamento, Parqueadero, Depósito).</li>
            <li><strong>Torre o Bloque:</strong> Nombre o número de la torre (Ej: Torre A, Bloque 2).</li>
            <li><strong>Piso:</strong> Número entero.</li>
            <li><strong>Área Privada:</strong> Número decimal (Ej: 75.5, acepta coma o punto decimal).</li>
            <li><strong>Área Balcón:</strong> Número decimal (Ej: 5.2, acepta coma o punto decimal).</li>
            <li><strong>Coeficiente:</strong> Coeficiente de copropiedad (Ej: 1.25). <em>¡La suma total del conjunto debe ser exactamente 100%!</em></li>
            <li><strong>Estado:</strong> Opciones: <code>Activa y Ocupada</code>, <code>Activa y Desocupada</code>, <code>En Proceso de Entrega</code>, <code>En Litigio</code>, <code>Inactiva</code>.</li>
            <li><strong>Tiene Parqueadero:</strong> <code>sí</code> o <code>no</code>.</li>
            <li><strong>Identificador Parqueadero:</strong> Texto (si aplica).</li>
            <li><strong>Tiene Depósito:</strong> <code>sí</code> o <code>no</code>.</li>
            <li><strong>Identificador Depósito:</strong> Texto (si aplica).</li>
          </ul>
        </div>

        <div className="mb-6">
          <label className="block text-sm font-semibold text-gray-700 mb-2">Seleccionar Archivo CSV Diligenciado</label>
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
