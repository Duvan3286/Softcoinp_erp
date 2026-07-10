'use client';

import React, { useState, useEffect } from 'react';
import { useRouter, useParams } from 'next/navigation';
import { Loader2, ArrowLeft, AlertTriangle, Star, Phone, Mail, MapPin, FileText, Edit, Trash2, Plus, Send, DollarSign } from 'lucide-react';
import { Button } from '@/components/ui/Button';
import { Card, CardContent, CardHeader } from '@/components/ui/Card';
import supplierService, { ProviderDetail } from '@/lib/supplier-service';

const typeLabels: Record<string, string> = { Natural: 'Natural', Legal: 'Jurídica' };
const statusLabels: Record<string, string> = { Active: 'Activo', Inactive: 'Inactivo' };
const contractTypeLabels: Record<string, string> = {
  ServiceAgreement: 'Contrato de Servicios',
  Supply: 'Suministro',
  CivilWorks: 'Obra Civil',
  Lease: 'Arrendamiento',
};
const recommendationLabels: Record<string, string> = {
  Renew: 'Renovar',
  DoNotRenew: 'No Renovar',
  EvaluateOtherOptions: 'Evaluar Otras Opciones',
};
const invoiceStatusLabels: Record<string, string> = {
  PendingPayment: 'Pendiente',
  PartiallyPaid: 'Parcial',
  FullyPaid: 'Pagada',
};

