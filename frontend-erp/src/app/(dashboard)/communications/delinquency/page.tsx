'use client';

import React, { useState, useEffect } from 'react';
import { Loader2, Save, Play, Pause, Trash2, Search } from 'lucide-react';
import { Button } from '@/components/ui/Button';
import { Card, CardContent } from '@/components/ui/Card';
import communicationService, {
  DelinquencySequenceConfig,
  DelinquencySequencePause,
  NotificationTemplate,
} from '@/lib/communication-service';
import axios from 'axios';

const stepLabels: Record<number, string> = {
  1: 'Primer Aviso',
  2: 'Segundo Aviso',
  3: 'Tercer Aviso',
  4: 'Aviso Prejurídico',
};

export default function DelinquencyPage() {
  const [configs, setConfigs] = useState<DelinquencySequenceConfig[]>([]);
  const [templates, setTemplates] = useState<NotificationTemplate[]>([]);
  const [pauses, setPauses] = useState<DelinquencySequencePause[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');
  const [processing, setProcessing] = useState(false);
  const [processResults, setProcessResults] = useState<string[]>([]);

  // Config edit state
  const [editingStep, setEditingStep] = useState<number | null>(null);
  const [editDays, setEditDays] = useState(0);
  const [editTemplateId, setEditTemplateId] = useState('');
  const [editActive, setEditActive] = useState(true);

  // Pause form
  const [showPauseForm, setShowPauseForm] = useState(false);
  const [pauseUnitIdentifier, setPauseUnitIdentifier] = useState('');
  const [pauseReason, setPauseReason] = useState('');
  const [pauseStartDate, setPauseStartDate] = useState('');
  const [pauseEndDate, setPauseEndDate] = useState('');
  const [pauseSaving, setPauseSaving] = useState(false);
  const [searchTerm, setSearchTerm] = useState('');

  useEffect(() => {
    fetchData();
  }, []);

  const fetchData = async () => {
    setLoading(true);
    setError('');
    try {
      const [configsData, templatesData, pausesData] = await Promise.all([
        communicationService.getDelinquencyConfig(),
        communicationService.getTemplates(),
        communicationService.getActiveDelinquencyPauses(),
      ]);
      setConfigs(configsData);
      setTemplates(templatesData);
      setPauses(pausesData);
    } catch {
      setError('Error al cargar datos.');
    } finally {
      setLoading(false);
    }
  };

  const handleEditConfig = (config: DelinquencySequenceConfig) => {
    setEditingStep(config.stepNumber);
    setEditDays(config.daysAfterDue);
    setEditTemplateId(config.templateId);
    setEditActive(config.isActive);
  };

  const handleSaveConfig = async () => {
    if (!editingStep) return;
    setError('');
    try {
      await communicationService.updateDelinquencyConfig(editingStep, {
        daysAfterDue: editDays,
        templateId: editTemplateId,
        isActive: editActive,
      });
      setEditingStep(null);
      fetchData();
    } catch (err) {
      if (axios.isAxiosError(err) && err.response?.data?.message) {
        setError(err.response.data.message);
      } else {
        setError('Error al guardar configuración.');
      }
    }
  };

  const handleRunProcess = async () => {
    setProcessing(true);
    setError('');
    setProcessResults([]);
    try {
      const results = await communicationService.runDelinquencyProcess();
      setProcessResults(results);
    } catch (err) {
      if (axios.isAxiosError(err) && err.response?.data?.message) {
        setError(err.response.data.message);
      } else {
        setError('Error al ejecutar proceso.');
      }
    } finally {
      setProcessing(false);
    }
  };

  const handleCreatePause = async () => {
    setPauseSaving(true);
    setError('');
    try {
      await communicationService.createDelinquencyPause({
        unitId: pauseUnitIdentifier,
        reason: pauseReason,
        startDate: pauseStartDate || new Date().toISOString().split('T')[0],
        endDate: pauseEndDate || null,
      });
      setShowPauseForm(false);
      setPauseUnitIdentifier('');
      setPauseReason('');
      setPauseStartDate('');
      setPauseEndDate('');
      fetchData();
    } catch (err) {
      if (axios.isAxiosError(err) && err.response?.data?.message) {
        setError(err.response.data.message);
      } else {
        setError('Error al crear pausa.');
      }
    } finally {
      setPauseSaving(false);
    }
  };

  const handleRemovePause = async (id: string) => {
    if (!confirm('¿Eliminar esta pausa?')) return;
    try {
      await communicationService.removeDelinquencyPause(id);
      fetchData();
    } catch {
      setError('Error al eliminar pausa.');
    }
  };

  const filteredPauses = pauses.filter(
    (p) =>
      p.unitIdentifier.toLowerCase().includes(searchTerm.toLowerCase()) ||
      p.reason.toLowerCase().includes(searchTerm.toLowerCase())
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
      <h1 className="text-2xl font-bold text-foreground">Secuencia de Avisos de Mora</h1>

      {error && (
        <div className="p-4 bg-red-50 dark:bg-red-950/30 border border-red-200 dark:border-red-900 rounded-xl text-red-700 dark:text-red-400 text-sm">
          {error}
        </div>
      )}

      <div className="grid grid-cols-1 lg:grid-cols-2 gap-6">
        <div className="space-y-4">
          <h2 className="text-lg font-semibold text-foreground">Configuración de Pasos</h2>
          {[1, 2, 3, 4].map((step) => {
            const config = configs.find((c) => c.stepNumber === step);
            const isEditing = editingStep === step;

            return (
              <Card key={step}>
                <CardContent className="p-4">
                  <div className="flex items-start justify-between">
                    <div className="flex-1">
                      <h3 className="font-semibold text-foreground">{stepLabels[step]}</h3>
                      {isEditing ? (
                        <div className="space-y-3 mt-3">
                          <div>
                            <label className="block text-xs font-medium text-muted-foreground mb-1">
                              Días después de vencimiento
                            </label>
                            <input
                              type="number"
                              value={editDays}
                              onChange={(e) => setEditDays(parseInt(e.target.value) || 0)}
                              className="w-full border-b border-emerald-600/30 focus:border-emerald-600 text-sm font-medium py-1 outline-none bg-transparent"
                              min={0}
                            />
                          </div>
                          <div>
                            <label className="block text-xs font-medium text-muted-foreground mb-1">
                              Plantilla
                            </label>
                            <select
                              value={editTemplateId}
                              onChange={(e) => setEditTemplateId(e.target.value)}
                              className="w-full border-b border-emerald-600/30 focus:border-emerald-600 text-sm font-medium py-1 outline-none bg-background"
                            >
                              <option value="">Seleccionar plantilla</option>
                              {templates
                                .filter((t) => t.eventType.includes('Delinquency') || t.eventType.includes('PreLegal'))
                                .map((t) => (
                                  <option key={t.id} value={t.id}>{t.name}</option>
                                ))}
                            </select>
                          </div>
                          <label className="flex items-center gap-2 cursor-pointer">
                            <input
                              type="checkbox"
                              checked={editActive}
                              onChange={(e) => setEditActive(e.target.checked)}
                              className="rounded border-emerald-600/30 text-emerald-600"
                            />
                            <span className="text-sm">Activo</span>
                          </label>
                          <div className="flex gap-2">
                            <Button onClick={handleSaveConfig}>
                              <Save className="w-3 h-3 mr-1" />
                              Guardar
                            </Button>
                            <Button variant="secondary" onClick={() => setEditingStep(null)}>
                              Cancelar
                            </Button>
                          </div>
                        </div>
                      ) : (
                        <div className="mt-2 text-sm text-muted-foreground space-y-1">
                          <p>Envío: {config ? `Día ${config.daysAfterDue}` : 'No configurado'}</p>
                          <p>
                            Plantilla: {config ? config.templateName : '—'}
                          </p>
                          <p>
                            Estado:{' '}
                            {config?.isActive ? (
                              <span className="badge badge-success">Activo</span>
                            ) : (
                              <span className="badge badge-danger">Inactivo</span>
                            )}
                          </p>
                        </div>
                      )}
                    </div>
                    {!isEditing && (
                      <button
                        onClick={() => config && handleEditConfig(config)}
                        className="px-3 py-1 text-sm font-medium text-emerald-600 hover:bg-emerald-50 dark:hover:bg-emerald-950/20 rounded-lg transition-colors"
                      >
                        Configurar
                      </button>
                    )}
                  </div>
                </CardContent>
              </Card>
            );
          })}

          <Button onClick={handleRunProcess} disabled={processing}>
            <Play className="w-4 h-4 mr-2" />
            {processing ? 'Procesando...' : 'Ejecutar Proceso de Mora'}
          </Button>

          {processResults.length > 0 && (
            <Card>
              <CardContent className="p-4">
                <h3 className="font-semibold text-foreground mb-2">Resultados</h3>
                <ul className="text-sm space-y-1">
                  {processResults.map((r, i) => (
                    <li key={i} className="text-muted-foreground">{r}</li>
                  ))}
                </ul>
              </CardContent>
            </Card>
          )}
        </div>

        <div className="space-y-4">
          <div className="flex items-center justify-between">
            <h2 className="text-lg font-semibold text-foreground">Pausas Activas</h2>
            <Button variant="secondary" onClick={() => setShowPauseForm(!showPauseForm)}>
              <Pause className="w-4 h-4 mr-1" />
              {showPauseForm ? 'Cancelar' : 'Nueva Pausa'}
            </Button>
          </div>

          {showPauseForm && (
            <Card>
              <CardContent className="p-4 space-y-3">
                <h3 className="font-semibold text-foreground">Registrar Pausa</h3>
                <div>
                  <label className="block text-xs font-medium text-muted-foreground mb-1">ID de Unidad</label>
                  <input
                    type="text"
                    value={pauseUnitIdentifier}
                    onChange={(e) => setPauseUnitIdentifier(e.target.value)}
                    className="w-full border-b border-emerald-600/30 focus:border-emerald-600 text-sm font-medium py-1 outline-none bg-transparent"
                    placeholder="GUID de la unidad"
                  />
                </div>
                <div>
                  <label className="block text-xs font-medium text-muted-foreground mb-1">Motivo</label>
                  <textarea
                    value={pauseReason}
                    onChange={(e) => setPauseReason(e.target.value)}
                    className="w-full border border-emerald-600/30 focus:border-emerald-600 text-sm font-medium py-2 outline-none rounded-lg px-3 bg-background"
                    rows={2}
                    placeholder="Ej: Acuerdo de pago vigente"
                  />
                </div>
                <div className="grid grid-cols-2 gap-3">
                  <div>
                    <label className="block text-xs font-medium text-muted-foreground mb-1">Inicio</label>
                    <input
                      type="date"
                      value={pauseStartDate}
                      onChange={(e) => setPauseStartDate(e.target.value)}
                      className="w-full border-b border-emerald-600/30 focus:border-emerald-600 text-sm font-medium py-1 outline-none bg-transparent"
                    />
                  </div>
                  <div>
                    <label className="block text-xs font-medium text-muted-foreground mb-1">Fin (opcional)</label>
                    <input
                      type="date"
                      value={pauseEndDate}
                      onChange={(e) => setPauseEndDate(e.target.value)}
                      className="w-full border-b border-emerald-600/30 focus:border-emerald-600 text-sm font-medium py-1 outline-none bg-transparent"
                    />
                  </div>
                </div>
                <Button onClick={handleCreatePause} disabled={pauseSaving || !pauseUnitIdentifier}>
                  {pauseSaving ? 'Guardando...' : 'Registrar Pausa'}
                </Button>
              </CardContent>
            </Card>
          )}

          <div className="flex items-center gap-2 bg-muted rounded-lg px-3 py-2">
            <Search className="w-4 h-4 text-muted-foreground" />
            <input
              type="text"
              placeholder="Buscar por unidad o motivo..."
              value={searchTerm}
              onChange={(e) => setSearchTerm(e.target.value)}
              className="bg-transparent border-none outline-none text-sm flex-1"
            />
          </div>

          {filteredPauses.length === 0 ? (
            <p className="text-muted-foreground text-sm">No hay pausas activas.</p>
          ) : (
            filteredPauses.map((pause) => (
              <Card key={pause.id}>
                <CardContent className="p-4">
                  <div className="flex items-start justify-between">
                    <div>
                      <h3 className="font-semibold text-foreground">Unidad {pause.unitIdentifier}</h3>
                      <p className="text-sm text-muted-foreground mt-1">{pause.reason}</p>
                      <p className="text-xs text-muted-foreground mt-1">
                        {new Date(pause.startDate).toLocaleDateString('es-CO')}
                        {pause.endDate
                          ? ` — ${new Date(pause.endDate).toLocaleDateString('es-CO')}`
                          : ' — Indefinido'}
                      </p>
                    </div>
                    <button
                      onClick={() => handleRemovePause(pause.id)}
                      className="p-1.5 rounded-lg hover:bg-red-50 dark:hover:bg-red-950/20 transition-colors"
                      title="Eliminar pausa"
                    >
                      <Trash2 className="w-4 h-4 text-red-500" />
                    </button>
                  </div>
                </CardContent>
              </Card>
            ))
          )}
        </div>
      </div>
    </div>
  );
}
