#!/bin/bash

echo "🧹 Cleaning up unused code and resources"

# Remove unused Clean Architecture layers for simple services
echo "🗑️ Removing unused service layers..."
rm -rf src/InventoryService.Application/
rm -rf src/InventoryService.Domain/
rm -rf src/InventoryService.Infrastructure/
rm -rf src/PaymentService.Application/
rm -rf src/PaymentService.Domain/
rm -rf src/PaymentService.Infrastructure/

# Remove individual service Dockerfiles
echo "🗑️ Removing unused Dockerfiles..."
rm -f src/OrderingService.API/Dockerfile
rm -f src/PaymentService.API/Dockerfile
rm -f src/InventoryService.API/Dockerfile
rm -f src/NotificationService.API/Dockerfile
rm -f src/ShippingService.API/Dockerfile

# Remove unused files
echo "🗑️ Removing unused files..."
rm -f k8s/all-services.yaml
rm -f build.sh
rm -f run.sh
rm -f start-services.sh
rm -f Dockerfile
rm -f Dockerfile.shipping

# Remove incomplete ShippingService
echo "🗑️ Removing incomplete ShippingService..."
rm -rf src/ShippingService.API/

# Remove duplicate test directory
echo "🗑️ Removing duplicate test files..."
rm -rf tests/api-tests/

# Clean build artifacts
echo "🗑️ Cleaning build artifacts..."
find src/ -name "bin" -type d -exec rm -rf {} + 2>/dev/null || true
find src/ -name "obj" -type d -exec rm -rf {} + 2>/dev/null || true

# Remove NoOpEventPublisher (not needed with RabbitMQ)
echo "🗑️ Removing development-only files..."
rm -f src/OrderingService.API/NoOpEventPublisher.cs

echo "✅ Cleanup complete!"
echo ""
echo "📊 Remaining structure:"
echo "- Ordering Service: Full Clean Architecture (4 projects)"
echo "- Payment/Inventory/Notification: Simple single-project services"
echo "- Docker Compose & Kubernetes deployments"
echo "- Test payloads"