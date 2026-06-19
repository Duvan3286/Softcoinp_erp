# deploy.ps1
# ============================================================
# Script de despliegue del frontend ERP
# ============================================================
# Estrategia:
#   1. Compila el bundle de Next.js en el HOST (evita el Bus error de WSL2/BuildKit)
#   2. Empaqueta el artefacto .next en una imagen Docker ligera
#   3. Levanta todos los servicios con docker-compose
#
# Principio Single Responsibility:
#   - Este script orquesta; cada paso delega a la herramienta correcta.
# ============================================================

param(
    [switch]$OnlyFrontend,   # Solo reconstruye el frontend
    [switch]$SkipBuild       # Salta el npm build (usa el .next existente)
)

$ErrorActionPreference = "Stop"
$FrontendDir = Join-Path $PSScriptRoot "frontend-erp"

Write-Host ""
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "  Softcoinp ERP - Deploy Script" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

# ── Paso 1: Compilar Next.js en el host ──────────────────────
if (-not $SkipBuild) {
    Write-Host "→ [1/3] Compilando frontend en el host (Next.js build)..." -ForegroundColor Yellow
    Set-Location $FrontendDir
    
    # Verificar que node_modules existe
    if (-not (Test-Path "node_modules")) {
        Write-Host "  ⚠ node_modules no encontrado. Ejecutando npm install..." -ForegroundColor Yellow
        npm install
        if ($LASTEXITCODE -ne 0) { throw "npm install falló" }
    }

    # Compilar
    npm run build
    if ($LASTEXITCODE -ne 0) { throw "npm run build falló" }
    
    Write-Host "  ✓ Build completado exitosamente" -ForegroundColor Green
    Set-Location $PSScriptRoot
} else {
    Write-Host "→ [1/3] Compilación omitida (--SkipBuild)" -ForegroundColor Gray
}

# ── Paso 2: Construir imagen Docker ──────────────────────────
Write-Host ""
Write-Host "→ [2/3] Construyendo imagen Docker del frontend..." -ForegroundColor Yellow

if ($OnlyFrontend) {
    docker-compose build erp-frontend
} else {
    docker-compose build
}

if ($LASTEXITCODE -ne 0) { throw "docker-compose build falló" }
Write-Host "  ✓ Imagen construida" -ForegroundColor Green

# ── Paso 3: Levantar servicios ───────────────────────────────
Write-Host ""
Write-Host "→ [3/3] Levantando servicios..." -ForegroundColor Yellow
docker-compose up -d
if ($LASTEXITCODE -ne 0) { throw "docker-compose up falló" }

Write-Host ""
Write-Host "========================================" -ForegroundColor Green
Write-Host "  ✓ Despliegue completado exitosamente" -ForegroundColor Green
Write-Host "========================================" -ForegroundColor Green
Write-Host ""
Write-Host "  Frontend : http://test.localhost:3001" -ForegroundColor Cyan
Write-Host "  Backend  : http://localhost:5005"      -ForegroundColor Cyan
Write-Host "  phpMyAdmin: http://localhost:8080"     -ForegroundColor Cyan
Write-Host ""
