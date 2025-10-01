using System;
using Domain.Common;

namespace Domain.Events;

/// <summary>
/// Event raised when a payment fails
/// </summary>
public sealed record PaymentFailedEvent(
    int OrderId,
    decimal AttemptedAmount,
    string Currency,
    string CustomerEmail,
    string FailureReason,
    string PaymentMethod
) : DomainEvent;