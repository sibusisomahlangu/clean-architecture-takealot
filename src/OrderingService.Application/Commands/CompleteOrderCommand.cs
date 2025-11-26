using MediatR;

namespace OrderingService.Application.Commands;

public record CompleteOrderCommand(Guid OrderId) : IRequest<bool>;