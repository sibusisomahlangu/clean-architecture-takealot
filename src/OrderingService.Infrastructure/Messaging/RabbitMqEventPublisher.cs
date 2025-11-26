using System.Text;
using Newtonsoft.Json;
using OrderingService.Application.Interfaces;
using OrderingService.Domain.Events;
using RabbitMQ.Client;

namespace OrderingService.Infrastructure.Messaging;

public class RabbitMqEventPublisher : IEventPublisher
{
    private readonly IConnection _connection;
    private readonly IModel _channel;

    public RabbitMqEventPublisher(IConnection connection)
    {
        _connection = connection;
        _channel = _connection.CreateModel();
        
        // Declare exchange
        _channel.ExchangeDeclare("ordering-events", ExchangeType.Topic, true);
    }

    public async Task PublishAsync<T>(T @event) where T : IDomainEvent
    {
        var routingKey = @event.GetType().Name.ToLower();
        var message = JsonConvert.SerializeObject(@event);
        var body = Encoding.UTF8.GetBytes(message);

        Console.WriteLine($"\n=== ORDERING SERVICE PUBLISHING ===\nEvent: {routingKey}\nData: {message}\n================================\n");

        _channel.BasicPublish(
            exchange: "ordering-events",
            routingKey: routingKey,
            basicProperties: null,
            body: body);

        await Task.CompletedTask;
    }

    public void Dispose()
    {
        _channel?.Dispose();
        _connection?.Dispose();
    }
}