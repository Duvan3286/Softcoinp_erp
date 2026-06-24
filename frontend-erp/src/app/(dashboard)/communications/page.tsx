'use client';

import React, { useState, useEffect, useCallback } from 'react';
import { useRouter } from 'next/navigation';
import { Loader2, Plus, Search, Eye, Archive, Send, Clock, CheckCircle, XCircle } from 'lucide-react';
import { Button } from '@/components/ui/Button';
import { Card, CardContent } from '@/components/ui/Card';
import communicationService, { CommunicationSummary } from '@/lib/communication-service';

const statusLabels: Record<string, string> = {
  Draft: 'Borrador',
  Scheduled: 'Programado',
  Sent: 'Enviado',
  Archived: 'Archivado',
};

const statusBadgeClass: Record<string, string> = {
  Draft: 'badge-warning',
  Scheduled: 'badge-info',
  Sent: 'badge-success',
  Archived: 'badge-neutral',
};

const audienceLabels: Record<string, string> = {
  AllOwners: 'Todos los Propietarios',
  AllResidents: 'Todos los Residentes',
  SpecificUnits: 'Unidades Específicas',
  SpecificTowers: 'Torres Específicas',
  CustomGroup: 'Grupo Personalizado',
};

export default function CommunicationsPage() {
  const router = useRouter();
  const [communications, setCommunications] = useState<CommunicationSummary[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');
  const [searchTerm, setSearchTerm] = useState('');
  const [statusFilter, setStatusFilter] = useState('');
  const [fromDate, setFromDate] = useState('');
  const [toDate, setToDate] = useState('');

  const fetchCommunications = useCallback(async () => {
    setLoading(true);
    setError('');
    try {
      const data = await communicationService.getCommunications(
        statusFilter || undefined,
        fromDate || undefined,
        toDate || undefined
      );
      setCommunications(data);
    } catch {
      setError('Error al cargar los comunicados.');
    } finally {
      setLoading(false);
    }
  }, [statusFilter, fromDate, toDate]);

  useEffect(() => {
    fetchCommunications();
  }, [fetchCommunications]);

  const handleSend = async (id: string) => {
    try {
      await communicationService.sendCommunication(id);
      fetchCommunications();
    } catch {
      setError('Error al enviar el comunicado.');
    }
  };

  const handleArchive = async (id: string) => {
    try {
      await communicationService.archiveCommunication(id);
      fetchCommunications();
    } catch {
      setError('Error al archivar el comunicado.');
    }
  };

  const handleCancel = async (id: string) => {
    try {
      await communicationService.cancelScheduled(id);
      fetchCommunications();
    } catch {
      setError('Error al cancelar el comunicado.');
    }
  };

  const filtered = communications.filter((c) =>
    c.subject.toLowerCase().includes(searchTerm.toLowerCase())
  );

  if (loading) {
    return (
      <div className="flex items-center justify-center min-h-[60vh]">
        <Loader2 className="w-8 h-8 animate-spin text-emerald-600" />
      </div>
    );
  }

  return (
    <div className="p-6 space-y-6">
      <div className="flex items-center justify-between">
        <h1 className="text-2xl font-bold text-foreground">Comunicados</h1>
        <Button onClick={() => router.push('/communications/new')}>
          <Plus className="w-4 h-4 mr-2" />
          Nuevo Comunicado
        </Button>
      </div>

      {error && (
        <div className="p-4 bg-red-50 dark:bg-red-950/30 border border-red-200 dark:border-red-900 rounded-xl text-red-700 dark:text-red-400 text-sm">
          {error}
        </div>
      )}

      <Card>
        <CardContent className="p-4">
          <div className="flex flex-wrap gap-3 items-center">
            <div className="flex items-center gap-2 bg-muted rounded-lg px-3 py-2 flex-1 min-w-[200px]">
              <Search className="w-4 h-4 text-muted-foreground" />
              <input
                type="text"
                placeholder="Buscar por asunto..."
                value={searchTerm}
                onChange={(e) => setSearchTerm(e.target.value)}
                className="bg-transparent border-none outline-none text-sm flex-1"
              />
            </div>
            <select
              value={statusFilter}
              onChange={(e) => setStatusFilter(e.target.value)}
              className="border border-emerald-600/30 focus:border-emerald-600 text-sm font-medium py-2 outline-none rounded-lg px-3 bg-background"
            >
              <option value="">Todos los estados</option>
              <option value="Draft">Borrador</option>
              <option value="Scheduled">Programado</option>
              <option value="Sent">Enviado</option>
              <option value="Archived">Archivado</option>
            </select>
            <input
              type="date"
              value={fromDate}
              onChange={(e) => setFromDate(e.target.value)}
              className="border border-emerald-600/30 focus:border-emerald-600 text-sm font-medium py-2 outline-none rounded-lg px-3 bg-background"
              placeholder="Desde"
            />
            <input
              type="date"
              value={toDate}
              onChange={(e) => setToDate(e.target.value)}
              className="border border-emerald-600/30 focus:border-emerald-600 text-sm font-medium py-2 outline-none rounded-lg px-3 bg-background"
              placeholder="Hasta"
            />
          </div>
        </CardContent>
      </Card>

      {filtered.length === 0 ? (
        <div className="text-center py-12 text-muted-foreground">
          <Megaphone className="w-12 h-12 mx-auto mb-3 opacity-40" />
          <p>No hay comunicados registrados.</p>
        </div>
      ) : (
        <div className="space-y-3">
          {filtered.map((comm) => (
            <Card key={comm.id} className="hover:shadow-md transition-shadow">
              <CardContent className="p-4">
                <div className="flex items-start justify-between">
                  <div className="flex-1 min-w-0">
                    <div className="flex items-center gap-2 mb-1">
                      <span className={`badge ${statusBadgeClass[comm.status] || 'badge-neutral'}`}>
                        {statusLabels[comm.status] || comm.status}
                      </span>
                      {comm.requiresReadConfirmation && (
                        <span className="badge badge-info">Confirma Lectura</span>
                      )}
                      {comm.publishToBulletinBoard && (
                        <span className="badge badge-neutral">Cartelera</span>
                      )}
                    </div>
                    <h3 className="font-semibold text-foreground truncate">{comm.subject}</h3>
                    <div className="flex flex-wrap gap-x-4 gap-y-1 mt-1 text-xs text-muted-foreground">
                      <span>{audienceLabels[comm.audienceType] || comm.audienceType}</span>
                      <span>{comm.recipientCount} destinatarios</span>
                      {comm.requiresReadConfirmation && (
                        <span>{comm.readConfirmedCount} confirmaciones</span>
                      )}
                      {comm.sendAt && (
                        <span className="flex items-center gap-1">
                          <Clock className="w-3 h-3" />
                          Programado: {new Date(comm.sendAt).toLocaleDateString('es-CO')}
                        </span>
                      )}
                      {comm.sentAt && (
                        <span>Enviado: {new Date(comm.sentAt).toLocaleDateString('es-CO')}</span>
                      )}
                    </div>
                  </div>
                  <div className="flex items-center gap-2 ml-4">
                    <button
                      onClick={() => router.push(`/communications/${comm.id}`)}
                      className="p-2 rounded-lg hover:bg-muted transition-colors"
                      title="Ver detalle"
                    >
                      <Eye className="w-4 h-4 text-muted-foreground" />
                    </button>
                    {comm.status === 'Draft' && (
                      <button
                        onClick={() => handleSend(comm.id)}
                        className="p-2 rounded-lg hover:bg-emerald-50 dark:hover:bg-emerald-950/20 transition-colors"
                        title="Enviar ahora"
                      >
                        <Send className="w-4 h-4 text-emerald-600" />
                      </button>
                    )}
                    {comm.status === 'Scheduled' && (
                      <button
                        onClick={() => handleCancel(comm.id)}
                        className="p-2 rounded-lg hover:bg-amber-50 dark:hover:bg-amber-950/20 transition-colors"
                        title="Cancelar programación"
                      >
                        <XCircle className="w-4 h-4 text-amber-600" />
                      </button>
                    )}
                    {comm.status !== 'Archived' && (
                      <button
                        onClick={() => handleArchive(comm.id)}
                        className="p-2 rounded-lg hover:bg-slate-50 dark:hover:bg-slate-950/20 transition-colors"
                        title="Archivar"
                      >
                        <Archive className="w-4 h-4 text-muted-foreground" />
                      </button>
                    )}
                  </div>
                </div>
              </CardContent>
            </Card>
          ))}
        </div>
      )}
    </div>
  );
}

function Megaphone(props: React.SVGProps<SVGSVGElement>) {
  return (
    <svg {...props} fill="none" stroke="currentColor" viewBox="0 0 24 24">
      <path strokeLinecap="round" strokeLinejoin="round" strokeWidth="1.5"
        d="M11 5.882V19.24a1.76 1.76 0 01-3.417.592l-2.147-6.15M18 13a3 3 0 100-6M5.436 13.683A4.001 4.001 0 017 6h1.832c4.1 0 7.625-1.234 9.168-3v14c-1.543-1.766-5.067-3-9.168-3H7a3.988 3.988 0 01-1.564-.317z" />
    </svg>
  );
}
