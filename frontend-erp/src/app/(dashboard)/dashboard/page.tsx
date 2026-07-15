'use client';

import React, { useState, useEffect, useCallback } from 'react';
import { useRouter } from 'next/navigation';
import { useAuth } from '@/context/AuthContext';
import {
  BarChart, Bar, XAxis, YAxis, CartesianGrid, Tooltip, ResponsiveContainer
} from 'recharts';
import { Card, CardContent, CardHeader } from '@/components/ui/Card';
import { Button } from '@/components/ui/Button';
import dashboardService, {
  DashboardKpis, AlertItem, UpcomingEventItem, RecentActivityItem,
  PaymentStatusMap, UnitPaymentStatus, MonthlyCollectionItem,
  CouncilDashboard, AccountantBudgetPanel, AuditorDashboard, ResidentDashboard
} from '@/lib/dashboard-service';
import {
  Loader2, AlertTriangle, DollarSign, Calendar, Activity,
  Building2, TrendingUp, PiggyBank, FileText,
  CheckCircle2, Clock, RefreshCw, LogOut, MessageSquare, X
} from 'lucide-react';

const formatCurrency = (value: number) =>
  new Intl.NumberFormat('es-CO', { style: 'currency', currency: 'COP', minimumFractionDigits: 0 }).format(value);

const formatPercent = (value: number) => `${value.toFixed(1)}%`;

const formatDate = (dateStr: string) =>
  new Date(dateStr).toLocaleDateString('es-CO', { day: '2-digit', month: 'short', year: 'numeric' });

const formatDateTime = (dateStr: string) =>
  new Date(dateStr).toLocaleDateString('es-CO', {
    day: '2-digit', month: 'short', year: 'numeric', hour: '2-digit', minute: '2-digit'
  });

const periodLabels: Record<string, string> = {
  '01': 'Ene', '02': 'Feb', '03': 'Mar', '04': 'Abr',
  '05': 'May', '06': 'Jun', '07': 'Jul', '08': 'Ago',
  '09': 'Sep', '10': 'Oct', '11': 'Nov', '12': 'Dic'
};

function shortPeriodLabel(period: string): string {
  const parts = period.split('-');
  if (parts.length !== 2) {
    return period;
  }
  const monthLabel = periodLabels[parts[1]];
  if (monthLabel) {
    return `${monthLabel} ${parts[0].slice(2)}`;
  }
  return `${parts[1]} ${parts[0].slice(2)}`;
}

function getCollectionColorClass(percentage: number): string {
  if (percentage >= 70) {
    return 'text-emerald-600';
  }
  if (percentage >= 50) {
    return 'text-yellow-600';
  }
  return 'text-rose-600';
}

function getBudgetColorClass(executedPercentage: number, expectedPercentage: number): string {
  if (executedPercentage > 100) {
    return 'text-rose-600';
  }
  if (executedPercentage > expectedPercentage + 10) {
    return 'text-yellow-600';
  }
  return 'text-violet-600';
}

function getUrgencyBadgeClass(urgency: string): string {
  if (urgency === 'Critical') {
    return 'bg-rose-100 text-rose-800 border-rose-200';
  }
  if (urgency === 'High') {
    return 'bg-orange-100 text-orange-800 border-orange-200';
  }
  return 'bg-yellow-100 text-yellow-800 border-yellow-200';
}

function getUrgencyLabel(urgency: string): string {
  if (urgency === 'Critical') {
    return 'Crítica';
  }
  if (urgency === 'High') {
    return 'Alta';
  }
  return 'Media';
}

const paymentColorBg: Record<string, string> = {
  green: 'bg-emerald-500', yellow: 'bg-yellow-400', orange: 'bg-orange-400',
  red: 'bg-rose-500', gray: 'bg-slate-300 dark:bg-slate-600'
};

function getRefreshIconClass(refreshing?: boolean): string {
  if (refreshing) {
    return 'w-4 h-4 animate-spin';
  }
  return 'w-4 h-4';
}

function getRefreshButtonLabel(refreshing?: boolean): string {
  if (refreshing) {
    return 'Actualizando...';
  }
  return 'Actualizar';
}

export default function DashboardPage() {
  const { user, logout } = useAuth();
  const role = user?.role || '';

  if (role === 'Resident') {
    return <ResidentDashboardView user={user} logout={logout} />;
  }

  if (role === 'Auditor') {
    return <AuditorDashboardView user={user} logout={logout} />;
  }

  if (role === 'Accountant') {
    return <AccountantDashboardView user={user} logout={logout} />;
  }

  return <OperationalDashboardView user={user} logout={logout} role={role} />;
}

// ═══════════════════════════════════════════════════════════════════════
// Encabezado común
// ═══════════════════════════════════════════════════════════════════════

