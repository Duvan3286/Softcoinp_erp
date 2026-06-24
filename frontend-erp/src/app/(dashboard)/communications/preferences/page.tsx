'use client';

import React, { useState, useEffect } from 'react';
import { Loader2, Search, Save } from 'lucide-react';
import { Button } from '@/components/ui/Button';
import { Card, CardContent } from '@/components/ui/Card';
import communicationService, { CommunicationPreference } from '@/lib/communication-service';
import axios from 'axios';

export default function PreferencesPage() {
  const [preferences, setPreferences] = useState<CommunicationPreference[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');
  const [searchTerm, setSearchTerm] = useState('');
  const [editingId, setEditingId] = useState<string | null>(null);

  // Edit form state
  const [allowEmail, setAllowEmail] = useState(true);
  const [allowSms, setAllowSms] = useState(true);
  const [allowPush, setAllowPush] = useState(true);
  const [criticalOverride, setCriticalOverride] = useState(true);
  const [notes, setNotes] = useState('');
  const [saving, setSaving] = useState(false);

  useEffect(() => {
    fetchPreferences();
  }, []);

  const fetchPreferences = async () => {
    setLoading(true);
    setError('');
    try {
      const data = await communicationService.getAllPreferences();
      setPreferences(data);
    } catch {
      setError('Error al cargar preferencias.');
    } finally {
      setLoading(false);
    }
  };

  const handleEdit = (pref: CommunicationPreference) => {
    setAllowEmail(pref.allowEmail);
    setAllowSms(pref.allowSms);
    setAllowPush(pref.allowPush);
    setCriticalOverride(pref.criticalNotificationsOverride);
    setNotes(pref.notes || '');
    setEditingId(pref.id);
  };

  const handleSave = async () => {
    if (!editingId) return;
    setSaving(true);
    setError('');
    try {
      const pref = preferences.find((p) => p.id === editingId);
      if (!pref) return;

      if (pref.ownerId) {
        await communicationService.updateOwnerPreferences(pref.ownerId, {
          allowEmail,
          allowSms,
          allowPush,
          criticalNotificationsOverride: criticalOverride,
          notes,
        });
      } else if (pref.tenantResidentId) {
        await communicationService.updateTenantPreferences(pref.tenantResidentId, {
          allowEmail,
          allowSms,
          allowPush,
          criticalNotificationsOverride: criticalOverride,
          notes,
        });
      }

      setEditingId(null);
      fetchPreferences();
    } catch (err) {
      if (axios.isAxiosError(err) && err.response?.data?.message) {
        setError(err.response.data.message);
      } else {
        setError('Error al guardar preferencias.');
      }
    } finally {
      setSaving(false);
    }
  };

  const filtered = preferences.filter(
    (p) =>
      (p.ownerName || p.tenantResidentName || '')
        .toLowerCase()
        .includes(searchTerm.toLowerCase())
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
      <h1 className="text-2xl font-bold text-foreground">Preferencias de Comunicación</h1>

      {error && (
        <div className="p-4 bg-red-50 dark:bg-red-950/30 border border-red-200 dark:border-red-900 rounded-xl text-red-700 dark:text-red-400 text-sm">
          {error}
        </div>
      )}

      <div className="flex items-center gap-2 bg-muted rounded-lg px-3 py-2 max-w-sm">
        <Search className="w-4 h-4 text-muted-foreground" />
        <input
          type="text"
          placeholder="Buscar residente..."
          value={searchTerm}
          onChange={(e) => setSearchTerm(e.target.value)}
          className="bg-transparent border-none outline-none text-sm flex-1"
        />
      </div>

      {filtered.length === 0 ? (
        <div className="text-center py-12 text-muted-foreground">
          <p>No hay preferencias registradas. Las preferencias se crean automáticamente cuando un residente las configura.</p>
        </div>
      ) : (
        <div className="space-y-3">
          {filtered.map((pref) => (
            <Card key={pref.id}>
              <CardContent className="p-4">
                {editingId === pref.id ? (
                  <div className="space-y-4">
                    <h3 className="font-semibold text-foreground">
                      {pref.ownerName || pref.tenantResidentName}
                    </h3>
                    <div className="grid grid-cols-2 md:grid-cols-4 gap-4">
                      <label className="flex items-center gap-2 cursor-pointer">
                        <input
                          type="checkbox"
                          checked={allowEmail}
                          onChange={(e) => setAllowEmail(e.target.checked)}
                          className="rounded border-emerald-600/30 text-emerald-600"
                        />
                        <span className="text-sm">Correo electrónico</span>
                      </label>
                      <label className="flex items-center gap-2 cursor-pointer">
                        <input
                          type="checkbox"
                          checked={allowSms}
                          onChange={(e) => setAllowSms(e.target.checked)}
                          className="rounded border-emerald-600/30 text-emerald-600"
                        />
                        <span className="text-sm">SMS</span>
                      </label>
                      <label className="flex items-center gap-2 cursor-pointer">
                        <input
                          type="checkbox"
                          checked={allowPush}
                          onChange={(e) => setAllowPush(e.target.checked)}
                          className="rounded border-emerald-600/30 text-emerald-600"
                        />
                        <span className="text-sm">Notificación Push</span>
                      </label>
                      <label className="flex items-center gap-2 cursor-pointer">
                        <input
                          type="checkbox"
                          checked={criticalOverride}
                          onChange={(e) => setCriticalOverride(e.target.checked)}
                          className="rounded border-emerald-600/30 text-emerald-600"
                        />
                        <span className="text-sm">Recibir notificaciones críticas</span>
                      </label>
                    </div>
                    <div>
                      <label className="block text-sm font-medium text-foreground mb-1">Notas</label>
                      <textarea
                        value={notes}
                        onChange={(e) => setNotes(e.target.value)}
                        className="w-full border border-emerald-600/30 focus:border-emerald-600 text-sm font-medium py-2 outline-none rounded-lg px-3 bg-background"
                        rows={2}
                      />
                    </div>
                    <div className="flex gap-2">
                      <Button onClick={handleSave} disabled={saving}>
                        <Save className="w-4 h-4 mr-2" />
                        {saving ? 'Guardando...' : 'Guardar'}
                      </Button>
                      <Button variant="secondary" onClick={() => setEditingId(null)}>
                        Cancelar
                      </Button>
                    </div>
                  </div>
                ) : (
                  <div className="flex items-start justify-between">
                    <div>
                      <h3 className="font-semibold text-foreground">
                        {pref.ownerName || pref.tenantResidentName || 'Sin nombre'}
                      </h3>
                      <div className="flex flex-wrap gap-2 mt-2">
                        <span className={`badge ${pref.allowEmail ? 'badge-success' : 'badge-danger'}`}>
                          {pref.allowEmail ? 'Email OK' : 'Email No'}
                        </span>
                        <span className={`badge ${pref.allowSms ? 'badge-success' : 'badge-danger'}`}>
                          {pref.allowSms ? 'SMS OK' : 'SMS No'}
                        </span>
                        <span className={`badge ${pref.allowPush ? 'badge-success' : 'badge-danger'}`}>
                          {pref.allowPush ? 'Push OK' : 'Push No'}
                        </span>
                        <span className="badge badge-neutral">
                          Críticas: {pref.criticalNotificationsOverride ? 'Sí' : 'No'}
                        </span>
                      </div>
                      {pref.unsubscribedEventTypes.length > 0 && (
                        <p className="text-xs text-muted-foreground mt-1">
                          Desuscrito de: {pref.unsubscribedEventTypes.join(', ')}
                        </p>
                      )}
                      {pref.notes && (
                        <p className="text-xs text-muted-foreground mt-1">{pref.notes}</p>
                      )}
                    </div>
                    <button
                      onClick={() => handleEdit(pref)}
                      className="px-3 py-1.5 text-sm font-medium text-emerald-600 hover:bg-emerald-50 dark:hover:bg-emerald-950/20 rounded-lg transition-colors"
                    >
                      Editar
                    </button>
                  </div>
                )}
              </CardContent>
            </Card>
          ))}
        </div>
      )}
    </div>
  );
}
