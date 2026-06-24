'use client';

import React, { useState, useEffect, useCallback } from 'react';
import { useRouter } from 'next/navigation';
import { Loader2, Check, X, Eye, Clock, AlertTriangle } from 'lucide-react';
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

export default function AdminTrayPage() {
  const router = useRouter();
  const [requestedReservations, setRequestedReservations] = useState<ReservationListItem[]>([]);
  const [activeReservations, setActiveReservations] = useState<ReservationListItem[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');
  const [actionLoading, setActionLoading] = useState<string | null>(null);

  const fetchReservations = useCallback(async () => {
    setLoading(true);
    setError('');
    try {
      const [requested, active] = await Promise.all([
        reservationService.getReservations({ status: 'Requested' }),
        reservationService.getReservations({ status: 'Approved' }),
      ]);
      setRequestedReservations(requested);
      setActiveReservations(active);
    } catch {
      setError('Error al cargar las reservas.');
    } finally {
      setLoading(false);
    }
  }, []);

  useEffect(() => { fetchReservations(); }, [fetchReservations]);

  const handleApprove = async (id: string) => {
    setActionLoading(id);
    try {
      await reservationService.approveReservation(id);
      fetchReservations();
    } catch {
      setError('Error al aprobar la reserva.');
    } finally {
      setActionLoading(null);
    }
  };

  const handleReject = async (id: string) => {
    const reason = prompt('Motivo del rechazo:');
    if (!reason) return;
    setActionLoading(id);
    try {
      await reservationService.rejectReservation(id, { rejectionReason: reason });
      fetchReservations();
    } catch {
      setError('Error al rechazar la reserva.');
    } finally {
      setActionLoading(null);
    }
  };

  const handleCheckIn = async (id: string) => {
    setActionLoading(id);
    try {
      await reservationService.checkIn(id);
      fetchReservations();
    } catch {
      setError('Error al registrar check-in.');
    } finally {
      setActionLoading(null);
    }
  };

  const handleCheckOut = async (id: string) => {
    setActionLoading(id);
    try {
      await reservationService.checkOut(id);
      fetchReservations();
    } catch {
      setError('Error al registrar check-out.');
    } finally {
      setActionLoading(null);
    }
  };

  const formatDateTime = (d: string) => {
    const date = new Date(d);
    return date.toLocaleDateString('es-CO', { day: '2-digit', month: '2-digit', year: 'numeric' }) +
      ' ' + date.toLocaleTimeString('es-CO', { hour: '2-digit', minute: '2-digit' });
  };

  return (
    <div className="space-y-6">
      <div>
        <h1 className="text-2xl font-bold text-foreground tracking-tight">Bandeja de Administración</h1>
        <p className="text-sm text-muted-foreground mt-1">Aprueba reservas pendientes y gestiona check-in/check-out.</p>
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
      ) : (
        <>
          <Card>
            <CardContent className="p-6">
              <div className="flex items-center gap-2 mb-4">
                <Clock className="w-5 h-5 text-yellow-600" />
                <h2 className="font-semibold text-foreground">Reservas Pendientes ({requestedReservations.length})</h2>
              </div>
              {requestedReservations.length === 0 ? (
                <p className="text-sm text-muted-foreground">No hay reservas pendientes de aprobación.</p>
              ) : (
                <div className="space-y-3">
                  {requestedReservations.map((reservation) => (
                    <div key={reservation.id} className="flex flex-col sm:flex-row justify-between items-start sm:items-center gap-3 p-4 bg-yellow-50 dark:bg-yellow-900/10 rounded-lg border border-yellow-200 dark:border-yellow-800">
                      <div className="flex-1">
                        <div className="flex items-center gap-2 mb-1">
                          <span className="font-bold text-foreground">{reservation.reservationNumber}</span>
                          <span className={statusBadgeClass[reservation.status]}>
                            {statusLabels[reservation.status]}
                          </span>
                        </div>
                        <div className="text-sm text-muted-foreground">
                          <span>{reservation.spaceName}</span> · <span>{reservation.unitIdentifier} - {reservation.ownerName}</span>
                        </div>
                        <div className="text-sm text-muted-foreground">
                          <Clock className="w-3 h-3 inline mr-1" />
                          {formatDateTime(reservation.startDateTime)} - {formatDateTime(reservation.endDateTime)}
                        </div>
                      </div>
                      <div className="flex gap-2">
                        <Button onClick={() => handleApprove(reservation.id)} disabled={actionLoading === reservation.id}>
                          <Check className="w-4 h-4 mr-1" /> Aprobar
                        </Button>
                        <Button variant="danger" onClick={() => handleReject(reservation.id)} disabled={actionLoading === reservation.id}>
                          <X className="w-4 h-4 mr-1" /> Rechazar
                        </Button>
                        <Button variant="ghost" onClick={() => router.push(`/reservation/${reservation.id}`)}>
                          <Eye className="w-4 h-4" />
                        </Button>
                      </div>
                    </div>
                  ))}
                </div>
              )}
            </CardContent>
          </Card>

          <Card>
            <CardContent className="p-6">
              <div className="flex items-center gap-2 mb-4">
                <AlertTriangle className="w-5 h-5 text-green-600" />
                <h2 className="font-semibold text-foreground">Reservas Aprobadas - Check-In/Out ({activeReservations.length})</h2>
              </div>
              {activeReservations.length === 0 ? (
                <p className="text-sm text-muted-foreground">No hay reservas aprobadas pendientes de check-in.</p>
              ) : (
                <div className="space-y-3">
                  {activeReservations.map((reservation) => (
                    <div key={reservation.id} className="flex flex-col sm:flex-row justify-between items-start sm:items-center gap-3 p-4 bg-green-50 dark:bg-green-900/10 rounded-lg border border-green-200 dark:border-green-800">
                      <div className="flex-1">
                        <div className="flex items-center gap-2 mb-1">
                          <span className="font-bold text-foreground">{reservation.reservationNumber}</span>
                          <span className={statusBadgeClass[reservation.status]}>
                            {statusLabels[reservation.status]}
                          </span>
                        </div>
                        <div className="text-sm text-muted-foreground">
                          <span>{reservation.spaceName}</span> · <span>{reservation.unitIdentifier} - {reservation.ownerName}</span>
                        </div>
                        <div className="text-sm text-muted-foreground">
                          <Clock className="w-3 h-3 inline mr-1" />
                          {formatDateTime(reservation.startDateTime)} - {formatDateTime(reservation.endDateTime)}
                        </div>
                      </div>
                      <div className="flex gap-2">
                        <Button onClick={() => handleCheckIn(reservation.id)} disabled={actionLoading === reservation.id}>
                          <Check className="w-4 h-4 mr-1" /> Check-In
                        </Button>
                        <Button variant="ghost" onClick={() => router.push(`/reservation/${reservation.id}`)}>
                          <Eye className="w-4 h-4" />
                        </Button>
                      </div>
                    </div>
                  ))}
                </div>
              )}
            </CardContent>
          </Card>
        </>
      )}
    </div>
  );
}