function DashboardHeader({ title, subtitle, user, logout, onRefresh, refreshing }: {
  title: string; subtitle: string; user: any; logout: () => void;
  onRefresh?: () => void; refreshing?: boolean;
}) {
  return (
    <div className="flex items-center justify-between flex-wrap gap-4">
      <div>
        <h1 className="text-2xl font-black tracking-tight">{title}</h1>
        <p className="text-muted-foreground">{subtitle}</p>
      </div>
      <div className="flex items-center gap-2">
        {onRefresh && (
          <Button variant="secondary" onClick={onRefresh} disabled={refreshing} className="gap-2">
            <RefreshCw className={getRefreshIconClass(refreshing)} />
            {getRefreshButtonLabel(refreshing)}
          </Button>
        )}
        <Button variant="secondary" onClick={logout} className="gap-2">
          <LogOut size={18} />
          <span className="hidden sm:inline">Cerrar Sesión</span>
        </Button>
      </div>
    </div>
  );
}

// ═══════════════════════════════════════════════════════════════════════
// Vista operativa: Administrador / Consejo
// ═══════════════════════════════════════════════════════════════════════

function OperationalDashboardView({ user, logout, role }: { user: any; logout: () => void; role: string }) {
  const [kpis, setKpis] = useState<DashboardKpis | null>(null);
  const [alerts, setAlerts] = useState<AlertItem[]>([]);
  const [chart, setChart] = useState<MonthlyCollectionItem[]>([]);
  const [events, setEvents] = useState<UpcomingEventItem[]>([]);
  const [activity, setActivity] = useState<RecentActivityItem[]>([]);
  const [paymentMap, setPaymentMap] = useState<PaymentStatusMap | null>(null);
  const [councilData, setCouncilData] = useState<CouncilDashboard | null>(null);
  const [loading, setLoading] = useState(true);
  const [refreshing, setRefreshing] = useState(false);
  const [error, setError] = useState('');

  const isAdmin = role === 'SuperAdmin' || role === 'Admin';
  const isCouncil = role === 'Council';

  const fetchData = useCallback(async (silent = false) => {
    if (!silent) {
      setLoading(true);
    }
    setError('');
    try {
      const requests: Promise<any>[] = [
        dashboardService.getKpis(),
        dashboardService.getAlerts(),
        dashboardService.getCollectionChart(),
        dashboardService.getUpcomingEvents()
      ];

      const [kpisResult, alertsResult, chartResult, eventsResult] = await Promise.all(requests);
      setKpis(kpisResult);
      setAlerts(alertsResult);
      setChart(chartResult);
      setEvents(eventsResult);

      if (isAdmin) {
        const [mapResult, activityResult] = await Promise.all([
          dashboardService.getPaymentStatusMap(),
          dashboardService.getRecentActivity()
        ]);
        setPaymentMap(mapResult);
        setActivity(activityResult);
      }

      if (isCouncil) {
        const council = await dashboardService.getCouncilDashboard();
        setCouncilData(council);
      }
    } catch {
      setError('Error al cargar datos del dashboard.');
    } finally {
      setLoading(false);
      setRefreshing(false);
    }
  }, [isAdmin, isCouncil]);

  useEffect(() => { fetchData(); }, [fetchData]);

  const handleRefresh = () => {
    setRefreshing(true);
    fetchData(true);
  };

  if (loading) {
    return <div className="flex items-center justify-center h-96"><Loader2 className="w-8 h-8 animate-spin text-emerald-600" /></div>;
  }

  if (error || !kpis) {
    return (
      <div className="space-y-6">
        <DashboardHeader title="Dashboard" subtitle="Resumen general del conjunto." user={user} logout={logout} />
        <div className="bg-rose-50 dark:bg-rose-950/30 border border-rose-200 dark:border-rose-800 text-rose-700 dark:text-rose-300 px-4 py-3 rounded-lg text-sm">
          {error || 'No se pudo cargar el dashboard.'}
        </div>
      </div>
    );
  }

  return (
    <div className="space-y-6">
      <DashboardHeader
        title={`Bienvenido de nuevo, ${user?.name}`}
        subtitle="Resumen operativo del conjunto."
        user={user}
        logout={logout}
        onRefresh={handleRefresh}
        refreshing={refreshing}
      />

      <KpiBar kpis={kpis} activeAlertCount={alerts.length} />

      <div className="grid grid-cols-1 lg:grid-cols-3 gap-6">
        <div className="lg:col-span-2">
          <CollectionChartCard data={chart} />
        </div>
        <AlertsPanel alerts={alerts} />
      </div>

      <div className="grid grid-cols-1 lg:grid-cols-2 gap-6">
        <UpcomingEventsPanel events={events} />
        {isAdmin && <RecentActivityPanel activities={activity} />}
        {isCouncil && councilData && <CouncilApprovalsPanel data={councilData} />}
      </div>

      {isAdmin && <PaymentStatusMapSection map={paymentMap} />}
    </div>
  );
}

