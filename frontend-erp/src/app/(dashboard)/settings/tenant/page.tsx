'use client';

import React, { useState, useEffect } from 'react';
import { useAuth } from '@/context/AuthContext';
import tenantConfigService, { TenantConfiguration, ConfigurationAuditLog, LegalRepresentativeHistory, TenantDocument } from '@/lib/tenant-config-service';
import { 
  Building2,
  FileText,
  BadgeDollarSign,
  Settings,
  BellRing,
  History,
  Save,
  Loader2,
  UploadCloud,
  Download,
  AlertCircle,
  Users
} from 'lucide-react';
import { Button } from '@/components/ui/Button';

export default function TenantConfigPage() {
  const { user } = useAuth();
  const [activeTab, setActiveTab] = useState('legal');
  const [isLoading, setIsLoading] = useState(true);
  const [isSaving, setIsSaving] = useState(false);
  const [error, setError] = useState('');
  const [success, setSuccess] = useState('');

  const [config, setConfig] = useState<TenantConfiguration | null>(null);
  const [auditLogs, setAuditLogs] = useState<ConfigurationAuditLog[]>([]);
  const [reps, setReps] = useState<LegalRepresentativeHistory[]>([]);
  const [docs, setDocs] = useState<TenantDocument[]>([]);

  const [uploadRole, setUploadRole] = useState<number>(1);
  const [uploadSelection, setUploadSelection] = useState<string>('Reglamento (RPH)');
  const [uploadCustomTitle, setUploadCustomTitle] = useState<string>('');
  const [uploadFile, setUploadFile] = useState<File | null>(null);

  const documentOptions: Record<number, { label: string, type: number, value: string }[]> = {
    1: [
      { label: 'Reglamento (RPH)', type: 0, value: 'Reglamento (RPH)' },
      { label: 'Certificado Representación', type: 1, value: 'Certificado Representación' },
      { label: 'RUT', type: 2, value: 'RUT' },
      { label: 'Otro Documento...', type: 3, value: 'OTRO' }
    ],
    2: [
      { label: 'Acta de Nombramiento del Consejo', type: 3, value: 'Acta de Nombramiento del Consejo' },
      { label: 'Reglamento Interno del Consejo', type: 3, value: 'Reglamento Interno del Consejo' },
      { label: 'Documento de Identidad', type: 3, value: 'Documento de Identidad' },
      { label: 'Otro Documento...', type: 3, value: 'OTRO' }
    ],
    4: [
      { label: 'Acta de Nombramiento de Revisor Fiscal', type: 3, value: 'Acta de Nombramiento de Revisor Fiscal' },
      { label: 'Tarjeta Profesional', type: 3, value: 'Tarjeta Profesional' },
      { label: 'Certificado de Antecedentes', type: 3, value: 'Certificado de Antecedentes' },
      { label: 'Documento de Identidad', type: 3, value: 'Documento de Identidad' },
      { label: 'Otro Documento...', type: 3, value: 'OTRO' }
    ]
  };

  // Permissions
  const canEdit = user?.role === 'SuperAdmin' || user?.role === 'Admin';

  useEffect(() => {
    fetchData();
  }, []);

  const fetchData = async () => {
    try {
      setIsLoading(true);
      const conf = await tenantConfigService.getConfig().catch(() => null);
      if (conf) setConfig(conf);
      else setConfig(getDefaultConfig());
      
      const alogs = await tenantConfigService.getAuditLogs().catch(() => []);
      setAuditLogs(alogs);
      
      const repHist = await tenantConfigService.getRepresentatives().catch(() => []);
      setReps(repHist);

      const dcs = await tenantConfigService.getDocuments().catch(() => []);
      setDocs(dcs);
    } catch (err) {
      console.error(err);
    } finally {
      setIsLoading(false);
    }
  };

  const getDefaultConfig = (): TenantConfiguration => ({
    officialName: '',
    nit: '',
    verificationDigit: '',
    address: '',
    municipality: '',
    department: '',
    phone: '',
    email: '',
    realEstateRegistration: '',
    constitutionDate: new Date().toISOString().split('T')[0],
    legalRepresentativeName: '',
    legalRepresentativeId: '',
    legalRepresentativeDocumentType: 'CC',
    legalRepresentativeDv: '',
    billingCycleDay: 1,
    gracePeriodDays: 10,
    latePaymentInterestRate: 0,
    maxLegalInterestRate: 0,
    fiscalYearStartMonth: 1,
    fiscalYearStartDay: 1,
    annualBudget: 0,
    totalUnits: 0,
    totalTowers: 1,
    roundingPolicy: 0,
    maxActiveExtraordinaryQuotas: 3,
    hasContingencyFund: true,
    contingencyFundPercentage: 1,
    senderEmail: '',
    signatureFooterTemplate: '',
    autoSendLatePaymentNotifications: false,
    latePaymentNotificationFrequencyDays: 30
  });

  const handleChange = (field: keyof TenantConfiguration, value: any) => {
    if (!config || !canEdit) return;
    
    // Auto-capitalize first letter of each word for specific text fields
    const titleCaseFields = ['officialName', 'address', 'municipality', 'department', 'legalRepresentativeName'];
    if (typeof value === 'string' && titleCaseFields.includes(field)) {
      value = value.toLowerCase().replace(/(?:^|\s)\S/g, (a) => a.toUpperCase());
    }
    
    setConfig({ ...config, [field]: value });
  };

  const handleNitChange = (val: string) => {
    if (!config || !canEdit) return;
    const nit = val.replace(/\D/g, '').slice(0, 10);
    
    // Calculate DV
    let dv = '';
    if (nit.length > 0) {
      const vpri = [3, 7, 13, 17, 19, 23, 29, 37, 41, 43, 47, 53, 59, 67, 71];
      let x = 0;
      let y = 0;
      let z = nit.length;
      for (let i = 0; i < z; i++) {
        y = parseInt(nit.charAt(i));
        x += (y * vpri[z - 1 - i]);
      }
      y = x % 11;
      dv = (y > 1) ? (11 - y).toString() : y.toString();
    }
    
    setConfig({ ...config, nit, verificationDigit: dv });
  };

  const handlePhoneChange = (val: string) => {
    if (!config || !canEdit) return;
    const phone = val.replace(/[^0-9\-\+\s]/g, '').slice(0, 20);
    setConfig({ ...config, phone });
  };

  const calculateDV = (nit: string): string => {
    if (nit.length === 0) return '';
    const vpri = [3, 7, 13, 17, 19, 23, 29, 37, 41, 43, 47, 53, 59, 67, 71];
    let x = 0;
    let y = 0;
    const z = nit.length;
    for (let i = 0; i < z; i++) {
      y = parseInt(nit.charAt(i));
      x += (y * vpri[z - 1 - i]);
    }
    y = x % 11;
    return (y > 1) ? (11 - y).toString() : y.toString();
  };

  const handleLegalDocTypeChange = (docType: string) => {
    if (!config || !canEdit) return;
    setConfig({ ...config, legalRepresentativeDocumentType: docType, legalRepresentativeId: '', legalRepresentativeDv: '' });
  };

  const handleLegalDocChange = (val: string) => {
    if (!config || !canEdit) return;
    const docType = config.legalRepresentativeDocumentType;
    const isNumericOnly = docType === 'CC' || docType === 'NIT';
    const maxLen = (docType === 'CC' || docType === 'NIT') ? 10 : 50;
    const doc = isNumericOnly ? val.replace(/\D/g, '').slice(0, maxLen) : val.replace(/[^a-zA-Z0-9]/g, '').slice(0, maxLen);
    const dv = docType === 'NIT' ? calculateDV(doc) : '';
    setConfig({ ...config, legalRepresentativeId: doc, legalRepresentativeDv: dv });
  };

  const handleSave = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!config || !canEdit) return;
    
    setError('');
    setSuccess('');

    // Client side validations
    if (config.billingCycleDay < 1 || config.billingCycleDay > 28) {
      setError('El día de corte debe estar entre 1 y 28.');
      return;
    }
    if (config.latePaymentInterestRate > config.maxLegalInterestRate) {
      setError(`La tasa de interés de mora (${config.latePaymentInterestRate}%) no puede superar el límite legal ingresado (${config.maxLegalInterestRate}%).`);
      return;
    }
    if (config.hasContingencyFund && config.contingencyFundPercentage < 1) {
      setError('Según la Ley 675, el fondo de imprevistos debe ser mínimo el 1% del presupuesto.');
      return;
    }

    setIsSaving(true);
    try {
      const updated = await tenantConfigService.updateConfig(config);
      setConfig(updated);
      setSuccess('Configuración guardada exitosamente.');
      
      // Refresh audit logs and reps history
      const alogs = await tenantConfigService.getAuditLogs().catch(() => []);
      setAuditLogs(alogs);
      const repHist = await tenantConfigService.getRepresentatives().catch(() => []);
      setReps(repHist);

    } catch (err: any) {
      console.error("Save error:", err);
      let errorMessage = 'Error al guardar la configuración';
      if (err.response?.data) {
        if (typeof err.response.data === 'string') {
          errorMessage = err.response.data;
        } else if (err.response.data.title) {
          errorMessage = err.response.data.title;
          if (err.response.data.errors) {
            const firstError = Object.values(err.response.data.errors)[0] as string[];
            if (firstError && firstError.length > 0) {
              errorMessage = firstError[0];
            }
          }
        } else {
          errorMessage = JSON.stringify(err.response.data);
        }
      } else if (err.message) {
        errorMessage = err.message;
      }
      setError(errorMessage);
    } finally {
      setIsSaving(false);
    }
  };

  const handleLogoUpload = async (e: React.ChangeEvent<HTMLInputElement>) => {
    if (!e.target.files || e.target.files.length === 0 || !canEdit) return;
    const file = e.target.files[0];
    
    if (file.size > 2 * 1024 * 1024) {
      setError('El logo excede el tamaño máximo de 2MB.');
      return;
    }
    
    setIsSaving(true);
    try {
      const res = await tenantConfigService.uploadLogo(file);
      if (config) setConfig({ ...config, logoUrl: res.logoUrl });
      setSuccess('Logo actualizado.');
    } catch (err: any) {
      console.error("Logo upload error:", err);
      let errorMessage = 'Error al subir el logo. Asegúrate de guardar primero la configuración básica.';
      if (err.response?.data) {
        if (typeof err.response.data === 'string') {
          errorMessage = err.response.data;
        } else if (err.response.data.title) {
          errorMessage = err.response.data.title;
        } else {
          errorMessage = JSON.stringify(err.response.data);
        }
      }
      setError(errorMessage);
    } finally {
      setIsSaving(false);
    }
  };

  const tabs = [
    { id: 'legal', label: 'Legal & Identidad', icon: <Building2 className="w-4 h-4" /> },
    { id: 'financiero', label: 'Financiero', icon: <BadgeDollarSign className="w-4 h-4" /> },
    { id: 'operativo', label: 'Operativo', icon: <Settings className="w-4 h-4" /> },
    { id: 'notificaciones', label: 'Notificaciones', icon: <BellRing className="w-4 h-4" /> },
    { id: 'documentos', label: 'Documentos', icon: <FileText className="w-4 h-4" /> },
    { id: 'historial', label: 'Historial / Auditoría', icon: <History className="w-4 h-4" /> }
  ];

  if (isLoading) {
    return (
      <div className="flex items-center justify-center min-h-[500px]">
        <Loader2 className="w-8 h-8 animate-spin text-emerald-600" />
      </div>
    );
  }

  return (
    <div className="p-6 max-w-6xl mx-auto">
      
      <div className="flex items-center justify-between mb-8">
        <div>
          <h1 className="text-3xl font-black text-foreground uppercase tracking-tight">Configuración del Conjunto</h1>
          <p className="text-sm text-slate-500 font-medium">Gestiona la identidad legal y parámetros operativos de la copropiedad.</p>
        </div>
        {canEdit && (
          <Button onClick={handleSave} disabled={isSaving || !config} className="bg-emerald-600 hover:bg-emerald-700">
            {isSaving ? <Loader2 className="w-4 h-4 mr-2 animate-spin" /> : <Save className="w-4 h-4 mr-2" />}
            Guardar Cambios
          </Button>
        )}
      </div>

      {error && (
        <div className="bg-rose-50 border-l-4 border-rose-500 p-4 mb-6 text-sm text-rose-700 flex items-center gap-3">
          <AlertCircle className="w-5 h-5" />
          {error}
        </div>
      )}

      {success && (
        <div className="bg-emerald-50 border-l-4 border-emerald-500 p-4 mb-6 text-sm text-emerald-700 flex items-center gap-3">
          <Settings className="w-5 h-5" />
          {success}
        </div>
      )}

      {!canEdit && (
        <div className="bg-blue-50 border-l-4 border-blue-500 p-4 mb-6 text-sm text-blue-700">
          <strong>Modo Solo Lectura:</strong> No tienes permisos para modificar la configuración.
        </div>
      )}

      <div className="bg-card rounded-xl shadow-sm border border-border flex flex-col md:flex-row overflow-hidden">
        
        {/* Tabs sidebar */}
        <div className="w-full md:w-64 bg-slate-50/50 dark:bg-slate-900/50 border-b md:border-b-0 md:border-r border-border flex flex-col p-4 gap-2">
          {tabs.map(tab => (
            <button
              key={tab.id}
              onClick={() => setActiveTab(tab.id)}
              className={`flex items-center gap-3 px-4 py-3 rounded-lg text-sm font-semibold transition-all ${
                activeTab === tab.id 
                  ? 'bg-emerald-100/50 text-emerald-700 dark:bg-emerald-900/30 dark:text-emerald-400'
                  : 'text-slate-600 hover:bg-slate-100 dark:text-slate-400 dark:hover:bg-slate-800'
              }`}
            >
              {tab.icon}
              {tab.label}
            </button>
          ))}
        </div>

        {/* Tab Content */}
        <div className="flex-1 p-6 md:p-8">
          {config && (
            <form onSubmit={handleSave} className="space-y-6">
              
              {/* LEGAL */}
              {activeTab === 'legal' && (
                <div className="animate-in fade-in space-y-6">
                  <h2 className="text-lg font-bold text-foreground border-b border-border pb-2">Identidad Legal</h2>
                  
                  <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
                    <div>
                      <label className="block text-xs font-bold text-slate-500 uppercase tracking-wider mb-2">Nombre Oficial</label>
                      <input disabled={!canEdit} value={config.officialName} onChange={e => handleChange('officialName', e.target.value)} required maxLength={200}
                        className="w-full bg-transparent border-b border-emerald-600/30 focus:border-emerald-600 text-sm font-medium py-2 outline-none disabled:opacity-50" />
                    </div>
                    <div className="flex gap-4">
                      <div className="flex-1">
                        <label className="block text-xs font-bold text-slate-500 uppercase tracking-wider mb-2">NIT (Sin DV)</label>
                        <input type="text" inputMode="numeric" disabled={!canEdit} value={config.nit} onChange={e => handleNitChange(e.target.value)} required maxLength={10}
                          className="w-full bg-transparent border-b border-emerald-600/30 focus:border-emerald-600 text-sm font-medium py-2 outline-none disabled:opacity-50" />
                      </div>
                      <div className="w-16">
                        <label className="block text-xs font-bold text-slate-500 uppercase tracking-wider mb-2">DV</label>
                        <input disabled value={config.verificationDigit} required readOnly
                          className="w-full bg-slate-100 dark:bg-slate-800 border-b border-slate-300 text-slate-500 text-sm font-bold py-2 outline-none text-center cursor-not-allowed" />
                      </div>
                    </div>

                    <div>
                      <label className="block text-xs font-bold text-slate-500 uppercase tracking-wider mb-2">Dirección</label>
                      <input disabled={!canEdit} value={config.address} onChange={e => handleChange('address', e.target.value)} required maxLength={200}
                        className="w-full bg-transparent border-b border-emerald-600/30 focus:border-emerald-600 text-sm font-medium py-2 outline-none disabled:opacity-50" />
                    </div>
                    <div className="grid grid-cols-2 gap-4">
                      <div>
                        <label className="block text-xs font-bold text-slate-500 uppercase tracking-wider mb-2">Municipio</label>
                        <input disabled={!canEdit} value={config.municipality} onChange={e => handleChange('municipality', e.target.value)} required maxLength={100}
                          className="w-full bg-transparent border-b border-emerald-600/30 focus:border-emerald-600 text-sm font-medium py-2 outline-none disabled:opacity-50" />
                      </div>
                      <div>
                        <label className="block text-xs font-bold text-slate-500 uppercase tracking-wider mb-2">Depto.</label>
                        <input disabled={!canEdit} value={config.department} onChange={e => handleChange('department', e.target.value)} required maxLength={100}
                          className="w-full bg-transparent border-b border-emerald-600/30 focus:border-emerald-600 text-sm font-medium py-2 outline-none disabled:opacity-50" />
                      </div>
                    </div>

                    <div>
                      <label className="block text-xs font-bold text-slate-500 uppercase tracking-wider mb-2">Teléfono Admin</label>
                      <input type="tel" disabled={!canEdit} value={config.phone} onChange={e => handlePhoneChange(e.target.value)} required maxLength={20}
                        className="w-full bg-transparent border-b border-emerald-600/30 focus:border-emerald-600 text-sm font-medium py-2 outline-none disabled:opacity-50" />
                    </div>
                    <div>
                      <label className="block text-xs font-bold text-slate-500 uppercase tracking-wider mb-2">Correo Oficial</label>
                      <input type="email" disabled={!canEdit} value={config.email} onChange={e => handleChange('email', e.target.value)} required maxLength={256}
                        className="w-full bg-transparent border-b border-emerald-600/30 focus:border-emerald-600 text-sm font-medium py-2 outline-none disabled:opacity-50" />
                    </div>

                    <div>
                      <label className="block text-xs font-bold text-slate-500 uppercase tracking-wider mb-2">Matrícula Inmobiliaria</label>
                      <input disabled={!canEdit} value={config.realEstateRegistration} onChange={e => handleChange('realEstateRegistration', e.target.value)} required maxLength={50}
                        className="w-full bg-transparent border-b border-emerald-600/30 focus:border-emerald-600 text-sm font-medium py-2 outline-none disabled:opacity-50" />
                    </div>
                    <div>
                      <label className="block text-xs font-bold text-slate-500 uppercase tracking-wider mb-2">Fecha Constitución</label>
                      <input type="date" disabled={!canEdit} value={config.constitutionDate.split('T')[0]} onChange={e => handleChange('constitutionDate', e.target.value)} required
                        className="w-full bg-transparent border-b border-emerald-600/30 focus:border-emerald-600 text-sm font-medium py-2 outline-none disabled:opacity-50" />
                    </div>

                    <div>
                      <label className="block text-xs font-bold text-slate-500 uppercase tracking-wider mb-2">Rep. Legal (Nombre)</label>
                      <input disabled={!canEdit} value={config.legalRepresentativeName} onChange={e => handleChange('legalRepresentativeName', e.target.value)} required maxLength={200}
                        className="w-full bg-transparent border-b border-emerald-600/30 focus:border-emerald-600 text-sm font-medium py-2 outline-none disabled:opacity-50" />
                    </div>
                    <div>
                      <label className="block text-xs font-bold text-slate-500 uppercase tracking-wider mb-2">Tipo de Documento</label>
                      <select disabled={!canEdit} value={config.legalRepresentativeDocumentType} onChange={e => handleLegalDocTypeChange(e.target.value)} required
                        className="w-full bg-transparent border-b border-emerald-600/30 focus:border-emerald-600 text-sm font-medium py-2 outline-none disabled:opacity-50">
                        <option value="CC">Cédula de Ciudadanía (CC)</option>
                        <option value="CE">Cédula de Extranjería (CE)</option>
                        <option value="NIT">NIT</option>
                        <option value="PASAPORTE">Pasaporte</option>
                        <option value="PEP">Persona Expuesta Políticamente (PEP)</option>
                        <option value="PPT">Pasaporte Temporal (PPT)</option>
                      </select>
                    </div>
                    <div className="flex gap-4">
                      <div className="flex-1">
                        <label className="block text-xs font-bold text-slate-500 uppercase tracking-wider mb-2">
                          {config.legalRepresentativeDocumentType === 'NIT' ? 'NIT Rep. Legal' : 'Número de Documento'}
                        </label>
                        <input type="text" inputMode={['CC', 'NIT'].includes(config.legalRepresentativeDocumentType) ? 'numeric' : 'text'}
                          disabled={!canEdit} value={config.legalRepresentativeId} onChange={e => handleLegalDocChange(e.target.value)} required
                          maxLength={['CC', 'NIT'].includes(config.legalRepresentativeDocumentType) ? 10 : 50}
                          className="w-full bg-transparent border-b border-emerald-600/30 focus:border-emerald-600 text-sm font-medium py-2 outline-none disabled:opacity-50" />
                      </div>
                      {config.legalRepresentativeDocumentType === 'NIT' && (
                        <div className="w-16">
                          <label className="block text-xs font-bold text-slate-500 uppercase tracking-wider mb-2">DV</label>
                          <input disabled value={config.legalRepresentativeDv} readOnly
                            className="w-full bg-slate-100 dark:bg-slate-800 border-b border-slate-300 text-slate-500 text-sm font-bold py-2 outline-none text-center cursor-not-allowed" />
                        </div>
                      )}
                    </div>
                  </div>

                  <div className="pt-6 border-t border-border mt-6">
                    <h2 className="text-sm font-bold text-foreground mb-4">Logotipo del Conjunto</h2>
                    <div className="flex items-center gap-6">
                      {config.logoUrl ? (
                        <img src={`${process.env.NEXT_PUBLIC_API_URL?.replace('/api', '')}${config.logoUrl}`} alt="Logo" className="w-24 h-24 object-contain bg-slate-100 rounded-lg p-2" />
                      ) : (
                        <div className="w-24 h-24 bg-slate-100 flex items-center justify-center rounded-lg text-slate-400">
                          Sin Logo
                        </div>
                      )}
                      
                      {canEdit && (
                        <div>
                          <input type="file" id="logoUpload" className="hidden" accept=".png,.svg" onChange={handleLogoUpload} />
                          <label htmlFor="logoUpload" className="cursor-pointer bg-slate-100 hover:bg-slate-200 text-slate-700 px-4 py-2 rounded text-sm font-semibold flex items-center gap-2 transition-colors">
                            <UploadCloud className="w-4 h-4" /> Subir Logo (PNG/SVG)
                          </label>
                          <p className="text-xs text-slate-400 mt-2">Máximo 2MB.</p>
                        </div>
                      )}
                    </div>
                  </div>
                </div>
              )}

              {/* FINANCIERO */}
              {activeTab === 'financiero' && (
                <div className="animate-in fade-in space-y-6">
                  <h2 className="text-lg font-bold text-foreground border-b border-border pb-2 flex items-center gap-2">
                    <BadgeDollarSign className="w-5 h-5 text-emerald-600" /> Parámetros Financieros
                  </h2>
                  <div className="bg-amber-50 border-l-4 border-amber-500 p-4 mb-6 text-xs text-amber-700 font-medium">
                    Nota: Los cambios realizados aquí se registrarán en la Auditoría. Las tasas no aplican retroactivamente a deudas ya liquidadas.
                  </div>
                  
                  <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
                    <div>
                      <label className="block text-xs font-bold text-slate-500 uppercase tracking-wider mb-2">Día de Corte (1-28)</label>
                      <input type="number" min={1} max={28} disabled={!canEdit} value={config.billingCycleDay} onChange={e => handleChange('billingCycleDay', Number(e.target.value))} required
                        className="w-full bg-transparent border-b border-emerald-600/30 focus:border-emerald-600 text-sm font-medium py-2 outline-none disabled:opacity-50" />
                    </div>
                    <div>
                      <label className="block text-xs font-bold text-slate-500 uppercase tracking-wider mb-2">Días de gracia para pago sin mora</label>
                      <input type="number" min={0} disabled={!canEdit} value={config.gracePeriodDays} onChange={e => handleChange('gracePeriodDays', Number(e.target.value))} required
                        className="w-full bg-transparent border-b border-emerald-600/30 focus:border-emerald-600 text-sm font-medium py-2 outline-none disabled:opacity-50" />
                    </div>

                    <div>
                      <label className="block text-xs font-bold text-slate-500 uppercase tracking-wider mb-2">Tasa Máx Legal Vigente (%)</label>
                      <input type="number" step="0.01" disabled={!canEdit} value={config.maxLegalInterestRate} onChange={e => handleChange('maxLegalInterestRate', Number(e.target.value))} required
                        className="w-full bg-transparent border-b border-emerald-600/30 focus:border-emerald-600 text-sm font-medium py-2 outline-none disabled:opacity-50" />
                    </div>
                    <div>
                      <label className="block text-xs font-bold text-emerald-600 uppercase tracking-wider mb-2">Tasa Mora Aplicada (%)</label>
                      <input type="number" step="0.01" disabled={!canEdit} value={config.latePaymentInterestRate} onChange={e => handleChange('latePaymentInterestRate', Number(e.target.value))} required
                        className="w-full bg-emerald-50 border-b-2 border-emerald-600 text-emerald-900 text-sm font-bold py-2 px-2 outline-none disabled:opacity-50" />
                    </div>

                    <div className="grid grid-cols-2 gap-4">
                      <div>
                        <label className="block text-xs font-bold text-slate-500 uppercase tracking-wider mb-2">Inicio Año Fiscal (Mes)</label>
                        <input type="number" min={1} max={12} disabled={!canEdit} value={config.fiscalYearStartMonth} onChange={e => handleChange('fiscalYearStartMonth', Number(e.target.value))} required
                          className="w-full bg-transparent border-b border-emerald-600/30 focus:border-emerald-600 text-sm font-medium py-2 outline-none disabled:opacity-50" />
                      </div>
                      <div>
                        <label className="block text-xs font-bold text-slate-500 uppercase tracking-wider mb-2">Inicio (Día)</label>
                        <input type="number" min={1} max={31} disabled={!canEdit} value={config.fiscalYearStartDay} onChange={e => handleChange('fiscalYearStartDay', Number(e.target.value))} required
                          className="w-full bg-transparent border-b border-emerald-600/30 focus:border-emerald-600 text-sm font-medium py-2 outline-none disabled:opacity-50" />
                      </div>
                    </div>
                    
                    <div>
                      <label className="block text-xs font-bold text-slate-500 uppercase tracking-wider mb-2">Presupuesto Anual Aprobado (COP)</label>
                      <input type="number" step="0.01" disabled={!canEdit} value={config.annualBudget} onChange={e => handleChange('annualBudget', Number(e.target.value))} required
                        className="w-full bg-transparent border-b border-emerald-600/30 focus:border-emerald-600 text-sm font-medium py-2 outline-none disabled:opacity-50" />
                    </div>
                  </div>
                </div>
              )}

              {/* OPERATIVO */}
              {activeTab === 'operativo' && (
                <div className="animate-in fade-in space-y-6">
                  <h2 className="text-lg font-bold text-foreground border-b border-border pb-2">Parámetros Operativos</h2>
                  
                  <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
                    <div>
                      <label className="block text-xs font-bold text-slate-500 uppercase tracking-wider mb-2">Total Unidades (Casas/Aptos)</label>
                      <input type="number" disabled={!canEdit} value={config.totalUnits} onChange={e => handleChange('totalUnits', Number(e.target.value))} required
                        className="w-full bg-transparent border-b border-emerald-600/30 focus:border-emerald-600 text-sm font-medium py-2 outline-none disabled:opacity-50" />
                    </div>
                    <div>
                      <label className="block text-xs font-bold text-slate-500 uppercase tracking-wider mb-2">Total Torres/Bloques</label>
                      <input type="number" disabled={!canEdit} value={config.totalTowers} onChange={e => handleChange('totalTowers', Number(e.target.value))} required
                        className="w-full bg-transparent border-b border-emerald-600/30 focus:border-emerald-600 text-sm font-medium py-2 outline-none disabled:opacity-50" />
                    </div>

                    <div>
                      <label className="block text-xs font-bold text-slate-500 uppercase tracking-wider mb-2">Política de Redondeo Financiero</label>
                      <select disabled={!canEdit} value={config.roundingPolicy} onChange={e => handleChange('roundingPolicy', Number(e.target.value))}
                        className="w-full bg-transparent border-b border-emerald-600/30 focus:border-emerald-600 text-sm font-medium py-2 outline-none disabled:opacity-50">
                        <option value={0}>Al peso más cercano</option>
                        <option value={1}>Siempre hacia arriba</option>
                        <option value={2}>Siempre hacia abajo</option>
                      </select>
                    </div>
                    <div>
                      <label className="block text-xs font-bold text-slate-500 uppercase tracking-wider mb-2">Máx Cuotas Extra Activas</label>
                      <input type="number" disabled={!canEdit} value={config.maxActiveExtraordinaryQuotas} onChange={e => handleChange('maxActiveExtraordinaryQuotas', Number(e.target.value))} required
                        className="w-full bg-transparent border-b border-emerald-600/30 focus:border-emerald-600 text-sm font-medium py-2 outline-none disabled:opacity-50" />
                    </div>

                    <div className="md:col-span-2 grid grid-cols-1 md:grid-cols-2 gap-6 bg-slate-50 dark:bg-slate-900/50 p-4 rounded-lg border border-border">
                      <div className="flex items-center gap-3 h-full">
                        <input type="checkbox" id="hasFund" disabled={!canEdit} checked={config.hasContingencyFund} onChange={e => handleChange('hasContingencyFund', e.target.checked)} className="w-5 h-5 text-emerald-600" />
                        <label htmlFor="hasFund" className="text-sm font-bold text-foreground">El conjunto recauda Fondo de Imprevistos</label>
                      </div>
                      <div>
                        <label className="block text-xs font-bold text-slate-500 uppercase tracking-wider mb-2">Porcentaje Imprevistos (%)</label>
                        <input type="number" step="0.01" disabled={!canEdit || !config.hasContingencyFund} value={config.contingencyFundPercentage} onChange={e => handleChange('contingencyFundPercentage', Number(e.target.value))} required={config.hasContingencyFund}
                          className="w-full bg-transparent border-b border-emerald-600/30 focus:border-emerald-600 text-sm font-medium py-2 outline-none disabled:opacity-50" />
                        {config.hasContingencyFund && config.contingencyFundPercentage < 1 && (
                          <p className="text-[10px] text-rose-500 mt-1 font-bold">Según Ley 675, debe ser mínimo 1%.</p>
                        )}
                      </div>
                    </div>
                  </div>
                </div>
              )}

              {/* NOTIFICACIONES */}
              {activeTab === 'notificaciones' && (
                <div className="animate-in fade-in space-y-6">
                  <h2 className="text-lg font-bold text-foreground border-b border-border pb-2">Notificaciones y Correos</h2>
                  
                  <div className="grid grid-cols-1 gap-6">
                    <div>
                      <label className="block text-xs font-bold text-slate-500 uppercase tracking-wider mb-2">Correo Remitente Oficial</label>
                      <input type="email" disabled={!canEdit} value={config.senderEmail} onChange={e => handleChange('senderEmail', e.target.value)} required maxLength={256}
                        className="w-full bg-transparent border-b border-emerald-600/30 focus:border-emerald-600 text-sm font-medium py-2 outline-none disabled:opacity-50" />
                    </div>

                    <div>
                      <label className="block text-xs font-bold text-slate-500 uppercase tracking-wider mb-2">Plantilla de Pie de Firma</label>
                      <textarea disabled={!canEdit} value={config.signatureFooterTemplate} onChange={e => handleChange('signatureFooterTemplate', e.target.value)} rows={4} maxLength={1000}
                        className="w-full bg-slate-50 dark:bg-slate-900 border border-border focus:border-emerald-600 rounded-md text-sm p-3 outline-none disabled:opacity-50" />
                    </div>

                    <div className="flex items-center gap-3">
                      <input type="checkbox" id="autoMora" disabled={!canEdit} checked={config.autoSendLatePaymentNotifications} onChange={e => handleChange('autoSendLatePaymentNotifications', e.target.checked)} className="w-5 h-5 text-emerald-600" />
                      <label htmlFor="autoMora" className="text-sm font-bold text-foreground">Enviar notificaciones automáticas a morosos</label>
                    </div>

                    {config.autoSendLatePaymentNotifications && (
                       <div>
                         <label className="block text-xs font-bold text-slate-500 uppercase tracking-wider mb-2">Frecuencia de Notificación (Días)</label>
                         <input type="number" min={1} max={365} disabled={!canEdit} value={config.latePaymentNotificationFrequencyDays} onChange={e => handleChange('latePaymentNotificationFrequencyDays', Number(e.target.value))} required
                           className="w-64 bg-transparent border-b border-emerald-600/30 focus:border-emerald-600 text-sm font-medium py-2 outline-none disabled:opacity-50" />
                       </div>
                    )}
                  </div>
                </div>
              )}

              {/* HISTORIAL */}
              {activeTab === 'historial' && (
                <div className="animate-in fade-in space-y-8">
                  
                  <div>
                    <h2 className="text-lg font-bold text-foreground border-b border-border pb-2 mb-4 flex items-center gap-2">
                       <History className="w-5 h-5 text-slate-500" /> Historial de Cambios Financieros
                    </h2>
                    {auditLogs.length === 0 ? (
                      <p className="text-sm text-slate-500">No hay registros de auditoría.</p>
                    ) : (
                      <div className="overflow-x-auto border border-border rounded-lg">
                        <table className="w-full text-left text-sm whitespace-nowrap">
                          <thead className="bg-slate-50 dark:bg-slate-900 border-b border-border text-xs uppercase tracking-wider text-slate-500">
                            <tr>
                              <th className="px-4 py-3">Fecha</th>
                              <th className="px-4 py-3">Usuario ID</th>
                              <th className="px-4 py-3">Parámetro</th>
                              <th className="px-4 py-3 text-rose-600">Valor Anterior</th>
                              <th className="px-4 py-3 text-emerald-600">Nuevo Valor</th>
                            </tr>
                          </thead>
                          <tbody className="divide-y divide-border">
                            {auditLogs.map(log => (
                              <tr key={log.id} className="hover:bg-slate-50 dark:hover:bg-slate-900/50">
                                <td className="px-4 py-3">{new Date(log.timestamp).toLocaleString()}</td>
                                <td className="px-4 py-3 font-mono text-xs">{log.changedByUserId.substring(0,8)}...</td>
                                <td className="px-4 py-3 font-semibold">{log.parameterName}</td>
                                <td className="px-4 py-3 text-rose-600 bg-rose-50/30">{log.oldValue}</td>
                                <td className="px-4 py-3 text-emerald-600 bg-emerald-50/30 font-bold">{log.newValue}</td>
                              </tr>
                            ))}
                          </tbody>
                        </table>
                      </div>
                    )}
                  </div>

                  <div>
                    <h2 className="text-lg font-bold text-foreground border-b border-border pb-2 mb-4 flex items-center gap-2">
                       <Users className="w-5 h-5 text-slate-500" /> Histórico de Representantes Legales
                    </h2>
                    {reps.length === 0 ? (
                      <p className="text-sm text-slate-500">No hay históricos.</p>
                    ) : (
                      <div className="overflow-x-auto border border-border rounded-lg">
                        <table className="w-full text-left text-sm whitespace-nowrap">
                          <thead className="bg-slate-50 dark:bg-slate-900 border-b border-border text-xs uppercase tracking-wider text-slate-500">
                            <tr>
                              <th className="px-4 py-3">Nombre Completo</th>
                              <th className="px-4 py-3">Identificación</th>
                              <th className="px-4 py-3">Desde</th>
                              <th className="px-4 py-3">Hasta</th>
                            </tr>
                          </thead>
                          <tbody className="divide-y divide-border">
                            {reps.map(rep => (
                              <tr key={rep.id} className="hover:bg-slate-50 dark:hover:bg-slate-900/50">
                                <td className="px-4 py-3 font-semibold">{rep.fullName}</td>
                                <td className="px-4 py-3">{rep.identificationDocument}</td>
                                <td className="px-4 py-3 text-emerald-600">{new Date(rep.startDate).toLocaleDateString()}</td>
                                <td className="px-4 py-3 text-slate-500">{rep.endDate ? new Date(rep.endDate).toLocaleDateString() : 'Actual'}</td>
                              </tr>
                            ))}
                          </tbody>
                        </table>
                      </div>
                    )}
                  </div>

                </div>
              )}

              {/* DOCUMENTOS */}
              {activeTab === 'documentos' && (
                <div className="animate-in fade-in space-y-6">
                  <h2 className="text-lg font-bold text-foreground border-b border-border pb-2 flex items-center gap-2">
                    <FileText className="w-5 h-5" /> Documentos Oficiales
                  </h2>

                  {/* Tabla de Documentos */}
                  <div className="overflow-x-auto border border-border rounded-lg">
                    <table className="w-full text-left text-sm whitespace-nowrap">
                      <thead className="bg-slate-50 dark:bg-slate-900 border-b border-border text-xs uppercase tracking-wider text-slate-500">
                        <tr>
                          <th className="px-4 py-3">Título</th>
                          <th className="px-4 py-3">Fecha</th>
                          <th className="px-4 py-3">Rol Mínimo</th>
                          <th className="px-4 py-3 text-right">Acción</th>
                        </tr>
                      </thead>
                      <tbody className="divide-y divide-border">
                        {docs.length === 0 ? (
                          <tr><td colSpan={4} className="px-4 py-6 text-center text-slate-500">No hay documentos cargados.</td></tr>
                        ) : (
                          docs.map(doc => (
                            <tr key={doc.id} className="hover:bg-slate-50 dark:hover:bg-slate-900/50">
                              <td className="px-4 py-3 font-semibold text-emerald-700">{doc.title}</td>
                              <td className="px-4 py-3 text-slate-500">{new Date(doc.uploadedAt).toLocaleDateString()}</td>
                              <td className="px-4 py-3">
                                <span className="bg-slate-100 text-slate-600 px-2 py-1 rounded text-[10px] font-bold uppercase">
                                  {doc.minimumRoleRequired === 0 ? 'SuperAdmin' : doc.minimumRoleRequired === 1 ? 'Admin' : doc.minimumRoleRequired === 2 ? 'Consejo' : doc.minimumRoleRequired === 4 ? 'Auditor' : 'Todos'}
                                </span>
                              </td>
                              <td className="px-4 py-3 text-right">
                                <button 
                                  onClick={() => tenantConfigService.downloadDocument(doc.id, `${doc.title.replace(/[^a-z0-9]/gi, '_').toLowerCase()}.pdf`).catch(() => setError('Error al descargar el documento.'))}
                                  className="inline-flex items-center justify-center p-2 text-emerald-600 hover:bg-emerald-50 rounded"
                                  title="Descargar"
                                >
                                  <Download className="w-4 h-4" />
                                </button>
                              </td>
                            </tr>
                          ))
                        )}
                      </tbody>
                    </table>
                  </div>

                  {/* Formulario de Carga */}
                  {canEdit && (
                    <div className="bg-slate-50 dark:bg-slate-900/30 p-6 rounded-lg border border-dashed border-slate-300 mt-6">
                      <h3 className="text-sm font-bold text-foreground mb-4">Subir Nuevo Documento (PDF)</h3>
                      <div className="mt-4">
                        <div className="grid grid-cols-1 md:grid-cols-12 gap-4 mb-4 items-end">
                          <div className="md:col-span-3 flex flex-col gap-2">
                            <label className="text-[10px] font-bold text-slate-500 uppercase">Perfil / Rol</label>
                            <select 
                              value={uploadRole} 
                              onChange={(e) => {
                                const newRole = parseInt(e.target.value);
                                setUploadRole(newRole);
                                setUploadSelection(documentOptions[newRole][0].value);
                              }}
                              className="bg-white dark:bg-slate-800 border border-border p-2 text-sm rounded outline-none focus:border-emerald-600"
                            >
                              <option value={1}>Administración</option>
                              <option value={2}>Consejo (Council)</option>
                              <option value={4}>Revisoría Fiscal / Auditor</option>
                            </select>
                          </div>

                          <div className="md:col-span-4 flex flex-col gap-2">
                            <label className="text-[10px] font-bold text-slate-500 uppercase">Tipo de Documento</label>
                            <select 
                              value={uploadSelection}
                              onChange={(e) => setUploadSelection(e.target.value)}
                              className="bg-white dark:bg-slate-800 border border-border p-2 text-sm rounded outline-none focus:border-emerald-600"
                            >
                              {documentOptions[uploadRole].map(opt => (
                                <option key={opt.value} value={opt.value}>{opt.label}</option>
                              ))}
                            </select>
                          </div>

                          {uploadSelection === 'OTRO' && (
                            <div className="md:col-span-5 flex flex-col gap-2">
                              <label className="text-[10px] font-bold text-slate-500 uppercase">Título del Documento</label>
                              <input type="text" value={uploadCustomTitle} onChange={e => setUploadCustomTitle(e.target.value)} placeholder="Escriba un título..." className="bg-white dark:bg-slate-800 border border-border p-2 text-sm rounded outline-none focus:border-emerald-600" />
                            </div>
                          )}

                          <div className={`flex flex-col gap-2 ${uploadSelection === 'OTRO' ? 'md:col-span-12' : 'md:col-span-5'}`}>
                            <label className="text-[10px] font-bold text-slate-500 uppercase">Archivo (PDF)</label>
                            <input 
                              id="doc-file-input"
                              type="file" 
                              accept=".pdf" 
                              onChange={(e) => setUploadFile(e.target.files?.[0] || null)}
                              className="w-full text-sm text-slate-600 dark:text-slate-300 file:mr-4 file:py-2 file:px-4 file:rounded file:border-0 file:text-xs file:font-semibold file:bg-emerald-50 file:text-emerald-700 hover:file:bg-emerald-100 cursor-pointer" 
                            />
                          </div>
                        </div>
                        <Button 
                          type="button" 
                          disabled={isSaving} 
                          onClick={async () => {
                            const title = uploadSelection === 'OTRO' ? uploadCustomTitle : uploadSelection;
                            
                            const selectedOption = documentOptions[uploadRole].find(o => o.value === uploadSelection) || documentOptions[uploadRole][0];
                            const type = selectedOption.type;
                            const role = uploadRole;

                            if (!uploadFile || !title) {
                              setError('El título y el archivo son obligatorios para subir un documento.');
                              return;
                            }

                            if (uploadFile.size > 10 * 1024 * 1024) {
                              setError('El archivo excede el tamaño máximo permitido de 10MB.');
                              return;
                            }

                            setIsSaving(true);
                            try {
                              await tenantConfigService.uploadDocument(uploadFile, type, title, role);
                              setSuccess('Documento subido correctamente.');
                              const dcs = await tenantConfigService.getDocuments();
                              setDocs(dcs);
                              
                              // Limpiar formulario
                              setUploadCustomTitle('');
                              setUploadFile(null);
                              // Reset the file input visually
                              const fileInput = document.getElementById('doc-file-input') as HTMLInputElement;
                              if (fileInput) fileInput.value = '';
                              
                            } catch (err) {
                              setError('Error al subir el documento.');
                            } finally {
                              setIsSaving(false);
                            }
                          }}
                          className="bg-emerald-600 text-xs py-2 h-auto px-4 mt-2"
                        >
                          <UploadCloud className="w-4 h-4 mr-2" /> Subir Documento
                        </Button>
                      </div>
                    </div>
                  )}
                </div>
              )}

            </form>
          )}
        </div>

      </div>
    </div>
  );
}
