using System;
using System.Threading.Tasks;
using Application.Dtos;
using Application.Orders.Commands;
using Application.Orders.Queries;
using FakePaymentService;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace WebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class OrderController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<OrderController> _logger;

    public OrderController(IMediator mediator, ILogger<OrderController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    [HttpPost]
    public async Task<IActionResult> CreateOrder([FromBody] CreateOrderDto createOrderDto)
    {
        _logger.LogInformation(
            "Creating new order | Customer: {CustomerEmail}",
            createOrderDto.CustomerEmail);

        var id = await _mediator.Send(new CreateOrderCommand(createOrderDto));

        _logger.LogInformation(
            "Order created successfully | Order ID: {OrderId} | Customer: {CustomerEmail}",
            id,
            createOrderDto.CustomerEmail);

        return Ok(id);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetOrderById([FromRoute] int id)
    {
        _logger.LogDebug("Retrieving order by ID: {OrderId}", id);

        var order = await _mediator.Send(new GetOrderByIdQuery(id));
        if (order == null)
        {
            _logger.LogWarning("Order not found: {OrderId}", id);
            return NotFound();
        }

        _logger.LogDebug(
            "Order retrieved successfully | Order ID: {OrderId} | Customer: {CustomerEmail} | Status: {IsPaid}",
            order.Id,
            order.CustomerEmail,
            order.IsPaid ? "PAID" : "PENDING");

        return Ok(order);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateOrder([FromRoute] int id, [FromBody] UpdateOrderDto updateOrderDto)
    {
        var result = await _mediator.Send(new UpdateOrderCommand(id, updateOrderDto));
        if (!result) return NotFound();
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteOrder([FromRoute] int id)
    {
        var result = await _mediator.Send(new DeleteOrderCommand(id));
        if (!result) return NotFound();
        return NoContent();
    }

    [HttpPost("{id}/payment")]
    public async Task<IActionResult> ProcessOrderPayment([FromRoute] int id, [FromBody] ProcessPaymentDto processPaymentDto)
    {
        // Set the order ID from the route parameter
        processPaymentDto.OrderId = id;

        var result = await _mediator.Send(new ProcessOrderPaymentCommand(processPaymentDto));

        return result.Status == PaymentStatus.Success
            ? Ok(result)
            : BadRequest(result);
    }
}
