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
    <div className="bg-card rounded-lg shadow-lg border border-border">
      <div className="px-6 py-4 border-b border-border flex justify-between items-center bg-muted/50">
        <h3 className="text-lg font-semibold text-foreground">Carga Masiva de Unidades</h3>
      </div>
      
      <div className="p-6">
        <div className="mb-6">
          <p className="text-sm text-muted-foreground mb-4">
            Para facilitar la carga masiva, descarga nuestra plantilla prediseñada en español, diligénciala y súbela a continuación.
          </p>
          <div className="mb-6 p-4 bg-blue-50 dark:bg-blue-950/20 border border-blue-100 dark:border-blue-900 rounded-xl flex items-center justify-between">
            <div>
              <h4 className="text-sm font-semibold text-blue-900 dark:text-blue-300">Plantilla Oficial de Carga</h4>
              <p className="text-xs text-blue-700 dark:text-blue-400 mt-0.5">El archivo incluye los encabezados requeridos y un ejemplo de uso.</p>
            </div>
            <button
              type="button"
              onClick={downloadTemplate}
              className="px-4 py-2 text-xs font-bold text-white bg-emerald-600 hover:bg-emerald-700 rounded-lg transition-colors flex items-center gap-1.5 shadow-sm"
            >
              Descargar Plantilla
            </button>
          </div>
          
          <h4 className="text-xs font-semibold text-muted-foreground uppercase tracking-wider mb-2">Columnas Requeridas:</h4>
          <ul className="list-disc pl-5 text-sm text-muted-foreground mb-4 space-y-1">
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
          <label className="block text-sm font-semibold text-muted-foreground mb-2">Seleccionar Archivo CSV Diligenciado</label>
          <input
            type="file"
            accept=".csv"
            onChange={handleFileChange}
            disabled={isSubmitting}
            className="w-full px-4 py-2.5 bg-muted/50 border border-border rounded-xl focus:bg-card focus:ring-2 focus:ring-emerald-500/20 focus:border-emerald-500 transition-all text-foreground"
          />
        </div>

        {globalMessage && (
          <div className={`mb-6 p-4 rounded-xl text-sm font-medium border shadow-sm ${errors.length > 0 ? 'bg-rose-50 dark:bg-rose-950/20 text-rose-700 dark:text-rose-400 border-rose-100 dark:border-rose-900' : 'bg-emerald-50 dark:bg-emerald-950/20 text-emerald-700 dark:text-emerald-400 border-emerald-100 dark:border-emerald-900'}`}>
            {globalMessage}
          </div>
        )}

        {errors.length > 0 && (
          <div className="mb-6 bg-rose-50 dark:bg-rose-950/20 rounded-xl border border-rose-100 dark:border-rose-900 overflow-hidden">
            <div className="px-4 py-2 bg-rose-100 dark:bg-rose-950/30 border-b border-rose-200 dark:border-rose-900">
              <h4 className="text-sm font-bold text-rose-700 dark:text-rose-400">Errores de Validación</h4>
            </div>
            <div className="p-4 max-h-64 overflow-y-auto">
              <ul className="list-disc pl-5 text-sm text-rose-700 dark:text-rose-400 space-y-1">
                {errors.map((errorMsg, idx) => (
                  <li key={idx}>{errorMsg}</li>
                ))}
              </ul>
            </div>
          </div>
        )}

        <div className="flex justify-end gap-3 pt-6 border-t border-border">
          <button
            type="button"
            onClick={onCancel}
            disabled={isSubmitting}
            className="px-6 py-2.5 text-sm font-semibold text-muted-foreground bg-card border border-border rounded-xl hover:bg-muted/30 transition-colors"
          >
            Cancelar
          </button>
          
          <button
            type="button"
            onClick={handleUpload}
            disabled={!file || isSubmitting}
            className="px-6 py-2.5 text-sm font-semibold text-white bg-emerald-600 rounded-xl hover:bg-emerald-700 transition-colors disabled:opacity-50 disabled:cursor-not-allowed shadow-sm shadow-emerald-200"
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
