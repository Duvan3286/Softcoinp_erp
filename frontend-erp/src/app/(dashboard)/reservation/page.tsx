'use client';

import React, { useState, useEffect, useCallback } from 'react';
import { useRouter } from 'next/navigation';
import { Loader2, Plus, Search, Eye, Calendar, MapPin, Users } from 'lucide-react';
import { Button } from '@/components/ui/Button';
import { Card, CardContent } from '@/components/ui/Card';
import reservationService, { ReservationListItem } from '@/lib/reservation-service';

const statusLabels: Record<string, string> = {
  Requested: 'Pendiente',
  Approved: 'Aprobada',
  InUse: 'En Uso',
  Completed: 'Completada',
  Cancelled: 'Cancelada',
  Rejected: 'Rechazada',
  WithIncident: 'Con Incidente',
};

const statusBadgeClass: Record<string, string> = {
  Requested: 'badge-warning',
  Approved: 'badge-success',
  InUse: 'badge-info',
  Completed: 'badge-neutral',
  Cancelled: 'badge-danger',
  Rejected: 'badge-danger',
  WithIncident: 'badge-warning',
};

const depositStatusLabels: Record<string, string> = {
  NotRequired: 'No Requerido',
  Pending: 'Pendiente',
  Paid: 'Pagado',
  Returned: 'Devuelto',
  AppliedToDamage: 'Aplicado a Daño',
};

