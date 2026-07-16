'use client';

import React, { useState } from 'react';
import { Loader2, Download, FileSpreadsheet } from 'lucide-react';
import { Button } from '@/components/ui/Button';
import { Card, CardContent } from '@/components/ui/Card';
import reportService from '@/lib/report-service';

export default function AccountantExportPage() {
  const [periodFrom, setPeriodFrom] = useState('');
  const [periodTo, setPeriodTo] = useState('');
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState('');
  const [success, setSuccess] = useState('');

  const handleExport = async () => {
    if (!periodFrom || !periodTo) {
      setError('Debe seleccionar un rango de fechas.');
      return;
    }
    setLoading(true);
    setError('');
    setSuccess('');
    try {
      const blob = await reportService.generateAccountantExport(
        new Date(periodFrom).toISOString(),
        new Date(periodTo).toISOString()
      );
      const url = window.URL.createObjectURL(blob);
      const a = document.createElement('a');
      a.href = url;
      a.download = `Exportacion_Contador_${periodFrom.replace(/-/g, '')}_${periodTo.replace(/-/g, '')}.xlsx`;
      a.click();
      window.URL.revokeObjectURL(url);
      setSuccess('Exportación generada exitosamente.');
    } catch {
      setError('Error al generar la exportación.');
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="p-6 space-y-6">
      <div className="flex items-center justify-between">
        <h1 className="text-2xl font-bold text-foreground">Exportación para el Contador</h1>
      </div>

      <p className="text-sm text-muted-foreground">
        Genere un archivo Excel estructurado con dos hojas separadas: una de ingresos (recaudo) y una de egresos (gastos del período).
        Este archivo está diseñado para ser procesado directamente en el software contable externo.
      </p>

      {error && (
        <div className="p-4 bg-red-50 dark:bg-red-950/30 border border-red-200 dark:border-red-900 rounded-xl text-red-700 dark:text-red-400 text-sm">{error}</div>
      )}

      {success && (
        <div className="p-4 bg-emerald-50 dark:bg-emerald-950/30 border border-emerald-200 dark:border-emerald-900 rounded-xl text-emerald-700 dark:text-emerald-400 text-sm">{success}</div>
      )}

      <Card>
        <CardContent className="p-6 space-y-4">
          <h2 className="text-lg font-semibold text-foreground">Seleccione el Período</h2>

          <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
            <div>
              <label className="block text-sm font-medium text-foreground mb-1">Desde</label>
              <input
                type="date"
                value={periodFrom}
                onChange={(e) => setPeriodFrom(e.target.value)}
                className="w-full border-b border-emerald-600/30 focus:border-emerald-600 text-sm font-medium py-2 outline-none bg-transparent"
              />
            </div>
            <div>
              <label className="block text-sm font-medium text-foreground mb-1">Hasta</label>
              <input
                type="date"
                value={periodTo}
                onChange={(e) => setPeriodTo(e.target.value)}
                className="w-full border-b border-emerald-600/30 focus:border-emerald-600 text-sm font-medium py-2 outline-none bg-transparent"
              />
            </div>
          </div>

          <Button onClick={handleExport} disabled={loading || !periodFrom || !periodTo}>
            {loading ? (
              <Loader2 className="w-4 h-4 mr-2 animate-spin" />
            ) : (
              <FileSpreadsheet className="w-4 h-4 mr-2" />
            )}
            {loading ? 'Generando...' : 'Generar Exportación'}
          </Button>

          <div className="mt-4 p-3 bg-blue-50 dark:bg-blue-950/20 border border-blue-200 dark:border-blue-900 rounded-lg text-sm">
            <p className="font-medium text-blue-700 dark:text-blue-400 mb-1">Estructura del archivo:</p>
            <ul className="text-blue-600 dark:text-blue-500 text-xs space-y-1 list-disc list-inside">
              <li>Hoja &quot;Ingresos&quot;: fecha, unidad, propietario, identificación, valor, medio de pago, comprobante, concepto, período</li>
              <li>Hoja &quot;Egresos&quot;: fecha, proveedor, identificación, descripción, rubro, valor, factura, medio de pago, período</li>
              <li>Subtotales por mes en cada hoja</li>
              <li>Total general del período</li>
              <li>Filtros habilitados en todas las columnas</li>
            </ul>
          </div>
        </CardContent>
      </Card>
    </div>
  );
}
