'use client'

import React, { useState, useEffect } from 'react'
import { useRouter } from 'next/navigation'
import { Button } from '@/components/ui/Button'
import { Card, CardContent, CardHeader } from '@/components/ui/Card'
import accountingService, { JournalEntry } from '@/lib/accounting-service'
import { Plus, Loader2, Eye, X, Trash2 } from 'lucide-react'

const statusBadge = (s: string) => {
  const map: Record<string, string> = { Draft: 'badge-warning', Final: 'badge-success', Reversed: 'badge-danger' }
  const labels: Record<string, string> = { Draft: 'Borrador', Final: 'Contabilizado', Reversed: 'Revertido' }
  return <span className={`${map[s] || 'badge-neutral'} px-2.5 py-0.5 text-xs font-semibold`}>{labels[s] || s}</span>
}

const entryTypeLabel = (t: string) => t === 'Manual' ? 'Manual' : 'Automático'

const formatCurrency = (val: number) =>
  new Intl.NumberFormat('es-CO', { style: 'currency', currency: 'COP', minimumFractionDigits: 0 }).format(val)

export default function JournalEntriesPage() {
  const router = useRouter()
  const [loading, setLoading] = useState(true)
  const [entries, setEntries] = useState<JournalEntry[]>([])
  const [error, setError] = useState('')
  const [statusFilter, setStatusFilter] = useState('')
  const [typeFilter, setTypeFilter] = useState('')

  useEffect(() => { fetchEntries() }, [statusFilter, typeFilter])

  const fetchEntries = async () => {
    setLoading(true); setError('')
    try {
      const params: any = {}
      if (statusFilter) params.status = statusFilter
      if (typeFilter) params.entryType = typeFilter
      const r = await accountingService.getEntries(params); setEntries(r)
    } catch { setError('Error al cargar asientos contables.') }
    finally { setLoading(false) }
  }

  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-2xl font-bold text-foreground">Libro Diario</h1>
          <p className="text-sm text-muted-foreground mt-1">Asientos contables del plan de cuentas</p>
        </div>
        <Button variant="primary" onClick={() => router.push('/accounting/journal-entries/new')}>
          <Plus className="w-4 h-4 mr-2" /> Nuevo Asiento
        </Button>
      </div>

      {error && (
        <div className="bg-rose-50 dark:bg-rose-950/30 border border-rose-200 dark:border-rose-800 text-rose-700 dark:text-rose-300 px-4 py-3 rounded-lg text-sm">{error}</div>
      )}

      <Card>
        <CardHeader className="py-3 px-6">
          <div className="flex items-center gap-3">
            <select value={statusFilter} onChange={e => setStatusFilter(e.target.value)} className="input-standard w-40 text-sm">
              <option value="">Todos los estados</option>
              <option value="Draft">Borrador</option>
              <option value="Final">Contabilizado</option>
              <option value="Reversed">Revertido</option>
            </select>
            <select value={typeFilter} onChange={e => setTypeFilter(e.target.value)} className="input-standard w-40 text-sm">
              <option value="">Todos los tipos</option>
              <option value="Manual">Manual</option>
              <option value="Automatic">Automático</option>
            </select>
          </div>
        </CardHeader>
        <CardContent className="p-0">
          <div className="overflow-x-auto">
            <table className="min-w-full divide-y divide-border">
              <thead className="bg-muted/50">
                <tr>
                  <th className="px-6 py-4 text-left text-xs font-bold text-muted-foreground uppercase tracking-wider">#</th>
                  <th className="px-6 py-4 text-left text-xs font-bold text-muted-foreground uppercase tracking-wider">Fecha</th>
                  <th className="px-6 py-4 text-left text-xs font-bold text-muted-foreground uppercase tracking-wider">Descripción</th>
                  <th className="px-6 py-4 text-left text-xs font-bold text-muted-foreground uppercase tracking-wider">Tipo</th>
                  <th className="px-6 py-4 text-left text-xs font-bold text-muted-foreground uppercase tracking-wider">Estado</th>
                  <th className="px-6 py-4 text-right text-xs font-bold text-muted-foreground uppercase tracking-wider">Débitos</th>
                  <th className="px-6 py-4 text-right text-xs font-bold text-muted-foreground uppercase tracking-wider">Créditos</th>
                  <th className="px-6 py-4 text-right text-xs font-bold text-muted-foreground uppercase tracking-wider">Acción</th>
                </tr>
              </thead>
              <tbody className="divide-y divide-border">
                {loading ? (
                  <tr><td colSpan={8} className="px-6 py-12 text-center"><Loader2 className="w-6 h-6 animate-spin mx-auto text-emerald-600" /></td></tr>
                ) : entries.length === 0 ? (
                  <tr><td colSpan={8} className="px-6 py-12 text-center text-muted-foreground">No hay asientos contables registrados.</td></tr>
                ) : entries.map(e => (
                  <tr key={e.id} className="hover:bg-muted/30 transition-colors cursor-pointer" onClick={() => router.push(`/accounting/journal-entries/${e.id}`)}>
                    <td className="px-6 py-4 whitespace-nowrap font-mono text-sm text-foreground">{e.entryNumber}</td>
                    <td className="px-6 py-4 whitespace-nowrap text-sm text-muted-foreground">{new Date(e.entryDate).toLocaleDateString('es-CO')}</td>
                    <td className="px-6 py-4 max-w-xs truncate text-sm text-foreground">{e.description}</td>
                    <td className="px-6 py-4 whitespace-nowrap text-sm text-muted-foreground">{entryTypeLabel(e.entryType)}</td>
                    <td className="px-6 py-4 whitespace-nowrap">{statusBadge(e.status)}</td>
                    <td className="px-6 py-4 whitespace-nowrap text-right text-sm font-mono text-foreground">{formatCurrency(e.totalDebit)}</td>
                    <td className="px-6 py-4 whitespace-nowrap text-right text-sm font-mono text-foreground">{formatCurrency(e.totalCredit)}</td>
                    <td className="px-6 py-4 whitespace-nowrap text-right">
                      <button onClick={(ev) => { ev.stopPropagation(); router.push(`/accounting/journal-entries/${e.id}`) }}
                        className="text-emerald-600 hover:text-emerald-800 text-sm font-semibold px-3 py-1.5 bg-emerald-50 rounded-lg hover:bg-emerald-100 transition-colors">
                        <Eye className="w-4 h-4 inline mr-1" /> Ver
                      </button>
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        </CardContent>
      </Card>
    </div>
  )
}
