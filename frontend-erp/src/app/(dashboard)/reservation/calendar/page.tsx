'use client';

import React, { useState, useEffect } from 'react';
import { useRouter } from 'next/navigation';
import { Loader2, ChevronLeft, ChevronRight, Plus } from 'lucide-react';
import { Button } from '@/components/ui/Button';
import { Card, CardContent } from '@/components/ui/Card';
import reservationService, { ReservableSpaceListItem, CalendarEvent } from '@/lib/reservation-service';

const statusLabels: Record<string, string> = {
  Requested: 'Pendiente',
  Approved: 'Aprobada',
  InUse: 'En Uso',
  Completed: 'Completada',
  Cancelled: 'Cancelada',
  Rejected: 'Rechazada',
  WithIncident: 'Con Incidente',
};

const statusColors: Record<string, string> = {
  Requested: 'bg-yellow-100 text-yellow-800',
  Approved: 'bg-green-100 text-green-800',
  InUse: 'bg-blue-100 text-blue-800',
  Completed: 'bg-gray-100 text-gray-800',
  Cancelled: 'bg-red-100 text-red-800',
  Rejected: 'bg-red-100 text-red-800',
  WithIncident: 'bg-orange-100 text-orange-800',
};

export default function CalendarPage() {
  const router = useRouter();
  const [spaces, setSpaces] = useState<ReservableSpaceListItem[]>([]);
  const [selectedSpaceId, setSelectedSpaceId] = useState<string>('');
  const [events, setEvents] = useState<CalendarEvent[]>([]);
  const [loading, setLoading] = useState(true);
  const [currentDate, setCurrentDate] = useState(new Date());

  useEffect(() => {
    const fetchSpaces = async () => {
      try {
        const data = await reservationService.getSpaces(true);
        setSpaces(data);
        if (data.length > 0) setSelectedSpaceId(data[0].id);
      } catch {}
    };
    fetchSpaces();
  }, []);

  useEffect(() => {
    const fetchEvents = async () => {
      if (!selectedSpaceId) return;
      setLoading(true);
      try {
        const year = currentDate.getFullYear();
        const month = currentDate.getMonth();
        const monthStart = new Date(year, month, 1).toISOString();
        const monthEnd = new Date(year, month + 1, 0, 23, 59, 59).toISOString();
        const data = await reservationService.getCalendarEvents(selectedSpaceId, monthStart, monthEnd);
        setEvents(data);
      } catch {
        setEvents([]);
      } finally {
        setLoading(false);
      }
    };
    fetchEvents();
  }, [selectedSpaceId, currentDate]);

  const year = currentDate.getFullYear();
  const month = currentDate.getMonth();
  const firstDay = new Date(year, month, 1);
  const lastDay = new Date(year, month + 1, 0);
  const daysInMonth = lastDay.getDate();
  const startDayOfWeek = firstDay.getDay();
  const monthName = currentDate.toLocaleDateString('es-CO', { month: 'long', year: 'numeric' });

  const days = [];
  for (let i = 0; i < startDayOfWeek; i++) {
    days.push(null);
  }
  for (let i = 1; i <= daysInMonth; i++) {
    days.push(i);
  }

  const getEventsForDay = (day: number) => {
    const dateStr = new Date(year, month, day).toISOString().split('T')[0];
    return events.filter(e => {
      const eventDate = new Date(e.startDateTime).toISOString().split('T')[0];
      return eventDate === dateStr;
    });
  };

  const prevMonth = () => setCurrentDate(new Date(year, month - 1, 1));
  const nextMonth = () => setCurrentDate(new Date(year, month + 1, 1));

  return (
    <div className="space-y-6">
      <div className="flex flex-col sm:flex-row justify-between items-start sm:items-center gap-4">
        <div>
          <h1 className="text-2xl font-bold text-foreground tracking-tight">Calendario de Reservas</h1>
          <p className="text-sm text-muted-foreground mt-1">Visualiza las reservas del mes.</p>
        </div>
        <Button onClick={() => router.push('/reservation/new')}>
          <Plus className="w-4 h-4 mr-2" /> Nueva Reserva
        </Button>
      </div>

      <div className="flex items-center gap-4">
        <select
          value={selectedSpaceId}
          onChange={(e) => setSelectedSpaceId(e.target.value)}
          className="border-b border-emerald-600/30 focus:border-emerald-600 text-sm font-medium py-2 outline-none min-w-[200px]"
        >
          {spaces.map((space) => (
            <option key={space.id} value={space.id}>{space.name}</option>
          ))}
        </select>
      </div>

      <Card>
        <CardContent className="p-6">
          <div className="flex items-center justify-between mb-6">
            <Button variant="ghost" onClick={prevMonth}>
              <ChevronLeft className="w-5 h-5" />
            </Button>
            <h2 className="text-lg font-bold text-foreground capitalize">{monthName}</h2>
            <Button variant="ghost" onClick={nextMonth}>
              <ChevronRight className="w-5 h-5" />
            </Button>
          </div>

          {loading ? (
            <div className="flex justify-center py-12">
              <Loader2 className="w-8 h-8 animate-spin text-emerald-600" />
            </div>
          ) : (
            <div className="grid grid-cols-7 gap-1">
              {['Dom', 'Lun', 'Mar', 'Mié', 'Jue', 'Vie', 'Sáb'].map((day) => (
                <div key={day} className="text-center text-xs font-semibold text-muted-foreground py-2">
                  {day}
                </div>
              ))}
              {days.map((day, idx) => (
                <div
                  key={idx}
                  className={`min-h-[80px] p-1 border border-border rounded ${
                    day ? 'bg-background' : 'bg-muted/30'
                  }`}
                >
                  {day && (
                    <>
                      <div className="text-xs font-medium text-muted-foreground mb-1">{day}</div>
                      <div className="space-y-1">
                        {getEventsForDay(day).map((event) => (
                          <div
                            key={event.reservationId}
                            className="text-[10px] p-1 rounded truncate cursor-pointer hover:opacity-80"
                            style={{ backgroundColor: event.color + '20', color: event.color }}
                            onClick={() => router.push(`/reservation/${event.reservationId}`)}
                          >
                            {event.spaceName} - {event.unitIdentifier}
                          </div>
                        ))}
                      </div>
                    </>
                  )}
                </div>
              ))}
            </div>
          )}
        </CardContent>
      </Card>

      <div className="flex flex-wrap gap-4 text-xs">
        {Object.entries(statusLabels).map(([status, label]) => (
          <div key={status} className="flex items-center gap-2">
            <div
              className="w-3 h-3 rounded"
              style={{
                backgroundColor:
                  status === 'Requested' ? '#F59E0B' :
                  status === 'Approved' ? '#10B981' :
                  status === 'InUse' ? '#3B82F6' :
                  status === 'Completed' ? '#6B7280' :
                  status === 'Cancelled' || status === 'Rejected' ? '#EF4444' :
                  '#F97316',
              }}
            />
            <span>{label}</span>
          </div>
        ))}
      </div>
    </div>
  );
}
