using MediatR;
using OrderingService.Application.DTOs;
using OrderingService.Application.Interfaces;
using OrderingService.Domain.Entities;
using OrderingService.Domain.Interfaces;

namespace OrderingService.Application.Commands;

public class CreateOrderCommandHandler : IRequestHandler<CreateOrderCommand, OrderDto>
{
    private readonly IOrderRepository _orderRepository;
    private readonly IEventPublisher _eventPublisher;

    public CreateOrderCommandHandler(IOrderRepository orderRepository, IEventPublisher eventPublisher)
    {
        _orderRepository = orderRepository;
        _eventPublisher = eventPublisher;
    }

    public async Task<OrderDto> Handle(CreateOrderCommand request, CancellationToken cancellationToken)
    {
        var orderItems = request.Items.Select(item => 
            new OrderItem(item.ProductId, item.ProductName, item.Price, item.Quantity)).ToList();

        var order = new Order(request.CustomerId, orderItems);
        
        await _orderRepository.AddAsync(order);

        // Publish domain events
        foreach (var domainEvent in order.DomainEvents)
        {
            await _eventPublisher.PublishAsync(domainEvent);
        }
        order.ClearDomainEvents();

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

public class AcceptOrderCommandHandler : IRequestHandler<AcceptOrderCommand, bool>
{
    private readonly IOrderRepository _orderRepository;
    private readonly IEventPublisher _eventPublisher;

    public AcceptOrderCommandHandler(IOrderRepository orderRepository, IEventPublisher eventPublisher)
    {
        _orderRepository = orderRepository;
        _eventPublisher = eventPublisher;
    }

    public async Task<bool> Handle(AcceptOrderCommand request, CancellationToken cancellationToken)
    {
        var order = await _orderRepository.GetByIdAsync(request.OrderId);
        if (order == null) return false;

        order.Accept();
        await _orderRepository.UpdateAsync(order);

        foreach (var domainEvent in order.DomainEvents)
        {
            await _eventPublisher.PublishAsync(domainEvent);
        }
        order.ClearDomainEvents();

        return true;
    }
}

public class CancelOrderCommandHandler : IRequestHandler<CancelOrderCommand, bool>
{
    private readonly IOrderRepository _orderRepository;
    private readonly IEventPublisher _eventPublisher;

    public CancelOrderCommandHandler(IOrderRepository orderRepository, IEventPublisher eventPublisher)
    {
        _orderRepository = orderRepository;
        _eventPublisher = eventPublisher;
    }

    public async Task<bool> Handle(CancelOrderCommand request, CancellationToken cancellationToken)
    {
        var order = await _orderRepository.GetByIdAsync(request.OrderId);
        if (order == null) return false;

        order.Cancel(request.Reason);
        await _orderRepository.UpdateAsync(order);

        foreach (var domainEvent in order.DomainEvents)
        {
            await _eventPublisher.PublishAsync(domainEvent);
        }
        order.ClearDomainEvents();

        return true;
    }
}

public class CompleteOrderCommandHandler : IRequestHandler<CompleteOrderCommand, bool>
{
    private readonly IOrderRepository _orderRepository;
    private readonly IEventPublisher _eventPublisher;

    public CompleteOrderCommandHandler(IOrderRepository orderRepository, IEventPublisher eventPublisher)
    {
        _orderRepository = orderRepository;
        _eventPublisher = eventPublisher;
    }

    public async Task<bool> Handle(CompleteOrderCommand request, CancellationToken cancellationToken)
    {
        var order = await _orderRepository.GetByIdAsync(request.OrderId);
        if (order == null) return false;

        order.Complete();
        await _orderRepository.UpdateAsync(order);

        foreach (var domainEvent in order.DomainEvents)
        {
            await _eventPublisher.PublishAsync(domainEvent);
        }
        order.ClearDomainEvents();

        return true;
    }
}

public class UpdateOrderItemsCommandHandler : IRequestHandler<UpdateOrderItemsCommand, bool>
{
    private readonly IOrderRepository _orderRepository;

    public UpdateOrderItemsCommandHandler(IOrderRepository orderRepository)
    {
        _orderRepository = orderRepository;
    }

    public async Task<bool> Handle(UpdateOrderItemsCommand request, CancellationToken cancellationToken)
    {
        var order = await _orderRepository.GetByIdAsync(request.OrderId);
        if (order == null) return false;

        var newItems = request.Items.Select(item => 
            new OrderItem(item.ProductId, item.ProductName, item.Price, item.Quantity)).ToList();

        order.UpdateItems(newItems);
        await _orderRepository.UpdateAsync(order);

        return true;
    }
}