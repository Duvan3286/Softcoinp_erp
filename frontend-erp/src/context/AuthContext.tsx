'use client';

import React, { createContext, useContext, useState, useEffect, ReactNode } from 'react';
import { User, LoginCredentials, TenantOption } from '../lib/auth-service';
import authService from '../lib/auth-service';
import { setAuthCookie, clearAuthCookie } from '../lib/api-client';
import { useRouter } from 'next/navigation';

const API_URL = process.env.NEXT_PUBLIC_API_URL || '/api';
const isSameOrigin = API_URL.startsWith('/');

interface AuthContextType {
  user: User | null;
  isAuthenticated: boolean;
  isLoading: boolean;
  availableTenants: TenantOption[];
  login: (credentials: LoginCredentials) => Promise<void>;
  logout: () => Promise<void>;
  switchTenant: (tenantId: string) => Promise<void>;
}

const AuthContext = createContext<AuthContextType | undefined>(undefined);

export function AuthProvider({ children }: { children: ReactNode }) {
  const [user, setUser] = useState<User | null>(null);
  const [isLoading, setIsLoading] = useState(true);
  const [availableTenants, setAvailableTenants] = useState<TenantOption[]>([]);
  const router = useRouter();

  const loadAvailableTenants = async () => {
    try {
      const tenants = await authService.getMyTenants();
      setAvailableTenants(tenants);
    } catch {
      setAvailableTenants([]);
    }
  };

  useEffect(() => {
    const initAuth = async () => {
      let token: string | null = null;

      // Modo cross-origen: recuperar token de sessionStorage o cookie
      if (!isSameOrigin && typeof window !== 'undefined') {
        token = sessionStorage.getItem('auth_token');
        if (!token) {
          const match = document.cookie.match(/(?:^|;\s*)auth_token=([^;]*)/);
          if (match) {
            token = decodeURIComponent(match[1]);
            sessionStorage.setItem('auth_token', token);
          }
        }
      }

      // Modo mismo origen: la cookie httpOnly se envía automáticamente
      // Solo intentar getCurrentUser si hay un token (cross-origen) o siempre (mismo origen)
      if (isSameOrigin || token) {
        try {
          const currentUser = await authService.getCurrentUser();
          setUser(currentUser);
          await loadAvailableTenants();
        } catch {
          if (!isSameOrigin && typeof window !== 'undefined') {
            sessionStorage.removeItem('auth_token');
            sessionStorage.removeItem('refresh_token');
            clearAuthCookie();
          }
        }
      }
      setIsLoading(false);
    };

    initAuth();
  }, []);

  const login = async (credentials: LoginCredentials) => {
    setIsLoading(true);
    try {
      const response = await authService.login(credentials);
      setUser(response.user);
      await loadAvailableTenants();
      router.push('/dashboard');
    } catch (error) {
      throw error;
    } finally {
      setIsLoading(false);
    }
  };

  const logout = async () => {
    try {
      await authService.logout();
    } catch {
      // ignore logout errors
    } finally {
      if (!isSameOrigin && typeof window !== 'undefined') {
        sessionStorage.removeItem('auth_token');
        sessionStorage.removeItem('refresh_token');
        clearAuthCookie();
      }
      setUser(null);
      setAvailableTenants([]);
    }
  };

  const switchTenant = async (tenantId: string) => {
    await authService.switchTenant(tenantId);
    // Recarga completa del contexto: todos los datos del dashboard y del resto
    // de módulos deben volver a pedirse contra el nuevo conjunto, sin cerrar sesión.
    if (typeof window !== 'undefined') {
      window.location.href = '/dashboard';
    }
  };

  return (
    <AuthContext.Provider
      value={{
        user,
        isAuthenticated: !!user,
        isLoading,
        availableTenants,
        login,
        logout,
        switchTenant,
      }}
    >
      {children}
    </AuthContext.Provider>
  );
}

export function useAuth() {
  const context = useContext(AuthContext);
  if (context === undefined) {
    throw new Error('useAuth must be used within an AuthProvider');
  }
  return context;
}
