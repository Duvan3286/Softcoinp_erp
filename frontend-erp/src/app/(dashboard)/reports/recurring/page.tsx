'use client';

import React, { useState, useEffect } from 'react';
import { Loader2, Plus, PauseCircle, PlayCircle, X, Save } from 'lucide-react';
import { Button } from '@/components/ui/Button';
import { Card, CardContent } from '@/components/ui/Card';
import reportService, { RecurringReportConfig, ReportCatalogItem, CreateRecurringReportConfigRequest } from '@/lib/report-service';
import axios from 'axios';

const frequencyLabels: Record<string, string> = {
  Diario: 'Diario',
  Semanal: 'Semanal',
  Mensual: 'Mensual',
  Trimestral: 'Trimestral',
  Anual: 'Anual',
};

const statusBadgeClass: Record<string, string> = {
  Active: 'badge-success',
  Paused: 'badge-warning',
  Disabled: 'badge-danger',
};

const statusLabels: Record<string, string> = {
  Active: 'Activo',
  Paused: 'En pausa',
  Disabled: 'Desactivado',
};

export default function RecurringPage() {
  const [configs, setConfigs] = useState<RecurringReportConfig[]>([]);
  const [catalog, setCatalog] = useState<ReportCatalogItem[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');
  const [success, setSuccess] = useState('');

  const [showForm, setShowForm] = useState(false);
  const [reportTypeId, setReportTypeId] = useState('');
  const [name, setName] = useState('');
  const [frequency, setFrequency] = useState('Mensual');
  const [format, setFormat] = useState('PDF');
  const [recipientEmails, setRecipientEmails] = useState('');
  const [subjectTemplate, setSubjectTemplate] = useState('');
  const [saving, setSaving] = useState(false);

  useEffect(() => {
    fetchData();
  }, []);

  const fetchData = async () => {
    setLoading(true);
    setError('');
    try {
      const [configsData, catalogData] = await Promise.all([
        reportService.getRecurringConfigs(),
        reportService.getCatalog(),
      ]);
      setConfigs(configsData);
      setCatalog(catalogData);
    } catch {
      setError('Error al cargar datos.');
    } finally {
      setLoading(false);
    }
  };

  const resetForm = () => {
    setReportTypeId('');
    setName('');
    setFrequency('Mensual');
    setFormat('PDF');
    setRecipientEmails('');
    setSubjectTemplate('');
  };

  const handleCreate = async () => {
    setSaving(true);
    setError('');
    try {
      const emails = recipientEmails
        .split(',')
        .map((e) => e.trim())
        .filter(Boolean);

      const data: CreateRecurringReportConfigRequest = {
        reportTypeId,
        name,
        frequency,
        format,
        recipientEmails: emails,
        subjectTemplate: subjectTemplate || undefined,
      };

      await reportService.createRecurringConfig(data);
      setSuccess('Configuración recurrente creada exitosamente.');
      resetForm();
      setShowForm(false);
      fetchData();
    } catch (err) {
      if (axios.isAxiosError(err) && err.response?.data?.message) {
        setError(err.response.data.message);
      } else {
        setError('Error al crear configuración.');
      }
    } finally {
      setSaving(false);
    }
  };

  const handlePauseResume = async (config: RecurringReportConfig) => {
    try {
      if (config.status === 'Active') {
        await reportService.pauseRecurringConfig(config.id);
        setSuccess('Configuración pausada.');
      } else {
        await reportService.resumeRecurringConfig(config.id);
        setSuccess('Configuración reanudada.');
      }
      fetchData();
    } catch (err) {
      if (axios.isAxiosError(err) && err.response?.data?.message) {
        setError(err.response.data.message);
      } else {
        setError('Error al cambiar estado.');
      }
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
      <div className="flex items-center justify-between">
        <h1 className="text-2xl font-bold text-foreground">Reportes Recurrentes</h1>
        <Button onClick={() => { resetForm(); setShowForm(true); }}>
          <Plus className="w-4 h-4 mr-2" />
          Nueva Configuración
        </Button>
      </div>

      {error && (
        <div className="p-4 bg-red-50 dark:bg-red-950/30 border border-red-200 dark:border-red-900 rounded-xl text-red-700 dark:text-red-400 text-sm">
          {error}
        </div>
      )}

      {success && (
        <div className="p-4 bg-emerald-50 dark:bg-emerald-950/30 border border-emerald-200 dark:border-emerald-900 rounded-xl text-emerald-700 dark:text-emerald-400 text-sm">
          {success}
        </div>
      )}

      {showForm && (
        <Card>
          <CardContent className="p-6 space-y-4">
            <div className="flex items-center justify-between">
              <h2 className="text-lg font-semibold text-foreground">Nueva Configuración Recurrente</h2>
              <button
                onClick={() => setShowForm(false)}
                className="p-1 rounded-lg hover:bg-muted transition-colors"
              >
                <X className="w-5 h-5 text-muted-foreground" />
              </button>
            </div>

            <div>
              <label className="block text-sm font-medium text-foreground mb-1">Tipo de Reporte</label>
              <select
                value={reportTypeId}
                onChange={(e) => setReportTypeId(e.target.value)}
                className="w-full border-b border-emerald-600/30 focus:border-emerald-600 text-sm font-medium py-2 outline-none bg-background"
              >
                <option value="">Seleccionar reporte</option>
                {catalog.map((r) => (
                  <option key={r.code} value={r.code}>{r.name}</option>
                ))}
              </select>
            </div>

            <div>
              <label className="block text-sm font-medium text-foreground mb-1">Nombre</label>
              <input
                type="text"
                value={name}
                onChange={(e) => setName(e.target.value)}
                className="w-full border-b border-emerald-600/30 focus:border-emerald-600 text-sm font-medium py-2 outline-none bg-transparent"
              />
            </div>

            <div className="grid grid-cols-2 gap-4">
              <div>
                <label className="block text-sm font-medium text-foreground mb-1">Frecuencia</label>
                <select
                  value={frequency}
                  onChange={(e) => setFrequency(e.target.value)}
                  className="w-full border-b border-emerald-600/30 focus:border-emerald-600 text-sm font-medium py-2 outline-none bg-background"
                >
                  {Object.entries(frequencyLabels).map(([key, label]) => (
                    <option key={key} value={key}>{label}</option>
                  ))}
                </select>
              </div>
              <div>
                <label className="block text-sm font-medium text-foreground mb-1">Formato</label>
                <select
                  value={format}
                  onChange={(e) => setFormat(e.target.value)}
                  className="w-full border-b border-emerald-600/30 focus:border-emerald-600 text-sm font-medium py-2 outline-none bg-background"
                >
                  <option value="PDF">PDF</option>
                  <option value="Excel">Excel</option>
                </select>
              </div>
            </div>

            <div>
              <label className="block text-sm font-medium text-foreground mb-1">
                Correos destinatarios (separados por coma)
              </label>
              <textarea
                value={recipientEmails}
                onChange={(e) => setRecipientEmails(e.target.value)}
                className="w-full border border-emerald-600/30 focus:border-emerald-600 text-sm font-medium py-2 outline-none rounded-lg px-3 bg-background"
                rows={2}
                placeholder="correo1@ejemplo.com, correo2@ejemplo.com"
              />
            </div>

            <div>
              <label className="block text-sm font-medium text-foreground mb-1">Asunto del correo (opcional)</label>
              <input
                type="text"
                value={subjectTemplate}
                onChange={(e) => setSubjectTemplate(e.target.value)}
                className="w-full border-b border-emerald-600/30 focus:border-emerald-600 text-sm font-medium py-2 outline-none bg-transparent"
                placeholder="Reporte {{nombre}} - {{periodo}}"
              />
            </div>

            <div className="flex gap-2">
              <Button onClick={handleCreate} disabled={saving || !name || !reportTypeId}>
                <Save className="w-4 h-4 mr-1" />
                {saving ? 'Guardando...' : 'Crear Configuración'}
              </Button>
              <Button variant="secondary" onClick={() => setShowForm(false)}>
                Cancelar
              </Button>
            </div>
          </CardContent>
        </Card>
      )}

      {configs.length === 0 ? (
        <div className="text-center py-12 text-muted-foreground">
          <p>No hay configuraciones recurrentes.</p>
        </div>
      ) : (
        <div className="space-y-3">
          {configs.map((config) => (
            <Card key={config.id}>
              <CardContent className="p-4">
                <div className="flex items-start justify-between">
                  <div className="flex-1 min-w-0">
                    <div className="flex items-center gap-2 mb-1">
                      <span className={`badge ${statusBadgeClass[config.status] || 'badge-neutral'}`}>
                        {statusLabels[config.status] || config.status}
                      </span>
                      <span className="badge badge-neutral">{frequencyLabels[config.frequency] || config.frequency}</span>
                    </div>
                    <h3 className="font-semibold text-foreground">{config.name}</h3>
                    <p className="text-xs text-muted-foreground mt-1">
                      Reporte: {config.reportTypeName}
                      {config.nextExecutionAt && (
                        <> · Próxima ejecución: {new Date(config.nextExecutionAt).toLocaleDateString('es-CO')}</>
                      )}
                    </p>
                    {config.recipientEmails.length > 0 && (
                      <p className="text-xs text-muted-foreground mt-1">
                        Destinatarios: {config.recipientEmails.join(', ')}
                      </p>
                    )}
                  </div>
                  <div className="flex items-center gap-2 ml-4">
                    {config.status === 'Active' ? (
                      <button
                        onClick={() => handlePauseResume(config)}
                        className="p-2 rounded-lg hover:bg-amber-50 dark:hover:bg-amber-950/20 transition-colors"
                        title="Pausar"
                      >
                        <PauseCircle className="w-5 h-5 text-amber-600" />
                      </button>
                    ) : (
                      <button
                        onClick={() => handlePauseResume(config)}
                        className="p-2 rounded-lg hover:bg-emerald-50 dark:hover:bg-emerald-950/20 transition-colors"
                        title="Reanudar"
                      >
                        <PlayCircle className="w-5 h-5 text-emerald-600" />
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
