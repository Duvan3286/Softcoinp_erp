'use client';

import React, { useState, useEffect } from 'react';
import { useRouter } from 'next/navigation';
import { Loader2, AlertTriangle, Plus } from 'lucide-react';
import { Button } from '@/components/ui/Button';
import { Card, CardContent } from '@/components/ui/Card';
import maintenanceService, { IncidentListItem } from '@/lib/maintenance-service';

const incidentTypeLabels: Record<string, string> = {
  Flood: 'Inundación', Fire: 'Incendio', StructuralDamage: 'Daño Estructural',
  ElectricalFailure: 'Falla Eléctrica', Other: 'Otro',
};

export default function IncidentsPage() {
  const router = useRouter();
  const [incidents, setIncidents] = useState<IncidentListItem[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');

  const fetchIncidents = async () => {
    setLoading(true);
    setError('');
    try {
      const data = await maintenanceService.getIncidents();
      setIncidents(data);
    } catch {
      setError('Error al cargar los siniestros.');
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => { fetchIncidents(); }, []);

  const formatCurrency = (val: number) => new Intl.NumberFormat('es-CO', { style: 'currency', currency: 'COP', maximumFractionDigits: 0 }).format(val);
  const formatDate = (d: string) => new Date(d).toLocaleDateString('es-CO', { day: '2-digit', month: '2-digit', year: 'numeric' });

  return (
    <div className="space-y-6">
      <div className="flex flex-col sm:flex-row justify-between items-start sm:items-center gap-4">
        <div>
          <h1 className="text-2xl font-bold text-foreground tracking-tight">Siniestros</h1>
          <p className="text-sm text-muted-foreground mt-1">Registro de siniestros con agrupación de órdenes de trabajo.</p>
        </div>
        <Button onClick={() => router.push('/maintenance/incidents/new')}>
          <Plus className="w-4 h-4 mr-2" /> Nuevo Siniestro
        </Button>
      </div>

      {loading ? (
        <div className="flex justify-center py-20"><Loader2 className="w-8 h-8 animate-spin text-emerald-600" /></div>
      ) : error ? (
        <div className="flex flex-col items-center gap-3 text-rose-600 py-12">
          <AlertTriangle className="w-10 h-10" />
          <p className="font-semibold">{error}</p>
        </div>
      ) : incidents.length === 0 ? (
        <div className="flex flex-col items-center gap-3 text-muted-foreground py-12">
          <AlertTriangle className="w-10 h-10" />
          <p className="font-semibold">No hay siniestros registrados</p>
        </div>
      ) : (
        <Card>
          <CardContent className="p-0">
            <div className="overflow-x-auto">
              <table className="min-w-full divide-y divide-border">
                <thead className="bg-muted/50">
                  <tr>
                    <th className="px-5 py-3 text-left text-xs font-bold text-muted-foreground uppercase">Nombre</th>
                    <th className="px-5 py-3 text-left text-xs font-bold text-muted-foreground uppercase">Tipo</th>
                    <th className="px-5 py-3 text-left text-xs font-bold text-muted-foreground uppercase">Fecha</th>
                    <th className="px-5 py-3 text-left text-xs font-bold text-muted-foreground uppercase">Daño Total</th>
                    <th className="px-5 py-3 text-left text-xs font-bold text-muted-foreground uppercase">Póliza</th>
                    <th className="px-5 py-3 text-center text-xs font-bold text-muted-foreground uppercase">Órdenes</th>
                    <th className="px-5 py-3 text-left text-xs font-bold text-muted-foreground uppercase">Estado</th>
                    <th className="px-5 py-3 text-right text-xs font-bold text-muted-foreground uppercase">Acciones</th>
                  </tr>
                </thead>
                <tbody className="divide-y divide-border">
                  {incidents.map((i) => (
                    <tr key={i.id} className="hover:bg-muted/30 transition-colors">
                      <td className="px-5 py-3 font-medium text-sm">{i.name}</td>
                      <td className="px-5 py-3 text-sm text-muted-foreground">{incidentTypeLabels[i.incidentType] || i.incidentType}</td>
                      <td className="px-5 py-3 text-sm text-muted-foreground">{formatDate(i.occurredAt)}</td>
                      <td className="px-5 py-3 text-sm font-medium">{formatCurrency(i.totalDamageValue)}</td>
                      <td className="px-5 py-3 text-sm text-muted-foreground">{i.insurancePolicyNumber || '—'}</td>
                      <td className="px-5 py-3 text-center"><span className="badge-info">{i.relatedWorkOrders}</span></td>
                      <td className="px-5 py-3">
                        <span className={i.status === 'Open' ? 'badge-warning' : 'badge-success'}>{i.status === 'Open' ? 'Abierto' : 'Cerrado'}</span>
                      </td>
                      <td className="px-5 py-3 text-right">
                        <button onClick={() => router.push(`/maintenance/incidents/${i.id}`)}
                          className="text-emerald-600 hover:text-emerald-800 text-sm font-semibold">Ver</button>
                      </td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>
          </CardContent>
        </Card>
      )}
    </div>
  );
}
