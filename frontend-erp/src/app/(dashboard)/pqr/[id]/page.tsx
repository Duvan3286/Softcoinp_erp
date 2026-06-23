'use client';

import React, { useState, useEffect } from 'react';
import { useRouter, useParams } from 'next/navigation';
import { Loader2, ArrowLeft, Clock, User, AlertTriangle, MessageSquare, FileText, Send, CheckCircle2, XCircle, RefreshCw, Plus } from 'lucide-react';
import { Button } from '@/components/ui/Button';
import { Card, CardHeader, CardContent } from '@/components/ui/Card';
import pqrService, { PqrDetail } from '@/lib/pqr-service';

type DetailTab = 'info' | 'followup' | 'responses' | 'notes' | 'files' | 'alerts';

const statusLabels: Record<string, string> = {
  Filed: 'Radicada', UnderReview: 'En Revisión', InManagement: 'En Trámite',
  Responded: 'Respondida', Closed: 'Cerrada', Reopened: 'Reabierta', Escalated: 'Escalada',
};
const typeLabels: Record<string, string> = { Request: 'Petición', Complaint: 'Queja', Claim: 'Reclamo' };
const priorityLabels: Record<string, string> = { Low: 'Baja', Normal: 'Normal', High: 'Alta', Urgent: 'Urgente' };
const channelLabels: Record<string, string> = { InPerson: 'Presencial', Email: 'Correo', Phone: 'Teléfono', Web: 'Portal', WhatsApp: 'WhatsApp', Letter: 'Carta', Other: 'Otro' };
const categoryLabels: Record<string, string> = { Billing: 'Facturación', Maintenance: 'Mantenimiento', Coexistence: 'Convivencia', CommonAreas: 'Zonas Comunes', Administration: 'Administración', Other: 'Otro' };
const documentTypeLabels: Record<string, string> = { CitizenshipCard: 'Cédula de Ciudadanía', ForeignerID: 'Cédula de Extranjería', NIT: 'NIT', Passport: 'Pasaporte', PEP: 'PEP', PPT: 'PPT' };
const alertTypeLabels: Record<string, string> = { FiftyPercent: 'Alerta 50%', EightyPercent: 'Alerta 80%', Overdue: 'Vencida' };

const availableStatuses = [
  { value: 'UnderReview', label: 'En Revisión' },
  { value: 'InManagement', label: 'En Trámite' },
  { value: 'Responded', label: 'Respondida' },
  { value: 'Closed', label: 'Cerrar Definitivamente' },
  { value: 'Escalated', label: 'Escalar' },
];

