'use client';

import React, { useState, useEffect } from 'react';
import { Loader2, AlertTriangle, MessageSquare, Clock, CheckCircle2, AlertOctagon, Users, TrendingUp } from 'lucide-react';
import { Card, CardHeader, CardContent } from '@/components/ui/Card';
import pqrService, { PqrIndicators, ActiveAlert } from '@/lib/pqr-service';

export default function PqrIndicatorsPage() {
  const [indicators, setIndicators] = useState<PqrIndicators | null>(null);
  const [activeAlerts, setActiveAlerts] = useState<ActiveAlert[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');

  const fetchData = async () => {
    setLoading(true);
    setError('');
    try {
      const [ind, alerts] = await Promise.all([
        pqrService.getIndicators(),
        pqrService.getActiveAlerts(),
      ]);
      setIndicators(ind);
      setActiveAlerts(alerts);
    } catch {
      setError('Error al cargar los indicadores.');
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => { fetchData(); }, []);

  const typeLabels: Record<string, string> = { Request: 'Petición', Complaint: 'Queja', Claim: 'Reclamo' };
  const categoryLabels: Record<string, string> = { Billing: 'Facturación', Maintenance: 'Mantenimiento', Coexistence: 'Convivencia', CommonAreas: 'Zonas Comunes', Administration: 'Administración', Other: 'Otro' };
  const statusLabels: Record<string, string> = {
    Filed: 'Radicada', UnderReview: 'En Revisión', InManagement: 'En Trámite',
    Responded: 'Respondida', Closed: 'Cerrada', Reopened: 'Reabierta', Escalated: 'Escalada',
  };
  const alertTypeLabels: Record<string, string> = { FiftyPercent: 'Alerta 50%', EightyPercent: 'Alerta 80%', Overdue: 'Vencida' };

  const formatNumber = (n: number) => n.toLocaleString('es-CO');
  const formatHours = (h: number) => {
    if (h < 1) return `${Math.round(h * 60)} min`;
    if (h < 24) return `${h.toFixed(1)} h`;
    return `${(h / 24).toFixed(1)} días`;
  };

  if (loading) {
    return <div className="flex justify-center py-20"><Loader2 className="w-8 h-8 animate-spin text-emerald-600" /></div>;
  }

  if (error || !indicators) {
    return (
      <div className="p-6 bg-rose-50 border border-rose-200 rounded-xl text-rose-700 text-sm flex items-center gap-2">
        <AlertTriangle className="w-5 h-5 shrink-0" /> {error || 'No se pudieron cargar los indicadores'}
      </div>
    );
  }

  const kpiCards = [
    { label: 'Total PQR', value: formatNumber(indicators.totalPQRs), icon: MessageSquare, color: 'text-blue-600 bg-blue-50' },
    { label: 'En Trámite', value: formatNumber(indicators.openPQRs), icon: Clock, color: 'text-amber-600 bg-amber-50' },
    { label: 'Cerradas', value: formatNumber(indicators.closedPQRs), icon: CheckCircle2, color: 'text-emerald-600 bg-emerald-50' },
    { label: 'Escaladas', value: formatNumber(indicators.escalatedPQRs), icon: AlertOctagon, color: 'text-rose-600 bg-rose-50' },
    { label: 'Alertas Activas', value: formatNumber(indicators.activeAlerts), icon: AlertTriangle, color: 'text-orange-600 bg-orange-50' },
    { label: 'Respuesta Promedio', value: formatHours(indicators.averageResponseHours), icon: TrendingUp, color: 'text-purple-600 bg-purple-50' },
  ];

  return (
    <div className="space-y-6">
      <div>
        <h1 className="text-2xl font-bold text-foreground tracking-tight">Indicadores de PQR</h1>
        <p className="text-sm text-muted-foreground mt-1">Métricas y estadísticas de gestión de PQR.</p>
      </div>

      <div className="grid grid-cols-2 md:grid-cols-3 lg:grid-cols-6 gap-4">
        {kpiCards.map((k) => (
          <Card key={k.label}>
            <CardContent className="p-4">
              <div className={`w-9 h-9 rounded-xl flex items-center justify-center ${k.color} mb-3`}>
                <k.icon className="w-5 h-5" />
              </div>
              <p className="text-2xl font-bold text-foreground">{k.value}</p>
              <p className="text-xs text-muted-foreground font-medium mt-0.5">{k.label}</p>
            </CardContent>
          </Card>
        ))}
      </div>

      <div className="grid grid-cols-1 lg:grid-cols-2 gap-6">
        <Card>
          <CardHeader><h3 className="font-bold text-foreground">PQR por Tipo</h3></CardHeader>
          <CardContent className="p-0">
            <div className="overflow-x-auto">
              <table className="min-w-full divide-y divide-border">
                <thead className="bg-muted/50">
                  <tr>
                    <th className="px-5 py-3 text-left text-xs font-bold text-muted-foreground uppercase">Tipo</th>
                    <th className="px-5 py-3 text-right text-xs font-bold text-muted-foreground uppercase">Total</th>
                    <th className="px-5 py-3 text-right text-xs font-bold text-muted-foreground uppercase">Abiertas</th>
                  </tr>
                </thead>
                <tbody className="divide-y divide-border">
                  {indicators.byType.map((t) => (
                    <tr key={t.type}>
                      <td className="px-5 py-3 text-sm font-semibold text-foreground">{typeLabels[t.type] || t.type}</td>
                      <td className="px-5 py-3 text-sm text-right text-foreground font-mono">{formatNumber(t.count)}</td>
                      <td className="px-5 py-3 text-sm text-right text-amber-600 font-mono">{formatNumber(t.openCount)}</td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>
          </CardContent>
        </Card>

        <Card>
          <CardHeader><h3 className="font-bold text-foreground">PQR por Estado</h3></CardHeader>
          <CardContent className="p-0">
            <div className="overflow-x-auto">
              <table className="min-w-full divide-y divide-border">
                <thead className="bg-muted/50">
                  <tr>
                    <th className="px-5 py-3 text-left text-xs font-bold text-muted-foreground uppercase">Estado</th>
                    <th className="px-5 py-3 text-right text-xs font-bold text-muted-foreground uppercase">Cantidad</th>
                  </tr>
                </thead>
                <tbody className="divide-y divide-border">
                  {indicators.byStatus.map((s) => (
                    <tr key={s.status}>
                      <td className="px-5 py-3 text-sm font-semibold text-foreground">{_statusLabels[s.status] || s.status}</td>
                      <td className="px-5 py-3 text-sm text-right text-foreground font-mono">{formatNumber(s.count)}</td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>
          </CardContent>
        </Card>

        <Card>
          <CardHeader><h3 className="font-bold text-foreground">PQR por Categoría</h3></CardHeader>
          <CardContent className="p-0">
            <div className="overflow-x-auto">
              <table className="min-w-full divide-y divide-border">
                <thead className="bg-muted/50">
                  <tr>
                    <th className="px-5 py-3 text-left text-xs font-bold text-muted-foreground uppercase">Categoría</th>
                    <th className="px-5 py-3 text-right text-xs font-bold text-muted-foreground uppercase">Cantidad</th>
                  </tr>
                </thead>
                <tbody className="divide-y divide-border">
                  {indicators.byCategory.map((c) => (
                    <tr key={c.category}>
                      <td className="px-5 py-3 text-sm font-semibold text-foreground">{categoryLabels[c.category] || c.category}</td>
                      <td className="px-5 py-3 text-sm text-right text-foreground font-mono">{formatNumber(c.count)}</td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>
          </CardContent>
        </Card>

        <Card>
          <CardHeader><h3 className="font-bold text-foreground">Tiempo Promedio de Respuesta por Tipo</h3></CardHeader>
          <CardContent className="p-0">
            <div className="overflow-x-auto">
              <table className="min-w-full divide-y divide-border">
                <thead className="bg-muted/50">
                  <tr>
                    <th className="px-5 py-3 text-left text-xs font-bold text-muted-foreground uppercase">Tipo</th>
                    <th className="px-5 py-3 text-right text-xs font-bold text-muted-foreground uppercase">Promedio</th>
                    <th className="px-5 py-3 text-right text-xs font-bold text-muted-foreground uppercase">Casos</th>
                  </tr>
                </thead>
                <tbody className="divide-y divide-border">
                  {indicators.averageResponseByType.map((t) => (
                    <tr key={t.type}>
                      <td className="px-5 py-3 text-sm font-semibold text-foreground">{typeLabels[t.type] || t.type}</td>
                      <td className="px-5 py-3 text-sm text-right text-foreground font-mono">{formatHours(t.averageResponseHours)}</td>
                      <td className="px-5 py-3 text-sm text-right text-muted-foreground font-mono">{formatNumber(t.count)}</td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>
          </CardContent>
        </Card>
      </div>

      <Card>
        <CardHeader>
          <h3 className="font-bold text-foreground">Tendencia Mensual</h3>
        </CardHeader>
        <CardContent className="p-0">
          <div className="overflow-x-auto">
            <table className="min-w-full divide-y divide-border">
              <thead className="bg-muted/50">
                <tr>
                  <th className="px-5 py-3 text-left text-xs font-bold text-muted-foreground uppercase">Período</th>
                  <th className="px-5 py-3 text-right text-xs font-bold text-muted-foreground uppercase">PQR Radicadas</th>
                </tr>
              </thead>
              <tbody className="divide-y divide-border">
                {indicators.monthlyTrend.length === 0 ? (
                  <tr><td colSpan={2} className="px-6 py-12 text-center text-sm text-muted-foreground">No hay datos de tendencia mensual.</td></tr>
                ) : (
                  indicators.monthlyTrend.map((m) => (
                    <tr key={m.period}>
                      <td className="px-5 py-3 text-sm font-semibold text-foreground">{m.period}</td>
                      <td className="px-5 py-3 text-sm text-right text-foreground font-mono">{formatNumber(m.count)}</td>
                    </tr>
                  ))
                )}
              </tbody>
            </table>
          </div>
        </CardContent>
      </Card>

      <Card>
        <CardHeader>
          <h3 className="font-bold text-foreground flex items-center gap-2">
            <AlertTriangle className="w-4 h-4 text-rose-500" />
            Alertas Activas ({activeAlerts.length})
          </h3>
        </CardHeader>
        <CardContent className="p-0">
          <div className="overflow-x-auto">
            <table className="min-w-full divide-y divide-border">
              <thead className="bg-muted/50">
                <tr>
                  <th className="px-5 py-3 text-left text-xs font-bold text-muted-foreground uppercase">Alerta</th>
                  <th className="px-5 py-3 text-left text-xs font-bold text-muted-foreground uppercase">PQR</th>
                  <th className="px-5 py-3 text-left text-xs font-bold text-muted-foreground uppercase">Unidad</th>
                  <th className="px-5 py-3 text-left text-xs font-bold text-muted-foreground uppercase">Vencimiento</th>
                  <th className="px-5 py-3 text-left text-xs font-bold text-muted-foreground uppercase">Estado</th>
                  <th className="px-5 py-3 text-right text-xs font-bold text-muted-foreground uppercase">Escalada</th>
                </tr>
              </thead>
              <tbody className="divide-y divide-border">
                {activeAlerts.length === 0 ? (
                  <tr><td colSpan={6} className="px-6 py-12 text-center text-sm text-muted-foreground">No hay alertas activas.</td></tr>
                ) : (
                  activeAlerts.map((a) => (
                    <tr key={a.id}>
                      <td className="px-5 py-3 text-sm">
                        <span className="badge-warning">{alertTypeLabels[a.alertType] || a.alertType}</span>
                      </td>
                      <td className="px-5 py-3 text-sm font-semibold text-foreground font-mono">{a.pqr.radicadoNumber}</td>
                      <td className="px-5 py-3 text-sm text-muted-foreground">{a.pqr.unitIdentifier}</td>
                      <td className="px-5 py-3 text-sm text-muted-foreground">{new Date(a.pqr.deadline).toLocaleDateString('es-CO')}</td>
                      <td className="px-5 py-3 text-sm">{statusBadge(a.pqr.status)}</td>
                      <td className="px-5 py-3 text-sm text-right">{a.escalatedToCouncil ? <span className="badge-danger">Sí</span> : <span className="badge-neutral">No</span>}</td>
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

const _statusLabels: Record<string, string> = {
  Filed: 'Radicada', UnderReview: 'En Revisión', InManagement: 'En Trámite',
  Responded: 'Respondida', Closed: 'Cerrada', Reopened: 'Reabierta', Escalated: 'Escalada',
};

function statusBadge(status: string) {
  const map: Record<string, string> = {
    Filed: 'badge-info', UnderReview: 'badge-warning', InManagement: 'badge-warning',
    Responded: 'badge-success', Closed: 'badge-neutral', Reopened: 'badge-warning', Escalated: 'badge-danger',
  };
  return <span className={map[status] || 'badge-neutral'}>{_statusLabels[status] || status}</span>;
}
