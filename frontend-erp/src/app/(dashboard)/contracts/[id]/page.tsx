'use client';

import React, { useState, useEffect } from 'react';
import { useRouter, useParams } from 'next/navigation';
import { Loader2, ArrowLeft, AlertTriangle, FileText, Edit, Trash2, CheckCircle2, XCircle, AlertOctagon, Send } from 'lucide-react';
import { Button } from '@/components/ui/Button';
import { Card, CardContent, CardHeader } from '@/components/ui/Card';
import supplierService, { ContractDetail } from '@/lib/supplier-service';

const statusLabels: Record<string, string> = {
  Draft: 'Borrador', Active: 'Activo', Expired: 'Vencido', Terminated: 'Terminado',
};
const typeLabels: Record<string, string> = {
  ServiceAgreement: 'Contrato de Servicios', Supply: 'Suministro',
  CivilWorks: 'Obra Civil', Lease: 'Arrendamiento',
};
const approvalLabels: Record<string, string> = {
  Administrator: 'Administrador', Council: 'Consejo',
};
const alertTypeLabels: Record<string, string> = {
  NinetyDaysToExpiration: 'Vence en 90 días',
  ThirtyDaysToExpiration: 'Vence en 30 días',
  FifteenDaysToExpiration: 'Vence en 15 días',
  AutoRenewalWarning: 'Renovación Automática',
};
const invoiceStatusLabels: Record<string, string> = {
  PendingPayment: 'Pendiente', PartiallyPaid: 'Pago Parcial', FullyPaid: 'Pagada', Cancelled: 'Anulada',
};
const paymentMethodLabels: Record<string, string> = {
  Cash: 'Efectivo', BankTransfer: 'Transferencia', Check: 'Cheque', CreditCard: 'Tarjeta de Crédito',
};

