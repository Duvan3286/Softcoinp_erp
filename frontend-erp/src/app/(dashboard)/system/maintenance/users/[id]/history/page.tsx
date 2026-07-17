'use client';

import React, { useState, useEffect } from 'react';
import { useRouter, useParams } from 'next/navigation';
import { ArrowLeft, Loader2, AlertTriangle, History, UserPlus, Edit, Trash2, ShieldOff, ShieldCheck, Key } from 'lucide-react';
import systemMaintenanceService, { UserChangeHistoryItem } from '@/lib/system-maintenance-service';

const changeTypeConfig: Record<string, { label: string; icon: React.ReactNode; color: string }> = {
  Created: {
    label: 'Creación',
    icon: <UserPlus className="w-4 h-4" />,
    color: 'text-emerald-600 bg-emerald-50 dark:bg-emerald-950/20 border-emerald-100 dark:border-emerald-900',
  },
  Edited: {
    label: 'Edición',
    icon: <Edit className="w-4 h-4" />,
    color: 'text-blue-600 bg-blue-50 dark:bg-blue-950/20 border-blue-100 dark:border-blue-900',
  },
  Deleted: {
    label: 'Eliminación',
    icon: <Trash2 className="w-4 h-4" />,
    color: 'text-rose-600 bg-rose-50 dark:bg-rose-950/20 border-rose-100 dark:border-rose-900',
  },
  Suspended: {
    label: 'Suspensión',
    icon: <ShieldOff className="w-4 h-4" />,
    color: 'text-rose-600 bg-rose-50 dark:bg-rose-950/20 border-rose-100 dark:border-rose-900',
  },
  Reactivated: {
    label: 'Reactivación',
    icon: <ShieldCheck className="w-4 h-4" />,
    color: 'text-emerald-600 bg-emerald-50 dark:bg-emerald-950/20 border-emerald-100 dark:border-emerald-900',
  },
  PasswordReset: {
    label: 'Reset de contraseña',
    icon: <Key className="w-4 h-4" />,
    color: 'text-amber-600 bg-amber-50 dark:bg-amber-950/20 border-amber-100 dark:border-amber-900',
  },
};

function formatDateTime(dateStr: string): string {
  const d = new Date(dateStr);
  return d.toLocaleDateString('es-CO', {
    year: 'numeric',
    month: 'long',
    day: 'numeric',
    hour: '2-digit',
    minute: '2-digit',
    second: '2-digit',
  });
}

export default function UserHistoryPage() {
  const router = useRouter();
  const params = useParams();
  const userId = params.id as string;

  const [history, setHistory] = useState<UserChangeHistoryItem[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');
  const [userName, setUserName] = useState('');

  useEffect(() => {
    const fetchData = async () => {
      setLoading(true);
      setError('');
      try {
        const user = await systemMaintenanceService.getUser(userId);
        setUserName(user.fullName);
        const data = await systemMaintenanceService.getUserHistory(userId);
        setHistory(data);
      } catch {
        setError('Error al cargar el historial del usuario.');
      } finally {
        setLoading(false);
      }
    };
    fetchData();
  }, [userId]);

  return (
    <div>
      <button
        onClick={() => router.push('/system/maintenance/users')}
        className="flex items-center gap-2 text-sm text-muted-foreground hover:text-foreground mb-4 transition-colors"
      >
        <ArrowLeft className="w-4 h-4" />
        Volver al listado
      </button>

      <div className="bg-card border border-border rounded-2xl shadow-sm overflow-hidden">
        <div className="px-6 py-5 border-b border-border">
          <div className="flex items-center gap-3">
            <div className="p-2 rounded-xl bg-blue-50 dark:bg-blue-950/20 border border-blue-100 dark:border-blue-900">
              <History className="w-5 h-5 text-blue-600 dark:text-blue-400" />
            </div>
            <div>
              <h2 className="text-lg font-bold text-foreground">Historial de cambios</h2>
              <p className="text-sm text-muted-foreground">
                {userName ? `Eventos registrados para ${userName}` : 'Cargando...'}
              </p>
            </div>
          </div>
        </div>

        <div className="p-6">
          {loading && (
            <div className="flex items-center justify-center py-12">
              <Loader2 className="w-6 h-6 animate-spin text-muted-foreground" />
            </div>
          )}

          {!loading && error && (
            <div className="p-3 bg-rose-50 border border-rose-200 rounded-lg text-rose-700 text-sm flex items-center gap-2">
              <AlertTriangle className="w-4 h-4" />
              {error}
            </div>
          )}

          {!loading && !error && history.length === 0 && (
            <div className="text-center py-12 text-muted-foreground text-sm">
              No hay eventos registrados para este usuario.
            </div>
          )}

          {!loading && !error && history.length > 0 && (
            <div className="relative">
              <div className="absolute left-[19px] top-3 bottom-3 w-0.5 bg-slate-200 dark:bg-zinc-800" />
              <div className="space-y-4">
                {history.map((item) => {
                  const config = changeTypeConfig[item.changeType] ?? {
                    label: item.changeType,
                    icon: <History className="w-4 h-4" />,
                    color: 'text-slate-600 bg-slate-50 dark:bg-zinc-900 border-slate-200 dark:border-zinc-700',
                  };

                  return (
                    <div key={item.id} className="relative pl-10">
                      <div className={`absolute left-0 top-1 p-1 rounded-lg border ${config.color}`}>
                        {config.icon}
                      </div>
                      <div className="bg-slate-50 dark:bg-zinc-900/50 border border-border rounded-xl p-4">
                        <div className="flex items-center justify-between mb-2">
                          <span className="inline-flex items-center gap-1.5 px-2.5 py-0.5 rounded-full text-xs font-semibold bg-slate-100 dark:bg-zinc-800 text-slate-700 dark:text-slate-300">
                            {config.label}
                          </span>
                          <span className="text-xs text-muted-foreground">
                            {formatDateTime(item.changedAt)}
                          </span>
                        </div>
                        <div className="text-sm space-y-1">
                          <p>
                            <span className="font-medium text-foreground">Campo:</span>{' '}
                            <span className="text-muted-foreground">{item.changedField}</span>
                          </p>
                          {item.oldValue && (
                            <p>
                              <span className="font-medium text-foreground">Valor anterior:</span>{' '}
                              <span className="text-muted-foreground">{item.oldValue}</span>
                            </p>
                          )}
                          {item.newValue && (
                            <p>
                              <span className="font-medium text-foreground">Valor nuevo:</span>{' '}
                              <span className="text-muted-foreground">{item.newValue}</span>
                            </p>
                          )}
                        </div>
                      </div>
                    </div>
                  );
                })}
              </div>
            </div>
          )}
        </div>
      </div>
    </div>
  );
}
