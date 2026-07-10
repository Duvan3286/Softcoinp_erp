'use client';

import React, { useState, useEffect } from 'react';
import { useRouter } from 'next/navigation';
import { Loader2, ArrowLeft, AlertTriangle } from 'lucide-react';
import { Card, CardContent, CardHeader } from '@/components/ui/Card';
import supplierService, { PendingPaymentItem } from '@/lib/supplier-service';

export default function PendingPaymentsPage() {
  const router = useRouter();
  const [payments, setPayments] = useState<PendingPaymentItem[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');

  useEffect(() => {
    const fetchPayments = async () => {
      try {
        const data = await supplierService.getPendingPayments();
        setPayments(data);
      } catch {
        setError('Error al cargar los pagos pendientes.');
      } finally {
        setLoading(false);
      }
    };
    fetchPayments();
  }, []);

  const formatCurrency = (value: number) => new Intl.NumberFormat('es-CO', { style: 'currency', currency: 'COP', maximumFractionDigits: 0 }).format(value);
  const formatDate = (value: string) => new Date(value).toLocaleDateString('es-CO', { day: '2-digit', month: '2-digit', year: 'numeric' });

  const dueDaysLabel = (payment: PendingPaymentItem) => {
    if (payment.daysOverdue > 0) {
      return `Vencida hace ${payment.daysOverdue} días`;
    }
    const remainingDays = Math.ceil((new Date(payment.dueDate).getTime() - Date.now()) / (1000 * 60 * 60 * 24));
    return `Faltan ${remainingDays} días`;
  };

  const dueDaysColor = (payment: PendingPaymentItem) => {
    if (payment.daysOverdue > 0) {
      return 'text-rose-600';
    }
    return 'text-muted-foreground';
  };

  if (loading) {
    return <div className="flex justify-center py-20"><Loader2 className="w-8 h-8 animate-spin text-emerald-600" /></div>;
  }

  return (
    <div className="space-y-6">
      <button onClick={() => router.push('/contracts')} className="inline-flex items-center gap-2 text-sm text-muted-foreground hover:text-foreground transition-colors">
        <ArrowLeft className="w-4 h-4" /> Volver a Contratos
      </button>

      <div>
        <h1 className="text-2xl font-bold text-foreground tracking-tight">Pagos Pendientes</h1>
        <p className="text-sm text-muted-foreground mt-1">Facturas de proveedores sin pagar por completo, ordenadas por fecha de vencimiento.</p>
      </div>

      {error && (
        <div className="p-3 bg-rose-50 border border-rose-200 rounded-lg text-rose-700 text-sm flex items-center gap-2">
          <AlertTriangle className="w-4 h-4 shrink-0" /> {error}
        </div>
      )}

      <Card>
        <CardHeader className="py-3 px-6"><h3 className="text-sm font-bold text-foreground">Facturas Pendientes ({payments.length})</h3></CardHeader>
        <CardContent className="p-0">
          <div className="overflow-x-auto">
            <table className="min-w-full divide-y divide-border">
              <thead className="bg-muted/50">
                <tr>
                  <th className="px-5 py-3 text-left text-xs font-bold text-muted-foreground uppercase">Nro. Factura</th>
                  <th className="px-5 py-3 text-left text-xs font-bold text-muted-foreground uppercase">Proveedor</th>
                  <th className="px-5 py-3 text-left text-xs font-bold text-muted-foreground uppercase">Contrato</th>
                  <th className="px-5 py-3 text-right text-xs font-bold text-muted-foreground uppercase">Pendiente</th>
                  <th className="px-5 py-3 text-left text-xs font-bold text-muted-foreground uppercase">Vencimiento</th>
                  <th className="px-5 py-3 text-left text-xs font-bold text-muted-foreground uppercase">Días</th>
                </tr>
              </thead>
              <tbody className="divide-y divide-border">
                {payments.map((payment) => (
                  <tr key={payment.invoiceId} className="hover:bg-muted/30">
                    <td className="px-5 py-3 font-mono font-bold text-sm">{payment.invoiceNumber}</td>
                    <td className="px-5 py-3 text-sm">{payment.providerName}</td>
                    <td className="px-5 py-3 font-mono text-sm text-muted-foreground">{payment.contractNumber || '—'}</td>
                    <td className="px-5 py-3 text-sm text-right font-bold text-orange-600">{formatCurrency(payment.pendingAmount)}</td>
                    <td className="px-5 py-3 text-sm text-muted-foreground">{formatDate(payment.dueDate)}</td>
                    <td className={`px-5 py-3 text-sm font-bold ${dueDaysColor(payment)}`}>{dueDaysLabel(payment)}</td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        </CardContent>
      </Card>
    </div>
  );
}
