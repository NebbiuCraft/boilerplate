using System;
using Domain.Common;

namespace Domain.Events;

/// <summary>
/// Event raised when an item is added to an order
/// </summary>
public sealed record OrderItemAddedEvent(
    int OrderId,
    string ProductName,
    int Quantity,
    string CustomerEmail
) : DomainEvent;