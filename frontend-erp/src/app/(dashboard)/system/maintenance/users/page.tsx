'use client';

import React, { useState, useEffect, useCallback } from 'react';
import { useRouter } from 'next/navigation';
import { Plus, Search, Edit, Key, ShieldOff, ShieldCheck, Trash2, History, Loader2, AlertTriangle, Eye, EyeOff } from 'lucide-react';
import systemMaintenanceService, { UserListItem } from '@/lib/system-maintenance-service';

type SortField = 'fullName' | 'email' | 'status' | 'createdAt' | 'lastLogin';
type SortOrder = 'asc' | 'desc';

const statusConfig: Record<string, { label: string; class: string }> = {
  Active: { label: 'Activo', class: 'badge-success' },
  Suspended: { label: 'Suspendido', class: 'badge-danger' },
  Deleted: { label: 'Eliminado', class: 'badge-neutral' },
};

function formatDate(dateStr?: string): string {
  if (!dateStr) return '-';
  const d = new Date(dateStr);
  return d.toLocaleDateString('es-CO', { year: 'numeric', month: 'short', day: 'numeric', hour: '2-digit', minute: '2-digit' });
}

export default function UsersListPage() {
  const router = useRouter();
  const [users, setUsers] = useState<UserListItem[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');
  const [search, setSearch] = useState('');
  const [sortField, setSortField] = useState<SortField>('createdAt');
  const [sortOrder, setSortOrder] = useState<SortOrder>('desc');

  const [confirmAction, setConfirmAction] = useState<{
    type: 'delete' | 'suspend' | 'reactivate' | 'resetPassword';
    user: UserListItem;
  } | null>(null);
  const [actionLoading, setActionLoading] = useState(false);
  const [newPassword, setNewPassword] = useState('');
  const [showPassword, setShowPassword] = useState(false);
  const [passwordError, setPasswordError] = useState('');

  const fetchUsers = useCallback(async () => {
    setLoading(true);
    setError('');
    try {
      const data = await systemMaintenanceService.getUsers(
        search || undefined,
        sortField,
        sortOrder,
      );
      setUsers(data);
    } catch {
      setError('Error al cargar los usuarios.');
    } finally {
      setLoading(false);
    }
  }, [search, sortField, sortOrder]);

  useEffect(() => {
    const timer = setTimeout(() => {
      fetchUsers();
    }, 300);
    return () => clearTimeout(timer);
  }, [fetchUsers]);

  const handleSort = (field: SortField) => {
    if (sortField === field) {
      setSortOrder(sortOrder === 'asc' ? 'desc' : 'asc');
    } else {
      setSortField(field);
      setSortOrder('asc');
    }
  };

  const sortIndicator = (field: SortField) => {
    if (sortField !== field) return null;
    return sortOrder === 'asc' ? ' ▲' : ' ▼';
  };

  const getErrorMessage = (err: unknown): string => {
    if (err && typeof err === 'object' && 'response' in err) {
      const axiosErr = err as { response?: { data?: { message?: string } } };
      if (axiosErr.response?.data?.message) {
        return axiosErr.response.data.message;
      }
    }
    if (err instanceof Error) {
      return err.message;
    }
    return 'Error inesperado.';
  };

  const handleDelete = async () => {
    if (!confirmAction || confirmAction.type !== 'delete') return;
    setActionLoading(true);
    try {
      await systemMaintenanceService.deleteUser(confirmAction.user.id);
      setConfirmAction(null);
      fetchUsers();
    } catch (err) {
      setConfirmAction(null);
      setError(getErrorMessage(err));
    } finally {
      setActionLoading(false);
    }
  };

  const handleSuspend = async () => {
    if (!confirmAction || confirmAction.type !== 'suspend') return;
    setActionLoading(true);
    try {
      await systemMaintenanceService.suspendUser(confirmAction.user.id, {});
      setConfirmAction(null);
      fetchUsers();
    } catch (err) {
      setConfirmAction(null);
      setError(getErrorMessage(err));
    } finally {
      setActionLoading(false);
    }
  };

  const handleReactivate = async () => {
    if (!confirmAction || confirmAction.type !== 'reactivate') return;
    setActionLoading(true);
    try {
      await systemMaintenanceService.reactivateUser(confirmAction.user.id);
      setConfirmAction(null);
      fetchUsers();
    } catch (err) {
      setConfirmAction(null);
      setError(getErrorMessage(err));
    } finally {
      setActionLoading(false);
    }
  };

  const handleResetPassword = async () => {
    if (!confirmAction || confirmAction.type !== 'resetPassword') return;

    if (!newPassword || newPassword.length < 8) {
      setPasswordError('La contraseña debe tener al menos 8 caracteres.');
      return;
    }

    setActionLoading(true);
    setPasswordError('');
    try {
      await systemMaintenanceService.resetPassword(confirmAction.user.id, { newPassword });
      setConfirmAction(null);
      setNewPassword('');
    } catch (err) {
      setConfirmAction(null);
      setNewPassword('');
      setError(getErrorMessage(err));
    } finally {
      setActionLoading(false);
    }
  };

  const renderConfirmDialog = () => {
    if (!confirmAction) return null;

    const configs: Record<string, { title: string; description: string; buttonClass: string; buttonText: string; handler: () => void }> = {
      delete: {
        title: 'Eliminar usuario',
        description: `Estás a punto de eliminar permanentemente a "${confirmAction.user.fullName}" (${confirmAction.user.email}). Esta acción es irreversible y el usuario perderá acceso inmediatamente.`,
        buttonClass: 'btn-danger',
        buttonText: 'Eliminar permanentemente',
        handler: handleDelete,
      },
      suspend: {
        title: 'Suspender usuario',
        description: `Estás a punto de suspender a "${confirmAction.user.fullName}" (${confirmAction.user.email}). El usuario no podrá iniciar sesión hasta que sea reactivado. Si tiene sesión activa, será cerrada inmediatamente.`,
        buttonClass: 'btn-danger',
        buttonText: 'Suspender usuario',
        handler: handleSuspend,
      },
      reactivate: {
        title: 'Reactivar usuario',
        description: `Vas a reactivar a "${confirmAction.user.fullName}" (${confirmAction.user.email}). El usuario podrá iniciar sesión nuevamente.`,
        buttonClass: 'btn-success',
        buttonText: 'Reactivar usuario',
        handler: handleReactivate,
      },
      resetPassword: {
        title: 'Resetear contraseña',
        description: `Ingresa la nueva contraseña para "${confirmAction.user.fullName}" (${confirmAction.user.email}). Si tiene sesión activa, será cerrada.`,
        buttonClass: 'btn-primary',
        buttonText: 'Guardar contraseña',
        handler: handleResetPassword,
      },
    };

    const config = configs[confirmAction.type];

    return (
      <div className="fixed inset-0 bg-black/60 backdrop-blur-sm z-[150] flex items-center justify-center animate-in fade-in duration-200">
        <div className="bg-card border border-border rounded-2xl shadow-2xl w-full max-w-md mx-4 animate-in zoom-in-95 duration-200">
          <div className="p-6">
            <div className="flex items-center gap-3 mb-4">
              <div className="p-2.5 rounded-xl bg-rose-50 dark:bg-rose-950/20 border border-rose-100 dark:border-rose-900/50">
                <AlertTriangle className="w-5 h-5 text-rose-600 dark:text-rose-400" />
              </div>
              <h3 className="text-lg font-bold text-foreground">{config.title}</h3>
            </div>
            <p className="text-sm text-muted-foreground leading-relaxed">{config.description}</p>
            {confirmAction.type === 'resetPassword' && (
              <div className="mt-4">
                <label className="block text-sm font-semibold text-foreground mb-2">Nueva contraseña</label>
                <div className="relative">
                  <input
                    type={showPassword ? 'text' : 'password'}
                    value={newPassword}
                    onChange={(e) => {
                      setNewPassword(e.target.value);
                      if (passwordError) setPasswordError('');
                    }}
                    placeholder="Mínimo 8 caracteres"
                    className={`w-full px-4 py-2.5 bg-muted/50 border rounded-xl text-sm focus:outline-none focus:ring-2 focus:ring-emerald-500/20 focus:border-emerald-500 pr-10 ${
                      passwordError ? 'border-rose-300' : 'border-border'
                    }`}
                  />
                  <button
                    type="button"
                    onClick={() => setShowPassword(!showPassword)}
                    className="absolute right-3 top-1/2 -translate-y-1/2 text-muted-foreground hover:text-foreground"
                  >
                    {showPassword ? <EyeOff className="w-4 h-4" /> : <Eye className="w-4 h-4" />}
                  </button>
                </div>
                {passwordError && (
                  <p className="mt-1 text-xs text-rose-500">{passwordError}</p>
                )}
              </div>
            )}
          </div>
          <div className="px-6 pb-6 flex justify-end gap-3">
            <button
              onClick={() => setConfirmAction(null)}
              className="btn-secondary px-4 py-2 rounded-xl text-sm font-semibold"
              disabled={actionLoading}
            >
              Cancelar
            </button>
            <button
              onClick={config.handler}
              className={`${config.buttonClass} px-4 py-2 rounded-xl text-sm font-semibold flex items-center gap-2`}
              disabled={actionLoading}
            >
              {actionLoading && <Loader2 className="w-4 h-4 animate-spin" />}
              {config.buttonText}
            </button>
          </div>
        </div>
      </div>
    );
  };

  return (
    <div>
      <div className="flex items-center justify-between mb-6">
        <div>
          <h2 className="text-lg font-bold text-foreground">Usuarios Administradores</h2>
          <p className="text-sm text-muted-foreground">Gestiona los usuarios del conjunto actual</p>
        </div>
        <button
          onClick={() => router.push('/system/maintenance/users/new')}
          className="btn-primary px-4 py-2.5 rounded-xl text-sm font-semibold flex items-center gap-2"
        >
          <Plus className="w-4 h-4" />
          Nuevo usuario
        </button>
      </div>

      <div className="bg-card border border-border rounded-xl shadow-sm overflow-hidden">
        <div className="p-4 border-b border-border bg-slate-50/50 dark:bg-slate-900/50">
          <div className="relative">
            <Search className="absolute left-3 top-1/2 -translate-y-1/2 w-4 h-4 text-slate-400" />
            <input
              type="text"
              placeholder="Buscar por nombre o correo..."
              value={search}
              onChange={(e) => setSearch(e.target.value)}
              className="w-full pl-10 pr-4 py-2.5 bg-card border border-border rounded-xl text-sm focus:outline-none focus:ring-2 focus:ring-emerald-500/20 focus:border-emerald-500"
            />
          </div>
        </div>

        {error && (
          <div className="mx-4 mt-4 p-3 bg-rose-50 dark:bg-rose-950/20 border border-rose-200 dark:border-rose-900/50 rounded-xl text-sm text-rose-700 dark:text-rose-300 flex items-start gap-2">
            <AlertTriangle className="w-4 h-4 flex-shrink-0 mt-0.5" />
            <span className="flex-1">{error}</span>
            <button
              onClick={() => setError('')}
              className="text-rose-500 hover:text-rose-700 flex-shrink-0 ml-2"
            >
              x
            </button>
          </div>
        )}

        <div className="overflow-x-auto">
          <table className="w-full text-sm">
            <thead>
              <tr className="border-b border-border bg-slate-50/50 dark:bg-slate-900/50">
                <th
                  className="text-left px-4 py-3 font-semibold text-muted-foreground cursor-pointer hover:text-foreground select-none"
                  onClick={() => handleSort('fullName')}
                >
                  Nombre completo{sortIndicator('fullName')}
                </th>
                <th
                  className="text-left px-4 py-3 font-semibold text-muted-foreground cursor-pointer hover:text-foreground select-none"
                  onClick={() => handleSort('email')}
                >
                  Correo{sortIndicator('email')}
                </th>
                <th
                  className="text-left px-4 py-3 font-semibold text-muted-foreground cursor-pointer hover:text-foreground select-none"
                  onClick={() => handleSort('status')}
                >
                  Estado{sortIndicator('status')}
                </th>
                <th
                  className="text-left px-4 py-3 font-semibold text-muted-foreground cursor-pointer hover:text-foreground select-none"
                  onClick={() => handleSort('createdAt')}
                >
                  Creado{sortIndicator('createdAt')}
                </th>
                <th
                  className="text-left px-4 py-3 font-semibold text-muted-foreground cursor-pointer hover:text-foreground select-none"
                  onClick={() => handleSort('lastLogin')}
                >
                  Último acceso{sortIndicator('lastLogin')}
                </th>
                <th className="text-right px-4 py-3 font-semibold text-muted-foreground">Acciones</th>
              </tr>
            </thead>
            <tbody>
              {loading && (
                <tr>
                  <td colSpan={6} className="px-4 py-12 text-center">
                    <div className="flex items-center justify-center gap-2 text-muted-foreground">
                      <Loader2 className="w-5 h-5 animate-spin" />
                      <span>Cargando usuarios...</span>
                    </div>
                  </td>
                </tr>
              )}
              {!loading && !error && users.length === 0 && (
                <tr>
                  <td colSpan={6} className="px-4 py-12 text-center">
                    <div className="text-muted-foreground text-sm">
                      {search ? 'No se encontraron usuarios con ese criterio de búsqueda.' : 'No hay usuarios registrados en este conjunto.'}
                    </div>
                  </td>
                </tr>
              )}
              {!loading && !error && users.map((user) => (
                <tr key={user.id} className="border-b border-border last:border-b-0 hover:bg-slate-50/50 dark:hover:bg-zinc-900/50 transition-colors">
                  <td className="px-4 py-3.5 font-medium text-foreground">{user.fullName}</td>
                  <td className="px-4 py-3.5 text-muted-foreground">{user.email}</td>
                  <td className="px-4 py-3.5">
                    <span className={statusConfig[user.status]?.class ?? 'badge-neutral'}>
                      {statusConfig[user.status]?.label ?? user.status}
                    </span>
                  </td>
                  <td className="px-4 py-3.5 text-muted-foreground text-xs">{formatDate(user.createdAt)}</td>
                  <td className="px-4 py-3.5 text-muted-foreground text-xs">{formatDate(user.lastLogin)}</td>
                  <td className="px-4 py-3.5 text-right">
                    <div className="flex items-center justify-end gap-1">
                      <button
                        onClick={() => router.push(`/system/maintenance/users/${user.id}/edit`)}
                        className="p-2 rounded-lg hover:bg-slate-100 dark:hover:bg-zinc-800 text-slate-500 hover:text-emerald-600 transition-colors"
                        title="Editar usuario"
                      >
                        <Edit className="w-4 h-4" />
                      </button>
                      <button
                        onClick={() => setConfirmAction({ type: 'resetPassword', user })}
                        className="p-2 rounded-lg hover:bg-slate-100 dark:hover:bg-zinc-800 text-slate-500 hover:text-amber-600 transition-colors"
                        title="Resetear contraseña"
                      >
                        <Key className="w-4 h-4" />
                      </button>
                      {user.status === 'Suspended' ? (
                        <button
                          onClick={() => setConfirmAction({ type: 'reactivate', user })}
                          className="p-2 rounded-lg hover:bg-slate-100 dark:hover:bg-zinc-800 text-slate-500 hover:text-emerald-600 transition-colors"
                          title="Reactivar usuario"
                        >
                          <ShieldCheck className="w-4 h-4" />
                        </button>
                      ) : (
                        <button
                          onClick={() => setConfirmAction({ type: 'suspend', user })}
                          className="p-2 rounded-lg hover:bg-slate-100 dark:hover:bg-zinc-800 text-slate-500 hover:text-rose-600 transition-colors"
                          title="Suspender usuario"
                        >
                          <ShieldOff className="w-4 h-4" />
                        </button>
                      )}
                      <button
                        onClick={() => router.push(`/system/maintenance/users/${user.id}/history`)}
                        className="p-2 rounded-lg hover:bg-slate-100 dark:hover:bg-zinc-800 text-slate-500 hover:text-blue-600 transition-colors"
                        title="Historial de cambios"
                      >
                        <History className="w-4 h-4" />
                      </button>
                      <button
                        onClick={() => setConfirmAction({ type: 'delete', user })}
                        className="p-2 rounded-lg hover:bg-slate-100 dark:hover:bg-zinc-800 text-slate-500 hover:text-rose-600 transition-colors"
                        title="Eliminar usuario"
                      >
                        <Trash2 className="w-4 h-4" />
                      </button>
                    </div>
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
        {!loading && !error && (
          <div className="px-4 py-3 border-t border-border bg-slate-50/50 dark:bg-slate-900/50 text-xs text-muted-foreground">
            {users.length} usuario{users.length !== 1 ? 's' : ''} encontrado{users.length !== 1 ? 's' : ''}
          </div>
        )}
      </div>

      {renderConfirmDialog()}
    </div>
  );
}
