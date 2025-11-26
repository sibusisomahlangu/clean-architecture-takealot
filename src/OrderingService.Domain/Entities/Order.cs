using OrderingService.Domain.Enums;
using OrderingService.Domain.Events;

namespace OrderingService.Domain.Entities;

public class Order
{
    public Guid Id { get; private set; }
    public string CustomerId { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public OrderStatus Status { get; private set; }
    public decimal TotalAmount { get; private set; }
    public List<OrderItem> Items { get; private set; } = new();
    public List<IDomainEvent> DomainEvents { get; private set; } = new();

    private Order() { CustomerId = string.Empty; } // EF Core

    public Order(string customerId, List<OrderItem> items)
    {
        ValidateOrder(customerId, items);
        
        Id = Guid.NewGuid();
        CustomerId = customerId;
        Items = items;
        Status = OrderStatus.Pending;
        CreatedAt = DateTime.UtcNow;
        TotalAmount = items.Sum(x => x.Price * x.Quantity);
        
        AddDomainEvent(new OrderCreatedEvent(Id, CustomerId, TotalAmount));
    }

    public void Accept()
    {
        if (Status != OrderStatus.Pending)
            throw new InvalidOperationException("Only pending orders can be accepted");
            
        Status = OrderStatus.Accepted;
        AddDomainEvent(new OrderAcceptedEvent(Id));
    }

    public void Cancel(string reason)
    {
        if (Status == OrderStatus.Completed || Status == OrderStatus.Cancelled)
            throw new InvalidOperationException("Cannot cancel completed or already cancelled orders");
            
        Status = OrderStatus.Cancelled;
        AddDomainEvent(new OrderCancelledEvent(Id, reason));
    }

    public void Complete()
    {
        if (Status != OrderStatus.Accepted)
            throw new InvalidOperationException("Only accepted orders can be completed");
            
        Status = OrderStatus.Completed;
        AddDomainEvent(new OrderCompletedEvent(Id));
    }

    private void AddDomainEvent(IDomainEvent domainEvent)
    {
        DomainEvents.Add(domainEvent);
    }

    public void ClearDomainEvents()
    {
        DomainEvents.Clear();
    }

    public void UpdateItems(List<OrderItem> newItems)
    {
        if (Status != OrderStatus.Pending)
            throw new InvalidOperationException("Can only update items for pending orders");
            
        ValidateItems(newItems);
        Items = newItems;
        TotalAmount = newItems.Sum(x => x.Price * x.Quantity);
    }

    private static void ValidateOrder(string customerId, List<OrderItem> items)
    {
        if (string.IsNullOrWhiteSpace(customerId))
            throw new ArgumentException("Customer ID is required");
            
        ValidateItems(items);
    }

    private static void ValidateItems(List<OrderItem> items)
    {
        if (items == null || !items.Any())
            throw new ArgumentException("At least one order item is required");
            
        foreach (var item in items)
        {
            if (string.IsNullOrWhiteSpace(item.ProductId))
                throw new ArgumentException("Product ID is required for all items");
                
            if (string.IsNullOrWhiteSpace(item.ProductName))
                throw new ArgumentException("Product name is required for all items");
                
            if (item.Price < 0)
                throw new ArgumentException("Item price cannot be negative");
                
            if (item.Quantity <= 0)
                throw new ArgumentException("Item quantity must be positive");
                
            // Simulate out-of-stock validation
            if (item.ProductId == "out-of-stock-product")
                throw new InvalidOperationException($"Product {item.ProductName} is out of stock");
        }
        
        var totalAmount = items.Sum(x => x.Price * x.Quantity);
        if (totalAmount > 10000)
            throw new InvalidOperationException("Order total cannot exceed $10,000");
    }
}