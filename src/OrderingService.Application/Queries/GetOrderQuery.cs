using MediatR;
using OrderingService.Application.DTOs;

namespace OrderingService.Application.Queries;

public record GetOrderQuery(Guid OrderId) : IRequest<OrderDto?>;

public record GetAllOrdersQuery : IRequest<List<OrderDto>>;

public record GetOrdersByCustomerQuery(string CustomerId) : IRequest<List<OrderDto>>;