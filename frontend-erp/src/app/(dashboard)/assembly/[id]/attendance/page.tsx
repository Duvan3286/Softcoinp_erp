'use client';

import React, { useState, useEffect, useCallback } from 'react';
import { useRouter, useParams } from 'next/navigation';
import assemblyService, {
  AssemblyDetail,
  AttendanceDto,
  QuorumStatus,
  UnitWithOwnerInfo,
} from '@/lib/assembly-service';

export default function AttendanceRegistrationPage() {
  const router = useRouter();
  const params = useParams();
  const assemblyId = params.id as string;

  const [assembly, setAssembly] = useState<AssemblyDetail | null>(null);
  const [quorum, setQuorum] = useState<QuorumStatus | null>(null);
  const [attendances, setAttendances] = useState<AttendanceDto[]>([]);
  const [units, setUnits] = useState<UnitWithOwnerInfo[]>([]);
  const [loading, setLoading] = useState(true);
  const [submitting, setSubmitting] = useState(false);
  const [error, setError] = useState('');

  const [selectedUnitId, setSelectedUnitId] = useState('');
  const [attendsPersonally, setAttendsPersonally] = useState(true);
  const [representativeName, setRepresentativeName] = useState('');
  const [representativeDocumentNumber, setRepresentativeDocumentNumber] = useState('');
  const [powerOfAttorneyFilePath, setPowerOfAttorneyFilePath] = useState('');
  const [isCommissionMember, setIsCommissionMember] = useState(false);
  const [commissionRole, setCommissionRole] = useState('');
  const [notes, setNotes] = useState('');

  const loadData = useCallback(async () => {
    try {
      const [assemblyData, quorumData, attendancesData, unitsData] = await Promise.all([
        assemblyService.getAssemblyById(assemblyId),
        assemblyService.getQuorumStatus(assemblyId),
        assemblyService.getAttendances(assemblyId),
        assemblyService.getUnitsForAttendance(),
      ]);
      setAssembly(assemblyData);
      setQuorum(quorumData);
      setAttendances(attendancesData);
      setUnits(unitsData);
    } catch {
      setError('Error al cargar los datos de la asamblea.');
    } finally {
      setLoading(false);
    }
  }, [assemblyId]);

  useEffect(() => {
    loadData();
  }, [loadData]);

  const refreshQuorum = async () => {
    try {
      const [quorumData, attendancesData] = await Promise.all([
        assemblyService.getQuorumStatus(assemblyId),
        assemblyService.getAttendances(assemblyId),
      ]);
      setQuorum(quorumData);
      setAttendances(attendancesData);
    } catch {
      // silent
    }
  };

  const handleRegisterAttendance = async (e: React.FormEvent) => {
    e.preventDefault();
    setError('');

    if (!selectedUnitId) {
      setError('Seleccione una unidad para registrar asistencia.');
      return;
    }

    if (!attendsPersonally) {
      if (!representativeName.trim()) {
        setError('Ingrese el nombre del representante.');
        return;
      }
      if (!representativeDocumentNumber.trim()) {
        setError('Ingrese el documento del representante.');
        return;
      }
    }

    const selectedUnit = units.find((u) => u.unitId === selectedUnitId);
    if (!selectedUnit || !selectedUnit.ownerId) {
      setError('La unidad seleccionada no tiene un propietario asignado.');
      return;
    }

    setSubmitting(true);
    try {
      await assemblyService.registerAttendance(assemblyId, {
        unitId: selectedUnitId,
        ownerId: selectedUnit.ownerId,
        attendsPersonally,
        representativeName: attendsPersonally ? undefined : representativeName,
        representativeDocumentNumber: attendsPersonally ? undefined : representativeDocumentNumber,
        powerOfAttorneyFilePath: attendsPersonally ? undefined : powerOfAttorneyFilePath || undefined,
        isCommissionMember,
        commissionRole: commissionRole || undefined,
        notes: notes || undefined,
      });

      setSelectedUnitId('');
      setAttendsPersonally(true);
      setRepresentativeName('');
      setRepresentativeDocumentNumber('');
      setPowerOfAttorneyFilePath('');
      setIsCommissionMember(false);
      setCommissionRole('');
      setNotes('');

      await refreshQuorum();
    } catch {
      setError('Error al registrar la asistencia. Intente de nuevo.');
    } finally {
      setSubmitting(false);
    }
  };

  const handleMarkDeparture = async (attendanceId: string) => {
    try {
      await assemblyService.updateAttendance(attendanceId, {
        status: 'Departed',
        departureTime: new Date().toISOString(),
      });
      await refreshQuorum();
    } catch {
      setError('Error al registrar la salida.');
    }
  };

  const handleLiftRestriction = async (attendanceId: string) => {
    const reason = prompt('Ingrese el motivo para levantar la restricción de voto:');
    if (!reason) return;

    try {
      await assemblyService.liftVotingRestriction(attendanceId, reason);
      await refreshQuorum();
    } catch {
      setError('Error al levantar la restricción de voto.');
    }
  };

  const registeredUnitIds = attendances.map((a) => a.unitId);
  const availableUnits = units.filter((u) => !registeredUnitIds.includes(u.unitId));
  const selectedUnit = units.find((u) => u.unitId === selectedUnitId);

  if (loading) {
    return (
      <div className="flex items-center justify-center">
        <p className="text-muted-foreground text-sm">Cargando datos de asistencia...</p>
      </div>
    );
  }

  if (!assembly) {
    return (
      <div className="flex items-center justify-center">
        <p className="text-red-500 text-sm">No se pudo cargar la asamblea.</p>
      </div>
    );
  }

  return (
    <div className="p-6 space-y-6">
        <div className="flex items-center gap-4">
          <button
            onClick={() => router.push(`/assembly/${assemblyId}`)}
            className="text-emerald-600 hover:text-emerald-800 text-sm font-semibold"
          >
            &larr; Volver
          </button>
          <h1 className="text-2xl font-bold text-foreground tracking-tight">
            Registro de Asistencia - {assembly.title}
          </h1>
        </div>

        {error && (
          <div className="bg-rose-50 dark:bg-rose-950/20 border border-rose-200 dark:border-rose-900 text-rose-700 dark:text-rose-400 px-4 py-3 rounded-lg text-sm">
            {error}
          </div>
        )}

        {/* Quorum Panel */}
        {quorum && (
          <div className="bg-card border border-border rounded-lg p-5 space-y-4">
            <div className="flex items-center justify-between">
              <h2 className="text-lg font-bold text-foreground">Estado de Quórum</h2>
              <span
                className={`text-sm font-bold px-3 py-1 rounded-full ${
                  quorum.firstCallQuorumMet
                    ? 'bg-emerald-100 text-emerald-700'
                    : 'bg-rose-100 dark:bg-rose-950/30 text-rose-700 dark:text-rose-400'
                }`}
              >
                {quorum.firstCallQuorumMet
                  ? 'Quórum alcanzado'
                  : 'Quórum no alcanzado'}
              </span>
            </div>

            <div className="space-y-2">
              <div className="flex items-center justify-between text-sm">
                <span className="text-muted-foreground">
                  Quórum Primera Convocatoria: {quorum.percentagePresent.toFixed(1)}% (
                  {quorum.presentCoefficients.toFixed(2)} de{' '}
                  {quorum.totalCoefficients.toFixed(2)} coeficientes)
                </span>
              </div>
              <div className="w-full bg-muted rounded-full h-3">
                <div
                  className={`h-3 rounded-full transition-all duration-500 ${
                    quorum.firstCallQuorumMet ? 'bg-emerald-500' : 'bg-red-400'
                  }`}
                  style={{ width: `${Math.min(quorum.percentagePresent, 100)}%` }}
                />
              </div>
            </div>

            <div className="grid grid-cols-2 sm:grid-cols-4 gap-4 pt-2">
              <div className="text-center">
                <p className="text-2xl font-bold text-foreground">{quorum.presentOwners}</p>
                <p className="text-xs text-muted-foreground">Presentes</p>
              </div>
              <div className="text-center">
                <p className="text-2xl font-bold text-foreground">{quorum.absentOwners}</p>
                <p className="text-xs text-muted-foreground">Ausentes</p>
              </div>
              <div className="text-center">
                <p className="text-2xl font-bold text-orange-600">{quorum.ownersWithArrears}</p>
                <p className="text-xs text-muted-foreground">Con mora</p>
              </div>
              <div className="text-center">
                <p className="text-2xl font-bold text-yellow-600">
                  {quorum.ownersWithRestrictedVoting}
                </p>
                <p className="text-xs text-muted-foreground">Voto restringido</p>
              </div>
            </div>
          </div>
        )}

        {/* Registration Form */}
        <div className="bg-card border border-border rounded-lg p-5">
          <h2 className="text-lg font-bold text-foreground mb-4">Registrar Asistencia</h2>
          <form onSubmit={handleRegisterAttendance} className="space-y-4">
            <div>
              <label className="block text-sm font-medium text-foreground mb-1">
                Unidad / Propietario
              </label>
              <select
                value={selectedUnitId}
                onChange={(e) => setSelectedUnitId(e.target.value)}
                className="w-full bg-transparent border-b border-emerald-600/30 focus:border-emerald-600 text-sm font-medium py-2 outline-none"
              >
                <option value="">Seleccione una unidad...</option>
                {availableUnits.map((u) => (
                  <option key={u.unitId} value={u.unitId}>
                    {u.unitIdentifier} - {u.ownerName || 'Sin propietario'} (Coef.{' '}
                    {u.coefficient})
                  </option>
                ))}
              </select>
            </div>

            <div className="flex items-center gap-3">
              <input
                type="checkbox"
                id="attendsPersonally"
                checked={attendsPersonally}
                onChange={(e) => setAttendsPersonally(e.target.checked)}
                className="h-4 w-4 text-emerald-600 rounded border-emerald-600/30"
              />
              <label htmlFor="attendsPersonally" className="text-sm font-medium text-foreground">
                Asiste personalmente
              </label>
            </div>

            {!attendsPersonally && (
              <div className="space-y-3 pl-6 border-l-2 border-emerald-600/20">
                <div>
                  <label className="block text-sm font-medium text-foreground mb-1">
                    Nombre del representante
                  </label>
                  <input
                    type="text"
                    value={representativeName}
                    onChange={(e) => setRepresentativeName(e.target.value)}
                    className="w-full bg-transparent border-b border-emerald-600/30 focus:border-emerald-600 text-sm font-medium py-2 outline-none"
                    placeholder="Nombre completo"
                  />
                </div>
                <div>
                  <label className="block text-sm font-medium text-foreground mb-1">
                    Documento del representante
                  </label>
                  <input
                    type="text"
                    value={representativeDocumentNumber}
                    onChange={(e) => setRepresentativeDocumentNumber(e.target.value)}
                    className="w-full bg-transparent border-b border-emerald-600/30 focus:border-emerald-600 text-sm font-medium py-2 outline-none"
                    placeholder="Número de documento"
                  />
                </div>
                <div>
                  <label className="block text-sm font-medium text-foreground mb-1">
                    Poder / Documento de poder
                  </label>
                  <input
                    type="text"
                    value={powerOfAttorneyFilePath}
                    onChange={(e) => setPowerOfAttorneyFilePath(e.target.value)}
                    className="w-full bg-transparent border-b border-emerald-600/30 focus:border-emerald-600 text-sm font-medium py-2 outline-none"
                    placeholder="Ruta o referencia del poder"
                  />
                </div>
              </div>
            )}

            {selectedUnit && (
              <div className="bg-emerald-50 rounded-lg px-4 py-2 text-sm">
                <span className="font-medium text-emerald-700">
                  Coeficiente: {selectedUnit.coefficient}
                </span>
                <span className="text-emerald-600 ml-3">
                  | Propietario: {selectedUnit.ownerName || 'No asignado'}
                </span>
              </div>
            )}

            <div className="flex items-center gap-3">
              <input
                type="checkbox"
                id="isCommissionMember"
                checked={isCommissionMember}
                onChange={(e) => setIsCommissionMember(e.target.checked)}
                className="h-4 w-4 text-emerald-600 rounded border-emerald-600/30"
              />
              <label htmlFor="isCommissionMember" className="text-sm font-medium text-foreground">
                Miembro de comisión
              </label>
            </div>

            {isCommissionMember && (
              <div className="pl-6 border-l-2 border-emerald-600/20">
                <label className="block text-sm font-medium text-foreground mb-1">
                  Rol de comisión
                </label>
                <input
                  type="text"
                  value={commissionRole}
                  onChange={(e) => setCommissionRole(e.target.value)}
                  className="w-full bg-transparent border-b border-emerald-600/30 focus:border-emerald-600 text-sm font-medium py-2 outline-none"
                  placeholder="Ej: Presidente, Secretario, Vocal"
                />
              </div>
            )}

            <div>
              <label className="block text-sm font-medium text-foreground mb-1">Notas</label>
              <textarea
                value={notes}
                onChange={(e) => setNotes(e.target.value)}
                rows={3}
                className="w-full bg-transparent border-b border-emerald-600/30 focus:border-emerald-600 text-sm font-medium py-2 outline-none resize-none"
                placeholder="Observaciones adicionales..."
              />
            </div>

            <button
              type="submit"
              disabled={submitting || !selectedUnitId}
              className="bg-emerald-600 hover:bg-emerald-700 disabled:opacity-50 disabled:cursor-not-allowed text-white font-semibold px-5 py-2.5 rounded-lg transition-colors text-sm"
            >
              {submitting ? 'Registrando...' : 'Registrar Asistencia'}
            </button>
          </form>
        </div>

        {/* Registered Attendees */}
        <div className="bg-card border border-border rounded-lg p-5">
          <h2 className="text-lg font-bold text-foreground mb-4">
            Asistentes Registrados ({attendances.length})
          </h2>

          {attendances.length === 0 ? (
            <p className="text-sm text-muted-foreground text-center py-6">
              No hay asistencias registradas aún.
            </p>
          ) : (
            <div className="overflow-x-auto">
              <table className="min-w-full divide-y divide-border">
                <thead className="bg-muted/50">
                  <tr>
                    <th className="px-4 py-2 text-left text-xs font-bold text-muted-foreground uppercase">
                      Unidad
                    </th>
                    <th className="px-4 py-2 text-left text-xs font-bold text-muted-foreground uppercase">
                      Propietario
                    </th>
                    <th className="px-4 py-2 text-center text-xs font-bold text-muted-foreground uppercase">
                      Coeficiente
                    </th>
                    <th className="px-4 py-2 text-center text-xs font-bold text-muted-foreground uppercase">
                      Tipo
                    </th>
                    <th className="px-4 py-2 text-center text-xs font-bold text-muted-foreground uppercase">
                      Estado
                    </th>
                    <th className="px-4 py-2 text-center text-xs font-bold text-muted-foreground uppercase">
                      Mora
                    </th>
                    <th className="px-4 py-2 text-center text-xs font-bold text-muted-foreground uppercase">
                      Voto
                    </th>
                    <th className="px-4 py-2 text-right text-xs font-bold text-muted-foreground uppercase">
                      Acciones
                    </th>
                  </tr>
                </thead>
                <tbody className="divide-y divide-border">
                  {attendances.map((att) => (
                    <tr key={att.id} className="hover:bg-muted/30 transition-colors">
                      <td className="px-4 py-2 text-sm font-medium">{att.unitIdentifier}</td>
                      <td className="px-4 py-2 text-sm">{att.ownerName}</td>
                      <td className="px-4 py-2 text-sm text-center">{att.coefficient}</td>
                      <td className="px-4 py-2 text-center">
                        {att.attendsPersonally ? (
                          <span className="inline-flex items-center px-2 py-0.5 rounded-full text-xs font-medium bg-blue-100 dark:bg-blue-950/30 text-blue-700 dark:text-blue-400">
                            Personal
                          </span>
                        ) : (
                          <span className="inline-flex items-center px-2 py-0.5 rounded-full text-xs font-medium bg-purple-100 text-purple-700">
                            Representante
                          </span>
                        )}
                      </td>
                      <td className="px-4 py-2 text-center">
                        {att.departureTime ? (
                          <span className="inline-flex items-center px-2 py-0.5 rounded-full text-xs font-medium bg-muted text-muted-foreground">
                            Retirado
                          </span>
                        ) : (
                          <span className="inline-flex items-center px-2 py-0.5 rounded-full text-xs font-medium bg-emerald-100 text-emerald-700">
                            Presente
                          </span>
                        )}
                      </td>
                      <td className="px-4 py-2 text-center">
                        {att.hasDuesArrears ? (
                          <span className="inline-flex items-center px-2 py-0.5 rounded-full text-xs font-medium bg-orange-100 text-orange-700">
                            Sí
                          </span>
                        ) : (
                          <span className="text-xs text-muted-foreground">No</span>
                        )}
                      </td>
                      <td className="px-4 py-2 text-center">
                        {att.votingRightRestricted ? (
                          <span className="inline-flex items-center px-2 py-0.5 rounded-full text-xs font-medium bg-rose-100 dark:bg-rose-950/30 text-rose-700 dark:text-rose-400">
                            Restringido
                          </span>
                        ) : (
                          <span className="inline-flex items-center px-2 py-0.5 rounded-full text-xs font-medium bg-emerald-100 text-emerald-700">
                            Habilitado
                          </span>
                        )}
                      </td>
                      <td className="px-4 py-2 text-right">
                        <div className="flex items-center justify-end gap-2">
                          {!att.departureTime && (
                            <button
                              onClick={() => handleMarkDeparture(att.id)}
                              className="text-orange-600 hover:text-orange-800 text-xs font-semibold"
                            >
                              Salida
                            </button>
                          )}
                          {att.votingRightRestricted && (
                            <button
                              onClick={() => handleLiftRestriction(att.id)}
                              className="text-emerald-600 hover:text-emerald-800 text-xs font-semibold"
                            >
                              Levantar
                            </button>
                          )}
                        </div>
                      </td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>
          )}
        </div>
      </div>
  );
}
