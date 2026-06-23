'use client';

import React, { useState, useEffect } from 'react';
import { useRouter, useParams } from 'next/navigation';
import { Loader2, ArrowLeft, AlertTriangle, FileText } from 'lucide-react';
import { Button } from '@/components/ui/Button';
import { Card, CardContent, CardHeader } from '@/components/ui/Card';
import maintenanceService, { IncidentDetail } from '@/lib/maintenance-service';

const incidentTypeLabels: Record<string, string> = {
  Flood: 'Inundación', Fire: 'Incendio', StructuralDamage: 'Daño Estructural',
  ElectricalFailure: 'Falla Eléctrica', Other: 'Otro',
};

const statusBadge = (status: string) => {
  if (status === 'Open') return <span className="badge-warning">Abierto</span>;
  return <span className="badge-success">Cerrado</span>;
};

export default function IncidentDetailPage() {
  const router = useRouter();
  const params = useParams();
  const id = params.id as string;
  const [incident, setIncident] = useState<IncidentDetail | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');

  const fetchIncident = async () => {
    setLoading(true);
    setError('');
    try {
      const data = await maintenanceService.getIncidentById(id);
      setIncident(data);
    } catch {
      setError('Error al cargar el siniestro.');
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => { fetchIncident(); }, [id]);

  const formatDate = (d: string) => new Date(d).toLocaleDateString('es-CO', { day: '2-digit', month: '2-digit', year: 'numeric' });
  const formatDateTime = (d: string | null) => d ? new Date(d).toLocaleString('es-CO', { day: '2-digit', month: '2-digit', year: 'numeric', hour: '2-digit', minute: '2-digit' }) : '—';
  const formatCurrency = (val: number) => new Intl.NumberFormat('es-CO', { style: 'currency', currency: 'COP', maximumFractionDigits: 0 }).format(val);

  if (loading) return <div className="flex justify-center py-20"><Loader2 className="w-8 h-8 animate-spin text-emerald-600" /></div>;
  if (error || !incident) return (
    <div className="space-y-6 max-w-2xl mx-auto">
      <button onClick={() => router.push('/maintenance/incidents')} className="inline-flex items-center gap-2 text-sm text-muted-foreground hover:text-foreground">
        <ArrowLeft className="w-4 h-4" /> Volver
      </button>
      <div className="flex flex-col items-center gap-3 text-rose-600 py-12">
        <AlertTriangle className="w-10 h-10" />
        <p className="font-semibold">{error || 'Siniestro no encontrado.'}</p>
      </div>
    </div>
  );

  return (
    <div className="space-y-6">
      <button onClick={() => router.push('/maintenance/incidents')} className="inline-flex items-center gap-2 text-sm text-muted-foreground hover:text-foreground transition-colors">
        <ArrowLeft className="w-4 h-4" /> Volver a Siniestros
      </button>

      <div className="flex flex-col sm:flex-row justify-between items-start sm:items-center gap-4">
        <div>
          <div className="flex items-center gap-3">
            <h1 className="text-2xl font-bold text-foreground tracking-tight">{incident.name}</h1>
            {statusBadge(incident.status)}
          </div>
          <p className="text-sm text-muted-foreground mt-1">
            {incidentTypeLabels[incident.incidentType] || incident.incidentType} — {formatDate(incident.occurredAt)}
          </p>
        </div>
      </div>

      <div className="grid grid-cols-1 lg:grid-cols-3 gap-6">
        <div className="lg:col-span-2 space-y-6">
          <Card>
            <CardHeader className="py-3 px-6"><h3 className="text-sm font-bold text-foreground">Información del Siniestro</h3></CardHeader>
            <CardContent className="p-6">
              <div className="grid grid-cols-2 gap-4 text-sm">
                <div><span className="text-muted-foreground">Tipo:</span><p className="font-medium">{incidentTypeLabels[incident.incidentType]}</p></div>
                <div><span className="text-muted-foreground">Fecha:</span><p className="font-medium">{formatDateTime(incident.occurredAt)}</p></div>
                <div><span className="text-muted-foreground">Valor Total Daño:</span><p className="font-medium">{formatCurrency(incident.totalDamageValue)}</p></div>
                <div><span className="text-muted-foreground">Estado:</span><p className="font-medium">{incident.status === 'Open' ? 'Abierto' : 'Cerrado'}</p></div>
                {incident.description && (
                  <div className="col-span-2"><span className="text-muted-foreground">Descripción:</span><p className="font-medium mt-1">{incident.description}</p></div>
                )}
              </div>
            </CardContent>
          </Card>

          <Card>
            <CardHeader className="py-3 px-6"><h3 className="text-sm font-bold text-foreground">Órdenes de Trabajo Relacionadas</h3></CardHeader>
            <CardContent className="p-0">
              {incident.relatedWorkOrders.length === 0 ? (
                <p className="px-6 py-8 text-center text-sm text-muted-foreground">No hay órdenes vinculadas.</p>
              ) : (
                <div className="overflow-x-auto">
                  <table className="min-w-full divide-y divide-border">
                    <thead className="bg-muted/50">
                      <tr>
                        <th className="px-5 py-3 text-left text-xs font-bold text-muted-foreground uppercase">Tipo</th>
                        <th className="px-5 py-3 text-left text-xs font-bold text-muted-foreground uppercase">Descripción</th>
                        <th className="px-5 py-3 text-left text-xs font-bold text-muted-foreground uppercase">Estado</th>
                        <th className="px-5 py-3 text-right text-xs font-bold text-muted-foreground uppercase">Costo</th>
                        <th className="px-5 py-3 text-right text-xs font-bold text-muted-foreground uppercase">Acciones</th>
                      </tr>
                    </thead>
                    <tbody className="divide-y divide-border">
                      {incident.relatedWorkOrders.map((w) => (
                        <tr key={w.id} className="hover:bg-muted/30 transition-colors">
                          <td className="px-5 py-3 text-sm">{w.orderType === 'Preventive' ? 'Preventivo' : 'Correctivo'}</td>
                          <td className="px-5 py-3 text-sm text-muted-foreground max-w-[200px] truncate">{w.description}</td>
                          <td className="px-5 py-3 text-sm">{w.status}</td>
                          <td className="px-5 py-3 text-sm text-right font-medium">{formatCurrency(w.actualCost)}</td>
                          <td className="px-5 py-3 text-right">
                            <button onClick={() => router.push(`/maintenance/work-orders/${w.id}`)}
                              className="text-emerald-600 hover:text-emerald-800 text-sm font-semibold">Ver</button>
                          </td>
                        </tr>
                      ))}
                    </tbody>
                  </table>
                </div>
              )}
            </CardContent>
          </Card>
        </div>

        <div className="space-y-6">
          <Card>
            <CardHeader className="py-3 px-6"><h3 className="text-sm font-bold text-foreground">Seguro</h3></CardHeader>
            <CardContent className="p-4 space-y-3 text-sm">
              <div><span className="text-muted-foreground">Póliza:</span><p className="font-medium">{incident.insurancePolicyNumber || '—'}</p></div>
              <div><span className="text-muted-foreground">Aseguradora:</span><p className="font-medium">{incident.insuranceCompany || '—'}</p></div>
            </CardContent>
          </Card>

          <Card>
            <CardHeader className="py-3 px-6"><h3 className="text-sm font-bold text-foreground">Historial</h3></CardHeader>
            <CardContent className="p-4 space-y-3 text-sm">
              <div><span className="text-muted-foreground">Creado:</span><p className="font-medium">{formatDateTime(incident.createdAt)}</p></div>
              {incident.updatedAt && (
                <div><span className="text-muted-foreground">Actualizado:</span><p className="font-medium">{formatDateTime(incident.updatedAt)}</p></div>
              )}
            </CardContent>
          </Card>
        </div>
      </div>
    </div>
  );
}
