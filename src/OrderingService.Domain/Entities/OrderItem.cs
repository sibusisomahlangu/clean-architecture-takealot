namespace OrderingService.Domain.Entities;

public class OrderItem
{
    public Guid Id { get; private set; }
    public string ProductId { get; private set; }
    public string ProductName { get; private set; }
    public decimal Price { get; private set; }
    public int Quantity { get; private set; }

    private OrderItem() { ProductId = string.Empty; ProductName = string.Empty; } // EF Core

    public OrderItem(string productId, string productName, decimal price, int quantity)
    {
        if (quantity <= 0) throw new ArgumentException("Quantity must be positive");
        if (price < 0) throw new ArgumentException("Price cannot be negative");
        
        Id = Guid.NewGuid();
        ProductId = productId;
        ProductName = productName;
        Price = price;
        Quantity = quantity;
    }
}