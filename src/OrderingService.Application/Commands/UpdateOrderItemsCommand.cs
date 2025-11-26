using MediatR;
using OrderingService.Application.DTOs;

namespace OrderingService.Application.Commands;

public record UpdateOrderItemsCommand(Guid OrderId, List<OrderItemDto> Items) : IRequest<bool>;