'use client';

import React, { useState, useEffect, useCallback } from 'react';
import { useAuth } from '@/context/AuthContext';
import {
  BarChart, Bar, XAxis, YAxis, CartesianGrid, Tooltip, ResponsiveContainer
} from 'recharts';
import { Card, CardContent, CardHeader } from '@/components/ui/Card';
import { Button } from '@/components/ui/Button';
import dashboardService, {
  DashboardData, AlertDto, UpcomingEventDto, RecentActivityDto,
  UnitMoraDto, MonthlyCollectionDto, UnitSummaryDto
} from '@/lib/dashboard-service';
import pqrService from '@/lib/pqr-service';
import {
  Loader2, AlertTriangle, DollarSign, Calendar, Activity,
  Building2, Users, TrendingUp, PiggyBank, FileText,
  CheckCircle2, Clock, Wallet, RefreshCw, LogOut, MessageSquare
} from 'lucide-react';

const formatCurrency = (val: number) =>
  new Intl.NumberFormat('es-CO', { style: 'currency', currency: 'COP', minimumFractionDigits: 0 }).format(val);

const formatPercent = (val: number) => `${val.toFixed(1)}%`;

const formatDate = (dateStr: string) =>
  new Date(dateStr).toLocaleDateString('es-CO', { day: '2-digit', month: 'short', year: 'numeric' });

const formatDateTime = (dateStr: string) =>
  new Date(dateStr).toLocaleDateString('es-CO', {
    day: '2-digit', month: 'short', year: 'numeric',
    hour: '2-digit', minute: '2-digit'
  });

const periodLabels: Record<string, string> = {
  '01': 'Ene', '02': 'Feb', '03': 'Mar', '04': 'Abr',
  '05': 'May', '06': 'Jun', '07': 'Jul', '08': 'Ago',
  '09': 'Sep', '10': 'Oct', '11': 'Nov', '12': 'Dic'
};

const shortPeriodLabel = (period: string) => {
  const parts = period.split('-');
  if (parts.length !== 2) return period;
  return `${periodLabels[parts[1]] || parts[1]} ${parts[0].slice(2)}`;
};

const urgencyColors: Record<number, string> = {
  0: 'bg-emerald-100 text-emerald-800 border-emerald-200',
  1: 'bg-yellow-100 text-yellow-800 border-yellow-200',
  2: 'bg-orange-100 text-orange-800 border-orange-200',
  3: 'bg-rose-100 text-rose-800 border-rose-200'
};

const urgencyLabels: Record<number, string> = {
  0: 'Info', 1: 'Media', 2: 'Alta', 3: 'Crítica'
};

