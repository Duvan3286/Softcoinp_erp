'use client';

import React, { useState, useEffect } from 'react';
import { Loader2, AlertTriangle, Calendar, DollarSign } from 'lucide-react';
import { Button } from '@/components/ui/Button';
import { Card, CardContent } from '@/components/ui/Card';
import maintenanceService, { MaintenanceReport } from '@/lib/maintenance-service';

export default function MaintenanceReportsPage() {
  const [report, setReport] = useState<MaintenanceReport | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');
  const [daysAhead, setDaysAhead] = useState(30);

  const fetchReport = async () => {
    setLoading(true);
    setError('');
    try {
      const data = await maintenanceService.getScheduledReport(daysAhead);
      setReport(data);
    } catch {
      setError('Error al cargar el reporte.');
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => { fetchReport(); }, [daysAhead]);

  const formatCurrency = (val: number) =>
    new Intl.NumberFormat('es-CO', { style: 'currency', currency: 'COP', maximumFractionDigits: 0 }).format(val);

  const formatDate = (d: string) => new Date(d).toLocaleDateString('es-CO', { day: '2-digit', month: '2-digit', year: 'numeric' });

  return (
    <div className="space-y-6">
      <div>
        <h1 className="text-2xl font-bold text-foreground tracking-tight">Reporte de Mantenimientos Programados</h1>
        <p className="text-sm text-muted-foreground mt-1">Proyección de costos para los próximos días.</p>
      </div>

      <Card>
        <CardContent className="p-4">
          <div className="flex items-center gap-4">
            <label className="text-sm font-medium text-foreground">Horizonte:</label>
            <select value={daysAhead} onChange={(e) => setDaysAhead(parseInt(e.target.value))}
              className="bg-transparent border-b border-emerald-600/30 focus:border-emerald-600 text-sm font-medium py-2 outline-none">
              <option value={30}>30 días</option>
              <option value={60}>60 días</option>
              <option value={90}>90 días</option>
            </select>
            <Button variant="secondary" onClick={fetchReport} disabled={loading}>Actualizar</Button>
          </div>
        </CardContent>
      </Card>

      {loading ? (
        <div className="flex justify-center py-20"><Loader2 className="w-8 h-8 animate-spin text-emerald-600" /></div>
      ) : error ? (
        <div className="flex flex-col items-center gap-3 text-rose-600 py-12">
          <AlertTriangle className="w-10 h-10" />
          <p className="font-semibold">{error}</p>
        </div>
      ) : report && (
        <>
          <div className="grid grid-cols-1 md:grid-cols-3 gap-4">
            <Card><CardContent className="p-4 text-center">
              <Calendar className="w-6 h-6 text-emerald-600 mx-auto mb-2" />
              <p className="text-2xl font-bold text-foreground">{report.scheduledItems.length}</p>
              <p className="text-xs text-muted-foreground">Mantenimientos Programados</p>
            </CardContent></Card>
            <Card><CardContent className="p-4 text-center">
              <DollarSign className="w-6 h-6 text-emerald-600 mx-auto mb-2" />
              <p className="text-2xl font-bold text-foreground">{formatCurrency(report.totalEstimatedCost)}</p>
              <p className="text-xs text-muted-foreground">Costo Estimado Total</p>
            </CardContent></Card>
            <Card><CardContent className="p-4 text-center">
              <DollarSign className="w-6 h-6 text-blue-600 mx-auto mb-2" />
              <p className="text-2xl font-bold text-foreground">{formatCurrency(report.budgetAvailable)}</p>
              <p className="text-xs text-muted-foreground">Saldo Disponible</p>
            </CardContent></Card>
          </div>

          <Card>
            <CardContent className="p-0">
              {report.scheduledItems.length === 0 ? (
                <p className="px-6 py-8 text-center text-sm text-muted-foreground">No hay mantenimientos programados en este período.</p>
              ) : (
                <div className="overflow-x-auto">
                  <table className="min-w-full divide-y divide-border">
                    <thead className="bg-muted/50">
                      <tr>
                        <th className="px-5 py-3 text-left text-xs font-bold text-muted-foreground uppercase">Bien</th>
                        <th className="px-5 py-3 text-left text-xs font-bold text-muted-foreground uppercase">Ubicación</th>
                        <th className="px-5 py-3 text-left text-xs font-bold text-muted-foreground uppercase">Actividad</th>
                        <th className="px-5 py-3 text-left text-xs font-bold text-muted-foreground uppercase">Fecha</th>
                        <th className="px-5 py-3 text-left text-xs font-bold text-muted-foreground uppercase">Proveedor</th>
                        <th className="px-5 py-3 text-right text-xs font-bold text-muted-foreground uppercase">Costo Est.</th>
                      </tr>
                    </thead>
                    <tbody className="divide-y divide-border">
                      {report.scheduledItems.map((item, idx) => (
                        <tr key={idx} className="hover:bg-muted/30 transition-colors">
                          <td className="px-5 py-3 text-sm font-medium">{item.assetName}</td>
                          <td className="px-5 py-3 text-sm text-muted-foreground">{item.assetLocation}</td>
                          <td className="px-5 py-3 text-sm text-muted-foreground">{item.activityType}</td>
                          <td className="px-5 py-3 text-sm text-muted-foreground">{formatDate(item.scheduledDate)}</td>
                          <td className="px-5 py-3 text-sm text-muted-foreground">{item.preferredProviderName || '—'}</td>
                          <td className="px-5 py-3 text-sm text-right font-medium">{formatCurrency(item.estimatedCost)}</td>
                        </tr>
                      ))}
                    </tbody>
                  </table>
                </div>
              )}
            </CardContent>
          </Card>
        </>
      )}
    </div>
  );
}
