'use client';

import React, { useState, useEffect } from 'react';
import { useAuth } from '@/context/AuthContext';
import accountingService, { AccountingAccount } from '@/lib/accounting-service';
import { 
  BookOpen, 
  Plus, 
  Search, 
  Check, 
  X,
  AlertCircle,
  ToggleLeft,
  ToggleRight,
  FolderPlus,
  FolderOpen,
  Edit2,
  Trash2
} from 'lucide-react';
import { Button } from '@/components/ui/Button';

export default function AccountingAccountsPage() {
  const { user } = useAuth();
  const [accounts, setAccounts] = useState<AccountingAccount[]>([]);
  const [filteredAccounts, setFilteredAccounts] = useState<AccountingAccount[]>([]);
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState('');
  const [success, setSuccess] = useState('');

  // Filter states
  const [searchTerm, setSearchTerm] = useState('');
  const [categoryFilter, setCategoryFilter] = useState('ALL');
  const [natureFilter, setNatureFilter] = useState('ALL');
  const [statusFilter, setStatusFilter] = useState('ALL');

  // Modal / Form states
  const [isCreateOpen, setIsCreateOpen] = useState(false);
  const [parentAccount, setParentAccount] = useState<AccountingAccount | null>(null);
  const [subCode, setSubCode] = useState('');
  const [newAccountName, setNewAccountName] = useState('');
  const [newIsGroup, setNewIsGroup] = useState(false);

  // Edit states
  const [editingId, setEditingId] = useState<string | null>(null);
  const [editingName, setEditingName] = useState('');
  const [editingIsActive, setEditingIsActive] = useState(true);

  // Permissions
  const canEdit = user?.role === 'SuperAdmin' || user?.role === 'Admin' || user?.role === 'Accountant';

  useEffect(() => {
    fetchAccounts();
  }, []);

  useEffect(() => {
    filterData();
  }, [accounts, searchTerm, categoryFilter, natureFilter, statusFilter]);

  const fetchAccounts = async () => {
    try {
      setIsLoading(true);
      setError('');
      const data = await accountingService.getAccounts();
      setAccounts(data);
    } catch (err: any) {
      console.error(err);
      setError('Error al cargar el plan de cuentas contables.');
    } finally {
      setIsLoading(false);
    }
  };

  const filterData = () => {
    let result = [...accounts];

    // Search term (code or name)
    if (searchTerm) {
      const term = searchTerm.toLowerCase();
      result = result.filter(
        (a) => a.code.toLowerCase().includes(term) || a.name.toLowerCase().includes(term)
      );
    }

    // Category Filter
    if (categoryFilter !== 'ALL') {
      result = result.filter((a) => a.category === categoryFilter);
    }

    // Nature Filter
    if (natureFilter !== 'ALL') {
      result = result.filter((a) => a.nature === natureFilter);
    }

    // Status Filter
    if (statusFilter !== 'ALL') {
      const activeBool = statusFilter === 'ACTIVE';
      result = result.filter((a) => a.isActive === activeBool);
    }

    setFilteredAccounts(result);
  };

  const handleOpenCreate = (parent: AccountingAccount) => {
    if (!canEdit) return;
    setParentAccount(parent);
    setSubCode('');
    setNewAccountName('');
    setNewIsGroup(false);
    setIsCreateOpen(true);
    setError('');
  };

  const handleCreateAuxiliary = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!canEdit || !parentAccount) return;

    setError('');
    setSuccess('');

    if (!subCode || subCode.length !== 2 || !/^\d+$/.test(subCode)) {
      setError('El subcódigo debe tener exactamente 2 dígitos numéricos.');
      return;
    }

    if (!newAccountName.trim()) {
      setError('El nombre de la cuenta es obligatorio.');
      return;
    }

    try {
      const titleCaseName = newAccountName.trim()
        .toLowerCase()
        .replace(/(?:^|\s)\S/g, (a) => a.toUpperCase());

      await accountingService.createAuxiliaryAccount({
        parentCode: parentAccount.code,
        subCode,
        name: titleCaseName,
        isGroup: newIsGroup
      });

      setSuccess(`Cuenta auxiliar ${parentAccount.code}${subCode} creada exitosamente.`);
      setIsCreateOpen(false);
      fetchAccounts();
    } catch (err: any) {
      console.error(err);
      let errMsg = 'Ocurrió un error al registrar la cuenta auxiliar.';
      if (err.response && err.response.data) {
        errMsg = err.response.data;
      }
      setError(errMsg);
    }
  };

  const startEditing = (account: AccountingAccount) => {
    if (!canEdit || account.isOfficialStandard) return;
    setEditingId(account.id);
    setEditingName(account.name);
    setEditingIsActive(account.isActive);
    setError('');
  };

  const cancelEditing = () => {
    setEditingId(null);
  };

  const handleUpdateAccount = async (id: string) => {
    if (!canEdit) return;
    setError('');
    setSuccess('');

    if (!editingName.trim()) {
      setError('El nombre de la cuenta no puede estar vacío.');
      return;
    }

    try {
      const titleCaseName = editingName.trim()
        .toLowerCase()
        .replace(/(?:^|\s)\S/g, (a) => a.toUpperCase());

      await accountingService.updateAccount(id, {
        name: titleCaseName,
        isActive: editingIsActive
      });

      setSuccess('Cuenta actualizada correctamente.');
      setEditingId(null);
      fetchAccounts();
    } catch (err: any) {
      console.error(err);
      let errMsg = 'Ocurrió un error al actualizar la cuenta.';
      if (err.response && err.response.data) {
        errMsg = err.response.data;
      }
      setError(errMsg);
    }
  };

  const handleDeleteAccount = async (account: AccountingAccount) => {
    if (!canEdit) return;
    
    if (account.isOfficialStandard) {
      alert('Las cuentas oficiales del estándar de la Resolución 029 no pueden ser eliminadas.');
      return;
    }

    if (!confirm(`¿Está seguro de eliminar la cuenta ${account.code} - ${account.name}? Esta acción no se puede deshacer.`)) {
      return;
    }

    setError('');
    setSuccess('');

    try {
      await accountingService.deleteAccount(account.id);
      setSuccess(`Cuenta ${account.code} eliminada con éxito.`);
      fetchAccounts();
    } catch (err: any) {
      console.error(err);
      let errMsg = 'No se pudo eliminar la cuenta. Valida que no registre movimientos contables o presupuestales.';
      if (err.response && err.response.data) {
        errMsg = err.response.data;
      }
      setError(errMsg);
    }
  };

  // Helper for indentation class based on account code length
  const getIndentationClass = (code: string) => {
    const len = code.length;
    if (len === 1) {
      return 'pl-2 font-black text-gray-900 border-l-4 border-emerald-600 bg-emerald-50/40 dark:bg-emerald-950/10 dark:text-emerald-300';
    }
    if (len === 2) {
      return 'pl-6 font-bold text-gray-800 bg-slate-50/50 dark:bg-zinc-900/20 dark:text-zinc-300';
    }
    if (len === 4) {
      return 'pl-10 font-semibold text-gray-700 dark:text-zinc-400';
    }
    if (len === 6) {
      return 'pl-16 text-gray-600 dark:text-zinc-400';
    }
    return 'pl-24 text-gray-500 italic dark:text-zinc-500';
  };

  const getCategoryBadgeColor = (cat: string) => {
    const normalized = cat.toLowerCase();
    if (normalized === 'asset') {
      return 'bg-blue-100 text-blue-800 dark:bg-blue-950/30 dark:text-blue-400';
    }
    if (normalized === 'liability') {
      return 'bg-amber-100 text-amber-800 dark:bg-amber-950/30 dark:text-amber-400';
    }
    if (normalized === 'equity') {
      return 'bg-purple-100 text-purple-800 dark:bg-purple-950/30 dark:text-purple-400';
    }
    if (normalized === 'income') {
      return 'bg-emerald-100 text-emerald-800 dark:bg-emerald-950/30 dark:text-emerald-400';
    }
    if (normalized === 'expense') {
      return 'bg-rose-100 text-rose-800 dark:bg-rose-950/30 dark:text-rose-400';
    }
    return 'bg-slate-100 text-slate-800 dark:bg-slate-950/30 dark:text-slate-400';
  };

  const translateCategory = (cat: string) => {
    const normalized = cat.toLowerCase();
    if (normalized === 'asset') {
      return 'Activo';
    }
    if (normalized === 'liability') {
      return 'Pasivo';
    }
    if (normalized === 'equity') {
      return 'Fondo Social / Pat.';
    }
    if (normalized === 'income') {
      return 'Ingreso';
    }
    if (normalized === 'expense') {
      return 'Gasto';
    }
    return cat;
  };

  const translateNature = (nat: string) => {
    if (nat.toLowerCase() === 'debit') {
      return 'Débito';
    }
    if (nat.toLowerCase() === 'credit') {
      return 'Crédito';
    }
    return nat;
  };

  // Helper functions to completely avoid ternary operators in conditional rendering and class bindings
  function getRowFontWeight(code: string): string {
    if (code.length <= 2) {
      return 'font-bold';
    }
    return '';
  }

  function renderFolderIcon(isGroup: boolean) {
    if (isGroup) {
      return <FolderOpen className="w-4 h-4 text-emerald-600 dark:text-emerald-500 flex-shrink-0" />;
    }
    return <BookOpen className="w-4 h-4 text-slate-400 flex-shrink-0" />;
  }

  function renderAccountNameCell(isEditing: boolean, account: AccountingAccount, val: string, onChangeFn: (v: string) => void) {
    if (isEditing) {
      return (
        <input
          type="text"
          value={val}
          onChange={(e) => onChangeFn(e.target.value)}
          className="input-standard max-w-xs"
        />
      );
    }
    return <span className="dark:text-zinc-300">{account.name}</span>;
  }

  function renderActiveStatusCell(isEditing: boolean, account: AccountingAccount, editingIsActiveVal: boolean, toggleFn: () => void) {
    if (isEditing) {
      let icon = <ToggleLeft className="w-8 h-8 text-slate-400" />;
      if (editingIsActiveVal) {
        icon = <ToggleRight className="w-8 h-8 text-emerald-600" />;
      }
      return (
        <button
          type="button"
          onClick={toggleFn}
          className="text-emerald-600 focus:outline-none"
        >
          {icon}
        </button>
      );
    }

    let spanClass = 'bg-rose-50 text-rose-700 border border-rose-200 dark:bg-rose-950/20 dark:text-rose-400 dark:border-rose-900/50';
    let text = 'Inactiva';
    if (account.isActive) {
      spanClass = 'bg-emerald-50 text-emerald-700 border border-emerald-200 dark:bg-emerald-950/20 dark:text-emerald-400 dark:border-emerald-900/50';
      text = 'Activa';
    }

    return (
      <span className={`px-2 py-0.5 text-xs font-bold rounded-md ${spanClass}`}>
        {text}
      </span>
    );
  }

  function renderActionsCell(
    isEditing: boolean,
    account: AccountingAccount,
    isParentEligible: boolean,
    onUpdate: (id: string) => void,
    onCancel: () => void,
    onCreateChild: (acc: AccountingAccount) => void,
    onStartEdit: (acc: AccountingAccount) => void,
    onDelete: (acc: AccountingAccount) => void
  ) {
    if (isEditing) {
      return (
        <>
          <Button
            variant="success"
            onClick={() => onUpdate(account.id)}
            className="p-2.5 rounded-lg"
            title="Guardar cambios"
          >
            <Check className="w-4 h-4" />
          </Button>
          <Button
            variant="ghost"
            onClick={onCancel}
            className="p-2.5 rounded-lg"
            title="Cancelar"
          >
            <X className="w-4 h-4" />
          </Button>
        </>
      );
    }

    const buttons = [];

    if (canEdit && isParentEligible && account.isGroup) {
      buttons.push(
        <Button
          key="create"
          variant="ghost"
          onClick={() => onCreateChild(account)}
          className="text-emerald-600 dark:text-emerald-400 p-2 border border-emerald-100 hover:bg-emerald-50 dark:border-emerald-900/50 dark:hover:bg-emerald-950/20"
          title="Crear cuenta auxiliar hija"
        >
          <FolderPlus className="w-4 h-4" />
        </Button>
      );
    }

    if (canEdit && !account.isOfficialStandard) {
      buttons.push(
        <Button
          key="edit"
          variant="secondary"
          onClick={() => onStartEdit(account)}
          className="p-2"
          title="Editar nombre/estado"
        >
          <Edit2 className="w-4 h-4" />
        </Button>
      );
      buttons.push(
        <Button
          key="delete"
          variant="danger"
          onClick={() => onDelete(account)}
          className="p-2"
          title="Eliminar cuenta"
        >
          <Trash2 className="w-4 h-4" />
        </Button>
      );
    }

    return <div className="flex items-center justify-end gap-2">{buttons}</div>;
  }

  function renderTableContent() {
    if (isLoading) {
      return (
        <tr>
          <td colSpan={6} className="py-12 text-center text-slate-400 dark:text-zinc-500">
            Cargando catálogo contable...
          </td>
        </tr>
      );
    }

    if (filteredAccounts.length === 0) {
      return (
        <tr>
          <td colSpan={6} className="py-12 text-center text-slate-400 dark:text-zinc-500">
            No se encontraron cuentas contables con los filtros seleccionados.
          </td>
        </tr>
      );
    }

    return filteredAccounts.map((account) => {
      const isEditing = editingId === account.id;
      const isParentEligible = account.code.length === 4 || account.code.length === 6;

      return (
        <tr 
          key={account.id}
          className={`hover:bg-slate-50/50 dark:hover:bg-zinc-900/30 transition-colors ${getRowFontWeight(account.code)}`}
        >
          {/* Code column with indentation styling */}
          <td className={`py-4 px-6 ${getIndentationClass(account.code)}`}>
            <div className="flex items-center gap-2">
              {renderFolderIcon(account.isGroup)}
              <span className="font-mono tracking-wider">{account.code}</span>
            </div>
          </td>

          {/* Name column / edit form */}
          <td className="py-4 px-6">
            {renderAccountNameCell(isEditing, account, editingName, setEditingName)}
            {account.isOfficialStandard && (
              <span className="ml-2 text-[10px] bg-slate-100 text-slate-500 dark:bg-zinc-800 dark:text-zinc-400 px-1.5 py-0.5 rounded font-medium">
                Oficial
              </span>
            )}
          </td>

          {/* Category */}
          <td className="py-4 px-6">
            <span className={`px-2.5 py-1 text-xs font-semibold rounded-full ${getCategoryBadgeColor(account.category)}`}>
              {translateCategory(account.category)}
            </span>
          </td>

          {/* Nature */}
          <td className="py-4 px-6 text-center text-sm font-medium text-slate-600 dark:text-zinc-400">
            {translateNature(account.nature)}
          </td>

          {/* Active status */}
          <td className="py-4 px-6 text-center">
            {renderActiveStatusCell(isEditing, account, editingIsActive, () => setEditingIsActive(!editingIsActive))}
          </td>

          {/* Actions */}
          <td className="py-4 px-6 text-right">
            {renderActionsCell(
              isEditing,
              account,
              isParentEligible,
              handleUpdateAccount,
              cancelEditing,
              handleOpenCreate,
              startEditing,
              handleDeleteAccount
            )}
          </td>
        </tr>
      );
    });
  }

  function renderCreateModal() {
    if (!isCreateOpen || !parentAccount) {
      return null;
    }

    const paddingValue = (parentAccount.code.length * 9) + 20;

    return (
      <div className="fixed inset-0 bg-black/60 backdrop-blur-sm z-[150] flex items-center justify-center p-4">
        <div className="bg-card text-card-foreground w-full max-w-lg rounded-xl border border-border shadow-lg overflow-hidden animate-in zoom-in-95 duration-200">
          <div className="p-6 border-b border-border flex items-center justify-between">
            <h3 className="font-bold text-lg text-gray-900 dark:text-white">Agregar Cuenta Auxiliar</h3>
            <button 
              onClick={() => setIsCreateOpen(false)}
              className="text-slate-400 hover:text-slate-600 dark:text-zinc-500 dark:hover:text-zinc-300"
            >
              <X className="w-5 h-5" />
            </button>
          </div>

          <form onSubmit={handleCreateAuxiliary} className="p-6 space-y-4">
            <div>
              <label className="block text-xs font-bold text-slate-400 dark:text-zinc-500 uppercase tracking-widest mb-1">
                Cuenta Padre
              </label>
              <div className="p-3 bg-slate-50 dark:bg-zinc-900 rounded-lg border border-border flex items-center gap-2">
                <span className="font-mono font-bold text-emerald-700 dark:text-emerald-400">
                  {parentAccount.code}
                </span>
                <span className="text-gray-700 dark:text-zinc-300 font-semibold text-sm">
                  - {parentAccount.name}
                </span>
              </div>
            </div>

            <div className="grid grid-cols-2 gap-4">
              <div>
                <label className="block text-xs font-bold text-slate-400 dark:text-zinc-500 uppercase tracking-widest mb-1">
                  Subcódigo (2 dígitos)
                </label>
                <div className="relative">
                  <span className="absolute left-3 top-1/2 -translate-y-1/2 font-mono font-bold text-emerald-700 dark:text-emerald-400">
                    {parentAccount.code}
                  </span>
                  <input
                    type="text"
                    maxLength={2}
                    placeholder="01"
                    value={subCode}
                    onChange={(e) => setSubCode(e.target.value.replace(/\D/g, ''))}
                    className="input-standard font-mono"
                    style={{ paddingLeft: `${paddingValue}px` }}
                    required
                  />
                </div>
                <p className="text-[10px] text-slate-400 mt-1 dark:text-zinc-500">
                  Ejemplo: &apos;01&apos;, &apos;02&apos; para crear {parentAccount.code}01.
                </p>
              </div>

              <div className="flex flex-col justify-end pb-1.5">
                <label className="flex items-center gap-2 cursor-pointer select-none text-sm text-gray-700 dark:text-zinc-300">
                  <input
                    type="checkbox"
                    checked={newIsGroup}
                    onChange={(e) => setNewIsGroup(e.target.checked)}
                    className="w-4 h-4 rounded border-gray-300 text-emerald-600 focus:ring-emerald-500"
                  />
                  <span>¿Es cuenta agrupadora?</span>
                </label>
                <p className="text-[10px] text-slate-400 mt-1 dark:text-zinc-500">
                  Permite agregar subcuentas adicionales debajo.
                </p>
              </div>
            </div>

            <div>
              <label className="block text-xs font-bold text-slate-400 dark:text-zinc-500 uppercase tracking-widest mb-1">
                Nombre de la Cuenta Auxiliar
              </label>
              <input
                type="text"
                placeholder="Ej. Banco de Bogotá Ahorros"
                value={newAccountName}
                onChange={(e) => setNewAccountName(e.target.value)}
                className="input-standard"
                required
              />
            </div>

            <div className="p-3 bg-slate-50 dark:bg-zinc-900 rounded-lg border border-border text-xs text-slate-500 dark:text-zinc-400 space-y-1">
              <p><strong>Naturaleza heredada:</strong> {translateNature(parentAccount.nature)} ({parentAccount.nature})</p>
              <p><strong>Categoría heredada:</strong> {translateCategory(parentAccount.category)} ({parentAccount.category})</p>
            </div>

            <div className="pt-4 flex justify-end gap-3 border-t border-border">
              <Button
                type="button"
                variant="ghost"
                onClick={() => setIsCreateOpen(false)}
              >
                Cancelar
              </Button>
              <Button
                type="submit"
                variant="primary"
              >
                Crear Cuenta
              </Button>
            </div>
          </form>
        </div>
      </div>
    );
  }

  return (
    <div className="space-y-6">
      {/* HEADER */}
      <div className="flex flex-col sm:flex-row sm:items-center sm:justify-between gap-4">
        <div>
          <h1 className="text-2xl font-bold text-gray-900 tracking-tight dark:text-white">Plan de Cuentas</h1>
          <p className="text-sm text-gray-500 mt-1 dark:text-zinc-400">
            Estructura contable oficial para Propiedad Horizontal (Resolución 029 de 2019). Agrega cuentas auxiliares personalizadas.
          </p>
        </div>
      </div>

      {/* STATUS MESSAGES */}
      {error && (
        <div className="flex items-center gap-3 p-4 bg-rose-50 dark:bg-rose-950/20 text-rose-700 dark:text-rose-400 rounded-xl border border-rose-100 dark:border-rose-900/50">
          <AlertCircle className="w-5 h-5 flex-shrink-0" />
          <p className="text-sm font-semibold">{error}</p>
        </div>
      )}

      {success && (
        <div className="flex items-center gap-3 p-4 bg-emerald-50 dark:bg-emerald-950/20 text-emerald-700 dark:text-emerald-400 rounded-xl border border-emerald-100 dark:border-emerald-900/50">
          <Check className="w-5 h-5 flex-shrink-0" />
          <p className="text-sm font-semibold">{success}</p>
        </div>
      )}

      {/* FILTER BAR */}
      <div className="card-standard p-6 bg-card text-card-foreground">
        <div className="grid grid-cols-1 md:grid-cols-4 gap-4">
          {/* SEARCH */}
          <div className="relative">
            <Search className="absolute left-3 top-1/2 -translate-y-1/2 w-4 h-4 text-slate-400" />
            <input
              type="text"
              placeholder="Buscar por código o nombre..."
              value={searchTerm}
              onChange={(e) => setSearchTerm(e.target.value)}
              className="input-standard pl-10"
            />
          </div>

          {/* CATEGORY FILTER */}
          <div>
            <select
              value={categoryFilter}
              onChange={(e) => setCategoryFilter(e.target.value)}
              className="input-standard"
            >
              <option value="ALL">Todas las Categorías</option>
              <option value="Asset">Activo (Cuentas 1)</option>
              <option value="Liability">Pasivo (Cuentas 2)</option>
              <option value="Equity">Fondo Social / Patrimonio (Cuentas 3)</option>
              <option value="Income">Ingresos (Cuentas 4)</option>
              <option value="Expense">Gastos (Cuentas 5)</option>
            </select>
          </div>

          {/* NATURE FILTER */}
          <div>
            <select
              value={natureFilter}
              onChange={(e) => setNatureFilter(e.target.value)}
              className="input-standard"
            >
              <option value="ALL">Todas las Naturalezas</option>
              <option value="Debit">Débito</option>
              <option value="Credit">Crédito</option>
            </select>
          </div>

          {/* STATUS FILTER */}
          <div>
            <select
              value={statusFilter}
              onChange={(e) => setStatusFilter(e.target.value)}
              className="input-standard"
            >
              <option value="ALL">Todos los Estados</option>
              <option value="ACTIVE">Activas</option>
              <option value="INACTIVE">Inactivas</option>
            </select>
          </div>
        </div>
      </div>

      {/* MAIN ACCOUNTS TABLE */}
      <div className="card-standard bg-card text-card-foreground">
        <div className="overflow-x-auto">
          <table className="w-full border-collapse">
            <thead>
              <tr className="border-b border-border bg-slate-50 dark:bg-zinc-900/50 text-left text-xs font-bold text-slate-400 dark:text-zinc-500 uppercase tracking-widest">
                <th className="py-4 px-6">Código Contable</th>
                <th className="py-4 px-6">Nombre de la Cuenta</th>
                <th className="py-4 px-6">Clasificación / Grupo</th>
                <th className="py-4 px-6 text-center">Naturaleza</th>
                <th className="py-4 px-6 text-center">Estado</th>
                <th className="py-4 px-6 text-right">Acciones</th>
              </tr>
            </thead>
            <tbody className="divide-y divide-border">
              {renderTableContent()}
            </tbody>
          </table>
        </div>
      </div>

      {/* MODAL TO CREATE AUXILIARY ACCOUNT */}
      {renderCreateModal()}
    </div>
  );
}