export default function DashboardPage() {
  const { user, logout } = useAuth();
  const [data, setData] = useState<DashboardData | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');
  const [refreshing, setRefreshing] = useState(false);
  const [pqrAlertCount, setPqrAlertCount] = useState(0);

  const fetchData = useCallback(async (silent = false) => {
    if (!silent) setLoading(true);
    setError('');
    try {
      const [result, pqrAlerts] = await Promise.all([
        dashboardService.getDashboard(),
        pqrService.getActiveAlerts().catch(() => [] as any[]),
      ]);
      setData(result);
      setPqrAlertCount(pqrAlerts.length);
    } catch {
      setError('Error al cargar datos del dashboard.');
    } finally {
      setLoading(false);
      setRefreshing(false);
    }
  }, []);

  useEffect(() => { fetchData(); }, [fetchData]);

  const handleRefresh = () => {
    setRefreshing(true);
    fetchData(true);
  };

  if (loading) {
    return (
      <div className="flex items-center justify-center h-96">
        <Loader2 className="w-8 h-8 animate-spin text-emerald-600" />
      </div>
    );
  }

  if (error) {
    return (
      <div className="space-y-6">
        <div className="flex items-center justify-between">
          <h1 className="text-2xl font-black tracking-tight">Dashboard</h1>
        </div>
        <div className="bg-rose-50 dark:bg-rose-950/30 border border-rose-200 dark:border-rose-800 text-rose-700 dark:text-rose-300 px-4 py-3 rounded-lg text-sm">{error}</div>
      </div>
    );
  }

  if (!data) return null;

  const role = user?.role || '';

  if (role === 'Resident') {
    return <ResidentDashboard data={data} user={user} logout={logout} />;
  }

  const isAdmin = role === 'SuperAdmin' || role === 'Admin';
  const isCouncil = role === 'Council';
  const isAccountant = role === 'Accountant';
  const isAuditor = role === 'Auditor';

  return (
    <div className="space-y-6">
      <DashboardHeader user={user} logout={logout} onRefresh={handleRefresh} refreshing={refreshing} />

      {isAuditor && (
        <Card>
          <CardContent className="p-6 text-center text-muted-foreground">
            <FileText className="w-12 h-12 mx-auto mb-3 text-slate-300" />
            <p>Vista de solo lectura. Los reportes financieros están disponibles en la sección de Reportes Contables.</p>
          </CardContent>
        </Card>
      )}

      {(isAdmin || isCouncil) && (
        <>
          <KpiCards kpis={data.kpis} />

          <div className="grid grid-cols-1 lg:grid-cols-3 gap-6">
            <div className="lg:col-span-2">
              <CollectionChart data={data.monthlyCollection} />
            </div>
            <div className="space-y-4">
              <AlertsPanel alerts={data.alerts} />
              <PqrAlertsCard alertCount={pqrAlertCount} />
            </div>
          </div>

          <div className="grid grid-cols-1 lg:grid-cols-2 gap-6">
            <UpcomingEventsPanel events={data.upcomingEvents} />
            <RecentActivityPanel activities={data.recentActivity} />
          </div>
        </>
      )}

      {isAdmin && (
        <>
          <MoraMapSection units={data.moraMap} />
          <UnitSummariesSection summaries={data.unitSummaries} />
        </>
      )}

      {isCouncil && (
        <div className="grid grid-cols-1 lg:grid-cols-2 gap-6">
          <ContingencyFundCard fund={data.contingencyFund} />
          <PendingApprovalsCard approvals={data.pendingCouncilApprovals} />
        </div>
      )}

      {isAccountant && (
        <>
          <KpiCards kpis={data.kpis} />
          <CollectionChart data={data.monthlyCollection} />
        </>
      )}
    </div>
  );
}

function DashboardHeader({ user, logout, onRefresh, refreshing }: {
  user: any; logout: () => void; onRefresh: () => void; refreshing: boolean;
}) {
  return (
    <div className="flex items-center justify-between flex-wrap gap-4">
      <div>
        <h1 className="text-2xl font-black tracking-tight">Bienvenido de nuevo, {user?.name}</h1>
        <p className="text-muted-foreground">Resumen general del conjunto.</p>
      </div>
      <div className="flex items-center gap-2">
        <Button variant="secondary" onClick={onRefresh} disabled={refreshing} className="gap-2">
          <RefreshCw className={`w-4 h-4 ${refreshing ? 'animate-spin' : ''}`} />
          {refreshing ? 'Actualizando...' : 'Actualizar'}
        </Button>
        <Button variant="secondary" onClick={logout} className="gap-2">
          <LogOut size={18} />
          <span className="hidden sm:inline">Cerrar Sesión</span>
        </Button>
      </div>
    </div>
  );
}

