'use client';

import React from 'react';
import { useAuth } from '@/context/AuthContext';
import { Bell, Search } from 'lucide-react';

export const Header = () => {
  const { user, logout } = useAuth();

  return (
    <header className="h-14 border-b border-border bg-card px-6 flex items-center justify-between sticky top-0 z-40 transition-colors">
      <div className="flex items-center gap-4 flex-1">
        <div className="relative w-96 hidden md:block">
          <Search className="absolute left-3 top-1/2 -translate-y-1/2 text-slate-400" size={16} />
          <input 
            type="text" 
            placeholder="Buscar..." 
            className="w-full bg-background border border-border rounded-lg py-1.5 pl-10 pr-4 text-sm tracking-widest focus:ring-2 focus:ring-ring focus:outline-none transition-all"
          />
        </div>
      </div>

      <div className="flex items-center gap-4">
        <button className="p-2 hover:bg-slate-100 dark:hover:bg-slate-800 rounded-lg transition-colors relative">
          <Bell size={18} />
          <span className="absolute top-1.5 right-1.5 w-2 h-2 bg-emerald-600 rounded-full border-2 border-card"></span>
        </button>

        <div className="h-8 w-[1px] bg-border mx-2"></div>

        <div className="flex items-center gap-3">
          <div className="text-right hidden sm:block">
            <p className="text-sm font-black text-foreground leading-none">{user?.name}</p>
            <p className="text-xs text-slate-500 mt-1 capitalize font-medium">{user?.role}</p>
          </div>
          <div className="h-9 w-9 rounded-lg bg-emerald-600 flex items-center justify-center text-white font-black">
            {user?.name?.charAt(0) || 'U'}
          </div>
          <button 
            onClick={logout}
            className="text-xs font-bold text-slate-400 hover:text-rose-600 transition-colors ml-2 tracking-widest"
          >
            Salir
          </button>
        </div>
      </div>
    </header>
  );
};
