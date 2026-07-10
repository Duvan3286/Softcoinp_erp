'use client';

import React, { useState } from 'react';
import { useRouter } from 'next/navigation';
import { Loader2, ArrowLeft, Save, AlertTriangle, Calendar } from 'lucide-react';
import { Button } from '@/components/ui/Button';
import { Card, CardContent } from '@/components/ui/Card';
import assemblyService from '@/lib/assembly-service';

const assemblyTypes = [
  { value: 'Ordinaria', label: 'Ordinaria' },
  { value: 'Extraordinaria', label: 'Extraordinaria' },
];

const participationTypes = [
  { value: 'Presencial', label: 'Presencial' },
  { value: 'Remota', label: 'Remota' },
  { value: 'Hibrida', label: 'Híbrida' },
];

export default function NewAssemblyPage() {
  const router = useRouter();
  const [submitting, setSubmitting] = useState(false);
  const [error, setError] = useState('');

  const [title, setTitle] = useState('');
  const [description, setDescription] = useState('');
  const [type, setType] = useState('Ordinaria');
  const [participationType, setParticipationType] = useState('Presencial');
  const [scheduledDate, setScheduledDate] = useState('');
  const [scheduledTime, setScheduledTime] = useState('');
  const [location, setLocation] = useState('');
  const [secondConvocationDate, setSecondConvocationDate] = useState('');
  const [secondConvocationTime, setSecondConvocationTime] = useState('');
  const [secondConvocationLocation, setSecondConvocationLocation] = useState('');

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setError('');

    if (!title.trim()) {
      setError('El título es requerido.');
      return;
    }
    if (!scheduledDate) {
      setError('La fecha de la asamblea es requerida.');
      return;
    }
    if (!scheduledTime) {
      setError('La hora de la asamblea es requerida.');
      return;
    }
    if (!location.trim()) {
      setError('El lugar es requerido.');
      return;
    }

    setSubmitting(true);
    try {
      const result = await assemblyService.createAssembly({
        title: title.trim(),
        description: description.trim() || undefined,
        type,
        participationType,
        scheduledDate,
        scheduledTime,
        location: location.trim(),
        secondConvocationDate: secondConvocationDate || undefined,
        secondConvocationTime: secondConvocationTime || undefined,
        secondConvocationLocation: secondConvocationLocation.trim() || undefined,
      });
      router.push(`/assembly/${result.id}`);
    } catch (err: any) {
      setError(err?.response?.data?.error || 'Error al crear la asamblea.');
    } finally {
      setSubmitting(false);
    }
  };

  return (
    <main className="p-8">
        <div className="max-w-3xl mx-auto space-y-6">
          <button
            onClick={() => router.back()}
            className="inline-flex items-center gap-2 text-sm text-muted-foreground hover:text-foreground transition-colors"
          >
            <ArrowLeft className="w-4 h-4" /> Volver
          </button>

          <div>
            <h1 className="text-2xl font-bold text-foreground tracking-tight">
              Nueva Asamblea de Copropietarios
            </h1>
            <p className="text-sm text-muted-foreground mt-1">
              Registra una nueva asamblea de copropietarios.
            </p>
          </div>

          <Card>
            <CardContent className="p-6">
              <form onSubmit={handleSubmit} className="space-y-6">
                <div>
                  <h3 className="text-sm font-bold text-foreground mb-4 flex items-center gap-2">
                    <Calendar className="w-4 h-4 text-emerald-600" /> Información General
                  </h3>
                  <div className="grid grid-cols-1 md:grid-cols-2 gap-5">
                    <div className="md:col-span-2">
                      <label className="block text-xs font-bold text-muted-foreground uppercase tracking-widest mb-1.5">
                        Título *
                      </label>
                      <input
                        type="text"
                        placeholder="Título de la asamblea"
                        value={title}
                        onChange={(e) => setTitle(e.target.value.slice(0, 300))}
                        maxLength={300}
                        required
                        className="w-full bg-transparent border-b border-emerald-600/30 focus:border-emerald-600 text-sm font-medium py-2 outline-none"
                      />
                    </div>
                    <div>
                      <label className="block text-xs font-bold text-muted-foreground uppercase tracking-widest mb-1.5">
                        Tipo de Asamblea
                      </label>
                      <select
                        value={type}
                        onChange={(e) => setType(e.target.value)}
                        className="w-full bg-transparent border-b border-emerald-600/30 focus:border-emerald-600 text-sm font-medium py-2 outline-none"
                      >
                        {assemblyTypes.map((t) => (
                          <option key={t.value} value={t.value}>
                            {t.label}
                          </option>
                        ))}
                      </select>
                    </div>
                    <div>
                      <label className="block text-xs font-bold text-muted-foreground uppercase tracking-widest mb-1.5">
                        Modalidad de Participación
                      </label>
                      <select
                        value={participationType}
                        onChange={(e) => setParticipationType(e.target.value)}
                        className="w-full bg-transparent border-b border-emerald-600/30 focus:border-emerald-600 text-sm font-medium py-2 outline-none"
                      >
                        {participationTypes.map((p) => (
                          <option key={p.value} value={p.value}>
                            {p.label}
                          </option>
                        ))}
                      </select>
                    </div>
                    <div className="md:col-span-2">
                      <label className="block text-xs font-bold text-muted-foreground uppercase tracking-widest mb-1.5">
                        Descripción
                      </label>
                      <textarea
                        placeholder="Descripción de la asamblea (opcional)"
                        value={description}
                        onChange={(e) => setDescription(e.target.value.slice(0, 2000))}
                        rows={3}
                        maxLength={2000}
                        className="w-full bg-muted border border-border focus:border-emerald-600 rounded-md text-sm p-3 outline-none resize-none"
                      />
                    </div>
                  </div>
                </div>

                <div className="border-t border-border pt-6">
                  <h3 className="text-sm font-bold text-foreground mb-4">Fecha y Lugar</h3>
                  <div className="grid grid-cols-1 md:grid-cols-3 gap-5">
                    <div>
                      <label className="block text-xs font-bold text-muted-foreground uppercase tracking-widest mb-1.5">
                        Fecha de la Asamblea *
                      </label>
                      <input
                        type="date"
                        value={scheduledDate}
                        onChange={(e) => setScheduledDate(e.target.value)}
                        required
                        className="w-full bg-transparent border-b border-emerald-600/30 focus:border-emerald-600 text-sm font-medium py-2 outline-none"
                      />
                    </div>
                    <div>
                      <label className="block text-xs font-bold text-muted-foreground uppercase tracking-widest mb-1.5">
                        Hora *
                      </label>
                      <input
                        type="time"
                        value={scheduledTime}
                        onChange={(e) => setScheduledTime(e.target.value)}
                        required
                        className="w-full bg-transparent border-b border-emerald-600/30 focus:border-emerald-600 text-sm font-medium py-2 outline-none"
                      />
                    </div>
                    <div>
                      <label className="block text-xs font-bold text-muted-foreground uppercase tracking-widest mb-1.5">
                        Lugar *
                      </label>
                      <input
                        type="text"
                        placeholder="Lugar de la asamblea"
                        value={location}
                        onChange={(e) => setLocation(e.target.value.slice(0, 300))}
                        maxLength={300}
                        required
                        className="w-full bg-transparent border-b border-emerald-600/30 focus:border-emerald-600 text-sm font-medium py-2 outline-none"
                      />
                    </div>
                  </div>
                </div>

                <div className="border-t border-border pt-6">
                  <h3 className="text-sm font-bold text-foreground mb-4">Segunda Convocatoria</h3>
                  <div className="grid grid-cols-1 md:grid-cols-3 gap-5">
                    <div>
                      <label className="block text-xs font-bold text-muted-foreground uppercase tracking-widest mb-1.5">
                        Fecha Segunda Convocatoria
                      </label>
                      <input
                        type="date"
                        value={secondConvocationDate}
                        onChange={(e) => setSecondConvocationDate(e.target.value)}
                        className="w-full bg-transparent border-b border-emerald-600/30 focus:border-emerald-600 text-sm font-medium py-2 outline-none"
                      />
                    </div>
                    <div>
                      <label className="block text-xs font-bold text-muted-foreground uppercase tracking-widest mb-1.5">
                        Hora Segunda Convocatoria
                      </label>
                      <input
                        type="time"
                        value={secondConvocationTime}
                        onChange={(e) => setSecondConvocationTime(e.target.value)}
                        className="w-full bg-transparent border-b border-emerald-600/30 focus:border-emerald-600 text-sm font-medium py-2 outline-none"
                      />
                    </div>
                    <div>
                      <label className="block text-xs font-bold text-muted-foreground uppercase tracking-widest mb-1.5">
                        Lugar Segunda Convocatoria
                      </label>
                      <input
                        type="text"
                        placeholder="Lugar (opcional)"
                        value={secondConvocationLocation}
                        onChange={(e) => setSecondConvocationLocation(e.target.value.slice(0, 300))}
                        maxLength={300}
                        className="w-full bg-transparent border-b border-emerald-600/30 focus:border-emerald-600 text-sm font-medium py-2 outline-none"
                      />
                    </div>
                  </div>
                </div>

                {error && (
                  <div className="p-3 bg-rose-50 border border-rose-200 rounded-lg text-rose-700 text-xs flex items-center gap-2">
                    <AlertTriangle className="w-4 h-4 shrink-0" /> {error}
                  </div>
                )}

                <div className="flex justify-end gap-3 pt-4 border-t border-border">
                  <Button type="button" variant="ghost" onClick={() => router.back()}>
                    Cancelar
                  </Button>
                  <Button type="submit" disabled={submitting}>
                    {submitting ? (
                      <Loader2 className="w-4 h-4 animate-spin mr-2" />
                    ) : (
                      <Save className="w-4 h-4 mr-2" />
                    )}
                    Crear Asamblea
                  </Button>
                </div>
              </form>
            </CardContent>
          </Card>
        </div>
    </main>
  );
}
