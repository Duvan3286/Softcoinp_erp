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

## Default Credentials

### Database (erp-db)
- **Host**: erp-db (internal) / localhost:3307 (external)
- **User**: erp_user
- **Password**: erp_password
- **Database**: erp_db
- **Root Password**: rootpassword

### Backend API
- **URL**: http://localhost:5000

### Frontend
- **URL**: http://localhost:3001

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
