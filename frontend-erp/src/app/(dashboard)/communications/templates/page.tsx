'use client';

import React, { useState, useEffect } from 'react';
import { Loader2, Plus, Pencil, Trash2, Eye } from 'lucide-react';
import { Button } from '@/components/ui/Button';
import { Card, CardContent } from '@/components/ui/Card';
import communicationService, { NotificationTemplate } from '@/lib/communication-service';
import axios from 'axios';

const eventTypeLabels: Record<string, string> = {
  PaymentConfirmed: 'Pago Confirmado',
  NewMonthlyBillingAvailable: 'Nueva Liquidación',
  DelinquencyNotice1: 'Aviso Mora 1',
  DelinquencyNotice2: 'Aviso Mora 2',
  DelinquencyNotice3: 'Aviso Mora 3',
  PreLegalNotice: 'Aviso Prejurídico',
  PaymentAgreementConfirmed: 'Acuerdo de Pago Confirmado',
  PaymentAgreementDueSoon: 'Cuota de Acuerdo Próxima a Vencer',
  PeaceAndSafetyIssued: 'Paz y Salvo Expedido',
  PQRReceived: 'PQR Radicada',
  PQRStatusUpdated: 'Estado PQR Actualizado',
  PQRResponseAvailable: 'Respuesta PQR Disponible',
  PQRClosed: 'PQR Cerrada',
  ReservationApproved: 'Reserva Aprobada',
  ReservationRejected: 'Reserva Rechazada',
  ReservationReminder24h: 'Recordatorio Reserva 24h',
  ReservationReminder2h: 'Recordatorio Reserva 2h',
  DepositReturned: 'Depósito Devuelto',
  AssemblyConvocation: 'Convocatoria Asamblea',
  AssemblyReminder72h: 'Recordatorio Asamblea 72h',
  AssemblyMinutesPublished: 'Acta de Asamblea Publicada',
  MaintenanceScheduled: 'Mantenimiento Programado',
  OutOfService: 'Fuera de Servicio',
  WorkOrderResolved: 'Orden de Trabajo Resuelta',
};

