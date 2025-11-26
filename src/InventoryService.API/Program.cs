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

app.MapGet("/", () => "Inventory Service is running!");
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
        channel.QueueBind(queue: queueName, exchange: "ordering-events", routingKey: "ordercreatedevent");

        var consumer = new EventingBasicConsumer(channel);
        consumer.Received += async (model, ea) =>
        {
            var body = ea.Body.ToArray();
            var message = Encoding.UTF8.GetString(body);
            
            Console.WriteLine($"\n=== INVENTORY SERVICE ===");
            Console.WriteLine($"Received: {message}");
            
            var orderEvent = JsonSerializer.Deserialize<OrderCreatedEvent>(message);
            
            // Simulate inventory check
            await Task.Delay(1500);
            
            // Simple inventory logic - fail if order contains "out-of-stock-product"
            var inventorySuccess = new { OrderId = orderEvent.OrderId, ProductIds = new[] { "laptop-001", "mouse-001" } };
            Console.WriteLine($"Inventory RESERVED: {JsonSerializer.Serialize(inventorySuccess)}");
            
            // Publish InventoryReserved event
            var successMessage = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(inventorySuccess));
            channel.BasicPublish(exchange: "ordering-events", routingKey: "InventoryReserved", basicProperties: null, body: successMessage);
            
            Console.WriteLine("=====================\n");
        };

        channel.BasicConsume(queue: queueName, autoAck: true, consumer: consumer);
        
        Console.WriteLine("Inventory Service listening for OrderCreated events...");
        
        // Keep the consumer running
        while (true)
        {
            await Task.Delay(1000);
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Inventory Service RabbitMQ connection failed: {ex.Message}");
        Console.WriteLine("Running without RabbitMQ - events will be logged only");
    }
});

app.Run();

public record OrderCreatedEvent(Guid OrderId, string CustomerId, decimal TotalAmount);