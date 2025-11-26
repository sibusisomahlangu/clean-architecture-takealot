using MediatR;
using OrderingService.Application.DTOs;

namespace OrderingService.Application.Commands;

public record CreateOrderCommand(string CustomerId, List<OrderItemDto> Items) : IRequest<OrderDto>;

public record AcceptOrderCommand(Guid OrderId) : IRequest<bool>;

public record CancelOrderCommand(Guid OrderId, string Reason) : IRequest<bool>;