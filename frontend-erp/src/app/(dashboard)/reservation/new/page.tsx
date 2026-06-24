'use client';

import React, { useState, useEffect } from 'react';
import { useRouter } from 'next/navigation';
import { Loader2, Save, ArrowLeft, AlertTriangle, CheckCircle } from 'lucide-react';
import { Button } from '@/components/ui/Button';
import { Card, CardContent } from '@/components/ui/Card';
import reservationService, {
  ReservableSpaceListItem,
  CreateReservationRequest,
  AvailabilityCheck,
} from '@/lib/reservation-service';

export default function NewReservationPage() {
  const router = useRouter();
  const [loading, setLoading] = useState(false);
  const [checking, setChecking] = useState(false);
  const [error, setError] = useState('');
  const [spaces, setSpaces] = useState<ReservableSpaceListItem[]>([]);
  const [availability, setAvailability] = useState<AvailabilityCheck | null>(null);
  const [form, setForm] = useState<CreateReservationRequest>({
    spaceId: '',
    unitId: '',
    ownerId: '',
    startDateTime: '',
    endDateTime: '',
    estimatedAttendees: 1,
    eventDescription: '',
    hasMusic: false,
    musicEndTime: '',
    rulesAccepted: false,
  });

  useEffect(() => {
    const fetchSpaces = async () => {
      try {
        const data = await reservationService.getSpaces(true);
        setSpaces(data);
        if (data.length > 0) setForm((prev) => ({ ...prev, spaceId: data[0].id }));
      } catch {}
    };
    fetchSpaces();
  }, []);

  const checkAvailability = async () => {
    if (!form.spaceId || !form.startDateTime || !form.endDateTime || !form.unitId) return;
    setChecking(true);
    try {
      const result = await reservationService.checkAvailability(
        form.spaceId,
        form.startDateTime,
        form.endDateTime,
        form.unitId
      );
      setAvailability(result);
    } catch {
      setAvailability(null);
    } finally {
      setChecking(false);
    }
  };

  useEffect(() => {
    if (form.spaceId && form.startDateTime && form.endDateTime && form.unitId) {
      const timer = setTimeout(checkAvailability, 500);
      return () => clearTimeout(timer);
    }
  }, [form.spaceId, form.startDateTime, form.endDateTime, form.unitId]);

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setError('');
    if (!form.rulesAccepted) {
      setError('Debes aceptar las reglas del espacio para continuar.');
      return;
    }
    setLoading(true);
    try {
      await reservationService.createReservation(form);
      router.push('/reservation');
    } catch (err: unknown) {
      const message = err instanceof Error ? err.message : 'Error al crear la reserva.';
      setError(message);
    } finally {
      setLoading(false);
    }
  };

  const formatCurrency = (val: number) =>
    new Intl.NumberFormat('es-CO', { style: 'currency', currency: 'COP', maximumFractionDigits: 0 }).format(val);

  return (
    <div className="space-y-6">
      <div className="flex items-center gap-4">
        <Button variant="ghost" onClick={() => router.back()}>
          <ArrowLeft className="w-4 h-4 mr-2" /> Volver
        </Button>
        <div>
          <h1 className="text-2xl font-bold text-foreground tracking-tight">Nueva Reserva</h1>
          <p className="text-sm text-muted-foreground mt-1">Registra una nueva reserva de espacio.</p>
        </div>
      </div>

      {error && (
        <div className="bg-red-50 dark:bg-red-900/20 border border-red-200 dark:border-red-800 text-red-700 dark:text-red-300 px-4 py-3 rounded-lg text-sm">
          {error}
        </div>
      )}

      <form onSubmit={handleSubmit}>
        <div className="grid grid-cols-1 lg:grid-cols-2 gap-6">
          <Card>
            <CardContent className="p-6 space-y-4">
              <h3 className="font-semibold text-foreground border-b border-emerald-600/30 pb-2">Información de la Reserva</h3>
              <div>
                <label className="block text-sm font-medium text-foreground mb-1">Espacio *</label>
                <select
                  required
                  value={form.spaceId}
                  onChange={(e) => setForm({ ...form, spaceId: e.target.value })}
                  className="w-full border-b border-emerald-600/30 focus:border-emerald-600 text-sm font-medium py-2 outline-none"
                >
                  <option value="">Seleccionar espacio</option>
                  {spaces.map((space) => (
                    <option key={space.id} value={space.id}>{space.name}</option>
                  ))}
                </select>
              </div>
              <div>
                <label className="block text-sm font-medium text-foreground mb-1">ID Unidad *</label>
                <input
                  type="text"
                  required
                  value={form.unitId}
                  onChange={(e) => setForm({ ...form, unitId: e.target.value })}
                  className="w-full border-b border-emerald-600/30 focus:border-emerald-600 text-sm font-medium py-2 outline-none"
                  placeholder="ID de la unidad"
                />
              </div>
              <div>
                <label className="block text-sm font-medium text-foreground mb-1">ID Propietario *</label>
                <input
                  type="text"
                  required
                  value={form.ownerId}
                  onChange={(e) => setForm({ ...form, ownerId: e.target.value })}
                  className="w-full border-b border-emerald-600/30 focus:border-emerald-600 text-sm font-medium py-2 outline-none"
                  placeholder="ID del propietario"
                />
              </div>
              <div className="grid grid-cols-2 gap-4">
                <div>
                  <label className="block text-sm font-medium text-foreground mb-1">Fecha/Hora Inicio *</label>
                  <input
                    type="datetime-local"
                    required
                    value={form.startDateTime}
                    onChange={(e) => setForm({ ...form, startDateTime: e.target.value })}
                    className="w-full border-b border-emerald-600/30 focus:border-emerald-600 text-sm font-medium py-2 outline-none"
                  />
                </div>
                <div>
                  <label className="block text-sm font-medium text-foreground mb-1">Fecha/Hora Fin *</label>
                  <input
                    type="datetime-local"
                    required
                    value={form.endDateTime}
                    onChange={(e) => setForm({ ...form, endDateTime: e.target.value })}
                    className="w-full border-b border-emerald-600/30 focus:border-emerald-600 text-sm font-medium py-2 outline-none"
                  />
                </div>
              </div>
              <div>
                <label className="block text-sm font-medium text-foreground mb-1">Asistentes Estimados *</label>
                <input
                  type="number"
                  required
                  min="1"
                  value={form.estimatedAttendees}
                  onChange={(e) => setForm({ ...form, estimatedAttendees: parseInt(e.target.value) || 1 })}
                  className="w-full border-b border-emerald-600/30 focus:border-emerald-600 text-sm font-medium py-2 outline-none"
                />
              </div>
            </CardContent>
          </Card>

          <Card>
            <CardContent className="p-6 space-y-4">
              <h3 className="font-semibold text-foreground border-b border-emerald-600/30 pb-2">Detalles del Evento</h3>
              <div>
                <label className="block text-sm font-medium text-foreground mb-1">Descripción del Evento</label>
                <textarea
                  value={form.eventDescription || ''}
                  onChange={(e) => setForm({ ...form, eventDescription: e.target.value })}
                  className="w-full border-b border-emerald-600/30 focus:border-emerald-600 text-sm font-medium py-2 outline-none"
                  rows={3}
                  placeholder="Describe el evento"
                />
              </div>
              <div className="flex items-center gap-3">
                <input
                  type="checkbox"
                  id="hasMusic"
                  checked={form.hasMusic}
                  onChange={(e) => setForm({ ...form, hasMusic: e.target.checked })}
                  className="w-4 h-4 text-emerald-600 rounded"
                />
                <label htmlFor="hasMusic" className="text-sm font-medium">Incluye Música</label>
              </div>
              {form.hasMusic && (
                <div>
                  <label className="block text-sm font-medium text-foreground mb-1">Hora Fin de Música</label>
                  <input
                    type="time"
                    value={form.musicEndTime || ''}
                    onChange={(e) => setForm({ ...form, musicEndTime: e.target.value })}
                    className="w-full border-b border-emerald-600/30 focus:border-emerald-600 text-sm font-medium py-2 outline-none"
                  />
                </div>
              )}
              <div className="flex items-center gap-3">
                <input
                  type="checkbox"
                  id="rulesAccepted"
                  checked={form.rulesAccepted}
                  onChange={(e) => setForm({ ...form, rulesAccepted: e.target.checked })}
                  className="w-4 h-4 text-emerald-600 rounded"
                  required
                />
                <label htmlFor="rulesAccepted" className="text-sm font-medium">Acepto las reglas del espacio *</label>
              </div>
            </CardContent>
          </Card>
        </div>

        {availability && (
          <Card className="mt-6">
            <CardContent className="p-6">
              <h3 className="font-semibold text-foreground border-b border-emerald-600/30 pb-2 mb-4">Disponibilidad</h3>
              <div className="flex items-center gap-3">
                {availability.isAvailable ? (
                  <>
                    <CheckCircle className="w-5 h-5 text-green-600" />
                    <span className="text-green-700 font-medium">Espacio disponible</span>
                  </>
                ) : (
                  <>
                    <AlertTriangle className="w-5 h-5 text-red-600" />
                    <span className="text-red-700 font-medium">{availability.reason || 'Espacio no disponible'}</span>
                  </>
                )}
              </div>
              {availability.isAvailable && (
                <div className="grid grid-cols-3 gap-4 mt-4 text-sm">
                  <div>
                    <span className="text-muted-foreground">Costo Estimado:</span>
                    <p className="font-bold text-foreground">{formatCurrency(availability.estimatedCost)}</p>
                  </div>
                  <div>
                    <span className="text-muted-foreground">Depósito:</span>
                    <p className="font-bold text-foreground">{formatCurrency(availability.depositAmount)}</p>
                  </div>
                  <div>
                    <span className="text-muted-foreground">Total:</span>
                    <p className="font-bold text-foreground">{formatCurrency(availability.estimatedCost + availability.depositAmount)}</p>
                  </div>
                </div>
              )}
              {availability.arrearsWarning && (
                <div className="mt-4 bg-yellow-50 dark:bg-yellow-900/20 border border-yellow-200 dark:border-yellow-800 text-yellow-700 dark:text-yellow-300 px-4 py-3 rounded-lg text-sm">
                  {availability.arrearsWarning}
                </div>
              )}
            </CardContent>
          </Card>
        )}

        <div className="flex justify-end gap-3 pt-6">
          <Button type="button" variant="ghost" onClick={() => router.back()}>
            Cancelar
          </Button>
          <Button type="submit" disabled={loading || !availability?.isAvailable}>
            {loading ? <Loader2 className="w-4 h-4 mr-2 animate-spin" /> : <Save className="w-4 h-4 mr-2" />}
            Crear Reserva
          </Button>
        </div>
      </form>
    </div>
  );
}
