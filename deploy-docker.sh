#!/bin/bash

echo "🚀 Deploying E-Commerce Microservices with Docker Compose"

# Stop any existing containers
echo "🛑 Stopping existing containers..."
docker-compose down

# Build and start all services
echo "🔨 Building and starting services..."
docker-compose up --build -d

# Wait for services to be ready
echo "⏳ Waiting for services to start..."
sleep 30

echo "✅ Deployment Complete!"
echo ""
echo "🌐 Services are running on:"
echo "- Ordering Service: http://localhost:8080"
echo "- Payment Service: http://localhost:8081" 
echo "- Inventory Service: http://localhost:8082"
echo "- Notification Service: http://localhost:8083"
echo "- RabbitMQ Management: http://localhost:15672 (admin/admin123)"
echo ""
echo "📊 Service Status:"
docker-compose ps
echo ""
echo "🧪 Test the services:"
echo "./test-docker.sh"
echo ""
echo "📋 View logs:"
echo "docker-compose logs -f ordering-service"
echo "docker-compose logs -f payment-service"
echo "docker-compose logs -f inventory-service"
echo "docker-compose logs -f notification-service"