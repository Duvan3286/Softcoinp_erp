'use client';

import React, { useState } from 'react';
import { useRouter } from 'next/navigation';
import { Loader2, ArrowLeft, Save, AlertTriangle, User, Building2, Send } from 'lucide-react';
import { Button } from '@/components/ui/Button';
import { Card, CardContent } from '@/components/ui/Card';
import supplierService, { CreateProviderRequest } from '@/lib/supplier-service';

const providerTypes = [
  { value: 'Natural', label: 'Persona Natural' },
  { value: 'Legal', label: 'Persona Jurídica' },
];

const documentTypes = [
  { value: 'CitizenshipCard', label: 'Cédula de Ciudadanía' },
  { value: 'ForeignerID', label: 'Cédula de Extranjería' },
  { value: 'NIT', label: 'NIT' },
  { value: 'Passport', label: 'Pasaporte' },
  { value: 'Other', label: 'Otro' },
];

const serviceTypes = [
  { value: 'Mantenimiento', label: 'Mantenimiento' },
  { value: 'Aseo', label: 'Aseo' },
  { value: 'Seguridad', label: 'Seguridad' },
  { value: 'Jardinería', label: 'Jardinería' },
  { value: 'Electricidad', label: 'Electricidad' },
  { value: 'Plomería', label: 'Plomería' },
  { value: 'Pintura', label: 'Pintura' },
  { value: 'Construcción', label: 'Construcción' },
  { value: 'Servicios Generales', label: 'Servicios Generales' },
  { value: 'Otros', label: 'Otros' },
];

const sanitizeDocNumber = (val: string, docType: string): string => {
  const isNumeric = docType === 'CitizenshipCard' || docType === 'NIT';
  const maxLen = isNumeric ? 10 : 50;
  return isNumeric
    ? val.replace(/\D/g, '').slice(0, maxLen)
    : val.replace(/[^a-zA-Z0-9]/g, '').slice(0, maxLen);
};

const sanitizePhone = (val: string): string =>
  val.replace(/[^0-9\-\+\s]/g, '').slice(0, 20);

