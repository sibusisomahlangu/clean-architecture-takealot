# E-Commerce Microservices - Clean Architecture Implementation

## Architecture Overview

This is a complete **Event-Driven E-Commerce System** built using **Clean Architecture** principles with .NET 8. The system consists of five microservices that communicate through RabbitMQ events, providing a robust, scalable, and maintainable solution for managing the complete order lifecycle in a distributed system.

### Why Clean Architecture?

- **Separation of Concerns**: Clear boundaries between business logic, application services, and infrastructure
- **Dependency Inversion**: Core business logic doesn't depend on external frameworks
- **Testability**: Business rules can be tested independently of UI, database, and external services
- **Framework Independence**: Easy to swap out databases, message brokers, or web frameworks
- **Microservice-Ready**: Promotes loose coupling and high cohesion

## Microservices Architecture

### Services Overview

1. **Ordering Service** - Core order management with Clean Architecture (Domain, Application, Infrastructure, API)
2. **Payment Service** - Payment processing and validation (simplified single-project architecture)
3. **Inventory Service** - Stock management and reservation (simplified single-project architecture)
4. **Notification Service** - Customer communications (simplified single-project architecture)
5. **Shipping Service** - Order fulfillment and tracking (simplified single-project architecture)

### Project Structure

```
src/
├── OrderingService.Domain/          # Clean Architecture - Core Domain
│   ├── Entities/                   # Order, OrderItem entities
│   ├── Events/                     # Domain events
│   ├── Enums/                      # Order status enums
│   └── Interfaces/                 # Repository contracts
├── OrderingService.Application/     # Clean Architecture - Application Layer
│   ├── Commands/                   # CQRS command handlers
│   ├── DTOs/                       # Data transfer objects
│   ├── EventHandlers/              # Event handlers
│   ├── Queries/                    # CQRS query handlers
│   └── Interfaces/                 # Service contracts
├── OrderingService.Infrastructure/  # Clean Architecture - Infrastructure
│   ├── Persistence/                # EF Core implementation
│   └── Messaging/                  # RabbitMQ implementation
├── OrderingService.API/            # Clean Architecture - Presentation
│   └── Controllers/                # REST API endpoints
├── PaymentService.API/             # Simplified single-project service
├── InventoryService.API/           # Simplified single-project service
├── NotificationService.API/        # Simplified single-project service
└── ShippingService.API/            # Simplified single-project service
```

## Key Features

### Complete E-Commerce Flow
- **Order Creation**: Customer places order through Ordering Service
- **Inventory Check**: Automatic stock validation and reservation
- **Payment Processing**: Secure payment handling with failure scenarios
- **Customer Notifications**: Real-time email/SMS notifications
- **Order Fulfillment**: Shipping arrangement and tracking

### Event-Driven Architecture

#### Event Flow Diagram
```
Order Created → [Payment Service] → Payment Success/Failure
     ↓              ↓                        ↓
[Inventory Service] [Ordering Service] → Order Accepted/Cancelled
     ↓              ↓                        ↓
Stock Reserved → [Shipping Service] → [Notification Service]
     ↓              ↓                        ↓
Shipping Arranged → Order Completed → Customer Notified
```

#### Published Events
- **OrderCreatedEvent**: New order placed
- **OrderAcceptedEvent**: Payment successful, order confirmed
- **OrderCancelledEvent**: Payment failed or inventory unavailable
- **OrderCompletedEvent**: Order fulfilled and shipped
- **PaymentSucceededEvent**: Payment processed successfully
- **PaymentFailedEvent**: Payment processing failed
- **InventoryReservedEvent**: Stock reserved for order
- **InventoryReservationFailedEvent**: Insufficient stock
- **ShippingArrangedEvent**: Shipping scheduled with tracking

#### Event Consumers
- **Ordering Service**: Consumes payment and inventory events
- **Inventory Service**: Consumes order creation events
- **Payment Service**: Consumes order creation events
- **Notification Service**: Consumes all order lifecycle events
- **Shipping Service**: Consumes order acceptance events

