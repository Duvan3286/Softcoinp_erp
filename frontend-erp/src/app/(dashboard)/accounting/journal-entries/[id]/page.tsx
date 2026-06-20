'use client'

import React, { useState, useEffect } from 'react'
import { useParams, useRouter } from 'next/navigation'
import { Button } from '@/components/ui/Button'
import { Card, CardContent, CardHeader } from '@/components/ui/Card'
import accountingService, { JournalEntry } from '@/lib/accounting-service'
import { Loader2, ArrowLeft, CheckCircle, RotateCcw, X } from 'lucide-react'

const formatCurrency = (val: number) =>
  new Intl.NumberFormat('es-CO', { style: 'currency', currency: 'COP', minimumFractionDigits: 0 }).format(val)

const statusBadge = (s: string) => {
  const map: Record<string, string> = { Draft: 'badge-warning', Final: 'badge-success', Reversed: 'badge-danger' }
  const labels: Record<string, string> = { Draft: 'Borrador', Final: 'Contabilizado', Reversed: 'Revertido' }
  return <span className={`${map[s] || 'badge-neutral'} px-3 py-1 text-sm font-semibold`}>{labels[s] || s}</span>
}

export default function JournalEntryDetailPage() {
  const params = useParams()
  const router = useRouter()
  const rawId = params?.id
  const id = Array.isArray(rawId) ? rawId[0] : rawId ?? ''

  const [loading, setLoading] = useState(true)
  const [entry, setEntry] = useState<JournalEntry | null>(null)
  const [error, setError] = useState('')
  const [actionLoading, setActionLoading] = useState('')

  const [showReverseModal, setShowReverseModal] = useState(false)
  const [reverseReason, setReverseReason] = useState('')

  useEffect(() => { if (id) fetchEntry() }, [id])

  const fetchEntry = async () => {
    setLoading(true); setError('')
    try { const r = await accountingService.getEntry(id); setEntry(r) }
    catch { setError('Error al cargar el asiento contable.') }
    finally { setLoading(false) }
  }

  const handlePost = async () => {
    if (!confirm('¿Contabilizar este asiento? Ya no podrá modificarlo.')) return
    setActionLoading('post')
    try { await accountingService.postEntry(id); await fetchEntry() }
    catch (err: any) { setError(err?.response?.data || 'Error al contabilizar.') }
    finally { setActionLoading('') }
  }

  const handleReverse = async (e: React.FormEvent) => {
    e.preventDefault()
    if (!reverseReason.trim()) { setError('Debe indicar el motivo de la reversión.'); return }
    setActionLoading('reverse')
    try {
      await accountingService.reverseEntry(id, { reason: reverseReason.trim() })
      setShowReverseModal(false); await fetchEntry()
    } catch (err: any) { setError(err?.response?.data || 'Error al revertir.') }
    finally { setActionLoading('') }
  }

  if (loading) return (
    <div className="flex items-center justify-center min-h-[400px]"><Loader2 className="w-8 h-8 animate-spin text-emerald-600" /></div>
  )

  if (!entry) return (
    <div className="space-y-6">
      <button onClick={() => router.push('/accounting/journal-entries')} className="text-sm text-muted-foreground hover:text-foreground flex items-center gap-1.5">
        <ArrowLeft className="w-4 h-4" /> Volver
      </button>
      <Card><CardContent className="p-12 text-center text-muted-foreground">Asiento contable no encontrado.</CardContent></Card>
    </div>
  )

  return (
    <div className="space-y-6 max-w-5xl">
      <button onClick={() => router.push('/accounting/journal-entries')} className="text-sm text-muted-foreground hover:text-foreground flex items-center gap-1.5 transition-colors">
        <ArrowLeft className="w-4 h-4" /> Volver al libro diario
      </button>

      {error && (
        <div className="bg-rose-50 dark:bg-rose-950/30 border border-rose-200 dark:border-rose-800 text-rose-700 dark:text-rose-300 px-4 py-3 rounded-lg text-sm">{error}</div>
      )}

      <div className="flex items-center justify-between">
        <div className="flex items-center gap-4">
          <h1 className="text-2xl font-bold text-foreground">Asiento #{entry.entryNumber}</h1>
          {statusBadge(entry.status)}
        </div>
        <div className="flex gap-2">
          {entry.status === 'Draft' && (
            <Button variant="success" onClick={handlePost} disabled={actionLoading === 'post'}>
              {actionLoading === 'post' ? <Loader2 className="w-4 h-4 animate-spin mr-2" /> : <CheckCircle className="w-4 h-4 mr-2" />}
              Contabilizar
            </Button>
          )}
          {entry.status === 'Final' && (
            <Button variant="danger" onClick={() => setShowReverseModal(true)} disabled={actionLoading === 'reverse'}>
              <RotateCcw className="w-4 h-4 mr-2" /> Revertir
            </Button>
          )}
        </div>
      </div>

      <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-4">
        <Card><CardContent className="p-4">
          <p className="text-xs text-muted-foreground uppercase tracking-wider">Fecha</p>
          <p className="text-lg font-bold text-foreground">{new Date(entry.entryDate).toLocaleDateString('es-CO')}</p>
        </CardContent></Card>
        <Card><CardContent className="p-4">
          <p className="text-xs text-muted-foreground uppercase tracking-wider">Tipo</p>
          <p className="text-lg font-bold text-foreground">{entry.entryType === 'Manual' ? 'Manual' : 'Automático'}</p>
        </CardContent></Card>
        <Card><CardContent className="p-4">
          <p className="text-xs text-muted-foreground uppercase tracking-wider">Total Débitos</p>
          <p className="text-lg font-bold text-emerald-600">{formatCurrency(entry.totalDebit)}</p>
        </CardContent></Card>
        <Card><CardContent className="p-4">
          <p className="text-xs text-muted-foreground uppercase tracking-wider">Total Créditos</p>
          <p className="text-lg font-bold text-rose-600">{formatCurrency(entry.totalCredit)}</p>
        </CardContent></Card>
      </div>

      <Card>
        <CardHeader><h2 className="font-bold text-foreground">Detalle del Asiento</h2></CardHeader>
        <CardContent className="space-y-3">
          <div className="grid grid-cols-2 gap-4 text-sm">
            <div><span className="text-muted-foreground">Periodo:</span> <span className="text-foreground font-medium ml-2">{entry.periodLabel || '-'}</span></div>
            <div><span className="text-muted-foreground">Ref. Externa:</span> <span className="text-foreground font-medium ml-2">{entry.externalReference || '-'}</span></div>
            <div className="col-span-2"><span className="text-muted-foreground">Descripción:</span> <span className="text-foreground font-medium ml-2">{entry.description}</span></div>
            <div><span className="text-muted-foreground">Creado por:</span> <span className="text-foreground font-medium ml-2">{entry.createdByUserId}</span></div>
            <div><span className="text-muted-foreground">Fecha creación:</span> <span className="text-foreground font-medium ml-2">{new Date(entry.createdAt).toLocaleString('es-CO')}</span></div>
          </div>
        </CardContent>
      </Card>

      <Card>
        <CardHeader><h2 className="font-bold text-foreground">Líneas</h2></CardHeader>
        <CardContent className="p-0">
          <div className="overflow-x-auto">
            <table className="min-w-full divide-y divide-border">
              <thead className="bg-muted/50">
                <tr>
                  <th className="px-6 py-4 text-left text-xs font-bold text-muted-foreground uppercase tracking-wider">Cuenta</th>
                  <th className="px-6 py-4 text-left text-xs font-bold text-muted-foreground uppercase tracking-wider">Nombre</th>
                  <th className="px-6 py-4 text-left text-xs font-bold text-muted-foreground uppercase tracking-wider">Tercero</th>
                  <th className="px-6 py-4 text-right text-xs font-bold text-muted-foreground uppercase tracking-wider">Débito</th>
                  <th className="px-6 py-4 text-right text-xs font-bold text-muted-foreground uppercase tracking-wider">Crédito</th>
                </tr>
              </thead>
              <tbody className="divide-y divide-border">
                {entry.lines.map(line => (
                  <tr key={line.id} className="hover:bg-muted/30 transition-colors">
                    <td className="px-6 py-4 whitespace-nowrap font-mono text-sm text-foreground">{line.accountCode}</td>
                    <td className="px-6 py-4 text-sm text-foreground">{line.accountName}</td>
                    <td className="px-6 py-4 text-sm text-muted-foreground">{line.thirdPartyId || '-'}</td>
                    <td className="px-6 py-4 whitespace-nowrap text-right text-sm font-mono text-emerald-600">{line.debit > 0 ? formatCurrency(line.debit) : '-'}</td>
                    <td className="px-6 py-4 whitespace-nowrap text-right text-sm font-mono text-rose-600">{line.credit > 0 ? formatCurrency(line.credit) : '-'}</td>
                  </tr>
                ))}
              </tbody>
              <tfoot className="bg-muted/30">
                <tr>
                  <td colSpan={3} className="px-6 py-3 text-right font-bold text-foreground">Totales</td>
                  <td className="px-6 py-3 text-right font-bold font-mono text-emerald-600">{formatCurrency(entry.totalDebit)}</td>
                  <td className="px-6 py-3 text-right font-bold font-mono text-rose-600">{formatCurrency(entry.totalCredit)}</td>
                </tr>
              </tfoot>
            </table>
          </div>
        </CardContent>
      </Card>

      {showReverseModal && (
        <div className="fixed inset-0 bg-black/60 backdrop-blur-sm z-[150] flex items-center justify-center p-4">
          <div className="bg-card text-card-foreground w-full max-w-md rounded-xl border border-border shadow-lg animate-in zoom-in-95 duration-200">
            <div className="p-6 border-b border-border flex items-center justify-between">
              <h3 className="font-bold text-lg text-foreground">Revertir Asiento #{entry.entryNumber}</h3>
              <button onClick={() => setShowReverseModal(false)} className="text-muted-foreground hover:text-foreground"><X className="w-5 h-5" /></button>
            </div>
            <form onSubmit={handleReverse} className="p-6 space-y-5">
              <p className="text-sm text-muted-foreground">Se creará un asiento de reversión automático que invertirá débitos y créditos.</p>
              <div>
                <label className="block text-sm font-medium text-foreground mb-1.5">Motivo de la reversión *</label>
                <textarea value={reverseReason} onChange={e => setReverseReason(e.target.value)} rows={3} className="input-standard w-full" placeholder="Ej: Error en el registro, asiento duplicado..." required />
              </div>
              <div className="flex justify-end gap-3 pt-2">
                <Button variant="ghost" type="button" onClick={() => setShowReverseModal(false)}>Cancelar</Button>
                <Button variant="danger" type="submit" disabled={actionLoading === 'reverse'}>
                  {actionLoading === 'reverse' ? <Loader2 className="w-4 h-4 animate-spin mr-2" /> : <RotateCcw className="w-4 h-4 mr-2" />}
                  Revertir Asiento
                </Button>
              </div>
            </form>
          </div>
        </div>
      )}
    </div>
  )
}