export default function NewSupplierPage() {
  const router = useRouter();
  const [submitting, setSubmitting] = useState(false);
  const [error, setError] = useState('');
  const [createdId, setCreatedId] = useState<string | null>(null);

  const [providerType, setProviderType] = useState('Natural');
  const [documentType, setDocumentType] = useState('');
  const [documentNumber, setDocumentNumber] = useState('');
  const [businessName, setBusinessName] = useState('');
  const [contactName, setContactName] = useState('');
  const [email, setEmail] = useState('');
  const [phone, setPhone] = useState('');
  const [address, setAddress] = useState('');
  const [serviceType, setServiceType] = useState('');

  const handleDocTypeChange = (val: string) => {
    setDocumentType(val);
    setDocumentNumber('');
  };

  const handleDocNumberChange = (val: string) => {
    const cleaned = sanitizeDocNumber(val, documentType);
    setDocumentNumber(cleaned);
  };

  const validateEmail = (emailVal: string): boolean => {
    if (!emailVal) return true;
    return /^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(emailVal);
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setError('');

    if (!documentType) { setError('Debe seleccionar un tipo de documento.'); return; }
    if (!documentNumber.trim()) { setError('El número de documento es requerido.'); return; }
    if (!businessName.trim()) { setError('El nombre o razón social es requerido.'); return; }

    if (!validateEmail(email)) {
      setError('El correo electrónico no tiene un formato válido.');
      return;
    }

    setSubmitting(true);
    try {
      const request: CreateProviderRequest = {
        providerType,
        documentType,
        documentNumber: documentNumber.trim(),
        businessName: businessName.trim(),
        contactName: contactName || undefined,
        email: email || undefined,
        phone: phone || undefined,
        address: address || undefined,
        serviceType: serviceType || undefined,
      };
      const result = await supplierService.createProvider(request);
      setCreatedId(result.id);
    } catch (err: any) {
      setError(err?.response?.data?.error || 'Error al crear el proveedor.');
    } finally {
      setSubmitting(false);
    }
  };

  if (createdId) {
    return (
      <div className="space-y-6 max-w-2xl mx-auto">
        <button onClick={() => router.push('/suppliers')} className="inline-flex items-center gap-2 text-sm text-muted-foreground hover:text-foreground transition-colors">
          <ArrowLeft className="w-4 h-4" /> Volver a Proveedores
        </button>
        <Card>
          <CardContent className="p-6 text-center">
            <div className="w-16 h-16 bg-emerald-50 rounded-full flex items-center justify-center mx-auto mb-4">
              <Send className="w-8 h-8 text-emerald-600" />
            </div>
            <h2 className="text-xl font-bold text-foreground">Proveedor Creado Exitosamente</h2>
            <p className="text-sm text-muted-foreground mt-2">El proveedor ha sido registrado en el sistema.</p>
            <div className="mt-6 flex justify-center gap-3">
              <Button variant="secondary" onClick={() => router.push('/suppliers')}>Volver a Proveedores</Button>
              <Button onClick={() => router.push(`/suppliers/${createdId}`)}>Ver Detalle</Button>
            </div>
          </CardContent>
        </Card>
      </div>
    );
  }

  return (
    <div className="space-y-6 max-w-3xl mx-auto">
      <button onClick={() => router.push('/suppliers')} className="inline-flex items-center gap-2 text-sm text-muted-foreground hover:text-foreground transition-colors">
        <ArrowLeft className="w-4 h-4" /> Volver a Proveedores
      </button>

      <div>
        <h1 className="text-2xl font-bold text-foreground tracking-tight">Nuevo Proveedor</h1>
        <p className="text-sm text-muted-foreground mt-1">Registra un nuevo proveedor o contratista.</p>
      </div>

      <Card>
        <CardContent className="p-6">
          <form onSubmit={handleSubmit} className="space-y-6">
            <div>
              <h3 className="text-sm font-bold text-foreground mb-4 flex items-center gap-2">
                <Building2 className="w-4 h-4 text-emerald-600" /> Información del Proveedor
              </h3>
              <div className="grid grid-cols-1 md:grid-cols-3 gap-5">
                <div>
                  <label className="block text-xs font-bold text-muted-foreground uppercase tracking-widest mb-1.5">Tipo de Proveedor</label>
                  <select value={providerType} onChange={(e) => setProviderType(e.target.value)}
                    className="w-full bg-transparent border-b border-emerald-600/30 focus:border-emerald-600 text-sm font-medium py-2 outline-none">
                    {providerTypes.map((t) => <option key={t.value} value={t.value}>{t.label}</option>)}
                  </select>
                </div>
                <div>
                  <label className="block text-xs font-bold text-muted-foreground uppercase tracking-widest mb-1.5">Tipo de Documento *</label>
                  <select value={documentType} onChange={(e) => handleDocTypeChange(e.target.value)}
                    className="w-full bg-transparent border-b border-emerald-600/30 focus:border-emerald-600 text-sm font-medium py-2 outline-none" required>
                    <option value="">Seleccione...</option>
                    {documentTypes.map((d) => <option key={d.value} value={d.value}>{d.label}</option>)}
                  </select>
                </div>
                <div>
                  <label className="block text-xs font-bold text-muted-foreground uppercase tracking-widest mb-1.5">Nro. Documento *</label>
                  <input type="text" placeholder="Número de documento"
                    value={documentNumber} onChange={(e) => handleDocNumberChange(e.target.value)}
                    maxLength={documentType === 'NIT' || documentType === 'CitizenshipCard' ? 10 : 50}
                    disabled={!documentType}
                    className="w-full bg-transparent border-b border-emerald-600/30 focus:border-emerald-600 text-sm font-medium py-2 outline-none disabled:opacity-50" required />
                </div>
                <div className="md:col-span-2">
                  <label className="block text-xs font-bold text-muted-foreground uppercase tracking-widest mb-1.5">Nombre o Razón Social *</label>
                  <input type="text" placeholder="Nombre completo o razón social" value={businessName}
                    onChange={(e) => setBusinessName(e.target.value.slice(0, 300))}
                    maxLength={300} required
                    className="w-full bg-transparent border-b border-emerald-600/30 focus:border-emerald-600 text-sm font-medium py-2 outline-none" />
                </div>
                <div>
                  <label className="block text-xs font-bold text-muted-foreground uppercase tracking-widest mb-1.5">Tipo de Servicio</label>
                  <select value={serviceType} onChange={(e) => setServiceType(e.target.value)}
                    className="w-full bg-transparent border-b border-emerald-600/30 focus:border-emerald-600 text-sm font-medium py-2 outline-none">
                    <option value="">Seleccione...</option>
                    {serviceTypes.map((s) => <option key={s.value} value={s.value}>{s.label}</option>)}
                  </select>
                </div>
              </div>
            </div>

            <div className="border-t border-border pt-6">
              <h3 className="text-sm font-bold text-foreground mb-4 flex items-center gap-2">
                <User className="w-4 h-4 text-emerald-600" /> Información de Contacto
              </h3>
              <div className="grid grid-cols-1 md:grid-cols-2 gap-5">
                <div className="md:col-span-2">
                  <label className="block text-xs font-bold text-muted-foreground uppercase tracking-widest mb-1.5">Nombre del Contacto</label>
                  <input type="text" placeholder="Nombre del contacto" value={contactName}
                    onChange={(e) => setContactName(e.target.value.slice(0, 300))}
                    maxLength={300}
                    className="w-full bg-transparent border-b border-emerald-600/30 focus:border-emerald-600 text-sm font-medium py-2 outline-none" />
                </div>
                <div>
                  <label className="block text-xs font-bold text-muted-foreground uppercase tracking-widest mb-1.5">Correo Electrónico</label>
                  <input type="email" placeholder="correo@ejemplo.com" value={email}
                    onChange={(e) => setEmail(e.target.value.slice(0, 256))}
                    maxLength={256}
                    className="w-full bg-transparent border-b border-emerald-600/30 focus:border-emerald-600 text-sm font-medium py-2 outline-none" />
                </div>
                <div>
                  <label className="block text-xs font-bold text-muted-foreground uppercase tracking-widest mb-1.5">Teléfono</label>
                  <input type="text" placeholder="Ej: 300 123 4567" value={phone}
                    onChange={(e) => setPhone(sanitizePhone(e.target.value))}
                    maxLength={20}
                    className="w-full bg-transparent border-b border-emerald-600/30 focus:border-emerald-600 text-sm font-medium py-2 outline-none" />
                </div>
                <div className="md:col-span-2">
                  <label className="block text-xs font-bold text-muted-foreground uppercase tracking-widest mb-1.5">Dirección</label>
                  <input type="text" placeholder="Dirección" value={address}
                    onChange={(e) => setAddress(e.target.value.slice(0, 500))}
                    maxLength={500}
                    className="w-full bg-transparent border-b border-emerald-600/30 focus:border-emerald-600 text-sm font-medium py-2 outline-none" />
                </div>
              </div>
            </div>

            {error && (
              <div className="p-3 bg-rose-50 border border-rose-200 rounded-lg text-rose-700 text-xs flex items-center gap-2">
                <AlertTriangle className="w-4 h-4 shrink-0" /> {error}
              </div>
            )}

            <div className="flex justify-between items-center pt-4 border-t border-border">
              <Button type="button" variant="ghost" onClick={() => router.push('/suppliers')}>Cancelar</Button>
              <Button type="submit" disabled={submitting}>
                {submitting ? <Loader2 className="w-4 h-4 animate-spin mr-2" /> : <Save className="w-4 h-4 mr-2" />}
                Crear Proveedor
              </Button>
            </div>
          </form>
        </CardContent>
      </Card>
    </div>
  );
}
