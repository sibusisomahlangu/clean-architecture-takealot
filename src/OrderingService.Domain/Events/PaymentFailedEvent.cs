namespace OrderingService.Domain.Events;

public record PaymentFailedEvent(Guid OrderId, string Reason) : IDomainEvent
{
    public Guid Id { get; } = Guid.NewGuid();
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}