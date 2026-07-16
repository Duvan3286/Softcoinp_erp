'use client';

import React, { useState, useEffect } from 'react';
import { useRouter } from 'next/navigation';
import { Loader2, Plus, FileText, Download, Filter, Search } from 'lucide-react';
import { Button } from '@/components/ui/Button';
import { Card, CardContent } from '@/components/ui/Card';
import reportService, { ReportCatalogItem, GeneratedReport } from '@/lib/report-service';
import axios from 'axios';

const categoryLabels: Record<string, string> = {
  Todos: 'Todos',
  Portfolio: 'Cartera',
  Financial: 'Financieros',
  Operational: 'Operativos',
  Assembly: 'Asamblea',
  Annual: 'Anuales',
};

const formatBadgeClass: Record<string, string> = {
  Pdf: 'badge-danger',
  Excel: 'badge-success',
};

export default function ReportsPage() {
  const router = useRouter();
  const [catalog, setCatalog] = useState<ReportCatalogItem[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');
  const [success, setSuccess] = useState('');
  const [activeCategory, setActiveCategory] = useState('Todos');
  const [searchTerm, setSearchTerm] = useState('');

  const [showGenerateModal, setShowGenerateModal] = useState(false);
  const [selectedReport, setSelectedReport] = useState<ReportCatalogItem | null>(null);
  const [periodFrom, setPeriodFrom] = useState('');
  const [periodTo, setPeriodTo] = useState('');
  const [format, setFormat] = useState('Pdf');
  const [notes, setNotes] = useState('');
  const [generating, setGenerating] = useState(false);
  const [generatedReport, setGeneratedReport] = useState<GeneratedReport | null>(null);
  const [previewing, setPreviewing] = useState(false);

  useEffect(() => {
    fetchCatalog();
  }, []);

  const fetchCatalog = async () => {
    setLoading(true);
    setError('');
    try {
      const data = await reportService.getCatalog();
      setCatalog(data);
    } catch {
      setError('Error al cargar el catalogo de reportes.');
    } finally {
      setLoading(false);
    }
  };

  const categories = ['Todos', ...Array.from(new Set(catalog.map((r) => r.category)))];

  const filtered = catalog.filter((r) => {
    const matchCategory = activeCategory === 'Todos' || r.category === activeCategory;
    const matchSearch = r.name.toLowerCase().includes(searchTerm.toLowerCase()) ||
      r.description.toLowerCase().includes(searchTerm.toLowerCase());
    return matchCategory && matchSearch;
  });

  const dedicatedRouteByCode: Record<string, string> = {
    AnnualManagementReport: '/reports/annual',
  };

  const openGenerate = (report: ReportCatalogItem) => {
    const dedicatedRoute = dedicatedRouteByCode[report.code];
    if (dedicatedRoute) {
      router.push(dedicatedRoute);
      return;
    }
    setSelectedReport(report);
    setFormat(report.availableFormats.includes('Pdf') ? 'Pdf' : report.availableFormats[0]);
    setPeriodFrom('');
    setPeriodTo('');
    setNotes('');
    setGeneratedReport(null);
    setShowGenerateModal(true);
  };

  const handleGenerate = async () => {
    if (!selectedReport) return;
    setGenerating(true);
    setError('');
    try {
      const result = await reportService.generateReport({
        reportTypeCode: selectedReport.code,
        format,
        periodFrom: periodFrom || undefined,
        periodTo: periodTo || undefined,
        notes: notes || undefined,
      });
      setGeneratedReport(result);
      setSuccess('Reporte generado exitosamente.');
    } catch (err) {
      if (axios.isAxiosError(err) && err.response?.data?.message) {
        setError(err.response.data.message);
      } else {
        setError('Error al generar el reporte.');
      }
    } finally {
      setGenerating(false);
    }
  };

  const handlePreview = async () => {
    if (!selectedReport) return;
    setPreviewing(true);
    setError('');
    try {
      const blob = await reportService.getPreviewBlob(
        selectedReport.code,
        periodFrom || undefined,
        periodTo || undefined
      );
      const url = window.URL.createObjectURL(blob);
      window.open(url, '_blank');
    } catch {
      setError('Error al generar la vista previa.');
    } finally {
      setPreviewing(false);
    }
  };

  const handleDownload = async (id: string) => {
    try {
      const blob = await reportService.downloadReport(id);
      const url = window.URL.createObjectURL(blob);
      const a = document.createElement('a');
      a.href = url;
      a.download = `${id}.${format.toLowerCase()}`;
      a.click();
      window.URL.revokeObjectURL(url);
    } catch {
      setError('Error al descargar el reporte.');
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
        <h1 className="text-2xl font-bold text-foreground">Reportes y Exportaciones</h1>
        <div className="flex gap-2">
          <Button onClick={() => router.push('/reports/history')}>
            <FileText className="w-4 h-4 mr-2" />
            Historial
          </Button>
        </div>
      </div>

      {error && (
        <div className="p-4 bg-red-50 dark:bg-red-950/30 border border-red-200 dark:border-red-900 rounded-xl text-red-700 dark:text-red-400 text-sm">{error}</div>
      )}

      {success && (
        <div className="p-4 bg-emerald-50 dark:bg-emerald-950/30 border border-emerald-200 dark:border-emerald-900 rounded-xl text-emerald-700 dark:text-emerald-400 text-sm">{success}</div>
      )}

      <div className="flex flex-wrap gap-3 items-center">
        <div className="flex items-center gap-2 bg-muted rounded-lg px-3 py-2 flex-1 min-w-[200px]">
          <Search className="w-4 h-4 text-muted-foreground" />
          <input
            type="text"
            placeholder="Buscar reportes..."
            value={searchTerm}
            onChange={(e) => setSearchTerm(e.target.value)}
            className="bg-transparent border-none outline-none text-sm flex-1"
          />
        </div>
      </div>

      <div className="flex gap-2 flex-wrap">
        {categories.map((cat) => (
          <button
            key={cat}
            onClick={() => setActiveCategory(cat)}
            className={`px-4 py-2 rounded-lg text-sm font-medium transition-colors ${
              activeCategory === cat
                ? 'bg-emerald-600 text-white'
                : 'bg-muted text-muted-foreground hover:bg-emerald-50 hover:text-emerald-700'
            }`}
          >
            {categoryLabels[cat] || cat}
          </button>
        ))}
      </div>

      {filtered.length === 0 ? (
        <div className="text-center py-12 text-muted-foreground">
          <Filter className="w-12 h-12 mx-auto mb-3 opacity-40" />
          <p>No hay reportes disponibles en esta categoria.</p>
        </div>
      ) : (
        <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-4">
          {filtered.map((report) => (
            <Card key={report.code} className="hover:shadow-md transition-shadow">
              <CardContent className="p-4">
                <div className="flex flex-col h-full">
                  <div className="flex-1">
                    <h3 className="font-semibold text-foreground">{report.name}</h3>
                    <p className="text-sm text-muted-foreground mt-1 line-clamp-2">{report.description}</p>
                    <div className="flex flex-wrap gap-2 mt-3">
                      {report.availableFormats.map((fmt) => (
                        <span key={fmt} className={`badge ${formatBadgeClass[fmt] || 'badge-neutral'}`}>{fmt}</span>
                      ))}
                      {report.containsPersonalData && (
                        <span className="badge badge-warning">Datos personales</span>
                      )}
                    </div>
                  </div>
                  <div className="mt-4">
                    <Button onClick={() => openGenerate(report)}>
                      <Plus className="w-4 h-4 mr-1" />
                      Generar
                    </Button>
                  </div>
                </div>
              </CardContent>
            </Card>
          ))}
        </div>
      )}

      {showGenerateModal && selectedReport && (
        <div className="fixed inset-0 bg-black/60 backdrop-blur-sm z-50 flex items-center justify-center p-4">
          <Card className="w-full max-w-lg">
            <CardContent className="p-6 space-y-4">
              <div className="flex items-center justify-between">
                <h2 className="text-xl font-bold text-foreground">Generar Reporte</h2>
                <button
                  onClick={() => { setShowGenerateModal(false); setGeneratedReport(null); }}
                  className="p-1 rounded-lg hover:bg-muted transition-colors"
                >
                  <svg className="w-5 h-5 text-muted-foreground" fill="none" viewBox="0 0 24 24" stroke="currentColor"><path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M6 18L18 6M6 6l12 12" /></svg>
                </button>
              </div>

              <div>
                <p className="text-sm text-muted-foreground mb-1">Reporte</p>
                <p className="font-semibold text-foreground">{selectedReport.name}</p>
              </div>

              <div className="grid grid-cols-2 gap-4">
                <div>
                  <label className="block text-sm font-medium text-foreground mb-1">Desde</label>
                  <input
                    type="date"
                    value={periodFrom}
                    onChange={(e) => setPeriodFrom(e.target.value)}
                    className="w-full border-b border-emerald-600/30 focus:border-emerald-600 text-sm font-medium py-2 outline-none bg-transparent"
                  />
                </div>
                <div>
                  <label className="block text-sm font-medium text-foreground mb-1">Hasta</label>
                  <input
                    type="date"
                    value={periodTo}
                    onChange={(e) => setPeriodTo(e.target.value)}
                    className="w-full border-b border-emerald-600/30 focus:border-emerald-600 text-sm font-medium py-2 outline-none bg-transparent"
                  />
                </div>
              </div>

              <div>
                <label className="block text-sm font-medium text-foreground mb-2">Formato</label>
                <div className="flex gap-4">
                  {selectedReport.availableFormats.map((fmt) => (
                    <label key={fmt} className="flex items-center gap-2 cursor-pointer">
                      <input
                        type="radio"
                        name="format"
                        value={fmt}
                        checked={format === fmt}
                        onChange={(e) => setFormat(e.target.value)}
                        className="text-emerald-600 border-emerald-600/30"
                      />
                      <span className="text-sm font-medium">{fmt}</span>
                    </label>
                  ))}
                </div>
              </div>

              <div>
                <label className="block text-sm font-medium text-foreground mb-1">Notas (opcional)</label>
                <textarea
                  value={notes}
                  onChange={(e) => setNotes(e.target.value)}
                  className="w-full border border-emerald-600/30 focus:border-emerald-600 text-sm font-medium py-2 outline-none rounded-lg px-3 bg-background"
                  rows={2}
                />
              </div>

              {generatedReport && (
                <div className="p-3 bg-emerald-50 dark:bg-emerald-950/30 border border-emerald-200 dark:border-emerald-900 rounded-lg text-sm">
                  <p className="font-semibold text-emerald-700 dark:text-emerald-400">Reporte generado</p>
                  <p className="text-emerald-600 dark:text-emerald-500 mt-1">{generatedReport.fileName}</p>
                  {generatedReport.consecutiveNumber > 0 && (
                    <p className="text-xs text-emerald-500 mt-1">No. {String(generatedReport.consecutiveNumber).padStart(4, '0')}</p>
                  )}
                </div>
              )}

              <div className="flex gap-2">
                {generatedReport ? (
                  <>
                    <Button variant="success" onClick={() => handleDownload(generatedReport.id)}>
                      <Download className="w-4 h-4 mr-1" />
                      Descargar
                    </Button>
                    <Button variant="secondary" onClick={() => router.push('/reports/history')}>
                      Ver Historial
                    </Button>
                    <Button variant="ghost" onClick={() => { setShowGenerateModal(false); setGeneratedReport(null); }}>
                      Cerrar
                    </Button>
                  </>
                ) : (
                  <>
                    {format === 'Pdf' && (
                      <Button variant="secondary" onClick={handlePreview} disabled={previewing}>
                        {previewing ? 'Generando vista previa...' : 'Vista Previa'}
                      </Button>
                    )}
                    <Button onClick={handleGenerate} disabled={generating}>
                      {generating ? 'Generando...' : 'Generar'}
                    </Button>
                    <Button variant="secondary" onClick={() => { setShowGenerateModal(false); setGeneratedReport(null); }}>
                      Cancelar
                    </Button>
                  </>
                )}
              </div>
            </CardContent>
          </Card>
        </div>
      )}
    </div>
  );
}
