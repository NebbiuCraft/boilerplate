using System.Threading;
using System.Threading.Tasks;
using Application.Common;
using Domain.Events;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Orders.EventHandlers;

/// <summary>
/// Handles PaymentFailedEvent - could trigger retry logic, customer notifications, etc.
/// </summary>
public class PaymentFailedEventHandler : INotificationHandler<DomainEventNotification>
{
  private readonly ILogger<PaymentFailedEventHandler> _logger;

  public PaymentFailedEventHandler(ILogger<PaymentFailedEventHandler> logger)
  {
    _logger = logger;
  }

  public Task Handle(DomainEventNotification notification, CancellationToken cancellationToken)
  {
    if (notification.DomainEvent is not PaymentFailedEvent paymentFailed)
      return Task.CompletedTask;

    _logger.LogWarning(
        "Payment failed: OrderId={OrderId}, Amount={Amount} {Currency}, Customer={CustomerEmail}, Method={PaymentMethod}, Reason={FailureReason}",
        paymentFailed.OrderId,
        paymentFailed.AttemptedAmount,
        paymentFailed.Currency,
        paymentFailed.CustomerEmail,
        paymentFailed.PaymentMethod,
        paymentFailed.FailureReason);

    return HandlePaymentFailedAsync(paymentFailed, cancellationToken);
  }

  private async Task HandlePaymentFailedAsync(PaymentFailedEvent paymentFailed, CancellationToken cancellationToken)
  {
    // Mock business logic
    await Task.Delay(80, cancellationToken); // Simulate some async work

    _logger.LogInformation("Payment failure notification sent to {CustomerEmail}", paymentFailed.CustomerEmail);
    _logger.LogInformation("Inventory reservation released for OrderId={OrderId}", paymentFailed.OrderId);
    _logger.LogInformation("Payment retry strategy evaluated for OrderId={OrderId}", paymentFailed.OrderId);
    _logger.LogInformation("Fraud analytics updated with failure pattern for customer {CustomerEmail}",
        paymentFailed.CustomerEmail);
  }
}