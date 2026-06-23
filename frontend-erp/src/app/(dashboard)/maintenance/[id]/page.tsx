'use client';

import React, { useState, useEffect } from 'react';
import { useRouter, useParams } from 'next/navigation';
import { Loader2, ArrowLeft, AlertTriangle, Edit, Trash2, Plus, Wrench, Calendar, Shield, MapPin, Star } from 'lucide-react';
import { Button } from '@/components/ui/Button';
import { Card, CardContent, CardHeader } from '@/components/ui/Card';
import maintenanceService, { CommonAssetDetail } from '@/lib/maintenance-service';

const categoryLabels: Record<string, string> = {
  Structure: 'Estructura', ElectricalEquipment: 'Eléctricos', HydraulicEquipment: 'Hidráulicos',
  SafetyEquipment: 'Seguridad', RecreationalAreas: 'Recreativas', GreenAreas: 'Zonas Verdes',
};
const statusLabels: Record<string, string> = {
  Operational: 'Operativo', OperationalWithObservations: 'Operativo con Obs.',
  UnderMaintenance: 'En Mantenimiento', OutOfService: 'Fuera de Servicio', Decommissioned: 'Dado de Baja',
};
const priorityLabels: Record<string, string> = { Emergency: 'Emergencia', High: 'Alta', Medium: 'Media', Low: 'Baja' };

const statusBadge = (status: string) => {
  if (status === 'Operational') return <span className="badge-success">Operativo</span>;
  if (status === 'OperationalWithObservations') return <span className="badge-warning">Con Obs.</span>;
  if (status === 'UnderMaintenance') return <span className="badge-info">En Mant.</span>;
  if (status === 'OutOfService') return <span className="badge-danger">Fuera de Serv.</span>;
  if (status === 'Decommissioned') return <span className="badge-neutral">Baja</span>;
  return <span className="badge-neutral">{status}</span>;
};

