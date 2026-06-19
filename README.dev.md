# Guía de desarrollo – Softcoinp ERP

## Prerrequisitos

- Docker Desktop ≥ 4.x (con WSL2 o Hyper-V habilitado)
- `make` disponible en la terminal (incluido en Git for Windows / WSL)

---

## 🚀 Levantar el proyecto

### Producción (modo por defecto)

Construye las imágenes optimizadas y las sirve en modo `next start`:

```bash
make up
```

| Servicio | URL | Descripción |
|----------|-----|-------------|
| Frontend | http://test.localhost:3001 | Next.js (producción) |
| Backend | http://localhost:5005 | ASP.NET Core API |
| phpMyAdmin | http://localhost:8080 | Administración BD |
| MySQL | localhost:3307 | Conexión directa |

---

### Desarrollo (hot-reload)

Monta el código fuente como volumen y usa `next dev` con Fast Refresh:

```bash
make dev
```

> ✅ Cualquier cambio en `frontend-erp/src/` se refleja **al instante** en el navegador sin reiniciar el contenedor.

---

## 🛑 Detener el proyecto

```bash
make down         # Producción
make dev-down     # Desarrollo
```

---

## 🔨 Forzar reconstrucción limpia

Úsalo cuando instales nuevas dependencias npm o cambies el `Dockerfile`:

```bash
make rebuild      # Producción con --no-cache
```

Para desarrollo:

```bash
docker-compose -f docker-compose.yml -f docker-compose.dev.yml build --no-cache erp-frontend
make dev
```

---

## 📂 Estructura de archivos Docker

```
softcoinp-erp/
├── docker-compose.yml         # Compose base (producción)
├── docker-compose.dev.yml     # Override para desarrollo (hot-reload)
├── Makefile                   # Comandos de gestión
└── frontend-erp/
    ├── Dockerfile             # Imagen de producción (multi-stage, optimizada)
    └── Dockerfile.dev         # Imagen de desarrollo (build tools + next dev)
```

---

## 🧠 Por qué dos Dockerfiles

| Aspecto | `Dockerfile` (prod) | `Dockerfile.dev` |
|---------|--------------------|--------------------|
| Etapas | Multi-stage (build + final slim) | Single stage |
| node_modules | Solo prod dependencies en final | All deps + herramientas nativas |
| Comando | `npm start` | `npm run dev` |
| Tamaño imagen | ~200 MB | ~800 MB (incluye compiladores) |
| Hot-reload | ❌ | ✅ |

---

## ⚠️ Nota sobre node_modules en Docker-for-Windows

Cuando se monta `./frontend-erp:/app` como volumen, Docker remplazaría el directorio `/app/node_modules` de la imagen (que contiene binarios Linux compilados) con el del host (que contiene binarios Windows o ninguno).

Para evitar esto, `docker-compose.dev.yml` declara un **volumen nombrado** en `node_modules`:

```yaml
volumes:
  - ./frontend-erp:/app
  - frontend_node_modules:/app/node_modules   # ← tiene precedencia
```

Esto preserva los binarios Linux dentro del contenedor y el código fuente se actualiza en tiempo real desde el host.
