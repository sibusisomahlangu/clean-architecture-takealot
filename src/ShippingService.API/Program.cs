using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();
app.UseRouting();
app.MapControllers();

app.MapGet("/", () => "Shipping Service is running!");
app.MapGet("/health", () => "OK");

// Start RabbitMQ consumer in background
_ = Task.Run(async () =>
{
    try
    {
        var factory = new ConnectionFactory() { HostName = Environment.GetEnvironmentVariable("RABBITMQ_HOST") ?? "localhost", UserName = "admin", Password = "admin123" };
        using var connection = factory.CreateConnection();
        using var channel = connection.CreateModel();

        channel.ExchangeDeclare(exchange: "ordering-events", type: ExchangeType.Topic, durable: true);
        var queueName = channel.QueueDeclare().QueueName;
        channel.QueueBind(queue: queueName, exchange: "ordering-events", routingKey: "orderacceptedevent");

        var consumer = new EventingBasicConsumer(channel);
        consumer.Received += async (model, ea) =>
        {
            var body = ea.Body.ToArray();
            var message = Encoding.UTF8.GetString(body);
            
            Console.WriteLine($"\n=== SHIPPING SERVICE ===");
            Console.WriteLine($"Received: {message}");
            
            var orderEvent = JsonSerializer.Deserialize<OrderAcceptedEvent>(message);
            
            // Simulate shipping arrangement
            await Task.Delay(3000);
            
            var trackingNumber = $"TRK-{Random.Shared.Next(10000000, 99999999)}";
            var shippingArranged = new { OrderId = orderEvent.OrderId, TrackingNumber = trackingNumber, EstimatedDelivery = DateTime.UtcNow.AddDays(3) };
            Console.WriteLine($"Shipping ARRANGED: {JsonSerializer.Serialize(shippingArranged)}");
            
            // Publish ShippingArranged event
            var shippingMessage = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(shippingArranged));
            channel.BasicPublish(exchange: "ordering-events", routingKey: "ShippingArranged", basicProperties: null, body: shippingMessage);
            
            Console.WriteLine("========================\n");
        };

        channel.BasicConsume(queue: queueName, autoAck: true, consumer: consumer);
        
        Console.WriteLine("Shipping Service listening for OrderAccepted events...");
        
        // Keep the consumer running
        while (true)
        {
            await Task.Delay(1000);
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Shipping Service RabbitMQ connection failed: {ex.Message}");
        Console.WriteLine("Running without RabbitMQ - shipping will be logged only");
    }
});

app.Run();

public record OrderAcceptedEvent(Guid OrderId);