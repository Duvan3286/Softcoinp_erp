'use client';

import React, { useState } from 'react';
import { useRouter } from 'next/navigation';
import { Loader2, ArrowLeft, Save, AlertTriangle, Building2, Wrench, Shield, MapPin } from 'lucide-react';
import { Button } from '@/components/ui/Button';
import { Card, CardContent } from '@/components/ui/Card';
import maintenanceService, { CreateCommonAssetRequest } from '@/lib/maintenance-service';

const categories = [
  { value: 'Structure', label: 'Estructura' },
  { value: 'ElectricalEquipment', label: 'Equipos Eléctricos' },
  { value: 'HydraulicEquipment', label: 'Equipos Hidráulicos' },
  { value: 'SafetyEquipment', label: 'Equipos de Seguridad' },
  { value: 'RecreationalAreas', label: 'Zonas Recreativas' },
  { value: 'GreenAreas', label: 'Zonas Verdes' },
];

export default function NewAssetPage() {
  const router = useRouter();
  const [submitting, setSubmitting] = useState(false);
  const [error, setError] = useState('');
  const [createdId, setCreatedId] = useState<string | null>(null);

  const [name, setName] = useState('');
  const [category, setCategory] = useState('');
  const [location, setLocation] = useState('');
  const [isEssential, setIsEssential] = useState(false);
  const [brand, setBrand] = useState('');
  const [model, setModel] = useState('');
  const [serialNumber, setSerialNumber] = useState('');
  const [acquisitionDate, setAcquisitionDate] = useState('');
  const [acquisitionValue, setAcquisitionValue] = useState('');
  const [estimatedUsefulLifeMonths, setEstimatedUsefulLifeMonths] = useState('');
  const [manufacturer, setManufacturer] = useState('');
  const [hasWarranty, setHasWarranty] = useState(false);
  const [warrantyEndDate, setWarrantyEndDate] = useState('');
  const [statusNotes, setStatusNotes] = useState('');

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setError('');

    if (!name.trim()) { setError('El nombre del bien es requerido.'); return; }
    if (!category) { setError('La categoría es requerida.'); return; }
    if (!location.trim()) { setError('La ubicación es requerida.'); return; }

    setSubmitting(true);
    try {
      const request: CreateCommonAssetRequest = {
        name: name.trim(),
        category,
        location: location.trim(),
        isEssential,
        brand: brand || undefined,
        model: model || undefined,
        serialNumber: serialNumber || undefined,
        acquisitionDate: acquisitionDate || undefined,
        acquisitionValue: acquisitionValue ? parseFloat(acquisitionValue) : undefined,
        estimatedUsefulLifeMonths: estimatedUsefulLifeMonths ? parseInt(estimatedUsefulLifeMonths) : undefined,
        manufacturer: manufacturer || undefined,
        hasWarranty,
        warrantyEndDate: warrantyEndDate || undefined,
        statusNotes: statusNotes || undefined,
      };
      const result = await maintenanceService.createAsset(request);
      setCreatedId(result.id);
    } catch (err: any) {
      setError(err?.response?.data?.error || 'Error al crear el bien.');
    } finally {
      setSubmitting(false);
    }
  };

  if (createdId) {
    return (
      <div className="space-y-6 max-w-2xl mx-auto">
        <button onClick={() => router.push('/maintenance')} className="inline-flex items-center gap-2 text-sm text-muted-foreground hover:text-foreground transition-colors">
          <ArrowLeft className="w-4 h-4" /> Volver al Inventario
        </button>
        <Card>
          <CardContent className="p-6 text-center">
            <div className="w-16 h-16 bg-emerald-50 rounded-full flex items-center justify-center mx-auto mb-4">
              <Wrench className="w-8 h-8 text-emerald-600" />
            </div>
            <h2 className="text-xl font-bold text-foreground">Bien Creado Exitosamente</h2>
            <p className="text-sm text-muted-foreground mt-2">El bien ha sido registrado en el inventario.</p>
            <div className="mt-6 flex justify-center gap-3">
              <Button variant="secondary" onClick={() => router.push('/maintenance')}>Volver al Inventario</Button>
              <Button onClick={() => router.push(`/maintenance/${createdId}`)}>Ver Detalle</Button>
            </div>
          </CardContent>
        </Card>
      </div>
    );
  }

  return (
    <div className="space-y-6 max-w-3xl mx-auto">
      <button onClick={() => router.push('/maintenance')} className="inline-flex items-center gap-2 text-sm text-muted-foreground hover:text-foreground transition-colors">
        <ArrowLeft className="w-4 h-4" /> Volver al Inventario
      </button>

      <div>
        <h1 className="text-2xl font-bold text-foreground tracking-tight">Nuevo Bien Común</h1>
        <p className="text-sm text-muted-foreground mt-1">Registra un nuevo bien común en el inventario del conjunto.</p>
      </div>

      <Card>
        <CardContent className="p-6">
          <form onSubmit={handleSubmit} className="space-y-6">
            <div>
              <h3 className="text-sm font-bold text-foreground mb-4 flex items-center gap-2">
                <Building2 className="w-4 h-4 text-emerald-600" /> Información General
              </h3>
              <div className="grid grid-cols-1 md:grid-cols-2 gap-5">
                <div className="md:col-span-2">
                  <label className="block text-xs font-bold text-muted-foreground uppercase tracking-widest mb-1.5">Nombre del Bien *</label>
                  <input type="text" placeholder="Ej: Ascensor Torre A" value={name}
                    onChange={(e) => setName(e.target.value)} maxLength={300} required
                    className="w-full bg-transparent border-b border-emerald-600/30 focus:border-emerald-600 text-sm font-medium py-2 outline-none" />
                </div>
                <div>
                  <label className="block text-xs font-bold text-muted-foreground uppercase tracking-widest mb-1.5">Categoría *</label>
                  <select value={category} onChange={(e) => setCategory(e.target.value)} required
                    className="w-full bg-transparent border-b border-emerald-600/30 focus:border-emerald-600 text-sm font-medium py-2 outline-none">
                    <option value="">Seleccione...</option>
                    {categories.map((c) => <option key={c.value} value={c.value}>{c.label}</option>)}
                  </select>
                </div>
                <div>
                  <label className="block text-xs font-bold text-muted-foreground uppercase tracking-widest mb-1.5">Ubicación *</label>
                  <input type="text" placeholder="Ej: Piso 1, Zona Común" value={location}
                    onChange={(e) => setLocation(e.target.value)} maxLength={300} required
                    className="w-full bg-transparent border-b border-emerald-600/30 focus:border-emerald-600 text-sm font-medium py-2 outline-none" />
                </div>
                <div>
                  <label className="block text-xs font-bold text-muted-foreground uppercase tracking-widest mb-1.5">Bien Esencial</label>
                  <label className="flex items-center gap-2 text-sm text-foreground cursor-pointer mt-2">
                    <input type="checkbox" checked={isEssential} onChange={(e) => setIsEssential(e.target.checked)} className="accent-emerald-600 w-4 h-4" />
                    Marcar como esencial
                  </label>
                </div>
              </div>
            </div>

            <div className="border-t border-border pt-6">
              <h3 className="text-sm font-bold text-foreground mb-4 flex items-center gap-2">
                <Wrench className="w-4 h-4 text-emerald-600" /> Especificaciones Técnicas
              </h3>
              <div className="grid grid-cols-1 md:grid-cols-3 gap-5">
                <div>
                  <label className="block text-xs font-bold text-muted-foreground uppercase tracking-widest mb-1.5">Marca</label>
                  <input type="text" placeholder="Marca" value={brand}
                    onChange={(e) => setBrand(e.target.value)} maxLength={150}
                    className="w-full bg-transparent border-b border-emerald-600/30 focus:border-emerald-600 text-sm font-medium py-2 outline-none" />
                </div>
                <div>
                  <label className="block text-xs font-bold text-muted-foreground uppercase tracking-widest mb-1.5">Modelo</label>
                  <input type="text" placeholder="Modelo" value={model}
                    onChange={(e) => setModel(e.target.value)} maxLength={150}
                    className="w-full bg-transparent border-b border-emerald-600/30 focus:border-emerald-600 text-sm font-medium py-2 outline-none" />
                </div>
                <div>
                  <label className="block text-xs font-bold text-muted-foreground uppercase tracking-widest mb-1.5">Nro. Serie</label>
                  <input type="text" placeholder="Número de serie" value={serialNumber}
                    onChange={(e) => setSerialNumber(e.target.value)} maxLength={100}
                    className="w-full bg-transparent border-b border-emerald-600/30 focus:border-emerald-600 text-sm font-medium py-2 outline-none" />
                </div>
                <div>
                  <label className="block text-xs font-bold text-muted-foreground uppercase tracking-widest mb-1.5">Fabricante</label>
                  <input type="text" placeholder="Fabricante" value={manufacturer}
                    onChange={(e) => setManufacturer(e.target.value)} maxLength={200}
                    className="w-full bg-transparent border-b border-emerald-600/30 focus:border-emerald-600 text-sm font-medium py-2 outline-none" />
                </div>
                <div>
                  <label className="block text-xs font-bold text-muted-foreground uppercase tracking-widest mb-1.5">Fecha Adquisición</label>
                  <input type="date" value={acquisitionDate}
                    onChange={(e) => setAcquisitionDate(e.target.value)}
                    className="w-full bg-transparent border-b border-emerald-600/30 focus:border-emerald-600 text-sm font-medium py-2 outline-none" />
                </div>
                <div>
                  <label className="block text-xs font-bold text-muted-foreground uppercase tracking-widest mb-1.5">Valor Adquisición</label>
                  <input type="number" placeholder="Valor en COP" value={acquisitionValue}
                    onChange={(e) => setAcquisitionValue(e.target.value)} min="0"
                    className="w-full bg-transparent border-b border-emerald-600/30 focus:border-emerald-600 text-sm font-medium py-2 outline-none" />
                </div>
                <div>
                  <label className="block text-xs font-bold text-muted-foreground uppercase tracking-widest mb-1.5">Vida Útil (meses)</label>
                  <input type="number" placeholder="Meses" value={estimatedUsefulLifeMonths}
                    onChange={(e) => setEstimatedUsefulLifeMonths(e.target.value)} min="0"
                    className="w-full bg-transparent border-b border-emerald-600/30 focus:border-emerald-600 text-sm font-medium py-2 outline-none" />
                </div>
              </div>
            </div>

            <div className="border-t border-border pt-6">
              <h3 className="text-sm font-bold text-foreground mb-4 flex items-center gap-2">
                <Shield className="w-4 h-4 text-emerald-600" /> Garantía
              </h3>
              <div className="grid grid-cols-1 md:grid-cols-2 gap-5">
                <div>
                  <label className="block text-xs font-bold text-muted-foreground uppercase tracking-widest mb-1.5">Tiene Garantía</label>
                  <label className="flex items-center gap-2 text-sm text-foreground cursor-pointer mt-2">
                    <input type="checkbox" checked={hasWarranty} onChange={(e) => setHasWarranty(e.target.checked)} className="accent-emerald-600 w-4 h-4" />
                    Garantía vigente
                  </label>
                </div>
                {hasWarranty && (
                  <div>
                    <label className="block text-xs font-bold text-muted-foreground uppercase tracking-widest mb-1.5">Fecha Fin Garantía</label>
                    <input type="date" value={warrantyEndDate}
                      onChange={(e) => setWarrantyEndDate(e.target.value)}
                      className="w-full bg-transparent border-b border-emerald-600/30 focus:border-emerald-600 text-sm font-medium py-2 outline-none" />
                  </div>
                )}
              </div>
            </div>

            <div className="border-t border-border pt-6">
              <div>
                <label className="block text-xs font-bold text-muted-foreground uppercase tracking-widest mb-1.5">Notas de Estado</label>
                <textarea value={statusNotes} onChange={(e) => setStatusNotes(e.target.value)} rows={3}
                  placeholder="Observaciones sobre el estado actual del bien..."
                  className="w-full bg-slate-50 dark:bg-slate-900 border border-border focus:border-emerald-600 rounded-md text-sm p-3 outline-none resize-none" />
              </div>
            </div>

            {error && (
              <div className="p-3 bg-rose-50 border border-rose-200 rounded-lg text-rose-700 text-xs flex items-center gap-2">
                <AlertTriangle className="w-4 h-4 shrink-0" /> {error}
              </div>
            )}

            <div className="flex justify-between items-center pt-4 border-t border-border">
              <Button type="button" variant="ghost" onClick={() => router.push('/maintenance')}>Cancelar</Button>
              <Button type="submit" disabled={submitting}>
                {submitting ? <Loader2 className="w-4 h-4 animate-spin mr-2" /> : <Save className="w-4 h-4 mr-2" />}
                Crear Bien
              </Button>
            </div>
          </form>
        </CardContent>
      </Card>
    </div>
  );
}
