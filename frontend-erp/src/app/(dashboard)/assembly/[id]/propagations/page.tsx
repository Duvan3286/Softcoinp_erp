"use client";

import { useState, useEffect, useCallback } from "react";
import { useRouter, useParams } from "next/navigation";
import assemblyService, { DecisionPropagationDto } from "@/lib/assembly-service";

export default function AssemblyPropagationsPage() {
  const router = useRouter();
  const params = useParams();
  const id = params.id as string;

  const [propagations, setPropagations] = useState<DecisionPropagationDto[]>([]);
  const [loading, setLoading] = useState(true);
  const [assemblyTitle, setAssemblyTitle] = useState("");

  const loadData = useCallback(async () => {
    try {
      const assembly = await assemblyService.getAssemblyById(id);
      setAssemblyTitle(assembly.title);
      const props = await assemblyService.getPropagations(id);
      setPropagations(props);
    } catch (error) {
      console.error("Error loading propagations:", error);
    } finally {
      setLoading(false);
    }
  }, [id]);

  useEffect(() => {
    loadData();
  }, [loadData]);

  const getStatusBadge = (status: string) => {
    const base = "px-2 py-1 rounded-full text-xs font-medium";
    if (status === "Propagated") {
      return base + " bg-emerald-100 dark:bg-emerald-950/30 text-emerald-700 dark:text-emerald-400";
    }
    if (status === "Failed") {
      return base + " bg-rose-100 dark:bg-rose-950/30 text-rose-700 dark:text-rose-400";
    }
    return base + " bg-yellow-100 text-yellow-800";
  };

  const getStatusLabel = (status: string) => {
    if (status === "Propagated") return "Propagada";
    if (status === "Failed") return "Fallida";
    return "Pendiente";
  };

  const getModuleLabel = (module: string) => {
    const labels: Record<string, string> = {
      Budget: "Presupuesto",
      ExtraordinaryFee: "Cuota Extraordinaria",
      AuthRoles: "Roles y Permisos",
      Contract: "Contrato",
      Other: "Otro",
    };
    return labels[module] || module;
  };

  if (loading) {
    return (
      <main className="p-8">
        <div className="flex items-center justify-center h-64">
          <p className="text-muted-foreground">Cargando decisiones...</p>
        </div>
      </main>
    );
  }

  return (
    <main className="p-8">
        <div className="flex items-center justify-between mb-6">
          <div>
            <h1 className="text-2xl font-bold text-foreground">Propagación de Decisiones</h1>
            <p className="text-sm text-muted-foreground mt-1">{assemblyTitle}</p>
          </div>
          <button
            onClick={() => router.push(`/assembly/${id}`)}
            className="px-4 py-2 text-sm font-medium text-muted-foreground hover:text-foreground"
          >
            Volver a la Asamblea
          </button>
        </div>

        {propagations.length === 0 ? (
          <div className="bg-card rounded-lg shadow p-8 text-center">
            <p className="text-muted-foreground">No hay decisiones registradas para propagar.</p>
          </div>
        ) : (
          <div className="bg-card rounded-lg shadow overflow-hidden">
            <table className="w-full">
              <thead className="bg-muted/50">
                <tr>
                  <th className="px-4 py-3 text-left text-xs font-medium text-muted-foreground uppercase">Punto</th>
                  <th className="px-4 py-3 text-left text-xs font-medium text-muted-foreground uppercase">Módulo Destino</th>
                  <th className="px-4 py-3 text-left text-xs font-medium text-muted-foreground uppercase">Descripción</th>
                  <th className="px-4 py-3 text-left text-xs font-medium text-muted-foreground uppercase">Estado</th>
                  <th className="px-4 py-3 text-left text-xs font-medium text-muted-foreground uppercase">Fecha</th>
                  <th className="px-4 py-3 text-left text-xs font-medium text-muted-foreground uppercase">Error</th>
                </tr>
              </thead>
              <tbody className="divide-y divide-border">
                {propagations.map((p) => (
                  <tr key={p.id} className="hover:bg-muted/30">
                    <td className="px-4 py-3 text-sm text-foreground">{p.agendaItemTitle}</td>
                    <td className="px-4 py-3 text-sm text-muted-foreground">{getModuleLabel(p.targetModule)}</td>
                    <td className="px-4 py-3 text-sm text-muted-foreground max-w-xs truncate">{p.description}</td>
                    <td className="px-4 py-3">
                      <span className={getStatusBadge(p.status)}>{getStatusLabel(p.status)}</span>
                    </td>
                    <td className="px-4 py-3 text-sm text-muted-foreground">
                      {p.propagatedAt
                        ? new Date(p.propagatedAt).toLocaleDateString("es-CO")
                        : new Date(p.createdAt).toLocaleDateString("es-CO")}
                    </td>
                    <td className="px-4 py-3 text-sm text-rose-600 dark:text-rose-400 max-w-xs truncate">
                      {p.errorMessage || "—"}
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        )}
    </main>
  );
}
