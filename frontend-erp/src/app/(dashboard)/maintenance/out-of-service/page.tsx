'use client';

import React, { useState, useEffect } from 'react';
import { useRouter } from 'next/navigation';
import { Loader2, AlertTriangle, Plus, AlertCircle } from 'lucide-react';
import { Button } from '@/components/ui/Button';
import { Card, CardContent } from '@/components/ui/Card';
import maintenanceService, { OutOfServiceAsset } from '@/lib/maintenance-service';

const categoryLabels: Record<string, string> = {
  Structure: 'Estructura',
  ElectricalEquipment: 'Eléctricos',
  HydraulicEquipment: 'Hidráulicos',
  SafetyEquipment: 'Seguridad',
  RecreationalAreas: 'Recreativas',
  GreenAreas: 'Zonas Verdes',
};

export default function OutOfServicePage() {
  const router = useRouter();
  const [assets, setAssets] = useState<OutOfServiceAsset[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');

  const fetchAssets = async () => {
    setLoading(true);
    setError('');
    try {
      const data = await maintenanceService.getOutOfServiceAssets();
      setAssets(data);
    } catch {
      setError('Error al cargar los bienes fuera de servicio.');
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => { fetchAssets(); }, []);

  return (
    <div className="space-y-6">
      <div className="flex flex-col sm:flex-row justify-between items-start sm:items-center gap-4">
        <div>
          <h1 className="text-2xl font-bold text-foreground tracking-tight">Bienes Fuera de Servicio</h1>
          <p className="text-sm text-muted-foreground mt-1">Bienes que requieren atención inmediata.</p>
        </div>
      </div>

      {loading ? (
        <div className="flex justify-center py-20"><Loader2 className="w-8 h-8 animate-spin text-emerald-600" /></div>
      ) : error ? (
        <div className="flex flex-col items-center gap-3 text-rose-600 py-12">
          <AlertTriangle className="w-10 h-10" />
          <p className="font-semibold">{error}</p>
        </div>
      ) : assets.length === 0 ? (
        <div className="flex flex-col items-center gap-3 text-muted-foreground py-12">
          <AlertCircle className="w-10 h-10" />
          <p className="font-semibold">No hay bienes fuera de servicio</p>
          <p className="text-sm">Todos los bienes están operativos.</p>
        </div>
      ) : (
        <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
          {assets.map((a) => (
            <Card key={a.id}>
              <CardContent className="p-5">
                <div className="flex justify-between items-start">
                  <div>
                    <h3 className="font-bold text-foreground">{a.name}</h3>
                    <p className="text-sm text-muted-foreground mt-1">{categoryLabels[a.category] || a.category} — {a.location}</p>
                    <div className="flex flex-wrap gap-2 mt-2">
                      {a.isEssential && (
                        <span className="badge-danger">Bien Esencial — Alerta enviada al Consejo</span>
                      )}
                      {a.hasReservationBlock && (
                        <span className="badge-warning">Reservas bloqueadas</span>
                      )}
                    </div>
                  </div>
                  <div className="text-right">
                    <p className="text-2xl font-bold text-rose-600">{a.daysOutOfService}</p>
                    <p className="text-xs text-muted-foreground">días fuera</p>
                  </div>
                </div>
                {a.reason && (
                  <p className="text-sm text-muted-foreground mt-3 border-t border-border pt-3">
                    <span className="font-medium">Motivo:</span> {a.reason}
                  </p>
                )}
                <button onClick={() => router.push(`/maintenance/${a.id}`)}
                  className="mt-3 text-emerald-600 hover:text-emerald-800 text-sm font-semibold">
                  Ver detalle del bien →
                </button>
              </CardContent>
            </Card>
          ))}
        </div>
      )}
    </div>
  );
}