function KpiCards({ kpis }: { kpis: DashboardData['kpis'] }) {
  const kpiList = [
    {
      title: 'Recaudo del Mes',
      value: formatPercent(kpis.currentMonthCollectionPercentage),
      subtitle: `${formatCurrency(kpis.currentMonthCollected)} / ${formatCurrency(kpis.currentMonthBilled)}`,
      icon: <TrendingUp className="w-5 h-5 text-emerald-600" />,
      color: kpis.currentMonthCollectionPercentage >= 70 ? 'text-emerald-600' : kpis.currentMonthCollectionPercentage >= 50 ? 'text-yellow-600' : 'text-rose-600'
    },
    {
      title: 'Cartera Vencida',
      value: formatCurrency(kpis.totalOverduePortfolio),
      subtitle: `${kpis.earlyOverdue > 0 ? `${formatCurrency(kpis.earlyOverdue)} temprana` : ''}`,
      icon: <AlertTriangle className="w-5 h-5 text-rose-600" />,
      color: kpis.totalOverduePortfolio > 0 ? 'text-rose-600' : 'text-emerald-600'
    },
    {
      title: 'Efectivo Disponible',
      value: formatCurrency(kpis.availableCash),
      subtitle: `${formatPercent(kpis.budgetExecutionPercentage)} ejecutado`,
      icon: <Wallet className="w-5 h-5 text-cyan-600" />,
      color: 'text-cyan-600'
    },
    {
      title: 'Ejecución Presupuestal',
      value: formatPercent(kpis.budgetExecutionPercentage),
      subtitle: `Año ${formatPercent(kpis.yearProgressPercentage)} transcurrido`,
      icon: <PiggyBank className="w-5 h-5 text-violet-600" />,
      color: kpis.budgetExecutionPercentage <= 100 ? 'text-violet-600' : 'text-rose-600'
    }
  ];

  return (
    <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-4">
      {kpiList.map((kpi, idx) => (
        <Card key={idx}>
          <CardContent className="p-5">
            <div className="flex items-center justify-between mb-3">
              <p className="text-xs font-bold text-muted-foreground uppercase tracking-wider">{kpi.title}</p>
              <div className="p-2 bg-slate-50 dark:bg-zinc-800 rounded-lg">{kpi.icon}</div>
            </div>
            <p className={`text-2xl font-black ${kpi.color}`}>{kpi.value}</p>
            <p className="text-xs text-muted-foreground mt-1">{kpi.subtitle}</p>
          </CardContent>
        </Card>
      ))}
    </div>
  );
}

function CollectionChart({ data }: { data: MonthlyCollectionDto[] }) {
  if (!data || data.length === 0) {
    return (
      <Card>
        <CardHeader><h3 className="font-bold text-lg">Recaudo Mensual</h3></CardHeader>
        <CardContent><p className="text-muted-foreground text-center py-8">No hay datos de recaudo disponibles.</p></CardContent>
      </Card>
    );
  }

  const chartData = data.map(d => ({
    period: shortPeriodLabel(d.period),
    Facturado: d.billed,
    Recaudado: d.collected
  }));

  return (
    <Card>
      <CardHeader>
        <div className="flex items-center justify-between">
          <h3 className="font-bold text-lg">Recaudo Mensual (12 meses)</h3>
          <DollarSign className="w-5 h-5 text-emerald-600" />
        </div>
      </CardHeader>
      <CardContent>
        <div className="h-72">
          <ResponsiveContainer width="100%" height="100%">
            <BarChart data={chartData} barGap={2}>
              <CartesianGrid strokeDasharray="3 3" stroke="#e2e8f0" />
              <XAxis dataKey="period" tick={{ fontSize: 11 }} />
              <YAxis tick={{ fontSize: 11 }} tickFormatter={(v) => `${(Number(v) / 1000000).toFixed(0)}M`} />
              <Tooltip formatter={(value) => [formatCurrency(Number(value)), '']} />
              <Bar dataKey="Facturado" fill="#94a3b8" radius={[4, 4, 0, 0]} />
              <Bar dataKey="Recaudado" fill="#059669" radius={[4, 4, 0, 0]} />
            </BarChart>
          </ResponsiveContainer>
        </div>
      </CardContent>
    </Card>
  );
}

