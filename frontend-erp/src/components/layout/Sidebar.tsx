'use client';

import React, { useState, useEffect } from 'react';
import { usePathname, useRouter } from 'next/navigation';
import { useSidebar } from '@/context/SidebarContext';
import {
  LayoutDashboard,
  Briefcase,
  FileText,
  Users,
  Database,
  Settings,
  ChevronRight,
} from 'lucide-react';

// ─────────────────────────────────────────────
// Types
// ─────────────────────────────────────────────
interface NavItemProps {
  icon?: React.ReactNode;
  text: string;
  path: string;
  currentPath: string;
  isExpanded: boolean;
  router: ReturnType<typeof useRouter>;
  isSubItem?: boolean;
  highlightRed?: boolean;
}

interface NavGroupProps {
  icon: React.ReactNode;
  text: string;
  isOpen: boolean;
  isExpanded: boolean;
  onToggle: () => void;
  children: React.ReactNode;
}

// ─────────────────────────────────────────────
// Sidebar
// ─────────────────────────────────────────────
export const Sidebar = () => {
  const pathname = usePathname();
  const router = useRouter();
  const { isOpen, setIsOpen } = useSidebar();
  const [isExpanded, setIsExpanded] = useState(false);
  const [openGroup, setOpenGroup] = useState<string | null>(null);

  // Auto-open groups based on current path
  useEffect(() => {
    if (pathname.includes('billing') || pathname.includes('portfolio')) setOpenGroup('finanzas');
    else if (pathname.includes('users') || pathname.includes('integrations')) setOpenGroup('admin');
    // Close sidebar on mobile when navigating
    setIsOpen(false);
  }, [pathname, setIsOpen]);

  const toggleGroup = (group: string) => {
    setOpenGroup(openGroup === group ? null : group);
  };

  return (
    <>
      {/* 🌑 BACKDROP (Solo móvil) */}
      {isOpen && (
        <div
          className="fixed inset-0 bg-black/60 backdrop-blur-sm z-[115] lg:hidden animate-in fade-in duration-300"
          onClick={() => setIsOpen(false)}
        />
      )}

      <aside
        className={`bg-card text-foreground flex flex-col pt-2 pb-4 flex-shrink-0 shadow-[4px_0_24px_rgba(0,0,0,0.05)] dark:shadow-none border-r border-border transition-all duration-300 ease-in-out
          fixed inset-y-0 left-0 z-[120] lg:static lg:z-10 h-full
          ${isOpen ? 'translate-x-0' : '-translate-x-full lg:translate-x-0'}
          ${isExpanded ? 'w-72' : 'w-72 lg:w-[88px]'}
        `}
        onMouseEnter={() => setIsExpanded(true)}
        onMouseLeave={() => setIsExpanded(false)}
      >
        {/* Logo area */}
        <div className={`flex items-center gap-2.5 px-3 mb-4 overflow-hidden ${isExpanded ? '' : 'justify-center'}`}>
          <div className="w-9 h-9 rounded-xl bg-emerald-700 flex items-center justify-center flex-shrink-0 shadow-md">
            <svg className="w-5 h-5 text-white" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path strokeLinecap="round" strokeLinejoin="round" strokeWidth="2.5"
                d="M9 12l2 2 4-4m5.618-4.016A11.955 11.955 0 0112 2.944a11.955 11.955 0 01-8.618 3.04A12.02 12.02 0 003 9c0 5.591 3.824 10.29 9 11.622 5.176-1.332 9-6.03 9-11.622 0-1.042-.133-2.052-.382-3.016z" />
            </svg>
          </div>
          <span className={`font-black text-foreground tracking-tight text-lg whitespace-nowrap transition-opacity duration-300 ${isExpanded ? 'opacity-100' : 'lg:opacity-0 lg:w-0 lg:hidden'}`}>
            SOFTCOINP
          </span>
        </div>

        {/* Divider */}
        <div className="mx-3 mb-3 border-t border-border" />

        {/* Navigation */}
        <nav className="flex flex-col gap-1.5 flex-grow px-3 overflow-y-auto overflow-x-hidden custom-scrollbar pb-4">

          <NavItem
            icon={<LayoutDashboard className="w-5 h-5" />}
            text="Dashboard"
            path="/dashboard"
            currentPath={pathname}
            isExpanded={isExpanded}
            router={router}
          />

          <NavItem
            icon={<Database className="w-5 h-5" />}
            text="Unidades"
            path="/units"
            currentPath={pathname}
            isExpanded={isExpanded}
            router={router}
          />

          <NavGroup
            icon={<Briefcase className="w-5 h-5" />}
            text="Finanzas"
            isOpen={openGroup === 'finanzas'}
            isExpanded={isExpanded}
            onToggle={() => toggleGroup('finanzas')}
          >
            <NavItem text="Cartera" path="/portfolio" currentPath={pathname} isExpanded={isExpanded} router={router} isSubItem />
            <NavItem text="Facturación" path="/billing" currentPath={pathname} isExpanded={isExpanded} router={router} isSubItem />
          </NavGroup>

          <NavGroup
            icon={<Users className="w-5 h-5" />}
            text="Administración"
            isOpen={openGroup === 'admin'}
            isExpanded={isExpanded}
            onToggle={() => toggleGroup('admin')}
          >
            <NavItem text="Usuarios" path="/users" currentPath={pathname} isExpanded={isExpanded} router={router} isSubItem />
            <NavItem text="Integraciones" path="/integrations" currentPath={pathname} isExpanded={isExpanded} router={router} isSubItem />
          </NavGroup>

          <NavItem
            icon={<Settings className="w-5 h-5" />}
            text="Configuración"
            path="/settings/tenant"
            currentPath={pathname}
            isExpanded={isExpanded}
            router={router}
          />

        </nav>

        {/* Footer */}
        <div className="mt-auto pt-4 border-t border-border text-center overflow-hidden flex flex-col items-center justify-center min-h-[40px]">
          {isExpanded ? (
            <p className="text-[10px] text-slate-400 dark:text-slate-500 font-bold tracking-widest whitespace-nowrap animate-in fade-in duration-300 uppercase">
              Softcoinp ERP v1.0.0
            </p>
          ) : (
            <p className="text-[10px] text-slate-400 dark:text-slate-500 font-bold tracking-widest uppercase">
              ERP
            </p>
          )}
        </div>
      </aside>
    </>
  );
};

