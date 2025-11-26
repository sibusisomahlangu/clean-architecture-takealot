using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using OrderingService.Application.Interfaces;
using OrderingService.Domain.Interfaces;
using OrderingService.Infrastructure.Messaging;
using OrderingService.Infrastructure.Persistence;
using RabbitMQ.Client;

var builder = WebApplication.CreateBuilder(args);

// Add services
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter());
    });
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Add MediatR
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(OrderingService.Application.Commands.CreateOrderCommand).Assembly));

// Add Entity Framework
builder.Services.AddDbContext<OrderingDbContext>(options =>
    options.UseInMemoryDatabase("OrderingDb"));

// Add repositories
builder.Services.AddScoped<IOrderRepository, OrderRepository>();

// Add Event Publisher (RabbitMQ)
try
{
    builder.Services.AddSingleton<IConnection>(sp =>
    {
        var factory = new ConnectionFactory()
        {
            HostName = Environment.GetEnvironmentVariable("RABBITMQ_HOST") ?? "localhost",
            UserName = "admin",
            Password = "admin123"
        };
        return factory.CreateConnection();
    });
    builder.Services.AddScoped<IEventPublisher, RabbitMqEventPublisher>();
}
catch
{
    // Fallback to NoOp if RabbitMQ not available
    builder.Services.AddScoped<IEventPublisher, OrderingService.API.NoOpEventPublisher>();
}

var app = builder.Build();

// Configure pipeline
app.UseSwagger();
app.UseSwaggerUI();

// Remove HTTPS redirection for local development
// app.UseHttpsRedirection();
app.UseRouting();
app.UseAuthorization();
app.MapControllers();

// Add a simple health check endpoint
app.MapGet("/", () => "Ordering Service is running!");
app.MapGet("/health", () => "OK");

app.Run();