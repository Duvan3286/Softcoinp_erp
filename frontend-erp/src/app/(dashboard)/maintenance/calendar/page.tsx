'use client';

import React, { useState, useEffect } from 'react';
import { Loader2, AlertTriangle, ChevronLeft, ChevronRight } from 'lucide-react';
import { Card, CardContent } from '@/components/ui/Card';
import maintenanceService, { MaintenanceReport } from '@/lib/maintenance-service';

const months = ['Enero', 'Febrero', 'Marzo', 'Abril', 'Mayo', 'Junio', 'Julio', 'Agosto', 'Septiembre', 'Octubre', 'Noviembre', 'Diciembre'];
const dayNames = ['Dom', 'Lun', 'Mar', 'Mie', 'Jue', 'Vie', 'Sab'];

export default function MaintenanceCalendarPage() {
  const [report, setReport] = useState<MaintenanceReport | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');
  const [currentMonth, setCurrentMonth] = useState(() => new Date().getMonth());
  const [currentYear, setCurrentYear] = useState(() => new Date().getFullYear());

  const fetchData = async () => {
    setLoading(true);
    setError('');
    try {
      const data = await maintenanceService.getScheduledReport(90);
      setReport(data);
    } catch {
      setError('Error al cargar el calendario de mantenimientos.');
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => { fetchData(); }, []);

  const getScheduledForDay = (day: number) => {
    if (!report) return [];
    const dateStr = `${currentYear}-${String(currentMonth + 1).padStart(2, '0')}-${String(day).padStart(2, '0')}`;
    return report.scheduledItems.filter((item) => item.scheduledDate.startsWith(dateStr));
  };

  const daysInMonth = new Date(currentYear, currentMonth + 1, 0).getDate();
  const firstDayOfWeek = new Date(currentYear, currentMonth, 1).getDay();
  const prevMonth = () => { if (currentMonth === 0) { setCurrentMonth(11); setCurrentYear(currentYear - 1); } else { setCurrentMonth(currentMonth - 1); } };
  const nextMonth = () => { if (currentMonth === 11) { setCurrentMonth(0); setCurrentYear(currentYear + 1); } else { setCurrentMonth(currentMonth + 1); } };

  const formatCurrency = (val: number) =>
    new Intl.NumberFormat('es-CO', { style: 'currency', currency: 'COP', maximumFractionDigits: 0 }).format(val);

  if (loading) return <div className="flex justify-center py-20"><Loader2 className="w-8 h-8 animate-spin text-emerald-600" /></div>;
  if (error) return (
    <div className="flex flex-col items-center gap-3 text-rose-600 py-12">
      <AlertTriangle className="w-10 h-10" />
      <p className="font-semibold">{error}</p>
    </div>
  );

  return (
    <div className="space-y-6">
      <div>
        <h1 className="text-2xl font-bold text-foreground tracking-tight">Calendario de Mantenimientos</h1>
        <p className="text-sm text-muted-foreground mt-1">Mantenimientos preventivos programados para los proximos 90 dias.</p>
      </div>

      <Card>
        <CardContent className="p-6">
          <div className="flex items-center justify-between mb-6">
            <button onClick={prevMonth} className="p-2 hover:bg-muted rounded-lg transition-colors">
              <ChevronLeft className="w-5 h-5" />
            </button>
            <h2 className="text-lg font-bold text-foreground">{months[currentMonth]} {currentYear}</h2>
            <button onClick={nextMonth} className="p-2 hover:bg-muted rounded-lg transition-colors">
              <ChevronRight className="w-5 h-5" />
            </button>
          </div>

          <div className="grid grid-cols-7 gap-px bg-border rounded-lg overflow-hidden">
            {dayNames.map((d) => (
              <div key={d} className="bg-muted/50 px-3 py-2 text-xs font-bold text-muted-foreground uppercase text-center">
                {d}
              </div>
            ))}
            {Array.from({ length: firstDayOfWeek }).map((_, i) => (
              <div key={`empty-${i}`} className="bg-card min-h-[100px] p-2" />
            ))}
            {Array.from({ length: daysInMonth }).map((_, i) => {
              const day = i + 1;
              const items = getScheduledForDay(day);
              const isToday = day === new Date().getDate() && currentMonth === new Date().getMonth() && currentYear === new Date().getFullYear();
              return (
                <div key={day} className={`bg-card min-h-[100px] p-2 border-b border-r border-border/50 ${isToday ? 'ring-2 ring-emerald-500 ring-inset' : ''}`}>
                  <p className={`text-xs font-bold mb-1 ${isToday ? 'text-emerald-600' : 'text-muted-foreground'}`}>{day}</p>
                  {items.map((item, idx) => (
                    <div key={idx} className="text-[10px] bg-emerald-50 text-emerald-700 rounded px-1 py-0.5 mb-0.5 truncate leading-tight">
                      {item.assetName}
                    </div>
                  ))}
                </div>
              );
            })}
          </div>
        </CardContent>
      </Card>

      {report && report.scheduledItems.length > 0 && (
        <Card>
          <CardContent className="p-6">
            <h3 className="text-sm font-bold text-foreground mb-4">Proximos Mantenimientos</h3>
            <div className="overflow-x-auto">
              <table className="min-w-full divide-y divide-border">
                <thead className="bg-muted/50">
                  <tr>
                    <th className="px-4 py-2 text-left text-xs font-bold text-muted-foreground uppercase">Fecha</th>
                    <th className="px-4 py-2 text-left text-xs font-bold text-muted-foreground uppercase">Bien</th>
                    <th className="px-4 py-2 text-left text-xs font-bold text-muted-foreground uppercase">Actividad</th>
                    <th className="px-4 py-2 text-right text-xs font-bold text-muted-foreground uppercase">Costo Est.</th>
                  </tr>
                </thead>
                <tbody className="divide-y divide-border">
                  {report.scheduledItems.map((item, idx) => (
                    <tr key={idx} className="hover:bg-muted/30">
                      <td className="px-4 py-2 text-sm">{new Date(item.scheduledDate).toLocaleDateString('es-CO', { day: '2-digit', month: '2-digit', year: 'numeric' })}</td>
                      <td className="px-4 py-2 text-sm font-medium">{item.assetName}</td>
                      <td className="px-4 py-2 text-sm text-muted-foreground">{item.activityType}</td>
                      <td className="px-4 py-2 text-sm text-right font-medium">{formatCurrency(item.estimatedCost)}</td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>
          </CardContent>
        </Card>
      )}
    </div>
  );
}
