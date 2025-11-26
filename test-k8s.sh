#!/bin/bash

echo "🧪 Testing E-Commerce Microservices in Kubernetes"

# Use port forwarding instead of minikube service
echo "🔗 Setting up port forwarding..."
kubectl port-forward service/ordering-service 8080:80 -n ecommerce-microservices &
PORT_FORWARD_PID=$!
sleep 5

SERVICE_URL="http://localhost:8080"
echo "🔗 Ordering Service URL: $SERVICE_URL"

echo ""
echo "📋 Testing API endpoints..."

# Test health check
echo "1. Health Check:"
curl -s $SERVICE_URL/health
echo ""

# Test order creation
echo "2. Creating Order:"
ORDER_RESPONSE=$(curl -s -X POST $SERVICE_URL/api/orders \
  -H "Content-Type: application/json" \
  -d @tests/payloads/create-order-success.json)

echo $ORDER_RESPONSE | jq .
ORDER_ID=$(echo $ORDER_RESPONSE | jq -r '.id')
echo "Order ID: $ORDER_ID"

echo ""
echo "3. Getting All Orders:"
curl -s $SERVICE_URL/api/orders | jq .

echo ""
echo "4. Accepting Order:"
curl -s -X POST $SERVICE_URL/api/orders/$ORDER_ID/accept
echo ""

echo ""
echo "5. Getting Order Status:"
curl -s $SERVICE_URL/api/orders/$ORDER_ID | jq .

echo ""
echo "📊 Check service logs to see event flow:"
echo "kubectl logs -f deployment/ordering-service -n ecommerce-microservices"
echo "kubectl logs -f deployment/payment-service -n ecommerce-microservices"
echo "kubectl logs -f deployment/inventory-service -n ecommerce-microservices"
echo "kubectl logs -f deployment/notification-service -n ecommerce-microservices"

echo ""
echo "🎯 Access Swagger UI:"
echo "$SERVICE_URL/swagger"

echo ""
echo "📋 Cleaning up port forwarding..."
kill $PORT_FORWARD_PID 2>/dev/null

echo ""
echo "🚀 To access services manually:"
echo "kubectl port-forward service/ordering-service 8080:80 -n ecommerce-microservices"
echo "Then visit: http://localhost:8080"