function PqrAlertsCard({ alertCount }: { alertCount: number }) {
  return (
    <Card>
      <CardHeader>
        <div className="flex items-center justify-between">
          <h3 className="font-bold text-lg">Alertas PQR</h3>
          <MessageSquare className="w-5 h-5 text-emerald-600" />
        </div>
      </CardHeader>
      <CardContent className="p-4">
        {alertCount > 0 ? (
          <div className="flex items-center gap-3">
            <div className="w-10 h-10 rounded-xl bg-rose-50 flex items-center justify-center">
              <AlertTriangle className="w-5 h-5 text-rose-600" />
            </div>
            <div>
              <p className="text-xl font-black text-rose-600">{alertCount}</p>
              <p className="text-xs text-muted-foreground">alerta{alertCount !== 1 ? 's' : ''} activa{alertCount !== 1 ? 's' : ''}</p>
            </div>
          </div>
        ) : (
          <div className="flex items-center gap-3">
            <div className="w-10 h-10 rounded-xl bg-emerald-50 flex items-center justify-center">
              <CheckCircle2 className="w-5 h-5 text-emerald-600" />
            </div>
            <div>
              <p className="text-lg font-black text-emerald-600">Sin alertas</p>
              <p className="text-xs text-muted-foreground">todas las PQR en término</p>
            </div>
          </div>
        )}
        <a href="/pqr" className="mt-3 block text-xs font-semibold text-emerald-600 hover:text-emerald-800 transition-colors">
          Ir a Bandeja PQR →
        </a>
      </CardContent>
    </Card>
  );
}

function AlertsPanel({ alerts }: { alerts: AlertDto[] }) {
  return (
    <Card>
      <CardHeader>
        <div className="flex items-center justify-between">
          <h3 className="font-bold text-lg">Alertas</h3>
          <AlertTriangle className="w-5 h-5 text-amber-500" />
        </div>
      </CardHeader>
      <CardContent className="p-0">
        {alerts.length === 0 ? (
          <div className="p-6 text-center text-muted-foreground">
            <CheckCircle2 className="w-8 h-8 mx-auto mb-2 text-emerald-500" />
            <p className="text-sm">No hay alertas activas</p>
          </div>
        ) : (
          <div className="divide-y divide-border max-h-96 overflow-y-auto">
            {alerts.map(alert => (
              <a key={alert.id} href={alert.moduleLink} className="block p-4 hover:bg-slate-50 dark:hover:bg-zinc-900 transition-colors">
                <div className="flex items-start gap-3">
                  <span className={`px-2 py-0.5 rounded-full text-[10px] font-bold border ${urgencyColors[alert.urgency] || urgencyColors[1]}`}>
                    {urgencyLabels[alert.urgency] || 'Media'}
                  </span>
                  <div className="flex-1 min-w-0">
                    <p className="text-sm font-semibold text-foreground truncate">{alert.title}</p>
                    <p className="text-xs text-muted-foreground mt-0.5 line-clamp-2">{alert.description}</p>
                  </div>
                </div>
              </a>
            ))}
          </div>
        )}
      </CardContent>
    </Card>
  );
}

function UpcomingEventsPanel({ events }: { events: UpcomingEventDto[] }) {
  return (
    <Card>
      <CardHeader>
        <div className="flex items-center justify-between">
          <h3 className="font-bold text-lg">Próximos Eventos</h3>
          <Calendar className="w-5 h-5 text-blue-500" />
        </div>
      </CardHeader>
      <CardContent className="p-0">
        {events.length === 0 ? (
          <div className="p-6 text-center text-muted-foreground">
            <Calendar className="w-8 h-8 mx-auto mb-2 text-slate-300" />
            <p className="text-sm">No hay eventos próximos</p>
          </div>
        ) : (
          <div className="divide-y divide-border">
            {events.map((evt, idx) => (
              <a key={idx} href={evt.moduleLink} className="flex items-center gap-4 p-4 hover:bg-slate-50 dark:hover:bg-zinc-900 transition-colors">
                <div className="flex-shrink-0 w-10 h-10 rounded-lg bg-blue-50 dark:bg-blue-950/30 flex items-center justify-center">
                  <Calendar className="w-5 h-5 text-blue-600" />
                </div>
                <div className="flex-1 min-w-0">
                  <p className="text-sm font-semibold text-foreground truncate">{evt.title}</p>
                  <p className="text-xs text-muted-foreground mt-0.5 line-clamp-1">{evt.description}</p>
                </div>
                <span className="text-xs text-muted-foreground whitespace-nowrap">{formatDate(evt.eventDate)}</span>
              </a>
            ))}
          </div>
        )}
      </CardContent>
    </Card>
  );
}

