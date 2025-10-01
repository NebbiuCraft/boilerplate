using System.Threading.Tasks;
using Domain.Common;

namespace Application.Common;

/// <summary>
/// Interface for publishing domain events
/// </summary>
public interface IDomainEventPublisher
{
  /// <summary>
  /// Publish a single domain event
  /// </summary>
  Task PublishAsync(IDomainEvent domainEvent);

  /// <summary>
  /// Publish all domain events from an entity
  /// </summary>
  Task PublishEventsAsync(Entity entity);
}