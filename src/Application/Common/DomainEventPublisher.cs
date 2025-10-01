using System.Linq;
using System.Threading.Tasks;
using Domain.Common;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Common;

/// <summary>
/// Implementation of domain event publisher using MediatR
/// </summary>
public class DomainEventPublisher : IDomainEventPublisher
{
  private readonly IMediator _mediator;
  private readonly ILogger<DomainEventPublisher> _logger;

  public DomainEventPublisher(IMediator mediator, ILogger<DomainEventPublisher> logger)
  {
    _mediator = mediator;
    _logger = logger;
  }

  public async Task PublishAsync(IDomainEvent domainEvent)
  {
    _logger.LogDebug(
        "Publishing domain event: {EventType} | Event ID: {EventId} | Occurred At: {OccurredAt}",
        domainEvent.GetType().Name,
        domainEvent.EventId,
        domainEvent.OccurredAt);

    var notification = new DomainEventNotification(domainEvent);
    await _mediator.Publish(notification);

    _logger.LogDebug(
        "Domain event published successfully: {EventType} | Event ID: {EventId}",
        domainEvent.GetType().Name,
        domainEvent.EventId);
  }

  public async Task PublishEventsAsync(Entity entity)
  {
    var events = entity.DomainEvents.ToList();

    if (!events.Any())
    {
      _logger.LogDebug("No domain events to publish for entity {EntityType} with ID {EntityId}",
          entity.GetType().Name, entity.Id);
      return;
    }

    _logger.LogInformation(
        "Publishing {EventCount} domain events for entity {EntityType} with ID {EntityId}",
        events.Count,
        entity.GetType().Name,
        entity.Id);

    foreach (var domainEvent in events)
    {
      await PublishAsync(domainEvent);
    }

    entity.ClearDomainEvents();

    _logger.LogDebug(
        "All domain events published and cleared for entity {EntityType} with ID {EntityId}",
        entity.GetType().Name,
        entity.Id);
  }
}