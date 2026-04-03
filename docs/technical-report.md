# 🧠 Technical Report – PhoneStore Project

## 1. Overview
The PhoneStore project is a full-stack application built using a .NET Web API backend and a React frontend. It is containerized with Docker and deployed using Kubernetes.

## 2. Architecture
Frontend (React) → Backend (.NET API) → Database (MSSQL)

## 3. Technologies
- .NET 8
- React
- Docker
- Kubernetes
- Grafana & Prometheus

## 4. Backend
- REST API
- Swagger for testing
- CORS enabled

## 5. Frontend
- React UI
- Displays phone data from API

## 6. Docker
- Separate images for API and UI

## 7. Kubernetes
- Deployments + Services
- Ingress for access

## 8. Monitoring
- Grafana + Prometheus

## 9. Issues
- CORS fixed
- Dockerfile issue fixed
- ImagePullBackOff fixed

## 10. Conclusion
Complete DevOps pipeline implemented.