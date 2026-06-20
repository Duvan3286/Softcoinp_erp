"use client";

import React, { useState } from "react";
import {
  ResidentsService,
  OwnerType,
  DocumentType,
  CreateNaturalPersonOwnerPayload,
  CreateLegalEntityOwnerPayload,
} from "@/lib/residents-service";
import { useRouter } from "next/navigation";
import Link from "next/link";
import { Users, Building2, FileCheck } from "lucide-react";

const inputClass =
  "w-full px-4 py-2.5 bg-gray-50 border border-gray-200 rounded-xl focus:ring-2 focus:ring-blue-500/20 focus:border-blue-500 focus:bg-white text-sm text-gray-900 transition-all outline-none";
const labelClass = "block text-xs font-bold text-gray-700 uppercase tracking-wide mb-2";
const readonlyClass =
  "w-full px-4 py-2.5 bg-gray-100 border border-gray-200 rounded-xl text-sm text-gray-500 font-bold text-center cursor-not-allowed outline-none";

// ── Helpers ──────────────────────────────────────────────────────────────────

const calculateDV = (nitVal: string): string => {
  if (nitVal.length === 0) return "";
  const vpri = [3, 7, 13, 17, 19, 23, 29, 37, 41, 43, 47, 53, 59, 67, 71];
  let x = 0;
  let y = 0;
  const z = nitVal.length;
  for (let i = 0; i < z; i++) {
    y = parseInt(nitVal.charAt(i));
    x += y * vpri[z - 1 - i];
  }
  y = x % 11;
  return y > 1 ? (11 - y).toString() : y.toString();
};

const toTitleCase = (val: string): string =>
  val.toLowerCase().replace(/(?:^|\s)\S/g, (a) => a.toUpperCase());

const sanitizePhone = (val: string): string =>
  val.replace(/[^0-9\-\+\s]/g, "").slice(0, 20);

const sanitizeDocNumber = (val: string, dt: DocumentType): string => {
  const isNumeric = dt === DocumentType.CitizenshipCard || dt === DocumentType.NIT;
  const maxLen = isNumeric ? 10 : 50;
  return isNumeric
    ? val.replace(/\D/g, "").slice(0, maxLen)
    : val.replace(/[^a-zA-Z0-9]/g, "").slice(0, maxLen);
};

// ── Component ─────────────────────────────────────────────────────────────────

