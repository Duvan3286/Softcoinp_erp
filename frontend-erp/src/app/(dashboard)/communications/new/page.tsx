'use client';

import React, { useState, useEffect } from 'react';
import { useRouter } from 'next/navigation';
import { Loader2, Send, Clock, Save, Upload } from 'lucide-react';
import { Button } from '@/components/ui/Button';
import { Card, CardContent } from '@/components/ui/Card';
import communicationService from '@/lib/communication-service';
import axios from 'axios';

const audienceOptions = [
  { value: 'AllOwners', label: 'Todos los Propietarios' },
  { value: 'AllResidents', label: 'Todos los Residentes' },
  { value: 'SpecificUnits', label: 'Unidades Específicas' },
  { value: 'SpecificTowers', label: 'Torres Específicas' },
];

const channelOptions = [
  { value: 'Email', label: 'Correo Electrónico' },
  { value: 'Sms', label: 'SMS' },
  { value: 'Push', label: 'Notificación Push' },
  { value: 'BulletinBoard', label: 'Cartelera Digital' },
];

export default function NewCommunicationPage() {
  const router = useRouter();
  const [subject, setSubject] = useState('');
  const [body, setBody] = useState('');
  const [audienceType, setAudienceType] = useState('AllOwners');
  const [selectedChannels, setSelectedChannels] = useState<string[]>(['Email']);
  const [sendAt, setSendAt] = useState('');
  const [sendTime, setSendTime] = useState('');
  const [requiresReadConfirmation, setRequiresReadConfirmation] = useState(false);
  const [publishToBulletinBoard, setPublishToBulletinBoard] = useState(false);
  const [saving, setSaving] = useState(false);
  const [sending, setSending] = useState(false);
  const [error, setError] = useState('');

  const toggleChannel = (channel: string) => {
    setSelectedChannels((prev) =>
      prev.includes(channel)
        ? prev.filter((c) => c !== channel)
        : [...prev, channel]
    );
  };

  const buildRequest = () => ({
    subject,
    body,
    audienceType,
    selectedChannels,
    sendAt: sendAt && sendTime
      ? new Date(`${sendAt}T${sendTime}`).toISOString()
      : null,
    requiresReadConfirmation,
    publishToBulletinBoard,
  });

  const handleSaveDraft = async () => {
    setSaving(true);
    setError('');
    try {
      const result = await communicationService.createCommunication(buildRequest());
      router.push(`/communications/${result.id}`);
    } catch (err) {
      if (axios.isAxiosError(err) && err.response?.data?.message) {
        setError(err.response.data.message);
      } else {
        setError('Error al guardar el comunicado.');
      }
    } finally {
      setSaving(false);
    }
  };

  const handleSendNow = async () => {
    setSending(true);
    setError('');
    try {
      const result = await communicationService.createCommunication({
        ...buildRequest(),
        sendAt: null,
      });
      await communicationService.sendCommunication(result.id);
      router.push(`/communications/${result.id}`);
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

  return (
    <div className="p-6 space-y-6 max-w-4xl">
      <h1 className="text-2xl font-bold text-foreground">Nuevo Comunicado</h1>

      {error && (
        <div className="p-4 bg-red-50 dark:bg-red-950/30 border border-red-200 dark:border-red-900 rounded-xl text-red-700 dark:text-red-400 text-sm">
          {error}
        </div>
      )}

      <Card>
        <CardContent className="p-6 space-y-6">
          <div>
            <label className="block text-sm font-medium text-foreground mb-1">Asunto</label>
            <input
              type="text"
              value={subject}
              onChange={(e) => setSubject(e.target.value)}
              className="w-full border-b border-emerald-600/30 focus:border-emerald-600 text-sm font-medium py-2 outline-none bg-transparent"
              placeholder="Asunto del comunicado"
            />
          </div>

          <div>
            <label className="block text-sm font-medium text-foreground mb-1">Contenido</label>
            <textarea
              value={body}
              onChange={(e) => setBody(e.target.value)}
              className="w-full border border-emerald-600/30 focus:border-emerald-600 text-sm font-medium py-2 outline-none rounded-lg px-3 bg-background min-h-[200px]"
              placeholder="Redacte el contenido del comunicado..."
            />
          </div>

          <div>
            <label className="block text-sm font-medium text-foreground mb-2">Segmentación de Destinatarios</label>
            <select
              value={audienceType}
              onChange={(e) => setAudienceType(e.target.value)}
              className="w-full border-b border-emerald-600/30 focus:border-emerald-600 text-sm font-medium py-2 outline-none bg-background"
            >
              {audienceOptions.map((opt) => (
                <option key={opt.value} value={opt.value}>{opt.label}</option>
              ))}
            </select>
          </div>

          <div>
            <label className="block text-sm font-medium text-foreground mb-2">Canales de Envío</label>
            <div className="flex flex-wrap gap-3">
              {channelOptions.map((ch) => (
                <label key={ch.value} className="flex items-center gap-2 cursor-pointer">
                  <input
                    type="checkbox"
                    checked={selectedChannels.includes(ch.value)}
                    onChange={() => toggleChannel(ch.value)}
                    className="rounded border-emerald-600/30 text-emerald-600 focus:ring-emerald-600"
                  />
                  <span className="text-sm">{ch.label}</span>
                </label>
              ))}
            </div>
          </div>

          <div className="flex items-center gap-6">
            <div className="flex-1">
              <label className="block text-sm font-medium text-foreground mb-1">Programar Envío</label>
              <div className="flex gap-2">
                <input
                  type="date"
                  value={sendAt}
                  onChange={(e) => setSendAt(e.target.value)}
                  className="flex-1 border-b border-emerald-600/30 focus:border-emerald-600 text-sm font-medium py-2 outline-none bg-transparent"
                />
                <input
                  type="time"
                  value={sendTime}
                  onChange={(e) => setSendTime(e.target.value)}
                  className="border-b border-emerald-600/30 focus:border-emerald-600 text-sm font-medium py-2 outline-none bg-transparent"
                />
              </div>
            </div>
          </div>

          <div className="flex flex-wrap gap-6">
            <label className="flex items-center gap-2 cursor-pointer">
              <input
                type="checkbox"
                checked={requiresReadConfirmation}
                onChange={(e) => setRequiresReadConfirmation(e.target.checked)}
                className="rounded border-emerald-600/30 text-emerald-600 focus:ring-emerald-600"
              />
              <span className="text-sm">Requiere confirmación de lectura</span>
            </label>
            <label className="flex items-center gap-2 cursor-pointer">
              <input
                type="checkbox"
                checked={publishToBulletinBoard}
                onChange={(e) => setPublishToBulletinBoard(e.target.checked)}
                className="rounded border-emerald-600/30 text-emerald-600 focus:ring-emerald-600"
              />
              <span className="text-sm">Publicar también en cartelera digital</span>
            </label>
          </div>
        </CardContent>
      </Card>

      <div className="flex items-center gap-3 justify-end">
        <Button variant="secondary" onClick={() => router.back()}>
          Cancelar
        </Button>
        <Button variant="secondary" onClick={handleSaveDraft} disabled={saving || !subject}>
          <Save className="w-4 h-4 mr-2" />
          {saving ? 'Guardando...' : 'Guardar Borrador'}
        </Button>
        <Button onClick={handleSendNow} disabled={sending || !subject}>
          {sendAt && sendTime ? (
            <>
              <Clock className="w-4 h-4 mr-2" />
              Programar Envío
            </>
          ) : (
            <>
              <Send className="w-4 h-4 mr-2" />
              {sending ? 'Enviando...' : 'Enviar Ahora'}
            </>
          )}
        </Button>
      </div>
    </div>
  );
}
