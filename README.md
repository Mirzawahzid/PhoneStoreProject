# PhoneStore Project

A full-stack phone inventory management application with a .NET 8 REST API backend, React frontend, containerised with Docker, orchestrated on Kubernetes, and monitored with Grafana.

---

## Repository Structure

```
PhoneStoreProject/
├── Api/                  # .NET 8 Web API (CRUD for products)
│   ├── Controllers/      # ProductsController
│   ├── Models/           # Product model
│   ├── Program.cs        # App entry point, middleware, CORS
│   ├── appsettings.json  # Base configuration (no secrets)
│   ├── appsettings.Development.json  # Local dev connection string
│   └── Dockerfile        # Multi-stage Docker build
├── ui/                   # React frontend
│   ├── src/App.js        # Main component, fetches & displays products
│   └── Dockerfile        # Nginx-based production container
├── k8s/                  # Kubernetes manifests
│   ├── namespace.yaml
│   ├── configmap.yaml
│   ├── secret.yaml
│   ├── api-deployment.yaml
│   ├── api-service.yaml
│   ├── ui-deployment.yaml
│   ├── ui-service.yaml
│   └── ingress.yaml
├── docs/                 # Documentation
│   ├── debug-drills.md
│   ├── user-manual.md
│   └── design-document.md
└── README.md             # This file
```

---

## Tech Stack

| Layer | Technology |
|-------|-----------|
| Backend | .NET 8, ASP.NET Core, Dapper |
| Database | SQL Server (via Docker/Kubernetes) |
| Frontend | React 19, Create React App |
| Containerisation | Docker (multi-stage builds) |
| Orchestration | Kubernetes (Minikube) |
| Monitoring | Grafana |
| API Docs | Swagger / OpenAPI 3.0 |

---

## Quick Start (Local Dev)

### Prerequisites
- .NET 8 SDK
- Node.js 20+
- SQL Server (Docker: `docker run -e 'ACCEPT_EULA=Y' -e 'SA_PASSWORD=MySecure@123' -p 1433:1433 -d mcr.microsoft.com/mssql/server:2022-latest`)

### Run the API
```bash
cd Api
dotnet run --launch-profile PhoneStoreApi
# API available at http://localhost:5152
# Swagger UI at http://localhost:5152/swagger
```

### Run the UI
```bash
cd ui
npm install
npm start
# App available at http://localhost:3000
```

---

## Kubernetes Deployment

```bash
# Apply all manifests in order
kubectl apply -f k8s/namespace.yaml
kubectl apply -f k8s/configmap.yaml
kubectl apply -f k8s/secret.yaml
kubectl apply -f k8s/api-deployment.yaml
kubectl apply -f k8s/api-service.yaml
kubectl apply -f k8s/ui-deployment.yaml
kubectl apply -f k8s/ui-service.yaml
kubectl apply -f k8s/ingress.yaml

# Check deployments
kubectl get all -n phonestore
```

### Scale manually
```bash
kubectl scale deployment api-deployment --replicas=3 -n phonestore
```

---

## API Endpoints

| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/health` | Health check (used by K8s probes) |
| GET | `/api/Products` | Get all products |
| POST | `/api/Products` | Add a product |
| PUT | `/api/Products/{id}` | Update a product |
| DELETE | `/api/Products/{id}` | Delete a product |

---

## Grafana Dashboard

7 monitoring panels (running on `localhost:3000` via Docker):

1. System Load
2. Active Users
3. Error Rate
4. Response Time
5. API Requests
6. Memory Usage
7. CPU Usage

---

## Documentation

- [User Manual](docs/user-manual.md)
- [Design Document](docs/design-document.md)
- [Debug Drills](docs/debug-drills.md)