export default function NewOwnerPage() {
  const router = useRouter();
  const [submitting, setSubmitting] = useState(false);
  const [error, setError] = useState("");

  const [ownerType, setOwnerType] = useState<OwnerType>(OwnerType.NaturalPerson);

  // Natural person fields
  const [docType, setDocType] = useState<DocumentType>(DocumentType.CitizenshipCard);
  const [docNumber, setDocNumber] = useState("");
  const [fullName, setFullName] = useState("");
  const [email, setEmail] = useState("");
  const [mainPhone, setMainPhone] = useState("");
  const [alternativePhone, setAlternativePhone] = useState("");
  const [correspondenceAddress, setCorrespondenceAddress] = useState("");
  const [dateOfBirth, setDateOfBirth] = useState("");
  const [civilStatus, setCivilStatus] = useState("");

  // Legal entity fields
  const [nit, setNit] = useState("");
  const [dv, setDv] = useState("");
  const [companyName, setCompanyName] = useState("");
  const [companyEmail, setCompanyEmail] = useState("");
  const [companyPhone, setCompanyPhone] = useState("");
  const [companyAltPhone, setCompanyAltPhone] = useState("");
  const [fiscalAddress, setFiscalAddress] = useState("");
  const [legalRepName, setLegalRepName] = useState("");
  const [legalRepDocType, setLegalRepDocType] = useState<DocumentType>(DocumentType.CitizenshipCard);
  const [legalRepDoc, setLegalRepDoc] = useState("");
  const [legalRepRole, setLegalRepRole] = useState("Gerente General");
  const [powExpiration, setPowExpiration] = useState("");

  const handleChangeOwnerType = (type: OwnerType) => {
    setOwnerType(type);
    setError("");
  };

  const handleDocTypeChange = (dt: DocumentType) => {
    setDocType(dt);
    setDocNumber("");
  };

  const handleNitChange = (val: string) => {
    const cleaned = val.replace(/\D/g, "").slice(0, 10);
    setNit(cleaned);
    setDv(calculateDV(cleaned));
  };

  const handleLegalRepDocTypeChange = (dt: DocumentType) => {
    setLegalRepDocType(dt);
    setLegalRepDoc("");
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setSubmitting(true);
    setError("");

    try {
      let newId = "";
      if (ownerType === OwnerType.NaturalPerson) {
        const payload: CreateNaturalPersonOwnerPayload = {
          documentType: docType,
          documentNumber: docNumber,
          fullName: fullName,
          email: email,
          mainPhone: mainPhone,
          alternativePhone: alternativePhone || undefined,
          correspondenceAddress: correspondenceAddress || undefined,
          dateOfBirth: dateOfBirth || undefined,
          civilStatus: civilStatus || undefined,
        };
        const result = await ResidentsService.createNaturalPersonOwner(payload);
        newId = result.id;
      } else {
        const payload: CreateLegalEntityOwnerPayload = {
          documentNumber: nit,
          verificationDigit: dv,
          companyName: companyName,
          email: companyEmail,
          mainPhone: companyPhone,
          alternativePhone: companyAltPhone || undefined,
          fiscalAddress: fiscalAddress || undefined,
          legalRepresentativeName: legalRepName,
          legalRepresentativeDocumentType: legalRepDocType,
          legalRepresentativeDocument: legalRepDoc,
          legalRepresentativeRole: legalRepRole,
          powerOfAttorneyExpiration: powExpiration || undefined,
        };
        const result = await ResidentsService.createLegalEntityOwner(payload);
        newId = result.id;
      }
      router.push(`/residents/${newId}`);
    } catch (err: any) {
      const msg =
        err?.response?.data?.message ||
        err?.response?.data?.errors?.[Object.keys(err?.response?.data?.errors ?? {})[0]]?.[0] ||
        "Ocurrió un error al registrar el propietario.";
      setError(msg);
    } finally {
      setSubmitting(false);
    }
  };

  const isNatural = ownerType === OwnerType.NaturalPerson;

  return (
    <div className="max-w-3xl mx-auto space-y-6">
      <div className="flex items-center gap-4">
        <Link
          href="/residents"
          className="w-10 h-10 flex items-center justify-center bg-white border border-gray-200 rounded-xl hover:bg-gray-50 transition-colors shadow-sm text-gray-500 font-bold"
        >
          ←
        </Link>
        <div>
          <h1 className="text-2xl font-bold text-gray-900 tracking-tight">
            Registrar Nuevo Propietario
          </h1>
          <p className="text-sm text-gray-500 mt-0.5">
            Completa los datos del propietario para vincularlo al conjunto.
          </p>
        </div>
      </div>

      <form onSubmit={handleSubmit} className="space-y-6">
        {/* Tipo */}
        <div className="bg-white rounded-2xl shadow-sm border border-gray-100 p-6">
          <h3 className="text-base font-bold text-gray-800 mb-4">Tipo de Propietario</h3>
          <div className="grid grid-cols-1 sm:grid-cols-2 gap-4">
            <button
              type="button"
              onClick={() => handleChangeOwnerType(OwnerType.NaturalPerson)}
              className={`relative flex items-start p-4 rounded-xl border-2 transition-all text-left ${
                isNatural
                  ? "border-blue-600 bg-blue-50/50"
                  : "border-gray-200 hover:border-gray-300 bg-white"
              }`}
            >
              <Users
                className={`w-8 h-8 mr-4 shrink-0 ${isNatural ? "text-blue-600" : "text-gray-400"}`}
              />
              <div>
                <span
                  className={`block font-bold text-sm ${isNatural ? "text-blue-900" : "text-gray-900"}`}
                >
                  Persona Natural
                </span>
                <span className="block text-xs text-gray-500 mt-1">
                  Ciudadano nacional o extranjero.
                </span>
              </div>
            </button>

            <button
              type="button"
              onClick={() => handleChangeOwnerType(OwnerType.LegalEntity)}
              className={`relative flex items-start p-4 rounded-xl border-2 transition-all text-left ${
                !isNatural
                  ? "border-indigo-600 bg-indigo-50/50"
                  : "border-gray-200 hover:border-gray-300 bg-white"
              }`}
            >
              <Building2
                className={`w-8 h-8 mr-4 shrink-0 ${!isNatural ? "text-indigo-600" : "text-gray-400"}`}
              />
              <div>
                <span
                  className={`block font-bold text-sm ${!isNatural ? "text-indigo-900" : "text-gray-900"}`}
                >
                  Persona Jurídica
                </span>
                <span className="block text-xs text-gray-500 mt-1">
                  Empresa, fideicomiso o sociedad.
                </span>
              </div>
            </button>
          </div>
        </div>

        {/* Natural Person Form */}
        {isNatural && (
          <>
            <div className="bg-white rounded-2xl shadow-sm border border-gray-100 p-6 space-y-5">
              <h3 className="text-base font-bold text-gray-800">Identificación</h3>
              <div className="grid grid-cols-1 md:grid-cols-12 gap-5">
                <div className="col-span-12 md:col-span-5">
                  <label className={labelClass}>Tipo de Documento</label>
                  <select
                    value={docType}
                    onChange={(e) => handleDocTypeChange(Number(e.target.value) as DocumentType)}
                    className={inputClass}
                  >
                    <option value={DocumentType.CitizenshipCard}>Cédula de Ciudadanía (CC)</option>
                    <option value={DocumentType.ForeignerID}>Cédula de Extranjería (CE)</option>
                    <option value={DocumentType.Passport}>Pasaporte</option>
                    <option value={DocumentType.PEP}>Persona Expuesta Políticamente (PEP)</option>
                    <option value={DocumentType.PPT}>Pasaporte Temporal (PPT)</option>
                  </select>
                </div>
                <div className="col-span-12 md:col-span-7">
                  <label className={labelClass}>Número de Documento *</label>
                  <input
                    type="text"
                    inputMode={
                      docType === DocumentType.CitizenshipCard ? "numeric" : "text"
                    }
                    required
                    value={docNumber}
                    onChange={(e) => setDocNumber(sanitizeDocNumber(e.target.value, docType))}
                    className={inputClass}
                    placeholder={
                      docType === DocumentType.CitizenshipCard ? "Ej: 1020304050" : "Ej: AB123456"
                    }
                  />
                </div>
                <div className="col-span-12">
                  <label className={labelClass}>Nombre Completo *</label>
                  <input
                    type="text"
                    required
                    maxLength={200}
                    value={fullName}
                    onChange={(e) => setFullName(toTitleCase(e.target.value))}
                    className={inputClass}
                    placeholder="Ej: Juan Carlos Pérez Gómez"
                  />
                </div>
              </div>
            </div>

            <div className="bg-white rounded-2xl shadow-sm border border-gray-100 p-6 space-y-5">
              <h3 className="text-base font-bold text-gray-800">Datos Personales</h3>
              <div className="grid grid-cols-1 md:grid-cols-2 gap-5">
                <div>
                  <label className={labelClass}>Fecha de Nacimiento</label>
                  <input
                    type="date"
                    value={dateOfBirth}
                    onChange={(e) => setDateOfBirth(e.target.value)}
                    max={new Date().toISOString().split("T")[0]}
                    className={inputClass}
                  />
                </div>
                <div>
                  <label className={labelClass}>Estado Civil</label>
                  <select
                    value={civilStatus}
                    onChange={(e) => setCivilStatus(e.target.value)}
                    className={inputClass}
                  >
                    <option value="">Seleccione...</option>
                    <option value="Soltero/a">Soltero/a</option>
                    <option value="Casado/a">Casado/a</option>
                    <option value="Unión Libre">Unión Libre</option>
                    <option value="Divorciado/a">Divorciado/a</option>
                    <option value="Viudo/a">Viudo/a</option>
                  </select>
                </div>
              </div>
            </div>

            <div className="bg-white rounded-2xl shadow-sm border border-gray-100 p-6 space-y-5">
              <h3 className="text-base font-bold text-gray-800">Contacto</h3>
              <div className="grid grid-cols-1 md:grid-cols-2 gap-5">
                <div className="md:col-span-2">
                  <label className={labelClass}>Correo Electrónico *</label>
                  <input
                    type="email"
                    required
                    maxLength={256}
                    value={email}
                    onChange={(e) => setEmail(e.target.value)}
                    className={inputClass}
                    placeholder="propietario@ejemplo.com"
                  />
                </div>
                <div>
                  <label className={labelClass}>Teléfono Principal *</label>
                  <input
                    type="tel"
                    required
                    value={mainPhone}
                    onChange={(e) => setMainPhone(sanitizePhone(e.target.value))}
                    className={inputClass}
                    placeholder="3001234567"
                  />
                </div>
                <div>
                  <label className={labelClass}>Teléfono Alternativo</label>
                  <input
                    type="tel"
                    value={alternativePhone}
                    onChange={(e) => setAlternativePhone(sanitizePhone(e.target.value))}
                    className={inputClass}
                    placeholder="6011234567"
                  />
                </div>
                <div className="md:col-span-2">
                  <label className={labelClass}>Dirección de Correspondencia Externa</label>
                  <input
                    type="text"
                    maxLength={200}
                    value={correspondenceAddress}
                    onChange={(e) => setCorrespondenceAddress(toTitleCase(e.target.value))}
                    className={inputClass}
                    placeholder="Si no reside en el conjunto, dirección para notificaciones"
                  />
                </div>
              </div>
            </div>
          </>
        )}

        {/* Legal Entity Form */}
        {!isNatural && (
          <>
            <div className="bg-white rounded-2xl shadow-sm border border-gray-100 p-6 space-y-5">
              <h3 className="text-base font-bold text-gray-800">Datos de la Empresa</h3>
              <div className="grid grid-cols-1 md:grid-cols-12 gap-5">
                <div className="col-span-12 md:col-span-9">
                  <label className={labelClass}>NIT (Sin DV) *</label>
                  <input
                    type="text"
                    inputMode="numeric"
                    required
                    value={nit}
                    onChange={(e) => handleNitChange(e.target.value)}
                    className={inputClass}
                    placeholder="Ej: 900123456"
                  />
                </div>
                <div className="col-span-12 md:col-span-3">
                  <label className={labelClass}>DV</label>
                  <input
                    type="text"
                    value={dv}
                    readOnly
                    className={readonlyClass}
                  />
                </div>
                <div className="col-span-12">
                  <label className={labelClass}>Razón Social *</label>
                  <input
                    type="text"
                    required
                    maxLength={200}
                    value={companyName}
                    onChange={(e) => setCompanyName(toTitleCase(e.target.value))}
                    className={inputClass}
                    placeholder="Ej: Inversiones Abc S.A.S."
                  />
                </div>
                <div className="col-span-12 md:col-span-6">
                  <label className={labelClass}>Correo Corporativo *</label>
                  <input
                    type="email"
                    required
                    maxLength={256}
                    value={companyEmail}
                    onChange={(e) => setCompanyEmail(e.target.value)}
                    className={inputClass}
                  />
                </div>
                <div className="col-span-12 md:col-span-6">
                  <label className={labelClass}>Teléfono *</label>
                  <input
                    type="tel"
                    required
                    value={companyPhone}
                    onChange={(e) => setCompanyPhone(sanitizePhone(e.target.value))}
                    className={inputClass}
                  />
                </div>
                <div className="col-span-12 md:col-span-6">
                  <label className={labelClass}>Teléfono Alternativo</label>
                  <input
                    type="tel"
                    value={companyAltPhone}
                    onChange={(e) => setCompanyAltPhone(sanitizePhone(e.target.value))}
                    className={inputClass}
                  />
                </div>
                <div className="col-span-12 md:col-span-6">
                  <label className={labelClass}>Dirección Fiscal</label>
                  <input
                    type="text"
                    maxLength={200}
                    value={fiscalAddress}
                    onChange={(e) => setFiscalAddress(toTitleCase(e.target.value))}
                    className={inputClass}
                  />
                </div>
              </div>
            </div>

            <div className="bg-white rounded-2xl shadow-sm border border-gray-100 p-6 space-y-5">
              <h3 className="text-base font-bold text-gray-800">Representante Legal</h3>
              <div className="grid grid-cols-1 md:grid-cols-2 gap-5">
                <div className="md:col-span-2">
                  <label className={labelClass}>Nombre Completo *</label>
                  <input
                    type="text"
                    required
                    maxLength={200}
                    value={legalRepName}
                    onChange={(e) => setLegalRepName(toTitleCase(e.target.value))}
                    className={inputClass}
                  />
                </div>
                <div>
                  <label className={labelClass}>Tipo de Documento *</label>
                  <select
                    value={legalRepDocType}
                    onChange={(e) => handleLegalRepDocTypeChange(Number(e.target.value) as DocumentType)}
                    className={inputClass}
                  >
                    <option value={DocumentType.CitizenshipCard}>Cédula de Ciudadanía (CC)</option>
                    <option value={DocumentType.ForeignerID}>Cédula de Extranjería (CE)</option>
                    <option value={DocumentType.Passport}>Pasaporte</option>
                  </select>
                </div>
                <div>
                  <label className={labelClass}>Número de Documento *</label>
                  <input
                    type="text"
                    inputMode={
                      legalRepDocType === DocumentType.CitizenshipCard ? "numeric" : "text"
                    }
                    required
                    value={legalRepDoc}
                    onChange={(e) =>
                      setLegalRepDoc(sanitizeDocNumber(e.target.value, legalRepDocType))
                    }
                    className={inputClass}
                  />
                </div>
                <div>
                  <label className={labelClass}>Cargo *</label>
                  <input
                    type="text"
                    required
                    maxLength={100}
                    value={legalRepRole}
                    onChange={(e) => setLegalRepRole(toTitleCase(e.target.value))}
                    className={inputClass}
                    placeholder="Ej: Gerente General"
                  />
                </div>
                <div>
                  <label className={labelClass}>Vencimiento de Poder</label>
                  <input
                    type="date"
                    value={powExpiration}
                    onChange={(e) => setPowExpiration(e.target.value)}
                    className={inputClass}
                  />
                </div>
              </div>
            </div>
          </>
        )}

        {/* Footer */}
        <div className="bg-white rounded-2xl shadow-sm border border-gray-100 p-5 flex flex-col sm:flex-row justify-between items-center gap-4">
          {error ? (
            <div className="text-sm font-semibold text-red-600 bg-red-50 border border-red-200 rounded-xl px-4 py-2 w-full sm:w-auto">
              {error}
            </div>
          ) : (
            <div className="flex items-center gap-2 text-sm text-gray-500">
              <FileCheck className="w-4 h-4" />
              Los datos podrán actualizarse después del registro.
            </div>
          )}

          <div className="flex gap-3 w-full sm:w-auto">
            <Link
              href="/residents"
              className="px-5 py-2.5 text-gray-700 font-semibold rounded-xl hover:bg-gray-100 transition-colors text-center flex-1 sm:flex-none"
            >
              Cancelar
            </Link>
            <button
              type="submit"
              disabled={submitting}
              className="px-6 py-2.5 bg-blue-600 hover:bg-blue-700 text-white font-bold rounded-xl shadow-sm shadow-blue-200 transition-colors flex-1 sm:flex-none flex items-center justify-center gap-2 disabled:opacity-50"
            >
              {submitting && (
                <div className="w-4 h-4 rounded-full border-2 border-white/30 border-t-white animate-spin" />
              )}
              Guardar Propietario
            </button>
          </div>
        </div>
      </form>
    </div>
  );
}
