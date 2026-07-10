"use client";

import React, { useState, useEffect, useCallback } from "react";
import { useRouter, useParams } from "next/navigation";
import assemblyService, {
  AssemblyDetail,
  MinutesDto,
  GenerateMinutesRequest,
  ApproveMinutesRequest,
} from "@/lib/assembly-service";


const MINUTES_STATUS_LABELS: Record<string, string> = {
  Draft: "Borrador",
  Generated: "Generada",
  UnderReview: "En Revision",
  Approved: "Aprobada",
  Published: "Publicada",
};

const MINUTES_STATUS_COLORS: Record<string, string> = {
  Draft: "bg-muted text-muted-foreground",
  Generated: "bg-blue-100 dark:bg-blue-950/30 text-blue-700 dark:text-blue-400",
  UnderReview: "bg-yellow-100 text-yellow-700",
  Approved: "bg-emerald-100 text-emerald-700",
  Published: "bg-emerald-200 text-emerald-800",
};

export default function MinutesPage() {
  const router = useRouter();
  const params = useParams();
  const assemblyId = params.id as string;

  const [assembly, setAssembly] = useState<AssemblyDetail | null>(null);
  const [loading, setLoading] = useState(true);
  const [actionLoading, setActionLoading] = useState(false);

  const [minutesPresident, setMinutesPresident] = useState("");
  const [minutesSecretary, setMinutesSecretary] = useState("");
  const [minutesCommissionMembers, setMinutesCommissionMembers] = useState("");

  const [commissionDeadline, setCommissionDeadline] = useState("");
  const [commissionComments, setCommissionComments] = useState("");
  const [revisionNotes, setRevisionNotes] = useState("");

  const [presidentSignatureFile, setPresidentSignatureFile] = useState<File | null>(null);
  const [secretarySignatureFile, setSecretarySignatureFile] = useState<File | null>(null);

  const fetchAssembly = useCallback(async () => {
    try {
      setLoading(true);
      const data = await assemblyService.getAssemblyById(assemblyId);
      setAssembly(data);
      if (data.minutes) {
        setCommissionComments(data.minutes.commissionComments || "");
        setCommissionDeadline(
          data.minutes.commissionReviewDeadline
            ? data.minutes.commissionReviewDeadline.substring(0, 10)
            : ""
        );
      }
    } catch (error) {
      console.error("Error al cargar asamblea:", error);
    } finally {
      setLoading(false);
    }
  }, [assemblyId]);

  useEffect(() => {
    fetchAssembly();
  }, [fetchAssembly]);

  const handleGenerateMinutes = async () => {
    if (!assembly) return;
    try {
      setActionLoading(true);
      const request: GenerateMinutesRequest = {
        presidentName: minutesPresident || undefined,
        secretaryName: minutesSecretary || undefined,
        commissionMemberNames: minutesCommissionMembers || undefined,
      };
      await assemblyService.generateMinutes(assembly.id, request);
      await fetchAssembly();
    } catch (error) {
      console.error("Error al generar acta:", error);
    } finally {
      setActionLoading(false);
    }
  };

  const handleSendToReview = async () => {
    if (!assembly) return;
    try {
      setActionLoading(true);
      const request: ApproveMinutesRequest = {
        commissionComments: commissionComments || undefined,
      };
      await assemblyService.approveMinutes(assembly.id, request);
      await fetchAssembly();
    } catch (error) {
      console.error("Error al enviar a revision:", error);
    } finally {
      setActionLoading(false);
    }
  };

  const handleApproveMinutes = async () => {
    if (!assembly) return;
    try {
      setActionLoading(true);
      const request: ApproveMinutesRequest = {
        commissionComments: commissionComments || undefined,
      };
      await assemblyService.approveMinutes(assembly.id, request);
      await fetchAssembly();
    } catch (error) {
      console.error("Error al aprobar acta:", error);
    } finally {
      setActionLoading(false);
    }
  };

  const handlePublishMinutes = async () => {
    if (!assembly) return;
    try {
      setActionLoading(true);
      await assemblyService.publishMinutes(assembly.id);
      await fetchAssembly();
    } catch (error) {
      console.error("Error al publicar acta:", error);
    } finally {
      setActionLoading(false);
    }
  };

  const formatDateTime = (dateStr: string) => {
    try {
      return new Date(dateStr).toLocaleString('es-CO', { day: '2-digit', month: '2-digit', year: 'numeric', hour: '2-digit', minute: '2-digit' });
    } catch {
      return dateStr;
    }
  };

  if (loading) {
    return (
      <main className="flex items-center justify-center">
        <div className="text-center">
          <div className="animate-spin rounded-full h-12 w-12 border-b-2 border-emerald-600 mx-auto mb-4"></div>
          <p className="text-muted-foreground text-sm font-medium">Cargando acta...</p>
        </div>
      </main>
    );
  }

  if (!assembly) {
    return (
      <main className="flex items-center justify-center">
        <div className="text-center">
          <p className="text-muted-foreground text-lg">Asamblea no encontrada</p>
          <button
            onClick={() => router.push("/assembly")}
            className="mt-4 text-emerald-600 hover:text-emerald-700 font-medium"
          >
            Volver a la lista
          </button>
        </div>
      </main>
    );
  }

  const minutes = assembly.minutes;
  const hasMinutes = minutes !== null;
  const isDraft = minutes?.status === "Draft" || minutes?.status === "Generated";
  const isUnderReview = minutes?.status === "UnderReview";
  const isApproved = minutes?.status === "Approved";
  const isPublished = minutes?.status === "Published";

  return (
    <main>
        <div className="max-w-7xl mx-auto px-6 py-8">
          {/* Header */}
          <div className="mb-8">
            <div className="flex items-center gap-3 mb-2">
              <button
                onClick={() => router.push(`/assembly/${assemblyId}`)}
                className="text-muted-foreground hover:text-muted-foreground"
              >
                &larr; Volver
              </button>
            </div>
            <div className="flex items-start justify-between">
              <div>
                <h1 className="text-2xl font-bold text-foreground">
                  Acta de Asamblea - {assembly.title}
                </h1>
                <p className="text-sm text-muted-foreground mt-1">{assembly.description}</p>
              </div>
              {hasMinutes && (
                <span
                  className={`px-3 py-1 rounded-full text-xs font-semibold ${MINUTES_STATUS_COLORS[minutes.status] || "bg-muted text-muted-foreground"}`}
                >
                  {MINUTES_STATUS_LABELS[minutes.status] || minutes.status}
                </span>
              )}
            </div>
          </div>

          {/* No Minutes - Generation Form */}
          {!hasMinutes && (
            <div className="bg-card rounded-xl shadow-sm border border-border p-6">
              <h2 className="text-lg font-semibold text-foreground mb-2">Generar Acta</h2>
              <p className="text-sm text-muted-foreground mb-6">
                No se ha generado el acta para esta asamblea. Complete los datos y genere el acta.
              </p>

              <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
                <div>
                  <label className="block text-xs font-medium text-muted-foreground mb-1">
                    Nombre del Presidente
                  </label>
                  <input
                    type="text"
                    value={minutesPresident}
                    onChange={(e) => setMinutesPresident(e.target.value)}
                    className="w-full px-3 py-2 border border-border rounded-lg text-sm focus:outline-none focus:ring-2 focus:ring-emerald-500 focus:border-emerald-500"
                    placeholder="Nombre completo del presidente"
                  />
                </div>
                <div>
                  <label className="block text-xs font-medium text-muted-foreground mb-1">
                    Nombre del Secretario
                  </label>
                  <input
                    type="text"
                    value={minutesSecretary}
                    onChange={(e) => setMinutesSecretary(e.target.value)}
                    className="w-full px-3 py-2 border border-border rounded-lg text-sm focus:outline-none focus:ring-2 focus:ring-emerald-500 focus:border-emerald-500"
                    placeholder="Nombre completo del secretario"
                  />
                </div>
                <div className="md:col-span-2">
                  <label className="block text-xs font-medium text-muted-foreground mb-1">
                    Miembros de Comision (separados por coma)
                  </label>
                  <input
                    type="text"
                    value={minutesCommissionMembers}
                    onChange={(e) => setMinutesCommissionMembers(e.target.value)}
                    className="w-full px-3 py-2 border border-border rounded-lg text-sm focus:outline-none focus:ring-2 focus:ring-emerald-500 focus:border-emerald-500"
                    placeholder="Nombre 1, Nombre 2, Nombre 3"
                  />
                </div>
              </div>

              <div className="mt-6">
                <button
                  onClick={handleGenerateMinutes}
                  disabled={actionLoading}
                  className="px-6 py-2.5 bg-emerald-600 text-white text-sm font-medium rounded-lg hover:bg-emerald-700 disabled:opacity-50 transition-colors"
                >
                  {actionLoading ? (
                    <span className="flex items-center gap-2">
                      <span className="animate-spin h-4 w-4 border-2 border-white border-t-transparent rounded-full"></span>
                      Generando...
                    </span>
                  ) : (
                    "Generar Acta"
                  )}
                </button>
              </div>
            </div>
          )}

          {/* Minutes Exist */}
          {hasMinutes && minutes && (
            <div className="space-y-6">
              {/* Minutes Metadata */}
              <div className="bg-card rounded-xl shadow-sm border border-border p-6">
                <h2 className="text-lg font-semibold text-foreground mb-4">Informacion del Acta</h2>
                <div className="grid grid-cols-2 md:grid-cols-4 gap-4">
                  <div>
                    <label className="block text-xs font-medium text-muted-foreground uppercase tracking-wider mb-1">
                      Generada
                    </label>
                    <p className="text-sm font-semibold text-foreground">
                      {formatDateTime(minutes.generatedAt)}
                    </p>
                  </div>
                  <div>
                    <label className="block text-xs font-medium text-muted-foreground uppercase tracking-wider mb-1">
                      Presidente
                    </label>
                    <p className="text-sm font-semibold text-foreground">
                      {minutes.presidentName || "\u2014"}
                    </p>
                  </div>
                  <div>
                    <label className="block text-xs font-medium text-muted-foreground uppercase tracking-wider mb-1">
                      Secretario
                    </label>
                    <p className="text-sm font-semibold text-foreground">
                      {minutes.secretaryName || "\u2014"}
                    </p>
                  </div>
                  {minutes.commissionMemberNames && (
                    <div>
                      <label className="block text-xs font-medium text-muted-foreground uppercase tracking-wider mb-1">
                        Miembros de Comision
                      </label>
                      <p className="text-sm font-semibold text-foreground">
                        {minutes.commissionMemberNames}
                      </p>
                    </div>
                  )}
                  {minutes.approvedAt && (
                    <div>
                      <label className="block text-xs font-medium text-muted-foreground uppercase tracking-wider mb-1">
                        Aprobada
                      </label>
                      <p className="text-sm font-semibold text-foreground">
                        {formatDateTime(minutes.approvedAt)}
                      </p>
                    </div>
                  )}
                </div>
              </div>

              {/* Full Text */}
              <div className="bg-card rounded-xl shadow-sm border border-border p-6">
                <h2 className="text-lg font-semibold text-foreground mb-4">Texto Completo del Acta</h2>
                <div className="max-h-[500px] overflow-y-auto border border-border rounded-lg p-4 bg-muted/50">
                  <pre className="text-sm text-foreground whitespace-pre-wrap font-mono leading-relaxed">
                    {minutes.fullText}
                  </pre>
                </div>
              </div>

              {/* Commission Review Section */}
              {(isDraft || isUnderReview) && (
                <div className="bg-card rounded-xl shadow-sm border border-border p-6">
                  <h2 className="text-lg font-semibold text-foreground mb-4">
                    Revision de Comision
                  </h2>
                  <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
                    <div>
                      <label className="block text-xs font-medium text-muted-foreground mb-1">
                        Fecha Limite de Revision
                      </label>
                      <input
                        type="date"
                        value={commissionDeadline}
                        onChange={(e) => setCommissionDeadline(e.target.value)}
                        className="w-full px-3 py-2 border border-border rounded-lg text-sm focus:outline-none focus:ring-2 focus:ring-emerald-500 focus:border-emerald-500"
                      />
                    </div>
                    <div>
                      <label className="block text-xs font-medium text-muted-foreground mb-1">
                        Notas de Revision
                      </label>
                      <input
                        type="text"
                        value={revisionNotes}
                        onChange={(e) => setRevisionNotes(e.target.value)}
                        className="w-full px-3 py-2 border border-border rounded-lg text-sm focus:outline-none focus:ring-2 focus:ring-emerald-500 focus:border-emerald-500"
                        placeholder="Observaciones de la comision"
                      />
                    </div>
                    <div className="md:col-span-2">
                      <label className="block text-xs font-medium text-muted-foreground mb-1">
                        Comentarios
                      </label>
                      <textarea
                        value={commissionComments}
                        onChange={(e) => setCommissionComments(e.target.value)}
                        className="w-full px-3 py-2 border border-border rounded-lg text-sm focus:outline-none focus:ring-2 focus:ring-emerald-500 focus:border-emerald-500 resize-none"
                        rows={4}
                        placeholder="Comentarios de la comision sobre el acta"
                      ></textarea>
                    </div>
                  </div>
                </div>
              )}

              {/* Signature Fields */}
              {(isDraft || isUnderReview || isApproved) && (
                <div className="bg-card rounded-xl shadow-sm border border-border p-6">
                  <h2 className="text-lg font-semibold text-foreground mb-4">Firmas</h2>
                  <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
                    <div>
                      <label className="block text-xs font-medium text-muted-foreground mb-1">
                        Firma del Presidente
                      </label>
                      <div className="border-2 border-dashed border-border rounded-lg p-6 text-center hover:border-emerald-400 transition-colors cursor-pointer">
                        <input
                          type="file"
                          accept="image/*"
                          className="hidden"
                          id="president-signature"
                          onChange={(e) =>
                            setPresidentSignatureFile(e.target.files?.[0] || null)
                          }
                        />
                        <label
                          htmlFor="president-signature"
                          className="cursor-pointer"
                        >
                          {presidentSignatureFile ? (
                            <p className="text-sm text-emerald-600 font-medium">
                              {presidentSignatureFile.name}
                            </p>
                          ) : (
                            <>
                              <svg
                                className="mx-auto h-8 w-8 text-muted-foreground mb-2"
                                fill="none"
                                viewBox="0 0 24 24"
                                stroke="currentColor"
                              >
                                <path
                                  strokeLinecap="round"
                                  strokeLinejoin="round"
                                  strokeWidth={1.5}
                                  d="M7 16a4 4 0 01-.88-7.903A5 5 0 1115.9 6L16 6a5 5 0 011 9.9M15 13l-3-3m0 0l-3 3m3-3v12"
                                />
                              </svg>
                              <p className="text-xs text-muted-foreground">
                                Arrastre o seleccione un archivo
                              </p>
                            </>
                          )}
                        </label>
                      </div>
                    </div>
                    <div>
                      <label className="block text-xs font-medium text-muted-foreground mb-1">
                        Firma del Secretario
                      </label>
                      <div className="border-2 border-dashed border-border rounded-lg p-6 text-center hover:border-emerald-400 transition-colors cursor-pointer">
                        <input
                          type="file"
                          accept="image/*"
                          className="hidden"
                          id="secretary-signature"
                          onChange={(e) =>
                            setSecretarySignatureFile(e.target.files?.[0] || null)
                          }
                        />
                        <label
                          htmlFor="secretary-signature"
                          className="cursor-pointer"
                        >
                          {secretarySignatureFile ? (
                            <p className="text-sm text-emerald-600 font-medium">
                              {secretarySignatureFile.name}
                            </p>
                          ) : (
                            <>
                              <svg
                                className="mx-auto h-8 w-8 text-muted-foreground mb-2"
                                fill="none"
                                viewBox="0 0 24 24"
                                stroke="currentColor"
                              >
                                <path
                                  strokeLinecap="round"
                                  strokeLinejoin="round"
                                  strokeWidth={1.5}
                                  d="M7 16a4 4 0 01-.88-7.903A5 5 0 1115.9 6L16 6a5 5 0 011 9.9M15 13l-3-3m0 0l-3 3m3-3v12"
                                />
                              </svg>
                              <p className="text-xs text-muted-foreground">
                                Arrastre o seleccione un archivo
                              </p>
                            </>
                          )}
                        </label>
                      </div>
                    </div>
                  </div>
                </div>
              )}

              {/* Published Info */}
              {isPublished && (
                <div className="bg-card rounded-xl shadow-sm border border-border p-6">
                  <h2 className="text-lg font-semibold text-foreground mb-4">Publicacion</h2>
                  <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
                    <div>
                      <label className="block text-xs font-medium text-muted-foreground uppercase tracking-wider mb-1">
                        Fecha de Publicacion
                      </label>
                      <p className="text-sm font-semibold text-foreground">
                        {minutes.publishedAt ? formatDateTime(minutes.publishedAt) : "\u2014"}
                      </p>
                    </div>
                    <div>
                      <label className="block text-xs font-medium text-muted-foreground uppercase tracking-wider mb-1">
                        Notificaciones Enviadas
                      </label>
                      <p className="text-sm font-semibold text-foreground">
                        {minutes.publishNotificationCount !== null
                          ? minutes.publishNotificationCount
                          : "0"}
                      </p>
                    </div>
                  </div>
                </div>
              )}

              {/* Action Buttons */}
              <div className="bg-card rounded-xl shadow-sm border border-border p-6">
                <h2 className="text-lg font-semibold text-foreground mb-4">Acciones</h2>
                <div className="flex flex-wrap gap-3">
                  {isDraft && (
                    <>
                      <button
                        onClick={handleSendToReview}
                        disabled={actionLoading}
                        className="px-5 py-2.5 bg-yellow-500 text-white text-sm font-medium rounded-lg hover:bg-yellow-600 disabled:opacity-50 transition-colors"
                      >
                        {actionLoading ? (
                          <span className="flex items-center gap-2">
                            <span className="animate-spin h-4 w-4 border-2 border-white border-t-transparent rounded-full"></span>
                            Enviando...
                          </span>
                        ) : (
                          "Enviar a Revision"
                        )}
                      </button>
                      <button
                        onClick={handleApproveMinutes}
                        disabled={actionLoading}
                        className="px-5 py-2.5 bg-emerald-600 text-white text-sm font-medium rounded-lg hover:bg-emerald-700 disabled:opacity-50 transition-colors"
                      >
                        {actionLoading ? (
                          <span className="flex items-center gap-2">
                            <span className="animate-spin h-4 w-4 border-2 border-white border-t-transparent rounded-full"></span>
                            Aprobando...
                          </span>
                        ) : (
                          "Aprobar"
                        )}
                      </button>
                    </>
                  )}
                  {isApproved && (
                    <button
                      onClick={handlePublishMinutes}
                      disabled={actionLoading}
                      className="px-5 py-2.5 bg-emerald-600 text-white text-sm font-medium rounded-lg hover:bg-emerald-700 disabled:opacity-50 transition-colors"
                    >
                      {actionLoading ? (
                        <span className="flex items-center gap-2">
                          <span className="animate-spin h-4 w-4 border-2 border-white border-t-transparent rounded-full"></span>
                          Publicando...
                        </span>
                      ) : (
                        "Publicar"
                      )}
                    </button>
                  )}
                  {isPublished && (
                    <span className="px-5 py-2.5 bg-emerald-100 text-emerald-700 text-sm font-medium rounded-lg">
                      Acta publicada correctamente
                    </span>
                  )}
                </div>
              </div>
            </div>
          )}
        </div>
    </main>
  );
}
