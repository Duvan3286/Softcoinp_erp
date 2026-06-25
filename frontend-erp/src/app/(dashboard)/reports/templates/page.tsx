'use client';

import React, { useState, useEffect } from 'react';
import { Loader2, Save, Palette, Upload } from 'lucide-react';
import { Button } from '@/components/ui/Button';
import { Card, CardContent } from '@/components/ui/Card';
import reportService, { PDFTemplate, UpdatePDFTemplate } from '@/lib/report-service';
import axios from 'axios';

export default function TemplatesPage() {
  const [templates, setTemplates] = useState<PDFTemplate[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');
  const [success, setSuccess] = useState('');
  const [editingId, setEditingId] = useState<string | null>(null);

  const [headerText, setHeaderText] = useState('');
  const [footerText, setFooterText] = useState('');
  const [signatureName, setSignatureName] = useState('');
  const [signatureRole, setSignatureRole] = useState('');
  const [confidentialityNote, setConfidentialityNote] = useState('');
  const [disclaimerNote, setDisclaimerNote] = useState('');
  const [primaryColor, setPrimaryColor] = useState('#059669');
  const [secondaryColor, setSecondaryColor] = useState('#d1d5db');
  const [isDefault, setIsDefault] = useState(false);
  const [saving, setSaving] = useState(false);

  useEffect(() => {
    fetchTemplates();
  }, []);

  const fetchTemplates = async () => {
    setLoading(true);
    setError('');
    try {
      const data = await reportService.getTemplates();
      setTemplates(data);
    } catch {
      setError('Error al cargar plantillas PDF.');
    } finally {
      setLoading(false);
    }
  };

  const handleEdit = (template: PDFTemplate) => {
    setHeaderText(template.headerText);
    setFooterText(template.footerText);
    setSignatureName(template.signatureName);
    setSignatureRole(template.signatureRole);
    setConfidentialityNote(template.confidentialityNote || '');
    setDisclaimerNote(template.disclaimerNote || '');
    setPrimaryColor(template.primaryColor);
    setSecondaryColor(template.secondaryColor);
    setIsDefault(template.isDefault);
    setEditingId(template.id);
  };

  const handleSave = async () => {
    if (!editingId) return;
    setSaving(true);
    setError('');
    try {
      const data: UpdatePDFTemplate = {
        headerText,
        footerText,
        signatureName,
        signatureRole,
        confidentialityNote: confidentialityNote || undefined,
        disclaimerNote: disclaimerNote || undefined,
        primaryColor,
        secondaryColor,
        isDefault: isDefault || undefined,
      };

      await reportService.updateTemplate(editingId, data);
      setSuccess('Plantilla actualizada exitosamente.');
      setEditingId(null);
      fetchTemplates();
    } catch (err) {
      if (axios.isAxiosError(err) && err.response?.data?.message) {
        setError(err.response.data.message);
      } else {
        setError('Error al guardar la plantilla.');
      }
    } finally {
      setSaving(false);
    }
  };

  const handleLogoUpload = () => {
    const input = document.createElement('input');
    input.type = 'file';
    input.accept = 'image/*';
    input.onchange = (e) => {
      const file = (e.target as HTMLInputElement).files?.[0];
      if (file) {
        setSuccess('Logo seleccionado. La carga se completará al guardar.');
      }
    };
    input.click();
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
      <h1 className="text-2xl font-bold text-foreground">Configuración de Plantillas PDF</h1>

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

      {templates.length === 0 ? (
        <div className="text-center py-12 text-muted-foreground">
          <p>No hay plantillas PDF configuradas.</p>
        </div>
      ) : (
        <div className="space-y-4">
          {templates.map((template) => (
            <Card key={template.id}>
              <CardContent className="p-6">
                {editingId === template.id ? (
                  <div className="space-y-4">
                    <div className="flex items-center justify-between">
                        <h2 className="text-lg font-semibold text-foreground">
                          Editando: {template.reportTypeName}
                        </h2>
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
                        <label className="flex items-center gap-2 cursor-pointer">
                          <input
                            type="checkbox"
                            checked={isDefault}
                            onChange={(e) => setIsDefault(e.target.checked)}
                            className="rounded border-emerald-600/30 text-emerald-600"
                          />
                          <span className="text-sm">Por defecto</span>
                        </label>
                      </div>
                    </div>

                    <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
                      <div>
                        <label className="block text-sm font-medium text-foreground mb-1">Encabezado</label>
                        <input
                          type="text"
                          value={headerText}
                          onChange={(e) => setHeaderText(e.target.value)}
                          className="w-full border-b border-emerald-600/30 focus:border-emerald-600 text-sm font-medium py-2 outline-none bg-transparent"
                        />
                      </div>
                      <div>
                        <label className="block text-sm font-medium text-foreground mb-1">Pie de página</label>
                        <input
                          type="text"
                          value={footerText}
                          onChange={(e) => setFooterText(e.target.value)}
                          className="w-full border-b border-emerald-600/30 focus:border-emerald-600 text-sm font-medium py-2 outline-none bg-transparent"
                        />
                      </div>
                    </div>

                    <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
                      <div>
                        <label className="block text-sm font-medium text-foreground mb-1">Nombre de la firma</label>
                        <input
                          type="text"
                          value={signatureName}
                          onChange={(e) => setSignatureName(e.target.value)}
                          className="w-full border-b border-emerald-600/30 focus:border-emerald-600 text-sm font-medium py-2 outline-none bg-transparent"
                        />
                      </div>
                      <div>
                        <label className="block text-sm font-medium text-foreground mb-1">Cargo de la firma</label>
                        <input
                          type="text"
                          value={signatureRole}
                          onChange={(e) => setSignatureRole(e.target.value)}
                          className="w-full border-b border-emerald-600/30 focus:border-emerald-600 text-sm font-medium py-2 outline-none bg-transparent"
                        />
                      </div>
                    </div>

                    <div>
                      <label className="block text-sm font-medium text-foreground mb-1">Nota de confidencialidad</label>
                      <textarea
                        value={confidentialityNote}
                        onChange={(e) => setConfidentialityNote(e.target.value)}
                        className="w-full border border-emerald-600/30 focus:border-emerald-600 text-sm font-medium py-2 outline-none rounded-lg px-3 bg-background"
                        rows={2}
                      />
                    </div>

                    <div>
                      <label className="block text-sm font-medium text-foreground mb-1">Aviso legal</label>
                      <textarea
                        value={disclaimerNote}
                        onChange={(e) => setDisclaimerNote(e.target.value)}
                        className="w-full border border-emerald-600/30 focus:border-emerald-600 text-sm font-medium py-2 outline-none rounded-lg px-3 bg-background"
                        rows={2}
                      />
                    </div>

                    <div>
                      <label className="block text-sm font-medium text-foreground mb-1">Logo</label>
                      <div className="flex items-center gap-3">
                        <Button variant="secondary" onClick={handleLogoUpload}>
                          <Upload className="w-4 h-4 mr-1" />
                          {template.logoFilePath ? 'Cambiar Logo' : 'Subir Logo'}
                        </Button>
                        {template.logoFilePath && (
                          <span className="text-xs text-muted-foreground">Logo actual: {template.logoFilePath}</span>
                        )}
                      </div>
                    </div>

                    <div className="flex gap-2">
                      <Button onClick={handleSave} disabled={saving}>
                        <Save className="w-4 h-4 mr-1" />
                        {saving ? 'Guardando...' : 'Guardar Plantilla'}
                      </Button>
                      <Button variant="secondary" onClick={() => setEditingId(null)}>
                        Cancelar
                      </Button>
                    </div>
                  </div>
                ) : (
                  <div>
                    <div className="flex items-start justify-between">
                      <div className="flex-1 min-w-0">
                        <div className="flex items-center gap-2 mb-1">
                          <h3 className="font-semibold text-foreground">{template.reportTypeName}</h3>
                          {template.isDefault && (
                            <span className="badge badge-success">Por defecto</span>
                          )}
                        </div>
                        <div className="grid grid-cols-2 md:grid-cols-4 gap-3 mt-3 text-sm">
                          <div>
                            <span className="text-muted-foreground">Encabezado:</span>
                            <p className="font-medium text-foreground">{template.headerText}</p>
                          </div>
                          <div>
                            <span className="text-muted-foreground">Pie:</span>
                            <p className="font-medium text-foreground">{template.footerText}</p>
                          </div>
                          <div>
                            <span className="text-muted-foreground">Firma:</span>
                            <p className="font-medium text-foreground">{template.signatureName}</p>
                          </div>
                          <div>
                            <span className="text-muted-foreground">Cargo:</span>
                            <p className="font-medium text-foreground">{template.signatureRole}</p>
                          </div>
                        </div>
                        <div className="flex items-center gap-3 mt-2">
                          <span className="flex items-center gap-1 text-xs">
                            <span
                              className="inline-block w-3 h-3 rounded-full"
                              style={{ backgroundColor: template.primaryColor }}
                            />
                            Primario
                          </span>
                          <span className="flex items-center gap-1 text-xs">
                            <span
                              className="inline-block w-3 h-3 rounded-full"
                              style={{ backgroundColor: template.secondaryColor }}
                            />
                            Secundario
                          </span>
                        </div>
                      </div>
                      <button
                        onClick={() => handleEdit(template)}
                        className="px-3 py-1.5 text-sm font-medium text-emerald-600 hover:bg-emerald-50 dark:hover:bg-emerald-950/20 rounded-lg transition-colors"
                      >
                        Editar
                      </button>
                    </div>
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
