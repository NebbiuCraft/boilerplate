using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Application.Common;
using Domain.Events;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Orders.EventHandlers;

/// <summary>
/// Handles PaymentInitiatedEvent - could trigger fraud detection, payment logging, etc.
/// </summary>
public class PaymentInitiatedEventHandler : INotificationHandler<DomainEventNotification>
{
  private readonly ILogger<PaymentInitiatedEventHandler> _logger;

  public PaymentInitiatedEventHandler(ILogger<PaymentInitiatedEventHandler> logger)
  {
    _logger = logger;
  }

  public Task Handle(DomainEventNotification notification, CancellationToken cancellationToken)
  {
    if (notification.DomainEvent is not PaymentInitiatedEvent paymentInitiated)
      return Task.CompletedTask;

    _logger.LogInformation(
        "Payment initiated for order {OrderId} by customer {CustomerEmail} | Amount: {Amount} {Currency} | Payment Method: {PaymentMethod} | Event ID: {EventId}",
        paymentInitiated.OrderId,
        paymentInitiated.CustomerEmail,
        paymentInitiated.Amount,
        paymentInitiated.Currency,
        paymentInitiated.PaymentMethod,
        paymentInitiated.EventId);

    return HandlePaymentInitiatedAsync(paymentInitiated, cancellationToken);
  }

  private async Task HandlePaymentInitiatedAsync(PaymentInitiatedEvent paymentInitiated, CancellationToken cancellationToken)
  {
    using var scope = _logger.BeginScope(new Dictionary<string, object>
    {
      ["OrderId"] = paymentInitiated.OrderId,
      ["CustomerEmail"] = paymentInitiated.CustomerEmail,
      ["EventId"] = paymentInitiated.EventId
    });

    // Mock business logic
    await Task.Delay(75, cancellationToken); // Simulate some async work

    _logger.LogInformation(
        "Fraud detection initiated | Payment Amount: {PaymentAmount} | Risk Assessment: {RiskLevel}",
        paymentInitiated.Amount,
        paymentInitiated.Amount > 1000 ? "HIGH" : "LOW");

    _logger.LogInformation(
        "Payment audit log created | Audit Type: {AuditType} | Payment Method: {PaymentMethod}",
        "PAYMENT_INITIATED",
        paymentInitiated.PaymentMethod);
  }
}