# API Payloads

This directory contains JSON payload files for testing the Ordering Service API with curl.

## 📦 Order Creation Payloads

### Success Cases
- `create-order-success.json` - Standard order with 2 items
- `create-order-multiple-items.json` - Order with 3 different items
- `create-order-single-item.json` - Simple single item order

### Usage
```bash
curl -X POST http://localhost:8080/api/orders \
  -H "Content-Type: application/json" \
  -d @tests/payloads/create-order-success.json
```

## ❌ Validation Test Payloads

### Customer Validation
- `validation-empty-customer.json` - Empty customer ID

### Item Validation
- `validation-no-items.json` - No items in order
- `validation-empty-product-id.json` - Empty product ID
- `validation-empty-product-name.json` - Empty product name
- `validation-negative-price.json` - Negative item price
- `validation-zero-quantity.json` - Zero item quantity

### Business Rules
- `validation-out-of-stock.json` - Out of stock product
- `validation-order-too-large.json` - Order exceeds $10,000 limit

### Usage
```bash
# Test validation (should return 400 Bad Request)
curl -X POST http://localhost:8080/api/orders \
  -H "Content-Type: application/json" \
  -d @tests/payloads/validation-out-of-stock.json
```

## 🔄 Order Operations Payloads

### Cancel Order
- `cancel-order.json` - Standard cancellation
- `cancel-order-payment-failed.json` - Payment failure cancellation
- `cancel-order-inventory-failed.json` - Inventory failure cancellation

### Usage
```bash
# Cancel order (replace {order-id} with actual ID)
curl -X POST http://localhost:8080/api/orders/{order-id}/cancel \
  -H "Content-Type: application/json" \
  -d @tests/payloads/cancel-order.json
```

### Update Order Items
- `update-order-items.json` - Update with multiple items
- `update-order-items-single.json` - Update with single item

### Usage
```bash
# Update order items (only for pending orders)
curl -X PUT http://localhost:8080/api/orders/{order-id}/items \
  -H "Content-Type: application/json" \
  -d @tests/payloads/update-order-items.json
```

## 🧪 Complete Test Flow

```bash
# 1. Create order
ORDER_RESPONSE=$(curl -s -X POST http://localhost:8080/api/orders \
  -H "Content-Type: application/json" \
  -d @tests/payloads/create-order-success.json)

echo $ORDER_RESPONSE

# 2. Extract order ID (manually from response)
ORDER_ID="your-order-id-here"

# 3. Get order details
curl http://localhost:8080/api/orders/$ORDER_ID

# 4. Accept order
curl -X POST http://localhost:8080/api/orders/$ORDER_ID/accept

# 5. Complete order
curl -X POST http://localhost:8080/api/orders/$ORDER_ID/complete

# 6. Get all orders
curl http://localhost:8080/api/orders

# 7. Get orders by customer
curl http://localhost:8080/api/orders/customer/customer-123
```

## 📊 Expected Responses

### Success (201 Created)
```json
{
  "id": "guid",
  "customerId": "customer-123",
  "createdAt": "datetime",
  "status": "Pending",
  "totalAmount": 1399.97,
  "items": [...]
}
```

### Validation Error (400 Bad Request)
```json
"Product Unavailable Item is out of stock"
```

### Not Found (404)
```json
"Order not found"
```