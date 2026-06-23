'use client';

import React, { useState, useEffect } from 'react';
import { useRouter } from 'next/navigation';
import { Loader2, ArrowLeft, Save, AlertTriangle, AlertCircle } from 'lucide-react';
import { Button } from '@/components/ui/Button';
import { Card, CardContent } from '@/components/ui/Card';
import maintenanceService, { CreateIncidentRequest, WorkOrderListItem } from '@/lib/maintenance-service';

const incidentTypes = [
  { value: 'Flood', label: 'Inundación' },
  { value: 'Fire', label: 'Incendio' },
  { value: 'StructuralDamage', label: 'Daño Estructural' },
  { value: 'ElectricalFailure', label: 'Falla Eléctrica' },
  { value: 'Other', label: 'Otro' },
];

export default function NewIncidentPage() {
  const router = useRouter();
  const [submitting, setSubmitting] = useState(false);
  const [error, setError] = useState('');
  const [createdId, setCreatedId] = useState<string | null>(null);

  const [orders, setOrders] = useState<WorkOrderListItem[]>([]);

  const [name, setName] = useState('');
  const [description, setDescription] = useState('');
  const [incidentType, setIncidentType] = useState('');
  const [occurredAt, setOccurredAt] = useState('');
  const [totalDamageValue, setTotalDamageValue] = useState('');
  const [insurancePolicyNumber, setInsurancePolicyNumber] = useState('');
  const [insuranceCompany, setInsuranceCompany] = useState('');
  const [selectedOrderIds, setSelectedOrderIds] = useState<string[]>([]);

  useEffect(() => {
    maintenanceService.getWorkOrders().then(setOrders).catch(() => {});
  }, []);

  const toggleOrder = (id: string) => {
    setSelectedOrderIds((prev) =>
      prev.includes(id) ? prev.filter((x) => x !== id) : [...prev, id]
    );
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setError('');

    if (!name.trim()) { setError('El nombre del siniestro es requerido.'); return; }
    if (!incidentType) { setError('El tipo de siniestro es requerido.'); return; }
    if (!occurredAt) { setError('La fecha del siniestro es requerida.'); return; }

    setSubmitting(true);
    try {
      const request: CreateIncidentRequest = {
        name: name.trim(),
        description: description.trim(),
        incidentType,
        occurredAt,
        totalDamageValue: totalDamageValue ? parseFloat(totalDamageValue) : undefined,
        insurancePolicyNumber: insurancePolicyNumber || undefined,
        insuranceCompany: insuranceCompany || undefined,
        workOrderIds: selectedOrderIds.length > 0 ? selectedOrderIds : undefined,
      };
      const result = await maintenanceService.createIncident(request);
      setCreatedId(result.id);
    } catch (err: any) {
      setError(err?.response?.data?.error || 'Error al crear el siniestro.');
    } finally {
      setSubmitting(false);
    }
  };

  if (createdId) {
    return (
      <div className="space-y-6 max-w-2xl mx-auto">
        <button onClick={() => router.push('/maintenance/incidents')} className="inline-flex items-center gap-2 text-sm text-muted-foreground hover:text-foreground transition-colors">
          <ArrowLeft className="w-4 h-4" /> Volver a Siniestros
        </button>
        <Card>
          <CardContent className="p-6 text-center">
            <div className="w-16 h-16 bg-emerald-50 rounded-full flex items-center justify-center mx-auto mb-4">
              <AlertCircle className="w-8 h-8 text-emerald-600" />
            </div>
            <h2 className="text-xl font-bold text-foreground">Siniestro Creado Exitosamente</h2>
            <p className="text-sm text-muted-foreground mt-2">El siniestro ha sido registrado.</p>
            <div className="mt-6 flex justify-center gap-3">
              <Button variant="secondary" onClick={() => router.push('/maintenance/incidents')}>Volver a Siniestros</Button>
              <Button onClick={() => router.push(`/maintenance/incidents/${createdId}`)}>Ver Detalle</Button>
            </div>
          </CardContent>
        </Card>
      </div>
    );
  }

  return (
    <div className="space-y-6 max-w-3xl mx-auto">
      <button onClick={() => router.push('/maintenance/incidents')} className="inline-flex items-center gap-2 text-sm text-muted-foreground hover:text-foreground transition-colors">
        <ArrowLeft className="w-4 h-4" /> Volver a Siniestros
      </button>

      <div>
        <h1 className="text-2xl font-bold text-foreground tracking-tight">Nuevo Siniestro</h1>
        <p className="text-sm text-muted-foreground mt-1">Registra un evento de siniestro y vincula las órdenes de trabajo relacionadas.</p>
      </div>

      <Card>
        <CardContent className="p-6">
          <form onSubmit={handleSubmit} className="space-y-6">
            <div className="grid grid-cols-1 md:grid-cols-2 gap-5">
              <div className="md:col-span-2">
                <label className="block text-xs font-bold text-muted-foreground uppercase tracking-widest mb-1.5">Nombre del Siniestro *</label>
                <input type="text" placeholder="Ej: Inundación Torre A Nivel 1" value={name}
                  onChange={(e) => setName(e.target.value)} maxLength={300} required
                  className="w-full bg-transparent border-b border-emerald-600/30 focus:border-emerald-600 text-sm font-medium py-2 outline-none" />
              </div>
              <div>
                <label className="block text-xs font-bold text-muted-foreground uppercase tracking-widest mb-1.5">Tipo de Siniestro *</label>
                <select value={incidentType} onChange={(e) => setIncidentType(e.target.value)} required
                  className="w-full bg-transparent border-b border-emerald-600/30 focus:border-emerald-600 text-sm font-medium py-2 outline-none">
                  <option value="">Seleccione...</option>
                  {incidentTypes.map((t) => <option key={t.value} value={t.value}>{t.label}</option>)}
                </select>
              </div>
              <div>
                <label className="block text-xs font-bold text-muted-foreground uppercase tracking-widest mb-1.5">Fecha del Siniestro *</label>
                <input type="datetime-local" value={occurredAt}
                  onChange={(e) => setOccurredAt(e.target.value)} required
                  className="w-full bg-transparent border-b border-emerald-600/30 focus:border-emerald-600 text-sm font-medium py-2 outline-none" />
              </div>
              <div>
                <label className="block text-xs font-bold text-muted-foreground uppercase tracking-widest mb-1.5">Valor Total del Daño</label>
                <input type="number" placeholder="Valor en COP" value={totalDamageValue}
                  onChange={(e) => setTotalDamageValue(e.target.value)} min="0"
                  className="w-full bg-transparent border-b border-emerald-600/30 focus:border-emerald-600 text-sm font-medium py-2 outline-none" />
              </div>
              <div>
                <label className="block text-xs font-bold text-muted-foreground uppercase tracking-widest mb-1.5">Nro. Póliza de Seguro</label>
                <input type="text" placeholder="Número de póliza" value={insurancePolicyNumber}
                  onChange={(e) => setInsurancePolicyNumber(e.target.value)} maxLength={100}
                  className="w-full bg-transparent border-b border-emerald-600/30 focus:border-emerald-600 text-sm font-medium py-2 outline-none" />
              </div>
              <div>
                <label className="block text-xs font-bold text-muted-foreground uppercase tracking-widest mb-1.5">Aseguradora</label>
                <input type="text" placeholder="Nombre de la aseguradora" value={insuranceCompany}
                  onChange={(e) => setInsuranceCompany(e.target.value)} maxLength={200}
                  className="w-full bg-transparent border-b border-emerald-600/30 focus:border-emerald-600 text-sm font-medium py-2 outline-none" />
              </div>
              <div className="md:col-span-2">
                <label className="block text-xs font-bold text-muted-foreground uppercase tracking-widest mb-1.5">Descripción</label>
                <textarea value={description} onChange={(e) => setDescription(e.target.value)} rows={3}
                  placeholder="Describa el siniestro..."
                  className="w-full bg-slate-50 dark:bg-slate-900 border border-border focus:border-emerald-600 rounded-md text-sm p-3 outline-none resize-none" />
              </div>
            </div>

            {orders.length > 0 && (
              <div className="border-t border-border pt-6">
                <h3 className="text-sm font-bold text-foreground mb-3">Órdenes de Trabajo Relacionadas</h3>
                <p className="text-xs text-muted-foreground mb-3">Selecciona las órdenes que deseas vincular a este siniestro.</p>
                <div className="max-h-60 overflow-y-auto space-y-2 border border-border rounded-lg p-3">
                  {orders.map((o) => (
                    <label key={o.id} className="flex items-center gap-3 p-2 rounded hover:bg-muted/30 cursor-pointer">
                      <input type="checkbox" checked={selectedOrderIds.includes(o.id)}
                        onChange={() => toggleOrder(o.id)} className="accent-emerald-600 w-4 h-4" />
                      <div className="flex-1">
                        <p className="text-sm font-medium">{o.assetName} — {o.description.substring(0, 60)}</p>
                        <p className="text-xs text-muted-foreground">{o.status} | {o.priority}</p>
                      </div>
                    </label>
                  ))}
                </div>
              </div>
            )}

            {error && (
              <div className="p-3 bg-rose-50 border border-rose-200 rounded-lg text-rose-700 text-xs flex items-center gap-2">
                <AlertTriangle className="w-4 h-4 shrink-0" /> {error}
              </div>
            )}

            <div className="flex justify-between items-center pt-4 border-t border-border">
              <Button type="button" variant="ghost" onClick={() => router.push('/maintenance/incidents')}>Cancelar</Button>
              <Button type="submit" disabled={submitting}>
                {submitting ? <Loader2 className="w-4 h-4 animate-spin mr-2" /> : <Save className="w-4 h-4 mr-2" />}
                Crear Siniestro
              </Button>
            </div>
          </form>
        </CardContent>
      </Card>
    </div>
  );
}