export default function TemplatesPage() {
  const [templates, setTemplates] = useState<NotificationTemplate[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');
  const [editingId, setEditingId] = useState<string | null>(null);
  const [previewId, setPreviewId] = useState<string | null>(null);

  // Form state
  const [name, setName] = useState('');
  const [eventType, setEventType] = useState('PaymentConfirmed');
  const [forRecipientType, setForRecipientType] = useState('Owner');
  const [emailSubject, setEmailSubject] = useState('');
  const [emailBody, setEmailBody] = useState('');
  const [smsBody, setSmsBody] = useState('');
  const [dynamicVariables, setDynamicVariables] = useState('');
  const [saving, setSaving] = useState(false);

  useEffect(() => {
    fetchTemplates();
  }, []);

  const fetchTemplates = async () => {
    setLoading(true);
    setError('');
    try {
      const data = await communicationService.getTemplates();
      setTemplates(data);
    } catch {
      setError('Error al cargar plantillas.');
    } finally {
      setLoading(false);
    }
  };

  const resetForm = () => {
    setName('');
    setEventType('PaymentConfirmed');
    setForRecipientType('Owner');
    setEmailSubject('');
    setEmailBody('');
    setSmsBody('');
    setDynamicVariables('');
    setEditingId(null);
  };

  const handleEdit = (template: NotificationTemplate) => {
    setName(template.name);
    setEventType(template.eventType);
    setForRecipientType(template.forRecipientType);
    setEmailSubject(template.emailSubject);
    setEmailBody(template.emailBody);
    setSmsBody(template.smsBody);
    setDynamicVariables(template.dynamicVariables.join(', '));
    setEditingId(template.id);
    setPreviewId(null);
  };

  const handleSave = async () => {
    setSaving(true);
    setError('');
    try {
      const vars = dynamicVariables
        .split(',')
        .map((v) => v.trim())
        .filter(Boolean);

      if (editingId) {
        await communicationService.updateTemplate(editingId, {
          name,
          emailSubject,
          emailBody,
          smsBody,
          dynamicVariables: vars,
        });
      } else {
        await communicationService.createTemplate({
          name,
          eventType,
          forRecipientType,
          emailSubject,
          emailBody,
          smsBody,
          dynamicVariables: vars,
        });
      }

      resetForm();
      fetchTemplates();
    } catch (err) {
      if (axios.isAxiosError(err) && err.response?.data?.message) {
        setError(err.response.data.message);
      } else {
        setError('Error al guardar la plantilla.');
      }
    } finally {
      setSaving(false);
    }
  };

  const handleDelete = async (id: string) => {
    if (!confirm('¿Eliminar esta plantilla?')) return;
    try {
      await communicationService.deleteTemplate(id);
      fetchTemplates();
    } catch {
      setError('Error al eliminar la plantilla.');
    }
  };

  if (loading) {
    return (
      <div className="flex items-center justify-center min-h-[60vh]">
        <Loader2 className="w-8 h-8 animate-spin text-emerald-600" />
      </div>
    );
  }

  return (
    <div className="p-6 space-y-6">
      <h1 className="text-2xl font-bold text-foreground">Plantillas de Notificación</h1>

      {error && (
        <div className="p-4 bg-red-50 dark:bg-red-950/30 border border-red-200 dark:border-red-900 rounded-xl text-red-700 dark:text-red-400 text-sm">
          {error}
        </div>
      )}

      <div className="grid grid-cols-1 lg:grid-cols-2 gap-6">
        <Card>
          <CardContent className="p-6 space-y-4">
            <h2 className="text-lg font-semibold text-foreground">
              {editingId ? 'Editar Plantilla' : 'Nueva Plantilla'}
            </h2>

            <div>
              <label className="block text-sm font-medium text-foreground mb-1">Nombre</label>
              <input
                type="text"
                value={name}
                onChange={(e) => setName(e.target.value)}
                className="w-full border-b border-emerald-600/30 focus:border-emerald-600 text-sm font-medium py-2 outline-none bg-transparent"
              />
            </div>

            {!editingId && (
              <>
                <div>
                  <label className="block text-sm font-medium text-foreground mb-1">Tipo de Evento</label>
                  <select
                    value={eventType}
                    onChange={(e) => setEventType(e.target.value)}
                    className="w-full border-b border-emerald-600/30 focus:border-emerald-600 text-sm font-medium py-2 outline-none bg-background"
                  >
                    {Object.entries(eventTypeLabels).map(([key, label]) => (
                      <option key={key} value={key}>{label}</option>
                    ))}
                  </select>
                </div>

                <div>
                  <label className="block text-sm font-medium text-foreground mb-1">Para</label>
                  <select
                    value={forRecipientType}
                    onChange={(e) => setForRecipientType(e.target.value)}
                    className="w-full border-b border-emerald-600/30 focus:border-emerald-600 text-sm font-medium py-2 outline-none bg-background"
                  >
                    <option value="Owner">Propietario</option>
                    <option value="Tenant">Arrendatario</option>
                    <option value="Both">Ambos</option>
                  </select>
                </div>
              </>
            )}

            <div>
              <label className="block text-sm font-medium text-foreground mb-1">Asunto (Email)</label>
              <input
                type="text"
                value={emailSubject}
                onChange={(e) => setEmailSubject(e.target.value)}
                className="w-full border-b border-emerald-600/30 focus:border-emerald-600 text-sm font-medium py-2 outline-none bg-transparent"
              />
            </div>

            <div>
              <label className="block text-sm font-medium text-foreground mb-1">Cuerpo (Email)</label>
              <textarea
                value={emailBody}
                onChange={(e) => setEmailBody(e.target.value)}
                className="w-full border border-emerald-600/30 focus:border-emerald-600 text-sm font-medium py-2 outline-none rounded-lg px-3 bg-background min-h-[120px]"
              />
            </div>

            <div>
              <label className="block text-sm font-medium text-foreground mb-1">Texto SMS (máx. 160 caracteres)</label>
              <textarea
                value={smsBody}
                onChange={(e) => setSmsBody(e.target.value)}
                maxLength={160}
                className="w-full border border-emerald-600/30 focus:border-emerald-600 text-sm font-medium py-2 outline-none rounded-lg px-3 bg-background"
                rows={2}
              />
              <span className="text-xs text-muted-foreground">{smsBody.length}/160</span>
            </div>

            <div>
              <label className="block text-sm font-medium text-foreground mb-1">
                Variables dinámicas (separadas por coma)
              </label>
              <input
                type="text"
                value={dynamicVariables}
                onChange={(e) => setDynamicVariables(e.target.value)}
                placeholder="Propietario, Unidad, Valor, Fecha"
                className="w-full border-b border-emerald-600/30 focus:border-emerald-600 text-sm font-medium py-2 outline-none bg-transparent"
              />
            </div>

            <div className="flex gap-2">
              <Button onClick={handleSave} disabled={saving || !name}>
                {saving ? 'Guardando...' : editingId ? 'Actualizar' : 'Crear Plantilla'}
              </Button>
              {editingId && (
                <Button variant="secondary" onClick={resetForm}>
                  Cancelar
                </Button>
              )}
            </div>
          </CardContent>
        </Card>

        <div className="space-y-3">
          <h2 className="text-lg font-semibold text-foreground">
            Plantillas Existentes ({templates.length})
          </h2>
          {templates.length === 0 ? (
            <p className="text-muted-foreground text-sm">No hay plantillas creadas.</p>
          ) : (
            templates.map((t) => (
              <Card key={t.id}>
                <CardContent className="p-4">
                  <div className="flex items-start justify-between">
                    <div className="flex-1 min-w-0">
                      <h3 className="font-semibold text-foreground">{t.name}</h3>
                      <p className="text-xs text-muted-foreground mt-1">
                        {eventTypeLabels[t.eventType] || t.eventType}
                        {t.isActive ? (
                          <span className="badge badge-success ml-2">Activa</span>
                        ) : (
                          <span className="badge badge-danger ml-2">Inactiva</span>
                        )}
                      </p>
                    </div>
                    <div className="flex items-center gap-1 ml-2">
                      <button
                        onClick={() => setPreviewId(previewId === t.id ? null : t.id)}
                        className="p-1.5 rounded-lg hover:bg-muted transition-colors"
                        title="Vista previa"
                      >
                        <Eye className="w-4 h-4 text-muted-foreground" />
                      </button>
                      <button
                        onClick={() => handleEdit(t)}
                        className="p-1.5 rounded-lg hover:bg-muted transition-colors"
                        title="Editar"
                      >
                        <Pencil className="w-4 h-4 text-muted-foreground" />
                      </button>
                      <button
                        onClick={() => handleDelete(t.id)}
                        className="p-1.5 rounded-lg hover:bg-red-50 dark:hover:bg-red-950/20 transition-colors"
                        title="Eliminar"
                      >
                        <Trash2 className="w-4 h-4 text-red-500" />
                      </button>
                    </div>
                  </div>
                  {previewId === t.id && (
                    <div className="mt-3 p-3 bg-muted rounded-lg text-sm space-y-2">
                      <p><strong>Email Asunto:</strong> {t.emailSubject}</p>
                      <p><strong>Email Cuerpo:</strong> {t.emailBody}</p>
                      <p><strong>SMS:</strong> {t.smsBody}</p>
                      {t.dynamicVariables.length > 0 && (
                        <p><strong>Variables:</strong> {t.dynamicVariables.join(', ')}</p>
                      )}
                    </div>
                  )}
                </CardContent>
              </Card>
            ))
          )}
        </div>
      </div>
    </div>
  );
}
