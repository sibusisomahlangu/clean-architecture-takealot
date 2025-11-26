#!/bin/bash

echo "🚀 Deploying E-Commerce Microservices to Kubernetes"

# Check if minikube is already running
if minikube status | grep -q "Running"; then
    echo "📦 Minikube is already running"
else
    echo "📦 Starting Minikube with Docker driver..."
    minikube start --driver=docker
fi

# Set docker environment to use minikube's docker daemon
echo "🔧 Setting up Docker environment..."
eval $(minikube docker-env)

echo "🔨 Building Docker Images..."

# Build all services from root directory
docker build -f Dockerfile.ordering -t ordering-service:latest .
docker build -f Dockerfile.payment -t payment-service:latest .
docker build -f Dockerfile.inventory -t inventory-service:latest .
docker build -f Dockerfile.notification -t notification-service:latest .
docker build -f Dockerfile.shipping -t shipping-service:latest .

echo "☸️ Deploying to Kubernetes..."

# Create namespace
kubectl apply -f k8s/namespace.yaml

# Deploy RabbitMQ first
kubectl apply -f k8s/rabbitmq.yaml

# Wait for RabbitMQ to be ready
echo "⏳ Waiting for RabbitMQ to be ready..."
kubectl wait --for=condition=available --timeout=300s deployment/rabbitmq -n ecommerce-microservices

# Deploy all microservices
kubectl apply -f k8s/ordering-service.yaml
kubectl apply -f k8s/payment-service.yaml
kubectl apply -f k8s/inventory-service.yaml
kubectl apply -f k8s/notification-service.yaml
kubectl apply -f k8s/shipping-service.yaml

echo "⏳ Waiting for all services to be ready..."
kubectl wait --for=condition=available --timeout=300s deployment/ordering-service -n ecommerce-microservices
kubectl wait --for=condition=available --timeout=300s deployment/payment-service -n ecommerce-microservices
kubectl wait --for=condition=available --timeout=300s deployment/inventory-service -n ecommerce-microservices
kubectl wait --for=condition=available --timeout=300s deployment/notification-service -n ecommerce-microservices
kubectl wait --for=condition=available --timeout=300s deployment/shipping-service -n ecommerce-microservices

echo "✅ Deployment Complete!"
echo ""
echo "📊 Service Status:"
kubectl get pods -n ecommerce-microservices
echo ""
echo "🌐 Services:"
kubectl get services -n ecommerce-microservices
echo ""
echo "🔗 Access the Ordering Service:"
echo "Run: minikube service ordering-service -n ecommerce-microservices --url"
echo ""
echo "📋 View logs:"
echo "kubectl logs -f deployment/ordering-service -n ecommerce-microservices"
echo "kubectl logs -f deployment/payment-service -n ecommerce-microservices"
echo "kubectl logs -f deployment/inventory-service -n ecommerce-microservices"
echo "kubectl logs -f deployment/notification-service -n ecommerce-microservices"
echo "kubectl logs -f deployment/shipping-service -n ecommerce-microservices"