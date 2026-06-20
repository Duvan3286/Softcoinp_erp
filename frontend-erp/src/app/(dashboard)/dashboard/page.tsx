'use client';

import React from 'react';
import { useAuth } from '@/context/AuthContext';
import { LayoutDashboard, Users, CreditCard, Settings, LogOut } from 'lucide-react';
import { Card, CardContent } from '@/components/ui/Card';
import { Button } from '@/components/ui/Button';

export default function DashboardPage() {
  const { user, logout } = useAuth();

  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-2xl font-black tracking-tight">Bienvenido de nuevo, {user?.name}</h1>
          <p className="text-muted-foreground">Esto es lo que está pasando en tu tenant hoy.</p>
        </div>
        <Button variant="secondary" onClick={logout} className="gap-2">
          <LogOut size={18} />
          <span>Cerrar Sesión</span>
        </Button>
      </div>

      <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-6">
        <StatCard title="Usuarios Activos" value="24" icon={<Users className="text-emerald-600" />} />
        <StatCard title="Ingresos Totales" value="$12,450" icon={<CreditCard className="text-cyan-600" />} />
        <StatCard title="Almacenamiento" value="85%" icon={<LayoutDashboard className="text-emerald-600" />} />
        <StatCard title="Configuración" value="Actualizada" icon={<Settings className="text-cyan-600" />} />
      </div>

      <Card className="h-64 flex items-center justify-center bg-background/50 border-dashed">
        <CardContent>
          <p className="text-muted-foreground italic text-lg">
            El contenido principal del dashboard se cargará aquí...
          </p>
        </CardContent>
      </Card>
    </div>
  );
}

function StatCard({ title, value, icon }: { title: string; value: string; icon: React.ReactNode }) {
  return (
    <Card>
      <CardContent className="p-6">
        <div className="flex items-center justify-between mb-4">
          <div className="p-2.5 bg-emerald-50 dark:bg-emerald-950/30 rounded-xl">{icon}</div>
        </div>
        <p className="text-xs font-bold text-muted-foreground uppercase tracking-wider">{title}</p>
        <p className="text-2xl font-black mt-1">{value}</p>
      </CardContent>
    </Card>
  );
}