### Technology Stack
- **.NET 8**: Modern, high-performance runtime
- **Clean Architecture**: Domain-driven design with clear separation (Ordering Service)
- **Entity Framework Core**: In-memory database for development
- **MediatR**: CQRS and mediator pattern (Ordering Service)
- **RabbitMQ**: Reliable message broker with topic exchanges
- **Docker Compose**: Local development and testing
- **Kubernetes**: Container orchestration and scaling
- **Swagger/OpenAPI**: API documentation for all services



## Quick Start Guide

### Prerequisites
- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0) (or use existing .NET 10.0 installation)
- [Docker Desktop](https://www.docker.com/products/docker-desktop)

### 🚀 Deploy with Docker Compose (Recommended)

```bash
# 1. Clone the repository
git clone https://github.com/sibusisomahlangu/clean-architecture-takealot
cd clean-architecture-takealot

# 2. Deploy all services with Docker Compose
./deploy-docker.sh
```

**✅ All services are now running!**

### 🌐 Access the Services

- **Ordering Service**: http://localhost:8080/swagger
- **Payment Service**: http://localhost:8081/swagger
- **Inventory Service**: http://localhost:8082/swagger
- **Notification Service**: http://localhost:8083/swagger
- **Shipping Service**: http://localhost:8084/swagger
- **RabbitMQ Management**: http://localhost:15672 (admin/admin123)

### 🧪 Test Scenarios

#### 1. **Successful Order Creation**
```bash
# Create order with multiple items
curl -X POST http://localhost:8080/api/orders \
  -H "Content-Type: application/json" \
  -d @tests/payloads/create-order-success.json

# Expected: 201 Created with OrderCreatedEvent in console
```

#### 2. **Complete Order Lifecycle**
```bash
# 1. Create order
ORDER_RESPONSE=$(curl -s -X POST http://localhost:8080/api/orders \
  -H "Content-Type: application/json" \
  -d @tests/payloads/create-order-single-item.json)

# 2. Extract order ID from response
echo $ORDER_RESPONSE

# 3. Accept order (triggers OrderAcceptedEvent)
curl -X POST http://localhost:8080/api/orders/{order-id}/accept

# 4. Complete order (triggers OrderCompletedEvent)
curl -X POST http://localhost:8080/api/orders/{order-id}/complete

# 5. View final order status
curl http://localhost:8080/api/orders/{order-id}
```

#### 3. **Validation Test Scenarios**
```bash
# Empty customer ID (400 Bad Request)
curl -X POST http://localhost:8080/api/orders \
  -H "Content-Type: application/json" \
  -d @tests/payloads/validation-empty-customer.json

# Out of stock product (400 Bad Request)
curl -X POST http://localhost:8080/api/orders \
  -H "Content-Type: application/json" \
  -d @tests/payloads/validation-out-of-stock.json

# Order too large - over $10,000 (400 Bad Request)
curl -X POST http://localhost:8080/api/orders \
  -H "Content-Type: application/json" \
  -d @tests/payloads/validation-order-too-large.json

# Negative price (400 Bad Request)
curl -X POST http://localhost:8080/api/orders \
  -H "Content-Type: application/json" \
  -d @tests/payloads/validation-negative-price.json

# Zero quantity (400 Bad Request)
curl -X POST http://localhost:8080/api/orders \
  -H "Content-Type: application/json" \
  -d @tests/payloads/validation-zero-quantity.json
```

#### 4. **Order Management Operations**
```bash
# Get all orders
curl http://localhost:8080/api/orders

# Get orders by customer
curl http://localhost:8080/api/orders/customer/customer-123

# Cancel order with reason (triggers OrderCancelledEvent)
curl -X POST http://localhost:8080/api/orders/{order-id}/cancel \
  -H "Content-Type: application/json" \
  -d @tests/payloads/cancel-order.json

# Update order items (only for pending orders)
curl -X PUT http://localhost:8080/api/orders/{order-id}/items \
  -H "Content-Type: application/json" \
  -d @tests/payloads/update-order-items.json
```

#### 5. **Error Handling Scenarios**
```bash
# Try to accept non-existent order (404 Not Found)
curl -X POST http://localhost:8080/api/orders/00000000-0000-0000-0000-000000000000/accept

# Try to update items on accepted order (400 Bad Request)
# First accept an order, then try to update its items
curl -X PUT http://localhost:8080/api/orders/{accepted-order-id}/items \
  -H "Content-Type: application/json" \
  -d @tests/payloads/update-order-items.json

# Try to complete pending order (400 Bad Request)
curl -X POST http://localhost:8080/api/orders/{pending-order-id}/complete
```

**Watch the logs to see the event flow:**
```bash
# Events visible in Ordering Service console:
# - OrderCreatedEvent: When order is created
# - OrderAcceptedEvent: When order is accepted
# - OrderCancelledEvent: When order is cancelled
# - OrderCompletedEvent: When order is completed
```

### 🛑 Stop the Services

```bash
# Stop all services
docker-compose down
```

---

## Alternative: Run Individual Services

### 1. Start RabbitMQ
```bash
docker run -d --name rabbitmq -p 5672:5672 -p 15672:15672 \
  -e RABBITMQ_DEFAULT_USER=admin \
  -e RABBITMQ_DEFAULT_PASS=admin123 \
  rabbitmq:3-management
```

### 2. Build Solution
```bash
dotnet restore
dotnet build
```

### 3. Run Services (separate terminals)
```bash
# Terminal 1 - Ordering Service
cd src/OrderingService.API
dotnet run --urls "http://localhost:8080"

# Terminal 2 - Payment Service
cd src/PaymentService.API
dotnet run --urls "http://localhost:8081"

# Terminal 3 - Inventory Service
cd src/InventoryService.API
dotnet run --urls "http://localhost:8082"

# Terminal 4 - Notification Service
cd src/NotificationService.API
dotnet run --urls "http://localhost:8083"
```

## Kubernetes Deployment

### Prerequisites
- [Minikube](https://minikube.sigs.k8s.io/docs/start/) or [kubectl](https://kubernetes.io/docs/tasks/tools/)

### Deploy to Kubernetes

```bash
# Deploy all services to Kubernetes
./deploy-k8s.sh
```

### Test Kubernetes Deployment

```bash
# Test the deployed services
./test-k8s.sh
```

### Access Services

```bash
# Port forward to access services
kubectl port-forward service/ordering-service 8080:80 -n ecommerce-microservices

# Then access: http://localhost:8080/swagger
```

### Cleanup

```bash
# Clean up Kubernetes deployment
./cleanup-k8s.sh
```

## 🧪 Complete Test Suite

### 1. **Comprehensive Order Testing**

#### Success Path Testing
```bash
# Test 1: Single item order
curl -X POST http://localhost:8080/api/orders \
  -H "Content-Type: application/json" \
  -d @tests/payloads/create-order-single-item.json

# Test 2: Multiple items order
curl -X POST http://localhost:8080/api/orders \
  -H "Content-Type: application/json" \
  -d @tests/payloads/create-order-multiple-items.json

# Test 3: Standard order
curl -X POST http://localhost:8080/api/orders \
  -H "Content-Type: application/json" \
  -d @tests/payloads/create-order-success.json
```

#### Validation Testing Suite
```bash
# Customer validation tests
curl -X POST http://localhost:8080/api/orders -H "Content-Type: application/json" -d @tests/payloads/validation-empty-customer.json

# Item validation tests
curl -X POST http://localhost:8080/api/orders -H "Content-Type: application/json" -d @tests/payloads/validation-no-items.json
curl -X POST http://localhost:8080/api/orders -H "Content-Type: application/json" -d @tests/payloads/validation-empty-product-id.json
curl -X POST http://localhost:8080/api/orders -H "Content-Type: application/json" -d @tests/payloads/validation-empty-product-name.json

# Business rule validation tests
curl -X POST http://localhost:8080/api/orders -H "Content-Type: application/json" -d @tests/payloads/validation-negative-price.json
curl -X POST http://localhost:8080/api/orders -H "Content-Type: application/json" -d @tests/payloads/validation-zero-quantity.json
curl -X POST http://localhost:8080/api/orders -H "Content-Type: application/json" -d @tests/payloads/validation-out-of-stock.json
curl -X POST http://localhost:8080/api/orders -H "Content-Type: application/json" -d @tests/payloads/validation-order-too-large.json
```

### 2. **Order Lifecycle Management**

#### Complete Happy Path
```bash
# 1. Create order → OrderCreatedEvent
ORDER_ID=$(curl -s -X POST http://localhost:8080/api/orders \
  -H "Content-Type: application/json" \
  -d @tests/payloads/create-order-success.json | jq -r '.id')

# 2. Accept order → OrderAcceptedEvent
curl -X POST http://localhost:8080/api/orders/$ORDER_ID/accept

# 3. Complete order → OrderCompletedEvent
curl -X POST http://localhost:8080/api/orders/$ORDER_ID/complete

# 4. Verify final state
curl http://localhost:8080/api/orders/$ORDER_ID
```

#### Order Cancellation Scenarios
```bash
# Cancel pending order
curl -X POST http://localhost:8080/api/orders/{pending-order-id}/cancel \
  -H "Content-Type: application/json" \
  -d @tests/payloads/cancel-order.json

# Cancel due to payment failure
curl -X POST http://localhost:8080/api/orders/{order-id}/cancel \
  -H "Content-Type: application/json" \
  -d @tests/payloads/cancel-order-payment-failed.json

# Cancel due to inventory issues
curl -X POST http://localhost:8080/api/orders/{order-id}/cancel \
  -H "Content-Type: application/json" \
  -d @tests/payloads/cancel-order-inventory-failed.json
```

### 3. **Order Modification Testing**

#### Update Order Items (Pending Orders Only)
```bash
# Create pending order
ORDER_ID=$(curl -s -X POST http://localhost:8080/api/orders \
  -H "Content-Type: application/json" \
  -d @tests/payloads/create-order-single-item.json | jq -r '.id')

# Update with multiple items
curl -X PUT http://localhost:8080/api/orders/$ORDER_ID/items \
  -H "Content-Type: application/json" \
  -d @tests/payloads/update-order-items.json

# Update with single item
curl -X PUT http://localhost:8080/api/orders/$ORDER_ID/items \
  -H "Content-Type: application/json" \
  -d @tests/payloads/update-order-items-single.json
```

### 4. **Query Operations Testing**

```bash
# Get all orders
curl http://localhost:8080/api/orders

# Get specific order
curl http://localhost:8080/api/orders/{order-id}

# Get orders by customer
curl http://localhost:8080/api/orders/customer/customer-123
curl http://localhost:8080/api/orders/customer/customer-456
curl http://localhost:8080/api/orders/customer/customer-789
```

**Event Flow Monitoring:**
Watch the console output for these domain events:
- `OrderCreatedEvent` - New order created
- `OrderAcceptedEvent` - Order accepted (payment + inventory success)
- `OrderCancelledEvent` - Order cancelled (with reason)
- `OrderCompletedEvent` - Order fulfilled and completed

### 🎯 Business Rules Testing

#### Payment Simulation Rules
```bash
# Orders over $10,000 will fail validation
curl -X POST http://localhost:8080/api/orders \
  -H "Content-Type: application/json" \
  -d @tests/payloads/validation-order-too-large.json
# Expected: 400 Bad Request - "Order total cannot exceed $10,000"
```

#### Inventory Simulation Rules
```bash
# Product "out-of-stock-product" triggers inventory failure
curl -X POST http://localhost:8080/api/orders \
  -H "Content-Type: application/json" \
  -d @tests/payloads/validation-out-of-stock.json
# Expected: 400 Bad Request - "Product Unavailable Item is out of stock"
```

#### Order State Transition Rules
```bash
# Valid transitions: Pending → Accepted → Completed
# Valid transitions: Pending → Cancelled
# Valid transitions: Accepted → Cancelled
# Invalid: Completed → any other state
# Invalid: Cancelled → any other state

# Test invalid state transition
# 1. Create and complete an order
# 2. Try to cancel completed order (should fail)
curl -X POST http://localhost:8080/api/orders/{completed-order-id}/cancel \
  -H "Content-Type: application/json" \
  -d '{"reason": "Too late"}'
# Expected: 400 Bad Request - "Cannot cancel completed orders"
```

### 📊 Expected Event Flow

#### Successful Order Flow
```
1. POST /api/orders → OrderCreatedEvent
2. POST /{id}/accept → OrderAcceptedEvent  
3. POST /{id}/complete → OrderCompletedEvent
```

#### Failed Order Flow
```
1. POST /api/orders (invalid) → 400 Bad Request
2. POST /api/orders → OrderCreatedEvent
3. POST /{id}/cancel → OrderCancelledEvent
```

### 2. Advanced Test Scenarios
```bash
# Multiple customers test
curl -X POST http://localhost:8080/api/orders \
  -H "Content-Type: application/json" \
  -d '{
    "customerId": "customer-456",
    "items": [
      {
        "productId": "expensive-item",
        "productName": "Expensive Item",
        "price": 6000.00,
        "quantity": 1
      }
    ]
  }'
```

### 3. Test Inventory Failure Scenario
```bash
# Product "out-of-stock-product" will trigger inventory failure
curl -X POST http://localhost:8080/api/orders \
  -H "Content-Type: application/json" \
  -d '{
    "customerId": "customer-789",
    "items": [
      {
        "productId": "out-of-stock-product",
        "productName": "Out of Stock Item",
        "price": 99.99,
        "quantity": 1
      }
    ]
  }'
```

### 5. **Error Handling & Edge Cases**

#### Invalid Operations
```bash
# Try to accept non-existent order
curl -X POST http://localhost:8080/api/orders/00000000-0000-0000-0000-000000000000/accept
# Expected: 404 Not Found

# Try to complete pending order (must be accepted first)
curl -X POST http://localhost:8080/api/orders/{pending-order-id}/complete
# Expected: 400 Bad Request

# Try to update items on accepted order
curl -X PUT http://localhost:8080/api/orders/{accepted-order-id}/items \
  -H "Content-Type: application/json" \
  -d @tests/payloads/update-order-items.json
# Expected: 400 Bad Request

# Try to cancel completed order
curl -X POST http://localhost:8080/api/orders/{completed-order-id}/cancel \
  -H "Content-Type: application/json" \
  -d '{"reason": "Too late"}'
# Expected: 400 Bad Request
```

#### State Transition Validation
```bash
# Valid transitions:
# Pending → Accepted → Completed ✅
# Pending → Cancelled ✅
# Accepted → Cancelled ✅
# Completed → (no transitions) ❌
# Cancelled → (no transitions) ❌
```

### 6. **Performance & Load Testing**

#### Bulk Order Creation
```bash
# Create multiple orders quickly
for i in {1..10}; do
  curl -X POST http://localhost:8080/api/orders \
    -H "Content-Type: application/json" \
    -d @tests/payloads/create-order-success.json &
done
wait

# Check all orders created
curl http://localhost:8080/api/orders | jq length
```

## 📋 Test Results Reference

### Expected HTTP Status Codes

| Operation | Success | Validation Error | Not Found | Business Rule Error |
|-----------|---------|------------------|-----------|--------------------|
| Create Order | 201 Created | 400 Bad Request | - | 400 Bad Request |
| Get Orders | 200 OK | - | - | - |
| Get Order by ID | 200 OK | - | 404 Not Found | - |
| Accept Order | 200 OK | - | 404 Not Found | 400 Bad Request |
| Cancel Order | 200 OK | - | 404 Not Found | 400 Bad Request |
| Complete Order | 200 OK | - | 404 Not Found | 400 Bad Request |
| Update Items | 200 OK | 400 Bad Request | 404 Not Found | 400 Bad Request |

### Validation Error Messages

| Test Case | Expected Error Message |
|-----------|------------------------|
| Empty Customer | "Customer ID is required" |
| No Items | "At least one order item is required" |
| Empty Product ID | "Product ID is required for all items" |
| Empty Product Name | "Product name is required for all items" |
| Negative Price | "Item price cannot be negative" |
| Zero Quantity | "Item quantity must be positive" |
| Out of Stock | "Product [name] is out of stock" |
| Order Too Large | "Order total cannot exceed $10,000" |

### Business Rule Error Messages

| Operation | Condition | Expected Error |
|-----------|-----------|----------------|
| Accept Order | Order not pending | "Only pending orders can be accepted" |
| Cancel Order | Order completed/cancelled | "Cannot cancel completed or already cancelled orders" |
| Complete Order | Order not accepted | "Only accepted orders can be completed" |
| Update Items | Order not pending | "Can only update items for pending orders" |

## Event-Driven Architecture Details

### Domain Events Published

| Event | Trigger | Data Included |
|-------|---------|---------------|
| `OrderCreatedEvent` | Order creation | OrderId, CustomerId, TotalAmount |
| `OrderAcceptedEvent` | Order acceptance | OrderId |
| `OrderCancelledEvent` | Order cancellation | OrderId, Reason |
| `OrderCompletedEvent` | Order completion | OrderId |

### Event Flow in Console
```
=== EVENT PUBLISHED ===
Type: OrderCreatedEvent
Data: {
  "OrderId": "guid",
  "CustomerId": "customer-123",
  "TotalAmount": 1399.97
}
=====================
```

### 🔍 Debugging & Troubleshooting

#### Common Issues

1. **Service Not Responding**
```bash
# Check if service is running
curl http://localhost:8080/health
# Expected: "OK"
```

2. **Validation Not Working**
```bash
# Test with known validation failure
curl -v -X POST http://localhost:8080/api/orders \
  -H "Content-Type: application/json" \
  -d @tests/payloads/validation-out-of-stock.json
# Should return 400 with error message
```

3. **Events Not Appearing**
- Check console output where `dotnet run` is running
- Events appear as JSON formatted output
- Each operation should trigger corresponding event

4. **Order ID Extraction**
```bash
# Extract order ID from response (requires jq)
ORDER_ID=$(curl -s -X POST http://localhost:8080/api/orders \
  -H "Content-Type: application/json" \
  -d @tests/payloads/create-order-success.json | jq -r '.id')
echo "Order ID: $ORDER_ID"
```

#### Test Data Cleanup
```bash
# Service uses in-memory database
# Restart service to clear all test data
# Ctrl+C to stop, then `dotnet run` to restart
```

## Business Rules Implementation

### Ordering Service (Clean Architecture)
- **Domain Layer**: Order entity with business rules and domain events
- **Application Layer**: Command handlers with MediatR and CQRS
- **Infrastructure Layer**: EF Core repositories and RabbitMQ publishers
- **Presentation Layer**: REST API controllers

#### Order Validation Rules
- Customer ID must be provided and valid
- At least one order item required
- Item quantities must be positive integers
- Item prices cannot be negative
- Order total calculated automatically

#### Order State Transitions
```
Pending → Accepted (payment success + inventory reserved)
Pending → Cancelled (payment failed OR inventory unavailable)
Accepted → Completed (shipping arranged)
Accepted → Cancelled (manual cancellation)
```

### Payment Service Rules (Clean Architecture)
- **Domain Layer**: Payment entity with processing business logic
- **Application Layer**: Payment processing commands with MediatR
- **Business Rules**: Order amount ≤ $5,000 for success (domain logic)
- **Failure Simulation**: Orders > $5,000 automatically fail (business rule)
- **Processing Time**: 2-second simulation delay in domain method
- **Event Publishing**: Success/failure domain events to ordering system

### Inventory Service Rules (Clean Architecture)
- **Domain Layer**: InventoryItem entity with stock reservation business logic
- **Application Layer**: Stock reservation commands with MediatR
- **Infrastructure Layer**: In-memory repository and RabbitMQ publishers
- **Stock Check**: Real-time inventory validation through domain methods
- **Reservation**: Automatic stock reservation with domain events
- **Failure Simulation**: Product ID "out-of-stock-product" triggers failure
- **Event Publishing**: Reserved/failed events to ordering system

### Notification Service Rules
- **Multi-channel**: Email, SMS, push notifications (simulated)
- **Event Triggers**: All order lifecycle events
- **Customer Communication**: Real-time status updates
- **Delivery Confirmation**: Acknowledgment-based delivery

### Shipping Service Rules
- **Trigger**: Order acceptance (payment + inventory success)
- **Processing Time**: 3-second arrangement simulation
- **Tracking Generation**: Unique tracking numbers (TRK-XXXXXXXX)
- **Integration Ready**: Extensible for real shipping providers

### Transactional Consistency
- **Eventual Consistency**: Event-driven saga pattern
- **Compensating Actions**: Automatic rollback on failures
- **Idempotency**: Duplicate event handling protection
- **Retry Logic**: Failed message reprocessing
- **Dead Letter Queues**: Failed message isolation

## Monitoring and Observability

### Health Checks
```bash
curl http://localhost:8080/health
```

### RabbitMQ Management
- URL: http://localhost:15672
- Username: admin
- Password: admin123

### Kubernetes Monitoring
```bash
# View logs for all services
kubectl logs -f deployment/ordering-service -n ordering-service
kubectl logs -f deployment/inventory-service -n ordering-service
kubectl logs -f deployment/payment-service -n ordering-service
kubectl logs -f deployment/notification-service -n ordering-service
kubectl logs -f deployment/shipping-service -n ordering-service
kubectl logs -f deployment/rabbitmq -n ordering-service

# Monitor resources
kubectl top pods -n ordering-service
kubectl get pods -n ordering-service -w

# Check service endpoints
kubectl get services -n ordering-service
```

### Event Flow Monitoring
```bash
# Monitor RabbitMQ queues and exchanges
kubectl port-forward service/rabbitmq-service 15672:15672 -n ordering-service
# Open http://localhost:15672 (admin/admin123)

# Watch event flow in real-time
docker-compose logs -f ordering-service payment-service inventory-service
```

## Testing

### Unit Tests
```bash
dotnet test
```

### Integration Tests
```bash
# Start test environment
docker-compose -f docker-compose.test.yml up -d

# Run integration tests
dotnet test --filter Category=Integration
```

## Scaling Considerations

### Horizontal Scaling
- **Stateless Services**: All services are stateless and can scale independently
- **Load Balancing**: Kubernetes services provide automatic load balancing
- **Event Processing**: Multiple instances can process events concurrently
- **Database per Service**: Each service can have its own database

### Kubernetes Scaling
```bash
# Scale individual services
kubectl scale deployment ordering-service --replicas=3 -n ordering-service
kubectl scale deployment payment-service --replicas=2 -n ordering-service
kubectl scale deployment inventory-service --replicas=2 -n ordering-service

# Auto-scaling based on CPU/memory
kubectl autoscale deployment ordering-service --cpu-percent=70 --min=2 --max=10 -n ordering-service
```

### Performance Optimization
- **Async Processing**: All event handling is asynchronous
- **Connection Pooling**: RabbitMQ connection sharing
- **In-Memory Caching**: Redis for frequently accessed data
- **Database Optimization**: Proper indexing and query optimization
- **Message Batching**: Bulk event processing capabilities

### Service-Specific Scaling
- **Ordering Service**: CPU-intensive (business logic processing)
- **Payment Service**: I/O-intensive (external payment gateway calls)
- **Inventory Service**: Memory-intensive (stock level caching)
- **Notification Service**: Network-intensive (email/SMS delivery)
- **Shipping Service**: I/O-intensive (shipping provider APIs)

## Production Readiness

### Security
- Add authentication/authorization
- Implement API rate limiting
- Use HTTPS certificates
- Secure RabbitMQ credentials

### Persistence
- Replace in-memory database with persistent storage
- Implement database migrations
- Add connection string configuration

### Monitoring
- Add structured logging (Serilog)
- Implement health checks
- Add metrics collection (Prometheus)
- Distributed tracing (OpenTelemetry)

## Troubleshooting

### Common Issues

1. **RabbitMQ Connection Failed**
   ```bash
   kubectl logs deployment/rabbitmq -n ordering-service
   kubectl port-forward service/rabbitmq-service 5672:5672 -n ordering-service
   ```

2. **Service Not Accessible**
   ```bash
   kubectl get services -n ordering-service
   minikube tunnel  # For LoadBalancer services
   ```

3. **Pod Crashes**
   ```bash
   kubectl describe pod <pod-name> -n ordering-service
   kubectl logs <pod-name> -n ordering-service
   ```

## Architecture Benefits

### Clean Architecture Implementation

**Core Services (Ordering, Inventory, Payment):**
- **Domain Layer**: Contains business entities, domain events, and business rules
- **Application Layer**: Use cases, command handlers, and application services using MediatR
- **Infrastructure Layer**: External concerns like databases, messaging, and third-party services
- **API Layer**: Controllers, dependency injection, and HTTP concerns

**Simplified Services (Notification, Shipping):**
- **Single Layer**: Simple event consumers with minimal business logic
- **Justification**: These services primarily handle I/O operations (sending emails, arranging shipping)
- **Future Enhancement**: Can be refactored to Clean Architecture as business complexity grows

**Benefits:**
- **Testability**: Business logic isolated from external dependencies
- **Maintainability**: Clear separation of concerns across layers
- **Flexibility**: Easy to swap infrastructure components
- **Domain Focus**: Business rules are the center of the application
- **Consistency**: Core business services follow the same architectural patterns

### Microservices Benefits
- **Independent Deployment**: Each service can be deployed separately
- **Technology Diversity**: Services can use different tech stacks if needed
- **Fault Isolation**: Failure in one service doesn't bring down others
- **Team Autonomy**: Different teams can own different services
- **Scalability**: Scale services independently based on demand

### Event-Driven Benefits
- **Loose Coupling**: Services don't need direct knowledge of each other
- **Resilience**: Asynchronous processing handles temporary failures
- **Extensibility**: Easy to add new services that react to existing events
- **Audit Trail**: Complete event history for debugging and analytics
- **Real-time Processing**: Immediate reaction to business events

## Production Considerations

### Security Enhancements
- Add JWT authentication across all services
- Implement API rate limiting and throttling
- Use HTTPS with proper certificates
- Secure RabbitMQ with proper credentials and TLS
- Add input validation and sanitization

### Observability
- Implement distributed tracing (OpenTelemetry)
- Add structured logging (Serilog) with correlation IDs
- Set up metrics collection (Prometheus + Grafana)
- Configure health checks for all services
- Add application performance monitoring (APM)

### Data Persistence
- Replace in-memory databases with persistent storage
- Implement database migrations
- Add backup and disaster recovery strategies
- Consider event sourcing for audit requirements
- Implement CQRS with separate read/write models

## Contributing

1. Follow Clean Architecture principles for core services
2. Maintain event-driven communication patterns
3. Write comprehensive unit and integration tests
4. Update documentation for API and event schema changes
5. Use conventional commit messages
6. Ensure Docker images are optimized and secure