// ═══════════════════════════════════════════════════════════════════════
// KPIs
// ═══════════════════════════════════════════════════════════════════════

function KpiBar({ kpis, activeAlertCount }: { kpis: DashboardKpis; activeAlertCount: number }) {
  let periodContext = `${kpis.daysElapsedInPeriod} de ${kpis.totalDaysInPeriod} días transcurridos`;

  const kpiList = [
    {
      title: 'Recaudo del Mes',
      value: formatPercent(kpis.currentMonthCollectionPercentage),
      subtitle: `Mes anterior: ${formatPercent(kpis.previousMonthCollectionPercentage)} · ${periodContext}`,
      icon: <TrendingUp className="w-5 h-5 text-emerald-600" />,
      color: getCollectionColorClass(kpis.currentMonthCollectionPercentage)
    },
    {
      title: 'Cartera Vencida',
      value: formatCurrency(kpis.totalOverduePortfolio),
      subtitle: `1 mes: ${formatCurrency(kpis.overdueOneMonth)} · 2 meses: ${formatCurrency(kpis.overdueTwoMonths)} · 3+ meses: ${formatCurrency(kpis.overdueThreeOrMoreMonths)}`,
      icon: <AlertTriangle className="w-5 h-5 text-rose-600" />,
      color: 'text-rose-600'
    },
    {
      title: 'Ejecución Presupuestal',
      value: formatPercent(kpis.budgetExecutionPercentage),
      subtitle: `Esperado a la fecha: ${formatPercent(kpis.budgetExpectedExecutionPercentage)}`,
      icon: <PiggyBank className="w-5 h-5 text-violet-600" />,
      color: getBudgetColorClass(kpis.budgetExecutionPercentage, kpis.budgetExpectedExecutionPercentage)
    },
    {
      title: 'PQR Abiertos',
      value: `${kpis.openPqrCount}`,
      subtitle: `${kpis.overduePqrCount} superaron el tiempo límite`,
      icon: <MessageSquare className="w-5 h-5 text-blue-600" />,
      color: 'text-blue-600'
    },
    {
      title: 'Alertas Activas',
      value: `${activeAlertCount}`,
      subtitle: 'Requieren atención hoy',
      icon: <Activity className="w-5 h-5 text-amber-600" />,
      color: 'text-amber-600'
    }
  ];

  return (
    <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-5 gap-4">
      {kpiList.map((kpi) => (
        <Card key={kpi.title}>
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

// ═══════════════════════════════════════════════════════════════════════
// Gráfico de recaudo histórico
// ═══════════════════════════════════════════════════════════════════════

function CollectionChartCard({ data }: { data: MonthlyCollectionItem[] }) {
  if (!data || data.length === 0) {
    return (
      <Card>
        <CardHeader><h3 className="font-bold text-lg">Recaudo Mensual</h3></CardHeader>
        <CardContent><p className="text-muted-foreground text-center py-8">No hay datos de recaudo disponibles.</p></CardContent>
      </Card>
    );
  }

  const chartData = data.map((item) => ({
    period: shortPeriodLabel(item.period),
    Liquidado: item.billed,
    Recaudado: item.collected
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
              <Bar dataKey="Liquidado" fill="#94a3b8" radius={[4, 4, 0, 0]} />
              <Bar dataKey="Recaudado" fill="#059669" radius={[4, 4, 0, 0]} />
            </BarChart>
          </ResponsiveContainer>
        </div>
      </CardContent>
    </Card>
  );
}

// ═══════════════════════════════════════════════════════════════════════
// Panel de alertas operativas
// ═══════════════════════════════════════════════════════════════════════

function AlertsPanel({ alerts }: { alerts: AlertItem[] }) {
  const router = useRouter();
  const [filter, setFilter] = useState('All');

  let filteredAlerts = alerts;
  if (filter !== 'All') {
    filteredAlerts = alerts.filter((alert) => alert.urgency === filter);
  }

  return (
    <Card>
      <CardHeader>
        <div className="flex items-center justify-between gap-2">
          <h3 className="font-bold text-lg">Alertas Operativas</h3>
          <AlertTriangle className="w-5 h-5 text-amber-500" />
        </div>
        <select
          value={filter}
          onChange={(e) => setFilter(e.target.value)}
          className="mt-2 bg-transparent border border-border rounded-lg px-2 py-1 text-xs font-semibold text-foreground outline-none"
        >
          <option value="All">Todas las urgencias</option>
          <option value="Critical">Crítica</option>
          <option value="High">Alta</option>
          <option value="Medium">Media</option>
        </select>
      </CardHeader>
      <CardContent className="p-0">
        {filteredAlerts.length === 0 && (
          <div className="p-6 text-center text-muted-foreground">
            <CheckCircle2 className="w-8 h-8 mx-auto mb-2 text-emerald-500" />
            <p className="text-sm">No hay alertas activas</p>
          </div>
        )}
        {filteredAlerts.length > 0 && (
          <div className="divide-y divide-border max-h-96 overflow-y-auto">
            {filteredAlerts.map((alert) => (
              <button
                key={alert.id}
                onClick={() => router.push(alert.moduleLink)}
                className="w-full text-left block p-4 hover:bg-slate-50 dark:hover:bg-zinc-900 transition-colors"
              >
                <div className="flex items-start gap-3">
                  <span className={`px-2 py-0.5 rounded-full text-[10px] font-bold border ${getUrgencyBadgeClass(alert.urgency)}`}>
                    {getUrgencyLabel(alert.urgency)}
                  </span>
                  <div className="flex-1 min-w-0">
                    <p className="text-sm font-semibold text-foreground truncate">{alert.title}</p>
                    <p className="text-xs text-muted-foreground mt-0.5 line-clamp-2">{alert.description}</p>
                  </div>
                </div>
              </button>
            ))}
          </div>
        )}
      </CardContent>
    </Card>
  );
}

// ═══════════════════════════════════════════════════════════════════════
// Próximos eventos y actividad reciente
// ═══════════════════════════════════════════════════════════════════════

function UpcomingEventsPanel({ events }: { events: UpcomingEventItem[] }) {
  const router = useRouter();

  return (
    <Card>
      <CardHeader>
        <div className="flex items-center justify-between">
          <h3 className="font-bold text-lg">Próximos Eventos (30 días)</h3>
          <Calendar className="w-5 h-5 text-blue-500" />
        </div>
      </CardHeader>
      <CardContent className="p-0">
        {events.length === 0 && (
          <div className="p-6 text-center text-muted-foreground">
            <Calendar className="w-8 h-8 mx-auto mb-2 text-slate-300" />
            <p className="text-sm">No hay eventos próximos</p>
          </div>
        )}
        {events.length > 0 && (
          <div className="divide-y divide-border max-h-96 overflow-y-auto">
            {events.map((event, idx) => (
              <button
                key={idx}
                onClick={() => router.push(event.moduleLink)}
                className="w-full text-left flex items-center gap-4 p-4 hover:bg-slate-50 dark:hover:bg-zinc-900 transition-colors"
              >
                <div className="flex-shrink-0 w-10 h-10 rounded-lg bg-blue-50 dark:bg-blue-950/30 flex items-center justify-center">
                  <Calendar className="w-5 h-5 text-blue-600" />
                </div>
                <div className="flex-1 min-w-0">
                  <p className="text-sm font-semibold text-foreground truncate">{event.title}</p>
                  <p className="text-xs text-muted-foreground mt-0.5 line-clamp-1">{event.description}</p>
                </div>
                <span className="text-xs text-muted-foreground whitespace-nowrap">{formatDate(event.eventDate)}</span>
              </button>
            ))}
          </div>
        )}
      </CardContent>
    </Card>
  );
}

function RecentActivityPanel({ activities }: { activities: RecentActivityItem[] }) {
  return (
    <Card>
      <CardHeader>
        <div className="flex items-center justify-between">
          <h3 className="font-bold text-lg">Actividad Reciente</h3>
          <Activity className="w-5 h-5 text-violet-500" />
        </div>
      </CardHeader>
      <CardContent className="p-0">
        {activities.length === 0 && (
          <div className="p-6 text-center text-muted-foreground">
            <Activity className="w-8 h-8 mx-auto mb-2 text-slate-300" />
            <p className="text-sm">Sin actividad reciente</p>
          </div>
        )}
        {activities.length > 0 && (
          <div className="divide-y divide-border max-h-96 overflow-y-auto">
            {activities.map((activityItem, idx) => (
              <div key={idx} className="flex items-start gap-3 p-4">
                <div className="flex-shrink-0 w-8 h-8 rounded-full bg-slate-100 dark:bg-zinc-800 flex items-center justify-center">
                  <Activity className="w-4 h-4 text-slate-500" />
                </div>
                <div className="flex-1 min-w-0">
                  <p className="text-sm font-semibold text-foreground truncate">{activityItem.action}</p>
                  <p className="text-xs text-muted-foreground mt-0.5 line-clamp-1">{activityItem.description}</p>
                  <p className="text-[10px] text-muted-foreground mt-1">{formatDateTime(activityItem.timestamp)}</p>
                </div>
              </div>
            ))}
          </div>
        )}
      </CardContent>
    </Card>
  );
}

// ═══════════════════════════════════════════════════════════════════════
// Mapa interactivo de estado de pago
// ═══════════════════════════════════════════════════════════════════════

function PaymentStatusMapSection({ map }: { map: PaymentStatusMap | null }) {
  const router = useRouter();
  const [selectedUnit, setSelectedUnit] = useState<UnitPaymentStatus | null>(null);

  if (!map || map.towers.length === 0) {
    return (
      <Card>
        <CardHeader><h3 className="font-bold text-lg">Mapa de Estado de Pago</h3></CardHeader>
        <CardContent><p className="text-muted-foreground text-center py-8">No hay unidades registradas.</p></CardContent>
      </Card>
    );
  }

  return (
    <Card>
      <CardHeader>
        <div className="flex items-center justify-between flex-wrap gap-3">
          <h3 className="font-bold text-lg">Mapa de Estado de Pago</h3>
          <Building2 className="w-5 h-5 text-emerald-600" />
        </div>
        <div className="flex items-center gap-4 mt-2 text-xs text-muted-foreground flex-wrap">
          <span className="flex items-center gap-1"><span className="w-3 h-3 rounded-sm bg-emerald-500 inline-block" /> Al día</span>
          <span className="flex items-center gap-1"><span className="w-3 h-3 rounded-sm bg-yellow-400 inline-block" /> 1 mes</span>
          <span className="flex items-center gap-1"><span className="w-3 h-3 rounded-sm bg-orange-400 inline-block" /> 2 meses</span>
          <span className="flex items-center gap-1"><span className="w-3 h-3 rounded-sm bg-rose-500 inline-block" /> 3+ meses</span>
          <span className="flex items-center gap-1"><span className="w-3 h-3 rounded-sm bg-slate-300 dark:bg-slate-600 inline-block" /> Desocupada / Inactiva</span>
        </div>
      </CardHeader>
      <CardContent className="space-y-6">
        {map.towers.map((tower) => (
          <div key={tower.towerOrBlock}>
            <p className="text-xs font-bold text-muted-foreground uppercase tracking-wider mb-2">{tower.towerOrBlock}</p>
            <div className="space-y-2">
              {tower.floors.map((floor) => (
                <div key={floor.floorLevel} className="flex items-center gap-3 flex-wrap">
                  <span className="text-[10px] font-bold text-muted-foreground w-16 flex-shrink-0">Piso {floor.floorLevel}</span>
                  <div className="flex gap-2 flex-wrap">
                    {floor.units.map((unit) => (
                      <button
                        key={unit.unitId}
                        onClick={() => setSelectedUnit(unit)}
                        title={`${unit.identifier} — ${unit.ownerName}`}
                        className={`w-11 h-11 rounded-lg text-[10px] font-bold text-white flex items-center justify-center hover:scale-105 transition-transform ${paymentColorBg[unit.colorCode] || 'bg-slate-300'}`}
                      >
                        {unit.identifier}
                      </button>
                    ))}
                  </div>
                </div>
              ))}
            </div>
          </div>
        ))}
      </CardContent>

      {selectedUnit && (
        <div className="fixed inset-0 z-50 flex justify-end bg-black/30" onClick={() => setSelectedUnit(null)}>
          <div
            className="w-full max-w-sm h-full bg-card border-l border-border p-6 space-y-4 overflow-y-auto"
            onClick={(e) => e.stopPropagation()}
          >
            <div className="flex items-center justify-between">
              <h4 className="font-bold text-lg">Unidad {selectedUnit.identifier}</h4>
              <button onClick={() => setSelectedUnit(null)} className="text-muted-foreground hover:text-foreground">
                <X className="w-5 h-5" />
              </button>
            </div>
            <div>
              <p className="text-xs font-bold text-muted-foreground uppercase tracking-wider">Propietario</p>
              <p className="text-sm font-medium text-foreground mt-1">{selectedUnit.ownerName}</p>
            </div>
            <div>
              <p className="text-xs font-bold text-muted-foreground uppercase tracking-wider">Saldo Pendiente</p>
              <p className="text-2xl font-black text-rose-600 mt-1">{formatCurrency(selectedUnit.overdueBalance)}</p>
            </div>
            <div>
              <p className="text-xs font-bold text-muted-foreground uppercase tracking-wider">Estado</p>
              <p className="text-sm font-medium text-foreground mt-1">{selectedUnit.statusLabel}</p>
            </div>
            <Button onClick={() => router.push(`/units/${selectedUnit.unitId}`)} className="w-full">
              Ver estado de cuenta completo
            </Button>
          </div>
        </div>
      )}
    </Card>
  );
}

// ═══════════════════════════════════════════════════════════════════════
// Vista de Consejo: aprobaciones pendientes + fondo de imprevistos
// ═══════════════════════════════════════════════════════════════════════

function CouncilApprovalsPanel({ data }: { data: CouncilDashboard }) {
  const router = useRouter();

  return (
    <Card>
      <CardHeader>
        <div className="flex items-center justify-between">
          <h3 className="font-bold text-lg">Solicitudes Pendientes del Consejo</h3>
          <Clock className="w-5 h-5 text-amber-500" />
        </div>
      </CardHeader>
      <CardContent className="p-0">
        {data.pendingApprovals.length === 0 && (
          <div className="p-6 text-center text-muted-foreground">
            <CheckCircle2 className="w-8 h-8 mx-auto mb-2 text-emerald-500" />
            <p className="text-sm">No hay solicitudes pendientes</p>
          </div>
        )}
        {data.pendingApprovals.length > 0 && (
          <div className="divide-y divide-border">
            {data.pendingApprovals.map((approval, idx) => (
              <button
                key={idx}
                onClick={() => router.push(approval.moduleLink)}
                className="w-full text-left flex items-center gap-4 p-4 hover:bg-slate-50 dark:hover:bg-zinc-900 transition-colors"
              >
                <div className="flex-shrink-0 w-10 h-10 rounded-full bg-amber-50 dark:bg-amber-950/30 flex items-center justify-center">
                  <FileText className="w-5 h-5 text-amber-600" />
                </div>
                <div className="flex-1 min-w-0">
                  <p className="text-sm font-semibold text-foreground line-clamp-1">{approval.description}</p>
                  <p className="text-[10px] text-muted-foreground">{formatDate(approval.requestedAt)}</p>
                </div>
                <p className="text-sm font-bold text-foreground whitespace-nowrap">{formatCurrency(approval.amount)}</p>
              </button>
            ))}
          </div>
        )}
        <div className="p-4 border-t border-border">
          <p className="text-xs font-bold text-muted-foreground uppercase tracking-wider mb-2">Fondo de Imprevistos (informativo)</p>
          <p className="text-xl font-black text-emerald-600">{formatCurrency(data.contingencyFund.availableBalance)}</p>
          <p className="text-xs text-muted-foreground mt-1">
            Contribuido: {formatCurrency(data.contingencyFund.totalContributed)} · Usado: {formatCurrency(data.contingencyFund.totalUsed)}
          </p>
        </div>
      </CardContent>
    </Card>
  );
}

// ═══════════════════════════════════════════════════════════════════════
// Vista de Contador
// ═══════════════════════════════════════════════════════════════════════

function AccountantDashboardView({ user, logout }: { user: any; logout: () => void }) {
  const [panel, setPanel] = useState<AccountantBudgetPanel | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');
  const router = useRouter();

  useEffect(() => {
    const load = async () => {
      try {
        const data = await dashboardService.getAccountantPanel();
        setPanel(data);
      } catch {
        setError('Error al cargar el panel presupuestal.');
      } finally {
        setLoading(false);
      }
    };
    load();
  }, []);

  if (loading) {
    return <div className="flex items-center justify-center h-96"><Loader2 className="w-8 h-8 animate-spin text-emerald-600" /></div>;
  }

  return (
    <div className="space-y-6">
      <DashboardHeader title="Panel Presupuestal" subtitle="Ejecución del presupuesto por rubro." user={user} logout={logout} />

      {error && (
        <div className="bg-rose-50 dark:bg-rose-950/30 border border-rose-200 dark:border-rose-800 text-rose-700 dark:text-rose-300 px-4 py-3 rounded-lg text-sm">{error}</div>
      )}

      {panel && (
        <>
          <div className="grid grid-cols-1 sm:grid-cols-3 gap-4">
            <Card><CardContent className="p-5 text-center">
              <p className="text-xs font-bold text-muted-foreground uppercase tracking-wider">Ejecución Total</p>
              <p className="text-2xl font-black text-violet-600 mt-2">{formatPercent(panel.execution.overallExecutionPercentage)}</p>
            </CardContent></Card>
            <Card><CardContent className="p-5 text-center">
              <p className="text-xs font-bold text-muted-foreground uppercase tracking-wider">Ejecutado</p>
              <p className="text-2xl font-black text-foreground mt-2">{formatCurrency(panel.execution.totalExecutedExpense)}</p>
            </CardContent></Card>
            <Card><CardContent className="p-5 text-center">
              <p className="text-xs font-bold text-muted-foreground uppercase tracking-wider">Disponible</p>
              <p className="text-2xl font-black text-emerald-600 mt-2">{formatCurrency(panel.execution.totalAvailable)}</p>
            </CardContent></Card>
          </div>

          {panel.execution.alerts.length > 0 && (
            <Card>
              <CardHeader><h3 className="font-bold text-lg">Alertas de Ejecución (90% / 100%)</h3></CardHeader>
              <CardContent className="p-0">
                <div className="divide-y divide-border">
                  {panel.execution.alerts.map((alert, idx) => (
                    <div key={idx} className="p-4">
                      <p className="text-sm font-semibold text-foreground">{alert.message}</p>
                    </div>
                  ))}
                </div>
              </CardContent>
            </Card>
          )}

          <Card>
            <CardHeader><h3 className="font-bold text-lg">Gastos Ejecutados por Rubro</h3></CardHeader>
            <CardContent className="p-0">
              <div className="overflow-x-auto">
                <table className="min-w-full divide-y divide-border">
                  <thead className="bg-muted/50">
                    <tr>
                      <th className="px-4 py-3 text-left text-xs font-bold text-muted-foreground uppercase">Rubro</th>
                      <th className="px-4 py-3 text-right text-xs font-bold text-muted-foreground uppercase">Presupuestado</th>
                      <th className="px-4 py-3 text-right text-xs font-bold text-muted-foreground uppercase">Ejecutado</th>
                      <th className="px-4 py-3 text-right text-xs font-bold text-muted-foreground uppercase">%</th>
                    </tr>
                  </thead>
                  <tbody className="divide-y divide-border">
                    {panel.execution.expenseItems.map((item) => (
                      <tr key={item.id}>
                        <td className="px-4 py-3 text-sm font-medium text-foreground">{item.name}</td>
                        <td className="px-4 py-3 text-sm text-right">{formatCurrency(item.annualValue)}</td>
                        <td className="px-4 py-3 text-sm text-right">{formatCurrency(item.executedValue)}</td>
                        <td className="px-4 py-3 text-sm text-right font-semibold">{formatPercent(item.executionPercentage)}</td>
                      </tr>
                    ))}
                  </tbody>
                </table>
              </div>
            </CardContent>
          </Card>

          <Card>
            <CardHeader><h3 className="font-bold text-lg">Reportes Exportables</h3></CardHeader>
            <CardContent className="p-0">
              <div className="divide-y divide-border">
                {panel.reportLinks.map((link) => (
                  <button
                    key={link.reportTypeCode}
                    onClick={() => router.push(link.moduleLink)}
                    className="w-full text-left flex items-center gap-3 p-4 hover:bg-slate-50 dark:hover:bg-zinc-900 transition-colors"
                  >
                    <FileText className="w-5 h-5 text-emerald-600" />
                    <p className="text-sm font-semibold text-foreground">{link.name}</p>
                  </button>
                ))}
              </div>
            </CardContent>
          </Card>
        </>
      )}
    </div>
  );
}

// ═══════════════════════════════════════════════════════════════════════
// Vista de Auditor: solo lectura, acceso directo a reportes
// ═══════════════════════════════════════════════════════════════════════

function AuditorDashboardView({ user, logout }: { user: any; logout: () => void }) {
  const [dashboard, setDashboard] = useState<AuditorDashboard | null>(null);
  const [loading, setLoading] = useState(true);
  const router = useRouter();

  useEffect(() => {
    const load = async () => {
      try {
        const data = await dashboardService.getAuditorDashboard();
        setDashboard(data);
      } finally {
        setLoading(false);
      }
    };
    load();
  }, []);

  if (loading) {
    return <div className="flex items-center justify-center h-96"><Loader2 className="w-8 h-8 animate-spin text-emerald-600" /></div>;
  }

  return (
    <div className="space-y-6">
      <DashboardHeader title="Acceso de Auditoría" subtitle="Vista de solo lectura." user={user} logout={logout} />

      <Card>
        <CardContent className="p-6 text-center text-muted-foreground">
          <FileText className="w-12 h-12 mx-auto mb-3 text-slate-300" />
          <p>Acceso de solo lectura. No hay elementos operativos disponibles en este perfil.</p>
        </CardContent>
      </Card>

      {dashboard && (
        <Card>
          <CardHeader><h3 className="font-bold text-lg">Reportes del Período {dashboard.currentFiscalYear}</h3></CardHeader>
          <CardContent className="p-0">
            {dashboard.availableReports.length === 0 && (
              <p className="p-6 text-center text-muted-foreground text-sm">No hay reportes disponibles para este perfil.</p>
            )}
            {dashboard.availableReports.length > 0 && (
              <div className="divide-y divide-border">
                {dashboard.availableReports.map((link) => (
                  <button
                    key={link.reportTypeCode}
                    onClick={() => router.push(link.moduleLink)}
                    className="w-full text-left flex items-center gap-3 p-4 hover:bg-slate-50 dark:hover:bg-zinc-900 transition-colors"
                  >
                    <FileText className="w-5 h-5 text-emerald-600" />
                    <p className="text-sm font-semibold text-foreground">{link.name}</p>
                  </button>
                ))}
              </div>
            )}
          </CardContent>
        </Card>
      )}
    </div>
  );
}

// ═══════════════════════════════════════════════════════════════════════
// Vista de Residente
// ═══════════════════════════════════════════════════════════════════════

function ResidentDashboardView({ user, logout }: { user: any; logout: () => void }) {
  const [data, setData] = useState<ResidentDashboard | null>(null);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    const load = async () => {
      try {
        const result = await dashboardService.getResidentDashboard();
        setData(result);
      } finally {
        setLoading(false);
      }
    };
    load();
  }, []);

  if (loading) {
    return <div className="flex items-center justify-center h-96"><Loader2 className="w-8 h-8 animate-spin text-emerald-600" /></div>;
  }

  if (!data || !data.unitIdentifier) {
    return (
      <div className="space-y-6">
        <DashboardHeader title="Mi Resumen" subtitle="" user={user} logout={logout} />
        <Card>
          <CardContent className="p-8 text-center text-muted-foreground">
            <Building2 className="w-12 h-12 mx-auto mb-3 text-slate-300" />
            <p>No se encontró información para tu unidad. Contacta a la administración.</p>
          </CardContent>
        </Card>
      </div>
    );
  }

  let balanceMessage = 'No debes nada. Estás al día.';
  let balanceColorClass = 'text-emerald-600';
  if (data.currentBalance > 0) {
    balanceColorClass = 'text-rose-600';
    let sinceText = '';
    if (data.oldestDebtDate) {
      sinceText = ` desde el ${formatDate(data.oldestDebtDate)}`;
    }
    balanceMessage = `Debes ${formatCurrency(data.currentBalance)}${sinceText}.`;
  }

  return (
    <div className="space-y-6">
      <DashboardHeader title="Mi Resumen" subtitle={`Unidad ${data.unitIdentifier}`} user={user} logout={logout} />

      <Card>
        <CardContent className="p-6">
          <p className="text-xs font-bold text-muted-foreground uppercase tracking-wider">Estado de Cuenta</p>
          <p className={`text-xl font-black mt-2 ${balanceColorClass}`}>{balanceMessage}</p>
        </CardContent>
      </Card>

      <div className="grid grid-cols-1 lg:grid-cols-2 gap-6">
        <Card>
          <CardHeader><h3 className="font-bold text-lg">Mis PQR</h3></CardHeader>
          <CardContent className="p-0">
            {data.openPqrs.length === 0 && (
              <p className="p-6 text-center text-sm text-muted-foreground">No tienes PQR abiertos.</p>
            )}
            {data.openPqrs.length > 0 && (
              <div className="divide-y divide-border">
                {data.openPqrs.map((pqr) => (
                  <div key={pqr.radicadoNumber} className="p-4">
                    <div className="flex items-center justify-between">
                      <p className="text-sm font-semibold text-foreground">{pqr.radicadoNumber}</p>
                      {pqr.isOverdue && <span className="badge-danger">Vencido</span>}
                    </div>
                    <p className="text-xs text-muted-foreground mt-1">{pqr.subject}</p>
                    <p className="text-[10px] text-muted-foreground mt-1">Estado: {pqr.status}</p>
                  </div>
                ))}
              </div>
            )}
          </CardContent>
        </Card>

        <Card>
          <CardHeader><h3 className="font-bold text-lg">Mis Reservas</h3></CardHeader>
          <CardContent className="p-0">
            {data.activeReservations.length === 0 && (
              <p className="p-6 text-center text-sm text-muted-foreground">No tienes reservas activas.</p>
            )}
            {data.activeReservations.length > 0 && (
              <div className="divide-y divide-border">
                {data.activeReservations.map((reservation, idx) => (
                  <div key={idx} className="p-4">
                    <p className="text-sm font-semibold text-foreground">{reservation.spaceName}</p>
                    <p className="text-xs text-muted-foreground mt-1">{formatDateTime(reservation.startDateTime)}</p>
                    <p className="text-[10px] text-muted-foreground mt-1">Estado: {reservation.status}</p>
                  </div>
                ))}
              </div>
            )}
          </CardContent>
        </Card>
      </div>

      <Card>
        <CardHeader><h3 className="font-bold text-lg">Últimas Circulares</h3></CardHeader>
        <CardContent className="p-0">
          {data.latestCirculars.length === 0 && (
            <p className="p-6 text-center text-sm text-muted-foreground">No hay circulares recientes.</p>
          )}
          {data.latestCirculars.length > 0 && (
            <div className="divide-y divide-border">
              {data.latestCirculars.map((circular, idx) => (
                <div key={idx} className="flex items-center gap-3 p-4">
                  <Calendar className="w-5 h-5 text-blue-500 flex-shrink-0" />
                  <div>
                    <p className="text-sm font-semibold text-foreground">{circular.title}</p>
                    <p className="text-xs text-muted-foreground">{formatDate(circular.publishedAt)}</p>
                  </div>
                </div>
              ))}
            </div>
          )}
        </CardContent>
      </Card>
    </div>
  );
}
