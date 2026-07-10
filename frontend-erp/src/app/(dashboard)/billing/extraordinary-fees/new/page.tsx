'use client';

import React, { useState } from 'react';
import { useRouter } from 'next/navigation';
import { Loader2, DollarSign, ArrowLeft, Save, AlertTriangle } from 'lucide-react';
import { Button } from '@/components/ui/Button';
import { Card, CardHeader, CardContent } from '@/components/ui/Card';
import feesPortfolioService from '@/lib/fees-portfolio-service';

export default function NewExtraordinaryFeePage() {
  const router = useRouter();
  const [submitting, setSubmitting] = useState(false);
  const [error, setError] = useState('');
  const [name, setName] = useState('');
  const [totalAmount, setTotalAmount] = useState<number>(0);
  const [distributionType, setDistributionType] = useState('Equal');
  const [dueDate, setDueDate] = useState('');
  const [startPeriod, setStartPeriod] = useState('');
  const [numberOfInstallments, setNumberOfInstallments] = useState<number>(1);
  const [notes, setNotes] = useState('');

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setError('');
    if (!name.trim()) { setError('El nombre es requerido.'); return; }
    if (totalAmount <= 0) { setError('El monto total debe ser mayor a cero.'); return; }
    if (!dueDate) { setError('La fecha de vencimiento es requerida.'); return; }
    if (!startPeriod) { setError('El período de inicio es requerido.'); return; }
    if (numberOfInstallments < 1) { setError('El número de cuotas debe ser al menos 1.'); return; }

    setSubmitting(true);
    try {
      const result = await feesPortfolioService.createExtraordinaryFee({
        name,
        totalAmount,
        distributionType,
        dueDate,
        startPeriod,
        numberOfInstallments,
        notes,
      });
      router.push(`/billing/extraordinary-fees/${result.id}`);
    } catch (err: any) {
      setError(err?.response?.data || 'Error al crear la cuota extraordinaria.');
    } finally {
      setSubmitting(false);
    }
  };

  const distOptions = [
    { value: 'Equal', label: 'Igualitaria' },
    { value: 'ByCoefficient', label: 'Por Coeficiente' },
    { value: 'Custom', label: 'Personalizada' },
  ];

  return (
    <div className="space-y-6 max-w-2xl">
      <button onClick={() => router.push('/billing/extraordinary-fees')} className="inline-flex items-center gap-2 text-sm text-muted-foreground hover:text-foreground transition-colors">
        <ArrowLeft className="w-4 h-4" />
        Volver a Cuotas Extraordinarias
      </button>

      <div>
        <h1 className="text-2xl font-bold text-foreground tracking-tight">Nueva Cuota Extraordinaria</h1>
        <p className="text-sm text-muted-foreground mt-1">Registra una nueva cuota extraordinaria aprobada por la asamblea.</p>
      </div>

      <Card>
        <CardContent className="p-6">
          <form onSubmit={handleSubmit} className="space-y-6">
            <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
              <div className="md:col-span-2">
                <label className="block text-xs font-bold text-muted-foreground uppercase tracking-widest mb-1.5">Nombre</label>
                <input
                  type="text"
                  placeholder="Ej. Cuota Extraordinaria Parque Infantil"
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
                <label className="block text-xs font-bold text-muted-foreground uppercase tracking-widest mb-1.5">Tipo de Distribución</label>
                <select
                  value={distributionType}
                  onChange={(e) => setDistributionType(e.target.value)}
                  className="w-full bg-transparent border-b border-emerald-600 focus:border-b-2 text-foreground pl-0 pr-6 py-2 text-sm focus:outline-none transition-all"
                >
                  {distOptions.map((opt) => (
                    <option key={opt.value} value={opt.value}>{opt.label}</option>
                  ))}
                </select>
              </div>
              <div>
                <label className="block text-xs font-bold text-muted-foreground uppercase tracking-widest mb-1.5">Fecha de Vencimiento</label>
                <input
                  type="date"
                  value={dueDate}
                  onChange={(e) => setDueDate(e.target.value)}
                  className="w-full bg-transparent border-b border-emerald-600 focus:border-b-2 text-foreground pl-0 pr-6 py-2 text-sm focus:outline-none transition-all"
                  required
                />
              </div>
              <div>
                <label className="block text-xs font-bold text-muted-foreground uppercase tracking-widest mb-1.5">Período de Inicio</label>
                <input
                  type="month"
                  value={startPeriod}
                  onChange={(e) => setStartPeriod(e.target.value)}
                  className="w-full bg-transparent border-b border-emerald-600 focus:border-b-2 text-foreground pl-0 pr-6 py-2 text-sm focus:outline-none transition-all"
                  required
                />
              </div>
              <div>
                <label className="block text-xs font-bold text-muted-foreground uppercase tracking-widest mb-1.5">Número de Cuotas</label>
                <input
                  type="number"
                  min="1"
                  value={numberOfInstallments}
                  onChange={(e) => setNumberOfInstallments(Math.max(1, Number(e.target.value)))}
                  className="w-full bg-transparent border-b border-emerald-600 focus:border-b-2 text-foreground pl-0 pr-6 py-2 text-sm focus:outline-none transition-all"
                  required
                />
              </div>
              <div className="md:col-span-2">
                <label className="block text-xs font-bold text-muted-foreground uppercase tracking-widest mb-1.5">Notas</label>
                <textarea
                  placeholder="Notas adicionales..."
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
                Crear Cuota Extraordinaria
              </Button>
            </div>
          </form>
        </CardContent>
      </Card>
    </div>
  );
}