function RecentActivityPanel({ activities }: { activities: RecentActivityDto[] }) {
  return (
    <Card>
      <CardHeader>
        <div className="flex items-center justify-between">
          <h3 className="font-bold text-lg">Actividad Reciente</h3>
          <Activity className="w-5 h-5 text-violet-500" />
        </div>
      </CardHeader>
      <CardContent className="p-0">
        {activities.length === 0 ? (
          <div className="p-6 text-center text-muted-foreground">
            <Activity className="w-8 h-8 mx-auto mb-2 text-slate-300" />
            <p className="text-sm">Sin actividad reciente</p>
          </div>
        ) : (
          <div className="divide-y divide-border max-h-96 overflow-y-auto">
            {activities.map((act, idx) => (
              <a key={idx} href={act.moduleLink} className="flex items-start gap-3 p-4 hover:bg-slate-50 dark:hover:bg-zinc-900 transition-colors">
                <div className="flex-shrink-0 w-8 h-8 rounded-full bg-slate-100 dark:bg-zinc-800 flex items-center justify-center">
                  <Activity className="w-4 h-4 text-slate-500" />
                </div>
                <div className="flex-1 min-w-0">
                  <p className="text-sm font-semibold text-foreground truncate">{act.action}</p>
                  <p className="text-xs text-muted-foreground mt-0.5 line-clamp-1">{act.description}</p>
                  <p className="text-[10px] text-muted-foreground mt-1">{formatDateTime(act.timestamp)}</p>
                </div>
              </a>
            ))}
          </div>
        )}
      </CardContent>
    </Card>
  );
}

