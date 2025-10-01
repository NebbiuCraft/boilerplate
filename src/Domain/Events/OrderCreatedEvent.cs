using System;
using Domain.Common;

namespace Domain.Events;

/// <summary>
/// Event raised when a new order is created
/// </summary>
public sealed record OrderCreatedEvent(
    int OrderId,
    string CustomerEmail,
    int ItemCount
) : DomainEvent;