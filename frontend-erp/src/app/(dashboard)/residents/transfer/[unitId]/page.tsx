"use client";

import { useParams, useRouter } from "next/navigation";
import Link from "next/link";
import PropertyTransferWizard from "@/components/residents/PropertyTransferWizard";
import { ArrowLeft } from "lucide-react";

export default function PropertyTransferPage() {
  const params = useParams();
  const router = useRouter();

  const rawUnitId = params?.unitId;
  const unitId = Array.isArray(rawUnitId) ? rawUnitId[0] : rawUnitId ?? "";

  const handleClose = () => {
    router.back();
  };

  const handleSuccess = (id: string) => {
    router.push(`/units/${id}`);
  };

  return (
    <div className="space-y-6">
      <div className="flex items-center gap-4">
        <Link
          href={`/units/${unitId}`}
          className="flex items-center gap-2 text-sm text-muted-foreground hover:text-muted-foreground transition-colors"
        >
          <ArrowLeft className="w-4 h-4" />
          Volver a la Unidad
        </Link>
      </div>

      <div>
        <h1 className="text-2xl font-bold text-foreground tracking-tight">
          Transferencia de Propiedad
        </h1>
        <p className="text-sm text-muted-foreground mt-1">
          Registra formalmente el cambio de propietario ante el historial del conjunto.
        </p>
      </div>

      <div className="max-w-3xl">
        <PropertyTransferWizard
          unitId={unitId}
          onClose={handleClose}
          onSuccess={handleSuccess}
        />
      </div>
    </div>
  );
}
