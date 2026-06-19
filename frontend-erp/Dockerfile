# =============================================================
# Dockerfile – Producción (estrategia: artefacto pre-compilado)
# =============================================================
# El build de Next.js se ejecuta en el HOST (Windows) antes de
# llamar a docker-compose, evitando el "Bus error" de BuildKit
# causado por la restricción de /dev/shm en WSL2.
#
# Flujo:
#   1. npm run build   (en el host, fuera de Docker)
#   2. docker-compose up -d --build  (empaqueta y sirve)
#
# Principio: Single Responsibility
#   - Host: compilar el código TypeScript → bundle optimizado.
#   - Docker: empaquetar el artefacto y servirlo en producción.
# =============================================================

FROM node:20-slim
WORKDIR /app

# Instalar SOLO dependencias de producción (sin devDependencies)
COPY package*.json ./
RUN npm install --omit=dev --ignore-scripts

# Copiar el artefacto pre-compilado desde el host
COPY .next ./.next
COPY public ./public

EXPOSE 3000
CMD ["npm", "start"]
