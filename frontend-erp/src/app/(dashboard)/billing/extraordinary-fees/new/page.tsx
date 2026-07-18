'use client';

import React, { useState, useEffect } from 'react';
import { useRouter, useSearchParams } from 'next/navigation';
import { Loader2, ArrowLeft, Save, AlertTriangle } from 'lucide-react';
import { Button } from '@/components/ui/Button';
import { Card, CardContent } from '@/components/ui/Card';
import feesPortfolioService from '@/lib/fees-portfolio-service';
import { UnitsService, Unit, formatUnitLabel } from '@/lib/units-service';

type ApplyScope = 'AllByCoefficient' | 'SpecificGroup';

function computeInstallmentsFromRange(startDate: string, endDate: string): number {
  const start = new Date(startDate);
  const end = new Date(endDate);
  const months = (end.getFullYear() - start.getFullYear()) * 12 + (end.getMonth() - start.getMonth()) + 1;
  if (months < 1) {
    return 1;
  }
  return months;
}

export default function NewExtraordinaryFeePage() {
  const router = useRouter();
  const searchParams = useSearchParams();
  const preselectedUnitId = searchParams.get('unitId');
  const [submitting, setSubmitting] = useState(false);
  const [error, setError] = useState('');

  const [name, setName] = useState('');
  const [totalAmount, setTotalAmount] = useState<number>(0);
  const [applyScope, setApplyScope] = useState<ApplyScope>('SpecificGroup');
  const [startDate, setStartDate] = useState('');
  const [endDate, setEndDate] = useState('');
  const [meetingActNumber, setMeetingActNumber] = useState('');
  const [notes, setNotes] = useState('');

  const [units, setUnits] = useState<Unit[]>([]);
  const [loadingUnits, setLoadingUnits] = useState(true);
  const [unitFilter, setUnitFilter] = useState('');
  const [selectedUnitIds, setSelectedUnitIds] = useState<string[]>(() => {
    if (preselectedUnitId) {
      return [preselectedUnitId];
    }
    return [];
  });

  useEffect(() => {
    const fetchUnits = async () => {
      try {
        const data = await UnitsService.getUnits();
        setUnits(data);
      } catch {
        setError('Error al cargar las unidades.');
      } finally {
        setLoadingUnits(false);
      }
    };
    fetchUnits();
  }, []);

  const toggleUnitSelection = (unitId: string) => {
    if (selectedUnitIds.includes(unitId)) {
      setSelectedUnitIds(selectedUnitIds.filter((id) => id !== unitId));
    } else {
      setSelectedUnitIds([...selectedUnitIds, unitId]);
    }
  };

  const filteredUnits = units.filter((u) => {
    if (unitFilter.trim() === '') {
      return true;
    }
    return formatUnitLabel(u.identifier, u.towerOrBlock).toLowerCase().includes(unitFilter.toLowerCase());
  });

  const installmentsPreview = () => {
    if (!startDate || !endDate) {
      return 0;
    }
    return computeInstallmentsFromRange(startDate, endDate);
  };

  const amountPerInstallmentPreview = () => {
    const installments = installmentsPreview();
    if (installments === 0 || totalAmount <= 0) {
      return 0;
    }
    return totalAmount / installments;
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setError('');

    if (!name.trim()) {
      setError('El nombre es requerido.');
      return;
    }
    if (totalAmount <= 0) {
      setError('El monto total debe ser mayor a cero.');
      return;
    }
    if (!startDate) {
      setError('La fecha de inicio es requerida.');
      return;
    }
    if (!endDate) {
      setError('La fecha fin es requerida.');
      return;
    }
    if (new Date(endDate) < new Date(startDate)) {
      setError('La fecha fin no puede ser anterior a la fecha de inicio.');
      return;
    }
    if (applyScope === 'SpecificGroup' && selectedUnitIds.length === 0) {
      setError('Debe seleccionar al menos una unidad.');
      return;
    }

    const numberOfInstallments = computeInstallmentsFromRange(startDate, endDate);
    const startPeriod = startDate.slice(0, 7);

    setSubmitting(true);
    try {
      const result = await feesPortfolioService.createExtraordinaryFee({
        name,
        totalAmount,
        distributionType: applyScope,
        unitIds: applyScope === 'SpecificGroup' ? selectedUnitIds : undefined,
        dueDate: startDate,
        startPeriod,
        numberOfInstallments,
        meetingActNumber,
        notes,
      });
      router.push(`/billing/extraordinary-fees/${result.id}`);
    } catch (err: any) {
      setError(err?.response?.data || 'Error al crear la cuota extraordinaria.');
    } finally {
      setSubmitting(false);
    }
  };

  return (
    <div className="space-y-6 max-w-2xl">
      <button onClick={() => router.push('/billing/extraordinary-fees')} className="inline-flex items-center gap-2 text-sm text-muted-foreground hover:text-foreground transition-colors">
        <ArrowLeft className="w-4 h-4" />
        Volver a Cuotas Extraordinarias
      </button>

      <div>
        <h1 className="text-2xl font-bold text-foreground tracking-tight">Nueva Cuota Extraordinaria / Deuda</h1>
        <p className="text-sm text-muted-foreground mt-1">
          Registra una cuota extraordinaria, un rubro adicional, o deuda acumulada de una unidad. El monto total se reparte
          automáticamente en cuotas mensuales entre la fecha de inicio y la fecha fin.
        </p>
      </div>

      <Card>
        <CardContent className="p-6">
          <form onSubmit={handleSubmit} className="space-y-6">
            <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
              <div className="md:col-span-2">
                <label className="block text-xs font-bold text-muted-foreground uppercase tracking-widest mb-1.5">Nombre / Concepto</label>
                <input
                  type="text"
                  placeholder="Ej. Administración atrasada 2026, Cuota Extraordinaria Parque Infantil"
                  value={name}
                  onChange={(e) => setName(e.target.value)}
                  className="w-full bg-transparent border-b border-emerald-600 focus:border-b-2 text-foreground pl-0 pr-6 py-2 text-sm focus:outline-none transition-all"
                  required
                />
              </div>

              <div>
                <label className="block text-xs font-bold text-muted-foreground uppercase tracking-widest mb-1.5">Monto Total</label>
                <input
                  type="number"
                  step="0.01"
                  min="0"
                  placeholder="0.00"
                  value={totalAmount || ''}
                  onChange={(e) => setTotalAmount(Number(e.target.value))}
                  className="w-full bg-transparent border-b border-emerald-600 focus:border-b-2 text-foreground pl-0 pr-6 py-2 text-sm focus:outline-none transition-all"
                  required
                />
              </div>

              <div>
                <label className="block text-xs font-bold text-muted-foreground uppercase tracking-widest mb-1.5">Aplicar a</label>
                <select
                  value={applyScope}
                  onChange={(e) => setApplyScope(e.target.value as ApplyScope)}
                  className="w-full bg-transparent border-b border-emerald-600 focus:border-b-2 text-foreground pl-0 pr-6 py-2 text-sm focus:outline-none transition-all"
                >
                  <option value="SpecificGroup">Unidad(es) Específica(s)</option>
                  <option value="AllByCoefficient">Todo el Conjunto (por Coeficiente)</option>
                </select>
              </div>

              <div>
                <label className="block text-xs font-bold text-muted-foreground uppercase tracking-widest mb-1.5">Fecha Inicio</label>
                <input
                  type="date"
                  value={startDate}
                  onChange={(e) => setStartDate(e.target.value)}
                  className="w-full bg-transparent border-b border-emerald-600 focus:border-b-2 text-foreground pl-0 pr-6 py-2 text-sm focus:outline-none transition-all"
                  required
                />
              </div>

              <div>
                <label className="block text-xs font-bold text-muted-foreground uppercase tracking-widest mb-1.5">Fecha Fin</label>
                <input
                  type="date"
                  value={endDate}
                  onChange={(e) => setEndDate(e.target.value)}
                  className="w-full bg-transparent border-b border-emerald-600 focus:border-b-2 text-foreground pl-0 pr-6 py-2 text-sm focus:outline-none transition-all"
                  required
                />
              </div>

              <div>
                <label className="block text-xs font-bold text-muted-foreground uppercase tracking-widest mb-1.5">Número de Acta (opcional)</label>
                <input
                  type="text"
                  placeholder="Solo si viene de una decisión de asamblea"
                  value={meetingActNumber}
                  onChange={(e) => setMeetingActNumber(e.target.value)}
                  className="w-full bg-transparent border-b border-emerald-600 focus:border-b-2 text-foreground pl-0 pr-6 py-2 text-sm focus:outline-none transition-all"
                />
              </div>

              {startDate && endDate && (
                <div className="md:col-span-2 bg-emerald-50 dark:bg-emerald-950/20 border border-emerald-100 dark:border-emerald-900 rounded-xl p-4 text-sm text-emerald-700 dark:text-emerald-400">
                  Se generarán <strong>{installmentsPreview()}</strong> cuota(s) mensual(es) de aproximadamente{' '}
                  <strong>{amountPerInstallmentPreview().toLocaleString('es-CO', { style: 'currency', currency: 'COP', minimumFractionDigits: 0 })}</strong> cada una
                  {applyScope === 'SpecificGroup' && selectedUnitIds.length > 0 && ` por cada una de las ${selectedUnitIds.length} unidad(es) seleccionada(s)`}.
                </div>
              )}

              {applyScope === 'SpecificGroup' && (
                <div className="md:col-span-2">
                  <label className="block text-xs font-bold text-muted-foreground uppercase tracking-widest mb-1.5">
                    Unidades ({selectedUnitIds.length} seleccionada{selectedUnitIds.length === 1 ? '' : 's'})
                  </label>
                  <input
                    type="text"
                    placeholder="Buscar unidad..."
                    value={unitFilter}
                    onChange={(e) => setUnitFilter(e.target.value)}
                    className="w-full mb-2 px-3 py-2 border border-border rounded-lg text-sm focus:ring-2 focus:ring-emerald-500/20 focus:border-emerald-500 outline-none"
                  />
                  <div className="max-h-56 overflow-y-auto border border-border rounded-lg divide-y divide-border">
                    {loadingUnits && (
                      <div className="p-4 flex justify-center">
                        <Loader2 className="w-5 h-5 animate-spin text-emerald-600" />
                      </div>
                    )}
                    {!loadingUnits && filteredUnits.length === 0 && (
                      <p className="p-4 text-sm text-muted-foreground text-center">No se encontraron unidades.</p>
                    )}
                    {!loadingUnits && filteredUnits.map((u) => (
                      <label key={u.id} className="flex items-center gap-3 px-4 py-2 hover:bg-muted/30 cursor-pointer text-sm">
                        <input
                          type="checkbox"
                          checked={selectedUnitIds.includes(u.id)}
                          onChange={() => toggleUnitSelection(u.id)}
                          className="w-4 h-4 text-emerald-600 rounded focus:ring-emerald-500"
                        />
                        <span className="text-foreground">{formatUnitLabel(u.identifier, u.towerOrBlock)}</span>
                      </label>
                    ))}
                  </div>
                </div>
              )}

              <div className="md:col-span-2">
                <label className="block text-xs font-bold text-muted-foreground uppercase tracking-widest mb-1.5">Notas</label>
                <textarea
                  placeholder="Ej. Deuda heredada del sistema anterior, multa por..., etc."
                  value={notes}
                  onChange={(e) => setNotes(e.target.value)}
                  rows={3}
                  className="w-full bg-transparent border-b border-emerald-600 focus:border-b-2 text-foreground pl-0 pr-6 py-2 text-sm focus:outline-none transition-all resize-none"
                />
              </div>
            </div>

            {error && (
              <div className="p-3 bg-rose-50 border border-rose-200 rounded-lg text-rose-700 text-xs flex items-center gap-2">
                <AlertTriangle className="w-4 h-4 shrink-0" />
                {error}
              </div>
            )}

            <div className="flex justify-end gap-3 pt-4 border-t border-border">
              <Button type="button" variant="ghost" onClick={() => router.push('/billing/extraordinary-fees')}>Cancelar</Button>
              <Button type="submit" disabled={submitting}>
                {submitting ? <Loader2 className="w-4 h-4 animate-spin mr-2" /> : <Save className="w-4 h-4 mr-2" />}
                Crear
              </Button>
            </div>
          </form>
        </CardContent>
      </Card>
    </div>
  );
}
