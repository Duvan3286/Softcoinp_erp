'use client';

import React, { useState, useEffect } from 'react';
import { useRouter } from 'next/navigation';
import { Loader2, Plus, Eye, AlertTriangle, FileText, Handshake } from 'lucide-react';
import { Button } from '@/components/ui/Button';
import { Card, CardHeader, CardContent } from '@/components/ui/Card';
import feesPortfolioService, { PaymentAgreementSummary } from '@/lib/fees-portfolio-service';

export default function AgreementsPage() {
  const router = useRouter();
  const [agreements, setAgreements] = useState<PaymentAgreementSummary[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');

  useEffect(() => {
    fetchAgreements();
  }, []);

  const fetchAgreements = async () => {
    setLoading(true);
    setError('');
    try {
      const data = await feesPortfolioService.getAgreements();
      setAgreements(data);
    } catch {
      setError('Error al cargar los acuerdos de pago.');
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

  const formatCurrency = (val: number) =>
    new Intl.NumberFormat('es-CO', { style: 'currency', currency: 'COP', minimumFractionDigits: 0 }).format(val);

  return (
    <div className="space-y-6">
      <div className="flex flex-col sm:flex-row justify-between items-start sm:items-center gap-4">
        <div>
          <h1 className="text-2xl font-bold text-foreground tracking-tight">Acuerdos de Pago</h1>
          <p className="text-sm text-muted-foreground mt-1">Gestiona los acuerdos de pago con los propietarios.</p>
        </div>
        <Button onClick={() => router.push('/billing/agreements/new')}>
          <Plus className="w-4 h-4 mr-2" />
          Nuevo Acuerdo
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
                  <th className="px-6 py-4 text-left text-xs font-bold text-muted-foreground uppercase tracking-wider">Unidad</th>
                  <th className="px-6 py-4 text-right text-xs font-bold text-muted-foreground uppercase tracking-wider">Deuda Incluida</th>
                  <th className="px-6 py-4 text-right text-xs font-bold text-muted-foreground uppercase tracking-wider">Valor Cuota</th>
                  <th className="px-6 py-4 text-right text-xs font-bold text-muted-foreground uppercase tracking-wider">No. Cuotas</th>
                  <th className="px-6 py-4 text-right text-xs font-bold text-muted-foreground uppercase tracking-wider">% Condonación</th>
                  <th className="px-6 py-4 text-left text-xs font-bold text-muted-foreground uppercase tracking-wider">Estado</th>
                  <th className="px-6 py-4 text-right text-xs font-bold text-muted-foreground uppercase tracking-wider">Pagadas / Vencidas</th>
                  <th className="px-6 py-4 text-left text-xs font-bold text-muted-foreground uppercase tracking-wider">Inicio</th>
                  <th className="px-6 py-4 text-right text-xs font-bold text-muted-foreground uppercase tracking-wider">Acciones</th>
                </tr>
              </thead>
              <tbody className="divide-y divide-border">
                {loading ? (
                  <tr>
                    <td colSpan={9} className="px-6 py-12 text-center">
                      <Loader2 className="w-6 h-6 animate-spin mx-auto text-emerald-600" />
                    </td>
                  </tr>
                ) : agreements.length === 0 ? (
                  <tr>
                    <td colSpan={9} className="px-6 py-12 text-center text-muted-foreground">
                      <Handshake className="w-12 h-12 mx-auto text-muted-foreground/40 mb-3" />
                      <p className="font-semibold">No hay acuerdos de pago</p>
                      <p className="text-sm mt-1">Crea un nuevo acuerdo para comenzar.</p>
                    </td>
                  </tr>
                ) : (
                  agreements.map((a) => (
                    <tr
                      key={a.id}
                      className="hover:bg-muted/30 transition-colors cursor-pointer"
                      onClick={() => router.push(`/billing/agreements/${a.id}`)}
                    >
                      <td className="px-6 py-4 whitespace-nowrap font-semibold text-foreground">{a.unitIdentifier}</td>
                      <td className="px-6 py-4 whitespace-nowrap text-right font-mono text-sm">{formatCurrency(a.totalDebtIncluded)}</td>
                      <td className="px-6 py-4 whitespace-nowrap text-right font-mono text-sm">{formatCurrency(a.installmentAmount)}</td>
                      <td className="px-6 py-4 whitespace-nowrap text-right text-sm text-foreground">{a.numberOfInstallments}</td>
                      <td className="px-6 py-4 whitespace-nowrap text-right text-sm text-foreground">{a.interestForgivenessPercentage}%</td>
                      <td className="px-6 py-4 whitespace-nowrap">{statusBadge(a.status)}</td>
                      <td className="px-6 py-4 whitespace-nowrap text-right text-sm">
                        <span className="text-emerald-600 font-semibold">{a.paidInstallments}</span>
                        <span className="text-muted-foreground"> / </span>
                        <span className="text-rose-600 font-semibold">{a.overdueInstallments}</span>
                      </td>
                      <td className="px-6 py-4 whitespace-nowrap text-sm text-muted-foreground">{new Date(a.startedAt).toLocaleDateString('es-CO')}</td>
                      <td className="px-6 py-4 whitespace-nowrap text-right">
                        <button
                          onClick={(e) => { e.stopPropagation(); router.push(`/billing/agreements/${a.id}`); }}
                          className="text-emerald-600 hover:text-emerald-800 text-sm font-semibold px-3 py-1.5 bg-emerald-50 rounded-lg hover:bg-emerald-100 transition-colors"
                        >
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

      {!loading && agreements.length > 0 && (
        <p className="text-xs text-muted-foreground px-1">{agreements.length} acuerdo{agreements.length !== 1 ? 's' : ''} encontrado{agreements.length !== 1 ? 's' : ''}</p>
      )}
    </div>
  );
}
