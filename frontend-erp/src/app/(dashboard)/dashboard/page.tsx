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
  PaymentStatusMap, UnitPaymentStatus, MonthlyCollectionItem
} from '@/lib/dashboard-service';
import interestService, { InterestCheckResult } from '@/lib/interest-service';
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
    return 'Critica';
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
  return <DashboardView user={user} logout={logout} />;
}

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
          <span className="hidden sm:inline">Cerrar Sesion</span>
        </Button>
      </div>
    </div>
  );
}

function DashboardView({ user, logout }: { user: any; logout: () => void }) {
  const [kpis, setKpis] = useState<DashboardKpis | null>(null);
  const [alerts, setAlerts] = useState<AlertItem[]>([]);
  const [chart, setChart] = useState<MonthlyCollectionItem[]>([]);
  const [events, setEvents] = useState<UpcomingEventItem[]>([]);
  const [activity, setActivity] = useState<RecentActivityItem[]>([]);
  const [paymentMap, setPaymentMap] = useState<PaymentStatusMap | null>(null);
  const [loading, setLoading] = useState(true);
  const [refreshing, setRefreshing] = useState(false);
  const [error, setError] = useState('');
  const [interestRateAlert, setInterestRateAlert] = useState<InterestCheckResult | null>(null);

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

      const [mapResult, activityResult] = await Promise.all([
        dashboardService.getPaymentStatusMap(),
        dashboardService.getRecentActivity()
      ]);
      setPaymentMap(mapResult);
      setActivity(activityResult);

      try {
        const rateCheck = await interestService.checkMissingRates();
        setInterestRateAlert(rateCheck);
      } catch {
        setInterestRateAlert(null);
      }
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

      {interestRateAlert && !interestRateAlert.hasRateForCurrentPeriod && interestRateAlert.alertEnabled && (
        <div className="p-3 bg-amber-50 dark:bg-amber-950/30 border border-amber-200 dark:border-amber-800 rounded-lg text-amber-700 dark:text-amber-300 text-sm flex items-center gap-2">
          <AlertTriangle className="w-4 h-4 shrink-0" />
          {interestRateAlert.message}
        </div>
      )}

      <KpiBar kpis={kpis} activeAlertCount={alerts.length} />

      <div className="grid grid-cols-1 lg:grid-cols-3 gap-6">
        <div className="lg:col-span-2">
          <CollectionChartCard data={chart} />
        </div>
        <AlertsPanel alerts={alerts} />
      </div>

      <div className="grid grid-cols-1 lg:grid-cols-2 gap-6">
        <UpcomingEventsPanel events={events} />
        <RecentActivityPanel activities={activity} />
      </div>

      <PaymentStatusMapSection map={paymentMap} />
    </div>
  );
}

function KpiBar({ kpis, activeAlertCount }: { kpis: DashboardKpis; activeAlertCount: number }) {
  let periodContext = `${kpis.daysElapsedInPeriod} de ${kpis.totalDaysInPeriod} dias transcurridos`;

  const kpiList = [
    {
      title: 'Recaudo del Mes',
      value: formatPercent(kpis.currentMonthCollectionPercentage),
      subtitle: `Mes anterior: ${formatPercent(kpis.previousMonthCollectionPercentage)} . ${periodContext}`,
      icon: <TrendingUp className="w-5 h-5 text-emerald-600" />,
      color: getCollectionColorClass(kpis.currentMonthCollectionPercentage)
    },
    {
      title: 'Cartera Vencida',
      value: formatCurrency(kpis.totalOverduePortfolio),
      subtitle: `1 mes: ${formatCurrency(kpis.overdueOneMonth)} . 2 meses: ${formatCurrency(kpis.overdueTwoMonths)} . 3+ meses: ${formatCurrency(kpis.overdueThreeOrMoreMonths)}`,
      icon: <AlertTriangle className="w-5 h-5 text-rose-600" />,
      color: 'text-rose-600'
    },
    {
      title: 'Ejecucion Presupuestal',
      value: formatPercent(kpis.budgetExecutionPercentage),
      subtitle: `Esperado a la fecha: ${formatPercent(kpis.budgetExpectedExecutionPercentage)}`,
      icon: <PiggyBank className="w-5 h-5 text-violet-600" />,
      color: getBudgetColorClass(kpis.budgetExecutionPercentage, kpis.budgetExpectedExecutionPercentage)
    },
    {
      title: 'PQR Abiertos',
      value: `${kpis.openPqrCount}`,
      subtitle: `${kpis.overduePqrCount} superaron el tiempo limite`,
      icon: <MessageSquare className="w-5 h-5 text-blue-600" />,
      color: 'text-blue-600'
    },
    {
      title: 'Alertas Activas',
      value: `${activeAlertCount}`,
      subtitle: 'Requieren atencion hoy',
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
          <option value="Critical">Critica</option>
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

function UpcomingEventsPanel({ events }: { events: UpcomingEventItem[] }) {
  const router = useRouter();

  return (
    <Card>
      <CardHeader>
        <div className="flex items-center justify-between">
          <h3 className="font-bold text-lg">Proximos Eventos (30 dias)</h3>
          <Calendar className="w-5 h-5 text-blue-500" />
        </div>
      </CardHeader>
      <CardContent className="p-0">
        {events.length === 0 && (
          <div className="p-6 text-center text-muted-foreground">
            <Calendar className="w-8 h-8 mx-auto mb-2 text-slate-300" />
            <p className="text-sm">No hay eventos proximos</p>
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
          <span className="flex items-center gap-1"><span className="w-3 h-3 rounded-sm bg-emerald-500 inline-block" /> Al dia</span>
          <span className="flex items-center gap-1"><span className="w-3 h-3 rounded-sm bg-yellow-400 inline-block" /> 1 mes</span>
          <span className="flex items-center gap-1"><span className="w-3 h-3 rounded-sm bg-orange-400 inline-block" /> 2 meses</span>
          <span className="flex items-center gap-1"><span className="w-3 h-3 rounded-sm bg-rose-500 inline-block" /> 3+ meses</span>
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
                        title={`${unit.identifier} - ${unit.ownerName}`}
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
