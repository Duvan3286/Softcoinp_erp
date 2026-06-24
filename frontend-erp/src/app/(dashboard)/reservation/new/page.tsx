'use client';

import React, { useState, useEffect, useRef } from 'react';
import { useRouter } from 'next/navigation';
import { Loader2, Save, ArrowLeft, AlertTriangle, CheckCircle, Search, ChevronDown } from 'lucide-react';
import { Button } from '@/components/ui/Button';
import { Card, CardContent } from '@/components/ui/Card';
import reservationService, {
  ReservableSpaceListItem,
  CreateReservationRequest,
  AvailabilityCheck,
  UnitWithOwnerInfo,
} from '@/lib/reservation-service';
import axios from 'axios';

export default function NewReservationPage() {
  const router = useRouter();
  const [loading, setLoading] = useState(false);
  const [checking, setChecking] = useState(false);
  const [error, setError] = useState('');
  const [spaces, setSpaces] = useState<ReservableSpaceListItem[]>([]);
  const [units, setUnits] = useState<UnitWithOwnerInfo[]>([]);
  const [availability, setAvailability] = useState<AvailabilityCheck | null>(null);
  const [unitSearch, setUnitSearch] = useState('');
  const [showUnitDropdown, setShowUnitDropdown] = useState(false);
  const [selectedUnitLabel, setSelectedUnitLabel] = useState('');
  const [selectedOwnerLabel, setSelectedOwnerLabel] = useState('');
  const unitDropdownRef = useRef<HTMLDivElement>(null);
  const unitInputRef = useRef<HTMLInputElement>(null);
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
    const fetchData = async () => {
      try {
        const [spacesData, unitsData] = await Promise.all([
          reservationService.getSpaces(true),
          reservationService.getUnitsWithOwners(),
        ]);
        setSpaces(spacesData);
        setUnits(unitsData);
        if (spacesData.length > 0) setForm((prev) => ({ ...prev, spaceId: spacesData[0].id }));
      } catch {}
    };
    fetchData();
  }, []);

  useEffect(() => {
    const handleClickOutside = (event: MouseEvent) => {
      if (unitDropdownRef.current && !unitDropdownRef.current.contains(event.target as Node)) {
        setShowUnitDropdown(false);
      }
    };
    document.addEventListener('mousedown', handleClickOutside);
    return () => document.removeEventListener('mousedown', handleClickOutside);
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

  const filteredUnits = units.filter((u) =>
    u.unitIdentifier.toLowerCase().includes(unitSearch.toLowerCase()) ||
    (u.ownerName && u.ownerName.toLowerCase().includes(unitSearch.toLowerCase()))
  );

  const selectUnit = (unit: UnitWithOwnerInfo) => {
    setForm((prev) => ({
      ...prev,
      unitId: unit.unitId,
      ownerId: unit.ownerId || '',
    }));
    setSelectedUnitLabel(`${unit.unitIdentifier}${unit.ownerName ? ` - ${unit.ownerName}` : ''}`);
    setSelectedOwnerLabel(unit.ownerName || 'Sin propietario asignado');
    setUnitSearch('');
    setShowUnitDropdown(false);
  };

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
      let message = 'Error al crear la reserva.';
      if (axios.isAxiosError(err) && err.response?.data) {
        const data = err.response.data as { message?: string };
        if (data.message) message = data.message;
      } else if (err instanceof Error) {
        message = err.message;
      }
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

              <div ref={unitDropdownRef} className="relative">
                <label className="block text-sm font-medium text-foreground mb-1">Unidad *</label>
                <div
                  className="w-full border-b border-emerald-600/30 focus-within:border-emerald-600 text-sm font-medium py-2 outline-none flex items-center cursor-pointer"
                  onClick={() => {
                    setShowUnitDropdown(!showUnitDropdown);
                    setTimeout(() => unitInputRef.current?.focus(), 50);
                  }}
                >
                  <Search className="w-4 h-4 text-muted-foreground mr-2 flex-shrink-0" />
                  <input
                    ref={unitInputRef}
                    type="text"
                    value={selectedUnitLabel || unitSearch}
                    onChange={(e) => {
                      setUnitSearch(e.target.value);
                      setShowUnitDropdown(true);
                      if (!e.target.value) {
                        setSelectedUnitLabel('');
                        setForm((prev) => ({ ...prev, unitId: '', ownerId: '' }));
                        setSelectedOwnerLabel('');
                      }
                    }}
                    onFocus={() => setShowUnitDropdown(true)}
                    className="flex-1 bg-transparent border-none outline-none text-sm font-medium"
                    placeholder="Buscar unidad o propietario..."
                  />
                  <ChevronDown className="w-4 h-4 text-muted-foreground flex-shrink-0" />
                </div>
                {showUnitDropdown && (
                  <div className="absolute z-50 top-full left-0 right-0 mt-1 bg-card border border-border rounded-lg shadow-lg max-h-60 overflow-y-auto">
                    {filteredUnits.length === 0 ? (
                      <div className="p-3 text-sm text-muted-foreground">No se encontraron unidades.</div>
                    ) : (
                      filteredUnits.map((unit) => (
                        <div
                          key={unit.unitId}
                          className="p-3 hover:bg-muted/50 cursor-pointer border-b border-border last:border-b-0"
                          onClick={() => selectUnit(unit)}
                        >
                          <div className="font-medium text-foreground text-sm">{unit.unitIdentifier}</div>
                          {unit.ownerName && (
                            <div className="text-xs text-muted-foreground mt-0.5">
                              Propietario: {unit.ownerName}
                            </div>
                          )}
                        </div>
                      ))
                    )}
                  </div>
                )}
              </div>

              <div>
                <label className="block text-sm font-medium text-foreground mb-1">Propietario</label>
                <div className="w-full border-b border-emerald-600/30 text-sm font-medium py-2 text-muted-foreground">
                  {selectedOwnerLabel || 'Selecciona una unidad para ver el propietario'}
                </div>
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
