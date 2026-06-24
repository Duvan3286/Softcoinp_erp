'use client';

import React, { useState, useEffect } from 'react';
import { useRouter, useParams } from 'next/navigation';
import { Loader2, ArrowLeft, Check, X, LogIn, LogOut, AlertTriangle, DollarSign, RotateCcw } from 'lucide-react';
import { Button } from '@/components/ui/Button';
import { Card, CardContent } from '@/components/ui/Card';
import reservationService, { ReservationDetail } from '@/lib/reservation-service';

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

const severityLabels: Record<string, string> = {
  Minor: 'Menor',
  Moderate: 'Moderado',
  Severe: 'Grave',
  Critical: 'Crítico',
};

export default function ReservationDetailPage() {
  const router = useRouter();
  const params = useParams();
  const reservationId = params.id as string;
  const [reservation, setReservation] = useState<ReservationDetail | null>(null);
  const [loading, setLoading] = useState(true);
  const [actionLoading, setActionLoading] = useState(false);
  const [error, setError] = useState('');
  const [showRejectModal, setShowRejectModal] = useState(false);
  const [rejectionReason, setRejectionReason] = useState('');
  const [showIncidentModal, setShowIncidentModal] = useState(false);
  const [incidentForm, setIncidentForm] = useState({ description: '', severity: 'Minor', damageAmount: 0 });
  const [showDepositModal, setShowDepositModal] = useState(false);
  const [depositAction, setDepositAction] = useState<'pay' | 'return' | 'damage'>('pay');

  const fetchReservation = async () => {
    setLoading(true);
    setError('');
    try {
      const data = await reservationService.getReservation(reservationId);
      setReservation(data);
    } catch {
      setError('Error al cargar la reserva.');
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => { fetchReservation(); }, [reservationId]);

  const handleApprove = async () => {
    setActionLoading(true);
    try {
      await reservationService.approveReservation(reservationId);
      fetchReservation();
    } catch {
      setError('Error al aprobar la reserva.');
    } finally {
      setActionLoading(false);
    }
  };

  const handleReject = async () => {
    if (!rejectionReason) return;
    setActionLoading(true);
    try {
      await reservationService.rejectReservation(reservationId, { rejectionReason });
      setShowRejectModal(false);
      setRejectionReason('');
      fetchReservation();
    } catch {
      setError('Error al rechazar la reserva.');
    } finally {
      setActionLoading(false);
    }
  };

  const handleCancel = async () => {
    if (!confirm('¿Estás seguro de cancelar esta reserva?')) return;
    setActionLoading(true);
    try {
      await reservationService.cancelReservation(reservationId);
      fetchReservation();
    } catch {
      setError('Error al cancelar la reserva.');
    } finally {
      setActionLoading(false);
    }
  };

  const handleCheckIn = async () => {
    setActionLoading(true);
    try {
      await reservationService.checkIn(reservationId);
      fetchReservation();
    } catch {
      setError('Error al registrar check-in.');
    } finally {
      setActionLoading(false);
    }
  };

  const handleCheckOut = async () => {
    setActionLoading(true);
    try {
      await reservationService.checkOut(reservationId);
      fetchReservation();
    } catch {
      setError('Error al registrar check-out.');
    } finally {
      setActionLoading(false);
    }
  };

  const handleReportIncident = async (e: React.FormEvent) => {
    e.preventDefault();
    setActionLoading(true);
    try {
      await reservationService.reportIncident(reservationId, incidentForm);
      setShowIncidentModal(false);
      setIncidentForm({ description: '', severity: 'Minor', damageAmount: 0 });
      fetchReservation();
    } catch {
      setError('Error al reportar incidente.');
    } finally {
      setActionLoading(false);
    }
  };

  const handleDepositAction = async (e: React.FormEvent) => {
    e.preventDefault();
    setActionLoading(true);
    try {
      if (depositAction === 'pay') {
        await reservationService.processDepositPayment(reservationId);
      } else if (depositAction === 'return') {
        await reservationService.processDepositReturn(reservationId);
      } else {
        await reservationService.applyDepositToDamage(reservationId, {
          damageAmount: incidentForm.damageAmount,
          damageDescription: incidentForm.description,
        });
      }
      setShowDepositModal(false);
      fetchReservation();
    } catch {
      setError('Error al procesar el depósito.');
    } finally {
      setActionLoading(false);
    }
  };

  const formatCurrency = (val: number) =>
    new Intl.NumberFormat('es-CO', { style: 'currency', currency: 'COP', maximumFractionDigits: 0 }).format(val);

  const formatDateTime = (d: string) => {
    const date = new Date(d);
    return date.toLocaleDateString('es-CO', { day: '2-digit', month: '2-digit', year: 'numeric' }) +
      ' ' + date.toLocaleTimeString('es-CO', { hour: '2-digit', minute: '2-digit' });
  };

  if (loading) {
    return (
      <div className="flex justify-center items-center py-12">
        <Loader2 className="w-8 h-8 animate-spin text-emerald-600" />
      </div>
    );
  }

  if (!reservation) {
    return (
      <div className="text-center py-12">
        <p className="text-muted-foreground">Reserva no encontrada.</p>
        <Button onClick={() => router.back()} className="mt-4">Volver</Button>
      </div>
    );
  }

  return (
    <div className="space-y-6">
      <div className="flex items-center gap-4">
        <Button variant="ghost" onClick={() => router.back()}>
          <ArrowLeft className="w-4 h-4 mr-2" /> Volver
        </Button>
        <div>
          <h1 className="text-2xl font-bold text-foreground tracking-tight">{reservation.reservationNumber}</h1>
          <p className="text-sm text-muted-foreground mt-1">{reservation.spaceName}</p>
        </div>
        <span className={statusBadgeClass[reservation.status] || 'badge-neutral'}>
          {statusLabels[reservation.status] || reservation.status}
        </span>
      </div>

      {error && (
        <div className="bg-red-50 dark:bg-red-900/20 border border-red-200 dark:border-red-800 text-red-700 dark:text-red-300 px-4 py-3 rounded-lg text-sm">
          {error}
        </div>
      )}

      <div className="flex flex-wrap gap-3">
        {reservation.status === 'Requested' && (
          <>
            <Button onClick={handleApprove} disabled={actionLoading}>
              <Check className="w-4 h-4 mr-2" /> Aprobar
            </Button>
            <Button variant="danger" onClick={() => setShowRejectModal(true)} disabled={actionLoading}>
              <X className="w-4 h-4 mr-2" /> Rechazar
            </Button>
          </>
        )}
        {reservation.status === 'Approved' && (
          <>
            <Button onClick={handleCheckIn} disabled={actionLoading}>
              <LogIn className="w-4 h-4 mr-2" /> Check-In
            </Button>
            <Button variant="danger" onClick={handleCancel} disabled={actionLoading}>
              <X className="w-4 h-4 mr-2" /> Cancelar
            </Button>
          </>
        )}
        {reservation.status === 'InUse' && (
          <>
            <Button onClick={handleCheckOut} disabled={actionLoading}>
              <LogOut className="w-4 h-4 mr-2" /> Check-Out
            </Button>
            <Button variant="danger" onClick={() => setShowIncidentModal(true)} disabled={actionLoading}>
              <AlertTriangle className="w-4 h-4 mr-2" /> Reportar Incidente
            </Button>
          </>
        )}
        {reservation.depositStatus === 'Pending' && (
          <Button variant="secondary" onClick={() => { setDepositAction('pay'); setShowDepositModal(true); }}>
            <DollarSign className="w-4 h-4 mr-2" /> Registrar Pago Depósito
          </Button>
        )}
        {reservation.depositStatus === 'Paid' && (
          <>
            <Button variant="secondary" onClick={() => { setDepositAction('return'); setShowDepositModal(true); }}>
              <RotateCcw className="w-4 h-4 mr-2" /> Devolver Depósito
            </Button>
            <Button variant="danger" onClick={() => { setDepositAction('damage'); setShowDepositModal(true); }}>
              <AlertTriangle className="w-4 h-4 mr-2" /> Aplicar a Daño
            </Button>
          </>
        )}
      </div>

      <div className="grid grid-cols-1 lg:grid-cols-2 gap-6">
        <Card>
          <CardContent className="p-6 space-y-4">
            <h3 className="font-semibold text-foreground border-b border-emerald-600/30 pb-2">Detalles de la Reserva</h3>
            <div className="grid grid-cols-2 gap-4 text-sm">
              <div>
                <span className="text-muted-foreground">Espacio:</span>
                <p className="font-medium">{reservation.spaceName}</p>
              </div>
              <div>
                <span className="text-muted-foreground">Unidad:</span>
                <p className="font-medium">{reservation.unitIdentifier}</p>
              </div>
              <div>
                <span className="text-muted-foreground">Propietario:</span>
                <p className="font-medium">{reservation.ownerName}</p>
              </div>
              <div>
                <span className="text-muted-foreground">Email:</span>
                <p className="font-medium">{reservation.ownerEmail}</p>
              </div>
              <div>
                <span className="text-muted-foreground">Inicio:</span>
                <p className="font-medium">{formatDateTime(reservation.startDateTime)}</p>
              </div>
              <div>
                <span className="text-muted-foreground">Fin:</span>
                <p className="font-medium">{formatDateTime(reservation.endDateTime)}</p>
              </div>
              <div>
                <span className="text-muted-foreground">Asistentes:</span>
                <p className="font-medium">{reservation.estimatedAttendees}</p>
              </div>
              <div>
                <span className="text-muted-foreground">Música:</span>
                <p className="font-medium">{reservation.hasMusic ? `Sí (hasta ${reservation.musicEndTime})` : 'No'}</p>
              </div>
            </div>
            {reservation.eventDescription && (
              <div>
                <span className="text-muted-foreground text-sm">Descripción:</span>
                <p className="text-sm mt-1">{reservation.eventDescription}</p>
              </div>
            )}
          </CardContent>
        </Card>

        <Card>
          <CardContent className="p-6 space-y-4">
            <h3 className="font-semibold text-foreground border-b border-emerald-600/30 pb-2">Costos</h3>
            <div className="grid grid-cols-2 gap-4 text-sm">
              <div>
                <span className="text-muted-foreground">Costo Total:</span>
                <p className="font-bold text-foreground text-lg">{formatCurrency(reservation.totalCost)}</p>
              </div>
              <div>
                <span className="text-muted-foreground">Estado Depósito:</span>
                <p className="font-medium">{depositStatusLabels[reservation.depositStatus]}</p>
              </div>
              {reservation.depositAmount > 0 && (
                <div>
                  <span className="text-muted-foreground">Monto Depósito:</span>
                  <p className="font-medium">{formatCurrency(reservation.depositAmount)}</p>
                </div>
              )}
            </div>
            {reservation.adminNotes && (
              <div>
                <span className="text-muted-foreground text-sm">Notas Admin:</span>
                <p className="text-sm mt-1">{reservation.adminNotes}</p>
              </div>
            )}
          </CardContent>
        </Card>
      </div>

      {reservation.deposits.length > 0 && (
        <Card>
          <CardContent className="p-6 space-y-4">
            <h3 className="font-semibold text-foreground border-b border-emerald-600/30 pb-2">Depósitos</h3>
            <div className="space-y-2">
              {reservation.deposits.map((deposit) => (
                <div key={deposit.id} className="flex justify-between items-center p-3 bg-muted/50 rounded text-sm">
                  <div>
                    <span className="font-medium">{formatCurrency(deposit.amount)}</span>
                    <span className="ml-2 text-muted-foreground">- {depositStatusLabels[deposit.status]}</span>
                    {deposit.paymentMethod && <span className="ml-2 text-muted-foreground">({deposit.paymentMethod})</span>}
                  </div>
                  <span className="text-muted-foreground text-xs">
                    {deposit.paidAt && `Pagado: ${formatDateTime(deposit.paidAt)}`}
                    {deposit.returnedAt && `Devuelto: ${formatDateTime(deposit.returnedAt)}`}
                  </span>
                </div>
              ))}
            </div>
          </CardContent>
        </Card>
      )}

      {reservation.incidents.length > 0 && (
        <Card>
          <CardContent className="p-6 space-y-4">
            <h3 className="font-semibold text-foreground border-b border-emerald-600/30 pb-2">Incidentes</h3>
            <div className="space-y-2">
              {reservation.incidents.map((incident) => (
                <div key={incident.id} className="p-3 bg-muted/50 rounded text-sm">
                  <div className="flex justify-between items-start">
                    <div>
                      <span className="font-medium">{incident.description}</span>
                      <span className="ml-2 badge-warning">{severityLabels[incident.severity]}</span>
                    </div>
                    <span className="text-muted-foreground text-xs">{formatDateTime(incident.createdAt)}</span>
                  </div>
                  {incident.damageAmount > 0 && (
                    <p className="text-sm mt-1">Daño: {formatCurrency(incident.damageAmount)}</p>
                  )}
                  <p className="text-xs text-muted-foreground mt-1">Reportado por: {incident.reportedByName}</p>
                </div>
              ))}
            </div>
          </CardContent>
        </Card>
      )}

      {showRejectModal && (
        <div className="fixed inset-0 bg-black/50 flex items-center justify-center z-50">
          <Card className="w-full max-w-md">
            <CardContent className="p-6">
              <h3 className="font-semibold text-foreground mb-4">Rechazar Reserva</h3>
              <div>
                <label className="block text-sm font-medium text-foreground mb-1">Motivo del Rechazo *</label>
                <textarea
                  required
                  value={rejectionReason}
                  onChange={(e) => setRejectionReason(e.target.value)}
                  className="w-full border-b border-emerald-600/30 focus:border-emerald-600 text-sm font-medium py-2 outline-none"
                  rows={3}
                  placeholder="Describe el motivo del rechazo"
                />
              </div>
              <div className="flex justify-end gap-3 pt-4 border-t border-border">
                <Button variant="ghost" onClick={() => setShowRejectModal(false)}>Cancelar</Button>
                <Button variant="danger" onClick={handleReject} disabled={!rejectionReason || actionLoading}>
                  Rechazar
                </Button>
              </div>
            </CardContent>
          </Card>
        </div>
      )}

      {showIncidentModal && (
        <div className="fixed inset-0 bg-black/50 flex items-center justify-center z-50">
          <Card className="w-full max-w-md">
            <CardContent className="p-6">
              <h3 className="font-semibold text-foreground mb-4">Reportar Incidente</h3>
              <form onSubmit={handleReportIncident} className="space-y-4">
                <div>
                  <label className="block text-sm font-medium text-foreground mb-1">Descripción *</label>
                  <textarea
                    required
                    value={incidentForm.description}
                    onChange={(e) => setIncidentForm({ ...incidentForm, description: e.target.value })}
                    className="w-full border-b border-emerald-600/30 focus:border-emerald-600 text-sm font-medium py-2 outline-none"
                    rows={3}
                    placeholder="Describe el incidente"
                  />
                </div>
                <div>
                  <label className="block text-sm font-medium text-foreground mb-1">Severidad</label>
                  <select
                    value={incidentForm.severity}
                    onChange={(e) => setIncidentForm({ ...incidentForm, severity: e.target.value })}
                    className="w-full border-b border-emerald-600/30 focus:border-emerald-600 text-sm font-medium py-2 outline-none"
                  >
                    <option value="Minor">Menor</option>
                    <option value="Moderate">Moderado</option>
                    <option value="Severe">Grave</option>
                    <option value="Critical">Crítico</option>
                  </select>
                </div>
                <div>
                  <label className="block text-sm font-medium text-foreground mb-1">Monto del Daño</label>
                  <input
                    type="number"
                    min="0"
                    value={incidentForm.damageAmount}
                    onChange={(e) => setIncidentForm({ ...incidentForm, damageAmount: parseFloat(e.target.value) || 0 })}
                    className="w-full border-b border-emerald-600/30 focus:border-emerald-600 text-sm font-medium py-2 outline-none"
                  />
                </div>
                <div className="flex justify-end gap-3 pt-4 border-t border-border">
                  <Button type="button" variant="ghost" onClick={() => setShowIncidentModal(false)}>Cancelar</Button>
                  <Button type="submit" disabled={actionLoading}>Reportar</Button>
                </div>
              </form>
            </CardContent>
          </Card>
        </div>
      )}

      {showDepositModal && (
        <div className="fixed inset-0 bg-black/50 flex items-center justify-center z-50">
          <Card className="w-full max-w-md">
            <CardContent className="p-6">
              <h3 className="font-semibold text-foreground mb-4">
                {depositAction === 'pay' ? 'Registrar Pago de Depósito' :
                 depositAction === 'return' ? 'Devolver Depósito' : 'Aplicar Depósito a Daño'}
              </h3>
              <form onSubmit={handleDepositAction} className="space-y-4">
                {depositAction === 'damage' && (
                  <>
                    <div>
                      <label className="block text-sm font-medium text-foreground mb-1">Monto del Daño *</label>
                      <input
                        type="number"
                        required
                        min="0"
                        value={incidentForm.damageAmount}
                        onChange={(e) => setIncidentForm({ ...incidentForm, damageAmount: parseFloat(e.target.value) || 0 })}
                        className="w-full border-b border-emerald-600/30 focus:border-emerald-600 text-sm font-medium py-2 outline-none"
                      />
                    </div>
                    <div>
                      <label className="block text-sm font-medium text-foreground mb-1">Descripción del Daño *</label>
                      <textarea
                        required
                        value={incidentForm.description}
                        onChange={(e) => setIncidentForm({ ...incidentForm, description: e.target.value })}
                        className="w-full border-b border-emerald-600/30 focus:border-emerald-600 text-sm font-medium py-2 outline-none"
                        rows={3}
                      />
                    </div>
                  </>
                )}
                <div className="flex justify-end gap-3 pt-4 border-t border-border">
                  <Button type="button" variant="ghost" onClick={() => setShowDepositModal(false)}>Cancelar</Button>
                  <Button type="submit" disabled={actionLoading}>
                    {depositAction === 'pay' ? 'Registrar Pago' :
                     depositAction === 'return' ? 'Devolver' : 'Aplicar'}
                  </Button>
                </div>
              </form>
            </CardContent>
          </Card>
        </div>
      )}
    </div>
  );
}
