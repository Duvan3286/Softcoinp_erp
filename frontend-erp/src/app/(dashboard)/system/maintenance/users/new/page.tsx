'use client';

import React, { useState } from 'react';
import { useRouter } from 'next/navigation';
import { ArrowLeft, Loader2, CheckCircle, AlertTriangle, Mail } from 'lucide-react';
import systemMaintenanceService from '@/lib/system-maintenance-service';

export default function CreateUserPage() {
  const router = useRouter();
  const [fullName, setFullName] = useState('');
  const [email, setEmail] = useState('');
  const [password, setPassword] = useState('');
  const [error, setError] = useState('');
  const [fieldErrors, setFieldErrors] = useState<{ fullName?: string; email?: string; password?: string }>({});
  const [loading, setLoading] = useState(false);
  const [success, setSuccess] = useState<{ fullName: string; email: string } | null>(null);

  const validate = (): boolean => {
    const errors: { fullName?: string; email?: string; password?: string } = {};

    if (!fullName.trim()) {
      errors.fullName = 'El nombre completo es obligatorio.';
    }

    if (!email.trim()) {
      errors.email = 'El correo electrónico es obligatorio.';
    } else if (!/^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(email.trim())) {
      errors.email = 'Ingresa un correo electrónico válido.';
    }

    if (!password) {
      errors.password = 'La contraseña es obligatoria.';
    } else if (password.length < 8) {
      errors.password = 'La contraseña debe tener al menos 8 caracteres.';
    }

    setFieldErrors(errors);
    return Object.keys(errors).length === 0;
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setError('');

    if (!validate()) return;

    setLoading(true);
    try {
      const result = await systemMaintenanceService.createUser({
        fullName: fullName.trim(),
        email: email.trim().toLowerCase(),
        password,
      });
      setSuccess({ fullName: result.fullName, email: result.email });
    } catch (err: unknown) {
      if (err && typeof err === 'object' && 'response' in err) {
        const axiosErr = err as { response?: { data?: { message?: string } } };
        setError(axiosErr.response?.data?.message ?? 'Error al crear el usuario.');
      } else {
        setError('Error al crear el usuario.');
      }
    } finally {
      setLoading(false);
    }
  };

  if (success) {
    return (
      <div className="max-w-lg mx-auto mt-8">
        <div className="bg-card border border-border rounded-2xl shadow-sm overflow-hidden">
          <div className="bg-emerald-50 dark:bg-emerald-950/20 px-6 py-8 text-center">
            <div className="inline-flex p-3 rounded-full bg-emerald-100 dark:bg-emerald-900/30 mb-4">
              <CheckCircle className="w-8 h-8 text-emerald-600 dark:text-emerald-400" />
            </div>
            <h2 className="text-xl font-bold text-foreground mb-2">Usuario creado exitosamente</h2>
            <p className="text-sm text-muted-foreground">
              El usuario ya puede iniciar sesión con el correo y la contraseña que ingresaste.
            </p>
          </div>
          <div className="p-6 space-y-4">
            <div className="bg-slate-50 dark:bg-zinc-900 rounded-xl p-4 space-y-3 border border-border">
              <div className="flex items-center gap-3">
                <Mail className="w-4 h-4 text-emerald-600" />
                <div>
                  <p className="text-xs text-muted-foreground">Correo del usuario</p>
                  <p className="text-sm font-semibold text-foreground">{success.email}</p>
                </div>
              </div>
            </div>
            <div className="flex gap-3">
              <button
                onClick={() => router.push('/system/maintenance/users')}
                className="btn-primary flex-1 py-2.5 rounded-xl text-sm font-semibold"
              >
                Volver al listado
              </button>
              <button
                onClick={() => {
                  setSuccess(null);
                  setFullName('');
                  setEmail('');
                }}
                className="btn-secondary py-2.5 rounded-xl text-sm font-semibold"
              >
                Crear otro usuario
              </button>
            </div>
          </div>
        </div>
      </div>
    );
  }

  return (
    <div>
      <button
        onClick={() => router.push('/system/maintenance/users')}
        className="flex items-center gap-2 text-sm text-muted-foreground hover:text-foreground mb-4 transition-colors"
      >
        <ArrowLeft className="w-4 h-4" />
        Volver al listado
      </button>

      <div className="max-w-lg">
        <div className="bg-card border border-border rounded-2xl shadow-sm overflow-hidden">
          <div className="px-6 py-5 border-b border-border">
            <h2 className="text-lg font-bold text-foreground">Nuevo usuario administrador</h2>
            <p className="text-sm text-muted-foreground mt-1">Ingresa los datos del nuevo usuario del conjunto</p>
          </div>

          <form onSubmit={handleSubmit} className="p-6 space-y-5">
            {error && (
              <div className="p-3 bg-rose-50 border border-rose-200 rounded-lg text-rose-700 text-xs flex items-center gap-2">
                <AlertTriangle className="w-4 h-4 flex-shrink-0" />
                {error}
              </div>
            )}

            <div>
              <label className="block text-sm font-semibold text-foreground mb-2">Nombre completo</label>
              <input
                type="text"
                value={fullName}
                onChange={(e) => {
                  setFullName(e.target.value);
                  if (fieldErrors.fullName) {
                    setFieldErrors((prev) => ({ ...prev, fullName: undefined }));
                  }
                }}
                placeholder="Ej: Juan Pérez"
                className={`w-full px-4 py-2.5 bg-muted/50 border rounded-xl text-sm focus:outline-none focus:ring-2 focus:ring-emerald-500/20 focus:border-emerald-500 ${
                  fieldErrors.fullName ? 'border-rose-300' : 'border-border'
                }`}
              />
              {fieldErrors.fullName && (
                <p className="mt-1 text-xs text-rose-500">{fieldErrors.fullName}</p>
              )}
            </div>

            <div>
              <label className="block text-sm font-semibold text-foreground mb-2">Correo electrónico</label>
              <input
                type="email"
                value={email}
                onChange={(e) => {
                  setEmail(e.target.value);
                  if (fieldErrors.email) {
                    setFieldErrors((prev) => ({ ...prev, email: undefined }));
                  }
                }}
                placeholder="Ej: juan.perez@correo.com"
                className={`w-full px-4 py-2.5 bg-muted/50 border rounded-xl text-sm focus:outline-none focus:ring-2 focus:ring-emerald-500/20 focus:border-emerald-500 ${
                  fieldErrors.email ? 'border-rose-300' : 'border-border'
                }`}
              />
              {fieldErrors.email && (
                <p className="mt-1 text-xs text-rose-500">{fieldErrors.email}</p>
              )}
            </div>

            <div>
              <label className="block text-sm font-semibold text-foreground mb-2">Contraseña</label>
              <input
                type="password"
                value={password}
                onChange={(e) => {
                  setPassword(e.target.value);
                  if (fieldErrors.password) {
                    setFieldErrors((prev) => ({ ...prev, password: undefined }));
                  }
                }}
                placeholder="Mínimo 8 caracteres"
                className={`w-full px-4 py-2.5 bg-muted/50 border rounded-xl text-sm focus:outline-none focus:ring-2 focus:ring-emerald-500/20 focus:border-emerald-500 ${
                  fieldErrors.password ? 'border-rose-300' : 'border-border'
                }`}
              />
              {fieldErrors.password && (
                <p className="mt-1 text-xs text-rose-500">{fieldErrors.password}</p>
              )}
            </div>

            <div className="flex justify-end gap-3 pt-2">
              <button
                type="button"
                onClick={() => router.push('/system/maintenance/users')}
                className="btn-secondary px-4 py-2.5 rounded-xl text-sm font-semibold"
              >
                Cancelar
              </button>
              <button
                type="submit"
                className="btn-primary px-6 py-2.5 rounded-xl text-sm font-semibold flex items-center gap-2"
                disabled={loading}
              >
                {loading && <Loader2 className="w-4 h-4 animate-spin" />}
                Crear usuario
              </button>
            </div>
          </form>
        </div>
      </div>
    </div>
  );
}
