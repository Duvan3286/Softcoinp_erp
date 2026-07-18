'use client';

import React, { useState, useEffect } from 'react';
import { useRouter, useSearchParams } from 'next/navigation';
import { Loader2, ArrowLeft, Eye, Save, AlertTriangle, CheckCircle, CreditCard } from 'lucide-react';
import { Button } from '@/components/ui/Button';
import { Card, CardHeader, CardContent } from '@/components/ui/Card';
import feesPortfolioService, { PaymentPreview, RegisterPaymentRequest } from '@/lib/fees-portfolio-service';
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

  const handlePreview = async () => {
    setError('');
    setPreview(null);
    setResult(null);
    if (!unitId) { setError('Debe seleccionar una unidad.'); return; }
    if (amount <= 0) { setError('El monto debe ser mayor a cero.'); return; }
    if (!paymentDate) { setError('La fecha de pago es requerida.'); return; }
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
    if (amount <= 0) { setError('El monto debe ser mayor a cero.'); return; }
    if (!paymentDate) { setError('La fecha de pago es requerida.'); return; }

    setSubmitting(true);
    try {
      const request: RegisterPaymentRequest = {
        unitId,
        paymentDate,
        amount,
        paymentMethod,
        referenceNumber,
        notes,
      };
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
          </div>

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
