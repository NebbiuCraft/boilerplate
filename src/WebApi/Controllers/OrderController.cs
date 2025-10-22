using System;
using System.Threading.Tasks;
using Application.Dtos;
using Application.Orders.Commands;
using Application.Orders.Queries;
using Application.Common;
using FakePaymentService;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Swashbuckle.AspNetCore.Annotations;

namespace WebApi.Controllers;

/// <summary>
/// Order management operations
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
[SwaggerTag("Order management including CRUD operations and payment processing")]
public class OrderController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<OrderController> _logger;

    public OrderController(IMediator mediator, ILogger<OrderController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    /// <summary>
    /// Create a new order
    /// </summary>
    /// <param name="createOrderDto">Order creation details</param>
    /// <returns>The ID of the created order</returns>
    /// <response code="200">Order created successfully</response>
    /// <response code="400">Invalid request data</response>
    [HttpPost]
    [ProducesResponseType(typeof(int), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [SwaggerOperation(
        Summary = "Create a new order",
        Description = "Creates a new order for the specified customer email address",
        OperationId = "CreateOrder",
        Tags = new[] { "Orders" }
    )]
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

    /// <summary>
    /// Get an order by ID
    /// </summary>
    /// <param name="id">The order ID</param>
    /// <returns>The order details</returns>
    /// <response code="200">Order found and returned</response>
    /// <response code="404">Order not found</response>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(OrderDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [SwaggerOperation(
        Summary = "Get order by ID",
        Description = "Retrieves a specific order by its unique identifier",
        OperationId = "GetOrderById",
        Tags = new[] { "Orders" }
    )]
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

    /// <summary>
    /// Update an existing order
    /// </summary>
    /// <param name="id">The order ID</param>
    /// <param name="updateOrderDto">Updated order details</param>
    /// <returns>No content on success</returns>
    /// <response code="204">Order updated successfully</response>
    /// <response code="404">Order not found</response>
    /// <response code="400">Invalid request data</response>
    [HttpPut("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [SwaggerOperation(
        Summary = "Update an order",
        Description = "Updates an existing order with new details",
        OperationId = "UpdateOrder",
        Tags = new[] { "Orders" }
    )]
    public async Task<IActionResult> UpdateOrder([FromRoute] int id, [FromBody] UpdateOrderDto updateOrderDto)
    {
        var result = await _mediator.Send(new UpdateOrderCommand(id, updateOrderDto));
        if (!result) return NotFound();
        return NoContent();
    }

    /// <summary>
    /// Get a paginated list of orders
    /// </summary>
    /// <param name="query">Pagination, filtering, and sorting parameters</param>
    /// <returns>Paginated list of orders</returns>
    /// <response code="200">Orders retrieved successfully</response>
    /// <response code="400">Invalid query parameters</response>
    [HttpGet]
    [ProducesResponseType(typeof(PaginatedResult<OrderDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [SwaggerOperation(
        Summary = "Get orders with pagination",
        Description = "Retrieves a paginated list of orders with optional filtering by customer email and amount range, plus sorting capabilities",
        OperationId = "GetOrders",
        Tags = new[] { "Orders" }
    )]
    public async Task<IActionResult> GetOrders([FromQuery] GetOrdersQuery query)
    {
        _logger.LogInformation(
            "Retrieving orders | Page: {Page} | PageSize: {PageSize} | SortBy: {SortBy} | CustomerEmail: {CustomerEmail}",
            query.PageNumber,
            query.PageSize,
            query.SortBy,
            query.CustomerEmail);

        var result = await _mediator.Send(query);

        _logger.LogInformation(
            "Orders retrieved successfully | Total: {TotalCount} | Page: {Page} | Items: {ItemCount}",
            result.TotalCount,
            result.PageNumber,
            result.Items.Count);

        return Ok(result);
    }

    /// <summary>
    /// Delete an order (soft delete)
    /// </summary>
    /// <param name="id">The order ID</param>
    /// <returns>No content on success</returns>
    /// <response code="204">Order deleted successfully</response>
    /// <response code="404">Order not found</response>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [SwaggerOperation(
        Summary = "Delete an order",
        Description = "Soft deletes an order (sets Active = false). The order is not physically removed from the database",
        OperationId = "DeleteOrder",
        Tags = new[] { "Orders" }
    )]
    public async Task<IActionResult> DeleteOrder([FromRoute] int id)
    {
        var result = await _mediator.Send(new DeleteOrderCommand(id));
        if (!result) return NotFound();
        return NoContent();
    }

    /// <summary>
    /// Process payment for an order
    /// </summary>
    /// <param name="id">The order ID</param>
    /// <param name="processPaymentDto">Payment processing details</param>
    /// <returns>Payment result</returns>
    /// <response code="200">Payment processed successfully</response>
    /// <response code="400">Payment failed or invalid request</response>
    /// <response code="404">Order not found</response>
    [HttpPost("{id}/payment")]
    [ProducesResponseType(typeof(PaymentResult), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(PaymentResult), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [SwaggerOperation(
        Summary = "Process order payment",
        Description = "Processes payment for a specific order. Amounts over $10,000 will fail for demonstration purposes",
        OperationId = "ProcessOrderPayment",
        Tags = new[] { "Payments" }
    )]
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
