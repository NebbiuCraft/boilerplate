using System;

namespace Domain.Common;

/// <summary>
/// Base interface for all domain events
/// </summary>
public interface IDomainEvent
{
  /// <summary>
  /// When the event occurred
  /// </summary>
  DateTime OccurredAt { get; }

  /// <summary>
  /// Unique identifier for this event instance
  /// </summary>
  Guid EventId { get; }
}