using System;
using Domain.Common;

namespace Domain.Events;

/// <summary>
/// Event raised when a payment is successfully processed
/// </summary>
public sealed record PaymentSuccessfulEvent(
    int OrderId,
    string TransactionId,
    decimal ProcessedAmount,
    DateTime PaymentDate,
    string CustomerEmail
) : DomainEvent;