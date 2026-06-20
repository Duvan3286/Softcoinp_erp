'use client';

import React, { useState, useEffect } from 'react';
import { useRouter } from 'next/navigation';
import { Loader2, DollarSign, Calendar, Plus, Eye, AlertTriangle } from 'lucide-react';
import { Button } from '@/components/ui/Button';
import { Card, CardHeader, CardContent } from '@/components/ui/Card';
import feesPortfolioService, { ExtraordinaryFeeDto } from '@/lib/fees-portfolio-service';

export default function ExtraordinaryFeesPage() {
  const router = useRouter();
  const [fees, setFees] = useState<ExtraordinaryFeeDto[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');

  useEffect(() => {
    fetchFees();
  }, []);

  const fetchFees = async () => {
    setLoading(true);
    setError('');
    try {
      const data = await feesPortfolioService.getExtraordinaryFees();
      setFees(data);
    } catch {
      setError('Error al cargar las cuotas extraordinarias.');
    } finally {
      setLoading(false);
    }
  };

  const statusBadge = (status: string) => {
    const map: Record<string, string> = {
      Active: 'badge-success',
      Pending: 'badge-warning',
      Completed: 'badge-info',
      Cancelled: 'badge-danger',
    };
    const labels: Record<string, string> = {
      Active: 'Activa',
      Pending: 'Pendiente',
      Completed: 'Completada',
      Cancelled: 'Cancelada',
    };
    return <span className={map[status] || 'badge-neutral'}>{labels[status] || status}</span>;
  };

  const formatCurrency = (val: number) =>
    new Intl.NumberFormat('es-CO', { style: 'currency', currency: 'COP', minimumFractionDigits: 0 }).format(val);

  return (
    <div className="space-y-6">
      <div className="flex flex-col sm:flex-row justify-between items-start sm:items-center gap-4">
        <div>
          <h1 className="text-2xl font-bold text-foreground tracking-tight">Cuotas Extraordinarias</h1>
          <p className="text-sm text-muted-foreground mt-1">Gestiona cuotas extraordinarias aprobadas por la asamblea.</p>
        </div>
        <Button onClick={() => router.push('/billing/extraordinary-fees/new')}>
          <Plus className="w-4 h-4 mr-2" />
          Nueva Cuota
        </Button>
      </div>

      {error && (
        <div className="p-4 bg-rose-50 border border-rose-200 rounded-xl text-rose-700 text-sm flex items-center gap-2">
          <AlertTriangle className="w-5 h-5 shrink-0" />
          {error}
        </div>
      )}

      <Card>
        <CardContent className="p-0">
          <div className="overflow-x-auto">
            <table className="min-w-full divide-y divide-border">
              <thead className="bg-muted/50">
                <tr>
                  <th className="px-6 py-4 text-left text-xs font-bold text-muted-foreground uppercase tracking-wider">Nombre</th>
                  <th className="px-6 py-4 text-right text-xs font-bold text-muted-foreground uppercase tracking-wider">Monto Total</th>
                  <th className="px-6 py-4 text-left text-xs font-bold text-muted-foreground uppercase tracking-wider">Tipo Distribución</th>
                  <th className="px-6 py-4 text-left text-xs font-bold text-muted-foreground uppercase tracking-wider">Fecha Inicio</th>
                  <th className="px-6 py-4 text-left text-xs font-bold text-muted-foreground uppercase tracking-wider">Estado</th>
                  <th className="px-6 py-4 text-right text-xs font-bold text-muted-foreground uppercase tracking-wider">Recaudado</th>
                  <th className="px-6 py-4 text-right text-xs font-bold text-muted-foreground uppercase tracking-wider">Pendiente</th>
                  <th className="px-6 py-4 text-right text-xs font-bold text-muted-foreground uppercase tracking-wider">Acciones</th>
                </tr>
              </thead>
              <tbody className="divide-y divide-border">
                {loading ? (
                  <tr>
                    <td colSpan={8} className="px-6 py-12 text-center">
                      <Loader2 className="w-6 h-6 animate-spin mx-auto text-emerald-600" />
                    </td>
                  </tr>
                ) : fees.length === 0 ? (
                  <tr>
                    <td colSpan={8} className="px-6 py-12 text-center text-muted-foreground">
                      <DollarSign className="w-12 h-12 mx-auto text-muted-foreground/40 mb-3" />
                      <p className="font-semibold">No hay cuotas extraordinarias</p>
                      <p className="text-sm mt-1">Crea una nueva cuota extraordinaria para comenzar.</p>
                    </td>
                  </tr>
                ) : (
                  fees.map((f) => {
                    const distLabels: Record<string, string> = {
                      Equal: 'Igualitaria',
                      ByCoefficient: 'Por Coeficiente',
                      Custom: 'Personalizada',
                    };
                    return (
                      <tr key={f.id} className="hover:bg-muted/30 transition-colors">
                        <td className="px-6 py-4 whitespace-nowrap font-semibold text-foreground">{f.name}</td>
                        <td className="px-6 py-4 whitespace-nowrap text-right font-mono text-sm">{formatCurrency(f.totalAmount)}</td>
                        <td className="px-6 py-4 whitespace-nowrap text-sm text-muted-foreground">{distLabels[f.distributionType] || f.distributionType}</td>
                        <td className="px-6 py-4 whitespace-nowrap text-sm text-muted-foreground">{new Date(f.createdAt).toLocaleDateString('es-CO')}</td>
                        <td className="px-6 py-4 whitespace-nowrap">{statusBadge(f.status)}</td>
                        <td className="px-6 py-4 whitespace-nowrap text-right font-mono text-sm text-emerald-600">{formatCurrency(f.totalCollected)}</td>
                        <td className="px-6 py-4 whitespace-nowrap text-right font-mono text-sm text-rose-600">{formatCurrency(f.totalOutstanding)}</td>
                        <td className="px-6 py-4 whitespace-nowrap text-right">
                          <button onClick={() => router.push(`/billing/extraordinary-fees/${f.id}`)} className="text-emerald-600 hover:text-emerald-800 text-sm font-semibold px-3 py-1.5 bg-emerald-50 rounded-lg hover:bg-emerald-100 transition-colors">
                            <Eye className="w-4 h-4 inline mr-1" />
                            Ver
                          </button>
                        </td>
                      </tr>
                    );
                  })
                )}
              </tbody>
            </table>
          </div>
        </CardContent>
      </Card>

      {!loading && fees.length > 0 && (
        <p className="text-xs text-muted-foreground px-1">{fees.length} cuota{fees.length !== 1 ? 's' : ''} encontrada{fees.length !== 1 ? 's' : ''}</p>
      )}
    </div>
  );
}
