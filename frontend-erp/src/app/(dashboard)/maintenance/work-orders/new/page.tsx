'use client';

import React, { useState, useEffect } from 'react';
import { useRouter } from 'next/navigation';
import { Loader2, ArrowLeft, Save, AlertTriangle, FileText } from 'lucide-react';
import { Button } from '@/components/ui/Button';
import { Card, CardContent } from '@/components/ui/Card';
import maintenanceService, { CreateWorkOrderRequest, CommonAssetListItem } from '@/lib/maintenance-service';
import supplierService, { ProviderListItem } from '@/lib/supplier-service';
import budgetService, { ExpenseExecutionItem } from '@/lib/budget-service';

const orderTypes = [
  { value: 'Preventive', label: 'Preventivo' },
  { value: 'Corrective', label: 'Correctivo' },
];

const priorities = [
  { value: 'Emergency', label: 'Emergencia' },
  { value: 'High', label: 'Alta' },
  { value: 'Medium', label: 'Media' },
  { value: 'Low', label: 'Baja' },
];

const origins = [
  { value: 'AutomaticScheduling', label: 'Programación Automática' },
  { value: 'AdminReport', label: 'Reporte del Administrador' },
  { value: 'ResidentPqr', label: 'PQR de Residente' },
];

export default function NewWorkOrderPage() {
  const router = useRouter();
  const [submitting, setSubmitting] = useState(false);
  const [error, setError] = useState('');
  const [createdId, setCreatedId] = useState<string | null>(null);

  const [assets, setAssets] = useState<CommonAssetListItem[]>([]);
  const [providers, setProviders] = useState<ProviderListItem[]>([]);
  const [expenseItems, setExpenseItems] = useState<ExpenseExecutionItem[]>([]);

  const [orderType, setOrderType] = useState('Corrective');
  const [assetId, setAssetId] = useState('');
  const [description, setDescription] = useState('');
  const [priority, setPriority] = useState('Medium');
  const [origin, setOrigin] = useState('AdminReport');
  const [assignedProviderId, setAssignedProviderId] = useState('');
  const [scheduledDate, setScheduledDate] = useState('');
  const [estimatedCost, setEstimatedCost] = useState('');
  const [budgetItemId, setBudgetItemId] = useState('');

  useEffect(() => {
    const loadData = async () => {
      try {
        const [assetsData, providersData] = await Promise.all([
          maintenanceService.getAssets(),
          supplierService.getProviders('Active'),
        ]);
        setAssets(assetsData);
        setProviders(providersData);
      } catch {}

      try {
        const currentYear = new Date().getFullYear();
        const execution = await budgetService.getBudgetExecution(currentYear);
        setExpenseItems(execution.expenseItems);
      } catch {
        setExpenseItems([]);
      }
    };
    loadData();
  }, []);

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setError('');

    if (!assetId) { setError('Debe seleccionar un bien.'); return; }
    if (!description.trim()) { setError('La descripción es requerida.'); return; }
    if (!scheduledDate) { setError('La fecha programada es requerida.'); return; }

    setSubmitting(true);
    try {
      const request: CreateWorkOrderRequest = {
        orderType,
        assetId,
        description: description.trim(),
        priority,
        origin,
        assignedProviderId: assignedProviderId || undefined,
        scheduledDate,
        estimatedCost: estimatedCost ? parseFloat(estimatedCost) : undefined,
        budgetItemId: budgetItemId || undefined,
      };
      const result = await maintenanceService.createWorkOrder(request);
      setCreatedId(result.id);
    } catch (err: any) {
      setError(err?.response?.data?.error || 'Error al crear la orden de trabajo.');
    } finally {
      setSubmitting(false);
    }
  };

  if (createdId) {
    return (
      <div className="space-y-6 max-w-2xl mx-auto">
        <button onClick={() => router.push('/maintenance/work-orders')} className="inline-flex items-center gap-2 text-sm text-muted-foreground hover:text-foreground transition-colors">
          <ArrowLeft className="w-4 h-4" /> Volver a Órdenes
        </button>
        <Card>
          <CardContent className="p-6 text-center">
            <div className="w-16 h-16 bg-emerald-50 rounded-full flex items-center justify-center mx-auto mb-4">
              <FileText className="w-8 h-8 text-emerald-600" />
            </div>
            <h2 className="text-xl font-bold text-foreground">Orden Creada Exitosamente</h2>
            <p className="text-sm text-muted-foreground mt-2">La orden de trabajo ha sido registrada.</p>
            <div className="mt-6 flex justify-center gap-3">
              <Button variant="secondary" onClick={() => router.push('/maintenance/work-orders')}>Volver a Órdenes</Button>
              <Button onClick={() => router.push(`/maintenance/work-orders/${createdId}`)}>Ver Detalle</Button>
            </div>
          </CardContent>
        </Card>
      </div>
    );
  }

  return (
    <div className="space-y-6 max-w-3xl mx-auto">
      <button onClick={() => router.push('/maintenance/work-orders')} className="inline-flex items-center gap-2 text-sm text-muted-foreground hover:text-foreground transition-colors">
        <ArrowLeft className="w-4 h-4" /> Volver a Órdenes
      </button>

      <div>
        <h1 className="text-2xl font-bold text-foreground tracking-tight">Nueva Orden de Trabajo</h1>
        <p className="text-sm text-muted-foreground mt-1">Crea una nueva orden de mantenimiento preventivo o correctivo.</p>
      </div>

      <Card>
        <CardContent className="p-6">
          <form onSubmit={handleSubmit} className="space-y-6">
            <div className="grid grid-cols-1 md:grid-cols-2 gap-5">
              <div>
                <label className="block text-xs font-bold text-muted-foreground uppercase tracking-widest mb-1.5">Tipo de Orden *</label>
                <select value={orderType} onChange={(e) => setOrderType(e.target.value)}
                  className="w-full bg-transparent border-b border-emerald-600/30 focus:border-emerald-600 text-sm font-medium py-2 outline-none">
                  {orderTypes.map((t) => <option key={t.value} value={t.value}>{t.label}</option>)}
                </select>
              </div>
              <div>
                <label className="block text-xs font-bold text-muted-foreground uppercase tracking-widest mb-1.5">Prioridad *</label>
                <select value={priority} onChange={(e) => setPriority(e.target.value)}
                  className="w-full bg-transparent border-b border-emerald-600/30 focus:border-emerald-600 text-sm font-medium py-2 outline-none">
                  {priorities.map((p) => <option key={p.value} value={p.value}>{p.label}</option>)}
                </select>
              </div>
              <div className="md:col-span-2">
                <label className="block text-xs font-bold text-muted-foreground uppercase tracking-widest mb-1.5">Bien Afectado *</label>
                <select value={assetId} onChange={(e) => setAssetId(e.target.value)} required
                  className="w-full bg-transparent border-b border-emerald-600/30 focus:border-emerald-600 text-sm font-medium py-2 outline-none">
                  <option value="">Seleccione un bien...</option>
                  {assets.map((a) => <option key={a.id} value={a.id}>{a.name} — {a.location}</option>)}
                </select>
              </div>
              <div className="md:col-span-2">
                <label className="block text-xs font-bold text-muted-foreground uppercase tracking-widest mb-1.5">Descripción *</label>
                <textarea value={description} onChange={(e) => setDescription(e.target.value)} rows={3} required
                  placeholder="Describa el trabajo a realizar o la falla detectada..."
                  className="w-full bg-slate-50 dark:bg-slate-900 border border-border focus:border-emerald-600 rounded-md text-sm p-3 outline-none resize-none" />
              </div>
              <div>
                <label className="block text-xs font-bold text-muted-foreground uppercase tracking-widest mb-1.5">Origen *</label>
                <select value={origin} onChange={(e) => setOrigin(e.target.value)}
                  className="w-full bg-transparent border-b border-emerald-600/30 focus:border-emerald-600 text-sm font-medium py-2 outline-none">
                  {origins.map((o) => <option key={o.value} value={o.value}>{o.label}</option>)}
                </select>
              </div>
              <div>
                <label className="block text-xs font-bold text-muted-foreground uppercase tracking-widest mb-1.5">Proveedor Asignado</label>
                <select value={assignedProviderId} onChange={(e) => setAssignedProviderId(e.target.value)}
                  className="w-full bg-transparent border-b border-emerald-600/30 focus:border-emerald-600 text-sm font-medium py-2 outline-none">
                  <option value="">Sin asignar</option>
                  {providers.map((p) => <option key={p.id} value={p.id}>{p.businessName}</option>)}
                </select>
              </div>
              <div>
                <label className="block text-xs font-bold text-muted-foreground uppercase tracking-widest mb-1.5">Fecha Programada *</label>
                <input type="datetime-local" value={scheduledDate}
                  onChange={(e) => setScheduledDate(e.target.value)} required
                  className="w-full bg-transparent border-b border-emerald-600/30 focus:border-emerald-600 text-sm font-medium py-2 outline-none" />
              </div>
              <div>
                <label className="block text-xs font-bold text-muted-foreground uppercase tracking-widest mb-1.5">Costo Estimado</label>
                <input type="number" placeholder="Valor en COP" value={estimatedCost}
                  onChange={(e) => setEstimatedCost(e.target.value)} min="0"
                  className="w-full bg-transparent border-b border-emerald-600/30 focus:border-emerald-600 text-sm font-medium py-2 outline-none" />
              </div>
              <div className="md:col-span-2">
                <label className="block text-xs font-bold text-muted-foreground uppercase tracking-widest mb-1.5">Rubro Presupuestal</label>
                <select value={budgetItemId} onChange={(e) => setBudgetItemId(e.target.value)}
                  className="w-full bg-transparent border-b border-emerald-600/30 focus:border-emerald-600 text-sm font-medium py-2 outline-none">
                  <option value="">Sin imputar al presupuesto</option>
                  {expenseItems.map((item) => <option key={item.id} value={item.id}>{item.name} (disponible: ${item.availableValue.toLocaleString()})</option>)}
                </select>
              </div>
            </div>

            {error && (
              <div className="p-3 bg-rose-50 border border-rose-200 rounded-lg text-rose-700 text-xs flex items-center gap-2">
                <AlertTriangle className="w-4 h-4 shrink-0" /> {error}
              </div>
            )}

            <div className="flex justify-between items-center pt-4 border-t border-border">
              <Button type="button" variant="ghost" onClick={() => router.push('/maintenance/work-orders')}>Cancelar</Button>
              <Button type="submit" disabled={submitting}>
                {submitting ? <Loader2 className="w-4 h-4 animate-spin mr-2" /> : <Save className="w-4 h-4 mr-2" />}
                Crear Orden
              </Button>
            </div>
          </form>
        </CardContent>
      </Card>
    </div>
  );
}
