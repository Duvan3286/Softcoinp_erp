'use client';

import React, { createContext, useContext, useState, useEffect, ReactNode } from 'react';
import { User, LoginCredentials } from '../lib/auth-service';
import authService from '../lib/auth-service';
import { setAuthCookie, clearAuthCookie } from '../lib/api-client';
import { useRouter } from 'next/navigation';

interface AuthContextType {
  user: User | null;
  isAuthenticated: boolean;
  isLoading: boolean;
  login: (credentials: LoginCredentials) => Promise<void>;
  logout: () => Promise<void>;
}

const AuthContext = createContext<AuthContextType | undefined>(undefined);

function readTokenFromCookie(): string | null {
  if (typeof window === 'undefined') return null;
  const match = document.cookie.match(/(?:^|;\s*)auth_token=([^;]*)/);
  return match ? decodeURIComponent(match[1]) : null;
}

export function AuthProvider({ children }: { children: ReactNode }) {
  const [user, setUser] = useState<User | null>(null);
  const [isLoading, setIsLoading] = useState(true);
  const router = useRouter();

  useEffect(() => {
    const initAuth = async () => {
      const cookieToken = readTokenFromCookie();
      const sessionToken = typeof window !== 'undefined' ? sessionStorage.getItem('auth_token') : null;
      const token = sessionToken || cookieToken;

      if (token) {
        if (!sessionToken) {
          sessionStorage.setItem('auth_token', token);
          setAuthCookie(token);
        }
        try {
          const currentUser = await authService.getCurrentUser();
          setUser(currentUser);
        } catch {
          sessionStorage.removeItem('auth_token');
          sessionStorage.removeItem('refresh_token');
          clearAuthCookie();
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
      if (typeof window !== 'undefined') {
        sessionStorage.removeItem('auth_token');
        sessionStorage.removeItem('refresh_token');
        clearAuthCookie();
      }
      setUser(null);
      router.push('/login');
    }
  };

  return (
    <AuthContext.Provider
      value={{
        user,
        isAuthenticated: !!user,
        isLoading,
        login,
        logout,
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
