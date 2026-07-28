'use client';

import React, { useState, useEffect } from 'react';
import { Loader2, Plus, AlertTriangle, Trash2, Percent, Settings, Ban, Calculator, ScrollText, CheckCircle2, XCircle, DollarSign, CalendarDays, FileDown } from 'lucide-react';
import { Button } from '@/components/ui/Button';
import { Card, CardHeader, CardContent } from '@/components/ui/Card';
import interestService, {
  MonthlyInterestRateDto,
  LateInterestConfigurationDto,
  UnitInterestExceptionDto,
  AccruedInterestDto,
  InterestCheckResult,
  InterestReportDto,
} from '@/lib/interest-service';
import { UnitsService, Unit, formatUnitLabel } from '@/lib/units-service';

type Tab = 'rates' | 'configuration' | 'exceptions' | 'accrued' | 'reports';

export default function InterestPage() {
  const [activeTab, setActiveTab] = useState<Tab>('rates');

  return (
    <div className="space-y-6">
      <div className="flex flex-col sm:flex-row justify-between items-start sm:items-center gap-4">
        <div>
          <h1 className="text-2xl font-bold text-foreground tracking-tight">Intereses de Mora</h1>
          <p className="text-sm text-muted-foreground mt-1">Gestión de tasas, configuración y cálculo de intereses sobre cartera vencida.</p>
        </div>
      </div>

      <div className="flex gap-1 bg-muted/50 rounded-xl p-1 w-fit border border-border">
        <TabButton active={activeTab === 'rates'} onClick={() => setActiveTab('rates')} icon={<Percent className="w-4 h-4" />} label="Tasas" />
        <TabButton active={activeTab === 'configuration'} onClick={() => setActiveTab('configuration')} icon={<Settings className="w-4 h-4" />} label="Configuración" />
        <TabButton active={activeTab === 'exceptions'} onClick={() => setActiveTab('exceptions')} icon={<Ban className="w-4 h-4" />} label="Excepciones" />
        <TabButton active={activeTab === 'accrued'} onClick={() => setActiveTab('accrued')} icon={<Calculator className="w-4 h-4" />} label="Intereses Acumulados" />
        <TabButton active={activeTab === 'reports'} onClick={() => setActiveTab('reports')} icon={<FileDown className="w-4 h-4" />} label="Reportes" />
      </div>

      {activeTab === 'rates' && <RatesTab />}
      {activeTab === 'configuration' && <ConfigurationTab />}
      {activeTab === 'exceptions' && <ExceptionsTab />}
      {activeTab === 'accrued' && <AccruedTab />}
      {activeTab === 'reports' && <ReportsTab />}
    </div>
  );
}

function TabButton({ active, onClick, icon, label }: { active: boolean; onClick: () => void; icon: React.ReactNode; label: string }) {
  return (
    <button
      onClick={onClick}
      className={`flex items-center gap-2 px-4 py-2 rounded-lg text-sm font-semibold transition-colors ${
        active ? 'bg-card text-foreground shadow-sm border border-border' : 'text-muted-foreground hover:text-foreground'
      }`}
    >
      {icon}
      {label}
    </button>
  );
}

// ── Rates Tab ─────────────────────────────────────────────────────────────────

