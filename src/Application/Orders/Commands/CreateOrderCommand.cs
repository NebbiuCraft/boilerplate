using System;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using Application.Common;
using Application.Dtos;
using Domain.Entities;
using Domain.Repositories;
using MediatR;
namespace Application.Orders.Commands;

public record CreateOrderCommand(CreateOrderDto OrderDto) : IRequest<int>;

public class CreateOrderHandler : IRequestHandler<CreateOrderCommand, int>
{
    private readonly IOrderRepository _repo;
    private readonly IMapper _mapper;
    private readonly IDomainEventPublisher _domainEventPublisher;

    public CreateOrderHandler(IOrderRepository repo, IMapper mapper, IDomainEventPublisher domainEventPublisher)
    {
        _repo = repo;
        _mapper = mapper;
        _domainEventPublisher = domainEventPublisher;
    }

    public async Task<int> Handle(CreateOrderCommand request, CancellationToken cancellationToken)
    {
        Order order = _mapper.Map<Order>(request.OrderDto);
        await _repo.AddAsync(order, cancellationToken);
        await _repo.SaveAsync(cancellationToken);

        // Publish domain events
        await _domainEventPublisher.PublishEventsAsync(order);

        return order.Id;
    }
}