'use client';

import React, { useState, useEffect } from 'react';
import { useRouter, useSearchParams } from 'next/navigation';
import { Loader2, FileText, ShieldCheck, AlertTriangle, CheckCircle, XCircle, Search, Calendar, Download, Eye } from 'lucide-react';
import { Button } from '@/components/ui/Button';
import { Card, CardHeader, CardContent } from '@/components/ui/Card';
import feesPortfolioService, { UnitStatement, ClearanceCertificateSummary } from '@/lib/fees-portfolio-service';
import { UnitsService as unitsService, Unit, formatUnitLabel } from '@/lib/units-service';

type Tab = 'statement' | 'clearance';

export default function DocumentsPage() {
  const router = useRouter();
  const searchParams = useSearchParams();
  const [units, setUnits] = useState<Unit[]>([]);
  const [loadingUnits, setLoadingUnits] = useState(true);
  const [error, setError] = useState('');
  const [success, setSuccess] = useState('');
  const [activeTab, setActiveTab] = useState<Tab>('statement');

  // Statement state
  const [stmtUnitId, setStmtUnitId] = useState(searchParams.get('unitId') || '');
  const [stmtStartDate, setStmtStartDate] = useState('');
  const [stmtEndDate, setStmtEndDate] = useState('');
  const [generatingStmt, setGeneratingStmt] = useState(false);
  const [statement, setStatement] = useState<UnitStatement | null>(null);

  // Clearance state
  const [certUnitId, setCertUnitId] = useState('');
  const [validityDays, setValidityDays] = useState<number>(90);
  const [issuing, setIssuing] = useState(false);
  const [certificate, setCertificate] = useState<{ id: string; certificateNumber: string; issueDate: string; expirationDate: string; status: string } | null>(null);
  const [certificates, setCertificates] = useState<ClearanceCertificateSummary[]>([]);
  const [loadingCerts, setLoadingCerts] = useState(false);
  const [revokingId, setRevokingId] = useState('');
  const [downloadingId, setDownloadingId] = useState('');

  useEffect(() => {
    const fetchUnits = async () => {
      try {
        const data = await unitsService.getUnits();
        setUnits(data);
      } catch {
        setError('Error al cargar las unidades.');
      } finally {
        setLoadingUnits(false);
      }
    };
    fetchUnits();
  }, []);

  const handleGenerateStatement = async () => {
    setError('');
    setStatement(null);
    if (!stmtUnitId) { setError('Debe seleccionar una unidad.'); return; }
    setGeneratingStmt(true);
    try {
      const data = await feesPortfolioService.getUnitStatement({
        unitId: stmtUnitId,
        startDate: stmtStartDate || undefined,
        endDate: stmtEndDate || undefined,
      });
      setStatement(data);
    } catch (err: any) {
      setError(err?.response?.data || 'Error al generar el estado de cuenta.');
    } finally {
      setGeneratingStmt(false);
    }
  };

  const handleIssueCertificate = async () => {
    setError('');
    setSuccess('');
    setCertificate(null);
    if (!certUnitId) { setError('Debe seleccionar una unidad.'); return; }
    if (validityDays <= 0) { setError('Los días de validez deben ser mayor a cero.'); return; }
    setIssuing(true);
    try {
      const data = await feesPortfolioService.issueClearanceCertificate({
        unitId: certUnitId,
        validityDays,
      });
      setCertificate(data);
      setSuccess('Paz y salvo expedido exitosamente.');
      fetchCertificates(certUnitId);
    } catch (err: any) {
      setError(err?.response?.data || 'Error al expedir el paz y salvo.');
    } finally {
      setIssuing(false);
    }
  };

  const fetchCertificates = async (unitId: string) => {
    if (!unitId) return;
    setLoadingCerts(true);
    try {
      const data = await feesPortfolioService.getUnitCertificates(unitId);
      setCertificates(data);
    } catch {
      // do not overwrite main error
    } finally {
      setLoadingCerts(false);
    }
  };

  const handleCertUnitChange = (value: string) => {
    setCertUnitId(value);
    setCertificate(null);
    setSuccess('');
    if (value) {
      fetchCertificates(value);
    } else {
      setCertificates([]);
    }
  };

  const handleRevoke = async (certId: string) => {
    setError('');
    setSuccess('');
    setRevokingId(certId);
    try {
      await feesPortfolioService.revokeCertificate(certId);
      setSuccess('Certificado revocado exitosamente.');
      fetchCertificates(certUnitId);
    } catch (err: any) {
      setError(err?.response?.data || 'Error al revocar el certificado.');
    } finally {
      setRevokingId('');
    }
  };

  const handleDownloadPdf = async (certId: string, certNumber: string) => {
    setError('');
    setDownloadingId(certId);
    try {
      const blob = await feesPortfolioService.downloadCertificatePdf(certId);
      const url = window.URL.createObjectURL(blob);
      const link = document.createElement('a');
      link.href = url;
      link.download = `paz-y-salvo-${certNumber}.pdf`;
      document.body.appendChild(link);
      link.click();
      link.remove();
      window.URL.revokeObjectURL(url);
    } catch {
      setError('Error al descargar el PDF del paz y salvo.');
    } finally {
      setDownloadingId('');
    }
  };

  const formatCurrency = (val: number) =>
    new Intl.NumberFormat('es-CO', { style: 'currency', currency: 'COP', minimumFractionDigits: 0 }).format(val);

  const certStatusBadge = (status: string) => {
    const map: Record<string, string> = {
      Active: 'badge-success',
      Revoked: 'badge-danger',
      Expired: 'badge-neutral',
    };
    const labels: Record<string, string> = {
      Active: 'Vigente',
      Revoked: 'Revocado',
      Expired: 'Expirado',
    };
    return <span className={map[status] || 'badge-neutral'}>{labels[status] || status}</span>;
  };

  if (loadingUnits) {
    return (
      <div className="flex justify-center py-20">
        <Loader2 className="w-8 h-8 animate-spin text-emerald-600" />
      </div>
    );
  }

  return (
    <div className="space-y-6">
      <div>
        <h1 className="text-2xl font-bold text-foreground tracking-tight">Documentos</h1>
        <p className="text-sm text-muted-foreground mt-1">Genera estados de cuenta y paz y salvos.</p>
      </div>

      <div className="flex gap-1 bg-muted p-1 rounded-lg w-fit">
        <button
          onClick={() => setActiveTab('statement')}
          className={`px-4 py-2 text-sm font-semibold rounded-md transition-all ${activeTab === 'statement' ? 'bg-card text-foreground shadow-sm' : 'text-muted-foreground hover:text-foreground'}`}
        >
          <FileText className="w-4 h-4 inline mr-2" />
          Estado de Cuenta
        </button>
        <button
          onClick={() => setActiveTab('clearance')}
          className={`px-4 py-2 text-sm font-semibold rounded-md transition-all ${activeTab === 'clearance' ? 'bg-card text-foreground shadow-sm' : 'text-muted-foreground hover:text-foreground'}`}
        >
          <ShieldCheck className="w-4 h-4 inline mr-2" />
          Paz y Salvo
        </button>
      </div>

      {error && (
        <div className="p-4 bg-rose-50 border border-rose-200 rounded-xl text-rose-700 text-sm flex items-center gap-2">
          <AlertTriangle className="w-5 h-5 shrink-0" />
          {error}
        </div>
      )}

      {success && (
        <div className="p-4 bg-emerald-50 border border-emerald-200 rounded-xl text-emerald-700 text-sm flex items-center gap-2">
          <CheckCircle className="w-5 h-5 shrink-0" />
          {success}
        </div>
      )}

      {activeTab === 'statement' && (
        <div className="space-y-6">
          <Card>
            <CardContent className="p-6">
              <div className="grid grid-cols-1 md:grid-cols-4 gap-6">
                <div className="md:col-span-4">
                  <label className="block text-xs font-bold text-muted-foreground uppercase tracking-widest mb-1.5">Unidad</label>
                  <select
                    value={stmtUnitId}
                    onChange={(e) => setStmtUnitId(e.target.value)}
                    className="w-full bg-transparent border-b border-emerald-600 focus:border-b-2 text-foreground pl-0 pr-6 py-2 text-sm focus:outline-none transition-all"
                    required
                  >
                    <option value="">Seleccione una unidad...</option>
                    {units.map((u) => (
                      <option key={u.id} value={u.id}>{formatUnitLabel(u.identifier, u.towerOrBlock)}</option>
                    ))}
                  </select>
                </div>
                <div>
                  <label className="block text-xs font-bold text-muted-foreground uppercase tracking-widest mb-1.5">Fecha Inicio (opcional)</label>
                  <input
                    type="date"
                    value={stmtStartDate}
                    onChange={(e) => setStmtStartDate(e.target.value)}
                    className="w-full bg-transparent border-b border-emerald-600 focus:border-b-2 text-foreground pl-0 pr-6 py-2 text-sm focus:outline-none transition-all"
                  />
                </div>
                <div>
                  <label className="block text-xs font-bold text-muted-foreground uppercase tracking-widest mb-1.5">Fecha Fin (opcional)</label>
                  <input
                    type="date"
                    value={stmtEndDate}
                    onChange={(e) => setStmtEndDate(e.target.value)}
                    className="w-full bg-transparent border-b border-emerald-600 focus:border-b-2 text-foreground pl-0 pr-6 py-2 text-sm focus:outline-none transition-all"
                  />
                </div>
                <div className="flex items-end">
                  <Button onClick={handleGenerateStatement} disabled={generatingStmt}>
                    {generatingStmt ? <Loader2 className="w-4 h-4 animate-spin mr-2" /> : <Search className="w-4 h-4 mr-2" />}
                    Generar
                  </Button>
                </div>
              </div>
            </CardContent>
          </Card>

          {statement && (
            <>
              <div className="grid grid-cols-2 md:grid-cols-4 gap-4">
                <Card>
                  <CardContent className="p-4">
                    <p className="text-xs text-muted-foreground font-medium">Saldo Inicial</p>
                    <p className="text-lg font-bold text-foreground">{formatCurrency(statement.openingBalance)}</p>
                  </CardContent>
                </Card>
                <Card>
                  <CardContent className="p-4">
                    <p className="text-xs text-muted-foreground font-medium">Cargos</p>
                    <p className="text-lg font-bold text-rose-600">{formatCurrency(statement.totalCharges)}</p>
                  </CardContent>
                </Card>
                <Card>
                  <CardContent className="p-4">
                    <p className="text-xs text-muted-foreground font-medium">Pagos</p>
                    <p className="text-lg font-bold text-emerald-600">{formatCurrency(statement.totalPayments)}</p>
                  </CardContent>
                </Card>
                <Card>
                  <CardContent className="p-4">
                    <p className="text-xs text-muted-foreground font-medium">Saldo Final</p>
                    <p className={`text-lg font-bold ${statement.closingBalance >= 0 ? 'text-emerald-600' : 'text-rose-600'}`}>
                      {formatCurrency(statement.closingBalance)}
                    </p>
                  </CardContent>
                </Card>
              </div>

              <div className="grid grid-cols-1 md:grid-cols-3 gap-4">
                <Card>
                  <CardContent className="p-4">
                    <p className="text-xs text-muted-foreground font-medium">Total Capital Vencido</p>
                    <p className="text-lg font-bold text-foreground">{formatCurrency(statement.principalBalance)}</p>
                  </CardContent>
                </Card>
                <Card>
                  <CardContent className="p-4">
                    <p className="text-xs text-muted-foreground font-medium">Total Intereses Causados</p>
                    <p className="text-lg font-bold text-amber-600">{formatCurrency(statement.interestBalance)}</p>
                  </CardContent>
                </Card>
                <Card>
                  <CardContent className="p-4">
                    <p className="text-xs text-muted-foreground font-medium">Total Consolidado</p>
                    <p className="text-lg font-bold text-foreground">{formatCurrency(statement.principalBalance + statement.interestBalance)}</p>
                  </CardContent>
                </Card>
              </div>

              <Card>
                <CardHeader>
                  <h3 className="font-bold text-foreground">Movimientos</h3>
                </CardHeader>
                <CardContent className="p-0">
                  <div className="overflow-x-auto">
                    <table className="min-w-full divide-y divide-border">
                      <thead className="bg-muted/50">
                        <tr>
                          <th className="px-6 py-4 text-left text-xs font-bold text-muted-foreground uppercase tracking-wider">Fecha</th>
                          <th className="px-6 py-4 text-left text-xs font-bold text-muted-foreground uppercase tracking-wider">Descripción</th>
                          <th className="px-6 py-4 text-left text-xs font-bold text-muted-foreground uppercase tracking-wider">Detalle Interés</th>
                          <th className="px-6 py-4 text-right text-xs font-bold text-muted-foreground uppercase tracking-wider">Débito</th>
                          <th className="px-6 py-4 text-right text-xs font-bold text-muted-foreground uppercase tracking-wider">Crédito</th>
                          <th className="px-6 py-4 text-right text-xs font-bold text-muted-foreground uppercase tracking-wider">Saldo</th>
                        </tr>
                      </thead>
                      <tbody className="divide-y divide-border">
                        {statement.lines.length === 0 ? (
                          <tr>
                            <td colSpan={6} className="px-6 py-12 text-center text-muted-foreground">
                              No hay movimientos en el período seleccionado.
                            </td>
                          </tr>
                        ) : (
                          statement.lines.map((line, i) => {
                            let interestDetail = '—';
                            if (line.lineType === 'Interest' && line.dailyRate !== undefined && line.daysInPeriod !== undefined && line.baseAmount !== undefined) {
                              interestDetail = `Período ${line.period} · Tasa diaria ${(line.dailyRate * 100).toFixed(6)}% · ${line.daysInPeriod} días · Base ${formatCurrency(line.baseAmount)}`;
                            }

                            let paymentBadge = null;
                            if (line.lineType === 'Payment' && line.imputationType) {
                              const badgeClass = line.imputationType === 'Manual'
                                ? 'bg-amber-100 text-amber-800'
                                : 'bg-slate-100 text-slate-700';
                              const badgeLabel = line.imputationType === 'Manual' ? 'Manual' : 'Automática';
                              paymentBadge = (
                                <span className={`inline-block px-2 py-0.5 rounded-full text-xs font-semibold ${badgeClass}`}>
                                  {badgeLabel}
                                </span>
                              );
                            }

                            return (
                              <tr key={i} className="hover:bg-muted/30 transition-colors">
                                <td className="px-6 py-4 whitespace-nowrap text-sm text-muted-foreground">{new Date(line.date).toLocaleDateString('es-CO')}</td>
                                <td className="px-6 py-4 whitespace-nowrap text-sm text-foreground">{line.description}</td>
                                <td className="px-6 py-4 text-xs text-muted-foreground space-y-1">
                                  <div>{interestDetail}</div>
                                  {paymentBadge}
                                </td>
                                <td className="px-6 py-4 whitespace-nowrap text-right font-mono text-sm text-rose-600">
                                  {line.debit > 0 ? formatCurrency(line.debit) : '—'}
                                </td>
                                <td className="px-6 py-4 whitespace-nowrap text-right font-mono text-sm text-emerald-600">
                                  {line.credit > 0 ? formatCurrency(line.credit) : '—'}
                                </td>
                                <td className="px-6 py-4 whitespace-nowrap text-right font-mono text-sm font-bold text-foreground">
                                  {formatCurrency(line.balance)}
                                </td>
                              </tr>
                            );
                          })
                        )}
                      </tbody>
                    </table>
                  </div>
                </CardContent>
              </Card>

              <p className="text-xs text-muted-foreground px-1">
                Unidad: {statement.unitIdentifier} ({statement.unitTower})
              </p>
            </>
          )}
        </div>
      )}

      {activeTab === 'clearance' && (
        <div className="space-y-6">
          <Card>
            <CardContent className="p-6">
              <div className="grid grid-cols-1 md:grid-cols-4 gap-6">
                <div className="md:col-span-2">
                  <label className="block text-xs font-bold text-muted-foreground uppercase tracking-widest mb-1.5">Unidad</label>
                  <select
                    value={certUnitId}
                    onChange={(e) => handleCertUnitChange(e.target.value)}
                    className="w-full bg-transparent border-b border-emerald-600 focus:border-b-2 text-foreground pl-0 pr-6 py-2 text-sm focus:outline-none transition-all"
                    required
                  >
                    <option value="">Seleccione una unidad...</option>
                    {units.map((u) => (
                      <option key={u.id} value={u.id}>{formatUnitLabel(u.identifier, u.towerOrBlock)}</option>
                    ))}
                  </select>
                </div>
                <div>
                  <label className="block text-xs font-bold text-muted-foreground uppercase tracking-widest mb-1.5">Días de Validez</label>
                  <input
                    type="number"
                    min="1"
                    value={validityDays}
                    onChange={(e) => setValidityDays(Number(e.target.value))}
                    className="w-full bg-transparent border-b border-emerald-600 focus:border-b-2 text-foreground pl-0 pr-6 py-2 text-sm focus:outline-none transition-all"
                    required
                  />
                </div>
                <div className="flex items-end">
                  <Button onClick={handleIssueCertificate} disabled={issuing || !certUnitId}>
                    {issuing ? <Loader2 className="w-4 h-4 animate-spin mr-2" /> : <ShieldCheck className="w-4 h-4 mr-2" />}
                    Expedir
                  </Button>
                </div>
              </div>
            </CardContent>
          </Card>

          {certificate && (
            <Card>
              <CardHeader>
                <h3 className="font-bold text-foreground">Paz y Salvo Expedido</h3>
              </CardHeader>
              <CardContent>
                <div className="grid grid-cols-2 md:grid-cols-4 gap-4">
                  <div>
                    <p className="text-xs text-muted-foreground font-medium">Número</p>
                    <p className="text-sm font-bold text-foreground">{certificate.certificateNumber}</p>
                  </div>
                  <div>
                    <p className="text-xs text-muted-foreground font-medium">Fecha Expedición</p>
                    <p className="text-sm font-semibold text-foreground">{new Date(certificate.issueDate).toLocaleDateString('es-CO')}</p>
                  </div>
                  <div>
                    <p className="text-xs text-muted-foreground font-medium">Fecha Vencimiento</p>
                    <p className="text-sm font-semibold text-foreground">{new Date(certificate.expirationDate).toLocaleDateString('es-CO')}</p>
                  </div>
                  <div>
                    <p className="text-xs text-muted-foreground font-medium">Estado</p>
                    {certStatusBadge(certificate.status)}
                  </div>
                </div>
                <div className="mt-4">
                  <Button
                    variant="secondary"
                    onClick={() => handleDownloadPdf(certificate.id, certificate.certificateNumber)}
                    disabled={downloadingId === certificate.id}
                  >
                    {downloadingId === certificate.id ? <Loader2 className="w-4 h-4 animate-spin mr-2" /> : <Download className="w-4 h-4 mr-2" />}
                    Descargar PDF
                  </Button>
                </div>
              </CardContent>
            </Card>
          )}

          {certUnitId && (
            <Card>
              <CardHeader>
                <h3 className="font-bold text-foreground">Historial de Paz y Salvos</h3>
              </CardHeader>
              <CardContent className="p-0">
                <div className="overflow-x-auto">
                  <table className="min-w-full divide-y divide-border">
                    <thead className="bg-muted/50">
                      <tr>
                        <th className="px-6 py-4 text-left text-xs font-bold text-muted-foreground uppercase tracking-wider">No. Certificado</th>
                        <th className="px-6 py-4 text-left text-xs font-bold text-muted-foreground uppercase tracking-wider">Fecha Expedición</th>
                        <th className="px-6 py-4 text-left text-xs font-bold text-muted-foreground uppercase tracking-wider">Fecha Vencimiento</th>
                        <th className="px-6 py-4 text-left text-xs font-bold text-muted-foreground uppercase tracking-wider">Estado</th>
                        <th className="px-6 py-4 text-right text-xs font-bold text-muted-foreground uppercase tracking-wider">Acciones</th>
                      </tr>
                    </thead>
                    <tbody className="divide-y divide-border">
                      {loadingCerts ? (
                        <tr>
                          <td colSpan={5} className="px-6 py-12 text-center">
                            <Loader2 className="w-6 h-6 animate-spin mx-auto text-emerald-600" />
                          </td>
                        </tr>
                      ) : certificates.length === 0 ? (
                        <tr>
                          <td colSpan={5} className="px-6 py-12 text-center text-muted-foreground">
                            <ShieldCheck className="w-12 h-12 mx-auto text-muted-foreground/40 mb-3" />
                            <p className="font-semibold">No hay certificados</p>
                            <p className="text-sm mt-1">Expedir un paz y salvo para esta unidad.</p>
                          </td>
                        </tr>
                      ) : (
                        certificates.map((c) => (
                          <tr key={c.id} className="hover:bg-muted/30 transition-colors">
                            <td className="px-6 py-4 whitespace-nowrap font-semibold text-foreground">{c.certificateNumber}</td>
                            <td className="px-6 py-4 whitespace-nowrap text-sm text-muted-foreground">{new Date(c.issueDate).toLocaleDateString('es-CO')}</td>
                            <td className="px-6 py-4 whitespace-nowrap text-sm text-muted-foreground">{new Date(c.expirationDate).toLocaleDateString('es-CO')}</td>
                            <td className="px-6 py-4 whitespace-nowrap">{certStatusBadge(c.status)}</td>
                            <td className="px-6 py-4 whitespace-nowrap text-right space-x-2">
                              <button
                                onClick={() => handleDownloadPdf(c.id, c.certificateNumber)}
                                disabled={downloadingId === c.id}
                                className="text-emerald-600 hover:text-emerald-800 text-sm font-semibold px-3 py-1.5 bg-emerald-50 rounded-lg hover:bg-emerald-100 transition-colors"
                              >
                                {downloadingId === c.id ? <Loader2 className="w-4 h-4 animate-spin inline mr-1" /> : <Download className="w-4 h-4 inline mr-1" />}
                                PDF
                              </button>
                              {c.status === 'Active' && (
                                <button
                                  onClick={() => handleRevoke(c.id)}
                                  disabled={revokingId === c.id}
                                  className="text-rose-600 hover:text-rose-800 text-sm font-semibold px-3 py-1.5 bg-rose-50 rounded-lg hover:bg-rose-100 transition-colors"
                                >
                                  {revokingId === c.id ? <Loader2 className="w-4 h-4 animate-spin inline mr-1" /> : <XCircle className="w-4 h-4 inline mr-1" />}
                                  Revocar
                                </button>
                              )}
                            </td>
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
      )}
    </div>
  );
}
