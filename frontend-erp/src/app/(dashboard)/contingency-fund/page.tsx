'use client';

import React, { useState, useEffect } from 'react';
import { useAuth } from '@/context/AuthContext';
import budgetService, { 
  ContingencyFundStatus
} from '@/lib/budget-service';
import { 
  ShieldCheck, 
  TrendingUp, 
  ArrowDownCircle, 
  Coins, 
  History, 
  AlertCircle,
  Plus,
  Check,
  X,
  Scale
} from 'lucide-react';
import { Button } from '@/components/ui/Button';

export default function ContingencyFundPage() {
  const { user } = useAuth();
  const [fundStatus, setFundStatus] = useState<ContingencyFundStatus | null>(null);
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState('');
  const [success, setSuccess] = useState('');

  // Modals state
  const [isLiquidateOpen, setIsLiquidateOpen] = useState(false);
  const [liquidateYear, setLiquidateYear] = useState<number>(new Date().getFullYear());
  const [liquidateMonth, setLiquidateMonth] = useState<number>(new Date().getMonth() + 1);

  const [isUsageOpen, setIsUsageOpen] = useState(false);
  const [useAmount, setUseAmount] = useState<number>(0);
  const [useJustification, setUseJustification] = useState('');
  const [useActNumber, setUseActNumber] = useState('');
  const [useApprovalDate, setUseApprovalDate] = useState('');

  const canEdit = user?.role === 'SuperAdmin' || user?.role === 'Admin' || user?.role === 'Accountant';

  useEffect(() => {
    fetchFundStatus();
  }, []);

  const fetchFundStatus = async () => {
    try {
      setIsLoading(true);
      setError('');
      const data = await budgetService.getContingencyFund();
      setFundStatus(data);
    } catch (err: any) {
      console.error(err);
      setError('No se pudo cargar la información del fondo de imprevistos.');
    } finally {
      setIsLoading(false);
    }
  };

  const handleLiquidateContribution = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!canEdit) return;

    setError('');
    setSuccess('');

    try {
      const contribution = await budgetService.liquidateContingencyContribution({
        year: liquidateYear,
        month: liquidateMonth
      });

      setSuccess(`Fondo de imprevistos liquidado con éxito para el período ${contribution.period}. Aporte calculado: ${formatCurrency(contribution.amount)}.`);
      setIsLiquidateOpen(false);
      fetchFundStatus();
    } catch (err: any) {
      console.error(err);
      let errMsg = 'Error al liquidar el aporte mensual del fondo de imprevistos.';
      if (err.response && err.response.data) {
        errMsg = err.response.data;
      }
      setError(errMsg);
    }
  };

  const handleRecordUsage = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!canEdit) return;

    setError('');
    setSuccess('');

    if (useAmount <= 0) {
      setError('El monto a retirar debe ser mayor a cero.');
      return;
    }

    if (fundStatus && useAmount > fundStatus.currentBalance) {
      setError('Fondos insuficientes: El monto a retirar supera el saldo disponible actual del fondo.');
      return;
    }

    if (!useJustification.trim()) {
      setError('La justificación del uso de los fondos es requerida.');
      return;
    }

    if (!useActNumber.trim()) {
      setError('El número de acta del consejo o asamblea es requerido.');
      return;
    }

    if (!useApprovalDate) {
      setError('La fecha de la reunión de aprobación es requerida.');
      return;
    }

    try {
      await budgetService.recordContingencyUsage({
        amount: useAmount,
        justification: useJustification,
        councilApprovalActNumber: useActNumber,
        approvalDate: useApprovalDate
      });

      setSuccess(`Uso del fondo registrado correctamente por valor de ${formatCurrency(useAmount)}.`);
      setIsUsageOpen(false);
      fetchFundStatus();
    } catch (err: any) {
      console.error(err);
      let errMsg = 'Error al registrar el uso del fondo de imprevistos.';
      if (err.response && err.response.data) {
        errMsg = err.response.data;
      }
      setError(errMsg);
    }
  };

  const formatCurrency = (val: number) => {
    return new Intl.NumberFormat('es-CO', {
      style: 'currency',
      currency: 'COP',
      minimumFractionDigits: 0
    }).format(val);
  };

  const getMonthName = (m: number) => {
    const months = [
      'Enero', 'Febrero', 'Marzo', 'Abril', 'Mayo', 'Junio',
      'Julio', 'Agosto', 'Septiembre', 'Octubre', 'Noviembre', 'Diciembre'
    ];
    return months[m - 1];
  };

  // Helper functions to completely remove conditional ternary operators
  function renderContributionsTableRows() {
    if (!fundStatus || fundStatus.contributions.length === 0) {
      return (
        <tr>
          <td colSpan={5} className="py-12 text-center text-slate-400 dark:text-zinc-500">
            No se registran liquidaciones de aportes todavía.
          </td>
        </tr>
      );
    }

    return fundStatus.contributions.map((con) => (
      <tr key={con.id} className="hover:bg-slate-50/50 dark:hover:bg-zinc-900/30">
        <td className="py-3 px-6 font-bold font-mono">
          {con.period}
        </td>
        <td className="py-3 px-6 font-mono text-slate-600 dark:text-zinc-400">
          {formatCurrency(con.incomeBase)}
        </td>
        <td className="py-3 px-6 text-center text-slate-600 dark:text-zinc-400">
          {(con.percentage * 100).toFixed(1)}%
        </td>
        <td className="py-3 px-6 text-right font-mono font-bold text-emerald-600">
          {formatCurrency(con.amount)}
        </td>
        <td className="py-3 px-6 text-slate-500">
          {new Date(con.contributionDate).toLocaleDateString('es-CO')}
        </td>
      </tr>
    ));
  }

  function renderUsagesTableRows() {
    if (!fundStatus || fundStatus.usages.length === 0) {
      return (
        <tr>
          <td colSpan={4} className="py-12 text-center text-slate-400 dark:text-zinc-500">
            No se han registrado retiros del fondo de imprevistos.
          </td>
        </tr>
      );
    }

    return fundStatus.usages.map((use) => (
      <tr key={use.id} className="hover:bg-slate-50/50 dark:hover:bg-zinc-900/30">
        <td className="py-3 px-6 font-bold text-gray-700 dark:text-zinc-300">
          <div>{use.councilApprovalActNumber}</div>
          <div className="text-[10px] text-slate-400">
            Aprobado: {new Date(use.approvalDate).toLocaleDateString('es-CO')}
          </div>
        </td>
        <td className="py-3 px-6 text-right font-mono font-bold text-rose-600">
          -{formatCurrency(use.amount)}
        </td>
        <td className="py-3 px-6 text-slate-500">
          {new Date(use.approvalDate).toLocaleDateString('es-CO')}
        </td>
        <td className="py-3 px-6 text-slate-500 max-w-xs truncate" title={use.justification}>
          {use.justification}
        </td>
      </tr>
    ));
  }

  function renderMainPanel() {
    if (!fundStatus) return null;

    const actionButtons = [];
    if (canEdit) {
      actionButtons.push(
        <Button
          key="usage"
          variant="secondary"
          onClick={() => {
            setUseAmount(0);
            setUseJustification('');
            setUseActNumber('');
            setUseApprovalDate('');
            setIsUsageOpen(true);
            setError('');
          }}
          className="flex items-center gap-2"
        >
          <ArrowDownCircle className="w-4 h-4 text-rose-500" />
          Registrar Uso del Fondo
        </Button>
      );
      actionButtons.push(
        <Button
          key="liquidate"
          variant="primary"
          onClick={() => {
            setLiquidateYear(new Date().getFullYear());
            setLiquidateMonth(new Date().getMonth() + 1);
            setIsLiquidateOpen(true);
            setError('');
          }}
          className="flex items-center gap-2"
        >
          <Plus className="w-4 h-4" />
          Liquidar Aporte Mensual
        </Button>
      );
    }

    return (
      <>
        {/* ACTION BUTTONS HEADER */}
        <div className="flex justify-end gap-2 mb-4">
          {actionButtons}
        </div>

        <div className="grid grid-cols-1 md:grid-cols-3 gap-6">
          {/* CURRENT BALANCE */}
          <div className="card-standard p-6 bg-card text-card-foreground space-y-4">
            <div className="flex items-center justify-between">
              <span className="text-xs font-bold text-slate-400 dark:text-zinc-500 uppercase tracking-widest">Saldo Disponible Actual</span>
              <span className="w-8 h-8 rounded-full bg-emerald-50 dark:bg-emerald-950/20 text-emerald-600 dark:text-emerald-400 flex items-center justify-center">
                <Coins className="w-4 h-4" />
              </span>
            </div>
            <div className="space-y-1">
              <h3 className="text-3xl font-black text-emerald-700 dark:text-emerald-400">
                {formatCurrency(fundStatus.currentBalance)}
              </h3>
              <p className="text-xs text-slate-500 dark:text-zinc-400">
                Fondo líquido custodiado para emergencias
              </p>
            </div>
          </div>

          {/* PROJECTED CLOSING BALANCE */}
          <div className="card-standard p-6 bg-card text-card-foreground space-y-4">
            <div className="flex items-center justify-between">
              <span className="text-xs font-bold text-slate-400 dark:text-zinc-500 uppercase tracking-widest">Proyección de Cierre de Año</span>
              <span className="w-8 h-8 rounded-full bg-blue-50 dark:bg-blue-950/20 text-blue-600 dark:text-blue-400 flex items-center justify-center">
                <TrendingUp className="w-4 h-4" />
              </span>
            </div>
            <div className="space-y-1">
              <h3 className="text-3xl font-black text-gray-900 dark:text-white">
                {formatCurrency(fundStatus.projectedClosingBalance)}
              </h3>
              <p className="text-xs text-slate-500 dark:text-zinc-400">
                Incluye aportes pendientes por liquidar
              </p>
            </div>
          </div>

          {/* LEGAL RESTRICTIONS NOTICE */}
          <div className="card-standard p-6 bg-slate-50 dark:bg-zinc-900 border-dashed space-y-3">
            <div className="flex items-center gap-2 text-slate-700 dark:text-zinc-300">
              <Scale className="w-4 h-4 text-emerald-600" />
              <h4 className="font-bold text-xs uppercase tracking-widest">Normativa Ley 675</h4>
            </div>
            <p className="text-[11px] text-slate-500 dark:text-zinc-400 leading-relaxed">
              El fondo de imprevistos es inembargable y de uso restringido. Su disposición requiere la aprobación previa del Consejo de Administración (monto ordinario) o Asamblea General de Copropietarios.
            </p>
          </div>
        </div>

        {/* DUAL HISTORY TABS (CONTRIBUTIONS & USAGES) */}
        <div className="grid grid-cols-1 lg:grid-cols-2 gap-6">
          {/* CONTRIBUTIONS LIST */}
          <div className="card-standard bg-card text-card-foreground flex flex-col">
            <div className="p-6 border-b border-border flex items-center gap-2">
              <ShieldCheck className="w-5 h-5 text-emerald-600" />
              <h3 className="font-bold text-gray-900 dark:text-white">Historial de Aportes Liquidados</h3>
            </div>

            <div className="overflow-x-auto flex-1">
              <table className="w-full border-collapse">
                <thead>
                  <tr className="border-b border-border bg-slate-50 dark:bg-zinc-900/50 text-left text-xs font-bold text-slate-400 dark:text-zinc-500 uppercase tracking-widest">
                    <th className="py-3 px-6">Período</th>
                    <th className="py-3 px-6">Base Recaudo</th>
                    <th className="py-3 px-6 text-center">% Aporte</th>
                    <th className="py-3 px-6 text-right">Monto Liquidado</th>
                    <th className="py-3 px-6">Fecha Registro</th>
                  </tr>
                </thead>
                <tbody className="divide-y divide-border text-xs">
                  {renderContributionsTableRows()}
                </tbody>
              </table>
            </div>
          </div>

          {/* USAGES LIST */}
          <div className="card-standard bg-card text-card-foreground flex flex-col">
            <div className="p-6 border-b border-border flex items-center gap-2">
              <History className="w-5 h-5 text-rose-500" />
              <h3 className="font-bold text-gray-900 dark:text-white">Retiros y Egresos del Fondo</h3>
            </div>

            <div className="overflow-x-auto flex-1">
              <table className="w-full border-collapse">
                <thead>
                  <tr className="border-b border-border bg-slate-50 dark:bg-zinc-900/50 text-left text-xs font-bold text-slate-400 dark:text-zinc-500 uppercase tracking-widest">
                    <th className="py-3 px-6">Acta Autorización</th>
                    <th className="py-3 px-6 text-right">Monto Retirado</th>
                    <th className="py-3 px-6">Fecha Retiro</th>
                    <th className="py-3 px-6">Justificación del Retiro</th>
                  </tr>
                </thead>
                <tbody className="divide-y divide-border text-xs">
                  {renderUsagesTableRows()}
                </tbody>
              </table>
            </div>
          </div>
        </div>
      </>
    );
  }

  function renderContent() {
    if (isLoading) {
      return (
        <div className="card-standard p-12 text-center text-slate-400 dark:text-zinc-500 bg-card">
          Cargando estado del fondo de imprevistos...
        </div>
      );
    }

    if (!fundStatus) {
      return (
        <div className="card-standard p-12 text-center text-slate-400 dark:text-zinc-500 bg-card">
          No se encontró configuración activa del fondo de imprevistos para esta copropiedad.
        </div>
      );
    }

    return renderMainPanel();
  }

  return (
    <div className="space-y-6">
      {/* HEADER */}
      <div className="flex flex-col sm:flex-row sm:items-center sm:justify-between gap-4">
        <div>
          <h1 className="text-2xl font-bold text-gray-900 tracking-tight dark:text-white">Fondo de Imprevistos</h1>
          <p className="text-sm text-gray-500 mt-1 dark:text-zinc-400">
            Seguimiento y control del fondo de imprevistos obligatorio según la Ley 675 de 2001 (mínimo 1% sobre ingresos ordinarios).
          </p>
        </div>
      </div>

      {/* ERROR & SUCCESS */}
      {error && (
        <div className="flex items-center gap-3 p-4 bg-rose-50 dark:bg-rose-950/20 text-rose-700 dark:text-rose-400 rounded-xl border border-rose-100 dark:border-rose-900/50">
          <AlertCircle className="w-5 h-5 flex-shrink-0" />
          <p className="text-sm font-semibold">{error}</p>
        </div>
      )}

      {success && (
        <div className="flex items-center gap-3 p-4 bg-emerald-50 dark:bg-emerald-950/20 text-emerald-700 dark:text-emerald-400 rounded-xl border border-emerald-100 dark:border-emerald-900/50">
          <Check className="w-5 h-5 flex-shrink-0" />
          <p className="text-sm font-semibold">{success}</p>
        </div>
      )}

      {/* MAIN PANEL CONTENT */}
      {renderContent()}

      {/* LIQUIDATE APORTE MODAL */}
      {isLiquidateOpen && (
        <div className="fixed inset-0 bg-black/60 backdrop-blur-sm z-[150] flex items-center justify-center p-4">
          <div className="bg-card text-card-foreground w-full max-w-md rounded-xl border border-border shadow-lg overflow-hidden animate-in zoom-in-95 duration-200">
            <div className="p-6 border-b border-border flex items-center justify-between">
              <h3 className="font-bold text-lg text-gray-900 dark:text-white">Liquidar Aporte Mensual</h3>
              <button onClick={() => setIsLiquidateOpen(false)} className="text-slate-400 hover:text-slate-600">
                <X className="w-5 h-5" />
              </button>
            </div>

            <form onSubmit={handleLiquidateContribution} className="p-6 space-y-4">
              <div className="p-3 bg-blue-50 dark:bg-blue-950/20 text-blue-700 dark:text-blue-400 rounded-lg text-xs border border-blue-100 dark:border-blue-900/50">
                El sistema consultará todos los ingresos operacionales reales percibidos (Caja/Bancos de Cuotas de Administración) del período seleccionado, y liquidará el porcentaje de imprevistos configurado para el tenant.
              </div>

              <div className="grid grid-cols-2 gap-4">
                <div>
                  <label className="block text-xs font-bold text-slate-400 dark:text-zinc-500 uppercase tracking-widest mb-1">
                    Año
                  </label>
                  <select
                    value={liquidateYear}
                    onChange={(e) => setLiquidateYear(Number(e.target.value))}
                    className="input-standard"
                  >
                    {[2025, 2026, 2027].map((y) => (
                      <option key={y} value={y}>{y}</option>
                    ))}
                  </select>
                </div>

                <div>
                  <label className="block text-xs font-bold text-slate-400 dark:text-zinc-500 uppercase tracking-widest mb-1">
                    Mes
                  </label>
                  <select
                    value={liquidateMonth}
                    onChange={(e) => setLiquidateMonth(Number(e.target.value))}
                    className="input-standard"
                  >
                    {[1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12].map((m) => (
                      <option key={m} value={m}>{getMonthName(m)}</option>
                    ))}
                  </select>
                </div>
              </div>

              <div className="pt-4 flex justify-end gap-3 border-t border-border">
                <Button type="button" variant="ghost" onClick={() => setIsLiquidateOpen(false)}>
                  Cancelar
                </Button>
                <Button type="submit" variant="primary">
                  Procesar Liquidación
                </Button>
              </div>
            </form>
          </div>
        </div>
      )}

      {/* RECORD USAGE MODAL */}
      {isUsageOpen && (
        <div className="fixed inset-0 bg-black/60 backdrop-blur-sm z-[150] flex items-center justify-center p-4">
          <div className="bg-card text-card-foreground w-full max-w-md rounded-xl border border-border shadow-lg overflow-hidden animate-in zoom-in-95 duration-200">
            <div className="p-6 border-b border-border flex items-center justify-between">
              <h3 className="font-bold text-lg text-gray-900 dark:text-white">Registrar Egreso de Imprevistos</h3>
              <button onClick={() => setIsUsageOpen(false)} className="text-slate-400 hover:text-slate-600">
                <X className="w-5 h-5" />
              </button>
            </div>

            <form onSubmit={handleRecordUsage} className="p-6 space-y-4">
              <div>
                <label className="block text-xs font-bold text-slate-400 dark:text-zinc-500 uppercase tracking-widest mb-1">
                  Monto a Retirar (COP)
                </label>
                <input
                  type="number"
                  placeholder="0"
                  value={useAmount || ''}
                  onChange={(e) => setUseAmount(Number(e.target.value))}
                  className="input-standard font-mono font-bold"
                  required
                />
              </div>

              <div>
                <label className="block text-xs font-bold text-slate-400 dark:text-zinc-500 uppercase tracking-widest mb-1">
                  Justificación Técnica del Retiro
                </label>
                <textarea
                  placeholder="Ej. Reparación urgente del muro de contención de la zona norte..."
                  value={useJustification}
                  onChange={(e) => setUseJustification(e.target.value)}
                  className="input-standard min-h-[80px]"
                  required
                />
              </div>

              <div>
                <label className="block text-xs font-bold text-slate-400 dark:text-zinc-500 uppercase tracking-widest mb-1">
                  Acta de Aprobación del Consejo / Asamblea
                </label>
                <input
                  type="text"
                  placeholder="Ej. Acta Consejo 045"
                  value={useActNumber}
                  onChange={(e) => setUseActNumber(e.target.value)}
                  className="input-standard"
                  required
                />
              </div>

              <div>
                <label className="block text-xs font-bold text-slate-400 dark:text-zinc-500 uppercase tracking-widest mb-1">
                  Fecha de Aprobación
                </label>
                <input
                  type="date"
                  value={useApprovalDate}
                  onChange={(e) => setUseApprovalDate(e.target.value)}
                  className="input-standard"
                  required
                />
              </div>

              <div className="pt-4 flex justify-end gap-3 border-t border-border">
                <Button type="button" variant="ghost" onClick={() => setIsUsageOpen(false)}>
                  Cancelar
                </Button>
                <Button type="submit" variant="danger">
                  Autorizar Retiro
                </Button>
              </div>
            </form>
          </div>
        </div>
      )}
    </div>
  );
}
