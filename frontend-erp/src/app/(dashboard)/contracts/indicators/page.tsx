'use client';

import React, { useState, useEffect } from 'react';
import { useRouter } from 'next/navigation';
import { Loader2, ArrowLeft, AlertTriangle, DollarSign, FileText, Users, AlertOctagon, Clock, Shield, Star } from 'lucide-react';
import { Button } from '@/components/ui/Button';
import { Card, CardContent, CardHeader } from '@/components/ui/Card';
import supplierService, { ProviderIndicators } from '@/lib/supplier-service';

export default function ContractIndicatorsPage() {
  const router = useRouter();
  const [indicators, setIndicators] = useState<ProviderIndicators | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');

  useEffect(() => {
    const fetchIndicators = async () => {
      setLoading(true);
      setError('');
      try {
        const data = await supplierService.getIndicators();
        setIndicators(data);
      } catch {
        setError('Error al cargar los indicadores.');
      } finally {
        setLoading(false);
      }
    };
    fetchIndicators();
  }, []);

  const formatCurrency = (v: number) => new Intl.NumberFormat('es-CO', { style: 'currency', currency: 'COP', maximumFractionDigits: 0 }).format(v);

  if (loading) {
    return <div className="flex justify-center py-20"><Loader2 className="w-8 h-8 animate-spin text-emerald-600" /></div>;
  }

  if (error || !indicators) {
    return (
      <div className="space-y-6 max-w-2xl mx-auto">
        <button onClick={() => router.push('/contracts')} className="inline-flex items-center gap-2 text-sm text-muted-foreground hover:text-foreground">
          <ArrowLeft className="w-4 h-4" /> Volver
        </button>
        <div className="flex flex-col items-center gap-3 text-rose-600 py-12">
          <AlertTriangle className="w-10 h-10" />
          <p className="font-semibold">{error || 'Error al cargar indicadores.'}</p>
        </div>
      </div>
    );
  }

  const kpis = [
    { label: 'Total Proveedores', value: indicators.totalProviders, icon: Users, color: 'text-blue-600 bg-blue-50' },
    { label: 'Proveedores Activos', value: indicators.activeProviders, icon: Users, color: 'text-emerald-600 bg-emerald-50' },
    { label: 'Proveedores Preferidos', value: indicators.preferredProviders, icon: Star, color: 'text-amber-600 bg-amber-50' },
    { label: 'Total Contratos', value: indicators.totalContracts, icon: FileText, color: 'text-violet-600 bg-violet-50' },
    { label: 'Contratos Activos', value: indicators.activeContracts, icon: FileText, color: 'text-emerald-600 bg-emerald-50' },
    { label: 'Contratos por Vencer', value: indicators.expiringContracts, icon: AlertOctagon, color: 'text-orange-600 bg-orange-50' },
    { label: 'Facturas Pendientes', value: indicators.pendingInvoices, icon: Clock, color: 'text-amber-600 bg-amber-50' },
    { label: 'Facturas Vencidas', value: indicators.overdueInvoices, icon: AlertTriangle, color: 'text-rose-600 bg-rose-50' },
    { label: 'Pólizas por Vencer', value: indicators.expiringPolicies, icon: Shield, color: 'text-orange-600 bg-orange-50' },
    { label: 'Alertas Activas', value: indicators.activeAlerts, icon: AlertOctagon, color: 'text-rose-600 bg-rose-50' },
  ];

  return (
    <div className="space-y-6">
      <button onClick={() => router.push('/contracts')} className="inline-flex items-center gap-2 text-sm text-muted-foreground hover:text-foreground transition-colors">
        <ArrowLeft className="w-4 h-4" /> Volver a Contratos
      </button>

      <div>
        <h1 className="text-2xl font-bold text-foreground tracking-tight">Indicadores de Proveedores y Contratos</h1>
        <p className="text-sm text-muted-foreground mt-1">Resumen ejecutivo del módulo de proveedores.</p>
      </div>

      <div className="grid grid-cols-2 md:grid-cols-3 lg:grid-cols-5 gap-4">
        {kpis.map((k) => (
          <Card key={k.label}>
            <CardContent className="p-4 flex items-center gap-3">
              <div className={`w-10 h-10 rounded-xl flex items-center justify-center ${k.color}`}>
                <k.icon className="w-5 h-5" />
              </div>
              <div>
                <p className="text-xs text-muted-foreground font-medium">{k.label}</p>
                <p className="text-xl font-bold text-foreground">{k.value}</p>
              </div>
            </CardContent>
          </Card>
        ))}
      </div>

      <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
        <Card>
          <CardHeader className="py-3 px-6">
            <h3 className="text-sm font-bold text-foreground flex items-center gap-2">
              <DollarSign className="w-4 h-4 text-emerald-600" /> Valores de Contratos
            </h3>
          </CardHeader>
          <CardContent className="p-6 space-y-4">
            <div className="flex justify-between items-center p-3 bg-muted/30 rounded-lg">
              <span className="text-sm text-muted-foreground">Valor Total Contratos Activos</span>
              <span className="text-lg font-bold text-emerald-600">{formatCurrency(indicators.totalContractValue)}</span>
            </div>
            <div className="flex justify-between items-center p-3 bg-muted/30 rounded-lg">
              <span className="text-sm text-muted-foreground">Valor Mensual Contratos Activos</span>
              <span className="text-lg font-bold text-emerald-600">{formatCurrency(indicators.monthlyContractValue)}</span>
            </div>
            <div className="flex justify-between items-center p-3 bg-muted/30 rounded-lg">
              <span className="text-sm text-muted-foreground">Monto Total Facturas Pendientes</span>
              <span className="text-lg font-bold text-orange-600">{formatCurrency(indicators.pendingInvoiceAmount)}</span>
            </div>
          </CardContent>
        </Card>

        <Card>
          <CardHeader className="py-3 px-6">
            <h3 className="text-sm font-bold text-foreground flex items-center gap-2">
              <AlertOctagon className="w-4 h-4 text-orange-600" /> Alertas y Vencimientos
            </h3>
          </CardHeader>
          <CardContent className="p-6 space-y-4">
            <div className="flex justify-between items-center p-3 bg-orange-50 dark:bg-orange-950/20 rounded-lg">
              <span className="text-sm text-orange-800 dark:text-orange-300">Contratos por Vencer (90 días)</span>
              <span className="text-lg font-bold text-orange-600">{indicators.expiringContracts}</span>
            </div>
            <div className="flex justify-between items-center p-3 bg-orange-50 dark:bg-orange-950/20 rounded-lg">
              <span className="text-sm text-orange-800 dark:text-orange-300">Pólizas por Vencer (30 días)</span>
              <span className="text-lg font-bold text-orange-600">{indicators.expiringPolicies}</span>
            </div>
            <div className="flex justify-between items-center p-3 bg-rose-50 dark:bg-rose-950/20 rounded-lg">
              <span className="text-sm text-rose-800 dark:text-rose-300">Alertas Activas</span>
              <span className="text-lg font-bold text-rose-600">{indicators.activeAlerts}</span>
            </div>
          </CardContent>
        </Card>
      </div>
    </div>
  );
}
