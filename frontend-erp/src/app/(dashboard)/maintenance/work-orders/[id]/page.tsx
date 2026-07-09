'use client';

import React, { useState, useEffect } from 'react';
import { useRouter, useParams } from 'next/navigation';
import { Loader2, ArrowLeft, AlertTriangle, Play, CheckCircle, XCircle, Clock, DollarSign, Calendar, User, FileText } from 'lucide-react';
import { Button } from '@/components/ui/Button';
import { Card, CardContent, CardHeader } from '@/components/ui/Card';
import maintenanceService, { WorkOrderDetail, UpdateWorkOrderRequest } from '@/lib/maintenance-service';
import supplierService, { ProviderListItem } from '@/lib/supplier-service';

const typeLabels: Record<string, string> = { Preventive: 'Preventivo', Corrective: 'Correctivo' };
const priorityLabels: Record<string, string> = { Emergency: 'Emergencia', High: 'Alta', Medium: 'Media', Low: 'Baja' };
const originLabels: Record<string, string> = { AutomaticScheduling: 'Programación Automática', AdminReport: 'Reporte del Administrador', ResidentPqr: 'PQR de Residente' };
const outcomeLabels: Record<string, string> = { Resolved: 'Resuelto', PartiallyResolved: 'Resuelto Parcialmente', NotResolved: 'No Resuelto' };

