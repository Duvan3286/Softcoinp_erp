'use client';

import React, { useState, useEffect, useCallback } from 'react';
import { useRouter } from 'next/navigation';
import { Loader2, Plus, Search, AlertTriangle, Eye, Wrench, AlertCircle, CheckCircle, XCircle } from 'lucide-react';
import { Button } from '@/components/ui/Button';
import { Card, CardContent } from '@/components/ui/Card';
import maintenanceService, { CommonAssetListItem, MaintenanceIndicators } from '@/lib/maintenance-service';

const categoryLabels: Record<string, string> = {
  Structure: 'Estructura',
  ElectricalEquipment: 'Eléctricos',
  HydraulicEquipment: 'Hidráulicos',
  SafetyEquipment: 'Seguridad',
  RecreationalAreas: 'Recreativas',
  GreenAreas: 'Zonas Verdes',
};

const statusLabels: Record<string, string> = {
  Operational: 'Operativo',
  OperationalWithObservations: 'Operativo con Obs.',
  UnderMaintenance: 'En Mantenimiento',
  OutOfService: 'Fuera de Servicio',
  Decommissioned: 'Dado de Baja',
};

const statusBadge = (status: string) => {
  if (status === 'Operational') return <span className="badge-success">Operativo</span>;
  if (status === 'OperationalWithObservations') return <span className="badge-warning">Con Obs.</span>;
  if (status === 'UnderMaintenance') return <span className="badge-info">En Mant.</span>;
  if (status === 'OutOfService') return <span className="badge-danger">Fuera de Serv.</span>;
  if (status === 'Decommissioned') return <span className="badge-neutral">Baja</span>;
  return <span className="badge-neutral">{status}</span>;
};

