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

app.MapGet("/", () => "Payment Service is running!");
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
            
            Console.WriteLine($"\n=== PAYMENT SERVICE ===");
            Console.WriteLine($"Received: {message}");
            
            var orderEvent = JsonSerializer.Deserialize<OrderCreatedEvent>(message);
            
            // Simulate payment processing
            await Task.Delay(2000);
            
            if (orderEvent.TotalAmount <= 5000)
            {
                var paymentSuccess = new { OrderId = orderEvent.OrderId, Amount = orderEvent.TotalAmount };
                Console.WriteLine($"Payment SUCCESS: {JsonSerializer.Serialize(paymentSuccess)}");
                
                // Publish PaymentSucceeded event back to ordering service
                var successMessage = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(paymentSuccess));
                channel.BasicPublish(exchange: "ordering-events", routingKey: "PaymentSucceeded", basicProperties: null, body: successMessage);
            }
            else
            {
                var paymentFailed = new { OrderId = orderEvent.OrderId, Reason = "Amount exceeds limit" };
                Console.WriteLine($"Payment FAILED: {JsonSerializer.Serialize(paymentFailed)}");
                
                // Publish PaymentFailed event
                var failMessage = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(paymentFailed));
                channel.BasicPublish(exchange: "ordering-events", routingKey: "PaymentFailed", basicProperties: null, body: failMessage);
            }
            Console.WriteLine("=====================\n");
        };

        channel.BasicConsume(queue: queueName, autoAck: true, consumer: consumer);
        
        Console.WriteLine("Payment Service listening for OrderCreated events...");
        
        // Keep the consumer running
        while (true)
        {
            await Task.Delay(1000);
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Payment Service RabbitMQ connection failed: {ex.Message}");
        Console.WriteLine("Running without RabbitMQ - events will be logged only");
    }
});

app.Run();

public record OrderCreatedEvent(Guid OrderId, string CustomerId, decimal TotalAmount);