function MoraMapSection({ units }: { units: UnitMoraDto[] }) {
  const [search, setSearch] = useState('');

  const filtered = search
    ? units.filter(u =>
        u.identifier.toLowerCase().includes(search.toLowerCase()) ||
        u.ownerName.toLowerCase().includes(search.toLowerCase()) ||
        u.towerOrBlock.toLowerCase().includes(search.toLowerCase()))
    : units;

  const colorBg: Record<string, string> = {
    green: 'bg-emerald-500',
    yellow: 'bg-yellow-400',
    orange: 'bg-orange-400',
    red: 'bg-rose-500',
    gray: 'bg-slate-300 dark:bg-slate-600'
  };

  const colorBorder: Record<string, string> = {
    green: 'border-emerald-600',
    yellow: 'border-yellow-500',
    orange: 'border-orange-500',
    red: 'border-rose-600',
    gray: 'border-slate-400 dark:border-slate-500'
  };

  return (
    <Card>
      <CardHeader>
        <div className="flex items-center justify-between flex-wrap gap-3">
          <h3 className="font-bold text-lg">Mapa de Mora</h3>
          <input
            type="text"
            placeholder="Buscar unidad, propietario o torre..."
            value={search}
            onChange={e => setSearch(e.target.value)}
            className="input-standard text-sm w-64"
          />
        </div>
        <div className="flex items-center gap-4 mt-2 text-xs text-muted-foreground">
          <span className="flex items-center gap-1"><span className="w-3 h-3 rounded-sm bg-emerald-500 inline-block" /> Al día</span>
          <span className="flex items-center gap-1"><span className="w-3 h-3 rounded-sm bg-yellow-400 inline-block" /> 1-30 días</span>
          <span className="flex items-center gap-1"><span className="w-3 h-3 rounded-sm bg-orange-400 inline-block" /> 31-90 días</span>
          <span className="flex items-center gap-1"><span className="w-3 h-3 rounded-sm bg-rose-500 inline-block" /> 90+ días</span>
          <span className="flex items-center gap-1"><span className="w-3 h-3 rounded-sm bg-slate-300 dark:bg-slate-600 inline-block" /> Desocupada</span>
        </div>
      </CardHeader>
      <CardContent>
        <div className="grid grid-cols-2 sm:grid-cols-3 md:grid-cols-4 lg:grid-cols-6 xl:grid-cols-8 gap-2">
          {filtered.map(unit => (
            <div
              key={unit.unitId}
              className={`relative p-2 rounded-lg border-2 ${colorBorder[unit.colorCode] || 'border-slate-200'} bg-white dark:bg-zinc-900 hover:shadow-md transition-shadow cursor-pointer`}
              title={`${unit.identifier} - ${unit.ownerName} - ${formatCurrency(unit.overdueBalance)}`}
            >
              <div className={`absolute top-0 right-0 w-3 h-3 rounded-bl-lg ${colorBg[unit.colorCode] || 'bg-slate-300'}`} />
              <p className="text-xs font-bold text-foreground truncate">{unit.identifier}</p>
              <p className="text-[10px] text-muted-foreground truncate">{unit.ownerName}</p>
              {unit.overdueBalance > 0 && (
                <p className="text-[10px] font-semibold text-rose-600 mt-1">{formatCurrency(unit.overdueBalance)}</p>
              )}
            </div>
          ))}
        </div>
        {filtered.length === 0 && (
          <p className="text-center text-muted-foreground py-8">No se encontraron unidades.</p>
        )}
        <p className="text-xs text-muted-foreground mt-4 text-center">
          Mostrando {filtered.length} de {units.length} unidades
        </p>
      </CardContent>
    </Card>
  );
}

