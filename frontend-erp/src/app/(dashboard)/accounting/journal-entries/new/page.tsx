'use client'

import React, { useState, useEffect } from 'react'
import { useRouter } from 'next/navigation'
import { Button } from '@/components/ui/Button'
import { Card, CardContent, CardHeader, CardFooter } from '@/components/ui/Card'
import accountingService, { AccountingAccount, CreateJournalEntryLine } from '@/lib/accounting-service'
import { Loader2, ArrowLeft, Plus, Trash2 } from 'lucide-react'

export default function NewJournalEntryPage() {
  const router = useRouter()
  const [accounts, setAccounts] = useState<AccountingAccount[]>([])
  const [loading, setLoading] = useState(true)
  const [submitting, setSubmitting] = useState(false)
  const [error, setError] = useState('')

  const [entryDate, setEntryDate] = useState(new Date().toISOString().split('T')[0])
  const [description, setDescription] = useState('')
  const [externalReference, setExternalReference] = useState('')
  const [lines, setLines] = useState<CreateJournalEntryLine[]>([
    { accountingAccountId: '', thirdPartyId: '', debit: 0, credit: 0 },
    { accountingAccountId: '', thirdPartyId: '', debit: 0, credit: 0 },
  ])

  useEffect(() => {
    accountingService.getAccounts()
      .then(setAccounts)
      .catch(() => setError('Error al cargar cuentas contables.'))
      .finally(() => setLoading(false))
  }, [])

  const addLine = () => setLines([...lines, { accountingAccountId: '', thirdPartyId: '', debit: 0, credit: 0 }])

  const removeLine = (i: number) => { if (lines.length > 2) setLines(lines.filter((_, idx) => idx !== i)) }

  const updateLine = (i: number, field: keyof CreateJournalEntryLine, value: any) => {
    const updated = [...lines]
    ;(updated[i] as any)[field] = field === 'debit' || field === 'credit' ? Number(value) : value
    if (field === 'debit' && Number(value) > 0) updated[i].credit = 0
    if (field === 'credit' && Number(value) > 0) updated[i].debit = 0
    setLines(updated)
  }

  const totalDebit = lines.reduce((s, l) => s + l.debit, 0)
  const totalCredit = lines.reduce((s, l) => s + l.credit, 0)
  const balanced = totalDebit === totalCredit && totalDebit > 0

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault(); setError('')
    if (!description.trim()) { setError('La descripción es requerida.'); return }
    if (!balanced) { setError(`La suma de débitos (${totalDebit}) debe ser igual a la de créditos (${totalCredit}).`); return }
    const invalidLines = lines.some(l => !l.accountingAccountId || (l.debit === 0 && l.credit === 0))
    if (invalidLines) { setError('Complete todas las líneas del asiento.'); return }

    setSubmitting(true)
    try {
      const result = await accountingService.createEntry({
        entryDate,
        description: description.trim(),
        externalReference: externalReference.trim() || undefined,
        entryType: 'Manual',
        lines: lines.map(l => ({ ...l, thirdPartyId: l.thirdPartyId?.trim() || undefined })),
      })
      router.push(`/accounting/journal-entries/${result.id}`)
    } catch (err: any) {
      setError(err?.response?.data || 'Error al crear el asiento contable.')
    } finally { setSubmitting(false) }
  }

  const formatCurrency = (val: number) =>
    new Intl.NumberFormat('es-CO', { style: 'currency', currency: 'COP', minimumFractionDigits: 0 }).format(val)

  if (loading) return (
    <div className="flex items-center justify-center min-h-[400px]">
      <Loader2 className="w-8 h-8 animate-spin text-emerald-600" />
    </div>
  )

  return (
    <div className="space-y-6 max-w-4xl">
      <button onClick={() => router.back()} className="text-sm text-muted-foreground hover:text-foreground flex items-center gap-1.5 transition-colors">
        <ArrowLeft className="w-4 h-4" /> Volver
      </button>

      <div>
        <h1 className="text-2xl font-bold text-foreground">Nuevo Asiento Contable</h1>
        <p className="text-sm text-muted-foreground mt-1">Registre un asiento de diario con partida doble</p>
      </div>

      {error && (
        <div className="bg-rose-50 dark:bg-rose-950/30 border border-rose-200 dark:border-rose-800 text-rose-700 dark:text-rose-300 px-4 py-3 rounded-lg text-sm">{error}</div>
      )}

      <form onSubmit={handleSubmit}>
        <Card className="mb-6">
          <CardHeader><h2 className="font-bold text-foreground">Encabezado</h2></CardHeader>
          <CardContent className="space-y-5">
            <div className="grid grid-cols-1 md:grid-cols-3 gap-6">
              <div>
                <label className="block text-sm font-medium text-foreground mb-1.5">Fecha *</label>
                <input type="date" value={entryDate} onChange={e => setEntryDate(e.target.value)} className="input-standard w-full" required />
              </div>
              <div>
                <label className="block text-sm font-medium text-foreground mb-1.5">Ref. Externa</label>
                <input type="text" value={externalReference} onChange={e => setExternalReference(e.target.value)} className="input-standard w-full" placeholder="Opcional" />
              </div>
              <div className="md:col-span-1" />
            </div>
            <div>
              <label className="block text-sm font-medium text-foreground mb-1.5">Descripción *</label>
              <textarea value={description} onChange={e => setDescription(e.target.value)} rows={2} className="input-standard w-full" placeholder="Ej: Pago de servicios públicos enero 2026" required />
            </div>
          </CardContent>
        </Card>

        <Card className="mb-6">
          <CardHeader className="flex flex-row items-center justify-between">
            <h2 className="font-bold text-foreground">Líneas del Asiento</h2>
            <Button variant="secondary" type="button" onClick={addLine}><Plus className="w-4 h-4 mr-1" /> Agregar Línea</Button>
          </CardHeader>
          <CardContent className="p-0">
            <div className="overflow-x-auto">
              <table className="min-w-full divide-y divide-border">
                <thead className="bg-muted/50">
                  <tr>
                    <th className="px-4 py-3 text-left text-xs font-bold text-muted-foreground uppercase tracking-wider">Cuenta</th>
                    <th className="px-4 py-3 text-left text-xs font-bold text-muted-foreground uppercase tracking-wider">Tercero</th>
                    <th className="px-4 py-3 text-right text-xs font-bold text-muted-foreground uppercase tracking-wider">Débito</th>
                    <th className="px-4 py-3 text-right text-xs font-bold text-muted-foreground uppercase tracking-wider">Crédito</th>
                    <th className="px-4 py-3 text-center text-xs font-bold text-muted-foreground uppercase tracking-wider"></th>
                  </tr>
                </thead>
                <tbody className="divide-y divide-border">
                  {lines.map((line, i) => (
                    <tr key={i} className="hover:bg-muted/30 transition-colors">
                      <td className="px-4 py-2">
                        <select value={line.accountingAccountId} onChange={e => updateLine(i, 'accountingAccountId', e.target.value)}
                          className="input-standard w-64 text-sm" required>
                          <option value="">Seleccionar cuenta</option>
                          {accounts.filter(a => !a.isGroup).map(a => (
                            <option key={a.id} value={a.id}>{a.code} - {a.name}</option>
                          ))}
                        </select>
                      </td>
                      <td className="px-4 py-2">
                        <input type="text" value={line.thirdPartyId || ''} onChange={e => updateLine(i, 'thirdPartyId', e.target.value)}
                          className="input-standard w-32 text-sm" placeholder="Nit" />
                      </td>
                      <td className="px-4 py-2">
                        <input type="number" value={line.debit || ''} onChange={e => updateLine(i, 'debit', e.target.value)}
                          className="input-standard w-28 text-sm text-right" min={0} step="0.01" />
                      </td>
                      <td className="px-4 py-2">
                        <input type="number" value={line.credit || ''} onChange={e => updateLine(i, 'credit', e.target.value)}
                          className="input-standard w-28 text-sm text-right" min={0} step="0.01" />
                      </td>
                      <td className="px-4 py-2 text-center">
                        {lines.length > 2 && (
                          <button type="button" onClick={() => removeLine(i)} className="text-rose-500 hover:text-rose-700 p-1">
                            <Trash2 className="w-4 h-4" />
                          </button>
                        )}
                      </td>
                    </tr>
                  ))}
                </tbody>
                <tfoot className="bg-muted/30">
                  <tr>
                    <td colSpan={2} className="px-4 py-3 text-right font-bold text-foreground">Totales</td>
                    <td className="px-4 py-3 text-right font-bold font-mono text-foreground">{formatCurrency(totalDebit)}</td>
                    <td className="px-4 py-3 text-right font-bold font-mono text-foreground">{formatCurrency(totalCredit)}</td>
                    <td></td>
                  </tr>
                  <tr>
                    <td colSpan={5} className="px-4 py-2 text-center">
                      {totalDebit > 0 || totalCredit > 0 ? (
                        balanced
                          ? <span className="text-xs text-emerald-600 font-semibold">✓ Partida doble balanceada</span>
                          : <span className="text-xs text-rose-600 font-semibold">✗ Diferencia: {formatCurrency(Math.abs(totalDebit - totalCredit))}</span>
                      ) : (
                        <span className="text-xs text-muted-foreground">Ingrese valores de débito y crédito</span>
                      )}
                    </td>
                  </tr>
                </tfoot>
              </table>
            </div>
          </CardContent>
        </Card>

        <div className="flex justify-end gap-3">
          <Button variant="ghost" type="button" onClick={() => router.back()}>Cancelar</Button>
          <Button variant="primary" type="submit" disabled={submitting || !balanced}>
            {submitting ? <Loader2 className="w-4 h-4 animate-spin mr-2" /> : null}
            Crear Asiento
          </Button>
        </div>
      </form>
    </div>
  )
}
