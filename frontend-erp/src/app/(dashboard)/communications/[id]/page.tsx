'use client';

import React, { useState, useEffect } from 'react';
import { useParams, useRouter } from 'next/navigation';
import { Loader2, Send, Archive, RefreshCw, ArrowLeft, CheckCircle, XCircle, Clock } from 'lucide-react';
import { Button } from '@/components/ui/Button';
import { Card, CardContent } from '@/components/ui/Card';
import communicationService, { CommunicationDetail, CommunicationRecipient } from '@/lib/communication-service';
import axios from 'axios';

const statusLabels: Record<string, string> = {
  Pending: 'Pendiente',
  Sent: 'Enviado',
  Delivered: 'Entregado',
  Read: 'Leído',
  Failed: 'Fallido',
  Bounced: 'Rebotado',
};

const statusBadgeClass: Record<string, string> = {
  Pending: 'badge-warning',
  Sent: 'badge-info',
  Delivered: 'badge-success',
  Read: 'badge-success',
  Failed: 'badge-danger',
  Bounced: 'badge-danger',
};

const channelLabels: Record<string, string> = {
  Email: 'Correo',
  Sms: 'SMS',
  Push: 'Push',
  BulletinBoard: 'Cartelera',
};

export default function CommunicationDetailPage() {
  const params = useParams();
  const router = useRouter();
  const [communication, setCommunication] = useState<CommunicationDetail | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');
  const [sending, setSending] = useState(false);

  const id = params.id as string;

  useEffect(() => {
    fetchCommunication();
  }, [id]);

  const fetchCommunication = async () => {
    setLoading(true);
    setError('');
    try {
      const data = await communicationService.getCommunication(id);
      setCommunication(data);
    } catch {
      setError('Error al cargar el comunicado.');
    } finally {
      setLoading(false);
    }
  };

  const handleSend = async () => {
    setSending(true);
    setError('');
    try {
      const data = await communicationService.sendCommunication(id);
      setCommunication(data);
    } catch (err) {
      if (axios.isAxiosError(err) && err.response?.data?.message) {
        setError(err.response.data.message);
      } else {
        setError('Error al enviar el comunicado.');
      }
    } finally {
      setSending(false);
    }
  };

  const handleResendUnconfirmed = async () => {
    setError('');
    try {
      await communicationService.resendUnconfirmed(id);
      fetchCommunication();
    } catch (err) {
      if (axios.isAxiosError(err) && err.response?.data?.message) {
        setError(err.response.data.message);
      } else {
        setError('Error al reenviar.');
      }
    }
  };

  const handleArchive = async () => {
    try {
      await communicationService.archiveCommunication(id);
      router.push('/communications');
    } catch {
      setError('Error al archivar.');
    }
  };

  if (loading) {
    return (
      <div className="flex items-center justify-center min-h-[60vh]">
        <Loader2 className="w-8 h-8 animate-spin text-emerald-600" />
      </div>
    );
  }

  if (!communication) {
    return (
      <div className="p-6">
        <div className="p-4 bg-red-50 dark:bg-red-950/30 border border-red-200 dark:border-red-900 rounded-xl text-red-700 dark:text-red-400 text-sm">
          {error || 'Comunicado no encontrado'}
        </div>
      </div>
    );
  }

  const confirmedCount = communication.recipients.filter((r) => r.readConfirmedAt).length;

  return (
    <div className="p-6 space-y-6 max-w-5xl">
      <div className="flex items-center justify-between">
        <div className="flex items-center gap-3">
          <button onClick={() => router.back()} className="p-2 rounded-lg hover:bg-muted transition-colors">
            <ArrowLeft className="w-5 h-5 text-muted-foreground" />
          </button>
          <h1 className="text-2xl font-bold text-foreground truncate max-w-lg">{communication.subject}</h1>
          <span className={`badge ${statusBadgeClass[communication.status] || 'badge-neutral'}`}>
            {statusLabels[communication.status] || communication.status}
          </span>
        </div>
        <div className="flex items-center gap-2">
          {(communication.status === 'Draft' || communication.status === 'Scheduled') && (
            <Button onClick={handleSend} disabled={sending}>
              <Send className="w-4 h-4 mr-2" />
              {sending ? 'Enviando...' : 'Enviar Ahora'}
            </Button>
          )}
          {communication.status === 'Sent' && communication.requiresReadConfirmation && (
            <Button variant="secondary" onClick={handleResendUnconfirmed}>
              <RefreshCw className="w-4 h-4 mr-2" />
              Reenviar a no confirmados
            </Button>
          )}
          {communication.status !== 'Archived' && (
            <Button variant="secondary" onClick={handleArchive}>
              <Archive className="w-4 h-4 mr-2" />
              Archivar
            </Button>
          )}
        </div>
      </div>

      {error && (
        <div className="p-4 bg-red-50 dark:bg-red-950/30 border border-red-200 dark:border-red-900 rounded-xl text-red-700 dark:text-red-400 text-sm">
          {error}
        </div>
      )}

      <div className="grid grid-cols-1 lg:grid-cols-3 gap-6">
        <div className="lg:col-span-2 space-y-6">
          <Card>
            <CardContent className="p-6">
              <h2 className="text-lg font-semibold text-foreground mb-4">Contenido</h2>
              <div className="prose prose-sm dark:prose-invert max-w-none whitespace-pre-wrap">
                {communication.body}
              </div>
            </CardContent>
          </Card>

          <Card>
            <CardContent className="p-6">
              <div className="flex items-center justify-between mb-4">
                <h2 className="text-lg font-semibold text-foreground">
                  Destinatarios ({communication.recipients.length})
                </h2>
                {communication.requiresReadConfirmation && (
                  <span className="text-sm text-muted-foreground">
                    <CheckCircle className="w-4 h-4 inline mr-1 text-emerald-600" />
                    {confirmedCount} confirmaciones
                  </span>
                )}
              </div>

              {communication.recipients.length === 0 ? (
                <p className="text-muted-foreground text-sm">No hay destinatarios registrados.</p>
              ) : (
                <div className="overflow-x-auto">
                  <table className="w-full text-sm">
                    <thead>
                      <tr className="border-b border-border">
                        <th className="text-left py-2 font-medium text-muted-foreground">Nombre</th>
                        <th className="text-left py-2 font-medium text-muted-foreground">Email</th>
                        <th className="text-center py-2 font-medium text-muted-foreground">Correo</th>
                        <th className="text-center py-2 font-medium text-muted-foreground">SMS</th>
                        <th className="text-center py-2 font-medium text-muted-foreground">Push</th>
                        {communication.requiresReadConfirmation && (
                          <th className="text-center py-2 font-medium text-muted-foreground">Lectura</th>
                        )}
                      </tr>
                    </thead>
                    <tbody>
                      {communication.recipients.map((rec) => (
                        <tr key={rec.id} className="border-b border-border/50">
                          <td className="py-2">{rec.ownerName || rec.tenantResidentName || '—'}</td>
                          <td className="py-2 text-muted-foreground">{rec.recipientEmail || '—'}</td>
                          <td className="py-2 text-center">{renderStatusIcon(rec.emailStatus)}</td>
                          <td className="py-2 text-center">{renderStatusIcon(rec.smsStatus)}</td>
                          <td className="py-2 text-center">{renderStatusIcon(rec.pushStatus)}</td>
                          {communication.requiresReadConfirmation && (
                            <td className="py-2 text-center">
                              {rec.readConfirmedAt ? (
                                <CheckCircle className="w-4 h-4 text-emerald-600 mx-auto" />
                              ) : (
                                <XCircle className="w-4 h-4 text-slate-300 mx-auto" />
                              )}
                            </td>
                          )}
                        </tr>
                      ))}
                    </tbody>
                  </table>
                </div>
              )}
            </CardContent>
          </Card>
        </div>

        <div className="space-y-6">
          <Card>
            <CardContent className="p-6 space-y-3">
              <h3 className="font-semibold text-foreground mb-2">Información</h3>
              <div className="space-y-2 text-sm">
                <div>
                  <span className="text-muted-foreground">Audiencia:</span>
                  <p className="font-medium">{communication.audienceType}</p>
                </div>
                <div>
                  <span className="text-muted-foreground">Canales:</span>
                  <div className="flex flex-wrap gap-1 mt-1">
                    {communication.selectedChannels.map((ch) => (
                      <span key={ch} className="badge badge-neutral">{channelLabels[ch] || ch}</span>
                    ))}
                  </div>
                </div>
                {communication.sendAt && (
                  <div>
                    <span className="text-muted-foreground">Programado:</span>
                    <p className="font-medium flex items-center gap-1">
                      <Clock className="w-3 h-3" />
                      {new Date(communication.sendAt).toLocaleString('es-CO')}
                    </p>
                  </div>
                )}
                {communication.sentAt && (
                  <div>
                    <span className="text-muted-foreground">Enviado:</span>
                    <p className="font-medium">{new Date(communication.sentAt).toLocaleString('es-CO')}</p>
                  </div>
                )}
                <div>
                  <span className="text-muted-foreground">Confirmación lectura:</span>
                  <p className="font-medium">{communication.requiresReadConfirmation ? 'Sí' : 'No'}</p>
                </div>
                <div>
                  <span className="text-muted-foreground">Cartelera digital:</span>
                  <p className="font-medium">{communication.publishToBulletinBoard ? 'Sí' : 'No'}</p>
                </div>
                <div>
                  <span className="text-muted-foreground">Creado:</span>
                  <p className="font-medium">{new Date(communication.createdAt).toLocaleString('es-CO')}</p>
                </div>
              </div>
            </CardContent>
          </Card>

          {communication.requiresReadConfirmation && (
            <Card>
              <CardContent className="p-6">
                <h3 className="font-semibold text-foreground mb-2">Progreso de Lectura</h3>
                <div className="text-center">
                  <span className="text-3xl font-bold text-emerald-600">
                    {Math.round((confirmedCount / communication.recipients.length) * 100)}%
                  </span>
                  <p className="text-sm text-muted-foreground mt-1">
                    {confirmedCount} de {communication.recipients.length} confirmaron
                  </p>
                </div>
              </CardContent>
            </Card>
          )}
        </div>
      </div>
    </div>
  );
}

function renderStatusIcon(status: string) {
  switch (status) {
    case 'Delivered':
    case 'Read':
      return <CheckCircle className="w-4 h-4 text-emerald-600 mx-auto" />;
    case 'Failed':
    case 'Bounced':
      return <XCircle className="w-4 h-4 text-red-500 mx-auto" />;
    case 'Sent':
      return <Send className="w-4 h-4 text-blue-500 mx-auto" />;
    default:
      return <Clock className="w-4 h-4 text-slate-300 mx-auto" />;
  }
}