const statusBadge = (status: string) => {
  if (status === 'PendingAssignment') return <span className="badge-warning">Pendiente de Asignación</span>;
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

export default function WorkOrderDetailPage() {
  const router = useRouter();
  const params = useParams();
  const id = params.id as string;
  const [order, setOrder] = useState<WorkOrderDetail | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');
  const [providers, setProviders] = useState<ProviderListItem[]>([]);
  const [showUpdateForm, setShowUpdateForm] = useState(false);
  const [updating, setUpdating] = useState(false);

  const [assignProviderId, setAssignProviderId] = useState('');
  const [updateStatus, setUpdateStatus] = useState('');
  const [actualCost, setActualCost] = useState('');
  const [outcome, setOutcome] = useState('');
  const [outcomeNotes, setOutcomeNotes] = useState('');

  const fetchOrder = async () => {
    setLoading(true);
    setError('');
    try {
      const data = await maintenanceService.getWorkOrderById(id);
      setOrder(data);
      setAssignProviderId(data.assignedProviderId || '');
      setUpdateStatus(data.status);
      setActualCost(data.actualCost > 0 ? data.actualCost.toString() : '');
      setOutcome(data.outcome || '');
      setOutcomeNotes(data.outcomeNotes || '');
    } catch {
      setError('Error al cargar la orden de trabajo.');
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    fetchOrder();
    supplierService.getProviders('Active').then(setProviders).catch(() => {});
  }, [id]);

  const handleUpdate = async () => {
    setUpdating(true);
    setError('');
    try {
      const request: UpdateWorkOrderRequest = {
        assignedProviderId: assignProviderId || undefined,
        status: updateStatus || undefined,
        actualCost: actualCost ? parseFloat(actualCost) : undefined,
        outcome: outcome || undefined,
        outcomeNotes: outcomeNotes || undefined,
      };
      await maintenanceService.updateWorkOrder(id, request);
      setShowUpdateForm(false);
      fetchOrder();
    } catch (err: any) {
      setError(err?.response?.data?.error || 'Error al actualizar la orden.');
    } finally {
      setUpdating(false);
    }
  };

  const formatDate = (d: string | null) => d ? new Date(d).toLocaleDateString('es-CO', { day: '2-digit', month: '2-digit', year: 'numeric' }) : '—';
  const formatDateTime = (d: string | null) => d ? new Date(d).toLocaleString('es-CO', { day: '2-digit', month: '2-digit', year: 'numeric', hour: '2-digit', minute: '2-digit' }) : '—';
  const formatCurrency = (val: number) => new Intl.NumberFormat('es-CO', { style: 'currency', currency: 'COP', maximumFractionDigits: 0 }).format(val);

  if (loading) return <div className="flex justify-center py-20"><Loader2 className="w-8 h-8 animate-spin text-emerald-600" /></div>;
  if (error && !order) return (
    <div className="space-y-6 max-w-2xl mx-auto">
      <button onClick={() => router.push('/maintenance/work-orders')} className="inline-flex items-center gap-2 text-sm text-muted-foreground hover:text-foreground">
        <ArrowLeft className="w-4 h-4" /> Volver
      </button>
      <div className="flex flex-col items-center gap-3 text-rose-600 py-12">
        <AlertTriangle className="w-10 h-10" />
        <p className="font-semibold">{error}</p>
      </div>
    </div>
  );

  if (!order) return null;

  const canAdvance = order.status !== 'Completed' && order.status !== 'Cancelled';

  return (
    <div className="space-y-6">
      <button onClick={() => router.push('/maintenance/work-orders')} className="inline-flex items-center gap-2 text-sm text-muted-foreground hover:text-foreground transition-colors">
        <ArrowLeft className="w-4 h-4" /> Volver a Órdenes
      </button>

      <div className="flex flex-col sm:flex-row justify-between items-start sm:items-center gap-4">
        <div>
          <div className="flex items-center gap-3">
            <h1 className="text-2xl font-bold text-foreground tracking-tight">Orden de Trabajo</h1>
            {statusBadge(order.status)}
            {priorityBadge(order.priority)}
          </div>
          <p className="text-sm text-muted-foreground mt-1">
            {typeLabels[order.orderType]} — {order.assetName}
          </p>
        </div>
        <div className="flex gap-2">
          {canAdvance && (
            <Button onClick={() => setShowUpdateForm(!showUpdateForm)}>
              <Play className="w-4 h-4 mr-1" /> Actualizar Estado
            </Button>
          )}
        </div>
      </div>

      {showUpdateForm && (
        <Card>
          <CardHeader className="py-3 px-6"><h3 className="text-sm font-bold text-foreground">Actualizar Orden</h3></CardHeader>
          <CardContent className="p-6">
            <div className="grid grid-cols-1 md:grid-cols-2 gap-5">
              <div>
                <label className="block text-xs font-bold text-muted-foreground uppercase tracking-widest mb-1.5">Proveedor Asignado</label>
                <select value={assignProviderId} onChange={(e) => setAssignProviderId(e.target.value)}
                  className="w-full bg-transparent border-b border-emerald-600/30 focus:border-emerald-600 text-sm font-medium py-2 outline-none">
                  <option value="">Sin asignar</option>
                  {providers.map((p) => <option key={p.id} value={p.id}>{p.businessName}</option>)}
                </select>
              </div>
              <div>
                <label className="block text-xs font-bold text-muted-foreground uppercase tracking-widest mb-1.5">Estado</label>
                <select value={updateStatus} onChange={(e) => setUpdateStatus(e.target.value)}
                  className="w-full bg-transparent border-b border-emerald-600/30 focus:border-emerald-600 text-sm font-medium py-2 outline-none">
                  <option value="PendingAssignment">Pendiente de Asignación</option>
                  <option value="Assigned">Asignada</option>
                  <option value="InProgress">En Progreso</option>
                  <option value="Completed">Completada</option>
                  <option value="Cancelled">Cancelada</option>
                </select>
              </div>
              <div>
                <label className="block text-xs font-bold text-muted-foreground uppercase tracking-widest mb-1.5">Costo Real</label>
                <input type="number" placeholder="Costo real en COP" value={actualCost}
                  onChange={(e) => setActualCost(e.target.value)} min="0"
                  className="w-full bg-transparent border-b border-emerald-600/30 focus:border-emerald-600 text-sm font-medium py-2 outline-none" />
              </div>
              <div>
                <label className="block text-xs font-bold text-muted-foreground uppercase tracking-widest mb-1.5">Resultado</label>
                <select value={outcome} onChange={(e) => setOutcome(e.target.value)}
                  className="w-full bg-transparent border-b border-emerald-600/30 focus:border-emerald-600 text-sm font-medium py-2 outline-none">
                  <option value="">Sin resultado</option>
                  <option value="Resolved">Resuelto</option>
                  <option value="PartiallyResolved">Resuelto Parcialmente</option>
                  <option value="NotResolved">No Resuelto</option>
                </select>
              </div>
              <div className="md:col-span-2">
                <label className="block text-xs font-bold text-muted-foreground uppercase tracking-widest mb-1.5">Notas del Resultado</label>
                <textarea value={outcomeNotes} onChange={(e) => setOutcomeNotes(e.target.value)} rows={3}
                  className="w-full bg-slate-50 dark:bg-slate-900 border border-border focus:border-emerald-600 rounded-md text-sm p-3 outline-none resize-none" />
              </div>
            </div>
            {error && (
              <div className="mt-4 p-3 bg-rose-50 border border-rose-200 rounded-lg text-rose-700 text-xs flex items-center gap-2">
                <AlertTriangle className="w-4 h-4 shrink-0" /> {error}
              </div>
            )}
            <div className="flex gap-2 mt-4">
              <Button variant="ghost" onClick={() => setShowUpdateForm(false)}>Cancelar</Button>
              <Button onClick={handleUpdate} disabled={updating}>
                {updating ? <Loader2 className="w-4 h-4 animate-spin mr-1" /> : <CheckCircle className="w-4 h-4 mr-1" />}
                Guardar Cambios
              </Button>
            </div>
          </CardContent>
        </Card>
      )}

      <div className="grid grid-cols-1 lg:grid-cols-3 gap-6">
        <div className="lg:col-span-2 space-y-6">
          <Card>
            <CardHeader className="py-3 px-6"><h3 className="text-sm font-bold text-foreground">Información de la Orden</h3></CardHeader>
            <CardContent className="p-6">
              <div className="grid grid-cols-2 md:grid-cols-3 gap-4 text-sm">
                <div><span className="text-muted-foreground">Tipo:</span><p className="font-medium">{typeLabels[order.orderType]}</p></div>
                <div><span className="text-muted-foreground">Prioridad:</span><p className="font-medium">{priorityLabels[order.priority]}</p></div>
                <div><span className="text-muted-foreground">Origen:</span><p className="font-medium">{originLabels[order.origin]}</p></div>
                <div className="md:col-span-3"><span className="text-muted-foreground">Descripción:</span><p className="font-medium mt-1">{order.description}</p></div>
                {order.relatedPqrNumber && (
                  <div><span className="text-muted-foreground">PQR Relacionada:</span><p className="font-medium text-emerald-600">{order.relatedPqrNumber}</p></div>
                )}
              </div>
            </CardContent>
          </Card>

          <Card>
            <CardHeader className="py-3 px-6"><h3 className="text-sm font-bold text-foreground">Ejecución y Costos</h3></CardHeader>
            <CardContent className="p-6">
              <div className="grid grid-cols-2 md:grid-cols-3 gap-4 text-sm">
                <div><span className="text-muted-foreground">Proveedor:</span><p className="font-medium">{order.assignedProviderName || 'Sin asignar'}</p></div>
                <div><span className="text-muted-foreground">Fecha Programada:</span><p className="font-medium">{formatDateTime(order.scheduledDate)}</p></div>
                <div><span className="text-muted-foreground">Inicio Ejecución:</span><p className="font-medium">{formatDateTime(order.executionStartDate)}</p></div>
                <div><span className="text-muted-foreground">Fin Ejecución:</span><p className="font-medium">{formatDateTime(order.executionEndDate)}</p></div>
                <div><span className="text-muted-foreground">Costo Estimado:</span><p className="font-medium">{formatCurrency(order.estimatedCost)}</p></div>
                <div><span className="text-muted-foreground">Costo Real:</span><p className="font-medium">{formatCurrency(order.actualCost)}</p></div>
                <div><span className="text-muted-foreground">Rubro Presupuestal:</span><p className="font-medium">{order.expenseItemName || 'Sin imputar'}</p></div>
                {order.outcome && (
                  <div><span className="text-muted-foreground">Resultado:</span><p className="font-medium">{outcomeLabels[order.outcome] || order.outcome}</p></div>
                )}
                {order.outcomeNotes && (
                  <div className="md:col-span-3"><span className="text-muted-foreground">Notas:</span><p className="font-medium mt-1">{order.outcomeNotes}</p></div>
                )}
              </div>
              {order.costAlertSent && (
                <div className="mt-4 p-3 bg-amber-50 border border-amber-200 rounded-lg text-amber-700 text-xs flex items-center gap-2">
                  <DollarSign className="w-4 h-4 shrink-0" /> El costo real superó el costo estimado en más del 20%.
                </div>
              )}
            </CardContent>
          </Card>

          <Card>
            <CardHeader className="py-3 px-6"><h3 className="text-sm font-bold text-foreground">Evidencias Fotográficas</h3></CardHeader>
            <CardContent className="p-4">
              {order.evidences.length === 0 ? (
                <p className="text-center text-sm text-muted-foreground py-4">No hay evidencias registradas.</p>
              ) : (
                <div className="grid grid-cols-2 md:grid-cols-3 gap-3">
                  {order.evidences.map((e) => (
                    <div key={e.id} className="p-3 bg-muted/30 rounded-lg">
                      <div className="aspect-video bg-muted rounded flex items-center justify-center text-xs text-muted-foreground mb-2">
                        {e.isBeforeIntervention ? 'Antes' : 'Después'}
                      </div>
                      <p className="text-xs text-muted-foreground">{e.description || formatDate(e.capturedAt)}</p>
                    </div>
                  ))}
                </div>
              )}
            </CardContent>
          </Card>
        </div>

        <div className="space-y-6">
          <Card>
            <CardHeader className="py-3 px-6"><h3 className="text-sm font-bold text-foreground">Bien Afectado</h3></CardHeader>
            <CardContent className="p-4">
              <p className="text-sm font-medium">{order.assetName}</p>
              <p className="text-xs text-muted-foreground mt-1">{order.assetLocation}</p>
              <button onClick={() => router.push(`/maintenance/${order.assetId}`)}
                className="mt-3 text-emerald-600 hover:text-emerald-800 text-sm font-semibold">
                Ver detalle del bien →
              </button>
            </CardContent>
          </Card>

          <Card>
            <CardHeader className="py-3 px-6"><h3 className="text-sm font-bold text-foreground">Historial</h3></CardHeader>
            <CardContent className="p-4 space-y-3 text-sm">
              <div className="flex items-center gap-2">
                <Clock className="w-4 h-4 text-muted-foreground" />
                <div>
                  <p className="text-muted-foreground">Creada:</p>
                  <p className="font-medium">{formatDateTime(order.createdAt)}</p>
                </div>
              </div>
              {order.updatedAt && (
                <div className="flex items-center gap-2">
                  <Clock className="w-4 h-4 text-muted-foreground" />
                  <div>
                    <p className="text-muted-foreground">Última actualización:</p>
                    <p className="font-medium">{formatDateTime(order.updatedAt)}</p>
                  </div>
                </div>
              )}
            </CardContent>
          </Card>
        </div>
      </div>
    </div>
  );
}
