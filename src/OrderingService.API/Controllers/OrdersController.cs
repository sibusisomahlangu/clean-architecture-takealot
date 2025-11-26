using MediatR;
using Microsoft.AspNetCore.Mvc;
using OrderingService.Application.Commands;
using OrderingService.Application.DTOs;
using OrderingService.Application.Queries;

namespace OrderingService.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class OrdersController : ControllerBase
{
    private readonly IMediator _mediator;

    public OrdersController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<ActionResult<List<OrderDto>>> GetAllOrders()
    {
        var result = await _mediator.Send(new GetAllOrdersQuery());
        return Ok(result);
    }

    [HttpGet("customer/{customerId}")]
    public async Task<ActionResult<List<OrderDto>>> GetOrdersByCustomer(string customerId)
    {
        var result = await _mediator.Send(new GetOrdersByCustomerQuery(customerId));
        return Ok(result);
    }

    [HttpPost]
    public async Task<ActionResult<OrderDto>> CreateOrder([FromBody] CreateOrderCommand command)
    {
        try
        {
            var result = await _mediator.Send(command);
            return CreatedAtAction(nameof(GetOrder), new { id = result.Id }, result);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<OrderDto>> GetOrder(Guid id)
    {
        var result = await _mediator.Send(new GetOrderQuery(id));
        return result != null ? Ok(result) : NotFound();
    }

    [HttpPost("{id}/accept")]
    public async Task<ActionResult> AcceptOrder(Guid id)
    {
        try
        {
            var result = await _mediator.Send(new AcceptOrderCommand(id));
            return result ? Ok() : NotFound();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPost("{id}/cancel")]
    public async Task<ActionResult> CancelOrder(Guid id, [FromBody] CancelOrderRequest request)
    {
        try
        {
            var result = await _mediator.Send(new CancelOrderCommand(id, request.Reason));
            return result ? Ok() : NotFound();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPost("{id}/complete")]
    public async Task<ActionResult> CompleteOrder(Guid id)
    {
        try
        {
            var result = await _mediator.Send(new CompleteOrderCommand(id));
            return result ? Ok() : NotFound();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPut("{id}/items")]
    public async Task<ActionResult> UpdateOrderItems(Guid id, [FromBody] UpdateOrderItemsRequest request)
    {
        try
        {
            var result = await _mediator.Send(new UpdateOrderItemsCommand(id, request.Items));
            return result ? Ok() : NotFound();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }
}

public record CancelOrderRequest(string Reason);
public record UpdateOrderItemsRequest(List<OrderItemDto> Items);