export default function AssetDetailPage() {
  const router = useRouter();
  const params = useParams();
  const id = params.id as string;
  const [asset, setAsset] = useState<CommonAssetDetail | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');

  const fetchAsset = async () => {
    setLoading(true);
    setError('');
    try {
      const data = await maintenanceService.getAssetById(id);
      setAsset(data);
    } catch {
      setError('Error al cargar el bien.');
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => { fetchAsset(); }, [id]);

  const formatDate = (d: string | null) => d ? new Date(d).toLocaleDateString('es-CO', { day: '2-digit', month: '2-digit', year: 'numeric' }) : '—';
  const formatCurrency = (val: number) => new Intl.NumberFormat('es-CO', { style: 'currency', currency: 'COP', maximumFractionDigits: 0 }).format(val);

  const handleDelete = async () => {
    if (!confirm('¿Está seguro de eliminar este bien?')) return;
    try {
      await maintenanceService.deleteAsset(id);
      router.push('/maintenance');
    } catch (err: any) {
      alert(err?.response?.data?.error || 'Error al eliminar.');
    }
  };

  if (loading) return <div className="flex justify-center py-20"><Loader2 className="w-8 h-8 animate-spin text-emerald-600" /></div>;
  if (error || !asset) return (
    <div className="space-y-6 max-w-2xl mx-auto">
      <button onClick={() => router.push('/maintenance')} className="inline-flex items-center gap-2 text-sm text-muted-foreground hover:text-foreground">
        <ArrowLeft className="w-4 h-4" /> Volver
      </button>
      <div className="flex flex-col items-center gap-3 text-rose-600 py-12">
        <AlertTriangle className="w-10 h-10" />
        <p className="font-semibold">{error || 'Bien no encontrado.'}</p>
      </div>
    </div>
  );

  return (
    <div className="space-y-6">
      <button onClick={() => router.push('/maintenance')} className="inline-flex items-center gap-2 text-sm text-muted-foreground hover:text-foreground transition-colors">
        <ArrowLeft className="w-4 h-4" /> Volver al Inventario
      </button>

      <div className="flex flex-col sm:flex-row justify-between items-start sm:items-center gap-4">
        <div>
          <div className="flex items-center gap-2">
            <h1 className="text-2xl font-bold text-foreground tracking-tight">{asset.name}</h1>
            {asset.isEssential && <Star className="w-5 h-5 text-amber-500 fill-amber-500" />}
          </div>
          <p className="text-sm text-muted-foreground mt-1">
            {categoryLabels[asset.category] || asset.category} — {asset.location}
          </p>
        </div>
        <div className="flex gap-2">
          {statusBadge(asset.status)}
          <Button variant="danger" onClick={handleDelete}>
            <Trash2 className="w-4 h-4 mr-1" /> Eliminar
          </Button>
        </div>
      </div>

      <div className="grid grid-cols-1 lg:grid-cols-3 gap-6">
        <div className="lg:col-span-2 space-y-6">
          <Card>
            <CardHeader className="py-3 px-6"><h3 className="text-sm font-bold text-foreground">Ficha Técnica</h3></CardHeader>
            <CardContent className="p-6">
              <div className="grid grid-cols-2 md:grid-cols-3 gap-4 text-sm">
                <div><span className="text-muted-foreground">Marca:</span><p className="font-medium">{asset.brand || '—'}</p></div>
                <div><span className="text-muted-foreground">Modelo:</span><p className="font-medium">{asset.model || '—'}</p></div>
                <div><span className="text-muted-foreground">Nro. Serie:</span><p className="font-medium">{asset.serialNumber || '—'}</p></div>
                <div><span className="text-muted-foreground">Fabricante:</span><p className="font-medium">{asset.manufacturer || '—'}</p></div>
                <div><span className="text-muted-foreground">Fecha Adquisición:</span><p className="font-medium">{formatDate(asset.acquisitionDate)}</p></div>
                <div><span className="text-muted-foreground">Valor Adquisición:</span><p className="font-medium">{formatCurrency(asset.acquisitionValue)}</p></div>
                <div><span className="text-muted-foreground">Vida Útil:</span><p className="font-medium">{asset.estimatedUsefulLifeMonths} meses</p></div>
                <div><span className="text-muted-foreground">Proveedor Ref.:</span><p className="font-medium">{asset.referenceProviderName || '—'}</p></div>
                <div><span className="text-muted-foreground">Creado:</span><p className="font-medium">{formatDate(asset.createdAt)}</p></div>
              </div>
            </CardContent>
          </Card>

          <Card>
            <CardHeader className="py-3 px-6"><h3 className="text-sm font-bold text-foreground">Garantía</h3></CardHeader>
            <CardContent className="p-6">
              <div className="grid grid-cols-2 gap-4 text-sm">
                <div><span className="text-muted-foreground">Estado:</span><p className="font-medium">{asset.hasWarranty ? 'Vigente' : 'Sin garantía'}</p></div>
                {asset.hasWarranty && (
                  <div><span className="text-muted-foreground">Vence:</span><p className="font-medium">{formatDate(asset.warrantyEndDate)}</p></div>
                )}
              </div>
            </CardContent>
          </Card>

          <Card>
            <CardHeader className="py-3 px-6 flex items-center justify-between">
              <h3 className="text-sm font-bold text-foreground">Planes de Mantenimiento</h3>
              <Button variant="secondary" onClick={() => router.push(`/maintenance/${id}/plans/new`)}>
                <Plus className="w-4 h-4 mr-1" /> Nuevo Plan
              </Button>
            </CardHeader>
            <CardContent className="p-0">
              {asset.maintenancePlans.length === 0 ? (
                <p className="px-6 py-8 text-center text-sm text-muted-foreground">No hay planes de mantenimiento.</p>
              ) : (
                <div className="divide-y divide-border">
                  {asset.maintenancePlans.map((p) => (
                    <div key={p.id} className="px-6 py-4">
                      <div className="flex justify-between items-center">
                        <div>
                          <p className="text-sm font-medium">{p.activityType} — {p.description}</p>
                          <p className="text-xs text-muted-foreground mt-1">Cada {p.frequencyDays} días | {p.preferredProviderName || 'Sin proveedor'}</p>
                        </div>
                        <div className="text-right">
                          <p className="text-sm font-medium">{formatCurrency(p.estimatedCost)}</p>
                          <p className="text-xs text-muted-foreground">Próx: {formatDate(p.nextExecutionDate)}</p>
                        </div>
                      </div>
                    </div>
                  ))}
                </div>
              )}
            </CardContent>
          </Card>

          <Card>
            <CardHeader className="py-3 px-6 flex items-center justify-between">
              <h3 className="text-sm font-bold text-foreground">Órdenes de Trabajo</h3>
              <Button variant="secondary" onClick={() => router.push(`/maintenance/work-orders/new?assetId=${id}`)}>
                <Plus className="w-4 h-4 mr-1" /> Nueva Orden
              </Button>
            </CardHeader>
            <CardContent className="p-0">
              {asset.workOrders.length === 0 ? (
                <p className="px-6 py-8 text-center text-sm text-muted-foreground">No hay órdenes de trabajo.</p>
              ) : (
                <div className="overflow-x-auto">
                  <table className="min-w-full divide-y divide-border">
                    <thead className="bg-muted/50">
                      <tr>
                        <th className="px-5 py-3 text-left text-xs font-bold text-muted-foreground uppercase">Tipo</th>
                        <th className="px-5 py-3 text-left text-xs font-bold text-muted-foreground uppercase">Descripción</th>
                        <th className="px-5 py-3 text-left text-xs font-bold text-muted-foreground uppercase">Prioridad</th>
                        <th className="px-5 py-3 text-left text-xs font-bold text-muted-foreground uppercase">Estado</th>
                        <th className="px-5 py-3 text-right text-xs font-bold text-muted-foreground uppercase">Acciones</th>
                      </tr>
                    </thead>
                    <tbody className="divide-y divide-border">
                      {asset.workOrders.map((w) => (
                        <tr key={w.id} className="hover:bg-muted/30 transition-colors">
                          <td className="px-5 py-3 text-sm">{w.orderType === 'Preventive' ? 'Preventivo' : 'Correctivo'}</td>
                          <td className="px-5 py-3 text-sm text-muted-foreground max-w-[200px] truncate">{w.description}</td>
                          <td className="px-5 py-3 text-sm">{priorityLabels[w.priority] || w.priority}</td>
                          <td className="px-5 py-3 text-sm">{w.status}</td>
                          <td className="px-5 py-3 text-right">
                            <button onClick={() => router.push(`/maintenance/work-orders/${w.id}`)}
                              className="text-emerald-600 hover:text-emerald-800 text-sm font-semibold">Ver</button>
                          </td>
                        </tr>
                      ))}
                    </tbody>
                  </table>
                </div>
              )}
            </CardContent>
          </Card>
        </div>

        <div className="space-y-6">
          <Card>
            <CardHeader className="py-3 px-6"><h3 className="text-sm font-bold text-foreground">Fotografías</h3></CardHeader>
            <CardContent className="p-4">
              {asset.photos.length === 0 ? (
                <p className="text-center text-sm text-muted-foreground py-4">No hay fotografías.</p>
              ) : (
                <div className="grid grid-cols-2 gap-2">
                  {asset.photos.map((p) => (
                    <div key={p.id} className="aspect-square bg-muted rounded-lg flex items-center justify-center text-xs text-muted-foreground">
                      {formatDate(p.capturedAt)}
                    </div>
                  ))}
                </div>
              )}
            </CardContent>
          </Card>

          <Card>
            <CardHeader className="py-3 px-6"><h3 className="text-sm font-bold text-foreground">Historial de Estados</h3></CardHeader>
            <CardContent className="p-4">
              {asset.statusHistory.length === 0 ? (
                <p className="text-center text-sm text-muted-foreground py-4">Sin historial.</p>
              ) : (
                <div className="space-y-3">
                  {asset.statusHistory.map((h) => (
                    <div key={h.id} className="p-3 bg-muted/30 rounded-lg">
                      <div className="flex justify-between items-center">
                        <span className="text-xs font-bold text-muted-foreground">{formatDate(h.changedAt)}</span>
                      </div>
                      <p className="text-sm mt-1">
                        <span className="text-muted-foreground">{statusLabels[h.previousStatus] || h.previousStatus}</span>
                        {' → '}
                        <span className="font-medium">{statusLabels[h.newStatus] || h.newStatus}</span>
                      </p>
                      {h.reason && <p className="text-xs text-muted-foreground mt-1">{h.reason}</p>}
                    </div>
                  ))}
                </div>
              )}
            </CardContent>
          </Card>
        </div>
      </div>
    </div>
  );
}
