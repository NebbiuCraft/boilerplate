using System.Threading;
using System.Threading.Tasks;
using Application.Common;
using Domain.Events;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Orders.EventHandlers;

/// <summary>
/// Handles OrderItemAddedEvent - could trigger inventory updates, recommendations, etc.
/// </summary>
public class OrderItemAddedEventHandler : INotificationHandler<DomainEventNotification>
{
  private readonly ILogger<OrderItemAddedEventHandler> _logger;

  public OrderItemAddedEventHandler(ILogger<OrderItemAddedEventHandler> logger)
  {
    _logger = logger;
  }

  public Task Handle(DomainEventNotification notification, CancellationToken cancellationToken)
  {
    if (notification.DomainEvent is not OrderItemAddedEvent itemAdded)
      return Task.CompletedTask;

    _logger.LogInformation(
        "Item added to order: OrderId={OrderId}, Product={ProductName}, Quantity={Quantity}, Customer={CustomerEmail}",
        itemAdded.OrderId,
        itemAdded.ProductName,
        itemAdded.Quantity,
        itemAdded.CustomerEmail);

    return HandleItemAddedAsync(itemAdded, cancellationToken);
  }

  private async Task HandleItemAddedAsync(OrderItemAddedEvent itemAdded, CancellationToken cancellationToken)
  {
    // Mock business logic
    await Task.Delay(50, cancellationToken); // Simulate some async work

    _logger.LogInformation("Inventory reservation attempted for {ProductName} (Qty: {Quantity})",
        itemAdded.ProductName, itemAdded.Quantity);
    _logger.LogInformation("Product recommendations updated for customer {CustomerEmail}",
        itemAdded.CustomerEmail);
  }
}