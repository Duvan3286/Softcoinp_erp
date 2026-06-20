'use client';

import React, { useState, useEffect } from 'react';
import { useRouter } from 'next/navigation';
import { Loader2, DollarSign, Calendar, CheckCircle, XCircle, Plus, Search, FileText, Download, Eye, X, AlertTriangle, CreditCard, BarChart3 } from 'lucide-react';
import { Button } from '@/components/ui/Button';
import { Card, CardHeader, CardContent } from '@/components/ui/Card';
import feesPortfolioService, { BillingPeriodSummary, PortfolioSummary } from '@/lib/fees-portfolio-service';

type Tab = 'periods' | 'summary';

export default function BillingPage() {
  const router = useRouter();
  const [activeTab, setActiveTab] = useState<Tab>('periods');
  const [periods, setPeriods] = useState<BillingPeriodSummary[]>([]);
  const [portfolio, setPortfolio] = useState<PortfolioSummary | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');
  const [showModal, setShowModal] = useState(false);
  const [submitting, setSubmitting] = useState(false);
  const [period, setPeriod] = useState('');
  const [cutoffDate, setCutoffDate] = useState('');
  const [paymentDueDate, setPaymentDueDate] = useState('');

  useEffect(() => {
    if (activeTab === 'periods') fetchPeriods();
    else fetchPortfolio();
  }, [activeTab]);

  const fetchPeriods = async () => {
    setLoading(true);
    setError('');
    try {
      const data = await feesPortfolioService.getBillingPeriods();
      setPeriods(data);
    } catch {
      setError('Error al cargar los períodos de liquidación.');
    } finally {
      setLoading(false);
    }
  };

  const fetchPortfolio = async () => {
    setLoading(true);
    setError('');
    try {
      const data = await feesPortfolioService.getPortfolioSummary();
      setPortfolio(data);
    } catch {
      setError('Error al cargar el resumen de cartera.');
    } finally {
      setLoading(false);
    }
  };

  const handleOpenModal = () => {
    setPeriod(new Date().toISOString().slice(0, 7));
    setCutoffDate('');
    setPaymentDueDate('');
    setError('');
    setShowModal(true);
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setError('');
    setSubmitting(true);
    try {
      await feesPortfolioService.executeBilling({ period, cutoffDate, paymentDueDate });
      setShowModal(false);
      fetchPeriods();
    } catch (err: any) {
      setError(err?.response?.data || 'Error al ejecutar la liquidación.');
    } finally {
      setSubmitting(false);
    }
  };

  const statusBadge = (status: string) => {
    const map: Record<string, string> = {
      Draft: 'badge-warning',
      Executed: 'badge-info',
      Processed: 'badge-success',
      Closed: 'badge-neutral',
    };
    const labels: Record<string, string> = {
      Draft: 'Borrador',
      Executed: 'Ejecutada',
      Processed: 'Procesada',
      Closed: 'Cerrada',
    };
    return <span className={map[status] || 'badge-neutral'}>{labels[status] || status}</span>;
  };

  const formatCurrency = (val: number) =>
    new Intl.NumberFormat('es-CO', { style: 'currency', currency: 'COP', minimumFractionDigits: 0 }).format(val);

  return (
    <div className="space-y-6">
      <div className="flex flex-col sm:flex-row justify-between items-start sm:items-center gap-4">
        <div>
          <h1 className="text-2xl font-bold text-foreground tracking-tight">Liquidación de Cuotas</h1>
          <p className="text-sm text-muted-foreground mt-1">Gestiona los períodos de liquidación y la cartera del conjunto.</p>
        </div>
        {activeTab === 'periods' && (
          <Button onClick={handleOpenModal}>
            <Plus className="w-4 h-4 mr-2" />
            Nueva Liquidación
          </Button>
        )}
      </div>

      <div className="flex gap-1 bg-muted p-1 rounded-lg w-fit">
        <button onClick={() => setActiveTab('periods')} className={`px-4 py-2 text-sm font-semibold rounded-md transition-all ${activeTab === 'periods' ? 'bg-card text-foreground shadow-sm' : 'text-muted-foreground hover:text-foreground'}`}>
          <Calendar className="w-4 h-4 inline mr-2" />
          Períodos de Liquidación
        </button>
        <button onClick={() => setActiveTab('summary')} className={`px-4 py-2 text-sm font-semibold rounded-md transition-all ${activeTab === 'summary' ? 'bg-card text-foreground shadow-sm' : 'text-muted-foreground hover:text-foreground'}`}>
          <BarChart3 className="w-4 h-4 inline mr-2" />
          Resumen
        </button>
      </div>

      {activeTab === 'periods' && (
        <>
          <Card>
            <CardContent className="p-0">
              <div className="overflow-x-auto">
                <table className="min-w-full divide-y divide-border">
                  <thead className="bg-muted/50">
                    <tr>
                      <th className="px-6 py-4 text-left text-xs font-bold text-muted-foreground uppercase tracking-wider">Periodo</th>
                      <th className="px-6 py-4 text-right text-xs font-bold text-muted-foreground uppercase tracking-wider">Total Facturado</th>
                      <th className="px-6 py-4 text-left text-xs font-bold text-muted-foreground uppercase tracking-wider">Fecha Corte</th>
                      <th className="px-6 py-4 text-left text-xs font-bold text-muted-foreground uppercase tracking-wider">Estado</th>
                      <th className="px-6 py-4 text-right text-xs font-bold text-muted-foreground uppercase tracking-wider">Acciones</th>
                    </tr>
                  </thead>
                  <tbody className="divide-y divide-border">
                    {loading ? (
                      <tr>
                        <td colSpan={5} className="px-6 py-12 text-center">
                          <Loader2 className="w-6 h-6 animate-spin mx-auto text-emerald-600" />
                        </td>
                      </tr>
                    ) : periods.length === 0 ? (
                      <tr>
                        <td colSpan={5} className="px-6 py-12 text-center text-muted-foreground">
                          <Calendar className="w-12 h-12 mx-auto text-muted-foreground/40 mb-3" />
                          <p className="font-semibold">No hay períodos de liquidación</p>
                          <p className="text-sm mt-1">Crea una nueva liquidación para comenzar.</p>
                        </td>
                      </tr>
                    ) : (
                      periods.map((p) => (
                        <tr key={p.id} className="hover:bg-muted/30 transition-colors">
                          <td className="px-6 py-4 whitespace-nowrap font-semibold text-foreground">{p.period}</td>
                          <td className="px-6 py-4 whitespace-nowrap text-right font-mono text-sm">{formatCurrency(p.totalBilled)}</td>
                          <td className="px-6 py-4 whitespace-nowrap text-sm text-muted-foreground">{new Date(p.cutoffDate).toLocaleDateString('es-CO')}</td>
                          <td className="px-6 py-4 whitespace-nowrap">{statusBadge(p.status)}</td>
                          <td className="px-6 py-4 whitespace-nowrap text-right">
                            <button onClick={() => router.push(`/billing/periods/${p.id}`)} className="text-emerald-600 hover:text-emerald-800 text-sm font-semibold px-3 py-1.5 bg-emerald-50 rounded-lg hover:bg-emerald-100 transition-colors">
                              <Eye className="w-4 h-4 inline mr-1" />
                              Ver
                            </button>
                          </td>
                        </tr>
                      ))
                    )}
                  </tbody>
                </table>
              </div>
            </CardContent>
          </Card>

          {!loading && periods.length > 0 && (
            <p className="text-xs text-muted-foreground px-1">{periods.length} período{periods.length !== 1 ? 's' : ''} encontrado{periods.length !== 1 ? 's' : ''}</p>
          )}
        </>
      )}

      {activeTab === 'summary' && (
        <>
          {loading ? (
            <div className="flex justify-center py-12">
              <Loader2 className="w-6 h-6 animate-spin text-emerald-600" />
            </div>
          ) : error ? (
            <div className="p-4 bg-rose-50 border border-rose-200 rounded-xl text-rose-700 text-sm flex items-center gap-2">
              <AlertTriangle className="w-5 h-5 shrink-0" />
              {error}
            </div>
          ) : portfolio ? (
            <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-4">
              <Card>
                <CardContent className="p-5">
                  <div className="flex items-center gap-3">
                    <div className="w-10 h-10 bg-emerald-50 rounded-xl flex items-center justify-center">
                      <DollarSign className="w-5 h-5 text-emerald-600" />
                    </div>
                    <div>
                      <p className="text-sm text-muted-foreground font-medium">Total Facturado</p>
                      <p className="text-xl font-bold text-foreground">{formatCurrency(portfolio.totalBilled)}</p>
                    </div>
                  </div>
                </CardContent>
              </Card>
              <Card>
                <CardContent className="p-5">
                  <div className="flex items-center gap-3">
                    <div className="w-10 h-10 bg-green-50 rounded-xl flex items-center justify-center">
                      <CheckCircle className="w-5 h-5 text-green-600" />
                    </div>
                    <div>
                      <p className="text-sm text-muted-foreground font-medium">Recaudado</p>
                      <p className="text-xl font-bold text-foreground">{formatCurrency(portfolio.totalCollected)}</p>
                    </div>
                  </div>
                </CardContent>
              </Card>
              <Card>
                <CardContent className="p-5">
                  <div className="flex items-center gap-3">
                    <div className="w-10 h-10 bg-rose-50 rounded-xl flex items-center justify-center">
                      <XCircle className="w-5 h-5 text-rose-600" />
                    </div>
                    <div>
                      <p className="text-sm text-muted-foreground font-medium">Pendiente</p>
                      <p className="text-xl font-bold text-foreground">{formatCurrency(portfolio.totalOutstanding)}</p>
                    </div>
                  </div>
                </CardContent>
              </Card>
              <Card>
                <CardContent className="p-5">
                  <div className="flex items-center gap-3">
                    <div className="w-10 h-10 bg-blue-50 rounded-xl flex items-center justify-center">
                      <BarChart3 className="w-5 h-5 text-blue-600" />
                    </div>
                    <div>
                      <p className="text-sm text-muted-foreground font-medium">Recaudo</p>
                      <p className="text-xl font-bold text-foreground">{portfolio.collectionRate.toFixed(1)}%</p>
                    </div>
                  </div>
                </CardContent>
              </Card>
            </div>
          ) : null}
        </>
      )}

      {showModal && (
        <div className="fixed inset-0 bg-black/60 backdrop-blur-sm z-[150] flex items-center justify-center p-4">
          <div className="bg-card text-card-foreground w-full max-w-md rounded-xl border border-border shadow-lg animate-in zoom-in-95 duration-200">
            <div className="p-6 border-b border-border flex items-center justify-between">
              <h3 className="font-bold text-lg text-foreground">Nueva Liquidación</h3>
              <button onClick={() => setShowModal(false)} className="text-muted-foreground hover:text-foreground">
                <X className="w-5 h-5" />
              </button>
            </div>
            <form onSubmit={handleSubmit} className="p-6 space-y-5">
              <div>
                <label className="block text-xs font-bold text-muted-foreground uppercase tracking-widest mb-1.5">Período</label>
                <input
                  type="month"
                  value={period}
                  onChange={(e) => setPeriod(e.target.value)}
                  className="w-full bg-transparent border-b border-emerald-600 focus:border-b-2 text-foreground px-0 py-2 text-sm focus:outline-none transition-all"
                  required
                />
              </div>
              <div>
                <label className="block text-xs font-bold text-muted-foreground uppercase tracking-widest mb-1.5">Fecha de Corte</label>
                <input
                  type="date"
                  value={cutoffDate}
                  onChange={(e) => setCutoffDate(e.target.value)}
                  className="w-full bg-transparent border-b border-emerald-600 focus:border-b-2 text-foreground px-0 py-2 text-sm focus:outline-none transition-all"
                  required
                />
              </div>
              <div>
                <label className="block text-xs font-bold text-muted-foreground uppercase tracking-widest mb-1.5">Fecha de Vencimiento</label>
                <input
                  type="date"
                  value={paymentDueDate}
                  onChange={(e) => setPaymentDueDate(e.target.value)}
                  className="w-full bg-transparent border-b border-emerald-600 focus:border-b-2 text-foreground px-0 py-2 text-sm focus:outline-none transition-all"
                  required
                />
              </div>
              {error && (
                <div className="p-3 bg-rose-50 border border-rose-200 rounded-lg text-rose-700 text-xs flex items-center gap-2">
                  <AlertTriangle className="w-4 h-4 shrink-0" />
                  {error}
                </div>
              )}
              <div className="flex justify-end gap-3 pt-2">
                <Button type="button" variant="ghost" onClick={() => setShowModal(false)}>Cancelar</Button>
                <Button type="submit" disabled={submitting}>
                  {submitting ? <Loader2 className="w-4 h-4 animate-spin mr-2" /> : <Plus className="w-4 h-4 mr-2" />}
                  Ejecutar Liquidación
                </Button>
              </div>
            </form>
          </div>
        </div>
      )}
    </div>
  );
}
