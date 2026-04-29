'use client';

import React, { useState } from 'react';
import { useAuth } from '@/context/AuthContext';
import { ArrowRight, Database, Shield, AlertCircle, Loader2, CheckSquare, Square } from 'lucide-react';
import { Button } from '@/components/ui/Button';

export default function LoginPage() {
  const { login } = useAuth();
  const [email, setEmail] = useState("");
  const [password, setPassword] = useState("");
  const [acceptHabeasData, setAcceptHabeasData] = useState(false);
  const [error, setError] = useState("");
  const [isLoading, setIsLoading] = useState(false);
  const systemVersion = "V 2.0.26";

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setError("");

    if (!acceptHabeasData) {
      setError("Debes aceptar la política de tratamiento de datos personales.");
      return;
    }

    setIsLoading(true);
    try {
      await login({ email, password, acceptHabeasData });
    } catch (err: unknown) {
      if (err instanceof Error && 'response' in err) {
        const axiosError = err as { response: { data: { message: string } } };
        setError(axiosError.response?.data?.message || "Usuario o contraseña incorrectos");
      } else {
        setError("Ocurrió un error en el servidor. Intenta más tarde.");
      }
    } finally {
      setIsLoading(false);
    }
  };

  return (
    <div className="min-h-screen w-full flex bg-background selection:bg-emerald-600 selection:text-white transition-colors duration-300">
      {/* Lado izquierdo (2/3) - Decorativo Arquitectónico */}
      <div className="hidden lg:flex lg:w-2/3 relative flex-col justify-center overflow-hidden bg-background">
        
        {/* Grilla de líneas finas */}
        <div className="absolute inset-0 bg-[linear-gradient(to_right,#80808012_1px,transparent_1px),linear-gradient(to_bottom,#80808012_1px,transparent_1px)] bg-[size:40px_40px]"></div>
        
        {/* Polígonos angulares superpuestos con sutil brillo (Emerald) */}
        <div className="absolute inset-0 flex items-center justify-center opacity-30">
          <svg fill="none" viewBox="0 0 800 800" className="w-[120%] h-[120%] max-w-none text-emerald-600">
            <g className="origin-center" stroke="currentColor">
               <polygon points="100,700 300,100 700,300 500,800" strokeWidth="1" fill="none" className="drop-shadow-[0_0_15px_rgba(5,150,105,0.5)]" />
               <polygon points="200,600 400,200 800,400 600,900" strokeWidth="0.5" fill="none" className="drop-shadow-[0_0_10px_rgba(5,150,105,0.3)]" />
               <line x1="100" y1="700" x2="800" y2="400" strokeWidth="1" strokeDasharray="5,10" />
               <line x1="300" y1="100" x2="600" y2="900" strokeWidth="0.5" />
            </g>
          </svg>
        </div>

        {/* Borde derecho emitiendo el sutil brillo (Emerald) */}
        <div className="absolute right-0 top-0 bottom-0 w-[1px] bg-emerald-500/30 shadow-[0_0_20px_2px_rgba(16,185,129,0.4)] z-20"></div>

        <div className="relative z-10 px-24">
          <div className="w-16 h-16 border border-emerald-500/50 flex flex-col justify-center items-center shadow-[0_0_15px_rgba(16,185,129,0.2)] mb-10 bg-emerald-500/5 backdrop-blur-sm rounded-none">
            <Database className="w-8 h-8 text-emerald-500" strokeWidth={1.5} />
          </div>
          <h2 className="text-5xl font-black text-foreground uppercase tracking-tighter leading-[0.9] mb-6">
            Software De <br/>
            Control De <br/>
            <span className="text-emerald-600 drop-shadow-[0_0_10px_rgba(16,185,129,0.2)]">Ingresos</span>
          </h2>
          <p className="text-xs font-bold text-slate-500 max-w-sm uppercase tracking-[0.2em] leading-relaxed">
            Plataforma integral de gestión, auditoría y análisis de ingresos en tiempo real.
          </p>
        </div>
      </div>

      {/* Lado derecho (1/3) - Formulario */}
      <div className="w-full lg:w-1/3 flex flex-col bg-card relative z-30 shadow-[-20px_0_40px_rgba(0,0,0,0.05)] dark:shadow-[0_0_40px_rgba(0,0,0,0.5)] transition-colors duration-300">
        <div className="w-full max-w-[320px] mx-auto px-6 py-12 flex flex-col justify-center min-h-screen">
          
          <div className="mb-14">
            <Shield className="w-10 h-10 text-emerald-600 mb-6 drop-shadow-[0_0_8px_rgba(16,185,129,0.3)]" strokeWidth={1.5} />
            <h1 className="text-3xl font-black text-foreground uppercase tracking-tighter m-0 p-0 leading-none">
              SOFTCOINP
            </h1>
            <p className="text-emerald-600 font-bold text-sm tracking-[0.3em] mt-2 mb-6">
              {systemVersion}
            </p>
            <hr className="border-t-2 border-emerald-600 w-12 ml-0" />
          </div>

          <form onSubmit={handleSubmit} className="flex flex-col gap-6">
            {error && (
              <div className="bg-rose-50/50 dark:bg-rose-950/20 border-l-2 border-rose-600 text-rose-600 dark:text-rose-400 px-4 py-3 text-[10px] font-black uppercase tracking-widest flex items-start gap-3 mt-4">
                <AlertCircle className="w-4 h-4 mt-0.5" />
                <span className="leading-relaxed">{error}</span>
              </div>
            )}

            <div className="relative pt-6">
              <input
                type="email"
                id="email"
                value={email}
                onChange={(e) => setEmail(e.target.value)}
                className="peer w-full bg-transparent border-0 border-b border-emerald-600 focus:border-b-2 text-foreground text-sm font-bold pb-2 px-0 pt-1 outline-none transition-all placeholder-transparent uppercase tracking-widest rounded-none focus:ring-0 [&:-webkit-autofill]:transition-[background-color] [&:-webkit-autofill]:duration-[50000s] [&:-webkit-autofill]:ease-in-out [&:-webkit-autofill]:[-webkit-text-fill-color:inherit]"
                placeholder="email"
                required
              />
              <label 
                htmlFor="email" 
                className={`absolute left-0 uppercase tracking-[0.2em] transition-all cursor-text pointer-events-none
                  ${email ? 'top-0 text-[10px] font-black text-emerald-600' : 'top-7 text-xs text-slate-400 dark:text-slate-500 font-bold'}
                  peer-focus:top-0 peer-focus:text-[10px] peer-focus:font-black peer-focus:text-emerald-600
                  peer-autofill:top-0 peer-autofill:text-[10px] peer-autofill:font-black peer-autofill:text-emerald-600
                  peer-[:-webkit-autofill]:top-0 peer-[:-webkit-autofill]:text-[10px] peer-[:-webkit-autofill]:font-black peer-[:-webkit-autofill]:text-emerald-600
                `}
              >
                Correo Electrónico
              </label>
            </div>
            
            <div className="relative pt-6">
              <input
                type="password"
                id="password"
                value={password}
                onChange={(e) => setPassword(e.target.value)}
                className="peer w-full bg-transparent border-0 border-b border-emerald-600 focus:border-b-2 text-foreground text-sm font-bold pb-2 px-0 pt-1 outline-none transition-all placeholder-transparent uppercase tracking-widest rounded-none focus:ring-0 [&:-webkit-autofill]:transition-[background-color] [&:-webkit-autofill]:duration-[50000s] [&:-webkit-autofill]:ease-in-out [&:-webkit-autofill]:[-webkit-text-fill-color:inherit]"
                placeholder="password"
                required
              />
              <label 
                htmlFor="password" 
                className={`absolute left-0 uppercase tracking-[0.2em] transition-all cursor-text pointer-events-none
                  ${password ? 'top-0 text-[10px] font-black text-emerald-600' : 'top-7 text-xs text-slate-400 dark:text-slate-500 font-bold'}
                  peer-focus:top-0 peer-focus:text-[10px] peer-focus:font-black peer-focus:text-emerald-600
                  peer-autofill:top-0 peer-autofill:text-[10px] peer-autofill:font-black peer-autofill:text-emerald-600
                  peer-[:-webkit-autofill]:top-0 peer-[:-webkit-autofill]:text-[10px] peer-[:-webkit-autofill]:font-black peer-[:-webkit-autofill]:text-emerald-600
                `}
              >
                Contraseña
              </label>
            </div>

            {/* Habeas Data */}
            <div className="flex items-start gap-3 pt-2 cursor-pointer group" onClick={() => setAcceptHabeasData(!acceptHabeasData)}>
              <div className="text-emerald-600 mt-0.5">
                {acceptHabeasData ? <CheckSquare size={16} strokeWidth={3} /> : <Square size={16} strokeWidth={3} />}
              </div>
              <p className="text-[9px] font-bold uppercase leading-tight tracking-wider text-slate-500 dark:text-slate-400">
                Acepto la política de tratamiento de datos personales según la <span className="text-emerald-600 font-black">Ley 1581 de 2012</span>.
              </p>
            </div>

            <Button
              type="submit"
              disabled={isLoading}
              variant="primary"
              className="w-full py-4 rounded-none mt-6 justify-between px-6 font-black text-[10px] bg-emerald-600 hover:bg-emerald-700 uppercase tracking-[0.25em]"
            >
              {isLoading ? 'CARGANDO...' : 'Iniciar Sesión'}
              {!isLoading && <span className="text-lg leading-none font-light">→</span>}
              {isLoading && <Loader2 className="animate-spin" size={16} />}
            </Button>

            <div className="mt-8 pt-8">
              <p className="text-[9px] text-slate-400 dark:text-zinc-600 font-bold uppercase tracking-[0.2em]">
                &copy; {new Date().getFullYear()} Softcoinp
              </p>
            </div>
          </form>
        </div>
      </div>
    </div>
  );
}
