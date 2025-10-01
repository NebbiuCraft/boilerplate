using Domain.Common;
using MediatR;

namespace Application.Common;

/// <summary>
/// Wrapper to make domain events compatible with MediatR
/// </summary>
public class DomainEventNotification : INotification
{
  public IDomainEvent DomainEvent { get; }

  public DomainEventNotification(IDomainEvent domainEvent)
  {
    DomainEvent = domainEvent;
  }
}