#!/bin/bash

echo "🧪 Testing E-Commerce Microservices with Docker"

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
echo "docker-compose logs -f ordering-service"
echo "docker-compose logs -f payment-service"
echo "docker-compose logs -f inventory-service"
echo "docker-compose logs -f notification-service"

echo ""
echo "🎯 Access Swagger UI:"
echo "$SERVICE_URL/swagger"