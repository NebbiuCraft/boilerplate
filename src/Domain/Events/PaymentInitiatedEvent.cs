using System;
using Domain.Common;

namespace Domain.Events;

/// <summary>
/// Event raised when a payment is initiated for an order
/// </summary>
public sealed record PaymentInitiatedEvent(
    int OrderId,
    decimal Amount,
    string Currency,
    string CustomerEmail,
    string PaymentMethod
) : DomainEvent;