export default function MaintenanceAssetsPage() {
  const router = useRouter();
  const [assets, setAssets] = useState<CommonAssetListItem[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');
  const [categoryFilter, setCategoryFilter] = useState('');
  const [statusFilter, setStatusFilter] = useState('');
  const [searchTerm, setSearchTerm] = useState('');
  const [indicators, setIndicators] = useState<MaintenanceIndicators | null>(null);

  const fetchAssets = useCallback(async () => {
    setLoading(true);
    setError('');
    try {
      const data = await maintenanceService.getAssets(
        categoryFilter || undefined,
        statusFilter || undefined,
        undefined,
        searchTerm || undefined
      );
      setAssets(data);
    } catch {
      setError('Error al cargar el inventario de bienes.');
    } finally {
      setLoading(false);
    }
  }, [categoryFilter, statusFilter, searchTerm]);

  const fetchIndicators = async () => {
    try {
      const data = await maintenanceService.getIndicators();
      setIndicators(data);
    } catch {}
  };

  useEffect(() => { fetchAssets(); }, [fetchAssets]);
  useEffect(() => { fetchIndicators(); }, []);
  useEffect(() => {
    const timer = setTimeout(() => { fetchAssets(); }, 400);
    return () => clearTimeout(timer);
  }, [searchTerm]);

  const formatCurrency = (val: number) =>
    new Intl.NumberFormat('es-CO', { style: 'currency', currency: 'COP', maximumFractionDigits: 0 }).format(val);

  const formatDate = (d: string) => new Date(d).toLocaleDateString('es-CO', { day: '2-digit', month: '2-digit', year: 'numeric' });

  return (
    <div className="space-y-6">
      <div className="flex flex-col sm:flex-row justify-between items-start sm:items-center gap-4">
        <div>
          <h1 className="text-2xl font-bold text-foreground tracking-tight">Inventario de Bienes Comunes</h1>
          <p className="text-sm text-muted-foreground mt-1">Gestiona el inventario físico de los bienes del conjunto.</p>
        </div>
        <Button onClick={() => router.push('/maintenance/new')}>
          <Plus className="w-4 h-4 mr-2" /> Nuevo Bien
        </Button>
      </div>

      {indicators && (
        <div className="grid grid-cols-2 md:grid-cols-5 gap-4">
          <Card><CardContent className="p-4 text-center">
            <p className="text-2xl font-bold text-foreground">{indicators.totalAssets}</p>
            <p className="text-xs text-muted-foreground">Total Bienes</p>
          </CardContent></Card>
          <Card><CardContent className="p-4 text-center">
            <p className="text-2xl font-bold text-emerald-600">{indicators.operationalAssets}</p>
            <p className="text-xs text-muted-foreground">Operativos</p>
          </CardContent></Card>
          <Card><CardContent className="p-4 text-center">
            <p className="text-2xl font-bold text-rose-600">{indicators.outOfServiceAssets}</p>
            <p className="text-xs text-muted-foreground">Fuera de Servicio</p>
          </CardContent></Card>
          <Card><CardContent className="p-4 text-center">
            <p className="text-2xl font-bold text-amber-600">{indicators.pendingWorkOrders}</p>
            <p className="text-xs text-muted-foreground">Órdenes Pendientes</p>
          </CardContent></Card>
          <Card><CardContent className="p-4 text-center">
            <p className="text-2xl font-bold text-blue-600">{indicators.upcomingMaintenances30Days}</p>
            <p className="text-xs text-muted-foreground">Mant. Próx. 30 días</p>
          </CardContent></Card>
        </div>
      )}

      <Card>
        <CardContent className="p-4">
          <div className="flex flex-col md:flex-row gap-3">
            <select value={categoryFilter} onChange={(e) => setCategoryFilter(e.target.value)}
              className="bg-transparent border-b border-emerald-600/30 focus:border-emerald-600 text-sm font-medium py-2 outline-none">
              <option value="">Todas las categorías</option>
              {Object.entries(categoryLabels).map(([k, v]) => <option key={k} value={k}>{v}</option>)}
            </select>
            <select value={statusFilter} onChange={(e) => setStatusFilter(e.target.value)}
              className="bg-transparent border-b border-emerald-600/30 focus:border-emerald-600 text-sm font-medium py-2 outline-none">
              <option value="">Todos los estados</option>
              {Object.entries(statusLabels).map(([k, v]) => <option key={k} value={k}>{v}</option>)}
            </select>
            <div className="flex-1 relative">
              <Search className="absolute left-0 top-1/2 -translate-y-1/2 w-4 h-4 text-muted-foreground" />
              <input type="text" placeholder="Buscar por nombre, marca o modelo..." value={searchTerm}
                onChange={(e) => setSearchTerm(e.target.value)}
                className="w-full bg-transparent border-b border-emerald-600/30 focus:border-emerald-600 text-sm font-medium py-2 pl-6 outline-none" />
            </div>
          </div>
        </CardContent>
      </Card>

      {loading ? (
        <div className="flex justify-center py-20"><Loader2 className="w-8 h-8 animate-spin text-emerald-600" /></div>
      ) : error ? (
        <div className="flex flex-col items-center gap-3 text-rose-600 py-12">
          <AlertTriangle className="w-10 h-10" />
          <p className="font-semibold">{error}</p>
          <Button variant="secondary" onClick={fetchAssets}>Reintentar</Button>
        </div>
      ) : assets.length === 0 ? (
        <div className="flex flex-col items-center gap-3 text-muted-foreground py-12">
          <Wrench className="w-10 h-10" />
          <p className="font-semibold">No hay bienes registrados</p>
          <p className="text-sm">Crea el primer bien del inventario.</p>
        </div>
      ) : (
        <Card>
          <CardContent className="p-0">
            <div className="overflow-x-auto">
              <table className="min-w-full divide-y divide-border">
                <thead className="bg-muted/50">
                  <tr>
                    <th className="px-5 py-3 text-left text-xs font-bold text-muted-foreground uppercase">Nombre</th>
                    <th className="px-5 py-3 text-left text-xs font-bold text-muted-foreground uppercase">Categoría</th>
                    <th className="px-5 py-3 text-left text-xs font-bold text-muted-foreground uppercase">Ubicación</th>
                    <th className="px-5 py-3 text-left text-xs font-bold text-muted-foreground uppercase">Estado</th>
                    <th className="px-5 py-3 text-center text-xs font-bold text-muted-foreground uppercase">Esencial</th>
                    <th className="px-5 py-3 text-left text-xs font-bold text-muted-foreground uppercase">Próx. Mant.</th>
                    <th className="px-5 py-3 text-center text-xs font-bold text-muted-foreground uppercase">Órdenes</th>
                    <th className="px-5 py-3 text-right text-xs font-bold text-muted-foreground uppercase">Acciones</th>
                  </tr>
                </thead>
                <tbody className="divide-y divide-border">
                  {assets.map((a) => (
                    <tr key={a.id} className="hover:bg-muted/30 transition-colors">
                      <td className="px-5 py-3">
                        <p className="font-medium text-sm">{a.name}</p>
                        <p className="text-xs text-muted-foreground">{a.brand} {a.model}</p>
                      </td>
                      <td className="px-5 py-3 text-sm text-muted-foreground">{categoryLabels[a.category] || a.category}</td>
                      <td className="px-5 py-3 text-sm text-muted-foreground">{a.location}</td>
                      <td className="px-5 py-3">{statusBadge(a.status)}</td>
                      <td className="px-5 py-3 text-center">
                        {a.isEssential ? <CheckCircle className="w-4 h-4 text-emerald-600 inline" /> : <XCircle className="w-4 h-4 text-muted-foreground inline" />}
                      </td>
                      <td className="px-5 py-3 text-sm text-muted-foreground">
                        {a.nextMaintenanceDate ? formatDate(a.nextMaintenanceDate) : '—'}
                      </td>
                      <td className="px-5 py-3 text-center">
                        {a.pendingWorkOrders > 0 ? (
                          <span className="badge-warning">{a.pendingWorkOrders}</span>
                        ) : (
                          <span className="text-xs text-muted-foreground">0</span>
                        )}
                      </td>
                      <td className="px-5 py-3 text-right">
                        <button onClick={() => router.push(`/maintenance/${a.id}`)}
                          className="text-emerald-600 hover:text-emerald-800 text-sm font-semibold">Ver</button>
                      </td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>
            <div className="px-5 py-3 border-t border-border text-xs text-muted-foreground">
              {assets.length} bien(es) encontrado(s)
            </div>
          </CardContent>
        </Card>
      )}
    </div>
  );
}
