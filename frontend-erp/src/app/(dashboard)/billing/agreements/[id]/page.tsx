'use client';

import React, { useState, useEffect } from 'react';
import { useRouter, useParams } from 'next/navigation';
import { Loader2, ArrowLeft, AlertTriangle, CheckCircle, XCircle, Clock, Calendar, FileText, Handshake } from 'lucide-react';
import { Button } from '@/components/ui/Button';
import { Card, CardHeader, CardContent } from '@/components/ui/Card';
import feesPortfolioService, { PaymentAgreementDetail } from '@/lib/fees-portfolio-service';

export default function AgreementDetailPage() {
  const router = useRouter();
  const params = useParams();
  const id = params.id as string;
  const [agreement, setAgreement] = useState<PaymentAgreementDetail | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');

  useEffect(() => {
    fetchAgreement();
  }, [id]);

  const fetchAgreement = async () => {
    setLoading(true);
    setError('');
    try {
      const data = await feesPortfolioService.getAgreementDetail(id);
      setAgreement(data);
    } catch {
      setError('Error al cargar el acuerdo de pago.');
    } finally {
      setLoading(false);
    }
  };

  const statusBadge = (status: string) => {
    const map: Record<string, string> = {
      Active: 'badge-success',
      Defaulted: 'badge-danger',
      Completed: 'badge-info',
      Pending: 'badge-warning',
    };
    const labels: Record<string, string> = {
      Active: 'Activo',
      Defaulted: 'En Mora',
      Completed: 'Completado',
      Pending: 'Pendiente',
    };
    return <span className={map[status] || 'badge-neutral'}>{labels[status] || status}</span>;
  };

  const installmentStatusBadge = (status: string) => {
    const map: Record<string, string> = {
      Pending: 'badge-warning',
      Paid: 'badge-success',
      Overdue: 'badge-danger',
      Partial: 'badge-info',
    };
    const labels: Record<string, string> = {
      Pending: 'Pendiente',
      Paid: 'Pagado',
      Overdue: 'Vencido',
      Partial: 'Parcial',
    };
    return <span className={map[status] || 'badge-neutral'}>{labels[status] || status}</span>;
  };

  const formatCurrency = (val: number) =>
    new Intl.NumberFormat('es-CO', { style: 'currency', currency: 'COP', minimumFractionDigits: 0 }).format(val);

  if (loading) {
    return (
      <div className="flex justify-center py-20">
        <Loader2 className="w-8 h-8 animate-spin text-emerald-600" />
      </div>
    );
  }

  if (error) {
    return (
      <div className="space-y-6 max-w-4xl">
        <button onClick={() => router.push('/billing/agreements')} className="inline-flex items-center gap-2 text-sm text-muted-foreground hover:text-foreground transition-colors">
          <ArrowLeft className="w-4 h-4" />
          Volver a Acuerdos de Pago
        </button>
        <div className="p-4 bg-rose-50 border border-rose-200 rounded-xl text-rose-700 text-sm flex items-center gap-2">
          <AlertTriangle className="w-5 h-5 shrink-0" />
          {error}
        </div>
      </div>
    );
  }

  if (!agreement) return null;

  const totalPaid = agreement.installments
    .filter((i) => i.status === 'Paid' || i.status === 'Partial')
    .reduce((sum, i) => sum + i.paidAmount, 0);
  const paidCount = agreement.installments.filter((i) => i.status === 'Paid').length;
  const overdueCount = agreement.installments.filter((i) => i.status === 'Overdue').length;
  const pendingCount = agreement.installments.filter((i) => i.status === 'Pending').length;

  return (
    <div className="space-y-6 max-w-4xl">
      <button onClick={() => router.push('/billing/agreements')} className="inline-flex items-center gap-2 text-sm text-muted-foreground hover:text-foreground transition-colors">
        <ArrowLeft className="w-4 h-4" />
        Volver a Acuerdos de Pago
      </button>

      <div className="flex flex-col sm:flex-row justify-between items-start sm:items-center gap-4">
        <div>
          <h1 className="text-2xl font-bold text-foreground tracking-tight">Acuerdo de Pago</h1>
          <p className="text-sm text-muted-foreground mt-1">Unidad: <strong>{agreement.unitIdentifier}</strong></p>
        </div>
        {statusBadge(agreement.status)}
      </div>

      <div className="grid grid-cols-1 md:grid-cols-4 gap-4">
        <Card>
          <CardContent className="p-5">
            <p className="text-xs text-muted-foreground font-medium uppercase tracking-wider">Deuda Incluida</p>
            <p className="text-xl font-bold text-foreground mt-1">{formatCurrency(agreement.totalDebtIncluded)}</p>
          </CardContent>
        </Card>
        <Card>
          <CardContent className="p-5">
            <p className="text-xs text-muted-foreground font-medium uppercase tracking-wider">Valor Cuota</p>
            <p className="text-xl font-bold text-foreground mt-1">{formatCurrency(agreement.installmentAmount)}</p>
          </CardContent>
        </Card>
        <Card>
          <CardContent className="p-5">
            <p className="text-xs text-muted-foreground font-medium uppercase tracking-wider">% Condonación</p>
            <p className="text-xl font-bold text-foreground mt-1">{agreement.interestForgivenessPercentage}%</p>
          </CardContent>
        </Card>
        <Card>
          <CardContent className="p-5">
            <p className="text-xs text-muted-foreground font-medium uppercase tracking-wider">Total Pagado</p>
            <p className="text-xl font-bold text-emerald-600 mt-1">{formatCurrency(totalPaid)}</p>
          </CardContent>
        </Card>
      </div>

      <div className="grid grid-cols-1 md:grid-cols-3 gap-4">
        <Card>
          <CardContent className="p-5">
            <div className="flex items-center gap-3">
              <Calendar className="w-5 h-5 text-muted-foreground" />
              <div>
                <p className="text-xs text-muted-foreground font-medium">Fecha de Inicio</p>
                <p className="text-sm font-semibold text-foreground">{new Date(agreement.startedAt).toLocaleDateString('es-CO')}</p>
              </div>
            </div>
          </CardContent>
        </Card>
        {agreement.defaultedAt && (
          <Card>
            <CardContent className="p-5">
              <div className="flex items-center gap-3">
                <XCircle className="w-5 h-5 text-rose-500" />
                <div>
                  <p className="text-xs text-muted-foreground font-medium">Fecha de Mora</p>
                  <p className="text-sm font-semibold text-foreground">{new Date(agreement.defaultedAt).toLocaleDateString('es-CO')}</p>
                </div>
              </div>
            </CardContent>
          </Card>
        )}
      </div>

      <div className="flex gap-2 flex-wrap">
        <div className="flex items-center gap-1.5 px-3 py-1.5 bg-emerald-50 rounded-lg text-emerald-700 text-sm">
          <CheckCircle className="w-4 h-4" />
          <span>{paidCount} Pagadas</span>
        </div>
        <div className="flex items-center gap-1.5 px-3 py-1.5 bg-rose-50 rounded-lg text-rose-700 text-sm">
          <XCircle className="w-4 h-4" />
          <span>{overdueCount} Vencidas</span>
        </div>
        <div className="flex items-center gap-1.5 px-3 py-1.5 bg-amber-50 rounded-lg text-amber-700 text-sm">
          <Clock className="w-4 h-4" />
          <span>{pendingCount} Pendientes</span>
        </div>
      </div>

      {agreement.councilActNumber && (
        <div className="text-sm text-muted-foreground">
          Acta del Consejo: <strong className="text-foreground">{agreement.councilActNumber}</strong>
        </div>
      )}

      <Card>
        <CardHeader>
          <h3 className="font-bold text-foreground">Cuotas</h3>
        </CardHeader>
        <CardContent className="p-0">
          <div className="overflow-x-auto">
            <table className="min-w-full divide-y divide-border">
              <thead className="bg-muted/50">
                <tr>
                  <th className="px-6 py-4 text-left text-xs font-bold text-muted-foreground uppercase tracking-wider">No.</th>
                  <th className="px-6 py-4 text-left text-xs font-bold text-muted-foreground uppercase tracking-wider">Fecha Venc.</th>
                  <th className="px-6 py-4 text-right text-xs font-bold text-muted-foreground uppercase tracking-wider">Monto</th>
                  <th className="px-6 py-4 text-right text-xs font-bold text-muted-foreground uppercase tracking-wider">Pagado</th>
                  <th className="px-6 py-4 text-left text-xs font-bold text-muted-foreground uppercase tracking-wider">Estado</th>
                  <th className="px-6 py-4 text-left text-xs font-bold text-muted-foreground uppercase tracking-wider">Fecha Pago</th>
                </tr>
              </thead>
              <tbody className="divide-y divide-border">
                {agreement.installments.length === 0 ? (
                  <tr>
                    <td colSpan={6} className="px-6 py-12 text-center text-muted-foreground">
                      No hay cuotas registradas.
                    </td>
                  </tr>
                ) : (
                  agreement.installments.map((inst) => (
                    <tr key={inst.id} className="hover:bg-muted/30 transition-colors">
                      <td className="px-6 py-4 whitespace-nowrap font-semibold text-foreground">{inst.installmentNumber}</td>
                      <td className="px-6 py-4 whitespace-nowrap text-sm text-muted-foreground">{new Date(inst.dueDate).toLocaleDateString('es-CO')}</td>
                      <td className="px-6 py-4 whitespace-nowrap text-right font-mono text-sm">{formatCurrency(inst.amount)}</td>
                      <td className="px-6 py-4 whitespace-nowrap text-right font-mono text-sm">{formatCurrency(inst.paidAmount)}</td>
                      <td className="px-6 py-4 whitespace-nowrap">{installmentStatusBadge(inst.status)}</td>
                      <td className="px-6 py-4 whitespace-nowrap text-sm text-muted-foreground">
                        {inst.paidAt ? new Date(inst.paidAt).toLocaleDateString('es-CO') : '—'}
                      </td>
                    </tr>
                  ))
                )}
              </tbody>
            </table>
          </div>
        </CardContent>
      </Card>

      {agreement.digitalAcceptance && (
        <Card>
          <CardHeader>
            <h3 className="font-bold text-foreground">Aceptación Digital</h3>
          </CardHeader>
          <CardContent>
            <p className="text-sm text-muted-foreground whitespace-pre-wrap">{agreement.digitalAcceptance}</p>
          </CardContent>
        </Card>
      )}
    </div>
  );
}
