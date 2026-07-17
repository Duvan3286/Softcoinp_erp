'use client';

import React from 'react';
import { usePathname, useRouter } from 'next/navigation';
import { Shield, Users, Clock } from 'lucide-react';

export default function SystemMaintenanceLayout({
  children,
}: {
  children: React.ReactNode;
}) {
  const pathname = usePathname();
  const router = useRouter();

  const menuItems = [
    {
      label: 'Usuarios Administradores',
      icon: <Users className="w-4 h-4" />,
      path: '/system/maintenance/users',
      active: pathname.startsWith('/system/maintenance/users'),
    },
    {
      label: 'Próximamente',
      icon: <Clock className="w-4 h-4" />,
      path: '#',
      active: false,
      disabled: true,
    },
  ];

  return (
    <div>
      <div className="flex items-center gap-3 mb-6">
        <div className="p-2.5 rounded-xl bg-emerald-50 dark:bg-emerald-950/20 border border-emerald-100 dark:border-emerald-900/50">
          <Shield className="w-5 h-5 text-emerald-600 dark:text-emerald-400" />
        </div>
        <div>
          <h1 className="text-xl font-bold text-foreground tracking-tight">Mantenimiento del Sistema</h1>
          <p className="text-sm text-muted-foreground">Configuración y personalización del conjunto</p>
        </div>
      </div>

      <div className="flex gap-6">
        <nav className="w-56 flex-shrink-0 space-y-1">
          {menuItems.map((item) => (
            <button
              key={item.label}
              onClick={() => {
                if (!item.disabled) {
                  router.push(item.path);
                }
              }}
              disabled={item.disabled}
              className={`w-full flex items-center gap-3 px-4 py-2.5 rounded-xl text-sm font-medium transition-all duration-200 border ${
                item.active
                  ? 'bg-emerald-50 dark:bg-emerald-950/20 text-emerald-700 dark:text-emerald-400 border-emerald-100 dark:border-emerald-900/50 shadow-sm'
                  : item.disabled
                  ? 'text-slate-300 dark:text-slate-600 border-transparent cursor-not-allowed'
                  : 'text-slate-500 dark:text-slate-400 border-transparent hover:bg-slate-50 dark:hover:bg-zinc-900 hover:text-slate-800 dark:hover:text-slate-200'
              }`}
            >
              <span className="flex-shrink-0">{item.icon}</span>
              <span className="text-left">{item.label}</span>
              {item.disabled && (
                <span className="ml-auto text-[10px] text-slate-300 dark:text-slate-600 font-medium">PRONTO</span>
              )}
            </button>
          ))}
          <div className="pt-3 mt-3 border-t border-border">
            <p className="text-[10px] text-slate-400 dark:text-slate-500 font-semibold tracking-widest uppercase px-4">
              Configuración
            </p>
          </div>
          <button
            disabled
            className="w-full flex items-center gap-3 px-4 py-2.5 rounded-xl text-sm font-medium text-slate-300 dark:text-slate-600 border-transparent cursor-not-allowed"
          >
            <Clock className="w-4 h-4" />
            <span>Próximamente...</span>
          </button>
        </nav>

        <div className="flex-1 min-w-0">
          {children}
        </div>
      </div>
    </div>
  );
}
