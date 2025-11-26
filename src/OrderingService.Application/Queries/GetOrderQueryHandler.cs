using MediatR;
using OrderingService.Application.DTOs;
using OrderingService.Domain.Interfaces;

namespace OrderingService.Application.Queries;

public class GetOrderQueryHandler : IRequestHandler<GetOrderQuery, OrderDto?>
{
    private readonly IOrderRepository _orderRepository;

    public GetOrderQueryHandler(IOrderRepository orderRepository)
    {
        _orderRepository = orderRepository;
    }

    public async Task<OrderDto?> Handle(GetOrderQuery request, CancellationToken cancellationToken)
    {
        var order = await _orderRepository.GetByIdAsync(request.OrderId);
        if (order == null) return null;

        return new OrderDto(
            order.Id,
            order.CustomerId,
            order.CreatedAt,
            order.Status,
            order.TotalAmount,
            order.Items.Select(i => new OrderItemDto(i.ProductId, i.ProductName, i.Price, i.Quantity)).ToList()
        );
    }
}

public class GetAllOrdersQueryHandler : IRequestHandler<GetAllOrdersQuery, List<OrderDto>>
{
    private readonly IOrderRepository _orderRepository;

    public GetAllOrdersQueryHandler(IOrderRepository orderRepository)
    {
        _orderRepository = orderRepository;
    }

    public async Task<List<OrderDto>> Handle(GetAllOrdersQuery request, CancellationToken cancellationToken)
    {
        var orders = await _orderRepository.GetAllAsync();
        
        return orders.Select(order => new OrderDto(
            order.Id,
            order.CustomerId,
            order.CreatedAt,
            order.Status,
            order.TotalAmount,
            order.Items.Select(i => new OrderItemDto(i.ProductId, i.ProductName, i.Price, i.Quantity)).ToList()
        )).ToList();
    }
}

public class GetOrdersByCustomerQueryHandler : IRequestHandler<GetOrdersByCustomerQuery, List<OrderDto>>
{
    private readonly IOrderRepository _orderRepository;

    public GetOrdersByCustomerQueryHandler(IOrderRepository orderRepository)
    {
        _orderRepository = orderRepository;
    }

    public async Task<List<OrderDto>> Handle(GetOrdersByCustomerQuery request, CancellationToken cancellationToken)
    {
        var orders = await _orderRepository.GetByCustomerIdAsync(request.CustomerId);
        
        return orders.Select(order => new OrderDto(
            order.Id,
            order.CustomerId,
            order.CreatedAt,
            order.Status,
            order.TotalAmount,
            order.Items.Select(i => new OrderItemDto(i.ProductId, i.ProductName, i.Price, i.Quantity)).ToList()
        )).ToList();
    }
}