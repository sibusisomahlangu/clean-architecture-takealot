using OrderingService.Domain.Events;

namespace OrderingService.Application.Interfaces;

public interface IEventPublisher
{
    Task PublishAsync<T>(T @event) where T : IDomainEvent;
}

public interface IEventConsumer
{
    Task ConsumeAsync<T>(T @event);
}