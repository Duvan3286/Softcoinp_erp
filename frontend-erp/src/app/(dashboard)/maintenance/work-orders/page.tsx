'use client';

import React, { useState, useEffect, useCallback } from 'react';
import { useRouter } from 'next/navigation';
import { Loader2, Plus, Search, AlertTriangle, Eye, AlertCircle } from 'lucide-react';
import { Button } from '@/components/ui/Button';
import { Card, CardContent } from '@/components/ui/Card';
import maintenanceService, { WorkOrderListItem } from '@/lib/maintenance-service';

const typeLabels: Record<string, string> = { Preventive: 'Preventivo', Corrective: 'Correctivo' };
const priorityLabels: Record<string, string> = { Emergency: 'Emergencia', High: 'Alta', Medium: 'Media', Low: 'Baja' };
const originLabels: Record<string, string> = { AutomaticScheduling: 'Automática', AdminReport: 'Admin', ResidentPqr: 'PQR Residente' };

const statusBadge = (status: string) => {
  if (status === 'PendingAssignment') return <span className="badge-warning">Pendiente</span>;
  if (status === 'Assigned') return <span className="badge-info">Asignada</span>;
  if (status === 'InProgress') return <span className="badge-info">En Progreso</span>;
  if (status === 'Completed') return <span className="badge-success">Completada</span>;
  if (status === 'Cancelled') return <span className="badge-neutral">Cancelada</span>;
  return <span className="badge-neutral">{status}</span>;
};

const priorityBadge = (priority: string) => {
  if (priority === 'Emergency') return <span className="badge-danger">Emergencia</span>;
  if (priority === 'High') return <span className="badge-warning">Alta</span>;
  if (priority === 'Medium') return <span className="badge-info">Media</span>;
  if (priority === 'Low') return <span className="badge-neutral">Baja</span>;
  return <span className="badge-neutral">{priority}</span>;
};

