'use client';

import React, { useState, useEffect } from 'react';
import { useRouter } from 'next/navigation';
import { Loader2, Plus, Eye, AlertTriangle, Users, Briefcase, Star, Search, Filter, Truck } from 'lucide-react';
import { Button } from '@/components/ui/Button';
import { Card, CardContent } from '@/components/ui/Card';
import supplierService, { ProviderListItem, ProviderIndicators } from '@/lib/supplier-service';

type StatusFilter = '' | 'Active' | 'Inactive';
type TypeFilter = '' | 'Natural' | 'Legal';

const statusLabels: Record<string, string> = {
  Active: 'Activo',
  Inactive: 'Inactivo',
};

const typeLabels: Record<string, string> = {
  Natural: 'Natural',
  Legal: 'Jurídica',
};

export default function SuppliersPage() {
  const router = useRouter();
  const [providers, setProviders] = useState<ProviderListItem[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');
  const [statusFilter, setStatusFilter] = useState<StatusFilter>('');
  const [typeFilter, setTypeFilter] = useState<TypeFilter>('');
  const [searchTerm, setSearchTerm] = useState('');
  const [indicators, setIndicators] = useState<ProviderIndicators | null>(null);

  const fetchProviders = async () => {
    setLoading(true);
    setError('');
    try {
      const data = await supplierService.getProviders(
        statusFilter || undefined,
        typeFilter || undefined,
        undefined,
        searchTerm || undefined
      );
      setProviders(data);
    } catch {
      setError('Error al cargar los proveedores.');
    } finally {
      setLoading(false);
    }
  };

  const fetchIndicators = async () => {
    try {
      const data = await supplierService.getIndicators();
      setIndicators(data);
    } catch {}
  };

  useEffect(() => {
    fetchProviders();
    fetchIndicators();
  }, [statusFilter, typeFilter]);

  useEffect(() => {
    const timeout = setTimeout(() => {
      fetchProviders();
    }, 400);
    return () => clearTimeout(timeout);
  }, [searchTerm]);

  const formatDate = (d: string) => new Date(d).toLocaleDateString('es-CO', { day: '2-digit', month: '2-digit', year: 'numeric' });

  const statusBadge = (status: string) => {
    const map: Record<string, string> = {
      Active: 'badge-success',
      Inactive: 'badge-neutral',
    };
    return <span className={map[status] || 'badge-neutral'}>{statusLabels[status] || status}</span>;
  };

  const summaryCards = indicators ? [
    { label: 'Total Proveedores', value: indicators.totalProviders, icon: Users, color: 'text-blue-600 bg-blue-50' },
    { label: 'Activos', value: indicators.activeProviders, icon: Briefcase, color: 'text-emerald-600 bg-emerald-50' },
    { label: 'Preferidos', value: indicators.preferredProviders, icon: Star, color: 'text-amber-600 bg-amber-50' },
    { label: 'Contratos Vigentes', value: indicators.activeContracts, icon: Truck, color: 'text-violet-600 bg-violet-50' },
  ] : [];

  return (
    <div className="space-y-6">
      <div className="flex flex-col sm:flex-row justify-between items-start sm:items-center gap-4">
        <div>
          <h1 className="text-2xl font-bold text-foreground tracking-tight">Proveedores</h1>
          <p className="text-sm text-muted-foreground mt-1">Gestión de proveedores y contratistas del conjuntos habitacional.</p>
        </div>
        <Button onClick={() => router.push('/suppliers/new')}>
          <Plus className="w-4 h-4 mr-2" />
          Nuevo Proveedor
        </Button>
      </div>

      {indicators && (
        <div className="grid grid-cols-2 md:grid-cols-4 gap-4">
          {summaryCards.map((c) => (
            <Card key={c.label}>
              <CardContent className="p-4 flex items-center gap-3">
                <div className={`w-10 h-10 rounded-xl flex items-center justify-center ${c.color}`}>
                  <c.icon className="w-5 h-5" />
                </div>
                <div>
                  <p className="text-xs text-muted-foreground font-medium">{c.label}</p>
                  <p className="text-xl font-bold text-foreground">{c.value}</p>
                </div>
              </CardContent>
            </Card>
          ))}
        </div>
      )}

      <Card>
        <CardContent className="p-4 border-b border-border">
          <div className="flex flex-wrap items-center gap-3">
            <div className="flex items-center gap-2 text-sm text-muted-foreground">
              <Filter className="w-4 h-4" />
              <span className="font-semibold">Filtros:</span>
            </div>
            <select value={statusFilter} onChange={(e) => setStatusFilter(e.target.value as StatusFilter)}
              className="bg-transparent border border-border rounded-lg px-3 py-1.5 text-sm text-foreground focus:outline-none focus:ring-1 focus:ring-emerald-500">
              <option value="">Todos los estados</option>
              {Object.entries(statusLabels).map(([k, v]) => (
                <option key={k} value={k}>{v}</option>
              ))}
            </select>
            <select value={typeFilter} onChange={(e) => setTypeFilter(e.target.value as TypeFilter)}
              className="bg-transparent border border-border rounded-lg px-3 py-1.5 text-sm text-foreground focus:outline-none focus:ring-1 focus:ring-emerald-500">
              <option value="">Todos los tipos</option>
              {Object.entries(typeLabels).map(([k, v]) => (
                <option key={k} value={k}>{v}</option>
              ))}
            </select>
            <div className="flex-1 min-w-[200px] relative">
              <Search className="absolute left-3 top-1/2 -translate-y-1/2 w-4 h-4 text-muted-foreground" />
              <input type="text" placeholder="Buscar nombre, documento, contacto..."
                value={searchTerm} onChange={(e) => setSearchTerm(e.target.value)}
                className="w-full bg-transparent border border-border rounded-lg pl-9 pr-3 py-1.5 text-sm text-foreground focus:outline-none focus:ring-1 focus:ring-emerald-500" />
            </div>
          </div>
        </CardContent>

        <CardContent className="p-0">
          <div className="overflow-x-auto">
            <table className="min-w-full divide-y divide-border">
              <thead className="bg-muted/50">
                <tr>
                  <th className="px-5 py-3.5 text-left text-xs font-bold text-muted-foreground uppercase tracking-wider">Proveedor</th>
                  <th className="px-5 py-3.5 text-left text-xs font-bold text-muted-foreground uppercase tracking-wider">Documento</th>
                  <th className="px-5 py-3.5 text-left text-xs font-bold text-muted-foreground uppercase tracking-wider">Tipo</th>
                  <th className="px-5 py-3.5 text-left text-xs font-bold text-muted-foreground uppercase tracking-wider">Contacto</th>
                  <th className="px-5 py-3.5 text-left text-xs font-bold text-muted-foreground uppercase tracking-wider">Ciudad</th>
                  <th className="px-5 py-3.5 text-left text-xs font-bold text-muted-foreground uppercase tracking-wider">Servicio</th>
                  <th className="px-5 py-3.5 text-left text-xs font-bold text-muted-foreground uppercase tracking-wider">Contratos</th>
                  <th className="px-5 py-3.5 text-left text-xs font-bold text-muted-foreground uppercase tracking-wider">Estado</th>
                  <th className="px-5 py-3.5 text-right text-xs font-bold text-muted-foreground uppercase tracking-wider">Acciones</th>
                </tr>
              </thead>
              <tbody className="divide-y divide-border">
                {loading ? (
                  <tr><td colSpan={9} className="px-6 py-12 text-center">
                    <Loader2 className="w-6 h-6 animate-spin mx-auto text-emerald-600" />
                  </td></tr>
                ) : error ? (
                  <tr><td colSpan={9} className="px-6 py-12 text-center">
                    <div className="flex flex-col items-center gap-2 text-rose-600">
                      <AlertTriangle className="w-8 h-8" />
                      <p className="text-sm font-semibold">{error}</p>
                      <Button variant="secondary" onClick={fetchProviders}>Reintentar</Button>
                    </div>
                  </td></tr>
                ) : providers.length === 0 ? (
                  <tr><td colSpan={9} className="px-6 py-12 text-center">
                    <Users className="w-12 h-12 mx-auto text-muted-foreground/40 mb-3" />
                    <p className="font-semibold text-muted-foreground">No se encontraron proveedores</p>
                    <p className="text-sm text-muted-foreground/60 mt-1">Crea un nuevo proveedor para comenzar.</p>
                  </td></tr>
                ) : (
                  providers.map((p) => (
                    <tr key={p.id} className="hover:bg-muted/30 transition-colors">
                      <td className="px-5 py-4 whitespace-nowrap">
                        <div className="flex items-center gap-2">
                          <span className="font-semibold text-sm text-foreground">{p.businessName}</span>
                          {p.isPreferred && <Star className="w-4 h-4 text-amber-500 fill-amber-500" />}
                        </div>
                        {p.tradeName && <p className="text-xs text-muted-foreground mt-0.5">{p.tradeName}</p>}
                      </td>
                      <td className="px-5 py-4 whitespace-nowrap text-sm text-muted-foreground">{p.documentNumber}</td>
                      <td className="px-5 py-4 whitespace-nowrap text-sm text-muted-foreground">{typeLabels[p.providerType] || p.providerType}</td>
                      <td className="px-5 py-4 whitespace-nowrap text-sm text-muted-foreground">{p.contactName}</td>
                      <td className="px-5 py-4 whitespace-nowrap text-sm text-muted-foreground">{p.city}</td>
                      <td className="px-5 py-4 whitespace-nowrap text-sm text-muted-foreground">{p.serviceType}</td>
                      <td className="px-5 py-4 whitespace-nowrap">
                        <span className="text-sm font-semibold text-foreground">{p.activeContractCount}</span>
                        <span className="text-xs text-muted-foreground"> / {p.contractCount}</span>
                      </td>
                      <td className="px-5 py-4 whitespace-nowrap">{statusBadge(p.status)}</td>
                      <td className="px-5 py-4 whitespace-nowrap text-right">
                        <button onClick={() => router.push(`/suppliers/${p.id}`)}
                          className="text-emerald-600 hover:text-emerald-800 text-sm font-semibold px-3 py-1.5 bg-emerald-50 rounded-lg hover:bg-emerald-100 transition-colors">
                          <Eye className="w-4 h-4 inline mr-1" />
                          Ver
                        </button>
                      </td>
                    </tr>
                  ))
                )}
              </tbody>
            </table>
          </div>
        </CardContent>
      </Card>

      {!loading && providers.length > 0 && (
        <p className="text-xs text-muted-foreground px-1">{providers.length} proveedor{providers.length !== 1 ? 'es' : ''} encontrado{providers.length !== 1 ? 's' : ''}</p>
      )}
    </div>
  );
}
