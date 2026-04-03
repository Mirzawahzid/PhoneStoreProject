# PhoneStore — Design Document

## 1. Project Overview

PhoneStore is a full-stack inventory management system for a phone retail store. It demonstrates a complete DevOps pipeline from local development through containerisation, Kubernetes orchestration, and live monitoring.

---

## 2. Architecture Overview

```
┌─────────────┐        HTTP         ┌──────────────────┐
│  React UI   │ ──────────────────► │  .NET 8 REST API │
│  (Port 3000)│                     │  (Port 5152/80)  │
└─────────────┘                     └────────┬─────────┘
                                             │ Dapper / SQL
                                    ┌────────▼─────────┐
                                    │   SQL Server DB  │
                                    │  PhoneStoreDB    │
                                    └──────────────────┘

             Monitored by
┌──────────────────────────┐
│  Grafana Dashboard       │
│  7 panels (localhost:3000│
│  when Grafana active)    │
└──────────────────────────┘
```

### Kubernetes Architecture (Production)

```
Internet
    │
    ▼
┌─────────────────────────────────┐
│  Ingress (nginx)                │
│  phonestore.local               │
│  /api  → api-service (ClusterIP)│
│  /     → ui-service  (NodePort) │
└──────────┬──────────────────────┘
           │
    ┌──────┴──────┐
    │             │
┌───▼───┐   ┌────▼────┐
│  API  │   │   UI    │
│  x2   │   │  x2     │  ← replicas
│  pods │   │  pods   │
└───┬───┘   └─────────┘
    │
┌───▼──────────┐
│  SQL Server  │
│  (external / │
│   in-cluster)│
└──────────────┘
```

---

## 3. Component Design

### 3.1 Backend — .NET 8 API

| Property | Detail |
|----------|--------|
| Framework | ASP.NET Core 8 |
| ORM | Dapper (micro-ORM, raw SQL) |
| Database driver | Microsoft.Data.SqlClient |
| API style | REST, JSON |
| Documentation | Swagger / OpenAPI 3.0 (dev only) |
| Auth | None (demo scope) |

**Middleware pipeline order:**
1. `UseCors("AllowAll")` — allows any origin (suitable for dev/demo)
2. `UseAuthorization()`
3. `MapGet("/health", ...)` — K8s probe endpoint
4. `MapControllers()` — routes to ProductsController

**Database Table:**
```sql
CREATE TABLE Products (
    Id       INT IDENTITY PRIMARY KEY,
    Name     NVARCHAR(100) NOT NULL,
    Brand    NVARCHAR(100) NOT NULL,
    Price    DECIMAL(10,2) NOT NULL,
    Stock    INT NOT NULL,
    ImageUrl NVARCHAR(500)
);
```

### 3.2 Frontend — React

| Property | Detail |
|----------|--------|
| Framework | React 19 |
| Build tool | Create React App |
| State | `useState` / `useEffect` (hooks) |
| API URL | `REACT_APP_API_URL` env var (fallback: localhost:5152) |

**Data flow:**
1. Component mounts → `useEffect` fires
2. `fetch(API_URL/api/Products)` called
3. Loading state shown while awaiting response
4. On success → products rendered as cards
5. On error → error message shown

### 3.3 Database — SQL Server

- Runs in Docker locally or as a pod/external service in Kubernetes
- Connection string injected via environment variable (`ConnectionStrings__DefaultConnection`) — never baked into images
- Credentials stored in a Kubernetes Secret

---

## 4. Kubernetes Design

### 4.1 Manifests Summary

| File | Kind | Purpose |
|------|------|---------|
| `namespace.yaml` | Namespace | Isolates all resources under `phonestore` |
| `configmap.yaml` | ConfigMap | Non-secret config (`ASPNETCORE_ENVIRONMENT`) |
| `secret.yaml` | Secret | DB connection string, DB password, API key |
| `api-deployment.yaml` | Deployment | API pods (2 replicas), probes, env injection |
| `api-service.yaml` | Service (ClusterIP) | Internal routing to API pods |
| `ui-deployment.yaml` | Deployment | UI pods (2 replicas, nginx) |
| `ui-service.yaml` | Service (NodePort) | Exposes UI externally |
| `ingress.yaml` | Ingress | Single entry point, path-based routing |

### 4.2 Health Probes

| Probe | Path | Delay | Period | Purpose |
|-------|------|-------|--------|---------|
| Readiness | `/health` | 5s | 5s | Stop routing traffic until ready |
| Liveness | `/health` | 10s | 10s | Restart pod if it becomes unresponsive |

Readiness fires sooner to gate traffic; liveness fires later to avoid restarting a still-booting container.

### 4.3 Secrets Strategy

| Secret Key | Used By | Purpose |
|------------|---------|---------|
| `DefaultConnection` | API env var `ConnectionStrings__DefaultConnection` | Full DB connection string |
| `DB_PASSWORD` | Available for init scripts | Raw DB password |
| `API_KEY` | Available for future auth middleware | API authentication key |

Secrets are stored as Kubernetes `Secret` objects (base64-encoded). The `secret.yaml` file uses `stringData` for human-readable authoring — Kubernetes encodes them at apply time.

> **Production note:** In a real production system, use a secrets manager (Azure Key Vault, AWS Secrets Manager, HashiCorp Vault) and never commit `secret.yaml` to source control.

---

## 5. Monitoring Design

Grafana runs as a Docker container alongside the application. 7 dashboard panels cover:

| Panel | Metric Type | Alert Threshold (suggested) |
|-------|------------|----------------------------|
| CPU Usage | Gauge/Time series | > 80% |
| Memory Usage | Time series | > 512 MB |
| System Load | Time series | > 2.0 (1-min avg) |
| API Requests | Counter/Time series | N/A (informational) |
| Response Time | Time series | > 500ms avg |
| Error Rate | Time series | > 5% of requests |
| Active Users | Gauge | N/A (informational) |

---

## 6. Security Considerations

| Area | Decision | Reason |
|------|----------|--------|
| CORS | AllowAnyOrigin (dev) | Demo scope; production should restrict to specific origins |
| Secrets | K8s Secret objects | Never in code or Docker images |
| Connection string | Env var injection | Overrides empty placeholder in appsettings.json |
| `appsettings.Development.json` | Listed in `.dockerignore` | Prevents local credentials entering Docker image |
| Swagger | Dev environment only | Not exposed in production containers |
| SQL | Parameterised queries via Dapper | Prevents SQL injection |

---

## 7. Deployment Pipeline Summary

```
Developer pushes code
        │
        ▼
  docker build (multi-stage)
  ├── API: sdk:8.0 build → aspnet:8.0 runtime
  └── UI:  node:20 build → nginx:alpine serve
        │
        ▼
  docker push (image registry)
        │
        ▼
  kubectl apply -f k8s/
  ├── namespace → configmap → secret
  ├── api-deployment + api-service
  ├── ui-deployment  + ui-service
  └── ingress
        │
        ▼
  Kubernetes schedules pods (2 replicas each)
  Health probes gate traffic until ready
        │
        ▼
  Grafana monitors running application
```
