'use client';

import React, { useState, useEffect } from 'react';
import { useRouter, useSearchParams } from 'next/navigation';
import { Loader2, ArrowLeft, Eye, Save, AlertTriangle, CheckCircle, CreditCard } from 'lucide-react';
import { Button } from '@/components/ui/Button';
import { Card, CardHeader, CardContent } from '@/components/ui/Card';
import feesPortfolioService, { PaymentPreview, RegisterPaymentRequest, UnitDebtSummary, ManualAllocationLine } from '@/lib/fees-portfolio-service';
import { UnitsService as unitsService, Unit, formatUnitLabel } from '@/lib/units-service';

export default function RegisterPaymentPage() {
  const router = useRouter();
  const searchParams = useSearchParams();
  const [units, setUnits] = useState<Unit[]>([]);
  const [loadingUnits, setLoadingUnits] = useState(true);
  const [submitting, setSubmitting] = useState(false);
  const [previewing, setPreviewing] = useState(false);
  const [error, setError] = useState('');
  const [success, setSuccess] = useState('');
  const [preview, setPreview] = useState<PaymentPreview | null>(null);
  const [result, setResult] = useState<{ id: string; amount: number; advanceAmount: number } | null>(null);

  const [unitId, setUnitId] = useState(searchParams.get('unitId') || '');
  const [paymentDate, setPaymentDate] = useState(new Date().toISOString().slice(0, 10));
  const [amount, setAmount] = useState<number>(0);
  const [paymentMethod, setPaymentMethod] = useState('Cash');
  const [referenceNumber, setReferenceNumber] = useState('');
  const [notes, setNotes] = useState('');

  const [imputationType, setImputationType] = useState('Automatic');
  const [manualJustification, setManualJustification] = useState('');
  const [debtSummary, setDebtSummary] = useState<UnitDebtSummary | null>(null);
  const [loadingDebt, setLoadingDebt] = useState(false);
  const [manualAllocations, setManualAllocations] = useState<Record<string, number>>({});

  useEffect(() => {
    const fetchUnits = async () => {
      try {
        const data = await unitsService.getUnits();
        setUnits(data);
      } catch {
        setError('Error al cargar las unidades.');
      } finally {
        setLoadingUnits(false);
      }
    };
    fetchUnits();
  }, []);

  useEffect(() => {
    if (imputationType !== 'Manual' || !unitId) {
      setDebtSummary(null);
      setManualAllocations({});
      return;
    }

    const fetchDebt = async () => {
      setLoadingDebt(true);
      try {
        const data = await feesPortfolioService.getUnitBalance(unitId);
        setDebtSummary(data);
        setManualAllocations({});
      } catch {
        setError('Error al cargar el desglose de deuda de la unidad.');
      } finally {
        setLoadingDebt(false);
      }
    };
    fetchDebt();
  }, [imputationType, unitId]);

  const buildManualLines = (): ManualAllocationLine[] => {
    const lines: ManualAllocationLine[] = [];
    for (const key of Object.keys(manualAllocations)) {
      const value = manualAllocations[key];
      if (value > 0) {
        const parts = key.split('|');
        lines.push({ sourceType: parts[0], sourceId: parts[1], amount: value });
      }
    }
    return lines;
  };

  const manualTotal = (): number => {
    let total = 0;
    for (const key of Object.keys(manualAllocations)) {
      total += manualAllocations[key] || 0;
    }
    return total;
  };

  const handleManualAmountChange = (sourceType: string, sourceId: string, value: number) => {
    const key = `${sourceType}|${sourceId}`;
    setManualAllocations(prev => ({ ...prev, [key]: value }));
  };

  const handlePreview = async () => {
    setError('');
    setPreview(null);
    setResult(null);
    if (!unitId) { setError('Debe seleccionar una unidad.'); return; }
    if (!paymentDate) { setError('La fecha de pago es requerida.'); return; }

    if (imputationType === 'Manual') {
      const lines = buildManualLines();
      if (lines.length === 0) {
        setError('Debe asignar al menos una línea en modo manual.');
        return;
      }
      setPreviewing(true);
      try {
        const data = await feesPortfolioService.previewManualPayment(unitId, lines);
        setPreview(data);
      } catch (err: any) {
        setError(err?.response?.data || 'Error al obtener la vista previa del pago.');
      } finally {
        setPreviewing(false);
      }
      return;
    }

    if (amount <= 0) { setError('El monto debe ser mayor a cero.'); return; }

    setPreviewing(true);
    try {
      const data = await feesPortfolioService.previewPayment(unitId, amount);
      setPreview(data);
    } catch (err: any) {
      setError(err?.response?.data || 'Error al obtener la vista previa del pago.');
    } finally {
      setPreviewing(false);
    }
  };

  const handleRegister = async () => {
    setError('');
    setSuccess('');
    if (!unitId) { setError('Debe seleccionar una unidad.'); return; }
    if (!paymentDate) { setError('La fecha de pago es requerida.'); return; }

    if (imputationType === 'Manual') {
      if (!manualJustification.trim()) {
        setError('La justificación es obligatoria en modo manual.');
        return;
      }
      const lines = buildManualLines();
      if (lines.length === 0) {
        setError('Debe asignar al menos una línea en modo manual.');
        return;
      }
      const total = manualTotal();
      if (total !== amount) {
        setError(`La suma de las asignaciones manuales (${total}) debe coincidir con el monto del pago (${amount}).`);
        return;
      }
    } else if (amount <= 0) {
      setError('El monto debe ser mayor a cero.');
      return;
    }

    setSubmitting(true);
    try {
      const request: RegisterPaymentRequest = {
        unitId,
        paymentDate,
        amount,
        paymentMethod,
        referenceNumber,
        notes,
        imputationType,
      };

      if (imputationType === 'Manual') {
        request.manualJustification = manualJustification;
        request.manualAllocations = buildManualLines();
      }

      const data = await feesPortfolioService.registerPayment(request);
      setResult(data);
      setSuccess('Pago registrado exitosamente.');
      setPreview(null);
    } catch (err: any) {
      setError(err?.response?.data || 'Error al registrar el pago.');
    } finally {
      setSubmitting(false);
    }
  };

  const formatCurrency = (val: number) =>
    new Intl.NumberFormat('es-CO', { style: 'currency', currency: 'COP', minimumFractionDigits: 0 }).format(val);

  const sourceTypeLabel = (type: string) => {
    const map: Record<string, string> = {
      UnitFee: 'Cuota Ordinaria',
      ExtraordinaryFee: 'Cuota Extraordinaria',
      IndividualCharge: 'Cobro Individual',
      Interest: 'Interés de Mora',
    };
    return map[type] || type;
  };

  const paymentMethods = [
    { value: 'Cash', label: 'Efectivo' },
    { value: 'Transfer', label: 'Transferencia' },
    { value: 'Check', label: 'Cheque' },
  ];

  if (loadingUnits) {
    return (
      <div className="flex justify-center py-20">
        <Loader2 className="w-8 h-8 animate-spin text-emerald-600" />
      </div>
    );
  }

  return (
    <div className="space-y-6 max-w-3xl">
      <button onClick={() => router.push('/billing')} className="inline-flex items-center gap-2 text-sm text-muted-foreground hover:text-foreground transition-colors">
        <ArrowLeft className="w-4 h-4" />
        Volver a Liquidación
      </button>

      <div>
        <h1 className="text-2xl font-bold text-foreground tracking-tight">Registrar Pago</h1>
        <p className="text-sm text-muted-foreground mt-1">Registra un pago recibido de una unidad.</p>
      </div>

      {success && (
        <div className="p-4 bg-emerald-50 border border-emerald-200 rounded-xl text-emerald-700 text-sm flex items-center gap-2">
          <CheckCircle className="w-5 h-5 shrink-0" />
          {success}
        </div>
      )}

      {error && (
        <div className="p-3 bg-rose-50 border border-rose-200 rounded-lg text-rose-700 text-xs flex items-center gap-2">
          <AlertTriangle className="w-4 h-4 shrink-0" />
          {error}
        </div>
      )}

      <Card>
        <CardContent className="p-6">
          <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
            <div className="md:col-span-2">
              <label className="block text-xs font-bold text-muted-foreground uppercase tracking-widest mb-1.5">Unidad</label>
              <select
                value={unitId}
                onChange={(e) => setUnitId(e.target.value)}
                className="w-full bg-transparent border-b border-emerald-600 focus:border-b-2 text-foreground pl-0 pr-6 py-2 text-sm focus:outline-none transition-all"
                required
              >
                <option value="">Seleccione una unidad...</option>
                {units.map((u) => (
                  <option key={u.id} value={u.id}>{formatUnitLabel(u.identifier, u.towerOrBlock)}</option>
                ))}
              </select>
            </div>

            <div className="md:col-span-2">
              <label className="block text-xs font-bold text-muted-foreground uppercase tracking-widest mb-1.5">Modo de Imputación</label>
              <div className="flex gap-4">
                <label className="flex items-center gap-2 text-sm">
                  <input
                    type="radio"
                    checked={imputationType === 'Automatic'}
                    onChange={() => setImputationType('Automatic')}
                  />
                  Automática (intereses primero, luego capital, por antigüedad)
                </label>
                <label className="flex items-center gap-2 text-sm">
                  <input
                    type="radio"
                    checked={imputationType === 'Manual'}
                    onChange={() => setImputationType('Manual')}
                  />
                  Manual (distribución libre con justificación)
                </label>
              </div>
            </div>

            <div>
              <label className="block text-xs font-bold text-muted-foreground uppercase tracking-widest mb-1.5">Fecha de Pago</label>
              <input
                type="date"
                value={paymentDate}
                onChange={(e) => setPaymentDate(e.target.value)}
                className="w-full bg-transparent border-b border-emerald-600 focus:border-b-2 text-foreground pl-0 pr-6 py-2 text-sm focus:outline-none transition-all"
                required
              />
            </div>
            <div>
              <label className="block text-xs font-bold text-muted-foreground uppercase tracking-widest mb-1.5">Monto</label>
              <input
                type="number"
                step="0.01"
                min="0"
                placeholder="0.00"
                value={amount || ''}
                onChange={(e) => setAmount(Number(e.target.value))}
                className="w-full bg-transparent border-b border-emerald-600 focus:border-b-2 text-foreground pl-0 pr-6 py-2 text-sm focus:outline-none transition-all"
                required
              />
            </div>
            <div>
              <label className="block text-xs font-bold text-muted-foreground uppercase tracking-widest mb-1.5">Medio de Pago</label>
              <select
                value={paymentMethod}
                onChange={(e) => setPaymentMethod(e.target.value)}
                className="w-full bg-transparent border-b border-emerald-600 focus:border-b-2 text-foreground pl-0 pr-6 py-2 text-sm focus:outline-none transition-all"
              >
                {paymentMethods.map((m) => (
                  <option key={m.value} value={m.value}>{m.label}</option>
                ))}
              </select>
            </div>
            <div>
              <label className="block text-xs font-bold text-muted-foreground uppercase tracking-widest mb-1.5">Número de Referencia</label>
              <input
                type="text"
                placeholder="Ej. Consignación No. 12345"
                value={referenceNumber}
                onChange={(e) => setReferenceNumber(e.target.value)}
                className="w-full bg-transparent border-b border-emerald-600 focus:border-b-2 text-foreground pl-0 pr-6 py-2 text-sm focus:outline-none transition-all"
              />
            </div>
            <div className="md:col-span-2">
              <label className="block text-xs font-bold text-muted-foreground uppercase tracking-widest mb-1.5">Notas</label>
              <textarea
                placeholder="Notas adicionales..."
                value={notes}
                onChange={(e) => setNotes(e.target.value)}
                rows={2}
                className="w-full bg-transparent border-b border-emerald-600 focus:border-b-2 text-foreground pl-0 pr-6 py-2 text-sm focus:outline-none transition-all resize-none"
              />
            </div>

            {imputationType === 'Manual' && (
              <div className="md:col-span-2">
                <label className="block text-xs font-bold text-muted-foreground uppercase tracking-widest mb-1.5">Justificación (obligatoria)</label>
                <textarea
                  placeholder="Explique por qué se distribuye el pago de forma manual..."
                  value={manualJustification}
                  onChange={(e) => setManualJustification(e.target.value)}
                  rows={2}
                  className="w-full bg-transparent border-b border-amber-600 focus:border-b-2 text-foreground pl-0 pr-6 py-2 text-sm focus:outline-none transition-all resize-none"
                  required
                />
              </div>
            )}
          </div>

          {imputationType === 'Manual' && (
            <div className="mt-6 pt-4 border-t border-border">
              <h3 className="font-bold text-foreground mb-3">Desglose de Deuda Pendiente</h3>
              {loadingDebt && <Loader2 className="w-5 h-5 animate-spin text-emerald-600" />}
              {!loadingDebt && !unitId && (
                <p className="text-sm text-muted-foreground">Seleccione una unidad para ver su deuda pendiente.</p>
              )}
              {!loadingDebt && debtSummary && debtSummary.items.length === 0 && (
                <p className="text-sm text-muted-foreground">La unidad no tiene deudas pendientes.</p>
              )}
              {!loadingDebt && debtSummary && debtSummary.items.length > 0 && (
                <div className="overflow-x-auto">
                  <table className="min-w-full divide-y divide-border">
                    <thead className="bg-muted/50">
                      <tr>
                        <th className="px-4 py-2 text-left text-xs font-bold text-muted-foreground uppercase">Tipo</th>
                        <th className="px-4 py-2 text-left text-xs font-bold text-muted-foreground uppercase">Descripción</th>
                        <th className="px-4 py-2 text-right text-xs font-bold text-muted-foreground uppercase">Saldo</th>
                        <th className="px-4 py-2 text-right text-xs font-bold text-muted-foreground uppercase">Asignar</th>
                      </tr>
                    </thead>
                    <tbody className="divide-y divide-border">
                      {debtSummary.items.map((item) => {
                        const key = `${item.sourceType}|${item.sourceId}`;
                        return (
                          <tr key={key}>
                            <td className="px-4 py-2 text-sm">{sourceTypeLabel(item.sourceType)}</td>
                            <td className="px-4 py-2 text-sm text-muted-foreground">{item.description}</td>
                            <td className="px-4 py-2 text-right font-mono text-sm">{formatCurrency(item.balance)}</td>
                            <td className="px-4 py-2 text-right">
                              <input
                                type="number"
                                step="0.01"
                                min="0"
                                max={item.balance}
                                value={manualAllocations[key] || ''}
                                onChange={(e) => handleManualAmountChange(item.sourceType, item.sourceId, Number(e.target.value))}
                                className="w-28 bg-transparent border-b border-emerald-600 text-right text-sm focus:outline-none"
                              />
                            </td>
                          </tr>
                        );
                      })}
                    </tbody>
                    <tfoot>
                      <tr>
                        <td colSpan={3} className="px-4 py-2 text-right text-sm font-bold">Total asignado manualmente</td>
                        <td className="px-4 py-2 text-right font-mono text-sm font-bold">{formatCurrency(manualTotal())}</td>
                      </tr>
                    </tfoot>
                  </table>
                </div>
              )}
            </div>
          )}

          <div className="flex justify-end gap-3 mt-6 pt-4 border-t border-border">
            <Button type="button" variant="ghost" onClick={handlePreview} disabled={previewing}>
              {previewing ? <Loader2 className="w-4 h-4 animate-spin mr-2" /> : <Eye className="w-4 h-4 mr-2" />}
              Vista Previa
            </Button>
            <Button type="button" variant="primary" onClick={handleRegister} disabled={submitting}>
              {submitting ? <Loader2 className="w-4 h-4 animate-spin mr-2" /> : <Save className="w-4 h-4 mr-2" />}
              Registrar Pago
            </Button>
          </div>
        </CardContent>
      </Card>

      {preview && (
        <Card>
          <CardHeader>
            <h3 className="font-bold text-foreground">Vista Previa de Asignación</h3>
          </CardHeader>
          <CardContent className="p-0">
            <div className="overflow-x-auto">
              <table className="min-w-full divide-y divide-border">
                <thead className="bg-muted/50">
                  <tr>
                    <th className="px-6 py-4 text-left text-xs font-bold text-muted-foreground uppercase tracking-wider">Tipo</th>
                    <th className="px-6 py-4 text-left text-xs font-bold text-muted-foreground uppercase tracking-wider">Descripción</th>
                    <th className="px-6 py-4 text-right text-xs font-bold text-muted-foreground uppercase tracking-wider">Monto</th>
                  </tr>
                </thead>
                <tbody className="divide-y divide-border">
                  {preview.allocations.map((a, i) => (
                    <tr key={i}>
                      <td className="px-6 py-4 whitespace-nowrap text-sm font-medium text-foreground">{sourceTypeLabel(a.sourceType)}</td>
                      <td className="px-6 py-4 whitespace-nowrap text-sm text-muted-foreground">{a.description}</td>
                      <td className="px-6 py-4 whitespace-nowrap text-right font-mono text-sm">{formatCurrency(a.allocatedAmount)}</td>
                    </tr>
                  ))}
                </tbody>
                <tfoot className="bg-muted/30">
                  <tr>
                    <td colSpan={2} className="px-6 py-4 text-right text-sm font-bold text-foreground">Total Asignado</td>
                    <td className="px-6 py-4 text-right font-mono text-sm font-bold text-foreground">{formatCurrency(preview.totalAllocated)}</td>
                  </tr>
                </tfoot>
              </table>
            </div>
            {preview.advanceAmount > 0 && (
              <div className="p-4 bg-blue-50 border-t border-blue-200 text-blue-700 text-sm flex items-center gap-2">
                <CreditCard className="w-4 h-4 shrink-0" />
                <span>Saldo a favor para la siguiente liquidación: <strong>{formatCurrency(preview.advanceAmount)}</strong></span>
              </div>
            )}
          </CardContent>
        </Card>
      )}

      {result && (
        <Card>
          <CardContent className="p-5">
            <div className="flex items-center gap-3">
              <div className="w-10 h-10 bg-emerald-50 rounded-xl flex items-center justify-center">
                <CheckCircle className="w-5 h-5 text-emerald-600" />
              </div>
              <div>
                <p className="font-bold text-foreground">Pago Registrado</p>
                <p className="text-sm text-muted-foreground">ID: {result.id}</p>
                <p className="text-sm text-muted-foreground">Monto: {formatCurrency(result.amount)}</p>
                {result.advanceAmount > 0 && (
                  <p className="text-sm text-muted-foreground">Saldo a favor para la siguiente liquidación: {formatCurrency(result.advanceAmount)}</p>
                )}
              </div>
            </div>
          </CardContent>
        </Card>
      )}
    </div>
  );
}