export default function PqrDetailPage() {
  const router = useRouter();
  const params = useParams();
  const id = params.id as string;

  const [detail, setDetail] = useState<PqrDetail | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');
  const [activeTab, setActiveTab] = useState<DetailTab>('info');

  const [showStatusModal, setShowStatusModal] = useState(false);
  const [newStatus, setNewStatus] = useState('UnderReview');
  const [statusJus, setStatusJus] = useState('');

  const [showResponseModal, setShowResponseModal] = useState(false);
  const [responseText, setResponseText] = useState('');
  const [isDefinitive, setIsDefinitive] = useState(false);
  const [isPartialUpdate, setIsPartialUpdate] = useState(false);
  const [requiresConfirmation, setRequiresConfirmation] = useState(false);

  const [showNoteModal, setShowNoteModal] = useState(false);
  const [noteText, setNoteText] = useState('');

  const [submitting, setSubmitting] = useState(false);

  const fetchDetail = async () => {
    setLoading(true);
    setError('');
    try {
      const data = await pqrService.getPqrDetail(id);
      setDetail(data);
    } catch {
      setError('Error al cargar el detalle de la PQR.');
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => { fetchDetail(); }, [id]);

  const formatDate = (d: string) => d ? new Date(d).toLocaleDateString('es-CO', { day: '2-digit', month: '2-digit', year: 'numeric', hour: '2-digit', minute: '2-digit' }) : '-';

  const statusBadge = (s: string) => {
    const m: Record<string, string> = { Filed: 'badge-info', UnderReview: 'badge-warning', InManagement: 'badge-warning', Responded: 'badge-success', Closed: 'badge-neutral', Reopened: 'badge-warning', Escalated: 'badge-danger' };
    return <span className={m[s] || 'badge-neutral'}>{statusLabels[s] || s}</span>;
  };

  const priorityBadge = (p: string) => {
    const m: Record<string, string> = { Low: 'badge-neutral', Normal: 'badge-info', High: 'badge-warning', Urgent: 'badge-danger' };
    return <span className={m[p] || 'badge-neutral'}>{priorityLabels[p] || p}</span>;
  };

  const semaphoreColor = (ep: number) => {
    if (ep >= 100) return 'bg-rose-500';
    if (ep >= 80) return 'bg-orange-400';
    if (ep >= 50) return 'bg-amber-400';
    return 'bg-emerald-500';
  };

  const handleStatusChange = async () => {
    setError('');
    if (!statusJus.trim()) { setError('La justificación es requerida.'); return; }
    setSubmitting(true);
    try {
      await pqrService.changeStatus(id, { status: newStatus, justification: statusJus });
      setShowStatusModal(false);
      setStatusJus('');
      await fetchDetail();
    } catch (err: any) {
      setError(err?.response?.data || 'Error al cambiar el estado.');
    } finally {
      setSubmitting(false);
    }
  };

  const handleAddResponse = async () => {
    setError('');
    if (!responseText.trim()) { setError('El texto de la respuesta es requerido.'); return; }
    setSubmitting(true);
    try {
      await pqrService.addResponse(id, { responseText, isDefinitive, isPartialUpdate, requiresConfirmation });
      setShowResponseModal(false);
      setResponseText('');
      setIsDefinitive(false);
      setIsPartialUpdate(false);
      setRequiresConfirmation(false);
      await fetchDetail();
    } catch (err: any) {
      setError(err?.response?.data || 'Error al agregar la respuesta.');
    } finally {
      setSubmitting(false);
    }
  };

  const handleAddNote = async () => {
    setError('');
    if (!noteText.trim()) { setError('La nota es requerida.'); return; }
    setSubmitting(true);
    try {
      await pqrService.addInternalNote(id, { noteText });
      setShowNoteModal(false);
      setNoteText('');
      await fetchDetail();
    } catch (err: any) {
      setError(err?.response?.data || 'Error al agregar la nota.');
    } finally {
      setSubmitting(false);
    }
  };

  if (loading) {
    return <div className="flex justify-center py-20"><Loader2 className="w-8 h-8 animate-spin text-emerald-600" /></div>;
  }

  if (error || !detail) {
    return (
      <div className="space-y-6">
        <button onClick={() => router.push('/pqr')} className="inline-flex items-center gap-2 text-sm text-muted-foreground hover:text-foreground transition-colors">
          <ArrowLeft className="w-4 h-4" /> Volver a Bandeja PQR
        </button>
        <Card>
          <CardContent className="p-6 text-center text-rose-600">
            <AlertTriangle className="w-8 h-8 mx-auto mb-2" />
            <p className="font-semibold">{error || 'PQR no encontrada'}</p>
          </CardContent>
        </Card>
      </div>
    );
  }

  const tabs: { key: DetailTab; label: string; icon: React.ReactNode }[] = [
    { key: 'info', label: 'Información', icon: <FileText className="w-4 h-4" /> },
    { key: 'followup', label: 'Seguimiento', icon: <Clock className="w-4 h-4" /> },
    { key: 'responses', label: `Respuestas (${detail.responses.length})`, icon: <Send className="w-4 h-4" /> },
    { key: 'notes', label: `Notas Internas (${detail.internalNotes.length})`, icon: <MessageSquare className="w-4 h-4" /> },
    { key: 'files', label: `Archivos (${detail.files.length})`, icon: <FileText className="w-4 h-4" /> },
    { key: 'alerts', label: `Alertas (${detail.alerts.filter(a => a.isActive).length})`, icon: <AlertTriangle className="w-4 h-4" /> },
  ];

  const isClosed = detail.status === 'Closed' || detail.status === 'Responded';

  return (
    <div className="space-y-6">
      <button onClick={() => router.push('/pqr')} className="inline-flex items-center gap-2 text-sm text-muted-foreground hover:text-foreground transition-colors">
        <ArrowLeft className="w-4 h-4" /> Volver a Bandeja PQR
      </button>

      <Card>
        <CardContent className="p-5">
          <div className="flex flex-col sm:flex-row justify-between items-start sm:items-center gap-4">
            <div className="flex items-center gap-3">
              <div>
                <h1 className="text-xl font-bold text-foreground font-mono">{detail.radicadoNumber}</h1>
                <p className="text-sm text-muted-foreground mt-0.5">{detail.subject}</p>
              </div>
              <div className="flex items-center gap-2 mt-1">
                {statusBadge(detail.status)}
                {priorityBadge(detail.priority)}
                {detail.isInternal && <span className="text-[10px] font-bold text-amber-600 bg-amber-50 px-1.5 py-0.5 rounded">INTERNA</span>}
              </div>
            </div>
            <div className="flex flex-wrap gap-2">
              <Button variant="secondary" onClick={() => setShowStatusModal(true)} disabled={isClosed}>
                <RefreshCw className="w-4 h-4 mr-1" /> Cambiar Estado
              </Button>
              <Button variant="secondary" onClick={() => setShowResponseModal(true)} disabled={isClosed}>
                <Send className="w-4 h-4 mr-1" /> Responder
              </Button>
              <Button variant="ghost" onClick={() => setShowNoteModal(true)}>
                <Plus className="w-4 h-4 mr-1" /> Nota Interna
              </Button>
            </div>
          </div>

          <div className="flex items-center gap-3 mt-4 p-3 bg-muted/50 rounded-lg">
            <div className="flex-1">
              <div className="flex justify-between text-xs text-muted-foreground mb-1">
                <span>Progreso: {detail.elapsedPercent.toFixed(0)}%</span>
                <span>Vence: {formatDate(detail.deadline)}</span>
              </div>
              <div className="w-full h-2 bg-muted rounded-full overflow-hidden">
                <div className={`h-full rounded-full ${semaphoreColor(detail.elapsedPercent)}`}
                  style={{ width: `${Math.min(detail.elapsedPercent, 100)}%` }} />
              </div>
            </div>
          </div>
        </CardContent>
      </Card>

      <div className="flex items-center gap-2 border-b border-border overflow-x-auto">
        {tabs.map((t) => (
          <button key={t.key} onClick={() => setActiveTab(t.key)}
            className={`flex items-center gap-1.5 px-5 py-3 text-sm font-semibold border-b-2 transition-all whitespace-nowrap ${
              activeTab === t.key ? 'border-emerald-600 text-emerald-600' : 'border-transparent text-muted-foreground hover:text-foreground'
            }`}>
            {t.icon} {t.label}
          </button>
        ))}
      </div>

      {activeTab === 'info' && (
        <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
          <Card>
            <CardHeader><h3 className="font-bold text-foreground">Información General</h3></CardHeader>
            <CardContent className="p-5 space-y-3">
              <Row label="Tipo" value={typeLabels[detail.pqrType] || detail.pqrType} />
              <Row label="Categoría" value={categoryLabels[detail.category] || detail.category} />
              <Row label="Canal" value={channelLabels[detail.channel] || detail.channel} />
              <Row label="Radicado" value={detail.radicadoNumber} />
              <Row label="Fecha de Radicación" value={formatDate(detail.filedAt)} />
              <Row label="Fecha de Cierre" value={detail.closedAt ? formatDate(detail.closedAt) : '-'} />
              {detail.closedDefinitivelyAt && <Row label="Cierre Definitivo" value={formatDate(detail.closedDefinitivelyAt)} />}
            </CardContent>
          </Card>
          <Card>
            <CardHeader><h3 className="font-bold text-foreground">Radicador y Unidad</h3></CardHeader>
            <CardContent className="p-5 space-y-3">
              <Row label="Radicador" value={detail.radiadorName} />
              <Row label="Documento" value={detail.radiadorDocumentNumber ? `${documentTypeLabels[detail.radiadorDocumentType] || detail.radiadorDocumentType}: ${detail.radiadorDocumentNumber}` : '-'} />
              <Row label="Contacto" value={detail.radiadorContact || '-'} />
              <Row label="Unidad" value={detail.unitIdentifier} />
              <Row label="Asignado a" value={detail.assignedToUserId || 'Sin asignar'} />
            </CardContent>
          </Card>
          <Card className="md:col-span-2">
            <CardHeader><h3 className="font-bold text-foreground">Descripción</h3></CardHeader>
            <CardContent className="p-5">
              <p className="text-sm text-foreground whitespace-pre-wrap">{detail.description}</p>
            </CardContent>
          </Card>
          {detail.pqrType === 'Claim' && (
            <Card className="md:col-span-2">
              <CardHeader><h3 className="font-bold text-foreground">Información del Reclamo</h3></CardHeader>
              <CardContent className="p-5 space-y-3">
                <Row label="Vinculado a Cobro" value={detail.isLinkedToCharge ? 'Sí' : 'No'} />
                {detail.involvedResidentName && <Row label="Residente Involucrado" value={`${detail.involvedResidentName} (${detail.involvedResidentUnitId || ''})`} />}
                <Row label="Reclamo Resuelto" value={detail.claimResolved ? 'Sí' : 'No'} />
                {detail.claimResolutionNote && <Row label="Nota de Resolución" value={detail.claimResolutionNote} />}
                <Row label="Nota Crédito Generada" value={detail.creditNoteGenerated ? 'Sí' : 'No'} />
              </CardContent>
            </Card>
          )}
          {detail.relatedPQRId && (
            <Card className="md:col-span-2">
              <CardHeader><h3 className="font-bold text-foreground">PQR Relacionada</h3></CardHeader>
              <CardContent className="p-5">
                <button onClick={() => router.push(`/pqr/${detail.relatedPQRId}`)}
                  className="text-emerald-600 hover:text-emerald-800 text-sm font-semibold underline">
                  {detail.relatedRadicadoNumber || detail.relatedPQRId}
                </button>
              </CardContent>
            </Card>
          )}
        </div>
      )}

      {activeTab === 'followup' && (
        <Card>
          <CardContent className="p-0">
            <div className="p-5 space-y-0">
              {detail.followUps.length === 0 ? (
                <p className="text-sm text-muted-foreground text-center py-8">No hay seguimiento registrado.</p>
              ) : (
                detail.followUps.map((fu, idx) => (
                  <div key={fu.id} className="flex gap-4 pb-6 relative">
                    {idx < detail.followUps.length - 1 && <div className="absolute left-[11px] top-8 bottom-0 w-0.5 bg-border" />}
                    <div className="w-6 h-6 rounded-full bg-emerald-100 flex items-center justify-center flex-shrink-0 mt-0.5">
                      <div className="w-3 h-3 rounded-full bg-emerald-600" />
                    </div>
                    <div className="flex-1 min-w-0">
                      <div className="flex items-center gap-2 flex-wrap">
                        <span className="font-semibold text-sm text-foreground">{statusLabels[fu.previousStatus] || fu.previousStatus}</span>
                        <span className="text-muted-foreground text-xs">→</span>
                        <span className="font-bold text-sm text-emerald-600">{statusLabels[fu.newStatus] || fu.newStatus}</span>
                        {fu.isAutomatic && <span className="text-[10px] font-bold text-blue-600 bg-blue-50 px-1.5 py-0.5 rounded">AUTO</span>}
                      </div>
                      {fu.justification && <p className="text-xs text-muted-foreground mt-1">{fu.justification}</p>}
                      <div className="flex items-center gap-2 mt-1">
                        <User className="w-3 h-3 text-muted-foreground" />
                        <span className="text-xs text-muted-foreground">{fu.changedByUserName}</span>
                        <span className="text-xs text-muted-foreground">· {formatDate(fu.changedAt)}</span>
                      </div>
                    </div>
                  </div>
                ))
              )}
            </div>
          </CardContent>
        </Card>
      )}

      {activeTab === 'responses' && (
        <div className="space-y-4">
          {detail.responses.length === 0 ? (
            <Card><CardContent className="p-6 text-center text-sm text-muted-foreground">No hay respuestas registradas.</CardContent></Card>
          ) : (
            detail.responses.map((r) => (
              <Card key={r.id}>
                <CardContent className="p-5">
                  <div className="flex items-start justify-between gap-4">
                    <div className="flex-1">
                      <p className="text-sm text-foreground whitespace-pre-wrap">{r.responseText}</p>
                      <div className="flex items-center gap-3 mt-3 text-xs text-muted-foreground">
                        <span className="flex items-center gap-1"><User className="w-3 h-3" /> {r.sentByUserName}</span>
                        <span>{formatDate(r.sentAt)}</span>
                      </div>
                    </div>
                    <div className="flex flex-col gap-1 items-end shrink-0">
                      {r.isDefinitive && <span className="text-[10px] font-bold text-emerald-600 bg-emerald-50 px-1.5 py-0.5 rounded">DEFINITIVA</span>}
                      {r.isPartialUpdate && <span className="text-[10px] font-bold text-amber-600 bg-amber-50 px-1.5 py-0.5 rounded">ACT. PARCIAL</span>}
                      {r.requiresConfirmation && (
                        r.confirmedByRadiador
                          ? <span className="text-[10px] font-bold text-emerald-600 bg-emerald-50 px-1.5 py-0.5 rounded">CONFIRMADA</span>
                          : <span className="text-[10px] font-bold text-orange-600 bg-orange-50 px-1.5 py-0.5 rounded">PEND. CONFIRMAR</span>
                      )}
                    </div>
                  </div>
                  {r.files.length > 0 && (
                    <div className="mt-3 pt-3 border-t border-border">
                      <p className="text-xs font-bold text-muted-foreground mb-2">Archivos adjuntos:</p>
                      <div className="flex flex-wrap gap-2">
                        {r.files.map((f) => (
                          <span key={f.id} className="text-xs text-muted-foreground bg-muted px-2 py-1 rounded">{f.originalFileName}</span>
                        ))}
                      </div>
                    </div>
                  )}
                </CardContent>
              </Card>
            ))
          )}
        </div>
      )}

      {activeTab === 'notes' && (
        <Card>
          <CardContent className="p-0">
            <div className="p-5 space-y-4">
              {detail.internalNotes.length === 0 ? (
                <p className="text-sm text-muted-foreground text-center py-8">No hay notas internas.</p>
              ) : (
                detail.internalNotes.map((n) => (
                  <div key={n.id} className="p-4 bg-amber-50/50 border border-amber-100 rounded-lg">
                    <p className="text-sm text-foreground whitespace-pre-wrap">{n.noteText}</p>
                    <div className="flex items-center gap-2 mt-2 text-xs text-muted-foreground">
                      <User className="w-3 h-3" />
                      <span>{n.authorName}</span>
                      <span>· {formatDate(n.createdAt)}</span>
                    </div>
                  </div>
                ))
              )}
            </div>
          </CardContent>
        </Card>
      )}

      {activeTab === 'files' && (
        <Card>
          <CardContent className="p-0">
            <div className="overflow-x-auto">
              <table className="min-w-full divide-y divide-border">
                <thead className="bg-muted/50">
                  <tr>
                    <th className="px-5 py-3 text-left text-xs font-bold text-muted-foreground uppercase">Archivo</th>
                    <th className="px-5 py-3 text-left text-xs font-bold text-muted-foreground uppercase">Tipo</th>
                    <th className="px-5 py-3 text-left text-xs font-bold text-muted-foreground uppercase">Tamaño</th>
                    <th className="px-5 py-3 text-left text-xs font-bold text-muted-foreground uppercase">Subido por</th>
                    <th className="px-5 py-3 text-left text-xs font-bold text-muted-foreground uppercase">Fecha</th>
                    <th className="px-5 py-3 text-left text-xs font-bold text-muted-foreground uppercase">Origen</th>
                  </tr>
                </thead>
                <tbody className="divide-y divide-border">
                  {detail.files.length === 0 ? (
                    <tr><td colSpan={6} className="px-6 py-12 text-center text-sm text-muted-foreground">No hay archivos adjuntos.</td></tr>
                  ) : (
                    detail.files.map((f) => (
                      <tr key={f.id}>
                        <td className="px-5 py-3 text-sm font-semibold text-foreground">{f.originalFileName}</td>
                        <td className="px-5 py-3 text-sm text-muted-foreground">{f.contentType}</td>
                        <td className="px-5 py-3 text-sm text-muted-foreground">{(f.fileSize / 1024).toFixed(1)} KB</td>
                        <td className="px-5 py-3 text-sm text-muted-foreground">{f.uploadedByUserName}</td>
                        <td className="px-5 py-3 text-sm text-muted-foreground">{formatDate(f.uploadedAt)}</td>
                        <td className="px-5 py-3 text-sm">{f.isFromApplicant ? <span className="badge-info">Radicador</span> : <span className="badge-neutral">Interno</span>}</td>
                      </tr>
                    ))
                  )}
                </tbody>
              </table>
            </div>
          </CardContent>
        </Card>
      )}

      {activeTab === 'alerts' && (
        <div className="space-y-3">
          {detail.alerts.length === 0 ? (
            <Card><CardContent className="p-6 text-center text-sm text-muted-foreground">No hay alertas registradas.</CardContent></Card>
          ) : (
            detail.alerts.map((a) => (
              <Card key={a.id}>
                <CardContent className="p-4 flex items-center justify-between">
                  <div className="flex items-center gap-3">
                    <div className={`w-8 h-8 rounded-lg flex items-center justify-center ${a.isActive ? 'bg-rose-50 text-rose-600' : 'bg-emerald-50 text-emerald-600'}`}>
                      {a.isActive ? <AlertTriangle className="w-4 h-4" /> : <CheckCircle2 className="w-4 h-4" />}
                    </div>
                    <div>
                      <p className="text-sm font-semibold text-foreground">{alertTypeLabels[a.alertType] || a.alertType}</p>
                      <p className="text-xs text-muted-foreground">Generada: {formatDate(a.generatedAt)}</p>
                      {a.resolvedAt && <p className="text-xs text-muted-foreground">Resuelta: {formatDate(a.resolvedAt)}</p>}
                    </div>
                  </div>
                  <div className="flex items-center gap-2">
                    {a.escalatedToCouncil && <span className="badge-danger">ESCALADA</span>}
                    {a.isActive
                      ? <span className="badge-warning">ACTIVA</span>
                      : <span className="badge-success">RESUELTA</span>}
                  </div>
                </CardContent>
              </Card>
            ))
          )}
        </div>
      )}

      {/* Status Change Modal */}
      {showStatusModal && (
        <div className="fixed inset-0 bg-black/60 backdrop-blur-sm z-[150] flex items-center justify-center p-4" onClick={() => setShowStatusModal(false)}>
          <div className="bg-card text-card-foreground w-full max-w-md rounded-xl border border-border shadow-lg animate-in zoom-in-95 duration-200" onClick={(e) => e.stopPropagation()}>
            <div className="p-5 border-b border-border">
              <h3 className="font-bold text-foreground">Cambiar Estado de PQR</h3>
            </div>
            <div className="p-5 space-y-4">
              <div>
                <label className="block text-xs font-bold text-muted-foreground uppercase tracking-widest mb-1.5">Nuevo Estado</label>
                <select value={newStatus} onChange={(e) => setNewStatus(e.target.value)}
                  className="w-full bg-transparent border border-border rounded-lg px-3 py-2 text-sm text-foreground focus:outline-none focus:ring-1 focus:ring-emerald-500">
                  {availableStatuses.map((s) => <option key={s.value} value={s.value}>{s.label}</option>)}
                </select>
              </div>
              <div>
                <label className="block text-xs font-bold text-muted-foreground uppercase tracking-widest mb-1.5">Justificación</label>
                <textarea value={statusJus} onChange={(e) => setStatusJus(e.target.value)} rows={3}
                  placeholder="Motivo del cambio de estado..."
                  className="w-full bg-transparent border border-border rounded-lg px-3 py-2 text-sm text-foreground focus:outline-none focus:ring-1 focus:ring-emerald-500 resize-none" />
              </div>
              {error && <div className="p-3 bg-rose-50 border border-rose-200 rounded-lg text-rose-700 text-xs flex items-center gap-2"><AlertTriangle className="w-4 h-4 shrink-0" /> {error}</div>}
              <div className="flex justify-end gap-3 pt-2">
                <Button variant="ghost" onClick={() => setShowStatusModal(false)}>Cancelar</Button>
                <Button onClick={handleStatusChange} disabled={submitting}>
                  {submitting ? <Loader2 className="w-4 h-4 animate-spin mr-2" /> : <RefreshCw className="w-4 h-4 mr-2" />}
                  Cambiar Estado
                </Button>
              </div>
            </div>
          </div>
        </div>
      )}

      {/* Response Modal */}
      {showResponseModal && (
        <div className="fixed inset-0 bg-black/60 backdrop-blur-sm z-[150] flex items-center justify-center p-4" onClick={() => setShowResponseModal(false)}>
          <div className="bg-card text-card-foreground w-full max-w-lg rounded-xl border border-border shadow-lg animate-in zoom-in-95 duration-200" onClick={(e) => e.stopPropagation()}>
            <div className="p-5 border-b border-border">
              <h3 className="font-bold text-foreground">Agregar Respuesta</h3>
            </div>
            <div className="p-5 space-y-4">
              <div>
                <label className="block text-xs font-bold text-muted-foreground uppercase tracking-widest mb-1.5">Texto de la Respuesta</label>
                <textarea value={responseText} onChange={(e) => setResponseText(e.target.value)} rows={5}
                  placeholder="Redacte la respuesta al radicador..."
                  className="w-full bg-transparent border border-border rounded-lg px-3 py-2 text-sm text-foreground focus:outline-none focus:ring-1 focus:ring-emerald-500 resize-none" />
              </div>
              <div className="flex flex-wrap gap-4">
                <label className="flex items-center gap-2 text-sm text-muted-foreground cursor-pointer">
                  <input type="checkbox" checked={isDefinitive} onChange={(e) => setIsDefinitive(e.target.checked)} className="accent-emerald-600" />
                  Respuesta Definitiva
                </label>
                <label className="flex items-center gap-2 text-sm text-muted-foreground cursor-pointer">
                  <input type="checkbox" checked={isPartialUpdate} onChange={(e) => setIsPartialUpdate(e.target.checked)} className="accent-emerald-600" />
                  Actualización Parcial
                </label>
                <label className="flex items-center gap-2 text-sm text-muted-foreground cursor-pointer">
                  <input type="checkbox" checked={requiresConfirmation} onChange={(e) => setRequiresConfirmation(e.target.checked)} className="accent-emerald-600" />
Requiere Confirmación
                </label>
              </div>
              {error && <div className="p-3 bg-rose-50 border border-rose-200 rounded-lg text-rose-700 text-xs flex items-center gap-2"><AlertTriangle className="w-4 h-4 shrink-0" /> {error}</div>}
              <div className="flex justify-end gap-3 pt-2">
                <Button variant="ghost" onClick={() => setShowResponseModal(false)}>Cancelar</Button>
                <Button onClick={handleAddResponse} disabled={submitting}>
                  {submitting ? <Loader2 className="w-4 h-4 animate-spin mr-2" /> : <Send className="w-4 h-4 mr-2" />}
                  Enviar Respuesta
                </Button>
              </div>
            </div>
          </div>
        </div>
      )}

      {/* Note Modal */}
      {showNoteModal && (
        <div className="fixed inset-0 bg-black/60 backdrop-blur-sm z-[150] flex items-center justify-center p-4" onClick={() => setShowNoteModal(false)}>
          <div className="bg-card text-card-foreground w-full max-w-md rounded-xl border border-border shadow-lg animate-in zoom-in-95 duration-200" onClick={(e) => e.stopPropagation()}>
            <div className="p-5 border-b border-border">
              <h3 className="font-bold text-foreground">Agregar Nota Interna</h3>
            </div>
            <div className="p-5 space-y-4">
              <div>
                <label className="block text-xs font-bold text-muted-foreground uppercase tracking-widest mb-1.5">Nota</label>
                <textarea value={noteText} onChange={(e) => setNoteText(e.target.value)} rows={4}
                  placeholder="Nota interna (no visible para el radicador)..."
                  className="w-full bg-transparent border border-border rounded-lg px-3 py-2 text-sm text-foreground focus:outline-none focus:ring-1 focus:ring-emerald-500 resize-none" />
              </div>
              {error && <div className="p-3 bg-rose-50 border border-rose-200 rounded-lg text-rose-700 text-xs flex items-center gap-2"><AlertTriangle className="w-4 h-4 shrink-0" /> {error}</div>}
              <div className="flex justify-end gap-3 pt-2">
                <Button variant="ghost" onClick={() => setShowNoteModal(false)}>Cancelar</Button>
                <Button onClick={handleAddNote} disabled={submitting}>
                  {submitting ? <Loader2 className="w-4 h-4 animate-spin mr-2" /> : <Plus className="w-4 h-4 mr-2" />}
                  Agregar Nota
                </Button>
              </div>
            </div>
          </div>
        </div>
      )}
    </div>
  );
}

function Row({ label, value }: { label: string; value: string }) {
  return (
    <div className="flex justify-between items-center">
      <span className="text-xs font-semibold text-muted-foreground uppercase tracking-wider">{label}</span>
      <span className="text-sm font-medium text-foreground text-right max-w-[60%] truncate">{value}</span>
    </div>
  );
}
