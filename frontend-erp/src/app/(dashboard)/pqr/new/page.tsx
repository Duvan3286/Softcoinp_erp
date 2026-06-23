'use client';

import React, { useState, useEffect } from 'react';
import { useRouter } from 'next/navigation';
import { Loader2, ArrowLeft, Save, AlertTriangle, Send, Building2 } from 'lucide-react';
import { Button } from '@/components/ui/Button';
import { Card, CardContent } from '@/components/ui/Card';
import pqrService, { CreatePqrRequest } from '@/lib/pqr-service';
import { UnitsService as unitsService, Unit } from '@/lib/units-service';

const pqrTypes = [
  { value: 'Request', label: 'Petición' },
  { value: 'Complaint', label: 'Queja' },
  { value: 'Claim', label: 'Reclamo' },
];

const categories = [
  { value: 'Billing', label: 'Facturación' },
  { value: 'Maintenance', label: 'Mantenimiento' },
  { value: 'Coexistence', label: 'Convivencia' },
  { value: 'CommonAreas', label: 'Zonas Comunes' },
  { value: 'Administration', label: 'Administración' },
  { value: 'Other', label: 'Otro' },
];

const channels = [
  { value: 'InPerson', label: 'Presencial' },
  { value: 'Email', label: 'Correo Electrónico' },
  { value: 'Phone', label: 'Teléfono' },
  { value: 'Web', label: 'Portal Web' },
  { value: 'WhatsApp', label: 'WhatsApp' },
  { value: 'Letter', label: 'Carta Física' },
  { value: 'Other', label: 'Otro' },
];

const documentTypes = [
  { value: 'CitizenshipCard', label: 'Cédula de Ciudadanía' },
  { value: 'ForeignerID', label: 'Cédula de Extranjería' },
  { value: 'NIT', label: 'NIT' },
  { value: 'Passport', label: 'Pasaporte' },
  { value: 'Other', label: 'Otro' },
];

