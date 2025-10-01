using System;
using System.Collections.Generic;

namespace Domain.Common;

public abstract class Entity : IEntity
{
  public int Id { get; set; }
  public bool Active { get; set; }

  private readonly List<IDomainEvent> _domainEvents = new();

  /// <summary>
  /// Domain events that have been raised by this entity
  /// </summary>
  public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

  /// <summary>
  /// Add a domain event to be published
  /// </summary>
  protected void RaiseDomainEvent(IDomainEvent domainEvent)
  {
    _domainEvents.Add(domainEvent);
  }

  /// <summary>
  /// Clear all domain events (typically called after publishing)
  /// </summary>
  public void ClearDomainEvents()
  {
    _domainEvents.Clear();
  }
}