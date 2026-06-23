'use client';

import React, { useState, useEffect } from 'react';
import { useRouter, useParams } from 'next/navigation';
import { Loader2, ArrowLeft, Save, AlertTriangle, Calendar } from 'lucide-react';
import { Button } from '@/components/ui/Button';
import { Card, CardContent } from '@/components/ui/Card';
import maintenanceService, { CommonAssetListItem, MaintenancePlanSummary } from '@/lib/maintenance-service';
import supplierService, { ProviderListItem } from '@/lib/supplier-service';

const activityTypes = [
  { value: 'Lubrication', label: 'Lubricación' },
  { value: 'Calibration', label: 'Calibración' },
  { value: 'Inspection', label: 'Inspección' },
  { value: 'Cleaning', label: 'Limpieza' },
  { value: 'FilterReplacement', label: 'Cambio de Filtros' },
  { value: 'OilChange', label: 'Cambio de Aceite' },
  { value: 'GeneralRevision', label: 'Revisión General' },
  { value: 'Testing', label: 'Pruebas' },
  { value: 'Painting', label: 'Pintura' },
  { value: 'Landscaping', label: 'Jardinería' },
  { value: 'Other', label: 'Otro' },
];

export default function NewPlanPage() {
  const router = useRouter();
  const params = useParams();
  const assetId = params.id as string;
  const [submitting, setSubmitting] = useState(false);
  const [error, setError] = useState('');
  const [createdId, setCreatedId] = useState<string | null>(null);
  const [providers, setProviders] = useState<ProviderListItem[]>([]);

  const [assetName, setAssetName] = useState('');
  const [activityType, setActivityType] = useState('');
  const [description, setDescription] = useState('');
  const [frequencyDays, setFrequencyDays] = useState('');
  const [preferredProviderId, setPreferredProviderId] = useState('');
  const [estimatedCost, setEstimatedCost] = useState('');
  const [requiresServiceSuspension, setRequiresServiceSuspension] = useState(false);
  const [estimatedDowntimeHours, setEstimatedDowntimeHours] = useState('');

  useEffect(() => {
    supplierService.getProviders('Active').then(setProviders).catch(() => {});
    maintenanceService.getAssetById(assetId).then((a) => setAssetName(a.name)).catch(() => {});
  }, [assetId]);

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setError('');

    if (!activityType) { setError('El tipo de actividad es requerido.'); return; }
    if (!description.trim()) { setError('La descripción es requerida.'); return; }
    if (!frequencyDays || parseInt(frequencyDays) < 1) { setError('La frecuencia debe ser al menos 1 día.'); return; }

    setSubmitting(true);
    try {
      await maintenanceService.createMaintenancePlan({
        assetId,
        activityType,
        description: description.trim(),
        frequencyDays: parseInt(frequencyDays),
        preferredProviderId: preferredProviderId || undefined,
        estimatedCost: estimatedCost ? parseFloat(estimatedCost) : undefined,
        requiresServiceSuspension,
        estimatedUsefulLifeMonths: estimatedDowntimeHours ? parseInt(estimatedDowntimeHours) : undefined,
      } as any);
      setCreatedId('ok');
    } catch (err: any) {
      setError(err?.response?.data?.error || 'Error al crear el plan.');
    } finally {
      setSubmitting(false);
    }
  };

  if (createdId) {
    return (
      <div className="space-y-6 max-w-2xl mx-auto">
        <button onClick={() => router.push(`/maintenance/${assetId}`)} className="inline-flex items-center gap-2 text-sm text-muted-foreground hover:text-foreground transition-colors">
          <ArrowLeft className="w-4 h-4" /> Volver al Bien
        </button>
        <Card>
          <CardContent className="p-6 text-center">
            <div className="w-16 h-16 bg-emerald-50 rounded-full flex items-center justify-center mx-auto mb-4">
              <Calendar className="w-8 h-8 text-emerald-600" />
            </div>
            <h2 className="text-xl font-bold text-foreground">Plan Creado Exitosamente</h2>
            <p className="text-sm text-muted-foreground mt-2">El plan de mantenimiento ha sido registrado.</p>
            <div className="mt-6 flex justify-center gap-3">
              <Button variant="secondary" onClick={() => router.push(`/maintenance/${assetId}`)}>Volver al Bien</Button>
            </div>
          </CardContent>
        </Card>
      </div>
    );
  }

  return (
    <div className="space-y-6 max-w-3xl mx-auto">
      <button onClick={() => router.push(`/maintenance/${assetId}`)} className="inline-flex items-center gap-2 text-sm text-muted-foreground hover:text-foreground transition-colors">
        <ArrowLeft className="w-4 h-4" /> Volver al Bien
      </button>

      <div>
        <h1 className="text-2xl font-bold text-foreground tracking-tight">Nuevo Plan de Mantenimiento</h1>
        <p className="text-sm text-muted-foreground mt-1">Para: {assetName || 'Cargando...'}</p>
      </div>

      <Card>
        <CardContent className="p-6">
          <form onSubmit={handleSubmit} className="space-y-6">
            <div className="grid grid-cols-1 md:grid-cols-2 gap-5">
              <div>
                <label className="block text-xs font-bold text-muted-foreground uppercase tracking-widest mb-1.5">Tipo de Actividad *</label>
                <select value={activityType} onChange={(e) => setActivityType(e.target.value)} required
                  className="w-full bg-transparent border-b border-emerald-600/30 focus:border-emerald-600 text-sm font-medium py-2 outline-none">
                  <option value="">Seleccione...</option>
                  {activityTypes.map((a) => <option key={a.value} value={a.value}>{a.label}</option>)}
                </select>
              </div>
              <div>
                <label className="block text-xs font-bold text-muted-foreground uppercase tracking-widest mb-1.5">Frecuencia (días) *</label>
                <input type="number" placeholder="Ej: 30" value={frequencyDays}
                  onChange={(e) => setFrequencyDays(e.target.value)} min="1" required
                  className="w-full bg-transparent border-b border-emerald-600/30 focus:border-emerald-600 text-sm font-medium py-2 outline-none" />
              </div>
              <div className="md:col-span-2">
                <label className="block text-xs font-bold text-muted-foreground uppercase tracking-widest mb-1.5">Descripción *</label>
                <textarea value={description} onChange={(e) => setDescription(e.target.value)} rows={3} required
                  placeholder="Describa la actividad de mantenimiento..."
                  className="w-full bg-slate-50 dark:bg-slate-900 border border-border focus:border-emerald-600 rounded-md text-sm p-3 outline-none resize-none" />
              </div>
              <div>
                <label className="block text-xs font-bold text-muted-foreground uppercase tracking-widest mb-1.5">Proveedor Preferido</label>
                <select value={preferredProviderId} onChange={(e) => setPreferredProviderId(e.target.value)}
                  className="w-full bg-transparent border-b border-emerald-600/30 focus:border-emerald-600 text-sm font-medium py-2 outline-none">
                  <option value="">Ninguno</option>
                  {providers.map((p) => <option key={p.id} value={p.id}>{p.businessName}</option>)}
                </select>
              </div>
              <div>
                <label className="block text-xs font-bold text-muted-foreground uppercase tracking-widest mb-1.5">Costo Estimado</label>
                <input type="number" placeholder="Valor en COP" value={estimatedCost}
                  onChange={(e) => setEstimatedCost(e.target.value)} min="0"
                  className="w-full bg-transparent border-b border-emerald-600/30 focus:border-emerald-600 text-sm font-medium py-2 outline-none" />
              </div>
              <div>
                <label className="block text-xs font-bold text-muted-foreground uppercase tracking-widest mb-1.5">Requiere Suspensión</label>
                <label className="flex items-center gap-2 text-sm text-foreground cursor-pointer mt-2">
                  <input type="checkbox" checked={requiresServiceSuspension}
                    onChange={(e) => setRequiresServiceSuspension(e.target.checked)}
                    className="accent-emerald-600 w-4 h-4" />
                  Suspensión del servicio
                </label>
              </div>
              {requiresServiceSuspension && (
                <div>
                  <label className="block text-xs font-bold text-muted-foreground uppercase tracking-widest mb-1.5">Horas Fuera de Servicio</label>
                  <input type="number" placeholder="Horas" value={estimatedDowntimeHours}
                    onChange={(e) => setEstimatedDowntimeHours(e.target.value)} min="0"
                    className="w-full bg-transparent border-b border-emerald-600/30 focus:border-emerald-600 text-sm font-medium py-2 outline-none" />
                </div>
              )}
            </div>

            {error && (
              <div className="p-3 bg-rose-50 border border-rose-200 rounded-lg text-rose-700 text-xs flex items-center gap-2">
                <AlertTriangle className="w-4 h-4 shrink-0" /> {error}
              </div>
            )}

            <div className="flex justify-between items-center pt-4 border-t border-border">
              <Button type="button" variant="ghost" onClick={() => router.push(`/maintenance/${assetId}`)}>Cancelar</Button>
              <Button type="submit" disabled={submitting}>
                {submitting ? <Loader2 className="w-4 h-4 animate-spin mr-2" /> : <Save className="w-4 h-4 mr-2" />}
                Crear Plan
              </Button>
            </div>
          </form>
        </CardContent>
      </Card>
    </div>
  );
}
