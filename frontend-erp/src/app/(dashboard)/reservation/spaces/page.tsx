'use client';

import React, { useState, useEffect, useCallback } from 'react';
import { useRouter } from 'next/navigation';
import { Loader2, Plus, Search, MapPin, Users, DollarSign, CheckCircle, XCircle } from 'lucide-react';
import { Button } from '@/components/ui/Button';
import { Card, CardContent } from '@/components/ui/Card';
import reservationService, { ReservableSpaceListItem } from '@/lib/reservation-service';

const approvalModeLabels: Record<string, string> = {
  Automatic: 'Automática',
  Manual: 'Manual',
};

const arrearsPolicyLabels: Record<string, string> = {
  Block: 'Bloquear',
  Warn: 'Advertir',
};

const chargeTypeLabels: Record<string, string> = {
  PerHour: 'Por Hora',
  PerEvent: 'Por Evento',
  Other: 'Otro',
};

export default function ReservationSpacesPage() {
  const router = useRouter();
  const [spaces, setSpaces] = useState<ReservableSpaceListItem[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');
  const [searchTerm, setSearchTerm] = useState('');
  const [isActiveFilter, setIsActiveFilter] = useState<boolean | undefined>(undefined);

  const fetchSpaces = useCallback(async () => {
    setLoading(true);
    setError('');
    try {
      const data = await reservationService.getSpaces(isActiveFilter);
      setSpaces(data);
    } catch {
      setError('Error al cargar los espacios reservables.');
    } finally {
      setLoading(false);
    }
  }, [isActiveFilter]);

  useEffect(() => { fetchSpaces(); }, [fetchSpaces]);
  useEffect(() => {
    const timer = setTimeout(() => { fetchSpaces(); }, 400);
    return () => clearTimeout(timer);
  }, [searchTerm]);

  const formatCurrency = (val: number) =>
    new Intl.NumberFormat('es-CO', { style: 'currency', currency: 'COP', maximumFractionDigits: 0 }).format(val);

  const filteredSpaces = spaces.filter(s =>
    s.name.toLowerCase().includes(searchTerm.toLowerCase()) ||
    (s.location && s.location.toLowerCase().includes(searchTerm.toLowerCase()))
  );

  return (
    <div className="space-y-6">
      <div className="flex flex-col sm:flex-row justify-between items-start sm:items-center gap-4">
        <div>
          <h1 className="text-2xl font-bold text-foreground tracking-tight">Espacios Reservables</h1>
          <p className="text-sm text-muted-foreground mt-1">Configura los espacios disponibles para reservas.</p>
        </div>
        <Button onClick={() => router.push('/reservation/spaces/new')}>
          <Plus className="w-4 h-4 mr-2" /> Nuevo Espacio
        </Button>
      </div>

      <div className="flex flex-col sm:flex-row gap-3">
        <div className="relative flex-1">
          <Search className="absolute left-3 top-1/2 -translate-y-1/2 w-4 h-4 text-muted-foreground" />
          <input
            type="text"
            placeholder="Buscar por nombre o ubicación..."
            value={searchTerm}
            onChange={(e) => setSearchTerm(e.target.value)}
            className="w-full pl-10 pr-4 py-2 border-b border-emerald-600/30 focus:border-emerald-600 text-sm font-medium outline-none"
          />
        </div>
        <select
          value={isActiveFilter === undefined ? '' : isActiveFilter.toString()}
          onChange={(e) => setIsActiveFilter(e.target.value === '' ? undefined : e.target.value === 'true')}
          className="border-b border-emerald-600/30 focus:border-emerald-600 text-sm font-medium py-2 outline-none"
        >
          <option value="">Todos los estados</option>
          <option value="true">Activos</option>
          <option value="false">Inactivos</option>
        </select>
      </div>

      {error && (
        <div className="bg-red-50 dark:bg-red-900/20 border border-red-200 dark:border-red-800 text-red-700 dark:text-red-300 px-4 py-3 rounded-lg text-sm">
          {error}
        </div>
      )}

      {loading ? (
        <div className="flex justify-center items-center py-12">
          <Loader2 className="w-8 h-8 animate-spin text-emerald-600" />
        </div>
      ) : filteredSpaces.length === 0 ? (
        <Card>
          <CardContent className="p-12 text-center">
            <p className="text-muted-foreground">No se encontraron espacios reservables.</p>
          </CardContent>
        </Card>
      ) : (
        <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-4">
          {filteredSpaces.map((space) => (
            <div key={space.id} className="hover:shadow-md transition-shadow cursor-pointer" onClick={() => router.push(`/reservation/spaces/${space.id}`)}>
            <Card>
              <CardContent className="p-4">
                <div className="flex justify-between items-start mb-3">
                  <div>
                    <h3 className="font-bold text-foreground">{space.name}</h3>
                    {space.location && (
                      <div className="flex items-center gap-1 text-xs text-muted-foreground mt-1">
                        <MapPin className="w-3 h-3" />
                        <span>{space.location}</span>
                      </div>
                    )}
                  </div>
                  <span className={space.isActive ? 'badge-success' : 'badge-neutral'}>
                    {space.isActive ? 'Activo' : 'Inactivo'}
                  </span>
                </div>

                <div className="grid grid-cols-2 gap-2 text-xs mb-3">
                  <div className="flex items-center gap-1">
                    <Users className="w-3 h-3 text-muted-foreground" />
                    <span>Capacidad: {space.maxCapacity}</span>
                  </div>
                  <div className="flex items-center gap-1">
                    <DollarSign className="w-3 h-3 text-muted-foreground" />
                    <span>{space.hasAdditionalCost ? chargeTypeLabels[space.chargeType] || space.chargeType : 'Sin costo'}</span>
                  </div>
                </div>

                <div className="flex flex-wrap gap-2 text-xs">
                  <span className="badge-info">{approvalModeLabels[space.approvalMode] || space.approvalMode}</span>
                  <span className="badge-warning">{arrearsPolicyLabels[space.arrearsPolicy] || space.arrearsPolicy}</span>
                  {space.requiresDeposit && <span className="badge-danger">Depósito: {formatCurrency(space.depositAmount)}</span>}
                </div>

                <div className="mt-3 pt-3 border-t border-border flex justify-between items-center text-xs text-muted-foreground">
                  <span>{space.activeReservations} reservas activas</span>
                  {space.hasAdditionalCost && (
                    <span className="font-medium text-foreground">
                      {space.chargeType === 'PerHour' ? `${formatCurrency(space.hourlyRate)}/hora` : formatCurrency(space.eventRate)}
                    </span>
                  )}
                </div>
              </CardContent>
            </Card>
            </div>
          ))}
        </div>
      )}
    </div>
  );
}
