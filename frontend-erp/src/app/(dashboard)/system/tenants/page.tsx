"use client";

import { useCallback, useEffect, useState } from "react";
import { useRouter } from "next/navigation";
import { useAuth } from "@/context/AuthContext";
import tenantService, { TenantDto } from "@/lib/tenant-service";
import { Globe, Plus, Activity, Server, Search, CheckCircle2, XCircle } from "lucide-react";

export default function TenantsAdminPage() {
  const router = useRouter();
  const { user } = useAuth();
  const [tenants, setTenants] = useState<TenantDto[]>([]);
  const [loading, setLoading] = useState(true);
  const [showModal, setShowModal] = useState(false);
  const [creating, setCreating] = useState(false);
  const [searchTerm, setSearchTerm] = useState("");

  // Pagination State
  const [currentPage, setCurrentPage] = useState(1);
  const [totalPages, setTotalPages] = useState(1);
  const [totalCount, setTotalCount] = useState(0);
  const pageSize = 10;

  // Form state
  const [newSubdomain, setNewSubdomain] = useState("");
  const [error, setError] = useState("");

  useEffect(() => {
    if (user && !(user.role === 'SuperAdmin' && user.tenantSubdomain === 'dev')) {
      router.replace('/dashboard');
    }
  }, [user, router]);

  const loadTenants = useCallback(async () => {
    setLoading(true);
    try {
      const pagedData = await tenantService.getAllTenants(currentPage, pageSize);
      setTenants(pagedData.items);
      setTotalPages(pagedData.totalPages);
      setTotalCount(pagedData.totalCount);
    } catch (err) {
      console.error("Error al cargar tenants:", err);
    } finally {
      setLoading(false);
    }
  }, [currentPage]);

  useEffect(() => {
    const timer = setTimeout(() => {
      loadTenants();
    }, 0);
    return () => clearTimeout(timer);
  }, [loadTenants]);

  const handleCreateTenant = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!newSubdomain.trim()) return;

    setCreating(true);
    setError("");
    try {
      await tenantService.createTenant({ subdomain: newSubdomain.toLowerCase().trim() });
      setShowModal(false);
      setNewSubdomain("");
      await loadTenants();
    } catch (err: unknown) {
      if (err && typeof err === 'object' && 'response' in err) {
        const axiosErr = err as { response?: { data?: { message?: string } } };
        setError(axiosErr.response?.data?.message ?? "Error al crear el tenant");
      } else {
        setError("Error al crear el tenant");
      }
    } finally {
      setCreating(false);
    }
  };

  const handleToggleStatus = async (id: string) => {
    try {
      await tenantService.toggleStatus(id);
      await loadTenants();
    } catch (err) {
      console.error("Error al cambiar estado:", err);
    }
  };

  const filteredTenants = tenants.filter(t =>
    t.subdomain.toLowerCase().includes(searchTerm.toLowerCase())
  );

  return (
    <div className="p-6 bg-slate-50 dark:bg-zinc-950 md:h-screen md:overflow-hidden min-h-screen transition-colors duration-300 flex flex-col">
      <div className="max-w-7xl mx-auto w-full flex-1 flex flex-col min-h-0">
        {/* Header */}
        <div className="flex flex-col md:flex-row md:items-center justify-between gap-4 mb-6 flex-shrink-0">
          <div>
            <h1 className="text-2xl font-bold text-slate-800 dark:text-slate-100 flex items-center gap-2">
              <Globe className="text-emerald-600 dark:text-emerald-500" />
              Gestión de Tenants ERP
            </h1>
            <p className="text-slate-500 dark:text-slate-400 text-sm">Administración central de clientes y bases de datos</p>
          </div>
          <button
            onClick={() => setShowModal(true)}
            className="flex items-center justify-center gap-2 bg-emerald-600 hover:bg-emerald-700 dark:bg-emerald-600 dark:hover:bg-emerald-500 text-white px-4 py-2.5 rounded-lg font-medium transition-colors shadow-sm"
          >
            <Plus size={20} />
            Nuevo Cliente
          </button>
        </div>

        {/* Stats bar */}
        <div className="grid grid-cols-1 sm:grid-cols-3 gap-4 mb-6 flex-shrink-0">
          <div className="bg-white dark:bg-zinc-900 p-4 rounded-xl shadow-sm border border-slate-100 dark:border-zinc-800 flex items-center gap-4">
            <div className="p-3 bg-emerald-50 dark:bg-emerald-900/20 text-emerald-600 dark:text-emerald-400 rounded-lg">
              <Globe size={24} />
            </div>
            <div>
              <p className="text-xs text-slate-500 dark:text-slate-400 font-medium uppercase tracking-wider">Total Clientes</p>
              <p className="text-2xl font-bold text-slate-800 dark:text-slate-100">{totalCount}</p>
            </div>
          </div>
          <div className="bg-white dark:bg-zinc-900 p-4 rounded-xl shadow-sm border border-slate-100 dark:border-zinc-800 flex items-center gap-4">
            <div className="p-3 bg-blue-50 dark:bg-blue-900/20 text-blue-600 dark:text-blue-400 rounded-lg">
              <Activity size={24} />
            </div>
            <div>
              <p className="text-xs text-slate-500 dark:text-slate-400 font-medium uppercase tracking-wider">Actividad 24h</p>
              <p className="text-2xl font-bold text-slate-800 dark:text-slate-100">
                {tenants.reduce((acc, t) => acc + (t.metrics?.activity24h || 0), 0)}
              </p>
            </div>
          </div>
          <div className="bg-white dark:bg-zinc-900 p-4 rounded-xl shadow-sm border border-slate-100 dark:border-zinc-800 flex items-center gap-4">
            <div className="p-3 bg-amber-50 dark:bg-amber-900/20 text-amber-600 dark:text-amber-400 rounded-lg">
              <Server size={24} />
            </div>
            <div>
              <p className="text-xs text-slate-500 dark:text-slate-400 font-medium uppercase tracking-wider">Espacio Total</p>
              <p className="text-2xl font-bold text-slate-800 dark:text-slate-100">
                {tenants.reduce((acc, t) => acc + (t.metrics?.sizeMb || 0), 0).toFixed(2)} MB
              </p>
            </div>
          </div>
        </div>

        {/* List Container */}
        <div className="bg-white dark:bg-zinc-900 rounded-xl shadow-sm border border-slate-200 dark:border-zinc-800 overflow-hidden flex-1 flex flex-col min-h-0 mb-6">
          <div className="p-4 border-b border-slate-100 dark:border-zinc-800 bg-slate-50/50 dark:bg-zinc-900/50 flex-shrink-0">
            <div className="relative max-w-md">
              <Search className="absolute left-3 top-1/2 -translate-y-1/2 text-slate-400" size={18} />
              <input
                type="text"
                placeholder="Buscar por subdominio..."
                className="w-full pl-10 pr-4 py-2 rounded-lg border border-slate-200 dark:border-zinc-700 bg-white dark:bg-zinc-800 text-slate-800 dark:text-slate-200 focus:outline-none focus:ring-2 focus:ring-emerald-500/20 focus:border-emerald-500 transition-all text-sm"
                value={searchTerm}
                onChange={(e) => setSearchTerm(e.target.value)}
              />
            </div>
          </div>

          {/* PC VIEW (TABLE) */}
          <div className="hidden md:block flex-1 overflow-y-auto min-h-0 relative scrollbar-thin scrollbar-thumb-slate-200 dark:scrollbar-thumb-zinc-800">
            <table className="w-full text-left border-separate border-spacing-0">
              <thead className="sticky top-0 z-20">
                <tr className="bg-slate-100 dark:bg-zinc-800 text-slate-600 dark:text-slate-300 text-xs font-bold uppercase tracking-wider">
                  <th className="px-6 py-4 border-b border-slate-200 dark:border-zinc-700">Subdominio</th>
                  <th className="px-6 py-4 border-b border-slate-200 dark:border-zinc-700">Salud / Ping</th>
                  <th className="px-6 py-4 border-b border-slate-200 dark:border-zinc-700">Base de Datos</th>
                  <th className="px-6 py-4 border-b border-slate-200 dark:border-zinc-700 text-center">Actividad 24h</th>
                  <th className="px-6 py-4 border-b border-slate-200 dark:border-zinc-700 text-center">Tablas / Filas</th>
                  <th className="px-6 py-4 border-b border-slate-200 dark:border-zinc-700 text-right">Tamaño</th>
                  <th className="px-6 py-4 border-b border-slate-200 dark:border-zinc-700 text-right">Acciones</th>
                </tr>
              </thead>
              <tbody className="divide-y divide-slate-100 dark:divide-zinc-800">
                {loading ? (
                  <tr>
                    <td colSpan={7} className="px-6 py-12 text-center text-slate-400 dark:text-slate-500">Cargando tenants...</td>
                  </tr>
                ) : filteredTenants.length === 0 ? (
                  <tr>
                    <td colSpan={7} className="px-6 py-12 text-center text-slate-400 dark:text-slate-500">No se encontraron clientes</td>
                  </tr>
                ) : (
                  filteredTenants.map((t) => (
                    <tr key={t.id} className="hover:bg-slate-50/50 dark:hover:bg-zinc-800/50 transition-colors">
                      <td className="px-6 py-4 font-medium text-slate-800 dark:text-slate-200">
                        <div className="flex flex-col">
                           <div className="flex items-center gap-2">
                             <span className="p-1.5 bg-slate-100 dark:bg-zinc-800 text-slate-600 dark:text-slate-400 rounded">.</span>
                             {t.subdomain}
                           </div>
                           <span className="text-[10px] text-slate-400 mt-1">Creado: {new Date(t.createdAt).toLocaleDateString()}</span>
                        </div>
                      </td>
                      <td className="px-6 py-4">
                        <div className="flex flex-col gap-1.5">
                          <span className={`inline-flex items-center gap-1.5 px-2 py-0.5 rounded-full text-[10px] font-bold uppercase ${
                            t.isActive ? "bg-emerald-50 dark:bg-emerald-900/20 text-emerald-700 dark:text-emerald-400" : "bg-red-50 dark:bg-red-900/20 text-red-700 dark:text-red-400"
                          }`}>
                            {t.isActive ? <CheckCircle2 size={12} /> : <XCircle size={12} />}
                            {t.isActive ? "Activo" : "Suspendido"}
                          </span>
                          {t.metrics ? (
                            <div className="flex items-center gap-1 text-[10px] font-mono text-slate-400">
                              <Activity size={10} className={t.metrics.latencyMs < 100 ? "text-emerald-500" : "text-amber-500"} />
                              {t.metrics.latencyMs}ms
                            </div>
                          ) : (
                            <span className="text-[10px] text-red-500 font-bold uppercase italic">Sin conexión</span>
                          )}
                        </div>
                      </td>
                      <td className="px-6 py-4">
                        <div className="flex flex-col">
                          <span className="text-xs font-mono text-slate-600 dark:text-slate-300">
                            {t.metrics?.databaseName || "N/A"}
                          </span>
                          <span className="text-[10px] text-slate-400 truncate max-w-[150px]" title={t.connectionString}>
                            {t.connectionString.split(';')[0]}...
                          </span>
                        </div>
                      </td>
                      <td className="px-6 py-4 text-center">
                        <div className="inline-flex flex-col items-center px-3 py-1 bg-blue-50 dark:bg-blue-900/20 rounded-lg">
                          <span className="text-sm font-black text-blue-700 dark:text-blue-400">{t.metrics?.activity24h || 0}</span>
                          <span className="text-[9px] text-blue-600/70 dark:text-blue-400/70 uppercase font-bold">Eventos</span>
                        </div>
                      </td>
                      <td className="px-6 py-4 text-center">
                        <div className="flex flex-col items-center">
                          <span className="text-sm font-bold text-slate-700 dark:text-slate-200">{t.metrics?.tableCount || 0}</span>
                          <span className="text-[10px] text-slate-400 uppercase tracking-tighter">{t.metrics?.rowCount.toLocaleString() || 0} filas</span>
                        </div>
                      </td>
                      <td className="px-6 py-4 text-right">
                        <span className="px-2 py-1 bg-slate-100 dark:bg-zinc-800 rounded text-xs font-bold text-slate-600 dark:text-slate-300">
                          {t.metrics?.sizeMb?.toFixed(2) || "0.00"} MB
                        </span>
                      </td>
                      <td className="px-6 py-4 text-right">
                        <button
                          onClick={() => handleToggleStatus(t.id)}
                          className={`text-sm font-medium px-3 py-1.5 rounded-md transition-colors ${
                            t.isActive ? "text-red-600 dark:text-red-400 hover:bg-red-50 dark:hover:bg-red-900/20" : "text-emerald-600 dark:text-emerald-400 hover:bg-emerald-50 dark:hover:bg-emerald-900/20"
                          }`}
                        >
                          {t.isActive ? "Suspender" : "Habilitar"}
                        </button>
                      </td>
                    </tr>
                  ))
                )}
              </tbody>
            </table>
          </div>

          {/* MOBILE VIEW (CARDS) */}
          <div className="md:hidden flex-1 overflow-y-auto p-4 space-y-4 bg-slate-50/50 dark:bg-zinc-900/50">
            {loading ? (
               <div className="text-center py-8 text-slate-400 uppercase font-bold text-xs">Cargando clientes...</div>
            ) : filteredTenants.length === 0 ? (
               <div className="text-center py-8 text-slate-400 uppercase font-bold text-xs">No se encontraron clientes</div>
            ) : (
              filteredTenants.map((t) => (
                <div key={t.id} className="bg-white dark:bg-zinc-900 border border-slate-200 dark:border-zinc-800 rounded-xl p-4 shadow-sm space-y-4">
                  <div className="flex justify-between items-start">
                    <div className="flex flex-col">
                      <span className="text-lg font-bold text-slate-800 dark:text-slate-100 flex items-center gap-2">
                        <Globe size={18} className="text-emerald-500" />
                        {t.subdomain}
                      </span>
                      <span className="text-[10px] text-slate-400">ID: {t.id.substring(0, 8)}...</span>
                    </div>
                    <span className={`inline-flex items-center gap-1.5 px-2.5 py-1 rounded-full text-[10px] font-bold uppercase ${
                      t.isActive ? "bg-emerald-50 dark:bg-emerald-900/20 text-emerald-700 dark:text-emerald-400" : "bg-red-50 dark:bg-red-900/20 text-red-700 dark:text-red-400"
                    }`}>
                      {t.isActive ? "Activo" : "Suspendido"}
                    </span>
                  </div>

                  <div className="grid grid-cols-2 gap-4 py-3 border-y border-slate-100 dark:border-zinc-800">
                    <div>
                      <p className="text-[9px] text-slate-400 uppercase font-black mb-1">Salud / Latencia</p>
                      {t.metrics ? (
                        <div className="flex items-center gap-1 text-xs font-bold text-slate-600 dark:text-slate-300">
                          <Activity size={12} className={t.metrics.latencyMs < 100 ? "text-emerald-500" : "text-amber-500"} />
                          {t.metrics.latencyMs}ms
                        </div>
                      ) : <span className="text-[10px] text-red-500 font-bold uppercase">Sin conexión</span>}
                    </div>
                    <div>
                      <p className="text-[9px] text-slate-400 uppercase font-black mb-1">Actividad 24h</p>
                      <div className="flex items-center gap-1 text-xs font-bold text-blue-600 dark:text-blue-400">
                        <Activity size={12} />
                        {t.metrics?.activity24h || 0} eventos
                      </div>
                    </div>
                    <div>
                      <p className="text-[9px] text-slate-400 uppercase font-black mb-1">Tamaño DB</p>
                      <div className="text-xs font-bold text-slate-600 dark:text-slate-300">
                        {t.metrics?.sizeMb?.toFixed(2) || "0.00"} MB
                      </div>
                    </div>
                    <div>
                      <p className="text-[9px] text-slate-400 uppercase font-black mb-1">Total Datos</p>
                      <div className="text-xs font-bold text-slate-600 dark:text-slate-300">
                        {t.metrics?.rowCount.toLocaleString() || 0} filas
                      </div>
                    </div>
                  </div>

                  <div className="flex justify-between items-center pt-2">
                    <span className="text-[10px] text-slate-400">Creado: {new Date(t.createdAt).toLocaleDateString()}</span>
                    <button
                      onClick={() => handleToggleStatus(t.id)}
                      className={`text-xs font-bold px-4 py-2 rounded-lg transition-colors ${
                        t.isActive ? "bg-red-50 text-red-600 dark:bg-red-900/20 dark:text-red-400" : "bg-emerald-50 text-emerald-600 dark:bg-emerald-900/20 dark:text-emerald-400"
                      }`}
                    >
                      {t.isActive ? "Suspender Acceso" : "Activar Acceso"}
                    </button>
                  </div>
                </div>
              ))
            )}
          </div>

          {/* Pagination Controls */}
          <div className="px-6 py-4 border-t border-slate-100 dark:border-zinc-800 bg-slate-50/30 dark:bg-zinc-900/30 flex flex-col sm:flex-row items-center justify-between gap-4 flex-shrink-0">
            <div className="text-sm text-slate-500 dark:text-slate-400">
              Mostrando <span className="font-semibold text-slate-700 dark:text-slate-200">{filteredTenants.length}</span> de <span className="font-semibold text-slate-700 dark:text-slate-200">{totalCount}</span> clientes
            </div>
            <div className="flex items-center gap-2">
              <button
                onClick={() => setCurrentPage(prev => Math.max(prev - 1, 1))}
                disabled={currentPage === 1 || loading}
                className="px-4 py-2 text-sm font-medium rounded-lg border border-slate-200 dark:border-zinc-700 bg-white dark:bg-zinc-800 text-slate-600 dark:text-slate-300 hover:bg-slate-50 dark:hover:bg-zinc-700 disabled:opacity-50 disabled:cursor-not-allowed transition-all shadow-sm"
              >
                Anterior
              </button>
              <div className="flex items-center gap-1 px-2">
                <span className="text-sm font-bold text-emerald-600 dark:text-emerald-500">{currentPage}</span>
                <span className="text-sm text-slate-400">/</span>
                <span className="text-sm text-slate-500 dark:text-slate-400">{totalPages}</span>
              </div>
              <button
                onClick={() => setCurrentPage(prev => Math.min(prev + 1, totalPages))}
                disabled={currentPage === totalPages || loading}
                className="px-4 py-2 text-sm font-medium rounded-lg border border-slate-200 dark:border-zinc-700 bg-white dark:bg-zinc-800 text-slate-600 dark:text-slate-300 hover:bg-slate-50 dark:hover:bg-zinc-700 disabled:opacity-50 disabled:cursor-not-allowed transition-all shadow-sm"
              >
                Siguiente
              </button>
            </div>
          </div>
        </div>

        {/* Create Modal */}
        {showModal && (
          <div className="fixed inset-0 z-50 flex items-center justify-center p-4 bg-slate-900/40 dark:bg-black/60 backdrop-blur-sm">
            <div className="bg-white dark:bg-zinc-900 rounded-2xl shadow-xl w-full max-w-md overflow-hidden animate-in fade-in zoom-in duration-200 border border-transparent dark:border-zinc-800">
              <div className="p-6 border-b border-slate-100 dark:border-zinc-800 flex items-center justify-between bg-emerald-600 text-white">
                <h3 className="text-lg font-bold flex items-center gap-2">
                  <Globe size={20} />
                  Registrar Nuevo Cliente
                </h3>
                <button onClick={() => setShowModal(false)} className="text-white/80 hover:text-white">
                  <XCircle size={24} />
                </button>
              </div>
              <form onSubmit={handleCreateTenant} className="p-6">
                <div className="mb-6">
                  <label className="block text-sm font-semibold text-slate-700 dark:text-slate-300 mb-2">Subdominio del Cliente</label>
                  <div className="flex items-center">
                    <input
                      type="text"
                      placeholder="ej: condominio_brisas"
                      className="flex-1 px-4 py-2.5 rounded-l-lg border border-slate-200 dark:border-zinc-700 bg-white dark:bg-zinc-800 text-slate-800 dark:text-slate-200 focus:outline-none focus:ring-2 focus:ring-emerald-500/20 focus:border-emerald-500 transition-all"
                      value={newSubdomain}
                      onChange={(e) => setNewSubdomain(e.target.value.toLowerCase().replace(/\s+/g, '_'))}
                      disabled={creating}
                      required
                    />
                    <span className="px-4 py-2.5 bg-slate-50 dark:bg-zinc-800 border border-l-0 border-slate-200 dark:border-zinc-700 rounded-r-lg text-slate-400 dark:text-slate-500 text-sm font-medium">
                      .softcoinp.com
                    </span>
                  </div>
                  <p className="mt-2 text-xs text-slate-400 dark:text-slate-500">
                    Se creará una base de datos <strong className="text-slate-600 dark:text-slate-300">erp_{newSubdomain || '...'}</strong> automáticamente.
                  </p>
                </div>

                {error && (
                  <div className="mb-6 p-3 bg-red-50 dark:bg-red-900/20 text-red-600 dark:text-red-400 rounded-lg text-sm flex items-center gap-2">
                    <XCircle size={16} />
                    {error}
                  </div>
                )}

                <div className="flex flex-col gap-3">
                  <button
                    type="submit"
                    disabled={creating || !newSubdomain}
                    className="w-full bg-emerald-600 hover:bg-emerald-700 dark:bg-emerald-600 dark:hover:bg-emerald-500 text-white py-3 rounded-xl font-bold transition-all shadow-md shadow-emerald-600/20 disabled:opacity-50 flex items-center justify-center gap-2"
                  >
                    {creating ? (
                      <>
                        <div className="w-5 h-5 border-2 border-white/30 border-t-white rounded-full animate-spin"></div>
                        Inicializando Sistema...
                      </>
                    ) : "Crear e Inicializar"}
                  </button>
                  <button
                    type="button"
                    onClick={() => setShowModal(false)}
                    className="w-full text-slate-500 dark:text-slate-400 font-medium py-2 hover:text-slate-700 dark:hover:text-slate-200 transition-colors"
                  >
                    Cancelar
                  </button>
                </div>
              </form>
            </div>
          </div>
        )}
      </div>
    </div>
  );
}
