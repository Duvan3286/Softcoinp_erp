'use client';

import React, { useState, useEffect } from 'react';
import { useRouter, useParams } from 'next/navigation';
import { Loader2, ArrowLeft, Save, AlertTriangle, Send, Receipt } from 'lucide-react';
import { Button } from '@/components/ui/Button';
import { Card, CardContent } from '@/components/ui/Card';
import supplierService, { CreateInvoiceRequest, ContractDetail } from '@/lib/supplier-service';
import budgetService, { ExpenseExecutionItem } from '@/lib/budget-service';

const paymentMethods = [
  { value: 'Cash', label: 'Efectivo' },
  { value: 'BankTransfer', label: 'Transferencia' },
  { value: 'Check', label: 'Cheque' },
  { value: 'CreditCard', label: 'Tarjeta de Crédito' },
];

export default function NewInvoicePage() {
  const router = useRouter();
  const params = useParams();
  const contractId = params.id as string;

  const [contract, setContract] = useState<ContractDetail | null>(null);
  const [budgetItems, setBudgetItems] = useState<ExpenseExecutionItem[]>([]);
  const [loadingData, setLoadingData] = useState(true);
  const [loadError, setLoadError] = useState('');
  const [submitting, setSubmitting] = useState(false);
  const [error, setError] = useState('');
  const [createdId, setCreatedId] = useState<string | null>(null);

  const [invoiceNumber, setInvoiceNumber] = useState('');
  const [invoiceDate, setInvoiceDate] = useState('');
  const [dueDate, setDueDate] = useState('');
  const [totalAmount, setTotalAmount] = useState('');
  const [amountPaid, setAmountPaid] = useState('');
  const [paymentDate, setPaymentDate] = useState('');
  const [paymentMethod, setPaymentMethod] = useState('BankTransfer');
  const [paymentReferenceNumber, setPaymentReferenceNumber] = useState('');
  const [budgetItemId, setBudgetItemId] = useState('');

  useEffect(() => {
    const fetchData = async () => {
      setLoadingData(true);
      try {
        const contractData = await supplierService.getContractById(contractId);
        setContract(contractData);

        const currentYear = new Date(contractData.startDate).getFullYear();
        const execution = await budgetService.getBudgetExecution(currentYear);
        setBudgetItems(execution.expenseItems);
      } catch {
        setLoadError('Error al cargar el contrato o el presupuesto activo.');
      } finally {
        setLoadingData(false);
      }
    };
    fetchData();
  }, [contractId]);

  const selectedBudgetItem = budgetItems.find((item) => item.id === budgetItemId);

  const formatCurrency = (value: number) => new Intl.NumberFormat('es-CO', { style: 'currency', currency: 'COP', maximumFractionDigits: 0 }).format(value);

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setError('');

    if (!contract) {
      return;
    }
    if (!invoiceNumber.trim()) {
      setError('El número de factura es requerido.');
      return;
    }
    if (!invoiceDate) {
      setError('La fecha de la factura es requerida.');
      return;
    }
    if (!dueDate) {
      setError('La fecha de vencimiento es requerida.');
      return;
    }
    if (!totalAmount || parseFloat(totalAmount) <= 0) {
      setError('El valor total debe ser mayor a 0.');
      return;
    }

    const parsedAmountPaid = amountPaid ? parseFloat(amountPaid) : 0;
    if (parsedAmountPaid > parseFloat(totalAmount)) {
      setError('El valor pagado no puede superar el valor total de la factura.');
      return;
    }

    if (selectedBudgetItem && parseFloat(totalAmount) > selectedBudgetItem.availableValue) {
      setError(`El valor de la factura supera el saldo disponible del rubro (${formatCurrency(selectedBudgetItem.availableValue)}).`);
      return;
    }

    setSubmitting(true);
    try {
      const request: CreateInvoiceRequest = {
        providerId: contract.providerId,
        contractId: contract.id,
        invoiceNumber: invoiceNumber.trim(),
        invoiceDate,
        dueDate,
        totalAmount: parseFloat(totalAmount),
        amountPaid: parsedAmountPaid,
        budgetItemId: budgetItemId || undefined,
      };

      if (parsedAmountPaid > 0) {
        request.paymentDate = paymentDate || undefined;
        request.paymentMethod = paymentMethod;
        request.paymentReferenceNumber = paymentReferenceNumber || undefined;
      }

      const result = await supplierService.createInvoice(request);
      setCreatedId(result.id);
    } catch (err: any) {
      setError(err?.response?.data?.error || 'Error al registrar la factura.');
    } finally {
      setSubmitting(false);
    }
  };

  if (loadingData) {
    return <div className="flex justify-center py-20"><Loader2 className="w-8 h-8 animate-spin text-emerald-600" /></div>;
  }

  if (loadError || !contract) {
    return (
      <div className="space-y-6 max-w-2xl mx-auto">
        <button onClick={() => router.push(`/contracts/${contractId}`)} className="inline-flex items-center gap-2 text-sm text-muted-foreground hover:text-foreground">
          <ArrowLeft className="w-4 h-4" /> Volver
        </button>
        <div className="flex flex-col items-center gap-3 text-rose-600 py-12">
          <AlertTriangle className="w-10 h-10" />
          <p className="font-semibold">{loadError || 'Contrato no encontrado.'}</p>
        </div>
      </div>
    );
  }

  if (createdId) {
    return (
      <div className="space-y-6 max-w-2xl mx-auto">
        <button onClick={() => router.push(`/contracts/${contractId}`)} className="inline-flex items-center gap-2 text-sm text-muted-foreground hover:text-foreground transition-colors">
          <ArrowLeft className="w-4 h-4" /> Volver al Contrato
        </button>
        <Card>
          <CardContent className="p-6 text-center">
            <div className="w-16 h-16 bg-emerald-50 rounded-full flex items-center justify-center mx-auto mb-4">
              <Send className="w-8 h-8 text-emerald-600" />
            </div>
            <h2 className="text-xl font-bold text-foreground">Factura Registrada Exitosamente</h2>
            <div className="mt-6 flex justify-center gap-3">
              <Button onClick={() => router.push(`/contracts/${contractId}`)}>Ver Contrato</Button>
            </div>
          </CardContent>
        </Card>
      </div>
    );
  }

  return (
    <div className="space-y-6 max-w-3xl mx-auto">
      <button onClick={() => router.push(`/contracts/${contractId}`)} className="inline-flex items-center gap-2 text-sm text-muted-foreground hover:text-foreground transition-colors">
        <ArrowLeft className="w-4 h-4" /> Volver al Contrato
      </button>

      <div>
        <h1 className="text-2xl font-bold text-foreground tracking-tight">Nueva Factura</h1>
        <p className="text-sm text-muted-foreground mt-1">
          Contrato {contract.contractNumber} — {contract.providerBusinessName}
        </p>
      </div>

      <Card>
        <CardContent className="p-6">
          <form onSubmit={handleSubmit} className="space-y-6">
            <div>
              <h3 className="text-sm font-bold text-foreground mb-4 flex items-center gap-2">
                <Receipt className="w-4 h-4 text-emerald-600" /> Datos de la Factura
              </h3>
              <div className="grid grid-cols-1 md:grid-cols-2 gap-5">
                <div>
                  <label className="block text-xs font-bold text-muted-foreground uppercase tracking-widest mb-1.5">Nro. Factura *</label>
                  <input type="text" value={invoiceNumber} onChange={(e) => setInvoiceNumber(e.target.value.slice(0, 100))}
                    maxLength={100} required
                    className="w-full bg-transparent border-b border-emerald-600/30 focus:border-emerald-600 text-sm font-medium py-2 outline-none" />
                </div>
                <div>
                  <label className="block text-xs font-bold text-muted-foreground uppercase tracking-widest mb-1.5">Valor Total (COP) *</label>
                  <input type="number" value={totalAmount} onChange={(e) => setTotalAmount(e.target.value)} min="0" step="0.01" required
                    className="w-full bg-transparent border-b border-emerald-600/30 focus:border-emerald-600 text-sm font-medium py-2 outline-none" />
                </div>
                <div>
                  <label className="block text-xs font-bold text-muted-foreground uppercase tracking-widest mb-1.5">Fecha de Factura *</label>
                  <input type="date" value={invoiceDate} onChange={(e) => setInvoiceDate(e.target.value)} required
                    className="w-full bg-transparent border-b border-emerald-600/30 focus:border-emerald-600 text-sm font-medium py-2 outline-none" />
                </div>
                <div>
                  <label className="block text-xs font-bold text-muted-foreground uppercase tracking-widest mb-1.5">Fecha de Vencimiento *</label>
                  <input type="date" value={dueDate} onChange={(e) => setDueDate(e.target.value)} required
                    className="w-full bg-transparent border-b border-emerald-600/30 focus:border-emerald-600 text-sm font-medium py-2 outline-none" />
                </div>
              </div>
            </div>

            <div className="border-t border-border pt-6">
              <h3 className="text-sm font-bold text-foreground mb-4">Rubro Presupuestal</h3>
              <select value={budgetItemId} onChange={(e) => setBudgetItemId(e.target.value)}
                className="w-full bg-transparent border-b border-emerald-600/30 focus:border-emerald-600 text-sm font-medium py-2 outline-none">
                <option value="">Sin imputación presupuestal</option>
                {budgetItems.map((item) => (
                  <option key={item.id} value={item.id}>{item.name} — disponible {formatCurrency(item.availableValue)}</option>
                ))}
              </select>
              {selectedBudgetItem && (
                <div className="mt-3 grid grid-cols-3 gap-3 bg-muted/30 rounded-lg p-3 text-sm">
                  <div><span className="text-muted-foreground text-xs">Presupuestado</span><p className="font-bold">{formatCurrency(selectedBudgetItem.annualValue)}</p></div>
                  <div><span className="text-muted-foreground text-xs">Ejecutado</span><p className="font-bold text-orange-600">{formatCurrency(selectedBudgetItem.executedValue)}</p></div>
                  <div><span className="text-muted-foreground text-xs">Disponible</span><p className="font-bold text-emerald-600">{formatCurrency(selectedBudgetItem.availableValue)}</p></div>
                </div>
              )}
            </div>

            <div className="border-t border-border pt-6">
              <h3 className="text-sm font-bold text-foreground mb-4">Pago (opcional)</h3>
              <div className="grid grid-cols-1 md:grid-cols-2 gap-5">
                <div>
                  <label className="block text-xs font-bold text-muted-foreground uppercase tracking-widest mb-1.5">Valor Pagado (COP)</label>
                  <input type="number" value={amountPaid} onChange={(e) => setAmountPaid(e.target.value)} min="0" step="0.01"
                    className="w-full bg-transparent border-b border-emerald-600/30 focus:border-emerald-600 text-sm font-medium py-2 outline-none" />
                </div>
                <div>
                  <label className="block text-xs font-bold text-muted-foreground uppercase tracking-widest mb-1.5">Fecha de Pago</label>
                  <input type="date" value={paymentDate} onChange={(e) => setPaymentDate(e.target.value)}
                    className="w-full bg-transparent border-b border-emerald-600/30 focus:border-emerald-600 text-sm font-medium py-2 outline-none" />
                </div>
                <div>
                  <label className="block text-xs font-bold text-muted-foreground uppercase tracking-widest mb-1.5">Medio de Pago</label>
                  <select value={paymentMethod} onChange={(e) => setPaymentMethod(e.target.value)}
                    className="w-full bg-transparent border-b border-emerald-600/30 focus:border-emerald-600 text-sm font-medium py-2 outline-none">
                    {paymentMethods.map((m) => <option key={m.value} value={m.value}>{m.label}</option>)}
                  </select>
                </div>
                <div>
                  <label className="block text-xs font-bold text-muted-foreground uppercase tracking-widest mb-1.5">Comprobante / Referencia</label>
                  <input type="text" value={paymentReferenceNumber} onChange={(e) => setPaymentReferenceNumber(e.target.value.slice(0, 100))}
                    maxLength={100}
                    className="w-full bg-transparent border-b border-emerald-600/30 focus:border-emerald-600 text-sm font-medium py-2 outline-none" />
                </div>
              </div>
            </div>

            {error && (
              <div className="p-3 bg-rose-50 border border-rose-200 rounded-lg text-rose-700 text-xs flex items-center gap-2">
                <AlertTriangle className="w-4 h-4 shrink-0" /> {error}
              </div>
            )}

            <div className="flex justify-between items-center pt-4 border-t border-border">
              <Button type="button" variant="ghost" onClick={() => router.push(`/contracts/${contractId}`)}>Cancelar</Button>
              <Button type="submit" disabled={submitting}>
                {submitting ? <Loader2 className="w-4 h-4 animate-spin mr-2" /> : <Save className="w-4 h-4 mr-2" />}
                Registrar Factura
              </Button>
            </div>
          </form>
        </CardContent>
      </Card>
    </div>
  );
}
