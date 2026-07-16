'use client';

import React, { useState, useEffect, useCallback } from 'react';
import { useRouter } from 'next/navigation';
import { Loader2, Download, Search, FileText } from 'lucide-react';
import { Button } from '@/components/ui/Button';
import { Card, CardContent } from '@/components/ui/Card';
import reportService, { GeneratedReport, ReportCatalogItem } from '@/lib/report-service';

const formatBadgeClass: Record<string, string> = {
  Pdf: 'badge-danger',
  Excel: 'badge-success',
};

export default function HistoryPage() {
  const router = useRouter();
  const [reports, setReports] = useState<GeneratedReport[]>([]);
  const [catalog, setCatalog] = useState<ReportCatalogItem[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');
  const [reportTypeFilter, setReportTypeFilter] = useState('');
  const [fromDate, setFromDate] = useState('');
  const [toDate, setToDate] = useState('');

  const fetchHistory = useCallback(async () => {
    setLoading(true);
    setError('');
    try {
      const data = await reportService.getHistory(
        reportTypeFilter || undefined,
        fromDate || undefined,
        toDate || undefined
      );
      setReports(data);
    } catch {
      setError('Error al cargar el historial de reportes.');
    } finally {
      setLoading(false);
    }
  }, [reportTypeFilter, fromDate, toDate]);

  useEffect(() => {
    const init = async () => {
      try {
        const catalogData = await reportService.getCatalog();
        setCatalog(catalogData);
      } catch {
        // Ignore catalog error
      }
    };
    init();
  }, []);

  useEffect(() => {
    fetchHistory();
  }, [fetchHistory]);

  const handleDownload = async (report: GeneratedReport) => {
    try {
      const blob = await reportService.downloadReport(report.id);
      const url = window.URL.createObjectURL(blob);
      const a = document.createElement('a');
      a.href = url;
      a.download = report.fileName;
      a.click();
      window.URL.revokeObjectURL(url);
    } catch {
      setError('Error al descargar el reporte.');
    }
  };

  const formatFileSize = (bytes: number) => {
    if (bytes < 1024) return `${bytes} B`;
    if (bytes < 1048576) return `${(bytes / 1024).toFixed(1)} KB`;
    return `${(bytes / 1048576).toFixed(1)} MB`;
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
        <h1 className="text-2xl font-bold text-foreground">Historial de Reportes</h1>
        <Button onClick={() => router.push('/reports')}>
          <FileText className="w-4 h-4 mr-2" />
          Catálogo de Reportes
        </Button>
      </div>

      {error && (
        <div className="p-4 bg-red-50 dark:bg-red-950/30 border border-red-200 dark:border-red-900 rounded-xl text-red-700 dark:text-red-400 text-sm">{error}</div>
      )}

      <Card>
        <CardContent className="p-4">
          <div className="flex flex-wrap gap-3 items-center">
            <select
              value={reportTypeFilter}
              onChange={(e) => setReportTypeFilter(e.target.value)}
              className="border border-emerald-600/30 focus:border-emerald-600 text-sm font-medium py-2 outline-none rounded-lg px-3 bg-background"
            >
              <option value="">Todos los reportes</option>
              {catalog.map((r) => (
                <option key={r.code} value={r.code}>{r.name}</option>
              ))}
            </select>
            <input
              type="date"
              value={fromDate}
              onChange={(e) => setFromDate(e.target.value)}
              className="border border-emerald-600/30 focus:border-emerald-600 text-sm font-medium py-2 outline-none rounded-lg px-3 bg-background"
              placeholder="Desde"
            />
            <input
              type="date"
              value={toDate}
              onChange={(e) => setToDate(e.target.value)}
              className="border border-emerald-600/30 focus:border-emerald-600 text-sm font-medium py-2 outline-none rounded-lg px-3 bg-background"
              placeholder="Hasta"
            />
          </div>
        </CardContent>
      </Card>

      {reports.length === 0 ? (
        <div className="text-center py-12 text-muted-foreground">
          <Search className="w-12 h-12 mx-auto mb-3 opacity-40" />
          <p>No hay reportes generados.</p>
        </div>
      ) : (
        <Card>
          <CardContent className="p-0">
            <div className="overflow-x-auto">
              <table className="w-full text-sm">
                <thead>
                  <tr className="border-b border-border">
                    <th className="text-left p-4 font-semibold text-foreground">No.</th>
                    <th className="text-left p-4 font-semibold text-foreground">Reporte</th>
                    <th className="text-left p-4 font-semibold text-foreground">Formato</th>
                    <th className="text-left p-4 font-semibold text-foreground">Periodo</th>
                    <th className="text-left p-4 font-semibold text-foreground">Fecha</th>
                    <th className="text-left p-4 font-semibold text-foreground">Tamaño</th>
                    <th className="text-left p-4 font-semibold text-foreground">Acciones</th>
                  </tr>
                </thead>
                <tbody>
                  {reports.map((report) => (
                    <tr key={report.id} className="border-b border-border hover:bg-muted/50 transition-colors">
                      <td className="p-4 text-muted-foreground">
                        {report.consecutiveNumber > 0 ? String(report.consecutiveNumber).padStart(4, '0') : '-'}
                      </td>
                      <td className="p-4">
                        <span className="font-medium text-foreground">{report.reportTypeName}</span>
                      </td>
                      <td className="p-4">
                        <span className={`badge ${formatBadgeClass[report.format] || 'badge-neutral'}`}>{report.format}</span>
                      </td>
                      <td className="p-4 text-muted-foreground">
                        {report.periodFrom && report.periodTo
                          ? `${new Date(report.periodFrom).toLocaleDateString('es-CO')} - ${new Date(report.periodTo).toLocaleDateString('es-CO')}`
                          : '-'}
                      </td>
                      <td className="p-4 text-muted-foreground">
                        {new Date(report.generatedAt).toLocaleDateString('es-CO')}
                      </td>
                      <td className="p-4 text-muted-foreground">{formatFileSize(report.fileSizeBytes)}</td>
                      <td className="p-4">
                        <button
                          onClick={() => handleDownload(report)}
                          className="px-3 py-1.5 text-sm font-medium text-emerald-600 hover:bg-emerald-50 dark:hover:bg-emerald-950/20 rounded-lg transition-colors flex items-center gap-1"
                        >
                          <Download className="w-4 h-4" />
                          Descargar
                        </button>
                      </td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>
          </CardContent>
        </Card>
      )}
    </div>
  );
}
