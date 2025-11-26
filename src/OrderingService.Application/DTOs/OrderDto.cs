using OrderingService.Domain.Enums;

namespace OrderingService.Application.DTOs;

public record OrderDto(
    Guid Id,
    string CustomerId,
    DateTime CreatedAt,
    OrderStatus Status,
    decimal TotalAmount,
    List<OrderItemDto> Items
);

public record OrderItemDto(
    string ProductId,
    string ProductName,
    decimal Price,
    int Quantity
);