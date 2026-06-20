'use client'

import React, { useState, useEffect } from 'react'
import { Card, CardContent, CardHeader } from '@/components/ui/Card'
import accountingService, { AccountingPeriod, TrialBalanceItem, GeneralLedgerEntry, IncomeStatementItem, BalanceSheetItem, AccountingAccount } from '@/lib/accounting-service'
import { Loader2, Download } from 'lucide-react'

const formatCurrency = (val: number) =>
  new Intl.NumberFormat('es-CO', { style: 'currency', currency: 'COP', minimumFractionDigits: 0 }).format(val)

type ReportTab = 'trial-balance' | 'ledger' | 'income-statement' | 'balance-sheet'

export default function AccountingReportsPage() {
  const [activeTab, setActiveTab] = useState<ReportTab>('trial-balance')
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState('')

  const [periods, setPeriods] = useState<AccountingPeriod[]>([])
  const [accounts, setAccounts] = useState<AccountingAccount[]>([])
  const [selectedPeriodId, setSelectedPeriodId] = useState('')
  const [selectedAccountId, setSelectedAccountId] = useState('')

  const [trialBalance, setTrialBalance] = useState<TrialBalanceItem[]>([])
  const [ledger, setLedger] = useState<GeneralLedgerEntry[]>([])
  const [income, setIncome] = useState<IncomeStatementItem[]>([])
  const [balance, setBalance] = useState<BalanceSheetItem[]>([])

  useEffect(() => {
    Promise.all([
      accountingService.getPeriods(),
      accountingService.getAccounts(),
    ]).then(([p, a]) => {
      setPeriods(p)
      setAccounts(a.filter(ac => !ac.isGroup))
      const open = p.find(pr => pr.status === 'Open')
      if (open) setSelectedPeriodId(open.id)
    }).catch(() => setError('Error al cargar datos iniciales.')).finally(() => setLoading(false))
  }, [])

  useEffect(() => { if (activeTab === 'trial-balance') fetchTrialBalance() }, [activeTab, selectedPeriodId])
  useEffect(() => { if (activeTab === 'ledger' && selectedAccountId) fetchLedger() }, [activeTab, selectedAccountId])
  useEffect(() => { if (activeTab === 'income-statement') fetchIncome() }, [activeTab, selectedPeriodId])
  useEffect(() => { if (activeTab === 'balance-sheet') fetchBalance() }, [activeTab, selectedPeriodId])

  const fetchTrialBalance = async () => {
    setLoading(true)
    try {
      const params: any = {}
      if (selectedPeriodId) params.periodId = selectedPeriodId
      const r = await accountingService.getTrialBalance(params); setTrialBalance(r)
    } catch { setError('Error al cargar balance de comprobación.') }
    finally { setLoading(false) }
  }

  const fetchLedger = async () => {
    if (!selectedAccountId) return
    setLoading(true)
    try {
      const r = await accountingService.getGeneralLedger(selectedAccountId); setLedger(r)
    } catch { setError('Error al cargar mayor contable.') }
    finally { setLoading(false) }
  }

  const fetchIncome = async () => {
    setLoading(true)
    try {
      const params: any = {}
      if (selectedPeriodId) params.periodId = selectedPeriodId
      const r = await accountingService.getIncomeStatement(params); setIncome(r)
    } catch { setError('Error al cargar estado de resultados.') }
    finally { setLoading(false) }
  }

  const fetchBalance = async () => {
    setLoading(true)
    try {
      const params: any = {}
      if (selectedPeriodId) params.periodId = selectedPeriodId
      const r = await accountingService.getBalanceSheet(params); setBalance(r)
    } catch { setError('Error al cargar balance general.') }
    finally { setLoading(false) }
  }

  const tabs: { key: ReportTab; label: string }[] = [
    { key: 'trial-balance', label: 'Balance Comprobación' },
    { key: 'ledger', label: 'Mayor Contable' },
    { key: 'income-statement', label: 'Estado Resultados' },
    { key: 'balance-sheet', label: 'Balance General' },
  ]

  const renderSummaryRow = (label: string, value: number, isTotal = false) => (
    <div className={`flex justify-between py-1 ${isTotal ? 'border-t border-border pt-2 mt-1' : ''}`}>
      <span className={`${isTotal ? 'font-bold text-foreground' : 'text-sm text-muted-foreground'}`}>{label}</span>
      <span className={`font-mono ${isTotal ? 'font-bold text-foreground' : 'text-sm text-foreground'}`}>{formatCurrency(value)}</span>
    </div>
  )

  return (
    <div className="space-y-6">
      <div>
        <h1 className="text-2xl font-bold text-foreground">Reportes Contables</h1>
        <p className="text-sm text-muted-foreground mt-1">Estados financieros y reportes del módulo de contabilidad</p>
      </div>

      {error && (
        <div className="bg-rose-50 dark:bg-rose-950/30 border border-rose-200 dark:border-rose-800 text-rose-700 dark:text-rose-300 px-4 py-3 rounded-lg text-sm">{error}</div>
      )}

      <div className="flex items-center gap-2 border-b border-border">
        {tabs.map(t => (
          <button key={t.key} onClick={() => setActiveTab(t.key)}
            className={`px-5 py-3 text-sm font-semibold border-b-2 transition-all whitespace-nowrap ${
              activeTab === t.key ? 'border-emerald-600 text-emerald-600' : 'border-transparent text-muted-foreground hover:text-foreground'
            }`}>
            {t.label}
          </button>
        ))}
      </div>

      {activeTab !== 'ledger' && (
        <div className="flex items-center gap-3">
          <label className="text-sm font-medium text-muted-foreground">Período:</label>
          <select value={selectedPeriodId} onChange={e => setSelectedPeriodId(e.target.value)} className="input-standard w-64 text-sm">
            <option value="">Todos los períodos</option>
            {periods.map(p => (
              <option key={p.id} value={p.id}>{p.periodLabel} ({p.status === 'Open' ? 'Abierto' : 'Cerrado'})</option>
            ))}
          </select>
        </div>
      )}

      {activeTab === 'ledger' && (
        <div className="flex items-center gap-3">
          <label className="text-sm font-medium text-muted-foreground">Cuenta:</label>
          <select value={selectedAccountId} onChange={e => setSelectedAccountId(e.target.value)} className="input-standard w-96 text-sm">
            <option value="">Seleccionar cuenta</option>
            {accounts.map(a => (
              <option key={a.id} value={a.id}>{a.code} - {a.name}</option>
            ))}
          </select>
        </div>
      )}

      {activeTab === 'trial-balance' && (
        <Card>
          <CardHeader><h2 className="font-bold text-foreground">Balance de Comprobación</h2></CardHeader>
          <CardContent className="p-0">
            <div className="overflow-x-auto">
              <table className="min-w-full divide-y divide-border">
                <thead className="bg-muted/50">
                  <tr>
                    <th className="px-4 py-3 text-left text-xs font-bold text-muted-foreground uppercase tracking-wider">Código</th>
                    <th className="px-4 py-3 text-left text-xs font-bold text-muted-foreground uppercase tracking-wider">Cuenta</th>
                    <th className="px-4 py-3 text-center text-xs font-bold text-muted-foreground uppercase tracking-wider">Naturaleza</th>
                    <th className="px-4 py-3 text-right text-xs font-bold text-muted-foreground uppercase tracking-wider">Débitos</th>
                    <th className="px-4 py-3 text-right text-xs font-bold text-muted-foreground uppercase tracking-wider">Créditos</th>
                    <th className="px-4 py-3 text-right text-xs font-bold text-muted-foreground uppercase tracking-wider">Saldo</th>
                  </tr>
                </thead>
                <tbody className="divide-y divide-border">
                  {loading ? (
                    <tr><td colSpan={6} className="px-4 py-12 text-center"><Loader2 className="w-6 h-6 animate-spin mx-auto text-emerald-600" /></td></tr>
                  ) : trialBalance.length === 0 ? (
                    <tr><td colSpan={6} className="px-4 py-12 text-center text-muted-foreground">No hay movimientos en el período seleccionado.</td></tr>
                  ) : trialBalance.map((item, i) => (
                    <tr key={i} className="hover:bg-muted/30 transition-colors">
                      <td className="px-4 py-3 font-mono text-sm text-foreground">{item.accountCode}</td>
                      <td className="px-4 py-3 text-sm text-foreground">{item.accountName}</td>
                      <td className="px-4 py-3 text-center text-sm text-muted-foreground">{item.nature === 'Debit' ? 'Deudora' : 'Acreedora'}</td>
                      <td className="px-4 py-3 text-right text-sm font-mono text-emerald-600">{formatCurrency(item.totalDebit)}</td>
                      <td className="px-4 py-3 text-right text-sm font-mono text-rose-600">{formatCurrency(item.totalCredit)}</td>
                      <td className="px-4 py-3 text-right text-sm font-mono font-semibold text-foreground">{formatCurrency(item.balance)}</td>
                    </tr>
                  ))}
                </tbody>
                {trialBalance.length > 0 && (
                  <tfoot className="bg-muted/30">
                    <tr>
                      <td colSpan={3} className="px-4 py-3 text-right font-bold text-foreground">Totales</td>
                      <td className="px-4 py-3 text-right font-bold font-mono text-emerald-600">
                        {formatCurrency(trialBalance.reduce((s, i) => s + i.totalDebit, 0))}
                      </td>
                      <td className="px-4 py-3 text-right font-bold font-mono text-rose-600">
                        {formatCurrency(trialBalance.reduce((s, i) => s + i.totalCredit, 0))}
                      </td>
                      <td className="px-4 py-3 text-right font-bold font-mono text-foreground">
                        {formatCurrency(trialBalance.reduce((s, i) => s + Math.abs(i.balance), 0))}
                      </td>
                    </tr>
                  </tfoot>
                )}
              </table>
            </div>
          </CardContent>
        </Card>
      )}

      {activeTab === 'ledger' && (
        <Card>
          <CardHeader><h2 className="font-bold text-foreground">Mayor Contable</h2></CardHeader>
          <CardContent className="p-0">
            {!selectedAccountId ? (
              <div className="p-12 text-center text-muted-foreground">Seleccione una cuenta contable para ver su mayor.</div>
            ) : (
              <div className="overflow-x-auto">
                <table className="min-w-full divide-y divide-border">
                  <thead className="bg-muted/50">
                    <tr>
                      <th className="px-4 py-3 text-left text-xs font-bold text-muted-foreground uppercase tracking-wider">Fecha</th>
                      <th className="px-4 py-3 text-left text-xs font-bold text-muted-foreground uppercase tracking-wider"># Asiento</th>
                      <th className="px-4 py-3 text-left text-xs font-bold text-muted-foreground uppercase tracking-wider">Descripción</th>
                      <th className="px-4 py-3 text-right text-xs font-bold text-muted-foreground uppercase tracking-wider">Débito</th>
                      <th className="px-4 py-3 text-right text-xs font-bold text-muted-foreground uppercase tracking-wider">Crédito</th>
                      <th className="px-4 py-3 text-right text-xs font-bold text-muted-foreground uppercase tracking-wider">Saldo</th>
                    </tr>
                  </thead>
                  <tbody className="divide-y divide-border">
                    {loading ? (
                      <tr><td colSpan={6} className="px-4 py-12 text-center"><Loader2 className="w-6 h-6 animate-spin mx-auto text-emerald-600" /></td></tr>
                    ) : ledger.length === 0 ? (
                      <tr><td colSpan={6} className="px-4 py-12 text-center text-muted-foreground">No hay movimientos para esta cuenta.</td></tr>
                    ) : ledger.map((item, i) => (
                      <tr key={i} className="hover:bg-muted/30 transition-colors">
                        <td className="px-4 py-3 text-sm text-muted-foreground">{new Date(item.date).toLocaleDateString('es-CO')}</td>
                        <td className="px-4 py-3 font-mono text-sm text-foreground">{item.entryNumber}</td>
                        <td className="px-4 py-3 text-sm text-foreground">{item.description}</td>
                        <td className="px-4 py-3 text-right text-sm font-mono text-emerald-600">{item.debit > 0 ? formatCurrency(item.debit) : '-'}</td>
                        <td className="px-4 py-3 text-right text-sm font-mono text-rose-600">{item.credit > 0 ? formatCurrency(item.credit) : '-'}</td>
                        <td className="px-4 py-3 text-right text-sm font-mono font-semibold text-foreground">{formatCurrency(item.runningBalance)}</td>
                      </tr>
                    ))}
                  </tbody>
                </table>
              </div>
            )}
          </CardContent>
        </Card>
      )}

      {activeTab === 'income-statement' && (
        <Card>
          <CardHeader><h2 className="font-bold text-foreground">Estado de Resultados</h2></CardHeader>
          <CardContent>
            {loading ? (
              <div className="py-12 text-center"><Loader2 className="w-6 h-6 animate-spin mx-auto text-emerald-600" /></div>
            ) : income.length === 0 ? (
              <div className="py-12 text-center text-muted-foreground">No hay ingresos registrados en el período seleccionado.</div>
            ) : (
              <div className="max-w-lg">
                <h3 className="text-sm font-bold text-muted-foreground uppercase tracking-wider mb-3">Ingresos</h3>
                {income.map((item, i) => (
                  <div key={i} className="flex justify-between py-1.5">
                    <span className="text-sm text-foreground">{item.accountCode} - {item.accountName}</span>
                    <span className="text-sm font-mono text-foreground">{formatCurrency(item.balance)}</span>
                  </div>
                ))}
                {renderSummaryRow('Total Ingresos', income.reduce((s, i) => s + i.balance, 0), true)}
              </div>
            )}
          </CardContent>
        </Card>
      )}

      {activeTab === 'balance-sheet' && (
        <Card>
          <CardHeader><h2 className="font-bold text-foreground">Balance General</h2></CardHeader>
          <CardContent>
            {loading ? (
              <div className="py-12 text-center"><Loader2 className="w-6 h-6 animate-spin mx-auto text-emerald-600" /></div>
            ) : balance.length === 0 ? (
              <div className="py-12 text-center text-muted-foreground">No hay movimientos en el período seleccionado.</div>
            ) : (
              <div className="max-w-lg">
                <h3 className="text-sm font-bold text-muted-foreground uppercase tracking-wider mb-3">Activos, Pasivos y Patrimonio</h3>
                {balance.map((item, i) => (
                  <div key={i} className="flex justify-between py-1.5">
                    <span className="text-sm text-foreground">{item.accountCode} - {item.accountName}</span>
                    <span className="text-sm font-mono text-foreground">{formatCurrency(item.balance)}</span>
                  </div>
                ))}
                {renderSummaryRow('Total', balance.reduce((s, i) => s + i.balance, 0), true)}
              </div>
            )}
          </CardContent>
        </Card>
      )}
    </div>
  )
}
