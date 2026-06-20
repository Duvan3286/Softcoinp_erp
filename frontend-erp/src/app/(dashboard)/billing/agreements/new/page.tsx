'use client';

import React, { useState, useEffect } from 'react';
import { useRouter } from 'next/navigation';
import { Loader2, ArrowLeft, Calculator, Save, AlertTriangle, Handshake } from 'lucide-react';
import { Button } from '@/components/ui/Button';
import { Card, CardHeader, CardContent } from '@/components/ui/Card';
import feesPortfolioService, { AgreementSimulation, CreatePaymentAgreementRequest } from '@/lib/fees-portfolio-service';
import { UnitsService as unitsService, Unit } from '@/lib/units-service';

export default function NewAgreementPage() {
  const router = useRouter();
  const [units, setUnits] = useState<Unit[]>([]);
  const [loadingUnits, setLoadingUnits] = useState(true);
  const [submitting, setSubmitting] = useState(false);
  const [simulating, setSimulating] = useState(false);
  const [error, setError] = useState('');
  const [simulation, setSimulation] = useState<AgreementSimulation | null>(null);
  const [created, setCreated] = useState<{ id: string; status: string; installmentAmount: number } | null>(null);

  const [unitId, setUnitId] = useState('');
  const [totalDebtIncluded, setTotalDebtIncluded] = useState<number>(0);
  const [interestForgivenessPercentage, setInterestForgivenessPercentage] = useState<number>(0);
  const [numberOfInstallments, setNumberOfInstallments] = useState<number>(1);
  const [councilActNumber, setCouncilActNumber] = useState('');
  const [digitalAcceptance, setDigitalAcceptance] = useState('');
  const [startDate, setStartDate] = useState(new Date().toISOString().slice(0, 10));

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

  const handleSimulate = async () => {
    setError('');
    setSimulation(null);
    if (!unitId) { setError('Debe seleccionar una unidad.'); return; }
    if (totalDebtIncluded <= 0) { setError('La deuda incluida debe ser mayor a cero.'); return; }
    if (numberOfInstallments < 1) { setError('El número de cuotas debe ser al menos 1.'); return; }
    if (!startDate) { setError('La fecha de inicio es requerida.'); return; }

    setSimulating(true);
    try {
      const data = await feesPortfolioService.simulateAgreement(
        unitId,
        totalDebtIncluded,
        interestForgivenessPercentage,
        numberOfInstallments,
        startDate
      );
      setSimulation(data);
    } catch (err: any) {
      setError(err?.response?.data || 'Error al simular el acuerdo.');
    } finally {
      setSimulating(false);
    }
  };

  const handleCreate = async () => {
    setError('');
    if (!unitId) { setError('Debe seleccionar una unidad.'); return; }
    if (totalDebtIncluded <= 0) { setError('La deuda incluida debe ser mayor a cero.'); return; }
    if (numberOfInstallments < 1) { setError('El número de cuotas debe ser al menos 1.'); return; }
    if (!councilActNumber.trim()) { setError('El número de acta del consejo es requerido.'); return; }
    if (!startDate) { setError('La fecha de inicio es requerida.'); return; }

    setSubmitting(true);
    try {
      const request: CreatePaymentAgreementRequest = {
        unitId,
        totalDebtIncluded,
        numberOfInstallments,
        interestForgivenessPercentage,
        councilActNumber,
        digitalAcceptance,
        startDate,
      };
      const data = await feesPortfolioService.createAgreement(request);
      setCreated(data);
    } catch (err: any) {
      setError(err?.response?.data || 'Error al crear el acuerdo.');
    } finally {
      setSubmitting(false);
    }
  };

  const formatCurrency = (val: number) =>
    new Intl.NumberFormat('es-CO', { style: 'currency', currency: 'COP', minimumFractionDigits: 0 }).format(val);

  if (loadingUnits) {
    return (
      <div className="flex justify-center py-20">
        <Loader2 className="w-8 h-8 animate-spin text-emerald-600" />
      </div>
    );
  }

  if (created) {
    return (
      <div className="space-y-6 max-w-2xl">
        <button onClick={() => router.push('/billing/agreements')} className="inline-flex items-center gap-2 text-sm text-muted-foreground hover:text-foreground transition-colors">
          <ArrowLeft className="w-4 h-4" />
          Volver a Acuerdos de Pago
        </button>
        <Card>
          <CardContent className="p-6 text-center">
            <div className="w-16 h-16 bg-emerald-50 rounded-full flex items-center justify-center mx-auto mb-4">
              <Handshake className="w-8 h-8 text-emerald-600" />
            </div>
            <h2 className="text-xl font-bold text-foreground">Acuerdo Creado Exitosamente</h2>
            <p className="text-sm text-muted-foreground mt-2">ID: {created.id}</p>
            <p className="text-sm text-muted-foreground">Estado: {created.status}</p>
            <p className="text-sm text-muted-foreground">Valor Cuota: {formatCurrency(created.installmentAmount)}</p>
            <div className="mt-6 flex justify-center gap-3">
              <Button variant="secondary" onClick={() => router.push('/billing/agreements')}>Volver a Listado</Button>
              <Button onClick={() => router.push(`/billing/agreements/${created.id}`)}>Ver Acuerdo</Button>
            </div>
          </CardContent>
        </Card>
      </div>
    );
  }

  return (
    <div className="space-y-6 max-w-3xl">
      <button onClick={() => router.push('/billing/agreements')} className="inline-flex items-center gap-2 text-sm text-muted-foreground hover:text-foreground transition-colors">
        <ArrowLeft className="w-4 h-4" />
        Volver a Acuerdos de Pago
      </button>

      <div>
        <h1 className="text-2xl font-bold text-foreground tracking-tight">Nuevo Acuerdo de Pago</h1>
        <p className="text-sm text-muted-foreground mt-1">Crea un acuerdo de pago con un propietario para financiar su deuda.</p>
      </div>

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
                className="w-full bg-transparent border-b border-emerald-600 focus:border-b-2 text-foreground px-0 py-2 text-sm focus:outline-none transition-all"
                required
              >
                <option value="">Seleccione una unidad...</option>
                {units.map((u) => (
                  <option key={u.id} value={u.id}>{u.identifier} - {u.towerOrBlock}</option>
                ))}
              </select>
            </div>
            <div>
              <label className="block text-xs font-bold text-muted-foreground uppercase tracking-widest mb-1.5">Deuda Total Incluida</label>
              <input
                type="number"
                step="0.01"
                min="0"
                placeholder="0.00"
                value={totalDebtIncluded || ''}
                onChange={(e) => setTotalDebtIncluded(Number(e.target.value))}
                className="w-full bg-transparent border-b border-emerald-600 focus:border-b-2 text-foreground px-0 py-2 text-sm focus:outline-none transition-all"
                required
              />
            </div>
            <div>
              <label className="block text-xs font-bold text-muted-foreground uppercase tracking-widest mb-1.5">% Condonación de Intereses</label>
              <div className="flex items-center gap-3">
                <input
                  type="range"
                  min="0"
                  max="100"
                  value={interestForgivenessPercentage}
                  onChange={(e) => setInterestForgivenessPercentage(Number(e.target.value))}
                  className="flex-1 accent-emerald-600"
                />
                <span className="text-sm font-mono font-bold text-foreground w-12 text-right">{interestForgivenessPercentage}%</span>
              </div>
            </div>
            <div>
              <label className="block text-xs font-bold text-muted-foreground uppercase tracking-widest mb-1.5">Número de Cuotas</label>
              <input
                type="number"
                min="1"
                value={numberOfInstallments}
                onChange={(e) => setNumberOfInstallments(Math.max(1, Number(e.target.value)))}
                className="w-full bg-transparent border-b border-emerald-600 focus:border-b-2 text-foreground px-0 py-2 text-sm focus:outline-none transition-all"
                required
              />
            </div>
            <div>
              <label className="block text-xs font-bold text-muted-foreground uppercase tracking-widest mb-1.5">Fecha de Inicio</label>
              <input
                type="date"
                value={startDate}
                onChange={(e) => setStartDate(e.target.value)}
                className="w-full bg-transparent border-b border-emerald-600 focus:border-b-2 text-foreground px-0 py-2 text-sm focus:outline-none transition-all"
                required
              />
            </div>
            <div className="md:col-span-2">
              <label className="block text-xs font-bold text-muted-foreground uppercase tracking-widest mb-1.5">Número de Acta del Consejo</label>
              <input
                type="text"
                placeholder="Ej. Acta 015-2026"
                value={councilActNumber}
                onChange={(e) => setCouncilActNumber(e.target.value)}
                className="w-full bg-transparent border-b border-emerald-600 focus:border-b-2 text-foreground px-0 py-2 text-sm focus:outline-none transition-all"
                required
              />
            </div>
            <div className="md:col-span-2">
              <label className="block text-xs font-bold text-muted-foreground uppercase tracking-widest mb-1.5">Aceptación Digital</label>
              <textarea
                placeholder="Declaración de aceptación del deudor..."
                value={digitalAcceptance}
                onChange={(e) => setDigitalAcceptance(e.target.value)}
                rows={3}
                className="w-full bg-transparent border-b border-emerald-600 focus:border-b-2 text-foreground px-0 py-2 text-sm focus:outline-none transition-all resize-none"
              />
            </div>
          </div>

          <div className="flex justify-end gap-3 mt-6 pt-4 border-t border-border">
            <Button type="button" variant="secondary" onClick={handleSimulate} disabled={simulating}>
              {simulating ? <Loader2 className="w-4 h-4 animate-spin mr-2" /> : <Calculator className="w-4 h-4 mr-2" />}
              Simular
            </Button>
            <Button type="button" variant="primary" onClick={handleCreate} disabled={submitting}>
              {submitting ? <Loader2 className="w-4 h-4 animate-spin mr-2" /> : <Save className="w-4 h-4 mr-2" />}
              Crear Acuerdo
            </Button>
          </div>
        </CardContent>
      </Card>

      {simulation && (
        <Card>
          <CardHeader>
            <h3 className="font-bold text-foreground">Resultado de la Simulación</h3>
          </CardHeader>
          <CardContent>
            <div className="grid grid-cols-2 md:grid-cols-4 gap-4 mb-6">
              <div>
                <p className="text-xs text-muted-foreground font-medium">Deuda Total</p>
                <p className="text-lg font-bold text-foreground">{formatCurrency(simulation.totalDebt)}</p>
              </div>
              <div>
                <p className="text-xs text-muted-foreground font-medium">% Condonación</p>
                <p className="text-lg font-bold text-foreground">{simulation.interestForgivenessPercentage}%</p>
              </div>
              <div>
                <p className="text-xs text-muted-foreground font-medium">Monto Condonado</p>
                <p className="text-lg font-bold text-emerald-600">{formatCurrency(simulation.forgivenAmount)}</p>
              </div>
              <div>
                <p className="text-xs text-muted-foreground font-medium">Deuda Neta</p>
                <p className="text-lg font-bold text-foreground">{formatCurrency(simulation.netDebt)}</p>
              </div>
            </div>
            <div>
              <p className="text-xs text-muted-foreground font-medium mb-2">Valor Cuota: <strong className="text-foreground">{formatCurrency(simulation.installmentAmount)}</strong> &middot; {simulation.numberOfInstallments} cuotas</p>
            </div>
            <div className="overflow-x-auto">
              <table className="min-w-full divide-y divide-border text-sm">
                <thead className="bg-muted/50">
                  <tr>
                    <th className="px-4 py-3 text-left text-xs font-bold text-muted-foreground uppercase">No.</th>
                    <th className="px-4 py-3 text-left text-xs font-bold text-muted-foreground uppercase">Fecha Venc.</th>
                    <th className="px-4 py-3 text-right text-xs font-bold text-muted-foreground uppercase">Monto</th>
                  </tr>
                </thead>
                <tbody className="divide-y divide-border">
                  {simulation.installments.map((inst) => (
                    <tr key={inst.number}>
                      <td className="px-4 py-3 whitespace-nowrap font-semibold">{inst.number}</td>
                      <td className="px-4 py-3 whitespace-nowrap text-muted-foreground">{new Date(inst.dueDate).toLocaleDateString('es-CO')}</td>
                      <td className="px-4 py-3 whitespace-nowrap text-right font-mono">{formatCurrency(inst.amount)}</td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>
          </CardContent>
        </Card>
      )}
    </div>
  );
}
