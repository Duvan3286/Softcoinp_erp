'use client'

import React, { useState, useEffect } from 'react'
import { useRouter } from 'next/navigation'
import { Button } from '@/components/ui/Button'
import { Card, CardContent, CardHeader } from '@/components/ui/Card'
import accountingService, { AccountingPeriod } from '@/lib/accounting-service'
import { Plus, Loader2, Eye, X } from 'lucide-react'

const months = ['Enero', 'Febrero', 'Marzo', 'Abril', 'Mayo', 'Junio', 'Julio', 'Agosto', 'Septiembre', 'Octubre', 'Noviembre', 'Diciembre']

export default function AccountingPeriodsPage() {
  const router = useRouter()
  const [loading, setLoading] = useState(true)
  const [periods, setPeriods] = useState<AccountingPeriod[]>([])
  const [error, setError] = useState('')
  const [showModal, setShowModal] = useState(false)
  const [submitting, setSubmitting] = useState(false)

  const [fiscalYear, setFiscalYear] = useState(new Date().getFullYear())
  const [month, setMonth] = useState(new Date().getMonth() + 1)

  useEffect(() => { fetchPeriods() }, [])

  const fetchPeriods = async () => {
    setLoading(true); setError('')
    try { const r = await accountingService.getPeriods(); setPeriods(r) }
    catch { setError('Error al cargar períodos contables.') }
    finally { setLoading(false) }
  }

  const periodLabel = `${fiscalYear}-${month.toString().padStart(2, '0')} - ${months[month - 1]}`

  const handleOpen = async (e: React.FormEvent) => {
    e.preventDefault(); setSubmitting(true)
    try {
      await accountingService.openPeriod({ fiscalYear, month, periodLabel })
      setShowModal(false); await fetchPeriods()
    } catch (err: any) {
      setError(err?.response?.data || 'Error al abrir período.')
    } finally { setSubmitting(false) }
  }

  const handleClose = async (id: string) => {
    if (!confirm('¿Cerrar este período contable? Ya no se podrán registrar asientos en él.')) return
    try {
      await accountingService.closePeriod(id); await fetchPeriods()
    } catch (err: any) {
      setError(err?.response?.data || 'Error al cerrar período.')
    }
  }

  const statusBadge = (status: string) => {
    const cls = status === 'Open' ? 'badge-success' : 'badge-neutral'
    const label = status === 'Open' ? 'Abierto' : 'Cerrado'
    return <span className={`${cls} px-2.5 py-0.5 text-xs font-semibold`}>{label}</span>
  }

  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-2xl font-bold text-foreground">Períodos Contables</h1>
          <p className="text-sm text-muted-foreground mt-1">Gestión de períodos contables mensuales</p>
        </div>
        <Button variant="primary" onClick={() => setShowModal(true)}>
          <Plus className="w-4 h-4 mr-2" /> Nuevo Período
        </Button>
      </div>

      {error && (
        <div className="bg-rose-50 dark:bg-rose-950/30 border border-rose-200 dark:border-rose-800 text-rose-700 dark:text-rose-300 px-4 py-3 rounded-lg text-sm">{error}</div>
      )}

      <Card>
        <CardContent className="p-0">
          <div className="overflow-x-auto">
            <table className="min-w-full divide-y divide-border">
              <thead className="bg-muted/50">
                <tr>
                  <th className="px-6 py-4 text-left text-xs font-bold text-muted-foreground uppercase tracking-wider">Período</th>
                  <th className="px-6 py-4 text-left text-xs font-bold text-muted-foreground uppercase tracking-wider">Estado</th>
                  <th className="px-6 py-4 text-left text-xs font-bold text-muted-foreground uppercase tracking-wider">Apertura</th>
                  <th className="px-6 py-4 text-left text-xs font-bold text-muted-foreground uppercase tracking-wider">Cierre</th>
                  <th className="px-6 py-4 text-right text-xs font-bold text-muted-foreground uppercase tracking-wider">Último Asiento</th>
                  <th className="px-6 py-4 text-right text-xs font-bold text-muted-foreground uppercase tracking-wider">Acción</th>
                </tr>
              </thead>
              <tbody className="divide-y divide-border">
                {loading ? (
                  <tr><td colSpan={6} className="px-6 py-12 text-center"><Loader2 className="w-6 h-6 animate-spin mx-auto text-emerald-600" /></td></tr>
                ) : periods.length === 0 ? (
                  <tr><td colSpan={6} className="px-6 py-12 text-center text-muted-foreground">No hay períodos contables registrados.</td></tr>
                ) : periods.map(p => (
                  <tr key={p.id} className="hover:bg-muted/30 transition-colors">
                    <td className="px-6 py-4 whitespace-nowrap font-semibold text-foreground">{p.periodLabel}</td>
                    <td className="px-6 py-4 whitespace-nowrap">{statusBadge(p.status)}</td>
                    <td className="px-6 py-4 whitespace-nowrap text-sm text-muted-foreground">{new Date(p.openedAt).toLocaleDateString('es-CO')}</td>
                    <td className="px-6 py-4 whitespace-nowrap text-sm text-muted-foreground">{p.closedAt ? new Date(p.closedAt).toLocaleDateString('es-CO') : '-'}</td>
                    <td className="px-6 py-4 whitespace-nowrap text-right text-sm text-muted-foreground">{p.lastEntryNumber}</td>
                    <td className="px-6 py-4 whitespace-nowrap text-right">
                      {p.status === 'Open' ? (
                        <button onClick={() => handleClose(p.id)} className="text-amber-600 hover:text-amber-800 text-sm font-semibold px-3 py-1.5 bg-amber-50 rounded-lg hover:bg-amber-100 transition-colors">
                          Cerrar
                        </button>
                      ) : (
                        <span className="text-xs text-muted-foreground">-</span>
                      )}
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
          <div className="bg-card text-card-foreground w-full max-w-md rounded-xl border border-border shadow-lg animate-in zoom-in-95 duration-200">
            <div className="p-6 border-b border-border flex items-center justify-between">
              <h3 className="font-bold text-lg text-foreground">Abrir Nuevo Período</h3>
              <button onClick={() => setShowModal(false)} className="text-muted-foreground hover:text-foreground"><X className="w-5 h-5" /></button>
            </div>
            <form onSubmit={handleOpen} className="p-6 space-y-5">
              <div>
                <label className="block text-sm font-medium text-foreground mb-1.5">Año</label>
                <input type="number" value={fiscalYear} onChange={e => setFiscalYear(Number(e.target.value))} min={2000} max={2100}
                  className="input-standard w-full" required />
              </div>
              <div>
                <label className="block text-sm font-medium text-foreground mb-1.5">Mes</label>
                <select value={month} onChange={e => setMonth(Number(e.target.value))} className="input-standard w-full" required>
                  {months.map((m, i) => <option key={i + 1} value={i + 1}>{m}</option>)}
                </select>
              </div>
              <div className="p-3 bg-muted/50 rounded-lg text-sm text-muted-foreground">
                Se creará: <strong className="text-foreground">{periodLabel}</strong>
              </div>
              <div className="flex justify-end gap-3 pt-2">
                <Button variant="ghost" type="button" onClick={() => setShowModal(false)}>Cancelar</Button>
                <Button variant="primary" type="submit" disabled={submitting}>
                  {submitting ? <Loader2 className="w-4 h-4 animate-spin mr-2" /> : null}
                  Abrir Período
                </Button>
              </div>
            </form>
          </div>
        </div>
      )}
    </div>
  )
}
