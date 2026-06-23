'use client';

import React, { useState, useEffect } from 'react';
import { useRouter } from 'next/navigation';
import { Loader2, Plus, Eye, AlertTriangle, MessageSquare, Clock, AlertOctagon, CheckCircle2, Filter, Search } from 'lucide-react';
import { Button } from '@/components/ui/Button';
import { Card, CardContent } from '@/components/ui/Card';
import pqrService, { PqrListItem } from '@/lib/pqr-service';

type StatusFilter = '' | 'Filed' | 'UnderReview' | 'InManagement' | 'Responded' | 'Closed' | 'Reopened' | 'Escalated';
type TypeFilter = '' | 'Request' | 'Complaint' | 'Claim';

const statusLabels: Record<string, string> = {
  Filed: 'Radicada',
  UnderReview: 'En Revisión',
  InManagement: 'En Trámite',
  Responded: 'Respondida',
  Closed: 'Cerrada',
  Reopened: 'Reabierta',
  Escalated: 'Escalada',
};

const typeLabels: Record<string, string> = {
  Request: 'Petición',
  Complaint: 'Queja',
  Claim: 'Reclamo',
};

const categoryLabels: Record<string, string> = {
  Billing: 'Facturación',
  Maintenance: 'Mantenimiento',
  Coexistence: 'Convivencia',
  CommonAreas: 'Zonas Comunes',
  Administration: 'Administración',
  Other: 'Otro',
};

const priorityLabels: Record<string, string> = {
  Low: 'Baja',
  Normal: 'Normal',
  High: 'Alta',
  Urgent: 'Urgente',
};