export default function ReservationsPage() {
  const router = useRouter();
  const [reservations, setReservations] = useState<ReservationListItem[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');
  const [searchTerm, setSearchTerm] = useState('');
  const [statusFilter, setStatusFilter] = useState('');
  const [fromDate, setFromDate] = useState('');
  const [toDate, setToDate] = useState('');

  const fetchReservations = useCallback(async () => {
    setLoading(true);
    setError('');
    try {
      const data = await reservationService.getReservations({
        status: statusFilter || undefined,
        fromDate: fromDate || undefined,
        toDate: toDate || undefined,
      });
      setReservations(data);
    } catch {
      setError('Error al cargar las reservas.');
    } finally {
      setLoading(false);
    }
  }, [statusFilter, fromDate, toDate]);

  useEffect(() => { fetchReservations(); }, [fetchReservations]);
  useEffect(() => {
    const timer = setTimeout(() => { fetchReservations(); }, 400);
    return () => clearTimeout(timer);
  }, [searchTerm]);

  const formatCurrency = (val: number) =>
    new Intl.NumberFormat('es-CO', { style: 'currency', currency: 'COP', maximumFractionDigits: 0 }).format(val);

  const formatDateTime = (d: string) => {
    const date = new Date(d);
    return date.toLocaleDateString('es-CO', { day: '2-digit', month: '2-digit', year: 'numeric' }) +
      ' ' + date.toLocaleTimeString('es-CO', { hour: '2-digit', minute: '2-digit' });
  };

  const filteredReservations = reservations.filter(r =>
    r.reservationNumber.toLowerCase().includes(searchTerm.toLowerCase()) ||
    r.spaceName.toLowerCase().includes(searchTerm.toLowerCase()) ||
    r.unitIdentifier.toLowerCase().includes(searchTerm.toLowerCase()) ||
    r.ownerName.toLowerCase().includes(searchTerm.toLowerCase())
  );

  return (
    <div className="space-y-6">
      <div className="flex flex-col sm:flex-row justify-between items-start sm:items-center gap-4">
        <div>
          <h1 className="text-2xl font-bold text-foreground tracking-tight">Reservas</h1>
          <p className="text-sm text-muted-foreground mt-1">Gestiona las reservas de espacios comunes.</p>
        </div>
        <Button onClick={() => router.push('/reservation/new')}>
          <Plus className="w-4 h-4 mr-2" /> Nueva Reserva
        </Button>
      </div>

      <div className="flex flex-col sm:flex-row gap-3">
        <div className="relative flex-1">
          <Search className="absolute left-3 top-1/2 -translate-y-1/2 w-4 h-4 text-muted-foreground" />
          <input
            type="text"
            placeholder="Buscar por número, espacio, unidad o propietario..."
            value={searchTerm}
            onChange={(e) => setSearchTerm(e.target.value)}
            className="w-full pl-10 pr-4 py-2 border-b border-emerald-600/30 focus:border-emerald-600 text-sm font-medium outline-none"
          />
        </div>
        <select
          value={statusFilter}
          onChange={(e) => setStatusFilter(e.target.value)}
          className="border-b border-emerald-600/30 focus:border-emerald-600 text-sm font-medium py-2 outline-none"
        >
          <option value="">Todos los estados</option>
          <option value="Requested">Pendiente</option>
          <option value="Approved">Aprobada</option>
          <option value="InUse">En Uso</option>
          <option value="Completed">Completada</option>
          <option value="Cancelled">Cancelada</option>
          <option value="Rejected">Rechazada</option>
          <option value="WithIncident">Con Incidente</option>
        </select>
        <input
          type="date"
          value={fromDate}
          onChange={(e) => setFromDate(e.target.value)}
          className="border-b border-emerald-600/30 focus:border-emerald-600 text-sm font-medium py-2 outline-none"
          placeholder="Desde"
        />
        <input
          type="date"
          value={toDate}
          onChange={(e) => setToDate(e.target.value)}
          className="border-b border-emerald-600/30 focus:border-emerald-600 text-sm font-medium py-2 outline-none"
          placeholder="Hasta"
        />
      </div>

      {error && (
        <div className="bg-red-50 dark:bg-red-900/20 border border-red-200 dark:border-red-800 text-red-700 dark:text-red-300 px-4 py-3 rounded-lg text-sm">
          {error}
        </div>
      )}

      {loading ? (
        <div className="flex justify-center items-center py-12">
          <Loader2 className="w-8 h-8 animate-spin text-emerald-600" />
        </div>
      ) : filteredReservations.length === 0 ? (
        <Card>
          <CardContent className="p-12 text-center">
            <p className="text-muted-foreground">No se encontraron reservas.</p>
          </CardContent>
        </Card>
      ) : (
        <div className="space-y-3">
          {filteredReservations.map((reservation) => (
            <div key={reservation.id} className="hover:shadow-md transition-shadow cursor-pointer" onClick={() => router.push(`/reservation/${reservation.id}`)}>
              <Card>
              <CardContent className="p-4">
                <div className="flex flex-col sm:flex-row justify-between items-start sm:items-center gap-3">
                  <div className="flex-1">
                    <div className="flex items-center gap-3 mb-1">
                      <span className="font-bold text-foreground">{reservation.reservationNumber}</span>
                      <span className={statusBadgeClass[reservation.status] || 'badge-neutral'}>
                        {statusLabels[reservation.status] || reservation.status}
                      </span>
                      <span className="badge-info">{depositStatusLabels[reservation.depositStatus] || reservation.depositStatus}</span>
                    </div>
                    <div className="flex items-center gap-4 text-sm text-muted-foreground">
                      <div className="flex items-center gap-1">
                        <MapPin className="w-3 h-3" />
                        <span>{reservation.spaceName}</span>
                      </div>
                      <div className="flex items-center gap-1">
                        <Users className="w-3 h-3" />
                        <span>{reservation.unitIdentifier} - {reservation.ownerName}</span>
                      </div>
                      <div className="flex items-center gap-1">
                        <Calendar className="w-3 h-3" />
                        <span>{formatDateTime(reservation.startDateTime)} - {formatDateTime(reservation.endDateTime)}</span>
                      </div>
                    </div>
                  </div>
                  <div className="flex items-center gap-4">
                    <div className="text-right">
                      <p className="font-bold text-foreground">{formatCurrency(reservation.totalCost)}</p>
                      <p className="text-xs text-muted-foreground">{reservation.estimatedAttendees} asistentes</p>
                    </div>
                    <Button variant="ghost">
                      <Eye className="w-4 h-4" />
                    </Button>
                  </div>
                </div>
              </CardContent>
            </Card>
            </div>
          ))}
        </div>
      )}
    </div>
  );
}
