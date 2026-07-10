'use client';

import React, { useState, useEffect } from 'react';
import { useRouter, useSearchParams } from 'next/navigation';
import { Loader2, ArrowLeft, Save, AlertTriangle, Send, FileText, Calendar } from 'lucide-react';
import { Button } from '@/components/ui/Button';
import { Card, CardContent } from '@/components/ui/Card';
import supplierService, { CreateContractRequest, ProviderListItem } from '@/lib/supplier-service';

const contractTypes = [
  { value: 'ServiceAgreement', label: 'Contrato de Servicios' },
  { value: 'Supply', label: 'Suministro' },
  { value: 'CivilWorks', label: 'Obra Civil' },
  { value: 'Lease', label: 'Arrendamiento' },
];

export default function NewContractPage() {
  const router = useRouter();
  const searchParams = useSearchParams();
  const preselectedProviderId = searchParams.get('providerId') || '';

  const [providers, setProviders] = useState<ProviderListItem[]>([]);
  const [loadingProviders, setLoadingProviders] = useState(true);
  const [submitting, setSubmitting] = useState(false);
  const [error, setError] = useState('');
  const [createdId, setCreatedId] = useState<string | null>(null);

  const [providerId, setProviderId] = useState(preselectedProviderId);
  const [contractNumber, setContractNumber] = useState('');
  const [contractType, setContractType] = useState('ServiceAgreement');
  const [objectDescription, setObjectDescription] = useState('');
  const [totalValue, setTotalValue] = useState('');
  const [monthlyValue, setMonthlyValue] = useState('');
  const [isRecurrent, setIsRecurrent] = useState(false);
  const [startDate, setStartDate] = useState('');
  const [endDate, setEndDate] = useState('');
  const [hasAutoRenewal, setHasAutoRenewal] = useState(false);
  const [autoRenewalNoticeDays, setAutoRenewalNoticeDays] = useState('30');
  const [observations, setObservations] = useState('');

  useEffect(() => {
    const fetchProviders = async () => {
      try {
        const data = await supplierService.getProviders('Active');
        setProviders(data);
      } catch {
        setError('Error al cargar los proveedores.');
      } finally {
        setLoadingProviders(false);
      }
    };
    fetchProviders();
  }, []);

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setError('');

    if (!providerId) { setError('Debe seleccionar un proveedor.'); return; }
    if (!contractNumber.trim()) { setError('El número de contrato es requerido.'); return; }
    if (!objectDescription.trim()) { setError('El objeto del contrato es requerido.'); return; }
    if (!totalValue || parseFloat(totalValue) <= 0) { setError('El valor total debe ser mayor a 0.'); return; }
    if (!startDate) { setError('La fecha de inicio es requerida.'); return; }
    if (!endDate) { setError('La fecha de finalización es requerida.'); return; }

    if (new Date(endDate) <= new Date(startDate)) {
      setError('La fecha de finalización debe ser posterior a la fecha de inicio.');
      return;
    }

    setSubmitting(true);
    try {
      const request: CreateContractRequest = {
        providerId,
        contractNumber: contractNumber.trim(),
        contractType,
        objectDescription: objectDescription.trim(),
        totalValue: parseFloat(totalValue),
        monthlyValue: monthlyValue ? parseFloat(monthlyValue) : 0,
        isRecurrent,
        startDate,
        endDate,
        hasAutoRenewal,
        autoRenewalNoticeDays: parseInt(autoRenewalNoticeDays) || 30,
        observations: observations.trim() || undefined,
      };
      const result = await supplierService.createContract(request);
      setCreatedId(result.id);
    } catch (err: any) {
      setError(err?.response?.data?.error || 'Error al crear el contrato.');
    } finally {
      setSubmitting(false);
    }
  };

  if (loadingProviders) {
    return <div className="flex justify-center py-20"><Loader2 className="w-8 h-8 animate-spin text-emerald-600" /></div>;
  }

  if (createdId) {
    return (
      <div className="space-y-6 max-w-2xl mx-auto">
        <button onClick={() => router.push('/contracts')} className="inline-flex items-center gap-2 text-sm text-muted-foreground hover:text-foreground transition-colors">
          <ArrowLeft className="w-4 h-4" /> Volver a Contratos
        </button>
        <Card>
          <CardContent className="p-6 text-center">
            <div className="w-16 h-16 bg-emerald-50 rounded-full flex items-center justify-center mx-auto mb-4">
              <Send className="w-8 h-8 text-emerald-600" />
            </div>
            <h2 className="text-xl font-bold text-foreground">Contrato Creado Exitosamente</h2>
            <p className="text-sm text-muted-foreground mt-2">El contrato ha sido registrado y está en estado Borrador.</p>
            <div className="mt-6 flex justify-center gap-3">
              <Button variant="secondary" onClick={() => router.push('/contracts')}>Volver a Contratos</Button>
              <Button onClick={() => router.push(`/contracts/${createdId}`)}>Ver Detalle</Button>
            </div>
          </CardContent>
        </Card>
      </div>
    );
  }

  return (
    <div className="space-y-6 max-w-3xl mx-auto">
      <button onClick={() => router.push('/contracts')} className="inline-flex items-center gap-2 text-sm text-muted-foreground hover:text-foreground transition-colors">
        <ArrowLeft className="w-4 h-4" /> Volver a Contratos
      </button>

      <div>
        <h1 className="text-2xl font-bold text-foreground tracking-tight">Nuevo Contrato</h1>
        <p className="text-sm text-muted-foreground mt-1">Registra un nuevo contrato con un proveedor.</p>
      </div>

      <Card>
        <CardContent className="p-6">
          <form onSubmit={handleSubmit} className="space-y-6">
            <div>
              <h3 className="text-sm font-bold text-foreground mb-4 flex items-center gap-2">
                <FileText className="w-4 h-4 text-emerald-600" /> Información del Contrato
              </h3>
              <div className="grid grid-cols-1 md:grid-cols-2 gap-5">
                <div className="md:col-span-2">
                  <label className="block text-xs font-bold text-muted-foreground uppercase tracking-widest mb-1.5">Proveedor *</label>
                  <select value={providerId} onChange={(e) => setProviderId(e.target.value)}
                    className="w-full bg-transparent border-b border-emerald-600/30 focus:border-emerald-600 text-sm font-medium py-2 outline-none" required>
                    <option value="">Seleccione un proveedor...</option>
                    {providers.map((p) => <option key={p.id} value={p.id}>{p.businessName} — {p.documentNumber}</option>)}
                  </select>
                </div>
                <div>
                  <label className="block text-xs font-bold text-muted-foreground uppercase tracking-widest mb-1.5">Nro. Contrato *</label>
                  <input type="text" placeholder="Número del contrato" value={contractNumber}
                    onChange={(e) => setContractNumber(e.target.value.slice(0, 50))}
                    maxLength={50} required
                    className="w-full bg-transparent border-b border-emerald-600/30 focus:border-emerald-600 text-sm font-medium py-2 outline-none" />
                </div>
                <div>
                  <label className="block text-xs font-bold text-muted-foreground uppercase tracking-widest mb-1.5">Tipo de Contrato *</label>
                  <select value={contractType} onChange={(e) => setContractType(e.target.value)}
                    className="w-full bg-transparent border-b border-emerald-600/30 focus:border-emerald-600 text-sm font-medium py-2 outline-none">
                    {contractTypes.map((t) => <option key={t.value} value={t.value}>{t.label}</option>)}
                  </select>
                </div>
                <div className="md:col-span-2">
                  <label className="block text-xs font-bold text-muted-foreground uppercase tracking-widest mb-1.5">Objeto del Contrato *</label>
                  <textarea placeholder="Describa el objeto del contrato..." value={objectDescription}
                    onChange={(e) => setObjectDescription(e.target.value.slice(0, 2000))} rows={3}
                    maxLength={2000} required
                    className="w-full bg-slate-50 dark:bg-slate-900 border border-border focus:border-emerald-600 rounded-md text-sm p-3 outline-none resize-none" />
                </div>
              </div>
            </div>

            <div className="border-t border-border pt-6">
              <h3 className="text-sm font-bold text-foreground mb-4 flex items-center gap-2">
                <Calendar className="w-4 h-4 text-emerald-600" /> Vigencia y Valores
              </h3>
              <div className="grid grid-cols-1 md:grid-cols-2 gap-5">
                <div>
                  <label className="block text-xs font-bold text-muted-foreground uppercase tracking-widest mb-1.5">Valor Total (COP) *</label>
                  <input type="number" placeholder="0" value={totalValue}
                    onChange={(e) => setTotalValue(e.target.value)} min="0" step="0.01"
                    className="w-full bg-transparent border-b border-emerald-600/30 focus:border-emerald-600 text-sm font-medium py-2 outline-none" required />
                </div>
                <div>
                  <label className="block text-xs font-bold text-muted-foreground uppercase tracking-widest mb-1.5">Valor Mensual (COP)</label>
                  <input type="number" placeholder="0" value={monthlyValue}
                    onChange={(e) => setMonthlyValue(e.target.value)} min="0" step="0.01"
                    className="w-full bg-transparent border-b border-emerald-600/30 focus:border-emerald-600 text-sm font-medium py-2 outline-none" />
                </div>
                <div>
                  <label className="block text-xs font-bold text-muted-foreground uppercase tracking-widest mb-1.5">Fecha de Inicio *</label>
                  <input type="date" value={startDate} onChange={(e) => setStartDate(e.target.value)}
                    className="w-full bg-transparent border-b border-emerald-600/30 focus:border-emerald-600 text-sm font-medium py-2 outline-none" required />
                </div>
                <div>
                  <label className="block text-xs font-bold text-muted-foreground uppercase tracking-widest mb-1.5">Fecha de Finalización *</label>
                  <input type="date" value={endDate} onChange={(e) => setEndDate(e.target.value)}
                    className="w-full bg-transparent border-b border-emerald-600/30 focus:border-emerald-600 text-sm font-medium py-2 outline-none" required />
                </div>
                <div>
                  <label className="block text-xs font-bold text-muted-foreground uppercase tracking-widest mb-1.5">Recurrente</label>
                  <div className="flex items-center gap-3 mt-2">
                    <label className="flex items-center gap-2 text-sm text-foreground cursor-pointer">
                      <input type="checkbox" checked={isRecurrent} onChange={(e) => setIsRecurrent(e.target.checked)}
                        className="accent-emerald-600 w-4 h-4" />
                      Es un contrato recurrente
                    </label>
                  </div>
                </div>
                <div>
                  <label className="block text-xs font-bold text-muted-foreground uppercase tracking-widest mb-1.5">Renovación Automática</label>
                  <div className="flex items-center gap-3 mt-2">
                    <label className="flex items-center gap-2 text-sm text-foreground cursor-pointer">
                      <input type="checkbox" checked={hasAutoRenewal} onChange={(e) => setHasAutoRenewal(e.target.checked)}
                        className="accent-emerald-600 w-4 h-4" />
                      Renovar automáticamente
                    </label>
                  </div>
                </div>
                {hasAutoRenewal && (
                  <div>
                    <label className="block text-xs font-bold text-muted-foreground uppercase tracking-widest mb-1.5">Días de Aviso</label>
                    <input type="number" value={autoRenewalNoticeDays}
                      onChange={(e) => setAutoRenewalNoticeDays(e.target.value)} min="1" max="365"
                      className="w-full bg-transparent border-b border-emerald-600/30 focus:border-emerald-600 text-sm font-medium py-2 outline-none" />
                  </div>
                )}
                <div className="md:col-span-2">
                  <label className="block text-xs font-bold text-muted-foreground uppercase tracking-widest mb-1.5">Observaciones</label>
                  <textarea placeholder="Observaciones adicionales..." value={observations}
                    onChange={(e) => setObservations(e.target.value.slice(0, 1000))} rows={2}
                    maxLength={1000}
                    className="w-full bg-slate-50 dark:bg-slate-900 border border-border focus:border-emerald-600 rounded-md text-sm p-3 outline-none resize-none" />
                </div>
              </div>
            </div>

            {error && (
              <div className="p-3 bg-rose-50 border border-rose-200 rounded-lg text-rose-700 text-xs flex items-center gap-2">
                <AlertTriangle className="w-4 h-4 shrink-0" /> {error}
              </div>
            )}

            <div className="flex justify-between items-center pt-4 border-t border-border">
              <Button type="button" variant="ghost" onClick={() => router.push('/contracts')}>Cancelar</Button>
              <Button type="submit" disabled={submitting}>
                {submitting ? <Loader2 className="w-4 h-4 animate-spin mr-2" /> : <Save className="w-4 h-4 mr-2" />}
                Crear Contrato
              </Button>
            </div>
          </form>
        </CardContent>
      </Card>
    </div>
  );
}
