'use client';

import React, { useState, useEffect } from 'react';
import { useParams, useRouter } from 'next/navigation';
import { Loader2, DollarSign, Calendar, CheckCircle, XCircle, ArrowLeft, Play, Calculator, AlertTriangle } from 'lucide-react';
import { Button } from '@/components/ui/Button';
import { Card, CardHeader, CardContent } from '@/components/ui/Card';
import feesPortfolioService, { BillingPeriodDetail } from '@/lib/fees-portfolio-service';

export default function BillingPeriodDetailPage() {
  const params = useParams();
  const router = useRouter();
  const rawId = params?.id;
  const id = Array.isArray(rawId) ? rawId[0] : rawId ?? '';

  const [detail, setDetail] = useState<BillingPeriodDetail | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');
  const [processing, setProcessing] = useState(false);
  const [calculating, setCalculating] = useState(false);

  useEffect(() => {
    if (id) fetchDetail();
  }, [id]);

  const fetchDetail = async () => {
    setLoading(true);
    setError('');
    try {
      const data = await feesPortfolioService.getBillingPeriod(id);
      setDetail(data);
    } catch {
      setError('Error al cargar el detalle del período.');
    } finally {
      setLoading(false);
    }
  };

  const handleProcess = async () => {
    setProcessing(true);
    setError('');
    try {
      await feesPortfolioService.processBilling(id);
      fetchDetail();
    } catch (err: any) {
      setError(err?.response?.data || 'Error al procesar la liquidación.');
    } finally {
      setProcessing(false);
    }
  };

  const handleCalculateInterest = async () => {
    setCalculating(true);
    setError('');
    try {
      await feesPortfolioService.calculateLateInterest(id);
      fetchDetail();
    } catch (err: any) {
      setError(err?.response?.data || 'Error al calcular intereses.');
    } finally {
      setCalculating(false);
    }
  };

  const statusBadge = (status: string) => {
    const map: Record<string, string> = {
      Draft: 'badge-warning',
      Executed: 'badge-info',
      Processed: 'badge-success',
      Closed: 'badge-neutral',
      Paid: 'badge-success',
      Pending: 'badge-warning',
      Overdue: 'badge-danger',
    };
    const labels: Record<string, string> = {
      Draft: 'Borrador',
      Executed: 'Ejecutada',
      Processed: 'Procesada',
      Closed: 'Cerrada',
      Paid: 'Pagado',
      Pending: 'Pendiente',
      Overdue: 'Vencido',
    };
    return <span className={map[status] || 'badge-neutral'}>{labels[status] || status}</span>;
  };

  const formatCurrency = (val: number) =>
    new Intl.NumberFormat('es-CO', { style: 'currency', currency: 'COP', minimumFractionDigits: 0 }).format(val);

  if (loading) {
    return (
      <div className="flex justify-center py-12">
        <Loader2 className="w-6 h-6 animate-spin text-emerald-600" />
      </div>
    );
  }

  if (error && !detail) {
    return (
      <div className="space-y-6">
        <button onClick={() => router.push('/billing')} className="inline-flex items-center gap-2 text-sm text-muted-foreground hover:text-foreground transition-colors">
          <ArrowLeft className="w-4 h-4" />
          Volver a Liquidaciones
        </button>
        <div className="p-4 bg-rose-50 border border-rose-200 rounded-xl text-rose-700 text-sm flex items-center gap-2">
          <AlertTriangle className="w-5 h-5 shrink-0" />
          {error}
        </div>
      </div>
    );
  }

  if (!detail) return null;

  const canProcess = detail.status === 'Executed';
  const canCalculate = detail.status === 'Processed';

  return (
    <div className="space-y-6">
      <button onClick={() => router.push('/billing')} className="inline-flex items-center gap-2 text-sm text-muted-foreground hover:text-foreground transition-colors">
        <ArrowLeft className="w-4 h-4" />
        Volver a Liquidaciones
      </button>

      {error && (
        <div className="p-4 bg-rose-50 border border-rose-200 rounded-xl text-rose-700 text-sm flex items-center gap-2">
          <AlertTriangle className="w-5 h-5 shrink-0" />
          {error}
        </div>
      )}

      <div className="flex flex-col sm:flex-row justify-between items-start sm:items-center gap-4">
        <div>
          <h1 className="text-2xl font-bold text-foreground tracking-tight">Período {detail.period}</h1>
          <p className="text-sm text-muted-foreground mt-1">{statusBadge(detail.status)}</p>
        </div>
        <div className="flex gap-2">
          {canProcess && (
            <Button onClick={handleProcess} disabled={processing}>
              {processing ? <Loader2 className="w-4 h-4 animate-spin mr-2" /> : <Play className="w-4 h-4 mr-2" />}
              Procesar Liquidación
            </Button>
          )}
          {canCalculate && (
            <Button variant="secondary" onClick={handleCalculateInterest} disabled={calculating}>
              {calculating ? <Loader2 className="w-4 h-4 animate-spin mr-2" /> : <Calculator className="w-4 h-4 mr-2" />}
              Calcular Intereses
            </Button>
          )}
        </div>
      </div>

      <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-4">
        <Card>
          <CardContent className="p-5">
            <div className="flex items-center gap-3">
              <div className="w-10 h-10 bg-emerald-50 rounded-xl flex items-center justify-center">
                <DollarSign className="w-5 h-5 text-emerald-600" />
              </div>
              <div>
                <p className="text-xs text-muted-foreground font-medium uppercase tracking-wider">Total Presupuesto</p>
                <p className="text-lg font-bold text-foreground">{formatCurrency(detail.monthlyBudgetTotal)}</p>
              </div>
            </div>
          </CardContent>
        </Card>
        <Card>
          <CardContent className="p-5">
            <div className="flex items-center gap-3">
              <div className="w-10 h-10 bg-blue-50 rounded-xl flex items-center justify-center">
                <Calendar className="w-5 h-5 text-blue-600" />
              </div>
              <div>
                <p className="text-xs text-muted-foreground font-medium uppercase tracking-wider">Fecha Corte</p>
                <p className="text-lg font-bold text-foreground">{new Date(detail.cutoffDate).toLocaleDateString('es-CO')}</p>
              </div>
            </div>
          </CardContent>
        </Card>
        <Card>
          <CardContent className="p-5">
            <div className="flex items-center gap-3">
              <div className="w-10 h-10 bg-amber-50 rounded-xl flex items-center justify-center">
                <Calendar className="w-5 h-5 text-amber-600" />
              </div>
              <div>
                <p className="text-xs text-muted-foreground font-medium uppercase tracking-wider">Vencimiento</p>
                <p className="text-lg font-bold text-foreground">{new Date(detail.paymentDueDate).toLocaleDateString('es-CO')}</p>
              </div>
            </div>
          </CardContent>
        </Card>
        <Card>
          <CardContent className="p-5">
            <div className="flex items-center gap-3">
              <div className="w-10 h-10 bg-purple-50 rounded-xl flex items-center justify-center">
                <DollarSign className="w-5 h-5 text-purple-600" />
              </div>
              <div>
                <p className="text-xs text-muted-foreground font-medium uppercase tracking-wider">Ajuste Redondeo</p>
                <p className="text-lg font-bold text-foreground">{formatCurrency(detail.roundingAdjustment)}</p>
              </div>
            </div>
          </CardContent>
        </Card>
      </div>

      <Card>
        <CardHeader>
          <h3 className="font-bold text-foreground">Cuotas por Unidad</h3>
          <p className="text-xs text-muted-foreground mt-0.5">{detail.unitFees.length} unidad{detail.unitFees.length !== 1 ? 'es' : ''}</p>
        </CardHeader>
        <CardContent className="p-0">
          <div className="overflow-x-auto">
            <table className="min-w-full divide-y divide-border">
              <thead className="bg-muted/50">
                <tr>
                  <th className="px-6 py-4 text-left text-xs font-bold text-muted-foreground uppercase tracking-wider">Unidad</th>
                  <th className="px-6 py-4 text-right text-xs font-bold text-muted-foreground uppercase tracking-wider">Coeficiente</th>
                  <th className="px-6 py-4 text-right text-xs font-bold text-muted-foreground uppercase tracking-wider">Valor Cuota</th>
                  <th className="px-6 py-4 text-left text-xs font-bold text-muted-foreground uppercase tracking-wider">Fecha Venc.</th>
                  <th className="px-6 py-4 text-left text-xs font-bold text-muted-foreground uppercase tracking-wider">Estado</th>
                  <th className="px-6 py-4 text-right text-xs font-bold text-muted-foreground uppercase tracking-wider">Pagado</th>
                  <th className="px-6 py-4 text-right text-xs font-bold text-muted-foreground uppercase tracking-wider">Saldo</th>
                </tr>
              </thead>
              <tbody className="divide-y divide-border">
                {detail.unitFees.length === 0 ? (
                  <tr>
                    <td colSpan={7} className="px-6 py-12 text-center text-muted-foreground">
                      No hay cuotas registradas para este período.
                    </td>
                  </tr>
                ) : (
                  detail.unitFees.map((uf) => (
                    <tr key={uf.id} className="hover:bg-muted/30 transition-colors">
                      <td className="px-6 py-4 whitespace-nowrap">
                        <span className="font-semibold text-foreground">{uf.unitIdentifier}</span>
                        {uf.unitTower && <span className="text-xs text-muted-foreground ml-1">({uf.unitTower})</span>}
                      </td>
                      <td className="px-6 py-4 whitespace-nowrap text-right font-mono text-sm">{uf.coefficient.toFixed(4)}</td>
                      <td className="px-6 py-4 whitespace-nowrap text-right font-mono text-sm font-semibold">{formatCurrency(uf.feeValue)}</td>
                      <td className="px-6 py-4 whitespace-nowrap text-sm text-muted-foreground">{new Date(uf.dueDate).toLocaleDateString('es-CO')}</td>
                      <td className="px-6 py-4 whitespace-nowrap">{statusBadge(uf.status)}</td>
                      <td className="px-6 py-4 whitespace-nowrap text-right font-mono text-sm">{formatCurrency(uf.paidAmount)}</td>
                      <td className="px-6 py-4 whitespace-nowrap text-right font-mono text-sm font-bold text-rose-600">{formatCurrency(uf.balanceAmount)}</td>
                    </tr>
                  ))
                )}
              </tbody>
            </table>
          </div>
        </CardContent>
      </Card>
    </div>
  );
}
