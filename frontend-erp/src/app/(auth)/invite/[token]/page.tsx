'use client';

import React, { useState, useEffect } from 'react';
import { useParams, useRouter } from 'next/navigation';
import { Shield, AlertCircle, Loader2, CheckCircle2 } from 'lucide-react';
import { Button } from '@/components/ui/Button';
import apiClient from '@/lib/api-client';

export default function InvitePage() {
  const { token } = useParams<{ token: string }>();
  const router = useRouter();

  const [isLoading, setIsLoading] = useState(true);
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [error, setError] = useState('');
  const [success, setSuccess] = useState(false);
  
  const [inviteData, setInviteData] = useState<{ email: string; role: string; tenantId: string } | null>(null);
  
  const [fullName, setFullName] = useState('');
  const [password, setPassword] = useState('');
  const [confirmPassword, setConfirmPassword] = useState('');

  useEffect(() => {
    const validateToken = async () => {
      try {
        const res = await apiClient.get(`/invitations/${token}`);
        setInviteData(res.data);
      } catch (err: any) {
        setError(err.response?.data?.message || 'La invitación es inválida o ha expirado.');
      } finally {
        setIsLoading(false);
      }
    };
    if (token) validateToken();
  }, [token]);

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setError('');

    if (password !== confirmPassword) {
      setError('Las contraseñas no coinciden.');
      return;
    }
    if (password.length < 8) {
      setError('La contraseña debe tener al menos 8 caracteres.');
      return;
    }

    setIsSubmitting(true);
    try {
      await apiClient.post(`/invitations/${token}/accept`, {
        fullName,
        password
      });
      setSuccess(true);
      setTimeout(() => router.push('/login'), 3000);
    } catch (err: any) {
      setError(err.response?.data?.message || 'Error al aceptar la invitación.');
    } finally {
      setIsSubmitting(false);
    }
  };

  if (isLoading) {
    return (
      <div className="min-h-screen flex items-center justify-center bg-background">
        <Loader2 className="w-8 h-8 animate-spin text-emerald-600" />
      </div>
    );
  }

  return (
    <div className="min-h-screen flex items-center justify-center bg-background p-4">
      <div className="w-full max-w-md bg-card p-8 shadow-2xl relative overflow-hidden">
        
        {/* Decoración */}
        <div className="absolute top-0 left-0 w-full h-1 bg-gradient-to-r from-emerald-400 to-emerald-600" />
        
        <div className="flex flex-col items-center mb-8">
          <div className="w-16 h-16 bg-emerald-500/10 flex items-center justify-center rounded-full mb-4">
            <Shield className="w-8 h-8 text-emerald-600" />
          </div>
          <h1 className="text-2xl font-black text-foreground uppercase tracking-tight">
            Activa tu cuenta
          </h1>
        </div>

        {error && !inviteData && (
          <div className="bg-rose-50 border-l-4 border-rose-500 p-4 mb-6">
            <div className="flex">
              <AlertCircle className="h-5 w-5 text-rose-500" />
              <div className="ml-3">
                <p className="text-sm text-rose-700 font-medium">{error}</p>
              </div>
            </div>
            <Button
              variant="outline"
              className="mt-4 w-full"
              onClick={() => router.push('/login')}
            >
              Ir al Login
            </Button>
          </div>
        )}

        {success && (
          <div className="text-center">
            <CheckCircle2 className="w-16 h-16 text-emerald-500 mx-auto mb-4" />
            <h2 className="text-xl font-bold text-emerald-600 mb-2">¡Cuenta Activada!</h2>
            <p className="text-slate-500 text-sm mb-6">Tu cuenta ha sido configurada exitosamente. Redirigiendo al login...</p>
          </div>
        )}

        {!success && inviteData && (
          <form onSubmit={handleSubmit} className="space-y-6">
            {error && (
              <div className="bg-rose-50 border-l-2 border-rose-500 text-rose-700 p-3 text-sm flex gap-2">
                <AlertCircle className="w-4 h-4 mt-0.5" />
                <span>{error}</span>
              </div>
            )}

            <div>
              <label className="block text-xs font-bold text-slate-500 uppercase tracking-wider mb-2">Correo</label>
              <input
                type="email"
                disabled
                value={inviteData.email}
                className="w-full bg-slate-50 border border-slate-200 text-slate-500 px-4 py-3 text-sm focus:outline-none"
              />
            </div>

            <div>
              <label className="block text-xs font-bold text-slate-500 uppercase tracking-wider mb-2">Nombre Completo</label>
              <input
                type="text"
                required
                value={fullName}
                onChange={(e) => setFullName(e.target.value)}
                className="w-full bg-transparent border-b border-emerald-600 focus:border-b-2 text-foreground px-0 py-2 text-sm focus:outline-none transition-all"
                placeholder="Ingresa tu nombre completo"
              />
            </div>

            <div>
              <label className="block text-xs font-bold text-slate-500 uppercase tracking-wider mb-2">Contraseña</label>
              <input
                type="password"
                required
                value={password}
                onChange={(e) => setPassword(e.target.value)}
                className="w-full bg-transparent border-b border-emerald-600 focus:border-b-2 text-foreground px-0 py-2 text-sm focus:outline-none transition-all"
                placeholder="Mínimo 8 caracteres"
              />
            </div>

            <div>
              <label className="block text-xs font-bold text-slate-500 uppercase tracking-wider mb-2">Confirmar Contraseña</label>
              <input
                type="password"
                required
                value={confirmPassword}
                onChange={(e) => setConfirmPassword(e.target.value)}
                className="w-full bg-transparent border-b border-emerald-600 focus:border-b-2 text-foreground px-0 py-2 text-sm focus:outline-none transition-all"
                placeholder="Repite la contraseña"
              />
            </div>

            <Button
              type="submit"
              disabled={isSubmitting}
              className="w-full py-6 bg-emerald-600 hover:bg-emerald-700 text-white font-black uppercase tracking-widest text-xs mt-4"
            >
              {isSubmitting ? <Loader2 className="w-5 h-5 animate-spin mx-auto" /> : 'Establecer Contraseña'}
            </Button>
          </form>
        )}
      </div>
    </div>
  );
}
