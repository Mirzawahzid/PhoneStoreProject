# Debug Drills — PhoneStore Project

Two real failures encountered during development, with root cause analysis and fixes applied.

---

## Drill 1 — Kubernetes Pods Crash-Looping (Bad Health Probe Path)

### Symptom
After deploying to Minikube, both API pods immediately entered `CrashLoopBackOff`. Running `kubectl describe pod <pod-name> -n phonestore` showed:

```
Liveness probe failed: HTTP probe failed with statuscode: 404
```

### Root Cause
The liveness and readiness probes in `api-deployment.yaml` were pointing to `/swagger`:

```yaml
livenessProbe:
  httpGet:
    path: /swagger   # ← WRONG
    port: 80
```

Swagger is only enabled in `Development` environment (`app.Environment.IsDevelopment()` guard in `Program.cs`). In Kubernetes the environment was `Production`, so `/swagger` returned 404, causing Kubernetes to restart the pod in an endless loop.

### Fix Applied
1. Added a dedicated `/health` endpoint in `Program.cs`:
   ```csharp
   app.MapGet("/health", () => Results.Ok("Healthy"));
   ```
2. Updated both probes in `api-deployment.yaml` to use `/health`:
   ```yaml
   livenessProbe:
     httpGet:
       path: /health
       port: 80
     initialDelaySeconds: 10
     periodSeconds: 10
   readinessProbe:
     httpGet:
       path: /health
       port: 80
     initialDelaySeconds: 5
     periodSeconds: 5
   ```

### Verification
```bash
kubectl get pods -n phonestore
# All pods show STATUS: Running, READY: 1/1
curl http://localhost:5152/health
# Returns: "Healthy"
```

---

## Drill 2 — React UI Hardcoded Localhost URL Breaks in Kubernetes

### Symptom
The React app worked fine locally but showed a blank product list (and a CORS/network error in DevTools) when served from Kubernetes via the Ingress at `http://phonestore.local`.

```
GET http://localhost:5152/api/Products  net::ERR_CONNECTION_REFUSED
```

### Root Cause
The API URL was hardcoded in `ui/src/App.js`:

```js
fetch("http://localhost:5152/api/Products")
```

When the UI container runs in Kubernetes, `localhost` inside the container refers to the UI pod itself — not the API pod. There is no process listening on port 5152 in the UI container, so the request was refused immediately.

Additionally, the Ingress only routed `/` to the UI service and had no rule for `/api`, so the API was unreachable through the Ingress entirely.

### Fix Applied
1. Changed the fetch URL to use an environment variable with a localhost fallback:
   ```js
   const API_URL = process.env.REACT_APP_API_URL || "http://localhost:5152";
   // ...
   fetch(`${API_URL}/api/Products`)
   ```
2. Fixed `ingress.yaml` to route `/api` traffic to `api-service`:
   ```yaml
   - path: /api
     pathType: Prefix
     backend:
       service:
         name: api-service
         port:
           number: 80
   - path: /
     pathType: Prefix
     backend:
       service:
         name: ui-service
         port:
           number: 80
   ```
3. In Kubernetes, set `REACT_APP_API_URL=http://phonestore.local` at build time so the UI calls the correct external hostname.

### Verification
```bash
# Locally: falls back to localhost:5152 — works
npm start

# In K8s: build with env var set
REACT_APP_API_URL=http://phonestore.local npm run build
# Products load correctly from the Ingress URL
```
