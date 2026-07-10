'use client';

import React, { useState, useEffect } from 'react';
import { useRouter } from 'next/navigation';
import assemblyService, { AssemblyListItem } from '@/lib/assembly-service';


const statusLabels: Record<string, string> = {
  Draft: 'Borrador',
  Convoked: 'Convocada',
  InSession: 'En Sesión',
  Closed: 'Cerrada',
  MinutesApproved: 'Acta Aprobada',
  Published: 'Publicada',
};

const typeLabels: Record<string, string> = {
  Ordinary: 'Ordinaria',
  Extraordinary: 'Extraordinaria',
};

const statusBadgeClass = (status: string): string => {
  if (status === 'Draft') return 'bg-muted text-muted-foreground';
  if (status === 'Convoked') return 'bg-blue-100 dark:bg-blue-950/30 text-blue-700 dark:text-blue-400';
  if (status === 'InSession') return 'bg-yellow-100 text-yellow-700';
  if (status === 'Closed') return 'bg-orange-100 text-orange-700';
  if (status === 'MinutesApproved') return 'bg-purple-100 text-purple-700';
  if (status === 'Published') return 'bg-emerald-100 dark:bg-emerald-950/30 text-emerald-700 dark:text-emerald-400';
  return 'bg-muted text-muted-foreground';
};

const typeBadgeClass = (type: string): string => {
  if (type === 'Ordinary') return 'bg-blue-100 dark:bg-blue-950/30 text-blue-700 dark:text-blue-400';
  if (type === 'Extraordinary') return 'bg-purple-100 text-purple-700';
  return 'bg-muted text-muted-foreground';
};