export default function NewPqrPage() {
  const router = useRouter();
  const [units, setUnits] = useState<Unit[]>([]);
  const [loadingUnits, setLoadingUnits] = useState(true);
  const [submitting, setSubmitting] = useState(false);
  const [error, setError] = useState('');
  const [createdId, setCreatedId] = useState<string | null>(null);

  const [pqrType, setPqrType] = useState('Request');
  const [category, setCategory] = useState('');
  const [subject, setSubject] = useState('');
  const [description, setDescription] = useState('');
  const [unitId, setUnitId] = useState('');
  const [radiadorName, setRadiadorName] = useState('');
  const [radiadorDocumentType, setRadiadorDocumentType] = useState('');
  const [radiadorDocumentNumber, setRadiadorDocumentNumber] = useState('');
  const [radiadorContact, setRadiadorContact] = useState('');
  const [channel, setChannel] = useState('InPerson');
  const [isInternal, setIsInternal] = useState(false);
  const [involvedResidentName, setInvolvedResidentName] = useState('');
  const [involvedResidentUnitId, setInvolvedResidentUnitId] = useState('');
  const [isLinkedToCharge, setIsLinkedToCharge] = useState(false);

  useEffect(() => {
    const fetchUnits = async () => {
      try {
        const data = await unitsService.getUnits();
        setUnits(data);
      } catch {
        setError('Error al cargar las unidades.');
      } finally {
        setLoadingUnits(false);
      }
    };
    fetchUnits();
  }, []);

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setError('');

    if (!category) { setError('Debe seleccionar una categoría.'); return; }
    if (!subject.trim()) { setError('El asunto es requerido.'); return; }
    if (!description.trim()) { setError('La descripción es requerida.'); return; }
    if (!unitId) { setError('Debe seleccionar una unidad.'); return; }
    if (!radiadorName.trim()) { setError('El nombre del radicador es requerido.'); return; }

    const numericDocTypes = ['CitizenshipCard', 'ForeignerID', 'NIT'];
    if (radiadorDocumentNumber && numericDocTypes.includes(radiadorDocumentType)) {
      if (!/^\d+$/.test(radiadorDocumentNumber)) {
        setError('El número de documento debe contener solo dígitos para el tipo seleccionado.');
        return;
      }
    }

    setSubmitting(true);
    try {
      const request: CreatePqrRequest = {
        pqrType,
        category,
        subject,
        description,
        unitId,
        radiadorName,
        radiadorDocumentType: radiadorDocumentType || undefined,
        radiadorDocumentNumber: radiadorDocumentNumber || undefined,
        radiadorContact: radiadorContact || undefined,
        channel,
        isInternal,
        involvedResidentName: involvedResidentName || undefined,
        involvedResidentUnitId: involvedResidentUnitId || undefined,
        isLinkedToCharge,
      };
      const result = await pqrService.createPqr(request);
      setCreatedId(result.id);
    } catch (err: any) {
      setError(err?.response?.data || 'Error al radicar la PQR.');
    } finally {
      setSubmitting(false);
    }
  };

  const handleDocNumberChange = (val: string, docType?: string) => {
    const dt = docType || radiadorDocumentType;
    const numericDocTypes = ['CitizenshipCard', 'ForeignerID', 'NIT'];
    if (numericDocTypes.includes(dt)) {
      setRadiadorDocumentNumber(val.replace(/\D/g, '').slice(0, 20));
    } else {
      setRadiadorDocumentNumber(val.replace(/[^a-zA-Z0-9]/g, '').slice(0, 20));
    }
  };

  const handleDocTypeChange = (val: string) => {
    setRadiadorDocumentType(val);
    if (radiadorDocumentNumber) {
      handleDocNumberChange(radiadorDocumentNumber, val);
    }
  };

  const handleNameChange = (val: string) => {
    setRadiadorName(val.toLowerCase().replace(/(?:^|\s)\S/g, (a) => a.toUpperCase()).slice(0, 200));
  };

  const handleContactChange = (val: string) => {
    setRadiadorContact(val.replace(/[^a-zA-Z0-9@._\-\+\s]/g, '').slice(0, 200));
  };

  if (loadingUnits) {
    return <div className="flex justify-center py-20"><Loader2 className="w-8 h-8 animate-spin text-emerald-600" /></div>;
  }

  if (createdId) {
    return (
      <div className="space-y-6 max-w-2xl mx-auto">
        <button onClick={() => router.push('/pqr')} className="inline-flex items-center gap-2 text-sm text-muted-foreground hover:text-foreground transition-colors">
          <ArrowLeft className="w-4 h-4" /> Volver a Bandeja PQR
        </button>
        <Card>
          <CardContent className="p-6 text-center">
            <div className="w-16 h-16 bg-emerald-50 rounded-full flex items-center justify-center mx-auto mb-4">
              <Send className="w-8 h-8 text-emerald-600" />
            </div>
            <h2 className="text-xl font-bold text-foreground">PQR Radicada Exitosamente</h2>
            <p className="text-sm text-muted-foreground mt-2">La PQR ha sido radicada y está en proceso de revisión.</p>
            <div className="mt-6 flex justify-center gap-3">
              <Button variant="secondary" onClick={() => router.push('/pqr')}>Volver a Bandeja</Button>
              <Button onClick={() => router.push(`/pqr/${createdId}`)}>Ver Detalle de PQR</Button>
            </div>
          </CardContent>
        </Card>
      </div>
    );
  }

  return (
    <div className="space-y-6 max-w-3xl mx-auto">
      <button onClick={() => router.push('/pqr')} className="inline-flex items-center gap-2 text-sm text-muted-foreground hover:text-foreground transition-colors">
        <ArrowLeft className="w-4 h-4" /> Volver a Bandeja PQR
      </button>

      <div>
        <h1 className="text-2xl font-bold text-foreground tracking-tight">Radicar Nueva PQR</h1>
        <p className="text-sm text-muted-foreground mt-1">Registra una nueva Petición, Queja o Reclamo.</p>
      </div>

      <Card>
        <CardContent className="p-6">
          <form onSubmit={handleSubmit} className="space-y-6">
            <div>
              <h3 className="text-sm font-bold text-foreground mb-4 flex items-center gap-2">
                <Building2 className="w-4 h-4 text-emerald-600" /> Información de Clasificación
              </h3>
              <div className="grid grid-cols-1 md:grid-cols-3 gap-5">
                <div>
                  <label className="block text-xs font-bold text-muted-foreground uppercase tracking-widest mb-1.5">Tipo de PQR</label>
                  <select value={pqrType} onChange={(e) => setPqrType(e.target.value)}
                    className="w-full bg-transparent border-b border-emerald-600/30 focus:border-emerald-600 text-sm font-medium py-2 outline-none">
                    {pqrTypes.map((t) => <option key={t.value} value={t.value}>{t.label}</option>)}
                  </select>
                </div>
                <div>
                  <label className="block text-xs font-bold text-muted-foreground uppercase tracking-widest mb-1.5">Categoría</label>
                  <select value={category} onChange={(e) => setCategory(e.target.value)}
                    className="w-full bg-transparent border-b border-emerald-600/30 focus:border-emerald-600 text-sm font-medium py-2 outline-none">
                    <option value="">Seleccione una categoría...</option>
                    {categories.map((c) => <option key={c.value} value={c.value}>{c.label}</option>)}
                  </select>
                </div>
                <div>
                  <label className="block text-xs font-bold text-muted-foreground uppercase tracking-widest mb-1.5">Canal de Recepción</label>
                  <select value={channel} onChange={(e) => setChannel(e.target.value)}
                    className="w-full bg-transparent border-b border-emerald-600/30 focus:border-emerald-600 text-sm font-medium py-2 outline-none">
                    {channels.map((c) => <option key={c.value} value={c.value}>{c.label}</option>)}
                  </select>
                </div>
              </div>
            </div>

            <div className="border-t border-border pt-6">
              <h3 className="text-sm font-bold text-foreground mb-4 flex items-center gap-2">
                <Send className="w-4 h-4 text-emerald-600" /> Información del Radicador
              </h3>
              <div className="grid grid-cols-1 md:grid-cols-2 gap-5">
                <div className="md:col-span-2">
                  <label className="block text-xs font-bold text-muted-foreground uppercase tracking-widest mb-1.5">Nombre Completo</label>
                  <input type="text" placeholder="Nombre de quien radica" value={radiadorName} onChange={(e) => handleNameChange(e.target.value)}
                    maxLength={200} required
                    className="w-full bg-transparent border-b border-emerald-600/30 focus:border-emerald-600 text-sm font-medium py-2 outline-none" />
                </div>
                <div>
                  <label className="block text-xs font-bold text-muted-foreground uppercase tracking-widest mb-1.5">Tipo de Documento</label>
                  <select value={radiadorDocumentType} onChange={(e) => handleDocTypeChange(e.target.value)}
                    className="w-full bg-transparent border-b border-emerald-600/30 focus:border-emerald-600 text-sm font-medium py-2 outline-none">
                    <option value="">Seleccione...</option>
                    {documentTypes.map((d) => <option key={d.value} value={d.value}>{d.label}</option>)}
                  </select>
                </div>
                <div>
                  <label className="block text-xs font-bold text-muted-foreground uppercase tracking-widest mb-1.5">Número de Documento</label>
                  <input type="text" placeholder="Número de documento" value={radiadorDocumentNumber} onChange={(e) => handleDocNumberChange(e.target.value)}
                    maxLength={20}
                    className="w-full bg-transparent border-b border-emerald-600/30 focus:border-emerald-600 text-sm font-medium py-2 outline-none" />
                </div>
                <div className="md:col-span-2">
                  <label className="block text-xs font-bold text-muted-foreground uppercase tracking-widest mb-1.5">Contacto (Teléfono / Email)</label>
                  <input type="text" placeholder="Teléfono o correo electrónico" value={radiadorContact} onChange={(e) => handleContactChange(e.target.value)}
                    maxLength={200}
                    className="w-full bg-transparent border-b border-emerald-600/30 focus:border-emerald-600 text-sm font-medium py-2 outline-none" />
                </div>
              </div>
            </div>

            <div className="border-t border-border pt-6">
              <h3 className="text-sm font-bold text-foreground mb-4 flex items-center gap-2">
                <Building2 className="w-4 h-4 text-emerald-600" /> Asignación de Unidad
              </h3>
              <div className="grid grid-cols-1 md:grid-cols-2 gap-5">
                <div>
                  <label className="block text-xs font-bold text-muted-foreground uppercase tracking-widest mb-1.5">Unidad</label>
                  <select value={unitId} onChange={(e) => setUnitId(e.target.value)}
                    className="w-full bg-transparent border-b border-emerald-600/30 focus:border-emerald-600 text-sm font-medium py-2 outline-none" required>
                    <option value="">Seleccione una unidad...</option>
                    {units.map((u) => <option key={u.id} value={u.id}>{u.identifier} - {u.towerOrBlock}</option>)}
                  </select>
                </div>
                <div>
                  <label className="block text-xs font-bold text-muted-foreground uppercase tracking-widest mb-1.5">PQR Interna</label>
                  <div className="flex items-center gap-3 mt-2">
                    <label className="flex items-center gap-2 text-sm text-foreground cursor-pointer">
                      <input type="radio" name="isInternal" checked={!isInternal} onChange={() => setIsInternal(false)}
                        className="accent-emerald-600" /> Externa
                    </label>
                    <label className="flex items-center gap-2 text-sm text-foreground cursor-pointer">
                      <input type="radio" name="isInternal" checked={isInternal} onChange={() => setIsInternal(true)}
                        className="accent-emerald-600" /> Interna
                    </label>
                  </div>
                </div>
              </div>
            </div>

            <div className="border-t border-border pt-6">
              <h3 className="text-sm font-bold text-foreground mb-4">Detalle</h3>
              <div className="grid grid-cols-1 gap-5">
                <div>
                  <label className="block text-xs font-bold text-muted-foreground uppercase tracking-widest mb-1.5">Asunto</label>
                  <input type="text" placeholder="Asunto de la PQR" value={subject} onChange={(e) => setSubject(e.target.value)}
                    maxLength={500} required
                    className="w-full bg-transparent border-b border-emerald-600/30 focus:border-emerald-600 text-sm font-medium py-2 outline-none" />
                </div>
                <div>
                  <label className="block text-xs font-bold text-muted-foreground uppercase tracking-widest mb-1.5">Descripción</label>
                  <textarea placeholder="Describa en detalle su petición, queja o reclamo..." value={description}
                    onChange={(e) => setDescription(e.target.value)} rows={5}
                    maxLength={4000} required
                    className="w-full bg-slate-50 dark:bg-slate-900 border border-border focus:border-emerald-600 rounded-md text-sm p-3 outline-none resize-none" />
                </div>
              </div>
            </div>

            {isLinkedToCharge && (
              <div className="border-t border-border pt-6">
                <h3 className="text-sm font-bold text-foreground mb-4">Residente Involucrado</h3>
                <div className="grid grid-cols-1 md:grid-cols-2 gap-5">
                  <div>
                    <label className="block text-xs font-bold text-muted-foreground uppercase tracking-widest mb-1.5">Nombre del Residente</label>
                    <input type="text" placeholder="Nombre" value={involvedResidentName} onChange={(e) => setInvolvedResidentName(e.target.value)}
                      maxLength={200}
                      className="w-full bg-transparent border-b border-emerald-600/30 focus:border-emerald-600 text-sm font-medium py-2 outline-none" />
                  </div>
                  <div>
                    <label className="block text-xs font-bold text-muted-foreground uppercase tracking-widest mb-1.5">Unidad del Residente</label>
                    <select value={involvedResidentUnitId} onChange={(e) => setInvolvedResidentUnitId(e.target.value)}
                      className="w-full bg-transparent border-b border-emerald-600/30 focus:border-emerald-600 text-sm font-medium py-2 outline-none">
                      <option value="">Seleccione...</option>
                      {units.map((u) => <option key={u.id} value={u.id}>{u.identifier}</option>)}
                    </select>
                  </div>
                </div>
              </div>
            )}

            {error && (
              <div className="p-3 bg-rose-50 border border-rose-200 rounded-lg text-rose-700 text-xs flex items-center gap-2">
                <AlertTriangle className="w-4 h-4 shrink-0" /> {error}
              </div>
            )}

            <div className="flex justify-between items-center pt-4 border-t border-border">
              <label className="flex items-center gap-2 text-sm text-muted-foreground cursor-pointer">
                <input type="checkbox" checked={isLinkedToCharge} onChange={(e) => setIsLinkedToCharge(e.target.checked)}
                  className="accent-emerald-600 w-4 h-4" />
                Vincular a un cobro (multa / cargo)
              </label>
              <div className="flex gap-3">
                <Button type="button" variant="ghost" onClick={() => router.push('/pqr')}>Cancelar</Button>
                <Button type="submit" disabled={submitting}>
                  {submitting ? <Loader2 className="w-4 h-4 animate-spin mr-2" /> : <Save className="w-4 h-4 mr-2" />}
                  Radicar PQR
                </Button>
              </div>
            </div>
          </form>
        </CardContent>
      </Card>
    </div>
  );
}