export default function WorkOrdersPage() {
  const router = useRouter();
  const [orders, setOrders] = useState<WorkOrderListItem[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');
  const [typeFilter, setTypeFilter] = useState('');
  const [statusFilter, setStatusFilter] = useState('');
  const [priorityFilter, setPriorityFilter] = useState('');
  const [searchTerm, setSearchTerm] = useState('');

  const fetchOrders = useCallback(async () => {
    setLoading(true);
    setError('');
    try {
      const data = await maintenanceService.getWorkOrders(
        typeFilter || undefined, statusFilter || undefined,
        priorityFilter || undefined, undefined, searchTerm || undefined
      );
      setOrders(data);
    } catch {
      setError('Error al cargar las órdenes de trabajo.');
    } finally {
      setLoading(false);
    }
  }, [typeFilter, statusFilter, priorityFilter, searchTerm]);

  useEffect(() => { fetchOrders(); }, [fetchOrders]);
  useEffect(() => {
    const timer = setTimeout(() => { fetchOrders(); }, 400);
    return () => clearTimeout(timer);
  }, [searchTerm]);

  const formatCurrency = (val: number) =>
    new Intl.NumberFormat('es-CO', { style: 'currency', currency: 'COP', maximumFractionDigits: 0 }).format(val);

  const formatDate = (d: string | null) => d ? new Date(d).toLocaleDateString('es-CO', { day: '2-digit', month: '2-digit', year: 'numeric' }) : '—';

  return (
    <div className="space-y-6">
      <div className="flex flex-col sm:flex-row justify-between items-start sm:items-center gap-4">
        <div>
          <h1 className="text-2xl font-bold text-foreground tracking-tight">Órdenes de Trabajo</h1>
          <p className="text-sm text-muted-foreground mt-1">Gestiona las órdenes de mantenimiento preventivo y correctivo.</p>
        </div>
        <Button onClick={() => router.push('/maintenance/work-orders/new')}>
          <Plus className="w-4 h-4 mr-2" /> Nueva Orden
        </Button>
      </div>

      <Card>
        <CardContent className="p-4">
          <div className="flex flex-col md:flex-row gap-3">
            <select value={typeFilter} onChange={(e) => setTypeFilter(e.target.value)}
              className="bg-transparent border-b border-emerald-600/30 focus:border-emerald-600 text-sm font-medium py-2 outline-none">
              <option value="">Todos los tipos</option>
              <option value="Preventive">Preventivo</option>
              <option value="Corrective">Correctivo</option>
            </select>
            <select value={statusFilter} onChange={(e) => setStatusFilter(e.target.value)}
              className="bg-transparent border-b border-emerald-600/30 focus:border-emerald-600 text-sm font-medium py-2 outline-none">
              <option value="">Todos los estados</option>
              <option value="PendingAssignment">Pendiente</option>
              <option value="Assigned">Asignada</option>
              <option value="InProgress">En Progreso</option>
              <option value="Completed">Completada</option>
              <option value="Cancelled">Cancelada</option>
            </select>
            <select value={priorityFilter} onChange={(e) => setPriorityFilter(e.target.value)}
              className="bg-transparent border-b border-emerald-600/30 focus:border-emerald-600 text-sm font-medium py-2 outline-none">
              <option value="">Todas las prioridades</option>
              <option value="Emergency">Emergencia</option>
              <option value="High">Alta</option>
              <option value="Medium">Media</option>
              <option value="Low">Baja</option>
            </select>
            <div className="flex-1 relative">
              <Search className="absolute left-0 top-1/2 -translate-y-1/2 w-4 h-4 text-muted-foreground" />
              <input type="text" placeholder="Buscar por descripción o bien..." value={searchTerm}
                onChange={(e) => setSearchTerm(e.target.value)}
                className="w-full bg-transparent border-b border-emerald-600/30 focus:border-emerald-600 text-sm font-medium py-2 pl-6 outline-none" />
            </div>
          </div>
        </CardContent>
      </Card>

      {loading ? (
        <div className="flex justify-center py-20"><Loader2 className="w-8 h-8 animate-spin text-emerald-600" /></div>
      ) : error ? (
        <div className="flex flex-col items-center gap-3 text-rose-600 py-12">
          <AlertTriangle className="w-10 h-10" />
          <p className="font-semibold">{error}</p>
          <Button variant="secondary" onClick={fetchOrders}>Reintentar</Button>
        </div>
      ) : orders.length === 0 ? (
        <div className="flex flex-col items-center gap-3 text-muted-foreground py-12">
          <AlertCircle className="w-10 h-10" />
          <p className="font-semibold">No hay órdenes de trabajo</p>
        </div>
      ) : (
        <Card>
          <CardContent className="p-0">
            <div className="overflow-x-auto">
              <table className="min-w-full divide-y divide-border">
                <thead className="bg-muted/50">
                  <tr>
                    <th className="px-5 py-3 text-left text-xs font-bold text-muted-foreground uppercase">Tipo</th>
                    <th className="px-5 py-3 text-left text-xs font-bold text-muted-foreground uppercase">Bien</th>
                    <th className="px-5 py-3 text-left text-xs font-bold text-muted-foreground uppercase">Descripción</th>
                    <th className="px-5 py-3 text-left text-xs font-bold text-muted-foreground uppercase">Prioridad</th>
                    <th className="px-5 py-3 text-left text-xs font-bold text-muted-foreground uppercase">Origen</th>
                    <th className="px-5 py-3 text-left text-xs font-bold text-muted-foreground uppercase">Proveedor</th>
                    <th className="px-5 py-3 text-left text-xs font-bold text-muted-foreground uppercase">Fecha Programada</th>
                    <th className="px-5 py-3 text-left text-xs font-bold text-muted-foreground uppercase">Estado</th>
                    <th className="px-5 py-3 text-right text-xs font-bold text-muted-foreground uppercase">Acciones</th>
                  </tr>
                </thead>
                <tbody className="divide-y divide-border">
                  {orders.map((o) => (
                    <tr key={o.id} className="hover:bg-muted/30 transition-colors">
                      <td className="px-5 py-3 text-sm font-medium">{typeLabels[o.orderType] || o.orderType}</td>
                      <td className="px-5 py-3">
                        <p className="text-sm font-medium">{o.assetName}</p>
                        <p className="text-xs text-muted-foreground">{o.assetLocation}</p>
                      </td>
                      <td className="px-5 py-3 text-sm text-muted-foreground max-w-[200px] truncate">{o.description}</td>
                      <td className="px-5 py-3">{priorityBadge(o.priority)}</td>
                      <td className="px-5 py-3 text-sm text-muted-foreground">{originLabels[o.origin] || o.origin}</td>
                      <td className="px-5 py-3 text-sm text-muted-foreground">{o.assignedProviderName || '—'}</td>
                      <td className="px-5 py-3 text-sm text-muted-foreground">{formatDate(o.scheduledDate)}</td>
                      <td className="px-5 py-3">{statusBadge(o.status)}</td>
                      <td className="px-5 py-3 text-right">
                        <button onClick={() => router.push(`/maintenance/work-orders/${o.id}`)}
                          className="text-emerald-600 hover:text-emerald-800 text-sm font-semibold">Ver</button>
                      </td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>
            <div className="px-5 py-3 border-t border-border text-xs text-muted-foreground">
              {orders.length} orden(es) encontrada(s)
            </div>
          </CardContent>
        </Card>
      )}
    </div>
  );
}