function RatesTab() {
  const [rates, setRates] = useState<MonthlyInterestRateDto[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');
  const [showModal, setShowModal] = useState(false);
  const [submitting, setSubmitting] = useState(false);
  const [year, setYear] = useState(new Date().getFullYear().toString());
  const [month, setMonth] = useState((new Date().getMonth() + 1).toString().padStart(2, '0'));
  const [certifiedRate, setCertifiedRate] = useState('');
  const [appliedRate, setAppliedRate] = useState('');
  const [maxAllowed, setMaxAllowed] = useState<number | null>(null);
  const [missingCheck, setMissingCheck] = useState<InterestCheckResult | null>(null);
  const [successMsg, setSuccessMsg] = useState('');

  useEffect(() => { fetchRates(); }, []);

  const fetchRates = async () => {
    setLoading(true);
    setError('');
    try {
      const [ratesData, checkData] = await Promise.all([
        interestService.getRates(),
        interestService.checkMissingRates(),
      ]);
      setRates(ratesData);
      setMissingCheck(checkData);
    } catch {
      setError('Error al cargar las tasas.');
    } finally {
      setLoading(false);
    }
  };

  const handleOpenModal = () => {
    setYear(new Date().getFullYear().toString());
    setMonth((new Date().getMonth() + 1).toString().padStart(2, '0'));
    setCertifiedRate('');
    setAppliedRate('');
    setMaxAllowed(null);
    setError('');
    setSuccessMsg('');
    setShowModal(true);
  };

  const handleCertifiedRateChange = (value: string) => {
    setCertifiedRate(value);
    const parsed = parseFloat(value);
    if (!isNaN(parsed) && parsed > 0) {
      setMaxAllowed(parseFloat((parsed * 1.5).toFixed(4)));
    } else {
      setMaxAllowed(null);
    }
  };

  const handleSubmit = async () => {
    setError('');
    setSuccessMsg('');
    setSubmitting(true);
    try {
      const result = await interestService.registerRate({
        year: parseInt(year),
        month: parseInt(month),
        certifiedRate: parseFloat(certifiedRate),
        appliedRate: parseFloat(appliedRate),
      });
      setSuccessMsg(result.message);
      setShowModal(false);
      fetchRates();
    } catch (err: any) {
      const msg = err?.response?.data?.errors?.[0] || err?.response?.data?.title || 'Error al registrar la tasa.';
      setError(msg);
    } finally {
      setSubmitting(false);
    }
  };

  const handleDelete = async (id: string) => {
    if (!window.confirm('¿Eliminar esta tasa? No se puede eliminar si tiene intereses registrados.')) return;
    try {
      await interestService.deleteRate(id);
      fetchRates();
    } catch {
      setError('Error al eliminar la tasa.');
    }
  };

  if (loading) return <div className="flex justify-center py-12"><Loader2 className="w-8 h-8 animate-spin text-emerald-600" /></div>;

  return (
    <div className="space-y-4">
      {error && (
        <div className="p-3 bg-rose-50 border border-rose-200 rounded-lg text-rose-700 text-xs flex items-center gap-2">
          <AlertTriangle className="w-4 h-4 shrink-0" />{error}
        </div>
      )}
      {successMsg && (
        <div className="p-3 bg-emerald-50 border border-emerald-200 rounded-lg text-emerald-700 text-xs flex items-center gap-2">
          <CheckCircle2 className="w-4 h-4 shrink-0" />{successMsg}
        </div>
      )}

      {missingCheck && !missingCheck.hasRateForCurrentPeriod && missingCheck.alertEnabled && (
        <div className="p-3 bg-amber-50 border border-amber-200 rounded-lg text-amber-700 text-xs flex items-center gap-2">
          <AlertTriangle className="w-4 h-4 shrink-0" />{missingCheck.message}
        </div>
      )}

      <div className="flex justify-end">
        <Button onClick={handleOpenModal}>
          <Plus className="w-4 h-4 mr-2" /> Registrar Tasa
        </Button>
      </div>

      <Card>
        <CardContent className="p-0">
          <div className="overflow-x-auto">
            <table className="min-w-full divide-y divide-border">
              <thead className="bg-muted/50">
                <tr>
                  <th className="px-6 py-4 text-left text-xs font-bold text-muted-foreground uppercase tracking-wider">Período</th>
                  <th className="px-6 py-4 text-left text-xs font-bold text-muted-foreground uppercase tracking-wider">Tasa Certificada</th>
                  <th className="px-6 py-4 text-left text-xs font-bold text-muted-foreground uppercase tracking-wider">Tasa Aplicada</th>
                  <th className="px-6 py-4 text-left text-xs font-bold text-muted-foreground uppercase tracking-wider">Máx. Permitido</th>
                  <th className="px-6 py-4 text-left text-xs font-bold text-muted-foreground uppercase tracking-wider">Registrada</th>
                  <th className="px-6 py-4 text-left text-xs font-bold text-muted-foreground uppercase tracking-wider">Registrado Por</th>
                  <th className="px-6 py-4 text-right text-xs font-bold text-muted-foreground uppercase tracking-wider">Acción</th>
                </tr>
              </thead>
              <tbody className="divide-y divide-border">
                {rates.length === 0 && (
                  <tr>
                    <td colSpan={7} className="px-6 py-12 text-center text-sm text-muted-foreground">No hay tasas registradas.</td>
                  </tr>
                )}
                {rates.map(rate => (
                  <tr key={rate.id} className="hover:bg-muted/30 transition-colors">
                    <td className="px-6 py-4 text-sm font-medium">{rate.year}-{rate.month.toString().padStart(2, '0')}</td>
                    <td className="px-6 py-4 text-sm">{rate.certifiedRate.toFixed(4)}%</td>
                    <td className="px-6 py-4 text-sm">{rate.appliedRate.toFixed(4)}%</td>
                    <td className="px-6 py-4 text-sm text-muted-foreground">{rate.maxAllowedRate.toFixed(4)}%</td>
                    <td className="px-6 py-4 text-sm text-muted-foreground">{new Date(rate.registeredAt).toLocaleDateString()}</td>
                    <td className="px-6 py-4 text-sm text-muted-foreground font-mono">{rate.registeredByUserId}</td>
                    <td className="px-6 py-4 text-right">
                      <button
                        onClick={() => handleDelete(rate.id)}
                        className="text-rose-600 hover:text-rose-800 text-sm font-semibold px-3 py-1.5 bg-rose-50 rounded-lg hover:bg-rose-100"
                      >
                        <Trash2 className="w-4 h-4" />
                      </button>
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        </CardContent>
      </Card>

      {showModal && (
        <div className="fixed inset-0 bg-black/60 backdrop-blur-sm z-[150] flex items-center justify-center p-4">
          <div className="bg-card w-full max-w-md rounded-xl border border-border shadow-lg">
            <div className="p-6 border-b border-border">
              <h2 className="text-lg font-bold">Registrar Tasa de Interés</h2>
            </div>
            <div className="p-6 space-y-4">
              <div className="grid grid-cols-2 gap-4">
                <div>
                  <label className="block text-xs font-semibold text-muted-foreground mb-1">Año</label>
                  <input
                    type="number"
                    value={year}
                    onChange={e => setYear(e.target.value)}
                    className="w-full bg-transparent border-b border-emerald-600 focus:border-b-2 outline-none py-2 text-sm"
                  />
                </div>
                <div>
                  <label className="block text-xs font-semibold text-muted-foreground mb-1">Mes</label>
                  <select
                    value={month}
                    onChange={e => setMonth(e.target.value)}
                    className="w-full bg-transparent border border-border rounded-lg p-2 text-sm"
                  >
                    {Array.from({ length: 12 }, (_, i) => (
                      <option key={i + 1} value={(i + 1).toString().padStart(2, '0')}>
                        {(i + 1).toString().padStart(2, '0')}
                      </option>
                    ))}
                  </select>
                </div>
              </div>

              <div>
                <label className="block text-xs font-semibold text-muted-foreground mb-1">Tasa Certificada (%)</label>
                <input
                  type="number"
                  step="0.0001"
                  value={certifiedRate}
                  onChange={e => handleCertifiedRateChange(e.target.value)}
                  className="w-full bg-transparent border-b border-emerald-600 focus:border-b-2 outline-none py-2 text-sm"
                  placeholder="Ej: 3.2500"
                />
              </div>

              {maxAllowed !== null && (
                <p className="text-xs text-muted-foreground">Máximo permitido (1.5x): {maxAllowed.toFixed(4)}%</p>
              )}

              <div>
                <label className="block text-xs font-semibold text-muted-foreground mb-1">Tasa Aplicada (%)</label>
                <input
                  type="number"
                  step="0.0001"
                  value={appliedRate}
                  onChange={e => setAppliedRate(e.target.value)}
                  className="w-full bg-transparent border-b border-emerald-600 focus:border-b-2 outline-none py-2 text-sm"
                  placeholder="Ej: 3.0000"
                />
              </div>
            </div>
            <div className="p-6 border-t border-border flex justify-end gap-3">
              <Button variant="ghost" onClick={() => setShowModal(false)}>Cancelar</Button>
              <Button onClick={handleSubmit} disabled={submitting}>
                {submitting ? <Loader2 className="w-4 h-4 animate-spin mr-2" /> : null}
                Guardar
              </Button>
            </div>
          </div>
        </div>
      )}
    </div>
  );
}

// ── Configuration Tab ─────────────────────────────────────────────────────────

function ConfigurationTab() {
  const [config, setConfig] = useState<LateInterestConfigurationDto | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');
  const [saving, setSaving] = useState(false);
  const [saved, setSaved] = useState(false);
  const [interestStartDays, setInterestStartDays] = useState('15');
  const [applyToAll, setApplyToAll] = useState(true);
  const [alertMissing, setAlertMissing] = useState(true);

  useEffect(() => { fetchConfig(); }, []);

  const fetchConfig = async () => {
    setLoading(true);
    setError('');
    try {
      const data = await interestService.getConfiguration();
      setConfig(data);
      setInterestStartDays(data.interestStartDays.toString());
      setApplyToAll(data.applyToAllUnitsByDefault);
      setAlertMissing(data.alertOnMissingMonthlyRate);
    } catch {
      setConfig(null);
    } finally {
      setLoading(false);
    }
  };

  const handleSave = async () => {
    setSaving(true);
    setError('');
    setSaved(false);
    try {
      await interestService.updateConfiguration({
        interestStartDays: parseInt(interestStartDays),
        applyToAllUnitsByDefault: applyToAll,
        alertOnMissingMonthlyRate: alertMissing,
      });
      setSaved(true);
      fetchConfig();
    } catch {
      setError('Error al guardar la configuración.');
    } finally {
      setSaving(false);
    }
  };

  if (loading) return <div className="flex justify-center py-12"><Loader2 className="w-8 h-8 animate-spin text-emerald-600" /></div>;

  return (
    <div className="space-y-4">
      {error && (
        <div className="p-3 bg-rose-50 border border-rose-200 rounded-lg text-rose-700 text-xs flex items-center gap-2">
          <AlertTriangle className="w-4 h-4 shrink-0" />{error}
        </div>
      )}
      {saved && (
        <div className="p-3 bg-emerald-50 border border-emerald-200 rounded-lg text-emerald-700 text-xs flex items-center gap-2">
          <CheckCircle2 className="w-4 h-4 shrink-0" />Configuración guardada exitosamente.
        </div>
      )}

      <Card>
        <CardContent className="p-6 space-y-6">
          <div>
            <label className="block text-sm font-semibold mb-1">Días de Gracia</label>
            <p className="text-xs text-muted-foreground mb-2">Número de días después del vencimiento antes de comenzar a generar intereses.</p>
            <input
              type="number"
              min="0"
              value={interestStartDays}
              onChange={e => setInterestStartDays(e.target.value)}
              className="w-full max-w-xs bg-transparent border-b border-emerald-600 focus:border-b-2 outline-none py-2 text-sm"
            />
          </div>

          <div className="flex items-center gap-3">
            <input
              type="checkbox"
              id="applyToAll"
              checked={applyToAll}
              onChange={e => setApplyToAll(e.target.checked)}
              className="w-4 h-4 accent-emerald-600"
            />
            <label htmlFor="applyToAll" className="text-sm">Aplicar a todas las unidades por defecto</label>
          </div>

          <div className="flex items-center gap-3">
            <input
              type="checkbox"
              id="alertMissing"
              checked={alertMissing}
              onChange={e => setAlertMissing(e.target.checked)}
              className="w-4 h-4 accent-emerald-600"
            />
            <label htmlFor="alertMissing" className="text-sm">Alertar cuando no haya tasa registrada para el mes actual</label>
          </div>

          <div className="flex justify-end">
            <Button onClick={handleSave} disabled={saving}>
              {saving ? <Loader2 className="w-4 h-4 animate-spin mr-2" /> : null}
              Guardar Configuración
            </Button>
          </div>
        </CardContent>
      </Card>
    </div>
  );
}

// ── Exceptions Tab ────────────────────────────────────────────────────────────

function ExceptionsTab() {
  const [exceptions, setExceptions] = useState<UnitInterestExceptionDto[]>([]);
  const [units, setUnits] = useState<Unit[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');
  const [showModal, setShowModal] = useState(false);
  const [submitting, setSubmitting] = useState(false);
  const [selectedUnitId, setSelectedUnitId] = useState('');
  const [exceptionDays, setExceptionDays] = useState('30');
  const [reason, setReason] = useState('');

  useEffect(() => { fetchData(); }, []);

  const fetchData = async () => {
    setLoading(true);
    setError('');
    try {
      const [excData, unitsData] = await Promise.all([
        interestService.getExceptions(),
        UnitsService.getUnits(),
      ]);
      setExceptions(excData);
      setUnits(unitsData);
    } catch {
      setError('Error al cargar las excepciones.');
    } finally {
      setLoading(false);
    }
  };

  const handleOpenModal = () => {
    setSelectedUnitId('');
    setExceptionDays('30');
    setReason('');
    setError('');
    setShowModal(true);
  };

  const handleSubmit = async () => {
    setError('');
    if (!selectedUnitId) { setError('Seleccione una unidad.'); return; }
    if (!reason.trim()) { setError('Ingrese una razón.'); return; }
    setSubmitting(true);
    try {
      await interestService.upsertException({
        unitId: selectedUnitId,
        interestStartDays: parseInt(exceptionDays),
        reason: reason.trim(),
      });
      setShowModal(false);
      fetchData();
    } catch {
      setError('Error al guardar la excepción.');
    } finally {
      setSubmitting(false);
    }
  };

  const handleDelete = async (id: string) => {
    if (!window.confirm('¿Eliminar esta excepción?')) return;
    try {
      await interestService.deleteException(id);
      fetchData();
    } catch {
      setError('Error al eliminar la excepción.');
    }
  };

  if (loading) return <div className="flex justify-center py-12"><Loader2 className="w-8 h-8 animate-spin text-emerald-600" /></div>;

  const existingUnitIds = new Set(exceptions.map(e => e.unitId));
  const availableUnits = units.filter(u => !existingUnitIds.has(u.id));

  return (
    <div className="space-y-4">
      {error && (
        <div className="p-3 bg-rose-50 border border-rose-200 rounded-lg text-rose-700 text-xs flex items-center gap-2">
          <AlertTriangle className="w-4 h-4 shrink-0" />{error}
        </div>
      )}

      <div className="flex justify-end">
        <Button onClick={handleOpenModal}>
          <Plus className="w-4 h-4 mr-2" /> Nueva Excepción
        </Button>
      </div>

      <Card>
        <CardContent className="p-0">
          <div className="overflow-x-auto">
            <table className="min-w-full divide-y divide-border">
              <thead className="bg-muted/50">
                <tr>
                  <th className="px-6 py-4 text-left text-xs font-bold text-muted-foreground uppercase tracking-wider">Unidad</th>
                  <th className="px-6 py-4 text-left text-xs font-bold text-muted-foreground uppercase tracking-wider">Días de Gracia</th>
                  <th className="px-6 py-4 text-left text-xs font-bold text-muted-foreground uppercase tracking-wider">Razón</th>
                  <th className="px-6 py-4 text-right text-xs font-bold text-muted-foreground uppercase tracking-wider">Acción</th>
                </tr>
              </thead>
              <tbody className="divide-y divide-border">
                {exceptions.length === 0 && (
                  <tr>
                    <td colSpan={4} className="px-6 py-12 text-center text-sm text-muted-foreground">No hay excepciones registradas.</td>
                  </tr>
                )}
                {exceptions.map(exc => (
                  <tr key={exc.id} className="hover:bg-muted/30 transition-colors">
                    <td className="px-6 py-4 text-sm font-medium">{exc.unitIdentifier}</td>
                    <td className="px-6 py-4 text-sm">{exc.interestStartDays} días</td>
                    <td className="px-6 py-4 text-sm text-muted-foreground">{exc.reason}</td>
                    <td className="px-6 py-4 text-right">
                      <button
                        onClick={() => handleDelete(exc.id)}
                        className="text-rose-600 hover:text-rose-800 text-sm font-semibold px-3 py-1.5 bg-rose-50 rounded-lg hover:bg-rose-100"
                      >
                        <Trash2 className="w-4 h-4" />
                      </button>
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        </CardContent>
      </Card>

      {showModal && (
        <div className="fixed inset-0 bg-black/60 backdrop-blur-sm z-[150] flex items-center justify-center p-4">
          <div className="bg-card w-full max-w-md rounded-xl border border-border shadow-lg">
            <div className="p-6 border-b border-border">
              <h2 className="text-lg font-bold">Nueva Excepción por Unidad</h2>
            </div>
            <div className="p-6 space-y-4">
              <div>
                <label className="block text-xs font-semibold text-muted-foreground mb-1">Unidad</label>
                <select
                  value={selectedUnitId}
                  onChange={e => setSelectedUnitId(e.target.value)}
                  className="w-full bg-transparent border border-border rounded-lg p-2 text-sm"
                >
                  <option value="">Seleccione una unidad...</option>
                  {availableUnits.map(u => (
                    <option key={u.id} value={u.id}>{formatUnitLabel(u.identifier, u.towerOrBlock)}</option>
                  ))}
                </select>
              </div>

              <div>
                <label className="block text-xs font-semibold text-muted-foreground mb-1">Días de Gracia</label>
                <input
                  type="number"
                  min="0"
                  value={exceptionDays}
                  onChange={e => setExceptionDays(e.target.value)}
                  className="w-full bg-transparent border-b border-emerald-600 focus:border-b-2 outline-none py-2 text-sm"
                />
              </div>

              <div>
                <label className="block text-xs font-semibold text-muted-foreground mb-1">Razón</label>
                <textarea
                  value={reason}
                  onChange={e => setReason(e.target.value)}
                  className="w-full bg-transparent border border-border rounded-lg p-2 text-sm resize-none"
                  rows={3}
                  placeholder="Ej: Convenio especial de pago..."
                />
              </div>
            </div>
            <div className="p-6 border-t border-border flex justify-end gap-3">
              <Button variant="ghost" onClick={() => setShowModal(false)}>Cancelar</Button>
              <Button onClick={handleSubmit} disabled={submitting}>
                {submitting ? <Loader2 className="w-4 h-4 animate-spin mr-2" /> : null}
                Guardar
              </Button>
            </div>
          </div>
        </div>
      )}
    </div>
  );
}

// ── Accrued Interests Tab ─────────────────────────────────────────────────────

function AccruedTab() {
  const [units, setUnits] = useState<Unit[]>([]);
  const [selectedUnitId, setSelectedUnitId] = useState('');
  const [interests, setInterests] = useState<AccruedInterestDto[]>([]);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState('');
  const [calculating, setCalculating] = useState(false);
  const [calcResult, setCalcResult] = useState<string | null>(null);

  useEffect(() => {
    UnitsService.getUnits().then(setUnits).catch(() => {});
  }, []);

  const fetchInterests = async (unitId: string) => {
    setLoading(true);
    setError('');
    setCalcResult(null);
    try {
      const data = await interestService.getAccruedInterests(unitId);
      setInterests(data);
    } catch {
      setError('Error al cargar los intereses acumulados.');
      setInterests([]);
    } finally {
      setLoading(false);
    }
  };

  const handleUnitChange = (unitId: string) => {
    setSelectedUnitId(unitId);
    if (unitId) fetchInterests(unitId);
  };

  const handleCalculate = async () => {
    if (!selectedUnitId) return;
    setCalculating(true);
    setError('');
    setCalcResult(null);
    try {
      const result = await interestService.calculateInterests(selectedUnitId);
      setCalcResult(result.message);
      fetchInterests(selectedUnitId);
    } catch {
      setError('Error al calcular intereses.');
    } finally {
      setCalculating(false);
    }
  };

  const totalCalculated = interests.reduce((sum, i) => sum + i.calculatedAmount, 0);
  const totalBalance = interests.reduce((sum, i) => sum + i.balanceAmount, 0);

  return (
    <div className="space-y-4">
      {error && (
        <div className="p-3 bg-rose-50 border border-rose-200 rounded-lg text-rose-700 text-xs flex items-center gap-2">
          <AlertTriangle className="w-4 h-4 shrink-0" />{error}
        </div>
      )}
      {calcResult && (
        <div className="p-3 bg-emerald-50 border border-emerald-200 rounded-lg text-emerald-700 text-xs flex items-center gap-2">
          <CheckCircle2 className="w-4 h-4 shrink-0" />{calcResult}
        </div>
      )}

      <div className="flex flex-col sm:flex-row gap-4">
        <div className="flex-1">
          <label className="block text-xs font-semibold text-muted-foreground mb-1">Unidad</label>
          <select
            value={selectedUnitId}
            onChange={e => handleUnitChange(e.target.value)}
            className="w-full bg-transparent border border-border rounded-lg p-2 text-sm"
          >
            <option value="">Seleccione una unidad...</option>
            {units.map(u => (
              <option key={u.id} value={u.id}>{formatUnitLabel(u.identifier, u.towerOrBlock)}</option>
            ))}
          </select>
        </div>
        <div className="flex items-end">
          <Button onClick={handleCalculate} disabled={!selectedUnitId || calculating}>
            {calculating ? <Loader2 className="w-4 h-4 animate-spin mr-2" /> : <Calculator className="w-4 h-4 mr-2" />}
            Calcular Intereses
          </Button>
        </div>
      </div>

      {interests.length > 0 && (
        <div className="grid grid-cols-2 sm:grid-cols-4 gap-4">
          <Card>
            <CardContent className="p-4 flex items-center gap-3">
              <DollarSign className="w-5 h-5 text-emerald-600 shrink-0" />
              <div>
                <p className="text-xs text-muted-foreground">Total Intereses</p>
                <p className="text-lg font-bold">${totalCalculated.toLocaleString()}</p>
              </div>
            </CardContent>
          </Card>
          <Card>
            <CardContent className="p-4 flex items-center gap-3">
              <DollarSign className="w-5 h-5 text-amber-600 shrink-0" />
              <div>
                <p className="text-xs text-muted-foreground">Saldo Pendiente</p>
                <p className="text-lg font-bold">${totalBalance.toLocaleString()}</p>
              </div>
            </CardContent>
          </Card>
          <Card>
            <CardContent className="p-4 flex items-center gap-3">
              <ScrollText className="w-5 h-5 text-blue-600 shrink-0" />
              <div>
                <p className="text-xs text-muted-foreground">Registros</p>
                <p className="text-lg font-bold">{interests.length}</p>
              </div>
            </CardContent>
          </Card>
          <Card>
            <CardContent className="p-4 flex items-center gap-3">
              <CheckCircle2 className="w-5 h-5 text-emerald-600 shrink-0" />
              <div>
                <p className="text-xs text-muted-foreground">Pagados</p>
                <p className="text-lg font-bold">{interests.filter(i => i.status === 'Paid').length}</p>
              </div>
            </CardContent>
          </Card>
        </div>
      )}

      <Card>
        <CardContent className="p-0">
          {loading ? (
            <div className="flex justify-center py-12"><Loader2 className="w-8 h-8 animate-spin text-emerald-600" /></div>
          ) : (
            <div className="overflow-x-auto">
              <table className="min-w-full divide-y divide-border">
                <thead className="bg-muted/50">
                  <tr>
                    <th className="px-6 py-4 text-left text-xs font-bold text-muted-foreground uppercase tracking-wider">Período</th>
                    <th className="px-6 py-4 text-left text-xs font-bold text-muted-foreground uppercase tracking-wider">Base</th>
                    <th className="px-6 py-4 text-left text-xs font-bold text-muted-foreground uppercase tracking-wider">Tasa Diaria</th>
                    <th className="px-6 py-4 text-left text-xs font-bold text-muted-foreground uppercase tracking-wider">Días</th>
                    <th className="px-6 py-4 text-left text-xs font-bold text-muted-foreground uppercase tracking-wider">Calculado</th>
                    <th className="px-6 py-4 text-left text-xs font-bold text-muted-foreground uppercase tracking-wider">Saldo</th>
                    <th className="px-6 py-4 text-left text-xs font-bold text-muted-foreground uppercase tracking-wider">Estado</th>
                    <th className="px-6 py-4 text-left text-xs font-bold text-muted-foreground uppercase tracking-wider">Período</th>
                  </tr>
                </thead>
                <tbody className="divide-y divide-border">
                  {!selectedUnitId && (
                    <tr>
                      <td colSpan={8} className="px-6 py-12 text-center text-sm text-muted-foreground">Seleccione una unidad para ver sus intereses acumulados.</td>
                    </tr>
                  )}
                  {selectedUnitId && interests.length === 0 && !loading && (
                    <tr>
                      <td colSpan={8} className="px-6 py-12 text-center text-sm text-muted-foreground">No hay intereses registrados para esta unidad.</td>
                    </tr>
                  )}
                  {interests.map(interest => (
                    <tr key={interest.id} className="hover:bg-muted/30 transition-colors">
                      <td className="px-6 py-4 text-sm font-medium">{interest.period}</td>
                      <td className="px-6 py-4 text-sm">${interest.baseAmount.toLocaleString()}</td>
                      <td className="px-6 py-4 text-sm text-muted-foreground">{(interest.dailyRate * 100).toFixed(6)}%</td>
                      <td className="px-6 py-4 text-sm">{interest.daysInPeriod}</td>
                      <td className="px-6 py-4 text-sm">${interest.calculatedAmount.toLocaleString()}</td>
                      <td className="px-6 py-4 text-sm font-semibold">${interest.balanceAmount.toLocaleString()}</td>
                      <td className="px-6 py-4">
                        <span className={`inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-semibold ${
                          interest.status === 'Paid'
                            ? 'bg-emerald-50 text-emerald-700'
                            : 'bg-amber-50 text-amber-700'
                        }`}>
                          {interest.status === 'Paid' ? 'Pagado' : 'Pendiente'}
                        </span>
                      </td>
                      <td className="px-6 py-4 text-sm text-muted-foreground">
                        {new Date(interest.interestStartDate).toLocaleDateString()} - {new Date(interest.interestEndDate).toLocaleDateString()}
                      </td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>
          )}
        </CardContent>
      </Card>
    </div>
  );
}

// ── Reports Tab ───────────────────────────────────────────────────────────────

function ReportsTab() {
  const [units, setUnits] = useState<Unit[]>([]);
  const [selectedUnitId, setSelectedUnitId] = useState('');
  const [statusFilter, setStatusFilter] = useState('');
  const [fromDate, setFromDate] = useState('');
  const [toDate, setToDate] = useState('');
  const [report, setReport] = useState<InterestReportDto | null>(null);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState('');
  const [exporting, setExporting] = useState('');

  useEffect(() => {
    UnitsService.getUnits().then(setUnits).catch(() => {});
  }, []);

  const fetchReport = async () => {
    setLoading(true);
    setError('');
    try {
      const data = await interestService.getReport(
        selectedUnitId || undefined,
        statusFilter || undefined,
        fromDate || undefined,
        toDate || undefined,
      );
      setReport(data);
    } catch {
      setError('Error al generar el reporte.');
    } finally {
      setLoading(false);
    }
  };

  const handleExport = async (format: 'excel' | 'pdf') => {
    setExporting(format);
    setError('');
    try {
      const blob = await interestService.exportReport(
        format,
        selectedUnitId || undefined,
        statusFilter || undefined,
        fromDate || undefined,
        toDate || undefined,
      );
      const url = window.URL.createObjectURL(blob);
      const a = document.createElement('a');
      a.href = url;
      a.download = `reporte_intereses_mora.${format === 'excel' ? 'xlsx' : 'pdf'}`;
      document.body.appendChild(a);
      a.click();
      document.body.removeChild(a);
      window.URL.revokeObjectURL(url);
    } catch {
      setError('Error al exportar el reporte.');
    } finally {
      setExporting('');
    }
  };

  return (
    <div className="space-y-4">
      <Card>
        <CardContent className="p-6">
          <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-5 gap-4">
            <div>
              <label className="block text-xs font-semibold text-muted-foreground mb-1">Unidad</label>
              <select
                value={selectedUnitId}
                onChange={e => setSelectedUnitId(e.target.value)}
                className="w-full bg-transparent border border-border rounded-lg p-2 text-sm"
              >
                <option value="">Todas las unidades</option>
                {units.map(u => (
                  <option key={u.id} value={u.id}>{formatUnitLabel(u.identifier, u.towerOrBlock)}</option>
                ))}
              </select>
            </div>
            <div>
              <label className="block text-xs font-semibold text-muted-foreground mb-1">Estado</label>
              <select
                value={statusFilter}
                onChange={e => setStatusFilter(e.target.value)}
                className="w-full bg-transparent border border-border rounded-lg p-2 text-sm"
              >
                <option value="">Todos</option>
                <option value="Pending">Pendiente</option>
                <option value="Paid">Pagado</option>
              </select>
            </div>
            <div>
              <label className="block text-xs font-semibold text-muted-foreground mb-1">Desde</label>
              <input
                type="date"
                value={fromDate}
                onChange={e => setFromDate(e.target.value)}
                className="w-full bg-transparent border border-border rounded-lg p-2 text-sm"
              />
            </div>
            <div>
              <label className="block text-xs font-semibold text-muted-foreground mb-1">Hasta</label>
              <input
                type="date"
                value={toDate}
                onChange={e => setToDate(e.target.value)}
                className="w-full bg-transparent border border-border rounded-lg p-2 text-sm"
              />
            </div>
            <div className="flex items-end gap-2">
              <Button onClick={fetchReport} disabled={loading}>
                {loading ? <Loader2 className="w-4 h-4 animate-spin mr-2" /> : null}
                Generar
              </Button>
              {report && report.lines.length > 0 && (
                <>
                  <button
                    onClick={() => handleExport('excel')}
                    disabled={exporting !== ''}
                    className="px-3 py-2 bg-emerald-600 text-white rounded-lg hover:bg-emerald-700 text-sm font-semibold disabled:opacity-50 flex items-center gap-2"
                  >
                    {exporting === 'excel' ? <Loader2 className="w-4 h-4 animate-spin" /> : <FileDown className="w-4 h-4" />}
                    Excel
                  </button>
                  <button
                    onClick={() => handleExport('pdf')}
                    disabled={exporting !== ''}
                    className="px-3 py-2 bg-rose-600 text-white rounded-lg hover:bg-rose-700 text-sm font-semibold disabled:opacity-50 flex items-center gap-2"
                  >
                    {exporting === 'pdf' ? <Loader2 className="w-4 h-4 animate-spin" /> : <FileDown className="w-4 h-4" />}
                    PDF
                  </button>
                </>
              )}
            </div>
          </div>
        </CardContent>
      </Card>

      {error && (
        <div className="p-3 bg-rose-50 border border-rose-200 rounded-lg text-rose-700 text-xs flex items-center gap-2">
          <AlertTriangle className="w-4 h-4 shrink-0" />{error}
        </div>
      )}

      {report && (
        <>
          <div className="grid grid-cols-2 sm:grid-cols-6 gap-4">
            <Card>
              <CardContent className="p-4 flex items-center gap-3">
                <DollarSign className="w-5 h-5 text-emerald-600 shrink-0" />
                <div>
                  <p className="text-xs text-muted-foreground">Total Interés Generado</p>
                  <p className="text-lg font-bold">${report.totalCalculated.toLocaleString()}</p>
                </div>
              </CardContent>
            </Card>
            <Card>
              <CardContent className="p-4 flex items-center gap-3">
                <DollarSign className="w-5 h-5 text-emerald-600 shrink-0" />
                <div>
                  <p className="text-xs text-muted-foreground">Total Cobrado</p>
                  <p className="text-lg font-bold">${report.totalCollected.toLocaleString()}</p>
                </div>
              </CardContent>
            </Card>
            <Card>
              <CardContent className="p-4 flex items-center gap-3">
                <DollarSign className="w-5 h-5 text-amber-600 shrink-0" />
                <div>
                  <p className="text-xs text-muted-foreground">Saldo Pendiente</p>
                  <p className="text-lg font-bold">${report.totalBalance.toLocaleString()}</p>
                </div>
              </CardContent>
            </Card>
            <Card>
              <CardContent className="p-4 flex items-center gap-3">
                <DollarSign className="w-5 h-5 text-blue-600 shrink-0" />
                <div>
                  <p className="text-xs text-muted-foreground">Base Total</p>
                  <p className="text-lg font-bold">${report.totalBaseAmount.toLocaleString()}</p>
                </div>
              </CardContent>
            </Card>
            <Card>
              <CardContent className="p-4 flex items-center gap-3">
                <AlertTriangle className="w-5 h-5 text-amber-600 shrink-0" />
                <div>
                  <p className="text-xs text-muted-foreground">Pendientes</p>
                  <p className="text-lg font-bold">{report.pendingCount}</p>
                </div>
              </CardContent>
            </Card>
            <Card>
              <CardContent className="p-4 flex items-center gap-3">
                <CheckCircle2 className="w-5 h-5 text-emerald-600 shrink-0" />
                <div>
                  <p className="text-xs text-muted-foreground">Pagados</p>
                  <p className="text-lg font-bold">{report.paidCount}</p>
                </div>
              </CardContent>
            </Card>
          </div>

          <Card>
            <CardContent className="p-0">
              <div className="overflow-x-auto">
                <table className="min-w-full divide-y divide-border">
                  <thead className="bg-muted/50">
                    <tr>
                      <th className="px-6 py-4 text-left text-xs font-bold text-muted-foreground uppercase tracking-wider">Unidad</th>
                      <th className="px-6 py-4 text-left text-xs font-bold text-muted-foreground uppercase tracking-wider">Período</th>
                      <th className="px-6 py-4 text-left text-xs font-bold text-muted-foreground uppercase tracking-wider">Base</th>
                      <th className="px-6 py-4 text-left text-xs font-bold text-muted-foreground uppercase tracking-wider">Tasa Diaria</th>
                      <th className="px-6 py-4 text-left text-xs font-bold text-muted-foreground uppercase tracking-wider">Días</th>
                      <th className="px-6 py-4 text-left text-xs font-bold text-muted-foreground uppercase tracking-wider">Calculado</th>
                      <th className="px-6 py-4 text-left text-xs font-bold text-muted-foreground uppercase tracking-wider">Saldo</th>
                      <th className="px-6 py-4 text-left text-xs font-bold text-muted-foreground uppercase tracking-wider">Estado</th>
                    </tr>
                  </thead>
                  <tbody className="divide-y divide-border">
                    {report.lines.length === 0 && (
                      <tr>
                        <td colSpan={8} className="px-6 py-12 text-center text-sm text-muted-foreground">No se encontraron registros.</td>
                      </tr>
                    )}
                    {report.lines.map(line => (
                      <tr key={line.id} className="hover:bg-muted/30 transition-colors">
                        <td className="px-6 py-4 text-sm font-medium">{line.unitIdentifier}</td>
                        <td className="px-6 py-4 text-sm">{line.period}</td>
                        <td className="px-6 py-4 text-sm">${line.baseAmount.toLocaleString()}</td>
                        <td className="px-6 py-4 text-sm text-muted-foreground">{(line.dailyRate * 100).toFixed(6)}%</td>
                        <td className="px-6 py-4 text-sm">{line.daysInPeriod}</td>
                        <td className="px-6 py-4 text-sm">${line.calculatedAmount.toLocaleString()}</td>
                        <td className="px-6 py-4 text-sm font-semibold">${line.balanceAmount.toLocaleString()}</td>
                        <td className="px-6 py-4">
                          <span className={`inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-semibold ${
                            line.status === 'Paid'
                              ? 'bg-emerald-50 text-emerald-700'
                              : 'bg-amber-50 text-amber-700'
                          }`}>
                            {line.status === 'Paid' ? 'Pagado' : 'Pendiente'}
                          </span>
                        </td>
                      </tr>
                    ))}
                  </tbody>
                </table>
              </div>
            </CardContent>
          </Card>
        </>
      )}
    </div>
  );
}
