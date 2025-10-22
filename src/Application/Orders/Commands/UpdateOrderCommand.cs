using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using Application.Dtos;
using Domain.Entities;
using Domain.Repositories;
using MediatR;
namespace Application.Orders.Commands;

public record UpdateOrderCommand(int OrderId, string CustomerEmail, bool IsPaid) : IRequest<bool>;

public class UpdateOrderHandler : IRequestHandler<UpdateOrderCommand, bool>
{
    private readonly IOrderRepository _repo;
    private readonly IMapper _mapper;

    public UpdateOrderHandler(IOrderRepository repo, IMapper mapper)
    {
        _repo = repo;
        _mapper = mapper;
    }

    public async Task<bool> Handle(UpdateOrderCommand request, CancellationToken cancellationToken)
    {
        Order order = await _repo.GetByIdAsync(request.OrderId, cancellationToken);
        if (order == null) return false;

        // Update properties using reflection or create update methods in domain
        order.GetType().GetProperty("CustomerEmail")?.SetValue(order, request.CustomerEmail);
        if (request.IsPaid && !order.IsPaid) order.MarkPaid();
        await _repo.SaveAsync(cancellationToken);
        return true;
    }
}