// ─────────────────────────────────────────────
// NavGroup
// ─────────────────────────────────────────────
function NavGroup({ icon, text, isOpen, isExpanded, onToggle, children }: NavGroupProps) {
  return (
    <div className="flex flex-col">
      <button
        onClick={onToggle}
        className={`flex items-center p-3 rounded-xl transition-all duration-200 font-medium border whitespace-nowrap overflow-hidden group relative w-full
          ${isOpen
            ? 'bg-emerald-50 dark:bg-emerald-950/20 text-emerald-700 dark:text-emerald-400 border-emerald-100 dark:border-emerald-900/50 shadow-sm'
            : 'text-slate-500 dark:text-slate-400 border-transparent hover:bg-slate-50 dark:hover:bg-zinc-900 hover:text-slate-800 dark:hover:text-slate-200'}
        `}
        title={text}
      >
        <span className="flex-shrink-0 flex items-center justify-center w-6 group-hover:scale-110 transition-transform">
          {icon}
        </span>
        <span
          className={`ml-3 text-sm transition-opacity duration-300 flex-1 text-left tracking-wide font-semibold ${isExpanded ? 'lg:opacity-100 lg:w-auto' : 'lg:opacity-0 lg:w-0 lg:hidden'}`}
        >
          {text}
        </span>
        {isExpanded && (
          <span className={`transition-transform duration-300 flex-shrink-0 ${isOpen ? 'rotate-90 text-emerald-600 dark:text-emerald-400' : 'text-slate-400'}`}>
            <ChevronRight className="w-4 h-4" />
          </span>
        )}
      </button>

      {isOpen && (
        <div className={`flex flex-col gap-1 mt-1 mb-2 relative before:absolute before:inset-y-2 before:left-[24px] before:w-[1px] before:bg-slate-200 dark:before:bg-zinc-800 animate-in slide-in-from-top-1 duration-200 ${!isExpanded ? 'lg:hidden' : ''}`}>
          {children}
        </div>
      )}
    </div>
  );
}

// ─────────────────────────────────────────────
// NavItem
// ─────────────────────────────────────────────
function NavItem({ icon, text, path, currentPath, isExpanded, router, isSubItem = false, highlightRed = false }: NavItemProps) {
  const isActive = currentPath === path || currentPath.startsWith(path + '/');

  let baseClass = `flex items-center rounded-xl transition-all duration-200 whitespace-nowrap overflow-hidden group relative w-full border ${
    isSubItem
      ? 'pl-6 pr-3 py-2.5 font-medium text-[13px]'
      : 'p-3 font-semibold text-sm'
  } `;

  if (highlightRed) {
    baseClass += `text-rose-600 dark:text-rose-400 border-transparent hover:bg-rose-50 dark:hover:bg-rose-950/20 hover:border-rose-200 dark:hover:border-rose-900/50 `;
  } else if (isActive) {
    if (isSubItem) {
      baseClass += `text-emerald-600 dark:text-emerald-400 border-transparent font-bold bg-transparent`;
    } else {
      baseClass += `bg-emerald-50 dark:bg-emerald-950/30 text-emerald-700 dark:text-emerald-400 border-emerald-200 dark:border-emerald-800 shadow-sm `;
    }
  } else {
    baseClass += `text-slate-500 dark:text-slate-400 border-transparent hover:text-slate-800 dark:hover:text-slate-200 ${!isSubItem ? 'hover:bg-slate-50 dark:hover:bg-zinc-900' : ''} `;
  }

  return (
    <button onClick={() => router.push(path)} className={baseClass} title={text}>
      {!isSubItem && (
        <span className={`flex-shrink-0 flex items-center justify-center w-6 group-hover:scale-110 transition-transform ${isActive ? 'drop-shadow-md z-10 relative' : 'z-10 relative'}`}>
          {icon}
        </span>
      )}

      {isSubItem && isActive && (
        <div className="absolute left-[21px] top-1/2 -translate-y-1/2 w-1.5 h-1.5 rounded-full bg-emerald-500 z-10 shadow-sm ring-4 ring-card" />
      )}

      <span
        className={`transition-opacity duration-300 flex-1 text-left tracking-wide ${(!isExpanded && !isSubItem) ? 'lg:opacity-0 lg:w-0 lg:hidden' : ''} ${!isSubItem ? 'ml-3' : 'ml-6'}`}
      >
        {text}
      </span>

      {!isExpanded && isActive && !isSubItem && (
        <div className="absolute left-0 top-1/2 -translate-y-1/2 w-1 h-1/2 bg-emerald-600 dark:bg-emerald-500 rounded-r-full shadow-sm"></div>
      )}
    </button>
  );
}

// Trigger refresh
