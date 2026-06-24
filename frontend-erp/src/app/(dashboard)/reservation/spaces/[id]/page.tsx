'use client';

import React, { useState, useEffect } from 'react';
import { useRouter, useParams } from 'next/navigation';
import { Loader2, Save, ArrowLeft, Plus, Trash2, Clock } from 'lucide-react';
import { Button } from '@/components/ui/Button';
import { Card, CardContent } from '@/components/ui/Card';
import reservationService, { ReservableSpaceDetail, SpaceSchedule, CreateSpaceScheduleRequest } from '@/lib/reservation-service';

const dayNames = ['Domingo', 'Lunes', 'Martes', 'Miércoles', 'Jueves', 'Viernes', 'Sábado'];

const approvalModeLabels: Record<string, string> = {
  Automatic: 'Automática',
  Manual: 'Manual',
};

const arrearsPolicyLabels: Record<string, string> = {
  Block: 'Bloquear',
  Warn: 'Advertir',
};

const chargeTypeLabels: Record<string, string> = {
  PerHour: 'Por Hora',
  PerEvent: 'Por Evento',
  Other: 'Otro',
};

export default function SpaceDetailPage() {
  const router = useRouter();
  const params = useParams();
  const spaceId = params.id as string;
  const [space, setSpace] = useState<ReservableSpaceDetail | null>(null);
  const [loading, setLoading] = useState(true);
  const [saving, setSaving] = useState(false);
  const [error, setError] = useState('');
  const [showScheduleForm, setShowScheduleForm] = useState(false);
  const [scheduleForm, setScheduleForm] = useState<CreateSpaceScheduleRequest>({
    dayOfWeek: 1,
    startTime: '08:00',
    endTime: '17:00',
  });

  const fetchSpace = async () => {
    setLoading(true);
    setError('');
    try {
      const data = await reservationService.getSpace(spaceId);
      setSpace(data);
    } catch {
      setError('Error al cargar el espacio.');
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => { fetchSpace(); }, [spaceId]);

  const handleAddSchedule = async (e: React.FormEvent) => {
    e.preventDefault();
    try {
      await reservationService.createSchedule(spaceId, scheduleForm);
      setShowScheduleForm(false);
      fetchSpace();
    } catch (err: unknown) {
      const message = err instanceof Error ? err.message : 'Error al agregar horario.';
      setError(message);
    }
  };

  const handleDeleteSchedule = async (scheduleId: string) => {
    if (!confirm('¿Estás seguro de eliminar este horario?')) return;
    try {
      await reservationService.deleteSchedule(scheduleId);
      fetchSpace();
    } catch {
      setError('Error al eliminar el horario.');
    }
  };

  const formatCurrency = (val: number) =>
    new Intl.NumberFormat('es-CO', { style: 'currency', currency: 'COP', maximumFractionDigits: 0 }).format(val);

  if (loading) {
    return (
      <div className="flex justify-center items-center py-12">
        <Loader2 className="w-8 h-8 animate-spin text-emerald-600" />
      </div>
    );
  }

  if (!space) {
    return (
      <div className="text-center py-12">
        <p className="text-muted-foreground">Espacio no encontrado.</p>
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
          <h1 className="text-2xl font-bold text-foreground tracking-tight">{space.name}</h1>
          <p className="text-sm text-muted-foreground mt-1">{space.location || 'Sin ubicación'}</p>
        </div>
      </div>

      {error && (
        <div className="bg-red-50 dark:bg-red-900/20 border border-red-200 dark:border-red-800 text-red-700 dark:text-red-300 px-4 py-3 rounded-lg text-sm">
          {error}
        </div>
      )}

      <div className="grid grid-cols-1 lg:grid-cols-2 gap-6">
        <Card>
          <CardContent className="p-6 space-y-4">
            <h3 className="font-semibold text-foreground border-b border-emerald-600/30 pb-2">Información General</h3>
            <div className="grid grid-cols-2 gap-4 text-sm">
              <div>
                <span className="text-muted-foreground">Capacidad:</span>
                <p className="font-medium">{space.maxCapacity} personas</p>
              </div>
              <div>
                <span className="text-muted-foreground">Estado:</span>
                <p><span className={space.isActive ? 'badge-success' : 'badge-neutral'}>{space.isActive ? 'Activo' : 'Inactivo'}</span></p>
              </div>
              <div>
                <span className="text-muted-foreground">Aprobación:</span>
                <p className="font-medium">{approvalModeLabels[space.approvalMode] || space.approvalMode}</p>
              </div>
              <div>
                <span className="text-muted-foreground">Política Mora:</span>
                <p className="font-medium">{arrearsPolicyLabels[space.arrearsPolicy] || space.arrearsPolicy}</p>
              </div>
            </div>
          </CardContent>
        </Card>

        <Card>
          <CardContent className="p-6 space-y-4">
            <h3 className="font-semibold text-foreground border-b border-emerald-600/30 pb-2">Costos</h3>
            <div className="grid grid-cols-2 gap-4 text-sm">
              <div>
                <span className="text-muted-foreground">Cobro Adicional:</span>
                <p className="font-medium">{space.hasAdditionalCost ? 'Sí' : 'No'}</p>
              </div>
              {space.hasAdditionalCost && (
                <div>
                  <span className="text-muted-foreground">Tipo:</span>
                  <p className="font-medium">{chargeTypeLabels[space.chargeType] || space.chargeType}</p>
                </div>
              )}
              {space.hasAdditionalCost && space.chargeType === 'PerHour' && (
                <div>
                  <span className="text-muted-foreground">Tarifa/Hora:</span>
                  <p className="font-medium">{formatCurrency(space.hourlyRate)}</p>
                </div>
              )}
              {space.hasAdditionalCost && space.chargeType === 'PerEvent' && (
                <div>
                  <span className="text-muted-foreground">Tarifa/Evento:</span>
                  <p className="font-medium">{formatCurrency(space.eventRate)}</p>
                </div>
              )}
              <div>
                <span className="text-muted-foreground">Requiere Depósito:</span>
                <p className="font-medium">{space.requiresDeposit ? 'Sí' : 'No'}</p>
              </div>
              {space.requiresDeposit && (
                <div>
                  <span className="text-muted-foreground">Monto Depósito:</span>
                  <p className="font-medium">{formatCurrency(space.depositAmount)}</p>
                </div>
              )}
            </div>
          </CardContent>
        </Card>

        <Card>
          <CardContent className="p-6 space-y-4">
            <h3 className="font-semibold text-foreground border-b border-emerald-600/30 pb-2">Reglas de Reserva</h3>
            <div className="grid grid-cols-2 gap-4 text-sm">
              <div>
                <span className="text-muted-foreground">Mín. Horas:</span>
                <p className="font-medium">{space.minReservationHours}</p>
              </div>
              <div>
                <span className="text-muted-foreground">Máx. Horas:</span>
                <p className="font-medium">{space.maxReservationHours}</p>
              </div>
              <div>
                <span className="text-muted-foreground">Mín. Anticipación:</span>
                <p className="font-medium">{space.minAdvanceHours} horas</p>
              </div>
              <div>
                <span className="text-muted-foreground">Máx. Anticipación:</span>
                <p className="font-medium">{space.maxAdvanceDays} días</p>
              </div>
              <div>
                <span className="text-muted-foreground">Máx. Reservas/Unidad:</span>
                <p className="font-medium">{space.maxSimultaneousReservationsPerUnit}</p>
              </div>
            </div>
          </CardContent>
        </Card>

        <Card>
          <CardContent className="p-6 space-y-4">
            <div className="flex justify-between items-center border-b border-emerald-600/30 pb-2">
              <h3 className="font-semibold text-foreground">Horarios</h3>
              <Button onClick={() => setShowScheduleForm(true)}>
                <Plus className="w-4 h-4 mr-1" /> Agregar
              </Button>
            </div>
            <div className="space-y-2 max-h-64 overflow-y-auto">
              {space.schedules.length === 0 ? (
                <p className="text-sm text-muted-foreground">No hay horarios configurados.</p>
              ) : (
                space.schedules.map((schedule) => (
                  <div key={schedule.id} className="flex items-center justify-between p-2 bg-muted/50 rounded text-sm">
                    <div className="flex items-center gap-2">
                      <Clock className="w-4 h-4 text-emerald-600" />
                      <span className="font-medium">{dayNames[schedule.dayOfWeek]}</span>
                      <span className="text-muted-foreground">{schedule.startTime} - {schedule.endTime}</span>
                    </div>
                    <Button
                      variant="ghost"
                      onClick={() => handleDeleteSchedule(schedule.id)}
                    >
                      <Trash2 className="w-4 h-4 text-red-500" />
                    </Button>
                  </div>
                ))
              )}
            </div>
          </CardContent>
        </Card>
      </div>

      {showScheduleForm && (
        <div className="fixed inset-0 bg-black/50 flex items-center justify-center z-50">
          <Card className="w-full max-w-md">
            <CardContent className="p-6">
              <h3 className="font-semibold text-foreground mb-4">Agregar Horario</h3>
              <form onSubmit={handleAddSchedule} className="space-y-4">
                <div>
                  <label className="block text-sm font-medium text-foreground mb-1">Día de la Semana</label>
                  <select
                    value={scheduleForm.dayOfWeek}
                    onChange={(e) => setScheduleForm({ ...scheduleForm, dayOfWeek: parseInt(e.target.value) })}
                    className="w-full border-b border-emerald-600/30 focus:border-emerald-600 text-sm font-medium py-2 outline-none"
                  >
                    {dayNames.map((day, idx) => (
                      <option key={idx} value={idx}>{day}</option>
                    ))}
                  </select>
                </div>
                <div className="grid grid-cols-2 gap-4">
                  <div>
                    <label className="block text-sm font-medium text-foreground mb-1">Hora Inicio</label>
                    <input
                      type="time"
                      required
                      value={scheduleForm.startTime}
                      onChange={(e) => setScheduleForm({ ...scheduleForm, startTime: e.target.value })}
                      className="w-full border-b border-emerald-600/30 focus:border-emerald-600 text-sm font-medium py-2 outline-none"
                    />
                  </div>
                  <div>
                    <label className="block text-sm font-medium text-foreground mb-1">Hora Fin</label>
                    <input
                      type="time"
                      required
                      value={scheduleForm.endTime}
                      onChange={(e) => setScheduleForm({ ...scheduleForm, endTime: e.target.value })}
                      className="w-full border-b border-emerald-600/30 focus:border-emerald-600 text-sm font-medium py-2 outline-none"
                    />
                  </div>
                </div>
                <div className="flex justify-end gap-3 pt-4 border-t border-border">
                  <Button type="button" variant="ghost" onClick={() => setShowScheduleForm(false)}>
                    Cancelar
                  </Button>
                  <Button type="submit">
                    <Save className="w-4 h-4 mr-2" /> Guardar
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
