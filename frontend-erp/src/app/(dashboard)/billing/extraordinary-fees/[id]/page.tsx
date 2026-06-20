'use client';

import React, { useState, useEffect } from 'react';
import { useParams, useRouter } from 'next/navigation';
import { Loader2, DollarSign, Calendar, CheckCircle, XCircle, ArrowLeft, AlertTriangle } from 'lucide-react';
import { Button } from '@/components/ui/Button';
import { Card, CardHeader, CardContent } from '@/components/ui/Card';
import feesPortfolioService, { ExtraordinaryFeeDetail } from '@/lib/fees-portfolio-service';

export default function ExtraordinaryFeeDetailPage() {
  const params = useParams();
  const router = useRouter();
  const rawId = params?.id;
  const id = Array.isArray(rawId) ? rawId[0] : rawId ?? '';

  const [detail, setDetail] = useState<ExtraordinaryFeeDetail | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');
  const [updating, setUpdating] = useState(false);

  useEffect(() => {
    if (id) fetchDetail();
  }, [id]);

  const fetchDetail = async () => {
    setLoading(true);
    setError('');
    try {
      const data = await feesPortfolioService.getExtraordinaryFeeDetail(id);
      setDetail(data);
    } catch {
      setError('Error al cargar el detalle de la cuota extraordinaria.');
    } finally {
      setLoading(false);
    }
  };

  const handleStatusChange = async (newStatus: string) => {
    setUpdating(true);
    setError('');
    try {
      await feesPortfolioService.updateExtraordinaryFeeStatus(id, newStatus);
      fetchDetail();
    } catch (err: any) {
      setError(err?.response?.data || 'Error al actualizar el estado.');
    } finally {
      setUpdating(false);
    }
  };

  const statusBadge = (status: string) => {
    const map: Record<string, string> = {
      Active: 'badge-success',
      Pending: 'badge-warning',
      Completed: 'badge-info',
      Cancelled: 'badge-danger',
      Paid: 'badge-success',
      Overdue: 'badge-danger',
    };
    const labels: Record<string, string> = {
      Active: 'Activa',
      Pending: 'Pendiente',
      Completed: 'Completada',
      Cancelled: 'Cancelada',
      Paid: 'Pagado',
      Overdue: 'Vencido',
    };
    return <span className={map[status] || 'badge-neutral'}>{labels[status] || status}</span>;
  };

  const formatCurrency = (val: number) =>
    new Intl.NumberFormat('es-CO', { style: 'currency', currency: 'COP', minimumFractionDigits: 0 }).format(val);

  const distLabels: Record<string, string> = {
    Equal: 'Igualitaria',
    ByCoefficient: 'Por Coeficiente',
    Custom: 'Personalizada',
  };

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
        <button onClick={() => router.push('/billing/extraordinary-fees')} className="inline-flex items-center gap-2 text-sm text-muted-foreground hover:text-foreground transition-colors">
          <ArrowLeft className="w-4 h-4" />
          Volver a Cuotas Extraordinarias
        </button>
        <div className="p-4 bg-rose-50 border border-rose-200 rounded-xl text-rose-700 text-sm flex items-center gap-2">
          <AlertTriangle className="w-5 h-5 shrink-0" />
          {error}
        </div>
      </div>
    );
  }

  if (!detail) return null;

  const statusOptions = [
    { value: 'Pending', label: 'Pendiente' },
    { value: 'Active', label: 'Activa' },
    { value: 'Completed', label: 'Completada' },
    { value: 'Cancelled', label: 'Cancelada' },
  ];

  return (
    <div className="space-y-6">
      <button onClick={() => router.push('/billing/extraordinary-fees')} className="inline-flex items-center gap-2 text-sm text-muted-foreground hover:text-foreground transition-colors">
        <ArrowLeft className="w-4 h-4" />
        Volver a Cuotas Extraordinarias
      </button>

      {error && (
        <div className="p-4 bg-rose-50 border border-rose-200 rounded-xl text-rose-700 text-sm flex items-center gap-2">
          <AlertTriangle className="w-5 h-5 shrink-0" />
          {error}
        </div>
      )}

      <div className="flex flex-col sm:flex-row justify-between items-start sm:items-center gap-4">
        <div>
          <h1 className="text-2xl font-bold text-foreground tracking-tight">{detail.name}</h1>
          <p className="text-sm text-muted-foreground mt-1">{distLabels[detail.distributionType] || detail.distributionType}</p>
        </div>
        <div className="flex items-center gap-2">
          <select
            value={detail.status}
            onChange={(e) => handleStatusChange(e.target.value)}
            disabled={updating}
            className="w-full bg-transparent border-b border-emerald-600 focus:border-b-2 text-foreground px-0 py-2 text-sm focus:outline-none transition-all min-w-[140px]"
          >
            {statusOptions.map((opt) => (
              <option key={opt.value} value={opt.value}>{opt.label}</option>
            ))}
          </select>
          {updating && <Loader2 className="w-4 h-4 animate-spin text-emerald-600" />}
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
                <p className="text-xs text-muted-foreground font-medium uppercase tracking-wider">Monto Total</p>
                <p className="text-lg font-bold text-foreground">{formatCurrency(detail.totalAmount)}</p>
              </div>
            </div>
          </CardContent>
        </Card>
        <Card>
          <CardContent className="p-5">
            <div className="flex items-center gap-3">
              <div className="w-10 h-10 bg-blue-50 rounded-xl flex items-center justify-center">
                <DollarSign className="w-5 h-5 text-blue-600" />
              </div>
              <div>
                <p className="text-xs text-muted-foreground font-medium uppercase tracking-wider">Valor por Unidad</p>
                <p className="text-lg font-bold text-foreground">{formatCurrency(detail.amountPerUnit)}</p>
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
                <p className="text-lg font-bold text-foreground">{new Date(detail.dueDate).toLocaleDateString('es-CO')}</p>
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
                <p className="text-xs text-muted-foreground font-medium uppercase tracking-wider">Cuotas</p>
                <p className="text-lg font-bold text-foreground">{detail.numberOfInstallments}</p>
              </div>
            </div>
          </CardContent>
        </Card>
      </div>

      {detail.notes && (
        <Card>
          <CardContent className="p-5">
            <p className="text-xs text-muted-foreground uppercase tracking-wider font-bold mb-2">Notas</p>
            <p className="text-sm text-foreground">{detail.notes}</p>
          </CardContent>
        </Card>
      )}

      <Card>
        <CardHeader>
          <h3 className="font-bold text-foreground">Distribución por Unidad</h3>
          <p className="text-xs text-muted-foreground mt-0.5">{detail.distributions.length} unidad{detail.distributions.length !== 1 ? 'es' : ''}</p>
        </CardHeader>
        <CardContent className="p-0">
          <div className="overflow-x-auto">
            <table className="min-w-full divide-y divide-border">
              <thead className="bg-muted/50">
                <tr>
                  <th className="px-6 py-4 text-left text-xs font-bold text-muted-foreground uppercase tracking-wider">Unidad</th>
                  <th className="px-6 py-4 text-right text-xs font-bold text-muted-foreground uppercase tracking-wider">Monto</th>
                  <th className="px-6 py-4 text-right text-xs font-bold text-muted-foreground uppercase tracking-wider">Cuota</th>
                  <th className="px-6 py-4 text-left text-xs font-bold text-muted-foreground uppercase tracking-wider">Fecha Venc.</th>
                  <th className="px-6 py-4 text-left text-xs font-bold text-muted-foreground uppercase tracking-wider">Estado</th>
                  <th className="px-6 py-4 text-right text-xs font-bold text-muted-foreground uppercase tracking-wider">Pagado</th>
                  <th className="px-6 py-4 text-right text-xs font-bold text-muted-foreground uppercase tracking-wider">Saldo</th>
                </tr>
              </thead>
              <tbody className="divide-y divide-border">
                {detail.distributions.length === 0 ? (
                  <tr>
                    <td colSpan={7} className="px-6 py-12 text-center text-muted-foreground">
                      No hay distribuciones registradas.
                    </td>
                  </tr>
                ) : (
                  detail.distributions.map((d) => (
                    <tr key={d.id} className="hover:bg-muted/30 transition-colors">
                      <td className="px-6 py-4 whitespace-nowrap font-semibold text-foreground">{d.unitIdentifier}</td>
                      <td className="px-6 py-4 whitespace-nowrap text-right font-mono text-sm">{formatCurrency(d.amount)}</td>
                      <td className="px-6 py-4 whitespace-nowrap text-right text-sm text-muted-foreground">{d.installmentNumber}</td>
                      <td className="px-6 py-4 whitespace-nowrap text-sm text-muted-foreground">{new Date(d.dueDate).toLocaleDateString('es-CO')}</td>
                      <td className="px-6 py-4 whitespace-nowrap">{statusBadge(d.status)}</td>
                      <td className="px-6 py-4 whitespace-nowrap text-right font-mono text-sm text-emerald-600">{formatCurrency(d.paidAmount)}</td>
                      <td className="px-6 py-4 whitespace-nowrap text-right font-mono text-sm font-bold text-rose-600">{formatCurrency(d.balanceAmount)}</td>
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
