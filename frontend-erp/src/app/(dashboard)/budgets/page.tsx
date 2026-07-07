'use client';

import React, { useState, useEffect } from 'react';
import { useAuth } from '@/context/AuthContext';
import budgetService, {
  BudgetSummary, BudgetDetail, IncomeItem, ExpenseItem,
  ExpenseExecutionItem, BudgetAlert,
  ContingencyFundStatus, ContingencyFundUsage,
  BudgetExecutionDashboard, ExecutedExpense, BudgetModification
} from '@/lib/budget-service';
import {
  Plus, TrendingUp, TrendingDown, AlertTriangle, Check, FileText,
  ArrowLeftRight, Play, Lock, Calendar, X, DollarSign, BadgeDollarSign,
  ClipboardList, BarChart3, RefreshCw, Eye, Pencil, Trash2
} from 'lucide-react';
import { Button } from '@/components/ui/Button';
import { Card, CardHeader, CardContent } from '@/components/ui/Card';

type TabKey = 'list' | 'execution' | 'expenses' | 'modifications' | 'contingency';

export default function BudgetsPage() {
  const { user } = useAuth();
  const canEdit = user?.role === 'SuperAdmin' || user?.role === 'Admin' || user?.role === 'Accountant';

  const [year, setYear] = useState<number>(new Date().getFullYear());
  const [tab, setTab] = useState<TabKey>('list');
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState('');
  const [success, setSuccess] = useState('');

  const [budgets, setBudgets] = useState<BudgetSummary[]>([]);
  const [selectedBudget, setSelectedBudget] = useState<BudgetDetail | null>(null);
  const [dashboard, setDashboard] = useState<BudgetExecutionDashboard | null>(null);
  const [expenses, setExpenses] = useState<ExecutedExpense[]>([]);
  const [modifications, setModifications] = useState<BudgetModification[]>([]);
  const [contingency, setContingency] = useState<ContingencyFundStatus | null>(null);

  const [showCreate, setShowCreate] = useState(false);
  const [showApprove, setShowApprove] = useState(false);
  const [showExpense, setShowExpense] = useState(false);
  const [expenseItemOptions, setExpenseItemOptions] = useState<{ id: string; name: string; budgetLabel: string }[]>([]);
  const [showModification, setShowModification] = useState(false);
  const [showContingencyUsage, setShowContingencyUsage] = useState(false);
  const [showEditItems, setShowEditItems] = useState(false);
  const [editIncomeItems, setEditIncomeItems] = useState<{ name: string; description: string; annualValue: number }[]>([]);
  const [editExpenseItems, setEditExpenseItems] = useState<{ name: string; description: string; category: string; annualValue: number; isContingencyFund: boolean; requiresCouncilApproval: boolean }[]>([]);

  const fetchBudgets = async () => {
    setLoading(true);
    try {
      const data = await budgetService.getBudgets(year);
      setBudgets(data);
    } catch { setError('Error al cargar presupuestos'); }
    finally { setLoading(false); }
  };

  const fetchDashboard = async () => {
    setLoading(true);
    try {
      const data = await budgetService.getBudgetExecution(year);
      setDashboard(data);
    } catch { setDashboard(null); }
    finally { setLoading(false); }
  };

  const fetchExpenses = async () => {
    try {
      const data = await budgetService.getExpenses();
      setExpenses(data);
    } catch { setExpenses([]); }
  };

  const fetchModifications = async (budgetId: string) => {
    try {
      const data = await budgetService.getModifications(budgetId);
      setModifications(data);
    } catch { setModifications([]); }
  };

  const fetchContingency = async () => {
    try {
      const data = await budgetService.getContingencyFundStatus();
      setContingency(data);
    } catch { setContingency(null); }
  };

  useEffect(() => { fetchBudgets(); }, [year]);
  useEffect(() => { if (tab === 'execution') fetchDashboard(); }, [tab, year]);
  useEffect(() => { if (tab === 'expenses') fetchExpenses(); }, [tab]);
  useEffect(() => { if (tab === 'contingency') fetchContingency(); }, [tab]);

  const handleSelectBudget = async (id: string) => {
    try {
      const data = await budgetService.getBudget(id);
      setSelectedBudget(data);
      fetchModifications(id);
    } catch { setError('Error al cargar detalle del presupuesto'); }
  };

  const handleCreateBudget = async (e: React.FormEvent<HTMLFormElement>) => {
    e.preventDefault();
    const form = new FormData(e.currentTarget);
    try {
      await budgetService.createBudget({
        fiscalYear: Number(form.get('fiscalYear')),
        meetingActNumber: form.get('meetingActNumber') as string,
        approvalDate: (form.get('approvalDate') as string) || undefined,
        observations: form.get('observations') as string,
        copyFromPrevious: form.get('copyFromPrevious') === 'on',
        globalPercentageAdjustment: form.get('globalAdjustment') ? Number(form.get('globalAdjustment')) : undefined,
      });
      console.log('Presupuesto creado exitosamente');
      setSuccess('Presupuesto creado exitosamente');
      setShowCreate(false);
      fetchBudgets();
    } catch (err) {
      const anyErr = err as { response?: { data?: unknown }; message?: string };
      const msg = anyErr?.response?.data || anyErr?.message || 'Error desconocido';
      console.error('Error al crear presupuesto:', msg);
      setError(typeof msg === 'string' ? msg : 'Error al crear presupuesto');
    }
  };

  const openEditItems = async () => {
    if (!selectedBudget) return;
    setEditIncomeItems(selectedBudget.incomeItems.map(i => ({ name: i.name, description: i.description, annualValue: i.annualValue })));
    setEditExpenseItems(selectedBudget.expenseItems.map(e => ({ name: e.name, description: e.description, category: e.category, annualValue: e.annualValue, isContingencyFund: e.isContingencyFund, requiresCouncilApproval: e.requiresCouncilApproval })));
    setShowEditItems(true);
  };

  const handleSaveItems = async () => {
    if (!selectedBudget) return;
    try {
      await budgetService.updateDraftBudget(selectedBudget.id, {
        incomeItems: editIncomeItems.map(i => ({ name: i.name, description: i.description, annualValue: i.annualValue })),
        expenseItems: editExpenseItems.map(e => ({ name: e.name, description: e.description, category: e.category, annualValue: e.annualValue, isContingencyFund: e.isContingencyFund, contingencyPercentage: e.isContingencyFund ? 5 : 0, requiresCouncilApproval: e.requiresCouncilApproval, approvalThreshold: 0 })),
      });
      setSuccess('Partidas guardadas exitosamente');
      setShowEditItems(false);
      handleSelectBudget(selectedBudget.id);
    } catch { setError('Error al guardar partidas'); }
  };

  const handleApproveBudget = async (e: React.FormEvent<HTMLFormElement>) => {
    e.preventDefault();
    const form = new FormData(e.currentTarget);
    if (!selectedBudget) return;
    try {
      await budgetService.approveBudget(selectedBudget.id, {
        meetingActNumber: form.get('meetingActNumber') as string,
        approvalDate: form.get('approvalDate') as string,
      });
      setSuccess('Presupuesto aprobado exitosamente');
      setShowApprove(false);
      setSelectedBudget(null);
      fetchBudgets();
    } catch { setError('Error al aprobar presupuesto'); }
  };

  useEffect(() => {
    if (!showExpense) return;
    const loadItems = async () => {
      const approved = budgets.filter(b => b.status === 'Approved');
      const items: { id: string; name: string; budgetLabel: string }[] = [];
      for (const b of approved) {
        try {
          const detail = await budgetService.getBudget(b.id);
          detail.expenseItems.forEach(ei => items.push({ id: ei.id, name: ei.name, budgetLabel: `Ppto ${detail.fiscalYear}` }));
        } catch { }
      }
      setExpenseItemOptions(items);
    };
    loadItems();
  }, [showExpense]);

  const handleRecordExpense = async (e: React.FormEvent<HTMLFormElement>) => {
    e.preventDefault();
    const form = new FormData(e.currentTarget);
    try {
      await budgetService.recordExpense({
        expenseItemId: form.get('expenseItemId') as string,
        description: form.get('description') as string,
        amount: Number(form.get('amount')),
        expenseDate: form.get('expenseDate') as string,
        invoiceReference: form.get('invoiceReference') as string,
      });
      setSuccess('Gasto registrado exitosamente');
      setShowExpense(false);
      fetchExpenses();
      if (dashboard) fetchDashboard();
    } catch { setError('Error al registrar gasto'); }
  };

  const handleCreateModification = async (e: React.FormEvent<HTMLFormElement>) => {
    e.preventDefault();
    const form = new FormData(e.currentTarget);
    if (!selectedBudget) return;
    try {
      await budgetService.createModification({
        budgetId: selectedBudget.id,
        expenseItemId: form.get('expenseItemId') ? form.get('expenseItemId') as string : undefined,
        incomeItemId: form.get('incomeItemId') ? form.get('incomeItemId') as string : undefined,
        modificationType: form.get('modificationType') as string,
        amount: Number(form.get('amount')),
        justification: form.get('justification') as string,
        approvalType: form.get('approvalType') as string,
        meetingActNumber: form.get('meetingActNumber') as string,
        approvalDate: form.get('approvalDate') as string,
      });
      setSuccess('Modificación creada exitosamente');
      setShowModification(false);
      fetchModifications(selectedBudget.id);
    } catch { setError('Error al crear modificación'); }
  };

  const handleContingencyUsage = async (e: React.FormEvent<HTMLFormElement>) => {
    e.preventDefault();
    const form = new FormData(e.currentTarget);
    try {
      const budgetId = form.get('budgetId') as string;
      await budgetService.recordContingencyFundUsage({
        budgetId,
        justification: form.get('justification') as string,
        amount: Number(form.get('amount')),
        councilApprovalActNumber: form.get('councilApprovalActNumber') as string,
      });
      setSuccess('Uso de fondo registrado exitosamente');
      setShowContingencyUsage(false);
      fetchContingency();
    } catch { setError('Error al registrar uso de fondo'); }
  };

  const handleGenerateNext = async (id: string) => {
    try {
      await budgetService.generateNextBudget(id);
      setSuccess('Presupuesto del siguiente período generado');
      fetchBudgets();
    } catch { setError('Error al generar siguiente período'); }
  };

  const trafficColor = (t: string) => {
    if (t === 'Green') return 'text-green-600 bg-green-100 dark:bg-green-900/30';
    if (t === 'Yellow') return 'text-yellow-600 bg-yellow-100 dark:bg-yellow-900/30';
    return 'text-red-600 bg-red-100 dark:bg-red-900/30';
  };

  const statusBadge = (s: string) => {
    const colors: Record<string, string> = {
      Draft: 'bg-gray-100 text-gray-700 dark:bg-gray-800 dark:text-gray-300',
      Submitted: 'bg-blue-100 text-blue-700 dark:bg-blue-900/30 dark:text-blue-300',
      Approved: 'bg-green-100 text-green-700 dark:bg-green-900/30 dark:text-green-300',
      Rejected: 'bg-red-100 text-red-700 dark:bg-red-900/30 dark:text-red-300',
      Closed: 'bg-purple-100 text-purple-700 dark:bg-purple-900/30 dark:text-purple-300',
    };
    return colors[s] || colors.Draft;
  };

  const renderTabs = () => (
    <div className="flex gap-2 mb-6 flex-wrap">
      {[
        { key: 'list' as TabKey, label: 'Presupuestos', icon: ClipboardList },
        { key: 'execution' as TabKey, label: 'Ejecución', icon: BarChart3 },
        { key: 'expenses' as TabKey, label: 'Gastos', icon: DollarSign },
        { key: 'modifications' as TabKey, label: 'Modificaciones', icon: ArrowLeftRight },
        { key: 'contingency' as TabKey, label: 'Fondo Imprevistos', icon: BadgeDollarSign },
      ].map(t => (
        <button
          key={t.key}
          onClick={() => { setTab(t.key); setSelectedBudget(null); }}
          className={`flex items-center gap-2 px-4 py-2 rounded-lg text-sm font-medium transition-colors ${
            tab === t.key
              ? 'bg-primary text-primary-foreground shadow'
              : 'bg-card hover:bg-accent text-muted-foreground border border-border'
          }`}
        >
          <t.icon className="w-4 h-4" />
          {t.label}
        </button>
      ))}
    </div>
  );

  const renderBudgetList = () => (
    <div>
      <div className="flex items-center justify-between mb-4">
        <div className="flex items-center gap-3">
          <h2 className="text-lg font-semibold">Presupuestos</h2>
          <select
            value={year}
            onChange={e => setYear(Number(e.target.value))}
            className="text-sm border border-border rounded-lg px-3 py-1.5 bg-card"
          >
            {[2024, 2025, 2026, 2027, 2028].map(y => (
              <option key={y} value={y}>{y}</option>
            ))}
          </select>
        </div>
        {canEdit && (
          <Button variant="primary" onClick={() => setShowCreate(true)}>
            <Plus className="w-4 h-4 mr-1" /> Nuevo Presupuesto
          </Button>
        )}
      </div>

      {loading ? (
        <div className="text-center py-12 text-muted-foreground">Cargando...</div>
      ) : budgets.length === 0 ? (
        <Card>
          <CardContent className="text-center py-12 text-muted-foreground">
            No hay presupuestos para el año {year}
          </CardContent>
        </Card>
      ) : (
        <div className="grid gap-4">
          {budgets.map(b => (
            <Card key={b.id} className="hover:shadow-md transition-shadow">
              <CardContent className="p-5">
                <div className="flex items-start justify-between">
                  <div className="flex-1">
                    <div className="flex items-center gap-3 mb-2">
                      <span className="text-lg font-semibold">Presupuesto {b.fiscalYear}</span>
                      <span className={`text-xs font-medium px-2.5 py-0.5 rounded-full ${statusBadge(b.status)}`}>
                        {b.status === 'Draft' ? 'Borrador' : b.status === 'Submitted' ? 'En Revisión' : b.status === 'Approved' ? 'Aprobado' : b.status === 'Rejected' ? 'Rechazado' : b.status === 'Closed' ? 'Cerrado' : b.status}
                      </span>
                    </div>
                    {b.meetingActNumber && (
                      <p className="text-sm text-muted-foreground">Acta: {b.meetingActNumber}</p>
                    )}
                    <div className="grid grid-cols-3 gap-4 mt-3 text-sm">
                      <div>
                        <span className="text-muted-foreground">Ingresos: </span>
                        <span className="font-medium">${b.totalIncome.toLocaleString()}</span>
                      </div>
                      <div>
                        <span className="text-muted-foreground">Gastos: </span>
                        <span className="font-medium">${b.totalExpense.toLocaleString()}</span>
                      </div>
                      <div>
                        <span className="text-muted-foreground">Partidas: </span>
                        <span className="font-medium">{b.incomeItemsCount + b.expenseItemsCount}</span>
                      </div>
                    </div>
                    {b.observations && (
                      <p className="text-xs text-muted-foreground mt-2 italic">{b.observations}</p>
                    )}
                  </div>
                  <div className="flex items-center gap-2 ml-4">
                    <button
                      onClick={() => handleSelectBudget(b.id)}
                      className="p-2 hover:bg-accent rounded-lg transition-colors"
                      title="Ver detalle"
                    >
                      <Eye className="w-4 h-4" />
                    </button>
                    {canEdit && b.status === 'Draft' && (
                      <button
                        onClick={() => { handleSelectBudget(b.id); setShowApprove(true); }}
                        className="p-2 hover:bg-accent rounded-lg transition-colors text-green-600"
                        title="Aprobar"
                      >
                        <Check className="w-4 h-4" />
                      </button>
                    )}
                    {canEdit && b.status === 'Approved' && (
                      <>
                        <button
                          onClick={() => handleGenerateNext(b.id)}
                          className="p-2 hover:bg-accent rounded-lg transition-colors"
                          title="Generar siguiente período"
                        >
                          <RefreshCw className="w-4 h-4" />
                        </button>
                      </>
                    )}
                  </div>
                </div>
              </CardContent>
            </Card>
          ))}
        </div>
      )}

      {selectedBudget && (
        <Card className="mt-6">
          <CardHeader>
            <div className="flex items-center justify-between">
              <h3 className="font-semibold">Detalle del Presupuesto {selectedBudget.fiscalYear}</h3>
              <div className="flex items-center gap-2">
                {canEdit && selectedBudget.status === 'Draft' && (
                  <Button variant="secondary" onClick={openEditItems}>
                    <Pencil className="w-4 h-4 mr-1" /> Editar Partidas
                  </Button>
                )}
                <button onClick={() => setSelectedBudget(null)} className="p-1 hover:bg-accent rounded">
                  <X className="w-5 h-5" />
                </button>
              </div>
            </div>
          </CardHeader>
          <CardContent>
            <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
              <div>
                <h4 className="font-medium text-sm mb-2 flex items-center gap-2">
                  <TrendingUp className="w-4 h-4 text-green-600" /> Ingresos
                </h4>
                <table className="w-full text-sm">
                  <thead>
                    <tr className="border-b border-border">
                      <th className="text-left py-2">Nombre</th>
                      <th className="text-right py-2">Valor Anual</th>
                    </tr>
                  </thead>
                  <tbody>
                    {selectedBudget.incomeItems.map(i => (
                      <tr key={i.id} className="border-b border-border/50">
                        <td className="py-2">{i.name}</td>
                        <td className="text-right py-2">${i.annualValue.toLocaleString()}</td>
                      </tr>
                    ))}
                  </tbody>
                  <tfoot>
                    <tr className="font-semibold">
                      <td className="py-2">Total Ingresos</td>
                      <td className="text-right py-2">${selectedBudget.incomeItems.reduce((s, i) => s + i.annualValue, 0).toLocaleString()}</td>
                    </tr>
                  </tfoot>
                </table>
              </div>
              <div>
                <h4 className="font-medium text-sm mb-2 flex items-center gap-2">
                  <TrendingDown className="w-4 h-4 text-red-600" /> Gastos
                </h4>
                <table className="w-full text-sm">
                  <thead>
                    <tr className="border-b border-border">
                      <th className="text-left py-2">Nombre</th>
                      <th className="text-center py-2">Categoría</th>
                      <th className="text-right py-2">Valor Anual</th>
                    </tr>
                  </thead>
                  <tbody>
                    {selectedBudget.expenseItems.map(e => (
                      <tr key={e.id} className="border-b border-border/50">
                        <td className="py-2">{e.name} {e.isContingencyFund ? '(Fondo Imprevistos)' : ''}</td>
                        <td className="text-center py-2 text-xs">{e.category}</td>
                        <td className="text-right py-2">${e.annualValue.toLocaleString()}</td>
                      </tr>
                    ))}
                  </tbody>
                  <tfoot>
                    <tr className="font-semibold">
                      <td className="py-2">Total Gastos</td>
                      <td></td>
                      <td className="text-right py-2">${selectedBudget.expenseItems.reduce((s, e) => s + e.annualValue, 0).toLocaleString()}</td>
                    </tr>
                  </tfoot>
                </table>
              </div>
            </div>
          </CardContent>
        </Card>
      )}

      {showApprove && selectedBudget && (
        <div className="fixed inset-0 bg-black/50 flex items-start justify-center z-50 p-4 pt-8 overflow-y-auto">
          <Card className="w-full max-w-md">
            <CardHeader>
              <h3 className="font-semibold">Aprobar Presupuesto {selectedBudget.fiscalYear}</h3>
            </CardHeader>
            <CardContent>
              <form onSubmit={handleApproveBudget} className="space-y-4">
                <div>
                  <label className="block text-sm font-medium mb-1">N° Acta</label>
                  <input name="meetingActNumber" required className="w-full border border-border rounded-lg px-3 py-2 text-sm bg-card" />
                </div>
                <div>
                  <label className="block text-sm font-medium mb-1">Fecha Aprobación</label>
                  <input name="approvalDate" type="date" required className="w-full border border-border rounded-lg px-3 py-2 text-sm bg-card" />
                </div>
                <div className="flex gap-3 justify-end pt-2">
                  <Button variant="ghost" type="button" onClick={() => setShowApprove(false)}>Cancelar</Button>
                  <Button variant="success" type="submit">Aprobar</Button>
                </div>
              </form>
            </CardContent>
          </Card>
        </div>
      )}
    </div>
  );

  const renderExecution = () => (
    <div>
      <div className="flex items-center gap-3 mb-4">
        <h2 className="text-lg font-semibold">Ejecución Presupuestal</h2>
        <select
          value={year}
          onChange={e => setYear(Number(e.target.value))}
          className="text-sm border border-border rounded-lg px-3 py-1.5 bg-card"
        >
          {[2024, 2025, 2026, 2027, 2028].map(y => (
            <option key={y} value={y}>{y}</option>
          ))}
        </select>
      </div>

      {loading ? (
        <div className="text-center py-12 text-muted-foreground">Cargando...</div>
      ) : !dashboard ? (
        <Card>
          <CardContent className="text-center py-12 text-muted-foreground">
            No hay presupuesto aprobado para el año {year}
          </CardContent>
        </Card>
      ) : (
        <>
          <div className="grid grid-cols-1 md:grid-cols-4 gap-4 mb-6">
            <Card>
              <CardContent className="p-4">
                <p className="text-xs text-muted-foreground mb-1">Presupuesto Total (Gastos)</p>
                <p className="text-2xl font-bold">${dashboard.totalApprovedExpense.toLocaleString()}</p>
              </CardContent>
            </Card>
            <Card>
              <CardContent className="p-4">
                <p className="text-xs text-muted-foreground mb-1">Ejecutado</p>
                <p className="text-2xl font-bold text-blue-600">${dashboard.totalExecutedExpense.toLocaleString()}</p>
              </CardContent>
            </Card>
            <Card>
              <CardContent className="p-4">
                <p className="text-xs text-muted-foreground mb-1">Disponible</p>
                <p className="text-2xl font-bold text-green-600">${dashboard.totalAvailable.toLocaleString()}</p>
              </CardContent>
            </Card>
            <Card>
              <CardContent className="p-4">
                <p className="text-xs text-muted-foreground mb-1">% Ejecución</p>
                <p className={`text-2xl font-bold ${dashboard.overallExecutionPercentage > 80 ? 'text-red-600' : dashboard.overallExecutionPercentage > 50 ? 'text-yellow-600' : 'text-green-600'}`}>
                  {dashboard.overallExecutionPercentage.toFixed(1)}%
                </p>
              </CardContent>
            </Card>
          </div>

          <Card>
            <CardHeader>
              <h3 className="font-semibold">Detalle por Partida</h3>
            </CardHeader>
            <CardContent className="p-0">
              <div className="overflow-x-auto">
                <table className="w-full text-sm">
                  <thead>
                    <tr className="border-b border-border bg-muted/50">
                      <th className="text-left px-4 py-3">Partida</th>
                      <th className="text-center px-4 py-3">Categoría</th>
                      <th className="text-right px-4 py-3">Anual</th>
                      <th className="text-right px-4 py-3">Proporcional</th>
                      <th className="text-right px-4 py-3">Ejecutado</th>
                      <th className="text-right px-4 py-3">Disponible</th>
                      <th className="text-center px-4 py-3">% Ejec.</th>
                      <th className="text-center px-4 py-3">Estado</th>
                    </tr>
                  </thead>
                  <tbody>
                    {dashboard.expenseItems.map(item => (
                      <tr key={item.id} className="border-b border-border/50 hover:bg-muted/30">
                        <td className="px-4 py-3 font-medium">{item.name}</td>
                        <td className="px-4 py-3 text-center text-xs">{item.category}</td>
                        <td className="px-4 py-3 text-right">${item.annualValue.toLocaleString()}</td>
                        <td className="px-4 py-3 text-right">${item.proportionalToDate.toLocaleString()}</td>
                        <td className="px-4 py-3 text-right">${item.executedValue.toLocaleString()}</td>
                        <td className="px-4 py-3 text-right">${item.availableValue.toLocaleString()}</td>
                        <td className="px-4 py-3 text-right">{item.executionPercentage.toFixed(1)}%</td>
                        <td className="px-4 py-3 text-center">
                          <span className={`inline-flex items-center gap-1 px-2 py-0.5 rounded-full text-xs font-medium ${trafficColor(item.trafficLight)}`}>
                            {item.trafficLight === 'Green' && <TrendingDown className="w-3 h-3" />}
                            {item.trafficLight === 'Yellow' && <AlertTriangle className="w-3 h-3" />}
                            {item.trafficLight === 'Red' && <AlertTriangle className="w-3 h-3" />}
                            {item.trafficLight === 'Green' ? 'Normal' : item.trafficLight === 'Yellow' ? 'Alerta' : 'Crítico'}
                          </span>
                        </td>
                      </tr>
                    ))}
                  </tbody>
                </table>
              </div>
            </CardContent>
          </Card>

          {dashboard.alerts.length > 0 && (
            <Card className="mt-4 border-yellow-300 dark:border-yellow-700">
              <CardHeader>
                <h3 className="font-semibold flex items-center gap-2 text-yellow-700 dark:text-yellow-400">
                  <AlertTriangle className="w-5 h-5" /> Alertas
                </h3>
              </CardHeader>
              <CardContent>
                {dashboard.alerts.map((a, i) => (
                  <div key={i} className="flex items-start gap-3 py-2 border-b border-border/50 last:border-0">
                    <AlertTriangle className={`w-4 h-4 mt-0.5 ${a.severity === 'Critical' ? 'text-red-500' : 'text-yellow-500'}`} />
                    <div>
                      <p className="text-sm font-medium">{a.itemName}</p>
                      <p className="text-xs text-muted-foreground">{a.message}</p>
                    </div>
                  </div>
                ))}
              </CardContent>
            </Card>
          )}
        </>
      )}
    </div>
  );

  const renderExpenses = () => (
    <div>
      <div className="flex items-center justify-between mb-4">
        <h2 className="text-lg font-semibold">Gastos Ejecutados</h2>
        {canEdit && (
          <Button variant="primary" onClick={() => setShowExpense(true)}>
            <Plus className="w-4 h-4 mr-1" /> Registrar Gasto
          </Button>
        )}
      </div>

      <Card>
        <CardContent className="p-0">
          <div className="overflow-x-auto">
            <table className="w-full text-sm">
              <thead>
                <tr className="border-b border-border bg-muted/50">
                  <th className="text-left px-4 py-3">Fecha</th>
                  <th className="text-left px-4 py-3">Partida</th>
                  <th className="text-left px-4 py-3">Descripción</th>
                  <th className="text-right px-4 py-3">Monto</th>
                  <th className="text-left px-4 py-3">Factura</th>
                  <th className="text-left px-4 py-3">Proveedor</th>
                </tr>
              </thead>
              <tbody>
                {expenses.length === 0 ? (
                  <tr>
                    <td colSpan={6} className="text-center py-8 text-muted-foreground">No hay gastos registrados</td>
                  </tr>
                ) : expenses.map(e => (
                  <tr key={e.id} className="border-b border-border/50 hover:bg-muted/30">
                    <td className="px-4 py-3">{new Date(e.expenseDate).toLocaleDateString()}</td>
                    <td className="px-4 py-3 font-medium">{e.expenseItemName}</td>
                    <td className="px-4 py-3 text-muted-foreground">{e.description}</td>
                    <td className="px-4 py-3 text-right font-medium">${e.amount.toLocaleString()}</td>
                    <td className="px-4 py-3">{e.invoiceReference || '-'}</td>
                    <td className="px-4 py-3">{e.providerName || '-'}</td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        </CardContent>
      </Card>

      {showExpense && (
        <div className="fixed inset-0 bg-black/50 flex items-start justify-center z-50 p-4 pt-8 overflow-y-auto">
          <Card className="w-full max-w-lg">
            <CardHeader>
              <h3 className="font-semibold">Registrar Gasto</h3>
            </CardHeader>
            <CardContent>
              <form onSubmit={handleRecordExpense} className="space-y-4">
                <div>
                  <label className="block text-sm font-medium mb-1">Partida Presupuestal</label>
                  <select name="expenseItemId" required className="w-full border border-border rounded-lg px-3 py-2 text-sm bg-card">
                    <option value="">Seleccione una partida...</option>
                    {expenseItemOptions.map(opt => (
                      <option key={opt.id} value={opt.id}>{opt.budgetLabel} - {opt.name}</option>
                    ))}
                  </select>
                </div>
                <div>
                  <label className="block text-sm font-medium mb-1">Descripción</label>
                  <input name="description" required className="w-full border border-border rounded-lg px-3 py-2 text-sm bg-card" />
                </div>
                <div className="grid grid-cols-2 gap-4">
                  <div>
                    <label className="block text-sm font-medium mb-1">Monto</label>
                    <input name="amount" type="number" step="0.01" required className="w-full border border-border rounded-lg px-3 py-2 text-sm bg-card" />
                  </div>
                  <div>
                    <label className="block text-sm font-medium mb-1">Fecha</label>
                    <input name="expenseDate" type="date" required className="w-full border border-border rounded-lg px-3 py-2 text-sm bg-card" />
                  </div>
                </div>
                <div>
                  <label className="block text-sm font-medium mb-1">Referencia Factura</label>
                  <input name="invoiceReference" className="w-full border border-border rounded-lg px-3 py-2 text-sm bg-card" />
                </div>
                <div className="flex gap-3 justify-end pt-2">
                  <Button variant="ghost" type="button" onClick={() => setShowExpense(false)}>Cancelar</Button>
                  <Button variant="primary" type="submit">Registrar</Button>
                </div>
              </form>
            </CardContent>
          </Card>
        </div>
      )}
    </div>
  );

  const renderModifications = () => (
    <div>
      <div className="flex items-center justify-between mb-4">
        <h2 className="text-lg font-semibold">Modificaciones Presupuestales</h2>
        {canEdit && selectedBudget && (
          <Button variant="primary" onClick={() => setShowModification(true)}>
            <Plus className="w-4 h-4 mr-1" /> Nueva Modificación
          </Button>
        )}
      </div>

      <div className="mb-4">
        <label className="block text-sm font-medium mb-1">Seleccionar Presupuesto</label>
        <select
          onChange={e => { if (e.target.value) handleSelectBudget(e.target.value); }}
          className="border border-border rounded-lg px-3 py-2 text-sm bg-card max-w-sm"
        >
          <option value="">Seleccione...</option>
          {budgets.map(b => (
            <option key={b.id} value={b.id}>Presupuesto {b.fiscalYear} - {b.status}</option>
          ))}
        </select>
      </div>

      {selectedBudget && (
        <Card>
          <CardContent className="p-0">
            <div className="overflow-x-auto">
              <table className="w-full text-sm">
                <thead>
                  <tr className="border-b border-border bg-muted/50">
                    <th className="text-left px-4 py-3">Tipo</th>
                    <th className="text-left px-4 py-3">Partida</th>
                    <th className="text-right px-4 py-3">Monto</th>
                    <th className="text-right px-4 py-3">Valor Anterior</th>
                    <th className="text-right px-4 py-3">Nuevo Valor</th>
                    <th className="text-left px-4 py-3">Justificación</th>
                    <th className="text-left px-4 py-3">Aprobación</th>
                    <th className="text-left px-4 py-3">Acta</th>
                  </tr>
                </thead>
                <tbody>
                  {modifications.length === 0 ? (
                    <tr>
                      <td colSpan={8} className="text-center py-8 text-muted-foreground">Sin modificaciones</td>
                    </tr>
                  ) : modifications.map(m => (
                    <tr key={m.id} className="border-b border-border/50 hover:bg-muted/30">
                      <td className="px-4 py-3">
                        <span className={`text-xs font-medium px-2 py-0.5 rounded ${
                          m.modificationType === 'Addition' ? 'bg-green-100 text-green-700 dark:bg-green-900/30 dark:text-green-300' :
                          m.modificationType === 'Reduction' ? 'bg-red-100 text-red-700 dark:bg-red-900/30 dark:text-red-300' :
                          'bg-blue-100 text-blue-700 dark:bg-blue-900/30 dark:text-blue-300'
                        }`}>
                          {m.modificationType === 'Addition' ? 'Adición' : m.modificationType === 'Reduction' ? 'Reducción' : 'Traslado'}
                        </span>
                      </td>
                      <td className="px-4 py-3">{m.expenseItemName || m.incomeItemName}</td>
                      <td className="px-4 py-3 text-right">${m.amount.toLocaleString()}</td>
                      <td className="px-4 py-3 text-right">${m.previousValue.toLocaleString()}</td>
                      <td className="px-4 py-3 text-right">${m.newValue.toLocaleString()}</td>
                      <td className="px-4 py-3 text-muted-foreground max-w-xs truncate">{m.justification}</td>
                      <td className="px-4 py-3 text-xs">{m.approvalType}</td>
                      <td className="px-4 py-3 text-xs">{m.meetingActNumber}</td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>
          </CardContent>
        </Card>
      )}

      {showModification && (
        <div className="fixed inset-0 bg-black/50 flex items-start justify-center z-50 p-4 pt-8 overflow-y-auto">
          <Card className="w-full max-w-lg">
            <CardHeader>
              <h3 className="font-semibold">Nueva Modificación</h3>
            </CardHeader>
            <CardContent>
              <form onSubmit={handleCreateModification} className="space-y-4">
                <div className="grid grid-cols-2 gap-4">
                  <div>
                    <label className="block text-sm font-medium mb-1">Tipo</label>
                    <select name="modificationType" required className="w-full border border-border rounded-lg px-3 py-2 text-sm bg-card">
                      <option value="Addition">Adición</option>
                      <option value="Reduction">Reducción</option>
                      <option value="Transfer">Traslado</option>
                    </select>
                  </div>
                  <div>
                    <label className="block text-sm font-medium mb-1">Monto</label>
                    <input name="amount" type="number" step="0.01" required className="w-full border border-border rounded-lg px-3 py-2 text-sm bg-card" />
                  </div>
                </div>
                <div>
                  <label className="block text-sm font-medium mb-1">Partida de Gasto (opcional)</label>
                  <select name="expenseItemId" className="w-full border border-border rounded-lg px-3 py-2 text-sm bg-card">
                    <option value="">Ninguna</option>
                    {selectedBudget?.expenseItems.map(ei => (
                      <option key={ei.id} value={ei.id}>{ei.name}</option>
                    ))}
                  </select>
                </div>
                <div>
                  <label className="block text-sm font-medium mb-1">Partida de Ingreso (opcional)</label>
                  <select name="incomeItemId" className="w-full border border-border rounded-lg px-3 py-2 text-sm bg-card">
                    <option value="">Ninguna</option>
                    {selectedBudget?.incomeItems.map(ii => (
                      <option key={ii.id} value={ii.id}>{ii.name}</option>
                    ))}
                  </select>
                </div>
                <div>
                  <label className="block text-sm font-medium mb-1">Justificación</label>
                  <textarea name="justification" required rows={3} className="w-full border border-border rounded-lg px-3 py-2 text-sm bg-card" />
                </div>
                <div className="grid grid-cols-2 gap-4">
                  <div>
                    <label className="block text-sm font-medium mb-1">Tipo Aprobación</label>
                    <select name="approvalType" required className="w-full border border-border rounded-lg px-3 py-2 text-sm bg-card">
                      <option value="Council">Consejo</option>
                      <option value="Assembly">Asamblea</option>
                    </select>
                  </div>
                  <div>
                    <label className="block text-sm font-medium mb-1">N° Acta</label>
                    <input name="meetingActNumber" required className="w-full border border-border rounded-lg px-3 py-2 text-sm bg-card" />
                  </div>
                </div>
                <div>
                  <label className="block text-sm font-medium mb-1">Fecha Aprobación</label>
                  <input name="approvalDate" type="date" required className="w-full border border-border rounded-lg px-3 py-2 text-sm bg-card" />
                </div>
                <div className="flex gap-3 justify-end pt-2">
                  <Button variant="ghost" type="button" onClick={() => setShowModification(false)}>Cancelar</Button>
                  <Button variant="primary" type="submit">Crear</Button>
                </div>
              </form>
            </CardContent>
          </Card>
        </div>
      )}
    </div>
  );

  const renderContingency = () => (
    <div>
      <div className="flex items-center justify-between mb-4">
        <h2 className="text-lg font-semibold">Fondo de Imprevistos</h2>
        {canEdit && (
          <Button variant="primary" onClick={() => setShowContingencyUsage(true)}>
            <Plus className="w-4 h-4 mr-1" /> Registrar Uso
          </Button>
        )}
      </div>

      {contingency && (
        <>
          <div className="grid grid-cols-1 md:grid-cols-3 gap-4 mb-6">
            <Card>
              <CardContent className="p-4">
                <p className="text-xs text-muted-foreground mb-1">Total Aportado</p>
                <p className="text-2xl font-bold text-green-600">${contingency.totalContributed.toLocaleString()}</p>
              </CardContent>
            </Card>
            <Card>
              <CardContent className="p-4">
                <p className="text-xs text-muted-foreground mb-1">Total Usado</p>
                <p className="text-2xl font-bold text-red-600">${contingency.totalUsed.toLocaleString()}</p>
              </CardContent>
            </Card>
            <Card>
              <CardContent className="p-4">
                <p className="text-xs text-muted-foreground mb-1">Saldo Disponible</p>
                <p className="text-2xl font-bold text-blue-600">${contingency.availableBalance.toLocaleString()}</p>
              </CardContent>
            </Card>
          </div>

          <Card>
            <CardHeader>
              <h3 className="font-semibold">Historial de Usos</h3>
            </CardHeader>
            <CardContent className="p-0">
              <div className="overflow-x-auto">
                <table className="w-full text-sm">
                  <thead>
                    <tr className="border-b border-border bg-muted/50">
                      <th className="text-left px-4 py-3">Fecha</th>
                      <th className="text-left px-4 py-3">Justificación</th>
                      <th className="text-right px-4 py-3">Monto</th>
                      <th className="text-left px-4 py-3">Acta Aprobación</th>
                    </tr>
                  </thead>
                  <tbody>
                    {contingency.usages.length === 0 ? (
                      <tr>
                        <td colSpan={4} className="text-center py-8 text-muted-foreground">Sin usos registrados</td>
                      </tr>
                    ) : contingency.usages.map(u => (
                      <tr key={u.id} className="border-b border-border/50 hover:bg-muted/30">
                        <td className="px-4 py-3">{new Date(u.createdAt).toLocaleDateString()}</td>
                        <td className="px-4 py-3">{u.justification}</td>
                        <td className="px-4 py-3 text-right font-medium">${u.amount.toLocaleString()}</td>
                        <td className="px-4 py-3">{u.councilApprovalActNumber || '-'}</td>
                      </tr>
                    ))}
                  </tbody>
                </table>
              </div>
            </CardContent>
          </Card>
        </>
      )}

      {!contingency && (
        <Card>
          <CardContent className="text-center py-12 text-muted-foreground">
            No hay información del fondo de imprevistos
          </CardContent>
        </Card>
      )}

      {showContingencyUsage && (
        <div className="fixed inset-0 bg-black/50 flex items-start justify-center z-50 p-4 pt-8 overflow-y-auto">
          <Card className="w-full max-w-md">
            <CardHeader>
              <h3 className="font-semibold">Registrar Uso de Fondo de Imprevistos</h3>
            </CardHeader>
            <CardContent>
              <form onSubmit={handleContingencyUsage} className="space-y-4">
                <div>
                  <label className="block text-sm font-medium mb-1">Presupuesto</label>
                  <select name="budgetId" required className="w-full border border-border rounded-lg px-3 py-2 text-sm bg-card">
                    <option value="">Seleccione...</option>
                    {budgets.filter(b => b.status === 'Approved').map(b => (
                      <option key={b.id} value={b.id}>Presupuesto {b.fiscalYear}</option>
                    ))}
                  </select>
                </div>
                <div>
                  <label className="block text-sm font-medium mb-1">Justificación</label>
                  <textarea name="justification" required rows={3} className="w-full border border-border rounded-lg px-3 py-2 text-sm bg-card" />
                </div>
                <div>
                  <label className="block text-sm font-medium mb-1">Monto</label>
                  <input name="amount" type="number" step="0.01" required className="w-full border border-border rounded-lg px-3 py-2 text-sm bg-card" />
                </div>
                <div>
                  <label className="block text-sm font-medium mb-1">N° Acta Aprobación Consejo</label>
                  <input name="councilApprovalActNumber" required className="w-full border border-border rounded-lg px-3 py-2 text-sm bg-card" />
                </div>
                <div className="flex gap-3 justify-end pt-2">
                  <Button variant="ghost" type="button" onClick={() => setShowContingencyUsage(false)}>Cancelar</Button>
                  <Button variant="primary" type="submit">Registrar</Button>
                </div>
              </form>
            </CardContent>
          </Card>
        </div>
      )}
    </div>
  );

  return (
    <div>
      <div className="flex items-center gap-3 mb-6">
        <BadgeDollarSign className="w-6 h-6 text-primary" />
        <h1 className="text-2xl font-bold">Presupuesto y Ejecución</h1>
      </div>

      {error && (
        <div className="mb-4 p-3 bg-red-100 dark:bg-red-900/30 text-red-700 dark:text-red-300 rounded-lg text-sm flex items-center gap-2">
          <AlertTriangle className="w-4 h-4" /> {error}
          <button onClick={() => setError('')} className="ml-auto"><X className="w-4 h-4" /></button>
        </div>
      )}
      {success && (
        <div className="mb-4 p-3 bg-green-100 dark:bg-green-900/30 text-green-700 dark:text-green-300 rounded-lg text-sm flex items-center gap-2">
          <Check className="w-4 h-4" /> {success}
          <button onClick={() => setSuccess('')} className="ml-auto"><X className="w-4 h-4" /></button>
        </div>
      )}

      {renderTabs()}

      {tab === 'list' && renderBudgetList()}
      {tab === 'execution' && renderExecution()}
      {tab === 'expenses' && renderExpenses()}
      {tab === 'modifications' && renderModifications()}
      {tab === 'contingency' && renderContingency()}

      {showCreate && (
        <div className="fixed inset-0 bg-black/50 flex items-center justify-center z-50">
          <Card className="w-full max-w-sm mx-4">
            <CardHeader className="!p-4">
              <h3 className="font-semibold text-sm">Nuevo Presupuesto</h3>
            </CardHeader>
            <CardContent className="!p-4 pt-0">
              <form onSubmit={handleCreateBudget} className="space-y-2.5">
                <div className="grid grid-cols-2 gap-2.5">
                  <div>
                    <label className="block text-xs font-medium mb-0.5">Año Fiscal</label>
                    <input name="fiscalYear" type="number" defaultValue={year} required className="w-full border border-border rounded px-2 py-1.5 text-xs bg-card" />
                  </div>
                  <div>
                    <label className="block text-xs font-medium mb-0.5">N° Acta</label>
                    <input name="meetingActNumber" required className="w-full border border-border rounded px-2 py-1.5 text-xs bg-card" />
                  </div>
                </div>
                <div>
                  <label className="block text-xs font-medium mb-0.5">Fecha Aprobación</label>
                  <input name="approvalDate" type="date" className="w-full border border-border rounded px-2 py-1.5 text-xs bg-card" />
                </div>
                <div>
                  <label className="block text-xs font-medium mb-0.5">Observaciones</label>
                  <textarea name="observations" rows={2} className="w-full border border-border rounded px-2 py-1.5 text-xs bg-card" />
                </div>
                <div className="flex items-center gap-2">
                  <input name="copyFromPrevious" id="copyFromPrevious" type="checkbox" className="rounded border-border w-3.5 h-3.5" />
                  <label htmlFor="copyFromPrevious" className="text-xs">Copiar partidas del presupuesto anterior</label>
                </div>
                <div>
                  <label className="block text-xs font-medium mb-0.5">Ajuste Global (%)</label>
                  <input name="globalAdjustment" type="number" step="0.01" className="w-full border border-border rounded px-2 py-1.5 text-xs bg-card" />
                </div>
                <div className="flex gap-2 justify-end pt-1">
                  <Button variant="ghost" type="button" onClick={() => setShowCreate(false)} className="text-xs !px-3 !py-1.5">Cancelar</Button>
                  <Button variant="primary" type="submit" className="text-xs !px-3 !py-1.5">Crear</Button>
                </div>
              </form>
            </CardContent>
          </Card>
        </div>
      )}

      {showEditItems && selectedBudget && (
        <div className="fixed inset-0 bg-black/50 flex items-start justify-center z-50 p-4 pt-8 overflow-y-auto">
          <Card className="w-full max-w-2xl max-h-[90vh] overflow-y-auto">
            <CardHeader>
              <div className="flex items-center justify-between">
                <h3 className="font-semibold">Editar Partidas - Presupuesto {selectedBudget.fiscalYear}</h3>
                <button onClick={() => setShowEditItems(false)} className="p-1 hover:bg-accent rounded">
                  <X className="w-5 h-5" />
                </button>
              </div>
            </CardHeader>
            <CardContent>
              <div className="space-y-6">
                <div>
                  <div className="flex items-center justify-between mb-2">
                    <h4 className="font-medium text-sm flex items-center gap-2">
                      <TrendingUp className="w-4 h-4 text-green-600" /> Ingresos
                    </h4>
                    <Button variant="ghost" onClick={() => setEditIncomeItems([...editIncomeItems, { name: '', description: '', annualValue: 0 }])}>
                      <Plus className="w-4 h-4 mr-1" /> Agregar Ingreso
                    </Button>
                  </div>
                  {editIncomeItems.map((item, idx) => (
                    <div key={idx} className="flex items-center gap-2 mb-2 p-2 border border-border rounded-lg">
                      <input
                        value={item.name} onChange={e => { const items = [...editIncomeItems]; items[idx].name = e.target.value; setEditIncomeItems(items); }}
                        placeholder="Nombre" className="flex-1 border border-border rounded px-2 py-1 text-sm bg-card"
                      />
                      <input
                        value={item.description} onChange={e => { const items = [...editIncomeItems]; items[idx].description = e.target.value; setEditIncomeItems(items); }}
                        placeholder="Descripción" className="flex-1 border border-border rounded px-2 py-1 text-sm bg-card"
                      />
                      <input
                        value={item.annualValue || ''} type="number" step="0.01"
                        onChange={e => { const items = [...editIncomeItems]; items[idx].annualValue = Number(e.target.value); setEditIncomeItems(items); }}
                        placeholder="Valor Anual" className="w-32 border border-border rounded px-2 py-1 text-sm bg-card"
                      />
                      <button onClick={() => setEditIncomeItems(editIncomeItems.filter((_, i) => i !== idx))} className="p-1 text-red-500 hover:bg-red-50 rounded">
                        <Trash2 className="w-4 h-4" />
                      </button>
                    </div>
                  ))}
                  {editIncomeItems.length === 0 && (
                    <p className="text-sm text-muted-foreground py-2">No hay ingresos. Presione "Agregar Ingreso" para añadir uno.</p>
                  )}
                </div>

                <div>
                  <div className="flex items-center justify-between mb-2">
                    <h4 className="font-medium text-sm flex items-center gap-2">
                      <TrendingDown className="w-4 h-4 text-red-600" /> Gastos
                    </h4>
                    <Button variant="ghost" onClick={() => setEditExpenseItems([...editExpenseItems, { name: '', description: '', category: 'Variable', annualValue: 0, isContingencyFund: false, requiresCouncilApproval: false }])}>
                      <Plus className="w-4 h-4 mr-1" /> Agregar Gasto
                    </Button>
                  </div>
                  {editExpenseItems.map((item, idx) => (
                    <div key={idx} className="flex items-center gap-2 mb-2 p-2 border border-border rounded-lg">
                      <input
                        value={item.name} onChange={e => { const items = [...editExpenseItems]; items[idx].name = e.target.value; setEditExpenseItems(items); }}
                        placeholder="Nombre" className="flex-1 border border-border rounded px-2 py-1 text-sm bg-card"
                      />
                      <select
                        value={item.category} onChange={e => { const items = [...editExpenseItems]; items[idx].category = e.target.value; setEditExpenseItems(items); }}
                        className="w-28 border border-border rounded px-2 py-1 text-sm bg-card"
                      >
                        <option value="Fijo">Fijo</option>
                        <option value="Variable">Variable</option>
                      </select>
                      <input
                        value={item.annualValue || ''} type="number" step="0.01"
                        onChange={e => { const items = [...editExpenseItems]; items[idx].annualValue = Number(e.target.value); setEditExpenseItems(items); }}
                        placeholder="Valor Anual" className="w-32 border border-border rounded px-2 py-1 text-sm bg-card"
                      />
                      <label className="flex items-center gap-1 text-xs whitespace-nowrap">
                        <input
                          type="checkbox" checked={item.isContingencyFund}
                          onChange={e => { const items = [...editExpenseItems]; items[idx].isContingencyFund = e.target.checked; setEditExpenseItems(items); }}
                          className="rounded border-border"
                        />
                        Fondo
                      </label>
                      <button onClick={() => setEditExpenseItems(editExpenseItems.filter((_, i) => i !== idx))} className="p-1 text-red-500 hover:bg-red-50 rounded">
                        <Trash2 className="w-4 h-4" />
                      </button>
                    </div>
                  ))}
                  {editExpenseItems.length === 0 && (
                    <p className="text-sm text-muted-foreground py-2">No hay gastos. Presione "Agregar Gasto" para añadir uno.</p>
                  )}
                </div>

                <div className="flex items-center justify-between pt-2 border-t border-border">
                  <div className="text-sm">
                    <span className="text-muted-foreground">Total Ingresos: </span>
                    <span className="font-medium text-green-600">${editIncomeItems.reduce((s, i) => s + (i.annualValue || 0), 0).toLocaleString()}</span>
                    <span className="mx-2">|</span>
                    <span className="text-muted-foreground">Total Gastos: </span>
                    <span className="font-medium text-red-600">${editExpenseItems.reduce((s, e) => s + (e.annualValue || 0), 0).toLocaleString()}</span>
                  </div>
                  <div className="flex gap-3">
                    <Button variant="ghost" onClick={() => setShowEditItems(false)}>Cancelar</Button>
                    <Button variant="primary" onClick={handleSaveItems}>Guardar Partidas</Button>
                  </div>
                </div>
              </div>
            </CardContent>
          </Card>
        </div>
      )}
    </div>
  );
}
