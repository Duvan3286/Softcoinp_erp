'use client';

import React, { useState, useEffect } from 'react';
import { useAuth } from '@/context/AuthContext';
import budgetService, { 
  BudgetExecutionReport, 
  BudgetExecutionItem, 
  BudgetAlert, 
  BudgetMovement, 
  CreateBudgetRequest, 
  CreateBudgetDetailRequest 
} from '@/lib/budget-service';
import accountingService, { AccountingAccount } from '@/lib/accounting-service';
import { 
  BadgeDollarSign, 
  Plus, 
  TrendingUp, 
  TrendingDown, 
  AlertTriangle, 
  Check, 
  FileText, 
  ArrowLeftRight, 
  Play, 
  Lock, 
  Calendar,
  X,
  Activity,
  DollarSign
} from 'lucide-react';
import { Button } from '@/components/ui/Button';

export default function BudgetsPage() {
  const { user } = useAuth();
  const [year, setYear] = useState<number>(new Date().getFullYear());
  const [report, setReport] = useState<BudgetExecutionReport | null>(null);
  const [accounts, setAccounts] = useState<AccountingAccount[]>([]);
  const [movements, setMovements] = useState<BudgetMovement[]>([]);
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState('');
  const [success, setSuccess] = useState('');

  // Active sub-views: 'execution' | 'movements' | 'create' | 'edit-draft'
  const [activeView, setActiveView] = useState<'execution' | 'movements' | 'create' | 'edit-draft'>('execution');

  // Create Budget Form State
  const [newFiscalPeriod, setNewFiscalPeriod] = useState<number>(new Date().getFullYear() + 1);
  const [newMeetingAct, setNewMeetingAct] = useState('');
  const [newApprovalDate, setNewApprovalDate] = useState('');
  const [copyFromPrevious, setCopyFromPrevious] = useState(false);
  const [globalAdjustment, setGlobalAdjustment] = useState<number>(0);
  const [draftDetails, setDraftDetails] = useState<Record<string, { val: number; obs: string }>>({});

  // Activate Budget Modal
  const [isActivateOpen, setIsActivateOpen] = useState(false);
  const [actNumber, setActNumber] = useState('');
  const [approvalDate, setApprovalDate] = useState('');

  // Create Budget Movement Form State
  const [isMovementOpen, setIsMovementOpen] = useState(false);
  const [movementType, setMovementType] = useState<'Addition' | 'Transfer'>('Transfer');
  const [sourceAccountId, setSourceAccountId] = useState('');
  const [destinationAccountId, setDestinationAccountId] = useState('');
  const [movementAmount, setMovementAmount] = useState<number>(0);
  const [justification, setJustification] = useState('');
  const [approvalType, setApprovalType] = useState<'Council' | 'Assembly'>('Council');
  const [movementActNumber, setMovementActNumber] = useState('');
  const [movementApprovalDate, setMovementApprovalDate] = useState('');

  const canEdit = user?.role === 'SuperAdmin' || user?.role === 'Admin' || user?.role === 'Accountant';

  useEffect(() => {
    fetchAccounts();
  }, []);

  useEffect(() => {
    fetchReport();
  }, [year]);

  const fetchAccounts = async () => {
    try {
      const data = await accountingService.getAccounts();
      setAccounts(data);
    } catch (err) {
      console.error(err);
    }
  };

  const fetchReport = async () => {
    try {
      setIsLoading(true);
      setError('');
      const rep = await budgetService.getExecutionReport(year);
      setReport(rep);
      
      if (rep && rep.budgetId) {
        const moves = await budgetService.getMovements(rep.budgetId);
        setMovements(moves);
      } else {
        setMovements([]);
      }
    } catch (err: any) {
      console.error(err);
      setReport(null);
      setMovements([]);
      if (err.response && err.response.status !== 404) {
        setError('Ocurrió un error al cargar el informe de ejecución presupuestal.');
      }
    } finally {
      setIsLoading(false);
    }
  };

  // Pre-fill draft form with leaf accounts and 0 values
  const initDraftDetails = () => {
    const details: Record<string, { val: number; obs: string }> = {};
    accounts.forEach((acc) => {
      if (!acc.isGroup && (acc.code.startsWith('4') || acc.code.startsWith('5'))) {
        details[acc.id] = { val: 0, obs: '' };
      }
    });
    setDraftDetails(details);
  };

  const handleOpenCreateView = () => {
    if (!canEdit) return;
    initDraftDetails();
    setNewFiscalPeriod(year);
    setNewMeetingAct('');
    setNewApprovalDate('');
    setCopyFromPrevious(false);
    setGlobalAdjustment(0);
    setActiveView('create');
    setError('');
  };

  const handleOpenEditDraftView = () => {
    if (!report) return;
    const details: Record<string, { val: number; obs: string }> = {};
    
    accounts.forEach((acc) => {
      if (!acc.isGroup && (acc.code.startsWith('4') || acc.code.startsWith('5'))) {
        const currentDetail = report.items.find((item) => item.accountId === acc.id);
        let currentVal = 0;
        if (currentDetail) {
          currentVal = currentDetail.approvedValue;
        }
        details[acc.id] = {
          val: currentVal,
          obs: ''
        };
      }
    });
    
    setDraftDetails(details);
    setActiveView('edit-draft');
    setError('');
  };

  const handleCreateBudget = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!canEdit) return;

    setError('');
    setSuccess('');

    const manualDetails: CreateBudgetDetailRequest[] = Object.entries(draftDetails)
      .map(([accId, data]) => ({
        accountingAccountId: accId,
        approvedValue: Number(data.val),
        observations: data.obs
      }))
      .filter((d) => d.approvedValue > 0);

    let finalApprovalDate = undefined;
    if (newApprovalDate) {
      finalApprovalDate = newApprovalDate;
    }

    let finalGlobalPercentageAdjustment = undefined;
    if (copyFromPrevious) {
      finalGlobalPercentageAdjustment = globalAdjustment;
    }

    let finalManualDetails = undefined;
    if (!copyFromPrevious) {
      finalManualDetails = manualDetails;
    }

    const request: CreateBudgetRequest = {
      fiscalPeriod: newFiscalPeriod,
      meetingActNumber: newMeetingAct,
      approvalDate: finalApprovalDate,
      copyFromPrevious,
      globalPercentageAdjustment: finalGlobalPercentageAdjustment,
      manualDetails: finalManualDetails
    };

    try {
      await budgetService.createBudget(request);
      setSuccess(`Presupuesto borrador para el año ${newFiscalPeriod} creado.`);
      setYear(newFiscalPeriod);
      setActiveView('execution');
      fetchReport();
    } catch (err: any) {
      console.error(err);
      let errMsg = 'Error al crear el presupuesto.';
      if (err.response && err.response.data) {
        errMsg = err.response.data;
      }
      setError(errMsg);
    }
  };

  const handleSaveDraftDetails = async () => {
    if (!canEdit || !report) return;
    setError('');
    setSuccess('');

    const details: CreateBudgetDetailRequest[] = Object.entries(draftDetails)
      .map(([accId, data]) => ({
        accountingAccountId: accId,
        approvedValue: Number(data.val),
        observations: data.obs
      }));

    try {
      await budgetService.updateDraftDetails(report.budgetId, details);
      setSuccess('Cambios en el borrador guardados correctamente.');
      setActiveView('execution');
      fetchReport();
    } catch (err: any) {
      console.error(err);
      let errMsg = 'Error al guardar los detalles del borrador.';
      if (err.response && err.response.data) {
        errMsg = err.response.data;
      }
      setError(errMsg);
    }
  };

  const handleOpenActivateModal = () => {
    setActNumber(report?.meetingActNumber || '');
    let dateStr = '';
    if (report && report.approvalDate) {
      dateStr = report.approvalDate.split('T')[0];
    }
    setApprovalDate(dateStr);
    setIsActivateOpen(true);
    setError('');
  };

  const handleActivateBudget = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!canEdit || !report) return;

    setError('');
    setSuccess('');

    if (!actNumber.trim()) {
      setError('El número de acta de aprobación de la asamblea es requerido.');
      return;
    }

    if (!approvalDate) {
      setError('La fecha de aprobación de la asamblea es requerida.');
      return;
    }

    try {
      await budgetService.activateBudget(report.budgetId, {
        meetingActNumber: actNumber,
        approvalDate
      });
      setSuccess(`¡Presupuesto del año fiscal ${report.fiscalPeriod} activado con éxito!`);
      setIsActivateOpen(false);
      fetchReport();
    } catch (err: any) {
      console.error(err);
      let errMsg = 'Ocurrió un error al activar el presupuesto.';
      if (err.response && err.response.data) {
        errMsg = err.response.data;
      }
      setError(errMsg);
    }
  };

  const handleCloseBudget = async () => {
    if (!canEdit || !report) return;
    if (!confirm(`¿Está seguro de CERRAR definitivamente el presupuesto de ${report.fiscalPeriod}? Esta acción bloqueará cualquier adición o traslado posterior.`)) {
      return;
    }

    setError('');
    setSuccess('');

    try {
      await budgetService.closeBudget(report.budgetId);
      setSuccess('El presupuesto ha sido cerrado con éxito.');
      fetchReport();
    } catch (err: any) {
      console.error(err);
      let errMsg = 'Ocurrió un error al cerrar el presupuesto.';
      if (err.response && err.response.data) {
        errMsg = err.response.data;
      }
      setError(errMsg);
    }
  };

  const handleOpenMovementModal = () => {
    setMovementType('Transfer');
    setSourceAccountId('');
    setDestinationAccountId('');
    setMovementAmount(0);
    setJustification('');
    setApprovalType('Council');
    setMovementActNumber('');
    setMovementApprovalDate('');
    setIsMovementOpen(true);
    setError('');
  };

  const handleCreateMovement = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!canEdit || !report) return;

    setError('');
    setSuccess('');

    if (movementAmount <= 0) {
      setError('El monto del movimiento debe ser superior a cero.');
      return;
    }

    if (movementType === 'Transfer' && !sourceAccountId) {
      setError('Debes especificar la cuenta de origen para un traslado presupuestal.');
      return;
    }

    if (!destinationAccountId) {
      setError('Debes especificar la cuenta de destino.');
      return;
    }

    if (!justification.trim()) {
      setError('Se requiere una justificación detallada del movimiento.');
      return;
    }

    if (!movementActNumber.trim()) {
      setError('El número de acta de aprobación es requerido.');
      return;
    }

    if (!movementApprovalDate) {
      setError('La fecha de aprobación es requerida.');
      return;
    }

    let finalSourceId = undefined;
    if (movementType === 'Transfer') {
      finalSourceId = sourceAccountId;
    }

    try {
      await budgetService.createMovement({
        budgetId: report.budgetId,
        movementType,
        sourceAccountId: finalSourceId,
        destinationAccountId,
        amount: movementAmount,
        justification,
        approvalType,
        meetingActNumber: movementActNumber,
        approvalDate: movementApprovalDate
      });

      setSuccess(`Movimiento presupuestal (${movementType}) registrado con éxito.`);
      setIsMovementOpen(false);
      fetchReport();
    } catch (err: any) {
      console.error(err);
      let errMsg = 'Error al procesar el traslado o adición.';
      if (err.response && err.response.data) {
        errMsg = err.response.data;
      }
      setError(errMsg);
    }
  };

  // Helper for metrics summary card
  const getSum = (category: string, field: keyof BudgetExecutionItem) => {
    if (!report) return 0;
    return report.items
      .filter((item) => item.category.toLowerCase() === category.toLowerCase() && !item.isGroup)
      .reduce((sum, item) => sum + Number(item[field]), 0);
  };

  // Totals calculations
  const totalApprovedIncome = getSum('income', 'approvedValue');
  const totalAdjustedIncome = getSum('income', 'adjustedBudget');
  const totalExecutedIncome = getSum('income', 'executedValue');

  const totalApprovedExpense = getSum('expense', 'approvedValue');
  const totalAdjustedExpense = getSum('expense', 'adjustedBudget');
  const totalExecutedExpense = getSum('expense', 'executedValue');

  const formatCurrency = (val: number) => {
    return new Intl.NumberFormat('es-CO', {
      style: 'currency',
      currency: 'COP',
      minimumFractionDigits: 0
    }).format(val);
  };

  const getTrafficLightColor = (percentage: number) => {
    if (percentage > 100) {
      return 'text-rose-600 dark:text-rose-400 font-extrabold';
    }
    if (percentage >= 90) {
      return 'text-amber-600 dark:text-amber-400 font-semibold';
    }
    return 'text-emerald-600 dark:text-emerald-400 font-medium';
  };

  // Helper functions for class toggle and UI mappings (avoiding ternary)
  function getTabButtonClass(isActive: boolean): string {
    if (isActive) {
      return 'px-3 py-1 text-xs font-semibold rounded-md transition-all bg-card text-foreground shadow-sm';
    }
    return 'px-3 py-1 text-xs font-semibold rounded-md transition-all text-slate-500 dark:text-zinc-400 hover:text-foreground';
  }

  function getMovementTypeBadgeClass(type: string): string {
    if (type === 'Addition') {
      return 'bg-emerald-50 text-emerald-700 dark:bg-emerald-950/20 dark:text-emerald-400';
    }
    return 'bg-blue-50 text-blue-700 dark:bg-blue-950/20 dark:text-blue-400';
  }

  function translateMovementType(type: string): string {
    if (type === 'Addition') {
      return 'Adición';
    }
    return 'Traslado';
  }

  function translateApprovalType(type: string): string {
    if (type === 'Council') {
      return 'Consejo';
    }
    return 'Asamblea';
  }

  function renderSourceAccountCell(move: BudgetMovement) {
    if (move.movementType === 'Transfer') {
      return (
        <div>
          <span className="font-mono font-bold mr-1">{move.sourceAccountCode}</span>
          <span className="text-gray-500 dark:text-zinc-400">{move.sourceAccountName}</span>
        </div>
      );
    }
    return <span className="text-slate-400 italic">— N/A</span>;
  }

  function getBudgetStatusIndicatorClass(status: string): string {
    if (status === 'Active') {
      return 'bg-emerald-100 text-emerald-800 dark:bg-emerald-950/30 dark:text-emerald-400';
    }
    if (status === 'Draft') {
      return 'bg-amber-100 text-amber-800 dark:bg-amber-950/30 dark:text-amber-400';
    }
    return 'bg-slate-100 text-slate-800 dark:bg-slate-950/30 dark:text-slate-400';
  }

  function getBudgetStatusTextClass(status: string): string {
    if (status === 'Active') {
      return 'bg-emerald-50 text-emerald-700 dark:bg-emerald-950/20 dark:text-emerald-400';
    }
    if (status === 'Draft') {
      return 'bg-amber-50 text-amber-700 dark:bg-amber-950/20 dark:text-amber-400';
    }
    return 'bg-slate-50 text-slate-700 dark:bg-slate-950/20 dark:text-slate-400';
  }

  function translateBudgetStatus(status: string): string {
    if (status === 'Active') {
      return 'Activo / Aprobado';
    }
    if (status === 'Draft') {
      return 'Borrador';
    }
    return 'Cerrado';
  }

  function getIncomeExecutionPercentage(): number {
    if (totalAdjustedIncome > 0) {
      return (totalExecutedIncome / totalAdjustedIncome) * 100;
    }
    return 0;
  }

  function getExpenseExecutionPercentage(): number {
    if (totalAdjustedExpense > 0) {
      return (totalExecutedExpense / totalAdjustedExpense) * 100;
    }
    return 0;
  }

  function getExpenseProgressBarColor(): string {
    if (totalExecutedExpense > totalAdjustedExpense) {
      return 'bg-rose-500';
    }
    return 'bg-blue-500';
  }

  function renderExpenseBalanceLabel() {
    const diff = totalAdjustedExpense - totalExecutedExpense;
    if (diff < 0) {
      return (
        <span className="text-rose-600 font-bold">
          Sobregirado: {formatCurrency(Math.abs(diff))}
        </span>
      );
    }
    return (
      <span>
        Disponible: {formatCurrency(diff)}
      </span>
    );
  }

  function getSurplusTextColor(): string {
    const diff = totalExecutedIncome - totalExecutedExpense;
    if (diff >= 0) {
      return 'text-emerald-700 dark:text-emerald-400';
    }
    return 'text-rose-700 dark:text-rose-400';
  }

  function renderCopyAdjustmentForm() {
    return (
      <div className="mt-4 max-w-xs space-y-2 animate-in slide-in-from-top-1 duration-200">
        <label className="block text-xs font-bold text-slate-400 dark:text-zinc-500 uppercase tracking-widest mb-1">
          Incremento global porcentual (%)
        </label>
        <div className="relative">
          <input
            type="number"
            step={0.01}
            placeholder="Ej. 6.5"
            value={globalAdjustment}
            onChange={(e) => setGlobalAdjustment(Number(e.target.value))}
            className="input-standard"
          />
          <span className="absolute right-3 top-1/2 -translate-y-1/2 text-slate-400">%</span>
        </div>
        <p className="text-[10px] text-slate-400 dark:text-zinc-500">
          El sistema tomará el presupuesto activo más reciente y aplicará este incremento a todos los rubros.
        </p>
      </div>
    );
  }

  function renderManualRubricsForm() {
    return (
      <div className="mt-4 space-y-4">
        <p className="text-xs text-slate-500 dark:text-zinc-400">
          Ingresa los valores proyectados anuales para cada cuenta de Ingresos (4) y Gastos (5):
        </p>
        
        <div className="max-h-96 overflow-y-auto border border-border rounded-lg divide-y divide-border">
          {accounts
            .filter((acc) => !acc.isGroup && (acc.code.startsWith('4') || acc.code.startsWith('5')))
            .map((acc) => {
              const currentVal = draftDetails[acc.id]?.val;
              const inputVal = currentVal === undefined ? '' : currentVal;
              return (
                <div key={acc.id} className="p-3 grid grid-cols-1 md:grid-cols-3 gap-4 items-center bg-card hover:bg-slate-50/55 dark:hover:bg-zinc-800/10">
                  <div className="font-semibold text-sm">
                    <span className="font-mono text-emerald-600 mr-2">{acc.code}</span>
                    <span className="text-gray-700 dark:text-zinc-300">{acc.name}</span>
                  </div>
                  <div>
                    <input
                      type="number"
                      placeholder="Valor anual"
                      value={inputVal}
                      onChange={(e) => setDraftDetails({
                        ...draftDetails,
                        [acc.id]: { val: Number(e.target.value), obs: draftDetails[acc.id]?.obs || '' }
                      })}
                      className="input-standard font-mono"
                    />
                  </div>
                  <div>
                    <input
                      type="text"
                      placeholder="Observación"
                      value={draftDetails[acc.id]?.obs || ''}
                      onChange={(e) => setDraftDetails({
                        ...draftDetails,
                        [acc.id]: { val: draftDetails[acc.id]?.val || 0, obs: e.target.value }
                      })}
                      className="input-standard"
                    />
                  </div>
                </div>
              );
            })}
        </div>
      </div>
    );
  }

  function renderCreateFormDetails() {
    if (copyFromPrevious) {
      return renderCopyAdjustmentForm();
    }
    return renderManualRubricsForm();
  }

  function renderCreateView() {
    return (
      <div className="card-standard bg-card text-card-foreground p-6 space-y-6">
        <div className="flex items-center justify-between border-b border-border pb-4">
          <h3 className="font-bold text-lg text-gray-900 dark:text-white">
            Nuevo Presupuesto para el Año Fiscal {newFiscalPeriod}
          </h3>
          <Button variant="ghost" onClick={() => setActiveView('execution')}>
            Volver
          </Button>
        </div>

        <form onSubmit={handleCreateBudget} className="space-y-6">
          <div className="grid grid-cols-1 md:grid-cols-3 gap-6">
            <div>
              <label className="block text-xs font-bold text-slate-400 dark:text-zinc-500 uppercase tracking-widest mb-1">
                Año Fiscal
              </label>
              <input
                type="number"
                value={newFiscalPeriod}
                onChange={(e) => setNewFiscalPeriod(Number(e.target.value))}
                className="input-standard"
                required
              />
            </div>

            <div>
              <label className="block text-xs font-bold text-slate-400 dark:text-zinc-500 uppercase tracking-widest mb-1">
                Número de Acta Provisoria / Acta Asamblea
              </label>
              <input
                type="text"
                placeholder="Ej. Borrador Inicial / Acta 038"
                value={newMeetingAct}
                onChange={(e) => setNewMeetingAct(e.target.value)}
                className="input-standard"
                required
              />
            </div>

            <div>
              <label className="block text-xs font-bold text-slate-400 dark:text-zinc-500 uppercase tracking-widest mb-1">
                Fecha de Reunión / Asamblea
              </label>
              <input
                type="date"
                value={newApprovalDate}
                onChange={(e) => setNewApprovalDate(e.target.value)}
                className="input-standard"
              />
            </div>
          </div>

          <div className="p-4 bg-slate-50 dark:bg-zinc-900 rounded-xl border border-border">
            <label className="flex items-center gap-2 cursor-pointer font-bold text-gray-800 dark:text-white">
              <input
                type="checkbox"
                checked={copyFromPrevious}
                onChange={(e) => setCopyFromPrevious(e.target.checked)}
                className="w-4 h-4 rounded border-gray-300 text-emerald-600 focus:ring-emerald-500"
              />
              <span>Copiar valores del presupuesto del año anterior</span>
            </label>

            {renderCreateFormDetails()}
          </div>

          <div className="flex justify-end gap-3">
            <Button type="button" variant="ghost" onClick={() => setActiveView('execution')}>
              Cancelar
            </Button>
            <Button type="submit" variant="primary">
              Crear Presupuesto Borrador
            </Button>
          </div>
        </form>
      </div>
    );
  }

  function renderEditDraftView() {
    if (!report) return null;

    return (
      <div className="card-standard bg-card text-card-foreground p-6 space-y-6">
        <div className="flex items-center justify-between border-b border-border pb-4">
          <h3 className="font-bold text-lg text-gray-900 dark:text-white">
            Editar Rubros del Presupuesto Borrador ({report.fiscalPeriod})
          </h3>
          <Button variant="ghost" onClick={() => setActiveView('execution')}>
            Volver
          </Button>
        </div>

        <div className="space-y-4">
          <p className="text-xs text-slate-500 dark:text-zinc-400">
            Modifica los valores presupuestados para las cuentas contables operativas:
          </p>
          
          <div className="max-h-[500px] overflow-y-auto border border-border rounded-lg divide-y divide-border">
            {accounts
              .filter((acc) => !acc.isGroup && (acc.code.startsWith('4') || acc.code.startsWith('5')))
              .map((acc) => {
                const currentVal = draftDetails[acc.id]?.val;
                const inputVal = currentVal === undefined ? '' : currentVal;
                return (
                  <div key={acc.id} className="p-3 grid grid-cols-1 md:grid-cols-3 gap-4 items-center bg-card hover:bg-slate-50/50 dark:hover:bg-zinc-800/10">
                    <div className="font-semibold text-sm">
                      <span className="font-mono text-emerald-600 mr-2">{acc.code}</span>
                      <span className="text-gray-700 dark:text-zinc-300">{acc.name}</span>
                    </div>
                    <div>
                      <input
                        type="number"
                        placeholder="Valor aprobado anual"
                        value={inputVal}
                        onChange={(e) => setDraftDetails({
                          ...draftDetails,
                          [acc.id]: { val: Number(e.target.value), obs: draftDetails[acc.id]?.obs || '' }
                        })}
                        className="input-standard font-mono"
                      />
                    </div>
                    <div>
                      <input
                        type="text"
                        placeholder="Observación"
                        value={draftDetails[acc.id]?.obs || ''}
                        onChange={(e) => setDraftDetails({
                          ...draftDetails,
                          [acc.id]: { val: draftDetails[acc.id]?.val || 0, obs: e.target.value }
                        })}
                        className="input-standard"
                      />
                    </div>
                  </div>
                );
              })}
          </div>
          
          <div className="flex justify-end gap-3">
            <Button type="button" variant="ghost" onClick={() => setActiveView('execution')}>
              Cancelar
            </Button>
            <Button type="button" variant="success" onClick={handleSaveDraftDetails}>
              Guardar Cambios en Borrador
            </Button>
          </div>
        </div>
      </div>
    );
  }

  function renderMovementsView() {
    if (!report) return null;

    return (
      <div className="space-y-6">
        <div className="card-standard p-6 bg-card text-card-foreground flex flex-col md:flex-row items-center justify-between gap-4">
          <div>
            <h3 className="font-bold text-lg text-gray-900 dark:text-white">Traslados y Adiciones Presupuestales</h3>
            <p className="text-xs text-slate-500 dark:text-zinc-400 mt-1">
              Historial completo de modificaciones del presupuesto para el período {report.fiscalPeriod}.
            </p>
          </div>
          {canEdit && report.status === 'Active' && (
            <Button onClick={handleOpenMovementModal} className="flex items-center gap-2">
              <ArrowLeftRight className="w-4 h-4" />
              Registrar Modificación
            </Button>
          )}
        </div>

        <div className="card-standard bg-card text-card-foreground overflow-hidden">
          <table className="w-full border-collapse">
            <thead>
              <tr className="border-b border-border bg-slate-50 dark:bg-zinc-900/50 text-left text-xs font-bold text-slate-400 dark:text-zinc-500 uppercase tracking-widest">
                <th className="py-4 px-6">Tipo</th>
                <th className="py-4 px-6">Cuenta Origen (Traslado)</th>
                <th className="py-4 px-6">Cuenta Destino</th>
                <th className="py-4 px-6 text-right">Monto</th>
                <th className="py-4 px-6">Aprobación / Acta</th>
                <th className="py-4 px-6">Fecha Aprobación</th>
                <th className="py-4 px-6">Justificación</th>
              </tr>
            </thead>
            <tbody className="divide-y divide-border text-sm">
              {movements.length === 0 ? (
                <tr>
                  <td colSpan={7} className="py-12 text-center text-slate-400 dark:text-zinc-500">
                    No se han registrado adiciones ni traslados presupuestales en este período fiscal.
                  </td>
                </tr>
              ) : (
                movements.map((move) => (
                  <tr key={move.id} className="hover:bg-slate-50/50 dark:hover:bg-zinc-900/30">
                    <td className="py-4 px-6 font-bold">
                      <span className={`px-2 py-0.5 rounded text-xs ${getMovementTypeBadgeClass(move.movementType)}`}>
                        {translateMovementType(move.movementType)}
                      </span>
                    </td>
                    <td className="py-4 px-6">
                      {renderSourceAccountCell(move)}
                    </td>
                    <td className="py-4 px-6 font-semibold">
                      <span className="font-mono font-bold mr-1">{move.destinationAccountCode}</span>
                      <span className="text-gray-700 dark:text-zinc-300">{move.destinationAccountName}</span>
                    </td>
                    <td className="py-4 px-6 text-right font-mono font-bold text-gray-900 dark:text-white">
                      {formatCurrency(move.amount)}
                    </td>
                    <td className="py-4 px-6">
                      <div className="font-medium text-gray-700 dark:text-zinc-300">
                        {translateApprovalType(move.approvalType)}
                      </div>
                      <div className="text-xs text-slate-400">{move.meetingActNumber}</div>
                    </td>
                    <td className="py-4 px-6 text-slate-500">
                      {new Date(move.approvalDate).toLocaleDateString('es-CO')}
                    </td>
                    <td className="py-4 px-6 text-xs text-slate-500 max-w-xs truncate" title={move.justification}>
                      {move.justification}
                    </td>
                  </tr>
                ))
              )}
            </tbody>
          </table>
        </div>
      </div>
    );
  }

  function renderNoBudgetView() {
    return (
      <div className="card-standard p-12 text-center bg-card text-card-foreground space-y-6">
        <div className="w-16 h-16 bg-slate-100 dark:bg-zinc-800 rounded-full flex items-center justify-center mx-auto text-slate-400">
          <BadgeDollarSign className="w-8 h-8" />
        </div>
        <div>
          <h3 className="font-bold text-lg text-gray-900 dark:text-white">No existe presupuesto registrado para el año {year}</h3>
          <p className="text-sm text-slate-500 dark:text-zinc-400 mt-1 max-w-md mx-auto">
            Debes registrar la estructura presupuestal aprobada por la asamblea para poder realizar el control de ejecución financiera.
          </p>
        </div>
        {canEdit && (
          <Button onClick={handleOpenCreateView} className="flex items-center gap-2 mx-auto">
            <Plus className="w-4 h-4" />
            Configurar Presupuesto {year}
          </Button>
        )}
      </div>
    );
  }

  function renderMovementDetailCell(value: number, prefix: string, colorClass: string) {
    if (value > 0) {
      return (
        <td className={`py-4 px-6 text-right font-mono text-xs ${colorClass}`}>
          {prefix}{formatCurrency(value)}
        </td>
      );
    }
    return (
      <td className="py-4 px-6 text-right font-mono text-xs">
        —
      </td>
    );
  }

  function renderMainExecutionView() {
    if (!report) return null;

    const incomePercent = getIncomeExecutionPercentage();
    const incomeWidth = Math.min(incomePercent, 100);

    const expensePercent = getExpenseExecutionPercentage();
    const expenseWidth = Math.min(expensePercent, 100);

    let approvalDateString = '';
    if (report.approvalDate) {
      approvalDateString = ` el ${new Date(report.approvalDate).toLocaleDateString('es-CO')}`;
    }

    const reportHeaderButtons = [];

    if (canEdit && report.status === 'Draft') {
      reportHeaderButtons.push(
        <Button key="edit" variant="secondary" onClick={handleOpenEditDraftView} className="flex items-center gap-2">
          <FileText className="w-4 h-4" />
          Editar Rubros
        </Button>
      );
      reportHeaderButtons.push(
        <Button key="activate" variant="primary" onClick={handleOpenActivateModal} className="flex items-center gap-2">
          <Play className="w-4 h-4" />
          Activar Presupuesto
        </Button>
      );
    }

    if (canEdit && report.status === 'Active') {
      reportHeaderButtons.push(
        <Button key="close" variant="secondary" onClick={handleCloseBudget} className="flex items-center gap-2 text-rose-600 dark:text-rose-400 border-rose-100 hover:bg-rose-50 dark:border-rose-900/50 dark:hover:bg-rose-950/20">
          <Lock className="w-4 h-4" />
          Cerrar Año Fiscal
        </Button>
      );
    }

    return (
      <div className="space-y-6">
        {/* HEADER STATUS INFOBAR */}
        <div className="card-standard p-6 bg-card text-card-foreground flex flex-col sm:flex-row items-center justify-between gap-4">
          <div className="flex items-center gap-3">
            <div className={`w-10 h-10 rounded-full flex items-center justify-center ${getBudgetStatusIndicatorClass(report.status)}`}>
              <Activity className="w-5 h-5" />
            </div>
            <div>
              <div className="flex items-center gap-2">
                <span className="font-bold text-gray-900 dark:text-white">Estado del Presupuesto:</span>
                <span className={`px-2 py-0.5 rounded text-xs font-bold ${getBudgetStatusTextClass(report.status)}`}>
                  {translateBudgetStatus(report.status)}
                </span>
              </div>
              <div className="text-xs text-slate-400 mt-0.5">
                Aprobado mediante <strong>{report.meetingActNumber}</strong>
                {approvalDateString}
              </div>
            </div>
          </div>

          <div className="flex items-center gap-2">
            {reportHeaderButtons}
          </div>
        </div>

        {/* OVER-BUDGET ALERTS SECTION */}
        {report.alerts.length > 0 && (
          <div className="card-standard border-rose-200 dark:border-rose-950/50 bg-rose-50/30 dark:bg-rose-950/5 p-6 space-y-3">
            <div className="flex items-center gap-2 text-rose-700 dark:text-rose-400">
              <AlertTriangle className="w-5 h-5 flex-shrink-0" />
              <h4 className="font-bold text-sm">Alertas de Desviación Presupuestal Importante</h4>
            </div>
            <ul className="space-y-1.5 list-disc pl-5 text-xs text-rose-600 dark:text-rose-400">
              {report.alerts.map((alert, i) => (
                <li key={i}>
                  Cuenta <strong className="font-mono">{alert.accountCode} ({alert.accountName})</strong>: {alert.message} — Presupuesto Ajustado: {formatCurrency(alert.adjustedBudget)} / Proyección Cierre: {formatCurrency(alert.closingProjection)}.
                </li>
              ))}
            </ul>
          </div>
        )}

        {/* METRIC SUMMARY CARDS */}
        <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
          {/* INCOME SUMMARY CARD */}
          <div className="card-standard p-6 bg-card text-card-foreground space-y-4">
            <div className="flex items-center justify-between">
              <span className="text-xs font-bold text-slate-400 dark:text-zinc-500 uppercase tracking-widest">Ejecución de Ingresos</span>
              <span className="w-8 h-8 rounded-full bg-emerald-50 dark:bg-emerald-950/20 text-emerald-600 dark:text-emerald-400 flex items-center justify-center">
                <TrendingUp className="w-4 h-4" />
              </span>
            </div>
            <div className="space-y-1">
              <h3 className="text-2xl font-black text-gray-900 dark:text-white">{formatCurrency(totalExecutedIncome)}</h3>
              <p className="text-xs text-slate-500 dark:text-zinc-400">
                Recaudado de {formatCurrency(totalAdjustedIncome)} proyectados
              </p>
            </div>
            <div className="w-full bg-slate-100 dark:bg-zinc-800 h-2 rounded-full overflow-hidden">
              <div 
                className="bg-emerald-500 h-full rounded-full transition-all duration-300"
                style={{ width: `${incomeWidth}%` }}
              />
            </div>
            <div className="flex items-center justify-between text-[11px] text-slate-500 dark:text-zinc-400 font-semibold">
              <span>Avance: {incomePercent.toFixed(1)}%</span>
              <span>Faltante: {formatCurrency(Math.max(totalAdjustedIncome - totalExecutedIncome, 0))}</span>
            </div>
          </div>

          {/* EXPENSE SUMMARY CARD */}
          <div className="card-standard p-6 bg-card text-card-foreground space-y-4">
            <div className="flex items-center justify-between">
              <span className="text-xs font-bold text-slate-400 dark:text-zinc-500 uppercase tracking-widest">Ejecución de Gastos</span>
              <span className="w-8 h-8 rounded-full bg-rose-50 dark:bg-rose-950/20 text-rose-600 dark:text-rose-400 flex items-center justify-center">
                <TrendingDown className="w-4 h-4" />
              </span>
            </div>
            <div className="space-y-1">
              <h3 className="text-2xl font-black text-gray-900 dark:text-white">{formatCurrency(totalExecutedExpense)}</h3>
              <p className="text-xs text-slate-500 dark:text-zinc-400">
                Gastado de {formatCurrency(totalAdjustedExpense)} autorizados
              </p>
            </div>
            <div className="w-full bg-slate-100 dark:bg-zinc-800 h-2 rounded-full overflow-hidden">
              <div 
                className={`h-full rounded-full transition-all duration-300 ${getExpenseProgressBarColor()}`}
                style={{ width: `${expenseWidth}%` }}
              />
            </div>
            <div className="flex items-center justify-between text-[11px] text-slate-500 dark:text-zinc-400 font-semibold">
              <span>Consumido: {expensePercent.toFixed(1)}%</span>
              {renderExpenseBalanceLabel()}
            </div>
          </div>

          {/* BALANCE CARD */}
          <div className="card-standard p-6 bg-card text-card-foreground space-y-4">
            <div className="flex items-center justify-between">
              <span className="text-xs font-bold text-slate-400 dark:text-zinc-500 uppercase tracking-widest">Resultado de Caja Ejecutado</span>
              <span className="w-8 h-8 rounded-full bg-blue-50 dark:bg-blue-950/20 text-blue-600 dark:text-blue-400 flex items-center justify-center">
                <DollarSign className="w-4 h-4" />
              </span>
            </div>
            <div className="space-y-1">
              <h3 className={`text-2xl font-black ${getSurplusTextColor()}`}>
                {formatCurrency(totalExecutedIncome - totalExecutedExpense)}
              </h3>
              <p className="text-xs text-slate-500 dark:text-zinc-400">
                Diferencia real entre ingresos y gastos del período
              </p>
            </div>
            <div className="p-3 bg-slate-50 dark:bg-zinc-900 rounded-lg border border-border text-[11px] font-semibold text-slate-500 dark:text-zinc-400 flex items-center justify-between">
              <span>Resultado Presupuestado:</span>
              <span className="font-mono text-gray-700 dark:text-zinc-300">
                {formatCurrency(totalAdjustedIncome - totalAdjustedExpense)}
              </span>
            </div>
          </div>
        </div>

        {/* DETAILED BUDGET ITEMS TABLE */}
        <div className="card-standard bg-card text-card-foreground">
          <div className="p-6 border-b border-border">
            <h3 className="font-bold text-gray-900 dark:text-white">Detalle de Ejecución de Rubros</h3>
          </div>
          
          <div className="overflow-x-auto">
            <table className="w-full border-collapse">
              <thead>
                <tr className="border-b border-border bg-slate-50 dark:bg-zinc-900/50 text-left text-xs font-bold text-slate-400 dark:text-zinc-500 uppercase tracking-widest">
                  <th className="py-4 px-6">Código / Rubro</th>
                  <th className="py-4 px-6 text-right">Inicial</th>
                  <th className="py-4 px-6 text-right">Adiciones</th>
                  <th className="py-4 px-6 text-right">Traslados (+)</th>
                  <th className="py-4 px-6 text-right">Traslados (-)</th>
                  <th className="py-4 px-6 text-right">Ajustado</th>
                  <th className="py-4 px-6 text-right">Ejecutado</th>
                  <th className="py-4 px-6 text-right">Disponible</th>
                  <th className="py-4 px-6 text-center">% Ejec.</th>
                  <th className="py-4 px-6 text-center">Proyección</th>
                </tr>
              </thead>
              <tbody className="divide-y divide-border text-sm">
                {report.items.map((item) => {
                  const isGroup = item.isGroup;
                  let rowClass = 'hover:bg-slate-50/50 dark:hover:bg-zinc-900/30';
                  if (isGroup) {
                    rowClass = 'font-bold bg-slate-50/30 dark:bg-zinc-900/10';
                  }

                  let indentClass = 'pl-14';
                  if (item.accountCode.length === 1) {
                    indentClass = 'pl-6 font-black';
                  } else if (item.accountCode.length === 2) {
                    indentClass = 'pl-10 font-bold';
                  }

                  let availableValueCellClass = '';
                  if (item.availableValue < 0 && !isGroup) {
                    availableValueCellClass = 'text-rose-600 font-bold';
                  }

                  return (
                    <tr key={item.accountId} className={`${rowClass} transition-colors`}>
                      {/* Account Code & Name */}
                      <td className={`py-4 px-6 ${indentClass} max-w-xs truncate`}>
                        <span className="font-mono text-emerald-600 mr-2">{item.accountCode}</span>
                        <span className="text-gray-700 dark:text-zinc-300">{item.accountName}</span>
                      </td>

                      {/* Approved Value */}
                      <td className="py-4 px-6 text-right font-mono text-xs">
                        {formatCurrency(item.approvedValue)}
                      </td>

                      {/* Additions */}
                      {renderMovementDetailCell(item.additions, '+', 'text-emerald-600')}

                      {/* Transfers In */}
                      {renderMovementDetailCell(item.transfersIn, '+', 'text-blue-600')}

                      {/* Transfers Out */}
                      {renderMovementDetailCell(item.transfersOut, '-', 'text-rose-600')}

                      {/* Adjusted Budget */}
                      <td className="py-4 px-6 text-right font-mono text-xs font-bold text-gray-900 dark:text-white">
                        {formatCurrency(item.adjustedBudget)}
                      </td>

                      {/* Executed Value */}
                      <td className="py-4 px-6 text-right font-mono text-xs font-bold">
                        {formatCurrency(item.executedValue)}
                      </td>

                      {/* Available Value */}
                      <td className={`py-4 px-6 text-right font-mono text-xs ${availableValueCellClass}`}>
                        {formatCurrency(item.availableValue)}
                      </td>

                      {/* Execution % */}
                      <td className={`py-4 px-6 text-center font-mono text-xs ${getTrafficLightColor(item.executionPercentage)}`}>
                        {item.executionPercentage.toFixed(1)}%
                      </td>

                      {/* Closing Projection */}
                      <td className="py-4 px-6 text-center font-mono text-xs text-slate-500">
                        {formatCurrency(item.closingProjection)}
                      </td>
                    </tr>
                  );
                })}
              </tbody>
            </table>
          </div>
        </div>
      </div>
    );
  }

  function renderActivateBudgetModal() {
    if (!isActivateOpen || !report) return null;

    return (
      <div className="fixed inset-0 bg-black/60 backdrop-blur-sm z-[150] flex items-center justify-center p-4">
        <div className="bg-card text-card-foreground w-full max-w-md rounded-xl border border-border shadow-lg overflow-hidden animate-in zoom-in-95 duration-200">
          <div className="p-6 border-b border-border flex items-center justify-between">
            <h3 className="font-bold text-lg text-gray-900 dark:text-white">Activar Presupuesto Anual</h3>
            <button onClick={() => setIsActivateOpen(false)} className="text-slate-400 hover:text-slate-600">
              <X className="w-5 h-5" />
            </button>
          </div>

          <form onSubmit={handleActivateBudget} className="p-6 space-y-4">
            <div className="p-3 bg-blue-50 dark:bg-blue-950/20 text-blue-700 dark:text-blue-400 rounded-lg text-xs space-y-1.5 border border-blue-100 dark:border-blue-900/50">
              <p className="font-bold">Información Importante:</p>
              <p>Al activar el presupuesto, los rubros se consideran oficialmente aprobados. Esto bloqueará la edición directa de rubros y habilitará el motor de control presupuestario.</p>
            </div>

            <div>
              <label className="block text-xs font-bold text-slate-400 dark:text-zinc-500 uppercase tracking-widest mb-1">
                Número de Acta de Aprobación de la Asamblea
              </label>
              <input
                type="text"
                placeholder="Ej. Acta 042 Ordinaria"
                value={actNumber}
                onChange={(e) => setActNumber(e.target.value)}
                className="input-standard"
                required
              />
            </div>

            <div>
              <label className="block text-xs font-bold text-slate-400 dark:text-zinc-500 uppercase tracking-widest mb-1">
                Fecha de Aprobación de la Asamblea
              </label>
              <input
                type="date"
                value={approvalDate}
                onChange={(e) => setApprovalDate(e.target.value)}
                className="input-standard"
                required
              />
            </div>

            <div className="pt-4 flex justify-end gap-3 border-t border-border">
              <Button type="button" variant="ghost" onClick={() => setIsActivateOpen(false)}>
                Cancelar
              </Button>
              <Button type="submit" variant="primary" className="flex items-center gap-2">
                <Play className="w-4 h-4" />
                Activar Ahora
              </Button>
            </div>
          </form>
        </div>
      </div>
    );
  }

  function renderCreateMovementModal() {
    if (!isMovementOpen || !report) return null;

    const sourceAccountOptions = report.items
      .filter((item) => !item.isGroup && item.accountCode.startsWith('5')) // Exp.
      .map((item) => (
        <option key={item.accountId} value={item.accountId}>
          {item.accountCode} - {item.accountName} (Disponible: {formatCurrency(item.availableValue)})
        </option>
      ));

    const destAccountOptions = report.items
      .filter((item) => !item.isGroup && (item.accountCode.startsWith('4') || item.accountCode.startsWith('5')))
      .map((item) => (
        <option key={item.accountId} value={item.accountId}>
          {item.accountCode} - {item.accountName}
        </option>
      ));

    return (
      <div className="fixed inset-0 bg-black/60 backdrop-blur-sm z-[150] flex items-center justify-center p-4">
        <div className="bg-card text-card-foreground w-full max-w-lg rounded-xl border border-border shadow-lg overflow-hidden animate-in zoom-in-95 duration-200">
          <div className="p-6 border-b border-border flex items-center justify-between">
            <h3 className="font-bold text-lg text-gray-900 dark:text-white">Registrar Modificación Presupuestal</h3>
            <button onClick={() => setIsMovementOpen(false)} className="text-slate-400 hover:text-slate-600">
              <X className="w-5 h-5" />
            </button>
          </div>

          <form onSubmit={handleCreateMovement} className="p-6 space-y-4">
            {/* MOVEMENT TYPE */}
            <div>
              <label className="block text-xs font-bold text-slate-400 dark:text-zinc-500 uppercase tracking-widest mb-1">
                Tipo de Operación
              </label>
              <div className="grid grid-cols-2 gap-2 bg-slate-100 dark:bg-zinc-950 p-1 rounded-lg">
                <button
                  type="button"
                  onClick={() => setMovementType('Transfer')}
                  className={`py-2 text-xs font-bold rounded-md transition-all ${
                    movementType === 'Transfer'
                      ? 'bg-card text-foreground shadow-sm'
                      : 'text-slate-500 dark:text-zinc-400'
                  }`}
                >
                  Traslado (Entre Rubros)
                </button>
                <button
                  type="button"
                  onClick={() => setMovementType('Addition')}
                  className={`py-2 text-xs font-bold rounded-md transition-all ${
                    movementType === 'Addition'
                      ? 'bg-card text-foreground shadow-sm'
                      : 'text-slate-500 dark:text-zinc-400'
                  }`}
                >
                  Adición (Aumento Global)
                </button>
              </div>
            </div>

            {/* SOURCE ACCOUNT (ONLY FOR TRANSFERS) */}
            {movementType === 'Transfer' && (
              <div>
                <label className="block text-xs font-bold text-slate-400 dark:text-zinc-500 uppercase tracking-widest mb-1">
                  Cuenta Origen (Se reduce saldo)
                </label>
                <select
                  value={sourceAccountId}
                  onChange={(e) => setSourceAccountId(e.target.value)}
                  className="input-standard"
                  required
                >
                  <option value="">Seleccione cuenta origen...</option>
                  {sourceAccountOptions}
                </select>
              </div>
            )}

            {/* DESTINATION ACCOUNT */}
            <div>
              <label className="block text-xs font-bold text-slate-400 dark:text-zinc-500 uppercase tracking-widest mb-1">
                Cuenta Destino (Se incrementa saldo)
              </label>
              <select
                value={destinationAccountId}
                onChange={(e) => setDestinationAccountId(e.target.value)}
                className="input-standard"
                required
              >
                <option value="">Seleccione cuenta destino...</option>
                {destAccountOptions}
              </select>
            </div>

            {/* AMOUNT */}
            <div>
              <label className="block text-xs font-bold text-slate-400 dark:text-zinc-500 uppercase tracking-widest mb-1">
                Monto del Ajuste (COP)
              </label>
              <input
                type="number"
                placeholder="0"
                value={movementAmount || ''}
                onChange={(e) => setMovementAmount(Number(e.target.value))}
                className="input-standard font-mono font-bold"
                required
              />
            </div>

            {/* JUSTIFICATION */}
            <div>
              <label className="block text-xs font-bold text-slate-400 dark:text-zinc-500 uppercase tracking-widest mb-1">
                Justificación Técnica / Financiera
              </label>
              <textarea
                placeholder="Ej. Reubicación de fondos para cubrir mantenimiento de ascensores..."
                value={justification}
                onChange={(e) => setJustification(e.target.value)}
                className="input-standard min-h-[60px]"
                required
              />
            </div>

            {/* APPROVAL DETAILS */}
            <div className="grid grid-cols-2 gap-4 border-t border-border pt-4">
              <div>
                <label className="block text-xs font-bold text-slate-400 dark:text-zinc-500 uppercase tracking-widest mb-1">
                  Tipo de Aprobación
                </label>
                <select
                  value={approvalType}
                  onChange={(e) => setApprovalType(e.target.value as any)}
                  className="input-standard"
                >
                  <option value="Council">Consejo de Administración</option>
                  <option value="Assembly">Asamblea de Copropietarios</option>
                </select>
              </div>

              <div>
                <label className="block text-xs font-bold text-slate-400 dark:text-zinc-500 uppercase tracking-widest mb-1">
                  Acta de Aprobación
                </label>
                <input
                  type="text"
                  placeholder="Ej. Acta Consejo 12"
                  value={movementActNumber}
                  onChange={(e) => setMovementActNumber(e.target.value)}
                  className="input-standard"
                  required
                />
              </div>
            </div>

            <div>
              <label className="block text-xs font-bold text-slate-400 dark:text-zinc-500 uppercase tracking-widest mb-1">
                Fecha de Aprobación
              </label>
              <input
                type="date"
                value={movementApprovalDate}
                onChange={(e) => setMovementApprovalDate(e.target.value)}
                className="input-standard"
                required
              />
            </div>

            <div className="pt-4 flex justify-end gap-3 border-t border-border">
              <Button type="button" variant="ghost" onClick={() => setIsMovementOpen(false)}>
                Cancelar
              </Button>
              <Button type="submit" variant="primary">
                Registrar Operación
              </Button>
            </div>
          </form>
        </div>
      </div>
    );
  }

  function renderDashboardBody() {
    if (isLoading) {
      return (
        <div className="card-standard p-12 text-center text-slate-400 dark:text-zinc-500 bg-card">
          <p className="text-base font-semibold">Procesando información del presupuesto fiscal...</p>
        </div>
      );
    }
    
    if (activeView === 'create') {
      return renderCreateView();
    }
    
    if (activeView === 'edit-draft') {
      return renderEditDraftView();
    }
    
    if (activeView === 'movements') {
      return renderMovementsView();
    }
    
    if (!report) {
      return renderNoBudgetView();
    }
    
    return renderMainExecutionView();
  }

  return (
    <div className="space-y-6">
      {/* HEADER */}
      <div className="flex flex-col sm:flex-row sm:items-center sm:justify-between gap-4">
        <div>
          <h1 className="text-2xl font-bold text-gray-900 tracking-tight dark:text-white">Presupuesto y Ejecución</h1>
          <p className="text-sm text-gray-500 mt-1 dark:text-zinc-400">
            Aprobación del presupuesto anual de copropietarios, ejecución en tiempo real y flujo de adiciones o traslados.
          </p>
        </div>

        {/* YEAR PICKER & TABS */}
        <div className="flex items-center gap-2">
          <div className="flex items-center gap-1.5 bg-card border border-border rounded-lg px-3 py-1.5">
            <Calendar className="w-4 h-4 text-slate-400" />
            <select
              value={year}
              onChange={(e) => {
                setYear(Number(e.target.value));
                setActiveView('execution');
              }}
              className="bg-transparent border-0 font-bold focus:ring-0 text-sm focus:outline-none dark:text-white"
            >
              {[year - 3, year - 2, year - 1, year, year + 1, year + 2].map((y) => (
                <option key={y} value={y} className="dark:bg-zinc-900">{y}</option>
              ))}
            </select>
          </div>

          {report && (
            <div className="flex bg-slate-100 dark:bg-zinc-800 p-1 rounded-lg border border-border">
              <button
                onClick={() => setActiveView('execution')}
                className={getTabButtonClass(activeView === 'execution')}
              >
                Ejecución
              </button>
              <button
                onClick={() => setActiveView('movements')}
                className={getTabButtonClass(activeView === 'movements')}
              >
                Modificaciones ({movements.length})
              </button>
            </div>
          )}
        </div>
      </div>

      {/* ERROR & SUCCESS */}
      {error && (
        <div className="flex items-center gap-3 p-4 bg-rose-50 dark:bg-rose-950/20 text-rose-700 dark:text-rose-400 rounded-xl border border-rose-100 dark:border-rose-900/50 animate-in fade-in duration-300">
          <AlertTriangle className="w-5 h-5 flex-shrink-0" />
          <p className="text-sm font-semibold">{error}</p>
        </div>
      )}

      {success && (
        <div className="flex items-center gap-3 p-4 bg-emerald-50 dark:bg-emerald-950/20 text-emerald-700 dark:text-emerald-400 rounded-xl border border-emerald-100 dark:border-emerald-900/50 animate-in fade-in duration-300">
          <Check className="w-5 h-5 flex-shrink-0" />
          <p className="text-sm font-semibold">{success}</p>
        </div>
      )}

      {/* RENDER DYNAMIC VIEW */}
      {renderDashboardBody()}

      {/* ACTIVATE BUDGET MODAL */}
      {renderActivateBudgetModal()}

      {/* CREATE BUDGET MOVEMENT MODAL */}
      {renderCreateMovementModal()}
    </div>
  );
}
