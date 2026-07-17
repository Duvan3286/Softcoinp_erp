'use client';

import React, { useState, useEffect } from 'react';
import { useRouter, useParams } from 'next/navigation';
import { ArrowLeft, Loader2, AlertTriangle, CheckCircle, MailWarning } from 'lucide-react';
import systemMaintenanceService, { UserDetail } from '@/lib/system-maintenance-service';

export default function EditUserPage() {
  const router = useRouter();
  const params = useParams();
  const userId = params.id as string;

  const [fullName, setFullName] = useState('');
  const [email, setEmail] = useState('');
  const [originalEmail, setOriginalEmail] = useState('');
  const [loading, setLoading] = useState(true);
  const [saving, setSaving] = useState(false);
  const [error, setError] = useState('');
  const [fieldErrors, setFieldErrors] = useState<{ fullName?: string; email?: string }>({});
  const [successMessage, setSuccessMessage] = useState('');

  useEffect(() => {
    const fetchUser = async () => {
      setLoading(true);
      try {
        const user = await systemMaintenanceService.getUser(userId);
        setFullName(user.fullName);
        setEmail(user.email);
        setOriginalEmail(user.email);
      } catch {
        setError('Error al cargar los datos del usuario.');
      } finally {
        setLoading(false);
      }
    };
    fetchUser();
  }, [userId]);

  const validate = (): boolean => {
    const errors: { fullName?: string; email?: string } = {};

    if (!fullName.trim()) {
      errors.fullName = 'El nombre completo es obligatorio.';
    }

    if (!email.trim()) {
      errors.email = 'El correo electrónico es obligatorio.';
    } else if (!/^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(email.trim())) {
      errors.email = 'Ingresa un correo electrónico válido.';
    }

    setFieldErrors(errors);
    return Object.keys(errors).length === 0;
  };

  const hasChanges = fullName.trim() !== '' && email.trim() !== '' &&
    (fullName.trim() !== fullName || email.trim() !== originalEmail);

  const emailChanged = email.trim().toLowerCase() !== originalEmail.toLowerCase();

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setError('');
    setSuccessMessage('');

    if (!validate()) return;

    setSaving(true);
    try {
      await systemMaintenanceService.editUser(userId, {
        fullName: fullName.trim(),
        email: email.trim().toLowerCase(),
      });
      setOriginalEmail(email.trim().toLowerCase());

      if (emailChanged) {
        setSuccessMessage('Cambios guardados. Se ha enviado un correo de verificación al nuevo email.');
      } else {
        setSuccessMessage('Cambios guardados exitosamente.');
      }
    } catch (err: unknown) {
      if (err && typeof err === 'object' && 'response' in err) {
        const axiosErr = err as { response?: { data?: { message?: string } } };
        setError(axiosErr.response?.data?.message ?? 'Error al guardar los cambios.');
      } else {
        setError('Error al guardar los cambios.');
      }
    } finally {
      setSaving(false);
    }
  };

  if (loading) {
    return (
      <div className="flex items-center justify-center py-20">
        <Loader2 className="w-6 h-6 animate-spin text-muted-foreground" />
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
            <h2 className="text-lg font-bold text-foreground">Editar usuario</h2>
            <p className="text-sm text-muted-foreground mt-1">Modifica los datos del usuario administrador</p>
          </div>

          <form onSubmit={handleSubmit} className="p-6 space-y-5">
            {error && (
              <div className="p-3 bg-rose-50 border border-rose-200 rounded-lg text-rose-700 text-xs flex items-center gap-2">
                <AlertTriangle className="w-4 h-4 flex-shrink-0" />
                {error}
              </div>
            )}

            {successMessage && (
              <div className="p-3 bg-emerald-50 border border-emerald-200 rounded-lg text-emerald-700 text-xs flex items-center gap-2">
                <CheckCircle className="w-4 h-4 flex-shrink-0" />
                {successMessage}
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
                className={`w-full px-4 py-2.5 bg-muted/50 border rounded-xl text-sm focus:outline-none focus:ring-2 focus:ring-emerald-500/20 focus:border-emerald-500 ${
                  fieldErrors.email ? 'border-rose-300' : 'border-border'
                }`}
              />
              {fieldErrors.email && (
                <p className="mt-1 text-xs text-rose-500">{fieldErrors.email}</p>
              )}
            </div>

            {emailChanged && (
              <div className="p-3 bg-blue-50 dark:bg-blue-950/20 border border-blue-100 dark:border-blue-900 rounded-xl text-xs text-blue-700 dark:text-blue-300 flex items-start gap-2">
                <MailWarning className="w-4 h-4 flex-shrink-0 mt-0.5" />
                <span>
                  <strong>Importante:</strong> Al cambiar el correo electrónico, se enviará un enlace de verificación al nuevo correo. El cambio no será efectivo hasta que el usuario lo verifique. Mientras tanto, el usuario seguirá accediendo con su correo anterior.
                </span>
              </div>
            )}

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
                disabled={saving}
              >
                {saving && <Loader2 className="w-4 h-4 animate-spin" />}
                Guardar cambios
              </button>
            </div>
          </form>
        </div>
      </div>
    </div>
  );
}