export default function ContractDetailPage() {
  const router = useRouter();
  const params = useParams();
  const id = params.id as string;

  const [contract, setContract] = useState<ContractDetail | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');
  const [showStatusModal, setShowStatusModal] = useState(false);
  const [newStatus, setNewStatus] = useState('');
  const [justification, setJustification] = useState('');
  const [submittingAction, setSubmittingAction] = useState(false);
  const [statusError, setStatusError] = useState('');
  const [councilActNumber, setCouncilActNumber] = useState('');

  const fetchContract = async () => {
    setLoading(true);
    setError('');
    try {
      const data = await supplierService.getContractById(id);
      setContract(data);
    } catch {
      setError('Error al cargar el contrato.');
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => { fetchContract(); }, [id]);

  const formatCurrency = (v: number) => new Intl.NumberFormat('es-CO', { style: 'currency', currency: 'COP', maximumFractionDigits: 0 }).format(v);
  const formatDate = (d: string) => new Date(d).toLocaleDateString('es-CO', { day: '2-digit', month: '2-digit', year: 'numeric' });

  const handleStatusChange = async () => {
    if (!newStatus || !justification.trim() || !contract) return;
    setStatusError('');

    const requiresCouncilAct = newStatus === 'Active' && contract.approvalLevel === 'Council';
    const missingAct = !contract.councilMeetingActNumber.trim() && !councilActNumber.trim();

    if (requiresCouncilAct && missingAct) {
      setStatusError('Este contrato requiere aprobación del consejo. Ingrese el número de acta antes de activarlo.');
      return;
    }

    setSubmittingAction(true);
    try {
      if (requiresCouncilAct && councilActNumber.trim() && !contract.councilMeetingActNumber.trim()) {
        await supplierService.updateContract(id, { councilMeetingActNumber: councilActNumber.trim() });
      }
      await supplierService.changeContractStatus(id, { newStatus });
      setShowStatusModal(false);
      setNewStatus('');
      setJustification('');
      setCouncilActNumber('');
      fetchContract();
    } catch (err: any) {
      setStatusError(err?.response?.data?.error || 'Error al cambiar el estado.');
    } finally {
      setSubmittingAction(false);
    }
  };

  const handleCancelInvoice = async (invoiceId: string) => {
    if (!confirm('¿Está seguro de anular esta factura? Se revertirá la ejecución presupuestal asociada.')) return;
    try {
      await supplierService.cancelInvoice(invoiceId);
      fetchContract();
    } catch (err: any) {
      alert(err?.response?.data?.error || 'Error al anular la factura.');
    }
  };

  const handleDelete = async () => {
    if (!confirm('¿Está seguro de eliminar este contrato?')) return;
    try {
      await supplierService.deleteContract(id);
      router.push('/contracts');
    } catch (err: any) {
      alert(err?.response?.data?.error || 'Error al eliminar.');
    }
  };

  const handleResolveAlert = async (alertId: string) => {
    try {
      await supplierService.resolveAlert(alertId);
      fetchContract();
    } catch (err: any) {
      alert(err?.response?.data?.error || 'Error al resolver la alerta.');
    }
  };

  const statusColor = (status: string) => {
    const map: Record<string, string> = {
      Draft: 'badge-info', Active: 'badge-success', Expired: 'badge-warning', Terminated: 'badge-neutral',
    };
    return map[status] || 'badge-neutral';
  };

  if (loading) {
    return <div className="flex justify-center py-20"><Loader2 className="w-8 h-8 animate-spin text-emerald-600" /></div>;
  }

  if (error || !contract) {
    return (
      <div className="space-y-6 max-w-2xl mx-auto">
        <button onClick={() => router.push('/contracts')} className="inline-flex items-center gap-2 text-sm text-muted-foreground hover:text-foreground">
          <ArrowLeft className="w-4 h-4" /> Volver
        </button>
        <div className="flex flex-col items-center gap-3 text-rose-600 py-12">
          <AlertTriangle className="w-10 h-10" />
          <p className="font-semibold">{error || 'Contrato no encontrado.'}</p>
        </div>
      </div>
    );
  }

  return (
    <div className="space-y-6">
      <button onClick={() => router.push('/contracts')} className="inline-flex items-center gap-2 text-sm text-muted-foreground hover:text-foreground transition-colors">
        <ArrowLeft className="w-4 h-4" /> Volver a Contratos
      </button>

      <div className="flex flex-col sm:flex-row justify-between items-start sm:items-center gap-4">
        <div>
          <div className="flex items-center gap-3">
            <h1 className="text-2xl font-bold text-foreground tracking-tight">{contract.contractNumber}</h1>
            <span className={statusColor(contract.status)}>{statusLabels[contract.status] || contract.status}</span>
          </div>
          <p className="text-sm text-muted-foreground mt-1">
            {typeLabels[contract.contractType]} — {contract.providerBusinessName}
          </p>
        </div>
        <div className="flex gap-2 flex-wrap">
          {contract.status === 'Draft' && (
            <Button variant="success" onClick={() => { setNewStatus('Active'); setShowStatusModal(true); }}>
              <CheckCircle2 className="w-4 h-4 mr-1" /> Activar
            </Button>
          )}
          {contract.status === 'Active' && (
            <Button variant="secondary" onClick={() => { setNewStatus('Terminated'); setShowStatusModal(true); }}>
              <XCircle className="w-4 h-4 mr-1" /> Terminar
            </Button>
          )}
          {contract.status === 'Draft' && (
            <Button variant="danger" onClick={handleDelete}>
              <Trash2 className="w-4 h-4 mr-1" /> Eliminar
            </Button>
          )}
        </div>
      </div>

      {showStatusModal && (
        <Card>
          <CardContent className="p-4 space-y-3">
            <h3 className="text-sm font-bold text-foreground">Cambiar Estado a: {statusLabels[newStatus] || newStatus}</h3>

            {newStatus === 'Active' && contract.approvalLevel === 'Council' && !contract.councilMeetingActNumber.trim() && (
              <input type="text" placeholder="Número de acta del consejo" value={councilActNumber}
                onChange={(e) => setCouncilActNumber(e.target.value.slice(0, 100))}
                className="w-full bg-transparent border-b border-emerald-600/30 focus:border-emerald-600 text-sm font-medium py-2 outline-none" />
            )}

            <div className="flex gap-3">
              <input type="text" placeholder="Justificación del cambio de estado" value={justification}
                onChange={(e) => setJustification(e.target.value)}
                className="flex-1 bg-transparent border-b border-emerald-600/30 focus:border-emerald-600 text-sm font-medium py-2 outline-none" />
              <Button variant="success" onClick={handleStatusChange} disabled={submittingAction || !justification.trim()}>
                {submittingAction ? <Loader2 className="w-4 h-4 animate-spin mr-1" /> : <Send className="w-4 h-4 mr-1" />}
                Confirmar
              </Button>
              <Button variant="ghost" onClick={() => { setShowStatusModal(false); setStatusError(''); }}>Cancelar</Button>
            </div>

            {statusError && (
              <div className="p-3 bg-rose-50 border border-rose-200 rounded-lg text-rose-700 text-xs flex items-center gap-2">
                <AlertTriangle className="w-4 h-4 shrink-0" /> {statusError}
              </div>
            )}
          </CardContent>
        </Card>
      )}

      {contract.alerts.filter(a => a.isActive).length > 0 && (
        <div className="bg-orange-50 dark:bg-orange-950/20 border border-orange-200 dark:border-orange-800 rounded-xl p-4">
          <h3 className="text-sm font-bold text-orange-800 dark:text-orange-300 mb-3 flex items-center gap-2">
            <AlertOctagon className="w-4 h-4" /> Alertas Activas ({contract.alerts.filter(a => a.isActive).length})
          </h3>
          <div className="space-y-2">
            {contract.alerts.filter(a => a.isActive).map((a) => (
              <div key={a.id} className="flex items-center justify-between bg-white dark:bg-orange-950/30 rounded-lg px-3 py-2">
                <div>
                  <span className="text-xs font-bold text-orange-700 dark:text-orange-400">{alertTypeLabels[a.alertType] || a.alertType}</span>
                  <p className="text-xs text-muted-foreground mt-0.5">{a.message}</p>
                </div>
                <button onClick={() => handleResolveAlert(a.id)}
                  className="text-xs font-semibold text-emerald-600 hover:text-emerald-800 px-2 py-1 rounded bg-emerald-50 hover:bg-emerald-100">
                  Resolver
                </button>
              </div>
            ))}
          </div>
        </div>
      )}

      <div className="grid grid-cols-1 lg:grid-cols-3 gap-6">
        <div className="lg:col-span-2 space-y-6">
          <Card>
            <CardHeader className="py-3 px-6"><h3 className="text-sm font-bold text-foreground">Información del Contrato</h3></CardHeader>
            <CardContent className="p-6">
              <div className="grid grid-cols-2 md:grid-cols-3 gap-4 text-sm">
                <div><span className="text-muted-foreground">Proveedor:</span><p className="font-medium">{contract.providerBusinessName}</p></div>
                <div><span className="text-muted-foreground">NIT/CC:</span><p className="font-medium">{contract.providerDocumentNumber}</p></div>
                <div><span className="text-muted-foreground">Tipo:</span><p className="font-medium">{typeLabels[contract.contractType]}</p></div>
                <div className="md:col-span-3"><span className="text-muted-foreground">Objeto:</span><p className="font-medium">{contract.objectDescription}</p></div>
                <div><span className="text-muted-foreground">Valor Total:</span><p className="font-bold text-lg text-emerald-600">{formatCurrency(contract.totalValue)}</p></div>
                <div><span className="text-muted-foreground">Valor Mensual:</span><p className="font-medium">{formatCurrency(contract.monthlyValue)}</p></div>
                <div><span className="text-muted-foreground">Inicio:</span><p className="font-medium">{formatDate(contract.startDate)}</p></div>
                <div><span className="text-muted-foreground">Fin:</span><p className="font-medium">{formatDate(contract.endDate)}</p></div>
                <div>
                  <span className="text-muted-foreground">Días Restantes:</span>
                  <p className={`font-bold ${contract.daysUntilExpiration <= 30 ? 'text-rose-600' : contract.daysUntilExpiration <= 90 ? 'text-orange-500' : 'text-foreground'}`}>
                    {contract.daysUntilExpiration > 0 ? contract.daysUntilExpiration : 'Vencido'}
                  </p>
                </div>
                <div><span className="text-muted-foreground">Aprobación:</span><p className="font-medium">{approvalLabels[contract.approvalLevel]}</p></div>
                <div><span className="text-muted-foreground">Renovación Auto.:</span><p className="font-medium">{contract.hasAutoRenewal ? `Sí (${contract.autoRenewalNoticeDays} días)` : 'No'}</p></div>
                {contract.councilMeetingActNumber && <div><span className="text-muted-foreground">Acta Consejo:</span><p className="font-medium">{contract.councilMeetingActNumber}</p></div>}
                {contract.approvedInAssemblyTitle && <div><span className="text-muted-foreground">Aprobado en Asamblea:</span><p className="font-medium">{contract.approvedInAssemblyTitle}</p></div>}
              </div>
              {contract.observations && (
                <div className="mt-4 p-3 bg-muted/30 rounded-lg">
                  <span className="text-xs font-bold text-muted-foreground uppercase tracking-wider">Observaciones</span>
                  <p className="text-sm mt-1 text-foreground">{contract.observations}</p>
                </div>
              )}
            </CardContent>
          </Card>

          <Card>
            <CardHeader className="py-3 px-6 flex flex-row items-center justify-between">
              <h3 className="text-sm font-bold text-foreground">Facturas</h3>
              <Button variant="secondary" onClick={() => router.push(`/contracts/${id}/invoices/new`)}>Nueva Factura</Button>
            </CardHeader>
            <CardContent className="p-0">
              {contract.invoices.length === 0 ? (
                <p className="text-sm text-muted-foreground text-center py-8">Sin facturas registradas.</p>
              ) : (
                <div className="overflow-x-auto">
                  <table className="min-w-full divide-y divide-border">
                    <thead className="bg-muted/50">
                      <tr>
                        <th className="px-5 py-3 text-left text-xs font-bold text-muted-foreground uppercase">Nro. Factura</th>
                        <th className="px-5 py-3 text-left text-xs font-bold text-muted-foreground uppercase">Fecha</th>
                        <th className="px-5 py-3 text-right text-xs font-bold text-muted-foreground uppercase">Total</th>
                        <th className="px-5 py-3 text-right text-xs font-bold text-muted-foreground uppercase">Pagado</th>
                        <th className="px-5 py-3 text-right text-xs font-bold text-muted-foreground uppercase">Pendiente</th>
                        <th className="px-5 py-3 text-left text-xs font-bold text-muted-foreground uppercase">Estado</th>
                        <th className="px-5 py-3 text-right text-xs font-bold text-muted-foreground uppercase">Acciones</th>
                      </tr>
                    </thead>
                    <tbody className="divide-y divide-border">
                      {contract.invoices.map((inv) => (
                        <tr key={inv.id} className="hover:bg-muted/30">
                          <td className="px-5 py-3 font-mono font-bold text-sm">{inv.invoiceNumber}</td>
                          <td className="px-5 py-3 text-sm text-muted-foreground">{formatDate(inv.invoiceDate)}</td>
                          <td className="px-5 py-3 text-sm text-right font-bold">{formatCurrency(inv.totalAmount)}</td>
                          <td className="px-5 py-3 text-sm text-right text-emerald-600">{formatCurrency(inv.amountPaid)}</td>
                          <td className="px-5 py-3 text-sm text-right font-bold text-orange-600">{formatCurrency(inv.pendingAmount)}</td>
                          <td className="px-5 py-3">
                            <span className={`badge-${inv.status === 'FullyPaid' ? 'success' : inv.status === 'PendingPayment' ? 'warning' : 'info'}`}>
                              {invoiceStatusLabels[inv.status] || inv.status}
                            </span>
                          </td>
                          <td className="px-5 py-3 text-right">
                            {inv.status !== 'Cancelled' && (
                              <button onClick={() => handleCancelInvoice(inv.id)}
                                className="text-xs font-semibold text-rose-600 hover:text-rose-800 px-2 py-1 rounded bg-rose-50 hover:bg-rose-100">
                                Anular
                              </button>
                            )}
                          </td>
                        </tr>
                      ))}
                    </tbody>
                  </table>
                </div>
              )}
            </CardContent>
          </Card>

          {contract.invoices.length > 0 && contract.invoices.some(inv => inv.payments.length > 0) && (
            <Card>
              <CardHeader className="py-3 px-6"><h3 className="text-sm font-bold text-foreground">Pagos Registrados</h3></CardHeader>
              <CardContent className="p-0">
                <div className="overflow-x-auto">
                  <table className="min-w-full divide-y divide-border">
                    <thead className="bg-muted/50">
                      <tr>
                        <th className="px-5 py-3 text-left text-xs font-bold text-muted-foreground uppercase">Factura</th>
                        <th className="px-5 py-3 text-left text-xs font-bold text-muted-foreground uppercase">Fecha</th>
                        <th className="px-5 py-3 text-right text-xs font-bold text-muted-foreground uppercase">Monto</th>
                        <th className="px-5 py-3 text-left text-xs font-bold text-muted-foreground uppercase">Método</th>
                        <th className="px-5 py-3 text-left text-xs font-bold text-muted-foreground uppercase">Referencia</th>
                      </tr>
                    </thead>
                    <tbody className="divide-y divide-border">
                      {contract.invoices.map((inv) =>
                        inv.payments.map((p) => (
                          <tr key={p.id} className="hover:bg-muted/30">
                            <td className="px-5 py-3 font-mono font-bold text-sm">{inv.invoiceNumber}</td>
                            <td className="px-5 py-3 text-sm text-muted-foreground">{formatDate(p.paymentDate)}</td>
                            <td className="px-5 py-3 text-sm text-right font-bold text-emerald-600">{formatCurrency(p.amount)}</td>
                            <td className="px-5 py-3 text-sm text-muted-foreground">{paymentMethodLabels[p.paymentMethod] || p.paymentMethod}</td>
                            <td className="px-5 py-3 font-mono text-sm text-muted-foreground">{p.referenceNumber}</td>
                          </tr>
                        ))
                      )}
                    </tbody>
                  </table>
                </div>
              </CardContent>
            </Card>
          )}
        </div>

        <div className="space-y-6">
          <Card>
            <CardHeader className="py-3 px-6"><h3 className="text-sm font-bold text-foreground">Resumen</h3></CardHeader>
            <CardContent className="p-4 space-y-3">
              <div className="flex justify-between text-sm">
                <span className="text-muted-foreground">Estado:</span>
                <span className={statusColor(contract.status)}>{statusLabels[contract.status]}</span>
              </div>
              <div className="flex justify-between text-sm">
                <span className="text-muted-foreground">Valor Total:</span>
                <span className="font-bold text-emerald-600">{formatCurrency(contract.totalValue)}</span>
              </div>
              <div className="flex justify-between text-sm">
                <span className="text-muted-foreground">Valor Mensual:</span>
                <span className="font-medium">{formatCurrency(contract.monthlyValue)}</span>
              </div>
              <div className="flex justify-between text-sm">
                <span className="text-muted-foreground">Facturas:</span>
                <span className="font-medium">{contract.invoices.length}</span>
              </div>
              <div className="flex justify-between text-sm">
                <span className="text-muted-foreground">Alertas Activas:</span>
                <span className="font-medium">{contract.alerts.filter(a => a.isActive).length}</span>
              </div>
            </CardContent>
          </Card>
        </div>
      </div>
    </div>
  );
}
