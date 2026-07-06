# Softcoinp ERP

Ecosystem for Enterprise Resource Planning, following Clean Architecture and Dockerization.

## Technical Architecture

### Backend (ASP.NET Core 8.0)
- **Softcoinp.ERP.Domain**: Entities, Interfaces, Domain logic.
- **Softcoinp.ERP.Application**: DTOs, Services, Mappers, Business logic (FluentValidation).
- **Softcoinp.ERP.Infrastructure**: Data access (EF Core + MySQL), Migrations, External services.
- **Softcoinp.ERP.WebAPI**: RESTful API, Authentication (JWT), Controllers.

### Frontend (Next.js 15+)
- **TypeScript**: Type safety.
- **Tailwind CSS 4**: Modern styling.
- **App Router**: Next.js latest routing pattern.
- **Axios**: API communication.
- **Lucide React**: Icon library.
- **Next Themes**: Dark/Light mode support.

### Infrastructure
- **Docker Compose**: Orquestration of DB, Backend, and Frontend.
- **MySQL 8.0**: Relational database.

## Quick Start

### Production

```bash
make up
```

| Service | URL | Description |
|---------|-----|-------------|
| Frontend | http://test.localhost:3001 | Next.js (production) |
| Backend | http://localhost:5005 | ASP.NET Core API |
| phpMyAdmin | http://localhost:8080 | Database administration |
| MySQL | localhost:3307 | Direct database connection |

### Development (hot-reload)

```bash
make dev
```

Any change in `frontend-erp/src/` reflects instantly in the browser.

### Stop

```bash
make down         # Production
make dev-down     # Development
```

### Clean rebuild

```bash
make deploy       # Build in host + Docker + up (recommended)
```

## Default Credentials

### Database (erp-db)
- **Host**: erp-db (internal) / localhost:3307 (external)
- **User**: erp_user
- **Password**: erp_password
- **Database**: erp_db
- **Root Password**: rootpassword

### Backend API
- **URL**: http://localhost:5000 (internal) / http://localhost:5005 (compose)

### Frontend
- **URL**: http://localhost:3001

## Docker Architecture

```
softcoinp-erp/
├── docker-compose.yml         # Base compose (production)
├── docker-compose.dev.yml     # Override for development (hot-reload)
├── Makefile                   # Management commands
└── frontend-erp/
    ├── Dockerfile             # Production image (multi-stage, optimized)
    └── Dockerfile.dev         # Development image (build tools + next dev)
```

Two Dockerfiles are provided: **Dockerfile** uses multi-stage build with only production dependencies (~200 MB), while **Dockerfile.dev** includes all dev dependencies and compilers for hot-reload (~800 MB).

## Networking

This project connects to `softcoinp-network` to allow communication with the `softcoinp-backend` container from Project A.

## Tenant Management

To register a new tenant in the system, use the administrative API. Each tenant requires its own database or connection string.

### Create a New Tenant

**Endpoint:** `POST /api/v1/admin/tenants`

**Example Request (curl):**
```bash
curl -X POST http://localhost:5000/api/v1/admin/tenants \
-H "Content-Type: application/json" \
-d '{
  "name": "Client Alpha",
  "subdomain": "alpha",
  "connectionString": "Server=erp-db;Database=erp_tenant_alpha;User=erp_user;Password=erp_password;",
  "isActive": true
}'
```

### Run Migrations for All Tenants

**Endpoint:** `POST /api/v1/admin/maintenance/migrate-all`

This process will ensure all tenant databases are up to date with the latest schema changes.
