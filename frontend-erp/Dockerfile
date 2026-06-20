# =============================================================
# Dockerfile – Producción (multi-stage: compila dentro de Docker)
# =============================================================
# Stage 1 compila el bundle de Next.js DENTRO del contenedor.
# Esto elimina la dependencia de tener ".next" pre-compilado en el
# host, de modo que "docker compose up --build" siempre refleja
# los últimos cambios en el código fuente.
#
# El docker-compose.yml ya expone:
#   build.shm_size: '256mb'
# para evitar el "Bus error (core dumped)" de Turbopack en WSL2.
# =============================================================

# ── Stage 1: Build ───────────────────────────────────────────
FROM node:20-slim AS builder
WORKDIR /app

# Copiar manifiestos primero para aprovechar el caché de capas.
# Si package*.json no cambia, "npm ci" se cachea automáticamente.
COPY package*.json ./
RUN npm ci

# Copiar el código fuente (node_modules viene del paso anterior,
# no del host — ver .dockerignore).
COPY . .

# NEXT_PUBLIC_* se embeben en el bundle en tiempo de compilación.
# El valor por defecto coincide con la URL expuesta en docker-compose.
ARG NEXT_PUBLIC_API_URL=http://localhost:5005/api
ENV NEXT_PUBLIC_API_URL=$NEXT_PUBLIC_API_URL
ENV NEXT_TELEMETRY_DISABLED=1

# Aumentar el límite de memoria de Node.js para builds grandes.
ENV NODE_OPTIONS=--max-old-space-size=2048

RUN npm run build

# ── Stage 2: Runner ──────────────────────────────────────────
FROM node:20-slim AS runner
WORKDIR /app

# Solo dependencias de producción en la imagen final.
COPY package*.json ./
RUN npm install --omit=dev --ignore-scripts

# Artefacto compilado desde el stage builder (no del host).
COPY --from=builder /app/.next ./.next
COPY --from=builder /app/public ./public

EXPOSE 3000

ENV NEXT_TELEMETRY_DISABLED=1

CMD ["npm", "start"]
