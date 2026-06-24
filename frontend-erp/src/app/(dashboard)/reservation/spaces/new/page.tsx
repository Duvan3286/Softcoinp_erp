'use client';

import React, { useState } from 'react';
import { useRouter } from 'next/navigation';
import { Loader2, Save, ArrowLeft } from 'lucide-react';
import { Button } from '@/components/ui/Button';
import { Card, CardContent } from '@/components/ui/Card';
import reservationService, { CreateReservableSpaceRequest } from '@/lib/reservation-service';
import axios from 'axios';

export default function NewSpacePage() {
  const router = useRouter();
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState('');
  const [form, setForm] = useState<CreateReservableSpaceRequest>({
    name: '',
    description: '',
    location: '',
    maxCapacity: 1,
    minReservationHours: 1,
    maxReservationHours: 8,
    minAdvanceHours: 2,
    maxAdvanceDays: 30,
    maxSimultaneousReservationsPerUnit: 2,
    requiresDeposit: false,
    depositAmount: 0,
    hasAdditionalCost: false,
    chargeType: 'PerHour',
    hourlyRate: 0,
    eventRate: 0,
    approvalMode: 'Automatic',
    arrearsPolicy: 'Warn',
  });

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setError('');
    setLoading(true);
    try {
      await reservationService.createSpace(form);
      router.push('/reservation/spaces');
    } catch (err: unknown) {
      if (axios.isAxiosError(err) && err.response?.data) {
        const data = err.response.data as { message?: string };
        setError(data.message || 'Error al crear el espacio.');
      } else if (err instanceof Error) {
        setError(err.message);
      } else {
        setError('Error al crear el espacio.');
      }
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="space-y-6">
      <div className="flex items-center gap-4">
        <Button variant="ghost" onClick={() => router.back()}>
          <ArrowLeft className="w-4 h-4 mr-2" /> Volver
        </Button>
        <div>
          <h1 className="text-2xl font-bold text-foreground tracking-tight">Nuevo Espacio Reservable</h1>
          <p className="text-sm text-muted-foreground mt-1">Configura un nuevo espacio para reservas.</p>
        </div>
      </div>

      {error && (
        <div className="bg-red-50 dark:bg-red-900/20 border border-red-200 dark:border-red-800 text-red-700 dark:text-red-300 px-4 py-3 rounded-lg text-sm">
          {error}
        </div>
      )}

      <form onSubmit={handleSubmit}>
        <Card>
          <CardContent className="p-6 space-y-6">
            <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
              <div className="space-y-4">
                <h3 className="font-semibold text-foreground border-b border-emerald-600/30 pb-2">Información General</h3>
                <div>
                  <label className="block text-sm font-medium text-foreground mb-1">Nombre *</label>
                  <input
                    type="text"
                    required
                    value={form.name}
                    onChange={(e) => setForm({ ...form, name: e.target.value })}
                    className="w-full border-b border-emerald-600/30 focus:border-emerald-600 text-sm font-medium py-2 outline-none"
                    placeholder="Ej: Salón de Eventos"
                  />
                </div>
                <div>
                  <label className="block text-sm font-medium text-foreground mb-1">Descripción</label>
                  <textarea
                    value={form.description || ''}
                    onChange={(e) => setForm({ ...form, description: e.target.value })}
                    className="w-full border-b border-emerald-600/30 focus:border-emerald-600 text-sm font-medium py-2 outline-none"
                    rows={3}
                    placeholder="Descripción del espacio"
                  />
                </div>
                <div>
                  <label className="block text-sm font-medium text-foreground mb-1">Ubicación</label>
                  <input
                    type="text"
                    value={form.location || ''}
                    onChange={(e) => setForm({ ...form, location: e.target.value })}
                    className="w-full border-b border-emerald-600/30 focus:border-emerald-600 text-sm font-medium py-2 outline-none"
                    placeholder="Ej: Edificio A, Piso 2"
                  />
                </div>
                <div>
                  <label className="block text-sm font-medium text-foreground mb-1">Capacidad Máxima *</label>
                  <input
                    type="number"
                    required
                    min="1"
                    value={form.maxCapacity}
                    onChange={(e) => setForm({ ...form, maxCapacity: parseInt(e.target.value) || 1 })}
                    className="w-full border-b border-emerald-600/30 focus:border-emerald-600 text-sm font-medium py-2 outline-none"
                  />
                </div>
              </div>

              <div className="space-y-4">
                <h3 className="font-semibold text-foreground border-b border-emerald-600/30 pb-2">Reglas de Reserva</h3>
                <div className="grid grid-cols-2 gap-4">
                  <div>
                    <label className="block text-sm font-medium text-foreground mb-1">Mín. Horas</label>
                    <input
                      type="number"
                      min="1"
                      value={form.minReservationHours}
                      onChange={(e) => setForm({ ...form, minReservationHours: parseInt(e.target.value) || 1 })}
                      className="w-full border-b border-emerald-600/30 focus:border-emerald-600 text-sm font-medium py-2 outline-none"
                    />
                  </div>
                  <div>
                    <label className="block text-sm font-medium text-foreground mb-1">Máx. Horas</label>
                    <input
                      type="number"
                      min="1"
                      value={form.maxReservationHours}
                      onChange={(e) => setForm({ ...form, maxReservationHours: parseInt(e.target.value) || 8 })}
                      className="w-full border-b border-emerald-600/30 focus:border-emerald-600 text-sm font-medium py-2 outline-none"
                    />
                  </div>
                </div>
                <div className="grid grid-cols-2 gap-4">
                  <div>
                    <label className="block text-sm font-medium text-foreground mb-1">Mín. Anticipación (horas)</label>
                    <input
                      type="number"
                      min="0"
                      value={form.minAdvanceHours}
                      onChange={(e) => setForm({ ...form, minAdvanceHours: parseInt(e.target.value) || 2 })}
                      className="w-full border-b border-emerald-600/30 focus:border-emerald-600 text-sm font-medium py-2 outline-none"
                    />
                  </div>
                  <div>
                    <label className="block text-sm font-medium text-foreground mb-1">Máx. Anticipación (días)</label>
                    <input
                      type="number"
                      min="1"
                      value={form.maxAdvanceDays}
                      onChange={(e) => setForm({ ...form, maxAdvanceDays: parseInt(e.target.value) || 30 })}
                      className="w-full border-b border-emerald-600/30 focus:border-emerald-600 text-sm font-medium py-2 outline-none"
                    />
                  </div>
                </div>
                <div>
                  <label className="block text-sm font-medium text-foreground mb-1">Máx. Reservas Simultáneas por Unidad</label>
                  <input
                    type="number"
                    min="1"
                    value={form.maxSimultaneousReservationsPerUnit}
                    onChange={(e) => setForm({ ...form, maxSimultaneousReservationsPerUnit: parseInt(e.target.value) || 2 })}
                    className="w-full border-b border-emerald-600/30 focus:border-emerald-600 text-sm font-medium py-2 outline-none"
                  />
                </div>
              </div>

              <div className="space-y-4">
                <h3 className="font-semibold text-foreground border-b border-emerald-600/30 pb-2">Costos y Depósito</h3>
                <div className="flex items-center gap-3">
                  <input
                    type="checkbox"
                    id="hasAdditionalCost"
                    checked={form.hasAdditionalCost}
                    onChange={(e) => setForm({ ...form, hasAdditionalCost: e.target.checked })}
                    className="w-4 h-4 text-emerald-600 rounded"
                  />
                  <label htmlFor="hasAdditionalCost" className="text-sm font-medium">Cobro Adicional</label>
                </div>
                {form.hasAdditionalCost && (
                  <div className="grid grid-cols-2 gap-4">
                    <div>
                      <label className="block text-sm font-medium text-foreground mb-1">Tipo de Cobro</label>
                      <select
                        value={form.chargeType}
                        onChange={(e) => setForm({ ...form, chargeType: e.target.value })}
                        className="w-full border-b border-emerald-600/30 focus:border-emerald-600 text-sm font-medium py-2 outline-none"
                      >
                        <option value="PerHour">Por Hora</option>
                        <option value="PerEvent">Por Evento</option>
                        <option value="Other">Otro</option>
                      </select>
                    </div>
                    <div>
                      <label className="block text-sm font-medium text-foreground mb-1">Tarifa</label>
                      <input
                        type="number"
                        min="0"
                        value={form.chargeType === 'PerHour' ? form.hourlyRate : form.eventRate}
                        onChange={(e) => {
                          const val = parseFloat(e.target.value) || 0;
                          if (form.chargeType === 'PerHour') {
                            setForm({ ...form, hourlyRate: val });
                          } else {
                            setForm({ ...form, eventRate: val });
                          }
                        }}
                        className="w-full border-b border-emerald-600/30 focus:border-emerald-600 text-sm font-medium py-2 outline-none"
                      />
                    </div>
                  </div>
                )}
                <div className="flex items-center gap-3">
                  <input
                    type="checkbox"
                    id="requiresDeposit"
                    checked={form.requiresDeposit}
                    onChange={(e) => setForm({ ...form, requiresDeposit: e.target.checked })}
                    className="w-4 h-4 text-emerald-600 rounded"
                  />
                  <label htmlFor="requiresDeposit" className="text-sm font-medium">Requiere Depósito</label>
                </div>
                {form.requiresDeposit && (
                  <div>
                    <label className="block text-sm font-medium text-foreground mb-1">Monto del Depósito</label>
                    <input
                      type="number"
                      min="0"
                      value={form.depositAmount}
                      onChange={(e) => setForm({ ...form, depositAmount: parseFloat(e.target.value) || 0 })}
                      className="w-full border-b border-emerald-600/30 focus:border-emerald-600 text-sm font-medium py-2 outline-none"
                    />
                  </div>
                )}
              </div>

              <div className="space-y-4">
                <h3 className="font-semibold text-foreground border-b border-emerald-600/30 pb-2">Políticas</h3>
                <div>
                  <label className="block text-sm font-medium text-foreground mb-1">Modo de Aprobación</label>
                  <select
                    value={form.approvalMode}
                    onChange={(e) => setForm({ ...form, approvalMode: e.target.value })}
                    className="w-full border-b border-emerald-600/30 focus:border-emerald-600 text-sm font-medium py-2 outline-none"
                  >
                    <option value="Automatic">Automática</option>
                    <option value="Manual">Manual</option>
                  </select>
                </div>
                <div>
                  <label className="block text-sm font-medium text-foreground mb-1">Política de Mora</label>
                  <select
                    value={form.arrearsPolicy}
                    onChange={(e) => setForm({ ...form, arrearsPolicy: e.target.value })}
                    className="w-full border-b border-emerald-600/30 focus:border-emerald-600 text-sm font-medium py-2 outline-none"
                  >
                    <option value="Block">Bloquear Reservas</option>
                    <option value="Warn">Advertir</option>
                  </select>
                </div>
              </div>
            </div>

            <div className="flex justify-end gap-3 pt-4 border-t border-border">
              <Button type="button" variant="ghost" onClick={() => router.back()}>
                Cancelar
              </Button>
              <Button type="submit" disabled={loading}>
                {loading ? <Loader2 className="w-4 h-4 mr-2 animate-spin" /> : <Save className="w-4 h-4 mr-2" />}
                Guardar Espacio
              </Button>
            </div>
          </CardContent>
        </Card>
      </form>
    </div>
  );
}
