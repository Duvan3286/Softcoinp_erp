'use client';

import React, { useState, useEffect } from 'react';
import { useRouter } from 'next/navigation';
import { Loader2, DollarSign, Plus, Eye, AlertTriangle, CheckCircle, XCircle, MinusCircle } from 'lucide-react';
import { Button } from '@/components/ui/Button';
import { Card, CardHeader, CardContent } from '@/components/ui/Card';
import feesPortfolioService, { IndividualChargeDto } from '@/lib/fees-portfolio-service';

export default function IndividualChargesPage() {
  const router = useRouter();
  const [charges, setCharges] = useState<IndividualChargeDto[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');
  const [statusFilter, setStatusFilter] = useState('');

  useEffect(() => {
    fetchCharges();
  }, [statusFilter]);

  const fetchCharges = async () => {
    setLoading(true);
    setError('');
    try {
      const data = await feesPortfolioService.getIndividualCharges(statusFilter || undefined);
      setCharges(data);
    } catch {
      setError('Error al cargar los cobros individuales.');
    } finally {
      setLoading(false);
    }
  };

  const handleStatusChange = async (chargeId: string, newStatus: string) => {
    try {
      await feesPortfolioService.updateIndividualChargeStatus(chargeId, newStatus);
      fetchCharges();
    } catch (err: any) {
      setError(err?.response?.data || 'Error al actualizar el estado del cobro.');
    }
  };

  const statusBadge = (status: string) => {
    const map: Record<string, string> = {
      Pending: 'badge-warning',
      Paid: 'badge-success',
      Waived: 'badge-info',
      Disputed: 'badge-danger',
    };
    const labels: Record<string, string> = {
      Pending: 'Pendiente',
      Paid: 'Pagado',
      Waived: 'Condenado',
      Disputed: 'Disputado',
    };
    return <span className={map[status] || 'badge-neutral'}>{labels[status] || status}</span>;
  };

  const formatCurrency = (val: number) =>
    new Intl.NumberFormat('es-CO', { style: 'currency', currency: 'COP', minimumFractionDigits: 0 }).format(val);

  const typeLabels: Record<string, string> = {
    Fine: 'Multa',
    Surcharge: 'Recargo',
    Other: 'Otro',
  };

  const statusOptions = [
    { value: '', label: 'Todos' },
    { value: 'Pending', label: 'Pendiente' },
    { value: 'Paid', label: 'Pagado' },
    { value: 'Waived', label: 'Condenado' },
    { value: 'Disputed', label: 'Disputado' },
  ];

  const actionOptions = [
    { value: 'Paid', label: 'Marcar Pagado', icon: CheckCircle },
    { value: 'Waived', label: 'Condenar', icon: MinusCircle },
    { value: 'Disputed', label: 'Disputar', icon: XCircle },
  ];

  return (
    <div className="space-y-6">
      <div className="flex flex-col sm:flex-row justify-between items-start sm:items-center gap-4">
        <div>
          <h1 className="text-2xl font-bold text-foreground tracking-tight">Cobros Individuales</h1>
          <p className="text-sm text-muted-foreground mt-1">Gestiona multas, recargos y otros cobros individuales por unidad.</p>
        </div>
        <Button onClick={() => router.push('/billing/individual-charges/new')}>
          <Plus className="w-4 h-4 mr-2" />
          Nuevo Cobro
        </Button>
      </div>

      {error && (
        <div className="p-4 bg-rose-50 border border-rose-200 rounded-xl text-rose-700 text-sm flex items-center gap-2">
          <AlertTriangle className="w-5 h-5 shrink-0" />
          {error}
        </div>
      )}

      <div className="flex items-center gap-3">
        <span className="text-xs font-bold text-muted-foreground uppercase tracking-wider">Filtrar por estado:</span>
        <select
          value={statusFilter}
          onChange={(e) => setStatusFilter(e.target.value)}
          className="w-full bg-transparent border-b border-emerald-600 focus:border-b-2 text-foreground px-0 py-2 text-sm focus:outline-none transition-all max-w-[200px]"
        >
          {statusOptions.map((opt) => (
            <option key={opt.value} value={opt.value}>{opt.label}</option>
          ))}
        </select>
      </div>

      <Card>
        <CardContent className="p-0">
          <div className="overflow-x-auto">
            <table className="min-w-full divide-y divide-border">
              <thead className="bg-muted/50">
                <tr>
                  <th className="px-6 py-4 text-left text-xs font-bold text-muted-foreground uppercase tracking-wider">Unidad</th>
                  <th className="px-6 py-4 text-left text-xs font-bold text-muted-foreground uppercase tracking-wider">Tipo</th>
                  <th className="px-6 py-4 text-right text-xs font-bold text-muted-foreground uppercase tracking-wider">Monto</th>
                  <th className="px-6 py-4 text-right text-xs font-bold text-muted-foreground uppercase tracking-wider">Saldo</th>
                  <th className="px-6 py-4 text-left text-xs font-bold text-muted-foreground uppercase tracking-wider">Concepto</th>
                  <th className="px-6 py-4 text-left text-xs font-bold text-muted-foreground uppercase tracking-wider">Fecha</th>
                  <th className="px-6 py-4 text-left text-xs font-bold text-muted-foreground uppercase tracking-wider">Estado</th>
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
                ) : charges.length === 0 ? (
                  <tr>
                    <td colSpan={8} className="px-6 py-12 text-center text-muted-foreground">
                      <DollarSign className="w-12 h-12 mx-auto text-muted-foreground/40 mb-3" />
                      <p className="font-semibold">No hay cobros individuales</p>
                      <p className="text-sm mt-1">Crea un nuevo cobro para comenzar.</p>
                    </td>
                  </tr>
                ) : (
                  charges.map((c) => (
                    <tr key={c.id} className="hover:bg-muted/30 transition-colors">
                      <td className="px-6 py-4 whitespace-nowrap font-semibold text-foreground">{c.unitIdentifier}</td>
                      <td className="px-6 py-4 whitespace-nowrap text-sm text-muted-foreground">{typeLabels[c.chargeType] || c.chargeType}</td>
                      <td className="px-6 py-4 whitespace-nowrap text-right font-mono text-sm">{formatCurrency(c.amount)}</td>
                      <td className="px-6 py-4 whitespace-nowrap text-right font-mono text-sm font-bold text-rose-600">{formatCurrency(c.balanceAmount)}</td>
                      <td className="px-6 py-4 whitespace-nowrap text-sm text-foreground max-w-[200px] truncate">{c.concept}</td>
                      <td className="px-6 py-4 whitespace-nowrap text-sm text-muted-foreground">{new Date(c.chargeDate).toLocaleDateString('es-CO')}</td>
                      <td className="px-6 py-4 whitespace-nowrap">{statusBadge(c.status)}</td>
                      <td className="px-6 py-4 whitespace-nowrap text-right">
                        {c.status === 'Pending' && (
                          <div className="flex gap-1 justify-end">
                            {actionOptions.map((action) => {
                              const Icon = action.icon;
                              return (
                                <button
                                  key={action.value}
                                  onClick={() => handleStatusChange(c.id, action.value)}
                                  className="p-1.5 rounded-lg text-xs font-semibold transition-colors hover:bg-muted text-muted-foreground hover:text-foreground"
                                  title={action.label}
                                >
                                  <Icon className="w-4 h-4" />
                                </button>
                              );
                            })}
                          </div>
                        )}
                      </td>
                    </tr>
                  ))
                )}
              </tbody>
            </table>
          </div>
        </CardContent>
      </Card>

      {!loading && charges.length > 0 && (
        <p className="text-xs text-muted-foreground px-1">{charges.length} cobro{charges.length !== 1 ? 's' : ''} encontrado{charges.length !== 1 ? 's' : ''}</p>
      )}
    </div>
  );
}
