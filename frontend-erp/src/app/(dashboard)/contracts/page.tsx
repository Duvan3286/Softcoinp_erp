'use client';

import React, { useState, useEffect } from 'react';
import { useRouter } from 'next/navigation';
import { Loader2, Plus, Eye, AlertTriangle, FileText, AlertOctagon, Clock, Search, Filter, DollarSign, Calendar } from 'lucide-react';
import { Button } from '@/components/ui/Button';
import { Card, CardContent } from '@/components/ui/Card';
import supplierService, { ContractListItem } from '@/lib/supplier-service';

type StatusFilter = '' | 'Draft' | 'Active' | 'Expired' | 'Terminated';
type TypeFilter = '' | 'ServiceAgreement' | 'Supply' | 'CivilWorks' | 'Lease';

const statusLabels: Record<string, string> = {
  Draft: 'Borrador',
  Active: 'Activo',
  Expired: 'Vencido',
  Terminated: 'Terminado',
};

const typeLabels: Record<string, string> = {
  ServiceAgreement: 'Contrato de Servicios',
  Supply: 'Suministro',
  CivilWorks: 'Obra Civil',
  Lease: 'Arrendamiento',
};

const approvalLabels: Record<string, string> = {
  Administrator: 'Administrador',
};

export default function ContractsPage() {
  const router = useRouter();
  const [contracts, setContracts] = useState<ContractListItem[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');
  const [statusFilter, setStatusFilter] = useState<StatusFilter>('');
  const [typeFilter, setTypeFilter] = useState<TypeFilter>('');
  const [searchTerm, setSearchTerm] = useState('');

  const fetchContracts = async () => {
    setLoading(true);
    setError('');
    try {
      const data = await supplierService.getContracts(
        statusFilter || undefined,
        typeFilter || undefined,
        undefined,
        searchTerm || undefined
      );
      setContracts(data);
    } catch {
      setError('Error al cargar los contratos.');
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => { fetchContracts(); }, [statusFilter, typeFilter]);

  useEffect(() => {
    const timeout = setTimeout(() => { fetchContracts(); }, 400);
    return () => clearTimeout(timeout);
  }, [searchTerm]);

  const formatCurrency = (v: number) => new Intl.NumberFormat('es-CO', { style: 'currency', currency: 'COP', maximumFractionDigits: 0 }).format(v);
  const formatDate = (d: string) => new Date(d).toLocaleDateString('es-CO', { day: '2-digit', month: '2-digit', year: 'numeric' });

  const statusBadge = (status: string) => {
    const map: Record<string, string> = {
      Draft: 'badge-info',
      Active: 'badge-success',
      Expired: 'badge-warning',
      Terminated: 'badge-neutral',
    };
    return <span className={map[status] || 'badge-neutral'}>{statusLabels[status] || status}</span>;
  };

  const expiringCount = contracts.filter(c => c.status === 'Active' && c.daysUntilExpiration <= 90 && c.daysUntilExpiration > 0).length;
  const alertCount = contracts.filter(c => c.alertCount > 0).length;

  const summaryCards = [
    { label: 'Total Contratos', value: contracts.length, icon: FileText, color: 'text-blue-600 bg-blue-50' },
    { label: 'Activos', value: contracts.filter(c => c.status === 'Active').length, icon: Clock, color: 'text-emerald-600 bg-emerald-50' },
    { label: 'Por Vencer (90d)', value: expiringCount, icon: AlertOctagon, color: 'text-orange-600 bg-orange-50' },
    { label: 'Con Alertas', value: alertCount, icon: AlertTriangle, color: 'text-rose-600 bg-rose-50' },
  ];

  return (
    <div className="space-y-6">
      <div className="flex flex-col sm:flex-row justify-between items-start sm:items-center gap-4">
        <div>
          <h1 className="text-2xl font-bold text-foreground tracking-tight">Contratos</h1>
          <p className="text-sm text-muted-foreground mt-1">Gestion de contratos con proveedores y contratistas.</p>
        </div>
        <div className="flex gap-2">
          <Button variant="secondary" onClick={() => router.push('/contracts/payments-pending')}>
            <Clock className="w-4 h-4 mr-1" /> Pagos Pendientes
          </Button>
          <Button variant="secondary" onClick={() => router.push('/contracts/indicators')}>
            <DollarSign className="w-4 h-4 mr-1" /> Indicadores
          </Button>
          <Button onClick={() => router.push('/contracts/new')}>
            <Plus className="w-4 h-4 mr-1" /> Nuevo Contrato
          </Button>
        </div>
      </div>

      <div className="grid grid-cols-2 md:grid-cols-4 gap-4">
        {summaryCards.map((c) => (
          <Card key={c.label}>
            <CardContent className="p-4 flex items-center gap-3">
              <div className={`w-10 h-10 rounded-xl flex items-center justify-center ${c.color}`}>
                <c.icon className="w-5 h-5" />
              </div>
              <div>
                <p className="text-xs text-muted-foreground font-medium">{c.label}</p>
                <p className="text-xl font-bold text-foreground">{c.value}</p>
              </div>
            </CardContent>
          </Card>
        ))}
      </div>

      <Card>
        <CardContent className="p-4 border-b border-border">
          <div className="flex flex-wrap items-center gap-3">
            <div className="flex items-center gap-2 text-sm text-muted-foreground">
              <Filter className="w-4 h-4" />
              <span className="font-semibold">Filtros:</span>
            </div>
            <select value={statusFilter} onChange={(e) => setStatusFilter(e.target.value as StatusFilter)}
              className="bg-transparent border border-border rounded-lg px-3 py-1.5 text-sm text-foreground focus:outline-none focus:ring-1 focus:ring-emerald-500">
              <option value="">Todos los estados</option>
              {Object.entries(statusLabels).map(([k, v]) => <option key={k} value={k}>{v}</option>)}
            </select>
            <select value={typeFilter} onChange={(e) => setTypeFilter(e.target.value as TypeFilter)}
              className="bg-transparent border border-border rounded-lg px-3 py-1.5 text-sm text-foreground focus:outline-none focus:ring-1 focus:ring-emerald-500">
              <option value="">Todos los tipos</option>
              {Object.entries(typeLabels).map(([k, v]) => <option key={k} value={k}>{v}</option>)}
            </select>
            <div className="flex-1 min-w-[200px] relative">
              <Search className="absolute left-3 top-1/2 -translate-y-1/2 w-4 h-4 text-muted-foreground" />
              <input type="text" placeholder="Buscar numero, objeto, proveedor..."
                value={searchTerm} onChange={(e) => setSearchTerm(e.target.value)}
                className="w-full bg-transparent border border-border rounded-lg pl-9 pr-3 py-1.5 text-sm text-foreground focus:outline-none focus:ring-1 focus:ring-emerald-500" />
            </div>
          </div>
        </CardContent>

        <CardContent className="p-0">
          <div className="overflow-x-auto">
            <table className="min-w-full divide-y divide-border">
              <thead className="bg-muted/50">
                <tr>
                  <th className="px-5 py-3.5 text-left text-xs font-bold text-muted-foreground uppercase tracking-wider">Nro. Contrato</th>
                  <th className="px-5 py-3.5 text-left text-xs font-bold text-muted-foreground uppercase tracking-wider">Proveedor</th>
                  <th className="px-5 py-3.5 text-left text-xs font-bold text-muted-foreground uppercase tracking-wider">Tipo</th>
                  <th className="px-5 py-3.5 text-right text-xs font-bold text-muted-foreground uppercase tracking-wider">Valor Total</th>
                  <th className="px-5 py-3.5 text-right text-xs font-bold text-muted-foreground uppercase tracking-wider">Mensual</th>
                  <th className="px-5 py-3.5 text-left text-xs font-bold text-muted-foreground uppercase tracking-wider">Vigencia</th>
                  <th className="px-5 py-3.5 text-left text-xs font-bold text-muted-foreground uppercase tracking-wider">Aprobacion</th>
                  <th className="px-5 py-3.5 text-left text-xs font-bold text-muted-foreground uppercase tracking-wider">Estado</th>
                  <th className="px-5 py-3.5 text-right text-xs font-bold text-muted-foreground uppercase tracking-wider">Acciones</th>
                </tr>
              </thead>
              <tbody className="divide-y divide-border">
                {loading ? (
                  <tr><td colSpan={9} className="px-6 py-12 text-center">
                    <Loader2 className="w-6 h-6 animate-spin mx-auto text-emerald-600" />
                  </td></tr>
                ) : error ? (
                  <tr><td colSpan={9} className="px-6 py-12 text-center">
                    <div className="flex flex-col items-center gap-2 text-rose-600">
                      <AlertTriangle className="w-8 h-8" />
                      <p className="text-sm font-semibold">{error}</p>
                      <Button variant="secondary" onClick={fetchContracts}>Reintentar</Button>
                    </div>
                  </td></tr>
                ) : contracts.length === 0 ? (
                  <tr><td colSpan={9} className="px-6 py-12 text-center">
                    <FileText className="w-12 h-12 mx-auto text-muted-foreground/40 mb-3" />
                    <p className="font-semibold text-muted-foreground">No se encontraron contratos</p>
                    <p className="text-sm text-muted-foreground/60 mt-1">Crea un nuevo contrato para comenzar.</p>
                  </td></tr>
                ) : (
                  contracts.map((c) => (
                    <tr key={c.id} className="hover:bg-muted/30 transition-colors">
                      <td className="px-5 py-4 whitespace-nowrap">
                        <span className="font-mono font-bold text-sm text-foreground">{c.contractNumber}</span>
                        {c.alertCount > 0 && <span className="ml-2 text-[10px] font-bold text-rose-600 bg-rose-50 px-1.5 py-0.5 rounded">{c.alertCount} ALERTA{c.alertCount > 1 ? 'S' : ''}</span>}
                      </td>
                      <td className="px-5 py-4 whitespace-nowrap text-sm font-medium text-foreground max-w-[180px] truncate">{c.providerBusinessName}</td>
                      <td className="px-5 py-4 whitespace-nowrap text-sm text-muted-foreground">{typeLabels[c.contractType] || c.contractType}</td>
                      <td className="px-5 py-4 whitespace-nowrap text-sm text-right font-medium">{formatCurrency(c.totalValue)}</td>
                      <td className="px-5 py-4 whitespace-nowrap text-sm text-right text-muted-foreground">{formatCurrency(c.monthlyValue)}</td>
                      <td className="px-5 py-4 whitespace-nowrap text-sm text-muted-foreground">
                        <div className="flex items-center gap-1">
                          <Calendar className="w-3 h-3" />
                          {formatDate(c.startDate)} - {formatDate(c.endDate)}
                        </div>
                        {c.status === 'Active' && (
                          <p className={`text-xs font-bold mt-0.5 ${c.daysUntilExpiration <= 30 ? 'text-rose-600' : c.daysUntilExpiration <= 90 ? 'text-orange-500' : 'text-muted-foreground'}`}>
                            {c.daysUntilExpiration > 0 ? `${c.daysUntilExpiration} dias restantes` : 'Vencido'}
                          </p>
                        )}
                      </td>
                      <td className="px-5 py-4 whitespace-nowrap text-sm text-muted-foreground">{approvalLabels[c.approvalLevel] || c.approvalLevel}</td>
                      <td className="px-5 py-4 whitespace-nowrap">{statusBadge(c.status)}</td>
                      <td className="px-5 py-4 whitespace-nowrap text-right">
                        <button onClick={() => router.push(`/contracts/${c.id}`)}
                          className="text-emerald-600 hover:text-emerald-800 text-sm font-semibold px-3 py-1.5 bg-emerald-50 rounded-lg hover:bg-emerald-100 transition-colors">
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

      {!loading && contracts.length > 0 && (
        <p className="text-xs text-muted-foreground px-1">{contracts.length} contrato{contracts.length !== 1 ? 's' : ''} encontrado{contracts.length !== 1 ? 's' : ''}</p>
      )}
    </div>
  );
}