export default function AssemblyListPage() {
  const router = useRouter();
  const [assemblies, setAssemblies] = useState<AssemblyListItem[]>([]);
  const [loading, setLoading] = useState(true);
  const [statusFilter, setStatusFilter] = useState('');
  const [typeFilter, setTypeFilter] = useState('');
  const [searchTerm, setSearchTerm] = useState('');

  const fetchAssemblies = async () => {
    setLoading(true);
    try {
      const data = await assemblyService.getAssemblies(
        statusFilter || undefined,
        typeFilter || undefined,
        undefined,
        undefined,
        searchTerm || undefined
      );
      setAssemblies(data);
    } catch {
      // eslint-disable-next-line no-console
      console.error('Error al cargar asambleas');
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    fetchAssemblies();
  }, [statusFilter, typeFilter]);

  useEffect(() => {
    const timer = setTimeout(() => {
      fetchAssemblies();
    }, 400);
    return () => clearTimeout(timer);
  }, [searchTerm]);

  const formatDate = (dateStr: string): string => {
    try {
      return new Date(dateStr).toLocaleDateString('es-CO', { day: '2-digit', month: 'short', year: 'numeric' });
    } catch {
      return dateStr;
    }
  };

  const totalOrdinary = assemblies.filter((a) => a.type === 'Ordinary').length;
  const totalExtraordinary = assemblies.filter((a) => a.type === 'Extraordinary').length;
  const totalPublished = assemblies.filter((a) => a.status === 'Published').length;

  return (
    <div className="p-6 space-y-6">
        <div className="flex flex-col sm:flex-row justify-between items-start sm:items-center gap-4">
          <div>
            <h1 className="text-2xl font-bold text-foreground tracking-tight">
              Asambleas de Copropietarios
            </h1>
            <p className="text-sm text-muted-foreground mt-1">
              Gestiona las asambleas del conjunto residencial.
            </p>
          </div>
          <button
            onClick={() => router.push('/assembly/new')}
            className="bg-emerald-600 hover:bg-emerald-700 text-white font-semibold px-4 py-2 rounded-lg transition-colors"
          >
            + Nueva Asamblea
          </button>
        </div>

        <div className="grid grid-cols-1 sm:grid-cols-2 md:grid-cols-4 gap-4">
          <div className="bg-card border border-border rounded-lg p-4 text-center">
            <p className="text-2xl font-bold text-foreground">{assemblies.length}</p>
            <p className="text-xs text-muted-foreground">Total Asambleas</p>
          </div>
          <div className="bg-card border border-border rounded-lg p-4 text-center">
            <p className="text-2xl font-bold text-blue-600 dark:text-blue-400">{totalOrdinary}</p>
            <p className="text-xs text-muted-foreground">Ordinarias</p>
          </div>
          <div className="bg-card border border-border rounded-lg p-4 text-center">
            <p className="text-2xl font-bold text-purple-600">{totalExtraordinary}</p>
            <p className="text-xs text-muted-foreground">Extraordinarias</p>
          </div>
          <div className="bg-card border border-border rounded-lg p-4 text-center">
            <p className="text-2xl font-bold text-green-600">{totalPublished}</p>
            <p className="text-xs text-muted-foreground">Publicadas</p>
          </div>
        </div>

        <div className="bg-card border border-border rounded-lg p-4">
          <div className="flex flex-col md:flex-row gap-3">
            <select
              value={statusFilter}
              onChange={(e) => setStatusFilter(e.target.value)}
              className="bg-transparent border-b border-emerald-600/30 focus:border-emerald-600 text-sm font-medium py-2 outline-none"
            >
              <option value="">Todos los estados</option>
              <option value="Draft">Borrador</option>
              <option value="Convoked">Convocada</option>
              <option value="InSession">En Sesión</option>
              <option value="Closed">Cerrada</option>
              <option value="MinutesApproved">Acta Aprobada</option>
              <option value="Published">Publicada</option>
            </select>
            <select
              value={typeFilter}
              onChange={(e) => setTypeFilter(e.target.value)}
              className="bg-transparent border-b border-emerald-600/30 focus:border-emerald-600 text-sm font-medium py-2 outline-none"
            >
              <option value="">Todos los tipos</option>
              <option value="Ordinary">Ordinaria</option>
              <option value="Extraordinary">Extraordinaria</option>
            </select>
            <div className="flex-1 relative">
              <input
                type="text"
                placeholder="Buscar por título o lugar..."
                value={searchTerm}
                onChange={(e) => setSearchTerm(e.target.value)}
                className="w-full bg-transparent border-b border-emerald-600/30 focus:border-emerald-600 text-sm font-medium py-2 outline-none"
              />
            </div>
          </div>
        </div>

        {loading ? (
          <div className="flex justify-center py-20">
            <p className="text-muted-foreground text-sm">Cargando asambleas...</p>
          </div>
        ) : assemblies.length === 0 ? (
          <div className="flex flex-col items-center gap-3 text-muted-foreground py-12">
            <p className="font-semibold">No hay asambleas registradas</p>
            <p className="text-sm">Crea la primera asamblea del conjunto.</p>
          </div>
        ) : (
          <div className="bg-card border border-border rounded-lg overflow-hidden">
            <div className="overflow-x-auto">
              <table className="min-w-full divide-y divide-border">
                <thead className="bg-muted/50">
                  <tr>
                    <th className="px-5 py-3 text-left text-xs font-bold text-muted-foreground uppercase">
                      Título
                    </th>
                    <th className="px-5 py-3 text-left text-xs font-bold text-muted-foreground uppercase">
                      Tipo
                    </th>
                    <th className="px-5 py-3 text-left text-xs font-bold text-muted-foreground uppercase">
                      Estado
                    </th>
                    <th className="px-5 py-3 text-left text-xs font-bold text-muted-foreground uppercase">
                      Fecha
                    </th>
                    <th className="px-5 py-3 text-left text-xs font-bold text-muted-foreground uppercase">
                      Lugar
                    </th>
                    <th className="px-5 py-3 text-center text-xs font-bold text-muted-foreground uppercase">
                      Asistentes
                    </th>
                    <th className="px-5 py-3 text-center text-xs font-bold text-muted-foreground uppercase">
                      Quórum
                    </th>
                    <th className="px-5 py-3 text-right text-xs font-bold text-muted-foreground uppercase">
                      Acciones
                    </th>
                  </tr>
                </thead>
                <tbody className="divide-y divide-border">
                  {assemblies.map((a) => (
                    <tr key={a.id} className="hover:bg-muted/30 transition-colors">
                      <td className="px-5 py-3">
                        <p className="font-medium text-sm">{a.title}</p>
                      </td>
                      <td className="px-5 py-3">
                        <span className={`inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-medium ${typeBadgeClass(a.type)}`}>
                          {typeLabels[a.type] || a.type}
                        </span>
                      </td>
                      <td className="px-5 py-3">
                        <span className={`inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-medium ${statusBadgeClass(a.status)}`}>
                          {statusLabels[a.status] || a.status}
                        </span>
                      </td>
                      <td className="px-5 py-3 text-sm text-muted-foreground">
                        {formatDate(a.scheduledDate)}
                      </td>
                      <td className="px-5 py-3 text-sm text-muted-foreground">
                        {a.location}
                      </td>
                      <td className="px-5 py-3 text-center text-sm text-muted-foreground">
                        {a.attendanceCount}
                      </td>
                      <td className="px-5 py-3 text-center text-sm text-muted-foreground">
                        {a.quorumAchievedFirstCall ? (
                          <span className="text-green-600 font-medium">Sí</span>
                        ) : (
                          <span className="text-red-500 font-medium">No</span>
                        )}
                      </td>
                      <td className="px-5 py-3 text-right">
                        <button
                          onClick={() => router.push(`/assembly/${a.id}`)}
                          className="text-emerald-600 hover:text-emerald-800 text-sm font-semibold"
                        >
                          Ver
                        </button>
                      </td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>
            <div className="px-5 py-3 border-t border-border text-xs text-muted-foreground">
              {assemblies.length} asamblea(s) encontrada(s)
            </div>
          </div>
        )}
    </div>
  );
}
