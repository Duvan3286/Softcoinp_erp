'use client';

import React, { useState, useEffect } from 'react';
import { usePathname, useRouter } from 'next/navigation';
import { useSidebar } from '@/context/SidebarContext';
import { useAuth } from '@/context/AuthContext';
import {
  LayoutDashboard,
  Briefcase,
  Users,
  Database,
  Settings,
  ChevronRight,
  MessageSquare,
  Truck,
  Wrench,
  Gavel,
  Calendar,
  Megaphone,
  FileText,
  Shield,
  Globe,
} from 'lucide-react';

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

export const Sidebar = () => {
  const pathname = usePathname();
  const router = useRouter();
  const { isOpen, setIsOpen } = useSidebar();
  const { user } = useAuth();
  const [isExpanded, setIsExpanded] = useState(false);
  const [openGroup, setOpenGroup] = useState<string | null>(null);
  const isSuperAdmin = user?.role === 'SuperAdmin';
  const isDevTenant = user?.tenantSubdomain === 'dev';

  useEffect(() => {
    if (pathname.includes('billing') || pathname.includes('portfolio') || pathname.includes('budgets') || pathname.includes('interest')) setOpenGroup('finanzas');

    else if (pathname.startsWith('/residents')) setOpenGroup('residents');
    else if (pathname.startsWith('/suppliers') || pathname.startsWith('/contracts')) setOpenGroup('proveedores');
    else if (pathname.startsWith('/maintenance')) setOpenGroup('mantenimiento');
    else if (pathname.startsWith('/system/maintenance')) setOpenGroup('systemMaintenance');
    else if (pathname.startsWith('/pqr')) setOpenGroup('pqr');
    else if (pathname.startsWith('/reservation')) setOpenGroup('reservas');
    else if (pathname.startsWith('/communications')) setOpenGroup('comunicaciones');
    else if (pathname.startsWith('/reports')) setOpenGroup('reportes');
    setIsOpen(false);
  }, [pathname, setIsOpen]);

  const toggleGroup = (group: string) => {
    setOpenGroup(openGroup === group ? null : group);
  };

  return (
    <>
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
        <div className="h-1" />

        <nav className="flex flex-col gap-1.5 flex-grow px-3 overflow-y-auto overflow-x-hidden custom-scrollbar pb-4">

          {!isDevTenant && (
          <>
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
            icon={<MessageSquare className="w-5 h-5" />}
            text="PQR"
            isOpen={openGroup === 'pqr'}
            isExpanded={isExpanded}
            onToggle={() => toggleGroup('pqr')}
          >
            <NavItem text="Bandeja PQR" path="/pqr" currentPath={pathname} isExpanded={isExpanded} router={router} isSubItem />
            <NavItem text="Radicar PQR" path="/pqr/new" currentPath={pathname} isExpanded={isExpanded} router={router} isSubItem />
            <NavItem text="Indicadores" path="/pqr/indicators" currentPath={pathname} isExpanded={isExpanded} router={router} isSubItem />
          </NavGroup>

          <NavGroup
            icon={<Users className="w-5 h-5" />}
            text="Residentes y Prop."
            isOpen={openGroup === 'residents'}
            isExpanded={isExpanded}
            onToggle={() => toggleGroup('residents')}
          >
            <NavItem text="Propietarios" path="/residents" currentPath={pathname} isExpanded={isExpanded} router={router} isSubItem />
            <NavItem text="Arrendatarios" path="/residents/tenants" currentPath={pathname} isExpanded={isExpanded} router={router} isSubItem />
            <NavItem text="Residentes" path="/residents/directory" currentPath={pathname} isExpanded={isExpanded} router={router} isSubItem />
          </NavGroup>

          <NavGroup
            icon={<Truck className="w-5 h-5" />}
            text="Proveedores"
            isOpen={openGroup === 'proveedores'}
            isExpanded={isExpanded}
            onToggle={() => toggleGroup('proveedores')}
          >
            <NavItem text="Proveedores" path="/suppliers" currentPath={pathname} isExpanded={isExpanded} router={router} isSubItem />
            <NavItem text="Nuevo Proveedor" path="/suppliers/new" currentPath={pathname} isExpanded={isExpanded} router={router} isSubItem />
            <NavItem text="Contratos" path="/contracts" currentPath={pathname} isExpanded={isExpanded} router={router} isSubItem />
            <NavItem text="Nuevo Contrato" path="/contracts/new" currentPath={pathname} isExpanded={isExpanded} router={router} isSubItem />
            <NavItem text="Indicadores" path="/contracts/indicators" currentPath={pathname} isExpanded={isExpanded} router={router} isSubItem />
          </NavGroup>

          <NavGroup
            icon={<Wrench className="w-5 h-5" />}
            text="Mantenimiento"
            isOpen={openGroup === 'mantenimiento'}
            isExpanded={isExpanded}
            onToggle={() => toggleGroup('mantenimiento')}
          >
            <NavItem text="Inventario Bienes" path="/maintenance" currentPath={pathname} isExpanded={isExpanded} router={router} isSubItem />
            <NavItem text="Nuevo Bien" path="/maintenance/new" currentPath={pathname} isExpanded={isExpanded} router={router} isSubItem />
            <NavItem text="Calendario" path="/maintenance/calendar" currentPath={pathname} isExpanded={isExpanded} router={router} isSubItem />
            <NavItem text="Ordenes de Trabajo" path="/maintenance/work-orders" currentPath={pathname} isExpanded={isExpanded} router={router} isSubItem />
            <NavItem text="Nueva Orden" path="/maintenance/work-orders/new" currentPath={pathname} isExpanded={isExpanded} router={router} isSubItem />
            <NavItem text="Fuera de Servicio" path="/maintenance/out-of-service" currentPath={pathname} isExpanded={isExpanded} router={router} isSubItem />
            <NavItem text="Siniestros" path="/maintenance/incidents" currentPath={pathname} isExpanded={isExpanded} router={router} isSubItem />
            <NavItem text="Nuevo Siniestro" path="/maintenance/incidents/new" currentPath={pathname} isExpanded={isExpanded} router={router} isSubItem />
            <NavItem text="Reportes" path="/maintenance/reports" currentPath={pathname} isExpanded={isExpanded} router={router} isSubItem />
          </NavGroup>

          <NavGroup
            icon={<Gavel className="w-5 h-5" />}
            text="Asambleas"
            isOpen={openGroup === 'asambleas'}
            isExpanded={isExpanded}
            onToggle={() => toggleGroup('asambleas')}
          >
            <NavItem text="Asambleas" path="/assembly" currentPath={pathname} isExpanded={isExpanded} router={router} isSubItem />
            <NavItem text="Nueva Asamblea" path="/assembly/new" currentPath={pathname} isExpanded={isExpanded} router={router} isSubItem />
          </NavGroup>

          <NavGroup
            icon={<Calendar className="w-5 h-5" />}
            text="Reservas"
            isOpen={openGroup === 'reservas'}
            isExpanded={isExpanded}
            onToggle={() => toggleGroup('reservas')}
          >
            <NavItem text="Espacios" path="/reservation/spaces" currentPath={pathname} isExpanded={isExpanded} router={router} isSubItem />
            <NavItem text="Calendario" path="/reservation/calendar" currentPath={pathname} isExpanded={isExpanded} router={router} isSubItem />
            <NavItem text="Reservas" path="/reservation" currentPath={pathname} isExpanded={isExpanded} router={router} isSubItem />
            <NavItem text="Aprobaciones" path="/reservation/admin" currentPath={pathname} isExpanded={isExpanded} router={router} isSubItem />
          </NavGroup>

          <NavGroup
            icon={<Megaphone className="w-5 h-5" />}
            text="Comunicaciones"
            isOpen={openGroup === 'comunicaciones'}
            isExpanded={isExpanded}
            onToggle={() => toggleGroup('comunicaciones')}
          >
            <NavItem text="Comunicados" path="/communications" currentPath={pathname} isExpanded={isExpanded} router={router} isSubItem />
            <NavItem text="Nuevo Comunicado" path="/communications/new" currentPath={pathname} isExpanded={isExpanded} router={router} isSubItem />
            <NavItem text="Plantillas" path="/communications/templates" currentPath={pathname} isExpanded={isExpanded} router={router} isSubItem />
            <NavItem text="Cartelera" path="/communications/bulletin-board" currentPath={pathname} isExpanded={isExpanded} router={router} isSubItem />
            <NavItem text="Preferencias" path="/communications/preferences" currentPath={pathname} isExpanded={isExpanded} router={router} isSubItem />
            <NavItem text="Secuencia Mora" path="/communications/delinquency" currentPath={pathname} isExpanded={isExpanded} router={router} isSubItem />
          </NavGroup>

          <NavGroup
            icon={<FileText className="w-5 h-5" />}
            text="Reportes"
            isOpen={openGroup === 'reportes'}
            isExpanded={isExpanded}
            onToggle={() => toggleGroup('reportes')}
          >
            <NavItem text="Catalogo" path="/reports" currentPath={pathname} isExpanded={isExpanded} router={router} isSubItem />
            <NavItem text="Historial" path="/reports/history" currentPath={pathname} isExpanded={isExpanded} router={router} isSubItem />
            <NavItem text="Recurrentes" path="/reports/recurring" currentPath={pathname} isExpanded={isExpanded} router={router} isSubItem />
            <NavItem text="Informe Anual" path="/reports/annual" currentPath={pathname} isExpanded={isExpanded} router={router} isSubItem />
            <NavItem text="Membrete PDF" path="/reports/templates" currentPath={pathname} isExpanded={isExpanded} router={router} isSubItem />
          </NavGroup>

          <NavGroup
            icon={<Briefcase className="w-5 h-5" />}
            text="Finanzas"
            isOpen={openGroup === 'finanzas'}
            isExpanded={isExpanded}
            onToggle={() => toggleGroup('finanzas')}
          >
            <NavItem text="Cartera" path="/portfolio" currentPath={pathname} isExpanded={isExpanded} router={router} isSubItem />
            <NavItem text="Facturacion" path="/billing" currentPath={pathname} isExpanded={isExpanded} router={router} isSubItem />
            <NavItem text="Registrar Pago" path="/billing/payments/register" currentPath={pathname} isExpanded={isExpanded} router={router} isSubItem />
            <NavItem text="Cuotas Extraordinarias" path="/billing/extraordinary-fees" currentPath={pathname} isExpanded={isExpanded} router={router} isSubItem />
            <NavItem text="Cobros Individuales" path="/billing/individual-charges" currentPath={pathname} isExpanded={isExpanded} router={router} isSubItem />
            <NavItem text="Intereses" path="/billing/interest" currentPath={pathname} isExpanded={isExpanded} router={router} isSubItem />
            <NavItem text="Documentos" path="/billing/documents" currentPath={pathname} isExpanded={isExpanded} router={router} isSubItem />
            <NavItem text="Presupuesto" path="/budgets" currentPath={pathname} isExpanded={isExpanded} router={router} isSubItem />

          </NavGroup>

          <NavItem
            icon={<Settings className="w-5 h-5" />}
            text="Configuracion"
            path="/settings/tenant"
            currentPath={pathname}
            isExpanded={isExpanded}
            router={router}
          />
          </>
          )}

          {isSuperAdmin && (
            <NavGroup
              icon={<Shield className="w-5 h-5" />}
              text="Mantenimiento del Sistema"
              isOpen={openGroup === 'systemMaintenance'}
              isExpanded={isExpanded}
              onToggle={() => toggleGroup('systemMaintenance')}
            >
              <NavItem text="Usuarios Administradores" path="/system/maintenance/users" currentPath={pathname} isExpanded={isExpanded} router={router} isSubItem />
              <NavItem text="Próximamente..." path="/system/maintenance/coming-soon" currentPath={pathname} isExpanded={isExpanded} router={router} isSubItem highlightRed />
            </NavGroup>
          )}

          {isSuperAdmin && isDevTenant && (
            <NavItem
              icon={<Globe className="w-5 h-5" />}
              text="Gestión de Tenants"
              path="/system/tenants"
              currentPath={pathname}
              isExpanded={isExpanded}
              router={router}
            />
          )}

        </nav>

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
