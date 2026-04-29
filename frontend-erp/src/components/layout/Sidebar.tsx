'use client';

import React from 'react';
import Link from 'next/link';
import { usePathname } from 'next/navigation';
import { 
  LayoutDashboard, 
  Users, 
  Settings, 
  Database,
  Briefcase,
  FileText
} from 'lucide-react';

const menuItems = [
  { icon: LayoutDashboard, label: 'Dashboard', href: '/dashboard' },
  { icon: Briefcase, label: 'Cartera', href: '/portfolio' },
  { icon: FileText, label: 'Facturación', href: '/billing' },
  { icon: Users, label: 'Usuarios', href: '/users' },
  { icon: Database, label: 'Integraciones', href: '/integrations' },
  { icon: Settings, label: 'Configuración', href: '/settings' },
];

export const Sidebar = () => {
  const pathname = usePathname();

  return (
    <aside className="w-64 border-r border-border bg-card flex flex-col hidden lg:flex">
      <div className="h-14 flex items-center px-6 border-b border-border bg-emerald-600">
        <div className="flex items-center gap-2 text-white">
          <Database size={22} strokeWidth={3} />
          <span className="font-black tracking-tighter text-xl">SOFTCOINP</span>
        </div>
      </div>

      <nav className="flex-1 p-4 space-y-1">
        {menuItems.map((item) => {
          const isActive = pathname === item.href;
          return (
            <Link
              key={item.href}
              href={item.href}
              className={`flex items-center gap-3 px-3 py-2 rounded-lg text-sm font-medium transition-all ${
                isActive 
                  ? 'bg-emerald-50 text-emerald-700 dark:bg-emerald-950/30 dark:text-emerald-400' 
                  : 'text-muted-foreground hover:bg-accent hover:text-foreground'
              }`}
            >
              <item.icon size={18} strokeWidth={isActive ? 2.5 : 2} />
              {item.label}
            </Link>
          );
        })}
      </nav>

      <div className="p-4 border-t border-border">
        <div className="p-4 rounded-xl bg-emerald-50 dark:bg-emerald-950/20 border border-emerald-100 dark:border-emerald-900/30">
          <p className="text-xs font-bold text-emerald-800 dark:text-emerald-400 mb-1">PRO PLAN</p>
          <p className="text-[10px] text-emerald-600 dark:text-emerald-500 leading-tight">
            Acceso total a todas las integraciones de Softcoinp.
          </p>
        </div>
      </div>
    </aside>
  );
};