export default function SupplierDetailPage() {
  const router = useRouter();
  const params = useParams();
  const id = params.id as string;

  const [provider, setProvider] = useState<ProviderDetail | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');
  const [showEvaluationForm, setShowEvaluationForm] = useState(false);
  const [evaluationPeriod, setEvaluationPeriod] = useState('');
  const [qualityScore, setQualityScore] = useState(3);
  const [complianceScore, setComplianceScore] = useState(3);
  const [priceScore, setPriceScore] = useState(3);
  const [attentionScore, setAttentionScore] = useState(3);
  const [evaluationComments, setEvaluationComments] = useState('');
  const [submittingEvaluation, setSubmittingEvaluation] = useState(false);

  const fetchProvider = async () => {
    setLoading(true);
    setError('');
    try {
      const data = await supplierService.getProviderById(id);
      setProvider(data);
    } catch {
      setError('Error al cargar el proveedor.');
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => { fetchProvider(); }, [id]);

  const formatCurrency = (v: number) => new Intl.NumberFormat('es-CO', { style: 'currency', currency: 'COP', maximumFractionDigits: 0 }).format(v);
  const formatDate = (d: string) => new Date(d).toLocaleDateString('es-CO', { day: '2-digit', month: '2-digit', year: 'numeric' });

  const handleDelete = async () => {
    if (!confirm('¿Está seguro de eliminar este proveedor?')) return;
    try {
      await supplierService.deleteProvider(id);
      router.push('/suppliers');
    } catch (err: any) {
      alert(err?.response?.data?.error || 'Error al eliminar.');
    }
  };

  const handleSubmitEvaluation = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!evaluationPeriod.trim()) return;
    setSubmittingEvaluation(true);
    try {
      await supplierService.createProviderEvaluation(id, {
        evaluationPeriod: evaluationPeriod.trim(),
        qualityScore,
        complianceScore,
        priceScore,
        attentionScore,
        comments: evaluationComments || undefined,
      });
      setShowEvaluationForm(false);
      setEvaluationPeriod('');
      setEvaluationComments('');
      setQualityScore(3);
      setComplianceScore(3);
      setPriceScore(3);
      setAttentionScore(3);
      fetchProvider();
    } catch (err: any) {
      alert(err?.response?.data?.error || 'Error al crear la evaluación.');
    } finally {
      setSubmittingEvaluation(false);
    }
  };

  const ScoreSelect = ({ label, value, onChange }: { label: string; value: number; onChange: (v: number) => void }) => (
    <div>
      <label className="block text-xs font-bold text-muted-foreground uppercase tracking-widest mb-1.5">{label}</label>
      <div className="flex gap-1">
        {[1, 2, 3, 4, 5].map((s) => (
          <button key={s} type="button" onClick={() => onChange(s)}
            className={`w-8 h-8 rounded-lg text-xs font-bold transition-colors ${s === value ? 'bg-emerald-600 text-white' : 'bg-muted text-muted-foreground hover:bg-emerald-50'}`}>
            {s}
          </button>
        ))}
      </div>
    </div>
  );

  if (loading) {
    return <div className="flex justify-center py-20"><Loader2 className="w-8 h-8 animate-spin text-emerald-600" /></div>;
  }

  if (error || !provider) {
    return (
      <div className="space-y-6 max-w-2xl mx-auto">
        <button onClick={() => router.push('/suppliers')} className="inline-flex items-center gap-2 text-sm text-muted-foreground hover:text-foreground">
          <ArrowLeft className="w-4 h-4" /> Volver
        </button>
        <div className="flex flex-col items-center gap-3 text-rose-600 py-12">
          <AlertTriangle className="w-10 h-10" />
          <p className="font-semibold">{error || 'Proveedor no encontrado.'}</p>
        </div>
      </div>
    );
  }

  return (
    <div className="space-y-6">
      <button onClick={() => router.push('/suppliers')} className="inline-flex items-center gap-2 text-sm text-muted-foreground hover:text-foreground transition-colors">
        <ArrowLeft className="w-4 h-4" /> Volver a Proveedores
      </button>

      <div className="flex flex-col sm:flex-row justify-between items-start sm:items-center gap-4">
        <div>
          <div className="flex items-center gap-2">
            <h1 className="text-2xl font-bold text-foreground tracking-tight">{provider.businessName}</h1>
          </div>
          <p className="text-sm text-muted-foreground mt-1">
            {typeLabels[provider.providerType]} — {provider.documentNumber} — {provider.status === 'Active' ? 'Activo' : 'Inactivo'}
          </p>
        </div>
        <div className="flex gap-2">
          <Button variant="secondary" onClick={() => router.push(`/suppliers/${id}`)}>
            <Edit className="w-4 h-4 mr-1" /> Editar
          </Button>
          <Button variant="danger" onClick={handleDelete}>
            <Trash2 className="w-4 h-4 mr-1" /> Eliminar
          </Button>
        </div>
      </div>

      <div className="grid grid-cols-1 lg:grid-cols-3 gap-6">
        <div className="lg:col-span-2 space-y-6">
          <Card>
            <CardHeader className="py-3 px-6"><h3 className="text-sm font-bold text-foreground">Información del Proveedor</h3></CardHeader>
            <CardContent className="p-6">
              <div className="grid grid-cols-2 md:grid-cols-3 gap-4 text-sm">
                <div><span className="text-muted-foreground">Tipo:</span><p className="font-medium">{typeLabels[provider.providerType]}</p></div>
                <div><span className="text-muted-foreground">Tipo Documento:</span><p className="font-medium">{provider.documentType}</p></div>
                <div><span className="text-muted-foreground">Nro. Documento:</span><p className="font-medium">{provider.documentNumber}</p></div>
                <div><span className="text-muted-foreground">Razón Social:</span><p className="font-medium">{provider.businessName}</p></div>
                <div><span className="text-muted-foreground">Tipo Servicio:</span><p className="font-medium">{provider.serviceType || '—'}</p></div>
                <div><span className="text-muted-foreground">Creado:</span><p className="font-medium">{formatDate(provider.createdAt)}</p></div>
                {provider.updatedAt && <div><span className="text-muted-foreground">Actualizado:</span><p className="font-medium">{formatDate(provider.updatedAt)}</p></div>}
              </div>
            </CardContent>
          </Card>

          <Card>
            <CardHeader className="py-3 px-6"><h3 className="text-sm font-bold text-foreground">Contacto</h3></CardHeader>
            <CardContent className="p-6">
              <div className="grid grid-cols-1 md:grid-cols-2 gap-4 text-sm">
                {provider.contactName && (
                  <div className="flex items-center gap-2"><span className="text-muted-foreground">Contacto:</span><p className="font-medium">{provider.contactName}</p></div>
                )}
                {provider.email && (
                  <div className="flex items-center gap-2"><Mail className="w-4 h-4 text-muted-foreground" /><p className="font-medium">{provider.email}</p></div>
                )}
                {provider.phone && (
                  <div className="flex items-center gap-2"><Phone className="w-4 h-4 text-muted-foreground" /><p className="font-medium">{provider.phone}</p></div>
                )}
                {provider.address && (
                  <div className="flex items-center gap-2"><MapPin className="w-4 h-4 text-muted-foreground" /><p className="font-medium">{provider.address}</p></div>
                )}
              </div>
            </CardContent>
          </Card>

          <Card>
            <CardHeader className="py-3 px-6 flex items-center justify-between">
              <h3 className="text-sm font-bold text-foreground">Contratos</h3>
              <Button variant="secondary" onClick={() => router.push(`/contracts/new?providerId=${provider.id}`)}>
                <Plus className="w-4 h-4 mr-1" /> Nuevo Contrato
              </Button>
            </CardHeader>
            <CardContent className="p-0">
              {provider.contracts.length === 0 ? (
                <p className="px-6 py-8 text-center text-sm text-muted-foreground">No hay contratos registrados.</p>
              ) : (
                <div className="overflow-x-auto">
                  <table className="min-w-full divide-y divide-border">
                    <thead className="bg-muted/50">
                      <tr>
                        <th className="px-5 py-3 text-left text-xs font-bold text-muted-foreground uppercase">Nro.</th>
                        <th className="px-5 py-3 text-left text-xs font-bold text-muted-foreground uppercase">Tipo</th>
                        <th className="px-5 py-3 text-right text-xs font-bold text-muted-foreground uppercase">Valor</th>
                        <th className="px-5 py-3 text-left text-xs font-bold text-muted-foreground uppercase">Inicio</th>
                        <th className="px-5 py-3 text-left text-xs font-bold text-muted-foreground uppercase">Fin</th>
                        <th className="px-5 py-3 text-left text-xs font-bold text-muted-foreground uppercase">Estado</th>
                        <th className="px-5 py-3 text-right text-xs font-bold text-muted-foreground uppercase">Acciones</th>
                      </tr>
                    </thead>
                    <tbody className="divide-y divide-border">
                      {provider.contracts.map((c) => (
                        <tr key={c.id} className="hover:bg-muted/30 transition-colors">
                          <td className="px-5 py-3 font-mono font-bold text-sm">{c.contractNumber}</td>
                          <td className="px-5 py-3 text-sm text-muted-foreground">{contractTypeLabels[c.contractType] || c.contractType}</td>
                          <td className="px-5 py-3 text-sm text-right font-medium">{formatCurrency(c.totalValue)}</td>
                          <td className="px-5 py-3 text-sm text-muted-foreground">{formatDate(c.startDate)}</td>
                          <td className="px-5 py-3 text-sm text-muted-foreground">{formatDate(c.endDate)}</td>
                          <td className="px-5 py-3">
                            <span className={`badge-${c.status === 'Active' ? 'success' : c.status === 'Draft' ? 'info' : c.status === 'Expired' ? 'warning' : 'neutral'}`}>{c.status}</span>
                          </td>
                          <td className="px-5 py-3 text-right">
                            <button onClick={() => router.push(`/contracts/${c.id}`)} className="text-emerald-600 hover:text-emerald-800 text-sm font-semibold">Ver</button>
                          </td>
                        </tr>
                      ))}
                    </tbody>
                  </table>
                </div>
              )}
            </CardContent>
          </Card>

          {provider.invoices.length > 0 && (
            <Card>
              <CardHeader className="py-3 px-6"><h3 className="text-sm font-bold text-foreground">Facturas</h3></CardHeader>
              <CardContent className="p-0">
                <div className="overflow-x-auto">
                  <table className="min-w-full divide-y divide-border">
                    <thead className="bg-muted/50">
                      <tr>
                        <th className="px-5 py-3 text-left text-xs font-bold text-muted-foreground uppercase">Nro.</th>
                        <th className="px-5 py-3 text-left text-xs font-bold text-muted-foreground uppercase">Contrato</th>
                        <th className="px-5 py-3 text-right text-xs font-bold text-muted-foreground uppercase">Total</th>
                        <th className="px-5 py-3 text-right text-xs font-bold text-muted-foreground uppercase">Pagado</th>
                        <th className="px-5 py-3 text-right text-xs font-bold text-muted-foreground uppercase">Pendiente</th>
                        <th className="px-5 py-3 text-left text-xs font-bold text-muted-foreground uppercase">Vence</th>
                        <th className="px-5 py-3 text-left text-xs font-bold text-muted-foreground uppercase">Estado</th>
                      </tr>
                    </thead>
                    <tbody className="divide-y divide-border">
                      {provider.invoices.map((inv) => (
                        <tr key={inv.id} className="hover:bg-muted/30">
                          <td className="px-5 py-3 font-mono font-bold text-sm">{inv.invoiceNumber}</td>
                          <td className="px-5 py-3 text-sm text-muted-foreground">{inv.contractNumber || '—'}</td>
                          <td className="px-5 py-3 text-sm text-right font-medium">{formatCurrency(inv.totalAmount)}</td>
                          <td className="px-5 py-3 text-sm text-right">{formatCurrency(inv.amountPaid)}</td>
                          <td className="px-5 py-3 text-sm text-right font-bold text-orange-600">{formatCurrency(inv.pendingAmount)}</td>
                          <td className="px-5 py-3 text-sm text-muted-foreground">{formatDate(inv.dueDate)}</td>
                          <td className="px-5 py-3">
                            <span className={`badge-${inv.status === 'FullyPaid' ? 'success' : inv.status === 'PartiallyPaid' ? 'warning' : 'info'}`}>
                              {invoiceStatusLabels[inv.status] || inv.status}
                            </span>
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

        <div className="space-y-6">
          <Card>
            <CardHeader className="py-3 px-6 flex items-center justify-between">
              <h3 className="text-sm font-bold text-foreground">Evaluaciones</h3>
              <Button variant="secondary" onClick={() => setShowEvaluationForm(!showEvaluationForm)}>
                <Plus className="w-4 h-4 mr-1" /> Evaluar
              </Button>
            </CardHeader>
            <CardContent className="p-4">
              {showEvaluationForm && (
                <form onSubmit={handleSubmitEvaluation} className="space-y-3 mb-4 p-3 bg-muted/30 rounded-lg">
                  <div>
                    <label className="block text-xs font-bold text-muted-foreground uppercase tracking-widest mb-1.5">Periodo (ej: 2026-Q1)</label>
                    <input type="text" placeholder="2026-Q1" value={evaluationPeriod} onChange={(e) => setEvaluationPeriod(e.target.value)} required
                      className="w-full bg-transparent border-b border-emerald-600/30 focus:border-emerald-600 text-sm font-medium py-2 outline-none" />
                  </div>
                  <ScoreSelect label="Calidad del Servicio" value={qualityScore} onChange={setQualityScore} />
                  <ScoreSelect label="Cumplimiento" value={complianceScore} onChange={setComplianceScore} />
                  <ScoreSelect label="Precio" value={priceScore} onChange={setPriceScore} />
                  <ScoreSelect label="Atención" value={attentionScore} onChange={setAttentionScore} />
                  <div>
                    <label className="block text-xs font-bold text-muted-foreground uppercase tracking-widest mb-1.5">Comentarios</label>
                    <textarea value={evaluationComments} onChange={(e) => setEvaluationComments(e.target.value)} rows={3}
                      className="w-full bg-slate-50 dark:bg-slate-900 border border-border focus:border-emerald-600 rounded-md text-sm p-3 outline-none resize-none" />
                  </div>
                  <div className="flex gap-2">
                    <Button type="button" variant="ghost" onClick={() => setShowEvaluationForm(false)}>Cancelar</Button>
                    <Button type="submit" disabled={submittingEvaluation}>
                      {submittingEvaluation ? <Loader2 className="w-4 h-4 animate-spin mr-1" /> : <Send className="w-4 h-4 mr-1" />}
                      Guardar
                    </Button>
                  </div>
                </form>
              )}

              {provider.evaluations.length === 0 ? (
                <p className="text-center text-sm text-muted-foreground py-4">No hay evaluaciones.</p>
              ) : (
                <div className="space-y-3">
                  {provider.evaluations.map((ev) => (
                    <div key={ev.id} className="p-3 bg-muted/30 rounded-lg">
                      <div className="flex justify-between items-center">
                        <span className="text-xs font-bold text-muted-foreground">{ev.evaluationPeriod}</span>
                        <span className="text-lg font-bold text-emerald-600">{ev.averageScore.toFixed(1)}</span>
                      </div>
                      <div className="flex items-center gap-2 mt-1">
                        <span className={`badge-${ev.recommendation === 'Renew' ? 'success' : ev.recommendation === 'DoNotRenew' ? 'danger' : 'warning'}`}>
                          {recommendationLabels[ev.recommendation] || ev.recommendation}
                        </span>
                      </div>
                      <p className="text-xs text-muted-foreground mt-1">Por {ev.evaluatedByUserName} — {formatDate(ev.createdAt)}</p>
                    </div>
                  ))}
                </div>
              )}
            </CardContent>
          </Card>
        </div>
      </div>
    </div>
  );
}
