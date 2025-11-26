using MediatR;
using OrderingService.Application.Interfaces;
using OrderingService.Domain.Interfaces;

namespace OrderingService.Application.EventHandlers;

// External events from other microservices
public record PaymentSucceededEvent(Guid OrderId, decimal Amount) : INotification;
public record PaymentFailedEvent(Guid OrderId, string Reason) : INotification;
public record InventoryReservedEvent(Guid OrderId, List<string> ProductIds) : INotification;
public record InventoryReservationFailedEvent(Guid OrderId, List<string> UnavailableProducts) : INotification;

public class PaymentSucceededEventHandler : INotificationHandler<PaymentSucceededEvent>
{
    private readonly IOrderRepository _orderRepository;
    private readonly IEventPublisher _eventPublisher;

    public PaymentSucceededEventHandler(IOrderRepository orderRepository, IEventPublisher eventPublisher)
    {
        _orderRepository = orderRepository;
        _eventPublisher = eventPublisher;
    }

    public async Task Handle(PaymentSucceededEvent notification, CancellationToken cancellationToken)
    {
        var order = await _orderRepository.GetByIdAsync(notification.OrderId);
        if (order != null)
        {
            order.Accept();
            await _orderRepository.UpdateAsync(order);

            foreach (var domainEvent in order.DomainEvents)
            {
                await _eventPublisher.PublishAsync(domainEvent);
            }
            order.ClearDomainEvents();
        }
    }
}

public class PaymentFailedEventHandler : INotificationHandler<PaymentFailedEvent>
{
    private readonly IOrderRepository _orderRepository;
    private readonly IEventPublisher _eventPublisher;

    public PaymentFailedEventHandler(IOrderRepository orderRepository, IEventPublisher eventPublisher)
    {
        _orderRepository = orderRepository;
        _eventPublisher = eventPublisher;
    }

    public async Task Handle(PaymentFailedEvent notification, CancellationToken cancellationToken)
    {
        var order = await _orderRepository.GetByIdAsync(notification.OrderId);
        if (order != null)
        {
            order.Cancel($"Payment failed: {notification.Reason}");
            await _orderRepository.UpdateAsync(order);

            foreach (var domainEvent in order.DomainEvents)
            {
                await _eventPublisher.PublishAsync(domainEvent);
            }
            order.ClearDomainEvents();
        }
    }
}