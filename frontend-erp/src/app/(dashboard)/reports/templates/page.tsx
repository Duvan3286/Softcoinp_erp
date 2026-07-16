'use client';

import React, { useState, useEffect } from 'react';
import { Loader2, Save, Palette } from 'lucide-react';
import { Button } from '@/components/ui/Button';
import { Card, CardContent } from '@/components/ui/Card';
import reportService, { GlobalPdfTemplate, UpdateGlobalPdfTemplate } from '@/lib/report-service';
import axios from 'axios';

export default function TemplatesPage() {
  const [template, setTemplate] = useState<GlobalPdfTemplate | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');
  const [success, setSuccess] = useState('');

  const [headerText, setHeaderText] = useState('');
  const [footerText, setFooterText] = useState('');
  const [signatureName, setSignatureName] = useState('');
  const [signatureRole, setSignatureRole] = useState('');
  const [confidentialityNote, setConfidentialityNote] = useState('');
  const [disclaimerNote, setDisclaimerNote] = useState('');
  const [primaryColor, setPrimaryColor] = useState('#059669');
  const [secondaryColor, setSecondaryColor] = useState('#d1d5db');
  const [saving, setSaving] = useState(false);

  useEffect(() => {
    fetchTemplate();
  }, []);

  const fetchTemplate = async () => {
    setLoading(true);
    setError('');
    try {
      const data = await reportService.getGlobalTemplate();
      setTemplate(data);
      setHeaderText(data.headerText);
      setFooterText(data.footerText || '');
      setSignatureName(data.signatureName);
      setSignatureRole(data.signatureRole);
      setConfidentialityNote(data.confidentialityNote || '');
      setDisclaimerNote(data.disclaimerNote || '');
      setPrimaryColor(data.primaryColor);
      setSecondaryColor(data.secondaryColor);
    } catch {
      setError('Error al cargar la configuración de plantilla.');
    } finally {
      setLoading(false);
    }
  };

  const handleSave = async () => {
    setSaving(true);
    setError('');
    try {
      const data: UpdateGlobalPdfTemplate = {
        headerText,
        footerText: footerText || undefined,
        signatureName,
        signatureRole,
        confidentialityNote: confidentialityNote || undefined,
        disclaimerNote: disclaimerNote || undefined,
        primaryColor,
        secondaryColor,
      };

      await reportService.updateGlobalTemplate(data);
      setSuccess('Configuración guardada exitosamente.');
      fetchTemplate();
    } catch (err) {
      if (axios.isAxiosError(err) && err.response?.data?.message) {
        setError(err.response.data.message);
      } else {
        setError('Error al guardar la configuración.');
      }
    } finally {
      setSaving(false);
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
      <h1 className="text-2xl font-bold text-foreground">Configuración de Membrete PDF</h1>
      <p className="text-sm text-muted-foreground">
        Esta configuración aplica a todos los reportes PDF generados por el sistema.
      </p>

      {error && (
        <div className="p-4 bg-red-50 dark:bg-red-950/30 border border-red-200 dark:border-red-900 rounded-xl text-red-700 dark:text-red-400 text-sm">{error}</div>
      )}

      {success && (
        <div className="p-4 bg-emerald-50 dark:bg-emerald-950/30 border border-emerald-200 dark:border-emerald-900 rounded-xl text-emerald-700 dark:text-emerald-400 text-sm">{success}</div>
      )}

      <Card>
        <CardContent className="p-6 space-y-4">
          <div className="flex items-center justify-between">
            <h2 className="text-lg font-semibold text-foreground">Membrete del Conjunto</h2>
            <div className="flex items-center gap-3">
              <div className="flex items-center gap-2">
                <Palette className="w-4 h-4 text-muted-foreground" />
                <input
                  type="color"
                  value={primaryColor}
                  onChange={(e) => setPrimaryColor(e.target.value)}
                  className="w-8 h-8 rounded cursor-pointer border border-border"
                  title="Color primario"
                />
                <input
                  type="color"
                  value={secondaryColor}
                  onChange={(e) => setSecondaryColor(e.target.value)}
                  className="w-8 h-8 rounded cursor-pointer border border-border"
                  title="Color secundario"
                />
              </div>
            </div>
          </div>

          <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
            <div>
              <label className="block text-sm font-medium text-foreground mb-1">Encabezado del documento</label>
              <input
                type="text"
                value={headerText}
                onChange={(e) => setHeaderText(e.target.value)}
                className="w-full border-b border-emerald-600/30 focus:border-emerald-600 text-sm font-medium py-2 outline-none bg-transparent"
                placeholder="Propiedad Horizontal"
              />
            </div>
            <div>
              <label className="block text-sm font-medium text-foreground mb-1">Pie de página</label>
              <input
                type="text"
                value={footerText}
                onChange={(e) => setFooterText(e.target.value)}
                className="w-full border-b border-emerald-600/30 focus:border-emerald-600 text-sm font-medium py-2 outline-none bg-transparent"
                placeholder="Documento generado por el sistema"
              />
            </div>
          </div>

          <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
            <div>
              <label className="block text-sm font-medium text-foreground mb-1">Nombre del firmante</label>
              <input
                type="text"
                value={signatureName}
                onChange={(e) => setSignatureName(e.target.value)}
                className="w-full border-b border-emerald-600/30 focus:border-emerald-600 text-sm font-medium py-2 outline-none bg-transparent"
                placeholder="Nombre del administrador"
              />
            </div>
            <div>
              <label className="block text-sm font-medium text-foreground mb-1">Cargo del firmante</label>
              <input
                type="text"
                value={signatureRole}
                onChange={(e) => setSignatureRole(e.target.value)}
                className="w-full border-b border-emerald-600/30 focus:border-emerald-600 text-sm font-medium py-2 outline-none bg-transparent"
                placeholder="Administrador"
              />
            </div>
          </div>

          <div>
            <label className="block text-sm font-medium text-foreground mb-1">Nota de confidencialidad (Ley 1581)</label>
            <textarea
              value={confidentialityNote}
              onChange={(e) => setConfidentialityNote(e.target.value)}
              className="w-full border border-emerald-600/30 focus:border-emerald-600 text-sm font-medium py-2 outline-none rounded-lg px-3 bg-background"
              rows={2}
              placeholder="ESTE DOCUMENTO CONTIENE DATOS PERSONALES PROTEGIDOS POR LA LEY 1581 DE 2012"
            />
          </div>

          <div>
            <label className="block text-sm font-medium text-foreground mb-1">Aviso legal / descargo</label>
            <textarea
              value={disclaimerNote}
              onChange={(e) => setDisclaimerNote(e.target.value)}
              className="w-full border border-emerald-600/30 focus:border-emerald-600 text-sm font-medium py-2 outline-none rounded-lg px-3 bg-background"
              rows={2}
              placeholder="Los datos aquí contenidos corresponden al momento de generación..."
            />
          </div>

          <div className="flex gap-2">
            <Button onClick={handleSave} disabled={saving}>
              <Save className="w-4 h-4 mr-1" />
              {saving ? 'Guardando...' : 'Guardar Configuración'}
            </Button>
          </div>
        </CardContent>
      </Card>
    </div>
  );
}
