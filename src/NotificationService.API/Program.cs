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

app.MapGet("/", () => "Notification Service is running!");
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
        
        // Listen to all order lifecycle events
        channel.QueueBind(queue: queueName, exchange: "ordering-events", routingKey: "ordercreatedevent");
        channel.QueueBind(queue: queueName, exchange: "ordering-events", routingKey: "orderacceptedevent");
        channel.QueueBind(queue: queueName, exchange: "ordering-events", routingKey: "ordercancelledevent");
        channel.QueueBind(queue: queueName, exchange: "ordering-events", routingKey: "ordercompletededvent");
        channel.QueueBind(queue: queueName, exchange: "ordering-events", routingKey: "PaymentSucceeded");
        channel.QueueBind(queue: queueName, exchange: "ordering-events", routingKey: "PaymentFailed");
        channel.QueueBind(queue: queueName, exchange: "ordering-events", routingKey: "InventoryReserved");

        var consumer = new EventingBasicConsumer(channel);
        consumer.Received += async (model, ea) =>
        {
            var body = ea.Body.ToArray();
            var message = Encoding.UTF8.GetString(body);
            var routingKey = ea.RoutingKey;
            
            Console.WriteLine($"\n=== NOTIFICATION SERVICE ===");
            Console.WriteLine($"Event: {routingKey}");
            Console.WriteLine($"Data: {message}");
            
            // Simulate sending notifications based on event type
            await Task.Delay(500);
            
            var notification = routingKey.ToLower() switch
            {
                "ordercreatedevent" => "📧 Email: Order confirmation sent to customer",
                "orderacceptedevent" => "📱 SMS: Your order has been accepted and is being processed",
                "ordercancelledevent" => "📧 Email: Order cancellation notification sent",
                "ordercompletededvent" => "📱 SMS: Your order has been completed and shipped!",
                "paymentsucceeded" => "💳 Email: Payment confirmation sent",
                "paymentfailed" => "❌ SMS: Payment failed notification sent",
                "inventoryreserved" => "📦 Email: Items reserved for your order",
                _ => "📢 General notification sent"
            };
            
            Console.WriteLine($"Notification: {notification}");
            Console.WriteLine("============================\n");
        };

        channel.BasicConsume(queue: queueName, autoAck: true, consumer: consumer);
        
        Console.WriteLine("Notification Service listening for all order events...");
        Console.WriteLine("Events: OrderCreated, OrderAccepted, OrderCancelled, OrderCompleted, PaymentSucceeded, PaymentFailed, InventoryReserved");
        
        // Keep the consumer running
        while (true)
        {
            await Task.Delay(1000);
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Notification Service RabbitMQ connection failed: {ex.Message}");
        Console.WriteLine("Running without RabbitMQ - notifications will be logged only");
    }
});

app.Run();