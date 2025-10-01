using System;

namespace Domain.Common;

/// <summary>
/// Base abstract class for domain events
/// </summary>
public abstract record DomainEvent : IDomainEvent
{
  public DateTime OccurredAt { get; init; } = DateTime.UtcNow;
  public Guid EventId { get; init; } = Guid.NewGuid();
}