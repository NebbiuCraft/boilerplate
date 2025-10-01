using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using Application.Dtos;
using Domain.Repositories;
using MediatR;
namespace Application.Orders.Queries;

public record GetOrderByIdQuery(int OrderId) : IRequest<OrderDto>;

public class GetOrderByIdHandler : IRequestHandler<GetOrderByIdQuery, OrderDto>
{
    private readonly IOrderRepository _repo;
    private readonly IMapper _mapper;

    public GetOrderByIdHandler(IOrderRepository repo, IMapper mapper)
    {
        _repo = repo;
        _mapper = mapper;
    }

    public async Task<OrderDto> Handle(GetOrderByIdQuery request, CancellationToken cancellationToken)
    {
        var order = await _repo.GetByIdAsync(request.OrderId, cancellationToken);
        return _mapper.Map<OrderDto>(order);
    }
}
