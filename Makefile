# ============================================================
# Makefile – Softcoinp ERP
# ============================================================
# Comandos disponibles:
#   make deploy       → Compila en host + build Docker + up (RECOMENDADO)
#   make up           → docker-compose up -d (usa imagen existente)
#   make down         → Detiene y elimina contenedores
#   make dev          → Modo desarrollo con hot-reload
#   make dev-down     → Detiene el modo desarrollo
#   make logs         → Logs del frontend en tiempo real
#   make status       → Estado de todos los contenedores
# ============================================================
#
# ¿Por qué compilar en el host?
#   Next.js 16 + Turbopack usa mmap(/dev/shm) que supera el límite
#   de BuildKit en WSL2 (Bus error, exit 135). Compilar en Windows
#   evita esa restricción. Docker solo empaqueta el artefacto .next.
# ============================================================

.PHONY: deploy up down dev dev-down logs status

COMPOSE     = docker-compose
COMPOSE_DEV = docker-compose -f docker-compose.yml -f docker-compose.dev.yml

# ── Producción (recomendado) ─────────────────────────────────
# Compila en host, luego empaqueta y sirve desde Docker
deploy:
	cd frontend-erp && npm run build
	$(COMPOSE) up -d --build

# Solo levanta (imagen ya construida)
up:
	$(COMPOSE) up -d

down:
	$(COMPOSE) down

# ── Desarrollo (hot-reload) ─────────────────────────────────
dev:
	$(COMPOSE_DEV) up -d --build erp-frontend

dev-down:
	$(COMPOSE_DEV) down --remove-orphans

# ── Utilidades ───────────────────────────────────────────────
logs:
	$(COMPOSE) logs -f erp-frontend

status:
	$(COMPOSE) ps

