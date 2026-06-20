'use client';

import React, { useState, useEffect } from 'react';
import { useRouter } from 'next/navigation';
import { Loader2, DollarSign, CheckCircle, XCircle, BarChart3, Users, Search, ChevronDown, ChevronUp, AlertTriangle, Clock, CreditCard } from 'lucide-react';
import { Button } from '@/components/ui/Button';
import { Card, CardHeader, CardContent } from '@/components/ui/Card';
import feesPortfolioService, { PortfolioSummary, PortfolioCollectionStages, CollectionStage } from '@/lib/fees-portfolio-service';

export default function PortfolioPage() {
  const router = useRouter();
  const [summary, setSummary] = useState<PortfolioSummary | null>(null);
  const [stages, setStages] = useState<PortfolioCollectionStages | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');
  const [search, setSearch] = useState('');
  const [expandedStages, setExpandedStages] = useState<Record<string, boolean>>({});
  const [stageFilter, setStageFilter] = useState('');

  useEffect(() => {
    fetchData();
  }, []);

  const fetchData = async () => {
    setLoading(true);
    setError('');
    try {
      const [summaryData, stagesData] = await Promise.all([
        feesPortfolioService.getPortfolioSummary(),
        feesPortfolioService.getCollectionStages(),
      ]);
      setSummary(summaryData);
      setStages(stagesData);
    } catch {
      setError('Error al cargar la cartera.');
    } finally {
      setLoading(false);
    }
  };

  const toggleStage = (key: string) => {
    setExpandedStages((prev) => ({ ...prev, [key]: !prev[key] }));
  };

  const formatCurrency = (val: number) =>
    new Intl.NumberFormat('es-CO', { style: 'currency', currency: 'COP', minimumFractionDigits: 0 }).format(val);

  const stageConfig: Record<string, { label: string; key: keyof PortfolioCollectionStages; color: string }> = {
    preventive: { label: 'Preventivo', key: 'preventive', color: 'bg-amber-50 border-amber-200' },
    preJudicial: { label: 'Prejurídico', key: 'preJudicial', color: 'bg-orange-50 border-orange-200' },
    judicial: { label: 'Jurídico', key: 'judicial', color: 'bg-rose-50 border-rose-200' },
    agreement: { label: 'Acuerdo de Pago', key: 'agreement', color: 'bg-emerald-50 border-emerald-200' },
  };

  const allStageUnits = stages
    ? Object.entries(stageConfig).flatMap(([stageKey, cfg]) => {
        const stage = stages[cfg.key];
        return stage.units.map((u) => ({ ...u, stageKey, stageLabel: cfg.label }));
      })
    : [];

  const filteredUnits = allStageUnits.filter((u) => {
    if (stageFilter && u.stageKey !== stageFilter) return false;
    if (search) {
      const term = search.toLowerCase();
      if (!u.unitIdentifier.toLowerCase().includes(term)) return false;
    }
    return true;
  });

  if (loading) {
    return (
      <div className="flex justify-center py-20">
        <Loader2 className="w-8 h-8 animate-spin text-emerald-600" />
      </div>
    );
  }

  if (error) {
    return (
      <div className="p-4 bg-rose-50 border border-rose-200 rounded-xl text-rose-700 text-sm flex items-center gap-2">
        <AlertTriangle className="w-5 h-5 shrink-0" />
        {error}
      </div>
    );
  }

  return (
    <div className="space-y-6">
      <div>
        <h1 className="text-2xl font-bold text-foreground tracking-tight">Cartera</h1>
        <p className="text-sm text-muted-foreground mt-1">Resumen general de cartera y etapas de cobro.</p>
      </div>

      {summary && (
        <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-5 gap-4">
          <Card>
            <CardContent className="p-5">
              <div className="flex items-center gap-3">
                <div className="w-10 h-10 bg-emerald-50 rounded-xl flex items-center justify-center">
                  <DollarSign className="w-5 h-5 text-emerald-600" />
                </div>
                <div>
                  <p className="text-sm text-muted-foreground font-medium">Total Facturado</p>
                  <p className="text-xl font-bold text-foreground">{formatCurrency(summary.totalBilled)}</p>
                </div>
              </div>
            </CardContent>
          </Card>
          <Card>
            <CardContent className="p-5">
              <div className="flex items-center gap-3">
                <div className="w-10 h-10 bg-green-50 rounded-xl flex items-center justify-center">
                  <CheckCircle className="w-5 h-5 text-green-600" />
                </div>
                <div>
                  <p className="text-sm text-muted-foreground font-medium">Total Recaudado</p>
                  <p className="text-xl font-bold text-foreground">{formatCurrency(summary.totalCollected)}</p>
                </div>
              </div>
            </CardContent>
          </Card>
          <Card>
            <CardContent className="p-5">
              <div className="flex items-center gap-3">
                <div className="w-10 h-10 bg-rose-50 rounded-xl flex items-center justify-center">
                  <XCircle className="w-5 h-5 text-rose-600" />
                </div>
                <div>
                  <p className="text-sm text-muted-foreground font-medium">Saldo Pendiente</p>
                  <p className="text-xl font-bold text-foreground">{formatCurrency(summary.totalOutstanding)}</p>
                </div>
              </div>
            </CardContent>
          </Card>
          <Card>
            <CardContent className="p-5">
              <div className="flex items-center gap-3">
                <div className="w-10 h-10 bg-blue-50 rounded-xl flex items-center justify-center">
                  <BarChart3 className="w-5 h-5 text-blue-600" />
                </div>
                <div>
                  <p className="text-sm text-muted-foreground font-medium">Tasa de Cobro</p>
                  <p className="text-xl font-bold text-foreground">{summary.collectionRate.toFixed(1)}%</p>
                </div>
              </div>
            </CardContent>
          </Card>
          <Card>
            <CardContent className="p-5">
              <div className="flex items-center gap-3">
                <div className="w-10 h-10 bg-purple-50 rounded-xl flex items-center justify-center">
                  <Users className="w-5 h-5 text-purple-600" />
                </div>
                <div>
                  <p className="text-sm text-muted-foreground font-medium">Unidades en Deuda</p>
                  <p className="text-xl font-bold text-foreground">{summary.unitsWithDebt} / {summary.totalUnits}</p>
                </div>
              </div>
            </CardContent>
          </Card>
        </div>
      )}

      {summary && summary.agingBuckets.length > 0 && (
        <Card>
          <CardHeader>
            <h3 className="font-bold text-foreground">Mora por Antigüedad</h3>
          </CardHeader>
          <CardContent>
            <div className="space-y-3">
              {summary.agingBuckets.map((bucket) => {
                const maxDebt = Math.max(...summary.agingBuckets.map((b) => b.totalDebt));
                const width = maxDebt > 0 ? (bucket.totalDebt / maxDebt) * 100 : 0;
                return (
                  <div key={bucket.bucket} className="flex items-center gap-4">
                    <span className="text-sm font-medium text-muted-foreground w-32 shrink-0">{bucket.bucket}</span>
                    <div className="flex-1 bg-muted rounded-full h-5 overflow-hidden">
                      <div
                        className="h-full bg-emerald-600 rounded-full transition-all duration-500"
                        style={{ width: `${width}%` }}
                      />
                    </div>
                    <span className="text-sm font-mono font-bold text-foreground w-36 text-right">{formatCurrency(bucket.totalDebt)}</span>
                    <span className="text-xs text-muted-foreground w-16 text-right">{bucket.unitCount} uds.</span>
                  </div>
                );
              })}
            </div>
          </CardContent>
        </Card>
      )}

      {stages && (
        <>
          <div className="flex flex-col sm:flex-row gap-3 items-start sm:items-center">
            <div className="flex items-center gap-2 flex-1">
              <Search className="w-4 h-4 text-muted-foreground" />
              <input
                type="text"
                placeholder="Buscar por unidad..."
                value={search}
                onChange={(e) => setSearch(e.target.value)}
                className="w-full bg-transparent border-b border-emerald-600 focus:border-b-2 text-foreground px-0 py-2 text-sm focus:outline-none transition-all max-w-xs"
              />
            </div>
            <select
              value={stageFilter}
              onChange={(e) => setStageFilter(e.target.value)}
              className="w-full bg-transparent border-b border-emerald-600 focus:border-b-2 text-foreground px-0 py-2 text-sm focus:outline-none transition-all max-w-[200px]"
            >
              <option value="">Todas las etapas</option>
              {Object.entries(stageConfig).map(([key, cfg]) => (
                <option key={key} value={key}>{cfg.label}</option>
              ))}
            </select>
          </div>

          <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
            {Object.entries(stageConfig).map(([stageKey, cfg]) => {
              const stage = stages[cfg.key];
              return (
                <Card key={stageKey} className={cfg.color}>
                  <CardContent className="p-0">
                    <button
                      onClick={() => toggleStage(stageKey)}
                      className="w-full flex items-center justify-between p-5"
                    >
                      <div>
                        <h3 className="font-bold text-foreground">{cfg.label}</h3>
                        <p className="text-sm text-muted-foreground mt-0.5">
                          {stage.unitCount} unidades · {formatCurrency(stage.totalDebt)}
                        </p>
                      </div>
                      {expandedStages[stageKey] ? (
                        <ChevronUp className="w-5 h-5 text-muted-foreground" />
                      ) : (
                        <ChevronDown className="w-5 h-5 text-muted-foreground" />
                      )}
                    </button>
                    {expandedStages[stageKey] && (
                      <div className="border-t border-border overflow-x-auto">
                        {stage.units.length === 0 ? (
                          <div className="p-5 text-center text-sm text-muted-foreground">
                            No hay unidades en esta etapa.
                          </div>
                        ) : (
                          <table className="min-w-full divide-y divide-border text-sm">
                            <thead className="bg-muted/50">
                              <tr>
                                <th className="px-4 py-3 text-left text-xs font-bold text-muted-foreground uppercase">Unidad</th>
                                <th className="px-4 py-3 text-right text-xs font-bold text-muted-foreground uppercase">Deuda Total</th>
                                <th className="px-4 py-3 text-right text-xs font-bold text-muted-foreground uppercase">Días Mora</th>
                                <th className="px-4 py-3 text-left text-xs font-bold text-muted-foreground uppercase">Último Pago</th>
                              </tr>
                            </thead>
                            <tbody className="divide-y divide-border">
                              {stage.units.map((u) => (
                                <tr key={u.unitId} className="hover:bg-muted/30 transition-colors">
                                  <td className="px-4 py-3 whitespace-nowrap font-semibold text-foreground">{u.unitIdentifier}</td>
                                  <td className="px-4 py-3 whitespace-nowrap text-right font-mono">{formatCurrency(u.totalDebt)}</td>
                                  <td className="px-4 py-3 whitespace-nowrap text-right font-mono text-rose-600">{u.lateDays}</td>
                                  <td className="px-4 py-3 whitespace-nowrap text-muted-foreground">
                                    {u.lastPaymentDate ? new Date(u.lastPaymentDate).toLocaleDateString('es-CO') : '—'}
                                  </td>
                                </tr>
                              ))}
                            </tbody>
                          </table>
                        )}
                      </div>
                    )}
                  </CardContent>
                </Card>
              );
            })}
          </div>
        </>
      )}
    </div>
  );
}
