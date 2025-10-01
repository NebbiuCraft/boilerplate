using System.Threading;
using System.Threading.Tasks;
using Application.Common;
using Domain.Events;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Orders.EventHandlers;

/// <summary>
/// Handles OrderCreatedEvent - could trigger welcome emails, analytics, etc.
/// </summary>
public class OrderCreatedEventHandler : INotificationHandler<DomainEventNotification>
{
  private readonly ILogger<OrderCreatedEventHandler> _logger;

  public OrderCreatedEventHandler(ILogger<OrderCreatedEventHandler> logger)
  {
    _logger = logger;
  }

  public Task Handle(DomainEventNotification notification, CancellationToken cancellationToken)
  {
    if (notification.DomainEvent is not OrderCreatedEvent orderCreated)
      return Task.CompletedTask;

    _logger.LogInformation(
        "Order created: OrderId={OrderId}, Customer={CustomerEmail}, ItemCount={ItemCount}",
        orderCreated.OrderId,
        orderCreated.CustomerEmail,
        orderCreated.ItemCount);

    // Mock: Here you might send welcome email, update analytics, etc.
    return HandleOrderCreatedAsync(orderCreated, cancellationToken);
  }

  private async Task HandleOrderCreatedAsync(OrderCreatedEvent orderCreated, CancellationToken cancellationToken)
  {
    // Mock business logic
    await Task.Delay(100, cancellationToken); // Simulate some async work

    _logger.LogInformation("Welcome email would be sent to {CustomerEmail}", orderCreated.CustomerEmail);
    _logger.LogInformation("Order analytics updated for order {OrderId}", orderCreated.OrderId);
  }
}