export default function PqrBandejaPage() {
  const router = useRouter();
  const [pqrs, setPqrs] = useState<PqrListItem[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');
  const [statusFilter, setStatusFilter] = useState<StatusFilter>('');
  const [typeFilter, setTypeFilter] = useState<TypeFilter>('');
  const [isInternalFilter, setIsInternalFilter] = useState<boolean | undefined>(undefined);
  const [searchTerm, setSearchTerm] = useState('');

  const fetchPqrs = async () => {
    setLoading(true);
    setError('');
    try {
      const data = await pqrService.getPqrList(
        statusFilter || undefined,
        typeFilter || undefined,
        isInternalFilter
      );
      setPqrs(data);
    } catch {
      setError('Error al cargar la bandeja de PQR.');
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => { fetchPqrs(); }, [statusFilter, typeFilter, isInternalFilter]);

  const formatDate = (d: string) => new Date(d).toLocaleDateString('es-CO', { day: '2-digit', month: '2-digit', year: 'numeric' });

  const semaphoreColor = (elapsedPercent: number) => {
    if (elapsedPercent >= 100) return 'bg-rose-500';
    if (elapsedPercent >= 80) return 'bg-orange-400';
    if (elapsedPercent >= 50) return 'bg-amber-400';
    return 'bg-emerald-500';
  };

  const statusBadge = (status: string) => {
    const map: Record<string, string> = {
      Filed: 'badge-info',
      UnderReview: 'badge-warning',
      InManagement: 'badge-warning',
      Responded: 'badge-success',
      Closed: 'badge-neutral',
      Reopened: 'badge-warning',
      Escalated: 'badge-danger',
    };
    return <span className={map[status] || 'badge-neutral'}>{statusLabels[status] || status}</span>;
  };

  const priorityBadge = (priority: string) => {
    const map: Record<string, string> = {
      Low: 'badge-neutral',
      Normal: 'badge-info',
      High: 'badge-warning',
      Urgent: 'badge-danger',
    };
    return <span className={map[priority] || 'badge-neutral'}>{priorityLabels[priority] || priority}</span>;
  };

  const filtered = pqrs.filter(p =>
    !searchTerm || p.radicadoNumber.toLowerCase().includes(searchTerm.toLowerCase()) ||
    p.subject.toLowerCase().includes(searchTerm.toLowerCase()) ||
    p.radiadorName.toLowerCase().includes(searchTerm.toLowerCase()) ||
    p.unitIdentifier.toLowerCase().includes(searchTerm.toLowerCase())
  );

  const openCount = pqrs.filter(p => !['Responded', 'Closed'].includes(p.status)).length;
  const alertCount = pqrs.filter(p => p.elapsedPercent >= 80).length;
  const escalatedCount = pqrs.filter(p => p.status === 'Escalated').length;

  const summaryCards = [
    { label: 'Total PQR', value: pqrs.length, icon: MessageSquare, color: 'text-blue-600 bg-blue-50' },
    { label: 'En Trámite', value: openCount, icon: Clock, color: 'text-amber-600 bg-amber-50' },
    { label: 'Próximas a Vencer', value: alertCount, icon: AlertOctagon, color: 'text-orange-600 bg-orange-50' },
    { label: 'Escaladas', value: escalatedCount, icon: AlertTriangle, color: 'text-rose-600 bg-rose-50' },
  ];

  return (
    <div className="space-y-6">
      <div className="flex flex-col sm:flex-row justify-between items-start sm:items-center gap-4">
        <div>
          <h1 className="text-2xl font-bold text-foreground tracking-tight">Bandeja PQR</h1>
          <p className="text-sm text-muted-foreground mt-1">Gestión de Peticiones, Quejas y Reclamos.</p>
        </div>
        <Button onClick={() => router.push('/pqr/new')}>
          <Plus className="w-4 h-4 mr-2" />
          Radicar PQR
        </Button>
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
              {Object.entries(statusLabels).map(([k, v]) => (
                <option key={k} value={k}>{v}</option>
              ))}
            </select>
            <select value={typeFilter} onChange={(e) => setTypeFilter(e.target.value as TypeFilter)}
              className="bg-transparent border border-border rounded-lg px-3 py-1.5 text-sm text-foreground focus:outline-none focus:ring-1 focus:ring-emerald-500">
              <option value="">Todos los tipos</option>
              {Object.entries(typeLabels).map(([k, v]) => (
                <option key={k} value={k}>{v}</option>
              ))}
            </select>
            <label className="flex items-center gap-2 text-sm text-muted-foreground cursor-pointer">
              <input type="checkbox" checked={isInternalFilter === true} onChange={(e) => setIsInternalFilter(e.target.checked ? true : undefined)}
                className="accent-emerald-600 w-4 h-4" />
              Solo internos
            </label>
            <div className="flex-1 min-w-[200px] relative">
              <Search className="absolute left-3 top-1/2 -translate-y-1/2 w-4 h-4 text-muted-foreground" />
              <input type="text" placeholder="Buscar radicado, asunto, radicador..."
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
                  <th className="px-5 py-3.5 text-left text-xs font-bold text-muted-foreground uppercase tracking-wider">Radicado</th>
                  <th className="px-5 py-3.5 text-left text-xs font-bold text-muted-foreground uppercase tracking-wider">Tipo</th>
                  <th className="px-5 py-3.5 text-left text-xs font-bold text-muted-foreground uppercase tracking-wider">Asunto</th>
                  <th className="px-5 py-3.5 text-left text-xs font-bold text-muted-foreground uppercase tracking-wider">Unidad</th>
                  <th className="px-5 py-3.5 text-left text-xs font-bold text-muted-foreground uppercase tracking-wider">Radicador</th>
                  <th className="px-5 py-3.5 text-left text-xs font-bold text-muted-foreground uppercase tracking-wider">Estado</th>
                  <th className="px-5 py-3.5 text-left text-xs font-bold text-muted-foreground uppercase tracking-wider">Prioridad</th>
                  <th className="px-5 py-3.5 text-left text-xs font-bold text-muted-foreground uppercase tracking-wider">Vencimiento</th>
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
                      <Button variant="secondary" onClick={fetchPqrs}>Reintentar</Button>
                    </div>
                  </td></tr>
                ) : filtered.length === 0 ? (
                  <tr><td colSpan={9} className="px-6 py-12 text-center">
                    <MessageSquare className="w-12 h-12 mx-auto text-muted-foreground/40 mb-3" />
                    <p className="font-semibold text-muted-foreground">No se encontraron PQR</p>
                    <p className="text-sm text-muted-foreground/60 mt-1">Intenta ajustar los filtros o radica una nueva PQR.</p>
                  </td></tr>
                ) : (
                  filtered.map((p) => (
                    <tr key={p.id} className="hover:bg-muted/30 transition-colors">
                      <td className="px-5 py-4 whitespace-nowrap">
                        <span className="font-mono font-bold text-sm text-foreground">{p.radicadoNumber}</span>
                        {p.isInternal && <span className="ml-2 text-[10px] font-bold text-amber-600 bg-amber-50 px-1.5 py-0.5 rounded">INT</span>}
                      </td>
                      <td className="px-5 py-4 whitespace-nowrap text-sm text-muted-foreground">{typeLabels[p.pqrType] || p.pqrType}</td>
                      <td className="px-5 py-4 whitespace-nowrap font-semibold text-foreground max-w-[200px] truncate">{p.subject}</td>
                      <td className="px-5 py-4 whitespace-nowrap text-sm text-muted-foreground">{p.unitIdentifier}</td>
                      <td className="px-5 py-4 whitespace-nowrap text-sm text-muted-foreground">{p.radiadorName}</td>
                      <td className="px-5 py-4 whitespace-nowrap">{statusBadge(p.status)}</td>
                      <td className="px-5 py-4 whitespace-nowrap">{priorityBadge(p.priority)}</td>
                      <td className="px-5 py-4 whitespace-nowrap">
                        <div className="flex items-center gap-2">
                          <div className="w-16 h-1.5 bg-muted rounded-full overflow-hidden">
                            <div className={`h-full rounded-full ${semaphoreColor(p.elapsedPercent)}`}
                              style={{ width: `${Math.min(p.elapsedPercent, 100)}%` }} />
                          </div>
                          <span className={`text-xs font-bold ${p.elapsedPercent >= 100 ? 'text-rose-600' : p.elapsedPercent >= 80 ? 'text-orange-500' : 'text-muted-foreground'}`}>
                            {formatDate(p.deadline)}
                          </span>
                        </div>
                      </td>
                      <td className="px-5 py-4 whitespace-nowrap text-right">
                        <button onClick={() => router.push(`/pqr/${p.id}`)}
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

      {!loading && filtered.length > 0 && (
        <p className="text-xs text-muted-foreground px-1">{filtered.length} PQR{filtered.length !== 1 ? 's' : ''} encontrada{filtered.length !== 1 ? 's' : ''}</p>
      )}
    </div>
  );
}