function UnitSummariesSection({ summaries }: { summaries: UnitSummaryDto[] }) {
  const totalBalance = summaries.reduce((sum, u) => sum + u.currentBalance, 0);
  const inDebt = summaries.filter(u => u.currentBalance > 0).length;
  const clear = summaries.filter(u => u.currentBalance <= 0).length;

  return (
    <Card>
      <CardHeader>
        <div className="flex items-center justify-between">
          <h3 className="font-bold text-lg">Resumen de Unidades</h3>
          <Building2 className="w-5 h-5 text-emerald-600" />
        </div>
        <div className="flex gap-6 text-sm text-muted-foreground mt-1">
          <span><strong className="text-foreground">{summaries.length}</strong> total</span>
          <span><strong className="text-emerald-600">{clear}</strong> al día</span>
          <span><strong className="text-rose-600">{inDebt}</strong> en mora</span>
          <span><strong className="text-foreground">{formatCurrency(totalBalance)}</strong> saldo total</span>
        </div>
      </CardHeader>
      <CardContent className="p-0">
        <div className="overflow-x-auto max-h-80 overflow-y-auto">
          <table className="min-w-full divide-y divide-border">
            <thead className="bg-muted/50 sticky top-0">
              <tr>
                <th className="px-4 py-3 text-left text-xs font-bold text-muted-foreground uppercase tracking-wider">Unidad</th>
                <th className="px-4 py-3 text-left text-xs font-bold text-muted-foreground uppercase tracking-wider">Propietario</th>
                <th className="px-4 py-3 text-right text-xs font-bold text-muted-foreground uppercase tracking-wider">Saldo</th>
                <th className="px-4 py-3 text-center text-xs font-bold text-muted-foreground uppercase tracking-wider">Estado</th>
              </tr>
            </thead>
            <tbody className="divide-y divide-border">
              {summaries.map(u => (
                <tr key={u.unitId} className="hover:bg-muted/30 transition-colors">
                  <td className="px-4 py-3 whitespace-nowrap text-sm font-medium text-foreground">{u.identifier}</td>
                  <td className="px-4 py-3 whitespace-nowrap text-sm text-muted-foreground">{u.ownerName}</td>
                  <td className="px-4 py-3 whitespace-nowrap text-sm text-right font-mono">
                    <span className={u.currentBalance > 0 ? 'text-rose-600' : 'text-emerald-600'}>
                      {formatCurrency(u.currentBalance)}
                    </span>
                  </td>
                  <td className="px-4 py-3 whitespace-nowrap text-center">
                    <span className={`inline-block w-2.5 h-2.5 rounded-full ${
                      u.colorCode === 'green' ? 'bg-emerald-500' :
                      u.colorCode === 'gray' ? 'bg-slate-300 dark:bg-slate-600' :
                      'bg-rose-500'
                    }`} />
                    <span className="text-[10px] text-muted-foreground ml-1">{u.status}</span>
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      </CardContent>
    </Card>
  );
}

function ContingencyFundCard({ fund }: { fund: DashboardData['contingencyFund'] }) {
  return (
    <Card>
      <CardHeader>
        <div className="flex items-center justify-between">
          <h3 className="font-bold text-lg">Fondo de Imprevistos</h3>
          <PiggyBank className="w-5 h-5 text-emerald-600" />
        </div>
      </CardHeader>
      <CardContent>
        {fund ? (
          <div className="space-y-4">
            <div>
              <p className="text-xs text-muted-foreground uppercase tracking-wider font-bold">Saldo Actual</p>
              <p className="text-3xl font-black text-emerald-600">{formatCurrency(fund.currentBalance)}</p>
            </div>
            <div className="grid grid-cols-2 gap-4 pt-2 border-t border-border">
              <div>
                <p className="text-xs text-muted-foreground uppercase tracking-wider font-bold">Último Aporte</p>
                <p className="text-lg font-bold text-foreground">{formatCurrency(fund.lastContributionAmount)}</p>
              </div>
              <div>
                <p className="text-xs text-muted-foreground uppercase tracking-wider font-bold">Período</p>
                <p className="text-lg font-bold text-foreground">{fund.lastContributionPeriod || 'N/A'}</p>
              </div>
            </div>
          </div>
        ) : (
          <div className="text-center text-muted-foreground py-6">
            <PiggyBank className="w-10 h-10 mx-auto mb-2 text-slate-300" />
            <p className="text-sm">No hay fondo configurado</p>
          </div>
        )}
      </CardContent>
    </Card>
  );
}

function PendingApprovalsCard({ approvals }: { approvals: DashboardData['pendingCouncilApprovals'] }) {
  return (
    <Card>
      <CardHeader>
        <div className="flex items-center justify-between">
          <h3 className="font-bold text-lg">Aprobaciones Pendientes</h3>
          <Clock className="w-5 h-5 text-amber-500" />
        </div>
      </CardHeader>
      <CardContent className="p-0">
        {approvals.length === 0 ? (
          <div className="p-6 text-center text-muted-foreground">
            <CheckCircle2 className="w-8 h-8 mx-auto mb-2 text-emerald-500" />
            <p className="text-sm">No hay solicitudes pendientes</p>
          </div>
        ) : (
          <div className="divide-y divide-border">
            {approvals.map((app, idx) => (
              <a key={idx} href={app.moduleLink} className="flex items-center gap-4 p-4 hover:bg-slate-50 dark:hover:bg-zinc-900 transition-colors">
                <div className="flex-shrink-0 w-10 h-10 rounded-full bg-amber-50 dark:bg-amber-950/30 flex items-center justify-center">
                  <FileText className="w-5 h-5 text-amber-600" />
                </div>
                <div className="flex-1 min-w-0">
                  <p className="text-sm font-semibold text-foreground">{app.type}</p>
                  <p className="text-xs text-muted-foreground mt-0.5 line-clamp-1">{app.description}</p>
                </div>
                <div className="text-right">
                  <p className="text-sm font-bold text-foreground">{formatCurrency(app.amount)}</p>
                  <p className="text-[10px] text-muted-foreground">{formatDate(app.requestedAt)}</p>
                </div>
              </a>
            ))}
          </div>
        )}
      </CardContent>
    </Card>
  );
}

function ResidentDashboard({ data, user, logout }: { data: DashboardData; user: any; logout: () => void }) {
  const rd = data.residentData;

  if (!rd) {
    return (
      <div className="space-y-6">
        <div className="flex items-center justify-between">
          <h1 className="text-2xl font-black tracking-tight">Mi Resumen</h1>
          <Button variant="secondary" onClick={logout} className="gap-2">
            <LogOut size={18} /> Cerrar Sesión
          </Button>
        </div>
        <Card>
          <CardContent className="p-8 text-center text-muted-foreground">
            <Building2 className="w-12 h-12 mx-auto mb-3 text-slate-300" />
            <p>No se encontró información para tu unidad. Contacta a la administración.</p>
          </CardContent>
        </Card>
      </div>
    );
  }

  const balanceColor = rd.currentBalance > 0 ? 'text-rose-600' : 'text-emerald-600';

  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-2xl font-black tracking-tight">Mi Resumen</h1>
          <p className="text-muted-foreground">Unidad {rd.unitIdentifier}</p>
        </div>
        <Button variant="secondary" onClick={logout} className="gap-2">
          <LogOut size={18} /> Cerrar Sesión
        </Button>
      </div>

      <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-4">
        <Card>
          <CardContent className="p-5">
            <p className="text-xs font-bold text-muted-foreground uppercase tracking-wider">Saldo Pendiente</p>
            <p className={`text-2xl font-black mt-1 ${balanceColor}`}>{formatCurrency(rd.currentBalance)}</p>
          </CardContent>
        </Card>
        <Card>
          <CardContent className="p-5">
            <p className="text-xs font-bold text-muted-foreground uppercase tracking-wider">Intereses Acumulados</p>
            <p className="text-2xl font-black mt-1 text-amber-600">{formatCurrency(rd.lateInterestAccrued)}</p>
          </CardContent>
        </Card>
        <Card>
          <CardContent className="p-5">
            <p className="text-xs font-bold text-muted-foreground uppercase tracking-wider">Tasa de Interés Diaria</p>
            <p className="text-2xl font-black mt-1 text-foreground">{formatPercent(rd.dailyInterestRate)}</p>
          </CardContent>
        </Card>
        <Card>
          <CardContent className="p-5">
            <p className="text-xs font-bold text-muted-foreground uppercase tracking-wider">Días en Mora</p>
            <p className={`text-2xl font-black mt-1 ${rd.daysOverdue > 0 ? 'text-rose-600' : 'text-emerald-600'}`}>
              {rd.daysOverdue}
            </p>
          </CardContent>
        </Card>
      </div>

      {data.upcomingEvents.length > 0 && (
        <Card>
          <CardHeader>
            <h3 className="font-bold text-lg">Próximos Eventos</h3>
          </CardHeader>
          <CardContent className="p-0">
            <div className="divide-y divide-border">
              {data.upcomingEvents.map((evt, idx) => (
                <div key={idx} className="flex items-center gap-3 p-4">
                  <Calendar className="w-5 h-5 text-blue-500 flex-shrink-0" />
                  <div>
                    <p className="text-sm font-semibold text-foreground">{evt.title}</p>
                    <p className="text-xs text-muted-foreground">{formatDate(evt.eventDate)}</p>
                  </div>
                </div>
              ))}
            </div>
          </CardContent>
        </Card>
      )}
    </div>
  );
}
