# PhoneStore — User Manual

## Overview

PhoneStore is a web application for managing a phone inventory. It allows you to view, add, update, and delete phone products through a simple browser interface and a REST API.

---

## Accessing the Application

| Interface | URL |
|-----------|-----|
| Phone Store UI | http://localhost:3000 |
| API (Swagger) | http://localhost:5152/swagger |
| Grafana Dashboard | http://localhost:3000 (when Grafana running) |
| Kubernetes (Minikube) | http://phonestore.local |

---

## Using the React UI

### View Products
1. Open a browser and go to `http://localhost:3000`
2. The page automatically loads all products from the database
3. Each product card displays:
   - Product name
   - Brand
   - Price (in USD)
   - Stock quantity

### What the UI Shows
- **Loading...** — data is being fetched from the API
- **No products found.** — database is empty
- **Error loading products: ...** — API is unreachable or returned an error

---

## Using the API (Swagger UI)

Open `http://localhost:5152/swagger` in your browser to access the interactive API documentation.

### Get All Products
- **Method:** `GET /api/Products`
- Click **Try it out** → **Execute**
- Returns a JSON array of all products

### Add a Product
- **Method:** `POST /api/Products`
- Click **Try it out**
- Fill in the request body:
  ```json
  {
    "name": "iPhone 16",
    "brand": "Apple",
    "price": 1299.99,
    "stock": 20,
    "imageUrl": "https://example.com/iphone16.jpg"
  }
  ```
- Click **Execute**
- Expected response: `201` with `"Product added successfully"`

### Update a Product
- **Method:** `PUT /api/Products/{id}`
- Enter the product `id` in the path field
- Provide the updated product details in the request body
- Expected response: `200` with `"Product updated successfully"`
- If the ID doesn't exist: `404 Not Found`

### Delete a Product
- **Method:** `DELETE /api/Products/{id}`
- Enter the product `id` in the path field
- Click **Execute**
- Expected response: `200` with `"Product deleted successfully"`
- If the ID doesn't exist: `404 Not Found`

### Health Check
- **Method:** `GET /health`
- Returns `"Healthy"` with status `200`
- Used by Kubernetes to verify the API is alive

---

## Using the Grafana Dashboard

1. Start Grafana: `docker run -d -p 3000:3000 grafana/grafana`
2. Open `http://localhost:3000`
3. Default login: **admin / admin**
4. Navigate to **Dashboards → PhoneStore Dashboard**

### Dashboard Panels

| Panel | What It Shows |
|-------|--------------|
| System Load | Overall server load over time |
| Active Users | Number of concurrent users |
| API Requests | Request volume to the API |
| Response Time | Average API response time (ms) |
| Error Rate | Number of errors per time window |
| Memory Usage | Application memory consumption (MB) |
| CPU Usage | CPU utilisation (%) |

---

## Kubernetes — Checking Application Status

```bash
# View all running resources in the phonestore namespace
kubectl get all -n phonestore

# Check pod logs (replace <pod-name> with actual name)
kubectl logs <pod-name> -n phonestore

# Describe a pod to see probe status and events
kubectl describe pod <pod-name> -n phonestore

# Manually scale the API
kubectl scale deployment api-deployment --replicas=3 -n phonestore
```

---

## Troubleshooting

| Problem | Solution |
|---------|----------|
| UI shows "Error loading products" | Make sure the API is running on port 5152. Check `dotnet run` output for errors. |
| Swagger page is blank | The API may not be running. Restart with `dotnet run`. |
| Products list is empty | The database may be empty. Use the POST endpoint to add products. |
| K8s pods in CrashLoopBackOff | Run `kubectl describe pod <name> -n phonestore` to check probe failures. |
| Cannot reach phonestore.local | Add `127.0.0.1 phonestore.local` to your `hosts` file and run `minikube tunnel`. |
