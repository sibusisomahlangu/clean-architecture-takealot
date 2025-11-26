#!/bin/bash

echo "🧹 Cleaning up E-Commerce Microservices from Kubernetes"

# Delete all deployments and services
echo "🗑️ Deleting all microservices..."
kubectl delete namespace ecommerce-microservices

# Wait for namespace deletion
echo "⏳ Waiting for namespace deletion..."
kubectl wait --for=delete namespace/ecommerce-microservices --timeout=60s

# Stop minikube (optional)
echo "🛑 Stopping Minikube..."
minikube stop

echo "✅ Cleanup Complete!"
echo ""
echo "📋 To restart:"
echo "./deploy-k8s.sh"