using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Application.Common;
using Domain.Events;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Orders.EventHandlers;

/// <summary>
/// Handles PaymentSuccessfulEvent - could trigger order fulfillment, emails, analytics, etc.
/// </summary>
public class PaymentSuccessfulEventHandler : INotificationHandler<DomainEventNotification>
{
  private readonly ILogger<PaymentSuccessfulEventHandler> _logger;

  public PaymentSuccessfulEventHandler(ILogger<PaymentSuccessfulEventHandler> logger)
  {
    _logger = logger;
  }

  public Task Handle(DomainEventNotification notification, CancellationToken cancellationToken)
  {
    if (notification.DomainEvent is not PaymentSuccessfulEvent paymentSuccessful)
      return Task.CompletedTask;

    _logger.LogInformation(
        "Payment successfully processed for order {OrderId} | Transaction: {TransactionId} | Amount: {ProcessedAmount} | Customer: {CustomerEmail} | Payment Date: {PaymentDate}",
        paymentSuccessful.OrderId,
        paymentSuccessful.TransactionId,
        paymentSuccessful.ProcessedAmount,
        paymentSuccessful.CustomerEmail,
        paymentSuccessful.PaymentDate);

    return HandlePaymentSuccessfulAsync(paymentSuccessful, cancellationToken);
  }

  private async Task HandlePaymentSuccessfulAsync(PaymentSuccessfulEvent paymentSuccessful, CancellationToken cancellationToken)
  {
    using var scope = _logger.BeginScope(new Dictionary<string, object>
    {
      ["OrderId"] = paymentSuccessful.OrderId,
      ["TransactionId"] = paymentSuccessful.TransactionId,
      ["CustomerEmail"] = paymentSuccessful.CustomerEmail,
      ["EventId"] = paymentSuccessful.EventId
    });

    // Mock business logic
    await Task.Delay(100, cancellationToken); // Simulate some async work

    _logger.LogInformation(
        "Order fulfillment workflow initiated | Fulfillment Priority: {Priority} | Expected Delivery: {DeliveryWindow}",
        paymentSuccessful.ProcessedAmount > 500 ? "HIGH" : "STANDARD",
        "3-5 business days");

    _logger.LogInformation(
        "Customer notification sent | Notification Type: {NotificationType} | Channel: {Channel}",
        "PAYMENT_CONFIRMATION",
        "EMAIL");

    _logger.LogInformation(
        "Revenue metrics updated | Revenue Amount: {RevenueAmount} | Revenue Category: {Category} | Transaction Fee: {Fee}",
        paymentSuccessful.ProcessedAmount,
        "ORDER_PAYMENT",
        paymentSuccessful.ProcessedAmount * 0.029m); // Mock 2.9% fee

    _logger.LogInformation(
        "Inventory commitment finalized | Commitment Type: {CommitmentType} | Inventory Status: {Status}",
        "PAYMENT_BASED",
        "ALLOCATED");
  }
}